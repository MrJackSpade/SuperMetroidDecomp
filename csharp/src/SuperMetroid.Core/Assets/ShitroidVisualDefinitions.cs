using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Shitroid's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class ShitroidVisualDefinitions
{
    /// <summary>Shitroid instruction and ordinary OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Three Tourian baby Metroid drain and remorse compositions at $A9:F9A8..FAD8.</summary>
    internal const int FrameCount = 3;

    /// <summary>Collects only the presentation operands consumed by the compiled Tourian baby Metroid program, in instruction order.</summary>
    internal static ushort[] Operands() => Enumerable.Range(0,
        ShitroidInstructionProgramDefinitions.PresentationWordCount)
        .Select(ShitroidInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Resolves the three selected OAM compositions from bank $A9 while leaving drain behavior in the compiled program.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "shitroid");
}

