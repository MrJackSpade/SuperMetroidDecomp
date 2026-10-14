using SuperMetroid.Core;
using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="SamusLiquidPhysicsState"/> members production does not use.</summary>
internal static class SamusLiquidPhysicsStateAccess
{
    /// <summary>Initializes the remembered air, water, or lava medium from room FX and Samus's bottom boundary.</summary>
    extension(SamusLiquidPhysicsState self)
    {
        /// <summary>
        /// Reproduces <c>SetLiquidPhysicsType</c> at <c>$90:8E0F</c>. Unlike movement-table
        /// selection, this room-load helper dispatches on FX type and does not exempt Gravity
        /// Suit; animation later suppresses the suit's delay while retaining the medium word.
        /// </summary>
        internal void InitializeRememberedMedium(SamusState samus)
        {
            Ensure.NotNull(samus);
            ushort bottom = samus.Kinematics.BottomBoundary;
            self.LiquidPhysicsType = self.FxType switch
            {
                RoomFxType.Lava or RoomFxType.Acid
                    when ((bool)(SamusLiquidPhysicsState.IsBelowSurface((ushort)(self.LavaAcidYPosition), (ushort)(bottom)))) => SamusLiquidMedium.LavaOrAcid,
                RoomFxType.Water or RoomFxType.TourianEntranceStatue when ((bool)(self.WaterAffectsBoundary((ushort)(bottom)))) => SamusLiquidMedium.Water,
                _ => SamusLiquidMedium.Air,
            };
        }
    }
}
