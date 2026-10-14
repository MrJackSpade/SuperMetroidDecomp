using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="ResidentDoorClosingDefinitions"/> members production does not use.</summary>
internal static class ResidentDoorClosingDefinitionsAccess
{
    /// <summary>Lists resident door-closing records across their orientation variants.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <summary>Enumerates the twenty supported identities in original header order without cached records.</summary>
        internal static IEnumerable<ResidentDoorClosingDefinition> All
        {
            get
            {
                yield return new(PlmHeaderId.BombTorizoGreyDoor, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.BombTorizoGreyDoor));
                yield return new(PlmHeaderId.GreyDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingLeft));
                yield return new(PlmHeaderId.GreyDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingRight));
                yield return new(PlmHeaderId.GreyDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingUp));
                yield return new(PlmHeaderId.GreyDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingDown));
                yield return new(PlmHeaderId.YellowDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingLeft));
                yield return new(PlmHeaderId.YellowDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingRight));
                yield return new(PlmHeaderId.YellowDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingUp));
                yield return new(PlmHeaderId.YellowDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingDown));
                yield return new(PlmHeaderId.GreenDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingLeft));
                yield return new(PlmHeaderId.GreenDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingRight));
                yield return new(PlmHeaderId.GreenDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingUp));
                yield return new(PlmHeaderId.GreenDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingDown));
                yield return new(PlmHeaderId.RedDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingLeft));
                yield return new(PlmHeaderId.RedDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingRight));
                yield return new(PlmHeaderId.RedDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingUp));
                yield return new(PlmHeaderId.RedDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingDown));
                yield return new(PlmHeaderId.MotherBrainEscapeRoomGate, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.MotherBrainEscapeRoomGate));
                yield return new(PlmHeaderId.EyeDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.EyeDoorFacingRight));
                yield return new(PlmHeaderId.EyeDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.EyeDoorFacingLeft));
            }
        }
    }
}
