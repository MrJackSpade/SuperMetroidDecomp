namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The installed key of each <see cref="ProjectileTrailFrame"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ProjectileTrailFrameKeys
{
    /// <summary>Each ProjectileTrailFrame member's installed key.</summary>
    internal static InstalledKeys<ProjectileTrailFrame> Installed { get; } = new(new Dictionary<ProjectileTrailFrame, string>
    {
        [ProjectileTrailFrame.IceSparkleClustered] = "IceSparkleClustered",
        [ProjectileTrailFrame.IceSparkleTwinkling] = "IceSparkleTwinkling",
        [ProjectileTrailFrame.IceSparkleScattered] = "IceSparkleScattered",
        [ProjectileTrailFrame.IceSparkleFading] = "IceSparkleFading",
        [ProjectileTrailFrame.WaveRing] = "WaveRing",
        [ProjectileTrailFrame.WaveRingSpreading] = "WaveRingSpreading",
        [ProjectileTrailFrame.WaveRingWide] = "WaveRingWide",
        [ProjectileTrailFrame.WaveRingBreaking] = "WaveRingBreaking",
        [ProjectileTrailFrame.MissileSmokePuff] = "MissileSmokePuff",
        [ProjectileTrailFrame.MissileSmokeBillowing] = "MissileSmokeBillowing",
        [ProjectileTrailFrame.MissileSmokeThinning] = "MissileSmokeThinning",
        [ProjectileTrailFrame.MissileSmokeFading] = "MissileSmokeFading",
    });
}
