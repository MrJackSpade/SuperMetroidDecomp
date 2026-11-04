using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One spawned-object sheet of a room graphics set (#1392, #1513).</summary>
/// <param name="Owner">Whose spawned objects it pictures, which names it to the game.</param>
/// <param name="ProjectileCharacters">The character index its projectiles' parts count from.</param>
/// <param name="ObjectCharacters">The character index its sprite objects' and encounter frames' parts count from.</param>
/// <param name="Palette">The palette row its frames are coloured in: its spawner's, or the common row at character zero.</param>
/// <param name="Spawners">The enemies whose own-graphics spawns, or encounter frames, it holds, and whose shades colour it.</param>
/// <param name="SharedSpawners">
/// On the shared sheet, the enemies drawn under the set whose spawns draw from the room's graphics, whose objects it holds.
/// </param>
/// <param name="Anywhere">Whether it also holds the objects any room spawns.</param>
/// <param name="AlsoOwnedBy">
/// The other owners the game names this sheet by: on the shared sheet, each enemy drawn under the set that has no
/// spawned objects of its own, whose objects are those any room spawns.
/// </param>
internal sealed record SpawnedObjectSheetPlan(SpawnedObjectOwner Owner, int ProjectileCharacters, int ObjectCharacters, int Palette,
    List<EnemyDefinitionId> Spawners, List<EnemyDefinitionId> SharedSpawners, bool Anywhere, List<SpawnedObjectOwner> AlsoOwnedBy);
