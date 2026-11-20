namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's seven linked body segments, leading saw first. Every segment animates and moves off screen, and the five
/// between the saws ignore Samus's touch. Each starts around the one before it, the chain curled a little more at each
/// joint from the leading saw on, and curls each update by a sixty-fourth of the turn by which its start falls short of
/// a full turn. Both saws start as their primary pieces, the head facing down and the four arms as their normal pieces;
/// the leading saw anchors the chain, and the head, the final saw and the arms each move their own way. As the chain
/// recovers from striking a wall, the saws bob as their primary and final pieces, the head bobs, and each arm bobs as the
/// back or front piece of its side.
/// </summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>The leading saw: the chain's fixed anchor, in its first place.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 0,
            RotationAngle.Zero, ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolSegmentMotion.Idle, RotationAngle.Zero, ShaktoolSegment.SawHandHeadBobPrimaryPiece, AnchorsChain: true, IsSaw: true);

    /// <summary>The outer arm on the leading saw's side, next to the saw.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingOuterArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 1,
            RotationAngle.FromSteps(248), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromStepFraction(1, 8), ShaktoolSegment.ArmPieceHeadBobBack);

    /// <summary>The inner arm on the leading saw's side, next to the head.</summary>
    internal static readonly ShaktoolSegmentDefinition LeadingInnerArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 2,
            RotationAngle.FromSteps(232), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromStepFraction(3, 8), ShaktoolSegment.ArmPieceHeadBobFront);

    /// <summary>The head at the chain's centre, which orients itself along the chain.</summary>
    internal static readonly ShaktoolSegmentDefinition Head =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 3,
            RotationAngle.FromSteps(208), ShaktoolSegment.HeadAimingDown,
            ShaktoolSegmentMotion.OrbitAndOrientCenter, RotationAngle.FromStepFraction(3, 4), ShaktoolSegment.HeadHeadBob);

    /// <summary>The inner arm on the trailing saw's side, next to the head.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingInnerArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 4,
            RotationAngle.FromSteps(176), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromStepFraction(5, 4), ShaktoolSegment.ArmPieceHeadBobFront);

    /// <summary>The outer arm on the trailing saw's side, next to the saw.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingOuterArm =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.IgnoresSamus, 5,
            RotationAngle.FromSteps(152), ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegmentMotion.OrbitPreviousSegment, RotationAngle.FromStepFraction(13, 8), ShaktoolSegment.ArmPieceHeadBobBack);

    /// <summary>The trailing saw, which drives the chain and reverses it at walls.</summary>
    internal static readonly ShaktoolSegmentDefinition TrailingSaw =
        new(EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyContact.None, 6,
            RotationAngle.FromSteps(136), ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolSegmentMotion.DriveTailAndReverseAtWalls, RotationAngle.FromStepFraction(15, 8), ShaktoolSegment.SawHandHeadBobFinalPiece, IsSaw: true);

    /// <summary>The seven segments in chain order, leading saw first.</summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
        [LeadingSaw, LeadingOuterArm, LeadingInnerArm, Head, TrailingInnerArm, TrailingOuterArm, TrailingSaw];

    /// <summary>How many segments Shaktool's chain has, in consecutive enemy places: the two saws and the five between.</summary>
    internal const int SegmentCount = 7;
}
