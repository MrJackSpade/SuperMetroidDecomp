namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The fixed animation selections Shaktool's mechanics state machine makes.</summary>
internal static class ShaktoolInstructionDefinitions
{
    /// <summary>
    /// $AA:DD15-$DD24 over the eight head animations stored from $AA:DAA4: the head animation of each eighth-turn
    /// direction bucket. The direction buckets start at up; program storage starts two eighth-turns earlier at left.
    /// </summary>
    internal static readonly Dictionary<int, ShaktoolProgram> OrientationPrograms = new()
    {
        [0] = ShaktoolProgram.HeadAimingUp,
        [32] = ShaktoolProgram.HeadAimingUpRight,
        [64] = ShaktoolProgram.HeadAimingRight,
        [96] = ShaktoolProgram.HeadAimingDownRight,
        [128] = ShaktoolProgram.HeadAimingDown,
        [160] = ShaktoolProgram.HeadAimingDownLeft,
        [192] = ShaktoolProgram.HeadAimingLeft,
        [224] = ShaktoolProgram.HeadAimingUpLeft,
    };

    /// <summary>
    /// $AA:DF13 and $AA:DF21: collision and dormant-attack behavior by body part: primary saw, rear arm, front arm,
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
