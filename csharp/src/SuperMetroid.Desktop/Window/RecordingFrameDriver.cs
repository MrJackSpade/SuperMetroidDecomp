using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Diagnostics.Saves;

namespace SuperMetroid.Desktop.Window;

/// <summary>
/// Replays a <see cref="ControllerInputRecording"/> through the host: the session starts from the
/// recording's save RAM and takes the recorded controller buttons for each update. The recording's saves are
/// never written back.
/// </summary>
internal sealed class RecordingFrameDriver : IHostFrameDriver
{
    /// <summary>The immutable recording supplying the save RAM seed and every frame's input.</summary>
    private readonly ControllerInputRecording recording;
    /// <summary>The game created by the latest start.</summary>
    private SuperMetroidGame? game;
    /// <summary>Drives the host from <paramref name="recording"/>.</summary>
    internal RecordingFrameDriver(ControllerInputRecording recording) => this.recording = Ensure.NotNull(recording);

    /// <inheritdoc/>
    public SuperMetroidGame Game => game ?? throw new InvalidOperationException("The recording replay has not started.");

    /// <inheritdoc/>
    public int InputsSupplied { get; private set; }

    /// <inheritdoc/>
    public string Progress => $"replay {InputsSupplied}/{recording.ControllerInputs.Length}";

    /// <inheritdoc/>
    public HostFramePacing Pacing => HostFramePacing.WallClock;

    /// <summary>Rejects a different ROM revision, reports content differences, then creates the game from the recording's saves.</summary>
    public SuperMetroidGame Start(HostSessionStart start)
    {
        Ensure.NotNull(start);
        InputsSupplied = 0;
        SaveSlotStore saves = DecodeSaves(start.Maps);
        ReportContentCompatibility(start.ContentIdentity);
        game = GameSessionComposer.Create(saves, start.Assists, start.Content, storage: null);
        return game;
    }

    /// <inheritdoc/>
    public ControllerButton? NextInput() =>
        InputsSupplied < recording.ControllerInputs.Length ? recording.InputAt(InputsSupplied++) : null;

    /// <summary>A recording has nothing to compare; the frame needs no further work.</summary>
    public void FrameCompleted(FrontendFrame frame)
    {
    }

    /// <summary>
    /// Decodes the recording's cartridge-compatible SRAM image into the session's saves and rejects a
    /// different ROM revision before the first input can produce a misleading divergence.
    /// </summary>
    private SaveSlotStore DecodeSaves(AreaMapPresentation maps)
    {
        byte[] actualDigest = SupportedCartridge.CreateSha256Digest();
        if (!CryptographicOperations.FixedTimeEquals(actualDigest, recording.RomSha256))
        {
            throw new InvalidDataException(
                "The replay was recorded from a different ROM image (SHA-256 mismatch).");
        }
        return SaveRamCodec.Decode(recording.InitialSaveRam, maps);
    }

    /// <summary>Writes installed-content differences against the recording before playback.</summary>
    private void ReportContentCompatibility(GameContentIdentity contentIdentity)
    {
        foreach (string warning in contentIdentity.GetCompatibilityWarnings(
                     recording.ContentIdentity,
                     "controller recording"))
        {
            Console.WriteLine($"REPLAY COMPATIBILITY WARNING: {warning}");
        }
    }
}
