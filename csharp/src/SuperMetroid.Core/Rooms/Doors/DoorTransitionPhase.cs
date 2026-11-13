namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The current stage of an ordinary door transition, in the order the transition runs them.</summary>
public enum DoorTransitionPhase
{
    /// <summary>No door transition has begun.</summary>
    Inactive,
    /// <summary>Wait for all three sound-effect queues to drain.</summary>
    WaitForSoundQueues,
    /// <summary>Fade the source-room palette while retaining required HUD colors.</summary>
    FadeOutSourcePalette,
    /// <summary>Stop the room's per-line effects and give the screen to the door transition's display.</summary>
    BeginDoorDisplay,
    /// <summary>Move the source camera one pixel per update to the required screen boundary.</summary>
    AlignSourceCamera,
    /// <summary>Spend the destination-setup wait's updates (<see cref="DoorDestinationSetupWait"/>).</summary>
    DestinationSetupWait,
    /// <summary>Move Samus and the camera by the door direction's step to where the door's scroll starts.</summary>
    MoveToDoorScrollStart,
    /// <summary>Place Samus in the destination room's coordinates and take the scroll's first step.</summary>
    PlaceSamusInDestination,
    /// <summary>Load the destination room and begin its door-opening scroll.</summary>
    LoadDestinationRoom,
    /// <summary>Wait for the incremental destination door scroll to finish.</summary>
    WaitForDoorOpeningScroll,
    /// <summary>Wait one update after the destination's room objects run, then align Samus with the doorway.</summary>
    AlignSamusWithDoorway,
    /// <summary>Run the destination animated-tile objects once after loading.</summary>
    StepDestinationAnimatedTiles,
    /// <summary>Wait for the global music queue to drain before selecting room music.</summary>
    WaitForMusicQueue,
    /// <summary>Finish the opening scroll and return the screen to the destination room's display.</summary>
    FinishDoorOpeningScroll,
    /// <summary>Build the first destination actor frame before applying a fade step.</summary>
    DrawFirstDestinationFrame,
    /// <summary>Run destination actors while fading the loaded room palette in.</summary>
    FadeInDestinationPalette,
    /// <summary>The transition is finished and ordinary gameplay may resume.</summary>
    Complete,
}
