namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 Brinstar plant identities; these are block actors, not Yapping Maws.</summary>
internal static class SamusEaterPlmRomData
{
    /// <summary>$84:B0FB/$B12E clear the special-air reaction bits while preserving the artwork and high collision bit.</summary>
    public const ushort DeactivatedTriggerMask = 0x8fff;
    /// <summary>$84:B0DC, floor-plant setup selected by Brinstar inside BTS $80.</summary>
    public const ushort FloorSetup = 0xb0dc;
    /// <summary>$84:B113, ceiling-plant setup selected by Brinstar inside BTS $81.</summary>
    public const ushort CeilingSetup = 0xb113;
    /// <summary>$84:AC89, pin Samus to saved coordinates and OR immunity with $10.</summary>
    public const ushort HoldPreInstruction = 0xac89;
    /// <summary>$84:AC9D, accumulate two whole points of periodic damage.</summary>
    public const ushort DamageInstruction = 0xac9d;
    /// <summary>$84:ACB1, publish $30 immunity before releasing the position owner.</summary>
    public const ushort ReleaseImmunityInstruction = 0xacb1;
}
