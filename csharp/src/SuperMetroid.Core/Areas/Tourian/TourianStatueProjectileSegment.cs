namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile animations (#641), in authored order.</summary>
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

    /// <summary>The entrance statue base decoration: shown until it may follow the reveal, then resting on its pose for good.</summary>
    BaseDecoration,

    /// <summary>The statue of Ridley, one still picture for good.</summary>
    Ridley,

    /// <summary>The statue of Phantoon, one still picture for good.</summary>
    Phantoon,
}
