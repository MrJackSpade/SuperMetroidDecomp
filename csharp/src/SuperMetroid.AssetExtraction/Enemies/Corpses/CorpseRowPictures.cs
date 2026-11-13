using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// Where a rotting corpse lies in one of its frames' pictures (#1539): the picture row its first pixel row lies at, which
/// the game draws the whole picture's rows from as the corpse's own rot moves them. The extraction finds it from the
/// characters the corpse's rows show in: each holds one band of eight of the corpse's rows, so every part that draws the
/// corpse lies unflipped top to bottom, each of its characters at the row of the picture its band lies at.
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

    /// <summary>Where <paramref name="corpse"/> lies in a picture of frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The frame turns the corpse's rows over, draws its characters off one row of the picture per band, or draws none.
    /// </exception>
    internal static CorpsePicturePlacement PlacementOf(string name, SpriteFrame frame, RottingCorpseCells corpse)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(corpse);
        int top = frame.Parts.Min(part => (int)part.Offset.Y);
        int? corpseTop = null;
        foreach (DrawnCharacter character in frame.Parts.SelectMany(Characters).Where(character => corpse.Holds(character.Character)))
        {
            if (character.FlippedTopToBottom)
                throw new InvalidDataException($"Corpse frame {name} flips {corpse.Name}'s character {character.Character} top to bottom, which turns its rows over.");
            int characterTop = character.Place.Y - top - corpse.Band(character.Character) * SpriteArtwork.CharacterPixels;
            if (corpseTop is int earlier && earlier != characterTop)
                throw new InvalidDataException($"Corpse frame {name} draws {corpse.Name}'s character {character.Character} off its band's row.");
            corpseTop = characterTop;
        }
        return corpseTop is int pictureTop ? new CorpsePicturePlacement(pictureTop)
            : throw new InvalidDataException($"Corpse frame {name} draws none of {corpse.Name}'s characters.");
    }

    /// <summary>Each character <paramref name="part"/> draws, at its place in the frame.</summary>
    private static IEnumerable<DrawnCharacter> Characters(SpriteFramePart part)
    {
        bool flipped = (part.Flip & PictureFlip.Vertical) != PictureFlip.None;
        for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
        {
            for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
            {
                int character = part.CharacterAt(new Position16<ScreenPixels>(
                    checked((ushort)(column * SpriteArtwork.CharacterPixels)), checked((ushort)(row * SpriteArtwork.CharacterPixels))), out _);
                yield return new DrawnCharacter(character, part.Offset + Offset16<ScreenPixels>.Of(column * SpriteArtwork.CharacterPixels,
                    row * SpriteArtwork.CharacterPixels), flipped);
            }
        }
    }

    /// <summary>One character a frame draws.</summary>
    /// <param name="Character">The character.</param>
    /// <param name="Place">Where its top-left corner lies in the frame.</param>
    /// <param name="FlippedTopToBottom">Whether its part flips it top to bottom.</param>
    private readonly record struct DrawnCharacter(int Character, Offset16<ScreenPixels> Place, bool FlippedTopToBottom);
}
