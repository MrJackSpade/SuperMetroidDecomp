using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Puromi/Nuclear Waffle's ordinary head compositions, separate from its projectile links.</summary>
internal static class NuclearWaffleVisualDefinitions
{
    /// <summary>Native Puromi instruction and head OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Eight distinct head frames at $A6:9954..9985 selected by the twelve-frame loop.</summary>
    internal const int FrameCount = 8;

    /// <summary>Collects Puromi's presentation operands from its compiled instruction sequence, preserving operand order and duplicates.</summary>
    internal static ushort[] Operands() => Enumerable.Range(0,
        NuclearWaffleInstructionProgramDefinitions.PresentationWordCount)
        .Select(NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Resolves the eight bank-$A6 head compositions used by Puromi's twelve-step animation loop.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "nuclear_waffle");
}
