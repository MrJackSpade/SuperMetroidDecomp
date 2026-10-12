using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Five distinct bank-$A2 OAM compositions shared by the ordinary and
/// Mother Brain Rinka loops. Their callbacks, timing and branches remain
/// compiled simulation definitions.
/// </summary>
internal static class RinkaVisualDefinitions
{
    /// <summary>Native Rinka spritemap bank.</summary>
    internal const byte Bank = 0xa2;
    internal const int FrameCount = 5;

    internal static EnemySpritemapDefinition[] Frames()
    {
        var seen = new HashSet<ushort>();
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        for (int index = 0; index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort pointer = FrameAt(operand);
            if (seen.Add(pointer))
                frames.Add(new EnemySpritemapDefinition(Bank, pointer,
                    $"rinka_spin_{pointer:x4}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Rinka selects {frames.Count} distinct frames, expected {FrameCount}.");
        return frames.ToArray();
    }

    /// <summary>Resolves only an authored Rinka visual operand.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (RinkaInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Rinka visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
