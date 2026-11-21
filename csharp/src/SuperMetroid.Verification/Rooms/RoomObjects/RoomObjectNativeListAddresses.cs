using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

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
    internal static ushort BlockReactionEntry(object change) =>
        BlockReactionEntries.Single(entry => ReferenceEquals(entry.Program, change)).Entry;

    /// <summary>Every block reaction's native list.</summary>
    internal static IReadOnlyList<NativeProgramEntry<object>> BlockReactionEntries { get; } =
    [
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CrumbleReveal[SpecialBlockReaction.RespawningCrumble1x1], 0xc8ec),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CrumbleReveal[SpecialBlockReaction.RespawningCrumble2x1], 0xc8f2),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CrumbleReveal[SpecialBlockReaction.RespawningCrumble1x2], 0xc8f8),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CrumbleReveal[SpecialBlockReaction.RespawningCrumble2x2], 0xc8fe),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombedPowerBombBlockReveal, 0xc91c),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal, 0xc922),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble1x1Respawning, 0xc9f9),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble2x1Respawning, 0xca1c),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble1x2Respawning, 0xca41),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble2x2Respawning, 0xca66),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble1x1Permanent, 0xca8b),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble2x1Permanent, 0xcaa0),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble1x2Permanent, 0xcab5),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ContactCrumble2x2Permanent, 0xcaca),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningShot[ShootableReaction.RespawningBreak1x1], 0xcadf),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningShot[ShootableReaction.RespawningBreak2x1], 0xcb02),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningShot[ShootableReaction.RespawningBreak1x2], 0xcb27),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningShot[ShootableReaction.RespawningBreak2x2], 0xcb4c),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningSuperMissileBlock, 0xcb71),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningPowerBombBlock, 0xcb94),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentShot[ShootableReaction.PermanentBreak1x1], 0xcbb7),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentShot[ShootableReaction.PermanentBreak2x1], 0xcbcc),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentShot[ShootableReaction.PermanentBreak1x2], 0xcbe1),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentShot[ShootableReaction.PermanentBreak2x2], 0xcbf6),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentSuperMissileBlock, 0xcc0b),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentPowerBombBlock, 0xcc20),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.RespawningBreak1x1], 0xcc35),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.RespawningBreak1x1], 0xcc3c),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.RespawningBreak1x1], 0xcc3f),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.RespawningBreak2x1], 0xcc5f),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.RespawningBreak2x1], 0xcc66),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.RespawningBreak2x1], 0xcc69),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.RespawningBreak1x2], 0xcc8b),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.RespawningBreak1x2], 0xcc92),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.RespawningBreak1x2], 0xcc95),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.RespawningBreak2x2], 0xccb7),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.RespawningBreak2x2], 0xccbe),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.RespawningBreak2x2], 0xccc1),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.PermanentBreak1x1], 0xcce3),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.PermanentBreak1x1], 0xccea),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.PermanentBreak1x1], 0xcced),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.PermanentBreak2x1], 0xccff),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.PermanentBreak2x1], 0xcd06),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.PermanentBreak2x1], 0xcd09),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.PermanentBreak1x2], 0xcd1b),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.PermanentBreak1x2], 0xcd22),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.PermanentBreak1x2], 0xcd25),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.CollisionBomb[BombableReaction.PermanentBreak2x2], 0xcd37),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.ReactionBomb[BombableReaction.PermanentBreak2x2], 0xcd3e),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.BombBreak[BombableReaction.PermanentBreak2x2], 0xcd41),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock, 0xcd6a),
        new NativeProgramEntry<object>(BlockReactionChangeDefinitions.PermanentBreakableGrappleBlock, 0xcda9),
    ];

    /// <summary>The Speed Booster blocks' lists, each with the crumble it plays.</summary>
    internal static IReadOnlyList<NativeProgramEntry<object>> SpeedBoosterBlockEntries { get; } =
    [
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.SlowRespawningCrumble, 0xc951),
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.RespawningCrumble, 0xc974),
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.RespawningCrumble, 0xc997),
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.SlowPermanentCrumble, 0xc9cf),
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.PermanentCrumble, 0xc9e4),
        new NativeProgramEntry<object>(SpeedBoosterBlockObjectDefinitions.BombReveal, 0xc928),
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
