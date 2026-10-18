using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Frame lists and counts of <see cref="KraidRockVisualDefinitions"/> that only development tools read.</summary>
internal static class KraidRockVisualDefinitionsFramesTooling
{
    /// <summary>The members, read as members of <see cref="KraidRockVisualDefinitions"/>.</summary>
    extension(KraidRockVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order: the two poses, then the burst.</summary>
        internal static IReadOnlyList<KraidRockFrame> ProjectileFrames =>
        [
            new KraidRockFrame(KraidRockProgram.SharedPose, 0),
            new KraidRockFrame(KraidRockProgram.RisingRightPose, 0),
            .. Enumerable.Range(0, KraidRockAnimations.ShotPhaseCount).Select(phase => new KraidRockFrame(KraidRockProgram.SpitRockShot, phase)),
        ];
    }
}
