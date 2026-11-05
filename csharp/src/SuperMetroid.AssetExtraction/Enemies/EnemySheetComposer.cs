using System.Security.Cryptography;
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
    /// Every graphics set's sheets, each distinct sheet once. Every sheet is laid out first, and sheets whose layouts
    /// share an identity paint alike, so each distinct layout is painted and encoded once, in parallel, each worker on
    /// its own reused canvas (#1490). The sheets are then named in set order: a layout that paints the same files as an
    /// earlier one takes its name, as composing and comparing every sheet in turn would.
    /// </summary>
    internal static EnemySheetComposition Compose(EnemySheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var layouts = new List<SpriteSheetLayout>();
        var layoutOf = new Dictionary<string, int>(StringComparer.Ordinal);
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
                    source.Variations, source.RotPictures, source.BlankPictures))],
                [.. laid.Sources.Spawned.Select((source, sheet) => new LaidSpawnedSheet(source.Plan, Laid(laid.Spawned[sheet], layouts, layoutOf),
                    source.Variations, source.RotPictures, source.BlankPictures))]));
        }
        var encoded = new EncodedSpriteSheet[layouts.Count];
        Parallel.For(0, layouts.Count, () => new SpriteSheetEncoder(), (index, _, encoder) =>
        {
            encoded[index] = encoder.Encode(layouts[index]);
            return encoder;
        }, encoder => encoder.Dispose());
        var names = new SheetNames(encoded);
        var sets = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var spawned = new Dictionary<string, Dictionary<string, SpawnedObjectSheetDocument>>(StringComparer.Ordinal);
        var variations = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>>(StringComparer.Ordinal);
        var spawnedVariations = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>>(StringComparer.Ordinal);
        var rotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        var spawnedRotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        var blankPictures = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.Ordinal);
        var spawnedBlankPictures = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.Ordinal);
        foreach (LaidGraphicsSet set in laidSets)
        {
            var members = new Dictionary<string, string>(StringComparer.Ordinal);
            var memberVariations = new Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>(StringComparer.Ordinal);
            var memberRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var memberBlank = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (LaidEnemySheet sheet in set.Enemies)
            {
                EnemyDefinitionId enemy = sheet.Enemy;
                string name = names.Of(sheet.Layout, enemy.ToString());
                // A set that lists an enemy twice loads it once into each record's row; both must show alike.
                if (members.TryGetValue(enemy.ToString(), out string? listed) && listed != name)
                    throw new InvalidDataException($"Enemy graphics set {set.Name} colours {enemy} two ways.");
                members[enemy.ToString()] = name;
                if (sheet.Variations.Count > 0)
                    memberVariations[enemy.ToString()] = Document(sheet.Variations);
                if (sheet.RotPictures.Count > 0)
                    memberRotPictures[enemy.ToString()] = new Dictionary<string, string>(sheet.RotPictures, StringComparer.Ordinal);
                if (sheet.BlankPictures.Count > 0)
                    memberBlank[enemy.ToString()] = [.. sheet.BlankPictures.Order(StringComparer.Ordinal)];
            }
            sets.Add(set.Name, members);
            if (memberVariations.Count > 0)
                variations.Add(set.Name, memberVariations);
            if (memberRotPictures.Count > 0)
                rotPictures.Add(set.Name, memberRotPictures);
            if (memberBlank.Count > 0)
                blankPictures.Add(set.Name, memberBlank);
            var bases = new Dictionary<string, SpawnedObjectSheetDocument>(StringComparer.Ordinal);
            var baseVariations = new Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>(StringComparer.Ordinal);
            var baseRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var baseBlank = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (LaidSpawnedSheet sheet in set.Spawned)
            {
                SpawnedObjectSheetPlan plan = sheet.Plan;
                string name = names.Of(sheet.Layout, $"Spawned-{SpawnedObjectOwnerNames.Name(plan.Owner)}");
                // Every owner the game names the sheet by finds it under its own name.
                foreach (string owner in plan.AlsoOwnedBy.Prepend(plan.Owner).Select(SpawnedObjectOwnerNames.Name))
                {
                    bases.Add(owner, new SpawnedObjectSheetDocument
                    {
                        Sheet = name,
                        Palette = plan.Palette,
                        Owner = SpawnedObjectOwnerNames.Name(plan.Owner),
                        Spawners = [.. plan.Spawners.Select(enemy => enemy.ToString())],
                        SharedSpawners = [.. plan.SharedSpawners.Select(enemy => enemy.ToString())],
                        Common = [.. plan.Common.Select(common => common.Spawner)],
                    });
                    if (sheet.Variations.Count > 0)
                        baseVariations[owner] = Document(sheet.Variations);
                    if (sheet.RotPictures.Count > 0)
                        baseRotPictures[owner] =
                            new Dictionary<string, string>(sheet.RotPictures, StringComparer.Ordinal);
                    if (sheet.BlankPictures.Count > 0)
                        baseBlank[owner] = [.. sheet.BlankPictures.Order(StringComparer.Ordinal)];
                }
            }
            spawned.Add(set.Name, bases);
            if (baseVariations.Count > 0)
                spawnedVariations.Add(set.Name, baseVariations);
            if (baseRotPictures.Count > 0)
                spawnedRotPictures.Add(set.Name, baseRotPictures);
            if (baseBlank.Count > 0)
                spawnedBlankPictures.Add(set.Name, baseBlank);
        }
        return new EnemySheetComposition(names.Sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetFiles.ManifestVersion, Sets = sets, Spawned = spawned, Variations = variations,
            SpawnedVariations = spawnedVariations, RotPictures = rotPictures, SpawnedRotPictures = spawnedRotPictures,
            BlankPictures = blankPictures, SpawnedBlankPictures = spawnedBlankPictures,
        });
    }

    /// <summary>How each frame follows the sequences that change it, as the manifest spells it: by frame, then sequence name.</summary>
    private static Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>> Document(IReadOnlyDictionary<string, FrameArtworkVariation[]> frames) =>
        frames.ToDictionary(frame => frame.Key, frame => frame.Value.ToDictionary(variation => variation.Sequence.ToString(),
            FrameArtworkVariationDocument.Of, StringComparer.Ordinal), StringComparer.Ordinal);

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
    /// <param name="Variations">How each frame encounter artwork sequences change follows them.</param>
    /// <param name="RotPictures">The corpse each frame drawn over a corpse's rows shows the rot of.</param>
    /// <param name="BlankPictures">The frames' pictures that show nothing.</param>
    private sealed record LaidEnemySheet(EnemyDefinitionId Enemy, int Layout, IReadOnlyDictionary<string, FrameArtworkVariation[]> Variations,
        IReadOnlyDictionary<string, string> RotPictures, IReadOnlySet<string> BlankPictures);

    /// <summary>One spawned-object sheet under a graphics set.</summary>
    /// <param name="Plan">The tile base, palette and spawners of the objects it draws.</param>
    /// <param name="Layout">The index of the distinct layout its sheet paints.</param>
    /// <param name="Variations">How each frame encounter artwork sequences change follows them.</param>
    /// <param name="RotPictures">The corpse each frame drawn over a corpse's rows shows the rot of.</param>
    /// <param name="BlankPictures">The frames' pictures that show nothing.</param>
    private sealed record LaidSpawnedSheet(SpawnedObjectSheetPlan Plan, int Layout, IReadOnlyDictionary<string, FrameArtworkVariation[]> Variations,
        IReadOnlyDictionary<string, string> RotPictures, IReadOnlySet<string> BlankPictures);

    /// <summary>
    /// Names the distinct sheets in the order they are first asked for: a layout whose files are the same as an earlier
    /// named sheet's takes its name; otherwise its owner's next ordinal, an enemy's sheets named for the enemy and a
    /// spawned-object sheet for its tile base.
    /// </summary>
    private sealed class SheetNames
    {
        /// <summary>Starts naming the distinct layouts' sheets.</summary>
        /// <param name="encoded">Each distinct layout's encoded files.</param>
        internal SheetNames(EncodedSpriteSheet[] encoded)
        {
            this.encoded = encoded;
            names = new string?[encoded.Length];
        }

        /// <summary>Each distinct layout's encoded files.</summary>
        private readonly EncodedSpriteSheet[] encoded;

        /// <summary>The name each layout's sheet was given, or null before it is asked for.</summary>
        private readonly string?[] names;

        /// <summary>The name of the sheet with each fingerprint.</summary>
        private readonly Dictionary<string, string> distinct = new(StringComparer.Ordinal);

        /// <summary>How many distinct sheets each owner has been given.</summary>
        private readonly Dictionary<string, int> ordinals = new(StringComparer.Ordinal);

        /// <summary>Each distinct sheet by installed name, in the order they were named.</summary>
        internal Dictionary<string, EncodedSpriteSheet> Sheets { get; } = new(StringComparer.Ordinal);

        /// <summary>The installed name of the sheet layout <paramref name="layout"/> paints, owned by <paramref name="owner"/> when it is new.</summary>
        internal string Of(int layout, string owner)
        {
            if (names[layout] is string known)
                return known;
            string key = Fingerprint(encoded[layout]);
            if (!distinct.TryGetValue(key, out string? name))
            {
                int ordinal = ordinals.GetValueOrDefault(owner);
                ordinals[owner] = ordinal + 1;
                name = $"{owner}.{ordinal}";
                distinct.Add(key, name);
                Sheets.Add(name, encoded[layout]);
            }
            names[layout] = name;
            return name;
        }

        /// <summary>A sheet's content hash, which sheets with the same files share.</summary>
        private static string Fingerprint(EncodedSpriteSheet sheet) =>
            Convert.ToHexString(SHA256.HashData(sheet.Image)) + Convert.ToHexString(SHA256.HashData(sheet.Palette))
            + Convert.ToHexString(SHA256.HashData(sheet.Frames));
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
        IReadOnlyList<CommonSpawnedObjects> common = CommonSpawnersOf(set);
        return new GraphicsSetSheetSources([.. EnemySources(inputs, loaded)],
            [.. SpawnedObjectSheetComposer.Plans(loaded.Layout, loaded.Drawn, loaded.Colours.RowOf, common).Select(plan =>
                SpawnedObjectSheetComposer.Sources(inputs.Enemies, plan, loaded.Characters, loaded.Shades, loaded.Colours.RowOf))]);
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
            var variations = new Dictionary<string, FrameArtworkVariation[]>(StringComparer.Ordinal);
            var rots = new Dictionary<string, string>(StringComparer.Ordinal);
            var blank = new HashSet<string>(StringComparer.Ordinal);
            var frames = new List<SheetFrameSource>();
            FramePictures[] pictures = [.. Frames(inputs.Enemies, shown, checked((ushort)tileBase), loaded.Characters, CorpseRotSimulation.Rots(enemy))];
            // An enemy whose encounter sets its colours is coloured in those its frames first show in, and a frame family
            // always drawn in one colouring in that colouring (#1513).
            RgbColor[] colours = loaded.Shades.WithFrameColourings(loaded.Shades.FirstShownColours([enemy]), pictures, paletteBits,
                SharedSpawnerFrames.None);
            foreach (FramePictures framePictures in pictures)
            {
                frames.AddRange(framePictures.Sources);
                if (framePictures.Variations.Length > 0)
                    variations.Add(framePictures.Name, framePictures.Variations);
                if (framePictures.RotCorpse is { } corpse)
                    rots.Add(framePictures.Name, corpse);
                blank.UnionWith(framePictures.Blank);
            }
            yield return new EnemySheetSource(enemy, new SpriteSheetSources(frames, colours,
                ObjectSelectionCodec.GameplaySizes, paletteBits,
                Shades: [.. loaded.Shades.OwnRow(enemy, paletteBits), .. fixedShades],
                RowShades: [.. loaded.Shades.Shared(), .. rowShades]), variations, rots, blank);
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
        GraphicsSetCharacterLayout layout = GraphicsSetCharacterLayout.Of(set);
        LoadedSetColours colours = LoadedSetColours.Of(inputs, set);
        EnemyDefinitionId[] drawn = DrawnUnder(set, layout);
        IReadOnlyList<CorpseRotSimulation> corpses = CorpseRotSimulation.Of(inputs.Enemies, inputs.Bosses, drawn);
        return new LoadedGraphicsSet(layout, new SetCharacters(Table(inputs, layout), Sequences(inputs, set, corpses),
            [.. corpses.Where(corpse => corpse.Kind is not null)]), colours, new EnemySheetShades(inputs, colours), drawn);
    }

    /// <summary>The common spawners whose objects graphics set <paramref name="set"/>'s shared sheet holds: those its rooms run (#1513).</summary>
    /// <exception cref="InvalidDataException">A common spawner has no presence.</exception>
    internal static IReadOnlyList<CommonSpawnedObjects> CommonSpawnersOf(RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(set);
        return CommonSpawners.RunningIn(StatesOf(set), DrawnUnder(set, GraphicsSetCharacterLayout.Of(set)));
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

    /// <summary>The room states that load graphics set <paramref name="set"/>.</summary>
    private static RoomStateSettings[] StatesOf(RoomEnemyGraphicsSetDefinition set) =>
        [.. RoomStateDefinitions.All.Where(state => state.EnemyGraphicsSet.Name == set.Name)];

    /// <summary>
    /// The artwork sequences played under graphics set <paramref name="set"/> (#1513): each its members' encounters play,
    /// resolved against <paramref name="inputs"/>, then Mother Brain's corpse rot when her corpse rots among
    /// <paramref name="corpses"/>, whose steps are its simulation's uploads.
    /// </summary>
    internal static IReadOnlyList<EncounterArtworkSteps> Sequences(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set,
        IEnumerable<CorpseRotSimulation> corpses) =>
        [
            .. EncounterArtworkUploads.Of(set).Select(sequence => new EncounterArtworkSteps(sequence, EncounterArtworkUploads.Steps(sequence, inputs))),
            .. Ensure.NotNull(corpses).Where(corpse => corpse.Kind is null).Select(corpse => new EncounterArtworkSteps(EncounterArtworkSequence.MotherBrainCorpseRot,
                corpse.StepUploads)),
        ];

    /// <summary>A room graphics set as a room load leaves it.</summary>
    /// <param name="Layout">Where its members' tiles land, which gives each enemy's character index.</param>
    /// <param name="Characters">Its OBJ characters, as its room load and its encounters' uploads leave them.</param>
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

    /// <summary>The names of the frames an enemy's sheets hold, in composition order.</summary>
    internal static IEnumerable<string> FrameNames(EnemyTileSourceArtwork enemies, EnemyDefinitionId enemy) =>
        EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown)
            ? Composed(enemies, shown).Select(frame => frame.Name) : [];

    /// <summary>
    /// Each shown frame's pictures over the set's characters, every frame one whole picture: an extended frame's pieces
    /// are composed into it at their placements (#1513).
    /// </summary>
    private static IEnumerable<FramePictures> Frames(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown,
        int tileBase, SetCharacters characters, bool showsRot) =>
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row.
        Composed(enemies, shown).Select(frame => characters.Pictures(frame.Name, frame.Picture(tileBase),
            FrameDrawings.Of(frame.Frame), showsRot));

    /// <summary>
    /// Each shown frame's composition by sheet frame name: an ordinary frame as one piece at the frame's origin, an
    /// extended frame as its pieces in drawing order. A frame no installed composition shows, and an extended frame with
    /// no sprite piece, draw nothing to compose.
    /// </summary>
    private static IEnumerable<ComposedFrame> Composed(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown)
    {
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps ?? throw new InvalidDataException("Enemy sheets require the installed enemy compositions.");
        EnemyExtendedFrameCatalog extended = enemies.ExtendedFrames ?? throw new InvalidDataException("Enemy sheets require the installed extended frames.");
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
