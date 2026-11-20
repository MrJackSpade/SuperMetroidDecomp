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
    internal const int FrameCount = 96;

    /// <summary>The name group of each crawling, turn, hiding and airborne program's poses.</summary>
    private static string GroupName(YardSegment program) => program switch
    {
        YardSegment.AirborneFacingLeft => "airborne_left_initial",
        YardSegment.ShellClosedOnFloorMovingLeft => "hiding_top_left",
        YardSegment.ShellClosedOnCeilingMovingLeft => "hiding_bottom_left",
        YardSegment.HiddenOnCeilingMovingLeft => "hiding_bottom_left",
        YardSegment.ShellClosedOnCeilingMovingRight => "hiding_bottom_right",
        YardSegment.HiddenOnCeilingMovingRight => "hiding_bottom_right",
        YardSegment.ShellClosedOnFloorMovingRight => "hiding_top_right",
        YardSegment.ShellClosedOnLeftWallMovingUp => "hiding_right_up",
        YardSegment.HiddenOnLeftWallMovingUp => "hiding_right_up",
        YardSegment.ShellClosedOnRightWallMovingUp => "hiding_left_up",
        YardSegment.HiddenOnRightWallMovingUp => "hiding_left_up",
        YardSegment.ShellClosedOnRightWallMovingDown => "hiding_left_down",
        YardSegment.HiddenOnRightWallMovingDown => "hiding_left_down",
        YardSegment.ShellClosedOnLeftWallMovingDown => "hiding_right_down",
        YardSegment.HiddenOnLeftWallMovingDown => "hiding_right_down",
        YardSegment.AirborneFacingLeftTumbling => "airborne_left_initial",
        YardSegment.AirborneFacingRightTumbling => "airborne_right_initial",
        YardSegment.AirborneFacingRight => "airborne_right_initial",
        YardSegment.CrawlingOnCeilingMovingLeft => "crawl_bottom_left",
        YardSegment.CrawlingOnCeilingMovingRight => "crawl_bottom_right",
        YardSegment.CrawlingOnRightWallMovingDown => "crawl_left_down",
        YardSegment.CrawlingOnRightWallMovingUp => "crawl_left_up",
        YardSegment.CrawlingOnLeftWallMovingDown => "crawl_right_down",
        YardSegment.CrawlingOnLeftWallMovingUp => "crawl_right_up",
        YardSegment.CrawlingOnFloorMovingLeft => "crawl_top_left",
        YardSegment.CrawlingOnFloorMovingRight => "crawl_top_right",
        YardSegment.HiddenOnFloorMovingLeft => "hidden_top_left",
        YardSegment.HiddenOnFloorMovingRight => "hidden_top_right",
        YardSegment.HidingOnCeilingMovingLeft => "hiding_bottom_left",
        YardSegment.HidingOnCeilingMovingRight => "hiding_bottom_right",
        YardSegment.HidingOnRightWallMovingDown => "hiding_left_down",
        YardSegment.HidingOnRightWallMovingUp => "hiding_left_up",
        YardSegment.HidingOnLeftWallMovingDown => "hiding_right_down",
        YardSegment.HidingOnLeftWallMovingUp => "hiding_right_up",
        YardSegment.HidingOnFloorMovingLeft => "hiding_top_left",
        YardSegment.HidingOnFloorMovingRight => "hiding_top_right",
        YardSegment.InsideTurnOnCeilingMovingLeft => "inside_turn_bottom_left",
        YardSegment.InsideTurnOnCeilingMovingRight => "inside_turn_bottom_right",
        YardSegment.InsideTurnOnRightWallMovingDown => "inside_turn_left_down",
        YardSegment.InsideTurnOnRightWallMovingUp => "inside_turn_left_up",
        YardSegment.InsideTurnOnLeftWallMovingDown => "inside_turn_right_down",
        YardSegment.InsideTurnOnLeftWallMovingUp => "inside_turn_right_up",
        YardSegment.InsideTurnOnFloorMovingLeft => "inside_turn_top_left",
        YardSegment.InsideTurnOnFloorMovingRight => "inside_turn_top_right",
        YardSegment.OutsideTurnOnCeilingMovingLeft => "outside_turn_bottom_left",
        YardSegment.OutsideTurnOnCeilingMovingRight => "outside_turn_bottom_right",
        YardSegment.OutsideTurnOnRightWallMovingDown => "outside_turn_left_down",
        YardSegment.OutsideTurnOnRightWallMovingUp => "outside_turn_left_up",
        YardSegment.OutsideTurnOnLeftWallMovingDown => "outside_turn_right_down",
        YardSegment.OutsideTurnOnLeftWallMovingUp => "outside_turn_right_up",
        YardSegment.OutsideTurnOnFloorMovingLeft => "outside_turn_top_left",
        YardSegment.OutsideTurnOnFloorMovingRight => "outside_turn_top_right",
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    };

    /// <summary>Names each distinct Yard spritemap by its program and pose order, in authored order.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames()
    {
        var frames = new List<NativeEnemySpritemapDefinition>(FrameCount);
        var seenPointers = new HashSet<ushort>();
        var groupPoses = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (YardSegment program in Enum.GetValues<YardSegment>())
        {
            string group = GroupName(program);
            int frameIndex = groupPoses.GetValueOrDefault(group);
            foreach (ushort pointer in YardAnimationsConstants.Spritemaps(program))
            {
                if (seenPointers.Add(pointer))
                    frames.Add(new(Bank, pointer, $"yard_{group}_{frameIndex:00}"));
                frameIndex++;
            }
            groupPoses[group] = frameIndex;
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Yard visual catalog has {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return frames.ToArray();
    }
}
