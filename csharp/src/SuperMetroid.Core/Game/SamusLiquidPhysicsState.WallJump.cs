using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusLiquidPhysicsState
{
    /// <summary>Ports $91:FA76's pose-entry dust writes, shared by ordinary and grapple wall jumps.</summary>
    internal void SpawnWallJumpDust(ISnesAddressSpace bus, SamusState samus)
    {
        // Samus_GetBottom_R18 uses the last occupied pixel, not the exclusive collision
        // boundary. Liquid suppression leaves all previous slot words intact, even when
        // Gravity Suit makes movement itself behave as though the room were dry.
        ushort bottom = unchecked((ushort)(samus.Kinematics.BottomBoundary - 1));
        if (DetermineRawMediumAtBoundary(bottom) != Air)
            return;
        int offset = samus.IsFacingRight(bus)
            ? -SamusWallJumpDustDefinitions.HorizontalOffset
            : SamusWallJumpDustDefinitions.HorizontalOffset;
        AtmosphericEffects.SetSlot(SamusWallJumpDustDefinitions.Slot,
            SamusWallJumpDustDefinitions.Type, 0, SamusWallJumpDustDefinitions.InitialTimer,
            unchecked((ushort)(samus.XPosition + offset)), bottom);
    }
}
