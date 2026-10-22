using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;

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

    /// <summary>File-select sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<FileSelectPresentation, SpriteSheet> fileSelects = new();

    /// <summary>The file-select sheet <paramref name="fileSelect"/> composes over the installation's menus.</summary>
    internal static SpriteSheet FileSelect(FileSelectPresentation fileSelect)
    {
        ArgumentNullException.ThrowIfNull(fileSelect);
        return fileSelect.ContentIdentity == InstalledFixtures.Maps.FileSelect.ContentIdentity
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.FileSelect)
            : fileSelects.GetValue(fileSelect, key => Load(SpriteSheetId.FileSelect, FileSelectSheetComposer.Sources(key, InstalledFixtures.Maps),
                $"presentation-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Options sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<GameOptionsPresentation, SpriteSheet> options = new();

    /// <summary>The options sheet <paramref name="presentation"/> composes over the installation's menus.</summary>
    internal static SpriteSheet GameOptions(GameOptionsPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        return presentation.ContentIdentity == InstalledFixtures.Maps.GameOptions.ContentIdentity
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.GameOptions)
            : options.GetValue(presentation, key => Load(SpriteSheetId.GameOptions, GameOptionsSheetComposer.Sources(key, InstalledFixtures.Maps),
                $"presentation-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Composes and registers a fixture presentation's menu sheets beside the installed others.</summary>
    private static InstalledSpriteSheets Compose(AreaMapPresentationCatalog maps)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach ((SpriteSheetId id, SpriteSheetSources sources) in new[]
                 {
                     (SpriteSheetId.GameOver, GameOverSheetComposer.Sources(maps)),
                     (SpriteSheetId.FileSelect, FileSelectSheetComposer.Sources(maps)),
                     (SpriteSheetId.GameOptions, GameOptionsSheetComposer.Sources(maps)),
                 })
            sheets[id] = Load(id, sources, $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Composes, loads and registers one fixture sheet under a name of its own.</summary>
    private static SpriteSheet Load(SpriteSheetId id, SpriteSheetSources sources, string fixture)
    {
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
        // A name of its own keeps its frames apart from the installed sheet's in the frame-source registry.
        SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileName(id)}-fixture-{fixture}", sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.Priorities, sheet.PartRows);
        SheetFrameSources.Register(loaded, sources);
        return loaded;
    }
}
