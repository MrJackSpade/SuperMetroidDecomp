namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable $8B:E5E7 palette-source selector for the sixteen final-logo fade steps.
/// The selected bank-$8C RGB5 colors remain presentation data, not engine definitions.
/// </summary>
internal static class EndingLogoPalettePointerDefinitions
{
    /// <summary>$8B:E5E7, sixteen pairs of reverse-copy bank-$8C palette pointers.</summary>
    public const int NativeTableAddress = 0x8be5e7;

    /// <summary>
    /// $8B:E5E7..E626. Each pair selects the two OBJ palettes copied by $8B:E58A;
    /// pair order and the reverse source traversal are cartridge mechanics.
    /// </summary>
    private static readonly EndingLogoPalettePointerPair[] Steps =
    [
        new(0xf3e7, 0xf007), new(0xf3c7, 0xf027),
        new(0xf3a7, 0xf047), new(0xf387, 0xf067),
        new(0xf367, 0xf087), new(0xf347, 0xf0a7),
        new(0xf327, 0xf0c7), new(0xf307, 0xf0e7),
        new(0xf2e7, 0xf107), new(0xf2c7, 0xf127),
        new(0xf2a7, 0xf147), new(0xf287, 0xf167),
        new(0xf267, 0xf187), new(0xf247, 0xf1a7),
        new(0xf227, 0xf1c7), new(0xf207, 0xf1e7),
    ];

    /// <summary>Returns the native source pointer for one fade step and OBJ palette.</summary>
    public static ushort Source(int step, int palette)
    {
        if ((uint)step >= Steps.Length)
            throw new ArgumentOutOfRangeException(nameof(step));
        return palette switch
        {
            0 => Steps[step].First,
            1 => Steps[step].Second,
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }
}

/// <summary>One immutable native pair of reverse-copy palette sources.</summary>
internal readonly record struct EndingLogoPalettePointerPair(ushort First, ushort Second);
