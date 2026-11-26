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
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Composes the pre-coloured enemy sheets (#1392). Each room graphics set is loaded as a room load leaves it: the
/// standard OBJ characters with each member's tiles uploaded after them, and each member's palette loaded into its
/// row. Every member's sheet holds the frames its behaviour shows, composed at its tile index and coloured in its row
/// as the set leaves it, so a row a later member overwrites shows that member's colours, as on the cartridge. Each
/// sheet is also pre-coloured in the shades its enemy shows (#1512): a hit or freeze flash and the enemy projectiles'
/// colours as the cartridge ORs their row into each part's own, and its own effects.
/// </summary>
internal static partial class EnemySheetComposer
{
    /// <summary>The enemies in the order a sheet appends those its set does not load (#2567), each listed once.</summary>
    private static readonly IReadOnlyList<EnemyDefinitionId> AppendedOrder = Ensure.EveryMemberOnce(EnemyDefinitionIdKeys.ListedOrder);

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
        InstallationWork.For(0, drawnSets.Length, index =>
        {
            RoomEnemyGraphicsSetDefinition set = drawnSets[index];
            GraphicsSetSheetSources setSources = Sources(inputs, set);
            setLayouts[index] = new SetLayouts(set,
                [.. setSources.Enemies.Select(source => new LaidOutEnemySheet(source,
                    new SpriteSheetLayout(source.Sources, EnemySheetShades.SheetRow, ActorDepth.BehindRoom)))],
                [.. setSources.Spawned.Select(source => new LaidOutSpawnedSheet(source,
                    new SpriteSheetLayout(source.Sources, EnemySheetShades.SheetRow, ActorDepth.BehindRoom)))]);
        });
        foreach (SetLayouts laid in setLayouts)
        {
            laidSets.Add(new LaidGraphicsSet(laid.Set,
                [.. laid.Enemies.Select(enemy => new LaidEnemySheet(enemy.Source.Enemy, Laid(enemy.Layout, layouts, layoutOf),
                    enemy.Source.LookChanges, enemy.Source.CorpsePictures))],
                [.. laid.Spawned.Select(spawned => new LaidSpawnedSheet(spawned.Source.Plan, Laid(spawned.Layout, spawnedLayouts, spawnedLayoutOf),
                    spawned.Source.LookChanges, spawned.Source.CorpsePictures, spawned.Source.Empty))]));
        }
        var installed = new InstalledSheet[layouts.Count];
        InstallationWork.For(0, layouts.Count, () => new InstalledSheetPainter(), (index, _, painter) =>
        {
            installed[index] = painter.Install(layouts[index]);
            return painter;
        }, _ => { });
        SharedSpawnedSheets shared = SpawnedObjectSheetSharing.Share(spawnedLayouts);
        var names = new SheetNames();
        var variants = new VariantNames<EnemyVariantDocument>(document => JsonSerializer.Serialize(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Enemies.EnemyVariantDocument>()));
        var spawnedVariants = new VariantNames<SpawnedObjectSheetDocument>(document => JsonSerializer.Serialize(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Enemies.SpawnedObjectSheetDocument>()));
        var setArtwork = new Dictionary<RoomEnemyGraphicsSetDefinition, PopulationArtworkDocument>();
        // The session start's variants are named first, as each owner's standard variant.
        foreach (LaidGraphicsSet set in laidSets.OrderBy(set => set.Set == RoomEnemyGraphicsSetDefinitions.SessionStart ? 0 : 1))
        {
            bool standard = set.Set == RoomEnemyGraphicsSetDefinitions.SessionStart;
            var enemies = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (LaidEnemySheet sheet in set.Enemies)
            {
                string enemy = EnemyDefinitionIdKeys.Installed.Of(sheet.Enemy);
                string variant = variants.Of(enemy, new EnemyVariantDocument
                {
                    Enemy = enemy,
                    Sheet = names.Of(installed[sheet.Layout], enemy),
                    LookChanges = sheet.LookChanges.Count > 0 ? Document(sheet.LookChanges) : null,
                    CorpsePictures = sheet.CorpsePictures.Count > 0 ? Document(sheet.CorpsePictures) : null,
                }, standard);
                // A set that lists an enemy twice loads it once into each record's row; both must show alike.
                if (enemies.TryGetValue(enemy, out string? listed) && listed != variant)
                    throw new InvalidDataException($"Enemy graphics set {set.Set} colours {sheet.Enemy} two ways.");
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
                foreach ((string picture, InstalledSheet commonSheet) in share.Common.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    string commonName = names.Of(commonSheet, "Spawned-Common");
                    if (!commonPictures.TryGetValue(commonName, out List<string>? held))
                        commonPictures.Add(commonName, held = []);
                    held.Add(picture);
                }
                string variant = spawnedVariants.Of(SpawnedObjectOwnerNames.Name(plan.Owner), new SpawnedObjectSheetDocument
                {
                    Sheet = own,
                    CommonPictures = commonPictures,
                    Owner = SpawnedObjectOwnerNames.Name(plan.Owner),
                    Spawners = [.. plan.Spawners.Select(EnemyDefinitionIdKeys.Installed.Of)],
                    SharedSpawners = [.. plan.SharedSpawners.Select(EnemyDefinitionIdKeys.Installed.Of)],
                    Common = [.. plan.Common.Select(common => CommonSpawnerKeys.Installed.Of(common.Spawner))],
                    Empty = [.. sheet.Empty.Order(StringComparer.Ordinal)],
                    LookChanges = sheet.LookChanges.Count > 0 ? Document(sheet.LookChanges) : null,
                    CorpsePictures = sheet.CorpsePictures.Count > 0 ? Document(sheet.CorpsePictures) : null,
                }, standard);
                // Every owner the game names the sheet by finds it under its own name.
                foreach (string owner in plan.AlsoOwnedBy.Prepend(plan.Owner).Select(SpawnedObjectOwnerNames.Name))
                    owners.Add(owner, variant);
            }
            setArtwork.Add(set.Set, new PopulationArtworkDocument { Enemies = enemies, Spawned = owners });
        }
        return new EnemySheetComposition(names.Sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetFiles.ManifestVersion,
            Variants = variants.Documents,
            SpawnedVariants = spawnedVariants.Documents,
            Standard = setArtwork[RoomEnemyGraphicsSetDefinitions.SessionStart],
            Populations = RoomEnemyGraphicsSetLists.PopulationSets.ToDictionary(population => population.Key,
                population => setArtwork[population.Value], StringComparer.Ordinal),
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
        frames.ToDictionary(frame => frame.Key, frame => frame.Value.OrderBy(change => change.Encounter).ToDictionary(change => EncounterArtworkKeys.Installed.Of(change.Encounter),
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

    /// <summary>A graphics set's sheets, each source with its layout, in the order the set's sources list them.</summary>
    /// <param name="Set">The set.</param>
    /// <param name="Enemies">Each enemy sheet's source and layout.</param>
    /// <param name="Spawned">Each spawned-object sheet's source and layout.</param>
    private sealed partial record SetLayouts(RoomEnemyGraphicsSetDefinition Set, IReadOnlyList<LaidOutEnemySheet> Enemies,
        IReadOnlyList<LaidOutSpawnedSheet> Spawned);

    /// <summary>An enemy sheet's source and the layout of its pictures.</summary>
    /// <param name="Source">The sheet's source.</param>
    /// <param name="Layout">The layout of the source's pictures.</param>
    private sealed partial record LaidOutEnemySheet(EnemySheetSource Source, SpriteSheetLayout Layout);

    /// <summary>A spawned-object sheet's source and the layout of its pictures.</summary>
    /// <param name="Source">The sheet's source.</param>
    /// <param name="Layout">The layout of the source's pictures.</param>
    private sealed partial record LaidOutSpawnedSheet(SpawnedObjectSheetSource Source, SpriteSheetLayout Layout);

    /// <summary>A graphics set's sheets, each by the distinct layout it paints.</summary>
    /// <param name="Set">The set.</param>
    /// <param name="Enemies">Each enemy's sheet, in the order the set's sources list them.</param>
    /// <param name="Spawned">Each spawned-object sheet, in the order the set's sources list them.</param>
    private sealed partial record LaidGraphicsSet(RoomEnemyGraphicsSetDefinition Set, IReadOnlyList<LaidEnemySheet> Enemies, IReadOnlyList<LaidSpawnedSheet> Spawned);

    /// <summary>One enemy's sheet under a graphics set.</summary>
    /// <param name="Enemy">The enemy.</param>
    /// <param name="Layout">The index of the distinct layout its sheet paints.</param>
    /// <param name="LookChanges">How encounters' looks change each frame they change.</param>
    /// <param name="CorpsePictures">Where a rotting corpse lies in each frame that shows one.</param>
    private sealed partial record LaidEnemySheet(EnemyDefinitionId Enemy, int Layout, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges,
        IReadOnlyDictionary<string, CorpsePicturePlacement> CorpsePictures);

    /// <summary>
    /// The part of an installed sheet's or variant's name that says what it holds: the first twelve hexadecimal digits of the
    /// digest of <paramref name="content"/>, how its content is spelled.
    /// </summary>
    private static string ContentName(string content) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(content)))[..12];

    /// <summary>One spawned-object sheet under a graphics set.</summary>
    /// <param name="Plan">The tile base, palette and spawners of the objects it draws.</param>
    /// <param name="Layout">The index of the distinct spawned-object layout its sheet paints.</param>
    /// <param name="LookChanges">How encounters' looks change each frame they change.</param>
    /// <param name="CorpsePictures">Where a rotting corpse lies in each frame that shows one.</param>
    /// <param name="Empty">The frames' finished pictures that show nothing.</param>
    private sealed partial record LaidSpawnedSheet(SpawnedObjectSheetPlan Plan, int Layout, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges,
        IReadOnlyDictionary<string, CorpsePicturePlacement> CorpsePictures, IReadOnlySet<string> Empty);

    /// <summary>
    /// Names the distinct variants of each owner (#1599, #2556): a variant the same as an earlier one takes its name;
    /// otherwise the session start's is its owner's standard variant, and any other is named for how the manifest spells
    /// it, its owner's name followed by a digest of that spelling, so no name depends on the order variants are asked for.
    /// </summary>
    /// <typeparam name="TDocument">How the manifest spells a variant.</typeparam>
    /// <param name="spelling">How the manifest spells a document; two variants are the same exactly when they spell alike.</param>
    private sealed partial class VariantNames<TDocument>(Func<TDocument, string> spelling)
    {
        /// <summary>The name of the variant with each identity: its owner and how the manifest spells it.</summary>
        private readonly Dictionary<string, string> named = new(StringComparer.Ordinal);

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
            string name = standard ? $"{owner}.Standard" : $"{owner}.{ContentName(identity)}";
            if (!Documents.TryAdd(name, document))
                throw new InvalidOperationException($"Two variants of {owner} are both named {name}.");
            named.Add(identity, name);
            return name;
        }
    }

    /// <summary>
    /// Names the distinct sheets: a sheet whose installed manifest, pixels and maps are the same as an earlier named sheet's
    /// takes its name; otherwise its owner's name followed by a digest of its content (#2556), an enemy's sheets named for
    /// the enemy, a spawned-object sheet's own pictures for its owner and a common sheet of spawned-object pictures as common
    /// (#1513). This is what makes variants the player cannot tell apart one sheet (#1643), for any enemy: graphics sets that
    /// load an enemy into different palette rows compose it different sheets, but the installation holds only its pictures,
    /// frame list and the looks its draws show, so where those are the same the sheets are installed once, every set's
    /// variant names that one sheet, and the variants, then spelled alike, are one variant every placement draws in.
    /// </summary>
    private sealed partial class SheetNames
    {
        /// <summary>The name each converted sheet was given.</summary>
        private readonly Dictionary<InstalledSheet, string> names = new(ReferenceEqualityComparer.Instance);

        /// <summary>The name of the sheet with each fingerprint.</summary>
        private readonly Dictionary<string, string> distinct = new(StringComparer.Ordinal);

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
                name = $"{owner}.{ContentName(key)}";
                distinct.Add(key, name);
                if (!Sheets.TryAdd(name, sheet))
                    throw new InvalidOperationException($"Two sheets of {owner} are both named {name}.");
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
        LoadedGraphicsSet loaded = LoadedGraphicsSet.Of(inputs, set);
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
            // The sheet is coloured in the row the set loads, or the enemy's initialization selects, its colours in.
            int paletteBits = loaded.Colours.RowOf(enemy);
            (IReadOnlyList<SheetShade> fixedShades, IReadOnlyList<SheetRowShade> rowShades) = loaded.Shades.Fixed(enemy);
            var changes = new Dictionary<string, IReadOnlyList<FrameLookChange>>(StringComparer.Ordinal);
            var corpses = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
            var blank = new HashSet<string>(StringComparer.Ordinal);
            var frames = new List<SheetFrameSource>();
            FramePictures[] pictures = [.. Frames(inputs.Enemies, enemy, shown, layout, loaded.Characters)];
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
    /// The placements the rooms drawing under graphics set <paramref name="set"/> place each enemy with, by enemy: an enemy
    /// whose initialization chooses its colours by its placement first shows in the colours its placements choose (#1642).
    /// </summary>
    private static Dictionary<EnemyDefinitionId, IReadOnlySet<EnemySpawnSettings>> Placements(RoomEnemyGraphicsSetDefinition set)
    {
        var placements = new Dictionary<EnemyDefinitionId, HashSet<EnemySpawnSettings>>();
        foreach (RoomStateSettings state in StatesOf(set))
        {
            foreach (RoomEnemyPlacement record in state.EnemyPopulation.Placements.Span)
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
    /// (#1513), its members as <paramref name="members"/> holds them by set.
    /// </summary>
    /// <exception cref="InvalidDataException">A common spawner has no presence, or the set's members are not catalogued.</exception>
    internal static IReadOnlyList<CommonSpawnedObjects> CommonSpawnersOf(RoomEnemyGraphicsSetDefinition set,
        IReadOnlyDictionary<RoomEnemyGraphicsSetDefinition, RoomEnemyGraphicsSetMembers> members)
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
        // A record loading artwork for no enemy draws nothing (#1686); only the set's enemies are drawn under it.
        List<EnemyDefinitionId> members = [.. layout.Members.Select(member => member.DefinitionId).OfType<EnemyDefinitionId>()];
        IEnumerable<EnemyDefinitionId> placed = InAppendedOrder(StatesOf(set)
            .SelectMany(state => state.EnemyPopulation.Placements.Span.SelectToArray(record => record.DefinitionId))
            .Except(members));
        EnemyDefinitionId[] loaded = [.. members, .. placed];
        // An encounter's own spawns, such as Mother Brain's tubes and the Baby, draw under the set its spawner loads.
        EnemyDefinitionId[] spawned = [.. InAppendedOrder(loaded.SelectMany(enemy => EncounterSpawnedEnemyDefinitions.BySpawner.GetValueOrDefault(enemy, []))
            .Except(loaded))];
        return [.. loaded, .. spawned];
    }

    /// <summary>Each of <paramref name="enemies"/> once, in <see cref="AppendedOrder"/>.</summary>
    private static IEnumerable<EnemyDefinitionId> InAppendedOrder(IEnumerable<EnemyDefinitionId> enemies)
    {
        HashSet<EnemyDefinitionId> appended = [.. enemies];
        return AppendedOrder.Where(appended.Contains);
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
    internal sealed partial record LoadedGraphicsSet(GraphicsSetCharacterLayout Layout, SetCharacters Characters, LoadedSetColours Colours, EnemySheetShades Shades,
        IReadOnlyList<EnemyDefinitionId> Drawn)
    {
        /// <summary>
        /// Graphics set <paramref name="set"/> as a room load leaves it, from <paramref name="inputs"/>: its enemy graphics,
        /// its OBJ page, its sprite colours, and the enemies drawn under it, its members in record order, then each enemy its
        /// rooms place without the set loading it, then the enemies those enemies' encounters spawn.
        /// </summary>
        internal static LoadedGraphicsSet Of(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
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
    }

    /// <summary>The OBJ character table as the set leaves it: the standard characters, then each member's tile upload.</summary>
    private static byte[] Table(EnemySheetInputs inputs, GraphicsSetCharacterLayout layout)
    {
        var table = new byte[SpriteCharacterTableLayout.TableByteCount];
        inputs.StandardObjects.Span.CopyTo(table);
        foreach (GraphicsSetMemberCharacters entry in layout.Members)
        {
            ReadOnlyMemory<byte> tiles = entry.Member.TilesIn(inputs.Enemies);
            if (tiles.Length != entry.TileByteCount)
                throw new InvalidDataException($"Graphics-set record {entry.Member.Name} has no installed tiles for its upload.");
            int first = EnemyRegionTableByte + entry.StagingOffset;
            int shown = Math.Min(tiles.Length, table.Length - first);
            if (shown > 0)
                tiles.Span[..shown].CopyTo(table.AsSpan(first));
        }
        return table;
    }

    /// <summary>
    /// Each shown frame's pictures over the set's characters, every frame one whole picture: an extended frame's pieces
    /// are composed into it at their placements (#1513). Each frame counts from the characters <paramref name="layout"/>
    /// gives <paramref name="enemy"/> for it: its own, or those of the artwork it draws the frame from (#2327).
    /// </summary>
    private static IEnumerable<FramePictures> Frames(EnemyTileSourceArtwork enemies, EnemyDefinitionId enemy, IReadOnlyList<EnemyFrameId> shown,
        GraphicsSetCharacterLayout layout, SetCharacters characters) =>
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row. A rotting corpse's
        // frames name where the corpse lies in them (#1539).
        Composed(enemies, shown).SelectMany(frame => WithSecondPictures(enemy, frame)).Select(frame => characters.Pictures(frame.Name,
            frame.Picture(layout.TileBaseOf(enemy, frame.Frame)), FrameDrawings.Of(frame.Frame), RottingCorpseCells.Shows(enemy, frame.Frame)));

    /// <summary>
    /// <paramref name="frame"/>, or for Mother Brain's body its picture without her far leg and her far leg as a picture of its
    /// own (#2633, #2634): the game draws her far leg behind the room's front tiles and her body, her near leg in front of
    /// them, so each is a picture at its own depth. The far leg is every piece the game draws behind the room's front tiles.
    /// </summary>
    /// <exception cref="InvalidDataException">One of her body frames has no far leg, or nothing but one.</exception>
    private static IEnumerable<ComposedFrame> WithSecondPictures(EnemyDefinitionId enemy, ComposedFrame frame)
    {
        if (enemy != EnemyDefinitionId.MotherBrainBody)
        {
            yield return frame;
            yield break;
        }
        EnemyExtendedDrawComponent[] near = PiecesWhere(frame.Pieces, part => part.Depth != ActorDepth.BehindFrontParts);
        EnemyExtendedDrawComponent[] far = PiecesWhere(frame.Pieces, part => part.Depth == ActorDepth.BehindFrontParts);
        if (near.Length == 0 || far.Length == 0)
            throw new InvalidDataException($"Mother Brain's body frame {frame.Frame} does not show both her near and her far leg.");
        yield return frame with { Pieces = near };
        yield return frame with { Name = MotherBrainBodyVisualDefinitions.SecondPictureName(frame.Frame), Pieces = far };
    }

    /// <summary>The pieces of <paramref name="pieces"/> with only their parts <paramref name="kept"/> keeps; a piece left with none is dropped.</summary>
    private static EnemyExtendedDrawComponent[] PiecesWhere(ReadOnlyMemory<EnemyExtendedDrawComponent> pieces, Func<SpritePart, bool> kept) =>
    [
        .. pieces.ToArray()
            .Select(piece => piece with { Parts = EnemySpritemapParts.FromOwnedArray([.. piece.Parts.Where(kept)]) })
            .Where(piece => piece.Parts.Count > 0),
    ];

    /// <summary>
    /// Each shown frame's composition by sheet frame name: an ordinary frame as one piece at the frame's origin, an
    /// extended frame as its pieces in drawing order. A frame no installed composition shows, and an extended frame with
    /// no sprite piece, draw nothing to compose.
    /// </summary>
    private static IEnumerable<ComposedFrame> Composed(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown) =>
        Composed(enemies.Spritemaps,
            enemies.ExtendedFrames, shown);

    /// <summary>Each shown frame's composition by sheet frame name, from the enemy compositions and extended frames.</summary>
    private static IEnumerable<ComposedFrame> Composed(EnemySpritemapCatalog spritemaps, EnemyExtendedFrameCatalog extended, IReadOnlyList<EnemyFrameId> shown)
    {
        foreach (EnemyFrameId frame in shown)
        {
            if (spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts))
                yield return new ComposedFrame(frame, EnemySheetDefinitions.FrameName(frame), (EnemyExtendedDrawComponent[])[new EnemyExtendedDrawComponent(Offset16<ScreenPixels>.Zero, parts)]);
            else if (extended.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) && !components.IsEmpty)
                yield return new ComposedFrame(frame, EnemySheetDefinitions.FrameName(frame), components);
        }
    }

    /// <summary>A shown frame's composition, by the sheet frame name it takes.</summary>
    /// <param name="Frame">The enemy frame it shows.</param>
    /// <param name="Name">The sheet frame's name.</param>
    /// <param name="Pieces">Its pieces in drawing order, frontmost first, each placed from the frame's origin.</param>
    private readonly partial record struct ComposedFrame(EnemyFrameId Frame, string Name, ReadOnlyMemory<EnemyExtendedDrawComponent> Pieces)
    {
        /// <summary>
        /// The frame's one whole picture over characters from <paramref name="tileBase"/>: each piece's parts moved by
        /// the piece's placement, the earlier pieces' parts in front of the later ones'. A single piece at the origin is
        /// the piece's own frame.
        /// </summary>
        internal SpriteFrame Picture(int tileBase)
        {
            ReadOnlySpan<EnemyExtendedDrawComponent> pieces = Pieces.Span;
            if (pieces.Length == 1 && pieces[0].Offset == Offset16<ScreenPixels>.Zero)
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
