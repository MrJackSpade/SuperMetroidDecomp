namespace Engine.World;

/// <summary>A named rectangle of tiles a layout can put in place of what is there, and take back.</summary>
/// <param name="Column">The leftmost tile column it covers.</param>
/// <param name="Row">The topmost tile row it covers.</param>
/// <param name="Tiles">Its tiles, by row then column.</param>
public sealed record TerrainPatch(int Column, int Row, TileDefinition[][] Tiles);
