namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bank-$84 Brinstar plant identities; these are block actors, not Yapping Maws.</summary>
internal static class SamusEaterPlmRomData
{
    /// <summary>$84:B0FB/$B12E clear the special-air reaction bits while preserving the artwork and high collision bit.</summary>
    public const ushort DeactivatedTriggerMask = 0x8fff;
    /// <summary>The chew sound, library 2.</summary>
    public const byte ChewSound = 0x31;
    /// <summary>The invincibility bit the hold ORs into Samus's timer each frame.</summary>
    public const ushort HeldInvincibilityBit = 0x10;
    /// <summary>The immunity the release gives Samus, in frames.</summary>
    public const ushort ReleaseInvincibilityFrames = 48;
    /// <summary>The periodic damage each chew deals: whole and subunit parts.</summary>
    public const ushort ChewDamage = 2;
}
