using F = SuperMetroid.Core.Bosses.Ridley.RidleyBreakupFrame;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Cartridge values and helpers of <see cref="RidleyBreakupFrameDefinitions"/> that only verification reads.</summary>
internal static class RidleyBreakupFrameDefinitionsConstants
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
        F.TailTipPointingDown => 0xdd2e,
        F.TailTipPointingDownDownLeft => 0xdd27,
        F.TailTipPointingDownLeft => 0xdd20,
        F.TailTipPointingLeftDownLeft => 0xdd19,
        F.TailTipPointingLeft => 0xdd12,
        F.TailTipPointingLeftUpLeft => 0xdd0b,
        F.TailTipPointingUpLeft => 0xdd04,
        F.TailTipPointingUpUpLeft => 0xdcfd,
        F.TailTipPointingUp => 0xdcf6,
        F.TailTipPointingUpUpRight => 0xdcef,
        F.TailTipPointingUpRight => 0xdce8,
        F.TailTipPointingRightUpRight => 0xdce1,
        F.TailTipPointingRight => 0xdcda,
        F.TailTipPointingRightDownRight => 0xdd43,
        F.TailTipPointingDownRight => 0xdd3c,
        F.TailTipPointingDownDownRight => 0xdd35,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ridley breakup frame."),
    };
}
