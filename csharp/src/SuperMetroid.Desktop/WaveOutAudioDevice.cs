using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop;

/// <summary>Small nonblocking PCM queue over Windows' built-in waveOut device.</summary>
/// <remarks>
/// Six pinned one-frame buffers provide roughly 100 ms of scheduler/debugger tolerance.
/// Exhausting all six is an observable host scheduling/audio failure. It throws instead of
/// silently dropping a cartridge audio frame and allowing A/V state to appear healthy.
/// </remarks>
internal sealed class WaveOutAudioDevice : IDisposable
{
    private const uint WaveMapper = uint.MaxValue;
    private const uint HeaderDone = 0x0000_0001;
    private const uint MultimediaSuccess = 0;
    private const int BufferCount = 6;

    private readonly BufferSlot[] slots;
    private readonly int volumePercent;
    private nint device;
    private int nextSlot;
    private bool disposed;

    public WaveOutAudioDevice(
        int sampleRate,
        int channelCount,
        int samplesPerBuffer,
        int volumePercent = 100)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
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
            slots = Enumerable.Range(0, BufferCount)
                .Select(_ => new BufferSlot(samplesPerBuffer))
                .ToArray();
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
        BufferSlot? available = null;
        for (int checkedSlots = 0; checkedSlots < slots.Length; checkedSlots++)
        {
            BufferSlot slot = slots[nextSlot];
            nextSlot = (nextSlot + 1) % slots.Length;
            if (!slot.Prepared || slot.IsDone)
            {
                available = slot;
                break;
            }
        }

        if (available is null)
        {
            throw new InvalidOperationException(
                $"waveOut still owns all {slots.Length} PCM buffers; refusing to drop " +
                "the newest emulated audio frame silently.");
        }
        if (samples.Length != available.Samples.Length)
            throw new ArgumentException("PCM block does not match the device buffer size.", nameof(samples));

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

    public void Reset()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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
            if (failures.Count != 0)
            {
                throw new AggregateException(
                    "One or more waveOut disposal operations failed.",
                    failures);
            }
        }
    }

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
        public bool IsDone =>
            (Marshal.PtrToStructure<WaveHeader>(Header).Flags & HeaderDone) != 0;

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

    private static class NativeMethods
    {
        [DllImport("winmm.dll", EntryPoint = "waveOutOpen")]
        internal static extern uint Open(
            out nint device,
            uint deviceId,
            ref WaveFormat format,
            nint callback,
            nint instance,
            uint flags);

        [DllImport("winmm.dll", EntryPoint = "waveOutPrepareHeader")]
        internal static extern uint PrepareHeader(nint device, nint header, uint headerSize);

        [DllImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader")]
        internal static extern uint UnprepareHeader(nint device, nint header, uint headerSize);

        [DllImport("winmm.dll", EntryPoint = "waveOutWrite")]
        internal static extern uint Write(nint device, nint header, uint headerSize);

        [DllImport("winmm.dll", EntryPoint = "waveOutReset")]
        internal static extern uint Reset(nint device);

        [DllImport("winmm.dll", EntryPoint = "waveOutClose")]
        internal static extern uint Close(nint device);
    }
}
