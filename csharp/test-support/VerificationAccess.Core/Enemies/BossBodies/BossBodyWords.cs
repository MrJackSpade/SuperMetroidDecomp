using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The cartridge's view of a boss body (#1466, #1515): the tilemap entry each cell of the boss's layer shows, read back
/// from the cells the import composed each shown frame from, so checks can compare a body with the cartridge's words.
/// </summary>
internal static class BossBodyWords
{
    /// <summary>Rows of the layer Phantoon, Draygon, Crocomire and Mother Brain show their bodies on: two screens.</summary>
    internal const int SharedLayerRows = 2 * BossBodySourceDefinitions.ScreenRows;

    /// <summary>Rows of Kraid's layer: four screens.</summary>
    internal const int KraidLayerRows = 4 * BossBodySourceDefinitions.ScreenRows;

    /// <summary>
    /// The tilemap entry each cell of <paramref name="layer"/> shows as the cartridge writes it, row by row, 32 to a row: the
    /// word the frame shown over the cell writes there, a later frame over an earlier, or where no frame shows,
    /// <paramref name="blankWord"/> or else <paramref name="owner"/>'s blank. A cell whose top lies at or below the row above
    /// which alone the body shows shows the blank.
    /// </summary>
    /// <exception cref="InvalidDataException">The layer shows a frame <paramref name="bosses"/> does not compose.</exception>
    internal static ushort[] Of(BossBodyLayer layer, BossBodyOwner owner, BossArtworkCatalog bosses, ushort? blankWord = null)
    {
        Dictionary<string, BossBodyFrameSource> frames = FramesOf(bosses.Sources, owner);
        ushort blank = blankWord ?? BossBodySheetComposer.BlankOf(owner).PackedWord;
        int rows = owner == BossBodyOwner.Kraid ? KraidLayerRows : SharedLayerRows;
        var words = new ushort[rows * BossBodySourceDefinitions.LayerColumns];
        Array.Fill(words, blank);
        foreach (BossBodyFrame shown in layer.Frames)
        {
            BossBodyFrameSource frame = frames.TryGetValue(shown.Name, out BossBodyFrameSource? found) ? found
                : throw new InvalidDataException($"{owner}'s body shows frame {shown.Name}, which no installed frame composes.");
            foreach ((int cell, BackgroundTile tile) in frame.Cells)
                words[cell] = tile.PackedWord;
        }
        if (layer.ShownAbove is { } hidden)
        {
            for (int cell = 0; cell < words.Length; cell++)
            {
                if (BossBodyPainter.PixelOf(cell).Y >= hidden)
                    words[cell] = blank;
            }
        }
        return words;
    }

    /// <summary>
    /// The frames composed from each installed artwork, by boss: installed artwork never changes, so a check reading a body
    /// every frame composes its boss's frames once rather than once per read.
    /// </summary>
    private static readonly ConditionalWeakTable<BossSourceArtwork, ConcurrentDictionary<BossBodyOwner, Dictionary<string, BossBodyFrameSource>>> composed = new();

    /// <summary>The frames <paramref name="owner"/> shows in <paramref name="sources"/>, by name, composed on first use.</summary>
    private static Dictionary<string, BossBodyFrameSource> FramesOf(BossSourceArtwork sources, BossBodyOwner owner) =>
        composed.GetValue(sources, _ => new()).GetOrAdd(owner, boss =>
            BossBodySheetComposer.Frames(sources, boss).ToDictionary(frame => frame.Name, StringComparer.Ordinal));
}
