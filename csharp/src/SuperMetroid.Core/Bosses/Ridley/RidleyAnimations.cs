using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyBodyCueKind;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyBodySegment;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed body animations (#641, #1600), shared by Ceres and Lower Norfair.</summary>
internal static class RidleyAnimations
{
    /// <summary>How long the standing pose lasts before the animation rests.</summary>
    private const int StandingDuration = 12;
    /// <summary>The lunge's full leg extension.</summary>
    private const int LungeExtendedDuration = 80;
    /// <summary>The opening roar's open-mouth hold.</summary>
    private const int OpeningRoarDuration = 96;
    /// <summary>The death roar's open-mouth hold.</summary>
    private const int DeathRoarDuration = 16;
    /// <summary>The pause between the two fireball bursts.</summary>
    private const int BurstPauseDuration = 32;
    /// <summary>The open-mouth hold after a fireball burst.</summary>
    private const int BurstTailDuration = 48;
    /// <summary>Each lift-off pose after the jump.</summary>
    private const int LiftoffPoseDuration = 17;

    /// <summary>Every body segment's beats; each rests after its last, but the fireballing segments, which run on into their ends.</summary>
    internal static AnimationSegments<RidleyBodySegment, F, RidleyBodyCue> Segments { get; } = new(
        new Dictionary<RidleyBodySegment, AnimationBeat<F, RidleyBodyCue>[]>
    {
        [P.InitialFacingLeft] = [Show(F.RidleyBodyLeft, StandingDuration)],
        [P.InitialFacingRight] = [Show(F.RidleyBodyRight, StandingDuration)],
        [P.CeresLungeFacingLeft] = [.. Lunge(Side.Left, LungeExtendedDuration), .. Settle(Side.Left)],
        [P.CeresLungeFacingRight] = [.. Lunge(Side.Right, LungeExtendedDuration), .. Settle(Side.Right)],
        [P.RetrieveBabyMetroidFacingLeft] = [.. Lunge(Side.Left, 1)],
        [P.RetrieveBabyMetroidFacingRight] = [.. Lunge(Side.Right, 1)],
        [P.OpeningRoarFacingLeft] = OpeningRoarBeats(Side.Left),
        [P.OpeningRoarFacingRight] = OpeningRoarBeats(Side.Right),
        [P.DeathRoarFacingLeft] = DeathRoarBeats(Side.Left),
        [P.DeathRoarFacingRight] = DeathRoarBeats(Side.Right),
        [P.TurnFromLeftToRight] = Turn(F.RidleyBodyLeft, HorizontalDirection.Right, F.RidleyBodyRight),
        [P.TurnFromRightToLeft] = Turn(F.RidleyBodyRight, HorizontalDirection.Left, F.RidleyBodyLeft),
        [P.FireballingFacingLeft] = FireballingBeats(Side.Left, [Cue(K.AimFireball)]),
        [P.FireballingEndFacingLeft] = [Cue(K.ClearRoaringFlag), Show(F.RidleyBodyLeft, 1)],
        [P.FireballingFacingRight] = FireballingBeats(Side.Right, []),
        [P.FireballingEndFacingRight] = [Cue(K.ClearRoaringFlag), Show(F.RidleyBodyRight, 1)],
        [P.TransitionToFlyingFacingLeft] = TransitionToFlyingBeats(Side.Left, new(1, -12), new(-4, -8)),
        [P.TransitionToFlyingFacingRight] = TransitionToFlyingBeats(Side.Right, new(-1, -12), new(4, -8)),
    });

    /// <summary>The standing pose, which his owner starts as he is placed.</summary>
    internal static RidleyFacedAnimation Initial { get; } = new(P.InitialFacingLeft, P.InitialFacingRight);
    /// <summary>Ceres Ridley's lunge.</summary>
    internal static RidleyFacedAnimation CeresLunge { get; } = new(P.CeresLungeFacingLeft, P.CeresLungeFacingRight);
    /// <summary>Ceres Ridley reaching for the Baby Metroid.</summary>
    internal static RidleyFacedAnimation RetrieveBabyMetroid { get; } = new(P.RetrieveBabyMetroidFacingLeft, P.RetrieveBabyMetroidFacingRight);
    /// <summary>The opening roar.</summary>
    internal static RidleyFacedAnimation OpeningRoar { get; } = new(P.OpeningRoarFacingLeft, P.OpeningRoarFacingRight);
    /// <summary>The death roar.</summary>
    internal static RidleyFacedAnimation DeathRoar { get; } = new(P.DeathRoarFacingLeft, P.DeathRoarFacingRight);
    /// <summary>Two fireball bursts, each running on into its end.</summary>
    internal static RidleyFacedAnimation Fireballing { get; } = new(P.FireballingFacingLeft, P.FireballingFacingRight);
    /// <summary>The fireballing tail that closes the mouth, where Ceres Ridley cuts his fireballing short.</summary>
    internal static RidleyFacedAnimation FireballingEnd { get; } = new(P.FireballingEndFacingLeft, P.FireballingEndFacingRight);
    /// <summary>Taking off.</summary>
    internal static RidleyFacedAnimation TransitionToFlying { get; } = new(P.TransitionToFlyingFacingLeft, P.TransitionToFlyingFacingRight);

    /// <summary>
    /// The animation that starts at each segment: its segment played once, then a rest on its last frame; a fireballing segment
    /// runs on into its end, authored next, which rests.
    /// </summary>
    internal static IReadOnlyDictionary<RidleyBodySegment, Animation<RidleyBodySegment>> StartingAt { get; } = new Dictionary<RidleyBodySegment, Animation<RidleyBodySegment>>
    {
        [P.InitialFacingLeft] = PlaysOnceAndHolds(P.InitialFacingLeft),
        [P.InitialFacingRight] = PlaysOnceAndHolds(P.InitialFacingRight),
        [P.CeresLungeFacingLeft] = PlaysOnceAndHolds(P.CeresLungeFacingLeft),
        [P.CeresLungeFacingRight] = PlaysOnceAndHolds(P.CeresLungeFacingRight),
        [P.RetrieveBabyMetroidFacingLeft] = PlaysOnceAndHolds(P.RetrieveBabyMetroidFacingLeft),
        [P.RetrieveBabyMetroidFacingRight] = PlaysOnceAndHolds(P.RetrieveBabyMetroidFacingRight),
        [P.OpeningRoarFacingLeft] = PlaysOnceAndHolds(P.OpeningRoarFacingLeft),
        [P.OpeningRoarFacingRight] = PlaysOnceAndHolds(P.OpeningRoarFacingRight),
        [P.DeathRoarFacingLeft] = PlaysOnceAndHolds(P.DeathRoarFacingLeft),
        [P.DeathRoarFacingRight] = PlaysOnceAndHolds(P.DeathRoarFacingRight),
        [P.TurnFromLeftToRight] = PlaysOnceAndHolds(P.TurnFromLeftToRight),
        [P.TurnFromRightToLeft] = PlaysOnceAndHolds(P.TurnFromRightToLeft),
        [P.FireballingFacingLeft] = new([Whole(P.FireballingFacingLeft), Whole(P.FireballingEndFacingLeft)], AnimationEnd.Holds),
        [P.FireballingEndFacingLeft] = PlaysOnceAndHolds(P.FireballingEndFacingLeft),
        [P.FireballingFacingRight] = new([Whole(P.FireballingFacingRight), Whole(P.FireballingEndFacingRight)], AnimationEnd.Holds),
        [P.FireballingEndFacingRight] = PlaysOnceAndHolds(P.FireballingEndFacingRight),
        [P.TransitionToFlyingFacingLeft] = PlaysOnceAndHolds(P.TransitionToFlyingFacingLeft),
        [P.TransitionToFlyingFacingRight] = PlaysOnceAndHolds(P.TransitionToFlyingFacingRight),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RidleyBodySegment> StartingWith(RidleyBodySegment segment) => StartingAt[segment];

    /// <summary>A phase playing every beat of <paramref name="segment"/> once.</summary>
    private static AnimationPhase<RidleyBodySegment> Whole(RidleyBodySegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>The animation that plays <paramref name="segment"/> once and rests on its last frame.</summary>
    private static Animation<RidleyBodySegment> PlaysOnceAndHolds(RidleyBodySegment segment) => new([Whole(segment)], AnimationEnd.Holds);

    /// <summary>The poses of one facing.</summary>
    private readonly record struct Side(F Standing, F MouthHalfOpen, F MouthOpen, F LegsHalfExtended, F LegsExtended)
    {
        /// <summary>The left-facing poses.</summary>
        internal static Side Left { get; } = new(F.RidleyBodyLeft, F.RidleyBodyLeftMouthHalfOpen, F.RidleyBodyLeftMouthOpen, F.RidleyBodyLeftLegsHalfExtended, F.RidleyBodyLeftLegsExtended);
        /// <summary>The right-facing poses.</summary>
        internal static Side Right { get; } = new(F.RidleyBodyRight, F.RidleyBodyRightMouthHalfOpen, F.RidleyBodyRightMouthOpen, F.RidleyBodyRightLegsHalfExtended, F.RidleyBodyRightLegsExtended);
    }

    /// <summary>Extends the legs, holding the full extension <paramref name="extendedDuration"/> updates.</summary>
    private static IEnumerable<AnimationBeat<F, RidleyBodyCue>> Lunge(Side side, int extendedDuration)
    {
        yield return Feet(RidleyClawHeight.Standing);
        yield return Show(side.Standing, 4);
        yield return Feet(RidleyClawHeight.LegsHalfExtended);
        yield return Show(side.LegsHalfExtended, 6);
        yield return Feet(RidleyClawHeight.LegsExtended);
        yield return Show(side.LegsExtended, extendedDuration);
    }

    /// <summary>Draws the legs back in after a lunge, through half-extended to standing.</summary>
    private static IEnumerable<AnimationBeat<F, RidleyBodyCue>> Settle(Side side)
    {
        yield return Feet(RidleyClawHeight.LegsHalfExtended);
        yield return Show(side.LegsHalfExtended, 6);
        yield return Feet(RidleyClawHeight.Standing);
        yield return Show(side.Standing, 4);
    }

    /// <summary>The opening roar of one facing.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] OpeningRoarBeats(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, OpeningRoarDuration),
        Show(side.MouthHalfOpen, 8),
        Cue(K.ClearRoaringFlag),
        Show(side.Standing, 1),
    ];

    /// <summary>The death roar of one facing; it rests on the open mouth.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] DeathRoarBeats(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, DeathRoarDuration),
        Cue(K.ClearRoaringFlag),
    ];

    /// <summary>Turns from <paramref name="from"/> through the forward pose to <paramref name="to"/>.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] Turn(F from, HorizontalDirection flip, F to) =>
    [
        Cue(K.FaceForward),
        Show(from, 1),
        Show(F.RidleyBodyForward, 8),
        Cue(new RidleyBodyCue.Flip(flip)),
        Show(F.RidleyBodyForward, 1),
        Show(to, 1),
    ];

    /// <summary>
    /// Two fireball bursts of one facing, either of which the encounter that owns the body may cut short
    /// (<see cref="RidleyBodyCueKind.MayCutFireballingShort"/>); the left-facing burst aims once more before its second roar,
    /// and the animation runs on into its end.
    /// </summary>
    private static AnimationBeat<F, RidleyBodyCue>[] FireballingBeats(Side side, AnimationBeat<F, RidleyBodyCue>[] beforeSecondBurst)
    {
        var steps = new List<AnimationBeat<F, RidleyBodyCue>>
        {
            Cue(K.MayCutFireballingShort),
            Show(side.Standing, 8),
        };
        steps.AddRange(Burst(side));
        steps.Add(Cue(K.MayCutFireballingShort));
        steps.Add(Show(side.Standing, BurstPauseDuration));
        steps.AddRange(beforeSecondBurst);
        steps.AddRange(Burst(side));
        return [.. steps];
    }

    /// <summary>One roar and four fireballs, the first with its afterburn.</summary>
    private static IEnumerable<AnimationBeat<F, RidleyBodyCue>> Burst(Side side)
    {
        yield return Cue(K.Roar);
        yield return Show(side.MouthHalfOpen, 8);
        yield return Show(side.MouthOpen, 2);
        for (int fireball = 0; fireball < 4; fireball++)
        {
            yield return Cue(K.AimFireball);
            yield return Cue(fireball == 0 ? K.SpawnFireballWithAfterburn : K.SpawnFireballWithoutAfterburn);
            yield return Show(side.MouthOpen, fireball == 3 ? BurstTailDuration : 5);
        }
        yield return Show(side.MouthHalfOpen, 8);
    }

    /// <summary>The crouch, two hops and lift-off of one facing; the encounter that owns the body handles the lift-off (#2230).</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] TransitionToFlyingBeats(
        Side side, Offset16<RoomPixels> firstHop, Offset16<RoomPixels> secondHop) =>
    [
        Show(side.Standing, 3),
        Cue(new RidleyBodyCue.Move(Displacement: firstHop)),
        Show(side.LegsHalfExtended, 4),
        Cue(new RidleyBodyCue.Move(Displacement: secondHop)),
        Show(side.LegsExtended, 5),
        Cue(K.LiftOff),
        Show(side.LegsHalfExtended, LiftoffPoseDuration),
        Show(side.Standing, LiftoffPoseDuration),
    ];

    /// <summary>Selects claw height <paramref name="height"/>.</summary>
    private static AnimationBeat<F, RidleyBodyCue> Feet(RidleyClawHeight height) =>
        Cue(new RidleyBodyCue.SetClawHeight(height));

    /// <summary>Publishes a cue of <paramref name="kind"/> that carries no values.</summary>
    private static AnimationBeat<F, RidleyBodyCue> Cue(RidleyBodyCueKind kind) => Cue(new RidleyBodyCue.Signal(kind));

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<F, RidleyBodyCue> Cue(RidleyBodyCue cue) =>
        new AnimationBeat<F, RidleyBodyCue>.Event(cue);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<F, RidleyBodyCue> Show(F frame, int duration) =>
        new AnimationBeat<F, RidleyBodyCue>.Frame(frame, duration);
}
