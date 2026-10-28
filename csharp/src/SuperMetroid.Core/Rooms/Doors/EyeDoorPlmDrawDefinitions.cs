using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical draws for both mirrored, three-component eye doors. The typed programs control timing, attacks, hits,
/// persistence and the conversion to a blue door.
/// </summary>
internal static class EyeDoorPlmDrawDefinitions
{
    /// <summary>The native run header's vertical direction bit, set for the eye and the clear.</summary>
    internal const int VerticalRunBit = 0x8000;

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
    internal static IReadOnlyDictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomLevelWord>> Cells { get; } =
        new Dictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomLevelWord>>
        {
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: true)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: true)] =
            [
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: true)] =
            [
                RoomLevelWord.Create(0xcb, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcb, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: true)] =
            [
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: true)] =
            [
                RoomLevelWord.Create(0xcd, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcd, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: true)] =
            [
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: true)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: true)] =
            [
                RoomLevelWord.Create(0xab, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: true)] =
            [
                RoomLevelWord.Create(0xac, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: true)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: true)] =
            [
                RoomLevelWord.Create(0xab, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: true)] =
            [
                RoomLevelWord.Create(0xac, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: false)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: false)] =
            [
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcc, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: false)] =
            [
                RoomLevelWord.Create(0xcb, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcb, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: false)] =
            [
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: false)] =
            [
                RoomLevelWord.Create(0xcd, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcd, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: false)] =
            [
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xca, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: false)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: false)] =
            [
                RoomLevelWord.Create(0xab, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: false)] =
            [
                RoomLevelWord.Create(0xac, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: false)] =
            [
                RoomLevelWord.Create(0xaa, LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: false)] =
            [
                RoomLevelWord.Create(0xab, LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: false)] =
            [
                RoomLevelWord.Create(0xac, LevelBlockFlipFlags.Vertical, RoomCollisionType.SpikeBlock),
            ],
        };
}
