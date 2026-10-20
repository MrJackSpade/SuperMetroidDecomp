using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Bosses.Ridley.RidleyCollisionDefinitions.BodyFrame;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyBodyCueKind;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyBodyProgram;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed body programs (#641), shared by Ceres and Lower Norfair, in place of their bank-$A6 lists.</summary>
internal static class RidleyAnimations
{
    /// <summary>How long the standing pose lasts before the program sleeps ($A6:E53C).</summary>
    private const int StandingDuration = 12;
    /// <summary>The lunge's full leg extension ($A6:E560).</summary>
    private const int LungeExtendedDuration = 80;
    /// <summary>The opening roar's open-mouth hold ($A6:E69E).</summary>
    private const int OpeningRoarDuration = 96;
    /// <summary>The death roar's open-mouth hold ($A6:E6D6).</summary>
    private const int DeathRoarDuration = 16;
    /// <summary>The pause between the two fireball bursts ($A6:E778).</summary>
    private const int BurstPauseDuration = 32;
    /// <summary>The open-mouth hold after a fireball burst ($A6:E76C).</summary>
    private const int BurstTailDuration = 48;
    /// <summary>Each lift-off pose after the jump ($A6:E93B/E93F).</summary>
    private const int LiftoffPoseDuration = 17;

    /// <summary>The claw-height pose for standing legs.</summary>
    private const int FeetStanding = 0;
    /// <summary>The claw-height pose for half-extended legs.</summary>
    private const int FeetHalfExtended = 1;
    /// <summary>The claw-height pose for extended legs.</summary>
    private const int FeetExtended = 2;

    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<RidleyBodyProgram, F, RidleyBodyCue> Programs { get; } = new(
        new Dictionary<RidleyBodyProgram, AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[]>
    {
        [P.Initial] = [UnlessFacingLeft(P.InitialFacingRight), Show(F.Left, StandingDuration), Hold],
        [P.InitialFacingRight] = [Show(F.Right, StandingDuration), Hold],
        [P.CeresLunge] = [UnlessFacingLeft(P.CeresLungeFacingRight), .. Lunge(Side.Left, LungeExtendedDuration, settle: true), Hold],
        [P.CeresLungeFacingRight] = [.. Lunge(Side.Right, LungeExtendedDuration, settle: true), Hold],
        [P.RetrieveBabyMetroid] = [UnlessFacingLeft(P.RetrieveBabyMetroidFacingRight), .. Lunge(Side.Left, 1, settle: false), Hold],
        [P.RetrieveBabyMetroidFacingRight] = [.. Lunge(Side.Right, 1, settle: false), Hold],
        [P.OpeningRoar] = [UnlessFacingLeft(P.OpeningRoarFacingRight), .. OpeningRoar(Side.Left)],
        [P.OpeningRoarFacingRight] = OpeningRoar(Side.Right),
        [P.DeathRoar] = [UnlessFacingLeft(P.DeathRoarFacingRight), .. DeathRoar(Side.Left)],
        [P.DeathRoarFacingRight] = DeathRoar(Side.Right),
        [P.TurnFromLeftToRight] = Turn(F.Left, K.FlipRight, F.Right),
        [P.TurnFromRightToLeft] = Turn(F.Right, K.FlipLeft, F.Left),
        [P.Fireballing] =
        [
            UnlessFacingLeft(P.FireballingFacingRight),
            .. Fireballing(Side.Left, P.FireballingEnd, aimBeforeSecondRoar: true),
        ],
        [P.FireballingEnd] = [Cue(K.ClearRoaringFlag), Show(F.Left, 1), Hold],
        [P.FireballingFacingRight] = Fireballing(Side.Right, P.FireballingEndFacingRight, aimBeforeSecondRoar: false),
        [P.FireballingEndFacingRight] = [Cue(K.ClearRoaringFlag), Show(F.Right, 1), Hold],
        [P.TransitionToFlying] =
        [
            UnlessFacingLeft(P.TransitionToFlyingFacingRight),
            .. TransitionToFlying(Side.Left, new(1, -12), new(-4, -8), K.CeresStartLiftoff),
        ],
        [P.TransitionToFlyingFacingRight] = TransitionToFlying(Side.Right, new(-1, -12), new(4, -8), K.StartLiftoff),
    });

    /// <summary>The poses of one facing.</summary>
    private readonly record struct Side(F Standing, F MouthHalfOpen, F MouthOpen, F LegsHalfExtended, F LegsExtended)
    {
        /// <summary>The left-facing poses.</summary>
        internal static Side Left { get; } = new(F.Left, F.LeftMouthHalfOpen, F.LeftMouthOpen, F.LeftLegsHalfExtended, F.LeftLegsExtended);
        /// <summary>The right-facing poses.</summary>
        internal static Side Right { get; } = new(F.Right, F.RightMouthHalfOpen, F.RightMouthOpen, F.RightLegsHalfExtended, F.RightLegsExtended);
    }

    /// <summary>Extends the legs, holding the full extension <paramref name="extendedDuration"/> updates, then optionally settles back.</summary>
    private static IEnumerable<AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>> Lunge(Side side, int extendedDuration, bool settle)
    {
        yield return Feet(FeetStanding);
        yield return Show(side.Standing, 4);
        yield return Feet(FeetHalfExtended);
        yield return Show(side.LegsHalfExtended, 6);
        yield return Feet(FeetExtended);
        yield return Show(side.LegsExtended, extendedDuration);
        if (!settle)
            yield break;
        yield return Feet(FeetHalfExtended);
        yield return Show(side.LegsHalfExtended, 6);
        yield return Feet(FeetStanding);
        yield return Show(side.Standing, 4);
    }

    /// <summary>The opening roar of one facing.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[] OpeningRoar(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, OpeningRoarDuration),
        Show(side.MouthHalfOpen, 8),
        Cue(K.ClearRoaringFlag),
        Show(side.Standing, 1),
        Hold,
    ];

    /// <summary>The death roar of one facing; it sleeps on the open mouth.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[] DeathRoar(Side side) =>
    [
        Show(side.Standing, 6),
        Cue(K.Roar),
        Show(side.MouthHalfOpen, 8),
        Show(side.MouthOpen, DeathRoarDuration),
        Cue(K.ClearRoaringFlag),
        Hold,
    ];

    /// <summary>Turns from <paramref name="from"/> through the forward pose to <paramref name="to"/>.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[] Turn(F from, RidleyBodyCueKind flip, F to) =>
    [
        Cue(K.FaceForward),
        Show(from, 1),
        Show(F.Forward, 8),
        Cue(flip),
        Show(F.Forward, 1),
        Show(to, 1),
        Hold,
    ];

    /// <summary>
    /// Two fireball bursts of one facing, either cut short to <paramref name="end"/> in Ceres when Samus is low on
    /// energy; the left-facing list aims once more before its second roar.
    /// </summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[] Fireballing(Side side, RidleyBodyProgram end, bool aimBeforeSecondRoar)
    {
        var steps = new List<AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>>
        {
            IfCeresAndSamusLowEnergy(end),
            Show(side.Standing, 8),
        };
        steps.AddRange(Burst(side));
        steps.Add(IfCeresAndSamusLowEnergy(end));
        steps.Add(Show(side.Standing, BurstPauseDuration));
        if (aimBeforeSecondRoar)
            steps.Add(Cue(K.AimFireball));
        steps.AddRange(Burst(side));
        steps.Add(FallInto(end));
        return [.. steps];
    }

    /// <summary>One roar and four fireballs, the first with its afterburn.</summary>
    private static IEnumerable<AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>> Burst(Side side)
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
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>[] TransitionToFlying(
        Side side, Offset16<RoomPixels> firstHop, Offset16<RoomPixels> secondHop, RidleyBodyCueKind liftoff) =>
    [
        Show(side.Standing, 3),
        Cue(new RidleyBodyCue(K.Move, Displacement: firstHop)),
        Show(side.LegsHalfExtended, 4),
        Cue(new RidleyBodyCue(K.Move, Displacement: secondHop)),
        Show(side.LegsExtended, 5),
        Cue(liftoff),
        Show(side.LegsHalfExtended, LiftoffPoseDuration),
        Show(side.Standing, LiftoffPoseDuration),
        Hold,
    ];

    /// <summary>Redirects to <paramref name="target"/> unless Ridley faces left.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> UnlessFacingLeft(RidleyBodyProgram target) =>
        Cue(new RidleyBodyCue(K.GoToUnlessFacingLeft, Target: target));

    /// <summary>Redirects to <paramref name="target"/> in Ceres when Samus is low on energy.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> IfCeresAndSamusLowEnergy(RidleyBodyProgram target) =>
        Cue(new RidleyBodyCue(K.GoToIfCeresAndSamusLowEnergy, Target: target));

    /// <summary>Selects the claw-height pose <paramref name="index"/>.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> Feet(int index) =>
        Cue(new RidleyBodyCue(K.SetFeetDistance, FeetDistanceIndex: index));

    /// <summary>Publishes a cue of <paramref name="kind"/> without operands.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> Cue(RidleyBodyCueKind kind) => Cue(new RidleyBodyCue(kind));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> Cue(RidleyBodyCue cue) =>
        AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>.Publish(cue);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> Show(F frame, int duration) =>
        AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>.Show(frame, duration);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> FallInto(RidleyBodyProgram program) =>
        AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue> Hold =>
        AnimationProgramStep<RidleyBodyProgram, F, RidleyBodyCue>.Hold;
}
