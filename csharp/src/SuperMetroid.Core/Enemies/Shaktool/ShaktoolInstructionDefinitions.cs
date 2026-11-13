namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The fixed animation selections Shaktool's mechanics state machine makes.</summary>
internal static class ShaktoolInstructionDefinitions
{
    /// <summary>
    /// The head animation of each eighth-turn
    /// direction bucket. The direction buckets start at up; program storage starts two eighth-turns earlier at left.
    /// </summary>
    internal static readonly Dictionary<CompassDirection, ShaktoolProgram> OrientationPrograms = new()
    {
        [CompassDirection.Up] = ShaktoolProgram.HeadAimingUp,
        [CompassDirection.UpRight] = ShaktoolProgram.HeadAimingUpRight,
        [CompassDirection.Right] = ShaktoolProgram.HeadAimingRight,
        [CompassDirection.DownRight] = ShaktoolProgram.HeadAimingDownRight,
        [CompassDirection.Down] = ShaktoolProgram.HeadAimingDown,
        [CompassDirection.DownLeft] = ShaktoolProgram.HeadAimingDownLeft,
        [CompassDirection.Left] = ShaktoolProgram.HeadAimingLeft,
        [CompassDirection.UpLeft] = ShaktoolProgram.HeadAimingUpLeft,
    };

    /// <summary>
    /// Collision and dormant-attack behavior by body part: primary saw, rear arm, front arm,
    /// head, front arm, rear arm, final saw.
    /// </summary>
    internal static readonly ShaktoolSegmentInstructionDefinition[] SegmentInstructions =
    [
        new(ShaktoolProgram.SawHandHeadBobPrimaryPiece),
        new(ShaktoolProgram.ArmPieceHeadBobBack),
        new(ShaktoolProgram.ArmPieceHeadBobFront),
        new(ShaktoolProgram.HeadHeadBob),
        new(ShaktoolProgram.ArmPieceHeadBobFront),
        new(ShaktoolProgram.ArmPieceHeadBobBack),
        new(ShaktoolProgram.SawHandHeadBobFinalPiece),
    ];
}
