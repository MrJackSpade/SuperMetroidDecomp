using static SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailInstructionDefinitions;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Chooses Oum's program for its authored animation index.</summary>
internal static class MaridiaLargeSnailPrograms
{
    /// <summary>The program of animation <paramref name="animationIndex"/>; the complete domain is zero through seven.</summary>
    internal static MaridiaLargeSnailProgram Program(int animationIndex) =>
        MaridiaLargeSnailInstructionDefinitions.Programs.TryGetValue(Animations.At(animationIndex), out MaridiaLargeSnailProgram program)
            ? program
            : throw new ArgumentOutOfRangeException(nameof(animationIndex), animationIndex,
                "Maridia Large Snail animation index must be zero through seven.");
}
