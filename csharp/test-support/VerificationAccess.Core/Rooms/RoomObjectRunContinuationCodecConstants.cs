using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The continuation <see cref="RoomObjectRunContinuationCodec"/> derives, taken from a draw list the game draws by rather
/// than from a pictured frame's layout: what only verification compares with the cartridge's draw lists.
/// </summary>
internal static class RoomObjectRunContinuationCodecConstants
{
    /// <summary>Where the run after <paramref name="run"/> of <paramref name="list"/> starts, or the origin after its last run.</summary>
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

    /// <summary>The offset word the cartridge stores after run <paramref name="run"/> of <paramref name="list"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the list's runs, or an axis does not fit a signed byte.</exception>
    internal static ushort Encode(DrawList list, int run) =>
        LittleEndianBytes.Word(unchecked((byte)X(list, run)), unchecked((byte)Y(list, run)));
}
