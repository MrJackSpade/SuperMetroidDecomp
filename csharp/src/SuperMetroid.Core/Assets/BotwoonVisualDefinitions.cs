using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Botwoon's named head compositions. The head's instruction timers, radius changes,
/// spit callbacks, and hitboxes remain compiled gameplay definitions; these records
/// supply only editable OAM parts for the cartridge-selected visual frames.
/// </summary>
internal static class BotwoonVisualDefinitions
{
    /// <summary>Botwoon's head instruction and OAM bank, $B3.</summary>
    internal const byte Bank = 0xb3;
    internal const int FrameCount = 16;

    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seen = new HashSet<ushort>();
        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort pointer = FrameAt(
                BotwoonInstructionProgramDefinitions.PresentationWordAddress(index));
            // The native hidden frame is the shared, zero-part bank-local OAM record.
            if (pointer != CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap &&
                seen.Add(pointer))
                frames.Add(new(Bank, pointer,
                    $"botwoon_head_{frames.Count:D2}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Botwoon head has {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return [.. frames];
    }

    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (operandAddress != BotwoonInstructionProgramDefinitions
                    .PresentationWordAddress(index))
                continue;
            if (CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort pointer))
                return pointer;
            break;
        }
        throw new InvalidDataException(
            $"Botwoon head visual operand $B3:{operandAddress:X4} is not compiled.");
    }
}
