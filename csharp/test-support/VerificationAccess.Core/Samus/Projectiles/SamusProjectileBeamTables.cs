
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where the cartridge keeps Samus's beam tables, and their record widths, for verification.</summary>
internal static class SamusProjectileBeamTables
{
    /// <summary>Charged firing sound IDs indexed by beam combination.</summary>
    public const int ChargedSounds = 0x90c2a7;
    /// <summary>Damage word followed by one instruction pointer per direction.</summary>
    public const int DataRecordByteCount = sizeof(ushort) + SamusFiringTableDefinitions.DirectionCount * sizeof(ushort);
    /// <summary>Duration, spritemap, radii, and animation word in one instruction record.</summary>
    public const int InstructionRecordByteCount = 8;
    /// <summary>Uncharged firing sound IDs indexed by beam combination.</summary>
    public const int UnchargedSounds = 0x90c28f;

    /// <summary>$93:83C1: bank-$93 projectile-data pointers for uncharged combinations.</summary>
    public const int UnchargedDataPointers = 0x9383c1;
    /// <summary>$93:83D9: bank-$93 projectile-data pointers for charged combinations.</summary>
    public const int ChargedDataPointers = 0x9383d9;
}
