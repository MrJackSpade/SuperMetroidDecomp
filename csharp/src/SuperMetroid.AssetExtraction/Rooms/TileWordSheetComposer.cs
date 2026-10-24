using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours the tile words written over each tileset's rooms (#1392): one image of each part each
/// writer writes over BG1 or BG2 (a boss's body frames, typed text) or BG3 (the HUD, an effect's page), coloured with the
/// tileset's standard palette over the characters the room has loaded, and each tile word pictured again over each
/// artwork the room binds after loading at its character.
/// </summary>
internal static class TileWordSheetComposer
{
    /// <summary>Tiles across a part: the width of the tilemaps the words are written into.</summary>
    private const int PartTiles = 32;

    /// <summary>
    /// Composes the sheet of <paramref name="writers"/>' BG1 and BG2 words over <paramref name="content"/>, the room as it
    /// has loaded and, where its backgrounds read the OBJ character table, as its encounters upload there.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    internal static ComposedTileWordSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IEnumerable<TileWordWriter> writers, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        List<NamedPart> parts = [.. Ensure.NotNull(writers).Order()
            .SelectMany(writer => Parts(inputs, writer).Select(part => part with { Name = $"{writer}.{part.Name}" }))];
        return Compose(inputs, tileset,
            new LayerWords(TileWordLayer.Bg12, parts, content, uploads.Uploads, [uploads.WordCharacterBase], uploads.WordUploads));
    }

    /// <summary>
    /// Composes the sheet of the BG3 words a room of <paramref name="tileset"/> shows: the HUD, its minimap's map tiles and
    /// the pages of the room effects its rooms run, over the HUD's and effects' characters at each HUD character base, and
    /// over each of <paramref name="bg3Uploads"/>, what any room binds over BG3's characters.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    internal static ComposedTileWordSheet ComposeBg3(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IReadOnlyList<BgCharacterUpload> bg3Uploads)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        // The minimap keeps showing the last area's map until it next updates, as across an area's elevator, so every
        // area's map tiles can show in any room.
        List<NamedPart> parts = [.. HudParts(inputs.Maps.GameplayHud), new("Minimap", Minimap(inputs.Maps, Enum.GetValues<AreaId>())),
            .. uploads.FxTypes.Order().Select(type => new NamedPart($"Fx{type}", Page(inputs.Maps.RoomFxLayer3Tilemaps.Resolve(type).Span))),
            Fill(RoomFxRomData.Layer3.ClearTilemapWord)];
        return Compose(inputs, tileset, new LayerWords(TileWordLayer.Bg3, parts,
            uploads.Bg3Content ?? throw new InvalidDataException($"No room of {tileset} records what BG3 shows over."),
            [RoomBlockUploads.HudCharacters(inputs), .. bg3Uploads, .. uploads.Uploads],
            [uploads.Bg3ContentBase, .. uploads.Bg3CharacterBases.Where(characterBase => characterBase != uploads.Bg3ContentBase).Order()], []));
    }

    /// <summary>
    /// Composes the sheet of <paramref name="words"/>' parts over its content, which binds its bound uploads beyond the
    /// room's load, each word's character read at the first of its character bases, and each word pictured again over
    /// each of its options covering its character at any of them.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    [PpuLayoutBoundary("codec: the BG characters written tile words name, from their layer's character bases")]
    private static ComposedTileWordSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, LayerWords words)
    {
        (TileWordLayer layer, List<NamedPart> parts, BgArtworkBindings content, IReadOnlyList<BgCharacterUpload> options, int[] bases,
            IReadOnlyList<BgCharacterUpload> bound) = words;
        Ensure.NotNull(content);
        int characterWords = TileWordSheet.CharacterWordsOf(layer);
        IReadOnlyList<BgCharacterUpload> load = CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Transfer,
            inputs.Characters.Get(tileset.Characters).Transfer);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Append(RoomBlockUploads.HudCharacters(inputs)).Concat(options).Concat(bound)
            .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).Transfer.Span);

        var tiles = new List<TileWordSheetTile>();
        var pictured = new HashSet<PicturedWord>();
        var placed = new List<PlacedWord>();
        int top = 0;
        foreach ((string name, BodyPart part) in parts)
        {
            // A part shows the last word it writes at each position.
            var written = new SortedDictionary<Position16<TilemapCells>, ushort>(RowMajor);
            foreach (BodyRun run in part.Runs)
            {
                for (int index = 0; index < run.Words.Length; index++)
                    written[Cell(run.Start.X + index, run.Start.Y - part.Top)] = run.Words[index];
            }
            foreach ((Position16<TilemapCells> cell, ushort word) in written)
            {
                int first = CharacterWord(bases[0], word, characterWords);
                if (!content.TryShowing(first, characterWords, out BgArtworkShown shown))
                    throw new InvalidDataException($"{TileWordSheet.NameOf(layer, tileset)} part {name} reads a character at VRAM word ${first:X4} its room loads nothing over.");
                var picture = new PicturedWord(new SnesBgTilemapWord(word), shown);
                placed.Add(new PlacedWord(top, picture, cell));
                if (pictured.Add(picture))
                    tiles.Add(new TileWordSheetTile(picture.Artwork, name, Corner(cell)));
            }
            top += part.Rows;
        }
        // A word the room shows over artwork it binds after loading is pictured once more over that artwork.
        var rebound = new List<PicturedWord>();
        foreach (SnesBgTilemapWord word in placed.Select(tile => tile.Picture.Word).Distinct())
        {
            foreach (int first in bases.Select(characterBase => CharacterWord(characterBase, word, characterWords)))
            {
                foreach (BgCharacterUpload upload in options.Where(upload => RoomBlockSheetComposer.Covers(upload, first, characterWords)))
                {
                    var picture = new PicturedWord(word, new BgArtworkShown(upload.Identity, first - upload.FirstWord));
                    if (pictured.Add(picture))
                        rebound.Add(picture);
                }
            }
        }
        int reboundRows = (rebound.Count + PartTiles - 1) / PartTiles;
        var canvas = new RoomTileCanvas(PartTiles * RoomTileCanvas.CharacterPixels, (top + reboundRows) * RoomTileCanvas.CharacterPixels);
        foreach (PlacedWord tile in placed)
            Paint(Cell(tile.Cell.X, tile.PartTop + tile.Cell.Y), tile.Picture);
        for (int index = 0; index < rebound.Count; index++)
        {
            Position16<TilemapCells> cell = Cell(index % PartTiles, index / PartTiles);
            Paint(Cell(cell.X, top + cell.Y), rebound[index]);
            tiles.Add(new TileWordSheetTile(rebound[index].Artwork, TileWordSheet.ReboundFrameName, Corner(cell)));
        }

        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int frameTop = 0;
        foreach ((string name, BodyPart part) in parts)
        {
            frames.Add(name, Frame(canvas, frameTop, part.Rows, layer));
            frameTop += part.Rows;
        }
        if (reboundRows > 0)
            frames.Add(TileWordSheet.ReboundFrameName, Frame(canvas, top, reboundRows, layer));
        return new ComposedTileWordSheet(layer, tileset, canvas.ToSheet(colours, frames, TileWordSheet.ColoursOf(layer)), tiles);

        void Paint(Position16<TilemapCells> cell, PicturedWord picture)
        {
            BgArtworkShown shown = picture.Shown;
            ReadOnlySpan<byte> character = bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), characterWords * sizeof(ushort));
            Position16<ImagePixels> corner = Corner(cell);
            if (layer == TileWordLayer.Bg3)
                canvas.PaintTwoBppTile(corner.X, corner.Y, picture.Word, character, colours);
            else
                canvas.PaintTile(corner.X, corner.Y, picture.Word, character, colours);
        }
    }

    /// <summary>Orders tilemap cells row by row, left to right within a row, as a part's words are laid out.</summary>
    private static readonly Comparer<Position16<TilemapCells>> RowMajor = Comparer<Position16<TilemapCells>>.Create(
        (left, right) => left.Y != right.Y ? left.Y.CompareTo(right.Y) : left.X.CompareTo(right.X));

    /// <summary>The tilemap cell at column <paramref name="column"/> of row <paramref name="row"/>.</summary>
    private static Position16<TilemapCells> Cell(int column, int row) => new(checked((ushort)column), checked((ushort)row));

    /// <summary>The pixel corner of the tile at <paramref name="cell"/> within its frame.</summary>
    private static Position16<ImagePixels> Corner(Position16<TilemapCells> cell) =>
        new(checked((ushort)(cell.X * RoomTileCanvas.CharacterPixels)), checked((ushort)(cell.Y * RoomTileCanvas.CharacterPixels)));

    /// <summary>The frame of <paramref name="rows"/> tile rows from row <paramref name="top"/>, across the sheet.</summary>
    internal static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, int top, int rows, TileWordLayer layer)
    {
        int rowColours = layer == TileWordLayer.Bg3 ? 4 : 16;
        var corner = new Position16<ImagePixels>(0, (ushort)(top * RoomTileCanvas.CharacterPixels));
        var size = new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)(rows * RoomTileCanvas.CharacterPixels));
        SpritePaletteRows coloured = SpritePaletteRows.None;
        for (int y = 0; y < size.Y; y++)
        for (int x = 0; x < size.X; x++)
        {
            byte index = canvas.PaletteIndices[(corner.Y + y) * canvas.Width + x];
            if (index % rowColours != 0)
                coloured |= SpritePaletteRowSet.Rows[index / 16];
        }
        return new SpriteSheetFrameDocument { Corner = corner, Size = size, Origin = default, Rows = coloured, Parts = [] };
    }

    /// <summary>The parts <paramref name="writer"/> writes, each named, in a stable order.</summary>
    /// <exception cref="InvalidDataException">The writer's installed artwork is missing.</exception>
    private static IEnumerable<NamedPart> Parts(RoomBlockSheetInputs inputs, TileWordWriter writer)
    {
        BossArtworkCatalog bosses = inputs.Bosses;
        switch (writer)
        {
            case TileWordWriter.ZebesEscapeText:
                yield return new("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase,
                    EscapeTypewriterDefinitions.ZebesTilemapBase));
                break;
            case TileWordWriter.CeresEscapeText:
                foreach (NamedPart part in CeresWarningParts(inputs))
                    yield return part;
                break;
            case TileWordWriter.Kraid:
                foreach (NamedPart part in KraidParts(bosses.KraidBackground ?? throw Missing(writer)))
                    yield return part;
                break;
            case TileWordWriter.Phantoon:
                foreach (NamedPart part in FrameParts((bosses.PhantoonBg2Frames ?? throw Missing(writer)).Writes))
                    yield return part;
                yield return Fill(PhantoonBg2FrameDefinitions.BlankTile);
                break;
            case TileWordWriter.Draygon:
                foreach (NamedPart part in FrameParts((bosses.DraygonBg2Frames ?? throw Missing(writer)).Writes))
                    yield return part;
                yield return Fill(EnemyBg2SurfaceDefinitions.BlankTile.Raw);
                break;
            case TileWordWriter.Crocomire:
                foreach (NamedPart part in FrameParts((bosses.CrocomireBg2Frames ?? throw Missing(writer)).Writes))
                    yield return part;
                CrocomireMeltingArtwork melting = bosses.CrocomireMelting ?? throw Missing(writer);
                foreach (CrocomireMeltingTilemapAddress image in Enum.GetValues<CrocomireMeltingTilemapAddress>())
                    yield return new($"Melting{image}", Rows(melting.Tilemap(image).ToArray()));
                yield return Fill(CrocomireBg2.CrocomireBlankBg2Tile);
                break;
            case TileWordWriter.MotherBrain:
                foreach (NamedPart part in FrameParts((bosses.MotherBrainBodyBg2Frames ?? throw Missing(writer)).Writes))
                    yield return part;
                yield return Fill(MotherBrainBg2Definitions.BlankTile, MotherBrainDeathRomData.EmptyBodyTile);
                break;
            default:
                throw new InvalidDataException($"Undefined tile word writer {writer}.");
        }
    }

    /// <summary>Kraid's body pages, with and without the priority his growth sets, and his head frames.</summary>
    private static IEnumerable<NamedPart> KraidParts(KraidBackgroundArtwork art)
    {
        ushort[] working = KraidBodyTilemap.ComposeWorkingMap(art);
        ushort[] upper = working[..KraidBackgroundRomData.VisiblePageWords];
        ushort[] lower = working.AsSpan(KraidBackgroundRomData.WorkingLowerHalfWord, KraidBackgroundRomData.VisiblePageWords).ToArray();
        yield return new("Upper", Rows(upper));
        yield return new("UpperPriority", Rows(WithPriority(upper)));
        yield return new("Lower", Rows(lower));
        yield return new("LowerPriority", Rows(WithPriority(lower)));
        foreach ((int mouthStage, KraidHeadTilemapAtlas head) in art.Heads.OrderBy(entry => entry.Key))
            yield return new($"Head{mouthStage}", Rows(head.Words.ToArray()));
        yield return Fill(KraidBackgroundRomData.BlankTile);
    }

    /// <summary>
    /// The warning Ceres Ridley's getaway writes over BG1: its EMERGENCY title, the Japanese subtitle's rows and its typed
    /// text, each word at the BG1 tilemap word it is written to.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed area artwork has no Ceres warning tilemaps.</exception>
    [PpuLayoutBoundary("codec: Ceres warning tilemaps' native BG1 tilemap destinations")]
    private static IEnumerable<NamedPart> CeresWarningParts(RoomBlockSheetInputs inputs)
    {
        CeresEscapeOverlayTilemapCatalog overlays = inputs.Objects.Areas.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The installed area artwork has no Ceres warning tilemaps.");
        CeresEscapeOverlayTilemapDefinition emergency = CeresEscapeOverlayTilemapDefinitions.Emergency;
        yield return new("Emergency", new BodyPart([.. Runs(CeresEscapeOverlayTilemapDefinitions.EmergencyDestination - GameplayVramLayout.Bg1TilemapWord,
            OverlayWords(overlays, emergency.SourceAddress, emergency.WordCount * sizeof(ushort)))]));
        var subtitle = new List<BodyRun>();
        for (int record = 0; record < CeresEscapeVramTransferDefinitions.JapaneseOverlayRecords; record++)
        {
            TileTransferDefinition transfer = CeresEscapeVramTransferDefinitions.Records[record];
            subtitle.AddRange(Runs(transfer.DestinationWord - GameplayVramLayout.Bg1TilemapWord,
                OverlayWords(overlays, transfer.SourceAddress, transfer.ByteCount)));
        }
        yield return new("Subtitle", new BodyPart([.. subtitle]));
        yield return new("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase,
            EscapeTypewriterDefinitions.CeresTilemapBase));
    }

    /// <summary>The words of the installed Ceres warning tilemap the getaway writes from <paramref name="sourceAddress"/>.</summary>
    /// <exception cref="InvalidDataException">No installed warning tilemap holds them.</exception>
    private static ushort[] OverlayWords(CeresEscapeOverlayTilemapCatalog overlays, int sourceAddress, int byteCount)
    {
        if (!overlays.TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data))
            throw new InvalidDataException($"The installed Ceres warning tilemaps hold no {byteCount} bytes from ${sourceAddress:X6}.");
        var words = new ushort[byteCount / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = SnesLittleEndian.Word(data.Span[index * 2], data.Span[index * 2 + 1]);
        return words;
    }

    /// <summary>
    /// The text <paramref name="program"/> types from <paramref name="tileBase"/>, each glyph at the word of the tilemap at
    /// <paramref name="tilemapWord"/> the typewriter writes it to.
    /// </summary>
    [PpuLayoutBoundary("codec: escape typewriter programs' native BG1 and BG2 tilemap destinations")]
    private static BodyPart Typed(EscapeTypewriterProgram program, ushort tileBase, int tilemapWord)
    {
        var runs = new List<BodyRun>();
        foreach (EscapeTypewriterLine line in program.Lines)
        {
            int destination = line.Destination;
            foreach (char character in line.Text)
            {
                if (EscapeTypewriterState.GlyphOf((byte)character, new SnesBgTilemapWord(tileBase)) is { } glyph)
                    runs.AddRange(Runs(destination - tilemapWord, [glyph.Raw]));
                destination = EscapeTypewriterState.NextDestination(destination);
            }
        }
        return new BodyPart([.. runs]);
    }

    /// <summary>
    /// The HUD as a fully stocked Samus sees it, its top row above its three rows, then every other word its writers write:
    /// each energy and reserve-tank count, each ammunition count, each item highlighted and not, the reserve indicator on,
    /// off and cleared, and no items at all.
    /// </summary>
    private static IEnumerable<NamedPart> HudParts(GameplayHudPresentation presentation)
    {
        SamusEquipmentFlags everything = Enum.GetValues<SamusEquipmentFlags>().Aggregate((all, item) => all | item);
        var stocked = new HudSnapshot(1499, 1499, 230, 230, 50, 50, 50, 50, everything, HudItem.Beam, 400, ReserveTankMode.Auto);
        var hud = new HudState();
        hud.BindPresentation(presentation);
        hud.Initialize(stocked);
        ushort[] shown = [.. presentation.TopRow(), .. hud.MutableRows];
        yield return new("Hud", Rows(shown));
        var glyphs = new SortedSet<ushort>();
        void Show(HudSnapshot snapshot)
        {
            hud.Initialize(snapshot);
            glyphs.UnionWith(hud.MutableRows.ToArray());
        }
        for (int tanks = 0; tanks <= 14; tanks++)
        {
            int maximum = 99 + tanks * 100;
            for (int health = 0; health <= maximum; health++)
                Show(stocked with { Health = health, MaxHealth = maximum });
        }
        for (int ammunition = 0; ammunition <= 999; ammunition++)
        {
            Show(stocked with
            {
                Missiles = ammunition, MaxMissiles = 999, SuperMissiles = ammunition % 100, MaxSuperMissiles = 99,
                PowerBombs = ammunition % 100, MaxPowerBombs = 99,
            });
        }
        foreach (HudItem item in Enum.GetValues<HudItem>())
        {
            Show(stocked with { SelectedItem = item });
            hud.ToggleItemHighlight(item, presentation.DeselectedPalette);
            glyphs.UnionWith(hud.MutableRows.ToArray());
        }
        Show(stocked with { ReserveHealth = 0 });
        Show(stocked with { ReserveMode = ReserveTankMode.None });
        hud.ClearAutoReserveIndicator();
        glyphs.UnionWith(hud.MutableRows.ToArray());
        Show(HudSnapshot.CeresDebug);
        glyphs.ExceptWith(shown);
        yield return new("Glyphs", Rows([.. glyphs]));
    }

    /// <summary>Every word the HUD's minimap shows of <paramref name="areas"/>' maps: each tile explored and not, blinking or not, and the blank.</summary>
    private static BodyPart Minimap(IAreaMapSource maps, IEnumerable<AreaId> areas)
    {
        var words = new SortedSet<ushort> { (ushort)MapTileWords.HudBlank };
        foreach (AreaId area in areas)
        {
            IAreaMapView map = maps.Get(area);
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
            {
                MapTileWord tile = map.GetTile(new Position16<MapTiles>((ushort)x, (ushort)y));
                foreach (MapTileWord shown in new[] { tile.ForHud(true), tile.ForHud(false) })
                {
                    words.Add((ushort)shown);
                    words.Add((ushort)shown.WithLocationBlink());
                }
            }
        }
        return Rows([.. words]);
    }

    /// <summary>A room effect's page, as its little-endian words lay it out.</summary>
    private static BodyPart Page(ReadOnlySpan<byte> page)
    {
        var words = new ushort[page.Length / sizeof(ushort)];
        for (int word = 0; word < words.Length; word++)
            words[word] = SnesLittleEndian.Word(page[word * 2], page[word * 2 + 1]);
        return Rows(words);
    }

    /// <summary>Each frame of a catalog: its writes, at the tilemap words they write.</summary>
    private static IEnumerable<NamedPart> FrameParts(EnemyBg2FrameCatalog catalog) =>
        catalog.Frames.OrderBy(frame => frame.Key.ToString(), StringComparer.Ordinal).Select(frame => new NamedPart(frame.Key.ToString(),
            new BodyPart([.. frame.Value.SelectMany(write => Runs(write.DestinationWord, write.Tiles.ToArray()))])));

    /// <summary>The tiles a boss fills its tilemap with, as one part.</summary>
    private static NamedPart Fill(params ushort[] words) =>
        new("Fill", new BodyPart([new BodyRun(Position16<TilemapCells>.Zero, [.. words.Distinct()])]));

    /// <summary><paramref name="words"/> laid out from the top-left, 32 to a row.</summary>
    private static BodyPart Rows(ushort[] words) => new([.. Runs(0, words)]);

    /// <summary><paramref name="words"/> written from tilemap word <paramref name="firstWord"/>, split where they reach a row's end.</summary>
    private static IEnumerable<BodyRun> Runs(int firstWord, ushort[] words)
    {
        for (int start = 0; start < words.Length;)
        {
            int word = firstWord + start, count = Math.Min(words.Length - start, PartTiles - word % PartTiles);
            yield return new BodyRun(Cell(word % PartTiles, word / PartTiles), words[start..(start + count)]);
            start += count;
        }
    }

    /// <summary><paramref name="words"/> with BG2 priority set, as Kraid shows his body once grown.</summary>
    private static ushort[] WithPriority(ushort[] words) => [.. words.Select(word => (ushort)(word | KraidBackgroundRomData.PriorityBit))];

    /// <summary>The failure for a writer whose installed artwork is missing.</summary>
    private static InvalidDataException Missing(TileWordWriter writer) => new($"The installed enemy artwork has no {writer} body.");

    /// <summary>The first VRAM word of the character <paramref name="word"/> names from <paramref name="characterBase"/>.</summary>
    [PpuLayoutBoundary("codec: the BG characters written tile words name, from their layer's character base")]
    private static int CharacterWord(int characterBase, ushort word, int characterWords) =>
        characterBase + new SnesBgTilemapWord(word).CharacterIndex * characterWords;

    /// <summary>One part: the runs of words it writes, laid out at their tilemap positions.</summary>
    /// <param name="Runs">The runs.</param>
    private sealed record BodyPart(BodyRun[] Runs)
    {
        /// <summary>The first tilemap row a run writes.</summary>
        internal int Top => Runs.Min(run => run.Start.Y);

        /// <summary>The tilemap rows from the first a run writes to the last.</summary>
        internal int Rows => Runs.Max(run => run.Start.Y) - Top + 1;
    }

    /// <summary>A run of words written left to right from one tilemap position.</summary>
    /// <param name="Start">The tilemap cell the first word is written to.</param>
    /// <param name="Words">The words.</param>
    private sealed record BodyRun(Position16<TilemapCells> Start, ushort[] Words);

    /// <summary>One part a writer writes, under the name of its frame in the sheet.</summary>
    /// <param name="Name">The frame name.</param>
    /// <param name="Part">The part.</param>
    private sealed record NamedPart(string Name, BodyPart Part);

    /// <summary>One tile word pictured over the artwork its character is read from.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Shown">The artwork, and the word within it, its character is read from.</param>
    private readonly record struct PicturedWord(SnesBgTilemapWord Word, BgArtworkShown Shown)
    {
        /// <summary>The word as the sheet's tile artwork.</summary>
        internal BgTileArtwork Artwork => new(Word, Shown);
    }

    /// <summary>A pictured word at its cell within a part.</summary>
    /// <param name="PartTop">The sheet row the part starts at.</param>
    /// <param name="Picture">The word and the artwork it shows over.</param>
    /// <param name="Cell">The cell within the part.</param>
    private sealed record PlacedWord(int PartTop, PicturedWord Picture, Position16<TilemapCells> Cell);

    /// <summary>The words one layer's sheet pictures, what they show over and where their characters are read.</summary>
    /// <param name="Layer">The backgrounds the words are written over.</param>
    /// <param name="Parts">The named parts, in sheet order.</param>
    /// <param name="Content">The room as it has loaded, which the parts show over.</param>
    /// <param name="Options">The artwork a room binds after loading, over which each word is pictured again.</param>
    /// <param name="Bases">The layer's character bases; the first is where each word's character is read.</param>
    /// <param name="Bound">What the content binds beyond the room's load.</param>
    private sealed record LayerWords(TileWordLayer Layer, List<NamedPart> Parts, BgArtworkBindings Content,
        IReadOnlyList<BgCharacterUpload> Options, int[] Bases, IReadOnlyList<BgCharacterUpload> Bound);
}

