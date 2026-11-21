using SuperMetroid.Core.Enemies.Artwork;
using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Lower Norfair Ridley's twelve death-breakup actors fly apart: their speeds, frames, order and lifetimes. The
/// selected animations remain authored data.
/// </summary>
internal static class RidleyExplosionDefinitions
{
    /// <summary>
    /// The breakup setup draws a fragment's horizontal speed from one random value as the sum of two
    /// selections: this one, none up to three sixteenths of a pixel per frame, and <see cref="FragmentSpeedWholePixel"/>.
    /// </summary>
    internal static readonly RandomSpeed FragmentSpeedFraction = new(RidleyRandomRolls.FragmentSpeedSixteenths, SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 16));
    /// <summary>The selection of a whole pixel per frame or none that a fragment's horizontal speed adds.</summary>
    internal static readonly RandomSpeed FragmentSpeedWholePixel = new(RandomSampleField.SecondOf(2), SignedEightEight.Zero, SignedEightEight.FromPixels(1));
    /// <summary>
    /// The breakup's spawn order: the tail from tip to base, then wings, legs, torso, head and claw. The places the
    /// pieces take and the draw order make this sequence observable.
    /// </summary>
    internal static readonly RidleyExplosionPart[] BreakupOrder =
    [
        RidleyExplosionPart.TailTip, RidleyExplosionPart.TailInnerJoint, RidleyExplosionPart.TailFourthShaftLink, RidleyExplosionPart.TailThirdShaftLink,
        RidleyExplosionPart.TailSecondShaftLink, RidleyExplosionPart.TailFirstShaftLink, RidleyExplosionPart.TailBase, RidleyExplosionPart.Wings,
        RidleyExplosionPart.Legs, RidleyExplosionPart.Torso, RidleyExplosionPart.OpenHeadAndNeck, RidleyExplosionPart.Claw,
    ];

    /// <summary>
    /// The updates each part lasts, dying on the last: tail fragments die eight updates apart from the base, the body parts
    /// follow the same eight-update progression, and the torso dies last.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, int> LifetimeUpdates = new Dictionary<RidleyExplosionPart, int>
    {
        [RidleyExplosionPart.TailBase] = 73,
        [RidleyExplosionPart.TailFirstShaftLink] = 81,
        [RidleyExplosionPart.TailSecondShaftLink] = 89,
        [RidleyExplosionPart.TailThirdShaftLink] = 97,
        [RidleyExplosionPart.TailFourthShaftLink] = 105,
        [RidleyExplosionPart.TailInnerJoint] = 113,
        [RidleyExplosionPart.TailTip] = 121,
        [RidleyExplosionPart.Wings] = 41,
        [RidleyExplosionPart.Legs] = 49,
        [RidleyExplosionPart.OpenHeadAndNeck] = 57,
        [RidleyExplosionPart.Claw] = 65,
        [RidleyExplosionPart.Torso] = 129,
    }.ToFrozenDictionary();

    /// <summary>The tail pose each of the first six tail fragments shows.</summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, EnemyFrameId> TailFrames = new Dictionary<RidleyExplosionPart, EnemyFrameId>
    {
        [RidleyExplosionPart.TailBase] = EnemyFrameId.RidleyTailBaseLink,
        [RidleyExplosionPart.TailFirstShaftLink] = EnemyFrameId.RidleyTailBaseLink,
        [RidleyExplosionPart.TailSecondShaftLink] = EnemyFrameId.RidleyTailMiddleLink,
        [RidleyExplosionPart.TailThirdShaftLink] = EnemyFrameId.RidleyTailMiddleLink,
        [RidleyExplosionPart.TailFourthShaftLink] = EnemyFrameId.RidleyTailOuterLink,
        [RidleyExplosionPart.TailInnerJoint] = EnemyFrameId.RidleyTailOuterLink,
    }.ToFrozenDictionary();

    /// <summary>
    /// The body-relative position while Ridley faces left and the pose each way of each body fragment, loaded by the
    /// breakup setup; a right-facing placement mirrors the left-facing one's X.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, RidleyExplosionBodyPartDefinition> BodyParts =
        new Dictionary<RidleyExplosionPart, RidleyExplosionBodyPartDefinition>
        {
            [RidleyExplosionPart.Wings] = new(new(0, 0), EnemyFrameId.RidleyWingFacingLeftRaised, EnemyFrameId.RidleyWingFacingRightRaised),
            [RidleyExplosionPart.Legs] = new(RidleyCollisionDefinitions.LeftLegsOrigin, EnemyFrameId.RidleyBreakupLegsLeft, EnemyFrameId.RidleyBreakupLegsRight),
            [RidleyExplosionPart.OpenHeadAndNeck] = new(RidleyCollisionDefinitions.LeftHeadOrigin, EnemyFrameId.RidleyBreakupOpenHeadAndNeckLeft, EnemyFrameId.RidleyBreakupOpenHeadAndNeckRight),
            [RidleyExplosionPart.Torso] = new(RidleyCollisionDefinitions.LeftTorsoOrigin, EnemyFrameId.RidleyBreakupTorsoLeft, EnemyFrameId.RidleyBreakupTorsoRight),
            [RidleyExplosionPart.Claw] = new(new(8, 7), EnemyFrameId.RidleyBreakupClawLeft, EnemyFrameId.RidleyBreakupClawRight),
        }.ToFrozenDictionary();

    /// <summary>The drag a breakup fragment's sideways speed loses each update, stopping at rest.</summary>
    internal static readonly SignedEightEight BreakupFragmentDrag = SignedEightEight.FromPixelFraction(1, 64);

    /// <summary>The gravity a breakup fragment's fall gains each update.</summary>
    internal static readonly SignedEightEight BreakupFragmentGravity = SignedEightEight.FromPixelFraction(1, 64);
}
