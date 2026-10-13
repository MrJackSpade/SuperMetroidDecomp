using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five glass-tube pieces that fall when Mother Brain's tank collapses, in the order of their bank-$A9
/// population records ($A9:8AE5, sixteen bytes each). Each piece also has its own still-pose program.
/// </summary>
public enum MotherBrainTubePiece
{
    /// <summary>The bottom-left outer piece.</summary>
    BottomLeft,
    /// <summary>The bottom-right outer piece.</summary>
    BottomRight,
    /// <summary>The bottom-middle-left inner piece.</summary>
    BottomMiddleLeft,
    /// <summary>The bottom-middle-right inner piece.</summary>
    BottomMiddleRight,
    /// <summary>The main tube, which waits before falling.</summary>
    Main,
}

/// <summary>A falling tube's pose, an index into the tube compositions of <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The piece whose pose this is.</param>
public readonly record struct MotherBrainTubeFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => MotherBrainVisualDefinitions.TubeFrame(Index).Pointer;
}

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
