using System.Buffers;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop;

/// <summary>Small bounded PCM queue over Windows' built-in waveOut device.</summary>
/// <remarks>
/// Six pinned one-frame buffers provide roughly 100 ms of device tolerance. A bounded
/// managed queue transfers pacing to one background worker so waveOut cannot block WinForms
/// painting. Queue exhaustion and worker/device failure remain loud on the submitting thread.
/// </remarks>
internal sealed partial class WaveOutAudioDevice : IHostAudioOutput
{
    /// <summary>Selects the Windows default output device for waveOut.</summary>
    private const uint WaveMapper = uint.MaxValue;
    /// <summary>Native <c>WHDR_DONE</c> bit indicating Windows has returned a header.</summary>
    private const uint HeaderDone = 0x0000_0001;
    /// <summary>Successful result returned by the waveOut multimedia API.</summary>
    private const uint MultimediaSuccess = 0;
    /// <summary>Pinned native buffers reused in a ring while Windows owns submitted audio.</summary>
    private readonly BufferSlot[] slots = [];
    /// <summary>One silent block reused to establish playback lead after startup or reset.</summary>
    private readonly short[] prerollSilence = [];
    /// <summary>Host volume scaling applied when copying emulated PCM into a device slot.</summary>
    private readonly int volumePercent;
    /// <summary>Bounded handoff from the UI producer to the waveOut submission worker.</summary>
    private readonly PendingFrameQueue pendingFrames = new(
        WaveOutAudioPolicy.ManagedQueueCapacityFrames);
    /// <summary>Serializes native device ownership changes against worker submissions.</summary>
    private readonly object deviceGate = new();
    /// <summary>Consumes queued PCM and performs all normally paced waveOut submissions.</summary>
    private readonly Thread submissionWorker = null!;
    /// <summary>Open native waveOut handle, cleared after close.</summary>
    private nint device;
    /// <summary>Next ring-buffer slot inspected for native availability.</summary>
    private int nextSlot;
    /// <summary>Invalidates queued audio when Reset or disposal abandons a generation.</summary>
    private long queueGeneration;
    /// <summary>Total frames accepted into the managed handoff queue.</summary>
    private long enqueuedFrames;
    /// <summary>Total frames removed from the worker queue, including invalidated frames.</summary>
    private long completedFrames;
    /// <summary>First worker exception, rethrown on the next operation from the UI thread.</summary>
    private ExceptionDispatchInfo? workerFailure;
    /// <summary>Whether silent lead-in must be queued before the next real PCM block.</summary>
    private bool prerollRequired = true;
    /// <summary>Prevents new operations and makes repeated disposal a no-op.</summary>
    private bool disposed;
    /// <summary>Lock-free counters describing native queue depth and underrun observations.</summary>
    private readonly WaveOutQueueHealth queueHealth = new();
    /// <summary>Current snapshot of native queue health and managed frames waiting for submission.</summary>
    public WaveOutQueueHealthSnapshot QueueHealth => queueHealth.Snapshot(pendingFrames.Count);

