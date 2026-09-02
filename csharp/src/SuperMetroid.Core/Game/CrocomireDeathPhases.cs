namespace SuperMetroid.Core.Game;

/// <summary>
/// Native even-numbered state indexes used by Crocomire's bank-$A4 death dispatcher.
/// Keeping this table separate from the executor makes the ROM-authored sequence readable
/// without mixing identity declarations into the state-transition implementation.
/// </summary>
internal static class CrocomireDeathPhases
{
    /// <summary>Bridge crumbles while Crocomire sinks into the acid.</summary>
    public const ushort CrumbleBridgeAndSink = 0x02;
    /// <summary>First submerged pause.</summary>
    public const ushort FirstSubmergedPause = 0x04;
    /// <summary>First hop rising phase.</summary>
    public const ushort FirstHopRise = 0x06;
    /// <summary>First hop sinking phase.</summary>
    public const ushort FirstHopSink = 0x08;
    /// <summary>Second submerged pause.</summary>
    public const ushort SecondSubmergedPause = 0x0a;
    /// <summary>Second hop rising phase.</summary>
    public const ushort SecondHopRise = 0x0c;
    /// <summary>Second hop sinking phase.</summary>
    public const ushort SecondHopSink = 0x0e;
    /// <summary>Installs the first melting tilemap and tongue/arm actor.</summary>
    public const ushort InstallFirstMeltImage = 0x10;
    /// <summary>Copies the first melting graphics into the native scratch image.</summary>
    public const ushort CopyFirstMeltGraphics = 0x12;
    /// <summary>Uploads one first-melt graphics slice per frame.</summary>
    public const ushort UploadFirstMeltGraphics = 0x14;
    /// <summary>Third hop rising phase.</summary>
    public const ushort ThirdHopRise = 0x16;
    /// <summary>Starts the first column dissolve and vertical-scroll HDMA table.</summary>
    public const ushort StartFirstDissolve = 0x18;
    /// <summary>Dissolves the first body image.</summary>
    public const ushort DissolveFirstImage = 0x1a;
    /// <summary>Clears BG2 after the first body image is gone.</summary>
    public const ushort ClearFirstMeltImage = 0x1c;
    /// <summary>Fourth hop sinking phase.</summary>
    public const ushort FourthHopSink = 0x1e;
    /// <summary>Third submerged pause.</summary>
    public const ushort ThirdSubmergedPause = 0x20;
    /// <summary>Fourth hop rising phase.</summary>
    public const ushort FourthHopRise = 0x22;
    /// <summary>Fifth hop sinking phase.</summary>
    public const ushort FifthHopSink = 0x24;
    /// <summary>Fourth submerged pause.</summary>
    public const ushort FourthSubmergedPause = 0x26;
    /// <summary>Fifth hop rising phase.</summary>
    public const ushort FifthHopRise = 0x28;
    /// <summary>Sixth hop sinking phase.</summary>
    public const ushort SixthHopSink = 0x2a;
    /// <summary>Installs the second melting tilemap.</summary>
    public const ushort InstallSecondMeltImage = 0x2c;
    /// <summary>Copies the second melting graphics into the scratch image.</summary>
    public const ushort CopySecondMeltGraphics = 0x2e;
    /// <summary>Uploads one second-melt graphics slice per frame.</summary>
    public const ushort UploadSecondMeltGraphics = 0x30;
    /// <summary>Shipped index-only spacer.</summary>
    public const ushort ShippedSpacer = 0x32;
    /// <summary>Sixth hop rising phase.</summary>
    public const ushort SixthHopRise = 0x34;
    /// <summary>Starts the second column dissolve.</summary>
    public const ushort StartSecondDissolve = 0x36;
    /// <summary>Dissolves the second body image.</summary>
    public const ushort DissolveSecondImage = 0x38;
    /// <summary>Clears BG2 after the second body image is gone.</summary>
    public const ushort ClearSecondMeltImage = 0x3a;
    /// <summary>Final sink before selecting the river-skeleton detour.</summary>
    public const ushort FinalSink = 0x3c;
    /// <summary>Waits behind the wall for Samus to return left.</summary>
    public const ushort WaitForSamusAtWall = 0x3e;
    /// <summary>Rumbles the hidden wall using the ROM's signed table.</summary>
    public const ushort RumbleHiddenWall = 0x40;
    /// <summary>Loads skeleton OBJ tiles and breaks the spike wall.</summary>
    public const ushort BreakSpikeWall = 0x42;
    /// <summary>Eighty-frame delay before the skeleton starts falling.</summary>
    public const ushort DelaySkeletonFall = 0x44;
    /// <summary>Arcs the skeleton back into the arena.</summary>
    public const ushort ArcSkeletonIntoArena = 0x46;
    /// <summary>Waits for the falls-apart list to reach its terminal image.</summary>
    public const ushort WaitForSkeletonTerminalImage = 0x48;
    /// <summary>Reopens the four left scroll cells and clears the wall.</summary>
    public const ushort ClearWallAndOpenScrolls = 0x4a;
    /// <summary>Waits for the stable skeleton frame.</summary>
    public const ushort WaitForStableSkeleton = 0x4c;
    /// <summary>Native one-frame index-only state.</summary>
    public const ushort NativeOneFrameSpacer = 0x4e;
    /// <summary>Publishes the miniboss bit and restores boss music.</summary>
    public const ushort PublishDefeatAndRestoreMusic = 0x50;
    /// <summary>Final live-room corpse state; intentionally inert.</summary>
    public const ushort InertCorpse = 0x52;
    /// <summary>Already-defeated initializer's one-frame advance.</summary>
    public const ushort DefeatedRoomAdvance = 0x54;
    /// <summary>Pins BG2 scrolls at zero in the already-defeated room.</summary>
    public const ushort PinDefeatedRoomBg2Scroll = 0x56;
    /// <summary>River detour sequenced between the final sink and wall wait.</summary>
    public const ushort RiverSkeletonDetour = 0x58;
}
