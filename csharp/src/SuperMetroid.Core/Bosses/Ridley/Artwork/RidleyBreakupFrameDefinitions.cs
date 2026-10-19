using SuperMetroid.Core.Enemies.Artwork;

using F = SuperMetroid.Core.Bosses.Ridley.RidleyBreakupFrame;

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
        { RidleyBreakupFrame.TailSegmentLarge, EnemyFrameId.RidleySupplementA6Dc9e },
        { RidleyBreakupFrame.TailSegmentMedium, EnemyFrameId.RidleySupplementA6Dc97 },
        { RidleyBreakupFrame.TailSegmentSmall, EnemyFrameId.RidleySupplementA6Dc90 },
        { RidleyBreakupFrame.TailTip0, EnemyFrameId.RidleySupplementA6Dd2e },
        { RidleyBreakupFrame.TailTip1, EnemyFrameId.RidleySupplementA6Dd27 },
        { RidleyBreakupFrame.TailTip10, EnemyFrameId.RidleySupplementA6Dce8 },
        { RidleyBreakupFrame.TailTip11, EnemyFrameId.RidleySupplementA6Dce1 },
        { RidleyBreakupFrame.TailTip12, EnemyFrameId.RidleySupplementA6Dcda },
        { RidleyBreakupFrame.TailTip13, EnemyFrameId.RidleySupplementA6Dd43 },
        { RidleyBreakupFrame.TailTip14, EnemyFrameId.RidleySupplementA6Dd3c },
        { RidleyBreakupFrame.TailTip15, EnemyFrameId.RidleySupplementA6Dd35 },
        { RidleyBreakupFrame.TailTip2, EnemyFrameId.RidleySupplementA6Dd20 },
        { RidleyBreakupFrame.TailTip3, EnemyFrameId.RidleySupplementA6Dd19 },
        { RidleyBreakupFrame.TailTip4, EnemyFrameId.RidleySupplementA6Dd12 },
        { RidleyBreakupFrame.TailTip5, EnemyFrameId.RidleySupplementA6Dd0b },
        { RidleyBreakupFrame.TailTip6, EnemyFrameId.RidleySupplementA6Dd04 },
        { RidleyBreakupFrame.TailTip7, EnemyFrameId.RidleySupplementA6Dcfd },
        { RidleyBreakupFrame.TailTip8, EnemyFrameId.RidleySupplementA6Dcf6 },
        { RidleyBreakupFrame.TailTip9, EnemyFrameId.RidleySupplementA6Dcef },
        { RidleyBreakupFrame.TorsoFacingLeft, EnemyFrameId.RidleyBreakupHeadLeft },
        { RidleyBreakupFrame.TorsoFacingRight, EnemyFrameId.RidleyBreakupHeadRight },
        { RidleyBreakupFrame.WingsFacingLeft, EnemyFrameId.RidleySupplementA6Dd4a },
        { RidleyBreakupFrame.WingsFacingRight, EnemyFrameId.RidleySupplementA6Dde2 },
    };
}
