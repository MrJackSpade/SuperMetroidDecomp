using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Bosses.Ridley.RidleyCollisionDefinitions.BodyFrame;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyBodyCueKind;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyBodySegment;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed body animations (#641, #1600), shared by Ceres and Lower Norfair.</summary>
internal static class RidleyAnimations
{
    /// <summary>How long the standing pose lasts before the animation sleeps.</summary>
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

    /// <summary>Every body segment's beats; each sleeps after its last, but the fireballing segments, which run on into their ends.</summary>
    internal static AnimationSegments<RidleyBodySegment, F, RidleyBodyCue> Segments { get; } = new(
        new Dictionary<RidleyBodySegment, AnimationBeat<F, RidleyBodyCue>[]>
    {
        [P.Initial] = [UnlessFacingLeft(P.InitialFacingRight), Show(F.Left, StandingDuration)],
        [P.InitialFacingRight] = [Show(F.Right, StandingDuration)],
        [P.CeresLunge] = [UnlessFacingLeft(P.CeresLungeFacingRight), .. Lunge(Side.Left, LungeExtendedDuration), .. Settle(Side.Left)],
        [P.CeresLungeFacingRight] = [.. Lunge(Side.Right, LungeExtendedDuration), .. Settle(Side.Right)],
        [P.RetrieveBabyMetroid] = [UnlessFacingLeft(P.RetrieveBabyMetroidFacingRight), .. Lunge(Side.Left, 1)],
        [P.RetrieveBabyMetroidFacingRight] = [.. Lunge(Side.Right, 1)],
        [P.OpeningRoar] = [UnlessFacingLeft(P.OpeningRoarFacingRight), .. OpeningRoar(Side.Left)],
        [P.OpeningRoarFacingRight] = OpeningRoar(Side.Right),
        [P.DeathRoar] = [UnlessFacingLeft(P.DeathRoarFacingRight), .. DeathRoar(Side.Left)],
        [P.DeathRoarFacingRight] = DeathRoar(Side.Right),
        [P.TurnFromLeftToRight] = Turn(F.Left, K.FlipRight, F.Right),
        [P.TurnFromRightToLeft] = Turn(F.Right, K.FlipLeft, F.Left),
        [P.Fireballing] =
        [
            UnlessFacingLeft(P.FireballingFacingRight),
            .. Fireballing(Side.Left, P.FireballingEnd, [Cue(K.AimFireball)]),
        ],
        [P.FireballingEnd] = [Cue(K.ClearRoaringFlag), Show(F.Left, 1)],
        [P.FireballingFacingRight] = Fireballing(Side.Right, P.FireballingEndFacingRight, []),
        [P.FireballingEndFacingRight] = [Cue(K.ClearRoaringFlag), Show(F.Right, 1)],
        [P.TransitionToFlying] =
        [
            UnlessFacingLeft(P.TransitionToFlyingFacingRight),
            .. TransitionToFlying(Side.Left, new(1, -12), new(-4, -8), K.CeresStartLiftoff),
        ],
        [P.TransitionToFlyingFacingRight] = TransitionToFlying(Side.Right, new(-1, -12), new(4, -8), K.StartLiftoff),
    });

    /// <summary>
    /// The animation that starts at each segment: its segment played once, then a sleep on its last frame; a fireballing segment
    /// runs on into its end, authored next, which sleeps.
    /// </summary>
    internal static IReadOnlyDictionary<RidleyBodySegment, Animation<RidleyBodySegment>> StartingAt { get; } = new Dictionary<RidleyBodySegment, Animation<RidleyBodySegment>>
    {
        [P.Initial] = Sleeps(P.Initial),
        [P.InitialFacingRight] = Sleeps(P.InitialFacingRight),
        [P.CeresLunge] = Sleeps(P.CeresLunge),
        [P.CeresLungeFacingRight] = Sleeps(P.CeresLungeFacingRight),
        [P.RetrieveBabyMetroid] = Sleeps(P.RetrieveBabyMetroid),
        [P.RetrieveBabyMetroidFacingRight] = Sleeps(P.RetrieveBabyMetroidFacingRight),
        [P.OpeningRoar] = Sleeps(P.OpeningRoar),
        [P.OpeningRoarFacingRight] = Sleeps(P.OpeningRoarFacingRight),
        [P.DeathRoar] = Sleeps(P.DeathRoar),
        [P.DeathRoarFacingRight] = Sleeps(P.DeathRoarFacingRight),
        [P.TurnFromLeftToRight] = Sleeps(P.TurnFromLeftToRight),
        [P.TurnFromRightToLeft] = Sleeps(P.TurnFromRightToLeft),
        [P.Fireballing] = new([Whole(P.Fireballing), Whole(P.FireballingEnd)], AnimationEnd.Holds),
        [P.FireballingEnd] = Sleeps(P.FireballingEnd),
        [P.FireballingFacingRight] = new([Whole(P.FireballingFacingRight), Whole(P.FireballingEndFacingRight)], AnimationEnd.Holds),
        [P.FireballingEndFacingRight] = Sleeps(P.FireballingEndFacingRight),
        [P.TransitionToFlying] = Sleeps(P.TransitionToFlying),
        [P.TransitionToFlyingFacingRight] = Sleeps(P.TransitionToFlyingFacingRight),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RidleyBodySegment> StartingWith(RidleyBodySegment segment) => StartingAt[segment];

    /// <summary>A phase playing every beat of <paramref name="segment"/> once.</summary>
    private static AnimationPhase<RidleyBodySegment> Whole(RidleyBodySegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>The animation that plays <paramref name="segment"/> once and sleeps on its last frame.</summary>
    private static Animation<RidleyBodySegment> Sleeps(RidleyBodySegment segment) => new([Whole(segment)], AnimationEnd.Holds);

    /// <summary>The poses of one facing.</summary>
    private readonly record struct Side(F Standing, F MouthHalfOpen, F MouthOpen, F LegsHalfExtended, F LegsExtended)
    {
        /// <summary>The left-facing poses.</summary>
        internal static Side Left { get; } = new(F.Left, F.LeftMouthHalfOpen, F.LeftMouthOpen, F.LeftLegsHalfExtended, F.LeftLegsExtended);
        /// <summary>The right-facing poses.</summary>
        internal static Side Right { get; } = new(F.Right, F.RightMouthHalfOpen, F.RightMouthOpen, F.RightLegsHalfExtended, F.RightLegsExtended);
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
    private static AnimationBeat<F, RidleyBodyCue>[] OpeningRoar(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, OpeningRoarDuration),
        Show(side.MouthHalfOpen, 8),
        Cue(K.ClearRoaringFlag),
        Show(side.Standing, 1),
    ];

    /// <summary>The death roar of one facing; it sleeps on the open mouth.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] DeathRoar(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, DeathRoarDuration),
        Cue(K.ClearRoaringFlag),
    ];

    /// <summary>Turns from <paramref name="from"/> through the forward pose to <paramref name="to"/>.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] Turn(F from, RidleyBodyCueKind flip, F to) =>
    [
        Cue(K.FaceForward),
        Show(from, 1),
        Show(F.Forward, 8),
        Cue(flip),
        Show(F.Forward, 1),
        Show(to, 1),
    ];

    /// <summary>
    /// Two fireball bursts of one facing, either cut short to <paramref name="end"/> in Ceres when Samus is low on
    /// energy; the left-facing burst aims once more before its second roar, and the animation runs on into its end.
    /// </summary>
    private static AnimationBeat<F, RidleyBodyCue>[] Fireballing(Side side, RidleyBodySegment end,
        AnimationBeat<F, RidleyBodyCue>[] beforeSecondBurst)
    {
        var steps = new List<AnimationBeat<F, RidleyBodyCue>>
        {
            IfCeresAndSamusLowEnergy(end),
            Show(side.Standing, 8),
        };
        steps.AddRange(Burst(side));
        steps.Add(IfCeresAndSamusLowEnergy(end));
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

    /// <summary>The crouch, two hops and lift-off of one facing.</summary>
    private static AnimationBeat<F, RidleyBodyCue>[] TransitionToFlying(
        Side side, Offset16<RoomPixels> firstHop, Offset16<RoomPixels> secondHop, RidleyBodyCueKind liftoff) =>
    [
        Show(side.Standing, 3),
        Cue(new RidleyBodyCue.Move(Displacement: firstHop)),
        Show(side.LegsHalfExtended, 4),
        Cue(new RidleyBodyCue.Move(Displacement: secondHop)),
        Show(side.LegsExtended, 5),
        Cue(liftoff),
        Show(side.LegsHalfExtended, LiftoffPoseDuration),
        Show(side.Standing, LiftoffPoseDuration),
    ];

    /// <summary>Redirects to <paramref name="target"/> unless Ridley faces left.</summary>
    private static AnimationBeat<F, RidleyBodyCue> UnlessFacingLeft(RidleyBodySegment target) =>
        Cue(new RidleyBodyCue.GoToUnlessFacingLeft(Target: target));

    /// <summary>Redirects to <paramref name="target"/> in Ceres when Samus is low on energy.</summary>
    private static AnimationBeat<F, RidleyBodyCue> IfCeresAndSamusLowEnergy(RidleyBodySegment target) =>
        Cue(new RidleyBodyCue.GoToIfCeresAndSamusLowEnergy(Target: target));

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
