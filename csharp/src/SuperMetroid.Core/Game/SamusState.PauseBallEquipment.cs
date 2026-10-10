using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// $91:E83A/E867, called by unpause command $0C: reconcile Morph/Spring Ball
    /// equipment without changing position, velocity, or vertical direction.
    /// </summary>
    public void ReconcilePauseBallEquipment(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool springEquipped = EquippedItems.HasAny(SamusEquipmentFlags.SpringBall);
        SamusMovementType movement = ReadMovementType(bus);
        bool enable = springEquipped && movement is
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling;
        bool disable = !springEquipped && movement is
            SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling;
        if (!enable && !disable)
            return;

        // The native handlers select the stationary pose even while airborne. Keeping
        // ascent intact here permits the equipment-toggle mid-air Spring Ball jump.
        bool left = IsFacingLeft(bus);
        Pose = enable
            ? left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose
            : left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
        RefreshCollisionRadii(bus);
        // $91:F9F4/FA56 preserve animation only within the same ball family. Crossing
        // families through the equipment menu therefore runs the normal frame-zero setup.
        InitializeAnimation(bus);
    }
}
