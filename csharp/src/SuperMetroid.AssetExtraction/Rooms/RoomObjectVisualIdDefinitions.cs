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
        [DraygonCannonDraw.RightShieldA] = "right-shield-a",
        [DraygonCannonDraw.RightShieldB] = "right-shield-b",
        [DraygonCannonDraw.RightDamagedA] = "right-damaged-a",
        [DraygonCannonDraw.RightDamagedB] = "right-damaged-b",
        [DraygonCannonDraw.RightDamagedC] = "right-damaged-c",
        [DraygonCannonDraw.RightDamagedD] = "right-damaged-d",
        [DraygonCannonDraw.LeftShieldA] = "left-shield-a",
        [DraygonCannonDraw.LeftShieldB] = "left-shield-b",
        [DraygonCannonDraw.LeftDamagedA] = "left-damaged-a",
        [DraygonCannonDraw.LeftDamagedB] = "left-damaged-b",
        [DraygonCannonDraw.LeftDamagedC] = "left-damaged-c",
        [DraygonCannonDraw.LeftDamagedD] = "left-damaged-d",
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
        [MotherBrainFakeDeathDraw.MuralRow1] = "background-row-2",
        [MotherBrainFakeDeathDraw.MuralRow2] = "background-row-3",
        [MotherBrainFakeDeathDraw.MuralRow3] = "background-row-4",
        [MotherBrainFakeDeathDraw.MuralRow4] = "background-row-5",
        [MotherBrainFakeDeathDraw.MuralRow5] = "background-row-6",
        [MotherBrainFakeDeathDraw.MuralRow6] = "background-row-7",
        [MotherBrainFakeDeathDraw.MuralRow7] = "background-row-8",
        [MotherBrainFakeDeathDraw.MuralRow8] = "background-row-9",
        [MotherBrainFakeDeathDraw.MuralRow9] = "background-row-a",
        [MotherBrainFakeDeathDraw.MuralRow10] = "background-row-b",
        [MotherBrainFakeDeathDraw.MuralRow11] = "background-row-c",
        [MotherBrainFakeDeathDraw.MuralRow12] = "background-row-d",
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
        [MotherBrainGlassDraw.PaneDamage1] = "pane-damage-1",
        [MotherBrainGlassDraw.PaneDamage2] = "pane-damage-2",
        [MotherBrainGlassDraw.PaneTransition] = "pane-transition",
        [MotherBrainGlassDraw.ShiftedPane1] = "shifted-pane-1",
        [MotherBrainGlassDraw.ShiftedPane2] = "shifted-pane-2",
        [MotherBrainGlassDraw.ShiftedPane3] = "shifted-pane-3",
        [MotherBrainGlassDraw.Shatter1] = "shatter-1",
        [MotherBrainGlassDraw.Shatter2] = "shatter-2",
        [MotherBrainGlassDraw.Shatter3] = "shatter-3",
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

    /// <summary>The three platform frames' stable artwork identifiers.</summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, string> ElevatorPlatform = new()
    {
        [ElevatorPlatformDraw.First] = "first-frame",
        [ElevatorPlatformDraw.Second] = "second-frame",
        [ElevatorPlatformDraw.Third] = "third-frame",
    };

    /// <summary>The station draws in their published enumeration order: frames by station, then save and access states.</summary>
    internal static readonly StationDraw[] StationPublishedOrder =
    [
        StationDraw.MapFrame0, StationDraw.EnergyFrame0, StationDraw.MissileFrame0,
        StationDraw.MapFrame1, StationDraw.EnergyFrame1, StationDraw.MissileFrame1,
        StationDraw.MapFrame2, StationDraw.EnergyFrame2, StationDraw.MissileFrame2,
        StationDraw.SaveIdle, StationDraw.SaveActive, StationDraw.SaveAlternate,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];
}
