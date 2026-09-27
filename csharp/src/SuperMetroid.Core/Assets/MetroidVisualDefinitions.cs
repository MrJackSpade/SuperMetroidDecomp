using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Four bank-$A3 ordinary-Metroid body compositions shared by the chasing and
/// draining loops. Animation cadence, cry callbacks, contact, and the separate
/// outer-body sprite objects remain engine-owned behavior.
/// </summary>
internal static class MetroidVisualDefinitions
{
    /// <summary>Native ordinary-Metroid body spritemap bank.</summary>
    internal const byte Bank = 0xa3;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xf10d, "metroid_body_0"),
        new(Bank, 0xf137, "metroid_body_1"),
        new(Bank, 0xf157, "metroid_body_2"),
        new(Bank, 0xf181, "metroid_body_3"),
    ];

    /// <summary>Resolves only the native chasing and draining visual operands.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (MetroidInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Ordinary-Metroid visual operand $A3:{operandAddress:X4} is not compiled.");
    }
}
