namespace SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="ResidentDoorClosingDefinitions"/> members production does not use.</summary>
internal static class ResidentDoorClosingDefinitionsAccess
{
    /// <summary>Lists resident door-closing records across their orientation variants.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <summary>Enumerates the three closers that keep a closing start (the escape gate and both eye doors) in original header order without cached records.</summary>
        internal static IEnumerable<ResidentDoorClosingDefinition> All
        {
            get
            {
                yield return new(PlmHeaderId.MotherBrainEscapeRoomGate, ResidentDoorClosings.Resolve(PlmHeaderId.MotherBrainEscapeRoomGate));
                yield return new(PlmHeaderId.EyeDoorFacingRight, ResidentDoorClosings.Resolve(PlmHeaderId.EyeDoorFacingRight));
                yield return new(PlmHeaderId.EyeDoorFacingLeft, ResidentDoorClosings.Resolve(PlmHeaderId.EyeDoorFacingLeft));
            }
        }
    }
}
