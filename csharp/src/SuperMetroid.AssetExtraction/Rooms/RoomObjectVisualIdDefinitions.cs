using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published visual identities the installed PLM visual files name compiled draws by, family by family, and the
/// order the station frames are published in. Only the import and its files know these names; the game draws by draw.
/// </summary>
internal static class RoomObjectVisualIdDefinitions
{
    /// <summary>The artwork identity naming each empty, crumble or six-row clear frame in visual files.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessDraw, string> TourianAccess = new Dictionary<TourianAccessDraw, string>
    {
        [TourianAccessDraw.EmptyRow] = "crumble-empty-row",
        [TourianAccessDraw.CrumbleFirst] = "crumble-frame-0",
        [TourianAccessDraw.CrumbleSecond] = "crumble-frame-1",
        [TourianAccessDraw.CrumbleThird] = "crumble-frame-2",
        [TourianAccessDraw.Clear] = "clear-six-rows",
    };

    /// <summary>Each arena draw's editable artwork key.</summary>
    internal static readonly Dictionary<CrocomireArenaDraw, string> CrocomireArena = new()
    {
        [CrocomireArenaDraw.ClearBridge] = "clear-bridge",
        [CrocomireArenaDraw.CrumbleBridgeBlock] = "crumble-bridge-block",
        [CrocomireArenaDraw.ClearBridgeBlock] = "clear-bridge-block",
        [CrocomireArenaDraw.ClearInvisibleWall] = "clear-invisible-wall",
        [CrocomireArenaDraw.CreateInvisibleWall] = "create-invisible-wall",
    };

    /// <summary>Each reachable cannon draw's editable artwork key: its facing, condition and pose.</summary>
    internal static readonly Dictionary<DraygonCannonDraw, string> DraygonCannon = new()
    {
        [DraygonCannonDraw.RightShield] = "right-shield",
        [DraygonCannonDraw.RightShieldFlash] = "right-shield-flash",
        [DraygonCannonDraw.RightSparksPeakingAt11And20] = "right-sparks-peaking-at-11-and-20",
        [DraygonCannonDraw.RightSparksPeakingAt3And28] = "right-sparks-peaking-at-3-and-28",
        [DraygonCannonDraw.RightSparksPeakingAt6And25] = "right-sparks-peaking-at-6-and-25",
        [DraygonCannonDraw.RightSparksFlat] = "right-sparks-flat",
        [DraygonCannonDraw.LeftShield] = "left-shield",
        [DraygonCannonDraw.LeftShieldFlash] = "left-shield-flash",
        [DraygonCannonDraw.LeftSparksPeakingAt11And20] = "left-sparks-peaking-at-11-and-20",
        [DraygonCannonDraw.LeftSparksPeakingAt3And28] = "left-sparks-peaking-at-3-and-28",
        [DraygonCannonDraw.LeftSparksPeakingAt6And25] = "left-sparks-peaking-at-6-and-25",
        [DraygonCannonDraw.LeftSparksFlat] = "left-sparks-flat",
    };

    /// <summary>Each Phantoon door frame's editable artwork key.</summary>
    internal static readonly Dictionary<PhantoonDoorDraw, string> PhantoonDoor = new()
    {
        [PhantoonDoorDraw.FightDoor] = "fight-door",
        [PhantoonDoorDraw.RestoredDoor] = "restored-door",
    };

    /// <summary>Each gate image's stable authored visual identifier.</summary>
    internal static readonly Dictionary<MotherBrainEscapeGateDraw, string> MotherBrainEscapeGate = new()
    {
        [MotherBrainEscapeGateDraw.Open] = "open",
        [MotherBrainEscapeGateDraw.HalfClosed] = "half-closed",
        [MotherBrainEscapeGateDraw.Closed] = "closed",
    };

