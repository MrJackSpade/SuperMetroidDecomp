namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's death phases, declared in the order the death sequence runs them for reading; a phase that finishes
/// hands over to the successor <see cref="CrocomireDeathPhases.Next"/> names. Nothing reads the values or their order.
/// Keeping the phases separate from the executor makes the authored sequence readable
/// without mixing identity declarations into the state-transition implementation.
/// </summary>
public enum CrocomireDeathPhase
{
    /// <summary>The fight and bridge checks run; the death sequence has not begun. A new body starts here.</summary>
    Fighting,
    /// <summary>Bridge crumbles while Crocomire sinks into the acid.</summary>
    CrumbleBridgeAndSink,
    /// <summary>First submerged pause.</summary>
    FirstSubmergedPause,
    /// <summary>First hop rising phase.</summary>
    FirstHopRise,
    /// <summary>First hop sinking phase.</summary>
    FirstHopSink,
    /// <summary>Second submerged pause.</summary>
    SecondSubmergedPause,
    /// <summary>Second hop rising phase.</summary>
    SecondHopRise,
    /// <summary>Second hop sinking phase.</summary>
    SecondHopSink,
    /// <summary>Shows the first melt image and the tongue/arm actor.</summary>
    InstallFirstMeltImage,
    /// <summary>Begins the first melt pass.</summary>
    BeginFirstMeltPass,
    /// <summary>Shows one band of the first melt image per frame.</summary>
    ShowFirstMeltBands,
    /// <summary>Third hop rising phase.</summary>
    ThirdHopRise,
    /// <summary>Starts the first column dissolve and the melt's per-line vertical scroll.</summary>
    StartFirstDissolve,
    /// <summary>Dissolves the first body image.</summary>
    DissolveFirstImage,
    /// <summary>Clears his body layer after the first body image is gone.</summary>
    ClearFirstMeltImage,
    /// <summary>Fourth hop sinking phase.</summary>
    FourthHopSink,
    /// <summary>Third submerged pause.</summary>
    ThirdSubmergedPause,
    /// <summary>Fourth hop rising phase.</summary>
    FourthHopRise,
    /// <summary>Fifth hop sinking phase.</summary>
    FifthHopSink,
    /// <summary>Fourth submerged pause.</summary>
    FourthSubmergedPause,
    /// <summary>Fifth hop rising phase.</summary>
    FifthHopRise,
    /// <summary>Sixth hop sinking phase.</summary>
    SixthHopSink,
    /// <summary>Shows the second melt image.</summary>
    InstallSecondMeltImage,
    /// <summary>Begins the second melt pass.</summary>
    BeginSecondMeltPass,
    /// <summary>Shows one band of the second melt image per frame.</summary>
    ShowSecondMeltBands,
    /// <summary>Waits one update after the second melt image's last band before the sixth rise.</summary>
    WaitBeforeSixthHopRise,
    /// <summary>Sixth hop rising phase.</summary>
    SixthHopRise,
    /// <summary>Starts the second column dissolve.</summary>
    StartSecondDissolve,
    /// <summary>Dissolves the second body image.</summary>
    DissolveSecondImage,
    /// <summary>Clears his body layer after the second body image is gone.</summary>
    ClearSecondMeltImage,
    /// <summary>Final sink before selecting the river-skeleton detour.</summary>
    FinalSink,
    /// <summary>Waits behind the wall for Samus to return left.</summary>
    WaitForSamusAtWall,
    /// <summary>Rumbles the hidden wall through its oscillations.</summary>
    RumbleHiddenWall,
    /// <summary>Loads the skeleton's pictures and breaks the spike wall.</summary>
    BreakSpikeWall,
    /// <summary>Eighty-frame delay before the skeleton starts falling.</summary>
    DelaySkeletonFall,
    /// <summary>Arcs the skeleton back into the arena.</summary>
    ArcSkeletonIntoArena,
    /// <summary>Waits for the falls-apart animation to reach its terminal image.</summary>
    WaitForSkeletonTerminalImage,
    /// <summary>Reopens the four left scroll cells and clears the wall.</summary>
    ClearWallAndOpenScrolls,
    /// <summary>Waits for the stable skeleton frame.</summary>
    WaitForStableSkeleton,
    /// <summary>Waits one update after the skeleton settles before recording the mini-boss's defeat.</summary>
    WaitBeforeRecordingDefeat,
    /// <summary>Records the mini-boss's defeat and restores boss music.</summary>
    PublishDefeatAndRestoreMusic,
    /// <summary>Final live-room corpse state; intentionally inert.</summary>
    InertCorpse,
    /// <summary>In the already-defeated room, waits one update after setup before pinning his body layer's placement.</summary>
    WaitBeforePinningDefeatedBodyLayer,
    /// <summary>Pins his body layer's placement at zero in the already-defeated room.</summary>
    PinDefeatedRoomBodyLayerPlacement,
    /// <summary>River detour sequenced between the final sink and wall wait.</summary>
    RiverSkeletonDetour,
}
