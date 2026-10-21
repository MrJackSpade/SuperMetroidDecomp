namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>The tile and colour hashes of the extracted Ceres door visuals.</summary>
/// <param name="TileHash">The tile hash.</param>
/// <param name="ColorHash">The color hash.</param>
public readonly record struct CeresDoorVisualHashes(string TileHash, string ColorHash);
