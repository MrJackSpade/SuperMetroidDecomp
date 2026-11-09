using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The dead Torizo's alternating bank-$A9 row upload lists at $A9:D549 and $A9:D583 (#1406): the half of its rows the
/// cartridge uploads from its staged characters each main-AI call. Only the extraction reads them, to find the characters
/// its body's and its sand heap's rows show in, and so where each lies in its pictures (#1539); the game rots and draws the
/// corpse whole.
/// </summary>
[EncodingOwner("bank-$A9 dead-Torizo row upload lists")]
internal static class DeadTorizoRowUploadDefinitions
{
    /// <summary>First destination word for the twelve clipped ten-tile body rows in VRAM.</summary>
    private const ushort BodyDestination = 0x7060;
    /// <summary>First VRAM destination word for the sand strips, which upload alternately to VRAM $7000/$7100.</summary>
    private const ushort SandDestination = 0x7000;

    /// <summary>The body rows each half uploads before its sand strip.</summary>
    internal const int BodyRows = 6;

    /// <summary>The even or odd half's rows.</summary>
    internal static PhaseRows ForHalf(bool odd) => new(odd);

    /// <summary>Six body rows and one sand strip; body bounds clip unused tiles on each scanline.</summary>
    internal readonly struct PhaseRows(bool odd) : IReadOnlyList<CorpseRowUpload>
    {
        /// <summary>Six clipped body-row uploads followed by one sand-strip upload.</summary>
        public int Count => BodyRows + 1;
        /// <summary>Calculates the half's row-specific destination and byte count, including the alternating sand strip.</summary>
        public CorpseRowUpload this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index == BodyRows)
                    return new(odd ? (ushort)0x100 : (ushort)0x120,
                        (ushort)(SandDestination + (odd ? 0 : 0x100)));
                int row = index + (odd ? BodyRows : 0);
                int firstTile = row < 2 ? 3 : Math.Clamp(10 - row, 0, 2);
                int tileCount = row < 2 ? 6 : 10 - firstTile;
                return new((ushort)(32 * tileCount),
                    (ushort)(BodyDestination + row * 256 + firstTile * 16));
            }
        }
        /// <summary>Yields body rows in increasing row order, then the half's sand strip.</summary>
        public IEnumerator<CorpseRowUpload> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Preserves descriptor order for non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
