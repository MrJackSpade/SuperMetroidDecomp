using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's body animations.</summary>
internal abstract record MotherBrainBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainBodyCue()
    {
    }

    /// <summary>Publishes a body pose to the AI.</summary>
    /// <param name="Pose">The pose published.</param>
    internal sealed record SetPose(MotherBrainBodyPose Pose) : MotherBrainBodyCue;

    /// <summary>Moves her body; her body's pictures follow it (#1949).</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record Move(Offset16<RoomPixels> Offset) : MotherBrainBodyCue;

    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record StepAndMove(Offset16<RoomPixels> Offset) : MotherBrainBodyCue;

    /// <summary>Spawns a charge particle at the hand.</summary>
    /// <param name="Offset">Where the particle spawns, from the body.</param>
    /// <param name="Dust">The particle spawned.</param>
    internal sealed record SpawnHandDust(Offset16<RoomPixels> Offset, DustExplosionAnimation Dust) : MotherBrainBodyCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainBodyCueKind Kind) : MotherBrainBodyCue;

    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new Signal(MotherBrainBodyCueKind.FireHandBeam);

    /// <summary>Advances the hand-beam attack from firing to finishing.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new Signal(MotherBrainBodyCueKind.AdvanceHandBeamPhase);
}
