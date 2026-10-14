using SuperMetroid.Core;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop.Audio;

/// <summary>Small bounded PCM queue over Windows' built-in waveOut device.</summary>
/// <remarks>
/// Six pinned one-frame buffers provide roughly 100 ms of device tolerance. A bounded
/// managed queue transfers pacing to one background worker so waveOut cannot block WinForms
/// painting. Queue exhaustion and worker/device failure remain loud on the submitting thread.
/// </remarks>
internal sealed partial class WaveOutAudioDevice : IHostAudioOutput
{
    /// <summary>Windows mapper ID selecting the default wave output device.</summary>
    private const uint WaveMapper = uint.MaxValue;
    /// <summary>Wave header flag set after Windows finishes playing a buffer.</summary>
    private const uint HeaderDone = 0x0000_0001;
    /// <summary>WinMM result code indicating a completed device operation.</summary>
    private const uint MultimediaSuccess = 0;
    /// <summary>Pinned hardware buffers submitted in a rotating sequence.</summary>
    internal readonly BufferSlot[] slots = [];
    /// <summary>Silent PCM submitted before resumed gameplay frames when preroll is required.</summary>
    private readonly short[] prerollSilence = [];
    /// <summary>Requested output gain applied to submitted samples.</summary>
    private readonly int volumePercent;
    /// <summary>Bounded producer-to-worker queue of copied PCM frames.</summary>
    private readonly PendingFrameQueue pendingFrames = new(
        WaveOutAudioPolicy.ManagedQueueCapacityFrames);
    /// <summary>Serializes WinMM operations with reset and disposal.</summary>
    internal readonly Lock deviceGate = new();
    /// <summary>Background owner of blocking waveOut buffer submission.</summary>
    private readonly Thread submissionWorker;
    /// <summary>Open WinMM wave output handle, or zero after cleanup.</summary>
    private nint device;
    /// <summary>Next pinned hardware slot to receive a PCM frame.</summary>
    private int nextSlot;
    /// <summary>Changes on reset so queued frames from an earlier run can be discarded.</summary>
    private long queueGeneration;
    /// <summary>Number of managed frames admitted to the submission queue.</summary>
    internal long enqueuedFrames;
    /// <summary>Number of frames completed by the submission worker.</summary>
    internal long completedFrames;
    /// <summary>Original worker exception rethrown on the UI submission thread.</summary>
    private ExceptionDispatchInfo? workerFailure;
    /// <summary>Requests silent hardware-buffer preroll after startup or reset.</summary>
    private bool prerollRequired = true;
    /// <summary>Marks the device closed after worker and hardware cleanup.</summary>
    private bool disposed;
    /// <summary>Collects bounded-queue telemetry for diagnostics.</summary>
    private readonly WaveOutQueueHealth queueHealth = new();
    /// <summary>Current queue occupancy and recorded submission health.</summary>
    public WaveOutQueueHealthSnapshot QueueHealth => queueHealth.Snapshot(pendingFrames.Count);

    /// <summary>Opens the default stereo PCM device and starts its buffer submission worker.</summary>
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
    /// <summary>Copies one frame into the bounded queue, reporting backlog or worker failure to the caller.</summary>
    public void Submit(ReadOnlySpan<short> samples)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ThrowWorkerFailure();
        Ensure.LengthEqual(samples, slots[0].Samples.Length);

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

    /// <summary>Flushes the device and pending frames, then marks the next submission for silent preroll.</summary>
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

    /// <summary>Stops the worker, releases pinned buffers, and closes the WinMM handle.</summary>
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

    /// <summary>Rethrows a captured worker error on the caller's thread.</summary>
    internal void ThrowWorkerFailure() =>
        Volatile.Read(ref workerFailure)?.Throw();

    /// <summary>Raises a device exception when a WinMM call does not report success.</summary>
    private static void ThrowOnError(uint result, string operation)
    {
        if (result != MultimediaSuccess)
            throw CreateError(result, operation);
    }

    /// <summary>Pairs a WinMM result code with the failed native operation.</summary>
    private static WaveOutDeviceException CreateError(uint result, string operation) =>
        new(result, operation);

    /// <summary>Adds cleanup failures without hiding an earlier device error.</summary>
    private static void RecordFailure(List<Exception> failures, uint result, string operation)
    {
        if (result != MultimediaSuccess)
            failures.Add(CreateError(result, operation));
    }

    /// <summary>Owns one pinned PCM array and the unmanaged wave header that references it.</summary>
    internal sealed class BufferSlot : IDisposable
    {
        /// <summary>Pin keeping the sample address stable while WinMM owns the header.</summary>
        private readonly GCHandle samplesHandle;

        /// <summary>Allocates one sample buffer and initializes its native header.</summary>
        public BufferSlot(int sampleCount)
        {
            Samples = new short[sampleCount];
            samplesHandle = GCHandle.Alloc(Samples, GCHandleType.Pinned);
            Header = Marshal.AllocHGlobal(unchecked((int)WaveHeader.Size));
            ResetHeader();
        }

        /// <summary>PCM storage copied into this hardware slot before submission.</summary>
        public short[] Samples { get; }
        /// <summary>Unmanaged pointer passed to WinMM for this slot.</summary>
        public nint Header { get; }
        /// <summary>Whether WinMM has prepared the header for writing.</summary>
        public bool Prepared { get; set; }
        /// <summary>Current native completion flags from the unmanaged header.</summary>
        public uint Flags => Marshal.PtrToStructure<WaveHeader>(Header).Flags;
        /// <summary>Whether the native completion flag permits this slot to be reused.</summary>
        public bool IsDone =>
            (Flags & HeaderDone) != 0;

