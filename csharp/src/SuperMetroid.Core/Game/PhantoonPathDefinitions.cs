namespace SuperMetroid.Core.Game;

/// <summary>Exact discrete Phantoon figure-eight movement, not a sampled analytic curve.</summary>
public static class PhantoonPathDefinitions
{
    /// <summary>$A7:E3D2..E7FD, PhantoonMovementData: 534 signed X/Y unit steps.</summary>
    public const int Length = 534;

    // Compact lossless direction notation: numeric keypad with screen Y increasing
    // downwards. 8=N, 6=E, 2=S, 4=W; 7/9/1/3 are NW/NE/SW/SE.
    // Rows are formatting only; the native cursor advances across row boundaries.
    private const string Directions =
        "8686898686886868688686868686886868686868668686868668668668668666" +
        "6866666666663666626662662663636266262662626263333262626226226226" +
        "2226222222224222422124224242421241142424244242442414144244244424" +
        "4441444444444484444844844844847484847748484877848484848784848488" +
        "4848848484884848784848848484878484848487848484848474848474844844" +
        "8448444484444444444144442444244244141424424244242424241112424242" +
        "2421224222422222222622262262262262626233263626262662626626363662" +
        "6626662666636666666666866668668668668668686868696868686868868686" +
        "8686886868986868986868";

    /// <summary>Reads the authored signed displacement at the native movement cursor.</summary>
    public static (sbyte X, sbyte Y) Step(int index)
    {
        if ((uint)index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        return Directions[index] switch
        {
            '1' => (-1, 1), '2' => (0, 1), '3' => (1, 1),
            '4' => (-1, 0), '6' => (1, 0),
            '7' => (-1, -1), '8' => (0, -1), '9' => (1, -1),
            _ => throw new InvalidOperationException("Invalid compiled Phantoon path direction."),
        };
    }
}
