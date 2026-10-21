using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    /// <summary>The native frame addresses of the ten stride phases.</summary>
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        /// <summary>The packed cartridge address of the walking frame for <paramref name="phase"/>.</summary>
        internal static int NativeFrameIdentity(int phase)
        {
            if ((uint)phase >= GoldenTorizoStrideGeometryDefinitions.WalkingFrames.Length) throw new ArgumentOutOfRangeException(nameof(phase));
            return EnemyArtworkCatalogsToolingConstants.NativeIdentity(GoldenTorizoStrideGeometryDefinitions.WalkingFrames[phase]);
        }
    }
}
