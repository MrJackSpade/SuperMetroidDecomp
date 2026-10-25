using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's breakup fragments (#641): the bank-$A6 spritemap each
/// <see cref="RidleyBreakupFrame"/> shows, as the operands of the native lists at $A6:CA47..CAEF.
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
        { RidleyBreakupFrame.TailTip0, EnemyFrameId.RidleyTailTip0 },
        { RidleyBreakupFrame.TailTip1, EnemyFrameId.RidleyTailTip1 },
        { RidleyBreakupFrame.TailTip10, EnemyFrameId.RidleyTailTip10 },
        { RidleyBreakupFrame.TailTip11, EnemyFrameId.RidleyTailTip11 },
        { RidleyBreakupFrame.TailTip12, EnemyFrameId.RidleyTailTip12 },
        { RidleyBreakupFrame.TailTip13, EnemyFrameId.RidleyTailTip13 },
        { RidleyBreakupFrame.TailTip14, EnemyFrameId.RidleyTailTip14 },
        { RidleyBreakupFrame.TailTip15, EnemyFrameId.RidleyTailTip15 },
        { RidleyBreakupFrame.TailTip2, EnemyFrameId.RidleyTailTip2 },
        { RidleyBreakupFrame.TailTip3, EnemyFrameId.RidleyTailTip3 },
        { RidleyBreakupFrame.TailTip4, EnemyFrameId.RidleyTailTip4 },
        { RidleyBreakupFrame.TailTip5, EnemyFrameId.RidleyTailTip5 },
        { RidleyBreakupFrame.TailTip6, EnemyFrameId.RidleyTailTip6 },
        { RidleyBreakupFrame.TailTip7, EnemyFrameId.RidleyTailTip7 },
        { RidleyBreakupFrame.TailTip8, EnemyFrameId.RidleyTailTip8 },
        { RidleyBreakupFrame.TailTip9, EnemyFrameId.RidleyTailTip9 },
        { RidleyBreakupFrame.TorsoFacingLeft, EnemyFrameId.RidleyBreakupHeadLeft },
        { RidleyBreakupFrame.TorsoFacingRight, EnemyFrameId.RidleyBreakupHeadRight },
        { RidleyBreakupFrame.WingsFacingLeft, EnemyFrameId.RidleyWingLeft0 },
        { RidleyBreakupFrame.WingsFacingRight, EnemyFrameId.RidleyWingRight0 },
    };
}
