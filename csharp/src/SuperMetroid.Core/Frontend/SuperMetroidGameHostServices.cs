namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The compatibility services and display policy a session runs against, fixed at construction. The game uses
/// <see cref="Default"/>, the game's own timing with every gameplay frame captured, which each service starts as;
/// another host may supply its own timing or keep the display it last showed.
/// </summary>
internal sealed record SuperMetroidGameHostServices
{
    /// <summary>The game's own timing, which every production host uses.</summary>
    public static SuperMetroidGameHostServices Default { get; } = new();

    /// <summary>Frames counted while a door's new music loads.</summary>
    public IDoorMusicLoadFrames DoorMusicLoadFrames { get; private init; } = InstantDoorMusicLoad.Instance;

    /// <summary>How far room loading has progressed when each enemy initializes.</summary>
    public IRoomLoadProgress RoomLoadProgress { get; init; } = InstantRoomLoad.Instance;

    /// <summary>On which update each sound library accepts the sound the game requested.</summary>
    public ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId> SoundAcceptance { get; init; } =
        ClockedSoundAcceptance<SoundEffectLibrary, SoundEffectId>.Instance;

    /// <summary>How each gameplay update becomes the published display.</summary>
    public IGameplayFramePublication GameplayFrames { get; init; } = CapturedGameplayFrames.Instance;
}
