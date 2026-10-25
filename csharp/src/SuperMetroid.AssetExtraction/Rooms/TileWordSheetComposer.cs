using SuperMetroid.Core;
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
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
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

    /// <summary>Cells in reading order: by row, then by column.</summary>
    private static readonly IComparer<Position16<TilemapCells>> RowMajor =
        Comparer<Position16<TilemapCells>>.Create((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

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
            .SelectMany(writer => Parts(inputs, writer).Select(part => new NamedPart($"{writer}.{part.Name}", part.Part)))];
        // Words over the OBJ character table show only what its load and encounters place there, never the tileset's.
        return Compose(inputs, tileset, TileWordLayer.Bg12, parts, content, new TileWordBindings(
            uploads.WordCharacters == RoomBackgroundCharacters.ObjectCharacterPage ? [] : uploads.Uploads, [BgCharacterSet.Tiles], uploads.WordUploads));
    }

    /// <summary>
    /// Composes the sheet of the BG3 words a room of <paramref name="tileset"/> shows: the HUD, its minimap's map tiles and
    /// the pages of the room effects its rooms run, over the HUD's and effects' characters in each set BG3 reads, and
    /// over each of <paramref name="bg3Uploads"/>, what any room binds over BG3's characters.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    internal static ComposedTileWordSheet ComposeBg3(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IReadOnlyList<BgCharacterPlacement> bg3Uploads)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        // The minimap keeps showing the last area's map until it next updates, as across an area's elevator, so every
        // area's map tiles can show in any room.
        List<NamedPart> parts = [.. HudParts(inputs.Maps.GameplayHud), new NamedPart("Minimap", Minimap(Enum.GetValues<AreaId>().Select(inputs.Maps.Get))),
            .. uploads.FxTypes.Order().Select(type => new NamedPart($"Fx{type}", Page(inputs.Maps.RoomFxLayer3Tilemaps.Resolve(type).Span))),
            Fill(Layer3Plane.ClearedCell)];
        return Compose(inputs, tileset, TileWordLayer.Bg3, parts,
            uploads.Bg3Content ?? throw new InvalidDataException($"No room of {tileset} records what BG3 shows over."),
            new TileWordBindings([RoomBlockUploads.HudCharacters(inputs), .. bg3Uploads, .. uploads.Uploads],
                [uploads.Bg3ContentSet, .. uploads.Bg3CharacterSets.Where(set => set != uploads.Bg3ContentSet).Order()], []));
    }

    /// <summary>
    /// Composes <paramref name="layer"/>'s sheet of <paramref name="parts"/> over <paramref name="content"/>, which binds
    /// <paramref name="bindings"/>' bound artwork beyond the room's load, each word's character read in the first of its
    /// sets, and each word pictured again over each of its options covering its character in any of them.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    private static ComposedTileWordSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, TileWordLayer layer,
        List<NamedPart> parts, BgArtworkBindings content, TileWordBindings bindings)
    {
        Ensure.NotNull(content);
        int characterWords = TileWordSheet.CharacterWordsOf(layer);
        IReadOnlyList<BgCharacterPlacement> load = CartridgeRoomAssets.CharacterPlacements(tileset, inputs.Characters.Cre.Characters,
            inputs.Characters.Get(tileset.Characters).Characters);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Append(RoomBlockUploads.HudCharacters(inputs)).Concat(bindings.Options).Concat(bindings.Bound)
            .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).PackedColours.Span);

        var tiles = new List<TileWordSheetTile>();
        var pictured = new HashSet<ShownWord>();
        var placed = new List<PlacedWord>();
        int top = 0;
        foreach (NamedPart part in parts)
        {
            // A part shows the last word it writes at each position.
            var written = new SortedDictionary<Position16<TilemapCells>, ushort>(RowMajor);
            foreach (BodyRun run in part.Part.Runs)
            {
                for (int index = 0; index < run.Words.Length; index++)
                    written[new((ushort)(run.Start.X + index), (ushort)(run.Start.Y - part.Part.Top))] = run.Words[index];
            }
            foreach ((Position16<TilemapCells> cell, ushort raw) in written)
            {
                var word = new SnesBgTilemapWord(raw);
                int character = word.CharacterIndex;
                if (!content.TryShowing(bindings.Sets[0], character, 1, out BgArtworkShown shown))
                    throw new InvalidDataException($"{TileWordSheet.NameOf(layer, tileset)} part {part.Name} reads {bindings.Sets[0]} character {character}, which its room loads nothing over.");
                var shownWord = new ShownWord(word, shown);
                placed.Add(new PlacedWord(top, shownWord, cell));
                if (pictured.Add(shownWord))
                    tiles.Add(new TileWordSheetTile(new BgTileArtwork(word, shown), part.Name, Corner(cell)));
            }
            top += part.Part.Rows;
        }
        // A word the room shows over artwork it binds after loading is pictured once more over that artwork.
        var rebound = new List<ShownWord>();
        foreach (SnesBgTilemapWord word in placed.Select(tile => tile.Shown.Word).Distinct())
        {
            int character = word.CharacterIndex;
            foreach (BgCharacterSet set in bindings.Sets)
            {
                foreach (BgCharacterPlacement placement in bindings.Options.Where(placement => placement.Covers(set, character)))
                {
                    var again = new ShownWord(word, placement.ShownAt(character));
                    if (pictured.Add(again))
                        rebound.Add(again);
                }
            }
        }
        int reboundRows = (rebound.Count + PartTiles - 1) / PartTiles;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(PartTiles * RoomTileCanvas.CharacterPixels, checked((ushort)((top + reboundRows) * RoomTileCanvas.CharacterPixels))));
        foreach (PlacedWord tile in placed)
            Paint(new(tile.Cell.X, (ushort)(tile.PartTop + tile.Cell.Y)), tile.Shown);
        for (int index = 0; index < rebound.Count; index++)
        {
            var cell = new Position16<TilemapCells>((ushort)(index % PartTiles), (ushort)(index / PartTiles));
            Paint(new(cell.X, (ushort)(top + cell.Y)), rebound[index]);
            tiles.Add(new TileWordSheetTile(new BgTileArtwork(rebound[index].Word, rebound[index].Shown),
                TileWordSheet.ReboundFrameName, Corner(cell)));
        }

        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int frameTop = 0;
        foreach (NamedPart part in parts)
        {
            frames.Add(part.Name, Frame(canvas, frameTop, part.Part.Rows, layer));
            frameTop += part.Part.Rows;
        }
        if (reboundRows > 0)
            frames.Add(TileWordSheet.ReboundFrameName, Frame(canvas, top, reboundRows, layer));
        return new ComposedTileWordSheet(layer, tileset, canvas.ToSheet(colours, frames, TileWordSheet.ColoursOf(layer)), tiles);

        void Paint(Position16<TilemapCells> cell, ShownWord word)
        {
            ReadOnlySpan<byte> character = bytes[word.Shown.Artwork].Span.Slice(word.Shown.WordOffset * sizeof(ushort), characterWords * sizeof(ushort));
            int x = cell.X * RoomTileCanvas.CharacterPixels, y = cell.Y * RoomTileCanvas.CharacterPixels;
            if (layer == TileWordLayer.Bg3)
                canvas.PaintTwoBppTile(x, y, word.Word, character, colours);
            else
                canvas.PaintTile(x, y, word.Word, character, colours);
        }
    }

    /// <summary>The pixel corner of the tile at <paramref name="cell"/> within its frame.</summary>
    private static Position16<ImagePixels> Corner(Position16<TilemapCells> cell) =>
        new((ushort)(cell.X * RoomTileCanvas.CharacterPixels), (ushort)(cell.Y * RoomTileCanvas.CharacterPixels));

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
                yield return new NamedPart("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase));
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
                foreach (CrocomireMeltingIllustration image in Enum.GetValues<CrocomireMeltingIllustration>())
                    yield return new NamedPart($"Melting{image}Tilemap", Rows(melting.Tilemap(image).ToArray()));
                yield return Fill(CrocomireBg2.CrocomireBlankBg2Tile);
                break;
            case TileWordWriter.MotherBrain:
                foreach (NamedPart part in FrameParts((bosses.MotherBrainBodyBg2Frames ?? throw Missing(writer)).Writes))
                    yield return part;
                yield return Fill(MotherBrainBg2Definitions.BlankTile);
                break;
            default:
                throw new InvalidDataException($"Undefined tile word writer {writer}.");
        }
    }

    /// <summary>Kraid's body pages, with and without the priority his growth sets, and his head frames.</summary>
    private static IEnumerable<NamedPart> KraidParts(KraidBackgroundArtwork art)
    {
        ushort[] working = KraidBodyTilemap.ComposeWorkingMap(art);
        ushort[] upper = working[..KraidBackgroundDefinitions.PageCells];
        ushort[] lower = working.AsSpan(KraidBackgroundDefinitions.LowerPageFirstCell, KraidBackgroundDefinitions.PageCells).ToArray();
        yield return new NamedPart("Upper", Rows(upper));
        yield return new NamedPart("UpperPriority", Rows(WithPriority(upper)));
        yield return new NamedPart("Lower", Rows(lower));
        yield return new NamedPart("LowerPriority", Rows(WithPriority(lower)));
        foreach ((int mouthStage, KraidHeadTilemapAtlas head) in art.Heads.OrderBy(entry => entry.Key))
            yield return new NamedPart($"Head{mouthStage}", Rows(head.Words.ToArray()));
        yield return Fill(KraidBackgroundDefinitions.BlankTile);
    }

    /// <summary>
    /// The warning Ceres Ridley's getaway writes over BG1: its EMERGENCY title, the Japanese subtitle's rows and its typed
    /// text, each word at the BG1 overlay cell it is written to.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed area artwork has no Ceres warning tilemaps.</exception>
    private static IEnumerable<NamedPart> CeresWarningParts(RoomBlockSheetInputs inputs)
    {
        CeresEscapeOverlayTilemapCatalog overlays = inputs.Objects.Areas.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The installed area artwork has no Ceres warning tilemaps.");
        CeresEscapeOverlayTilemapDefinition emergency = CeresEscapeOverlayTilemapDefinitions.Emergency;
        yield return new NamedPart("Emergency", new BodyPart([.. Runs(emergency.Cell, OverlayWords(overlays, emergency))]));
        var subtitle = new List<BodyRun>();
        foreach (CeresEscapeOverlayTilemapDefinition row in CeresEscapeOverlayTilemapDefinitions.Japanese)
            subtitle.AddRange(Runs(row.Cell, OverlayWords(overlays, row)));
        yield return new NamedPart("Subtitle", new BodyPart([.. subtitle]));
        yield return new NamedPart("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase));
    }

    /// <summary>The words of the installed Ceres warning tilemap <paramref name="page"/> the getaway writes.</summary>
    /// <exception cref="InvalidDataException">No installed warning tilemap holds them.</exception>
    private static ushort[] OverlayWords(CeresEscapeOverlayTilemapCatalog overlays, CeresEscapeOverlayTilemapDefinition page)
    {
        if (!overlays.TryResolve(page, out ReadOnlyMemory<byte> data))
            throw new InvalidDataException($"The installed Ceres warning tilemaps hold no {page.Name} page.");
        var words = new ushort[page.WordCount];
        for (int index = 0; index < words.Length; index++)
            words[index] = SnesLittleEndian.Word(data.Span[index * 2], data.Span[index * 2 + 1]);
        return words;
    }

    /// <summary>The text <paramref name="program"/> types from <paramref name="tileBase"/>, each glyph at the overlay cell it is typed over.</summary>
    private static BodyPart Typed(EscapeTypewriterProgram program, ushort tileBase)
    {
        var runs = new List<BodyRun>();
        foreach (EscapeTypewriterLine line in program.Lines)
        {
            int cell = line.Cell;
            foreach (char character in line.Text)
            {
                if (EscapeTypewriterState.GlyphOf((byte)character, new SnesBgTilemapWord(tileBase)) is { } glyph)
                    runs.AddRange(Runs(cell, [glyph.Raw]));
                cell++;
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
        yield return new NamedPart("Hud", Rows(shown));
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
        yield return new NamedPart("Glyphs", Rows([.. glyphs]));
    }

    /// <summary>Every word the HUD's minimap shows of <paramref name="maps"/>: each tile explored and not, blinking or not, and the blank.</summary>
    private static BodyPart Minimap(IEnumerable<IAreaMapView> maps)
    {
        var words = new SortedSet<ushort> { (ushort)MapTileWords.HudBlank };
        foreach (IAreaMapView map in maps)
        {
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
            new BodyPart([.. frame.Value.SelectMany(write => Runs(write.FirstCell, write.Tiles.ToArray()))])));

    /// <summary>The tiles a boss fills its tilemap with, as one part.</summary>
    private static NamedPart Fill(params ushort[] words) => new("Fill", new BodyPart([new BodyRun(default, [.. words.Distinct()])]));

    /// <summary><paramref name="words"/> laid out from the top-left, 32 to a row.</summary>
    private static BodyPart Rows(ushort[] words) => new([.. Runs(0, words)]);

    /// <summary><paramref name="words"/> written from tilemap word <paramref name="firstWord"/>, split where they reach a row's end.</summary>
    private static IEnumerable<BodyRun> Runs(int firstWord, ushort[] words)
    {
        for (int start = 0; start < words.Length;)
        {
            int word = firstWord + start, count = Math.Min(words.Length - start, PartTiles - word % PartTiles);
            yield return new BodyRun(new((ushort)(word % PartTiles), (ushort)(word / PartTiles)), words[start..(start + count)]);
            start += count;
        }
    }

    /// <summary><paramref name="words"/> with BG2 priority set, as Kraid shows his body once grown.</summary>
    private static ushort[] WithPriority(ushort[] words) => [.. words.Select(word => (ushort)(word | SnesBgTilemapWord.PriorityMask))];

    /// <summary>The failure for a writer whose installed artwork is missing.</summary>
    private static InvalidDataException Missing(TileWordWriter writer) => new($"The installed enemy artwork has no {writer} body.");

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
    /// <param name="Start">The first word's cell.</param>
    /// <param name="Words">The words.</param>
    private sealed record BodyRun(Position16<TilemapCells> Start, ushort[] Words);

    /// <summary>One part a writer writes, by the name its frame takes.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Part">The words it writes.</param>
    private sealed record NamedPart(string Name, BodyPart Part);

    /// <summary>A tile word as it shows over the artwork bound at its character.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Shown">The artwork it shows.</param>
    private readonly record struct ShownWord(SnesBgTilemapWord Word, BgArtworkShown Shown);

    /// <summary>A word placed in its part.</summary>
    /// <param name="PartTop">The sheet row its part starts at.</param>
    /// <param name="Shown">The word as it shows.</param>
    /// <param name="Cell">Its cell, from its part's first row.</param>
    private readonly record struct PlacedWord(int PartTop, ShownWord Shown, Position16<TilemapCells> Cell);

    /// <summary>What a layer's words are shown over beyond the room's load.</summary>
    /// <param name="Options">Artwork any room binds over the layer's characters, each word pictured again over.</param>
    /// <param name="Sets">The character sets the layer reads, the first the one its words are read in.</param>
    /// <param name="Bound">Artwork the room binds beyond its load.</param>
    private sealed record TileWordBindings(IReadOnlyList<BgCharacterPlacement> Options, BgCharacterSet[] Sets, IReadOnlyList<BgCharacterPlacement> Bound);
}

