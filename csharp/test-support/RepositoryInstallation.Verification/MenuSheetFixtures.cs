using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the sprite sheets a fixture's menus draw from. The installation's menu presentation
/// draws from the installed sheets; a fixture's own presentation gets its menu sheets composed from it, as an
/// installation of that presentation would hold, registered so OAM comparisons still apply to them.
/// </summary>
internal static class MenuSheetFixtures
{
    /// <summary>Sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<AreaMapPresentationCatalog, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the menu sheets <paramref name="maps"/> composes.</summary>
    internal static InstalledSpriteSheets For(AreaMapPresentationCatalog maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return maps.ContentIdentity == InstalledFixtures.Maps.ContentIdentity ? InstalledFixtures.SpriteSheets : composed.GetValue(maps, Compose);
    }

    /// <summary>Composes and registers a fixture presentation's menu sheets beside the installed others.</summary>
    private static InstalledSpriteSheets Compose(AreaMapPresentationCatalog maps)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        SpriteSheetSources sources = GameOverSheetComposer.Sources(maps);
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
        // A name of its own keeps its frames apart from the installed sheet's in the frame-source registry.
        SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileName(SpriteSheetId.GameOver)}-fixture-{RuntimeHelpers.GetHashCode(maps):X8}", sheet.Size, sheet.Pixels,
            sheet.PaletteIndices, sheet.Priorities, sheet.PartRows);
        SheetFrameSources.Register(loaded, sources);
        sheets[SpriteSheetId.GameOver] = loaded;
        return new InstalledSpriteSheets(sheets);
    }
}
