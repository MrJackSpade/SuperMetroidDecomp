namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The mechanics and behaviour of Shaktool's seven linked body segments, leading saw first. Processing and contact:
/// every segment runs its animation and is processed off screen; the five internal pieces also ignore Samus's
/// touch. Owner distance: how many places each segment sits after the chain's first segment, its owner: its
/// position in the chain. The joints integrate symmetric increments 1,2,3,4,3,2 into steps 0,1,3,6,10,13,15: the
/// initial neighbor angle is eight whole steps back for each, and the initial curl per update an eighth of a step on for
/// each, the same curvature at 1/64 of the angle's magnitude. Initial animation: both saws their primary-piece animation,
/// the center segment the head-down animation, the four arms the normal arm animation. Behaviour by part: fixed anchor, head,
/// final saw and ordinary arms have distinct roles. Collision animation: as the chain recovers from striking a wall, the
/// saws bob as their primary and final pieces, the head bobs, and each arm bobs as the back or front piece of its side.
/// </summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>The leading saw: the chain's fixed anchor, in its first place.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 0,
            RotationAngle.Zero, ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolSegmentMotion.Idle, RotationAngle.Zero, ShaktoolSegment.SawHandHeadBobPrimaryPiece);

    /// <summary>The outer arm on the leading saw's side, next to the saw.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingOuterArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 1,
            RotationAngle.WrappedSteps(-8 * 1), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromSteps(0, 32), ShaktoolSegment.ArmPieceHeadBobBack);

    /// <summary>The inner arm on the leading saw's side, next to the head.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingInnerArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 2,
            RotationAngle.WrappedSteps(-8 * 3), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromSteps(0, 96), ShaktoolSegment.ArmPieceHeadBobFront);

    /// <summary>The head at the chain's centre, which orients itself along the chain.</summary>
    internal static readonly ShaktoolSegmentDefinition Head =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 3,
            RotationAngle.WrappedSteps(-8 * 6), ShaktoolSegment.HeadAimingDown,
            ShaktoolSegmentMotion.OrbitAndOrientCenter, RotationAngle.FromSteps(0, 192), ShaktoolSegment.HeadHeadBob);

    /// <summary>The inner arm on the trailing saw's side, next to the head.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingInnerArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 4,
            RotationAngle.WrappedSteps(-8 * 10), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromSteps(1, 64), ShaktoolSegment.ArmPieceHeadBobFront);

    /// <summary>The outer arm on the trailing saw's side, next to the saw.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingOuterArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 5,
            RotationAngle.WrappedSteps(-8 * 13), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromSteps(1, 160), ShaktoolSegment.ArmPieceHeadBobBack);

    /// <summary>The trailing saw, which drives the chain and reverses it at walls.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 6,
            RotationAngle.WrappedSteps(-8 * 15), ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolSegmentMotion.DriveTailAndReverseAtWalls, RotationAngle.FromSteps(1, 224), ShaktoolSegment.SawHandHeadBobFinalPiece);

    /// <summary>The seven segments in chain order, leading saw first.</summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
        [LeadingSaw, LeadingOuterArm, LeadingInnerArm, Head, TrailingInnerArm, TrailingOuterArm, TrailingSaw];

    /// <summary>How many segments Shaktool's chain has, in consecutive enemy places: the two saws and the five between.</summary>
    internal const int SegmentCount = 7;
}
