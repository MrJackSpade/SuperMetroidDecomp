namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's death phases (variable A, $0FA8), in the order of the bank-$A4 death dispatcher's table. The
/// slot stores the member, not the table's even byte offset.
/// Keeping this table separate from the executor makes the ROM-authored sequence readable
/// without mixing identity declarations into the state-transition implementation.
/// </summary>
public enum CrocomireDeathPhase
{
    /// <summary>Zero: the fight and bridge checks run; the death sequence has not begun.</summary>
    Fighting = 0,
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
    /// <summary>Starts the first column dissolve and vertical-scroll HDMA table.</summary>
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
    /// <summary>Shipped index-only spacer.</summary>
    ShippedSpacer,
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
    /// <summary>Rumbles the hidden wall using the ROM's signed table.</summary>
    RumbleHiddenWall,
    /// <summary>Loads skeleton OBJ tiles and breaks the spike wall.</summary>
    BreakSpikeWall,
    /// <summary>Eighty-frame delay before the skeleton starts falling.</summary>
    DelaySkeletonFall,
    /// <summary>Arcs the skeleton back into the arena.</summary>
    ArcSkeletonIntoArena,
    /// <summary>Waits for the falls-apart list to reach its terminal image.</summary>
    WaitForSkeletonTerminalImage,
    /// <summary>Reopens the four left scroll cells and clears the wall.</summary>
    ClearWallAndOpenScrolls,
    /// <summary>Waits for the stable skeleton frame.</summary>
    WaitForStableSkeleton,
    /// <summary>Waits one update, doing nothing but advancing the phase.</summary>
    WaitOneUpdate,
    /// <summary>Publishes the miniboss bit and restores boss music.</summary>
    PublishDefeatAndRestoreMusic,
    /// <summary>Final live-room corpse state; intentionally inert.</summary>
    InertCorpse,
    /// <summary>Already-defeated initializer's one-frame advance.</summary>
    DefeatedRoomAdvance,
    /// <summary>Pins his body layer's placement at zero in the already-defeated room.</summary>
    PinDefeatedRoomBodyLayerPlacement,
    /// <summary>River detour sequenced between the final sink and wall wait.</summary>
    RiverSkeletonDetour,
}
