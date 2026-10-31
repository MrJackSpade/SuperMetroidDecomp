using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The tile runs the cartridge's seven Ceres floor animation frames write into a Ceres room's floor cells (#1467): the
/// import pictures each frame from them; gameplay draws the frame's picture by name. A run replaces only the tile each
/// floor cell shows, never the tiles' pixels.
/// </summary>
internal static class CeresFloorDrawingDefinitions
{
    /// <summary>The wing cell's tile that draws nothing.</summary>
    internal const int TransparentWingTile = 255;

    /// <summary>
    /// Every frame's runs in drawing order: the elevator platform's four-cell strip at floor (14, 12), the capsule's two
    /// rows at (4, 10), and the six rows of Ridley's wing tracing its drawn silhouette. The light and dark platform strips
    /// are a left cap, two interior cells and a right cap; the right caps sit below the light left cap in the floor's
    /// sixteen-column character atlas. Wing rows reuse contiguous atlas runs, and the second wing frame shares part of
    /// the first's middle rows. Every run origin and extent is authored artwork placement.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresFloorAnimationFrame, IReadOnlyList<CeresFloorRunDefinition>> Frames =
        new Dictionary<CeresFloorAnimationFrame, IReadOnlyList<CeresFloorRunDefinition>>
        {
            [CeresFloorAnimationFrame.ElevatorLight] = [new(14, 12, [new(104, 1), new(105, 2, 0), new(120, 1)])],
            [CeresFloorAnimationFrame.ElevatorDark] = [new(14, 12, [new(141, 1), new(142, 2, 0), new(121, 1)])],
            [CeresFloorAnimationFrame.BabyFrame0] = [new(4, 10, [new(89, 2)]), new(4, 11, [new(105, 2)])],
            [CeresFloorAnimationFrame.BabyFrame1] = [new(4, 10, [new(138, 2)]), new(4, 11, [new(140, 2)])],
            [CeresFloorAnimationFrame.BabyFrame2] = [new(4, 10, [new(142, 2)]), new(4, 11, [new(157, 2)])],
            [CeresFloorAnimationFrame.WingFrame0] =
            [
                new(11, 0, [new(0, 4)]),
                new(0, 1, [new(4, 6), new(TransparentWingTile, 2, 0), new(10, 6)]),
                new(0, 2, [new(16, 13), new(168, 1)]),
                new(1, 3, [new(33, 12)]),
                new(1, 4, [new(TransparentWingTile, 2, 0), new(29, 3), new(48, 5), new(TransparentWingTile, 5, 0)]),
                new(0, 5, [new(TransparentWingTile, 4, 0), new(46, 2), new(64, 5), new(TransparentWingTile, 5, 0)]),
            ],
            [CeresFloorAnimationFrame.WingFrame1] =
            [
                new(11, 0, [new(TransparentWingTile, 4, 0)]),
                new(0, 1, [new(TransparentWingTile, 14, 0)]),
                new(0, 2, [new(TransparentWingTile, 6, 0), new(32, 1), new(23, 1), new(170, 1), new(TransparentWingTile, 5, 0)]),
                new(1, 3, [new(TransparentWingTile, 5, 0), new(38, 3), new(TransparentWingTile, 4, 0)]),
                new(1, 4, [new(145, 5), new(48, 3), new(150, 5), new(152, 1), new(156, 1)]),
                new(0, 5, [new(144, 1), new(159, 5), new(64, 3), new(164, 4), new(125, 1), new(131, 1), new(45, 1)]),
            ],
        };
}