    /// <summary>Opens the default stereo PCM device and starts its bounded submission worker.</summary>
    /// <param name="sampleRate">PCM samples per second requested from Windows.</param>
    /// <param name="channelCount">Must be two because the game mixer emits stereo samples.</param>
    /// <param name="samplesPerBuffer">Interleaved sample count in each hardware buffer; must contain whole stereo frames.</param>
    /// <param name="volumePercent">Host-side gain from zero through one hundred.</param>
    public WaveOutAudioDevice(
        int sampleRate,
        int channelCount,
        int samplesPerBuffer,
        int volumePercent = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        if (channelCount != 2)
            throw new ArgumentOutOfRangeException(nameof(channelCount), "The SNES mixer is stereo.");
        if (samplesPerBuffer <= 0 || samplesPerBuffer % channelCount != 0)
            throw new ArgumentOutOfRangeException(nameof(samplesPerBuffer));
        if (volumePercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(volumePercent));
        this.volumePercent = volumePercent;

        var format = new WaveFormat
        {
            FormatTag = 1, // WAVE_FORMAT_PCM
            Channels = unchecked((ushort)channelCount),
            SamplesPerSecond = unchecked((uint)sampleRate),
            AverageBytesPerSecond = unchecked((uint)(sampleRate * channelCount * sizeof(short))),
            BlockAlign = unchecked((ushort)(channelCount * sizeof(short))),
            BitsPerSample = sizeof(short) * 8,
        };
        ThrowOnError(NativeMethods.Open(out device, WaveMapper, ref format, 0, 0, 0), "waveOutOpen");

        try
        {
            slots = Enumerable.Range(0, WaveOutAudioPolicy.HardwareBufferCount)
                .Select(_ => new BufferSlot(samplesPerBuffer))
                .ToArray();
            if (WaveOutAudioPolicy.PrerollSilenceBufferCount >= slots.Length)
            {
                throw new InvalidOperationException(
                    "waveOut preroll must leave at least one hardware slot for emulated PCM.");
            }
            prerollSilence = new short[samplesPerBuffer];
            submissionWorker = new Thread(ProcessPendingFrames)
            {
                IsBackground = true,
                Name = "Super Metroid waveOut submission",
            };
            submissionWorker.Start();
        }
        catch (Exception allocationException)
        {
            uint closeResult = NativeMethods.Close(device);
            device = 0;
            if (closeResult != MultimediaSuccess)
            {
                throw new AggregateException(
                    "PCM buffer allocation and waveOut cleanup both failed.",
                    allocationException,
                    CreateError(closeResult, "waveOutClose after allocation failure"));
            }
            throw;
        }
    }

    /// <summary>
    /// UI-owner preflight before emulation consumes input or produces PCM. Only the UI
    /// adds frames, so the worker can only increase capacity after this observation.
    /// A failed worker remains eligible so Submit reports its original device failure
    /// through the existing recoverable audio boundary instead of freezing gameplay.
    /// </summary>
    public bool CanAcceptFrame => Volatile.Read(ref workerFailure) is not null || pendingFrames.HasCapacity;
    /// <summary>Copies one complete emulated PCM block into the bounded worker queue.</summary>
    /// <param name="samples">Interleaved stereo samples matching the configured hardware block size.</param>
    /// <exception cref="ObjectDisposedException">The device has already been disposed.</exception>
    /// <exception cref="ArgumentException">The block length differs from the configured buffer size.</exception>
    /// <exception cref="InvalidOperationException">The bounded queue cannot accept the frame or the worker failed.</exception>
    public void Submit(ReadOnlySpan<short> samples)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ThrowWorkerFailure();
        if (samples.Length != slots[0].Samples.Length)
            throw new ArgumentException("PCM block does not match the device buffer size.", nameof(samples));

        short[] copy = ArrayPool<short>.Shared.Rent(samples.Length);
        samples.CopyTo(copy);
        var queued = new QueuedPcmFrame(
            copy,
            samples.Length,
            Volatile.Read(ref queueGeneration));
        if (pendingFrames.TryAdd(queued))
        {
            Interlocked.Increment(ref enqueuedFrames);
            return;
        }

