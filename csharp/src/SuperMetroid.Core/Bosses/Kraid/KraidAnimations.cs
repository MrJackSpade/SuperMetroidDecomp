using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartSegment;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's typed part animations (#641, #1600, #1885): each foot, arm, lint and fingernail segment's beats, each beat naming
/// the pose it shows (#2210): an installed picture, the foot's step from where its walk first shows that picture, and the
/// depth of the parts Kraid shows while he rises; and the animation starting at each, which plays its segments whole. A
/// pose the foot shows again straight after itself is one beat of both durations (#2296): nothing reads a part's frame timer
/// or new-frame flag, so the second showing changed nothing.
/// </summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail segment's beats.</summary>
    internal static AnimationSegments<KraidPartSegment, KraidPartPose, KraidPartCue> Segments { get; } = new(
        new Dictionary<KraidPartSegment, AnimationBeat<KraidPartPose, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            HoldSunk(F.KraidFootPlanted),
        ],
        [P.FootNeutral] =
        [
            Hold(F.KraidFootPlanted),
        ],
        [P.FootWalkingForward] =
        [
            Show(F.KraidFootPlanted, 4),
            Step(F.KraidFootPlanted, new(-2, -1), 4),
            Step(F.KraidFootPlanted, new(-4, -2), 4),
            Step(F.KraidFootPlanted, new(-6, -3), 4),
            Step(F.KraidFootPlanted, new(-8, -4), 4),
            Show(F.KraidFootLifting, 4),
            Step(F.KraidFootLifting, new(-2, -1), 4),
            Step(F.KraidFootLifting, new(-4, -2), 4),
            Step(F.KraidFootLifting, new(-6, -3), 4),
            Step(F.KraidFootLifting, new(-8, -4), 4),
            Show(F.KraidFootRaised, 4),
            Show(F.KraidFootSwinging, 3),
            Step(F.KraidFootSwinging, new(-2, 2), 1),
            Step(F.KraidFootSwinging, new(-4, 4), 1),
            Step(F.KraidFootSwinging, new(-6, 6), 1),
            Step(F.KraidFootSwinging, new(-8, 8), 1),
            Step(F.KraidFootSwinging, new(-10, 10), 1),
            Step(F.KraidFootSwinging, new(-12, 12), 16),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReaching, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootReaching, new(2, 0), 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootReaching, new(2, 0), 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootRaised, new(-6, 11), 4),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootRaised, new(-4, 11), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-12, 5), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-10, 5), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Step(F.KraidFootLifting, new(-8, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-6, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-4, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-2, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(0, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(2, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootPlanted, new(-6, 0), 1),
            Show(F.KraidFootPlanted, 1),
        ],
        [P.FootLungeForward] =
        [
            Show(F.KraidFootPlanted, 1),
            Step(F.KraidFootPlanted, new(-2, -1), 1),
            Step(F.KraidFootPlanted, new(-4, -2), 1),
            Step(F.KraidFootPlanted, new(-6, -3), 1),
            Step(F.KraidFootPlanted, new(-8, -4), 1),
            Show(F.KraidFootLifting, 1),
            Step(F.KraidFootLifting, new(-2, -1), 1),
            Step(F.KraidFootLifting, new(-4, -2), 1),
            Step(F.KraidFootLifting, new(-6, -3), 1),
            Step(F.KraidFootLifting, new(-8, -4), 1),
            Show(F.KraidFootRaised, 1),
            Show(F.KraidFootSwinging, 1),
            Step(F.KraidFootSwinging, new(-2, 2), 1),
            Step(F.KraidFootSwinging, new(-4, 4), 1),
            Step(F.KraidFootSwinging, new(-6, 6), 1),
            Step(F.KraidFootSwinging, new(-8, 8), 1),
            Step(F.KraidFootSwinging, new(-10, 10), 1),
            Step(F.KraidFootSwinging, new(-12, 12), 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReaching, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootReaching, new(2, 0), 2),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootReaching, new(2, 0), 2),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Step(F.KraidFootRaised, new(-6, 11), 2),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootRaised, new(-4, 11), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-12, 5), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-10, 5), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Step(F.KraidFootLifting, new(-8, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-6, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-4, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(-2, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(0, 5), 1),
            Cue(K.StepLeft),
            Step(F.KraidFootLifting, new(2, 5), 1),
            Step(F.KraidFootPlanted, new(-6, 0), 1),
            Cue(K.StepLeft),
            Show(F.KraidFootPlanted, 1),
        ],
        [P.FootWalkingBackward] =
        [
            Cue(K.StepRight),
            Show(F.KraidFootPlanted, 4),
            Cue(K.StepRight),
            Step(F.KraidFootPlanted, new(-4, 0), 1),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(2, 5), 1),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(0, 5), 1),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-2, 5), 1),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-4, 5), 1),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-6, 5), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-8, 5), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-10, 5), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Step(F.KraidFootLifting, new(-12, 5), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Step(F.KraidFootRaised, new(-4, 11), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Step(F.KraidFootRaised, new(-6, 11), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Step(F.KraidFootReaching, new(4, 0), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Step(F.KraidFootReaching, new(2, 0), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepSound),
            Show(F.KraidFootReaching, 1),
            Step(F.KraidFootSwinging, new(-12, 12), 20),
            Step(F.KraidFootSwinging, new(-10, 10), 4),
            Step(F.KraidFootSwinging, new(-8, 8), 4),
            Step(F.KraidFootSwinging, new(-6, 6), 4),
            Step(F.KraidFootSwinging, new(-4, 4), 4),
            Step(F.KraidFootSwinging, new(-2, 2), 4),
            Show(F.KraidFootSwinging, 4),
            Show(F.KraidFootRaised, 4),
            Step(F.KraidFootLifting, new(-8, -4), 4),
            Step(F.KraidFootLifting, new(-6, -3), 1),
            Step(F.KraidFootLifting, new(-4, -2), 1),
            Step(F.KraidFootLifting, new(-2, -1), 1),
            Show(F.KraidFootLifting, 1),
            Step(F.KraidFootPlanted, new(-8, -4), 1),
            Step(F.KraidFootPlanted, new(-6, -3), 1),
            Step(F.KraidFootPlanted, new(-4, -2), 1),
            Step(F.KraidFootPlanted, new(-2, -1), 1),
        ],
        [P.ArmNormal] =
        [
            Show(F.KraidArmRaisedClawClosed, 6),
            Show(F.KraidArmRaisedClawOpen, 6),
            Show(F.KraidArmLoweringClawHooked, 6),
            Show(F.KraidArmLevelClawHooked, 6),
            Show(F.KraidArmLevelClawHangingDown, 6),
            Show(F.KraidArmLevelClawCurlingUnder, 6),
            Show(F.KraidArmLevelClawTucked, 6),
            Show(F.KraidArmLevelClawTuckedTight, 6),
            Show(F.KraidArmLevelClawFolded, 6),
            Show(F.KraidArmLevelClawFoldedFlat, 6),
            Show(F.KraidArmLevelClawTuckedTight, 6),
            Show(F.KraidArmLevelClawTucked, 6),
            Show(F.KraidArmLevelClawCurlingUnder, 6),
            Show(F.KraidArmLevelClawHangingDown, 6),
            Show(F.KraidArmLevelClawHooked, 6),
            Show(F.KraidArmLoweringClawHooked, 6),
            Show(F.KraidArmRaisedClawOpen, 6),
        ],
        [P.ArmNormalPause] =
        [
            Show(F.KraidArmRaisedClawClosed, 32),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmSlow] =
        [
            Show(F.KraidArmRaisedClawClosed, 8),
            Show(F.KraidArmRaisedClawOpen, 8),
            Show(F.KraidArmLoweringClawHooked, 8),
            Show(F.KraidArmLevelClawHooked, 8),
            Show(F.KraidArmLevelClawHangingDown, 8),
            Show(F.KraidArmLevelClawCurlingUnder, 8),
            Show(F.KraidArmLevelClawTucked, 8),
            Show(F.KraidArmLevelClawTuckedTight, 8),
            Show(F.KraidArmLevelClawFolded, 8),
            Show(F.KraidArmLevelClawFoldedFlat, 8),
            Show(F.KraidArmLevelClawTuckedTight, 8),
            Show(F.KraidArmLevelClawTucked, 8),
            Show(F.KraidArmLevelClawCurlingUnder, 8),
            Show(F.KraidArmLevelClawHangingDown, 8),
            Show(F.KraidArmLevelClawHooked, 8),
            Show(F.KraidArmLoweringClawHooked, 8),
            Show(F.KraidArmRaisedClawOpen, 8),
            Show(F.KraidArmRaisedClawClosed, 48),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmRisingOrSinking] =
        [
            Sunk(F.KraidArmRaisedClawClosed, 6),
            Sunk(F.KraidArmRaisedClawOpen, 6),
            Sunk(F.KraidArmLoweringClawHooked, 6),
            Sunk(F.KraidArmLevelClawHooked, 6),
            Sunk(F.KraidArmLevelClawHangingDown, 6),
            Sunk(F.KraidArmLevelClawCurlingUnder, 6),
            Sunk(F.KraidArmLevelClawTucked, 6),
            Sunk(F.KraidArmLevelClawTuckedTight, 6),
            Sunk(F.KraidArmLevelClawFolded, 6),
            Sunk(F.KraidArmLevelClawFoldedFlat, 6),
            Sunk(F.KraidArmLevelClawTuckedTight, 6),
            Sunk(F.KraidArmLevelClawTucked, 6),
            Sunk(F.KraidArmLevelClawCurlingUnder, 6),
            Sunk(F.KraidArmLevelClawHangingDown, 6),
            Sunk(F.KraidArmLevelClawHooked, 6),
            Sunk(F.KraidArmLoweringClawHooked, 6),
            Sunk(F.KraidArmRaisedClawOpen, 6),
            Sunk(F.KraidArmRaisedClawClosed, 32),
        ],
        [P.ArmDyingOrPreparingToLunge] =
        [
            Show(F.KraidArmRaisedClawClosed, 6),
            Show(F.KraidArmDroopingDiagonal, 6),
            Hold(F.KraidArmOutstretched),
        ],
        [P.LintInitial] =
        [
            Hold(F.KraidLintBudding),
        ],
        [P.LintPostGrowth] =
        [
            Hold(F.KraidLintGrown),
        ],
        [P.NailLoop] =
        [
            Show(F.KraidNailTurned0, 3),
            Show(F.KraidNailTurned45, 3),
            Show(F.KraidNailTurned90, 3),
            Show(F.KraidNailTurned135, 3),
            Show(F.KraidNailTurned180, 3),
            Show(F.KraidNailTurned225, 3),
            Show(F.KraidNailTurned270, 3),
            Show(F.KraidNailTurned315, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KraidPartSegment, Animation<KraidPartSegment>> StartingAt { get; } = new Dictionary<KraidPartSegment, Animation<KraidPartSegment>>
    {
        [P.FootInitial] = new([Whole(P.FootInitial)], AnimationEnd.Holds),
        [P.FootNeutral] = new([Whole(P.FootNeutral)], AnimationEnd.Holds),
        [P.FootWalkingForward] = new([Whole(P.FootWalkingForward)], AnimationEnd.Holds),
        [P.FootLungeForward] = new([Whole(P.FootLungeForward)], AnimationEnd.Holds),
        [P.FootWalkingBackward] = new([Whole(P.FootWalkingBackward)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormal] = new([Whole(P.ArmNormal), Whole(P.ArmNormalPause)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormalPause] = new([Whole(P.ArmNormalPause), Whole(P.ArmNormal)], AnimationEnd.LoopsFrom(0)),
        [P.ArmSlow] = new([Whole(P.ArmSlow)], AnimationEnd.LoopsFrom(0)),
        [P.ArmRisingOrSinking] = new([Whole(P.ArmRisingOrSinking)], AnimationEnd.LoopsFrom(0)),
        [P.ArmDyingOrPreparingToLunge] = new([Whole(P.ArmDyingOrPreparingToLunge)], AnimationEnd.Holds),
        [P.LintInitial] = new([Whole(P.LintInitial)], AnimationEnd.Holds),
        [P.LintPostGrowth] = new([Whole(P.LintPostGrowth)], AnimationEnd.Holds),
        [P.NailLoop] = new([Whole(P.NailLoop)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidPartSegment> StartingWith(KraidPartSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/> (#2296).</summary>
    private static AnimationPhase<KraidPartSegment> Whole(KraidPartSegment segment) =>
        new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="picture"/> where the part stands for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> Show(EnemyFrameId picture, int duration) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.Frame(new KraidPartPose(picture), duration);

    /// <summary>
    /// Shows the foot's <paramref name="picture"/> moved by <paramref name="step"/> from where its walk first shows it, its
    /// collision moved with it, for <paramref name="duration"/> updates.
    /// </summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> Step(EnemyFrameId picture, Offset16<RoomPixels> step, int duration) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.Frame(new KraidPartPose(picture) { Placement = step }, duration);

    /// <summary>
    /// Shows <paramref name="picture"/> behind the room's front tiles, as Kraid shows his arm while he rises or sinks, for
    /// <paramref name="duration"/> updates.
    /// </summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> Sunk(EnemyFrameId picture, int duration) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.Frame(new KraidPartPose(picture) { Depth = ActorDepth.BehindFrontParts }, duration);

    /// <summary>
    /// Shows <paramref name="picture"/> for good (#1627, #2213): the part rests on it until its owner starts another animation.
    /// The feet's standing poses, the belly lints' poses and the arm's last lunge-ready pose are held: nothing reads a part's
    /// frame timer or new-frame flag, and every lunge and the death start the arm's next animation long before any frame
    /// duration could run out.
    /// </summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> Hold(EnemyFrameId picture) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.HeldFrame(new KraidPartPose(picture));

    /// <summary>Holds <paramref name="picture"/> behind the room's front tiles, as Kraid shows his foot before his rise ends.</summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> HoldSunk(EnemyFrameId picture) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.HeldFrame(new KraidPartPose(picture) { Depth = ActorDepth.BehindFrontParts });

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<KraidPartPose, KraidPartCue> Cue(KraidPartCue cue) =>
        new AnimationBeat<KraidPartPose, KraidPartCue>.Event(cue);
}