using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The geometry and collision of both mirrored, three-component eye doors' draws; what each frame shows is its installed
/// picture (#1557). The eye and its parts' objects control timing, attacks, hits, persistence and the conversion to a blue door.
/// </summary>
internal static class EyeDoorObjectDrawDefinitions
{
    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        RoomObjectDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: true));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        RoomObjectDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: false));

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
    /// How the cells of each eye-door draw collide, in authored run order: the four-block clear, the two-block eye and the
    /// one-block middle and bottom. Of the eye's closed, opening, vulnerable, hit-flash and open-but-solid frames, only
    /// the vulnerable frame is shootable.
    /// </summary>
    internal static IReadOnlyDictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =

        new Dictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: true)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 0, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 1, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 2, Left: false)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 3, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Eye, 4, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 0, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 1, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Middle, 2, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 0, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 1, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorDraws.Component.Bottom, 2, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
        };
}
