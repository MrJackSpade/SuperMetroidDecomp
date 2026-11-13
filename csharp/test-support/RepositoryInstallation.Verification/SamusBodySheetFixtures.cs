using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
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
        [SpriteSheetId.SamusBody, SpriteSheetId.SamusOverlays, SpriteSheetId.SamusAtmosphere,
            SpriteSheetId.SamusDeath];

    /// <summary>
    /// The stock artwork, which a fixture's body replaces, read from the cartridge-derived extraction: its characters are
    /// import data no installation holds (#1636).
    /// </summary>
    private static readonly Lazy<SamusSheetInputs> inputs = new(() => SharedFixtureTiming.Load("Fixture: Samus sheet inputs",
        () => SamusSheetInputs.Load(CartridgeImportContent.Directory)));

    /// <summary>The installation's stock artwork and the colour tables Samus's sheets are pre-coloured from.</summary>
    internal static SamusSheetInputs Inputs => inputs.Value;

    /// <summary>
    /// Sheets already composed, by the body's content identity and its characters' identity. Every table, spritemap,
    /// atmosphere, death and arm-cannon catalog the body sheets are composed from enters the first, and the characters the
    /// second, so the bodies a fixture rebuilds with the same content compose their sheets once.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<InstalledSpriteSheets>> composed =
        new(StringComparer.Ordinal);

    /// <summary>The characters each fixture body shows, registered or bound from the stock characters.</summary>
    private static readonly ConditionalWeakTable<SamusBodySources, SamusBodyCharacterArtwork> characters = new();

    /// <summary>
    /// Each character set's content identity, hashed once (#1680): a set's characters never change after it is built, so
    /// a body bound again, or a presentation bound to it again, names its sheets without hashing its characters again.
    /// </summary>
    private static readonly ConditionalWeakTable<SamusBodyCharacterArtwork, string> characterIdentities = new();

    /// <summary>The identity of the stock body and its characters, which name the installed sheets, hashed once per process.</summary>
    private static readonly Lazy<string> stockSheetIdentity = new(() => SheetIdentityOf(inputs.Value.SamusBody));

    /// <summary>The body each fixture presentation was last bound to by <c>BindBodyFixture</c>.</summary>
    private static readonly ConditionalWeakTable<SamusPresentation, SamusBodySources> boundBodies = new();

    /// <summary>
    /// Registered characters by the definition-group starts they fill, which other fixture bodies built over the same groups
    /// bind: a body holds no characters (#1636), so a body rebuilt from a registered fixture's tables shows its characters.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SamusBodyCharacterArtwork> registeredByGroups = new(StringComparer.Ordinal);

    /// <summary>The key of the definition-group starts <paramref name="body"/>'s frames select within.</summary>
    private static string GroupsOf(SamusBodySources body) =>
        $"{string.Join(',', body.TopSetStarts.ToArray())}/{string.Join(',', body.BottomSetStarts.ToArray())}";

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
    internal static InstalledSpriteSheets For(SamusBodySources body)
    {
        ArgumentNullException.ThrowIfNull(body);
        SamusBodyCharacterArtwork shown = CharactersOf(body);
        string identity = SheetIdentityOf(shown);
        return identity == stockSheetIdentity.Value ? InstalledFixtures.SpriteSheets
            : composed.GetOrAdd(identity, key => new(() => Compose(InstalledFixtures.SpriteSheets,
                inputs.Value with { SamusBody = shown }, BodySheets, body.ArmCannon, $"fixture-{key}"))).Value;
    }

    /// <summary>The content identity of <paramref name="artwork"/>'s characters, hashed on its first use.</summary>
    private static string CharactersIdentityOf(SamusBodyCharacterArtwork artwork) =>
        characterIdentities.GetValue(artwork, SamusBodyCharacterArtworkConstants.CharactersContentIdentityOf);

    /// <summary>The identity the body sheets <paramref name="shown"/> composes are kept by: its body's and its characters'.</summary>
    private static string SheetIdentityOf(SamusBodyCharacterArtwork shown) =>
        $"{shown.Body.ContentIdentity}-{CharactersIdentityOf(shown)}";

    /// <summary>Registers a fixture body's own characters, which its sheets compose from; returns the body.</summary>
    internal static SamusBodySources Register(SamusBodyCharacterArtwork artwork)
    {
        ArgumentNullException.ThrowIfNull(artwork);
        characters.AddOrUpdate(artwork.Body, artwork);
        registeredByGroups[GroupsOf(artwork.Body)] = artwork;
        return artwork.Body;
    }

    /// <summary>
    /// The characters <paramref name="body"/> shows: those registered for it, or those registered for a body over the same
    /// definition groups, or else the stock characters, bound to its frames. A body holds no characters (#1636), so one
    /// that shows characters of its own registers them.
    /// </summary>
    internal static SamusBodyCharacterArtwork CharactersOf(SamusBodySources body)
    {
        if (characters.TryGetValue(body, out SamusBodyCharacterArtwork? registered))
            return registered;
        SamusBodyCharacterArtwork stock = registeredByGroups.TryGetValue(GroupsOf(body), out SamusBodyCharacterArtwork? named)
            ? named : inputs.Value.SamusBody;
        return characters.GetValue(body, key =>
        {
            var bound = new SamusBodyCharacterArtwork(key,
                [.. Enumerable.Range(0, SamusBodySources.TopSetCount).Select(set => stock.TopSet(set).ToArray())],
                [.. Enumerable.Range(0, SamusBodySources.BottomSetCount).Select(set => stock.BottomSet(set).ToArray())]);
            // Each set is copied whole, so over the same group starts every record holds the source's definition and the
            // characters' identity is the source's, already hashed (#1680).
            if (GroupsOf(key) == GroupsOf(stock.Body))
                characterIdentities.AddOrUpdate(bound, CharactersIdentityOf(stock));
            return bound;
        });
    }

    /// <summary>The sheets <paramref name="body"/> draws from, with the overlay sheet <paramref name="cannon"/> composes.</summary>
    internal static InstalledSpriteSheets For(SamusBodySources body, SamusArmCannonSourceArtwork cannon)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(cannon);
        if (cannon.ContentIdentity == body.ArmCannon.ContentIdentity)
            return For(body);
        return cannons.GetOrAdd($"{SheetIdentityOf(CharactersOf(body))}-{cannon.ContentIdentity}", key => new(() => Compose(For(body),
            inputs.Value with { SamusBody = CharactersOf(body) }, [SpriteSheetId.SamusOverlays], cannon, $"fixture-{key}"))).Value;
    }

    /// <summary>
    /// The installed sheets with an atmosphere sheet composed from <paramref name="spritemaps"/> and
    /// <paramref name="atmosphere"/>, each the installed body's when not given.
    /// </summary>
    internal static InstalledSpriteSheets ForAtmosphere(SamusSpritemapSources? spritemaps = null,
        SamusAtmosphericSourceArtwork? atmosphere = null)
    {
        SamusBodySources installed = inputs.Value.SamusBody.Body;
        spritemaps ??= installed.Spritemaps;
        atmosphere ??= installed.Atmosphere;
        if (spritemaps.ContentIdentity == installed.Spritemaps.ContentIdentity && atmosphere.ContentIdentity == installed.Atmosphere.ContentIdentity)
            return InstalledFixtures.SpriteSheets;
        SamusSpritemapSources maps = spritemaps;
        SamusAtmosphericSourceArtwork attributes = atmosphere;
        return atmospheres.GetOrAdd($"{maps.ContentIdentity}-{attributes.ContentIdentity}", key => new(() =>
        {
            SpriteSheetSources sources = SamusAtmosphereSheetComposer.Sources(inputs.Value, maps, attributes);
            return Replace(InstalledFixtures.SpriteSheets, SpriteSheetId.SamusAtmosphere, sources, $"fixture-{key}");
        })).Value;
    }

    /// <summary>Binds a fixture's body artwork and the sheets it draws from.</summary>
    extension(SamusPresentation presentation)
    {
        /// <summary>Binds what the game reads of <paramref name="body"/> and the sheets composed from it.</summary>
        internal void BindBodyFixture(SamusBodySources body)
        {
            ArgumentNullException.ThrowIfNull(body);
            presentation.BindArtwork(body.Gameplay);
            presentation.BindSheets(For(body));
            boundBodies.AddOrUpdate(presentation, body);
        }

        /// <summary>The body a fixture bound to this presentation, as the import reads it.</summary>
        /// <exception cref="InvalidOperationException">The presentation shows no body a fixture bound.</exception>
        internal SamusBodySources BoundBodyFixture => boundBodies.TryGetValue(presentation, out SamusBodySources? body) &&
            ReferenceEquals(presentation.Artwork, body.Gameplay)
                ? body
                : throw new InvalidOperationException("Samus shows no body a fixture bound.");

        /// <summary>Binds the body of <paramref name="artwork"/> and the sheets composed from its characters.</summary>
        internal void BindBodyFixture(SamusBodyCharacterArtwork artwork) => presentation.BindBodyFixture(Register(artwork));
    }

    /// <summary>Binds a fixture's arm cannon and the overlay sheet composed from it.</summary>
    extension(SamusState samus)
    {
        /// <summary>Binds <paramref name="cannon"/> over the body fixture bound to Samus, and the sheets they draw from.</summary>
        /// <exception cref="InvalidOperationException">Samus shows no body a fixture bound.</exception>
        internal void BindArmCannonFixture(SamusArmCannonSourceArtwork cannon)
        {
            ArgumentNullException.ThrowIfNull(cannon);
            SamusBodySources body = samus.Presentation.BoundBodyFixture;
            samus.ArmCannon.Artwork = cannon.Gameplay;
            samus.Presentation.BindSheets(For(body, cannon));
        }
    }

    /// <summary>Composes and registers a fixture's sheets over <paramref name="basis"/>.</summary>
    /// <param name="basis">The sheets the fixture's replace.</param>
    /// <param name="fixture">The stock artwork with the fixture's body.</param>
    /// <param name="ids">The sheets the fixture replaces.</param>
    /// <param name="cannon">The arm cannon the overlay sheet composes.</param>
    /// <param name="suffix">Keeps the fixture's frames apart from the installed sheets' in the frame-source registry.</param>
    private static InstalledSpriteSheets Compose(InstalledSpriteSheets basis, SamusSheetInputs fixture, IEnumerable<SpriteSheetId> ids,
        SamusArmCannonSourceArtwork cannon, string suffix)
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
            // A fixture's cannon places the stock cover characters, which the cartridge-derived extraction holds (#1636).
            composedSources[index] = id == SpriteSheetId.SamusOverlays
                ? SamusOverlaySheetComposer.Sources(fixture, SamusArmCannonArtworkFiles.LoadCoverCharacters(cannon,
                    CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.SamusBodyDirectoryName)))
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
        return SpriteSheetFileConstants.Load(sheet.Manifest,
            $"{SpriteSheetDefinitions.FileNames[id]}-{suffix}", sheet.Size, sheet.Pixels, sheet.PaletteIndices);
    }
}
