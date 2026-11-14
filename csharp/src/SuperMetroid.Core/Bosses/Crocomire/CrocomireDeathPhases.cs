namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Domain-owned transitions over <see cref="CrocomireDeathPhase"/>.</summary>
public static class CrocomireDeathPhases
{
    /// <summary>
    /// The phase each finishing phase hands over to. The inert corpse, the pinned already-defeated room and the river
    /// detour never hand over, so they have none.
    /// </summary>
    private static readonly Dictionary<CrocomireDeathPhase, CrocomireDeathPhase> Successors = new()
    {
        [CrocomireDeathPhase.Fighting] = CrocomireDeathPhase.CrumbleBridgeAndSink,
        [CrocomireDeathPhase.CrumbleBridgeAndSink] = CrocomireDeathPhase.FirstSubmergedPause,
        [CrocomireDeathPhase.FirstSubmergedPause] = CrocomireDeathPhase.FirstHopRise,
        [CrocomireDeathPhase.FirstHopRise] = CrocomireDeathPhase.FirstHopSink,
        [CrocomireDeathPhase.FirstHopSink] = CrocomireDeathPhase.SecondSubmergedPause,
        [CrocomireDeathPhase.SecondSubmergedPause] = CrocomireDeathPhase.SecondHopRise,
        [CrocomireDeathPhase.SecondHopRise] = CrocomireDeathPhase.SecondHopSink,
        [CrocomireDeathPhase.SecondHopSink] = CrocomireDeathPhase.InstallFirstMeltImage,
        [CrocomireDeathPhase.InstallFirstMeltImage] = CrocomireDeathPhase.BeginFirstMeltPass,
        [CrocomireDeathPhase.BeginFirstMeltPass] = CrocomireDeathPhase.ShowFirstMeltBands,
        [CrocomireDeathPhase.ShowFirstMeltBands] = CrocomireDeathPhase.ThirdHopRise,
        [CrocomireDeathPhase.ThirdHopRise] = CrocomireDeathPhase.StartFirstDissolve,
        [CrocomireDeathPhase.StartFirstDissolve] = CrocomireDeathPhase.DissolveFirstImage,
        [CrocomireDeathPhase.DissolveFirstImage] = CrocomireDeathPhase.ClearFirstMeltImage,
        [CrocomireDeathPhase.ClearFirstMeltImage] = CrocomireDeathPhase.FourthHopSink,
        [CrocomireDeathPhase.FourthHopSink] = CrocomireDeathPhase.ThirdSubmergedPause,
        [CrocomireDeathPhase.ThirdSubmergedPause] = CrocomireDeathPhase.FourthHopRise,
        [CrocomireDeathPhase.FourthHopRise] = CrocomireDeathPhase.FifthHopSink,
        [CrocomireDeathPhase.FifthHopSink] = CrocomireDeathPhase.FourthSubmergedPause,
        [CrocomireDeathPhase.FourthSubmergedPause] = CrocomireDeathPhase.FifthHopRise,
        [CrocomireDeathPhase.FifthHopRise] = CrocomireDeathPhase.SixthHopSink,
        [CrocomireDeathPhase.SixthHopSink] = CrocomireDeathPhase.InstallSecondMeltImage,
        [CrocomireDeathPhase.InstallSecondMeltImage] = CrocomireDeathPhase.BeginSecondMeltPass,
        [CrocomireDeathPhase.BeginSecondMeltPass] = CrocomireDeathPhase.ShowSecondMeltBands,
        [CrocomireDeathPhase.ShowSecondMeltBands] = CrocomireDeathPhase.WaitBeforeSixthHopRise,
        [CrocomireDeathPhase.WaitBeforeSixthHopRise] = CrocomireDeathPhase.SixthHopRise,
        [CrocomireDeathPhase.SixthHopRise] = CrocomireDeathPhase.StartSecondDissolve,
        [CrocomireDeathPhase.StartSecondDissolve] = CrocomireDeathPhase.DissolveSecondImage,
        [CrocomireDeathPhase.DissolveSecondImage] = CrocomireDeathPhase.ClearSecondMeltImage,
        [CrocomireDeathPhase.ClearSecondMeltImage] = CrocomireDeathPhase.FinalSink,
        [CrocomireDeathPhase.FinalSink] = CrocomireDeathPhase.WaitForSamusAtWall,
        [CrocomireDeathPhase.WaitForSamusAtWall] = CrocomireDeathPhase.RumbleHiddenWall,
        [CrocomireDeathPhase.RumbleHiddenWall] = CrocomireDeathPhase.BreakSpikeWall,
        [CrocomireDeathPhase.BreakSpikeWall] = CrocomireDeathPhase.DelaySkeletonFall,
        [CrocomireDeathPhase.DelaySkeletonFall] = CrocomireDeathPhase.ArcSkeletonIntoArena,
        [CrocomireDeathPhase.ArcSkeletonIntoArena] = CrocomireDeathPhase.WaitForSkeletonTerminalImage,
        [CrocomireDeathPhase.WaitForSkeletonTerminalImage] = CrocomireDeathPhase.ClearWallAndOpenScrolls,
        [CrocomireDeathPhase.ClearWallAndOpenScrolls] = CrocomireDeathPhase.WaitForStableSkeleton,
        [CrocomireDeathPhase.WaitForStableSkeleton] = CrocomireDeathPhase.WaitBeforeRecordingDefeat,
        [CrocomireDeathPhase.WaitBeforeRecordingDefeat] = CrocomireDeathPhase.PublishDefeatAndRestoreMusic,
        [CrocomireDeathPhase.PublishDefeatAndRestoreMusic] = CrocomireDeathPhase.InertCorpse,
        [CrocomireDeathPhase.WaitBeforePinningDefeatedBodyLayer] = CrocomireDeathPhase.PinDefeatedRoomBodyLayerPlacement,
    };

    /// <summary>The phase <paramref name="phase"/> hands over to when it finishes.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="phase"/> never hands over.</exception>
    public static CrocomireDeathPhase Next(this CrocomireDeathPhase phase) =>
        Successors.TryGetValue(phase, out CrocomireDeathPhase next)
            ? next
            : throw new InvalidOperationException($"Crocomire's death phase {phase} hands over to no other phase.");
}
