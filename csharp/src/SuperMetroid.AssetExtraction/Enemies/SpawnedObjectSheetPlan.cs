using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One spawned-object sheet of a room graphics set (#1392).</summary>
/// <param name="TileBase">The packed tile base its objects' parts draw at.</param>
/// <param name="Palette">The palette row its frames are coloured in: the row of the spawner drawing at that base.</param>
/// <param name="Spawners">The enemies whose spawned objects it holds.</param>
/// <param name="Anywhere">Whether it also holds the objects any room spawns.</param>
internal sealed record SpawnedObjectSheetPlan(int TileBase, int Palette, List<EnemyDefinitionId> Spawners, bool Anywhere);