        /// <summary>Repoints the header at pinned PCM storage and clears prior native flags.</summary>
        public void ResetHeader()
        {
            var header = new WaveHeader
            {
                Data = samplesHandle.AddrOfPinnedObject(),
                BufferLength = unchecked((uint)(Samples.Length * sizeof(short))),
            };
            Marshal.StructureToPtr(header, Header, fDeleteOld: false);
        }

        /// <summary>Frees the unmanaged header and releases the sample pin.</summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(Header);
            if (samplesHandle.IsAllocated)
                samplesHandle.Free();
        }
    }

    /// <summary>Uses one lock for capacity checks and removal so producer preflight agrees with admission.</summary>
    private sealed class PendingFrameQueue(int capacity)
    {
        /// <summary>Protects queue count, completion state, and worker wakeups.</summary>
        private readonly object sync = new();
        /// <summary>Copied PCM frames waiting for the submission worker.</summary>
        private readonly Queue<QueuedPcmFrame> frames = new();
        /// <summary>Disallows new frames and lets the worker finish after draining.</summary>
        private bool completed;
        /// <summary>Current queued frame count under the same lock as admission.</summary>
        internal int Count { get { lock (sync) return frames.Count; } }
        /// <summary>Whether the sole producer can add a frame without exceeding capacity.</summary>
        internal bool HasCapacity { get { lock (sync) return !completed && frames.Count < capacity; } }

        /// <summary>Admits one copied frame and wakes the worker, or reports a full queue.</summary>
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

        /// <summary>Removes one queued frame without waiting, as used during reset and cleanup.</summary>
        internal bool TryTake(out QueuedPcmFrame frame)
        {
            lock (sync) return frames.TryDequeue(out frame);
        }

        /// <summary>Waits for frames until the queue is both completed and empty.</summary>
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

        /// <summary>Closes admission and wakes a worker waiting for another frame.</summary>
        internal void CompleteAdding()
        {
            lock (sync)
            {
                completed = true;
                Monitor.PulseAll(sync);
            }
        }
    }

    /// <summary>One pooled PCM copy tagged with its reset generation for stale-frame rejection.</summary>
    private readonly record struct QueuedPcmFrame(
        short[] Samples,
        int SampleCount,
        long Generation);

    /// <summary>Native WAVEFORMATEX layout for the device's stereo PCM stream.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormat
    {
        /// <summary>One selects uncompressed PCM.</summary>
        public ushort FormatTag;
        /// <summary>Two interleaved channels for the SNES mixer.</summary>
        public ushort Channels;
        /// <summary>Sample frames played per second.</summary>
        public uint SamplesPerSecond;
        /// <summary>Average PCM bytes consumed per second.</summary>
        public uint AverageBytesPerSecond;
        /// <summary>Bytes in one interleaved stereo sample frame.</summary>
        public ushort BlockAlign;
        /// <summary>Bits in each channel sample.</summary>
        public ushort BitsPerSample;
        /// <summary>Additional format bytes; zero for this PCM layout.</summary>
        public ushort ExtraSize;
    }

    /// <summary>Native WAVEHDR layout retained until WinMM marks a submitted buffer done.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        /// <summary>Pinned address of the PCM sample array.</summary>
        public nint Data;
        /// <summary>Number of PCM bytes submitted in this buffer.</summary>
        public uint BufferLength;
        /// <summary>Native recorded-byte count, unused for playback.</summary>
        public uint BytesRecorded;
        /// <summary>Native user data field, unused by this device.</summary>
        public nuint User;
        /// <summary>WinMM preparation and completion bits.</summary>
        public uint Flags;
        /// <summary>Playback loop count, unused for one-frame submissions.</summary>
        public uint Loops;
        /// <summary>Native linked-header pointer, unused by this device.</summary>
        public nint Next;
        /// <summary>WinMM reserved header storage.</summary>
        public nuint Reserved;

        /// <summary>Interop byte size passed with every header operation.</summary>
        public static readonly uint Size = unchecked((uint)Marshal.SizeOf<WaveHeader>());
    }

    /// <summary>WinMM imports used for device opening, buffer submission, reset, and cleanup.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Opens the default wave output device in the requested PCM format.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutOpen")]
        internal static partial uint Open(
            out nint device,
            uint deviceId,
            ref WaveFormat format,
            nint callback,
            nint instance,
            uint flags);

        /// <summary>Registers a pinned header before its first write.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutPrepareHeader")]
        internal static partial uint PrepareHeader(nint device, nint header, uint headerSize);

        /// <summary>Releases a completed header from the device.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader")]
        internal static partial uint UnprepareHeader(nint device, nint header, uint headerSize);

        /// <summary>Queues one prepared PCM header for playback.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutWrite")]
        internal static partial uint Write(nint device, nint header, uint headerSize);

        /// <summary>Stops playback and returns all pending headers to the caller.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutReset")]
        internal static partial uint Reset(nint device);

        /// <summary>Closes the device after pending headers are released.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "waveOutClose")]
        internal static partial uint Close(nint device);
    }
}
