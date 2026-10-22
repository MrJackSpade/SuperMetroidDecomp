using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured sprite sheets (#1392): for each sheet an RGBA PNG and its frame manifest, with a provenance
/// manifest of their hashes. A player override of either file replaces the stock one.
/// </summary>
[CpuBusBoundary("codec: ROM sprite frames, characters and palette rows → pre-coloured sprite sheets")]
public static class SpriteSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ManifestFileName = "sprite-sheets.json";

    /// <summary>Composes and writes every sheet, then validates the result.</summary>
    /// <param name="bus">Cartridge source for frames no installed artwork holds.</param>
    /// <param name="content">Installed stock content holding the characters, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string content, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ManifestFileName);
        var sources = new SpriteSheetSourceCatalog(content);
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
            store.Write(SpriteSheetDefinitions.FileName(sheet), Compose(bus, sources, sheet));
        store.Finish(sourceCartridgeSha256);
        _ = Load(directory, null);
    }

    /// <summary>Checks the stock files against their manifest and loads every sheet, preferring a player override of each file.</summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static InstalledSpriteSheets Load(string stockDirectory, string? overrideDirectory)
    {
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ManifestFileName, SpriteSheetDefinitions.Sheets.Count);
        var sheets = new Dictionary<SpriteSheetId, SpriteSheet>();
        // The body sheets are named from the stock body tables installed beside them.
        var body = new Lazy<SamusBodyArtworkCatalog>(() => SamusBodyArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))!, GameInstallationLayout.SamusBodyDirectoryName), null));
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet loaded = store.Read(SpriteSheetDefinitions.FileName(sheet));
            if (!loaded.HoldsExactly(FrameNames(sheet, body)))
                throw new InvalidDataException($"Sprite sheet {FrameFile(sheet)} does not hold exactly the frames its family draws.");
            sheets.Add(sheet, loaded);
        }
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Composes one sheet.</summary>
    private static ComposedSpriteSheet Compose(ISnesAddressSpace bus, SpriteSheetSourceCatalog sources, SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => EscapeTimerSheetComposer.Compose(bus),
        _ => sources.Compose(sheet),
    };

    /// <summary>The frames a sheet must hold: the ones its family draws.</summary>
    /// <param name="sheet">The sheet.</param>
    /// <param name="body">Samus's body tables, which name the body sheets' frames.</param>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, Lazy<SamusBodyArtworkCatalog> body) => sheet switch
    {
        SpriteSheetId.EscapeTimer => EscapeTimerPresentationDefinitions.Glyphs.Select(EscapeTimerPresentationDefinitions.FrameName),
        SpriteSheetId.Grapple => GrappleSheetDefinitions.Frames,
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.FrameNames(body.Value, sheet),
        SpriteSheetId.SamusOverlays => SamusOverlaySheetDefinitions.Frames,
        SpriteSheetId.SamusAtmosphere => SamusAtmosphereSheetDefinitions.Frames,
        SpriteSheetId.SamusDeath => SamusDeathSheetDefinitions.Frames,
        SpriteSheetId.GameOver => GameOverPresentationDefinitions.SpriteNames.ToArray(),
        SpriteSheetId.FileSelect => FileSelectPresentationDefinitions.SheetFrames,
        _ => ProjectileSheetDefinitions.Frames,
    };

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(SpriteSheetId sheet) => SpriteSheetStore.FrameFile(SpriteSheetDefinitions.FileName(sheet));
}
