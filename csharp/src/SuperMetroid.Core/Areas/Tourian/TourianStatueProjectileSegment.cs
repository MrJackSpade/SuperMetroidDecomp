namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statues' unlock effect animations (#641, #2554), in authored order.</summary>
internal enum TourianStatueProjectileSegment
{
    /// <summary>A particle's water splash.</summary>
    Splash,

    /// <summary>A boss statue's glowing eyes, which release the particles.</summary>
    EyeGlow,

    /// <summary>An unlocking particle, playing its run again and again as it falls.</summary>
    Particle,

    /// <summary>A particle's fading tail.</summary>
    Tail,

    /// <summary>A rising statue soul.</summary>
    Soul,

}