    /// <summary>The stable visual document name of each draw in the closed draw domain.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, string> MotherBrainFakeDeath = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] = "fill-wall",
        [MotherBrainFakeDeathDraw.EscapeDoor] = "escape-door",
        [MotherBrainFakeDeathDraw.MuralRow1] = "mural-row-1",
        [MotherBrainFakeDeathDraw.MuralRow2] = "mural-row-2",
        [MotherBrainFakeDeathDraw.MuralRow3] = "mural-row-3",
        [MotherBrainFakeDeathDraw.MuralRow4] = "mural-row-4",
        [MotherBrainFakeDeathDraw.MuralRow5] = "mural-row-5",
        [MotherBrainFakeDeathDraw.MuralRow6] = "mural-row-6",
        [MotherBrainFakeDeathDraw.MuralRow7] = "mural-row-7",
        [MotherBrainFakeDeathDraw.MuralRow8] = "mural-row-8",
        [MotherBrainFakeDeathDraw.MuralRow9] = "mural-row-9",
        [MotherBrainFakeDeathDraw.MuralRow10] = "mural-row-10",
        [MotherBrainFakeDeathDraw.MuralRow11] = "mural-row-11",
        [MotherBrainFakeDeathDraw.MuralRow12] = "mural-row-12",
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = "clear-ceiling-block",
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = "clear-ceiling-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = "clear-bottom-middle-side-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = "clear-bottom-middle-tubes",
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = "clear-bottom-left-tube",
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = "clear-bottom-right-tube",
    };

    /// <summary>Each glass state's editable presentation frame name.</summary>
    internal static readonly Dictionary<MotherBrainGlassDraw, string> MotherBrainGlass = new()
    {
        [MotherBrainGlassDraw.Initial] = "initial",
        [MotherBrainGlassDraw.PaneCracked] = "pane-cracked",
        [MotherBrainGlassDraw.PaneCrackedThrough] = "pane-cracked-through",
        [MotherBrainGlassDraw.PaneTransition] = "pane-transition",
        [MotherBrainGlassDraw.PaneShifted] = "pane-shifted",
        [MotherBrainGlassDraw.PaneShiftedCrumbling] = "pane-shifted-crumbling",
        [MotherBrainGlassDraw.PaneShiftedEmptying] = "pane-shifted-emptying",
        [MotherBrainGlassDraw.RimBuckling] = "rim-buckling",
        [MotherBrainGlassDraw.RimBursting] = "rim-bursting",
        [MotherBrainGlassDraw.RimGone] = "rim-gone",
        [MotherBrainGlassDraw.Cleared] = "cleared",
    };

    /// <summary>The clear layout's and the three successive crumble appearances' editable artwork keys.</summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, string> SporeSpawnCeiling = new()
    {
        [SporeSpawnCeilingDraw.Clear] = "clear-ceiling",
        [SporeSpawnCeilingDraw.CrumbleFirst] = "crumble-frame-0",
        [SporeSpawnCeilingDraw.CrumbleSecond] = "crumble-frame-1",
        [SporeSpawnCeilingDraw.CrumbleThird] = "crumble-frame-2",
    };

    /// <summary>The intact and cleared hand draws' stable artwork identities.</summary>
    internal static readonly Dictionary<BombTorizoHandDraw, string> BombTorizoHand = new()
    {
        [BombTorizoHandDraw.Intact] = "intact",
        [BombTorizoHandDraw.Cleared] = "cleared",
    };

    /// <summary>The cleared hand's and the blocked and clear slope layouts' published visual identities.</summary>
    internal static readonly Dictionary<ChozoStatueDraw, string> ChozoStatue = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] = "lower-norfair-cleared-hand",
        [ChozoStatueDraw.ClearSlopeAccess] = "wrecked-ship-clear-slope-access",
        [ChozoStatueDraw.BlockSlopeAccess] = "wrecked-ship-block-slope-access",
    };

    /// <summary>The platform's three looks' stable artwork identifiers.</summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, string> ElevatorPlatform = new()
    {
        [ElevatorPlatformDraw.Dim] = "dim",
        [ElevatorPlatformDraw.Glowing] = "glowing",
        [ElevatorPlatformDraw.Bright] = "bright",
    };

    /// <summary>The station draws in their published enumeration order: each glow by station, then save and access states.</summary>
    internal static readonly StationDraw[] StationPublishedOrder =
    [
        StationDraw.MapDim, StationDraw.EnergyDim, StationDraw.MissileDim,
        StationDraw.MapLit, StationDraw.EnergyLit, StationDraw.MissileLit,
        StationDraw.MapBright, StationDraw.EnergyBright, StationDraw.MissileBright,
        StationDraw.SaveIdle, StationDraw.SaveLit, StationDraw.SaveDark,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];
}
