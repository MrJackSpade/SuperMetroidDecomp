namespace SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="ResidentDoorClosingDefinitions"/> members production does not use.</summary>
internal static class ResidentDoorClosingDefinitionsAccess
{
    /// <summary>Lists resident door-closing records across their orientation variants.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <summary>Enumerates the two closers that keep a closing start (both eye doors) in original header order without cached records.</summary>
        internal static IEnumerable<ResidentDoorClosingDefinition> All
        {
            get
            {
                yield return new(RoomObjectHeaderId.EyeDoorFacingRight, ResidentDoorClosings.Resolve(RoomObjectHeaderId.EyeDoorFacingRight));
                yield return new(RoomObjectHeaderId.EyeDoorFacingLeft, ResidentDoorClosings.Resolve(RoomObjectHeaderId.EyeDoorFacingLeft));
            }
        }
    }
}
