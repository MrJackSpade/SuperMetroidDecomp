using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's body animations.</summary>
internal abstract record MotherBrainBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainBodyCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this mother brain body cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(MotherBrainBodyCueReactions reactions);


    /// <summary>Publishes a body pose to the AI.</summary>
    /// <param name="Pose">The pose published.</param>
    internal sealed record SetPose(MotherBrainBodyPose Pose) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainBodyCueReactions reactions) => Ensure.NotNull(reactions).SetPose(this);
    }

    /// <summary>Moves her body; her body's pictures follow it (#1949).</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record Move(Offset16<RoomPixels> Offset) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainBodyCueReactions reactions) => Ensure.NotNull(reactions).Move(this);
    }

    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record StepAndMove(Offset16<RoomPixels> Offset) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainBodyCueReactions reactions) => Ensure.NotNull(reactions).StepAndMove(this);
    }

    /// <summary>Spawns a charge particle at the hand.</summary>
    /// <param name="Offset">Where the particle spawns, from the body.</param>
    /// <param name="Dust">The particle spawned.</param>
    internal sealed record SpawnHandDust(Offset16<RoomPixels> Offset, DustExplosionAnimation Dust) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainBodyCueReactions reactions) => Ensure.NotNull(reactions).SpawnHandDust(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainBodyCueKind Kind) : MotherBrainBodyCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainBodyCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }

    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new Signal(MotherBrainBodyCueKind.FireHandBeam);

    /// <summary>Advances the hand-beam attack from firing to finishing.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new Signal(MotherBrainBodyCueKind.AdvanceHandBeamPhase);
}

/// <summary>What the owner of a <see cref="MotherBrainBodyCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetPose">Reacts to a <see cref="MotherBrainBodyCue.SetPose"/>.</param>
/// <param name="Move">Reacts to a <see cref="MotherBrainBodyCue.Move"/>.</param>
/// <param name="StepAndMove">Reacts to a <see cref="MotherBrainBodyCue.StepAndMove"/>.</param>
/// <param name="SpawnHandDust">Reacts to a <see cref="MotherBrainBodyCue.SpawnHandDust"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MotherBrainBodyCue.Signal"/>.</param>
internal sealed record MotherBrainBodyCueReactions(Action<MotherBrainBodyCue.SetPose> SetPose, Action<MotherBrainBodyCue.Move> Move, Action<MotherBrainBodyCue.StepAndMove> StepAndMove, Action<MotherBrainBodyCue.SpawnHandDust> SpawnHandDust, Action<MotherBrainBodyCue.Signal> Signal);
