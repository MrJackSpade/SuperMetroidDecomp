using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Hibashi's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class HibashiVisualDefinitions
{
    /// <summary>Hibashi instruction and ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The fire pillar's twenty-three selected OAM frames at $A6:9082..9469.</summary>
    internal const int FrameCount = 23;

    /// <summary>Lists the instruction-program addresses whose spritemap selectors provide Hibashi's editable frame identities.</summary>
    /// <returns>Ordered native operand addresses for the fire pillar's presentation frames.</returns>
    internal static ushort[] Operands() => Enumerable.Range(0,
        HibashiInstructionProgramDefinitions.PresentationWordCount)
        .Select(HibashiInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Builds Hibashi's ordinary OAM compositions from the compiled selector addresses and the declared frame count.</summary>
    /// <returns>The ordered fire-pillar spritemap definitions resolved from bank $A6.</returns>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "hibashi");
}

