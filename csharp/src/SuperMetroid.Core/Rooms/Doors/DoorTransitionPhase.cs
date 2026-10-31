namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Identifies the current stage of an ordinary door transition.</summary>
public enum DoorTransitionPhase
{
    /// <summary>No door transition has begun.</summary>
    Inactive,
    /// <summary>Wait for all three sound-effect queues to drain.</summary>
    WaitForSoundQueues,
    /// <summary>Fade the source-room palette while retaining required HUD colors.</summary>
    FadeOutSourcePalette,
    /// <summary>Read the door's header, stop the per-line effects and give the screen to the door transition's display.</summary>
    LoadDoorHeader,
    /// <summary>Move the source camera one pixel per update to the required screen boundary.</summary>
    AlignSourceCamera,
    /// <summary>The update in which an upward-moving door corrects the source room's top tile row.</summary>
    FixDoorsMovingUp,
    /// <summary>The room-setup update before scrolling setup.</summary>
    SetupNewRoom,
    /// <summary>Apply the door direction's pre-loading player movement.</summary>
    SetupScrolling,
    /// <summary>Place Samus for destination tile loading.</summary>
    PlaceSamusAndLoadTiles,
    /// <summary>Construct the destination room and begin its door-opening scroll.</summary>
    LoadMoreThingsAndOpenDoor,
    /// <summary>Wait for the incremental destination door scroll to finish.</summary>
    WaitForDoorOpeningScroll,
    /// <summary>Run the destination animated-tile objects once after loading.</summary>
    HandleAnimatedTiles,
    /// <summary>Wait for the global music queue to drain before selecting room music.</summary>
    WaitForMusicQueue,
    /// <summary>Finish the opening scroll and return the screen to the destination room's display.</summary>
    HandleTransition,
    /// <summary>Run destination actors while fading the loaded room palette in.</summary>
    FadeInDestinationPalette,
    /// <summary>The transition is finished and ordinary gameplay may resume.</summary>
    Complete,
    /// <summary>Build the first destination actor frame before applying a fade step.</summary>
    /// <remarks>Append to preserve numeric identities already stored in debugger states.</remarks>
    DrawFirstDestinationFrame,
    /// <summary>Yield once after destination PLM processing, then align Samus.</summary>
    FinishDoorLoading,
}
