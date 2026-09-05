/// <summary>Cartridge identities used by the focused rideable-platform audit.</summary>
internal static class PlatformAuditRomData
{
    /// <summary>
    /// Room header <c>$8F:ADAD</c>, Norfair map room <c>$02/$1E</c>. Its fourth enemy
    /// population record is the upward-moving Kamer that exposed grounded pose churn.
    /// </summary>
    public const ushort Room021eHeaderPointer = 0xadad;

    /// <summary>
    /// Managed slot for the fourth native population record in room <c>$02/$1E</c>, the
    /// Kamer beginning at world <c>($00D0,$00E8)</c> and travelling upward.
    /// </summary>
    public const int Room021eRisingKamerSlot = 3;
}
