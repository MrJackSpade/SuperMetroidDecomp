namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A trail sparkle's frame (#2348), named for what it shows, which selects its installed appearance. Both ice sparkles show
/// the same frames, and a frame a sparkle shows again is the same frame.
/// </summary>
public enum ProjectileTrailFrame
{
    /// <summary>An ice sparkle as a cluster of bright stars.</summary>
    IceSparkleClustered,
    /// <summary>An ice sparkle as two twinkling stars.</summary>
    IceSparkleTwinkling,
    /// <summary>An ice sparkle scattered into dim specks.</summary>
    IceSparkleScattered,
    /// <summary>An ice sparkle's last dark specks, fading.</summary>
    IceSparkleFading,
    /// <summary>The wave trail as a small bright ring.</summary>
    WaveRing,
    /// <summary>The wave trail's ring spreading wider and dimmer.</summary>
    WaveRingSpreading,
    /// <summary>The wave trail's ring at its widest and darkest.</summary>
    WaveRingWide,
    /// <summary>The wave trail's ring broken into specks.</summary>
    WaveRingBreaking,
    /// <summary>The missile trail as a dense puff of smoke.</summary>
    MissileSmokePuff,
    /// <summary>The missile trail's smoke billowing out.</summary>
    MissileSmokeBillowing,
    /// <summary>The missile trail's smoke thinning.</summary>
    MissileSmokeThinning,
    /// <summary>The missile trail's last wisps of smoke, fading.</summary>
    MissileSmokeFading,
}
