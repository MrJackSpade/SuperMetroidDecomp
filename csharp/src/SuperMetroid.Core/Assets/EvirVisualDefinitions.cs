using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Evir body, arms, and projectile compositions selected by six compiled
/// bank-$A8 programs. The three-slot movement, attack, and regeneration logic
/// remains owned by <see cref="EvirInstructionProgramDefinitions"/>.
/// </summary>
internal static class EvirVisualDefinitions
{
    /// <summary>Evir's native instruction and spritemap bank, $A8.</summary>
    internal const byte Bank = 0xa8;
    internal const int FrameCount = 24;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0x8b59, "evir_body_left_0"),
        new(Bank, 0x8b88, "evir_body_left_1"),
        new(Bank, 0x8bb7, "evir_body_left_2"),
        new(Bank, 0x8be6, "evir_body_left_3"),
        new(Bank, 0x8c15, "evir_body_left_4"),
        new(Bank, 0x8c44, "evir_body_left_5"),
        new(Bank, 0x8ca2, "evir_arms_left_0"),
        new(Bank, 0x8cbd, "evir_arms_left_1"),
        new(Bank, 0x8cd8, "evir_arms_left_2"),
        new(Bank, 0x8cf3, "evir_arms_left_3"),
        new(Bank, 0x8d13, "evir_arms_left_4"),
        new(Bank, 0x8d81, "evir_body_right_0"),
        new(Bank, 0x8db0, "evir_body_right_1"),
        new(Bank, 0x8ddf, "evir_body_right_2"),
        new(Bank, 0x8e0e, "evir_body_right_3"),
        new(Bank, 0x8e3d, "evir_body_right_4"),
        new(Bank, 0x8e6c, "evir_body_right_5"),
        new(Bank, 0x8eca, "evir_arms_right_0"),
        new(Bank, 0x8ee5, "evir_arms_right_1"),
        new(Bank, 0x8f00, "evir_arms_right_2"),
        new(Bank, 0x8f1b, "evir_arms_right_3"),
        new(Bank, 0x8f3b, "evir_arms_right_4"),
        new(Bank, 0x8d7a, "evir_projectile_flight"),
        new(Bank, 0x8d64, "evir_projectile_regenerating"),
    ];

    /// <summary>Resolves only Evir's 49 presentation operands, not adjacent AI words.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (EvirInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        }
        throw new InvalidDataException(
            $"Evir visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
