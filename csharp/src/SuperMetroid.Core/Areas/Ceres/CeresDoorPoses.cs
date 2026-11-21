using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres door's slide facing one way (#2557): its pictures from open to closed, which its animations are written over.</summary>
/// <param name="Open">Open.</param>
/// <param name="ThreeQuartersOpen">Three quarters open.</param>
/// <param name="HalfOpen">Half open.</param>
/// <param name="QuarterOpen">A quarter open.</param>
/// <param name="Closed">Closed.</param>
internal sealed record CeresDoorPoses(F Open, F ThreeQuartersOpen, F HalfOpen, F QuarterOpen, F Closed);
