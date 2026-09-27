using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Sciser's twelve distinct bank-$A3 OAM compositions. The native four-surface
/// instruction programs choose these frames; their timing and movement remain
/// compiled enemy behavior.
/// </summary>
internal static class SciserVisualDefinitions
{
    /// <summary>Native Sciser spritemap bank $A3.</summary>
    internal const byte Bank = 0xa3;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0x9745, "sciser_upside_right_0"),
        new(Bank, 0x975b, "sciser_upside_right_1"),
        new(Bank, 0x9771, "sciser_upside_right_2"),
        new(Bank, 0x97c9, "sciser_upside_left_0"),
        new(Bank, 0x97df, "sciser_upside_left_1"),
        new(Bank, 0x97f5, "sciser_upside_left_2"),
        new(Bank, 0x9787, "sciser_upside_down_0"),
        new(Bank, 0x979d, "sciser_upside_down_1"),
        new(Bank, 0x97b3, "sciser_upside_down_2"),
        new(Bank, 0x9703, "sciser_upside_up_0"),
        new(Bank, 0x9719, "sciser_upside_up_1"),
        new(Bank, 0x972f, "sciser_upside_up_2"),
    ];

    /// <summary>Only the sixteen visual operands in Sciser's four native loops.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (SciserInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Sciser visual operand $A3:{operandAddress:X4} is not compiled.");
    }
}
