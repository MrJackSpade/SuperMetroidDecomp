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
        if (BotwoonInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            if (operandAddress == BotwoonInstructionProgramDefinitions.Hidden + 2)
                return CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            bool spitting = operandAddress >= BotwoonInstructionProgramDefinitions.SpittingUpLeft;
            int relative = operandAddress - (spitting ? BotwoonInstructionProgramDefinitions.SpittingUpLeft : BotwoonInstructionProgramDefinitions.MovingUpLeft);
            int direction = relative / (spitting ? 16 : 8);
            bool open = spitting && relative % 16 == 12;
            // Closed maps all have two entries: two-byte count plus ten sprite bytes.
            if (!open) return (ushort)(0xe329 + 12 * direction);
            int pointer = 0xe3a1 + 12 * direction;
            // Each prior diagonal open-mouth map contributes a third five-byte piece.
            for (int prior = 0; prior < direction; prior++)
            {
                int movement = BotwoonInstructionProgramDefinitions.MovingUpLeft + 8 * prior;
                if (movement is BotwoonInstructionProgramDefinitions.MovingUpLeft or
                    BotwoonInstructionProgramDefinitions.MovingDownLeft or
                    BotwoonInstructionProgramDefinitions.MovingDownRight or
                    BotwoonInstructionProgramDefinitions.MovingUpRight)
                    pointer += 5;
            }
            return (ushort)pointer;
        }
        throw new InvalidDataException(
            $"Botwoon head visual operand $B3:{operandAddress:X4} is not compiled.");
    }
}
