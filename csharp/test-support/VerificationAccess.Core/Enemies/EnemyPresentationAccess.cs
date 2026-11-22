using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification reads of the body the room's boss attached to the enemies' presentation.</summary>
internal static class EnemyPresentationAccess
{
    /// <summary>Reads the boss body a check's room shows.</summary>
    extension(EnemyPresentation self)
    {
        /// <summary>The body the room's boss attached, whose parts show its frames.</summary>
        /// <exception cref="InvalidOperationException">No boss has attached a body of parts.</exception>
        internal BossBodyParts RequireBossBody() =>
            (self.BossBodyDisplay as PlacedBossBody)?.Parts ?? throw new InvalidOperationException("No boss has attached its body of parts.");
    }
}
