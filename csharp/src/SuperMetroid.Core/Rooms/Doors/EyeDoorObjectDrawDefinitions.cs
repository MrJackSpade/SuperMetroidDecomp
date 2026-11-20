using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.DefinitionRun;

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
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, EyeDoorOrientation.Left)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, EyeDoorOrientation.Right)] =
            [
                RoomCollisionType.SpikeBlock,
            ],
        };

    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right));
}
