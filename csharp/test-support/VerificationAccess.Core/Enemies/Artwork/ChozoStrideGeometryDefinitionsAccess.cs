namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="ChozoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class ChozoStrideGeometryDefinitionsAccess
{
    /// <summary>The native addresses of the eight stride poses.</summary>
    extension(ChozoStrideGeometryDefinitions)
    {
        /// <summary>The packed bank-$AA address of the composition for stride pose <paramref name="pose"/>.</summary>
        internal static int NativePoseIdentity(int pose)
        {
            if ((uint)pose >= ChozoStrideGeometryDefinitions.Poses.Length) throw new ArgumentOutOfRangeException(nameof(pose));
            return EnemyArtworkCatalogsToolingConstants.NativeIdentity(ChozoStrideGeometryDefinitions.Poses[pose]);
        }
    }
}
