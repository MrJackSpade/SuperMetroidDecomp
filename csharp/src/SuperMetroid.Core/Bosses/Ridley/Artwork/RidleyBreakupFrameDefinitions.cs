using SuperMetroid.Core.Enemies.Artwork;

using F = SuperMetroid.Core.Bosses.Ridley.RidleyBreakupFrame;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's breakup fragments (#641): the bank-$A6 spritemap each
/// <see cref="RidleyBreakupFrame"/> shows, as the operands of the native lists at $A6:CA47..CAEF.
/// </summary>
internal static class RidleyBreakupFrameDefinitions
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RidleyBreakupFrame frame) => frame switch
    {
        F.TailSegmentSmall => 0xdc90,
        F.TailSegmentMedium => 0xdc97,
        F.TailSegmentLarge => 0xdc9e,
        F.WingsFacingLeft => 0xdd4a,
        F.WingsFacingRight => 0xdde2,
        F.LegsFacingLeft => 0xed29,
        F.LegsFacingRight => 0xef25,
        F.OpenHeadAndNeckFacingLeft => 0xecdc,
        F.OpenHeadAndNeckFacingRight => 0xeed8,
        F.TorsoFacingLeft => 0xed95,
        F.TorsoFacingRight => 0xef91,
        F.ClawFacingLeft => 0xed8e,
        F.ClawFacingRight => 0xef8a,
        F.TailTip0 => 0xdd2e,
        F.TailTip1 => 0xdd27,
        F.TailTip2 => 0xdd20,
        F.TailTip3 => 0xdd19,
        F.TailTip4 => 0xdd12,
        F.TailTip5 => 0xdd0b,
        F.TailTip6 => 0xdd04,
        F.TailTip7 => 0xdcfd,
        F.TailTip8 => 0xdcf6,
        F.TailTip9 => 0xdcef,
        F.TailTip10 => 0xdce8,
        F.TailTip11 => 0xdce1,
        F.TailTip12 => 0xdcda,
        F.TailTip13 => 0xdd43,
        F.TailTip14 => 0xdd3c,
        F.TailTip15 => 0xdd35,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ridley breakup frame."),
    };

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
