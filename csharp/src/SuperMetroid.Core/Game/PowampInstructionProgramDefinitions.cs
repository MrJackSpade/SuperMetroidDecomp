namespace SuperMetroid.Core.Game;

internal readonly record struct PowampInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Powamp's body and balloon instruction programs.
/// The interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PowampInstructionProgramDefinitions
{
    /// <summary><c>InstList_Powamp_Body_FastAnimation</c> at $A8:C163.</summary>
    internal const ushort BodyFast = 0xc163;

    /// <summary><c>InstList_Powamp_Body_SlowAnimation</c> at $A8:C173.</summary>
    internal const ushort BodySlow = 0xc173;

    /// <summary><c>InstList_Powamp_Balloon_Inflate_0</c> at $A8:C183.</summary>
    internal const ushort BalloonInflate0 = 0xc183;

    /// <summary><c>InstList_Powamp_Balloon_Inflate_1</c> at $A8:C187.</summary>
    internal const ushort BalloonInflate1 = 0xc187;

    /// <summary><c>InstList_Powamp_Balloon_Inflate_2</c> at $A8:C18B.</summary>
    internal const ushort BalloonInflate2 = 0xc18b;

    /// <summary><c>InstList_Powamp_Balloon_StartSinking</c> at $A8:C191.</summary>
    internal const ushort BalloonStartSinking = 0xc191;

    /// <summary><c>InstList_Powamp_Balloon_Deflated</c> at $A8:C199.</summary>
    internal const ushort BalloonDeflated = 0xc199;

    /// <summary>The terminal common sleep word at $A8:C19D.</summary>
    internal const ushort BalloonDeflatedSleep = 0xc19d;

    /// <summary>The first non-program word after Powamp's instruction streams, at $A8:C19F.</summary>
    internal const ushort FirstAdjacentConstant = 0xc19f;

    private static readonly PowampInstructionMechanicsWord[] Words =
    [
        new(0xc163, 5), new(0xc167, 5), new(0xc16b, 5),
        new(0xc16f, CommonEnemyInstructionCodes.Goto), new(0xc171, BodyFast),
        new(0xc173, 9), new(0xc177, 9), new(0xc17b, 9),
        new(0xc17f, CommonEnemyInstructionCodes.Goto), new(0xc181, BodySlow),
        new(0xc183, 1), new(0xc187, 6), new(0xc18b, 0x00a0),
        new(0xc18f, CommonEnemyInstructionCodes.Sleep),
        new(0xc191, 1), new(0xc195, 6),
        new(0xc199, 0x00a0), new(BalloonDeflatedSleep, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xc165, 0xc169, 0xc16d,
        0xc175, 0xc179, 0xc17d,
        0xc185, 0xc189, 0xc18d,
        0xc193, 0xc197, 0xc19b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PowampInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PowampInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Powamp instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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
