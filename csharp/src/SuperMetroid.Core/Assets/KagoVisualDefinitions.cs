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

    /// <summary>Spritemap_Kago_0 at $A8:ABDA begins three consecutive four-part OAM records.</summary>
    private const ushort FirstFrame = 0xabda;

    /// <summary>Each composition occupies its two-byte count plus four five-byte parts.</summary>
    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int phase = 0; phase < 3; phase++)
            yield return new(Bank, (ushort)(FirstFrame + phase * (2 + 4 * 5)), $"kago_cycle_{phase}");
    }

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
