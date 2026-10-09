using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Zebetite's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class ZebetiteVisualDefinitions
{
    /// <summary>Zebetite instruction and ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Ten barrier health-tier compositions at $A6:FE08..FEB0.</summary>
    internal const int FrameCount = 10;

    /// <summary>Collects the presentation-word addresses that select Zebetite's barrier OAM frames.</summary>
    /// <returns>Instruction operand addresses in native program order.</returns>
    internal static ushort[] Operands() => Enumerable.Range(0,
        ZebetiteInstructionProgramDefinitions.PresentationWordCount)
        .Select(ZebetiteInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Builds the ten named compositions used for Zebetite's successive barrier health tiers.</summary>
    /// <returns>Compiled bank-$A6 spritemap definitions selected by the program's presentation operands.</returns>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "zebetite");
}

