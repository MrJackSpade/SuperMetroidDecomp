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
    /// selections: this one, 0 to 3 sixteenths of a pixel per frame, and <see cref="FragmentSpeedWholePixel"/>.
    /// </summary>
    internal static readonly RandomSampleField FragmentSpeedSixteenths = new(4, 2);
    /// <summary>The selection of a whole pixel per frame or none that a fragment's horizontal speed adds.</summary>
    internal static readonly RandomSampleField FragmentSpeedWholePixel = new(8, 1);
    /// <summary>
    /// The tail tip's breakup pose for each sixteenth of the turn its direction can point into, in turn order: each pose's
    /// sixteenth is centred on it, so the first opens half a pose before straight down.
    /// </summary>
    internal static readonly TurnArcs<EnemyFrameId> TailTipBreakupFrames = new(TrigonometryAngle.Wrapped(-8),
        EnemyFrameId.RidleyTailTipPointingToward180, EnemyFrameId.RidleyTailTipPointingToward157, EnemyFrameId.RidleyTailTipPointingToward135, EnemyFrameId.RidleyTailTipPointingToward112,
        EnemyFrameId.RidleyTailTipPointingToward90, EnemyFrameId.RidleyTailTipPointingToward67, EnemyFrameId.RidleyTailTipPointingToward45, EnemyFrameId.RidleyTailTipPointingToward22,
        EnemyFrameId.RidleyTailTipPointingToward0, EnemyFrameId.RidleyTailTipPointingToward337, EnemyFrameId.RidleyTailTipPointingToward315, EnemyFrameId.RidleyTailTipPointingToward292,
        EnemyFrameId.RidleyTailTipPointingToward270, EnemyFrameId.RidleyTailTipPointingToward247, EnemyFrameId.RidleyTailTipPointingToward225, EnemyFrameId.RidleyTailTipPointingToward202);

    /// <summary>
    /// The breakup's spawn order: the tail from tip to base, then wings, legs, torso, head and claw. The places the
    /// pieces take and the draw order make this sequence observable.
    /// </summary>
    internal static readonly RidleyExplosionPart[] BreakupOrder =
    [
        RidleyExplosionPart.TailTip, RidleyExplosionPart.TailSixthSegment, RidleyExplosionPart.TailFifthSegment, RidleyExplosionPart.TailFourthSegment,
        RidleyExplosionPart.TailThirdSegment, RidleyExplosionPart.TailSecondSegment, RidleyExplosionPart.TailBaseSegment, RidleyExplosionPart.Wings,
        RidleyExplosionPart.Legs, RidleyExplosionPart.Torso, RidleyExplosionPart.OpenHeadAndNeck, RidleyExplosionPart.Claw,
    ];

    /// <summary>
    /// The updates each part lasts, dying on the last: tail fragments die eight updates apart from the base, the body parts
    /// follow the same eight-update progression, and the torso dies last.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, int> LifetimeUpdates = new Dictionary<RidleyExplosionPart, int>
    {
        [RidleyExplosionPart.TailBaseSegment] = 73,
        [RidleyExplosionPart.TailSecondSegment] = 81,
        [RidleyExplosionPart.TailThirdSegment] = 89,
        [RidleyExplosionPart.TailFourthSegment] = 97,
        [RidleyExplosionPart.TailFifthSegment] = 105,
        [RidleyExplosionPart.TailSixthSegment] = 113,
        [RidleyExplosionPart.TailTip] = 121,
        [RidleyExplosionPart.Wings] = 41,
        [RidleyExplosionPart.Legs] = 49,
        [RidleyExplosionPart.OpenHeadAndNeck] = 57,
        [RidleyExplosionPart.Claw] = 65,
        [RidleyExplosionPart.Torso] = 129,
    }.ToFrozenDictionary();

    /// <summary>The tail fragment each link of the tail becomes, from the base to the tip (#2238).</summary>
    internal static readonly IReadOnlyList<RidleyExplosionPart> TailLinkParts =
    [
        RidleyExplosionPart.TailBaseSegment, RidleyExplosionPart.TailSecondSegment, RidleyExplosionPart.TailThirdSegment,
        RidleyExplosionPart.TailFourthSegment, RidleyExplosionPart.TailFifthSegment, RidleyExplosionPart.TailSixthSegment,
        RidleyExplosionPart.TailTip,
    ];

    /// <summary>The definition of <paramref name="part"/>.</summary>
    public static RidleyExplosionPartDefinition GetPart(RidleyExplosionPart part) => new(LifetimeUpdates[part]);

    /// <summary>The tail pose each of the first six tail fragments shows.</summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, EnemyFrameId> TailFrames = new Dictionary<RidleyExplosionPart, EnemyFrameId>
    {
        [RidleyExplosionPart.TailBaseSegment] = EnemyFrameId.RidleyTailBaseLink,
        [RidleyExplosionPart.TailSecondSegment] = EnemyFrameId.RidleyTailBaseLink,
        [RidleyExplosionPart.TailThirdSegment] = EnemyFrameId.RidleyTailMiddleLink,
        [RidleyExplosionPart.TailFourthSegment] = EnemyFrameId.RidleyTailMiddleLink,
        [RidleyExplosionPart.TailFifthSegment] = EnemyFrameId.RidleyTailOuterLink,
        [RidleyExplosionPart.TailSixthSegment] = EnemyFrameId.RidleyTailOuterLink,
    }.ToFrozenDictionary();

    /// <summary>
    /// The body-relative position while Ridley faces left and the pose each way of each body fragment, loaded by the
    /// breakup setup; a right-facing placement mirrors the left-facing one's X.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, RidleyExplosionBodyPartDefinition> BodyParts =
        new Dictionary<RidleyExplosionPart, RidleyExplosionBodyPartDefinition>
        {
            [RidleyExplosionPart.Wings] = new(new(0, 0), EnemyFrameId.RidleyWingFacingLeftRaised, EnemyFrameId.RidleyWingFacingRightRaised),
            [RidleyExplosionPart.Legs] = new(new(15, 22), EnemyFrameId.RidleyBreakupLegsLeft, EnemyFrameId.RidleyBreakupLegsRight),
            [RidleyExplosionPart.OpenHeadAndNeck] = new(new(-3, -24), EnemyFrameId.RidleyBreakupOpenHeadAndNeckLeft, EnemyFrameId.RidleyBreakupOpenHeadAndNeckRight),
            [RidleyExplosionPart.Torso] = new(new(16, 0), EnemyFrameId.RidleyBreakupTorsoLeft, EnemyFrameId.RidleyBreakupTorsoRight),
            [RidleyExplosionPart.Claw] = new(new(8, 7), EnemyFrameId.RidleyBreakupClawLeft, EnemyFrameId.RidleyBreakupClawRight),
        }.ToFrozenDictionary();

    /// <summary>The drag a breakup fragment's sideways speed loses each update, stopping at rest.</summary>
    internal static readonly SignedEightEight BreakupFragmentDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>The gravity a breakup fragment's fall gains each update.</summary>
    internal static readonly SignedEightEight BreakupFragmentGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);
}
