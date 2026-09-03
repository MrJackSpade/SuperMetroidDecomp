using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>
/// Desktop adapter for the fully managed Super Metroid SPC driver and S-DSP mixer.
/// The adapter only resolves cartridge upload commands and owns the reusable host PCM buffer;
/// all sequencer, voice, echo, pitch, and envelope behavior lives in debuggable Core code.
/// </summary>
internal sealed class SpcAudioEngine : IDisposable
{
    public const int SampleRate = 48_000;
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    public const int ChannelCount = 2;

    private readonly ISnesAddressSpace bus;
    private readonly ManagedSpcPlayer player = new();
    private readonly short[] sampleBuffer = new short[StereoFramesPerVideoFrame * ChannelCount];
    private bool disposed;

    public SpcAudioEngine(ISnesAddressSpace bus) =>
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));

    /// <summary>Applies this NMI's APU operations, then renders one complete audio frame.</summary>
    public ReadOnlySpan<short> RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(commands);
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    player.Upload(SpcUploadStreamReader.Read(bus, command.UploadAddress));
                    break;
                case CartridgeAudioCommandKind.WritePort:
                    player.WritePort(command.Port, command.Value);
                    break;
                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command.Kind}.");
            }
        }
        player.GenerateFrame(sampleBuffer);
        return sampleBuffer;
    }

    public CartridgeAudioAcknowledgements ReadAcknowledgements() => new(
        player.ReadPort(0),
        player.ReadPort(1),
        player.ReadPort(2),
        player.ReadPort(3));

    internal byte ReadDspRegister(byte address) => player.ReadDspRegister(address);

    internal byte ReadApuRam(ushort address) => player.ReadApuRam(address);

    internal void BeginDspWriteCapture() => player.BeginDspWriteCapture();

    internal IReadOnlyList<(byte Address, byte Value)> CapturedDspWrites => player.CapturedDspWrites;

    internal IReadOnlyList<string> CapturedDriverTrace => player.CapturedDriverTrace;

    internal int ReadDebugValue(SpcAudioDebugValue value, int channel = 0) =>
        player.ReadDebugValue(value, channel);

    public void Dispose() => disposed = true;
}
