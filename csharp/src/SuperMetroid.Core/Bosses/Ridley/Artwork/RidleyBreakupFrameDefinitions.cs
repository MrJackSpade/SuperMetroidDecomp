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
        { RidleyBreakupFrame.TailTip0, EnemyFrameId.RidleyBreakupTailTipFirst },
        { RidleyBreakupFrame.TailTip1, EnemyFrameId.RidleyBreakupTailTipSecond },
        { RidleyBreakupFrame.TailTip10, EnemyFrameId.RidleyBreakupTailTipEleventh },
        { RidleyBreakupFrame.TailTip11, EnemyFrameId.RidleyBreakupTailTipTwelfth },
        { RidleyBreakupFrame.TailTip12, EnemyFrameId.RidleyBreakupTailTipThirteenth },
        { RidleyBreakupFrame.TailTip13, EnemyFrameId.RidleyBreakupTailTipFourteenth },
        { RidleyBreakupFrame.TailTip14, EnemyFrameId.RidleyBreakupTailTipFifteenth },
        { RidleyBreakupFrame.TailTip15, EnemyFrameId.RidleyBreakupTailTipSixteenth },
        { RidleyBreakupFrame.TailTip2, EnemyFrameId.RidleyBreakupTailTipThird },
        { RidleyBreakupFrame.TailTip3, EnemyFrameId.RidleyBreakupTailTipFourth },
        { RidleyBreakupFrame.TailTip4, EnemyFrameId.RidleyBreakupTailTipFifth },
        { RidleyBreakupFrame.TailTip5, EnemyFrameId.RidleyBreakupTailTipSixth },
        { RidleyBreakupFrame.TailTip6, EnemyFrameId.RidleyBreakupTailTipSeventh },
        { RidleyBreakupFrame.TailTip7, EnemyFrameId.RidleyBreakupTailTipEighth },
        { RidleyBreakupFrame.TailTip8, EnemyFrameId.RidleyBreakupTailTipNinth },
        { RidleyBreakupFrame.TailTip9, EnemyFrameId.RidleyBreakupTailTipTenth },
        { RidleyBreakupFrame.TorsoFacingLeft, EnemyFrameId.RidleyBreakupHeadLeft },
        { RidleyBreakupFrame.TorsoFacingRight, EnemyFrameId.RidleyBreakupHeadRight },
        { RidleyBreakupFrame.WingsFacingLeft, EnemyFrameId.RidleyWingFacingLeftFirst },
        { RidleyBreakupFrame.WingsFacingRight, EnemyFrameId.RidleyWingFacingRightFirst },
    };
}
