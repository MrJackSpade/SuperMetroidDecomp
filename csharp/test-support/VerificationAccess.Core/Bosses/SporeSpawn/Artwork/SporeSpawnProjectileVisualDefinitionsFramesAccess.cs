using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileProgram;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Frame lists and counts of <see cref="SporeSpawnProjectileVisualDefinitions"/> that only verification read.</summary>
internal static class SporeSpawnProjectileVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="SporeSpawnProjectileVisualDefinitions"/>.</summary>
    extension(SporeSpawnProjectileVisualDefinitions)
    {
        /// <summary>Every frame in installed order.</summary>
        internal static IReadOnlyList<SporeSpawnProjectileFrame> ProjectileFrames =>
        [
            .. new[] { P.SpawnerClosed, P.SpawnerRelease, P.Spore, P.Stalk, P.SporeShot }.SelectMany(program =>
                Enumerable.Range(0, SporeSpawnProjectileAnimationsConstants.PoseCount(program)).Select(phase => new SporeSpawnProjectileFrame(program, phase))),
        ];

        /// <summary>The installed projectile frames.</summary>
        internal static int ProjectileFrameCount => 17;
    }
}
