namespace SuperMetroid.Core.Audio;

/// <summary>Immutable lookup tables embedded in Super Metroid's uploaded music driver.</summary>
internal static class SpcMusicTables
{
    /// <summary>Argument-byte count for opcodes $E0-$FE.</summary>
    internal static readonly byte[] EffectByteLengths =
    [
        1, 1, 2, 3, 0, 1, 2, 1, 2, 1, 1, 3, 0, 1, 2, 3,
        1, 3, 3, 0, 1, 3, 0, 3, 3, 3, 1, 2, 0, 0, 0,
    ];

    /// <summary>Nonlinear pan curve sampled at integer positions zero through 21.</summary>
    internal static readonly byte[] PanVolume =
        [0, 1, 3, 7, 13, 21, 30, 41, 52, 66, 81, 94, 103, 110, 115, 119, 122, 124, 125, 126, 127, 127];

    /// <summary>One-octave pitch basis plus the next C used for fractional interpolation.</summary>
    internal static readonly ushort[] BaseNoteFrequencies =
        [2143, 2270, 2405, 2548, 2700, 2860, 3030, 3211, 3402, 3604, 3818, 4045, 4286];

    internal static readonly byte[] NoteVolumes =
        [25, 50, 76, 101, 114, 127, 140, 152, 165, 178, 191, 203, 216, 229, 242, 252];

    internal static readonly byte[] NoteGateOffPercentages =
        [50, 101, 127, 152, 178, 203, 229, 252];

    /// <summary>Four eight-tap echo FIR presets selected by effect $F7.</summary>
    internal static readonly sbyte[] EchoFirParameters =
    [
        127, 0, 0, 0, 0, 0, 0, 0,
        88, -65, -37, -16, -2, 7, 12, 12,
        12, 33, 43, 43, 19, -2, -13, -7,
        52, 51, 0, -39, -27, 1, -4, -21,
    ];

    static SpcMusicTables()
    {
        if (EffectByteLengths.Length != 31 || BaseNoteFrequencies.Length != 13 ||
            EchoFirParameters.Length != 32)
        {
            throw new InvalidDataException("One or more fixed SPC music tables have an invalid length.");
        }
    }
}
