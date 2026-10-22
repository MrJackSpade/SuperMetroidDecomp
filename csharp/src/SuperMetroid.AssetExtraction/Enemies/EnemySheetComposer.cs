using SuperMetroid.Core.Rendering;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Hardware;
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
    private static readonly int EnemyRegionTableByte =
        (EnemyVramLayout.EnemyTiles.Offset - GameplayVramLayout.ObjectCharacters.Offset) * sizeof(ushort);

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>Every graphics set's sheets, each distinct sheet once.</summary>
    internal static EnemySheetComposition Compose(EnemySheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var distinct = new Dictionary<string, string>(StringComparer.Ordinal);
        var sheets = new Dictionary<string, ComposedSpriteSheet>(StringComparer.Ordinal);
        var perEnemy = new Dictionary<EnemyDefinitionId, int>();
        var sets = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var spawned = new Dictionary<string, Dictionary<string, SpawnedObjectSheetDocument>>(StringComparer.Ordinal);
        var perBase = new Dictionary<int, int>();
        foreach (RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetDefinitions.DrawnUnder)
        {
            GraphicsSetSheetSources setSources = Sources(inputs, set);
            var members = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources) in setSources.Enemies)
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
            }
            sets.Add(set.Name, members);
            if (!setSources.SpawnsFromSheets)
                continue;
            var bases = new Dictionary<string, SpawnedObjectSheetDocument>(StringComparer.Ordinal);
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources) in setSources.Spawned)
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
            }
            spawned.Add(set.Name, bases);
        }
        return new EnemySheetComposition(sheets, new EnemySheetManifestDocument
        {
            Version = EnemySheetDefinitions.ManifestVersion, Sets = sets, Spawned = spawned,
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
        bool spawnsFromSheets = SpawnedObjectSheetComposer.Covers(loaded.Drawn);
        return new GraphicsSetSheetSources([.. EnemySources(inputs, loaded)], spawnsFromSheets,
            spawnsFromSheets ? [.. SpawnedObjectSheetComposer.Plans(loaded.Graphics, loaded.Drawn).Select(plan => new SpawnedObjectSheetSource(
                plan, SpawnedObjectSheetComposer.Sources(inputs.Enemies, plan, loaded.Page, loaded.Colours)))] : []);
    }

    /// <summary>What each enemy drawn under <paramref name="loaded"/> composes.</summary>
    private static IEnumerable<EnemySheetSource> EnemySources(EnemySheetInputs inputs, LoadedGraphicsSet loaded)
    {
        EnemyGraphicsSet graphics = loaded.Graphics;
        SpriteArtwork page = loaded.Page;
        Bgr555[] colours = loaded.Colours;
        foreach (EnemyDefinitionId enemy in loaded.Drawn)
        {
            if (EnemySheetDefinitions.DrawnFromCharacters.Contains(enemy) ||
                !EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown))
                continue;
            EnemyGraphicsIndexes indexes = graphics.DrawIndexes(enemy);
            int paletteBits = SnesObjAttributeWord.FromPaletteBits(checked((ushort)indexes.PaletteIndex)).PaletteIndex;
            yield return new EnemySheetSource(enemy, new SpriteSheetSources(Frames(inputs.Enemies, shown, checked((ushort)indexes.TileIndex), page).ToList(),
                colours, ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection), paletteBits));
        }
    }

    /// <summary>
    /// Graphics set <paramref name="set"/> as a room load leaves it: its enemy graphics, its OBJ page, its sprite colours,
    /// and the enemies drawn under it, its members in record order and then each enemy its rooms place without the set
    /// loading it.
    /// </summary>
    private static LoadedGraphicsSet Load(EnemySheetInputs inputs, RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(set);
        var graphics = new EnemyGraphicsSet(new EnemyRoomBinding(new EnemySession { TileArtwork = inputs.Enemies },
            CompiledRoomEnemyDefinitions.Instance, services: null));
        SnesCgram cgram = RoomLoadPalettes.Cgram(inputs.Palettes);
        graphics.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, set, new SnesVram(), cgram);
        List<EnemyDefinitionId> members = [.. graphics.Entries.Select(entry => entry.DefinitionId)];
        IEnumerable<EnemyDefinitionId> placed = RoomStateDefinitions.All.Where(state => state.EnemyGraphicsSet.Name == set.Name)
            .SelectMany(state => state.EnemyPopulation.Records.ToArray().Select(record => record.DefinitionId))
            .Except(members).Distinct().Order();
        return new LoadedGraphicsSet(graphics, Page(inputs, graphics), cgram.Colors[SpriteColours..].ToArray(), [.. members, .. placed]);
    }

    /// <summary>A room graphics set as a room load leaves it.</summary>
    /// <param name="Graphics">Its enemy graphics, which give each enemy's tile base and palette.</param>
    /// <param name="Page">Its OBJ character page.</param>
    /// <param name="Colours">Its sprite colours.</param>
    /// <param name="Drawn">The enemies drawn under it: its members in record order, then the enemies its rooms place without it loading them.</param>
    private sealed record LoadedGraphicsSet(EnemyGraphicsSet Graphics, SpriteArtwork Page, Bgr555[] Colours, IReadOnlyList<EnemyDefinitionId> Drawn);

    /// <summary>The OBJ character table as the set leaves it: the standard characters, then each member's tile upload.</summary>
    private static SpriteArtwork Page(EnemySheetInputs inputs, EnemyGraphicsSet graphics)
    {
        var table = new byte[ObjCharacterUploads.TableByteCount];
        inputs.StandardObjects.Span.CopyTo(table);
        foreach (RoomEnemyGraphicsSetEntry entry in graphics.Entries)
        {
            if (!inputs.Enemies.TryResolve(entry.Definition.TileDataAddress, entry.TileByteCount, out ReadOnlyMemory<byte> tiles))
                throw new InvalidDataException($"Enemy {entry.DefinitionId} has no installed tiles for its graphics-set upload.");
            int first = EnemyRegionTableByte + entry.StagingOffset;
            int shown = Math.Min(tiles.Length, table.Length - first);
            if (shown > 0)
                tiles.Span[..shown].CopyTo(table.AsSpan(first));
        }
        return ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The names of the frames an enemy's sheets hold, in composition order.</summary>
    internal static IEnumerable<string> FrameNames(EnemyTileArtworkCatalog enemies, EnemyDefinitionId enemy) =>
        EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown)
            ? Composed(enemies, shown).Select(frame => frame.Name) : [];

    /// <summary>Each shown frame over the page: an ordinary frame whole, an extended frame component by component.</summary>
    private static IEnumerable<SheetFrameSource> Frames(EnemyTileArtworkCatalog enemies, IReadOnlyList<EnemyFrameId> shown,
        ushort tileBase, SpriteArtwork page) =>
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row.
        Composed(enemies, shown).Select(frame => new SheetFrameSource(frame.Name, EnemySpriteFrames.Frame(frame.Parts, tileBase, 0), page));

    /// <summary>
    /// Each shown frame's composition by sheet frame name: an ordinary frame whole, an extended frame component by
    /// component. A frame no installed composition shows draws nothing to compose.
    /// </summary>
    private static IEnumerable<(string Name, EnemySpritemapParts Parts)> Composed(EnemyTileArtworkCatalog enemies, IReadOnlyList<EnemyFrameId> shown)
    {
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps ?? throw new InvalidDataException("Enemy sheets require the installed enemy compositions.");
        EnemyExtendedFrameCatalog extended = enemies.ExtendedFrames ?? throw new InvalidDataException("Enemy sheets require the installed extended frames.");
        foreach (EnemyFrameId frame in shown)
        {
            if (spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts))
                yield return (EnemySheetDefinitions.FrameName(frame), parts);
            else if (extended.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components))
            {
                for (int component = 0; component < components.Length; component++)
                    yield return (EnemySheetDefinitions.ComponentFrameName(frame, component), components.Span[component].Parts);
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
}
