namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The common shootable reactions the shot-block dispatcher handles, by entry.</summary>
public static class ShotBlockReactionDefinitions
{
    /// <summary>
    /// The reactions of the first common entries, in entry order: four respawning and four permanent breaks (one, two
    /// wide, two tall and two by two blocks), the respawning and permanent Power Bomb and Super Missile blocks, four
    /// empty entries and the collision probe.
    /// </summary>
    public static readonly IReadOnlyList<ShotBlockReaction> LeadingEntries =
    [
        ShotBlockReaction.RespawningBreak, ShotBlockReaction.RespawningBreak,
        ShotBlockReaction.RespawningBreak, ShotBlockReaction.RespawningBreak,
        ShotBlockReaction.PermanentBreak, ShotBlockReaction.PermanentBreak,
        ShotBlockReaction.PermanentBreak, ShotBlockReaction.PermanentBreak,
        ShotBlockReaction.RespawningPowerBombBreak, ShotBlockReaction.PermanentPowerBombBreak,
        ShotBlockReaction.RespawningSuperMissileBreak, ShotBlockReaction.PermanentSuperMissileBreak,
        ShotBlockReaction.Nothing, ShotBlockReaction.Nothing, ShotBlockReaction.Nothing, ShotBlockReaction.Nothing,
        ShotBlockReaction.CollisionProbe,
    ];

    /// <summary>The first permanent break's entry; the permanent breaks follow the respawning ones in the same order.</summary>
    public const int FirstPermanentBreakEntry = 4;

    /// <summary>The last of the unused entries that directly follow <see cref="LeadingEntries"/>.</summary>
    public const int LastFollowingUnusedEntry = 63;

    /// <summary>The unused entry that lies after the door, collectible and gate entries.</summary>
    public const int LoneUnusedEntry = 78;

    /// <summary>The entry the closed downward gate gives its column: the collision probe.</summary>
    public static readonly BlockReaction CollisionProbe = BlockReaction.Common(16);
}
