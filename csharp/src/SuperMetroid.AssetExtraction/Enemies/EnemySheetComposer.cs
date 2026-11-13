using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Composes the pre-coloured enemy sheets (#1392). Each room graphics set is loaded as a room load leaves it: the
/// standard OBJ characters with each member's tiles uploaded after them, and each member's palette loaded into its
/// row. Every member's sheet holds the frames its behaviour shows, composed at its tile index and coloured in its row
/// as the set leaves it, so a row a later member overwrites shows that member's colours, as on the cartridge. Each
/// sheet is also pre-coloured in the shades its enemy shows (#1512): a hit or freeze flash and the enemy projectiles'
/// colours as the cartridge ORs their row into each part's own, and its own effects.
/// </summary>
internal static class EnemySheetComposer
{
    /// <summary>The byte of the OBJ character table the enemy tile region begins at.</summary>
    private const int EnemyRegionTableByte = EncounterCharacterPlacementDefinitions.EnemyTileAreaFirstCharacter * SpriteCharacterTableLayout.CharacterByteCount;


    /// <summary>
    /// Every variant of the enemies rooms draw and of their spawned objects (#1599), each distinct sheet once. Each graphics
    /// set rooms draw under is decoded once, as its rooms load it, and every sheet it composes is laid out first; sheets
    /// whose layouts share an identity paint alike, so each distinct layout is painted and converted as the installation holds
    /// it once, in parallel, each worker on its own reused canvas (#1490); the picture store writes the pictures (#1635). The
    /// spawned-object sheets' common art is then held once (#1513): each distinct spawned-object layout keeps the pictures
    /// particular to it, and a picture several hold is held on a common sheet of its colouring
    /// (<see cref="SpawnedObjectSheetSharing"/>). The sheets and variants are then named in set
    /// order, the session start's first, whose variants are the standard ones: a sheet or variant the same as an earlier one
    /// takes its name. Each population is then named the variants of the set its rooms draw under.
    /// </summary>
    internal static EnemySheetComposition Compose(EnemySheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var layouts = new List<SpriteSheetLayout>();
        var layoutOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var spawnedLayouts = new List<SpriteSheetLayout>();
        var spawnedLayoutOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var laidSets = new List<LaidGraphicsSet>();
        // Each set's sheets are sourced and laid out in parallel (#1530); the distinct layouts are then kept in set order,
        // so which set first lays a shared sheet, and so the sheets' order, stays the same on every install.
        RoomEnemyGraphicsSetDefinition[] drawnSets = [.. RoomEnemyGraphicsSetLists.DrawnUnder];
        var setLayouts = new SetLayouts[drawnSets.Length];
        Parallel.For(0, drawnSets.Length, index =>
        {
            GraphicsSetSheetSources setSources = Sources(inputs, drawnSets[index]);
            setLayouts[index] = new SetLayouts(setSources,
                [.. setSources.Enemies.Select(source => new SpriteSheetLayout(source.Sources, 0, 0))],
                [.. setSources.Spawned.Select(source => new SpriteSheetLayout(source.Sources, 0, 0))]);
        });
        for (int index = 0; index < drawnSets.Length; index++)
        {
            SetLayouts laid = setLayouts[index];
            laidSets.Add(new LaidGraphicsSet(drawnSets[index].Name,
                [.. laid.Sources.Enemies.Select((source, sheet) => new LaidEnemySheet(source.Enemy, Laid(laid.Enemies[sheet], layouts, layoutOf),
                    source.LookChanges, source.CorpsePictures))],
                [.. laid.Sources.Spawned.Select((source, sheet) => new LaidSpawnedSheet(source.Plan, Laid(laid.Spawned[sheet], spawnedLayouts, spawnedLayoutOf),
                    source.LookChanges, source.CorpsePictures, source.Empty))]));
        }
        var installed = new InstalledSheet[layouts.Count];
        Parallel.For(0, layouts.Count, () => new InstalledSheetPainter(), (index, _, painter) =>
        {
            installed[index] = painter.Install(layouts[index]);
            return painter;
        }, _ => { });
        SharedSpawnedSheets shared = SpawnedObjectSheetSharing.Share(spawnedLayouts);
        var names = new SheetNames();
        var variants = new VariantNames<EnemyVariantDocument>(document => JsonSerializer.Serialize(document));
        var spawnedVariants = new VariantNames<SpawnedObjectSheetDocument>(document => JsonSerializer.Serialize(document));
        var setArtwork = new Dictionary<string, PopulationArtworkDocument>(StringComparer.Ordinal);
        // The session start's variants are named first, as each owner's standard variant.
        foreach (LaidGraphicsSet set in laidSets.OrderBy(set => set.Name == RoomEnemyGraphicsSetDefinitions.SessionStart.Name ? 0 : 1))
        {
            bool standard = set.Name == RoomEnemyGraphicsSetDefinitions.SessionStart.Name;
            var enemies = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (LaidEnemySheet sheet in set.Enemies)
            {
                string enemy = sheet.Enemy.ToString();
                string variant = variants.Of(enemy, new EnemyVariantDocument
                {
                    Enemy = enemy,
                    Sheet = names.Of(installed[sheet.Layout], enemy),
                    LookChanges = sheet.LookChanges.Count > 0 ? Document(sheet.LookChanges) : null,
                    CorpsePictures = sheet.CorpsePictures.Count > 0 ? Document(sheet.CorpsePictures) : null,
                }, standard);
                // A set that lists an enemy twice loads it once into each record's row; both must show alike.
                if (enemies.TryGetValue(enemy, out string? listed) && listed != variant)
                    throw new InvalidDataException($"Enemy graphics set {set.Name} colours {sheet.Enemy} two ways.");
                enemies[enemy] = variant;
            }
            var owners = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (LaidSpawnedSheet sheet in set.Spawned)
            {
                SpawnedObjectSheetPlan plan = sheet.Plan;
                SpawnedSheetShare share = shared.Shares[sheet.Layout];
                // The pictures particular to the sheet are on its own sheet, and each common one on the common sheet of its
                // colouring (#1513).
                string? own = share.Own is { } ownSheet ? names.Of(ownSheet, $"Spawned-{SpawnedObjectOwnerNames.Name(plan.Owner)}") : null;
                var commonPictures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                foreach ((string picture, int commonSheet) in share.Common.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    string commonName = names.Of(shared.Common[commonSheet], "Spawned-Common");
                    if (!commonPictures.TryGetValue(commonName, out List<string>? held))
                        commonPictures.Add(commonName, held = []);
                    held.Add(picture);
                }
                string variant = spawnedVariants.Of(SpawnedObjectOwnerNames.Name(plan.Owner), new SpawnedObjectSheetDocument
                {
                    Sheet = own,
                    CommonPictures = commonPictures,
                    Palette = plan.Palette,
                    Owner = SpawnedObjectOwnerNames.Name(plan.Owner),
                    Spawners = [.. plan.Spawners.Select(enemy => enemy.ToString())],
                    SharedSpawners = [.. plan.SharedSpawners.Select(enemy => enemy.ToString())],
                    Common = [.. plan.Common.Select(common => common.Spawner)],
                    Empty = [.. sheet.Empty.Order(StringComparer.Ordinal)],
                    LookChanges = sheet.LookChanges.Count > 0 ? Document(sheet.LookChanges) : null,
                    CorpsePictures = sheet.CorpsePictures.Count > 0 ? Document(sheet.CorpsePictures) : null,
                }, standard);
                // Every owner the game names the sheet by finds it under its own name.
                foreach (string owner in plan.AlsoOwnedBy.Prepend(plan.Owner).Select(SpawnedObjectOwnerNames.Name))
                    owners.Add(owner, variant);
            }
            setArtwork.Add(set.Name, new PopulationArtworkDocument { Enemies = enemies, Spawned = owners });
        }
        return new EnemySheetComposition(names.Sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetFiles.ManifestVersion,
            Variants = variants.Documents,
            SpawnedVariants = spawnedVariants.Documents,
            Standard = setArtwork[RoomEnemyGraphicsSetDefinitions.SessionStart.Name],
            Populations = RoomEnemyGraphicsSetLists.PopulationSets.ToDictionary(population => population.Key,
                population => setArtwork[population.Value.Name], StringComparer.Ordinal),
        });
    }

    /// <summary>Where a rotting corpse lies in each frame that shows one, as the manifest spells it: by frame (#1539).</summary>
    private static Dictionary<string, CorpsePictureDocument> Document(IReadOnlyDictionary<string, CorpsePicturePlacement> frames) =>
        frames.ToDictionary(frame => frame.Key, frame => CorpsePictureDocument.Of(frame.Value), StringComparer.Ordinal);

    /// <summary>
    /// How encounters' looks change each frame, as the manifest spells it (#1538): by frame, then encounter name in encounter
    /// order, the names of the looks the frame is drawn in.
    /// </summary>
    private static Dictionary<string, Dictionary<string, string[]>> Document(IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> frames) =>
        frames.ToDictionary(frame => frame.Key, frame => frame.Value.OrderBy(change => change.Encounter).ToDictionary(change => change.Encounter.ToString(),
            change => change.Looks.ToArray(), StringComparer.Ordinal), StringComparer.Ordinal);

    /// <summary>
    /// The index in <paramref name="layouts"/> of the first layout with <paramref name="layout"/>'s identity, adding it when
    /// it is the first.
    /// </summary>
    private static int Laid(SpriteSheetLayout layout, List<SpriteSheetLayout> layouts, Dictionary<string, int> layoutOf)
    {
        if (!layoutOf.TryGetValue(layout.Identity, out int index))
        {
            index = layouts.Count;
            layoutOf.Add(layout.Identity, index);
            layouts.Add(layout);
        }
        return index;
    }

    /// <summary>A graphics set's sheet sources and each sheet's layout, in the order the sources list them.</summary>
    /// <param name="Sources">The set's sheet sources.</param>
    /// <param name="Enemies">Each enemy sheet's layout.</param>
    /// <param name="Spawned">Each spawned-object sheet's layout.</param>
    private sealed record SetLayouts(GraphicsSetSheetSources Sources, SpriteSheetLayout[] Enemies, SpriteSheetLayout[] Spawned);

    /// <summary>A graphics set's sheets, each by the distinct layout it paints.</summary>
    /// <param name="Name">The set's name.</param>
    /// <param name="Enemies">Each enemy's sheet, in the order the set's sources list them.</param>
    /// <param name="Spawned">Each spawned-object sheet, in the order the set's sources list them.</param>
    private sealed record LaidGraphicsSet(string Name, IReadOnlyList<LaidEnemySheet> Enemies, IReadOnlyList<LaidSpawnedSheet> Spawned);

    /// <summary>One enemy's sheet under a graphics set.</summary>
    /// <param name="Enemy">The enemy.</param>
    /// <param name="Layout">The index of the distinct layout its sheet paints.</param>
    /// <param name="LookChanges">How encounters' looks change each frame they change.</param>
    /// <param name="CorpsePictures">Where a rotting corpse lies in each frame that shows one.</param>
    private sealed record LaidEnemySheet(EnemyDefinitionId Enemy, int Layout, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges,
        IReadOnlyDictionary<string, CorpsePicturePlacement> CorpsePictures);

    /// <summary>One spawned-object sheet under a graphics set.</summary>
    /// <param name="Plan">The tile base, palette and spawners of the objects it draws.</param>
    /// <param name="Layout">The index of the distinct spawned-object layout its sheet paints.</param>
    /// <param name="LookChanges">How encounters' looks change each frame they change.</param>
    /// <param name="CorpsePictures">Where a rotting corpse lies in each frame that shows one.</param>
    /// <param name="Empty">The frames' finished pictures that show nothing.</param>
    private sealed record LaidSpawnedSheet(SpawnedObjectSheetPlan Plan, int Layout, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges,
        IReadOnlyDictionary<string, CorpsePicturePlacement> CorpsePictures, IReadOnlySet<string> Empty);

    /// <summary>
    /// Names the distinct variants of each owner in the order they are first asked for (#1599): a variant the same as an
    /// earlier one takes its name; otherwise the session start's is its owner's standard variant, and any other its owner's
    /// next ordinal.
    /// </summary>
    /// <typeparam name="TDocument">How the manifest spells a variant.</typeparam>
    /// <param name="spelling">How the manifest spells a document; two variants are the same exactly when they spell alike.</param>
    private sealed class VariantNames<TDocument>(Func<TDocument, string> spelling)
    {
        /// <summary>The name of the variant with each identity: its owner and how the manifest spells it.</summary>
        private readonly Dictionary<string, string> named = new(StringComparer.Ordinal);

        /// <summary>How many ordinal variants each owner has been given.</summary>
        private readonly Dictionary<string, int> ordinals = new(StringComparer.Ordinal);

        /// <summary>Each distinct variant by name, in the order they were named.</summary>
        internal Dictionary<string, TDocument> Documents { get; } = new(StringComparer.Ordinal);

        /// <summary>
        /// The name of <paramref name="owner"/>'s variant <paramref name="document"/> spells: its standard variant when
        /// <paramref name="standard"/> names it for the session start's set.
        /// </summary>
        /// <exception cref="ArgumentException">The owner has two standard variants.</exception>
        internal string Of(string owner, TDocument document, bool standard)
        {
            string identity = $"{owner}\n{spelling(document)}";
            if (named.TryGetValue(identity, out string? known))
                return known;
            string name;
            if (standard)
                name = $"{owner}.Standard";
            else
            {
                int ordinal = ordinals.GetValueOrDefault(owner);
                ordinals[owner] = ordinal + 1;
                name = $"{owner}.{ordinal}";
            }
            Documents.Add(name, document);
            named.Add(identity, name);
            return name;
        }
    }

    /// <summary>
    /// Names the distinct sheets in the order they are first asked for: a sheet whose installed manifest, pixels and maps are
    /// the same as an earlier named sheet's takes its name; otherwise its owner's next ordinal, an enemy's sheets named for
    /// the enemy, a spawned-object sheet's own pictures for its owner and a common sheet of spawned-object pictures as common
    /// (#1513). This is what makes variants the player cannot tell apart one sheet (#1643), for any enemy: graphics sets that
    /// load an enemy into different palette rows compose it different sheets, but the installation holds only its pictures,
    /// frame list and the looks its draws show, so where those are the same the sheets are installed once, every set's
    /// variant names that one sheet, and the variants, then spelled alike, are one variant every placement draws in.
    /// </summary>
    private sealed class SheetNames
    {
        /// <summary>The name each converted sheet was given.</summary>
        private readonly Dictionary<InstalledSheet, string> names = new(ReferenceEqualityComparer.Instance);

        /// <summary>The name of the sheet with each fingerprint.</summary>
        private readonly Dictionary<string, string> distinct = new(StringComparer.Ordinal);

        /// <summary>How many distinct sheets each owner has been given.</summary>
        private readonly Dictionary<string, int> ordinals = new(StringComparer.Ordinal);

        /// <summary>Each distinct sheet by installed name, in the order they were named.</summary>
        internal Dictionary<string, InstalledSheet> Sheets { get; } = new(StringComparer.Ordinal);

        /// <summary>The installed name of <paramref name="sheet"/>, owned by <paramref name="owner"/> when it is new.</summary>
        internal string Of(InstalledSheet sheet, string owner)
        {
            if (names.TryGetValue(sheet, out string? known))
                return known;
            string key = Fingerprint(sheet);
            if (!distinct.TryGetValue(key, out string? name))
            {
                int ordinal = ordinals.GetValueOrDefault(owner);
                ordinals[owner] = ordinal + 1;
                name = $"{owner}.{ordinal}";
                distinct.Add(key, name);
                Sheets.Add(name, sheet);
            }
            names.Add(sheet, name);
            return name;
        }

        /// <summary>A sheet's content hash, which sheets with the same manifest, pixels and maps share.</summary>
        private static string Fingerprint(InstalledSheet sheet)
        {
            SpriteImagePixels pixels = sheet.Pixels;
            return Convert.ToHexString(SHA256.HashData(SpriteSheetManifestWriter.Write(sheet.Manifest)))
                + Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pixels.Pixels.AsSpan())))
                + "/" + (pixels.RecolourRegions is { } regions ? Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(regions.AsSpan()))) : "none")
                + "/" + (pixels.Blending is { } blending ? Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(blending.AsSpan()))) : "none");
        }
    }

    /// <summary>
    /// What graphics set <paramref name="set"/> composes. Each enemy drawn under it: its members in record order, then
    /// each enemy its rooms place without the set loading it, which draws from the standard characters in the standard
    /// palette as the cartridge falls back to them; an enemy whose definition draws no frames, as the unported ones, or
    /// draws from characters it uploads, has no sheet. And each spawned-object sheet, when its spawned objects draw from
    /// sheets.
    /// </summary>
    internal static GraphicsSetSheetSources Sources(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
    {
        LoadedGraphicsSet loaded = Load(inputs, set);
        // The shared sheet holds the objects of the common spawners the set's rooms run (#1513).
        IReadOnlyList<CommonSpawnedObjects> common = CommonSpawnersOf(set, inputs.SetMembers);
        // A spawned-object sheet with no picture to show is not installed (#1513): nothing draws from it.
        return new GraphicsSetSheetSources([.. EnemySources(inputs, loaded)],
            [.. SpawnedObjectSheetComposer.Plans(loaded.Layout, loaded.Drawn, loaded.Colours.RowOf, common).Select(plan =>
                SpawnedObjectSheetComposer.Sources(inputs.Enemies, plan, loaded.Characters, loaded.Shades, loaded.Colours.RowOf))
                .Where(source => ShowsPictures(source.Sources.Frames.Count, source.Empty, $"{set.Name} spawned {source.Plan.Owner}"))]);
    }

    /// <summary>
    /// Whether a sheet of <paramref name="pictures"/> pictures holds any (#1513). A sheet with none shows nothing and is
    /// not installed: its owner draws nothing from it.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The sheet holds no picture but names frames that show nothing, which the game would look up on a sheet that is not installed.
    /// </exception>
    private static bool ShowsPictures(int pictures, IReadOnlySet<string> blank, string sheet)
    {
        if (pictures > 0)
            return true;
        if (blank.Count > 0)
            throw new InvalidDataException(
                $"Sheet {sheet} holds no picture, only frames that show nothing ({string.Join(", ", blank.Order(StringComparer.Ordinal))}); it would not be installed for them.");
        return false;
    }

    /// <summary>What each enemy drawn under <paramref name="loaded"/> composes.</summary>
    private static IEnumerable<EnemySheetSource> EnemySources(EnemySheetInputs inputs, LoadedGraphicsSet loaded)
    {
        GraphicsSetCharacterLayout layout = loaded.Layout;
        foreach (EnemyDefinitionId enemy in loaded.Drawn)
        {
            if (!EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown))
                continue;
            int tileBase = layout.TileBaseOf(enemy);
            // The sheet is coloured in the row the set loads, or the enemy's initialization selects, its colours in.
            int paletteBits = loaded.Colours.RowOf(enemy);
            (IReadOnlyList<SheetShade> fixedShades, IReadOnlyList<SheetRowShade> rowShades) = loaded.Shades.Fixed(enemy);
            var changes = new Dictionary<string, IReadOnlyList<FrameLookChange>>(StringComparer.Ordinal);
            var corpses = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
            var blank = new HashSet<string>(StringComparer.Ordinal);
            var frames = new List<SheetFrameSource>();
            FramePictures[] pictures = [.. Frames(inputs.Enemies, enemy, shown, checked((ushort)tileBase), loaded.Characters)];
            // An enemy whose encounter sets its colours is coloured in those its frames first show in, and a frame family
            // always drawn in one colouring in that colouring (#1513).
            RgbColor[] colours = loaded.Shades.WithFrameColourings(loaded.Shades.FirstShownColours([enemy]), pictures, paletteBits,
                SharedSpawnerFrames.None);
            foreach (FramePictures framePictures in pictures)
            {
                frames.AddRange(framePictures.Sources);
                if (framePictures.Changes.Count > 0)
                    changes.Add(framePictures.Name, framePictures.Changes);
                foreach ((string picture, CorpsePicturePlacement corpse) in framePictures.Corpses)
                    corpses.Add(picture, corpse);
                blank.UnionWith(framePictures.Blank);
            }
            // An enemy whose frames compose no picture, as one its room draws only on a background, has no sheet (#1513).
            if (!ShowsPictures(frames.Count, blank, $"{enemy}"))
                continue;
            yield return new EnemySheetSource(enemy, new SpriteSheetSources(frames, colours,
                ObjectSelectionCodec.GameplaySizes, paletteBits,
                Shades: [.. loaded.Shades.OwnRow(enemy, paletteBits), .. fixedShades],
                RowShades: [.. loaded.Shades.SharedSelectedBy(enemy), .. rowShades], Blank: [.. blank.Order(StringComparer.Ordinal)]), changes, corpses);
        }
    }

    /// <summary>
    /// Graphics set <paramref name="set"/> as a room load leaves it: its enemy graphics, its OBJ page, its sprite colours,
    /// and the enemies drawn under it, its members in record order, then each enemy its rooms place without the set
    /// loading it, then the enemies those enemies' encounters spawn.
    /// </summary>
    internal static LoadedGraphicsSet Load(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(set);
        RoomEnemyGraphicsSetMembers members = RoomEnemyGraphicsSetLists.MembersOf(set, inputs.SetMembers);
        GraphicsSetCharacterLayout layout = GraphicsSetCharacterLayout.Of(members);
        LoadedSetColours colours = LoadedSetColours.Of(inputs, members);
        EnemyDefinitionId[] drawn = DrawnUnder(set, layout);
        return new LoadedGraphicsSet(layout, new SetCharacters(Table(inputs, layout), Encounters(inputs, members), RottingCorpseCells.Of(drawn)),
            colours, new EnemySheetShades(inputs, colours, Placements(set)), drawn);
    }

    /// <summary>
    /// The placements the rooms drawing under graphics set <paramref name="set"/> place each enemy with, by enemy: an enemy
    /// whose initialization chooses its colours by its placement first shows in the colours its placements choose (#1642).
    /// </summary>
    private static Dictionary<EnemyDefinitionId, IReadOnlySet<EnemySpawnSettings>> Placements(RoomEnemyGraphicsSetDefinition set)
    {
        var placements = new Dictionary<EnemyDefinitionId, HashSet<EnemySpawnSettings>>();
        foreach (RoomStateSettings state in StatesOf(set))
        {
            foreach (RoomEnemyPopulationRecord record in state.EnemyPopulation.Records.Span)
            {
                if (!placements.TryGetValue(record.DefinitionId, out HashSet<EnemySpawnSettings>? placed))
                    placements.Add(record.DefinitionId, placed = []);
                placed.Add(record.SpawnSettings);
            }
        }
        return placements.ToDictionary(entry => entry.Key, entry => (IReadOnlySet<EnemySpawnSettings>)entry.Value);
    }

    /// <summary>
    /// The common spawners whose objects graphics set <paramref name="set"/>'s shared sheet holds: those its rooms run
    /// (#1513), its members as <paramref name="members"/> names them by set name.
    /// </summary>
    /// <exception cref="InvalidDataException">A common spawner has no presence, or the set's members are not catalogued.</exception>
    internal static IReadOnlyList<CommonSpawnedObjects> CommonSpawnersOf(RoomEnemyGraphicsSetDefinition set,
        IReadOnlyDictionary<string, RoomEnemyGraphicsSetMembers> members)
    {
        Ensure.NotNull(set);
        return CommonSpawners.RunningIn(StatesOf(set),
            DrawnUnder(set, GraphicsSetCharacterLayout.Of(RoomEnemyGraphicsSetLists.MembersOf(set, members))));
    }

    /// <summary>
    /// The enemies drawn under graphics set <paramref name="set"/> laid out as <paramref name="layout"/>: its members in
    /// record order, then each enemy its rooms place without the set loading it, then the enemies those enemies'
    /// encounters spawn.
    /// </summary>
    private static EnemyDefinitionId[] DrawnUnder(RoomEnemyGraphicsSetDefinition set, GraphicsSetCharacterLayout layout)
    {
        List<EnemyDefinitionId> members = [.. layout.Members.Select(member => member.DefinitionId)];
        IEnumerable<EnemyDefinitionId> placed = StatesOf(set)
            .SelectMany(state => state.EnemyPopulation.Records.Span.SelectToArray(record => record.DefinitionId))
            .Except(members).Distinct().Order();
        EnemyDefinitionId[] loaded = [.. members, .. placed];
        // An encounter's own spawns, such as Mother Brain's tubes and the Baby, draw under the set its spawner loads.
        EnemyDefinitionId[] spawned = [.. loaded.SelectMany(enemy => EncounterSpawnedEnemyDefinitions.BySpawner.GetValueOrDefault(enemy, []))
            .Except(loaded).Distinct().Order()];
        return [.. loaded, .. spawned];
    }

    /// <summary>
    /// The room states that draw under graphics set <paramref name="set"/>: those that load it, and, for the session-start
    /// set, every state whose population is empty. The room load skips the set of a state with no enemies
    /// (<c>RoomEnemySystem</c>'s population load), so such a room's spawned objects draw under the session-start set,
    /// whatever set its state names (#1560).
    /// </summary>
    private static RoomStateSettings[] StatesOf(RoomEnemyGraphicsSetDefinition set) =>
        [.. RoomStateDefinitions.All.Where(state => RoomEnemyGraphicsSetLists.DrawnUnderIn(state) == set)];

    /// <summary>
    /// The encounters played under graphics set <paramref name="set"/> (#1538): each its members play, with what each of their
    /// looks copies, resolved against <paramref name="inputs"/>.
    /// </summary>
    private static IReadOnlyList<EncounterLookSteps> Encounters(EnemySheetInputs inputs, RoomEnemyGraphicsSetMembers set) =>
        [.. EncounterLookUploads.Of(set).Select(encounter => EncounterLookUploads.Steps(encounter, inputs))];

    /// <summary>A room graphics set as a room load leaves it.</summary>
    /// <param name="Layout">Where its members' tiles land, which gives each enemy's character index.</param>
    /// <param name="Characters">Its OBJ characters, as its room load and its encounters' looks leave them.</param>
    /// <param name="Colours">Its sprite colours and the row each enemy drawn under it draws in.</param>
    /// <param name="Shades">The shades its sheets are pre-coloured in.</param>
    /// <param name="Drawn">The enemies drawn under it: its members in record order, then the enemies its rooms place without it loading them.</param>
    internal sealed record LoadedGraphicsSet(GraphicsSetCharacterLayout Layout, SetCharacters Characters, LoadedSetColours Colours, EnemySheetShades Shades,
        IReadOnlyList<EnemyDefinitionId> Drawn);

    /// <summary>The OBJ character table as the set leaves it: the standard characters, then each member's tile upload.</summary>
    private static byte[] Table(EnemySheetInputs inputs, GraphicsSetCharacterLayout layout)
    {
        var table = new byte[SpriteCharacterTableLayout.TableByteCount];
        inputs.StandardObjects.Span.CopyTo(table);
        foreach (GraphicsSetMemberCharacters entry in layout.Members)
        {
            ReadOnlyMemory<byte> tiles = inputs.Enemies.TilesOf(entry.DefinitionId);
            if (tiles.Length != entry.TileByteCount)
                throw new InvalidDataException($"Enemy {entry.DefinitionId} has no installed tiles for its graphics-set upload.");
            int first = EnemyRegionTableByte + entry.StagingOffset;
            int shown = Math.Min(tiles.Length, table.Length - first);
            if (shown > 0)
                tiles.Span[..shown].CopyTo(table.AsSpan(first));
        }
        return table;
    }

    /// <summary>
    /// Each shown frame's pictures over the set's characters, every frame one whole picture: an extended frame's pieces
    /// are composed into it at their placements (#1513).
    /// </summary>
    private static IEnumerable<FramePictures> Frames(EnemyTileSourceArtwork enemies, EnemyDefinitionId enemy, IReadOnlyList<EnemyFrameId> shown,
        int tileBase, SetCharacters characters) =>
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row. A rotting corpse's
        // frames name where the corpse lies in them (#1539).
        Composed(enemies, shown).Select(frame => characters.Pictures(frame.Name, frame.Picture(tileBase),
            FrameDrawings.Of(frame.Frame), RottingCorpseCells.Shows(enemy, frame.Frame)));

    /// <summary>
    /// Each shown frame's composition by sheet frame name: an ordinary frame as one piece at the frame's origin, an
    /// extended frame as its pieces in drawing order. A frame no installed composition shows, and an extended frame with
    /// no sprite piece, draw nothing to compose.
    /// </summary>
    private static IEnumerable<ComposedFrame> Composed(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown) =>
        Composed(enemies.Spritemaps ?? throw new InvalidDataException("Enemy sheets require the installed enemy compositions."),
            enemies.ExtendedFrames ?? throw new InvalidDataException("Enemy sheets require the installed extended frames."), shown);

    /// <summary>Each shown frame's composition by sheet frame name, from the enemy compositions and extended frames.</summary>
    private static IEnumerable<ComposedFrame> Composed(EnemySpritemapCatalog spritemaps, EnemyExtendedFrameCatalog extended, IReadOnlyList<EnemyFrameId> shown)
    {
        foreach (EnemyFrameId frame in shown)
        {
            if (spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts))
                yield return new ComposedFrame(frame, EnemySheetDefinitions.FrameName(frame), (EnemyExtendedDrawComponent[])[new EnemyExtendedDrawComponent(Offset16<SpritePixels>.Zero, parts)]);
            else if (extended.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) && !components.IsEmpty)
                yield return new ComposedFrame(frame, EnemySheetDefinitions.FrameName(frame), components);
        }
    }

    /// <summary>A shown frame's composition, by the sheet frame name it takes.</summary>
    /// <param name="Frame">The enemy frame it shows.</param>
    /// <param name="Name">The sheet frame's name.</param>
    /// <param name="Pieces">Its pieces in drawing order, frontmost first, each placed from the frame's origin.</param>
    private readonly record struct ComposedFrame(EnemyFrameId Frame, string Name, ReadOnlyMemory<EnemyExtendedDrawComponent> Pieces)
    {
        /// <summary>
        /// The frame's one whole picture over characters from <paramref name="tileBase"/>: each piece's parts moved by
        /// the piece's placement, the earlier pieces' parts in front of the later ones'. A single piece at the origin is
        /// the piece's own frame.
        /// </summary>
        internal SpriteFrame Picture(int tileBase)
        {
            ReadOnlySpan<EnemyExtendedDrawComponent> pieces = Pieces.Span;
            if (pieces.Length == 1 && pieces[0].Offset == Offset16<SpritePixels>.Zero)
                return EnemySpriteFrames.Frame(pieces[0].Parts, tileBase, 0);
            var parts = new List<SpriteFramePart>();
            foreach (EnemyExtendedDrawComponent piece in pieces)
            {
                foreach (SpriteFramePart part in EnemySpriteFrames.Frame(piece.Parts, tileBase, 0).Parts)
                    parts.Add(part with { Offset = Offset16<ScreenPixels>.Of(part.Offset.X + piece.Offset.X, part.Offset.Y + piece.Offset.Y) });
            }
            return new SpriteFrame(Name, [.. parts]);
        }
    }
}
