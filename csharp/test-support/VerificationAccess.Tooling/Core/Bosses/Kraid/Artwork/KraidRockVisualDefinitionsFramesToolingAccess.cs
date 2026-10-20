namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Verification access to <see cref="KraidRockVisualDefinitionsFramesTooling"/> members production does not use.</summary>
internal static class KraidRockVisualDefinitionsFramesToolingAccess
{
    /// <summary>Exposes <see cref="KraidRockVisualDefinitionsFramesTooling"/> state that only verification reads.</summary>
    extension(KraidRockVisualDefinitions)
    {
        /// <summary>Distinct frames: the two still poses and five burst frames.</summary>
        internal static int ProjectileFrameCount => 7;
    }
}
