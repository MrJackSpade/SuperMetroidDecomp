namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 Brinstar plant identities; these are block actors, not Yapping Maws.</summary>
internal static class SamusEaterPlmRomData
{
    /// <summary>$84:B0FB/$B12E clear the special-air reaction bits while preserving the artwork and high collision bit.</summary>
    public const ushort DeactivatedTriggerMask = 0x8fff;
    /// <summary>$84:AC89, pin Samus to saved coordinates and OR immunity with $10.</summary>
    public const ushort HoldPreInstruction = 0xac89;
}
