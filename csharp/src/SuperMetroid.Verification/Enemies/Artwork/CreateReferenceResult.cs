using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The first and second of one create reference result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct CreateReferenceResult(SnesCgram First, RidleyEnemyState Second);
