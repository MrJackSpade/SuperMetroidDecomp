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

    /// <summary>Maps a native bank-$A3 presentation operand to its containing named crawling/turn/hiding/airborne program; boundaries are the existing instruction entry identities.</summary>
    private static string GroupName(ushort operand) => operand switch
    {
        >= YardInstructionProgramDefinitions.AirborneFacingRightLoop => "airborne_right_loop",
        >= YardInstructionProgramDefinitions.AirborneFacingRight => "airborne_right_initial",
        >= YardInstructionProgramDefinitions.AirborneFacingLeftLoop => "airborne_left_loop",
        >= YardInstructionProgramDefinitions.AirborneFacingLeft => "airborne_left_initial",
        >= YardInstructionProgramDefinitions.HidingUpsideRightMovingDown => "hiding_right_down",
        >= YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown => "hiding_left_down",
        >= YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp => "hiding_left_up",
        >= YardInstructionProgramDefinitions.HidingUpsideRightMovingUp => "hiding_right_up",
        >= YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight => "hidden_top_right",
        >= YardInstructionProgramDefinitions.HidingUpsideUpMovingRight => "hiding_top_right",
        >= YardInstructionProgramDefinitions.HidingUpsideDownMovingRight => "hiding_bottom_right",
        >= YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft => "hiding_bottom_left",
        >= YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft => "hidden_top_left",
        >= YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft => "hiding_top_left",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown => "inside_turn_right_down",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft => "inside_turn_bottom_left",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp => "inside_turn_left_up",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight => "inside_turn_top_right",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown => "inside_turn_left_down",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight => "inside_turn_bottom_right",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp => "inside_turn_right_up",
        >= YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft => "inside_turn_top_left",
        >= YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp => "crawl_left_up",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft => "outside_turn_bottom_left",
        >= YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft => "crawl_bottom_left",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown => "outside_turn_right_down",
        >= YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown => "crawl_right_down",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight => "outside_turn_top_right",
        >= YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight => "crawl_top_right",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp => "outside_turn_left_up",
        >= YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp => "crawl_right_up",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight => "outside_turn_bottom_right",
        >= YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight => "crawl_bottom_right",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown => "outside_turn_left_down",
        >= YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown => "crawl_left_down",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft => "outside_turn_top_left",
        >= YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft => "crawl_top_left",
        >= YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp => "outside_turn_right_up",
        _ => throw new ArgumentOutOfRangeException(nameof(operand)),
    };

    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seenPointers = new HashSet<ushort>();
        string? previousGroup = null;
        int frameIndex = 0;
        for (int index = 0;
             index < YardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            string group = GroupName(operand);
            if (group != previousGroup)
            {
                previousGroup = group;
                frameIndex = 0;
            }
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Yard visual operand $A3:{operand:X4} is not compiled.");
            if (seenPointers.Add(pointer))
                frames.Add(new(Bank, pointer,
                    $"yard_{group}_{frameIndex:00}"));
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
