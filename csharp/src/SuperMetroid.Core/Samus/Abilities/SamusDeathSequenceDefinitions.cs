using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Fatal-damage pose entry and state boundaries.</summary>
public static class SamusDeathSequenceDefinitions
{
    /// <summary>The updates the ordinary death pose displays before flashing.</summary>
    public const int PreFlashFrameCount = 16;
    /// <summary>The updates that alternate shades before the suit explodes.</summary>
    public const int FlashFrameCount = 60;
    /// <summary>Updates the flash holds its first death shade, and waits before its first change of shade.</summary>
    public const int FirstFlashShadeUpdates = 3;
    /// <summary>Updates the flash holds its second death shade.</summary>
    public const int SecondFlashShadeUpdates = 1;
    /// <summary>The brightest whiteout shade the explosion's frames reach; the whiteout stays there until the last update.</summary>
    public const int BrightestExplosionWhiteoutShade = 20;
    /// <summary>The whiteout shade the sequence's last update shows, after the explosion's last frame: full white.</summary>
    public const int FinalWhiteoutShade = 21;
    /// <summary>The updates the death pose's starting picture shows for: the delay every death pose's frames share.</summary>
    public const int InitialFrameHold = 2;

    /// <summary>The death pose's first frame, which the glitch-ball movements start it on.</summary>
    internal static SamusAnimationFrame GlitchBallStart { get; } = new(SamusAnimationSegmentName.Main, 0);
    /// <summary>The death pose's ball frame, which the Morph Ball and Spring Ball movements start it on.</summary>
    internal static SamusAnimationFrame BallStart { get; } = new(SamusAnimationSegmentName.Main, 1);
    /// <summary>The death pose's upright frame, which its repeat holds and every other movement starts it on.</summary>
    internal static SamusAnimationFrame UprightStart { get; } = new(SamusAnimationSegmentName.Cycle, 0);

    /// <summary>The death pose's frame each movement type starts it on; InitialFrame looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusMovementType, SamusAnimationFrame> InitialFrames = new Dictionary<SamusMovementType, SamusAnimationFrame>
    {
        [SamusMovementType.MorphBallGround] = BallStart,
        [SamusMovementType.MorphBallFalling] = BallStart,
        [SamusMovementType.SpringBallGround] = BallStart,
        [SamusMovementType.SpringBallInAir] = BallStart,
        [SamusMovementType.SpringBallFalling] = BallStart,
        [SamusMovementType.GlitchBall] = GlitchBallStart,
        [SamusMovementType.GlitchBallKnockback] = GlitchBallStart,
        [SamusMovementType.Standing] = UprightStart,
        [SamusMovementType.Running] = UprightStart,
        [SamusMovementType.NormalJumping] = UprightStart,
        [SamusMovementType.SpinJumping] = UprightStart,
        [SamusMovementType.Crouching] = UprightStart,
        [SamusMovementType.Falling] = UprightStart,
        [SamusMovementType.Knockback] = UprightStart,
        [SamusMovementType.CompactAirborne] = UprightStart,
        [SamusMovementType.TurningOnGround] = UprightStart,
        [SamusMovementType.PostureTransition] = UprightStart,
        [SamusMovementType.Moonwalking] = UprightStart,
        [SamusMovementType.WallJumping] = UprightStart,
        [SamusMovementType.RanIntoWall] = UprightStart,
        [SamusMovementType.Grappling] = UprightStart,
        [SamusMovementType.TurningRisingOrAimedCrouched] = UprightStart,
        [SamusMovementType.TurningWhileFalling] = UprightStart,
        [SamusMovementType.DamageBoost] = UprightStart,
        [SamusMovementType.DraygonHeld] = UprightStart,
        [SamusMovementType.Special] = UprightStart,
    };
}
