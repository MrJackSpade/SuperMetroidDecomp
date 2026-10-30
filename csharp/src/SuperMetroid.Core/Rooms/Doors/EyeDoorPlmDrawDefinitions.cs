using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical draws for both mirrored, three-component eye doors. The typed programs control timing, attacks, hits,
/// persistence and the conversion to a blue door.
/// </summary>
internal static class EyeDoorPlmDrawDefinitions
{
    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static PlmFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        PlmDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: true));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static PlmFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        PlmDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: false));

    /// <summary>The number of animation frames of each component.</summary>
    internal static IReadOnlyDictionary<EyeDoorDraws.Component, int> FrameCounts { get; } =
        new Dictionary<EyeDoorDraws.Component, int>
        {
            [EyeDoorDraws.Component.Eye] = 5,
            [EyeDoorDraws.Component.Middle] = 3,
            [EyeDoorDraws.Component.Bottom] = 3,
            [EyeDoorDraws.Component.Clear] = 1,
        };

    /// <summary>
    /// The cells of each eye-door draw, in native run order: the four-block clear, the two-block eye and the one-block
    /// middle and bottom. The left-facing door mirrors horizontally, and lower cells mirror vertically. The eye shows
    /// its closed, opening, vulnerable, hit-flash and open-but-solid frames; only the vulnerable frame is shootable.
    /// </summary>
    internal static IReadOnlyDictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomTerrainBlock>> Cells { get; } =
        new Dictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomTerrainBlock>>
        {
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: true)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: true)] =
            [
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: true)] =
            [
                RoomTerrainBlock.Of(0xcb, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcb, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: true)] =
            [
                RoomTerrainBlock.Of(0xca, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0xca, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: true)] =
            [
                RoomTerrainBlock.Of(0xcd, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcd, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: true)] =
            [
                RoomTerrainBlock.Of(0xca, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xca, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: true)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: true)] =
            [
                RoomTerrainBlock.Of(0xab, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: true)] =
            [
                RoomTerrainBlock.Of(0xac, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: true)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: true)] =
            [
                RoomTerrainBlock.Of(0xab, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: true)] =
            [
                RoomTerrainBlock.Of(0xac, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: false)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: false)] =
            [
                RoomTerrainBlock.Of(0xcc, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcc, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: false)] =
            [
                RoomTerrainBlock.Of(0xcb, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcb, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: false)] =
            [
                RoomTerrainBlock.Of(0xca, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0xca, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: false)] =
            [
                RoomTerrainBlock.Of(0xcd, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xcd, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: false)] =
            [
                RoomTerrainBlock.Of(0xca, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0xca, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: false)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: false)] =
            [
                RoomTerrainBlock.Of(0xab, BlockMirroring.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: false)] =
            [
                RoomTerrainBlock.Of(0xac, BlockMirroring.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: false)] =
            [
                RoomTerrainBlock.Of(0xaa, BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: false)] =
            [
                RoomTerrainBlock.Of(0xab, BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: false)] =
            [
                RoomTerrainBlock.Of(0xac, BlockMirroring.Vertical, RoomCollisionType.SpikeBlock),
            ],
        };
}
