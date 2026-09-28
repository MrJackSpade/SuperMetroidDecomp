using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Named Yard poses selected by its 38 authored crawling, turn, hiding, and
/// airborne programs. The programs' movement, timing, and callbacks remain
/// compiled mechanics; only their OAM presentation is replaceable.
/// </summary>
internal static class YardVisualDefinitions
{
    /// <summary>Yard's instruction and spritemap bank, $A3.</summary>
    internal const byte Bank = 0xa3;
    internal const int FrameCount = 104;

    private static readonly (ushort Entry, string Name)[] Groups =
    [
        (YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp, "outside_turn_right_up"),
        (YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft, "crawl_top_left"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft, "outside_turn_top_left"),
        (YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown, "crawl_left_down"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown, "outside_turn_left_down"),
        (YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight, "crawl_bottom_right"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight, "outside_turn_bottom_right"),
        (YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp, "crawl_right_up"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp, "outside_turn_left_up"),
        (YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight, "crawl_top_right"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight, "outside_turn_top_right"),
        (YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown, "crawl_right_down"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown, "outside_turn_right_down"),
        (YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft, "crawl_bottom_left"),
        (YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft, "outside_turn_bottom_left"),
        (YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp, "crawl_left_up"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft, "inside_turn_top_left"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp, "inside_turn_right_up"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight, "inside_turn_bottom_right"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown, "inside_turn_left_down"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight, "inside_turn_top_right"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp, "inside_turn_left_up"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft, "inside_turn_bottom_left"),
        (YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown, "inside_turn_right_down"),
        (YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft, "hiding_top_left"),
        (YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft, "hidden_top_left"),
        (YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft, "hiding_bottom_left"),
        (YardInstructionProgramDefinitions.HidingUpsideDownMovingRight, "hiding_bottom_right"),
        (YardInstructionProgramDefinitions.HidingUpsideUpMovingRight, "hiding_top_right"),
        (YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight, "hidden_top_right"),
        (YardInstructionProgramDefinitions.HidingUpsideRightMovingUp, "hiding_right_up"),
        (YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp, "hiding_left_up"),
        (YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown, "hiding_left_down"),
        (YardInstructionProgramDefinitions.HidingUpsideRightMovingDown, "hiding_right_down"),
        (YardInstructionProgramDefinitions.AirborneFacingLeft, "airborne_left_initial"),
        (YardInstructionProgramDefinitions.AirborneFacingLeftLoop, "airborne_left_loop"),
        (YardInstructionProgramDefinitions.AirborneFacingRight, "airborne_right_initial"),
        (YardInstructionProgramDefinitions.AirborneFacingRightLoop, "airborne_right_loop"),
    ];

    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seenPointers = new HashSet<ushort>();
        int groupIndex = 0;
        int frameIndex = 0;
        for (int index = 0;
             index < YardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            while (groupIndex + 1 < Groups.Length &&
                   operand >= Groups[groupIndex + 1].Entry)
            {
                groupIndex++;
                frameIndex = 0;
            }
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Yard visual operand $A3:{operand:X4} is not compiled.");
            if (seenPointers.Add(pointer))
                frames.Add(new(Bank, pointer,
                    $"yard_{Groups[groupIndex].Name}_{frameIndex:00}"));
            frameIndex++;
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Yard visual catalog has {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return frames.ToArray();
    }

    /// <summary>Resolves only Yard's 112 visual operands, not neighboring AI words.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (YardInstructionProgramDefinitions.IsPresentationWordAddress(
                operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Yard visual operand $A3:{operandAddress:X4} is not compiled.");
    }
}
