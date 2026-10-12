using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Puromi/Nuclear Waffle's ordinary head compositions, separate from its projectile links.</summary>
internal static class NuclearWaffleVisualDefinitions
{
    /// <summary>Native Puromi instruction and head OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Eight distinct head frames at $A6:9954..9985 selected by the twelve-frame loop.</summary>
    internal const int FrameCount = 8;

    internal static ushort[] Operands() => Enumerable.Range(0,
        NuclearWaffleInstructionProgramDefinitions.PresentationWordCount)
        .Select(NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "nuclear_waffle");
}
