using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The part of an eye door each eye-door room object is, and the way that door faces.</summary>
internal static class EyeDoorPartDefinitions
{
    /// <summary>Each eye-door room object's part and facing.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectKind, EyeDoorPart> Parts = new Dictionary<RoomObjectKind, EyeDoorPart>
    {
        [RoomObjectKind.EyeDoorEyeFacingRight] = new(EyeDoorComponent.Eye, EyeDoorOrientation.Right),
        [RoomObjectKind.EyeDoorFacingRight] = new(EyeDoorComponent.Door, EyeDoorOrientation.Right),
        [RoomObjectKind.EyeDoorBottomFacingRight] = new(EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
        [RoomObjectKind.EyeDoorEyeFacingLeft] = new(EyeDoorComponent.Eye, EyeDoorOrientation.Left),
        [RoomObjectKind.EyeDoorFacingLeft] = new(EyeDoorComponent.Door, EyeDoorOrientation.Left),
        [RoomObjectKind.EyeDoorBottomFacingLeft] = new(EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
    };
}
