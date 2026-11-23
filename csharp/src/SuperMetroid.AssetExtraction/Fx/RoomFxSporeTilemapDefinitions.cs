namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Constant stock spore attributes in all 1056 words at $8A:98C0..A0FF.</summary>
public static partial class RoomFxSporeTilemapDefinitions
{
    /// <summary>Every cell of the spore page uses palette 6, clear priority and no flips: attribute word $1800.</summary>
    public static ushort Attributes(int index)
    {
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        return 6 << 10;
    }
}
