using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The first and second of one create enemy result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct CreateEnemyResult(RoomEnemySystem First, SnesCgram Second);
