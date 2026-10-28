using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The header word the cartridge stores before each PLM block run (#1452): the run's direction in the top bit above
/// its block count. Core runs hold a <see cref="PlmRunDirection"/> and their level words; this codec is the one place
/// the two become or leave that word, for import checks against the cartridge's draw lists.
/// </summary>
[EncodingOwner("PLM block-run header word")]
internal static class PlmRunHeaderCodec
{
    /// <summary>The header bit set when the run writes down a column.</summary>
    internal const int VerticalBit = 0x8000;

    /// <summary>The header bits below the direction bit, holding the run's block count.</summary>
    internal const int CountBits = 0x7fff;

    /// <summary>The header word of a run of <paramref name="count"/> blocks, 0 through 32767, in <paramref name="direction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> does not fit the header's count bits.</exception>
    internal static ushort Encode(PlmRunDirection direction, int count) => checked((ushort)(
        (direction switch
        {
            PlmRunDirection.Horizontal => 0,
            PlmRunDirection.Vertical => VerticalBit,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined PLM run direction."),
        }) | Ensure.BetweenInclusive(0, CountBits, count)));

    /// <summary>The header word the cartridge stores before <paramref name="run"/>.</summary>
    internal static ushort Encode(RoomPlmShotBlockDrawDefinitions.Run run) => Encode(run.Direction, run.Count);
}
