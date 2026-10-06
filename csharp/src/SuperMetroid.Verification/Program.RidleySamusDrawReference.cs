using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Read-only cartridge oracle for $90:85E2/$864E/$8C1F. Uses checkpoint physics
    /// and original ROM definitions, never production draw methods or extracted art.
    /// Validated against visible native records before covering opposite NMI phases.
    /// </summary>
    private static (ushort Top, ushort Bottom, ushort X, ushort Y) RidleyNativeBodyRecord(
        ISnesAddressSpace rom, byte[] checkpoint)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(checkpoint.AsSpan(address, 2));
        ushort R(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        if ((W(RidleyMovieMemory.CeresStatus) & 0x8000) != 0)
            throw new InvalidDataException("Movie body oracle requires a native Mode 7 transform mapping.");
        ushort pose = W(RidleyMovieMemory.Pose), frame = W(RidleyMovieMemory.Animation);
        var movement = (SamusMovementType)checkpoint[RidleyMovieMemory.SamusMovementType];
        int yOffset = -unchecked((sbyte)rom.ReadByte(RidleyMovieMemory.PoseDefinitions + pose * 8 + 4));
        if (movement == SamusMovementType.Standing)
        {
            if (pose is SamusPoseIds.ForwardFacingPowerSuitPose or SamusPoseIds.ForwardFacingSuitedPose)
            {
                if (frame >= 2) yOffset = -1;
            }
            else if (pose >= SamusPoseIds.NormalLandingRightPose && pose <= SamusPoseIds.SpinLandingLeftPose)
                yOffset = -R(RidleyMovieMemory.LandingDrawOffsets +
                    (pose - SamusPoseIds.NormalLandingRightPose) * 4 + frame);
        }
        else if (movement == SamusMovementType.PostureTransition &&
            pose >= SamusPoseIds.CrouchingTransitionRightPose && pose < SamusPoseIds.MorphBallGroundLeftPose)
            yOffset = unchecked((sbyte)rom.ReadByte(RidleyMovieMemory.PostureDrawOffsets +
                (pose - SamusPoseIds.CrouchingTransitionRightPose) * 2 + frame));
        else if (movement == SamusMovementType.Special)
        {
            if (pose is SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose)
                yOffset = unchecked((sbyte)rom.ReadByte(RidleyMovieMemory.DrainedDrawOffsets + frame));
            else if (pose is SamusPoseIds.DrainedStandingRightPose or SamusPoseIds.DrainedStandingLeftPose && frame >= 5)
                yOffset = -3;
        }

        bool bottom = movement switch
        {
            SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or
                SamusMovementType.MorphBallFalling or SamusMovementType.UnusedGlitchBallAlternate or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling => false,
            SamusMovementType.SpinJumping => frame == 0 || frame >= 11 ||
                pose is SamusPoseIds.SpaceJumpRightPose or SamusPoseIds.SpaceJumpLeftPose or
                    SamusPoseIds.ScrewAttackRightPose or SamusPoseIds.ScrewAttackLeftPose,
            SamusMovementType.Knockback => frame >= 3 ||
                pose is not (SamusPoseIds.DeathSequenceRightPose or SamusPoseIds.DeathSequenceLeftPose),
            SamusMovementType.PostureTransition => pose >= SamusPoseIds.CrouchingTransitionAimUpRightPose ||
                (pose >= SamusPoseIds.UnusedPoseDd ? frame == 2 :
                 pose >= SamusPoseIds.UnusedPoseDb ? frame == 0 :
                 pose is SamusPoseIds.CrouchingTransitionRightPose or SamusPoseIds.CrouchingTransitionLeftPose or
                    SamusPoseIds.StandingTransitionRightPose or SamusPoseIds.StandingTransitionLeftPose),
            SamusMovementType.Unused0D => frame == 0 || pose is not (SamusPoseIds.UnusedPose65 or SamusPoseIds.UnusedPose66),
            SamusMovementType.WallJumping => frame < 3 || frame >= 13,
            SamusMovementType.DamageBoost => frame < 2 || frame >= 9,
            SamusMovementType.Special =>
                pose is not (SamusPoseIds.ShinesparkVerticalRightPose or SamusPoseIds.ShinesparkVerticalLeftPose) &&
                (frame >= 2 || pose is not (SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose)),
            _ when (ushort)movement < 28 => true,
            _ => throw new InvalidDataException($"Unknown native body movement type {movement}"),
        };
        return (unchecked((ushort)(R(RidleyMovieMemory.TopSpritemapBases + pose * 2) + frame)),
            bottom ? unchecked((ushort)(R(RidleyMovieMemory.BottomSpritemapBases + pose * 2) + frame)) : (ushort)0,
            unchecked((ushort)(W(RidleyMovieMemory.X) - W(RidleyMovieMemory.CameraX))),
            unchecked((ushort)(W(RidleyMovieMemory.Y) + yOffset - W(RidleyMovieMemory.CameraY))));
    }
}
