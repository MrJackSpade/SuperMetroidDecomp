
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The cartridge's trail program and selector tables, which only verification reads.</summary>
internal static class SamusProjectileRomDataTrailsConstants
{
    /// <summary>$90:B5BB, the left trail program table.</summary>
    public const int LeftInstructionPointers = 0x90b5bb;

    /// <summary>$90:B609, the right trail program table.</summary>
    public const int RightInstructionPointers = 0x90b609;

    /// <summary>Number of trail pointer entries before the paired right table.</summary>
    public const int InstructionPointerCount = (RightInstructionPointers - LeftInstructionPointers) / sizeof(ushort);

    /// <summary>$9B:A4B3, the uncharged trail selector table.</summary>
    public const int UnchargedOffsetFamilies = 0x9ba4b3;

    /// <summary>$9B:A4CB, the charged trail selector table.</summary>
    public const int ChargedOffsetFamilies = 0x9ba4cb;

    /// <summary>$9B:A4E3, the special beam attack trail selector table.</summary>
    public const int SpazerSbaOffsetFamilies = 0x9ba4e3;
}
