namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile programs (#641), in authored order.</summary>
internal enum TourianStatueProjectileProgram
{
    /// <summary>The private delete list.</summary>
    Delete,

    /// <summary>A particle's water splash.</summary>
    Splash,

    /// <summary>A boss statue's glowing eyes, which release the particles.</summary>
    EyeGlow,

    /// <summary>An unlocking particle.</summary>
    Particle,

    /// <summary>A particle's fading tail.</summary>
    Tail,

    /// <summary>A rising statue soul.</summary>
    Soul,

    /// <summary>The entrance statue base decoration, until it may process.</summary>
    BaseDecoration,

    /// <summary>The base decoration's resting pose.</summary>
    BaseDecorationLoop,

    /// <summary>Ridley's statue.</summary>
    Ridley,

    /// <summary>Phantoon's statue.</summary>
    Phantoon,
}
