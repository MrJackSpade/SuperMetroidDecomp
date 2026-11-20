namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How the ninja, walking and wall Space Pirates move, sense Samus and fire (#2156).</summary>
internal static class SpacePirateMotionDefinitions
{
    /// <summary>A slow wall Space Pirate's jump: two steps an update, landing at steps 190 and 66 of the turn (#2182).</summary>
    internal static WallSpacePirateJump SlowWallJump { get; } = new(RotationAngle.FromSteps(190), RotationAngle.FromSteps(66), 2);

    /// <summary>A fast wall Space Pirate's jump: four steps an update, landing at steps 192 and 64 of the turn (#2182).</summary>
    internal static WallSpacePirateJump FastWallJump { get; } = new(RotationAngle.FromSteps(192), RotationAngle.FromSteps(64), 4);

    /// <summary>
    /// How far into a block a wall Space Pirate standing against a wall must be, in pixels, for it to snap to the next
    /// block's start rather than back to its eight-pixel boundary (#2182).
    /// </summary>
    internal const int WallSnapForwardFrom = 11;

    /// <summary>The speed a ninja's divekick jump and descent start at, six pixels an update.</summary>
    internal static SignedEightEight NinjaDiveStartSpeed { get; } = SignedEightEight.FromPixels(6);

    /// <summary>How much a ninja's spin jump speed changes each update, an eighth of a pixel.</summary>
    internal static SignedEightEight NinjaSpinSpeedChange { get; } = SignedEightEight.FromPixelFraction(1, 8);

    /// <summary>How much a ninja's divekick speed falls each update, a quarter of a pixel.</summary>
    internal static SignedEightEight NinjaDiveSpeedChange { get; } = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>How close horizontally Samus comes, in pixels, for an idle ninja to activate.</summary>
    internal const int NinjaActivationDistance = 128;

    /// <summary>How close on both axes, in pixels and exclusive, the shot the flinch tests
    /// (<see cref="PirateFlinchFromHighestShot"/>) comes before a ninja reacts.</summary>
    internal const int NinjaFlinchDistance = 32;

    /// <summary>How close to the posts' midpoint, in pixels and exclusive, Samus comes before a ninja may spin jump or divekick.</summary>
    internal const int NinjaMidpointTriggerDistance = 32;

    /// <summary>How close on each axis, in pixels and exclusive, Samus comes before a ninja kicks standing.</summary>
    internal const int NinjaKickDistance = 40;

    /// <summary>How far a ninja's divekick descent moves sideways each update, in whole pixels.</summary>
    internal const int NinjaDiveStep = 5;

    /// <summary>How far a ninja walking back to its post after a dive moves each update, in whole pixels.</summary>
    internal const int NinjaReturnStep = 2;

    /// <summary>How far each update of a ninja's spin jump moves it vertically, in whole pixels.</summary>
    internal const int NinjaSpinClimbStep = 2;

    /// <summary>How far to either side of a landing ninja its dust rises, in pixels.</summary>
    internal const int NinjaLandingDustSpread = 8;

    /// <summary>How far below a landing ninja its dust rises, in pixels.</summary>
    internal const int NinjaLandingDustDrop = 28;

    /// <summary>A thrown claw's launch speed, eight pixels an update.</summary>
    internal static ReturningClawSpeed NinjaClawLaunchSpeed { get; } = ReturningClawSpeed.FromPixels(8);

    /// <summary>How far down a walking Pirate's ground probe reaches, one pixel.</summary>
    internal static SignedSixteenSixteen WalkingGroundProbeStep { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>How far left of a walking Pirate its patrol's ledge probe reaches, in pixels: one further than the right.</summary>
    internal const int WalkingLeftLedgeProbe = 17;

    /// <summary>How far right of a walking Pirate its patrol's ledge probe reaches, in pixels.</summary>
    internal const int WalkingRightLedgeProbe = 16;

    /// <summary>A walking Pirate's rightward patrol step, seven thirty-seconds of a pixel.</summary>
    internal static SignedSixteenSixteen WalkingRightStep { get; } = SignedSixteenSixteen.FromPixelFraction(7, 32);

    /// <summary>A walking Pirate's leftward patrol step: the rightward step mirrored, and the finest distance further left.</summary>
    internal static SignedSixteenSixteen WalkingLeftStep { get; } = -WalkingRightStep - SignedSixteenSixteen.FromPixelFraction(1, 65536);

    /// <summary>The farthest vertically Samus may be from a walking Pirate, in pixels, for it to choose its laser attack.</summary>
    internal const int WalkingFiringBand = 16;

    /// <summary>How close horizontally a projectile comes, in pixels, before a walking Pirate flinches from it.</summary>
    internal const int WalkingFlinchReach = 32;

    /// <summary>How far ahead of a walking Pirate's centre its laser leaves, in pixels.</summary>
    internal const int WalkingLaserMuzzleAhead = 24;

    /// <summary>How far a walking Pirate's slow laser moves each update, in whole pixels.</summary>
    internal const int WalkingSlowLaserStep = 2;

    /// <summary>How far a walking Pirate's fast laser moves each update, in whole pixels.</summary>
    internal const int WalkingFastLaserStep = 4;

    /// <summary>How close vertically Samus comes, in pixels, before a climbing wall Pirate starts to fire and jump.</summary>
    internal const int WallDetectionBand = 32;

    /// <summary>How far above a wall Pirate's centre its laser leaves, in pixels.</summary>
    internal const int WallLaserMuzzleAbove = 16;
}
