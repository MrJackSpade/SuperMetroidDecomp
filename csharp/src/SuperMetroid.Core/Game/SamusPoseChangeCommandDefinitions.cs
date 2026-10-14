namespace SuperMetroid.Core.Game;

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
