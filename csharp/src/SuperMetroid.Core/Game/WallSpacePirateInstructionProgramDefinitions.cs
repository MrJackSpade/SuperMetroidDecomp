namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$B2 address.</summary>
internal readonly record struct WallSpacePirateInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for all wall Space Pirate body programs.
/// Interleaved extended-spritemap pointers remain live cartridge presentation data.
/// </summary>
internal static class WallSpacePirateInstructionProgramDefinitions
{
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpLeft</c> at $B2:ECC0.</summary>
    internal const ushort FireAndJumpLeft = 0xecc0;
    /// <summary><c>InstList_PirateWall_LandedOnLeftWall</c> at $B2:ECE4.</summary>
    internal const ushort LandedOnLeftWall = 0xece4;
    /// <summary><c>InstList_PirateWall_MovingUpLeftWall_0</c> at $B2:ECEC.</summary>
    internal const ushort MovingUpLeftWall = 0xecec;
    /// <summary><c>InstList_PirateWall_MovingDownLeftWall_0</c> at $B2:ED36.</summary>
    internal const ushort MovingDownLeftWall = 0xed36;
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpRight</c> at $B2:ED80.</summary>
    internal const ushort FireAndJumpRight = 0xed80;
    /// <summary><c>InstList_PirateWall_LandingOnRightWall</c> at $B2:EDA4.</summary>
    internal const ushort LandedOnRightWall = 0xeda4;
    /// <summary><c>InstList_PirateWall_MovingDownRightWall_0</c> at $B2:EDAC.</summary>
    internal const ushort MovingDownRightWall = 0xedac;
    /// <summary><c>InstList_PirateWall_MovingUpRightWall_0</c> at $B2:EDF6.</summary>
    internal const ushort MovingUpRightWall = 0xedf6;

