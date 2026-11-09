using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

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
    /// <summary>The offset the cartridge stores after run <paramref name="run"/> of <paramref name="list"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the list's runs.</exception>
    internal static Offset16<RoomBlocks> Of(DrawList list, int run)
    {
        ReadOnlySpan<Run> runs = list.Runs.Span;
        if ((uint)run >= (uint)runs.Length) throw new ArgumentOutOfRangeException(nameof(run));
        return run + 1 < runs.Length ? runs[run + 1].Start : Offset16<RoomBlocks>.Zero;
    }

    /// <summary>The horizontal offset the cartridge stores after run <paramref name="run"/>, -128 through 127.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the list's runs, or the offset does not fit a signed byte.</exception>
    internal static sbyte X(DrawList list, int run) => checked((sbyte)Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, Of(list, run).X));

    /// <summary>The vertical offset the cartridge stores after run <paramref name="run"/>, -128 through 127.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the list's runs, or the offset does not fit a signed byte.</exception>
    internal static sbyte Y(DrawList list, int run) => checked((sbyte)Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, Of(list, run).Y));

    /// <summary>The offset word the cartridge stores after run <paramref name="run"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the list's runs, or an axis does not fit a signed byte.</exception>
    internal static ushort Encode(DrawList list, int run) =>
        LittleEndianBytes.Word(unchecked((byte)X(list, run)), unchecked((byte)Y(list, run)));

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
