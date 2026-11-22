using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian entrance statue's decorations: what the scene's first placement spawns and the room point each stands at,
/// tied there as the statue descends.
/// </summary>
internal static partial class TourianStatueDecorationDefinitions
{
    /// <summary>
    /// The decorations in the order they spawn, which gives them their authored draw order: the decoration on the statue's
    /// base, Ridley's statue, then Phantoon's.
    /// </summary>
    internal static IReadOnlyList<TourianStatueDecoration> SpawnOrder { get; } =
    [
        new(RoomEnemyProjectileKind.TourianStatueBaseDecoration, TourianStatueProjectileSegment.BaseDecoration, new(120, 184)),
        new(RoomEnemyProjectileKind.TourianStatueRidley, TourianStatueProjectileSegment.Ridley, new(142, 85)),
        new(RoomEnemyProjectileKind.TourianStatuePhantoon, TourianStatueProjectileSegment.Phantoon, new(132, 136)),
    ];
}
