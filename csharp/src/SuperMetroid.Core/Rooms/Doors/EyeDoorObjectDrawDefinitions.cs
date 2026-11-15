using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The geometry and collision of both mirrored, three-component eye doors' draws; what each frame shows is its installed
/// picture (#1557). The eye and its parts' objects control timing, attacks, hits, persistence and the conversion to a blue door.
/// </summary>
internal static class EyeDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the blocks of each eye-door draw collide, from its first block down its column or along its row: the four-block clear, the two-block eye and the
    /// one-block middle and bottom. Of the eye's closed, half-open, open, flinching and open-but-solid looks, only
    /// the open eye is shootable.
    /// </summary>
    internal static IReadOnlyDictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =

        new Dictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, Left: true)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, Left: true)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, Left: true)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, Left: false)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, Left: false)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, Left: false)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
        };

    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, Left: true));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, Left: false));
}
