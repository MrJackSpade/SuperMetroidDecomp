namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's death phases (variable A, $0FA8), in the order of the bank-$A4 death dispatcher's table. The
/// slot stores the member, not the table's even byte offset.
/// Keeping this table separate from the executor makes the ROM-authored sequence readable
/// without mixing identity declarations into the state-transition implementation.
/// </summary>
public enum CrocomireDeathPhase : ushort
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
    /// <summary>Installs the first melting tilemap and tongue/arm actor.</summary>
    InstallFirstMeltImage,
    /// <summary>Copies the first melting graphics into the native scratch image.</summary>
    CopyFirstMeltGraphics,
    /// <summary>Uploads one first-melt graphics slice per frame.</summary>
    UploadFirstMeltGraphics,
    /// <summary>Third hop rising phase.</summary>
    ThirdHopRise,
    /// <summary>Starts the first column dissolve and vertical-scroll HDMA table.</summary>
    StartFirstDissolve,
    /// <summary>Dissolves the first body image.</summary>
    DissolveFirstImage,
    /// <summary>Clears BG2 after the first body image is gone.</summary>
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
    /// <summary>Installs the second melting tilemap.</summary>
    InstallSecondMeltImage,
    /// <summary>Copies the second melting graphics into the scratch image.</summary>
    CopySecondMeltGraphics,
    /// <summary>Uploads one second-melt graphics slice per frame.</summary>
    UploadSecondMeltGraphics,
    /// <summary>Shipped index-only spacer.</summary>
    ShippedSpacer,
    /// <summary>Sixth hop rising phase.</summary>
    SixthHopRise,
    /// <summary>Starts the second column dissolve.</summary>
    StartSecondDissolve,
    /// <summary>Dissolves the second body image.</summary>
    DissolveSecondImage,
    /// <summary>Clears BG2 after the second body image is gone.</summary>
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
    /// <summary>Native one-frame index-only state.</summary>
    NativeOneFrameSpacer,
    /// <summary>Publishes the miniboss bit and restores boss music.</summary>
    PublishDefeatAndRestoreMusic,
    /// <summary>Final live-room corpse state; intentionally inert.</summary>
    InertCorpse,
    /// <summary>Already-defeated initializer's one-frame advance.</summary>
    DefeatedRoomAdvance,
    /// <summary>Pins BG2 scrolls at zero in the already-defeated room.</summary>
    PinDefeatedRoomBg2Scroll,
    /// <summary>River detour sequenced between the final sink and wall wait.</summary>
    RiverSkeletonDetour,
}

