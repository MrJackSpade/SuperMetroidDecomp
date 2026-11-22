using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One stage of Crocomire's course from his fight to his corpse (#2540), as
/// <see cref="CrocomireDeathStageDefinitions.Stages"/> lists them in the order he goes through them: a stage that finishes
/// hands over to the one after it. A stage runs on each of his body's updates until it finishes.
/// </summary>
internal abstract partial record CrocomireDeathStage
{
    /// <summary>Allows only the stages below.</summary>
    private CrocomireDeathStage()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this crocomire death stage, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context);


    /// <summary>The fight runs, with its bridge checks: his death has not begun. A new body starts here.</summary>
    internal sealed partial record Fighting : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Fighting(this, context);
    }

    /// <summary>
    /// The bridge has given way: he sinks through it, whole, into the acid as its fragments fall one an update, until he
    /// rests at the acid's floor.
    /// </summary>
    internal sealed partial record SinkThroughBridge : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SinkThroughBridge(this, context);
    }

    /// <summary>
    /// He sinks into the acid, the acid smoking about him, until he rests at its floor; the submerged pause after it then
    /// starts its wait.
    /// </summary>
    /// <param name="Body">What his body is as he sinks.</param>
    internal sealed partial record Sink(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Sink(this, context);
    }

    /// <summary>He rests at the acid's floor while the wait his sink started runs, and starts to rise as it runs out.</summary>
    /// <param name="Body">What his body is as he rests.</param>
    internal sealed partial record SubmergedPause(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SubmergedPause(this, context);
    }

    /// <summary>He rises out of the acid, the acid smoking about him, until he has risen above its surface.</summary>
    /// <param name="Body">What his body is as he rises.</param>
    internal sealed partial record Rise(CrocomireBodyInAcid Body) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Rise(this, context);
    }

    /// <summary>His body shows <paramref name="Picture"/>'s melt image in place of his body frame, for one update.</summary>
    /// <param name="Picture">The melting illustration shown.</param>
    internal sealed partial record ShowMeltImage(CrocomireMeltingIllustration Picture) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShowMeltImage(this, context);
    }

    /// <summary>The melt pass of <paramref name="Picture"/> begins, for one update, its melt's stretch levelled.</summary>
    /// <param name="Picture">The melting illustration the pass shows and dissolves.</param>
    internal sealed partial record BeginMeltPass(CrocomireMeltingIllustration Picture) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).BeginMeltPass(this, context);
    }

    /// <summary>The pass's melt image shows one band an update, finishing on the update after its last band.</summary>
    internal sealed partial record ShowMeltBands : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShowMeltBands(this, context);
    }

    /// <summary>His death holds for one update and does nothing else.</summary>
    internal sealed partial record PauseOneUpdate : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PauseOneUpdate(this, context);
    }

    /// <summary>He cries out as his melt image starts to dissolve and the melt's stretch starts, for one update.</summary>
    /// <param name="Cry">The sound he cries out with.</param>
    internal sealed partial record StartDissolve(SoundEffectId Cry) : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StartDissolve(this, context);
    }

    /// <summary>His melt image dissolves a column an update, shaking, while the melt's stretch runs to its end.</summary>
    internal sealed partial record Dissolve : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Dissolve(this, context);
    }

    /// <summary>His body shows nothing, and the sink after it starts from rest, for one update.</summary>
    internal sealed partial record ClearMeltImage : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ClearMeltImage(this, context);
    }

    /// <summary>
    /// He sinks a last time, showing his bare skull's melting picture; at the acid's floor his skeleton is sent down the
    /// river, the boss tension music starts and the arena opens behind him.
    /// </summary>
    internal sealed partial record FinalSink : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).FinalSink(this, context);
    }

    /// <summary>His skeleton drifts left along the acid river until it comes to rest behind the arena's wall.</summary>
    internal sealed partial record FlowDownRiver : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).FlowDownRiver(this, context);
    }

    /// <summary>His skeleton waits behind the wall until Samus comes back left.</summary>
    internal sealed partial record WaitForSamusAtWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).WaitForSamusAtWall(this, context);
    }

    /// <summary>The hidden wall rumbles through its oscillations.</summary>
    internal sealed partial record RumbleHiddenWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).RumbleHiddenWall(this, context);
    }

    /// <summary>His skeleton is bared behind the spiked wall, then breaks through it.</summary>
    internal sealed partial record BreakSpikeWall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).BreakSpikeWall(this, context);
    }

    /// <summary>His skeleton bursts out of the wall until its fall's wait runs out.</summary>
    internal sealed partial record DelaySkeletonFall : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).DelaySkeletonFall(this, context);
    }

    /// <summary>His skeleton arcs back into the arena until it lands.</summary>
    internal sealed partial record ArcSkeletonIntoArena : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ArcSkeletonIntoArena(this, context);
    }

    /// <summary>His skeleton falls apart; once it has, his dead skeleton is laid down and his drops scatter.</summary>
    internal sealed partial record CollapseSkeleton : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).CollapseSkeleton(this, context);
    }

    /// <summary>The arena's screens reopen and the spiked wall clears, for one update.</summary>
    internal sealed partial record ClearWallAndOpenScrolls : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ClearWallAndOpenScrolls(this, context);
    }

    /// <summary>
    /// The mini-boss's defeat is recorded, the boss tension music restored and the camera's tracking returned, for one
    /// update.
    /// </summary>
    internal sealed partial record PublishDefeatAndRestoreMusic : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PublishDefeatAndRestoreMusic(this, context);
    }

    /// <summary>
    /// His corpse, inert: the live room's once his defeat is recorded, and the already-defeated room's from its setup. It
    /// never hands over.
    /// </summary>
    internal sealed partial record InertCorpse : CrocomireDeathStage
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CrocomireDeathStageReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).InertCorpse(this, context);
    }
}
