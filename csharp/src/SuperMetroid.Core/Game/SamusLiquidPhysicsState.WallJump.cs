using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusLiquidPhysicsState
{
    /// <summary>Ports $91:FA76's pose-entry dust writes, shared by ordinary and grapple wall jumps.</summary>
    internal void SpawnWallJumpDust(ISnesAddressSpace bus, SamusState samus)
    {
        // Get_Samus_Bottom_Boundary ($90:EC3E) uses the last occupied pixel of the current
        // pose's definition radius, not live $0B00, which keeps the spin body's radius
        // through the wall-jump frame. Liquid suppression leaves all previous slot words
        // intact, even when Gravity Suit makes movement itself behave as though the room
        // were dry.
        ushort bottom = unchecked((ushort)(samus.YPosition + SamusState.ReadPoseYRadius(samus.Pose) - 1));
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
