using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification oracle (#1392): an enemy-format frame composed from its parts over OBJ characters, as the cartridge
/// composes it. Gameplay draws the pre-coloured pictures the extraction composes; picture checks compare each picture
/// with this composition over the characters it was drawn over.
/// </summary>
internal static class EnemyPartComposition
{
    /// <summary>Publishes an installed enemy composition over <paramref name="objects"/>; the compiled common empty frame draws nothing.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The OBJ characters the parts read.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="spritemaps">The installed enemy compositions.</param>
    /// <param name="draw">The composition, its origin on screen, palette bits and packed tile base.</param>
    /// <exception cref="InvalidDataException">The frame has no installed composition.</exception>
    internal static void PublishSpritemap(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source,
        EnemySpritemapCatalog spritemaps, EnemySpritemapDraw draw)
    {
        ArgumentNullException.ThrowIfNull(spritemaps);
        ArgumentNullException.ThrowIfNull(draw);
        if (draw.Frame == EnemyFrameId.Empty)
            return;
        if (!spritemaps.TryGetDisplay(draw.Frame, out EnemySpritemapParts installed))
            throw new InvalidDataException($"Enemy frame {draw.Frame} requires an installed display composition.");
        ushort paletteBits = checked((ushort)draw.PaletteBits);
        PublishParts(sprites, objects, source, EnemySpriteFrames.Frame(installed, checked((ushort)draw.BaseTileIndex), paletteBits),
            new Position16<ScreenPixels>(draw.Origin.X, draw.Origin.Y), paletteBits);
    }

    /// <summary>Publishes one enemy-format frame, its parts' palettes and tiles already formed, over <paramref name="objects"/>.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The OBJ characters the parts read.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The whole frame.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="paletteBits">The palette selection the frame was formed with.</param>
    internal static void PublishParts(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source,
        SpriteFrame frame, Position16<ScreenPixels> origin, ushort paletteBits)
    {
        ArgumentNullException.ThrowIfNull(sprites);
        sprites.Add(new CharacterSpriteDraw(source, frame, objects, origin, default,
            (byte)SnesObjAttributeWord.FromPaletteBits(paletteBits).PaletteIndex, 0));
    }
}
