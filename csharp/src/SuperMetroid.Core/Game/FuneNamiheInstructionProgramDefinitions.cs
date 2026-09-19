namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A8 address.</summary>
internal readonly record struct FuneNamiheInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from all eight Fune and Namihe instruction programs.</summary>
/// <remarks>
/// Durations, callbacks, common sleep/goto opcodes, and loop targets affect simulation and
/// live here. Each word following a duration selects replaceable spritemap presentation and
/// deliberately remains a live cartridge read.
/// </remarks>
internal static class FuneNamiheInstructionProgramDefinitions
{
    /// <summary><c>$A8:939F</c>, active left-facing Fune program.</summary>
    internal const ushort FuneActiveLeft = 0x939f;

    /// <summary><c>$A8:93CF</c>, active right-facing Fune program.</summary>
    internal const ushort FuneActiveRight = 0x93cf;

    /// <summary><c>$A8:9399</c>, idle left-facing Fune program.</summary>
    internal const ushort FuneIdleLeft = 0x9399;

    /// <summary><c>$A8:93C9</c>, idle right-facing Fune program.</summary>
    internal const ushort FuneIdleRight = 0x93c9;

    /// <summary><c>$A8:95C3</c>, active left-facing Namihe program.</summary>
    internal const ushort NamiheActiveLeft = 0x95c3;

    /// <summary><c>$A8:95F7</c>, active right-facing Namihe program.</summary>
    internal const ushort NamiheActiveRight = 0x95f7;

    /// <summary><c>$A8:95BD</c>, idle left-facing Namihe program.</summary>
    internal const ushort NamiheIdleLeft = 0x95bd;

    /// <summary><c>$A8:95F1</c>, idle right-facing Namihe program.</summary>
    internal const ushort NamiheIdleRight = 0x95f1;

    private static readonly FuneNamiheInstructionMechanicsWord[] Words =
    [
        new(0x9399, 0x0001), new(0x939d, 0x812f),
        new(0x939f, 0x0010), new(0x93a3, 0x0008), new(0x93a7, 0x0008),
        new(0x93ab, 0x0008), new(0x93af, 0x9663), new(0x93b1, 0x9625),
        new(0x93b3, 0x0010), new(0x93b7, 0x0008), new(0x93bb, 0x0008),
        new(0x93bf, 0x0008), new(0x93c3, 0x9695), new(0x93c5, 0x80ed),
        new(0x93c7, 0x9399), new(0x93c9, 0x0001), new(0x93cd, 0x812f),
        new(0x93cf, 0x0010), new(0x93d3, 0x0008), new(0x93d7, 0x0008),
        new(0x93db, 0x0008), new(0x93df, 0x967c), new(0x93e1, 0x9625),
        new(0x93e3, 0x0010), new(0x93e7, 0x0008), new(0x93eb, 0x0008),
        new(0x93ef, 0x0008), new(0x93f3, 0x96b4), new(0x93f5, 0x80ed),
        new(0x93f7, 0x93c9),
        new(0x95bd, 0x0001), new(0x95c1, 0x812f),
        new(0x95c3, 0x0008), new(0x95c7, 0x0008), new(0x95cb, 0x0008),
        new(0x95cf, 0x0008), new(0x95d3, 0x0008), new(0x95d7, 0x9631),
        new(0x95d9, 0x9625), new(0x95db, 0x0008), new(0x95df, 0x0008),
        new(0x95e3, 0x0008), new(0x95e7, 0x0008), new(0x95eb, 0x9695),
        new(0x95ed, 0x80ed), new(0x95ef, 0x95bd),
        new(0x95f1, 0x0001), new(0x95f5, 0x812f),
        new(0x95f7, 0x0008), new(0x95fb, 0x0008), new(0x95ff, 0x0008),
        new(0x9603, 0x0008), new(0x9607, 0x0008), new(0x960b, 0x964a),
        new(0x960d, 0x9625), new(0x960f, 0x0008), new(0x9613, 0x0008),
        new(0x9617, 0x0008), new(0x961b, 0x0008), new(0x961f, 0x96b4),
        new(0x9621, 0x80ed), new(0x9623, 0x95f1),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x939b, 0x93a1, 0x93a5, 0x93a9, 0x93ad, 0x93b5, 0x93b9, 0x93bd,
        0x93c1, 0x93cb, 0x93d1, 0x93d5, 0x93d9, 0x93dd, 0x93e5, 0x93e9,
        0x93ed, 0x93f1, 0x95bf, 0x95c5, 0x95c9, 0x95cd, 0x95d1, 0x95d5,
        0x95dd, 0x95e1, 0x95e5, 0x95e9, 0x95f3, 0x95f9, 0x95fd, 0x9601,
        0x9605, 0x9609, 0x9611, 0x9615, 0x9619, 0x961d,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;

    internal static FuneNamiheInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns one fixed control word or rejects pointers outside all eight lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FuneNamiheInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Fune/Namihe instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    /// <summary>True when an absolute address names a byte owned by compiled mechanics.</summary>
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
