using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Kago's three distinct bank-$A8 OAM compositions. Its slow and hit-accelerated
/// instruction loops share these frames but retain separate compiled timings.
/// </summary>
internal static class KagoVisualDefinitions
{
    /// <summary>Native bank for the Kago enemy definition and spritemaps.</summary>
    internal const byte Bank = 0xa8;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xabda, "kago_cycle_0"),
        new(Bank, 0xabf0, "kago_cycle_1"),
        new(Bank, 0xac06, "kago_cycle_2"),
    ];

    /// <summary>Accepts only visual operands in Kago's two native loops.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (KagoInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Kago visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
