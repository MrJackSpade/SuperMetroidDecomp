namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>What one of Samus's particles shows: a splash, bubbles, lava spray or dust.</summary>
public enum SamusParticleKind
{
    /// <summary>A small splash at a wet footstep, a wet landing or a shallow entry into water.</summary>
    SurfaceSplash,
    /// <summary>The splash of a dive into or out of water, held on the water's surface.</summary>
    DivingSplash,
    /// <summary>Spray thrown up and outward where Samus breaks a lava or acid surface.</summary>
    LavaSpray,
    /// <summary>Breath bubbles rising from Samus under water.</summary>
    Bubbles,
    /// <summary>Rising dust from a landing, a wall jump or a boosted run.</summary>
    Dust,
}