        ArrayPool<short>.Shared.Return(copy, clearArray: false);
        ThrowWorkerFailure();
        throw new InvalidOperationException(
            $"waveOut worker queue still owns all " +
            $"{WaveOutAudioPolicy.ManagedQueueCapacityFrames} managed PCM frames; " +
            "refusing to drop the newest emulated audio frame silently.");
    }

    /// <summary>
    /// Owns all normally paced waveOut calls. Any failure is captured with its original
    /// stack and rethrown by the next UI-thread Submit, Reset, or Dispose operation.
    /// </summary>
    private void ProcessPendingFrames()
    {
        try
        {
            foreach (QueuedPcmFrame queued in pendingFrames.GetConsumingEnumerable())
            {
                try
                {
                    lock (deviceGate)
                    {
                        // Reset increments the generation before discarding queued audio.
                        // A frame already removed by the worker must receive the same test
                        // after acquiring the device gate or it could sound after Pause.
                        if (queued.Generation == queueGeneration)
                        {
                            // Read ownership only on the device owner, never by taking the
                            // potentially pacing deviceGate from the UI diagnostics path.
                            int nativeQueued = 0;
                            foreach (BufferSlot slot in slots)
                                if (slot.Prepared && !slot.IsDone) nativeQueued++;
                            queueHealth.Observe(nativeQueued, prerollRequired);
                            PrimeDeviceBeforeFirstFrame();
                            SubmitToDevice(queued.Samples.AsSpan(0, queued.SampleCount));
                        }
                    }
                    Interlocked.Increment(ref completedFrames);
                }
                finally
                {
                    ArrayPool<short>.Shared.Return(queued.Samples, clearArray: false);
                }
            }
        }
        catch (Exception exception)
        {
            Volatile.Write(ref workerFailure, ExceptionDispatchInfo.Capture(exception));
        }
        finally
        {
            ReturnPendingFramesToPool();
        }
    }

    /// <summary>Copies one queued block into the next native buffer and transfers ownership.</summary>
    private void SubmitToDevice(ReadOnlySpan<short> samples)
    {
        BufferSlot available = TakeAvailableSlot();

        if (available.Prepared)
        {
            ThrowOnError(
                NativeMethods.UnprepareHeader(device, available.Header, WaveHeader.Size),
                "waveOutUnprepareHeader");
            available.Prepared = false;
        }

        if (volumePercent == 100)
        {
            samples.CopyTo(available.Samples);
        }
        else
        {
            for (int index = 0; index < samples.Length; index++)
                available.Samples[index] = unchecked((short)(samples[index] * volumePercent / 100));
        }
        available.ResetHeader();
        ThrowOnError(
            NativeMethods.PrepareHeader(device, available.Header, WaveHeader.Size),
            "waveOutPrepareHeader");
        available.Prepared = true;
        ThrowOnError(NativeMethods.Write(device, available.Header, WaveHeader.Size), "waveOutWrite");
    }

    /// <summary>
    /// Establishes a small playback lead before the first real block. Merely allocating six
    /// native buffers provides no tolerance: without this fill, the device starts consuming
    /// the sole submitted 16.7-ms frame immediately while the WinForms producer runs at the
    /// same average cadence. A few silent frames absorb ordinary scheduling/RDP jitter and
    /// preserve every later emulated sample instead of repeating or dropping audio.
    /// </summary>
    private void PrimeDeviceBeforeFirstFrame()
    {
        if (!prerollRequired)
            return;
        for (int index = 0; index < WaveOutAudioPolicy.PrerollSilenceBufferCount; index++)
            SubmitToDevice(prerollSilence);
        prerollRequired = false;
    }

    /// <summary>
    /// Returns the next free ring slot, applying host-audio pacing when every slot is queued.
    /// </summary>
    private BufferSlot TakeAvailableSlot()
    {
        BufferSlot? available = TryTakeAvailableSlot();
        if (available is not null)
            return available;

        // The translated game can produce several frames during one WinForms catch-up tick.
        // Once the roughly 100-ms native queue is full, waveOut paces this worker alone;
        // the WinForms producer continues through the bounded managed queue above. Polling
        // is deliberate here: WHDR_DONE is the ownership bit documented by waveOut, and a
        // one-millisecond sleep avoids burning a core while retaining much finer resolution
        // than one 60-Hz emulated frame.
        Stopwatch timeout = Stopwatch.StartNew();
        do
        {
            Thread.Sleep(1);
            available = TryTakeAvailableSlot();
            if (available is not null)
                return available;
        }
        while (timeout.ElapsedMilliseconds < WaveOutAudioPolicy.BufferReturnTimeoutMilliseconds);

        string headerFlags = string.Join(
            ", ",
            slots.Select((slot, index) => $"{index}:${slot.Flags:X8}"));
        throw new TimeoutException(
            $"waveOut did not return any of its {slots.Length} PCM buffers within " +
            $"{WaveOutAudioPolicy.BufferReturnTimeoutMilliseconds} ms " +
            $"(header flags: {headerFlags}).");
    }

    /// <summary>
    /// Scans one complete ring rotation and advances the cursor past every examined slot.
    /// A slot was never submitted when it is unprepared; otherwise WHDR_DONE transfers its
    /// storage back from Windows and makes it safe to unprepare and overwrite.
    /// </summary>
    private BufferSlot? TryTakeAvailableSlot()
    {
        for (int checkedSlots = 0; checkedSlots < slots.Length; checkedSlots++)
        {
            BufferSlot slot = slots[nextSlot];
            nextSlot = (nextSlot + 1) % slots.Length;
            if (!slot.Prepared || slot.IsDone)
                return slot;
        }
        return null;
    }

    /// <summary>Discards queued audio, resets native buffers, and requires a fresh silent lead-in.</summary>
    /// <exception cref="ObjectDisposedException">The device has already been disposed.</exception>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ThrowWorkerFailure();
        Interlocked.Increment(ref queueGeneration);
        ReturnPendingFramesToPool();
        lock (deviceGate)
        {
            ResetDeviceBuffers();
            prerollRequired = true;
        }
        ThrowWorkerFailure();
    }

    /// <summary>Resets only native ownership; callers serialize this against the worker.</summary>
    private void ResetDeviceBuffers()
    {
        ThrowOnError(NativeMethods.Reset(device), "waveOutReset");
        foreach (BufferSlot slot in slots)
        {
            if (!slot.Prepared)
                continue;
            ThrowOnError(
                NativeMethods.UnprepareHeader(device, slot.Header, WaveHeader.Size),
                "waveOutUnprepareHeader");
            slot.Prepared = false;
        }
        nextSlot = 0;
    }

    /// <summary>Stops the worker, releases pinned buffers, and closes the Windows device.</summary>
    /// <exception cref="AggregateException">One or more worker or native cleanup operations failed.</exception>
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        if (device != 0)
        {
            var failures = new List<Exception>();
            Interlocked.Increment(ref queueGeneration);
            ReturnPendingFramesToPool();
            pendingFrames.CompleteAdding();
            submissionWorker.Join();
            if (Volatile.Read(ref workerFailure) is { } failure)
                failures.Add(failure.SourceException);

            lock (deviceGate)
            {
                RecordFailure(
                    failures,
                    NativeMethods.Reset(device),
                    "waveOutReset during disposal");
                foreach (BufferSlot slot in slots)
                {
                    if (slot.Prepared)
                    {
                        RecordFailure(
                            failures,
                            NativeMethods.UnprepareHeader(device, slot.Header, WaveHeader.Size),
                            "waveOutUnprepareHeader during disposal");
                    }
                    try
                    {
                        slot.Dispose();
                    }
                    catch (Exception exception)
                    {
                        failures.Add(exception);
                    }
                }
                RecordFailure(failures, NativeMethods.Close(device), "waveOutClose");
                device = 0;
            }

            if (failures.Count != 0)
            {
                throw new AggregateException(
                    "One or more waveOut disposal operations failed.",
                    failures);
            }
        }
    }

    /// <summary>Returns queued pool arrays when Reset, failure, or disposal abandons them.</summary>
    private void ReturnPendingFramesToPool()
    {
        while (pendingFrames.TryTake(out QueuedPcmFrame queued))
            ArrayPool<short>.Shared.Return(queued.Samples, clearArray: false);
    }

    /// <summary>Rethrows the worker's captured failure with its original stack trace.</summary>
    private void ThrowWorkerFailure() =>
        Volatile.Read(ref workerFailure)?.Throw();

    /// <summary>Converts a non-success waveOut result into a descriptive device exception.</summary>
    private static void ThrowOnError(uint result, string operation)
    {
        if (result != MultimediaSuccess)
            throw CreateError(result, operation);
    }

    /// <summary>Builds the shared exception representation for one failed native operation.</summary>
    private static WaveOutDeviceException CreateError(uint result, string operation) =>
        new(result, operation);

    /// <summary>Adds a native failure to cleanup results without interrupting later cleanup.</summary>
    private static void RecordFailure(List<Exception> failures, uint result, string operation)
    {
        if (result != MultimediaSuccess)
            failures.Add(CreateError(result, operation));
    }

    /// <summary>Owns one pinned sample array and its unmanaged WAVEHDR for the device lifetime.</summary>
    private sealed class BufferSlot : IDisposable
    {
        /// <summary>Keeps the sample storage at a stable address while waveOut may read it.</summary>
        private readonly GCHandle samplesHandle;

        /// <summary>Allocates pinned sample storage and an initialized unmanaged header.</summary>
        /// <param name="sampleCount">Number of 16-bit samples in this device buffer.</param>
        public BufferSlot(int sampleCount)
        {
            Samples = new short[sampleCount];
            samplesHandle = GCHandle.Alloc(Samples, GCHandleType.Pinned);
            Header = Marshal.AllocHGlobal(unchecked((int)WaveHeader.Size));
            ResetHeader();
        }

        /// <summary>Backing PCM array whose address is stored in <see cref="Header"/>.</summary>
        public short[] Samples { get; }
        /// <summary>Unmanaged WAVEHDR passed to the waveOut API.</summary>
        public nint Header { get; }
        /// <summary>Whether waveOutPrepareHeader currently owns this header.</summary>
        public bool Prepared { get; set; }
        /// <summary>Current flags read from the native header, including Windows ownership state.</summary>
        public uint Flags => Marshal.PtrToStructure<WaveHeader>(Header).Flags;
        /// <summary>True when Windows has finished consuming this slot's samples.</summary>
        public bool IsDone =>
            (Flags & HeaderDone) != 0;

        /// <summary>Reinitializes the header to reference this slot's pinned PCM storage.</summary>
        public void ResetHeader()
        {
            var header = new WaveHeader
            {
                Data = samplesHandle.AddrOfPinnedObject(),
                BufferLength = unchecked((uint)(Samples.Length * sizeof(short))),
            };
            Marshal.StructureToPtr(header, Header, fDeleteOld: false);
        }

        /// <summary>Releases the unmanaged header and then unpins the sample array.</summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(Header);
            if (samplesHandle.IsAllocated)
                samplesHandle.Free();
        }
    }

    // Keep capacity and removal in one lock. BlockingCollection.Count can decrease
    // before its free-slot semaphore is released; a preflight based on that count
    // can therefore pass immediately before TryAdd fails on the sole producer.
    /// <summary>Thread-safe bounded FIFO whose capacity and dequeue operations share one lock.</summary>
    /// <param name="capacity">Maximum number of managed PCM frames held for the worker.</param>
    private sealed class PendingFrameQueue(int capacity)
    {
        /// <summary>Protects queue contents, completion state, and capacity checks atomically.</summary>
        private readonly object sync = new();
        /// <summary>FIFO of frames accepted but not yet taken by the worker.</summary>
        private readonly Queue<QueuedPcmFrame> frames = new();
        /// <summary>Whether producers have been stopped and the consumer should exit when drained.</summary>
        private bool completed;
        /// <summary>Number of frames currently waiting for the device worker.</summary>
        internal int Count { get { lock (sync) return frames.Count; } }
        /// <summary>Whether an add can currently succeed without blocking.</summary>
        internal bool HasCapacity { get { lock (sync) return !completed && frames.Count < capacity; } }

        /// <summary>Adds a frame unless the bounded queue is full.</summary>
        internal bool TryAdd(QueuedPcmFrame frame)
        {
            lock (sync)
            {
                if (completed) throw new InvalidOperationException("The audio submission queue is closed.");
                if (frames.Count == capacity) return false;
                frames.Enqueue(frame);
                Monitor.Pulse(sync);
                return true;
            }
        }

        /// <summary>Removes one waiting frame without blocking, for reset or failure cleanup.</summary>
        internal bool TryTake(out QueuedPcmFrame frame)
        {
            lock (sync) return frames.TryDequeue(out frame);
        }

        /// <summary>Waits for frames until the queue is completed and drained.</summary>
        internal IEnumerable<QueuedPcmFrame> GetConsumingEnumerable()
        {
            while (true)
            {
                QueuedPcmFrame frame;
                lock (sync)
                {
                    while (frames.Count == 0 && !completed) Monitor.Wait(sync);
                    if (!frames.TryDequeue(out frame)) yield break;
                }
                yield return frame;
            }
        }

        /// <summary>Rejects future additions and wakes a worker waiting on an empty queue.</summary>
        internal void CompleteAdding()
        {
            lock (sync)
            {
                completed = true;
                Monitor.PulseAll(sync);
            }
        }
    }

    /// <summary>A pooled PCM block plus its length and reset generation.</summary>
    /// <param name="Samples">Pooled array containing the copied interleaved samples.</param>
    /// <param name="SampleCount">Number of valid samples in the pooled array.</param>
    /// <param name="Generation">Queue generation used to discard frames invalidated by reset.</param>
    private readonly record struct QueuedPcmFrame(
        short[] Samples,
        int SampleCount,
        long Generation);

    [StructLayout(LayoutKind.Sequential)]
    /// <summary>Native PCM format structure passed to <c>waveOutOpen</c>.</summary>
    private struct WaveFormat
    {
        /// <summary>Windows format tag; one denotes integer PCM.</summary>
        public ushort FormatTag;
        /// <summary>Number of interleaved audio channels.</summary>
        public ushort Channels;
        /// <summary>Sample rate in samples per second.</summary>
        public uint SamplesPerSecond;
        /// <summary>Required average byte rate derived from rate, channels, and sample width.</summary>
        public uint AverageBytesPerSecond;
        /// <summary>Bytes in one complete interleaved sample frame.</summary>
        public ushort BlockAlign;
        /// <summary>Bits in each channel sample.</summary>
        public ushort BitsPerSample;
        /// <summary>Additional format bytes, zero for PCM.</summary>
        public ushort ExtraSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    /// <summary>Native buffer descriptor whose ownership flags are updated by Windows.</summary>
    private struct WaveHeader
    {
        /// <summary>Pointer to the pinned PCM bytes supplied to waveOut.</summary>
        public nint Data;
        /// <summary>PCM buffer size in bytes.</summary>
        public uint BufferLength;
        /// <summary>Bytes recorded by input devices; unused for output.</summary>
        public uint BytesRecorded;
        /// <summary>Application-defined value; left zero by this output device.</summary>
        public nuint User;
        /// <summary>Native ownership and completion flags.</summary>
        public uint Flags;
        /// <summary>Loop count for looping output; unused for one-shot PCM blocks.</summary>
        public uint Loops;
        /// <summary>Driver-linked next header pointer, maintained by Windows.</summary>
        public nint Next;
        /// <summary>Reserved by the multimedia driver.</summary>
        public nuint Reserved;

        /// <summary>Native structure size required by the waveOut header calls.</summary>
        public static readonly uint Size = unchecked((uint)Marshal.SizeOf<WaveHeader>());
    }

    /// <summary>Source-generated imports for the Windows multimedia waveOut API.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Opens an output device using the requested PCM format and callback settings.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutOpen")]
        internal static partial uint Open(
            out nint device,
            uint deviceId,
            ref WaveFormat format,
            nint callback,
            nint instance,
            uint flags);

        /// <summary>Prepares a header before submitting its buffer to the driver.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutPrepareHeader")]
        internal static partial uint PrepareHeader(nint device, nint header, uint headerSize);

        /// <summary>Releases a completed prepared header for reuse or disposal.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader")]
        internal static partial uint UnprepareHeader(nint device, nint header, uint headerSize);

        /// <summary>Queues a prepared PCM buffer for playback.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutWrite")]
        internal static partial uint Write(nint device, nint header, uint headerSize);

        /// <summary>Stops playback and returns queued headers to the application.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutReset")]
        internal static partial uint Reset(nint device);

        /// <summary>Closes an output device after its submitted headers are released.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutClose")]
        internal static partial uint Close(nint device);
    }
}
