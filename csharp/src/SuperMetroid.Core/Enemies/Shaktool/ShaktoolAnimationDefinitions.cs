namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The fixed animation selections Shaktool's mechanics state machine makes.</summary>
internal static class ShaktoolAnimationDefinitions
{
    /// <summary>
    /// The head animation of each eighth-turn
    /// direction bucket. The direction buckets start at up; program storage starts two eighth-turns earlier at left.
    /// </summary>
    internal static readonly Dictionary<CompassDirection, ShaktoolSegment> OrientationAnimations = new()
    {
        [CompassDirection.Up] = ShaktoolSegment.HeadAimingUp,
        [CompassDirection.UpRight] = ShaktoolSegment.HeadAimingUpRight,
        [CompassDirection.Right] = ShaktoolSegment.HeadAimingRight,
        [CompassDirection.DownRight] = ShaktoolSegment.HeadAimingDownRight,
        [CompassDirection.Down] = ShaktoolSegment.HeadAimingDown,
        [CompassDirection.DownLeft] = ShaktoolSegment.HeadAimingDownLeft,
        [CompassDirection.Left] = ShaktoolSegment.HeadAimingLeft,
        [CompassDirection.UpLeft] = ShaktoolSegment.HeadAimingUpLeft,
    };

    /// <summary>
    /// Collision and dormant-attack behavior by body part: primary saw, rear arm, front arm,
    /// head, front arm, rear arm, final saw.
    /// </summary>
    internal static readonly ShaktoolSegmentAnimationDefinition[] SegmentAnimations =
    [
        new(ShaktoolSegment.SawHandHeadBobPrimaryPiece),
        new(ShaktoolSegment.ArmPieceHeadBobBack),
        new(ShaktoolSegment.ArmPieceHeadBobFront),
        new(ShaktoolSegment.HeadHeadBob),
        new(ShaktoolSegment.ArmPieceHeadBobFront),
        new(ShaktoolSegment.ArmPieceHeadBobBack),
        new(ShaktoolSegment.SawHandHeadBobFinalPiece),
    ];
}
