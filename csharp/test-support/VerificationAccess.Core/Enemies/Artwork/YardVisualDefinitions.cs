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
    private static string GroupName(YardSegment program) => program switch
    {
        YardSegment.AirborneFacingLeft => "airborne_left_initial",
        YardSegment.AirborneFacingLeftLoop => "airborne_left_loop",
        YardSegment.AirborneFacingRight => "airborne_right_initial",
        YardSegment.AirborneFacingRightLoop => "airborne_right_loop",
        YardSegment.CrawlingUpsideDownMovingLeft => "crawl_bottom_left",
        YardSegment.CrawlingUpsideDownMovingRight => "crawl_bottom_right",
        YardSegment.CrawlingUpsideLeftMovingDown => "crawl_left_down",
        YardSegment.CrawlingUpsideLeftMovingUp => "crawl_left_up",
        YardSegment.CrawlingUpsideRightMovingDown => "crawl_right_down",
        YardSegment.CrawlingUpsideRightMovingUp => "crawl_right_up",
        YardSegment.CrawlingUpsideUpMovingLeft => "crawl_top_left",
        YardSegment.CrawlingUpsideUpMovingRight => "crawl_top_right",
        YardSegment.HiddenUpsideUpMovingLeft => "hidden_top_left",
        YardSegment.HiddenUpsideUpMovingRight => "hidden_top_right",
        YardSegment.HidingUpsideDownMovingLeft => "hiding_bottom_left",
        YardSegment.HidingUpsideDownMovingRight => "hiding_bottom_right",
        YardSegment.HidingUpsideLeftMovingDown => "hiding_left_down",
        YardSegment.HidingUpsideLeftMovingUp => "hiding_left_up",
        YardSegment.HidingUpsideRightMovingDown => "hiding_right_down",
        YardSegment.HidingUpsideRightMovingUp => "hiding_right_up",
        YardSegment.HidingUpsideUpMovingLeft => "hiding_top_left",
        YardSegment.HidingUpsideUpMovingRight => "hiding_top_right",
        YardSegment.InsideTurnUpsideDownMovingLeft => "inside_turn_bottom_left",
        YardSegment.InsideTurnUpsideDownMovingRight => "inside_turn_bottom_right",
        YardSegment.InsideTurnUpsideLeftMovingDown => "inside_turn_left_down",
        YardSegment.InsideTurnUpsideLeftMovingUp => "inside_turn_left_up",
        YardSegment.InsideTurnUpsideRightMovingDown => "inside_turn_right_down",
        YardSegment.InsideTurnUpsideRightMovingUp => "inside_turn_right_up",
        YardSegment.InsideTurnUpsideUpMovingLeft => "inside_turn_top_left",
        YardSegment.InsideTurnUpsideUpMovingRight => "inside_turn_top_right",
        YardSegment.OutsideTurnUpsideDownMovingLeft => "outside_turn_bottom_left",
        YardSegment.OutsideTurnUpsideDownMovingRight => "outside_turn_bottom_right",
        YardSegment.OutsideTurnUpsideLeftMovingDown => "outside_turn_left_down",
        YardSegment.OutsideTurnUpsideLeftMovingUp => "outside_turn_left_up",
        YardSegment.OutsideTurnUpsideRightMovingDown => "outside_turn_right_down",
        YardSegment.OutsideTurnUpsideRightMovingUp => "outside_turn_right_up",
        YardSegment.OutsideTurnUpsideUpMovingLeft => "outside_turn_top_left",
        YardSegment.OutsideTurnUpsideUpMovingRight => "outside_turn_top_right",
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    };

    /// <summary>Names each distinct Yard spritemap by its program and pose order, in authored order.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames()
    {
        var frames = new List<NativeEnemySpritemapDefinition>(FrameCount);
        var seenPointers = new HashSet<ushort>();
        foreach (YardSegment program in Enum.GetValues<YardSegment>())
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
