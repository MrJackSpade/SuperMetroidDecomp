namespace SuperMetroid.Core.Audio;

/// <summary>
/// Platform-independent cartridge command/acknowledgement adapter. Hosts supply a PCM
/// sink; the driver, sample selection, pitch, envelope and echo remain shared C# behavior.
/// One owner must call this object in emulated-frame order, never from a paint callback.
/// </summary>
public sealed class CartridgeAudioRenderer
{
    /// <summary>Host PCM output rate of 48,000 stereo frames per second, after resampling the native 32-kHz DSP output.</summary>
    public const int SampleRate = 48_000;
    /// <summary>Two interleaved PCM channels per stereo frame: left followed by right.</summary>
    public const int ChannelCount = 2;
    /// <summary>Eight hundred stereo frames generated for each 60-Hz emulation update, yielding 1,600 signed 16-bit channel samples.</summary>
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    /// <summary>Upload and decoded-definition catalog shared by each command dispatch.</summary>
    private readonly ExtractedAudioAssetCatalog assets;
    /// <summary>Reusable interleaved output storage returned by <see cref="RenderFrame"/>.</summary>
    private readonly short[] samples = new short[StereoFramesPerVideoFrame * ChannelCount];

    /// <summary>Creates the cartridge-command adapter and its reusable PCM buffer, retaining an existing player state or creating a new managed APU.</summary>
    /// <param name="assets">Shared extracted upload, instrument, music, sound-program, and sample definitions used when commands select an upload.</param>
    /// <param name="player">Existing managed player retained without cloning or resetting, such as restored debugger audio state; null creates a fresh player.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assets"/> is null.</exception>
    public CartridgeAudioRenderer(ExtractedAudioAssetCatalog assets, ManagedSpcPlayer? player = null)
    {
        this.assets = assets ?? throw new ArgumentNullException(nameof(assets));
        Player = player ?? new ManagedSpcPlayer();
    }

    /// <summary>The exact managed APU graph, included in debugger state captures.</summary>
    public ManagedSpcPlayer Player { get; }

    /// <summary>Applies ordered cartridge uploads and CPU-to-APU port writes, then advances the player by one 60-Hz update and renders host PCM.</summary>
    /// <param name="commands">Non-null command sequence applied in its supplied order before synthesis; an empty sequence still advances audio.</param>
    /// <returns>The renderer-owned array of 1,600 interleaved signed 16-bit left/right samples at 48 kHz, borrowed only until the next call; sinks must consume or copy it.</returns>
    /// <remarks>Uploads install their decoded instrument, music, sound-effect, and sample definitions after the opaque upload data. Processing is not transactional: a failing command may leave earlier commands or upload steps applied.</remarks>
    /// <exception cref="InvalidDataException">A command kind is unknown, its upload identity or data is invalid, or the player cannot synthesize its current driver state.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A port-write command selects a port outside 0..3.</exception>
    public short[] RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    Player.Upload(assets.GetUpload(command.UploadAddress).Span);
                    Player.ApplyInstrumentDefinitions(
                        assets.GetInstrumentBank(command.UploadAddress));
                    Player.ApplyMusicDefinitions(assets.GetMusicBank(command.UploadAddress));
                    Player.ApplySoundEffectDefinitions(assets.SoundPrograms, assets.SoundLibraries);
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

    /// <summary>Snapshots the current SPC-to-CPU acknowledgement bytes corresponding to APU ports $2140-$2143, without advancing synthesis or consuming them.</summary>
    /// <returns>Value-copied output-port bytes in port order 0 through 3, ready for the cartridge command/acknowledgement owner.</returns>
    public CartridgeAudioAcknowledgements ReadAcknowledgements() => new(
        Player.ReadPort(0), Player.ReadPort(1), Player.ReadPort(2), Player.ReadPort(3));
}
