using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>WreckedShipGhost's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class WreckedShipGhostVisualDefinitions
{
    /// <summary>WreckedShipGhost instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Three Coven floating compositions at $A8:9E46..9E72.</summary>
    internal const int FrameCount = 3;

    /// <summary>Returns the presentation-word addresses selected by the compiled ghost instruction program.</summary>
    /// <returns>Bank-local addresses of every visual selector operand in program order.</returns>
    internal static ushort[] Operands() => Enumerable.Range(0,
        WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount)
        .Select(WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Builds the three distinct nonempty OAM frame definitions selected by the ghost program.</summary>
    /// <returns>Named spritemap definitions for the installed Wrecked Ship ghost compositions.</returns>
    /// <exception cref="InvalidDataException">A selector is uncompiled or the program selects a different number of visible compositions.</exception>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "wrecked_ship_ghost");
}

