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

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this crocomire death stage, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(CrocomireDeathStageReactions reactions);


    /// <summary>The fight runs, with its bridge checks: his death has not begun. A new body starts here.</summary>
    internal sealed record Fighting : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).Fighting(this);
    }

    /// <summary>
    /// The bridge has given way: he sinks through it, whole, into the acid as its fragments fall one an update, until he
    /// rests at the acid's floor.
    /// </summary>
    internal sealed record SinkThroughBridge : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).SinkThroughBridge(this);
    }

    /// <summary>
    /// He sinks into the acid, the acid smoking about him, until he rests at its floor; the submerged pause after it then
    /// starts its wait.
    /// </summary>
    /// <param name="Body">What his body is as he sinks.</param>
    internal sealed record Sink(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).Sink(this);
    }

    /// <summary>He rests at the acid's floor while the wait his sink started runs, and starts to rise as it runs out.</summary>
    /// <param name="Body">What his body is as he rests.</param>
    internal sealed record SubmergedPause(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).SubmergedPause(this);
    }

    /// <summary>He rises out of the acid, the acid smoking about him, until he has risen above its surface.</summary>
    /// <param name="Body">What his body is as he rises.</param>
    internal sealed record Rise(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).Rise(this);
    }

    /// <summary>His body shows <paramref name="Picture"/>'s melt image in place of his body frame, for one update.</summary>
    /// <param name="Picture">The melting illustration shown.</param>
    internal sealed record ShowMeltImage(CrocomireMeltingIllustration Picture) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).ShowMeltImage(this);
    }

    /// <summary>The melt pass of <paramref name="Picture"/> begins, for one update, its melt's stretch levelled.</summary>
    /// <param name="Picture">The melting illustration the pass shows and dissolves.</param>
    internal sealed record BeginMeltPass(CrocomireMeltingIllustration Picture) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).BeginMeltPass(this);
    }

    /// <summary>The pass's melt image shows one band an update, finishing on the update after its last band.</summary>
    internal sealed record ShowMeltBands : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).ShowMeltBands(this);
    }

    /// <summary>His death holds for one update and does nothing else.</summary>
    internal sealed record PauseOneUpdate : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).PauseOneUpdate(this);
    }

    /// <summary>He cries out as his melt image starts to dissolve and the melt's stretch starts, for one update.</summary>
    /// <param name="Cry">The sound he cries out with.</param>
    internal sealed record StartDissolve(SoundEffectId Cry) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).StartDissolve(this);
    }

    /// <summary>His melt image dissolves a column an update, shaking, while the melt's stretch runs to its end.</summary>
    internal sealed record Dissolve : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).Dissolve(this);
    }

    /// <summary>His body shows nothing, and the sink after it starts from rest, for one update.</summary>
    internal sealed record ClearMeltImage : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).ClearMeltImage(this);
    }

    /// <summary>
    /// He sinks a last time, showing his bare skull's melting picture; at the acid's floor his skeleton is sent down the
    /// river, the boss tension music starts and the arena opens behind him.
    /// </summary>
    internal sealed record FinalSink : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).FinalSink(this);
    }

    /// <summary>His skeleton drifts left along the acid river until it comes to rest behind the arena's wall.</summary>
    internal sealed record FlowDownRiver : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).FlowDownRiver(this);
    }

    /// <summary>His skeleton waits behind the wall until Samus comes back left.</summary>
    internal sealed record WaitForSamusAtWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).WaitForSamusAtWall(this);
    }

    /// <summary>The hidden wall rumbles through its oscillations.</summary>
    internal sealed record RumbleHiddenWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).RumbleHiddenWall(this);
    }

    /// <summary>His skeleton is bared behind the spiked wall, then breaks through it.</summary>
    internal sealed record BreakSpikeWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).BreakSpikeWall(this);
    }

    /// <summary>His skeleton bursts out of the wall until its fall's wait runs out.</summary>
    internal sealed record DelaySkeletonFall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).DelaySkeletonFall(this);
    }

    /// <summary>His skeleton arcs back into the arena until it lands.</summary>
    internal sealed record ArcSkeletonIntoArena : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).ArcSkeletonIntoArena(this);
    }

    /// <summary>His skeleton falls apart; once it has, his dead skeleton is laid down and his drops scatter.</summary>
    internal sealed record CollapseSkeleton : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).CollapseSkeleton(this);
    }

    /// <summary>The arena's screens reopen and the spiked wall clears, for one update.</summary>
    internal sealed record ClearWallAndOpenScrolls : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).ClearWallAndOpenScrolls(this);
    }

    /// <summary>
    /// The mini-boss's defeat is recorded, the boss tension music restored and the camera's tracking returned, for one
    /// update.
    /// </summary>
    internal sealed record PublishDefeatAndRestoreMusic : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).PublishDefeatAndRestoreMusic(this);
    }

    /// <summary>
    /// His corpse, inert: the live room's once his defeat is recorded, and the already-defeated room's from its setup. It
    /// never hands over.
    /// </summary>
    internal sealed record InertCorpse : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React(CrocomireDeathStageReactions reactions) => Ensure.NotNull(reactions).InertCorpse(this);
    }
}

