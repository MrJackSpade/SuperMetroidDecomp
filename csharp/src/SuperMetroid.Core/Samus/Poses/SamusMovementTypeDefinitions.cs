using Bomb = SuperMetroid.Core.Samus.Poses.SamusBombJumpSetup;
using Move = SuperMetroid.Core.Samus.Movement.SamusMovementType;
using Response = SuperMetroid.Core.Samus.SamusMomentumResponse;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What each way Samus moves does when her input finds no new pose, and how it sets up a published bomb jump (#2323).
/// Standing and crouching take the forward jump unless time is frozen; running, falling, the compact air poses,
/// moonwalking, wall jumps, wall stops and the Grapple take it always; the balls and knockback keep their pose; jumps,
/// turns, posture changes, damage boosts, Draygon's hold and the special poses turn the jump down.
/// </summary>
internal static class SamusMovementTypeDefinitions
{
    /// <summary>Each movement type's definition.</summary>
    internal static IReadOnlyDictionary<Move, SamusMovementTypeDefinition> Movements { get; } = new Dictionary<Move, SamusMovementTypeDefinition>
    {
        [Move.Standing] = new(Response.Stop, Bomb.ForwardJumpUnlessTimeFrozen),
        [Move.Running] = new(Response.Decelerate, Bomb.ForwardJump),
        [Move.NormalJumping] = new(Response.Decelerate, Bomb.TurnedDown),
        [Move.SpinJumping] = new(Response.None, Bomb.TurnedDown),
        [Move.MorphBallGround] = new(Response.KillXSpeed, Bomb.KeepPose),
        [Move.Crouching] = new(Response.Stop, Bomb.ForwardJumpUnlessTimeFrozen),
        [Move.Falling] = new(Response.KillRunSpeed, Bomb.ForwardJump),
        [Move.GlitchBall] = new(Response.Stop, Bomb.KeepPose),
        [Move.MorphBallFalling] = new(Response.Decelerate, Bomb.KeepPose),
        [Move.GlitchBallKnockback] = new(Response.KillXSpeed, Bomb.KeepPose),
        [Move.Knockback] = new(Response.Stop, Bomb.KeepPose),
        [Move.CompactAirborne] = new(Response.KillXSpeed, Bomb.ForwardJump),
        [Move.TurningOnGround] = new(Response.Stop, Bomb.TurnedDown),
        [Move.PostureTransition] = new(Response.Stop, Bomb.TurnedDown),
        [Move.Moonwalking] = new(Response.Stop, Bomb.ForwardJump),
        [Move.SpringBallGround] = new(Response.KillXSpeed, Bomb.KeepPose),
        [Move.SpringBallInAir] = new(Response.KillXSpeed, Bomb.KeepPose),
        [Move.SpringBallFalling] = new(Response.KillXSpeed, Bomb.KeepPose),
        [Move.WallJumping] = new(Response.KillXSpeed, Bomb.ForwardJump),
        [Move.RanIntoWall] = new(Response.Stop, Bomb.ForwardJump),
        [Move.Grappling] = new(Response.KillXSpeed, Bomb.ForwardJump),
        [Move.TurningWhileJumping] = new(Response.Stop, Bomb.TurnedDown),
        [Move.TurningWhileFalling] = new(Response.Stop, Bomb.TurnedDown),
        [Move.DamageBoost] = new(Response.Stop, Bomb.TurnedDown),
        [Move.DraygonHeld] = new(Response.Stop, Bomb.TurnedDown),
        [Move.Special] = new(Response.Stop, Bomb.TurnedDown),
    };
}
