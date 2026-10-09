using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Distinct OAM frame identities selected by the parent and follower lava-
/// jumper programs. The hidden frame is the bank-local empty spritemap;
/// movement, callback timing, and looping remain compiled gameplay logic.
/// </summary>
internal static class NorfairLavaJumperVisualDefinitions
{
    /// <summary>Native Norfair lava-jumper spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Expected number of distinct OAM roots selected across the parent and follower programs.</summary>
    internal const int FrameCount = 11;

    /// <summary>Builds the ordered distinct spritemap definitions selected by the compiled presentation operands.</summary>
    /// <returns>All eleven selected frames, each named from its bank-local pointer.</returns>
    /// <exception cref="InvalidDataException">The compiled operands do not resolve to exactly eleven distinct frames.</exception>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var seen = new HashSet<ushort>();
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = NorfairLavaJumperInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort pointer = FrameAt(operand);
            if (seen.Add(pointer))
                frames.Add(new EnemySpritemapDefinition(Bank, pointer,
                    $"norfair_lava_jumper_{pointer:x4}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Norfair lava jumper selects {frames.Count} distinct frames, expected {FrameCount}.");
        return frames.ToArray();
    }

    /// <summary>Resolves only a compiled visual operand of the three native programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (NorfairLavaJumperInstructionProgramDefinitions
                    .PresentationWordAddress(index) == operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Norfair lava-jumper visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
