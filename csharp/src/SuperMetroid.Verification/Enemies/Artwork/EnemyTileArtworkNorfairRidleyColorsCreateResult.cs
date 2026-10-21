using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The first, second, third and fourth of one enemy tile artwork norfair ridley colors create result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct EnemyTileArtworkNorfairRidleyColorsCreateResult(RoomEnemySystem First, SnesCgram Second, RoomEnemySlot Third, RidleyEnemyState Fourth);
