namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The shootable reactions the shot-block dispatcher handles, and what it does with each.</summary>
public static class ShotBlockReactionDefinitions
{
    /// <summary>
    /// What the dispatcher does with each shootable reaction it handles: the respawning and permanent breaks of every size,
    /// the Power Bomb and Super Missile blocks, the empty reactions and the collision probe. The blocks that ignore shots,
    /// and the blocks that react only when solid, set off the same one-update object as the empty reaction.
    /// </summary>
    public static readonly IReadOnlyDictionary<ShootableReaction, ShotBlockReaction> Handled =
        new Dictionary<ShootableReaction, ShotBlockReaction>
        {
            [ShootableReaction.RespawningBreak1x1] = ShotBlockReaction.RespawningBreak,
            [ShootableReaction.RespawningBreak2x1] = ShotBlockReaction.RespawningBreak,
            [ShootableReaction.RespawningBreak1x2] = ShotBlockReaction.RespawningBreak,
            [ShootableReaction.RespawningBreak2x2] = ShotBlockReaction.RespawningBreak,
            [ShootableReaction.PermanentBreak1x1] = ShotBlockReaction.PermanentBreak,
            [ShootableReaction.PermanentBreak2x1] = ShotBlockReaction.PermanentBreak,
            [ShootableReaction.PermanentBreak1x2] = ShotBlockReaction.PermanentBreak,
            [ShootableReaction.PermanentBreak2x2] = ShotBlockReaction.PermanentBreak,
            [ShootableReaction.RespawningPowerBombBlock] = ShotBlockReaction.RespawningPowerBombBreak,
            [ShootableReaction.PermanentPowerBombBlock] = ShotBlockReaction.PermanentPowerBombBreak,
            [ShootableReaction.RespawningSuperMissileBlock] = ShotBlockReaction.RespawningSuperMissileBreak,
            [ShootableReaction.PermanentSuperMissileBlock] = ShotBlockReaction.PermanentSuperMissileBreak,
            [ShootableReaction.Nothing] = ShotBlockReaction.Nothing,
            [ShootableReaction.CollisionProbe] = ShotBlockReaction.CollisionProbe,
            [ShootableReaction.IgnoresShots] = ShotBlockReaction.Nothing,
            [ShootableReaction.NothingWhenSolid] = ShotBlockReaction.Nothing,
        };

    /// <summary>
    /// The table's leading reactions, which every weapon's scan dispatches to the table: the breaks, the Power Bomb and
    /// Super Missile blocks and the empty reaction.
    /// </summary>
    public static readonly IReadOnlySet<ShootableReaction> LeadingReactions = new HashSet<ShootableReaction>
    {
        ShootableReaction.RespawningBreak1x1, ShootableReaction.RespawningBreak2x1,
        ShootableReaction.RespawningBreak1x2, ShootableReaction.RespawningBreak2x2,
        ShootableReaction.PermanentBreak1x1, ShootableReaction.PermanentBreak2x1,
        ShootableReaction.PermanentBreak1x2, ShootableReaction.PermanentBreak2x2,
        ShootableReaction.RespawningPowerBombBlock, ShootableReaction.PermanentPowerBombBlock,
        ShootableReaction.RespawningSuperMissileBlock, ShootableReaction.PermanentSuperMissileBlock,
        ShootableReaction.Nothing,
    };

    /// <summary>The reactions that set nothing off in shootable air, only in a shootable block.</summary>
    public static readonly IReadOnlySet<ShootableReaction> SolidBlockOnlyReactions = new HashSet<ShootableReaction>
    {
        ShootableReaction.NothingWhenSolid,
    };
}
