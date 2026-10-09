using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Distinct bank-$AA OAM compositions selected by the Lower Norfair and Wrecked
/// Ship Chozo programs. Activation events, movement, and timing remain compiled
/// game logic; only the selected sprite pieces are authored presentation.
/// </summary>
internal static class ChozoStatueVisualDefinitions
{
    /// <summary>Native shared Chozo statue spritemap bank $AA.</summary>
    internal const byte Bank = 0xaa;
    /// <summary>Expected number of distinct bank-$AA frames selected across the four compiled Chozo statue programs.</summary>
    internal const int FrameCount = 26;

    /// <summary>Builds the unique spritemap definitions referenced by Chozo statue presentation operands.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var seen = new HashSet<ushort>();
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ChozoStatueInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort pointer = FrameAt(operand);
            if (seen.Add(pointer))
                frames.Add(new EnemySpritemapDefinition(Bank, pointer,
                    $"chozo_statue_aa_{pointer:x4}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Chozo statue selects {frames.Count} distinct frames, expected {FrameCount}.");
        return frames.ToArray();
    }

    /// <summary>Resolves only a compiled presentation operand of the four native lists.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (ChozoStatueInstructionProgramDefinitions
                    .PresentationWordAddress(index) == operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Chozo statue visual operand $AA:{operandAddress:X4} is not compiled.");
    }
}
