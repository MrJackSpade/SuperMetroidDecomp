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

    /// <summary>$A3:9703 / Spritemap_Sciser_UpsideUp_0 begins twelve four-object records, each 22 bytes.</summary>
    private const ushort FirstFrame = 0x9703;
    /// <summary>Builds the twelve native Sciser spritemap definitions in surface and animation-frame order.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var result = new EnemySpritemapDefinition[12];
        for (int surface = 0; surface < 4; surface++)
        {
            (int group, string name) = surface switch
            {
                0 => (1, "right"), 1 => (3, "left"), 2 => (2, "down"), _ => (0, "up"),
            };
            for (int frame = 0; frame < 3; frame++)
                result[surface * 3 + frame] = new(Bank, (ushort)(FirstFrame + (group * 3 + frame) * 22),
                    $"sciser_upside_{name}_{frame}");
        }
        return result;
    }
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
