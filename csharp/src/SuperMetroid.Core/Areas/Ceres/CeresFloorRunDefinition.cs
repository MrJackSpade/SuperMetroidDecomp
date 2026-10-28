namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One row a Ceres floor animation frame draws: where it starts on the floor and the atlas runs it shows.</summary>
/// <param name="Column">The floor column of the row's first cell.</param>
/// <param name="Row">The floor row the run is drawn on.</param>
/// <param name="Tiles">The character runs drawn left to right; their lengths add up to the row's width.</param>
internal readonly record struct CeresFloorRunDefinition(int Column, int Row, IReadOnlyList<CeresFloorAtlasRun> Tiles);
