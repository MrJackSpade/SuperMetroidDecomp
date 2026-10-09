using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The eight distinct bank-$A3 OAM compositions selected by Viola's fourteen-frame
/// normal loop. The compiled instruction catalog still owns durations and branches.
/// </summary>
internal static class ViolaVisualDefinitions
{
    /// <summary>Native Viola spritemap bank.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>Number of distinct OAM compositions selected by the normal loop.</summary>
    internal const int FrameCount = 8;

    /// <summary>Builds one definition for each distinct spritemap selected by Viola's normal loop.</summary>
    /// <returns>Unique bank-$A3 frame definitions in first-selector order.</returns>
    /// <exception cref="InvalidDataException">The compiled selectors do not resolve to the expected number of frames.</exception>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var seen = new HashSet<ushort>();
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        for (int index = 0; index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort pointer = FrameAt(operand);
            if (seen.Add(pointer))
                frames.Add(new EnemySpritemapDefinition(Bank, pointer,
                    $"viola_spin_{pointer:x4}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Viola selects {frames.Count} distinct frames, expected {FrameCount}.");
        return frames.ToArray();
    }

    /// <summary>Resolves only a compiled visual operand of Viola's normal loop.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (ViolaInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Viola visual operand $A3:{operandAddress:X4} is not compiled.");
    }
}
