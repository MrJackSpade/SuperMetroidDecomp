using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.RoomStates;
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
                    source.Pictures, source.RotPictures))],
                [.. laid.Sources.Spawned.Select((source, sheet) => new LaidSpawnedSheet(source.Plan, Laid(laid.Spawned[sheet], layouts, layoutOf),
                    source.Pictures, source.RotPictures))]));
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
        var pictures = new Dictionary<string, Dictionary<string, Dictionary<string, CharacterRange[][]>>>(StringComparer.Ordinal);
        var spawnedPictures = new Dictionary<string, Dictionary<string, Dictionary<string, CharacterRange[][]>>>(StringComparer.Ordinal);
        var rotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        var spawnedRotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        foreach (LaidGraphicsSet set in laidSets)
        {
            var members = new Dictionary<string, string>(StringComparer.Ordinal);
            var memberPictures = new Dictionary<string, Dictionary<string, CharacterRange[][]>>(StringComparer.Ordinal);
            var memberRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (LaidEnemySheet sheet in set.Enemies)
            {
                EnemyDefinitionId enemy = sheet.Enemy;
                string name = names.Of(sheet.Layout, enemy.ToString());
                // A set that lists an enemy twice loads it once into each record's row; both must show alike.
                if (members.TryGetValue(enemy.ToString(), out string? listed) && listed != name)
                    throw new InvalidDataException($"Enemy graphics set {set.Name} colours {enemy} two ways.");
                members[enemy.ToString()] = name;
                if (sheet.Pictures.Count > 0)
                    memberPictures[enemy.ToString()] = new Dictionary<string, CharacterRange[][]>(sheet.Pictures, StringComparer.Ordinal);
                if (sheet.RotPictures.Count > 0)
                    memberRotPictures[enemy.ToString()] = new Dictionary<string, string>(sheet.RotPictures, StringComparer.Ordinal);
            }
            sets.Add(set.Name, members);
            if (memberPictures.Count > 0)
                pictures.Add(set.Name, memberPictures);
            if (memberRotPictures.Count > 0)
                rotPictures.Add(set.Name, memberRotPictures);
            var bases = new Dictionary<string, SpawnedObjectSheetDocument>(StringComparer.Ordinal);
            var basePictures = new Dictionary<string, Dictionary<string, CharacterRange[][]>>(StringComparer.Ordinal);
            var baseRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (LaidSpawnedSheet sheet in set.Spawned)
            {
                SpawnedObjectSheetPlan plan = sheet.Plan;
                string name = names.Of(sheet.Layout, $"Spawned-{plan.TileBase:X3}");
                bases.Add(plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), new SpawnedObjectSheetDocument
                {
                    Sheet = name,
                    Palette = plan.Palette,
                    Spawners = [.. plan.Spawners.Select(enemy => enemy.ToString())],
                    Anywhere = plan.Anywhere,
                });
                if (sheet.Pictures.Count > 0)
                    basePictures[plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                        new Dictionary<string, CharacterRange[][]>(sheet.Pictures, StringComparer.Ordinal);
                if (sheet.RotPictures.Count > 0)
                    baseRotPictures[plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                        new Dictionary<string, string>(sheet.RotPictures, StringComparer.Ordinal);
            }
            spawned.Add(set.Name, bases);
            if (basePictures.Count > 0)
                spawnedPictures.Add(set.Name, basePictures);
            if (baseRotPictures.Count > 0)
                spawnedRotPictures.Add(set.Name, baseRotPictures);
        }
        return new EnemySheetComposition(names.Sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetFiles.ManifestVersion, Sets = sets, Spawned = spawned, Pictures = pictures,
            SpawnedPictures = spawnedPictures, RotPictures = rotPictures, SpawnedRotPictures = spawnedRotPictures,
        });
    }

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
    /// <param name="Pictures">The layers each frame drawn over uploaded regions draws as.</param>
    /// <param name="RotPictures">The corpse each frame drawn over a corpse's rows shows the rot of.</param>
    private sealed record LaidEnemySheet(EnemyDefinitionId Enemy, int Layout, IReadOnlyDictionary<string, CharacterRange[][]> Pictures,
        IReadOnlyDictionary<string, string> RotPictures);

    /// <summary>One spawned-object sheet under a graphics set.</summary>
    /// <param name="Plan">The tile base, palette and spawners of the objects it draws.</param>
    /// <param name="Layout">The index of the distinct layout its sheet paints.</param>
    /// <param name="Pictures">The layers each frame drawn over uploaded regions draws as.</param>
    /// <param name="RotPictures">The corpse each frame drawn over a corpse's rows shows the rot of.</param>
    private sealed record LaidSpawnedSheet(SpawnedObjectSheetPlan Plan, int Layout, IReadOnlyDictionary<string, CharacterRange[][]> Pictures,
        IReadOnlyDictionary<string, string> RotPictures);

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
        return new GraphicsSetSheetSources([.. EnemySources(inputs, loaded)],
            [.. SpawnedObjectSheetComposer.Plans(loaded.Graphics, loaded.Drawn, loaded.Colours.RowOf).Select(plan =>
                SpawnedObjectSheetComposer.Sources(inputs.Enemies, plan, loaded.Characters, loaded.Colours.SpriteColours, loaded.Shades,
                    loaded.Colours.RowOf))]);
    }

    /// <summary>What each enemy drawn under <paramref name="loaded"/> composes.</summary>
    private static IEnumerable<EnemySheetSource> EnemySources(EnemySheetInputs inputs, LoadedGraphicsSet loaded)
    {
        EnemyGraphicsSet graphics = loaded.Graphics;
        RgbColor[] colours = loaded.Colours.SpriteColours;
        foreach (EnemyDefinitionId enemy in loaded.Drawn)
        {
            if (!EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown))
                continue;
            SpriteGraphics indexes = graphics.DrawIndexes(enemy);
            // The sheet is coloured in the row the set loads, or the enemy's initialization selects, its colours in.
            int paletteBits = loaded.Colours.RowOf(enemy);
            (IReadOnlyList<SheetShade> fixedShades, IReadOnlyList<SheetRowShade> rowShades) = loaded.Shades.Fixed(enemy);
            var pictures = new Dictionary<string, CharacterRange[][]>(StringComparer.Ordinal);
            var rots = new Dictionary<string, string>(StringComparer.Ordinal);
            var frames = new List<SheetFrameSource>();
            foreach (FramePictures framePictures in Frames(inputs.Enemies, shown, checked((ushort)indexes.TileBase), loaded.Characters,
                CorpseRotSimulation.Rots(enemy)))
            {
                frames.AddRange(framePictures.Sources);
                if (framePictures.Layers.Length > 0)
                    pictures.Add(framePictures.Name, framePictures.Layers);
                if (framePictures.RotCorpse is { } corpse)
                    rots.Add(framePictures.Name, corpse);
            }
            yield return new EnemySheetSource(enemy, new SpriteSheetSources(frames, colours,
                ObjectSelectionCodec.GameplaySizes, paletteBits,
                Shades: [.. loaded.Shades.OwnRow(enemy, paletteBits), .. fixedShades],
                RowShades: [.. loaded.Shades.Shared(), .. rowShades]), pictures, rots);
        }
    }

    /// <summary>
    /// Graphics set <paramref name="set"/> as a room load leaves it: its enemy graphics, its OBJ page, its sprite colours,
    /// and the enemies drawn under it, its members in record order, then each enemy its rooms place without the set
    /// loading it, then the enemies those enemies' encounters spawn.
    /// </summary>
    private static LoadedGraphicsSet Load(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(set);
        var graphics = new EnemyGraphicsSet(new EnemyRoomBinding(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { TileArtwork = inputs.Enemies },
            CompiledRoomEnemyDefinitions.Instance, services: null));
        graphics.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, set);
        LoadedSetColours colours = LoadedSetColours.Of(inputs, set);
        List<EnemyDefinitionId> members = [.. graphics.Entries.Select(entry => entry.DefinitionId)];
        IEnumerable<EnemyDefinitionId> placed = RoomStateDefinitions.All.Where(state => state.EnemyGraphicsSet.Name == set.Name)
            .SelectMany(state => state.EnemyPopulation.Records.Span.SelectToArray(record => record.DefinitionId))
            .Except(members).Distinct().Order();
        EnemyDefinitionId[] loaded = [.. members, .. placed];
        // An encounter's own spawns, such as Mother Brain's tubes and the Baby, draw under the set its spawner loads.
        EnemyDefinitionId[] spawned = [.. loaded.SelectMany(enemy => EncounterSpawnedEnemyDefinitions.BySpawner.GetValueOrDefault(enemy, []))
            .Except(loaded).Distinct().Order()];
        EnemyDefinitionId[] drawn = [.. loaded, .. spawned];
        IReadOnlyList<CorpseRotSimulation> corpses = CorpseRotSimulation.Of(inputs.Enemies, inputs.Bosses, drawn);
        var regions = new SetRegions(SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog.Of(set), [.. corpses.Where(corpse => corpse.Kind is null)]);
        return new LoadedGraphicsSet(graphics, new SetCharacters(Table(inputs, graphics), regions, inputs,
            [.. corpses.Where(corpse => corpse.Kind is not null)]), colours, new EnemySheetShades(inputs, colours), drawn);
    }

    /// <summary>A room graphics set as a room load leaves it.</summary>
    /// <param name="Graphics">Its enemy graphics, which give each enemy's tile base.</param>
    /// <param name="Characters">Its OBJ characters, as its room load and its encounters' uploads leave them.</param>
    /// <param name="Colours">Its sprite colours and the row each enemy drawn under it draws in.</param>
    /// <param name="Shades">The shades its sheets are pre-coloured in.</param>
    /// <param name="Drawn">The enemies drawn under it: its members in record order, then the enemies its rooms place without it loading them.</param>
    private sealed record LoadedGraphicsSet(EnemyGraphicsSet Graphics, SetCharacters Characters, LoadedSetColours Colours, EnemySheetShades Shades,
        IReadOnlyList<EnemyDefinitionId> Drawn);

    /// <summary>The OBJ character table as the set leaves it: the standard characters, then each member's tile upload.</summary>
    private static byte[] Table(EnemySheetInputs inputs, EnemyGraphicsSet graphics)
    {
        var table = new byte[SpriteCharacterTableLayout.TableByteCount];
        inputs.StandardObjects.Span.CopyTo(table);
        foreach (RoomEnemyGraphicsSetEntry entry in graphics.Entries)
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
    /// Each shown frame's pictures over the set's characters: an ordinary frame whole, an extended frame component by
    /// component.
    /// </summary>
    private static IEnumerable<FramePictures> Frames(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown,
        int tileBase, SetCharacters characters, bool showsRot) =>
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row.
        Composed(enemies, shown).Select(frame => characters.Pictures(frame.Name, EnemySpriteFrames.Frame(frame.Parts, tileBase, 0), showsRot));

    /// <summary>
    /// Each shown frame's composition by sheet frame name: an ordinary frame whole, an extended frame component by
    /// component. A frame no installed composition shows draws nothing to compose.
    /// </summary>
    private static IEnumerable<ComposedFrame> Composed(EnemyTileSourceArtwork enemies, IReadOnlyList<EnemyFrameId> shown)
    {
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps ?? throw new InvalidDataException("Enemy sheets require the installed enemy compositions.");
        EnemyExtendedFrameCatalog extended = enemies.ExtendedFrames ?? throw new InvalidDataException("Enemy sheets require the installed extended frames.");
        foreach (EnemyFrameId frame in shown)
        {
            if (spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts))
                yield return new ComposedFrame(EnemySheetDefinitions.FrameName(frame), parts);
            else if (extended.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components))
            {
                for (int component = 0; component < components.Length; component++)
                    yield return new ComposedFrame(EnemySheetDefinitions.ComponentFrameName(frame, component), components.Span[component].Parts);
            }
        }
    }

    /// <summary>A shown frame's composition, by the sheet frame name it takes.</summary>
    /// <param name="Name">The sheet frame's name.</param>
    /// <param name="Parts">Its parts.</param>
    private readonly record struct ComposedFrame(string Name, EnemySpritemapParts Parts);
}
