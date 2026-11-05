using SuperMetroid.Core;
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The bank-$A9 row upload lists of the dead Sidehoppers (#1406): the rows a dead Sidehopper uploads from its staged
/// characters after each rot step. Only the extraction reads them, to picture the steps. A dead Zoomer, Ripper or Skree is
/// drawn through its rows as its rot leaves them (#1531), so nothing pictures its uploads.
/// </summary>
[EncodingOwner("bank-$A9 dead-monster row upload lists")]
internal static class DeadMonsterRowUploadDefinitions
{
    /// <summary>The rows <paramref name="kind"/> uploads, in native descriptor order.</summary>
    /// <exception cref="InvalidDataException">The kind has no row upload list.</exception>
    internal static ImmutableArray<CorpseRowUpload> ForKind(RottingCorpseKind kind)
    {
        int Variant(int count) => (uint)kind.Variant < count
            ? kind.Variant
            : throw new InvalidDataException($"Corpse kind {kind.Name} has no row upload list.");
        return kind.Species switch
        {
            RottingCorpseSpecies.Sidehopper => SidehopperRows(Variant(2)),
            RottingCorpseSpecies.Zoomer or RottingCorpseSpecies.Ripper or RottingCorpseSpecies.Skree => throw new InvalidDataException(
                $"Corpse kind {kind.Name} is drawn through its rows as its rot leaves them; nothing pictures its row uploads."),
            RottingCorpseSpecies.Torizo => throw new InvalidDataException("The Torizo corpse uploads its rows by halves."),
            _ => throw new InvalidDataException($"Corpse kind {kind.Name} has no row upload list."),
        };
    }

    /// <summary>The rows of Sidehopper corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<CorpseRowUpload> SidehopperRows(int variant) =>
        Rows(5, 0xa0, (ushort)(0x2040 + variant * 0x2e0), (ushort)(0x7000 + variant * 0x90),
            variant == 0 ? (ushort)0x60 : (ushort)0x40, variant == 0 ? (ushort)0x20 : (ushort)0);

    /// <summary>
    /// Consecutive live-WRAM tile rows placed in 32-tile VRAM rows, in native descriptor order; sidehoppers clip their
    /// first row to <paramref name="firstSize"/> bytes and shift it by <paramref name="firstDestinationOffset"/>.
    /// </summary>
    private static ImmutableArray<CorpseRowUpload> Rows(int count, int stride, int source, int destination,
        int firstSize = 0, int firstDestinationOffset = 0)
    {
        var rows = ImmutableArray.CreateBuilder<CorpseRowUpload>(count);
        for (int index = 0; index < count; index++)
        {
            rows.Add(new(index == 0 && firstSize != 0 ? firstSize : stride, 0x7e00,
                (ushort)(source + stride * index - (index == 0 ? 0 : firstDestinationOffset * 2)),
                (ushort)(destination + 0x100 * index + (index == 0 ? firstDestinationOffset : 0))));
        }
        return rows.MoveToImmutable();
    }
}
