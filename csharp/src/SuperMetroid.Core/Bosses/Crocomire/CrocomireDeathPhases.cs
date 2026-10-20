namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Domain-owned transitions over <see cref="CrocomireDeathPhase"/>.</summary>
public static class CrocomireDeathPhases
{
    /// <summary>
    /// The dispatcher entry after <paramref name="phase"/>, as the native death handlers
    /// advance variable A by one table word. <see cref="CrocomireDeathPhase.RiverSkeletonDetour"/> is the last
    /// table entry and has no successor.
    /// </summary>
    public static CrocomireDeathPhase Next(this CrocomireDeathPhase phase) => phase switch
    {
        CrocomireDeathPhase.Fighting => CrocomireDeathPhase.CrumbleBridgeAndSink,
        CrocomireDeathPhase.CrumbleBridgeAndSink => CrocomireDeathPhase.FirstSubmergedPause,
        CrocomireDeathPhase.FirstSubmergedPause => CrocomireDeathPhase.FirstHopRise,
        CrocomireDeathPhase.FirstHopRise => CrocomireDeathPhase.FirstHopSink,
        CrocomireDeathPhase.FirstHopSink => CrocomireDeathPhase.SecondSubmergedPause,
        CrocomireDeathPhase.SecondSubmergedPause => CrocomireDeathPhase.SecondHopRise,
        CrocomireDeathPhase.SecondHopRise => CrocomireDeathPhase.SecondHopSink,
        CrocomireDeathPhase.SecondHopSink => CrocomireDeathPhase.InstallFirstMeltImage,
        CrocomireDeathPhase.InstallFirstMeltImage => CrocomireDeathPhase.CopyFirstMeltGraphics,
        CrocomireDeathPhase.CopyFirstMeltGraphics => CrocomireDeathPhase.UploadFirstMeltGraphics,
        CrocomireDeathPhase.UploadFirstMeltGraphics => CrocomireDeathPhase.ThirdHopRise,
        CrocomireDeathPhase.ThirdHopRise => CrocomireDeathPhase.StartFirstDissolve,
        CrocomireDeathPhase.StartFirstDissolve => CrocomireDeathPhase.DissolveFirstImage,
        CrocomireDeathPhase.DissolveFirstImage => CrocomireDeathPhase.ClearFirstMeltImage,
        CrocomireDeathPhase.ClearFirstMeltImage => CrocomireDeathPhase.FourthHopSink,
        CrocomireDeathPhase.FourthHopSink => CrocomireDeathPhase.ThirdSubmergedPause,
        CrocomireDeathPhase.ThirdSubmergedPause => CrocomireDeathPhase.FourthHopRise,
        CrocomireDeathPhase.FourthHopRise => CrocomireDeathPhase.FifthHopSink,
        CrocomireDeathPhase.FifthHopSink => CrocomireDeathPhase.FourthSubmergedPause,
        CrocomireDeathPhase.FourthSubmergedPause => CrocomireDeathPhase.FifthHopRise,
        CrocomireDeathPhase.FifthHopRise => CrocomireDeathPhase.SixthHopSink,
        CrocomireDeathPhase.SixthHopSink => CrocomireDeathPhase.InstallSecondMeltImage,
        CrocomireDeathPhase.InstallSecondMeltImage => CrocomireDeathPhase.CopySecondMeltGraphics,
        CrocomireDeathPhase.CopySecondMeltGraphics => CrocomireDeathPhase.UploadSecondMeltGraphics,
        CrocomireDeathPhase.UploadSecondMeltGraphics => CrocomireDeathPhase.ShippedSpacer,
        CrocomireDeathPhase.ShippedSpacer => CrocomireDeathPhase.SixthHopRise,
        CrocomireDeathPhase.SixthHopRise => CrocomireDeathPhase.StartSecondDissolve,
        CrocomireDeathPhase.StartSecondDissolve => CrocomireDeathPhase.DissolveSecondImage,
        CrocomireDeathPhase.DissolveSecondImage => CrocomireDeathPhase.ClearSecondMeltImage,
        CrocomireDeathPhase.ClearSecondMeltImage => CrocomireDeathPhase.FinalSink,
        CrocomireDeathPhase.FinalSink => CrocomireDeathPhase.WaitForSamusAtWall,
        CrocomireDeathPhase.WaitForSamusAtWall => CrocomireDeathPhase.RumbleHiddenWall,
        CrocomireDeathPhase.RumbleHiddenWall => CrocomireDeathPhase.BreakSpikeWall,
        CrocomireDeathPhase.BreakSpikeWall => CrocomireDeathPhase.DelaySkeletonFall,
        CrocomireDeathPhase.DelaySkeletonFall => CrocomireDeathPhase.ArcSkeletonIntoArena,
        CrocomireDeathPhase.ArcSkeletonIntoArena => CrocomireDeathPhase.WaitForSkeletonTerminalImage,
        CrocomireDeathPhase.WaitForSkeletonTerminalImage => CrocomireDeathPhase.ClearWallAndOpenScrolls,
        CrocomireDeathPhase.ClearWallAndOpenScrolls => CrocomireDeathPhase.WaitForStableSkeleton,
        CrocomireDeathPhase.WaitForStableSkeleton => CrocomireDeathPhase.NativeOneFrameSpacer,
        CrocomireDeathPhase.NativeOneFrameSpacer => CrocomireDeathPhase.PublishDefeatAndRestoreMusic,
        CrocomireDeathPhase.PublishDefeatAndRestoreMusic => CrocomireDeathPhase.InertCorpse,
        CrocomireDeathPhase.InertCorpse => CrocomireDeathPhase.DefeatedRoomAdvance,
        CrocomireDeathPhase.DefeatedRoomAdvance => CrocomireDeathPhase.PinDefeatedRoomBg2Scroll,
        CrocomireDeathPhase.PinDefeatedRoomBg2Scroll => CrocomireDeathPhase.RiverSkeletonDetour,
        CrocomireDeathPhase.RiverSkeletonDetour => throw new InvalidOperationException($"{nameof(CrocomireDeathPhase)} {(int)phase} has no successor."),
        _ => throw new InvalidOperationException($"{nameof(CrocomireDeathPhase)} {(int)phase} has no successor."),
    };
}
