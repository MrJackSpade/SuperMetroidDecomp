namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The bounded step domains of Samus's charge and Hyper-shot colour cycles.</summary>
/// <remarks>
/// These domains are verified against every top-level and nested list entry in the pinned cartridge. The colors they
/// select remain authored presentation data.
/// </remarks>
public static class SamusChargePaletteDefinitions
{
    /// <summary>The steps of the charge and pseudo-Screw colour cycles; the cycle wraps to its first step after the last.</summary>
    public const int ChargeSteps = 6;
    /// <summary>First native byte offset into the reversed Hyper-shot list, which the shot-glow timer encodes.</summary>
    public const ushort FirstHyperTableByteOffset = 2;
    /// <summary>Last native byte offset into the reversed Hyper-shot list, which the shot-glow timer encodes.</summary>
    public const ushort LastHyperTableByteOffset = 0x14;
}
