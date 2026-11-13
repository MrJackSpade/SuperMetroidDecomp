using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The momentum response each movement type runs.</summary>
public static class SamusMomentumResponses
{
    /// <summary>Reads the momentum response a movement type runs when its input finds no new pose.</summary>
    public static SamusMomentumResponse ForLookupFailure(SamusMovementType movementType) =>
        movementType switch
        {
            SamusMovementType.Standing => SamusMomentumResponse.Stop,
            SamusMovementType.Running => SamusMomentumResponse.Decelerate,
            SamusMovementType.NormalJumping => SamusMomentumResponse.Decelerate,
            SamusMovementType.SpinJumping => SamusMomentumResponse.None,
            SamusMovementType.MorphBallGround => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.Crouching => SamusMomentumResponse.Stop,
            SamusMovementType.Falling => SamusMomentumResponse.KillRunSpeed,
            SamusMovementType.UnusedGlitchBall => SamusMomentumResponse.Stop,
            SamusMovementType.MorphBallFalling => SamusMomentumResponse.Decelerate,
            SamusMovementType.UnusedGlitchBallAlternate => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.Knockback => SamusMomentumResponse.Stop,
            SamusMovementType.CompactAirborne => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.TurningOnGround => SamusMomentumResponse.Stop,
            SamusMovementType.PostureTransition => SamusMomentumResponse.Stop,
            SamusMovementType.Moonwalking => SamusMomentumResponse.Stop,
            SamusMovementType.SpringBallGround => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.SpringBallInAir => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.SpringBallFalling => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.WallJumping => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.RanIntoWall => SamusMomentumResponse.Stop,
            SamusMovementType.Grappling => SamusMomentumResponse.KillXSpeed,
            SamusMovementType.TurningWhileJumping => SamusMomentumResponse.Stop,
            SamusMovementType.TurningWhileFalling => SamusMomentumResponse.Stop,
            SamusMovementType.DamageBoost => SamusMomentumResponse.Stop,
            SamusMovementType.DraygonHeld => SamusMomentumResponse.Stop,
            SamusMovementType.Special => SamusMomentumResponse.Stop,
            _ => throw new ArgumentOutOfRangeException(nameof(movementType), movementType,
                "Movement type has no lookup-failure momentum response."),
        };
}
