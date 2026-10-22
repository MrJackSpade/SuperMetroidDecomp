using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the sprite sheets a fixture's Samus draws from. A fixture whose body tables, tiles or
/// arm cannon differ from the installation's gets body and overlay sheets composed from its own, as an installation of
/// that body would hold; the installation's body draws from the installed sheets.
/// </summary>
internal static class SamusBodySheetFixtures
{
    /// <summary>The sheets a body's tables, tiles and arm cannon name.</summary>
    private static readonly SpriteSheetId[] BodySheets =
        [SpriteSheetId.SamusBodyTop, SpriteSheetId.SamusBodyBottom, SpriteSheetId.SamusOverlays, SpriteSheetId.SamusAtmosphere];

    /// <summary>The installation's stock artwork, which a fixture's body replaces.</summary>
    private static readonly Lazy<SamusSheetInputs> inputs = new(() => SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory));

    /// <summary>Sheets already composed, by body.</summary>
    private static readonly ConditionalWeakTable<SamusBodyArtworkCatalog, InstalledSpriteSheets> composed = new();

    /// <summary>Atmosphere sheets already composed, by spritemaps and then atmospheric attributes.</summary>
    private static readonly ConditionalWeakTable<SamusSpritemapArtworkCatalog, ConditionalWeakTable<SamusAtmosphericArtworkCatalog, InstalledSpriteSheets>> atmospheres = new();

    /// <summary>Sheets already composed, by arm cannon and then body.</summary>
    private static readonly ConditionalWeakTable<SamusArmCannonArtworkCatalog, ConditionalWeakTable<SamusBodyArtworkCatalog, InstalledSpriteSheets>> cannons = new();

    /// <summary>The installed sheets with the body and overlay sheets <paramref name="body"/> composes.</summary>
    internal static InstalledSpriteSheets For(SamusBodyArtworkCatalog body)
    {
        ArgumentNullException.ThrowIfNull(body);
        return body.ContentIdentity == inputs.Value.SamusBody.ContentIdentity ? InstalledFixtures.SpriteSheets
            : composed.GetValue(body, key => Compose(InstalledFixtures.SpriteSheets, inputs.Value with { SamusBody = key }, BodySheets,
                key.ArmCannon, $"fixture-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>The sheets <paramref name="body"/> draws from, with the overlay sheet <paramref name="cannon"/> composes.</summary>
    internal static InstalledSpriteSheets For(SamusBodyArtworkCatalog body, SamusArmCannonArtworkCatalog cannon)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(cannon);
        if (cannon.ContentIdentity == body.ArmCannon.ContentIdentity)
            return For(body);
        return cannons.GetOrCreateValue(cannon).GetValue(body, key => Compose(For(key), inputs.Value with { SamusBody = key },
            [SpriteSheetId.SamusOverlays], cannon, $"fixture-{RuntimeHelpers.GetHashCode(key):X8}-{RuntimeHelpers.GetHashCode(cannon):X8}"));
    }

    /// <summary>
    /// The installed sheets with an atmosphere sheet composed from <paramref name="spritemaps"/> and
    /// <paramref name="atmosphere"/>, each the installed body's when not given.
    /// </summary>
    internal static InstalledSpriteSheets ForAtmosphere(SamusSpritemapArtworkCatalog? spritemaps = null,
        SamusAtmosphericArtworkCatalog? atmosphere = null)
    {
        SamusBodyArtworkCatalog installed = inputs.Value.SamusBody;
        spritemaps ??= installed.Spritemaps;
        atmosphere ??= installed.Atmosphere;
        if (spritemaps.ContentIdentity == installed.Spritemaps.ContentIdentity && atmosphere.ContentIdentity == installed.Atmosphere.ContentIdentity)
            return InstalledFixtures.SpriteSheets;
        SamusAtmosphericArtworkCatalog attributes = atmosphere;
        return atmospheres.GetOrCreateValue(spritemaps).GetValue(attributes, key =>
        {
            SpriteSheetSources sources = SamusAtmosphereSheetComposer.Sources(inputs.Value, spritemaps, key);
            return Replace(InstalledFixtures.SpriteSheets, SpriteSheetId.SamusAtmosphere, sources,
                $"fixture-{RuntimeHelpers.GetHashCode(spritemaps):X8}-{RuntimeHelpers.GetHashCode(key):X8}");
        });
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

    /// <summary>Binds a fixture's arm cannon and the overlay sheet composed from it.</summary>
    extension(SamusState samus)
    {
        /// <summary>Binds <paramref name="cannon"/> over Samus's bound body, and the sheets they draw from.</summary>
        internal void BindArmCannonFixture(SamusArmCannonArtworkCatalog cannon)
        {
            SamusBodyArtworkCatalog body = samus.Presentation.TileTransfers.Artwork
                ?? throw new InvalidOperationException("An arm-cannon fixture binds over a bound body.");
            samus.ArmCannon.Artwork = cannon;
            samus.Presentation.TileTransfers.BindSheets(For(body, cannon));
        }
    }

    /// <summary>Composes and registers a fixture's sheets over <paramref name="basis"/>.</summary>
    /// <param name="basis">The sheets the fixture's replace.</param>
    /// <param name="fixture">The stock artwork with the fixture's body.</param>
    /// <param name="ids">The sheets the fixture replaces.</param>
    /// <param name="cannon">The arm cannon the overlay sheet composes.</param>
    /// <param name="suffix">Keeps the fixture's frames apart from the installed sheets' in the frame-source registry.</param>
    private static InstalledSpriteSheets Compose(InstalledSpriteSheets basis, SamusSheetInputs fixture, IEnumerable<SpriteSheetId> ids,
        SamusArmCannonArtworkCatalog cannon, string suffix)
    {
        InstalledSpriteSheets sheets = basis;
        foreach (SpriteSheetId id in ids)
        {
            SpriteSheetSources sources = id == SpriteSheetId.SamusOverlays ? SamusOverlaySheetComposer.Sources(fixture, cannon)
                : SamusSheetSources.For(fixture, id);
            sheets = Replace(sheets, id, sources, suffix);
        }
        return sheets;
    }

    /// <summary>Composes and registers one fixture sheet in place of <paramref name="id"/>'s in <paramref name="basis"/>.</summary>
    private static InstalledSpriteSheets Replace(InstalledSpriteSheets basis, SpriteSheetId id, SpriteSheetSources sources, string suffix)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(sheet => sheet, basis.Get);
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
        SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileName(id)}-{suffix}", sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.Priorities);
        SheetFrameSources.Register(loaded, sources);
        sheets[id] = loaded;
        return new InstalledSpriteSheets(sheets);
    }
}
