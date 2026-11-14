using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Paints one boss's body frames over the characters its room shows (#1466, #1515, #1540): each frame as one whole picture
/// over the characters its room shows while the frame is shown, placed where its cells lie on the body's area. No piece of
/// a frame is pictured apart from it (#1513). The cartridge's layer cells are read here, once, and nowhere else.
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

    /// <summary>
    /// Adds <paramref name="frame"/>'s one whole picture: each cell over the characters the room shows there while the frame
    /// is shown, a cell a room animation paints over the frame <see cref="BossBodyFrameSource.Selected"/> names for that
    /// animation, and over the room's load where it names none (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The frame repeats a name, writes nothing, reads a character its room rebinds after loading or shows nothing at,
    /// a cell shows two animations, or the frame is pictured under a frame its room's animation does not paint.
    /// </exception>
    internal void Add(BossBodyFrameSource frame)
    {
        Ensure.NotNull(frame);
        if (frame.Cells.Count == 0)
            throw new InvalidDataException($"{owner}'s body frame {frame.Name} writes nothing.");
        foreach (BackgroundTile tile in frame.Cells.Values)
        {
            if (room.Rebound.Any(upload => upload.Covers(BgCharacterSet.Tiles, tile.Tile)))
                throw new InvalidDataException(
                    $"{owner}'s body frame {frame.Name} reads character {tile.Tile}, which its room rebinds after loading.");
        }
        foreach (ArtworkFrame selected in frame.Selected.Values)
        {
            if (!room.Animated.Any(placement => placement.Frame == selected))
                throw new InvalidDataException($"{owner}'s body frame {frame.Name} is pictured under {selected.Animation.Name} {selected.Name}, which its room does not paint.");
        }
        // A frame whose every cell's character inks only colour zero shows nothing: it has no picture, and the part showing
        // it is not drawn (#1513, #1540).
        bool shows = ShowsAnything(frame);
        string? picture = shows ? AddPicture(frame) : null;
        BossBodyDepth? depth = shows ? DepthOf(frame) : null;
        // A frame lying lower on the body's area than its cells carries those rows in where its picture lies (#1802).
        Position16<BackgroundPixels> corner = CornerOf(frame.Cells) + new Offset16<BackgroundPixels>(0, frame.LoweredPixels);
        if (!frames.TryAdd(frame.Name, new ComposedBossBodyFrame(frame.Name, picture, corner, depth) { Dissolving = frame.Dissolving }))
            throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
    }

    /// <summary>
    /// The depth <paramref name="frame"/> is drawn at whole (#1634): in front of the sprites where every cell inking a colour
    /// other than colour zero is written in its layer's upper tier, and behind them where none is. The tier is read here,
    /// once; the installed picture carries none.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame's inking cells lie in both tiers, so it cannot be drawn whole at one depth.</exception>
    private BossBodyDepth DepthOf(BossBodyFrameSource frame)
    {
        bool upper = false, lower = false;
        foreach (BackgroundTile tile in frame.Cells.Values)
        {
            if (Character(tile.Tile, ShownUnder(frame, tile.Tile)).Span.IndexOfAnyExcept((byte)0) < 0)
                continue;
            upper |= tile.HighPriority;
            lower |= !tile.HighPriority;
        }
        return upper && lower
            ? throw new InvalidDataException($"{owner}'s body frame {frame.Name} is written partly in front of the sprites and partly behind them.")
            : upper ? BossBodyDepth.InFrontOfSprites : BossBodyDepth.BehindSprites;
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
    internal static Position16<BackgroundPixels> CornerOf(SortedDictionary<int, BackgroundTile> cells)
    {
        Position16<BackgroundPixels>[] corners = [.. cells.Keys.Select(PixelOf)];
        return new Position16<BackgroundPixels>(corners.Min(corner => corner.X), corners.Min(corner => corner.Y));
    }

    /// <summary>
    /// Whether any of <paramref name="frame"/>'s cells' characters, as the room shows them while the frame is shown, inks a
    /// colour other than colour zero.
    /// </summary>
    private bool ShowsAnything(BossBodyFrameSource frame)
    {
        foreach (BackgroundTile tile in frame.Cells.Values)
        {
            if (Character(tile.Tile, ShownUnder(frame, tile.Tile)).Span.IndexOfAnyExcept((byte)0) >= 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds <paramref name="frame"/>'s whole picture, named for the frame, each cell over the characters the room shows there
    /// while the frame is shown, bounded by the cells' rectangle; returns its name.
    /// </summary>
    private string AddPicture(BossBodyFrameSource frame)
    {
        const int cell = RoomTileCanvas.CharacterPixels;
        Position16<BackgroundPixels> corner = CornerOf(frame.Cells);
        var painted = new List<BossBodyPaintedCell>(frame.Cells.Count);
        foreach ((int layerCell, BackgroundTile tile) in frame.Cells)
        {
            Position16<BackgroundPixels> at = PixelOf(layerCell);
            painted.Add(new BossBodyPaintedCell((at.X - corner.X) / cell, (at.Y - corner.Y) / cell, tile,
                Character(tile.Tile, ShownUnder(frame, tile.Tile))));
        }
        pictures.Add(new BossBodyPicture(frame.Name, painted.Max(entry => entry.Column) + 1, painted.Max(entry => entry.Row) + 1, painted));
        return frame.Name;
    }

    /// <summary>
    /// The frame of the room animation painting tile character <paramref name="character"/> that <paramref name="frame"/>
    /// is shown under, or null for a still character or an animation the frame names no frame of, which shows the room's load.
    /// </summary>
    private ArtworkFrame? ShownUnder(BossBodyFrameSource frame, int character) =>
        AnimationOf(character) is { } animation && frame.Selected.TryGetValue(animation, out ArtworkFrame selected) ? selected : null;

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
