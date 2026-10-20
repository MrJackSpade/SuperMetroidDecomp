using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The hardware-timing and display policies a session runs against, fixed at construction. Production uses
/// <see cref="Default"/>, which eliminates hardware lag and captures every gameplay frame; replay verification supplies
/// recorded evidence instead, and simulation-only verification may retain the prior display.
/// </summary>
internal sealed record SuperMetroidGameHostServices
{
    /// <summary>The lag-free policies every production host uses.</summary>
    public static SuperMetroidGameHostServices Default { get; } = new()
    {
        DoorMusicUploadNmis = LagFreeDoorMusicUploadNmis.Instance,
        DoorLoaderProgress = LagFreeDoorLoaderProgress.Instance,
        SoundAcknowledgements = ModeledSoundAcknowledgements.Instance,
        GameplayFrames = CapturedGameplayFrames.Instance,
    };

    /// <summary>Accepted NMIs taken while a door transition's music upload blocks the main loop.</summary>
    public required IDoorMusicUploadNmiSource DoorMusicUploadNmis { get; init; }

    /// <summary>How far the cartridge's door loader has progressed when enemies initialize.</summary>
    public required IDoorLoaderProgressSource DoorLoaderProgress { get; init; }

    /// <summary>The SPC acknowledgements each update's sound handlers read.</summary>
    public required ISoundAcknowledgementSource SoundAcknowledgements { get; init; }

    /// <summary>How each gameplay update becomes the published display.</summary>
    public required IGameplayFramePublication GameplayFrames { get; init; }
}
