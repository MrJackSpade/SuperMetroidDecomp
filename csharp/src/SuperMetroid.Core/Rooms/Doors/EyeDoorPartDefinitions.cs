using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The part of an eye door each eye-door room object is, and the way that door faces.</summary>
internal static class EyeDoorPartDefinitions
{
    /// <summary>Each eye-door room object's part and facing.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectHeaderId, EyeDoorPart> Parts = new Dictionary<RoomObjectHeaderId, EyeDoorPart>
    {
        [RoomObjectHeaderId.EyeDoorEyeFacingRight] = new(EyeDoorComponent.Eye, EyeDoorOrientation.Right),
        [RoomObjectHeaderId.EyeDoorFacingRight] = new(EyeDoorComponent.Door, EyeDoorOrientation.Right),
        [RoomObjectHeaderId.EyeDoorBottomFacingRight] = new(EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
        [RoomObjectHeaderId.EyeDoorEyeFacingLeft] = new(EyeDoorComponent.Eye, EyeDoorOrientation.Left),
        [RoomObjectHeaderId.EyeDoorFacingLeft] = new(EyeDoorComponent.Door, EyeDoorOrientation.Left),
        [RoomObjectHeaderId.EyeDoorBottomFacingLeft] = new(EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
    };
}
