using SuperMetroid.Core.Rendering;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Composes the pre-coloured enemy sheets (#1392). Each room graphics set is loaded as a room load leaves it: the
/// standard OBJ characters with each member's tiles uploaded after them, and each member's palette loaded into its
/// row. Every member's sheet holds the frames its behaviour shows, composed at its tile index and coloured in its row
/// as the set leaves it, so a row a later member overwrites shows that member's colours, as on the cartridge. Each
/// pixel keeps its part's own row, so a hit or freeze flash can still OR in its palette.
/// </summary>
internal static class EnemySheetComposer
{
    /// <summary>The byte of the OBJ character table the enemy tile region begins at.</summary>
    private const int EnemyRegionTableByte = EncounterCharacterPlacementDefinitions.EnemyTileAreaFirstCharacter * SpriteArtworkPlacements.CharacterByteCount;


    /// <summary>Every graphics set's sheets, each distinct sheet once.</summary>
    internal static EnemySheetComposition Compose(EnemySheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var distinct = new Dictionary<string, string>(StringComparer.Ordinal);
        var sheets = new Dictionary<string, ComposedSpriteSheet>(StringComparer.Ordinal);
        var perEnemy = new Dictionary<EnemyDefinitionId, int>();
        var sets = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var spawned = new Dictionary<string, Dictionary<string, SpawnedObjectSheetDocument>>(StringComparer.Ordinal);
        var pictures = new Dictionary<string, Dictionary<string, Dictionary<string, CharacterRange[][]>>>(StringComparer.Ordinal);
        var spawnedPictures = new Dictionary<string, Dictionary<string, Dictionary<string, CharacterRange[][]>>>(StringComparer.Ordinal);
        var rotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        var spawnedRotPictures = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);
        var perBase = new Dictionary<int, int>();
        foreach (RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetDefinitions.DrawnUnder)
        {
            GraphicsSetSheetSources setSources = Sources(inputs, set);
            var members = new Dictionary<string, string>(StringComparer.Ordinal);
            var memberPictures = new Dictionary<string, Dictionary<string, CharacterRange[][]>>(StringComparer.Ordinal);
            var memberRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, IReadOnlyDictionary<string, CharacterRange[][]> framePictures,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Enemies)
            {
                ComposedSpriteSheet composed = SpriteSheetComposer.Compose(sources, 0, 0);
                string key = Fingerprint(composed);
                if (!distinct.TryGetValue(key, out string? name))
                {
                    int ordinal = perEnemy.GetValueOrDefault(enemy);
                    perEnemy[enemy] = ordinal + 1;
                    name = SheetName(enemy, ordinal);
                    distinct.Add(key, name);
                    sheets.Add(name, composed);
                }
                // A set that lists an enemy twice loads it once into each record's row; both must show alike.
                if (members.TryGetValue(enemy.ToString(), out string? listed) && listed != name)
                    throw new InvalidDataException($"Enemy graphics set {set.Name} colours {enemy} two ways.");
                members[enemy.ToString()] = name;
                if (framePictures.Count > 0)
                    memberPictures[enemy.ToString()] = new Dictionary<string, CharacterRange[][]>(framePictures, StringComparer.Ordinal);
                if (frameRots.Count > 0)
                    memberRotPictures[enemy.ToString()] = new Dictionary<string, string>(frameRots, StringComparer.Ordinal);
            }
            sets.Add(set.Name, members);
            if (memberPictures.Count > 0)
                pictures.Add(set.Name, memberPictures);
            if (memberRotPictures.Count > 0)
                rotPictures.Add(set.Name, memberRotPictures);
            var bases = new Dictionary<string, SpawnedObjectSheetDocument>(StringComparer.Ordinal);
            var basePictures = new Dictionary<string, Dictionary<string, CharacterRange[][]>>(StringComparer.Ordinal);
            var baseRotPictures = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources, IReadOnlyDictionary<string, CharacterRange[][]> framePictures,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Spawned)
            {
                ComposedSpriteSheet composed = SpriteSheetComposer.Compose(sources, 0, 0);
                string key = Fingerprint(composed);
                if (!distinct.TryGetValue(key, out string? name))
                {
                    int ordinal = perBase.GetValueOrDefault(plan.TileBase);
                    perBase[plan.TileBase] = ordinal + 1;
                    name = $"Spawned-{plan.TileBase:X3}.{ordinal}";
                    distinct.Add(key, name);
                    sheets.Add(name, composed);
                }
                bases.Add(plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), new SpawnedObjectSheetDocument
                {
                    Sheet = name,
                    Palette = plan.Palette,
                    Spawners = [.. plan.Spawners.Select(enemy => enemy.ToString())],
                    Anywhere = plan.Anywhere,
                });
                if (framePictures.Count > 0)
                    basePictures[plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                        new Dictionary<string, CharacterRange[][]>(framePictures, StringComparer.Ordinal);
                if (frameRots.Count > 0)
                    baseRotPictures[plan.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                        new Dictionary<string, string>(frameRots, StringComparer.Ordinal);
            }
            spawned.Add(set.Name, bases);
            if (basePictures.Count > 0)
                spawnedPictures.Add(set.Name, basePictures);
            if (baseRotPictures.Count > 0)
                spawnedRotPictures.Add(set.Name, baseRotPictures);
        }
        return new EnemySheetComposition(sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetDefinitions.ManifestVersion, Sets = sets, Spawned = spawned, Pictures = pictures,
            SpawnedPictures = spawnedPictures, RotPictures = rotPictures, SpawnedRotPictures = spawnedRotPictures,
        });
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
            [.. SpawnedObjectSheetComposer.Plans(loaded.Graphics, loaded.Drawn).Select(plan =>
                SpawnedObjectSheetComposer.Sources(inputs.Enemies, plan, loaded.Characters, loaded.Colours))]);
    }

    /// <summary>What each enemy drawn under <paramref name="loaded"/> composes.</summary>
    private static IEnumerable<EnemySheetSource> EnemySources(EnemySheetInputs inputs, LoadedGraphicsSet loaded)
    {
        EnemyGraphicsSet graphics = loaded.Graphics;
        RgbColor[] colours = loaded.Colours;
        foreach (EnemyDefinitionId enemy in loaded.Drawn)
        {
            if (!EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown))
                continue;
            SpriteGraphics indexes = graphics.DrawIndexes(enemy);
            // The sheet is coloured in the sprite set the set places the enemy's colours in.
            int paletteBits = graphics.Placement.StoreOf(indexes.Colouring).Set;
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
                ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection), paletteBits), pictures, rots);
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
        var graphics = new EnemyGraphicsSet(new EnemyRoomBinding(new EnemySession { TileArtwork = inputs.Enemies },
            CompiledRoomEnemyDefinitions.Instance, services: null));
        ScreenPalette cgram = RoomLoadPalettes.Colours(inputs.Palettes);
        graphics.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, set, cgram.SpriteSets);
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
            [.. corpses.Where(corpse => corpse.Kind is not null)]), cgram.SpriteSets.All.Colors.ToArray(), drawn);
    }

    /// <summary>A room graphics set as a room load leaves it.</summary>
    /// <param name="Graphics">Its enemy graphics, which give each enemy's tile base and palette.</param>
    /// <param name="Characters">Its OBJ characters, as its room load and its encounters' uploads leave them.</param>
    /// <param name="Colours">Its sprite colours.</param>
    /// <param name="Drawn">The enemies drawn under it: its members in record order, then the enemies its rooms place without it loading them.</param>
    private sealed record LoadedGraphicsSet(EnemyGraphicsSet Graphics, SetCharacters Characters, RgbColor[] Colours, IReadOnlyList<EnemyDefinitionId> Drawn);

    /// <summary>The OBJ character table as the set leaves it: the standard characters, then each member's tile upload.</summary>
    private static byte[] Table(EnemySheetInputs inputs, EnemyGraphicsSet graphics)
    {
        var table = new byte[SpriteArtworkPlacements.TableByteCount];
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

    /// <summary>The installed name of an enemy's <paramref name="ordinal"/>th distinct sheet.</summary>
    private static string SheetName(EnemyDefinitionId enemy, int ordinal) => $"{enemy}.{ordinal}";

    /// <summary>A composed sheet's content hash, which identical sheets share.</summary>
    private static string Fingerprint(ComposedSpriteSheet sheet)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(MemoryMarshal.AsBytes(sheet.Pixels.AsSpan()));
        hash.AppendData(sheet.PaletteIndices);
        hash.AppendData(sheet.Priorities);
        hash.AppendData(sheet.PartRows);
        hash.AppendData(SpriteSheet.Write(sheet.Manifest));
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>A shown frame's composition, by the sheet frame name it takes.</summary>
    /// <param name="Name">The sheet frame's name.</param>
    /// <param name="Parts">Its parts.</param>
    private readonly record struct ComposedFrame(string Name, EnemySpritemapParts Parts);
}
