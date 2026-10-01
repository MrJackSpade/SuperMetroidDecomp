using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Shitroid's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class ShitroidVisualDefinitions
{
    /// <summary>Shitroid instruction and ordinary OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Three Tourian baby Metroid drain and remorse compositions at $A9:F9A8..FAD8.</summary>
    internal const int FrameCount = 3;

    internal static ushort[] Operands() => Enumerable.Range(0,
        ShitroidInstructionProgramDefinitions.PresentationWordCount)
        .Select(ShitroidInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "shitroid");
}

