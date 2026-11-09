using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Where in work memory the dead monsters' and the dead Torizo's row uploads read each row from (#1406): the source half of
/// each bank-$A9 row upload descriptor, which the extraction's descriptors no longer carry because the game draws a corpse
/// through its rows and reads only where each row lands (#1531). The character rot oracle reads its staged rows from these
/// addresses, and the descriptor checks compare them with the cartridge's.
/// </summary>
internal static class CorpseRowSources
{
    /// <summary>The bank word of every row's source: work memory bank $7E in its high byte.</summary>
    internal const int BankWord = 0x7e00;

    /// <summary>The dead Torizo's twelve ten-tile body rows, staged from $7E:2000.</summary>
    private const int TorizoBody = 0x2000;

    /// <summary>The dead Torizo's sand strips, staged at $7E:9500 and $7E:9620.</summary>
    private const int TorizoSand = 0x9500;

    /// <summary>The work-memory offset row <paramref name="index"/> of <paramref name="kind"/>'s uploads reads from, in native descriptor order.</summary>
    /// <exception cref="InvalidDataException">The kind has no row upload list.</exception>
    internal static int Of(CorpseRowUploadList kind, int index)
    {
        int variant = kind.Variant;
        return kind.Species switch
        {
            RottingCorpseSpecies.Sidehopper => Row(0x2040 + variant * 0x2e0, 0xa0, index, variant == 0 ? 0x20 : 0),
            RottingCorpseSpecies.Zoomer => Row(0x2940 + variant * 0xc0, 0x60, index, 0),
            RottingCorpseSpecies.Ripper => Row(0x2b80 + variant * 0xc0, 0x60, index, 0),
            RottingCorpseSpecies.Skree => Row(0x2640 + variant * 0x100, 0x40, index, 0),
            RottingCorpseSpecies.Torizo => throw new InvalidDataException("The Torizo corpse uploads its rows by halves."),
            _ => throw new InvalidDataException($"Corpse kind {kind.Name} has no row upload list."),
        };
    }

    /// <summary>
    /// The work-memory offset upload <paramref name="index"/> of the dead Torizo's even or odd half reads from: its six
    /// body rows, clipped to the tiles each scanline uses, then its sand strip.
    /// </summary>
    internal static int OfTorizoHalf(bool odd, int index)
    {
        if (index == DeadTorizoRowUploadDefinitions.BodyRows)
            return TorizoSand + (odd ? 0 : 0x120);
        int row = index + (odd ? DeadTorizoRowUploadDefinitions.BodyRows : 0);
        int firstTile = row < 2 ? 3 : Math.Clamp(10 - row, 0, 2);
        return TorizoBody + row * 320 + firstTile * 32;
    }

    /// <summary>
    /// Row <paramref name="index"/> of consecutive staged rows <paramref name="stride"/> bytes apart from
    /// <paramref name="first"/>; a Sidehopper's later rows begin <paramref name="firstDestinationOffset"/> words earlier,
    /// as its clipped first row is shifted by them.
    /// </summary>
    private static int Row(int first, int stride, int index, int firstDestinationOffset) =>
        (ushort)(first + stride * index - (index == 0 ? 0 : firstDestinationOffset * 2));
}
