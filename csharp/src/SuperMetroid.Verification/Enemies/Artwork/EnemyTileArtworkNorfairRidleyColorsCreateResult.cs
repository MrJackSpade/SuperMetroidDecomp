using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The initialized production Norfair Ridley encounter a colours check drives.</summary>
/// <param name="Enemies">The room enemies holding the encounter.</param>
/// <param name="Slot">Ridley's slot.</param>
/// <param name="State">Ridley's encounter state, which holds the shades his AI laid.</param>
internal readonly record struct EnemyTileArtworkNorfairRidleyColorsCreateResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, RidleyEnemyState State);
