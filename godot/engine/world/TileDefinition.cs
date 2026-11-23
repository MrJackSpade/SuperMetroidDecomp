namespace Engine.World;

/// <summary>One kind of tile a layout can place: what a letter of the layout's grid stands for.</summary>
/// <param name="Kind">What the tile is.</param>
/// <param name="Slope">Its shape, for a slope.</param>
/// <param name="Block">The data file of its block type, for a block.</param>
/// <param name="BlockWidth">How many tiles wide the block is, for a block.</param>
/// <param name="BlockHeight">How many tiles high the block is, for a block.</param>
/// <param name="Picture">The picture drawn for it, as a path within the assets tree; empty for none.</param>
public sealed record TileDefinition(TileKind Kind, SlopeShape? Slope, string Block, int BlockWidth, int BlockHeight, string Picture)
{
    /// <summary>Open space, drawn as nothing.</summary>
    public static TileDefinition Empty { get; } = new(TileKind.Empty, null, "", 1, 1, "");
}
