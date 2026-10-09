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

    /// <summary>Lists the native addresses of the two spritemap selectors used by Kraid's initial and post-growth lint poses.</summary>
    /// <returns>The ordered presentation operand addresses from the compiled instruction program.</returns>
    internal static ushort[] Operands() => Enumerable.Range(0,
        KraidLintInstructionProgramDefinitions.PresentationWordCount)
        .Select(KraidLintInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Builds the two ordinary OAM compositions selected for Kraid's belly-lint poses.</summary>
    /// <returns>The ordered initial and post-growth spritemap definitions resolved from bank $A7.</returns>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "kraid_lint");
}
