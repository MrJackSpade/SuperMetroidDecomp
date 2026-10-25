using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's body programs.</summary>
internal abstract record MotherBrainBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainBodyCue()
    {
    }

    /// <summary>Publishes a body pose to the AI.</summary>
    /// <param name="Pose">The pose published.</param>
    internal sealed record SetPose(MotherBrainBodyPose Pose) : MotherBrainBodyCue;

    /// <summary>Moves the body, keeping BG2 aligned with it.</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record Move(Offset16<RoomPixels> Offset) : MotherBrainBodyCue;

    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    /// <param name="Offset">The body displacement.</param>
    internal sealed record StepAndMove(Offset16<RoomPixels> Offset) : MotherBrainBodyCue;

    /// <summary>Moves the body vertically, counter-scrolling BG2 with a horizontal bias.</summary>
    /// <param name="Y">The vertical move.</param>
    /// <param name="ScrollBias">The horizontal bias BG2 scrolls with.</param>
    internal sealed record MoveWithScrollBias(int Y, int ScrollBias) : MotherBrainBodyCue;

    /// <summary>Spawns a charge particle at the hand.</summary>
    /// <param name="Offset">Where the particle spawns, from the body.</param>
    /// <param name="Dust">The particle spawned.</param>
    internal sealed record SpawnHandDust(Offset16<RoomPixels> Offset, MiscDustAnimation Dust) : MotherBrainBodyCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(MotherBrainBodyCueKind Kind) : MotherBrainBodyCue;

    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new Command(MotherBrainBodyCueKind.FireHandBeam);

    /// <summary>Advances the hand-beam attack phase.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new Command(MotherBrainBodyCueKind.AdvanceHandBeamPhase);
}
