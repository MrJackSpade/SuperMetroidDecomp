using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The offset word the cartridge stores after each PLM block run (#1546): the next run's start from the draw's origin,
/// a signed byte per axis with X first, and zero after the last run, which ends the cartridge's list. Core draws hold
/// where each run starts and end at their last run; this codec is the one place a run's continuation is derived, for
/// import checks against the cartridge's draw lists.
/// </summary>
[EncodingOwner("PLM block-run continuation word")]
internal static class RoomObjectRunContinuationCodec
{
    /// <summary>The offset word the cartridge stores after run <paramref name="run"/> of a pictured frame's <paramref name="layout"/> (#1557).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the layout's runs, or an axis does not fit a signed byte.</exception>
    internal static ushort Encode(RoomObjectFrameLayout layout, int run)
    {
        Offset16<RoomBlocks> next = layout.NextStart(run);
        return LittleEndianBytes.Word(
            unchecked((byte)checked((sbyte)Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, next.X))),
            unchecked((byte)checked((sbyte)Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, next.Y))));
    }
}
