using SuperMetroid.Core.Enemies.Artwork;
using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Lower Norfair Ridley's twelve death-breakup actors fly apart: their speeds, frames, order and lifetimes. The
/// selected animations remain authored data.
/// </summary>
internal static class RidleyExplosionDefinitions
{
    /// <summary>
    /// The breakup initializer draws a fragment's horizontal speed from one random value as the sum of two
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
    /// Each part's lifetime, the count its countdown (<see cref="BossCountdown"/>) is loaded with, so the fragment dies on
    /// the update after that many: tail fragments die eight updates apart from the base, the body parts follow the same
    /// eight-update progression, and the torso dies last.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, int> Lifetimes = new Dictionary<RidleyExplosionPart, int>
    {
        [RidleyExplosionPart.TailBaseSegment] = 72,
        [RidleyExplosionPart.TailSecondSegment] = 80,
        [RidleyExplosionPart.TailThirdSegment] = 88,
        [RidleyExplosionPart.TailFourthSegment] = 96,
        [RidleyExplosionPart.TailFifthSegment] = 104,
        [RidleyExplosionPart.TailSixthSegment] = 112,
        [RidleyExplosionPart.TailTip] = 120,
        [RidleyExplosionPart.Wings] = 40,
        [RidleyExplosionPart.Legs] = 48,
        [RidleyExplosionPart.OpenHeadAndNeck] = 56,
        [RidleyExplosionPart.Claw] = 64,
        [RidleyExplosionPart.Torso] = 128,
    }.ToFrozenDictionary();

    /// <summary>The tail fragment each link of the tail becomes, from the base to the tip (#2238).</summary>
    internal static readonly IReadOnlyList<RidleyExplosionPart> TailLinkParts =
    [
        RidleyExplosionPart.TailBaseSegment, RidleyExplosionPart.TailSecondSegment, RidleyExplosionPart.TailThirdSegment,
        RidleyExplosionPart.TailFourthSegment, RidleyExplosionPart.TailFifthSegment, RidleyExplosionPart.TailSixthSegment,
        RidleyExplosionPart.TailTip,
    ];

    /// <summary>The definition of <paramref name="part"/>.</summary>
    public static RidleyExplosionPartDefinition GetPart(RidleyExplosionPart part) => new(Lifetimes[part]);

    /// <summary>
    /// Returns one of the ten body-relative positions the small explosions near Ridley
    /// consume cyclically (shared with the Baby Metroid death).
    /// </summary>
    public static Offset16<RoomPixels> DeathExplosionPlacement(int index) =>
        DeathExplosionScatter.Offset(index);

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
    /// The body-relative position and pose of each body fragment by facing, loaded by the breakup initializer; a
    /// right-facing placement mirrors the left-facing one's X.
    /// </summary>
    internal static readonly FrozenDictionary<RidleyExplosionPartFacing, RidleyExplosionBodyPartDefinition> BodyParts =
        new Dictionary<RidleyExplosionPartFacing, RidleyExplosionBodyPartDefinition>
        {
            [new(RidleyExplosionPart.Wings, false)] = new(new(0, 0), EnemyFrameId.RidleyWingFacingLeftRaised),
            [new(RidleyExplosionPart.Wings, true)] = new(new(0, 0), EnemyFrameId.RidleyWingFacingRightRaised),
            [new(RidleyExplosionPart.Legs, false)] = new(new(15, 22), EnemyFrameId.RidleyBreakupLegsLeft),
            [new(RidleyExplosionPart.Legs, true)] = new(new(-15, 22), EnemyFrameId.RidleyBreakupLegsRight),
            [new(RidleyExplosionPart.OpenHeadAndNeck, false)] = new(new(-3, -24), EnemyFrameId.RidleyBreakupOpenHeadAndNeckLeft),
            [new(RidleyExplosionPart.OpenHeadAndNeck, true)] = new(new(3, -24), EnemyFrameId.RidleyBreakupOpenHeadAndNeckRight),
            [new(RidleyExplosionPart.Torso, false)] = new(new(16, 0), EnemyFrameId.RidleyBreakupTorsoLeft),
            [new(RidleyExplosionPart.Torso, true)] = new(new(-16, 0), EnemyFrameId.RidleyBreakupTorsoRight),
            [new(RidleyExplosionPart.Claw, false)] = new(new(8, 7), EnemyFrameId.RidleyBreakupClawLeft),
            [new(RidleyExplosionPart.Claw, true)] = new(new(-8, 7), EnemyFrameId.RidleyBreakupClawRight),
        }.ToFrozenDictionary();

    /// <summary>The drag a breakup fragment's sideways speed loses each update, stopping at rest.</summary>
    internal static readonly SignedEightEight BreakupFragmentDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>The gravity a breakup fragment's fall gains each update.</summary>
    internal static readonly SignedEightEight BreakupFragmentGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);
}
