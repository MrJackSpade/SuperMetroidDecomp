using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Hibashi's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class HibashiVisualDefinitions
{
    /// <summary>Hibashi instruction and ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The fire pillar's twenty-three selected OAM frames at $A6:9082..9469.</summary>
    internal const int FrameCount = 23;

    internal static ushort[] Operands() => Enumerable.Range(0,
        HibashiInstructionProgramDefinitions.PresentationWordCount)
        .Select(HibashiInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "hibashi");
}

