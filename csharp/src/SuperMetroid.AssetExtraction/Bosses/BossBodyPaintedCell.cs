using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One cell of a boss body picture: where in the picture it lies, its tile, and the character the tile shows.</summary>
/// <param name="Column">The cell's column in the picture.</param>
/// <param name="Row">The cell's row in the picture.</param>
/// <param name="Tile">The tile written there.</param>
/// <param name="Character">The bytes of the character the tile shows.</param>
internal sealed record BossBodyPaintedCell(int Column, int Row, BackgroundTile Tile, ReadOnlyMemory<byte> Character);
