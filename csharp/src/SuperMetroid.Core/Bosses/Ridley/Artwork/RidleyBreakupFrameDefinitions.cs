using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's breakup fragments (#641): the frame each
/// <see cref="RidleyBreakupFrame"/> shows.
/// </summary>
internal static class RidleyBreakupFrameDefinitions
{

    /// <summary>The installed frame each <see cref="RidleyBreakupFrame"/> shows.</summary>
    internal static EnemyFrameTable<RidleyBreakupFrame> ShownFrames { get; } = new()
    {
        { RidleyBreakupFrame.ClawFacingLeft, EnemyFrameId.RidleyBreakupClawLeft },
        { RidleyBreakupFrame.ClawFacingRight, EnemyFrameId.RidleyBreakupClawRight },
        { RidleyBreakupFrame.LegsFacingLeft, EnemyFrameId.RidleyBreakupLegsLeft },
        { RidleyBreakupFrame.LegsFacingRight, EnemyFrameId.RidleyBreakupLegsRight },
        { RidleyBreakupFrame.OpenHeadAndNeckFacingLeft, EnemyFrameId.RidleyBreakupTorsoLeft },
        { RidleyBreakupFrame.OpenHeadAndNeckFacingRight, EnemyFrameId.RidleyBreakupTorsoRight },
        { RidleyBreakupFrame.TailSegmentLarge, EnemyFrameId.RidleyTailSegmentLarge },
        { RidleyBreakupFrame.TailSegmentMedium, EnemyFrameId.RidleyTailSegmentMedium },
        { RidleyBreakupFrame.TailSegmentSmall, EnemyFrameId.RidleyTailSegmentSmall },
        { RidleyBreakupFrame.TailTipPointingDown, EnemyFrameId.RidleyBreakupTailTipFirst },
        { RidleyBreakupFrame.TailTipPointingDownDownLeft, EnemyFrameId.RidleyBreakupTailTipSecond },
        { RidleyBreakupFrame.TailTipPointingUpRight, EnemyFrameId.RidleyBreakupTailTipEleventh },
        { RidleyBreakupFrame.TailTipPointingRightUpRight, EnemyFrameId.RidleyBreakupTailTipTwelfth },
        { RidleyBreakupFrame.TailTipPointingRight, EnemyFrameId.RidleyBreakupTailTipThirteenth },
        { RidleyBreakupFrame.TailTipPointingRightDownRight, EnemyFrameId.RidleyBreakupTailTipFourteenth },
        { RidleyBreakupFrame.TailTipPointingDownRight, EnemyFrameId.RidleyBreakupTailTipFifteenth },
        { RidleyBreakupFrame.TailTipPointingDownDownRight, EnemyFrameId.RidleyBreakupTailTipSixteenth },
        { RidleyBreakupFrame.TailTipPointingDownLeft, EnemyFrameId.RidleyBreakupTailTipThird },
        { RidleyBreakupFrame.TailTipPointingLeftDownLeft, EnemyFrameId.RidleyBreakupTailTipFourth },
        { RidleyBreakupFrame.TailTipPointingLeft, EnemyFrameId.RidleyBreakupTailTipFifth },
        { RidleyBreakupFrame.TailTipPointingLeftUpLeft, EnemyFrameId.RidleyBreakupTailTipSixth },
        { RidleyBreakupFrame.TailTipPointingUpLeft, EnemyFrameId.RidleyBreakupTailTipSeventh },
        { RidleyBreakupFrame.TailTipPointingUpUpLeft, EnemyFrameId.RidleyBreakupTailTipEighth },
        { RidleyBreakupFrame.TailTipPointingUp, EnemyFrameId.RidleyBreakupTailTipNinth },
        { RidleyBreakupFrame.TailTipPointingUpUpRight, EnemyFrameId.RidleyBreakupTailTipTenth },
        { RidleyBreakupFrame.TorsoFacingLeft, EnemyFrameId.RidleyBreakupHeadLeft },
        { RidleyBreakupFrame.TorsoFacingRight, EnemyFrameId.RidleyBreakupHeadRight },
        { RidleyBreakupFrame.WingsFacingLeft, EnemyFrameId.RidleyWingFacingLeftFirst },
        { RidleyBreakupFrame.WingsFacingRight, EnemyFrameId.RidleyWingFacingRightFirst },
    };
}
