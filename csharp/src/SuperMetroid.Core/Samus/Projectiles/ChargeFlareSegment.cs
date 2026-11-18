namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One of the charge flare's runs of pictures (#2285, #2458).</summary>
public enum ChargeFlareSegment
{
    /// <summary>The main flare's first run, the ember and the glow, which its opening plays first.</summary>
    EmberAndGlow,

    /// <summary>The main flare's second run, the white ball and the glow, which its opening plays next.</summary>
    WhiteBallAndGlow,

    /// <summary>The main flare's loop run, the white ball and the starburst, which it repeats once the opening has played.</summary>
    WhiteBallAndStarburst,

    /// <summary>The slow spark ring's cycle.</summary>
    SlowSparks,

    /// <summary>The fast spark ring's cycle.</summary>
    FastSparks,
}
