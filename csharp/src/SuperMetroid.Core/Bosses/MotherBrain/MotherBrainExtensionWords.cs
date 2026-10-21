using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Mother Brain falling-tube words kept per physical slot rather than per occupant, like the native per-slot RAM they
/// model. The port never resets them, so words an initializer does not write keep the previous occupant's values,
/// even from an earlier room.
/// </summary>
internal sealed class MotherBrainExtensionWords
{
    /// <summary>Where the falling tube's program resumes (#641).</summary>
    internal AnimationProgramCursor<MotherBrainTubePiece> FallingTubeCursor { get; set; }
}
