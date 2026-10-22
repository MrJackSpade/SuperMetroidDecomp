using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
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
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each boss body (#1392): one image of each part the boss writes into the BG2 tilemap it
/// shows its body on, coloured with its room tileset's standard palette over the characters the room has loaded, and
/// each tile word pictured again over each artwork the room binds after loading at its character.
/// </summary>
internal static class BossBodySheetComposer
{
    /// <summary>Tiles across a part: the width of the tilemap a boss writes its body into.</summary>
    private const int PartTiles = 32;

    /// <summary>Composes <paramref name="body"/>'s sheet over <paramref name="content"/>, the room as it has loaded.</summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    [PpuLayoutBoundary("codec: the BG characters a boss body's tile words name, from the room tileset's character base")]
    internal static ComposedBossBody Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        BossBody body, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        Ensure.NotNull(content);
        IReadOnlyList<BgCharacterUpload> load = CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Transfer,
            inputs.Characters.Get(tileset.Characters).Transfer);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Concat(uploads.Uploads).GroupBy(upload => upload.Identity)
            .ToDictionary(group => group.Key, group => group.First().Bytes);
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).Transfer.Span);
        List<(string Name, BodyPart Part)> parts = [.. Parts(inputs.Bosses, body)];

        var tiles = new List<BossBodyTile>();
        var pictured = new HashSet<(ushort, BgArtworkShown)>();
        var placed = new List<(int Top, ushort Word, BgArtworkShown Shown, int X, int Y)>();
        int top = 0;
        foreach ((string name, BodyPart part) in parts)
        {
            // A part shows the last word it writes at each position.
            var written = new SortedDictionary<(int Y, int X), ushort>();
            foreach (BodyRun run in part.Runs)
            {
                for (int index = 0; index < run.Words.Length; index++)
                    written[(run.Y - part.Top, run.X + index)] = run.Words[index];
            }
            foreach (((int y, int x), ushort word) in written)
            {
                int first = CharacterWord(word);
                if (!content.TryShowing(first, RoomBlockPictureNames.CharacterWords, out BgArtworkShown shown))
                    throw new InvalidDataException($"{BossBodySheet.NameOf(body, tileset)} part {name} reads a character at VRAM word ${first:X4} its room loads nothing over.");
                placed.Add((top, word, shown, x, y));
                if (pictured.Add((word, shown)))
                    tiles.Add(new BossBodyTile(new BgTileArtwork(new SnesBgTilemapWord(word), shown), name, Corner(x, y)));
            }
            top += part.Rows;
        }
        // A word the room shows over artwork it binds after loading is pictured once more over that artwork.
        var rebound = new List<(ushort Word, BgArtworkShown Shown)>();
        foreach (ushort word in placed.Select(tile => tile.Word).Distinct())
        {
            int first = CharacterWord(word);
            foreach (BgCharacterUpload upload in uploads.Uploads.Where(upload => RoomBlockSheetComposer.Covers(upload, first)))
            {
                var shown = new BgArtworkShown(upload.Identity, first - upload.FirstWord);
                if (pictured.Add((word, shown)))
                    rebound.Add((word, shown));
            }
        }
        int reboundRows = (rebound.Count + PartTiles - 1) / PartTiles;
        var canvas = new RoomTileCanvas(PartTiles * RoomTileCanvas.CharacterPixels, (top + reboundRows) * RoomTileCanvas.CharacterPixels);
        foreach ((int partTop, ushort word, BgArtworkShown shown, int x, int y) in placed)
            Paint(x, partTop + y, word, shown);
        for (int index = 0; index < rebound.Count; index++)
        {
            (ushort word, BgArtworkShown shown) = rebound[index];
            int x = index % PartTiles, y = index / PartTiles;
            Paint(x, top + y, word, shown);
            tiles.Add(new BossBodyTile(new BgTileArtwork(new SnesBgTilemapWord(word), shown), BossBodySheet.ReboundFrameName, Corner(x, y)));
        }

        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int frameTop = 0;
        foreach ((string name, BodyPart part) in parts)
        {
            frames.Add(name, Frame(canvas, frameTop, part.Rows));
            frameTop += part.Rows;
        }
        if (reboundRows > 0)
            frames.Add(BossBodySheet.ReboundFrameName, Frame(canvas, top, reboundRows));
        return new ComposedBossBody(body, tileset, canvas.ToSheet(colours, frames), tiles);

        void Paint(int x, int y, ushort word, BgArtworkShown shown) =>
            canvas.PaintTile(x * RoomTileCanvas.CharacterPixels, y * RoomTileCanvas.CharacterPixels, new SnesBgTilemapWord(word),
                bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours);
    }

    /// <summary>The pixel corner of the tile at (<paramref name="x"/>, <paramref name="y"/>) within its frame.</summary>
    private static Position16<ImagePixels> Corner(int x, int y) =>
        new((ushort)(x * RoomTileCanvas.CharacterPixels), (ushort)(y * RoomTileCanvas.CharacterPixels));

    /// <summary>The frame of <paramref name="rows"/> tile rows from row <paramref name="top"/>, across the sheet.</summary>
    private static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, int top, int rows)
    {
        var corner = new Position16<ImagePixels>(0, (ushort)(top * RoomTileCanvas.CharacterPixels));
        var size = new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)(rows * RoomTileCanvas.CharacterPixels));
        SpritePaletteRows coloured = SpritePaletteRows.None;
        for (int y = 0; y < size.Y; y++)
        for (int x = 0; x < size.X; x++)
        {
            byte index = canvas.PaletteIndices[(corner.Y + y) * canvas.Width + x];
            if (index != 0)
                coloured |= SpritePaletteRowSet.Rows[index / 16];
        }
        return new SpriteSheetFrameDocument { Corner = corner, Size = size, Origin = default, Rows = coloured, Parts = [] };
    }

    /// <summary>The parts <paramref name="body"/> writes into its tilemap, each named, in a stable order.</summary>
    /// <exception cref="InvalidDataException">The boss's installed artwork is missing.</exception>
    private static IEnumerable<(string Name, BodyPart Part)> Parts(BossArtworkCatalog bosses, BossBody body)
    {
        switch (body)
        {
            case BossBody.Kraid:
                foreach ((string, BodyPart) part in KraidParts(bosses.KraidBackground ?? throw Missing(body)))
                    yield return part;
                break;
            case BossBody.Phantoon:
                foreach ((string, BodyPart) part in FrameParts((bosses.PhantoonBg2Frames ?? throw Missing(body)).Writes))
                    yield return part;
                yield return Fill(PhantoonBg2FrameDefinitions.BlankTile);
                break;
            case BossBody.Draygon:
                foreach ((string, BodyPart) part in FrameParts((bosses.DraygonBg2Frames ?? throw Missing(body)).Writes))
                    yield return part;
                yield return Fill(EnemyBg2SurfaceDefinitions.BlankTile.Raw);
                break;
            case BossBody.Crocomire:
                foreach ((string, BodyPart) part in FrameParts((bosses.CrocomireBg2Frames ?? throw Missing(body)).Writes))
                    yield return part;
                CrocomireMeltingArtwork melting = bosses.CrocomireMelting ?? throw Missing(body);
                foreach (CrocomireMeltingTilemapAddress image in Enum.GetValues<CrocomireMeltingTilemapAddress>())
                    yield return ($"Melting{image}", Rows(melting.Tilemap(image).ToArray()));
                yield return Fill(CrocomireBg2.CrocomireBlankBg2Tile);
                break;
            case BossBody.MotherBrain:
                foreach ((string, BodyPart) part in FrameParts((bosses.MotherBrainBodyBg2Frames ?? throw Missing(body)).Writes))
                    yield return part;
                yield return Fill(MotherBrainBg2Definitions.BlankTile, MotherBrainDeathRomData.EmptyBodyTile);
                break;
            default:
                throw new InvalidDataException($"Undefined boss body {body}.");
        }
    }

    /// <summary>Kraid's body pages, with and without the priority his growth sets, and his head frames.</summary>
    private static IEnumerable<(string Name, BodyPart Part)> KraidParts(KraidBackgroundArtwork art)
    {
        ushort[] working = KraidBodyTilemap.ComposeWorkingMap(art);
        ushort[] upper = working[..KraidBackgroundRomData.VisiblePageWords];
        ushort[] lower = working.AsSpan(KraidBackgroundRomData.WorkingLowerHalfWord, KraidBackgroundRomData.VisiblePageWords).ToArray();
        yield return ("Upper", Rows(upper));
        yield return ("UpperPriority", Rows(WithPriority(upper)));
        yield return ("Lower", Rows(lower));
        yield return ("LowerPriority", Rows(WithPriority(lower)));
        foreach ((int mouthStage, KraidHeadTilemapAtlas head) in art.Heads.OrderBy(entry => entry.Key))
            yield return ($"Head{mouthStage}", Rows(head.Words.ToArray()));
        yield return Fill(KraidBackgroundRomData.BlankTile);
    }

    /// <summary>Each frame of a catalog: its writes, at the tilemap words they write.</summary>
    private static IEnumerable<(string Name, BodyPart Part)> FrameParts(EnemyBg2FrameCatalog catalog) =>
        catalog.Frames.OrderBy(frame => frame.Key.ToString(), StringComparer.Ordinal).Select(frame => (frame.Key.ToString(),
            new BodyPart([.. frame.Value.SelectMany(write => Runs(write.DestinationWord, write.Tiles.ToArray()))])));

    /// <summary>The tiles a boss fills its tilemap with, as one part.</summary>
    private static (string Name, BodyPart Part) Fill(params ushort[] words) => ("Fill", new BodyPart([new BodyRun(0, 0, [.. words.Distinct()])]));

    /// <summary><paramref name="words"/> laid out from the top-left, 32 to a row.</summary>
    private static BodyPart Rows(ushort[] words) => new([.. Runs(0, words)]);

    /// <summary><paramref name="words"/> written from tilemap word <paramref name="firstWord"/>, split where they reach a row's end.</summary>
    private static IEnumerable<BodyRun> Runs(int firstWord, ushort[] words)
    {
        for (int start = 0; start < words.Length;)
        {
            int word = firstWord + start, count = Math.Min(words.Length - start, PartTiles - word % PartTiles);
            yield return new BodyRun(word % PartTiles, word / PartTiles, words[start..(start + count)]);
            start += count;
        }
    }

    /// <summary><paramref name="words"/> with BG2 priority set, as Kraid shows his body once grown.</summary>
    private static ushort[] WithPriority(ushort[] words) => [.. words.Select(word => (ushort)(word | KraidBackgroundRomData.PriorityBit))];

    /// <summary>The failure for a boss whose installed artwork is missing.</summary>
    private static InvalidDataException Missing(BossBody body) => new($"The installed enemy artwork has no {body} body.");

    /// <summary>The first VRAM word of the character tile word <paramref name="word"/> names.</summary>
    [PpuLayoutBoundary("codec: the BG characters a boss body's tile words name, from the room tileset's character base")]
    private static int CharacterWord(ushort word) =>
        RoomGraphicsLayout.AreaCharacters.Offset + new SnesBgTilemapWord(word).CharacterIndex * RoomBlockPictureNames.CharacterWords;

    /// <summary>One part: the runs of words it writes, laid out at their tilemap positions.</summary>
    /// <param name="Runs">The runs.</param>
    private sealed record BodyPart(BodyRun[] Runs)
    {
        /// <summary>The first tilemap row a run writes.</summary>
        internal int Top => Runs.Min(run => run.Y);

        /// <summary>The tilemap rows from the first a run writes to the last.</summary>
        internal int Rows => Runs.Max(run => run.Y) - Top + 1;
    }

    /// <summary>A run of words written left to right from one tilemap position.</summary>
    /// <param name="X">The first word's column.</param>
    /// <param name="Y">The row.</param>
    /// <param name="Words">The words.</param>
    private sealed record BodyRun(int X, int Y, ushort[] Words);
}

/// <summary>A composed boss body: its sheet and where it pictures each tile word over each artwork.</summary>
/// <param name="Body">The boss.</param>
/// <param name="Tileset">The tileset of the boss's room.</param>
/// <param name="Sheet">The part images.</param>
/// <param name="Tiles">Every tile pictured.</param>
internal sealed record ComposedBossBody(BossBody Body, TilesetDefinition Tileset, ComposedSpriteSheet Sheet, IReadOnlyList<BossBodyTile> Tiles);
