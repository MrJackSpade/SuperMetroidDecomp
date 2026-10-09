using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Eight distinct bank-$A2 OAM compositions in Multiviola's fourteen-frame
/// flight loop. Durations and the loop branch remain compiled simulation data.
/// </summary>
internal static class MultiviolaVisualDefinitions
{
    /// <summary>Native Multiviola spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Number of distinct visual operands used by the flight loop, excluding repeated instructions.</summary>
    internal const int FrameCount = 8;

    /// <summary>Builds the ordered visual definitions for the flight loop's distinct spritemap operands.</summary>
    /// <returns>One bank-$A2 definition per compiled presentation word, named by its spin-frame index.</returns>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new EnemySpritemapDefinition[FrameCount];
        for (int index = 0; index < frames.Length; index++)
        {
            ushort operand = MultiviolaInstructionProgramDefinitions
                .PresentationWordAddress(index);
            frames[index] = new EnemySpritemapDefinition(Bank, FrameAt(operand),
                $"multiviola_spin_{index}");
        }
        return frames;
    }

    /// <summary>Resolves only a visual operand of the native flight loop.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < MultiviolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (MultiviolaInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Multiviola visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
