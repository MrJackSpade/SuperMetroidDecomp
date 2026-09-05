using System.Buffers;
using System.Collections.Concurrent;
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
internal sealed partial class WaveOutAudioDevice : IDisposable
{
    private const uint WaveMapper = uint.MaxValue;
    private const uint HeaderDone = 0x0000_0001;
    private const uint MultimediaSuccess = 0;
    private readonly BufferSlot[] slots = [];
    private readonly short[] prerollSilence = [];
    private readonly int volumePercent;
    private readonly BlockingCollection<QueuedPcmFrame> pendingFrames = new(
        WaveOutAudioPolicy.ManagedQueueCapacityFrames);
    private readonly object deviceGate = new();
    private readonly Thread submissionWorker = null!;
    private nint device;
    private int nextSlot;
    private long queueGeneration;
    private long enqueuedFrames;
    private long completedFrames;
    private ExceptionDispatchInfo? workerFailure;
    private bool prerollRequired = true;
    private bool disposed;

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
            pendingFrames.Dispose();
            if (failures.Count != 0)
            {
                throw new AggregateException(
                    "One or more waveOut disposal operations failed.",
                    failures);
            }
        }
    }

    /// <summary>
    /// Test-only synchronization boundary proving that every accepted managed block reached
    /// the waveOut worker. Normal gameplay never waits here; doing so would restore the UI
    /// pacing defect this queue exists to remove.
    /// </summary>
    internal void WaitForPendingSubmissions()
    {
        long target = Volatile.Read(ref enqueuedFrames);
        Stopwatch timeout = Stopwatch.StartNew();
        while (Volatile.Read(ref completedFrames) < target)
        {
            ThrowWorkerFailure();
            if (timeout.ElapsedMilliseconds >= WaveOutAudioPolicy.BufferReturnTimeoutMilliseconds)
            {
                throw new TimeoutException(
                    $"waveOut worker completed {Volatile.Read(ref completedFrames)} of " +
                    $"{target} accepted PCM frames within the bounded audit interval.");
            }
            Thread.Sleep(1);
        }
        ThrowWorkerFailure();
    }

    /// <summary>
    /// Number of pinned headers that have entered waveOut ownership at least once. Unlike
    /// WHDR_DONE this remains deterministic after playback consumes a short buffer, making
    /// it suitable for verifying startup preroll without racing the physical device clock.
    /// </summary>
    internal int PreparedBufferCountForVerification
    {
        get
        {
            lock (deviceGate)
                return slots.Count(slot => slot.Prepared);
        }
    }

    /// <summary>Returns queued pool arrays when Reset, failure, or disposal abandons them.</summary>
    private void ReturnPendingFramesToPool()
    {
        while (pendingFrames.TryTake(out QueuedPcmFrame queued))
            ArrayPool<short>.Shared.Return(queued.Samples, clearArray: false);
    }

    private void ThrowWorkerFailure() =>
        Volatile.Read(ref workerFailure)?.Throw();

    private static void ThrowOnError(uint result, string operation)
    {
        if (result != MultimediaSuccess)
            throw CreateError(result, operation);
    }

    private static InvalidOperationException CreateError(uint result, string operation) =>
        new($"{operation} failed with multimedia error {result}.");

    private static void RecordFailure(List<Exception> failures, uint result, string operation)
    {
        if (result != MultimediaSuccess)
            failures.Add(CreateError(result, operation));
    }

    private sealed class BufferSlot : IDisposable
    {
        private readonly GCHandle samplesHandle;

        public BufferSlot(int sampleCount)
        {
            Samples = new short[sampleCount];
            samplesHandle = GCHandle.Alloc(Samples, GCHandleType.Pinned);
            Header = Marshal.AllocHGlobal(unchecked((int)WaveHeader.Size));
            ResetHeader();
        }

        public short[] Samples { get; }
        public nint Header { get; }
        public bool Prepared { get; set; }
        public uint Flags => Marshal.PtrToStructure<WaveHeader>(Header).Flags;
        public bool IsDone =>
            (Flags & HeaderDone) != 0;

        public void ResetHeader()
        {
            var header = new WaveHeader
            {
                Data = samplesHandle.AddrOfPinnedObject(),
                BufferLength = unchecked((uint)(Samples.Length * sizeof(short))),
            };
            Marshal.StructureToPtr(header, Header, fDeleteOld: false);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Header);
            if (samplesHandle.IsAllocated)
                samplesHandle.Free();
        }
    }

    private readonly record struct QueuedPcmFrame(
        short[] Samples,
        int SampleCount,
        long Generation);

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormat
    {
        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSecond;
        public uint AverageBytesPerSecond;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort ExtraSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        public nint Data;
        public uint BufferLength;
        public uint BytesRecorded;
        public nuint User;
        public uint Flags;
        public uint Loops;
        public nint Next;
        public nuint Reserved;

        public static readonly uint Size = unchecked((uint)Marshal.SizeOf<WaveHeader>());
    }

    private static partial class NativeMethods
    {
        [LibraryImport("winmm.dll", EntryPoint = "waveOutOpen")]
        internal static partial uint Open(
            out nint device,
            uint deviceId,
            ref WaveFormat format,
            nint callback,
            nint instance,
            uint flags);

        [LibraryImport("winmm.dll", EntryPoint = "waveOutPrepareHeader")]
        internal static partial uint PrepareHeader(nint device, nint header, uint headerSize);

        [LibraryImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader")]
        internal static partial uint UnprepareHeader(nint device, nint header, uint headerSize);

        [LibraryImport("winmm.dll", EntryPoint = "waveOutWrite")]
        internal static partial uint Write(nint device, nint header, uint headerSize);

        [LibraryImport("winmm.dll", EntryPoint = "waveOutReset")]
        internal static partial uint Reset(nint device);

        [LibraryImport("winmm.dll", EntryPoint = "waveOutClose")]
        internal static partial uint Close(nint device);
    }
}
