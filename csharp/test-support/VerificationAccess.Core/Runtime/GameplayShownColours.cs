using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The cartridge-ordered 256-colour image gameplay's pictures show (#1512), for checks that compare with native colour
/// memory: the room's background colours under the room pictures' published shading, the display's fade among it, the
/// HUD layer's colours under the HUD's shading, which a door's fade spares, and
/// Samus's suit and beam colours as her shades show them under the fade as it reaches her draws. Gameplay keeps no such
/// image; verification composes it from the shadings the frame publishes and the colours the import installs for them.
/// Only <see cref="ComparedIn"/> colours are composed: a background colour none of the room's or the HUD's pictures has
/// is drawn nowhere (#1544), and every other sprite row belongs to an enemy's own draws, whose shades their own checks
/// compare.
/// </summary>
internal static class GameplayShownColours
{
    /// <summary>The background colours, the first of the image.</summary>
    private const int BackgroundColours = 128;

    /// <summary>The HUD layer's colours: its eight sets of four, the first of the background colours.</summary>
    private const int HudColours = 32;

    /// <summary>Colours in one sprite row.</summary>
    private const int RowColours = 16;

    /// <summary>The first of Samus's suit colours in the native image.</summary>
    internal const int SamusSuitFirst = BackgroundColours + 4 * RowColours;

    /// <summary>The first of Samus's beam colours in the native image.</summary>
    internal const int SamusBeamsFirst = BackgroundColours + 6 * RowColours;

    /// <summary>
    /// The native colours <see cref="Of"/> composes for <paramref name="runtime"/>'s room: each background colour the HUD's
    /// pictures have, each the room's pictures have outside the HUD's colours, and Samus's suit row and her beams' row but
    /// for each row's first colour, which a sprite's clear pixels are composed as and no draw ever shows.
    /// </summary>
    /// <remarks>
    /// The cartridge keeps the HUD's colours in the room's first colours and spares them from a door's fade, so a room
    /// picture coloured from one of those places showed the HUD's unfaded colour there. That sharing was a presentation
    /// quirk (#1544): the room's pictures show the room's own colours under the room's shading, so the native colours at
    /// those places say nothing about what the port's room shows, and only the HUD's own places in its range are compared.
    /// </remarks>
    internal static IReadOnlyList<int> ComparedIn(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        SpriteImage room = runtime.Room.LoadedScope.Assets.BlockSheet.Sheet.Image;
        SpriteImage hud = runtime.Room.LoadedScope.HudPictures.Sheet.Image;
        return [.. Places(room).Where(place => place >= HudColours).Concat(Places(hud)).Distinct().Order(),
            .. Enumerable.Range(SamusSuitFirst + 1, RowColours - 1), .. Enumerable.Range(SamusBeamsFirst + 1, RowColours - 1)];
    }

    /// <summary>The places in the native image each of <paramref name="picture"/>'s colour keys was coloured from.</summary>
    private static IEnumerable<int> Places(SpriteImage picture) => picture.ColourKeys
        .SelectMany(key => SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.CartridgeColoursOf(picture, key));

    /// <summary>The colours <paramref name="runtime"/>'s published frame shows, with Samus's from <paramref name="sources"/>.</summary>
    /// <exception cref="InvalidOperationException">The published frame names no gameplay shading.</exception>
    /// <exception cref="InvalidDataException">A room or HUD picture's colour lies outside the background colours.</exception>
    internal static RgbColor[] Of(SuperMetroidRuntime runtime, SamusShadeSources sources)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(sources);
        GameplayShading shading = runtime.PublishDisplayFrame().Paint.Gameplay
            ?? throw new InvalidOperationException("A gameplay frame publishes its pictures' shading.");
        var image = new RgbColor[SnesColourCount];
        // The room's colours show as its tileset's pictures show them under the room's shading.
        Lay(image, runtime.Room.LoadedScope.Assets.BlockSheet.Sheet.Image, shading.Of(GameplayPictures.Room));
        // The HUD's colours, its layer's eight sets of four, show as its pictures show them under the HUD's shading: a
        // door's fade spares the HUD and lays the import's shade of each step over it instead, which keeps its minimap
        // and highlighted item lit where the room fades.
        Lay(image, runtime.Room.LoadedScope.HudPictures.Sheet.Image, shading.Of(GameplayPictures.Hud));
        // Samus's draws show her shading, then the display's fade, then any of her colours written over the faded display.
        RgbColor[] samus = SamusColoursAccess.InksUnder(shading.OfSprite(runtime.Player.Colours.Shading, isSamus: true), sources);
        Array.Copy(samus, SamusSheetInks.Suit, image, SamusSuitFirst, RowColours);
        Array.Copy(samus, SamusSheetInks.Beams, image, SamusBeamsFirst, RowColours);
        return image;
    }

    /// <summary>Lays each of <paramref name="picture"/>'s colour keys under <paramref name="shading"/> at every place it was coloured from.</summary>
    /// <exception cref="InvalidDataException">A colour lies outside the background colours.</exception>
    private static void Lay(RgbColor[] image, SpriteImage picture, PictureShading shading)
    {
        RgbColor[] colours = shading.KeyColours(picture);
        System.Collections.Immutable.ImmutableArray<ColourKey> keys = picture.ColourKeys;
        for (int key = 0; key < colours.Length; key++)
        {
            foreach (int place in SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.CartridgeColoursOf(picture, keys[key]))
            {
                if (place >= BackgroundColours)
                    throw new InvalidDataException($"Picture {picture.Name}'s colour key {keys[key]} lies at {place}, outside the background colours.");
                image[place] = colours[key];
            }
        }
    }

    /// <summary>The colours of a whole native image.</summary>
    private const int SnesColourCount = 256;
}
