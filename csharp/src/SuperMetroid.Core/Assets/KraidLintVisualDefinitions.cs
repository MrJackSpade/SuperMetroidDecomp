using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Kraid's ordinary belly-lint compositions; timing and actor motion remain compiled.</summary>
internal static class KraidLintVisualDefinitions
{
    /// <summary>Native Kraid instruction and artwork bank $A7.</summary>
    internal const byte Bank = KraidFootVisualDefinitions.Bank;
    /// <summary>Initial <c>Spritemap_KraidLint_Initial</c> at $A7:A5DF, selected by $A7:8B00.</summary>
    internal const ushort InitialFrame = 0xa5df;
    /// <summary>Initial and post-growth ordinary OAM frames at $A7:A5DF and $A7:8C6C.</summary>
    internal const int FrameCount = 2;

    internal static ushort[] Operands() => Enumerable.Range(0,
        KraidLintInstructionProgramDefinitions.PresentationWordCount)
        .Select(KraidLintInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "kraid_lint");
}
