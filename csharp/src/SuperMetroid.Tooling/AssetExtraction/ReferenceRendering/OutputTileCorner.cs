namespace SuperMetroid.AssetExtraction.ReferenceRendering;

/// <summary>A tile's column and row within an expanded two-by-two output block.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct OutputTileCorner(int X, int Y);
