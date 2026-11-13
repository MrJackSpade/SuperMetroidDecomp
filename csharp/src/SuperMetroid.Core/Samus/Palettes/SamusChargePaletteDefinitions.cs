namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The bounded step domains of Samus's charge and Hyper-shot colour cycles.</summary>
/// <remarks>
/// These domains are verified against every top-level and nested list entry in the authored data. The colors they
/// select remain authored presentation data, which the import pre-colours Samus in (#1512).
/// </remarks>
public static class SamusChargePaletteDefinitions
{
    /// <summary>The steps of the charge and pseudo-Screw colour cycles; the cycle wraps to its first step after the last.</summary>
    public const int ChargeSteps = 6;
    /// <summary>The Hyper-shot glow's steps that show a frame of the Hyper Beam's full-body glow, one per frame.</summary>
    public const int HyperShotSteps = 10;
}
