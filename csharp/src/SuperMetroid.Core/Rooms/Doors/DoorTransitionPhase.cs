namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Identifies the current stage of the ordinary non-elevator door coroutine.</summary>
public enum DoorTransitionPhase
{
    /// <summary>No door transition has begun.</summary>
    Inactive,
    /// <summary>Wait for all three sound-effect queues to drain.</summary>
    WaitForSoundQueues,
    /// <summary>Fade the source-room palette while retaining required HUD colors.</summary>
    FadeOutSourcePalette,
    /// <summary>Consume the door-header coroutine boundary and configure transition IRQ display.</summary>
    LoadDoorHeader,
    /// <summary>Move the source camera one pixel per update to the required screen boundary.</summary>
    AlignSourceCamera,
    /// <summary>Apply the native tile correction for upward-moving doors.</summary>
    FixDoorsMovingUp,
    /// <summary>Preserve the native room-setup coroutine boundary before scrolling setup.</summary>
    SetupNewRoom,
    /// <summary>Apply the door direction's pre-loading player movement.</summary>
    SetupScrolling,
    /// <summary>Place Samus for destination tile loading.</summary>
    PlaceSamusAndLoadTiles,
    /// <summary>Construct the destination room and begin its door-opening IRQ scroll.</summary>
    LoadMoreThingsAndOpenDoor,
    /// <summary>Wait for the incremental destination door scroll to finish.</summary>
    WaitForDoorOpeningScroll,
    /// <summary>Run the destination animated-tile objects once after loading.</summary>
    HandleAnimatedTiles,
    /// <summary>Wait for the global music queue to drain before selecting room music.</summary>
    WaitForMusicQueue,
    /// <summary>Finish the opening scroll and release transition IRQ display ownership.</summary>
    HandleTransition,
    /// <summary>Run destination actors while fading the loaded room palette in.</summary>
    FadeInDestinationPalette,
    /// <summary>The transition is finished and ordinary gameplay may resume.</summary>
    Complete,
    /// <summary>Build the first destination actor frame before applying a fade step.</summary>
    /// <remarks>Append to preserve numeric identities already stored in debugger states.</remarks>
    BuildDestinationOam,
    /// <summary>Yield once after destination PLM processing, then align Samus.</summary>
    FinishDoorLoading,
}
