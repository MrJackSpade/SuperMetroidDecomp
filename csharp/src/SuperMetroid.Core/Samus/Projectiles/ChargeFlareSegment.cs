namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One of the charge flare's runs of pictures (#2285).</summary>
public enum ChargeFlareSegment
{
    /// <summary>The main flare's opening, which it plays once: its pictures brightening toward the loop.</summary>
    MainFlareOpening,

    /// <summary>The main flare's loop, which it repeats once the opening has played.</summary>
    MainFlareLoop,

    /// <summary>The slow spark ring's cycle.</summary>
    SlowSparks,

    /// <summary>The fast spark ring's cycle.</summary>
    FastSparks,
}
