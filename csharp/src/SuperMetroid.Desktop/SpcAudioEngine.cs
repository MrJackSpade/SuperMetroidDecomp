using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop;

/// <summary>
/// Managed owner of the translated Super Metroid SPC sequencer and SNES DSP mixer.
/// </summary>
/// <remarks>
/// Gameplay never calls this class. It consumes the four-port commands emitted by Core,
/// resolves rare upload commands against the same mapped ROM, and produces exactly one
/// 48-kHz stereo block per emulated video frame. The native boundary is intentionally kept
/// here so every queue decision remains inspectable in C#.
/// </remarks>
internal sealed partial class SpcAudioEngine : IDisposable
{
    public const int SampleRate = 48_000;
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    public const int ChannelCount = 2;

    private readonly ISnesAddressSpace bus;
    private readonly short[] sampleBuffer =
        new short[StereoFramesPerVideoFrame * ChannelCount];
    private nint player;
    private bool disposed;

    public SpcAudioEngine(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        player = NativeMethods.Create();
        if (player == 0)
            throw new InvalidOperationException("The translated SPC player could not be allocated.");

        int nativeFrameSize = NativeMethods.DefaultSamplesPerFrame();
        if (nativeFrameSize != StereoFramesPerVideoFrame)
        {
            Dispose();
            throw new InvalidDataException(
                $"Native SPC bridge reports {nativeFrameSize} samples/frame; " +
                $"the desktop host requires {StereoFramesPerVideoFrame}.");
        }
    }

    /// <summary>Applies this NMI's port operations, then renders its complete audio frame.</summary>
    public ReadOnlySpan<short> RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(commands);

        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    byte[] upload = SpcUploadStreamReader.Read(bus, command.UploadAddress);
                    if (NativeMethods.Upload(player, upload, upload.Length) == 0)
                    {
                        throw new InvalidDataException(
                            $"Native SPC bridge rejected upload stream at " +
                            $"${command.UploadAddress >> 16:X2}:{command.UploadAddress & 0xffff:X4}.");
                    }
                    break;

                case CartridgeAudioCommandKind.WritePort:
                    if (NativeMethods.WritePort(player, command.Port, command.Value) == 0)
                    {
                        throw new InvalidDataException(
                            $"Native SPC bridge rejected APU port {command.Port}.");
                    }
                    break;

                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command.Kind}.");
            }
        }

        int generated = NativeMethods.GenerateFrame(
            player,
            sampleBuffer,
            StereoFramesPerVideoFrame);
        if (generated != StereoFramesPerVideoFrame)
            throw new InvalidOperationException($"Native SPC bridge generated {generated} audio frames.");
        return sampleBuffer;
    }

    /// <summary>Reads the SPC-to-65816 acknowledgement bytes after audio generation.</summary>
    public CartridgeAudioAcknowledgements ReadAcknowledgements() => new(
        ReadPort(0),
        ReadPort(1),
        ReadPort(2),
        ReadPort(3));

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        if (player != 0)
        {
            NativeMethods.Destroy(player);
            player = 0;
        }
    }

    private byte ReadPort(int port)
    {
        int value = NativeMethods.ReadPort(player, port);
        if ((uint)value > byte.MaxValue)
            throw new InvalidOperationException($"Native SPC bridge rejected output port {port}.");
        return unchecked((byte)value);
    }

    private static partial class NativeMethods
    {
        private const string LibraryName = "SuperMetroid.AudioNative";

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_create")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial nint Create();

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_destroy")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void Destroy(nint player);

        internal static unsafe int Upload(nint player, byte[] data, int length)
        {
            fixed (byte* dataPointer = data)
                return UploadNative(player, dataPointer, length);
        }

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_upload")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static unsafe partial int UploadNative(nint player, byte* data, int length);

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_write_port")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial int WritePort(nint player, int port, byte value);

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_read_port")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial int ReadPort(nint player, int port);

        internal static unsafe int GenerateFrame(
            nint player,
            short[] samples,
            int stereoFrameCount)
        {
            fixed (short* samplesPointer = samples)
                return GenerateFrameNative(player, samplesPointer, stereoFrameCount);
        }

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_generate_frame")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static unsafe partial int GenerateFrameNative(
            nint player,
            short* samples,
            int stereoFrameCount);

        [LibraryImport(LibraryName, EntryPoint = "sm_audio_default_samples_per_frame")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial int DefaultSamplesPerFrame();
    }
}
