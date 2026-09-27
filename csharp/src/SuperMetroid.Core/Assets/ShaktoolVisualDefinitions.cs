using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The fifteen distinct bank-$AA OAM compositions selected by Shaktool's seven
/// articulated enemy records. The native instruction programs own timing and
/// segment motion; these definitions only identify editable visual frames.
/// </summary>
internal static class ShaktoolVisualDefinitions
{
    /// <summary>Native Shaktool spritemap bank $AA.</summary>
    internal const byte Bank = 0xaa;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xdf5c, "shaktool_final_saw_0"),
        new(Bank, 0xdf63, "shaktool_final_saw_1"),
        new(Bank, 0xdf6a, "shaktool_final_saw_2"),
        new(Bank, 0xdf71, "shaktool_arm"),
        new(Bank, 0xdf78, "shaktool_head_left"),
        new(Bank, 0xdf8e, "shaktool_head_up_left"),
        new(Bank, 0xdfa4, "shaktool_head_up"),
        new(Bank, 0xdfba, "shaktool_head_up_right"),
        new(Bank, 0xdfd0, "shaktool_head_right"),
        new(Bank, 0xdfe6, "shaktool_head_down_right"),
        new(Bank, 0xdffc, "shaktool_head_down"),
        new(Bank, 0xe012, "shaktool_head_down_left"),
        new(Bank, 0xe028, "shaktool_primary_saw_0"),
        new(Bank, 0xe02f, "shaktool_primary_saw_1"),
        new(Bank, 0xe036, "shaktool_primary_saw_2"),
    ];

    /// <summary>Resolves only presentation words in Shaktool's bank-$AA programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (ShaktoolInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Shaktool visual operand $AA:{operandAddress:X4} is not compiled.");
    }
}
