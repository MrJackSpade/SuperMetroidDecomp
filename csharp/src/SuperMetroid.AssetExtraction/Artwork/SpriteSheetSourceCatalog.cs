using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What each installed sprite sheet composes from the stock content installed beside it (#1392), each family's inputs
/// loaded once when a sheet of it is first composed. The escape timer composes from the cartridge and is not here.
/// </summary>
internal sealed class SpriteSheetSourceCatalog
{
    /// <summary>Samus's stock artwork.</summary>
    private readonly Lazy<SamusSheetInputs> samus;

    /// <summary>The menus' presentation.</summary>
    private readonly Lazy<AreaMapPresentationCatalog> maps;

    /// <summary>Reads inputs from installed stock content.</summary>
    internal SpriteSheetSourceCatalog(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        samus = new(() => SamusSheetInputs.Load(content));
        maps = new(() => AreaMapPresentationCatalog.Load(Path.Combine(content, GameInstallationLayout.MapDirectoryName), null));
    }

    /// <summary>The OBJ palette row and priority of parts that choose neither, as the families' draws leave them.</summary>
    private const int DefaultPalette = 0, DefaultPriority = 0;

    /// <summary>Composes sheet <paramref name="sheet"/>.</summary>
    internal ComposedSpriteSheet Compose(SpriteSheetId sheet) => SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultPriority);

    /// <summary>What sheet <paramref name="sheet"/> composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is the escape timer, which composes from the cartridge.</exception>
    internal SpriteSheetSources For(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The escape timer composes from the cartridge."),
        SpriteSheetId.GameOver => GameOverSheetComposer.Sources(maps.Value),
        SpriteSheetId.FileSelect => FileSelectSheetComposer.Sources(maps.Value),
        _ => SamusSheetSources.For(samus.Value, sheet),
    };
}
