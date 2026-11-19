using Bomb = SuperMetroid.Core.Samus.Poses.SamusBombJumpSetup;
using Firing = SuperMetroid.Core.Samus.Grapple.GrappleFiringResponse;
using Selection = SuperMetroid.Core.Samus.Hud.SamusHudSelectionMode;
using Knockback = SuperMetroid.Core.Samus.Movement.SamusKnockbackResponse;
using Move = SuperMetroid.Core.Samus.Movement.SamusMovementType;
using Response = SuperMetroid.Core.Samus.SamusMomentumResponse;
using Setup = SuperMetroid.Core.Samus.Poses.SamusNoTransitionSetup;
using Splash = SuperMetroid.Core.Samus.Movement.WaterSplashKind;
using Xray = SuperMetroid.Core.Samus.Abilities.XrayPosture;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What each way Samus moves does (#2323, #2445, #2506).
/// <list type="bullet">
/// <item>With no new pose from her input: standing and crouching take the forward jump unless time is frozen; running,
/// falling, the compact air poses, moonwalking, wall jumps, wall stops and the Grapple take it always; the balls and
/// knockback keep their pose; jumps, turns, posture changes, damage boosts, Draygon's hold and the special poses turn the
/// jump down.</item>
/// <item>The X-ray starts standing from standing, running and wall stops, crouching from a crouch, and not at all from
/// any other movement.</item>
/// <item>Entering water, the grounded postures raise the paired surface splash and every other movement dives.</item>
/// <item>Firing the Grapple is cancelled in every movement but the upright ground and air movements, moonwalking, wall
/// stops, the Grapple's own and Draygon's hold.</item>
/// <item>A hit knocks an upright Samus back in the knockback pose, a ball back in its own pose and the glitch ball back in
/// its knockback pose; knockback, turns, posture changes, the Grapple, damage boosts, Draygon's hold and the special poses
/// refuse it.</item>
/// <item>When her input chooses no new pose and her pose names no setup of its own, the ground turn stops her momentum and
/// the normal jump takes its fallback's aim and slows down; every other movement sets nothing up.</item>
/// </list>
/// </summary>
internal static class SamusMovementTypeDefinitions
{
    /// <summary>Each movement type's definition.</summary>
    internal static IReadOnlyDictionary<Move, SamusMovementTypeDefinition> Movements { get; } = new Dictionary<Move, SamusMovementTypeDefinition>
    {
        [Move.Standing] = new(Response.Stop, Bomb.ForwardJumpUnlessTimeFrozen, Xray.Standing, Splash.GroundedPair,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.Running] = new(Response.Decelerate, Bomb.ForwardJump, Xray.Standing, Splash.Diving,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.NormalJumping] = new(Response.Decelerate, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Fires, Selection.Standard, Knockback.Upright, Setup.ContinueNormalJump),
        [Move.SpinJumping] = new(Response.None, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Upright),
        [Move.MorphBallGround] = new(Response.KillXSpeed, Bomb.KeepPose, Xray.Disallowed, Splash.GroundedPair,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.Crouching] = new(Response.Stop, Bomb.ForwardJumpUnlessTimeFrozen, Xray.Crouching, Splash.GroundedPair,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.Falling] = new(Response.KillRunSpeed, Bomb.ForwardJump, Xray.Disallowed, Splash.Diving,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.GlitchBall] = new(Response.Stop, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.MorphBall, Knockback.GlitchBall),
        [Move.MorphBallFalling] = new(Response.Decelerate, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.GlitchBallKnockback] = new(Response.KillXSpeed, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.Knockback] = new(Response.Stop, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Refused),
        [Move.CompactAirborne] = new(Response.KillXSpeed, Bomb.ForwardJump, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Upright),
        [Move.TurningOnGround] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.GroundedPair,
            Firing.Cancelled, Selection.Turning, Knockback.Refused, Setup.StopTurn),
        [Move.PostureTransition] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.GroundedPair,
            Firing.Cancelled, Selection.Transition, Knockback.Refused),
        [Move.Moonwalking] = new(Response.Stop, Bomb.ForwardJump, Xray.Disallowed, Splash.GroundedPair,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.SpringBallGround] = new(Response.KillXSpeed, Bomb.KeepPose, Xray.Disallowed, Splash.GroundedPair,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.SpringBallInAir] = new(Response.KillXSpeed, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.SpringBallFalling] = new(Response.KillXSpeed, Bomb.KeepPose, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.MorphBall, Knockback.Ball),
        [Move.WallJumping] = new(Response.KillXSpeed, Bomb.ForwardJump, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Upright),
        [Move.RanIntoWall] = new(Response.Stop, Bomb.ForwardJump, Xray.Standing, Splash.GroundedPair,
            Firing.Fires, Selection.Standard, Knockback.Upright),
        [Move.Grappling] = new(Response.KillXSpeed, Bomb.ForwardJump, Xray.Disallowed, Splash.Diving,
            Firing.Fires, Selection.Grapple, Knockback.Refused),
        [Move.TurningWhileJumping] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Turning, Knockback.Refused),
        [Move.TurningWhileFalling] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Turning, Knockback.Refused),
        [Move.DamageBoost] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Refused),
        [Move.DraygonHeld] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Fires, Selection.DraygonHeld, Knockback.Refused),
        [Move.Special] = new(Response.Stop, Bomb.TurnedDown, Xray.Disallowed, Splash.Diving,
            Firing.Cancelled, Selection.Jump, Knockback.Refused),
    };
}
