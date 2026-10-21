using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>The first, second and third of one load result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct LoadResult(RoomEnemySystem First, SnesVram Second, SnesCgram Third);
