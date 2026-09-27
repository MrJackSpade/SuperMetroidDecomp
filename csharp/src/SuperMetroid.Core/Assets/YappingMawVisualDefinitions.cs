using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Presentation identities selected by Yapping Maw's bank-$A8 programs.</summary>
internal static class YappingMawVisualDefinitions
{
    /// <summary>$A8, the bank containing Yapping Maw's instruction and OAM data.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>
    /// The 24 distinct $A8:A8EE-$AAED spritemaps selected by the 52 visual
    /// operands in Yapping Maw's attack/cooldown programs. The native address
    /// remains the stable identity while the extracted composition is editable.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var pointers = new SortedSet<ushort>();
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
             index++)
            pointers.Add(FrameAt(YappingMawInstructionProgramDefinitions.PresentationWordAddress(index)));
        return [.. pointers.Select(pointer => new EnemySpritemapDefinition(
            Bank, pointer, $"yapping_maw_a8_{pointer:x4}"))];
    }

    /// <summary>Reads a compiled presentation selector, never adjacent mechanics.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (YappingMawInstructionProgramDefinitions.PresentationWordAddress(index) ==
                operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
                return frame;
        }
        throw new InvalidDataException(
            $"Yapping Maw visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
