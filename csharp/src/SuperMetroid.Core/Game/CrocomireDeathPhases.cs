namespace SuperMetroid.Core.Game;

/// <summary>
/// Native even-numbered state indexes (Crocomire variable A, $0FA8) used by Crocomire's bank-$A4 death dispatcher.
/// Keeping this table separate from the executor makes the ROM-authored sequence readable
/// without mixing identity declarations into the state-transition implementation.
/// </summary>
public enum CrocomireDeathPhase : ushort
{
    /// <summary>Zero: the fight and bridge checks run; the death sequence has not begun.</summary>
    Fighting = 0x00,
    /// <summary>Bridge crumbles while Crocomire sinks into the acid.</summary>
    CrumbleBridgeAndSink = 0x02,
    /// <summary>First submerged pause.</summary>
    FirstSubmergedPause = 0x04,
    /// <summary>First hop rising phase.</summary>
    FirstHopRise = 0x06,
    /// <summary>First hop sinking phase.</summary>
    FirstHopSink = 0x08,
    /// <summary>Second submerged pause.</summary>
    SecondSubmergedPause = 0x0a,
    /// <summary>Second hop rising phase.</summary>
    SecondHopRise = 0x0c,
    /// <summary>Second hop sinking phase.</summary>
    SecondHopSink = 0x0e,
    /// <summary>Installs the first melting tilemap and tongue/arm actor.</summary>
    InstallFirstMeltImage = 0x10,
    /// <summary>Copies the first melting graphics into the native scratch image.</summary>
    CopyFirstMeltGraphics = 0x12,
    /// <summary>Uploads one first-melt graphics slice per frame.</summary>
    UploadFirstMeltGraphics = 0x14,
    /// <summary>Third hop rising phase.</summary>
    ThirdHopRise = 0x16,
    /// <summary>Starts the first column dissolve and vertical-scroll HDMA table.</summary>
    StartFirstDissolve = 0x18,
    /// <summary>Dissolves the first body image.</summary>
    DissolveFirstImage = 0x1a,
    /// <summary>Clears BG2 after the first body image is gone.</summary>
    ClearFirstMeltImage = 0x1c,
    /// <summary>Fourth hop sinking phase.</summary>
    FourthHopSink = 0x1e,
    /// <summary>Third submerged pause.</summary>
    ThirdSubmergedPause = 0x20,
    /// <summary>Fourth hop rising phase.</summary>
    FourthHopRise = 0x22,
    /// <summary>Fifth hop sinking phase.</summary>
    FifthHopSink = 0x24,
    /// <summary>Fourth submerged pause.</summary>
    FourthSubmergedPause = 0x26,
    /// <summary>Fifth hop rising phase.</summary>
    FifthHopRise = 0x28,
    /// <summary>Sixth hop sinking phase.</summary>
    SixthHopSink = 0x2a,
    /// <summary>Installs the second melting tilemap.</summary>
    InstallSecondMeltImage = 0x2c,
    /// <summary>Copies the second melting graphics into the scratch image.</summary>
    CopySecondMeltGraphics = 0x2e,
    /// <summary>Uploads one second-melt graphics slice per frame.</summary>
    UploadSecondMeltGraphics = 0x30,
    /// <summary>Shipped index-only spacer.</summary>
    ShippedSpacer = 0x32,
    /// <summary>Sixth hop rising phase.</summary>
    SixthHopRise = 0x34,
    /// <summary>Starts the second column dissolve.</summary>
    StartSecondDissolve = 0x36,
    /// <summary>Dissolves the second body image.</summary>
    DissolveSecondImage = 0x38,
    /// <summary>Clears BG2 after the second body image is gone.</summary>
    ClearSecondMeltImage = 0x3a,
    /// <summary>Final sink before selecting the river-skeleton detour.</summary>
    FinalSink = 0x3c,
    /// <summary>Waits behind the wall for Samus to return left.</summary>
    WaitForSamusAtWall = 0x3e,
    /// <summary>Rumbles the hidden wall using the ROM's signed table.</summary>
    RumbleHiddenWall = 0x40,
    /// <summary>Loads skeleton OBJ tiles and breaks the spike wall.</summary>
    BreakSpikeWall = 0x42,
    /// <summary>Eighty-frame delay before the skeleton starts falling.</summary>
    DelaySkeletonFall = 0x44,
    /// <summary>Arcs the skeleton back into the arena.</summary>
    ArcSkeletonIntoArena = 0x46,
    /// <summary>Waits for the falls-apart list to reach its terminal image.</summary>
    WaitForSkeletonTerminalImage = 0x48,
    /// <summary>Reopens the four left scroll cells and clears the wall.</summary>
    ClearWallAndOpenScrolls = 0x4a,
    /// <summary>Waits for the stable skeleton frame.</summary>
    WaitForStableSkeleton = 0x4c,
    /// <summary>Native one-frame index-only state.</summary>
    NativeOneFrameSpacer = 0x4e,
    /// <summary>Publishes the miniboss bit and restores boss music.</summary>
    PublishDefeatAndRestoreMusic = 0x50,
    /// <summary>Final live-room corpse state; intentionally inert.</summary>
    InertCorpse = 0x52,
    /// <summary>Already-defeated initializer's one-frame advance.</summary>
    DefeatedRoomAdvance = 0x54,
    /// <summary>Pins BG2 scrolls at zero in the already-defeated room.</summary>
    PinDefeatedRoomBg2Scroll = 0x56,
    /// <summary>River detour sequenced between the final sink and wall wait.</summary>
    RiverSkeletonDetour = 0x58,
}
