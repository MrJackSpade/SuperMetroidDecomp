namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess2
{
    /// <summary>Exposes <see cref="CeresDestructionCinematicState"/> state that only verification reads.</summary>
    extension(CeresDestructionCinematicState self)
    {
        /// <summary>
        /// The picture the turning floor holds (#1467), or null while it holds none: before the station is set up, and once the
        /// final explosion with Samus still aboard has cleared the whole floor (#1513). The Zebes reveal holds the gunship's
        /// rear view for the approach that follows it.
        /// </summary>
        /// <exception cref="InvalidOperationException">The view is undefined.</exception>
        internal CeresFloorShot? FloorShot => self.View switch
        {
            CeresDestructionView.BeforeStation or CeresDestructionView.StationCleared => null,
            CeresDestructionView.Station => CeresFloorShot.Station,
            CeresDestructionView.GunshipLeavingExplosion => CeresFloorShot.Explosion,
            CeresDestructionView.ZebesReveal or CeresDestructionView.ZebesApproach or CeresDestructionView.ZebesClose => CeresFloorShot.ZebesApproach,
            _ => throw new InvalidOperationException($"Undefined {nameof(CeresDestructionView)} {self.View}."),
        };
    }
}
