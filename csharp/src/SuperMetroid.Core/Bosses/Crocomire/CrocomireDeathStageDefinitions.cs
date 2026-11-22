using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The authored course of Crocomire's death (#2540): the fight, the bridge giving way, two hops in the burning acid, his
/// first melt and three hops showing it, his second melt and a last sink, then his skeleton's return through the wall, his
/// defeat and his corpse.
/// </summary>
internal static partial class CrocomireDeathStageDefinitions
{
    /// <summary>
    /// The stages in the order he goes through them, the fight first and his inert corpse last. The bridge gives way and he
    /// sinks through it, then hops twice while still whole. His first melt shows his flesh's melt image band by band, he
    /// rises showing it, it dissolves, and he hops three more times showing it. His second melt shows his bare skull's melt
    /// image band by band and, after a pause of an update, he rises a last time showing it as it dissolves, then sinks
    /// a last time. His skeleton then flows down the river, waits behind the wall until Samus comes back, breaks through it
    /// into the arena and falls apart. His dead skeleton, laid down as the collapse ended, already rests: an update's pause
    /// for it and another follow before his defeat is recorded.
    /// </summary>
    internal static IReadOnlyList<CrocomireDeathStage> Stages { get; } =
    [
        new CrocomireDeathStage.Fighting(),
        new CrocomireDeathStage.SinkThroughBridge(),
        .. Hop(CrocomireBodyInAcid.Whole),
        .. Hop(CrocomireBodyInAcid.Whole),
        new CrocomireDeathStage.ShowMeltImage(CrocomireMeltingIllustration.Flesh),
        new CrocomireDeathStage.BeginMeltPass(CrocomireMeltingIllustration.Flesh),
        new CrocomireDeathStage.ShowMeltBands(),
        new CrocomireDeathStage.Rise(CrocomireBodyInAcid.MeltingFlesh),
        new CrocomireDeathStage.StartDissolve(CrocomireSoundDefinitions.FirstMeltDissolve),
        new CrocomireDeathStage.Dissolve(),
        new CrocomireDeathStage.ClearMeltImage(),
        new CrocomireDeathStage.Sink(CrocomireBodyInAcid.MeltingFlesh),
        .. Hop(CrocomireBodyInAcid.MeltingFlesh),
        .. Hop(CrocomireBodyInAcid.MeltingFlesh),
        new CrocomireDeathStage.ShowMeltImage(CrocomireMeltingIllustration.BareSkull),
        new CrocomireDeathStage.BeginMeltPass(CrocomireMeltingIllustration.BareSkull),
        new CrocomireDeathStage.ShowMeltBands(),
        new CrocomireDeathStage.PauseOneUpdate(),
        new CrocomireDeathStage.Rise(CrocomireBodyInAcid.MeltingBareSkull),
        new CrocomireDeathStage.StartDissolve(CrocomireSoundDefinitions.SecondMeltDissolve),
        new CrocomireDeathStage.Dissolve(),
        new CrocomireDeathStage.ClearMeltImage(),
        new CrocomireDeathStage.FinalSink(),
        new CrocomireDeathStage.FlowDownRiver(),
        new CrocomireDeathStage.WaitForSamusAtWall(),
        new CrocomireDeathStage.RumbleHiddenWall(),
        new CrocomireDeathStage.BreakSpikeWall(),
        new CrocomireDeathStage.DelaySkeletonFall(),
        new CrocomireDeathStage.ArcSkeletonIntoArena(),
        new CrocomireDeathStage.CollapseSkeleton(),
        new CrocomireDeathStage.ClearWallAndOpenScrolls(),
        new CrocomireDeathStage.PauseOneUpdate(),
        new CrocomireDeathStage.PauseOneUpdate(),
        new CrocomireDeathStage.PublishDefeatAndRestoreMusic(),
        new CrocomireDeathStage.InertCorpse(),
    ];

    /// <summary>One hop in the acid showing <paramref name="body"/> (#2557): a pause submerged, a rise and a sink.</summary>
    private static CrocomireDeathStage[] Hop(CrocomireBodyInAcid body) =>
        [new CrocomireDeathStage.SubmergedPause(body), new CrocomireDeathStage.Rise(body), new CrocomireDeathStage.Sink(body)];
}
