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
    internal const byte TransparentWingTile = 0xff;

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
            [CeresFloorAnimationFrame.ElevatorLight] = [new(14, 12, [new(0x68, 1), new(0x69, 2, 0), new(0x78, 1)])],
            [CeresFloorAnimationFrame.ElevatorDark] = [new(14, 12, [new(0x8d, 1), new(0x8e, 2, 0), new(0x79, 1)])],
            [CeresFloorAnimationFrame.BabyFrame0] = [new(4, 10, [new(0x59, 2)]), new(4, 11, [new(0x69, 2)])],
            [CeresFloorAnimationFrame.BabyFrame1] = [new(4, 10, [new(0x8a, 2)]), new(4, 11, [new(0x8c, 2)])],
            [CeresFloorAnimationFrame.BabyFrame2] = [new(4, 10, [new(0x8e, 2)]), new(4, 11, [new(0x9d, 2)])],
            [CeresFloorAnimationFrame.WingFrame0] =
            [
                new(11, 0, [new(0x00, 4)]),
                new(0, 1, [new(0x04, 6), new(TransparentWingTile, 2, 0), new(0x0a, 6)]),
                new(0, 2, [new(0x10, 13), new(0xa8, 1)]),
                new(1, 3, [new(0x21, 12)]),
                new(1, 4, [new(TransparentWingTile, 2, 0), new(0x1d, 3), new(0x30, 5), new(TransparentWingTile, 5, 0)]),
                new(0, 5, [new(TransparentWingTile, 4, 0), new(0x2e, 2), new(0x40, 5), new(TransparentWingTile, 5, 0)]),
            ],
            [CeresFloorAnimationFrame.WingFrame1] =
            [
                new(11, 0, [new(TransparentWingTile, 4, 0)]),
                new(0, 1, [new(TransparentWingTile, 14, 0)]),
                new(0, 2, [new(TransparentWingTile, 6, 0), new(0x20, 1), new(0x17, 1), new(0xaa, 1), new(TransparentWingTile, 5, 0)]),
                new(1, 3, [new(TransparentWingTile, 5, 0), new(0x26, 3), new(TransparentWingTile, 4, 0)]),
                new(1, 4, [new(0x91, 5), new(0x30, 3), new(0x96, 5), new(0x98, 1), new(0x9c, 1)]),
                new(0, 5, [new(0x90, 1), new(0x9f, 5), new(0x40, 3), new(0xa4, 4), new(0x7d, 1), new(0x83, 1), new(0x2d, 1)]),
            ],
        };
}
