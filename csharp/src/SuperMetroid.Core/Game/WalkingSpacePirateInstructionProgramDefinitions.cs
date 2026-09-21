namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$B2 address.</summary>
internal readonly record struct WalkingSpacePirateInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for all walking Space Pirate body programs.
/// Interleaved extended-spritemap pointers remain live cartridge presentation data.
/// </summary>
internal static class WalkingSpacePirateInstructionProgramDefinitions
{
    /// <summary><c>InstList_PirateWalking_Flinch_FacingLeft</c> at $B2:FB4C.</summary>
    internal const ushort FlinchFacingLeft = 0xfb4c;
    /// <summary><c>InstList_PirateWalking_Flinch_FacingRight</c> at $B2:FB58.</summary>
    internal const ushort FlinchFacingRight = 0xfb58;
    /// <summary><c>InstList_PirateWalking_WalkingLeft_0</c> at $B2:FB64.</summary>
    internal const ushort WalkingLeft = 0xfb64;
    /// <summary><c>InstList_PirateWalking_FireLasersLeft</c> at $B2:FB8C.</summary>
    internal const ushort FireLasersLeft = 0xfb8c;
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingLeft</c> at $B2:FBC6.</summary>
    internal const ushort LookingFacingLeft = 0xfbc6;
    /// <summary><c>InstList_PirateWalking_WalkingRight_0</c> at $B2:FBE6.</summary>
    internal const ushort WalkingRight = 0xfbe6;
    /// <summary><c>InstList_PirateWalking_FireLasersRight</c> at $B2:FC0E.</summary>
    internal const ushort FireLasersRight = 0xfc0e;
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingRight</c> at $B2:FC48.</summary>
    internal const ushort LookingFacingRight = 0xfc48;

    private static readonly WalkingSpacePirateInstructionMechanicsWord[] Words =
    [
        new(0xfb4c, 0xfcb8), new(0xfb4e, 0x804b), new(0xfb50, 0x0010),
        new(0xfb54, 0x80ed), new(0xfb56, 0xfb64),
        new(0xfb58, 0xfcb8), new(0xfb5a, 0x804b), new(0xfb5c, 0x0010),
        new(0xfb60, 0x80ed), new(0xfb62, 0xfbe6),
        new(0xfb64, 0xfcb8), new(0xfb66, 0xfd44),
        new(0xfb68, 0x000a), new(0xfb6c, 0x000a), new(0xfb70, 0x000a),
        new(0xfb74, 0x000a), new(0xfb78, 0x000a), new(0xfb7c, 0x000a),
        new(0xfb80, 0x000a), new(0xfb84, 0x000a), new(0xfb88, 0x80ed),
        new(0xfb8a, 0xfb68),
        new(0xfb8c, 0xfcb8), new(0xfb8e, 0xfe4a), new(0xfb90, 0x0018),
        new(0xfb94, 0x0008), new(0xfb98, 0x0008), new(0xfb9c, 0x0008),
        new(0xfba0, 0xfc68), new(0xfba2, 0x0008), new(0xfba4, 0x0008),
        new(0xfba8, 0xfc68), new(0xfbaa, 0x0002), new(0xfbac, 0x0018),
        new(0xfbb0, 0xfc68), new(0xfbb2, 0xfff8), new(0xfbb4, 0x0008),
        new(0xfbb8, 0x0008), new(0xfbbc, 0x0008), new(0xfbc0, 0x0008),
        new(0xfbc4, 0xfcc8),
        new(0xfbc6, 0xfcb8), new(0xfbc8, 0xfe4a), new(0xfbca, 0x0020),
        new(0xfbce, 0x000a), new(0xfbd2, 0x0020), new(0xfbd6, 0x000a),
        new(0xfbda, 0x0020), new(0xfbde, 0x0008), new(0xfbe2, 0x80ed),
        new(0xfbe4, 0xfbe6),
        new(0xfbe6, 0xfcb8), new(0xfbe8, 0xfdce),
        new(0xfbea, 0x000a), new(0xfbee, 0x000a), new(0xfbf2, 0x000a),
        new(0xfbf6, 0x000a), new(0xfbfa, 0x000a), new(0xfbfe, 0x000a),
        new(0xfc02, 0x000a), new(0xfc06, 0x000a), new(0xfc0a, 0x80ed),
        new(0xfc0c, 0xfbea),
        new(0xfc0e, 0xfcb8), new(0xfc10, 0xfe4a), new(0xfc12, 0x0018),
        new(0xfc16, 0x0008), new(0xfc1a, 0x0008), new(0xfc1e, 0x0008),
        new(0xfc22, 0xfc90), new(0xfc24, 0x0008), new(0xfc26, 0x0008),
        new(0xfc2a, 0xfc90), new(0xfc2c, 0x0002), new(0xfc2e, 0x0018),
        new(0xfc32, 0xfc90), new(0xfc34, 0xfff8), new(0xfc36, 0x0008),
        new(0xfc3a, 0x0008), new(0xfc3e, 0x0008), new(0xfc42, 0x0008),
        new(0xfc46, 0xfcc8),
        new(0xfc48, 0xfcb8), new(0xfc4a, 0xfe4a), new(0xfc4c, 0x0020),
        new(0xfc50, 0x000a), new(0xfc54, 0x0020), new(0xfc58, 0x000a),
        new(0xfc5c, 0x0020), new(0xfc60, 0x0008), new(0xfc64, 0x80ed),
        new(0xfc66, 0xfb64),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xfb52, 0xfb5e,
        0xfb6a, 0xfb6e, 0xfb72, 0xfb76, 0xfb7a, 0xfb7e, 0xfb82, 0xfb86,
        0xfb92, 0xfb96, 0xfb9a, 0xfb9e, 0xfba6, 0xfbae, 0xfbb6, 0xfbba,
        0xfbbe, 0xfbc2,
        0xfbcc, 0xfbd0, 0xfbd4, 0xfbd8, 0xfbdc, 0xfbe0,
        0xfbec, 0xfbf0, 0xfbf4, 0xfbf8, 0xfbfc, 0xfc00, 0xfc04, 0xfc08,
        0xfc14, 0xfc18, 0xfc1c, 0xfc20, 0xfc28, 0xfc30, 0xfc38, 0xfc3c,
        0xfc40, 0xfc44,
        0xfc4e, 0xfc52, 0xfc56, 0xfc5a, 0xfc5e, 0xfc62,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static WalkingSpacePirateInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            WalkingSpacePirateInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Walking Space Pirate instruction mechanics pointer $B2:{address:X4} is not compiled.");
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
