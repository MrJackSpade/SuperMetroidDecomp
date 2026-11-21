namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresRidleyRoomDoorEntity"/> members production does not use.</summary>
internal static class CeresRidleyRoomDoorEntityAccess
{
    /// <summary>Exposes <see cref="CeresRidleyRoomDoorEntity"/> state that only verification reads.</summary>
    extension(CeresRidleyRoomDoorEntity self)
    {
        /// <summary>
        /// How the door shows (#2573): its overlay picture from its animation's <see cref="CeresDoorCueKind.ShowOverlay"/>
        /// until its <see cref="CeresDoorCueKind.MakeVisibleAndShowActor"/>, otherwise its actor frames.
        /// </summary>
        public CeresRidleyRoomDoorLook Look => self.Place.RequireSpeciesState<CeresDoorAnimationState>().Look;
    }
}
