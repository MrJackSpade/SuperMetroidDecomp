using Engine.Actors;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's reading of Samus's body for the blocks she is in. It asks about her feet; about her middle where
/// that lies in another row of tiles than her feet; and about her head only where that lies in another row than her
/// feet and the two rows' numbers, combined bit by bit (a bit set where exactly one of them has it), do not give her
/// middle's row. So her head is passed over in rows the combination happens to name, though it lies in a row of its
/// own: with her head in the fourth row, her middle in the sixth and her feet in the seventh, thorns at her head do
/// not hurt her, and three pixels higher, her middle in the fifth, they do. The rows are counted as the cartridge
/// counts them, from heights kept in sixteen bits.
/// </summary>
public sealed class CartridgeBodyReadings : IBodyReadings
{
    /// <inheritdoc/>
    public IEnumerable<int> HeightsStoodAt(int bottom, int middle, int top, int tileSize)
    {
        int bottomRow = Row(bottom, tileSize), middleRow = Row(middle, tileSize), topRow = Row(top, tileSize);
        yield return bottom;
        if (middleRow != bottomRow)
            yield return middle;
        if (topRow != bottomRow && (topRow ^ bottomRow) != middleRow)
            yield return top;
    }

    /// <summary>The row of tiles a height lies in, the height kept in sixteen bits.</summary>
    private static int Row(int height, int tileSize) => (height & 0xFFFF) / tileSize;
}
