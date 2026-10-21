using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Named Yard poses shown by its 38 typed crawling, turn, hiding, and airborne programs.
/// Their movement, timing, and cues stay in the programs; only the OAM presentation is replaceable.
/// </summary>
internal static class YardVisualDefinitions
{
    /// <summary>Yard's instruction and spritemap bank, $A3.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>Number of distinct Yard OAM compositions its programs show.</summary>
    internal const int FrameCount = 104;

    /// <summary>The name group of each crawling, turn, hiding and airborne program's poses.</summary>
    private static string GroupName(YardProgram program) => program switch
    {
        YardProgram.AirborneFacingLeft => "airborne_left_initial",
        YardProgram.AirborneFacingLeftLoop => "airborne_left_loop",
        YardProgram.AirborneFacingRight => "airborne_right_initial",
        YardProgram.AirborneFacingRightLoop => "airborne_right_loop",
        YardProgram.CrawlingUpsideDownMovingLeft => "crawl_bottom_left",
        YardProgram.CrawlingUpsideDownMovingRight => "crawl_bottom_right",
        YardProgram.CrawlingUpsideLeftMovingDown => "crawl_left_down",
        YardProgram.CrawlingUpsideLeftMovingUp => "crawl_left_up",
        YardProgram.CrawlingUpsideRightMovingDown => "crawl_right_down",
        YardProgram.CrawlingUpsideRightMovingUp => "crawl_right_up",
        YardProgram.CrawlingUpsideUpMovingLeft => "crawl_top_left",
        YardProgram.CrawlingUpsideUpMovingRight => "crawl_top_right",
        YardProgram.HiddenUpsideUpMovingLeft => "hidden_top_left",
        YardProgram.HiddenUpsideUpMovingRight => "hidden_top_right",
        YardProgram.HidingUpsideDownMovingLeft => "hiding_bottom_left",
        YardProgram.HidingUpsideDownMovingRight => "hiding_bottom_right",
        YardProgram.HidingUpsideLeftMovingDown => "hiding_left_down",
        YardProgram.HidingUpsideLeftMovingUp => "hiding_left_up",
        YardProgram.HidingUpsideRightMovingDown => "hiding_right_down",
        YardProgram.HidingUpsideRightMovingUp => "hiding_right_up",
        YardProgram.HidingUpsideUpMovingLeft => "hiding_top_left",
        YardProgram.HidingUpsideUpMovingRight => "hiding_top_right",
        YardProgram.InsideTurnUpsideDownMovingLeft => "inside_turn_bottom_left",
        YardProgram.InsideTurnUpsideDownMovingRight => "inside_turn_bottom_right",
        YardProgram.InsideTurnUpsideLeftMovingDown => "inside_turn_left_down",
        YardProgram.InsideTurnUpsideLeftMovingUp => "inside_turn_left_up",
        YardProgram.InsideTurnUpsideRightMovingDown => "inside_turn_right_down",
        YardProgram.InsideTurnUpsideRightMovingUp => "inside_turn_right_up",
        YardProgram.InsideTurnUpsideUpMovingLeft => "inside_turn_top_left",
        YardProgram.InsideTurnUpsideUpMovingRight => "inside_turn_top_right",
        YardProgram.OutsideTurnUpsideDownMovingLeft => "outside_turn_bottom_left",
        YardProgram.OutsideTurnUpsideDownMovingRight => "outside_turn_bottom_right",
        YardProgram.OutsideTurnUpsideLeftMovingDown => "outside_turn_left_down",
        YardProgram.OutsideTurnUpsideLeftMovingUp => "outside_turn_left_up",
        YardProgram.OutsideTurnUpsideRightMovingDown => "outside_turn_right_down",
        YardProgram.OutsideTurnUpsideRightMovingUp => "outside_turn_right_up",
        YardProgram.OutsideTurnUpsideUpMovingLeft => "outside_turn_top_left",
        YardProgram.OutsideTurnUpsideUpMovingRight => "outside_turn_top_right",
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    };

    /// <summary>Names each distinct Yard spritemap by its program and pose order, in authored order.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames()
    {
        var frames = new List<NativeEnemySpritemapDefinition>(FrameCount);
        var seenPointers = new HashSet<ushort>();
        foreach (YardProgram program in Enum.GetValues<YardProgram>())
        {
            int frameIndex = 0;
            foreach (ushort pointer in YardAnimationsConstants.Spritemaps(program))
            {
                if (seenPointers.Add(pointer))
                    frames.Add(new(Bank, pointer, $"yard_{GroupName(program)}_{frameIndex:00}"));
                frameIndex++;
            }
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Yard visual catalog has {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return frames.ToArray();
    }
}
