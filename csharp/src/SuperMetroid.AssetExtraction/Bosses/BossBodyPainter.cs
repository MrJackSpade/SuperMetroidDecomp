using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Paints one boss's body frames over the characters its room shows (#1466, #1515): each frame as one whole picture over
/// the room's load, placed where its cells lie on the body's area, with the rectangles it covers, and the cells of each
/// animation they show pictured again over each frame of the animation. The cartridge's layer cells are read here, once,
/// and nowhere else.
/// </summary>
/// <param name="owner">The boss.</param>
/// <param name="room">What the boss's room shows at its characters.</param>
/// <param name="colours">The room's standard palette.</param>
internal sealed class BossBodyPainter(BossBodyOwner owner, BossRoomCharacters room, RgbColor[] colours)
{
    /// <summary>The bytes of every artwork a cell can show, by identity.</summary>
    private readonly Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = Ensure.NotNull(room).Loaded
        .Concat(room.Animated.Select(frame => frame.Placement))
        .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);

    /// <summary>The pictures to paint, in the order they were added.</summary>
    private readonly List<BossBodyPicture> pictures = [];

    /// <summary>Each frame, by name.</summary>
    private readonly Dictionary<string, ComposedBossBodyFrame> frames = new(StringComparer.Ordinal);

    /// <summary>Adds <paramref name="frame"/>'s whole picture and its animations' pictures.</summary>
    /// <exception cref="InvalidDataException">
    /// The frame repeats a name, writes nothing, reads a character its room rebinds after loading or shows nothing at,
    /// or a cell shows two animations.
    /// </exception>
    internal void Add(BossBodyFrameSource frame)
    {
        Ensure.NotNull(frame);
        if (frame.Cells.Count == 0)
            throw new InvalidDataException($"{owner}'s body frame {frame.Name} writes nothing.");
        var animated = new SortedDictionary<string, SortedDictionary<int, BackgroundTile>>(StringComparer.Ordinal);
        foreach ((int cell, BackgroundTile tile) in frame.Cells)
        {
            if (room.Rebound.Any(upload => upload.Covers(BgCharacterSet.Tiles, tile.Tile)))
                throw new InvalidDataException(
                    $"{owner}'s body frame {frame.Name} reads character {tile.Tile}, which its room rebinds after loading.");
            if (AnimationOf(tile.Tile) is { } animation)
            {
                if (!animated.TryGetValue(animation.Name, out SortedDictionary<int, BackgroundTile>? cells))
                    animated.Add(animation.Name, cells = []);
                cells[cell] = tile;
            }
        }
        // A frame whose every cell's character inks only colour zero shows nothing: it has no picture and covers what lies
        // beneath it (#1513), as Crocomire's second melt image covers the rows below his first.
        string? picture = ShowsAnything(frame.Cells) ? AddPicture(frame.Name, frame.Cells, null) : null;
        var overlays = new List<ComposedBossBodyOverlay>();
        foreach ((string animation, SortedDictionary<int, BackgroundTile> cells) in animated)
        {
            var shown = new ArtworkAnimation(animation);
            var named = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (ArtworkFrame step in room.Animated.Select(placement => placement.Frame).Where(candidate => candidate.Animation == shown).Distinct())
                named.Add(step.Name, AddPicture($"{frame.Name}~{animation}~{step.Name}", cells, step));
            overlays.Add(new ComposedBossBodyOverlay(shown, CornerOf(cells), CoversOf(cells), named));
        }
        if (!frames.TryAdd(frame.Name, new ComposedBossBodyFrame(frame.Name, picture, CornerOf(frame.Cells), CoversOf(frame.Cells), overlays)))
            throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
    }

    /// <summary>
    /// The sheet of every frame added. <paramref name="blank"/>, the tile the boss's cleared layer holds, must ink only colour
    /// zero: a body shows nothing where no frame covers it. Pictures that paint the same cells with the same characters
    /// share one place, and the places are laid out in as many columns as the largest sheet holds.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The blank shows anything, reads a character its room rebinds after loading or animates, or the pictures do not fit
    /// the largest sheet.
    /// </exception>
    internal ComposedBossBodySheet Compose(BackgroundTile blank)
    {
        if (room.Rebound.Any(upload => upload.Covers(BgCharacterSet.Tiles, blank.Tile)) || AnimationOf(blank.Tile) is not null)
            throw new InvalidDataException($"{owner}'s blank reads character {blank.Tile}, which its room rebinds or animates.");
        if (Character(blank.Tile, null).Span.IndexOfAnyExcept((byte)0) >= 0)
            throw new InvalidDataException($"{owner}'s blank character {blank.Tile} shows something, but a body shows nothing where no frame covers it.");

        // Pictures that paint the same cells with the same characters share the first one's place.
        var placeOf = new Dictionary<BossBodyPicture, int>(BossBodyPictureContent.Instance);
        var places = new List<BossBodyPicture>();
        var placeIndex = new int[pictures.Count];
        for (int index = 0; index < pictures.Count; index++)
        {
            if (!placeOf.TryGetValue(pictures[index], out int place))
            {
                place = places.Count;
                placeOf.Add(pictures[index], place);
                places.Add(pictures[index]);
            }
            placeIndex[index] = place;
        }

        // Each place goes to the column with the fewest rows so far; a column is as wide as the widest picture.
        int columnPixels = places.Count == 0 ? RoomTileCanvas.CharacterPixels : places.Max(place => place.Columns) * RoomTileCanvas.CharacterPixels;
        int across = Math.Max(1, Math.Min(RgbaPng.MaximumDimension / columnPixels, places.Count));
        int maximumRows = RgbaPng.MaximumDimension / RoomTileCanvas.CharacterPixels;
        var used = new int[across];
        var corners = new Position16<ImagePixels>[places.Count];
        for (int place = 0; place < places.Count; place++)
        {
            int column = Array.IndexOf(used, used.Min());
            if (used[column] + places[place].Rows > maximumRows)
                throw new InvalidDataException(
                    $"{owner}'s body pictures need more than one {RgbaPng.MaximumDimension}-pixel sheet.");
            corners[place] = new Position16<ImagePixels>(column * columnPixels, used[column] * RoomTileCanvas.CharacterPixels);
            used[column] += places[place].Rows;
        }
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(across * columnPixels, Math.Max(1, used.Max()) * RoomTileCanvas.CharacterPixels));
        var coloured = new ImageColourGroups[places.Count];
        for (int place = 0; place < places.Count; place++)
        {
            foreach (BossBodyPaintedCell cell in places[place].Cells)
            {
                coloured[place] = coloured[place].Union(canvas.PaintTile(corners[place].X + cell.Column * RoomTileCanvas.CharacterPixels,
                    corners[place].Y + cell.Row * RoomTileCanvas.CharacterPixels, cell.Tile, cell.Character.Span, colours));
            }
        }
        var documents = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < pictures.Count; index++)
        {
            BossBodyPicture picture = pictures[index];
            int place = placeIndex[index];
            documents.Add(picture.Name, new SpriteSheetFrameDocument
            {
                Corner = corners[place],
                Size = new Extent16<ImagePixels>(picture.Columns * RoomTileCanvas.CharacterPixels, picture.Rows * RoomTileCanvas.CharacterPixels),
                Origin = default,
                ColourGroups = coloured[place].ManifestMembers,
            });
        }
        return new ComposedBossBodySheet(owner, canvas.ToRoomSheet(colours, documents), frames);
    }

    /// <summary>
    /// The body pixel layer cell <paramref name="cell"/>'s top-left corner lies on: a layer's screens of cells fill the body's
    /// area left to right and then downward.
    /// </summary>
    internal static Position16<BackgroundPixels> PixelOf(int cell)
    {
        int row = cell / BossBodySourceDefinitions.LayerColumns, column = cell % BossBodySourceDefinitions.LayerColumns;
        int screen = row / BossBodySourceDefinitions.ScreenRows;
        int screenPixels = BossBodySourceDefinitions.ScreenRows * RoomTileCanvas.CharacterPixels;
        return new Position16<BackgroundPixels>(
            screen % BossBodySourceDefinitions.ScreensAcross * screenPixels + column * RoomTileCanvas.CharacterPixels,
            screen / BossBodySourceDefinitions.ScreensAcross * screenPixels + row % BossBodySourceDefinitions.ScreenRows * RoomTileCanvas.CharacterPixels);
    }

    /// <summary>The body pixel the top-left corner of the rectangle bounding <paramref name="cells"/> lies on.</summary>
    private static Position16<BackgroundPixels> CornerOf(SortedDictionary<int, BackgroundTile> cells)
    {
        Position16<BackgroundPixels>[] corners = [.. cells.Keys.Select(PixelOf)];
        return new Position16<BackgroundPixels>(corners.Min(corner => corner.X), corners.Min(corner => corner.Y));
    }

    /// <summary>
    /// The rectangles <paramref name="cells"/> cover on the body: each run of cells across a row of pixels, joined with the
    /// same run of the rows below it.
    /// </summary>
    private static ImmutableArray<BossBodyRectangle> CoversOf(SortedDictionary<int, BackgroundTile> cells)
    {
        const int cell = RoomTileCanvas.CharacterPixels;
        var rows = cells.Keys.Select(PixelOf).GroupBy(corner => corner.Y).OrderBy(group => group.Key);
        var covers = ImmutableArray.CreateBuilder<BossBodyRectangle>();
        var open = new List<BossBodyRectangle>();
        int previousTop = -1;
        foreach (IGrouping<ushort, Position16<BackgroundPixels>> row in rows)
        {
            // A row not directly below the last ends every rectangle still open.
            if (row.Key != previousTop + cell)
            {
                covers.AddRange(open);
                open.Clear();
            }
            var continuing = new List<BossBodyRectangle>();
            int[] lefts = [.. row.Select(corner => (int)corner.X).Order()];
            int index = 0;
            while (index < lefts.Length)
            {
                int left = lefts[index], right = left + cell;
                index++;
                while (index < lefts.Length && lefts[index] == right)
                {
                    right += cell;
                    index++;
                }
                int match = open.FindIndex(rectangle => rectangle.Left == left && rectangle.Right == right);
                if (match >= 0)
                {
                    BossBodyRectangle grown = open[match];
                    open.RemoveAt(match);
                    continuing.Add(grown with { Size = grown.Size.WithY(grown.Size.Y + cell) });
                }
                else
                {
                    continuing.Add(BossBodyRectangle.Between(left, row.Key, right, row.Key + cell));
                }
            }
            covers.AddRange(open);
            open = continuing;
            previousTop = row.Key;
        }
        covers.AddRange(open);
        return covers.ToImmutable();
    }

    /// <summary>Whether any of <paramref name="cells"/>' characters, as the room loads them, inks a colour other than colour zero.</summary>
    private bool ShowsAnything(SortedDictionary<int, BackgroundTile> cells)
    {
        foreach (BackgroundTile tile in cells.Values)
        {
            if (Character(tile.Tile, null).Span.IndexOfAnyExcept((byte)0) >= 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds the picture named <paramref name="name"/> of <paramref name="cells"/>, over <paramref name="frame"/> where it
    /// paints, bounded by the cells' rectangle; returns its name.
    /// </summary>
    private string AddPicture(string name, SortedDictionary<int, BackgroundTile> cells, ArtworkFrame? frame)
    {
        const int cell = RoomTileCanvas.CharacterPixels;
        Position16<BackgroundPixels> corner = CornerOf(cells);
        var painted = new List<BossBodyPaintedCell>(cells.Count);
        foreach ((int layerCell, BackgroundTile tile) in cells)
        {
            Position16<BackgroundPixels> at = PixelOf(layerCell);
            painted.Add(new BossBodyPaintedCell((at.X - corner.X) / cell, (at.Y - corner.Y) / cell, tile, Character(tile.Tile, frame)));
        }
        pictures.Add(new BossBodyPicture(name, painted.Max(entry => entry.Column) + 1, painted.Max(entry => entry.Row) + 1, painted));
        return name;
    }

    /// <summary>
    /// The bytes of tile character <paramref name="character"/> as it shows over <paramref name="frame"/> where the frame
    /// paints it, and otherwise over the room's load.
    /// </summary>
    /// <exception cref="InvalidDataException">The room shows nothing at the character.</exception>
    private ReadOnlyMemory<byte> Character(int character, ArtworkFrame? frame)
    {
        AnimatedCharacterPlacement? painted = frame is { } selected
            ? room.Animated.FirstOrDefault(placement => placement.Frame == selected && placement.Placement.Covers(BgCharacterSet.Tiles, character))
            : null;
        BgArtworkShown shown;
        if (painted is not null)
            shown = painted.Placement.ShownAt(character);
        else if (!room.Content.TryShowing(BgCharacterSet.Tiles, character, 1, out shown))
            throw new InvalidDataException($"{owner}'s body reads character {character}, which its room loads nothing over.");
        ReadOnlyMemory<byte> artwork = bytes.TryGetValue(shown.Artwork, out ReadOnlyMemory<byte> found) ? found
            : throw new InvalidDataException($"{owner}'s body shows {shown.Artwork.Name}, whose characters are not installed.");
        return artwork.Slice(shown.WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes);
    }

    /// <summary>The animation painting tile character <paramref name="character"/> in the room, or null for a still one.</summary>
    /// <exception cref="InvalidDataException">Two animations paint the character.</exception>
    private ArtworkAnimation? AnimationOf(int character)
    {
        ArtworkAnimation[] animations = [.. room.Animated.Where(placement => placement.Placement.Covers(BgCharacterSet.Tiles, character))
            .Select(placement => placement.Frame.Animation).Distinct()];
        return animations.Length switch
        {
            0 => null,
            1 => animations[0],
            _ => throw new InvalidDataException($"{owner}'s body reads character {character}, which {animations.Length} animations paint."),
        };
    }
}
