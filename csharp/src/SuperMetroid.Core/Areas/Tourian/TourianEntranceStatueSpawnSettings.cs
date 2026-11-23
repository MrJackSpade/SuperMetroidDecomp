using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What one of the Tourian entrance statue scene's three placements does as it initializes (#1472, #2014).</summary>
/// <param name="Role">Whether the placement spawns the scene's decorations or nothing.</param>
internal sealed partial record TourianEntranceStatueSpawnSettings(TourianEntranceStatueRole Role) : EnemySpawnSettings;
