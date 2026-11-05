using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// Where a rotting corpse lies in one of its frames' pictures, and what the frame must be for the game to draw it through
/// the corpse's rows as its rot leaves them (#1531). The game rots the corpse's characters, a pixel row of every character
/// column at a time, each column from the first band of its flesh. So every part that draws the corpse lies unflipped top
/// to bottom on one grid of character rows and columns, each of its characters at the row of the picture its band of the
/// corpse's rows lies at; each character column of the picture shows one column of the corpse, unbroken from the band its
/// flesh begins at down; and no other character is drawn over the corpse's, where the rot would move it too.
/// </summary>
internal static class CorpseRowPictures
{
    /// <summary>The corpse among <paramref name="corpses"/> whose rows frame <paramref name="frame"/> reads, or none.</summary>
    /// <exception cref="InvalidDataException">The frame reads two corpses' rows.</exception>
    internal static RottingCorpseCells? CorpseOf(string name, SpriteFrame frame, IReadOnlyList<RottingCorpseCells> corpses)
    {
        Ensure.NotNull(frame);
        DrawnCharacter[] drawn = [.. frame.Parts.SelectMany(Characters)];
        RottingCorpseCells[] read = [.. Ensure.NotNull(corpses).Where(corpse => drawn.Any(character => corpse.Holds(character.Character)))];
        return read.Length switch
        {
            0 => null,
            1 => read[0],
            _ => throw new InvalidDataException($"Corpse frame {name} reads the rows of {string.Join(" and ", read.Select(corpse => corpse.Name))}."),
        };
    }

    /// <summary>
    /// Where <paramref name="corpse"/> lies in a picture of frame <paramref name="frame"/> whose characters a later step has
    /// <paramref name="covered"/> in part: a column shows the corpse rotting only in the bands whose characters it still
    /// shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not drawn as its corpse's rows rot.</exception>
    internal static CorpsePictureLayout LayoutOf(string name, SpriteFrame frame, RottingCorpseCells corpse, IReadOnlySet<int> covered)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(corpse);
        Ensure.NotNull(covered);
        DrawnCharacter[] drawn = [.. frame.Parts.SelectMany(Characters)];
        int left = frame.Parts.Min(part => (int)part.Offset.X), top = frame.Parts.Min(part => (int)part.Offset.Y);
        int right = frame.Parts.Max(part => part.Offset.X + part.Size.X);
        if ((right - left) % SpriteArtwork.CharacterPixels != 0)
            throw new InvalidDataException($"Corpse frame {name} is not a whole number of character columns wide.");
        int? corpseTop = null;
        var columnOf = new Dictionary<int, int>();
        var bandsOf = new Dictionary<int, SortedSet<int>>();
        var rottingOf = new Dictionary<int, SortedSet<int>>();
        foreach (DrawnCharacter character in drawn.Where(character => corpse.Holds(character.Character)))
        {
            if (character.FlippedTopToBottom)
                throw new InvalidDataException($"Corpse frame {name} flips {corpse.Name}'s character {character.Character} top to bottom, which turns its rows over.");
            if ((character.X - left) % SpriteArtwork.CharacterPixels != 0)
                throw new InvalidDataException($"Corpse frame {name} draws {corpse.Name}'s character {character.Character} between character columns.");
            int band = corpse.Band(character.Character);
            int characterTop = character.Y - top - band * SpriteArtwork.CharacterPixels;
            if (corpseTop is int earlier && earlier != characterTop)
                throw new InvalidDataException($"Corpse frame {name} draws {corpse.Name}'s character {character.Character} off its band's row.");
            corpseTop = characterTop;
            int pictureColumn = (character.X - left) / SpriteArtwork.CharacterPixels;
            int corpseColumn = RottingCorpseCells.Column(character.Character);
            if (columnOf.TryGetValue(pictureColumn, out int shown) && shown != corpseColumn)
                throw new InvalidDataException($"Corpse frame {name}'s character column {pictureColumn} shows two of {corpse.Name}'s columns.");
            columnOf[pictureColumn] = corpseColumn;
            Add(bandsOf, pictureColumn, band);
            if (!covered.Contains(character.Character))
                Add(rottingOf, pictureColumn, band);
            foreach (DrawnCharacter over in drawn)
            {
                if (Overlaps(over, character) && (over.X != character.X || over.Y != character.Y || over.Character != character.Character))
                    throw new InvalidDataException(
                        $"Corpse frame {name} draws character {over.Character} over {corpse.Name}'s character {character.Character}, where the rot would move it too.");
            }
        }
        if (corpseTop is not int pictureTop)
            throw new InvalidDataException($"Corpse frame {name} draws none of {corpse.Name}'s characters.");
        var columns = new CorpseColumnRot?[(right - left) / SpriteArtwork.CharacterPixels];
        foreach ((int pictureColumn, SortedSet<int> bands) in bandsOf)
        {
            int first = corpse.FirstBand(columnOf[pictureColumn]);
            if (bands.Min != first || bands.Max - bands.Min + 1 != bands.Count)
                throw new InvalidDataException(
                    $"Corpse frame {name}'s character column {pictureColumn} does not show {corpse.Name}'s column unbroken from the band its flesh begins at.");
            if (rottingOf.TryGetValue(pictureColumn, out SortedSet<int>? rotting))
                columns[pictureColumn] = new CorpseColumnRot(first, rotting);
        }
        return new CorpsePictureLayout(pictureTop, columns);
    }

    /// <summary>Adds <paramref name="band"/> to <paramref name="column"/>'s bands.</summary>
    private static void Add(Dictionary<int, SortedSet<int>> bands, int column, int band)
    {
        if (!bands.TryGetValue(column, out SortedSet<int>? set))
            bands.Add(column, set = []);
        set.Add(band);
    }

    /// <summary>Whether two drawn characters' pixels overlap.</summary>
    private static bool Overlaps(DrawnCharacter first, DrawnCharacter second) =>
        Math.Abs(first.X - second.X) < SpriteArtwork.CharacterPixels && Math.Abs(first.Y - second.Y) < SpriteArtwork.CharacterPixels;

    /// <summary>Each character <paramref name="part"/> draws, at its place in the frame.</summary>
    private static IEnumerable<DrawnCharacter> Characters(SpriteFramePart part)
    {
        bool flipped = (part.Flip & TileFlip.Vertical) != TileFlip.None;
        for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
        {
            for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
            {
                int character = part.CharacterAt(new Position16<ScreenPixels>(
                    checked((ushort)(column * SpriteArtwork.CharacterPixels)), checked((ushort)(row * SpriteArtwork.CharacterPixels))), out _);
                yield return new DrawnCharacter(character, part.Offset.X + column * SpriteArtwork.CharacterPixels,
                    part.Offset.Y + row * SpriteArtwork.CharacterPixels, flipped);
            }
        }
    }

    /// <summary>One character a frame draws.</summary>
    /// <param name="Character">The character.</param>
    /// <param name="X">The frame column of its left edge.</param>
    /// <param name="Y">The frame row of its top edge.</param>
    /// <param name="FlippedTopToBottom">Whether its part flips it top to bottom.</param>
    private readonly record struct DrawnCharacter(int Character, int X, int Y, bool FlippedTopToBottom);
}
