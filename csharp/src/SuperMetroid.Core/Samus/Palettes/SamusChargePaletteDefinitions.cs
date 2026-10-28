namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The bounded bank-$91 suit and phase domains of Samus's charge and Hyper-shot palette lists.</summary>
/// <remarks>
/// These domains are verified against every top-level and nested list entry in the pinned cartridge. The colors they
/// select remain authored presentation data.
/// </remarks>
public static class SamusChargePaletteDefinitions
{
    /// <summary>Highest native byte offset into either six-word charge list.</summary>
    public const ushort LastChargePhaseByteOffset = 10;
    /// <summary>First native byte offset into the reversed Hyper-shot list.</summary>
    public const ushort FirstHyperTableByteOffset = 2;
    /// <summary>Last native byte offset into the reversed Hyper-shot list.</summary>
    public const ushort LastHyperTableByteOffset = 0x14;
}
