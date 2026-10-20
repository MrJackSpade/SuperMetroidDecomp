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
}
