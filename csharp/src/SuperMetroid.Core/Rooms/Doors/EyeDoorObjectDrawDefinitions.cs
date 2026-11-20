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
    /// How each look's blocks collide, whichever way the door faces: the facing mirrors only the artwork, so each look's
    /// collisions are written once.
    /// </summary>
    private static readonly Dictionary<EyeDoorLook, IReadOnlyList<RoomCollisionType>> CollisionsByLook = new()
    {
        [EyeDoorLook.Clear] = DoorCapCollisionDefinitions.SolidCap,
        [EyeDoorLook.EyeClosed] = DoorCapCollisionDefinitions.SolidEye,
        [EyeDoorLook.EyeHalfOpen] = DoorCapCollisionDefinitions.SolidEye,
        [EyeDoorLook.EyeOpen] = DoorCapCollisionDefinitions.ShootableEye,
        [EyeDoorLook.EyeFlinching] = DoorCapCollisionDefinitions.SolidEye,
        [EyeDoorLook.EyeOpenSolid] = DoorCapCollisionDefinitions.SolidEye,
        [EyeDoorLook.MiddleResting] = DoorCapCollisionDefinitions.SpikedPart,
        [EyeDoorLook.MiddleSwelling] = DoorCapCollisionDefinitions.SpikedPart,
        [EyeDoorLook.MiddleSwollen] = DoorCapCollisionDefinitions.SpikedPart,
        [EyeDoorLook.BottomResting] = DoorCapCollisionDefinitions.SpikedPart,
        [EyeDoorLook.BottomSwelling] = DoorCapCollisionDefinitions.SpikedPart,
        [EyeDoorLook.BottomSwollen] = DoorCapCollisionDefinitions.SpikedPart,
    };

    /// <summary>
    /// How the blocks of each eye-door draw collide, from its first block down its column or along its row: the four-block clear, the two-block eye and the
    /// one-block middle and bottom. Of the eye's closed, half-open, open, flinching and open-but-solid looks, only
    /// the open eye is shootable.
    /// </summary>
    internal static IReadOnlyDictionary<EyeDoorDraws.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        ((EyeDoorOrientation[])[EyeDoorOrientation.Left, EyeDoorOrientation.Right])
            .SelectMany(orientation => CollisionsByLook.Select(look => KeyValuePair.Create(new EyeDoorDraws.Draw(look.Key, orientation), look.Value)))
            .ToDictionary();

    /// <summary>The left side's opening clear, which shares the right side's clearing artwork.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> MirroredOpeningClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left));

    /// <summary>The right side's clearing frame, whose artwork both clears show.</summary>
    internal static RoomObjectFrameDraw<EyeDoorDraws.Draw> SharedClear { get; } =
        EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right));
}
