using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Verification access to <see cref="SamusAerialMovement"/> members production does not use.</summary>
internal static class SamusAerialMovementAccess
{
    /// <summary>Sets up a dry-air jump and gravity through the translated bank-$90 movement rules.</summary>
    extension(SamusAerialMovement)
    {
        /// <summary>
        /// Ports the dry-air, no-hi-jump path through
        /// <c>Make_Samus_Jump</c> at <c>$90:98BC</c> and the normal-air branch of
        /// <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>.
        /// </summary>
        internal static void InitializeDryAirJump(ISnesAddressSpace bus, SamusState samus)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);

            samus.Kinematics.YSpeed =
                SamusVerticalMotion.Launch(SamusLiquidMedium.Air, highJump: false, kind: SamusLaunchKind.Jump);
            SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(samus);
            SamusAerialMovement.ConfigureDryAirGravity(bus, samus);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        }

        /// <summary>
        /// Publishes the normal-air gravity pair selected by <c>$90:9C5B</c>. The normal frame
        /// pipeline refreshes these environment-dependent words before movement even when the
        /// pose change was a walk-off rather than <c>Make_Samus_Jump</c>.
        /// </summary>
        internal static void ConfigureDryAirGravity(ISnesAddressSpace bus, SamusState samus)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            samus.Kinematics.YAcceleration =
                SamusVerticalMotion.Gravity(SamusLiquidMedium.Air);
        }
    }
}
