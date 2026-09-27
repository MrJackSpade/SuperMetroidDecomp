using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The twelve distinct body and wing OAM compositions selected by Dragon's six
/// compiled bank-$A2 programs. Animation timing and attack callbacks stay in
/// <see cref="DragonInstructionProgramDefinitions"/>.
/// </summary>
internal static class DragonVisualDefinitions
{
    /// <summary>Native Dragon spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xe80c, "dragon_body_idle_left"),
        new(Bank, 0xe8b4, "dragon_wing_left_0"),
        new(Bank, 0xe8bb, "dragon_wing_left_1"),
        new(Bank, 0xe8c2, "dragon_body_idle_right"),
        new(Bank, 0xe96a, "dragon_wing_right_0"),
        new(Bank, 0xe971, "dragon_wing_right_1"),
        new(Bank, 0xe836, "dragon_body_attack_left_0"),
        new(Bank, 0xe860, "dragon_body_attack_left_1"),
        new(Bank, 0xe88a, "dragon_body_attack_left_2"),
        new(Bank, 0xe8ec, "dragon_body_attack_right_0"),
        new(Bank, 0xe916, "dragon_body_attack_right_1"),
        new(Bank, 0xe940, "dragon_body_attack_right_2"),
    ];

    /// <summary>Resolves one of Dragon's sixteen compiled presentation operands.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < DragonInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (DragonInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Dragon visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
