using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Paints one boss's body frames over the characters its room shows (#1466): each frame's still cells as one part and the
/// cells of each animation they show as another, pictured over the room's load and again over each frame of the animation.
/// </summary>
/// <param name="owner">The boss.</param>
/// <param name="room">What the boss's room shows at its characters.</param>
/// <param name="colours">The room's standard palette.</param>
internal sealed class BossBodyPainter(BossBodyOwner owner, BossRoomCharacters room, RgbColor[] colours)
{
    /// <summary>The name of the part a boss clears its layer with.</summary>
    internal const string BlankPartName = "Blank";

    /// <summary>The bytes of every artwork a cell can show, by identity.</summary>
    private readonly Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = Ensure.NotNull(room).Loaded
        .Concat(room.Animated.Select(frame => frame.Placement))
        .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);

    /// <summary>The pictures to paint, in the order they were added.</summary>
    private readonly List<BossBodyPicture> pictures = [];

    /// <summary>Each frame's parts, by frame name.</summary>
    private readonly Dictionary<string, IReadOnlyList<ComposedBossBodyPart>> frames = new(StringComparer.Ordinal);

    /// <summary>Adds <paramref name="frame"/>'s parts and pictures.</summary>
    /// <exception cref="InvalidDataException">
    /// The frame repeats a name, writes nothing, reads a character its room rebinds after loading or shows nothing at,
    /// or a cell shows two animations.
    /// </exception>
    internal void Add(BossBodyFrameSource frame)
    {
        Ensure.NotNull(frame);
        if (frame.Cells.Count == 0)
            throw new InvalidDataException($"{owner}'s body frame {frame.Name} writes nothing.");
        var still = new SortedDictionary<int, BackgroundTile>();
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
            else
            {
                still[cell] = tile;
            }
        }
        List<ComposedBossBodyPart> parts = [];
        if (still.Count > 0)
            parts.Add(Part(frame.Name, still, null));
        foreach ((string animation, SortedDictionary<int, BackgroundTile> cells) in animated)
            parts.Add(Part($"{frame.Name}~{animation}", cells, new ArtworkAnimation(animation)));
        if (!frames.TryAdd(frame.Name, parts))
            throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
    }

    /// <summary>
    /// The sheet of every frame added and of <paramref name="blank"/>, the boss's blank cell. Pictures that paint the same
    /// cells with the same characters share one place, and the places are laid out in as many full-width columns as the
    /// largest sheet holds.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The blank reads a character its room rebinds after loading or animates, or the pictures do not fit the largest sheet.
    /// </exception>
    internal ComposedBossBodySheet Compose(BackgroundTile blank)
    {
        if (room.Rebound.Any(upload => upload.Covers(BgCharacterSet.Tiles, blank.Tile)) || AnimationOf(blank.Tile) is not null)
            throw new InvalidDataException($"{owner}'s blank reads character {blank.Tile}, which its room rebinds or animates.");
        pictures.Add(new BossBodyPicture(BlankPartName, 1, 1, [new BossBodyPaintedCell(0, 0, blank, Character(blank.Tile, null))]));
        var blankPart = new ComposedBossBodyPart(BlankPartName, 0, BossBodyFootprint.OfCells(0, 1), null,
            new Dictionary<string, string>(StringComparer.Ordinal));

        // Pictures that paint the same cells with the same characters share the first one's place.
        var placeOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var places = new List<BossBodyPicture>();
        var placeIndex = new int[pictures.Count];
        for (int index = 0; index < pictures.Count; index++)
        {
            string content = ContentKey(pictures[index]);
            if (!placeOf.TryGetValue(content, out int place))
            {
                place = places.Count;
                placeOf.Add(content, place);
                places.Add(pictures[index]);
            }
            placeIndex[index] = place;
        }

        // Each place goes to the column with the fewest rows so far.
        int columnPixels = BossBodyDefinitions.ColumnCount * RoomTileCanvas.CharacterPixels;
        int across = Math.Min(RgbaPng.MaximumDimension / columnPixels, places.Count);
        int maximumRows = RgbaPng.MaximumDimension / RoomTileCanvas.CharacterPixels;
        var used = new int[across];
        var corners = new Position16<ImagePixels>[places.Count];
        for (int place = 0; place < places.Count; place++)
        {
            int column = Array.IndexOf(used, used.Min());
            if (used[column] + places[place].Rows > maximumRows)
                throw new InvalidDataException(
                    $"{owner}'s body pictures need more than one {RgbaPng.MaximumDimension}-pixel sheet.");
            corners[place] = new Position16<ImagePixels>((ushort)(column * columnPixels), (ushort)(used[column] * RoomTileCanvas.CharacterPixels));
            used[column] += places[place].Rows;
        }
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>((ushort)(across * columnPixels), (ushort)(used.Max() * RoomTileCanvas.CharacterPixels)));
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
                Size = new Extent16<ImagePixels>((ushort)(picture.Columns * RoomTileCanvas.CharacterPixels), (ushort)(picture.Rows * RoomTileCanvas.CharacterPixels)),
                Origin = default,
                ColourGroups = coloured[place].ManifestMembers,
                Parts = [],
            });
        }
        return new ComposedBossBodySheet(owner, canvas.ToRoomSheet(colours, documents), blankPart, frames);
    }

    /// <summary>What <paramref name="picture"/> paints: its size and each cell's place, tile and character bytes.</summary>
    private static string ContentKey(BossBodyPicture picture)
    {
        var key = new System.Text.StringBuilder();
        key.Append(picture.Columns).Append('x').Append(picture.Rows);
        foreach (BossBodyPaintedCell cell in picture.Cells)
        {
            key.Append('|').Append(cell.Column).Append(',').Append(cell.Row).Append(',').Append(cell.Tile).Append(',')
                .Append(Convert.ToHexString(cell.Character.Span));
        }
        return key.ToString();
    }

    /// <summary>
    /// The part named <paramref name="name"/> covering <paramref name="cells"/>: its picture over the room's load and, for
    /// an animated part, a picture over each frame of <paramref name="animation"/>.
    /// </summary>
    private ComposedBossBodyPart Part(string name, SortedDictionary<int, BackgroundTile> cells, ArtworkAnimation? animation)
    {
        int top = cells.Keys.Min() / BossBodyDefinitions.ColumnCount;
        int rows = cells.Keys.Max() / BossBodyDefinitions.ColumnCount - top + 1;
        var masks = new uint[rows];
        foreach (int cell in cells.Keys)
            masks[cell / BossBodyDefinitions.ColumnCount - top] |= 1u << (cell % BossBodyDefinitions.ColumnCount);
        AddPicture(name, top, rows, cells, null);
        var animationPictures = new Dictionary<string, string>(StringComparer.Ordinal);
        if (animation is { } shown)
        {
            foreach (ArtworkFrame frame in room.Animated.Select(placement => placement.Frame).Where(frame => frame.Animation == shown).Distinct())
            {
                string pictureName = $"{name}~{frame.Name}";
                AddPicture(pictureName, top, rows, cells, frame);
                animationPictures.Add(frame.Name, pictureName);
            }
        }
        return new ComposedBossBodyPart(name, top, new BossBodyFootprint(top, ImmutableArray.Create(masks)), animation, animationPictures);
    }

    /// <summary>Adds the picture of <paramref name="cells"/> from row <paramref name="top"/>, over <paramref name="frame"/> where it paints.</summary>
    private void AddPicture(string name, int top, int rows, SortedDictionary<int, BackgroundTile> cells, ArtworkFrame? frame) =>
        pictures.Add(new BossBodyPicture(name, BossBodyDefinitions.ColumnCount, rows, [.. cells.Select(entry => new BossBodyPaintedCell(
            entry.Key % BossBodyDefinitions.ColumnCount, entry.Key / BossBodyDefinitions.ColumnCount - top, entry.Value, Character(entry.Value.Tile, frame)))]));

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
