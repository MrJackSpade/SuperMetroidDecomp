using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358, #1467): its palette, which of the ending's pre-assembled pictures its scenes have loaded,
/// the post-credits text each text page shows, and the floor's loads since the last full clear.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>The floor's loads since the last full clear, oldest first, which the floor's cells and tiles are read from.</summary>
    internal readonly List<EndingUpload> uploads = [];

    /// <summary>The ending's colours, including partial fades.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>Whether the explosion's four maps and their characters are loaded, which the explosion displays show.</summary>
    internal bool ExplosionMapsLoaded { get; private set; }

    /// <summary>Whether the post-credits art is loaded: the waiting backdrop and the wavy Samus the post-credits scenes show.</summary>
    internal bool PostCreditsArtLoaded { get; private set; }

    /// <summary>How many of the post-shot's subtitle and logo parts are shown.</summary>
    internal int PostShotPartsShown { get; private set; }

    /// <summary>The text the result and copyright page shows, as of its last showing.</summary>
    internal ImmutableArray<ShownEndingText> ResultPageText { get; private set; } = [];

    /// <summary>The text the final page's upper screen shows, or null before the white flash first shows it.</summary>
    internal ImmutableArray<ShownEndingText>? FinalPageText { get; private set; }

    /// <summary>The text the final page's lower screen shows, a copy of the staging page at the end of the white flash.</summary>
    internal ImmutableArray<ShownEndingText> FinalPageLowerText { get; private set; } = [];

    /// <summary>Clears every load and every page, as a full VRAM clear does.</summary>
    public void Clear()
    {
        uploads.Clear();
        ExplosionMapsLoaded = false;
        PostCreditsArtLoaded = false;
        PostShotPartsShown = 0;
        ResultPageText = [];
        FinalPageText = null;
        FinalPageLowerText = [];
        ForgetShown();
    }

    /// <summary>Loads the explosion's maps and characters.</summary>
    internal void LoadExplosionMaps() => ExplosionMapsLoaded = true;

    /// <summary>Loads the post-credits art, whose reward and shooting characters replace the floor's.</summary>
    internal void LoadPostCreditsArt()
    {
        PostCreditsArtLoaded = true;
        Record(new EndingSheetUpload(EndingUploadKind.CreditsCharacterArt));
    }

    /// <summary>Shows the post-shot's next subtitle or logo part.</summary>
    /// <exception cref="InvalidOperationException">Every part is already shown.</exception>
    internal void ShowPostShotPart()
    {
        if (PostShotPartsShown >= EndingPostShotParts.Count)
            throw new InvalidOperationException("The post-shot has shown every part.");
        PostShotPartsShown++;
    }

    /// <summary>Whether the post-shot has shown the logo's map, so the logo plane shows the logo.</summary>
    internal bool LogoShown => PostShotPartsShown == EndingPostShotParts.Count;

    /// <summary>How many of the logo's character parts the post-shot has shown.</summary>
    internal int LogoChunksShown => Math.Clamp(PostShotPartsShown - EndingPostShotParts.FirstLogoPart, 0, EndingPostShotParts.LogoParts);

    /// <summary>Shows <paramref name="text"/> on the text page <paramref name="page"/>.</summary>
    internal void ShowText(EndingTextTilemap page, ImmutableArray<ShownEndingText> text)
    {
        switch (page)
        {
            case EndingTextTilemap.ResultPanel:
                ResultPageText = text;
                break;
            case EndingTextTilemap.FinalText:
                FinalPageText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined ending text page.");
        }
    }

    /// <summary>Shows <paramref name="text"/> on the final page's lower screen.</summary>
    internal void ShowFinalLowerText(ImmutableArray<ShownEndingText> text) => FinalPageLowerText = text;

    /// <summary>The sheets <see cref="floorTiles"/> were read from.</summary>
    [NonSerialized]
    private InstalledSpriteSheets? tileSheets;

    /// <summary>The floor's cells, until the loads or the artwork change.</summary>
    [NonSerialized]
    private byte[]? floorCells;

    /// <summary>What the floor's loads' characters show (#1392), until the loads change.</summary>
    [NonSerialized]
    private FloorTilePictures? floorTiles;

    /// <summary>What the floor's loads' characters show (#1392).</summary>
    internal FloorTilePictures FloorTiles(InstalledSpriteSheets sheets)
    {
        if (!ReferenceEquals(tileSheets, sheets))
        {
            floorTiles = null;
            tileSheets = sheets;
        }
        return floorTiles ??= EndingScreenTiles.Floor(sheets, uploads);
    }

    /// <summary>Records a load of chunk <paramref name="chunk"/> of the floor's installed artwork.</summary>
    public void Record(EndingUploadKind kind, int chunk) => Record(new EndingChunkUpload(kind, chunk));

    /// <summary>Records the load of the installed floor scene <paramref name="scene"/>.</summary>
    public void Record(EndingFloorSceneId scene) => Record(new EndingFloorSceneUpload(scene));

    /// <summary>Forgets what the screen shows after the bound artwork changes.</summary>
    public void Invalidate()
    {
        ForgetShown();
    }

    /// <summary>The floor's cells over <paramref name="floor"/> and <paramref name="flight"/>.</summary>
    internal byte[] FloorCells(EndingFloorArtwork? floor, CeresFlightArtwork? flight) =>
        floorCells ??= EndingScreenFloor.Cells(uploads, floor, flight);

    /// <summary>Forgets the floor's cells and tiles.</summary>
    private void ForgetShown()
    {
        floorCells = null;
        floorTiles = null;
    }

    /// <summary>Appends <paramref name="upload"/>, dropping the earlier load whose cells it covers.</summary>
    private void Record(EndingUpload upload)
    {
        uploads.RemoveAll(earlier => earlier.IsReplacedBy(upload));
        uploads.Add(upload);
        ForgetShown();
    }
}
