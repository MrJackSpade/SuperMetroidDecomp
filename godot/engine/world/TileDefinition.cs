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
    /// <summary>The pictures it steps through, for a tile that moves as it is looked at; empty for one picture that stays.</summary>
    public IReadOnlyList<string> Frames { get; init; } = [];

    /// <summary>
    /// The picture it shows in place of its own while a revealing ability sees it, empty for nothing; null for a tile
    /// that looks the same seen or not.
    /// </summary>
    public string? SeenPicture { get; init; }

    /// <summary>How many updates each of those pictures shows for.</summary>
    public int FrameUpdates { get; init; } = 1;

    /// <summary>The picture it shows <paramref name="updates"/> updates into its room's stay.</summary>
    public string PictureAt(long updates) => Frames.Count > 0 ? Frames[(int)(updates / FrameUpdates % Frames.Count)] : Picture;

    /// <summary>Open space, drawn as nothing.</summary>
    public static TileDefinition Empty { get; } = new(TileKind.Empty, null, "", 1, 1, "");
}
