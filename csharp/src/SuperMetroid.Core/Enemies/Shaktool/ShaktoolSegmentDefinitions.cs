namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Compiled mechanics and callback metadata for Shaktool's seven linked body segments, leading saw first. Processing and contact:
/// every segment runs its animation script and is processed off screen; the five internal pieces also ignore Samus's
/// touch. Owner distance: how many slots each segment sits after the chain's first segment, its owner: its
/// position in the chain. The joints integrate symmetric increments 1,2,3,4,3,2 into steps 0,1,3,6,10,13,15: the
/// initial neighbor angle is eight whole steps back for each, and the initial curl per update an eighth of a step on for
/// each, the same curvature at 1/64 of the angle's magnitude. Initial program: both saws their primary-piece list,
/// the center segment the head-down list, the four arms the normal arm list. Callback by part: fixed anchor, head,
/// final saw and ordinary arms have distinct roles.
/// </summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>The leading saw: the chain's fixed anchor, in its first slot.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 0,
            RotationAngle.Zero, ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolFrameBehavior.Idle, RotationAngle.Zero);

    /// <summary>The arm segment after the leading saw.</summary>
    internal static readonly ShaktoolSegmentDefinition FirstArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 1,
            RotationAngle.WrappedSteps(-8 * 1), ShaktoolSegment.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, RotationAngle.FromSteps(0, 32));

    /// <summary>The arm segment before the head.</summary>
    internal static readonly ShaktoolSegmentDefinition SecondArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 2,
            RotationAngle.WrappedSteps(-8 * 3), ShaktoolSegment.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, RotationAngle.FromSteps(0, 96));

    /// <summary>The head at the chain's centre, which orients itself along the chain.</summary>
    internal static readonly ShaktoolSegmentDefinition Head =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 3,
            RotationAngle.WrappedSteps(-8 * 6), ShaktoolSegment.HeadAimingDown,
            ShaktoolFrameBehavior.OrbitAndOrientCenter, RotationAngle.FromSteps(0, 192));

    /// <summary>The arm segment after the head.</summary>
    internal static readonly ShaktoolSegmentDefinition ThirdArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 4,
            RotationAngle.WrappedSteps(-8 * 10), ShaktoolSegment.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, RotationAngle.FromSteps(1, 64));

    /// <summary>The arm segment before the trailing saw.</summary>
    internal static readonly ShaktoolSegmentDefinition FourthArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 5,
            RotationAngle.WrappedSteps(-8 * 13), ShaktoolSegment.ArmPieceNormal,
            ShaktoolFrameBehavior.OrbitPreviousSegment, RotationAngle.FromSteps(1, 160));

    /// <summary>The trailing saw, which drives the chain and reverses it at walls.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 6,
            RotationAngle.WrappedSteps(-8 * 15), ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolFrameBehavior.DriveTailAndReverseAtWalls, RotationAngle.FromSteps(1, 224));

    /// <summary>The seven segments in chain order, leading saw first.</summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
        [LeadingSaw, FirstArm, SecondArm, Head, ThirdArm, FourthArm, TrailingSaw];
}
