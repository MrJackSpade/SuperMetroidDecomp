using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's body animations.</summary>
internal abstract partial record MotherBrainBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainBodyCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this mother brain body cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context);


    /// <summary>Publishes a body pose to the AI.</summary>
    /// <param name="Pose">The pose published.</param>
    internal sealed partial record SetPose(MotherBrainBodyPose Pose) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetPose(this, context);
    }

    /// <summary>Moves her body; her body's pictures follow it (#1949).</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed partial record Move(Offset16<RoomPixels> Offset) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Move(this, context);
    }

    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed partial record StepAndMove(Offset16<RoomPixels> Offset) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StepAndMove(this, context);
    }

    /// <summary>Spawns a charge particle at the hand.</summary>
    /// <param name="Offset">Where the particle spawns, from the body.</param>
    /// <param name="Dust">The particle spawned.</param>
    internal sealed partial record SpawnHandDust(Offset16<RoomPixels> Offset, DustExplosionAnimation Dust) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnHandDust(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(MotherBrainBodyCueKind Kind) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainBodyCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }

    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new Signal(MotherBrainBodyCueKind.FireHandBeam);

    /// <summary>Advances the hand-beam attack from firing to finishing.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new Signal(MotherBrainBodyCueKind.AdvanceHandBeamPhase);
}
