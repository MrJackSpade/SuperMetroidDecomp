namespace SuperMetroid.Core.Audio;

/// <summary>
/// Platform-independent cartridge command/acknowledgement adapter. Hosts supply a PCM
/// sink; the driver, sample selection, pitch, envelope and echo remain shared C# behavior.
/// One owner must call this object in emulated-frame order, never from a paint callback.
/// </summary>
public sealed class CartridgeAudioRenderer
{
    public const int SampleRate = 48_000;
    public const int ChannelCount = 2;
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    private readonly ExtractedAudioAssetCatalog assets;
    private readonly short[] samples = new short[StereoFramesPerVideoFrame * ChannelCount];

    public CartridgeAudioRenderer(ExtractedAudioAssetCatalog assets, ManagedSpcPlayer? player = null)
    {
        this.assets = assets ?? throw new ArgumentNullException(nameof(assets));
        Player = player ?? new ManagedSpcPlayer();
    }

    /// <summary>The exact managed APU graph, included in debugger state captures.</summary>
    public ManagedSpcPlayer Player { get; }

    /// <summary>The returned buffer is borrowed until the next call; sinks must copy or consume it.</summary>
    public short[] RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    Player.Upload(assets.GetUpload(command.UploadAddress).Span);
                    Player.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                    break;
                case CartridgeAudioCommandKind.WritePort:
                    Player.WritePort(command.Port, command.Value);
                    break;
                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command.Kind}.");
            }
        }
        Player.GenerateFrame(samples);
        return samples;
    }

    public CartridgeAudioAcknowledgements ReadAcknowledgements() => new(
        Player.ReadPort(0), Player.ReadPort(1), Player.ReadPort(2), Player.ReadPort(3));
}