    private static readonly WallSpacePirateInstructionMechanicsWord[] Words =
    [
        new(0xecc0, 0xef83), new(0xecc2, 0xf0e3), new(0xecc4, 0x0009),
        new(0xecc8, 0x000f), new(0xeccc, 0xef2a), new(0xecce, 0x813a),
        new(0xecd0, 0x0020), new(0xecd2, 0xeefd), new(0xecd4, 0xef83),
        new(0xecd6, 0xf0e4), new(0xecd8, 0xef93), new(0xecda, 0x000a),
        new(0xecde, 0x0001), new(0xece2, 0x812f), new(0xece4, 0xef83),
        new(0xece6, 0xf034), new(0xece8, 0x000a), new(0xecec, 0xef83),
        new(0xecee, 0xf034), new(0xecf0, 0x8123), new(0xecf2, 0x0004),
        new(0xecf4, 0x000a), new(0xecf8, 0xee40), new(0xecfa, 0xfffd),
        new(0xecfc, 0x0008), new(0xed00, 0xee40), new(0xed02, 0xfffd),
        new(0xed04, 0x0005), new(0xed08, 0xee40), new(0xed0a, 0xfffd),
        new(0xed0c, 0x0008), new(0xed10, 0xee40), new(0xed12, 0xfffd),
        new(0xed14, 0x000a), new(0xed18, 0xee40), new(0xed1a, 0xfffd),
        new(0xed1c, 0x0008), new(0xed20, 0xee40), new(0xed22, 0xfffd),
        new(0xed24, 0x0005), new(0xed28, 0xee40), new(0xed2a, 0xfffd),
        new(0xed2c, 0x0008), new(0xed30, 0x8110), new(0xed32, 0xecf4),
        new(0xed34, 0xeea4), new(0xed36, 0xef83), new(0xed38, 0xf034),
        new(0xed3a, 0x8123), new(0xed3c, 0x0004), new(0xed3e, 0x000a),
        new(0xed42, 0xee40), new(0xed44, 0x0003), new(0xed46, 0x0008),
        new(0xed4a, 0xee40), new(0xed4c, 0x0003), new(0xed4e, 0x0005),
        new(0xed52, 0xee40), new(0xed54, 0x0003), new(0xed56, 0x0008),
        new(0xed5a, 0xee40), new(0xed5c, 0x0003), new(0xed5e, 0x000a),
        new(0xed62, 0xee40), new(0xed64, 0x0003), new(0xed66, 0x0008),
        new(0xed6a, 0xee40), new(0xed6c, 0x0003), new(0xed6e, 0x0005),
        new(0xed72, 0xee40), new(0xed74, 0x0003), new(0xed76, 0x0008),
        new(0xed7a, 0x8110), new(0xed7c, 0xed3e), new(0xed7e, 0xeea4),
        new(0xed80, 0xef83), new(0xed82, 0xf04f), new(0xed84, 0x0009),
        new(0xed88, 0x0001), new(0xed8c, 0xef5d), new(0xed8e, 0x813a),
        new(0xed90, 0x0020), new(0xed92, 0xeed4), new(0xed94, 0xef83),
        new(0xed96, 0xf050), new(0xed98, 0xef93), new(0xed9a, 0x000a),
        new(0xed9e, 0x0001), new(0xeda2, 0x812f), new(0xeda4, 0xef83),
        new(0xeda6, 0xf0c8), new(0xeda8, 0x000a), new(0xedac, 0xef83),
        new(0xedae, 0xf0c8), new(0xedb0, 0x8123), new(0xedb2, 0x0004),
        new(0xedb4, 0x000a), new(0xedb8, 0xee72), new(0xedba, 0x0003),
        new(0xedbc, 0x0008), new(0xedc0, 0xee72), new(0xedc2, 0x0003),
        new(0xedc4, 0x0005), new(0xedc8, 0xee72), new(0xedca, 0x0003),
        new(0xedcc, 0x0008), new(0xedd0, 0xee72), new(0xedd2, 0x0003),
        new(0xedd4, 0x000a), new(0xedd8, 0xee72), new(0xedda, 0x0003),
        new(0xeddc, 0x0008), new(0xede0, 0xee72), new(0xede2, 0x0003),
        new(0xede4, 0x0005), new(0xede8, 0xee72), new(0xedea, 0x0003),
        new(0xedec, 0x0008), new(0xedf0, 0x8110), new(0xedf2, 0xedb4),
        new(0xedf4, 0xeebc), new(0xedf6, 0xef83), new(0xedf8, 0xf0c8),
        new(0xedfa, 0x8123), new(0xedfc, 0x0004), new(0xedfe, 0x000a),
        new(0xee02, 0xee72), new(0xee04, 0xfffd), new(0xee06, 0x0008),
        new(0xee0a, 0xee72), new(0xee0c, 0xfffd), new(0xee0e, 0x0005),
        new(0xee12, 0xee72), new(0xee14, 0xfffd), new(0xee16, 0x0008),
        new(0xee1a, 0xee72), new(0xee1c, 0xfffd), new(0xee1e, 0x000a),
        new(0xee22, 0xee72), new(0xee24, 0xfffd), new(0xee26, 0x0008),
        new(0xee2a, 0xee72), new(0xee2c, 0xfffd), new(0xee2e, 0x0005),
        new(0xee32, 0xee72), new(0xee34, 0xfffd), new(0xee36, 0x0008),
        new(0xee3a, 0x8110), new(0xee3c, 0xedfe), new(0xee3e, 0xeebc),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xecc6, 0xecca, 0xecdc, 0xece0, 0xecea, 0xecf6, 0xecfe,
        0xed06, 0xed0e, 0xed16, 0xed1e, 0xed26, 0xed2e, 0xed40,
        0xed48, 0xed50, 0xed58, 0xed60, 0xed68, 0xed70, 0xed78,
        0xed86, 0xed8a, 0xed9c, 0xeda0, 0xedaa, 0xedb6, 0xedbe,
        0xedc6, 0xedce, 0xedd6, 0xedde, 0xede6, 0xedee, 0xee00,
        0xee08, 0xee10, 0xee18, 0xee20, 0xee28, 0xee30, 0xee38,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static WallSpacePirateInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            WallSpacePirateInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Wall Space Pirate instruction mechanics pointer $B2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
