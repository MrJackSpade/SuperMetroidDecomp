using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// One way a Ceres steam vent blows (#2557): the segments its dormant wait, its rest and its puff play, and its puff's
/// pictures in order. Every way plays the same vent animation; only these differ.
/// </summary>
/// <param name="Dormant">The segment its dormant wait before the first puff plays.</param>
/// <param name="Resting">The segment its rest between puffs plays.</param>
/// <param name="Puffing">The segment its puff plays.</param>
/// <param name="Puff">The puff's pictures, in the order they show.</param>
internal sealed record CeresSteamWay(CeresSteamSegment Dormant, CeresSteamSegment Resting, CeresSteamSegment Puffing, IReadOnlyList<EnemyFrameId> Puff);
