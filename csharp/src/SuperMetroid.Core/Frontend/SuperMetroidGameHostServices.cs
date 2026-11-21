namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The compatibility services and display policy a session runs against, fixed at construction. The game uses
/// <see cref="Default"/>, the game's own timing with every gameplay frame captured;
/// another host may supply its own timing or keep the display it last showed.
/// </summary>
internal sealed record SuperMetroidGameHostServices
{
    /// <summary>The game's own timing, which every production host uses.</summary>
    public static SuperMetroidGameHostServices Default { get; } = new(
        InstantDoorMusicLoad.Instance,
        InstantRoomLoad.Instance,
        ClockedSoundAcceptance<SoundEffectLibrary, SoundEffectId>.Instance,
        CapturedGameplayFrames.Instance);

    /// <summary>The services a session runs against; the game builds only <see cref="Default"/>.</summary>
    /// <param name="doorMusicLoadFrames">Frames counted while a door's new music loads.</param>
    /// <param name="roomLoadProgress">How far room loading has progressed when each enemy initializes.</param>
    /// <param name="soundAcceptance">On which update each sound library accepts the sound the game requested.</param>
    /// <param name="gameplayFrames">How each gameplay update becomes the published display.</param>
    internal SuperMetroidGameHostServices(
        IDoorMusicLoadFrames doorMusicLoadFrames,
        IRoomLoadProgress roomLoadProgress,
        ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId> soundAcceptance,
        IGameplayFramePublication gameplayFrames)
    {
        DoorMusicLoadFrames = doorMusicLoadFrames;
        RoomLoadProgress = roomLoadProgress;
        SoundAcceptance = soundAcceptance;
        GameplayFrames = gameplayFrames;
    }

    /// <summary>Frames counted while a door's new music loads.</summary>
    public IDoorMusicLoadFrames DoorMusicLoadFrames { get; }

    /// <summary>How far room loading has progressed when each enemy initializes.</summary>
    public IRoomLoadProgress RoomLoadProgress { get; }

    /// <summary>On which update each sound library accepts the sound the game requested.</summary>
    public ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId> SoundAcceptance { get; }

    /// <summary>How each gameplay update becomes the published display.</summary>
    public IGameplayFramePublication GameplayFrames { get; }
}
