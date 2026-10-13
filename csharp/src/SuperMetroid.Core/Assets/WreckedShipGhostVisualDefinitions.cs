using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>WreckedShipGhost's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class WreckedShipGhostVisualDefinitions
{
    /// <summary>WreckedShipGhost instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Three Coven floating compositions at $A8:9E46..9E72.</summary>
    internal const int FrameCount = 3;

    /// <summary>Collects spritemap operands from the ghost's compiled presentation instructions.</summary>
    internal static ushort[] Operands() => Enumerable.Range(0,
        WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount)
        .Select(WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Resolves the three ordinary floating compositions from bank-$A8 operand words.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "wrecked_ship_ghost");
}