/// <summary>What the owner of a <see cref="CrocomireDeathStage"/> does on each of its cases (#2553).</summary>
/// <param name="Fighting">Reacts to a <see cref="CrocomireDeathStage.Fighting"/>.</param>
/// <param name="SinkThroughBridge">Reacts to a <see cref="CrocomireDeathStage.SinkThroughBridge"/>.</param>
/// <param name="Sink">Reacts to a <see cref="CrocomireDeathStage.Sink"/>.</param>
/// <param name="SubmergedPause">Reacts to a <see cref="CrocomireDeathStage.SubmergedPause"/>.</param>
/// <param name="Rise">Reacts to a <see cref="CrocomireDeathStage.Rise"/>.</param>
/// <param name="ShowMeltImage">Reacts to a <see cref="CrocomireDeathStage.ShowMeltImage"/>.</param>
/// <param name="BeginMeltPass">Reacts to a <see cref="CrocomireDeathStage.BeginMeltPass"/>.</param>
/// <param name="ShowMeltBands">Reacts to a <see cref="CrocomireDeathStage.ShowMeltBands"/>.</param>
/// <param name="PauseOneUpdate">Reacts to a <see cref="CrocomireDeathStage.PauseOneUpdate"/>.</param>
/// <param name="StartDissolve">Reacts to a <see cref="CrocomireDeathStage.StartDissolve"/>.</param>
/// <param name="Dissolve">Reacts to a <see cref="CrocomireDeathStage.Dissolve"/>.</param>
/// <param name="ClearMeltImage">Reacts to a <see cref="CrocomireDeathStage.ClearMeltImage"/>.</param>
/// <param name="FinalSink">Reacts to a <see cref="CrocomireDeathStage.FinalSink"/>.</param>
/// <param name="FlowDownRiver">Reacts to a <see cref="CrocomireDeathStage.FlowDownRiver"/>.</param>
/// <param name="WaitForSamusAtWall">Reacts to a <see cref="CrocomireDeathStage.WaitForSamusAtWall"/>.</param>
/// <param name="RumbleHiddenWall">Reacts to a <see cref="CrocomireDeathStage.RumbleHiddenWall"/>.</param>
/// <param name="BreakSpikeWall">Reacts to a <see cref="CrocomireDeathStage.BreakSpikeWall"/>.</param>
/// <param name="DelaySkeletonFall">Reacts to a <see cref="CrocomireDeathStage.DelaySkeletonFall"/>.</param>
/// <param name="ArcSkeletonIntoArena">Reacts to a <see cref="CrocomireDeathStage.ArcSkeletonIntoArena"/>.</param>
/// <param name="CollapseSkeleton">Reacts to a <see cref="CrocomireDeathStage.CollapseSkeleton"/>.</param>
/// <param name="ClearWallAndOpenScrolls">Reacts to a <see cref="CrocomireDeathStage.ClearWallAndOpenScrolls"/>.</param>
/// <param name="PublishDefeatAndRestoreMusic">Reacts to a <see cref="CrocomireDeathStage.PublishDefeatAndRestoreMusic"/>.</param>
/// <param name="InertCorpse">Reacts to a <see cref="CrocomireDeathStage.InertCorpse"/>.</param>
internal sealed record CrocomireDeathStageReactions(Action<CrocomireDeathStage.Fighting> Fighting, Action<CrocomireDeathStage.SinkThroughBridge> SinkThroughBridge, Action<CrocomireDeathStage.Sink> Sink, Action<CrocomireDeathStage.SubmergedPause> SubmergedPause, Action<CrocomireDeathStage.Rise> Rise, Action<CrocomireDeathStage.ShowMeltImage> ShowMeltImage, Action<CrocomireDeathStage.BeginMeltPass> BeginMeltPass, Action<CrocomireDeathStage.ShowMeltBands> ShowMeltBands, Action<CrocomireDeathStage.PauseOneUpdate> PauseOneUpdate, Action<CrocomireDeathStage.StartDissolve> StartDissolve, Action<CrocomireDeathStage.Dissolve> Dissolve, Action<CrocomireDeathStage.ClearMeltImage> ClearMeltImage, Action<CrocomireDeathStage.FinalSink> FinalSink, Action<CrocomireDeathStage.FlowDownRiver> FlowDownRiver, Action<CrocomireDeathStage.WaitForSamusAtWall> WaitForSamusAtWall, Action<CrocomireDeathStage.RumbleHiddenWall> RumbleHiddenWall, Action<CrocomireDeathStage.BreakSpikeWall> BreakSpikeWall, Action<CrocomireDeathStage.DelaySkeletonFall> DelaySkeletonFall, Action<CrocomireDeathStage.ArcSkeletonIntoArena> ArcSkeletonIntoArena, Action<CrocomireDeathStage.CollapseSkeleton> CollapseSkeleton, Action<CrocomireDeathStage.ClearWallAndOpenScrolls> ClearWallAndOpenScrolls, Action<CrocomireDeathStage.PublishDefeatAndRestoreMusic> PublishDefeatAndRestoreMusic, Action<CrocomireDeathStage.InertCorpse> InertCorpse);
