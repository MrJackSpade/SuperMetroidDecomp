using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// The native bank-$84 lists of the room PLM programs typed by #1296, for the ROM oracles that walk them beside the
/// typed programs. Only verification names these lists.
/// </summary>
internal static class RoomObjectNativeListAddresses
{
    /// <summary>The n00b tube's first list (<c>InstList_PLM_NoobTube_0</c>, $84:D4D4).</summary>
    internal const ushort NoobTubeList = 0xd4d4;

    /// <summary>The native list that only deletes its object ($84:AAE3).</summary>
    internal const ushort DeleteList = 0xaae3;

    /// <summary>The native list the Metroid room's clearance sleeps on ($84:DB42).</summary>
    internal const ushort MetroidsClearedSleepList = 0xdb42;

    /// <summary>The Speed Booster lavaquake's list (<c>InstList_PLM_SpeedBoosterEscape</c>, $84:B88A), one wait per sleep.</summary>
    internal const ushort SpeedBoosterEscapeList = 0xb88a;

    /// <summary>The Speed Booster blocks' lists ($84:C928 and $84:C951..C9F8).</summary>
    /// <summary>
    /// The lists of the room objects that left the scripted program set for typed terrain changes (#1554): the Maridia
    /// elevatube, the critters' wall breakthrough, Crateria's escape passage and the enemy-breakable block.
    /// </summary>
    internal static IReadOnlyList<NativeProgramEntry<string>> ScriptedTerrainChangeEntries { get; } =
    [
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube), 0xb8f0),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough), 0xb9a2),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage), 0xbb19),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak), 0xcd53),
    ];

    /// <summary>The native list of block reaction <paramref name="change"/>.</summary>
    internal static ushort BlockReactionEntry(BlockReactionChange change) =>
        BlockReactionEntries.Single(entry => ReferenceEquals(entry.Program, change)).Entry;

    /// <summary>Every block reaction's native list.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BlockReactionChange>> BlockReactionEntries { get; } =
    [
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CrumbleReveal1x1, 0xc8ec),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CrumbleReveal2x1, 0xc8f2),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CrumbleReveal1x2, 0xc8f8),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CrumbleReveal2x2, 0xc8fe),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombedPowerBombBlockReveal, 0xc91c),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal, 0xc922),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble1x1Respawning, 0xc9f9),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble2x1Respawning, 0xca1c),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble1x2Respawning, 0xca41),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble2x2Respawning, 0xca66),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble1x1Permanent, 0xca8b),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble2x1Permanent, 0xcaa0),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble1x2Permanent, 0xcab5),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ContactCrumble2x2Permanent, 0xcaca),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningShotBlock1x1, 0xcadf),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningShotBlock2x1, 0xcb02),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningShotBlock1x2, 0xcb27),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningShotBlock2x2, 0xcb4c),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningSuperMissileBlock, 0xcb71),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningPowerBombBlock, 0xcb94),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentShotBlock1x1, 0xcbb7),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentShotBlock2x1, 0xcbcc),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentShotBlock1x2, 0xcbe1),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentShotBlock2x2, 0xcbf6),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentSuperMissileBlock, 0xcc0b),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentPowerBombBlock, 0xcc20),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock1x1Respawning, 0xcc35),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock1x1Respawning, 0xcc3c),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock1x1RespawningBreak, 0xcc3f),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock2x1Respawning, 0xcc5f),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock2x1Respawning, 0xcc66),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock2x1RespawningBreak, 0xcc69),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock1x2Respawning, 0xcc8b),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock1x2Respawning, 0xcc92),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock1x2RespawningBreak, 0xcc95),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock2x2Respawning, 0xccb7),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock2x2Respawning, 0xccbe),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock2x2RespawningBreak, 0xccc1),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock1x1Permanent, 0xcce3),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock1x1Permanent, 0xccea),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock1x1PermanentBreak, 0xcced),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock2x1Permanent, 0xccff),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock2x1Permanent, 0xcd06),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock2x1PermanentBreak, 0xcd09),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock1x2Permanent, 0xcd1b),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock1x2Permanent, 0xcd22),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock1x2PermanentBreak, 0xcd25),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.CollisionBombBlock2x2Permanent, 0xcd37),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.ReactionBombBlock2x2Permanent, 0xcd3e),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.BombBlock2x2PermanentBreak, 0xcd41),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock, 0xcd6a),
        new NativeProgramEntry<BlockReactionChange>(BlockReactionChangeDefinitions.PermanentBreakableGrappleBlock, 0xcda9),
    ];

    /// <summary>The Speed Booster blocks' lists, each with the crumble it plays.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BlockReactionChange>> SpeedBoosterBlockEntries { get; } =
    [
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.SlowRespawningCrumble, 0xc951),
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.RespawningCrumble, 0xc974),
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.RespawningCrumble, 0xc997),
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.SlowPermanentCrumble, 0xc9cf),
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.PermanentCrumble, 0xc9e4),
        new NativeProgramEntry<BlockReactionChange>(SpeedBoosterBlockObjectDefinitions.BombReveal, 0xc928),
    ];

    /// <summary>The Tourian elevator access lists (<c>InstList_PLM_CrumbleAccessToTourianElevator_0..1</c>, <c>..ClearAccess..</c>).</summary>
    internal static IReadOnlyList<NativeProgramEntry<TourianAccessFloor>> TourianAccessEntries { get; } =
    [
        new NativeProgramEntry<TourianAccessFloor>(TourianAccessFloor.Crumble, 0xaae5),
        new NativeProgramEntry<TourianAccessFloor>(TourianAccessFloor.Clear, 0xab0c),
    ];

    /// <summary>The Wrecked Ship attic's list (<c>InstList_PLM_WreckedShipAttic</c>, $84:BAFF).</summary>
    internal const ushort WreckedShipAtticList = 0xbaff;

    /// <summary>The lavaquake list's length in words: three install-and-sleep phases.</summary>
    internal const int SpeedBoosterEscapeWordCount = 9;
}
