using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the sprite sheets a fixture's Samus draws from. A fixture whose body tables or tiles
/// differ from the installation's gets body sheets composed from its own, as an installation of that body would hold;
/// the installation's body draws from the installed sheets.
/// </summary>
internal static class SamusBodySheetFixtures
{
    /// <summary>The installation's stock artwork, which a fixture's body replaces.</summary>
    private static readonly Lazy<SamusSheetInputs> inputs = new(() => SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory));

    /// <summary>Sheets already composed, by body.</summary>
    private static readonly ConditionalWeakTable<SamusBodyArtworkCatalog, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the body sheets <paramref name="body"/> composes.</summary>
    internal static InstalledSpriteSheets For(SamusBodyArtworkCatalog body)
    {
        ArgumentNullException.ThrowIfNull(body);
        return body.ContentIdentity == inputs.Value.SamusBody.ContentIdentity ? InstalledFixtures.SpriteSheets : composed.GetValue(body, Compose);
    }

    /// <summary>Binds a fixture's body artwork and the sheets it draws from.</summary>
    extension(SamusTileTransferState transfers)
    {
        /// <summary>Binds <paramref name="body"/> and the sheets composed from it.</summary>
        internal void BindBodyFixture(SamusBodyArtworkCatalog body)
        {
            transfers.BindArtwork(body);
            transfers.BindSheets(For(body));
        }
    }

    /// <summary>Composes and registers a fixture body's sheets beside the installed others.</summary>
    private static InstalledSpriteSheets Compose(SamusBodyArtworkCatalog body)
    {
        SamusSheetInputs fixture = inputs.Value with { SamusBody = body };
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach (SpriteSheetId id in new[] { SpriteSheetId.SamusBodyTop, SpriteSheetId.SamusBodyBottom })
        {
            SpriteSheetSources sources = SamusBodySheetComposer.Sources(fixture, id);
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
            // A name of its own keeps its frames apart from the installed sheet's in the frame-source registry.
            SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
                $"{SpriteSheetDefinitions.FileName(id)}-fixture-{RuntimeHelpers.GetHashCode(body):X8}", sheet.Size, sheet.Pixels,
                sheet.PaletteIndices, sheet.Priorities);
            SheetFrameSources.Register(loaded, sources);
            sheets[id] = loaded;
        }
        return new InstalledSpriteSheets(sheets);
    }
}
