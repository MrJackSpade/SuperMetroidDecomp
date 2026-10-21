namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Exact discrete Phantoon figure-eight movement, not a sampled analytic curve.</summary>
public static class PhantoonPathDefinitions
{
    /// <summary>$A7:E3D2..E7FD, PhantoonMovementData: 534 signed X/Y unit steps.</summary>
    public const int Length = 534;

    /// <summary>Lossless keypad notation for native unit steps with screen Y increasing downward; source rows are formatting only.</summary>
    /// <remarks>8/6/2/4 mean north/east/south/west; 7/9/1/3 are diagonals. The movement cursor crosses string-row boundaries without resetting.</remarks>
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
    public static Offset16<RoomPixels> Step(int index)
    {
        if ((uint)index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        return Directions[index] switch
        {
            '1' => new Offset16<RoomPixels>(-1, 1), '2' => new Offset16<RoomPixels>(0, 1), '3' => new Offset16<RoomPixels>(1, 1),
            '4' => new Offset16<RoomPixels>(-1, 0), '6' => new Offset16<RoomPixels>(1, 0),
            '7' => new Offset16<RoomPixels>(-1, -1), '8' => new Offset16<RoomPixels>(0, -1), '9' => new Offset16<RoomPixels>(1, -1),
            _ => throw new InvalidOperationException("Invalid compiled Phantoon path direction."),
        };
    }
}
