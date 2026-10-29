using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The cartridge's view of a boss body layer (#1466): the tilemap entry each cell shows, read back from the installed
/// frame documents the import composed each part from, so checks can compare a layer with the cartridge's words.
/// </summary>
internal static class BossBodyWords
{
    /// <summary>
    /// The tilemap entry each cell of <paramref name="layer"/> shows, row by row, 32 to a row: the word the frame a part was
    /// cut from writes at the cell, or where the blank shows, <paramref name="blankWord"/> or else <paramref name="owner"/>'s blank.
    /// </summary>
    /// <exception cref="InvalidDataException">A part names a frame <paramref name="bosses"/> does not hold, or a cell the frame does not write.</exception>
    internal static ushort[] Of(BossBodyLayer layer, BossBodyOwner owner, BossArtworkCatalog bosses, ushort? blankWord = null)
    {
        Dictionary<string, BossBodyFrameSource> frames = BossBodySheetComposer.Frames(bosses.Sources, owner).ToDictionary(frame => frame.Name, StringComparer.Ordinal);
        ushort blank = blankWord ?? BossBodySheetComposer.BlankOf(owner).PackedWord;
        var words = new ushort[layer.Rows * BossBodyDefinitions.ColumnCount];
        foreach (BossBodyPlacement placement in layer.Placements)
        {
            string frameName = placement.Part.Name.Split('~')[0];
            BossBodyFootprint shows = placement.Shows;
            for (int row = shows.Top; row < shows.End; row++)
            {
                for (int column = 0; column < BossBodyDefinitions.ColumnCount; column++)
                {
                    if (!shows.Covers(row, column))
                        continue;
                    int cell = row * BossBodyDefinitions.ColumnCount + column;
                    words[cell] = frameName == BossBodyPainter.BlankPartName ? blank
                        : frames.TryGetValue(frameName, out BossBodyFrameSource? frame) && frame.Cells.TryGetValue(cell, out BackgroundTile tile)
                            ? tile.PackedWord
                            : throw new InvalidDataException($"{owner}'s body part {placement.Part.Name} shows cell {cell}, which no installed frame writes.");
                }
            }
        }
        return words;
    }
}
