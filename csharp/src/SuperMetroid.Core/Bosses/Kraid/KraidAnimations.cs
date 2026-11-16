using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartSegment;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's typed part animations (#641, #1600, #1885): each foot, arm, lint and fingernail segment's beats, each beat naming
/// the installed frame it shows, and the animation starting at each.
/// </summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail segment's beats.</summary>
    internal static AnimationSegments<KraidPartSegment, EnemyFrameId, KraidPartCue> Segments { get; } = new(
        new Dictionary<KraidPartSegment, AnimationBeat<EnemyFrameId, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            Hold(F.KraidFootPlanted),
        ],
        [P.FootNeutral] =
        [
            Hold(F.KraidFootPlantedTwin),
        ],
        [P.FootWalkingForward] =
        [
            Show(F.KraidFootPlantedTwin, 4),
            Show(F.KraidFootPlantedOffset2By1, 4),
            Show(F.KraidFootPlantedOffset4By2, 4),
            Show(F.KraidFootPlantedOffset6By3, 4),
            Show(F.KraidFootPlantedOffset8By4, 4),
            Show(F.KraidFootLiftingOffset10By19, 4),
            Show(F.KraidFootLiftingOffset12By20, 4),
            Show(F.KraidFootLiftingOffset14By21, 4),
            Show(F.KraidFootLiftingOffset16By22, 4),
            Show(F.KraidFootLiftingOffset18By23, 4),
            Show(F.KraidFootRaisedOffset20By26, 4),
            Show(F.KraidFootSwingingOffset22By23, 3),
            Show(F.KraidFootSwingingOffset24By21, 1),
            Show(F.KraidFootSwingingOffset26By19, 1),
            Show(F.KraidFootSwingingOffset28By17, 1),
            Show(F.KraidFootSwingingOffset30By15, 1),
            Show(F.KraidFootSwingingOffset32By13, 1),
            Show(F.KraidFootSwingingOffset34By11, 16),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset32By10, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset30By10, 1),
            Show(F.KraidFootReachingOffset30By10, 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset30By10, 1),
            Show(F.KraidFootReachingOffset30By10, 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootRaisedOffset26By15, 1),
            Show(F.KraidFootRaisedOffset26By15, 3),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootRaisedOffset24By15, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset22By14, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset20By14, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(F.KraidFootLiftingOffset18By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset16By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset14By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset12By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset10By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset8By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootPlantedOffset6By0, 1),
            Show(F.KraidFootPlantedTwin, 1),
        ],
        [P.FootLungeForward] =
        [
            Show(F.KraidFootPlantedTwin, 1),
            Show(F.KraidFootPlantedOffset2By1, 1),
            Show(F.KraidFootPlantedOffset4By2, 1),
            Show(F.KraidFootPlantedOffset6By3, 1),
            Show(F.KraidFootPlantedOffset8By4, 1),
            Show(F.KraidFootLiftingOffset10By19, 1),
            Show(F.KraidFootLiftingOffset12By20, 1),
            Show(F.KraidFootLiftingOffset14By21, 1),
            Show(F.KraidFootLiftingOffset16By22, 1),
            Show(F.KraidFootLiftingOffset18By23, 1),
            Show(F.KraidFootRaisedOffset20By26, 1),
            Show(F.KraidFootSwingingOffset22By23, 1),
            Show(F.KraidFootSwingingOffset24By21, 1),
            Show(F.KraidFootSwingingOffset26By19, 1),
            Show(F.KraidFootSwingingOffset28By17, 1),
            Show(F.KraidFootSwingingOffset30By15, 1),
            Show(F.KraidFootSwingingOffset32By13, 1),
            Show(F.KraidFootSwingingOffset34By11, 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset32By10, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset30By10, 1),
            Show(F.KraidFootReachingOffset30By10, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootReachingOffset30By10, 1),
            Show(F.KraidFootReachingOffset30By10, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootRaisedOffset26By15, 1),
            Show(F.KraidFootRaisedOffset26By15, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootRaisedOffset24By15, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset22By14, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset20By14, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(F.KraidFootLiftingOffset18By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset16By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset14By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset12By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset10By14, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootLiftingOffset8By14, 1),
            Show(F.KraidFootPlantedOffset6By0, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootPlantedTwin, 1),
        ],
        [P.FootWalkingBackward] =
        [
            Cue(K.StepRight),
            Show(F.KraidFootPlantedTwinB, 4),
            Cue(K.StepRight),
            Show(F.KraidFootPlantedOffset4By0, 1),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset8By14, 1),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset10By14, 1),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset12By14, 1),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset14By14, 1),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset16By14, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset18By14, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset20By14, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootLiftingOffset22By14, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootRaisedOffset24By15, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootRaisedOffset26By15, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootReachingOffset28By10, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootReachingOffset30By10, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepSound),
            Show(F.KraidFootReachingOffset32By10, 1),
            Show(F.KraidFootSwingingOffset34By11, 20),
            Show(F.KraidFootSwingingOffset32By13, 4),
            Show(F.KraidFootSwingingOffset30By15, 4),
            Show(F.KraidFootSwingingOffset28By17, 4),
            Show(F.KraidFootSwingingOffset26By19, 4),
            Show(F.KraidFootSwingingOffset24By21, 4),
            Show(F.KraidFootSwingingOffset22By23, 4),
            Show(F.KraidFootRaisedOffset20By26, 4),
            Show(F.KraidFootLiftingOffset18By23, 4),
            Show(F.KraidFootLiftingOffset16By22, 1),
            Show(F.KraidFootLiftingOffset14By21, 1),
            Show(F.KraidFootLiftingOffset12By20, 1),
            Show(F.KraidFootLiftingOffset10By19, 1),
            Show(F.KraidFootPlantedOffset8By4, 1),
            Show(F.KraidFootPlantedOffset6By3, 1),
            Show(F.KraidFootPlantedOffset4By2, 1),
            Show(F.KraidFootPlantedOffset2By1, 1),
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
            Show(F.KraidArmRaisedClawClosedTwin, 6),
            Show(F.KraidArmRaisedClawOpenTwin, 6),
            Show(F.KraidArmLoweringClawHookedTwin, 6),
            Show(F.KraidArmLevelClawHookedTwin, 6),
            Show(F.KraidArmLevelClawHangingDownTwin, 6),
            Show(F.KraidArmLevelClawCurlingUnderTwin, 6),
            Show(F.KraidArmLevelClawTuckedTwin, 6),
            Show(F.KraidArmLevelClawTuckedTightTwin, 6),
            Show(F.KraidArmLevelClawFoldedTwin, 6),
            Show(F.KraidArmLevelClawFoldedFlatTwin, 6),
            Show(F.KraidArmLevelClawTuckedTightTwin, 6),
            Show(F.KraidArmLevelClawTuckedTwin, 6),
            Show(F.KraidArmLevelClawCurlingUnderTwin, 6),
            Show(F.KraidArmLevelClawHangingDownTwin, 6),
            Show(F.KraidArmLevelClawHookedTwin, 6),
            Show(F.KraidArmLoweringClawHookedTwin, 6),
            Show(F.KraidArmRaisedClawOpenTwin, 6),
            Show(F.KraidArmRaisedClawClosedTwin, 32),
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
        [P.FootInitial] = new([new(P.FootInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootNeutral] = new([new(P.FootNeutral, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootWalkingForward] = new([new(P.FootWalkingForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootLungeForward] = new([new(P.FootLungeForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootWalkingBackward] = new([new(P.FootWalkingBackward, 0, 55, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormal] = new([new(P.ArmNormal, 0, 17, 1), new(P.ArmNormalPause, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormalPause] = new([new(P.ArmNormalPause, 0, 2, 1), new(P.ArmNormal, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmSlow] = new([new(P.ArmSlow, 0, 19, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmRisingOrSinking] = new([new(P.ArmRisingOrSinking, 0, 18, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmDyingOrPreparingToLunge] = new([new(P.ArmDyingOrPreparingToLunge, 0, 3, 1)], AnimationEnd.Holds),
        [P.LintInitial] = new([new(P.LintInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.LintPostGrowth] = new([new(P.LintPostGrowth, 0, 1, 1)], AnimationEnd.Holds),
        [P.NailLoop] = new([new(P.NailLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidPartSegment> StartingWith(KraidPartSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.Frame(frame, duration);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627, #2213): the part rests on it until its owner starts another animation.
    /// The feet's standing poses, the belly lints' poses and the arm's last lunge-ready pose are held: nothing reads a part's
    /// frame timer or new-frame flag, and every lunge and the death start the arm's next animation long before any frame
    /// duration could run out.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Hold(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Cue(KraidPartCue cue) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.Event(cue);
}
