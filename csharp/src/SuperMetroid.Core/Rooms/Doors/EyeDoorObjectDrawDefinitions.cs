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
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SolidCap,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.ShootableEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, EyeDoorOrientation.Left)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SolidCap,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeClosed, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeHalfOpen, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpen, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.ShootableEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeFlinching, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.EyeOpenSolid, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SolidEye,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleResting, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwelling, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.MiddleSwollen, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomResting, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwelling, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
            [new EyeDoorDraws.Draw(EyeDoorLook.BottomSwollen, EyeDoorOrientation.Right)] = DoorCapCollisionDefinitions.SpikedPart,
        };

    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right));
}
