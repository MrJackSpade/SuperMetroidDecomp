using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One stage of Crocomire's course from his fight to his corpse (#2540), as
/// <see cref="CrocomireDeathStageDefinitions.Stages"/> lists them in the order he goes through them: a stage that finishes
/// hands over to the one after it. A stage runs on each of his body's updates until it finishes.
/// </summary>
internal abstract record CrocomireDeathStage
{
    /// <summary>Allows only the stages below.</summary>
    private CrocomireDeathStage()
    {
    }

    /// <summary>The fight runs, with its bridge checks: his death has not begun. A new body starts here.</summary>
    internal sealed record Fighting : CrocomireDeathStage;

    /// <summary>
    /// The bridge has given way: he sinks through it, whole, into the acid as its fragments fall one an update, until he
    /// rests at the acid's floor.
    /// </summary>
    internal sealed record SinkThroughBridge : CrocomireDeathStage;

    /// <summary>
    /// He sinks into the acid, the acid smoking about him, until he rests at its floor; the submerged pause after it then
    /// starts its wait.
    /// </summary>
    /// <param name="Body">What his body is as he sinks.</param>
    internal sealed record Sink(CrocomireBodyInAcid Body) : CrocomireDeathStage;

    /// <summary>He rests at the acid's floor while the wait his sink started runs, and starts to rise as it runs out.</summary>
    /// <param name="Body">What his body is as he rests.</param>
    internal sealed record SubmergedPause(CrocomireBodyInAcid Body) : CrocomireDeathStage;

    /// <summary>He rises out of the acid, the acid smoking about him, until he has risen above its surface.</summary>
    /// <param name="Body">What his body is as he rises.</param>
    internal sealed record Rise(CrocomireBodyInAcid Body) : CrocomireDeathStage;

    /// <summary>His body shows <paramref name="Picture"/>'s melt image in place of his body frame, for one update.</summary>
    /// <param name="Picture">The melting illustration shown.</param>
    internal sealed record ShowMeltImage(CrocomireMeltingIllustration Picture) : CrocomireDeathStage;

    /// <summary>The melt pass of <paramref name="Picture"/> begins, for one update, its melt's stretch levelled.</summary>
    /// <param name="Picture">The melting illustration the pass shows and dissolves.</param>
    internal sealed record BeginMeltPass(CrocomireMeltingIllustration Picture) : CrocomireDeathStage;

    /// <summary>The pass's melt image shows one band an update, finishing on the update after its last band.</summary>
    internal sealed record ShowMeltBands : CrocomireDeathStage;

    /// <summary>His death holds for one update and does nothing else.</summary>
    internal sealed record PauseOneUpdate : CrocomireDeathStage;

    /// <summary>He cries out as his melt image starts to dissolve and the melt's stretch starts, for one update.</summary>
    /// <param name="Cry">The sound he cries out with.</param>
    internal sealed record StartDissolve(SoundEffectId Cry) : CrocomireDeathStage;

    /// <summary>His melt image dissolves a column an update, shaking, while the melt's stretch runs to its end.</summary>
    internal sealed record Dissolve : CrocomireDeathStage;

    /// <summary>His body shows nothing, and the sink after it starts from rest, for one update.</summary>
    internal sealed record ClearMeltImage : CrocomireDeathStage;

    /// <summary>
    /// He sinks a last time, showing his bare skull's melting picture; at the acid's floor his skeleton is sent down the
    /// river, the boss tension music starts and the arena opens behind him.
    /// </summary>
    internal sealed record FinalSink : CrocomireDeathStage;

    /// <summary>His skeleton drifts left along the acid river until it comes to rest behind the arena's wall.</summary>
    internal sealed record FlowDownRiver : CrocomireDeathStage;

    /// <summary>His skeleton waits behind the wall until Samus comes back left.</summary>
    internal sealed record WaitForSamusAtWall : CrocomireDeathStage;

    /// <summary>The hidden wall rumbles through its oscillations.</summary>
    internal sealed record RumbleHiddenWall : CrocomireDeathStage;

    /// <summary>His skeleton is bared behind the spiked wall, then breaks through it.</summary>
    internal sealed record BreakSpikeWall : CrocomireDeathStage;

    /// <summary>His skeleton bursts out of the wall until its fall's wait runs out.</summary>
    internal sealed record DelaySkeletonFall : CrocomireDeathStage;

    /// <summary>His skeleton arcs back into the arena until it lands.</summary>
    internal sealed record ArcSkeletonIntoArena : CrocomireDeathStage;

    /// <summary>His skeleton falls apart; once it has, his dead skeleton is laid down and his drops scatter.</summary>
    internal sealed record CollapseSkeleton : CrocomireDeathStage;

    /// <summary>The arena's screens reopen and the spiked wall clears, for one update.</summary>
    internal sealed record ClearWallAndOpenScrolls : CrocomireDeathStage;

    /// <summary>
    /// The mini-boss's defeat is recorded, the boss tension music restored and the camera's tracking returned, for one
    /// update.
    /// </summary>
    internal sealed record PublishDefeatAndRestoreMusic : CrocomireDeathStage;

    /// <summary>
    /// His corpse, inert: the live room's once his defeat is recorded, and the already-defeated room's from its setup. His
    /// course rests here: it hands over to no other stage.
    /// </summary>
    internal sealed record InertCorpse : CrocomireDeathStage;
}
