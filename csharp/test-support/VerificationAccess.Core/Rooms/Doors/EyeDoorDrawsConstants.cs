using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values and helpers of <see cref="EyeDoorDraws"/> that only verification reads.</summary>
internal static class EyeDoorDrawsConstants
{
    /// <summary>Maps the mirrored opening clear to its shared editable visual source.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear ? EyeDoorObjectDrawDefinitions.SharedClear : request;
}
