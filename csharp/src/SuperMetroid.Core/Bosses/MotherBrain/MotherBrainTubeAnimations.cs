using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling tubes' typed programs (#641), in place of their lists at $A9:8C69, six bytes a piece.</summary>
internal static class MotherBrainTubeAnimations
{
    /// <summary>Every piece's program: its pose for one update, then the native sleep keeps it.</summary>
    internal static AnimationProgramSet<MotherBrainTubePiece, MotherBrainTubeFrame, NoAnimationCue> Programs { get; } = new(
        Enum.GetValues<MotherBrainTubePiece>().ToDictionary(piece => piece, piece =>
            new[]
            {
                AnimationProgramStep<MotherBrainTubePiece, MotherBrainTubeFrame, NoAnimationCue>.Show(new MotherBrainTubeFrame((int)piece), 1),
                AnimationProgramStep<MotherBrainTubePiece, MotherBrainTubeFrame, NoAnimationCue>.Hold,
            }));
}
