namespace SuperMetroid.Core.Game;

/// <summary>Authored explosion choreography for Phantoon's death sequence.</summary>
public static class PhantoonDeathExplosionDefinitions
{
    /// <summary>$A7:D9FF: thirteen four-byte position/type/delay records begin at $DA1D.</summary>
    public const int Count = 13;
    /// <summary>$A7:DA04: repeat from record five after the initial pass.</summary>
    public const ushort RepeatStart = 5;
    /// <summary>$A7:DA11: finish after three trips through the end of the record table.</summary>
    public const ushort PassCount = 3;

    /// <summary>$A7:DA1D..DA50: signed X/Y offsets, miscellaneous-explosion animation index, and delay until the next request.</summary>
    public static (sbyte X, sbyte Y, byte Type, byte Delay) Read(int index) => index switch
    {
        0 => (0, 0, 0x1d, 16),
        1 => (32, -32, 0x1d, 16),
        2 => (-32, 32, 0x1d, 16),
        3 => (-32, -32, 0x1d, 16),
        4 => (32, 32, 0x1d, 32),
        5 => (-32, -8, 0x1d, 8),
        6 => (0, 0, 3, 8),
        7 => (32, -8, 0x1d, 8),
        8 => (0, 0, 3, 8),
        9 => (0, 24, 3, 8),
        10 => (0, 48, 0x1d, 8),
        11 => (-24, 24, 3, 8),
        12 => (24, 24, 3, 8),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
