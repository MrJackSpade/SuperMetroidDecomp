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
/// Only <see cref="Compared"/> colours are composed: every other sprite row belongs to an enemy's own draws, whose shades
/// their own checks compare.
/// </summary>
internal static class GameplayShownColours
{
    /// <summary>The background colours a room's pictures ink, the first of the image.</summary>
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
    /// The native colours this image composes: every background colour, and Samus's suit row and her beams' row but for
    /// each row's first colour, which a sprite's transparent ink stands for and no draw ever shows.
    /// </summary>
    internal static IReadOnlyList<int> Compared { get; } =
        [.. Enumerable.Range(0, BackgroundColours), .. Enumerable.Range(SamusSuitFirst + 1, RowColours - 1), .. Enumerable.Range(SamusBeamsFirst + 1, RowColours - 1)];

    /// <summary>The colours <paramref name="runtime"/>'s published frame shows, with Samus's from <paramref name="sources"/>.</summary>
    /// <exception cref="InvalidOperationException">The published frame names no gameplay shading.</exception>
    /// <exception cref="InvalidDataException">The room's pictures do not ink every background colour.</exception>
    internal static RgbColor[] Of(SuperMetroidRuntime runtime, SamusShadeSources sources)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(sources);
        GameplayShading shading = runtime.PublishDisplayFrame().Paint.Gameplay
            ?? throw new InvalidOperationException("A gameplay frame publishes its pictures' shading.");
        var image = new RgbColor[SnesColourCount];
        RgbColor[] room = shading.Of(GameplayPictures.Room).InkColours(runtime.Room.LoadedScope.Assets.BlockSheet.Sheet.Image);
        if (room.Length != BackgroundColours)
            throw new InvalidDataException($"The room's pictures ink {room.Length} colours, not every background colour.");
        room.CopyTo(image, 0);
        // The HUD's colours, its layer's eight sets of four, show as its pictures show them under the HUD's shading: a
        // door's fade spares the HUD and lays the import's shade of each step over it instead, which keeps its minimap
        // and highlighted item lit where the room fades.
        SpriteImage hud = runtime.Room.LoadedScope.Bg3Pictures.Sheet.Sheet.Image;
        RgbColor[] hudInks = shading.Of(GameplayPictures.Hud).InkColours(hud);
        for (int ink = 0; ink < HudColours; ink++)
            image[SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.CartridgeColourOf(hud, ink)] = hudInks[ink];
        // Samus's draws show her shading, then the display's fade, then any of her colours written over the faded display.
        RgbColor[] samus = SamusColoursAccess.InksUnder(shading.OfSprite(runtime.Player.Colours.Shading, isSamus: true), sources);
        Array.Copy(samus, SamusSheetInks.Suit, image, SamusSuitFirst, RowColours);
        Array.Copy(samus, SamusSheetInks.Beams, image, SamusBeamsFirst, RowColours);
        return image;
    }

    /// <summary>The colours of a whole native image.</summary>
    private const int SnesColourCount = 256;
}
