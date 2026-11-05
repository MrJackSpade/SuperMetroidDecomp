using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// What a dead Zoomer's, Ripper's or Skree's frames must be for the game to draw them through the corpse's rows as its rot
/// leaves them (#1531): each frame is the corpse's rows, top to bottom. The game rots the corpse's characters, a pixel row
/// across every character column at a time, so the frame's parts lie unflipped top to bottom on one grid of character
/// rows, each part as far below the frame's top as its characters lie below the frame's first character row, and together
/// they cover the whole frame, which is as tall as the corpse.
/// </summary>
internal static class CorpseRowPictures
{
    /// <summary>
    /// The pixel rows of the corpse <paramref name="enemy"/> draws through its rows, the same for each of its variants; none
    /// for an enemy whose frames show their pictures' rows as they are.
    /// </summary>
    /// <exception cref="InvalidDataException">The corpse's variants are not all as tall.</exception>
    internal static int? RowsOf(EnemyDefinitionId enemy)
    {
        if (!DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy))
            return null;
        DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
        int rows = DeadTourianCorpses.For(profile.Species, 0).RowCount;
        for (int variant = 1; variant < profile.VariantCount; variant++)
        {
            if (DeadTourianCorpses.For(profile.Species, variant).RowCount != rows)
                throw new InvalidDataException($"Dead {profile.Species} variants are not all {rows} pixel rows tall; its frames cannot all be its rows.");
        }
        return rows;
    }

    /// <summary>Requires frame <paramref name="name"/> to be its corpse's <paramref name="rowCount"/> rows, top to bottom.</summary>
    /// <exception cref="InvalidDataException">The frame is not the corpse's rows, so drawing it through them would not show its rot.</exception>
    internal static void Require(string name, SpriteFrame frame, int rowCount)
    {
        Ensure.NotNull(frame);
        if (frame.Parts.IsEmpty)
            throw new InvalidDataException($"Corpse frame {name} has no parts to draw its rows.");
        int top = frame.Parts.Min(part => (int)part.Offset.Y), left = frame.Parts.Min(part => (int)part.Offset.X);
        int bottom = frame.Parts.Max(part => part.Offset.Y + part.Size.Y), right = frame.Parts.Max(part => part.Offset.X + part.Size.X);
        if (bottom - top != rowCount)
            throw new InvalidDataException($"Corpse frame {name} is {bottom - top} rows tall, not its corpse's {rowCount}.");
        if ((right - left) % SpriteArtwork.CharacterPixels != 0 || rowCount % SpriteArtwork.CharacterPixels != 0)
            throw new InvalidDataException($"Corpse frame {name} is not a whole number of characters wide and tall.");
        int firstCharacterRow = frame.Parts.Min(part => part.Character / SpriteArtwork.CharactersPerRow);
        int columns = (right - left) / SpriteArtwork.CharacterPixels, characterRows = rowCount / SpriteArtwork.CharacterPixels;
        var covered = new bool[columns * characterRows];
        foreach (SpriteFramePart part in frame.Parts)
        {
            if ((part.Flip & TileFlip.Vertical) != TileFlip.None)
                throw new InvalidDataException($"Corpse frame {name} flips a part top to bottom, which turns its rows over.");
            int characterRow = part.Character / SpriteArtwork.CharactersPerRow - firstCharacterRow;
            if (part.Offset.Y - top != characterRow * SpriteArtwork.CharacterPixels)
                throw new InvalidDataException($"Corpse frame {name} places a part of character row {characterRow} off its row's place.");
            if ((part.Offset.X - left) % SpriteArtwork.CharacterPixels != 0)
                throw new InvalidDataException($"Corpse frame {name} places a part between character columns.");
            int firstColumn = (part.Offset.X - left) / SpriteArtwork.CharacterPixels;
            for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
            {
                for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
                    covered[(characterRow + row) * columns + firstColumn + column] = true;
            }
        }
        if (Array.IndexOf(covered, false) is int gap and >= 0)
            throw new InvalidDataException(
                $"Corpse frame {name} leaves character column {gap % columns} of row {gap / columns} undrawn, where its rot would show what no part draws.");
    }
}