/// <summary>Domain-owned transitions over <see cref="CrocomireDeathPhase"/>.</summary>
public static class CrocomireDeathPhases
{
    /// <summary>
    /// The dispatcher entry after <paramref name="phase"/>, as the native death handlers
    /// advance variable A by one table word. <see cref="CrocomireDeathPhase.RiverSkeletonDetour"/> is the last
    /// table entry and has no successor.
    /// </summary>
    public static CrocomireDeathPhase Next(this CrocomireDeathPhase phase) => phase switch
    {
        CrocomireDeathPhase.Fighting => CrocomireDeathPhase.CrumbleBridgeAndSink,
        CrocomireDeathPhase.CrumbleBridgeAndSink => CrocomireDeathPhase.FirstSubmergedPause,
        CrocomireDeathPhase.FirstSubmergedPause => CrocomireDeathPhase.FirstHopRise,
        CrocomireDeathPhase.FirstHopRise => CrocomireDeathPhase.FirstHopSink,
        CrocomireDeathPhase.FirstHopSink => CrocomireDeathPhase.SecondSubmergedPause,
        CrocomireDeathPhase.SecondSubmergedPause => CrocomireDeathPhase.SecondHopRise,
        CrocomireDeathPhase.SecondHopRise => CrocomireDeathPhase.SecondHopSink,
        CrocomireDeathPhase.SecondHopSink => CrocomireDeathPhase.InstallFirstMeltImage,
        CrocomireDeathPhase.InstallFirstMeltImage => CrocomireDeathPhase.CopyFirstMeltGraphics,
        CrocomireDeathPhase.CopyFirstMeltGraphics => CrocomireDeathPhase.UploadFirstMeltGraphics,
        CrocomireDeathPhase.UploadFirstMeltGraphics => CrocomireDeathPhase.ThirdHopRise,
        CrocomireDeathPhase.ThirdHopRise => CrocomireDeathPhase.StartFirstDissolve,
        CrocomireDeathPhase.StartFirstDissolve => CrocomireDeathPhase.DissolveFirstImage,
        CrocomireDeathPhase.DissolveFirstImage => CrocomireDeathPhase.ClearFirstMeltImage,
        CrocomireDeathPhase.ClearFirstMeltImage => CrocomireDeathPhase.FourthHopSink,
        CrocomireDeathPhase.FourthHopSink => CrocomireDeathPhase.ThirdSubmergedPause,
        CrocomireDeathPhase.ThirdSubmergedPause => CrocomireDeathPhase.FourthHopRise,
        CrocomireDeathPhase.FourthHopRise => CrocomireDeathPhase.FifthHopSink,
        CrocomireDeathPhase.FifthHopSink => CrocomireDeathPhase.FourthSubmergedPause,
        CrocomireDeathPhase.FourthSubmergedPause => CrocomireDeathPhase.FifthHopRise,
        CrocomireDeathPhase.FifthHopRise => CrocomireDeathPhase.SixthHopSink,
        CrocomireDeathPhase.SixthHopSink => CrocomireDeathPhase.InstallSecondMeltImage,
        CrocomireDeathPhase.InstallSecondMeltImage => CrocomireDeathPhase.CopySecondMeltGraphics,
        CrocomireDeathPhase.CopySecondMeltGraphics => CrocomireDeathPhase.UploadSecondMeltGraphics,
        CrocomireDeathPhase.UploadSecondMeltGraphics => CrocomireDeathPhase.ShippedSpacer,
        CrocomireDeathPhase.ShippedSpacer => CrocomireDeathPhase.SixthHopRise,
        CrocomireDeathPhase.SixthHopRise => CrocomireDeathPhase.StartSecondDissolve,
        CrocomireDeathPhase.StartSecondDissolve => CrocomireDeathPhase.DissolveSecondImage,
        CrocomireDeathPhase.DissolveSecondImage => CrocomireDeathPhase.ClearSecondMeltImage,
        CrocomireDeathPhase.ClearSecondMeltImage => CrocomireDeathPhase.FinalSink,
        CrocomireDeathPhase.FinalSink => CrocomireDeathPhase.WaitForSamusAtWall,
        CrocomireDeathPhase.WaitForSamusAtWall => CrocomireDeathPhase.RumbleHiddenWall,
        CrocomireDeathPhase.RumbleHiddenWall => CrocomireDeathPhase.BreakSpikeWall,
        CrocomireDeathPhase.BreakSpikeWall => CrocomireDeathPhase.DelaySkeletonFall,
        CrocomireDeathPhase.DelaySkeletonFall => CrocomireDeathPhase.ArcSkeletonIntoArena,
        CrocomireDeathPhase.ArcSkeletonIntoArena => CrocomireDeathPhase.WaitForSkeletonTerminalImage,
        CrocomireDeathPhase.WaitForSkeletonTerminalImage => CrocomireDeathPhase.ClearWallAndOpenScrolls,
        CrocomireDeathPhase.ClearWallAndOpenScrolls => CrocomireDeathPhase.WaitForStableSkeleton,
        CrocomireDeathPhase.WaitForStableSkeleton => CrocomireDeathPhase.NativeOneFrameSpacer,
        CrocomireDeathPhase.NativeOneFrameSpacer => CrocomireDeathPhase.PublishDefeatAndRestoreMusic,
        CrocomireDeathPhase.PublishDefeatAndRestoreMusic => CrocomireDeathPhase.InertCorpse,
        CrocomireDeathPhase.InertCorpse => CrocomireDeathPhase.DefeatedRoomAdvance,
        CrocomireDeathPhase.DefeatedRoomAdvance => CrocomireDeathPhase.PinDefeatedRoomBg2Scroll,
        CrocomireDeathPhase.PinDefeatedRoomBg2Scroll => CrocomireDeathPhase.RiverSkeletonDetour,
        _ => throw new InvalidOperationException($"{nameof(CrocomireDeathPhase)} {(int)phase} has no successor."),
    };
}
