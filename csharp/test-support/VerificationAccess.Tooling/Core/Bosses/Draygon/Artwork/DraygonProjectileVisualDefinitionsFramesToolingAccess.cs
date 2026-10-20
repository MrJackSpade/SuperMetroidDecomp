namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Verification access to <see cref="DraygonProjectileVisualDefinitionsFramesTooling"/> members production does not use.</summary>
internal static class DraygonProjectileVisualDefinitionsFramesToolingAccess
{
    /// <summary>Exposes <see cref="DraygonProjectileVisualDefinitionsFramesTooling"/> state that only verification reads.</summary>
    extension(DraygonProjectileVisualDefinitions)
    {
        /// <summary>Distinct frames: six goop, two goop-burst, sixteen bloom and three flight frames.</summary>
        internal static int ProjectileFrameCount => 27;
    }
}
