namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The compatibility services and display policy a session runs against, fixed at construction. The game uses
/// <see cref="Default"/>, the game's own timing with every gameplay frame captured; replay verification supplies
/// recorded evidence instead, and simulation-only verification may retain the prior display.
/// </summary>
internal sealed record SuperMetroidGameHostServices
{
    /// <summary>The game's own timing, which every production host uses.</summary>
    public static SuperMetroidGameHostServices Default { get; } = new()
    {
        DoorMusicLoadFrames = InstantDoorMusicLoad.Instance,
        RoomLoadProgress = InstantRoomLoad.Instance,
        SoundAcknowledgements = ClockedSoundAcknowledgements<SoundEffectId>.Instance,
        GameplayFrames = CapturedGameplayFrames.Instance,
    };

    /// <summary>Frames counted while a door's new music loads.</summary>
    public required IDoorMusicLoadFrames DoorMusicLoadFrames { get; init; }

    /// <summary>How far room loading has progressed when each enemy initializes.</summary>
    public required IRoomLoadProgress RoomLoadProgress { get; init; }

    /// <summary>The sound acknowledgements each update's sound handlers read.</summary>
    public required ISoundAcknowledgementTiming<SoundEffectId> SoundAcknowledgements { get; init; }

    /// <summary>How each gameplay update becomes the published display.</summary>
    public required IGameplayFramePublication GameplayFrames { get; init; }
}
