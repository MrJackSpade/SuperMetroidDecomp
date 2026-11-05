using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction;

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
        [SpriteSheetId.SamusBody, SpriteSheetId.SamusOverlays, SpriteSheetId.SamusAtmosphere,
            SpriteSheetId.SamusDeath];

    /// <summary>The installation's stock artwork, which a fixture's body replaces.</summary>
    private static readonly Lazy<SamusSheetInputs> inputs = new(() => SharedFixtureTiming.Load("Fixture: Samus sheet inputs",
        () => SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory)));

    /// <summary>The installation's stock artwork and the colour tables Samus's sheets are pre-coloured from.</summary>
    internal static SamusSheetInputs Inputs => inputs.Value;

    /// <summary>
    /// Sheets already composed, by the body's content identity. Every table, character, spritemap, atmosphere, death and
    /// arm-cannon catalog the body sheets are composed from enters that identity, so the bodies a fixture rebuilds with the
    /// same content compose their sheets once.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<InstalledSpriteSheets>> composed =
        new(StringComparer.Ordinal);

    /// <summary>The characters each fixture body shows, registered or bound from the installation's.</summary>
    private static readonly ConditionalWeakTable<InstalledSamusBodyArtwork, SamusBodyCharacterArtwork> characters = new();

    /// <summary>Registered characters by their identity, which other fixture bodies naming the same characters bind.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SamusBodyCharacterArtwork> registeredByIdentity = new(StringComparer.Ordinal);

    /// <summary>Atmosphere sheets already composed, by the spritemaps' and then the atmospheric attributes' content identities.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<InstalledSpriteSheets>> atmospheres =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Body sheets already painted, by sheet and layout identity: a layout's identity covers everything painting reads,
    /// so fixture bodies whose tables lay a sheet out alike share its painted sheet and its registered frames.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<SpriteSheet>> paintedLayouts =
        new(StringComparer.Ordinal);

    /// <summary>Sheets already composed, by the body's and then the arm cannon's content identities.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<InstalledSpriteSheets>> cannons =
        new(StringComparer.Ordinal);

    /// <summary>The installed sheets with the body and overlay sheets <paramref name="body"/> composes.</summary>
    internal static InstalledSpriteSheets For(InstalledSamusBodyArtwork body)
    {
        ArgumentNullException.ThrowIfNull(body);
        string identity = body.ContentIdentity;
        return identity == inputs.Value.SamusBody.Body.ContentIdentity ? InstalledFixtures.SpriteSheets
            : composed.GetOrAdd(identity, key => new(() => Compose(InstalledFixtures.SpriteSheets,
                inputs.Value with { SamusBody = CharactersOf(body) }, BodySheets, body.ArmCannon, $"fixture-{key}"))).Value;
    }

    /// <summary>Registers a fixture body's own characters, which its sheets compose from; returns the body.</summary>
    internal static InstalledSamusBodyArtwork Register(SamusBodyCharacterArtwork artwork)
    {
        ArgumentNullException.ThrowIfNull(artwork);
        characters.AddOrUpdate(artwork.Body, artwork);
        registeredByIdentity[artwork.Body.BodyCharactersContentIdentity] = artwork;
        return artwork.Body;
    }

    /// <summary>
    /// The characters <paramref name="body"/> shows: those registered for it, or the registered or installed characters it
    /// names, bound to its frames.
    /// </summary>
    internal static SamusBodyCharacterArtwork CharactersOf(InstalledSamusBodyArtwork body)
    {
        if (characters.TryGetValue(body, out SamusBodyCharacterArtwork? registered))
            return registered;
        SamusBodyCharacterArtwork stock = registeredByIdentity.TryGetValue(body.BodyCharactersContentIdentity, out SamusBodyCharacterArtwork? named)
            ? named : inputs.Value.SamusBody;
        if (body.BodyCharactersContentIdentity != stock.Body.BodyCharactersContentIdentity)
            throw new InvalidOperationException("A fixture body with its own characters registers them before composing its sheets.");
        return characters.GetValue(body, key => new SamusBodyCharacterArtwork(key,
            [.. Enumerable.Range(0, SamusBodySources.TopSetCount).Select(set => stock.TopSet(set).ToArray())],
            [.. Enumerable.Range(0, SamusBodySources.BottomSetCount).Select(set => stock.BottomSet(set).ToArray())]));
    }

    /// <summary>The sheets <paramref name="body"/> draws from, with the overlay sheet <paramref name="cannon"/> composes.</summary>
    internal static InstalledSpriteSheets For(InstalledSamusBodyArtwork body, InstalledSamusArmCannonArtwork cannon)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(cannon);
        if (cannon.ContentIdentity == body.ArmCannon.ContentIdentity)
            return For(body);
        return cannons.GetOrAdd($"{body.ContentIdentity}-{cannon.ContentIdentity}", key => new(() => Compose(For(body),
            inputs.Value with { SamusBody = CharactersOf(body) }, [SpriteSheetId.SamusOverlays], cannon, $"fixture-{key}"))).Value;
    }

    /// <summary>
    /// The installed sheets with an atmosphere sheet composed from <paramref name="spritemaps"/> and
    /// <paramref name="atmosphere"/>, each the installed body's when not given.
    /// </summary>
    internal static InstalledSpriteSheets ForAtmosphere(InstalledSamusSpritemaps? spritemaps = null,
        InstalledSamusAtmosphericArtwork? atmosphere = null)
    {
        InstalledSamusBodyArtwork installed = inputs.Value.SamusBody.Body;
        spritemaps ??= installed.Spritemaps;
        atmosphere ??= installed.Atmosphere;
        if (spritemaps.ContentIdentity == installed.Spritemaps.ContentIdentity && atmosphere.ContentIdentity == installed.Atmosphere.ContentIdentity)
            return InstalledFixtures.SpriteSheets;
        InstalledSamusSpritemaps maps = spritemaps;
        InstalledSamusAtmosphericArtwork attributes = atmosphere;
        return atmospheres.GetOrAdd($"{maps.ContentIdentity}-{attributes.ContentIdentity}", key => new(() =>
        {
            SpriteSheetSources sources = SamusAtmosphereSheetComposer.Sources(inputs.Value, maps.Source, attributes.Source);
            return Replace(InstalledFixtures.SpriteSheets, SpriteSheetId.SamusAtmosphere, sources, $"fixture-{key}");
        })).Value;
    }

    /// <summary>Binds a fixture's body artwork and the sheets it draws from.</summary>
    extension(SamusBodyFrameState transfers)
    {
        /// <summary>Binds <paramref name="body"/> and the sheets composed from it.</summary>
        internal void BindBodyFixture(InstalledSamusBodyArtwork body)
        {
            transfers.BindArtwork(body);
            transfers.BindSheets(For(body));
        }

        /// <summary>Binds the body of <paramref name="artwork"/> and the sheets composed from its characters.</summary>
        internal void BindBodyFixture(SamusBodyCharacterArtwork artwork) => transfers.BindBodyFixture(Register(artwork));
    }

    /// <summary>Binds a fixture's arm cannon and the overlay sheet composed from it.</summary>
    extension(SamusState samus)
    {
        /// <summary>Binds <paramref name="cannon"/> over Samus's bound body, and the sheets they draw from.</summary>
        internal void BindArmCannonFixture(InstalledSamusArmCannonArtwork cannon)
        {
            InstalledSamusBodyArtwork body = samus.Presentation.BodyFrames.Artwork
                ?? throw new InvalidOperationException("An arm-cannon fixture binds over a bound body.");
            samus.ArmCannon.Artwork = cannon;
            samus.Presentation.BodyFrames.BindSheets(For(body, cannon));
        }
    }

    /// <summary>Composes and registers a fixture's sheets over <paramref name="basis"/>.</summary>
    /// <param name="basis">The sheets the fixture's replace.</param>
    /// <param name="fixture">The stock artwork with the fixture's body.</param>
    /// <param name="ids">The sheets the fixture replaces.</param>
    /// <param name="cannon">The arm cannon the overlay sheet composes.</param>
    /// <param name="suffix">Keeps the fixture's frames apart from the installed sheets' in the frame-source registry.</param>
    private static InstalledSpriteSheets Compose(InstalledSpriteSheets basis, SamusSheetInputs fixture, IEnumerable<SpriteSheetId> ids,
        InstalledSamusArmCannonArtwork cannon, string suffix)
    {
        SpriteSheetId[] replaced = [.. ids];
        var composedSheets = new SpriteSheet[replaced.Length];
        var composedSources = new SpriteSheetSources[replaced.Length];
        var painted = new bool[replaced.Length];
        // Each sheet composes from the fixture's inputs alone, so the sheets compose at once, each on its own worker; an
        // exception from any of them fails the composition. A sheet whose layout an earlier fixture painted is that
        // fixture's sheet: a fixture body that changes only some of the tables leaves the sheets the others compose alike.
        // The frames of each newly painted sheet are registered afterwards on this thread.
        System.Threading.Tasks.Parallel.For(0, replaced.Length, index =>
        {
            SpriteSheetId id = replaced[index];
            composedSources[index] = id == SpriteSheetId.SamusOverlays ? SamusOverlaySheetComposer.Sources(fixture, cannon.Source)
                : SamusSheetSources.For(fixture, id);
            var layout = new SpriteSheetLayout(composedSources[index], 0, 0);
            composedSheets[index] = paintedLayouts.GetOrAdd($"{id}-{layout.Identity}", _ => new(() =>
            {
                painted[index] = true;
                return Load(id, layout, suffix);
            })).Value;
        });
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(sheet => sheet, basis.Get);
        for (int index = 0; index < replaced.Length; index++)
        {
            if (painted[index])
                SheetFrameSources.Register(composedSheets[index], composedSources[index]);
            sheets[replaced[index]] = composedSheets[index];
        }
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Composes and registers one fixture sheet in place of <paramref name="id"/>'s in <paramref name="basis"/>.</summary>
    private static InstalledSpriteSheets Replace(InstalledSpriteSheets basis, SpriteSheetId id, SpriteSheetSources sources, string suffix)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(sheet => sheet, basis.Get);
        SpriteSheet loaded = Load(id, sources, suffix);
        SheetFrameSources.Register(loaded, sources);
        sheets[id] = loaded;
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Composes and loads the fixture sheet <paramref name="id"/> from <paramref name="sources"/>.</summary>
    private static SpriteSheet Load(SpriteSheetId id, SpriteSheetSources sources, string suffix) =>
        Load(id, new SpriteSheetLayout(sources, 0, 0), suffix);

    /// <summary>Paints and loads the fixture sheet <paramref name="id"/> laid out as <paramref name="layout"/>.</summary>
    private static SpriteSheet Load(SpriteSheetId id, SpriteSheetLayout layout, string suffix)
    {
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(layout);
        return SpriteSheetFileConstants.Load(new MemoryStream(SpriteSheetManifestWriter.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-{suffix}", sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.Priorities);
    }
}
