namespace SuperMetroid.Core.Game;

/// <summary>
/// Prospective pose change commands dispatched by <c>$91:EC16</c> when the prospective
/// pose equals the current pose. Each command is one handler, so this is not a flag set.
/// </summary>
public enum SamusProspectivePoseChangeCommand : byte
{
    /// <summary>Command 0, <c>RTS_91EFC3</c>.</summary>
    None = 0,

    /// <summary>Command 1, <c>ProspectivePoseCmd_1_Decelerate</c>.</summary>
    Decelerate = 1,

    /// <summary>Command 2, <c>ProspectivePoseCmd_2_Stop</c>.</summary>
    Stop = 2,

    /// <summary>Command 6, <c>ProspectivePoseCmd_6_KillXSpeed</c>.</summary>
    KillXSpeed = 6,

    /// <summary>Command 8, <c>ProspectivePoseCmd_8_KillRunSpeed</c>.</summary>
    KillRunSpeed = 8,
}

/// <summary>The lookup-failure command table at <c>$91:8332</c>, indexed by movement type.</summary>
public static class SamusPoseChangeCommandDefinitions
{
    /// <summary>Reads <c>Set_ProspectivePoseChangeCommand</c>'s table entry for a movement type.</summary>
    public static SamusProspectivePoseChangeCommand ForLookupFailure(SamusMovementType movementType) =>
        movementType switch
        {
            SamusMovementType.Standing => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Running => SamusProspectivePoseChangeCommand.Decelerate,
            SamusMovementType.NormalJumping => SamusProspectivePoseChangeCommand.Decelerate,
            SamusMovementType.SpinJumping => SamusProspectivePoseChangeCommand.None,
            SamusMovementType.MorphBallGround => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.Crouching => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Falling => SamusProspectivePoseChangeCommand.KillRunSpeed,
            SamusMovementType.UnusedGlitchBall => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.MorphBallFalling => SamusProspectivePoseChangeCommand.Decelerate,
            SamusMovementType.UnusedGlitchBallAlternate => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.Knockback => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Unused0B => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Unused0C => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Unused0D => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.TurningOnGround => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.PostureTransition => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Moonwalking => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.SpringBallGround => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.SpringBallInAir => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.SpringBallFalling => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.WallJumping => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.RanIntoWall => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Grappling => SamusProspectivePoseChangeCommand.KillXSpeed,
            SamusMovementType.TurningWhileJumping => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.TurningWhileFalling => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.DamageBoost => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.DraygonHeld => SamusProspectivePoseChangeCommand.Stop,
            SamusMovementType.Special => SamusProspectivePoseChangeCommand.Stop,
            _ => throw new ArgumentOutOfRangeException(nameof(movementType), movementType,
                "Movement type has no $91:8332 lookup-failure command."),
        };
}

/// <summary>The prospective pose and command chosen by <c>HandleTransitionTableLookupFailure</c>.</summary>
public readonly record struct SamusLookupFailurePose(byte ProspectivePose)
{
    /// <summary>
    /// Ports <c>$91:82D9</c> with <c>Set_ProspectivePoseChangeCommand</c> ($91:8304).
    /// Decelerate retains the current pose while base X speed remains and otherwise
    /// becomes Stop; every other command consults pose-definition byte two, where $FF
    /// retains the current pose.
    /// </summary>
    public static SamusLookupFailurePose Resolve(
        SamusMovementType movementType,
        byte currentPose,
        byte definitionFallbackPose,
        bool hasBaseXSpeed)
    {
        SamusProspectivePoseChangeCommand command = SamusPoseChangeCommandDefinitions.ForLookupFailure(movementType);
        if (command == SamusProspectivePoseChangeCommand.Decelerate)
        {
            if (hasBaseXSpeed)
                return new SamusLookupFailurePose(currentPose);
            command = SamusProspectivePoseChangeCommand.Stop;
        }
        byte pose = definitionFallbackPose == SamusMovementRomData.Poses.RetainCurrentPoseFallback
            ? currentPose
            : definitionFallbackPose;
        return new SamusLookupFailurePose(pose);
    }
}
