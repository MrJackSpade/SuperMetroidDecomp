namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Compiled mechanics and callback metadata for Shaktool's seven linked body segments, leading saw first. Processing and contact:
/// every segment runs its animation script and is processed off screen; the five internal pieces also ignore Samus's
/// touch. Owner distance: how many slots each segment sits after the chain's first segment, its owner: its
/// position in the chain. The joints integrate symmetric increments 1,2,3,4,3,2 into steps 0,1,3,6,10,13,15: the
/// initial neighbor angle is -2048 times the step, wrapped to a turn of 65536, and the initial curling angle delta is 32
/// times it, the same curvature at 1/64 of the angle's magnitude. Initial program: both saws their primary-piece list,
/// the center segment the head-down list, the four arms the normal arm list. Layer: saws and head use layer 2, arms
/// layer 4. Callback by part: fixed anchor, head, final saw and ordinary arms have distinct roles.
/// </summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>The leading saw: the chain's fixed anchor, in its first slot.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.None, 0,
            RotationAngle.FromRaw(0), ShaktoolProgram.SawHandPrimaryPiece,
            ShaktoolFrameBehavior.Idle, 0);

    /// <summary>The arm segment after the leading saw.</summary>
    internal static readonly ShaktoolSegmentDefinition FirstArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.IgnoresSamus, 1,
            RotationAngle.NormalizeRaw(-2048 * 1), ShaktoolProgram.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, 32);

    /// <summary>The arm segment before the head.</summary>
    internal static readonly ShaktoolSegmentDefinition SecondArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.IgnoresSamus, 2,
            RotationAngle.NormalizeRaw(-2048 * 3), ShaktoolProgram.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, 96);

    /// <summary>The head at the chain's centre, which orients itself along the chain.</summary>
    internal static readonly ShaktoolSegmentDefinition Head =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.IgnoresSamus, 3,
            RotationAngle.NormalizeRaw(-2048 * 6), ShaktoolProgram.HeadAimingDown,
            ShaktoolFrameBehavior.OrbitAndOrientCenter, 192);

    /// <summary>The arm segment after the head.</summary>
    internal static readonly ShaktoolSegmentDefinition ThirdArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.IgnoresSamus, 4,
            RotationAngle.NormalizeRaw(-2048 * 10), ShaktoolProgram.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, 320);

    /// <summary>The arm segment before the trailing saw.</summary>
    internal static readonly ShaktoolSegmentDefinition FourthArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.IgnoresSamus, 5,
            RotationAngle.NormalizeRaw(-2048 * 13), ShaktoolProgram.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, 416);

    /// <summary>The trailing saw, which drives the chain and reverses it at walls.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyContact.None, 6,
            RotationAngle.NormalizeRaw(-2048 * 15), ShaktoolProgram.SawHandPrimaryPiece,
            ShaktoolFrameBehavior.DriveTailAndReverseAtWalls, 480);

    /// <summary>The seven segments in chain order, leading saw first.</summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
        [LeadingSaw, FirstArm, SecondArm, Head, ThirdArm, FourthArm, TrailingSaw];
}
