using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyFireballFrameKind;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Frame lists and counts of <see cref="RidleyFireballVisualDefinitions"/> that only verification read.</summary>
internal static class RidleyFireballVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="RidleyFireballVisualDefinitions"/>.</summary>
    extension(RidleyFireballVisualDefinitions)
    {
        /// <summary>Every frame in installed order.</summary>
        internal static IReadOnlyList<RidleyFireballFrame> ProjectileFrames =>
        [
            .. RidleyFireballVisualDefinitions.Run(K.Launch, RidleyFireballAnimations.LaunchPoseCount),
            .. RidleyFireballVisualDefinitions.Run(K.Flight, RidleyFireballAnimations.FlightPoseCount),
            .. RidleyFireballVisualDefinitions.Run(K.AfterburnFinal, RidleyFireballAnimations.AfterburnPoseCount),
            .. RidleyFireballVisualDefinitions.Run(K.HorizontalCenter, RidleyFireballAnimations.AfterburnPoseCount),
            .. RidleyFireballVisualDefinitions.Run(K.VerticalCenter, RidleyFireballAnimations.AfterburnPoseCount),
            .. RidleyFireballVisualDefinitions.Run(K.Directional, RidleyFireballAnimations.AfterburnPoseCount),
        ];

        /// <summary>The <paramref name="count"/> poses of <paramref name="kind"/>.</summary>
        private static IEnumerable<RidleyFireballFrame> Run(K kind, int count) =>
            Enumerable.Range(0, count).Select(phase => new RidleyFireballFrame(kind, phase));

        /// <summary>The installed fireball and afterburn frames.</summary>
        internal static int ProjectileFrameCount => 26;
    }
}
