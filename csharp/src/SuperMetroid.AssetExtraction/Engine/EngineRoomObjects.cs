using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The pictures of the things the old game draws into a room with the room's own blocks: door caps, gates, stations.
/// The installation keeps each as cells of blocks, so the same thing looks as the room's set of graphics draws those
/// blocks. A picture is therefore written once for every set of graphics, into a folder named for the set, and named
/// by one path that has the Godot game's placeholder for the room's set in it.
/// </summary>
internal sealed partial class EngineRoomObjects
{
    /// <summary>The width and height of a block, in pixels.</summary>
    internal const int Block = 16;

    /// <summary>What stands in a picture's path for the set of graphics a room is drawn with, as the Godot game reads it.</summary>
    internal const string GraphicsPlaceholder = "{graphics}";

    private readonly EngineAssetTree _tree;
    private readonly EngineSheets _sheets;
    private readonly string _content;
    private readonly IReadOnlyList<string> _graphicsSets;
    private readonly Dictionary<string, Dictionary<string, JsonArray>> _frames = new(StringComparer.Ordinal);

    /// <summary>The room objects' pictures of the installation at <paramref name="content"/>, for the sets of graphics <paramref name="graphicsSets"/>.</summary>
    internal EngineRoomObjects(EngineAssetTree tree, EngineSheets sheets, string content, IReadOnlyList<string> graphicsSets)
    {
        _tree = tree;
        _sheets = sheets;
        _content = content;
        _graphicsSets = graphicsSets;
    }

    /// <summary>The name of a set of graphics as the Godot game's files write it: a layout's "graphics" and the folder of its pictures.</summary>
    internal static string GraphicsName(string graphicsSet) => EngineSheets.FileName(graphicsSet);

    /// <summary>
    /// The pixels of one block of a set of graphics, mirrored as asked: the part of it behind a room's things and the
    /// part in front, as one picture. None for a block the set does not draw.
    /// </summary>
    internal static RgbaPixel[]? BlockPixels(EngineSheets sheets, string graphicsSet, int block, bool mirroredAcross, bool mirroredDown)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        RgbaPixel[]? behind = sheets.FramePixels(family, graphicsSet, $"block {block}", Block, Block);
        RgbaPixel[]? front = sheets.FramePixels(family, graphicsSet, $"block {block}^front", Block, Block);
        if (behind is null && front is null)
            return null;
        var pixels = new RgbaPixel[Block * Block];
        for (int y = 0; y < Block; y++)
        {
            for (int x = 0; x < Block; x++)
            {
                int from = (mirroredDown ? Block - 1 - y : y) * Block + (mirroredAcross ? Block - 1 - x : x);
                pixels[y * Block + x] = front is not null && front[from].A != 0 ? front[from] : behind is not null ? behind[from] : default;
            }
        }

        return pixels;
    }

    /// <summary>
    /// The region of each pixel of a named picture of a set of graphics, a block in size and mirrored as asked, as
    /// <see cref="BlockPixels"/> lays its part in front over its part behind. Null for a set whose sheet tells no
    /// regions apart, and for a picture the set does not draw.
    /// </summary>
    internal static int[]? NamedBlockRegions(EngineSheets sheets, string graphicsSet, string frame, bool mirroredAcross, bool mirroredDown)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        if (!sheets.HasRegions(family, graphicsSet))
            return null;
        RgbaPixel[]? behind = sheets.FramePixels(family, graphicsSet, frame, Block, Block);
        RgbaPixel[]? front = sheets.FramePixels(family, graphicsSet, $"{frame}^front", Block, Block);
        if (behind is null && front is null)
            return null;
        int[]? behindRegions = sheets.FrameRegions(family, graphicsSet, frame, Block, Block);
        int[]? frontRegions = sheets.FrameRegions(family, graphicsSet, $"{frame}^front", Block, Block);
        var regions = new int[Block * Block];
        for (int y = 0; y < Block; y++)
        {
            for (int x = 0; x < Block; x++)
            {
                int from = (mirroredDown ? Block - 1 - y : y) * Block + (mirroredAcross ? Block - 1 - x : x);
                regions[y * Block + x] = front is not null && front[from].A != 0 ? frontRegions![from] : behind is not null ? behindRegions![from] : 0;
            }
        }

        return regions;
    }

    /// <summary>
    /// The pixels of the part of a named picture of a set of graphics that lies in front of a room's things, a block
    /// in size and mirrored as asked. Null for a picture with no such part, or whose such part shows nothing.
    /// </summary>
    internal static RgbaPixel[]? NamedBlockFrontPixels(EngineSheets sheets, string graphicsSet, string frame, bool mirroredAcross, bool mirroredDown)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        RgbaPixel[]? front = sheets.FramePixels(family, graphicsSet, $"{frame}^front", Block, Block);
        if (front is null || Array.TrueForAll(front, pixel => pixel.A == 0))
            return null;
        var pixels = new RgbaPixel[Block * Block];
        for (int y = 0; y < Block; y++)
        {
            for (int x = 0; x < Block; x++)
                pixels[y * Block + x] = front[(mirroredDown ? Block - 1 - y : y) * Block + (mirroredAcross ? Block - 1 - x : x)];
        }

        return pixels;
    }

    /// <summary>
    /// The region of each pixel of the part <see cref="NamedBlockFrontPixels"/> gives. Null for a set whose sheet
    /// tells no regions apart, and for a picture with no such part.
    /// </summary>
    internal static int[]? NamedBlockFrontRegions(EngineSheets sheets, string graphicsSet, string frame, bool mirroredAcross, bool mirroredDown)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        if (!sheets.HasRegions(family, graphicsSet) || sheets.FrameRegions(family, graphicsSet, $"{frame}^front", Block, Block) is not { } front)
            return null;
        var regions = new int[Block * Block];
        for (int y = 0; y < Block; y++)
        {
            for (int x = 0; x < Block; x++)
                regions[y * Block + x] = front[(mirroredDown ? Block - 1 - y : y) * Block + (mirroredAcross ? Block - 1 - x : x)];
        }

        return regions;
    }

    /// <summary>The name on a set's sheet of the picture of the block numbered <paramref name="block"/>.</summary>
    internal static string BlockFrame(int block) => $"block {block}";

    /// <summary>
    /// Writes one named picture of the sets of graphics, a block in size, for every set that has it, under
    /// <paramref name="folder"/>, and returns the one path that names them all. Not every set has every such picture:
    /// an item's own is only in the sets of the rooms that hold the item.
    /// </summary>
    /// <param name="picture">The picture's name on the sets' sheets.</param>
    /// <param name="folder">The folder of the asset tree its pictures go under.</param>
    /// <param name="name">The name of the picture's file.</param>
    /// <exception cref="InvalidDataException">No set of graphics has the picture.</exception>
    internal string SheetPicture(string picture, string folder, string name)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        name = EngineSheets.FileName(name);
        bool written = false;
        foreach (string graphicsSet in _graphicsSets)
        {
            RgbaPixel[]? behind = _sheets.FramePixels(family, graphicsSet, picture, Block, Block);
            RgbaPixel[]? front = _sheets.FramePixels(family, graphicsSet, $"{picture}^front", Block, Block);
            if (behind is null && front is null)
                continue;
            var pixels = new RgbaPixel[Block * Block];
            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = front is not null && front[index].A != 0 ? front[index] : behind is not null ? behind[index] : default;
            string path = $"{folder}/{GraphicsName(graphicsSet)}/{name}.png";
            if (!_tree.Holds(path))
                _tree.WritePicture(path, Block, Block, pixels);
            written = true;
        }

        return written
            ? $"{folder}/{GraphicsPlaceholder}/{name}.png"
            : throw new InvalidDataException($"No set of graphics has the picture {picture}.");
    }

    /// <summary>The cells of the frame named <paramref name="frame"/> among those the installation keeps under <paramref name="directory"/>.</summary>
    /// <exception cref="InvalidDataException">The installation keeps no such frame there.</exception>
    private JsonArray Cells(string directory, string frame)
    {
        if (!_frames.TryGetValue(directory, out Dictionary<string, JsonArray>? frames))
        {
            frames = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_content, directory), "*.json").Order(StringComparer.Ordinal))
            {
                if (JsonNode.Parse(File.ReadAllText(file))?["frames"] is not JsonArray listed)
                    continue;
                foreach (JsonNode? one in listed)
                    frames[one!["id"]!.GetValue<string>()] = one["cells"]!.AsArray();
            }

            _frames[directory] = frames;
        }

        return frames.TryGetValue(frame, out JsonArray? cells)
            ? cells
            : throw new InvalidDataException($"The installation keeps no room object frame {frame} under {directory}.");
    }

    /// <summary>
    /// The blocks a frame covers, counted from the block its object is placed on: the leftmost and topmost, and how
    /// many across and down. Only the cells <paramref name="wanted"/> picks out are counted, where it is given.
    /// </summary>
    internal (int Left, int Top, int Columns, int Rows) Bounds(string directory, string frame, Func<int, int, bool>? wanted = null)
    {
        var at = Cells(directory, frame).Select(cell => (X: cell!["x"]!.GetValue<int>(), Y: cell["y"]!.GetValue<int>()))
            .Where(cell => wanted is null || wanted(cell.X, cell.Y)).ToList();
        if (at.Count == 0)
            throw new InvalidDataException($"The room object frame {frame} has none of the cells asked for.");
        int left = at.Min(cell => cell.X), top = at.Min(cell => cell.Y);
        return (left, top, at.Max(cell => cell.X) - left + 1, at.Max(cell => cell.Y) - top + 1);
    }

    /// <summary>
    /// Writes the picture of one frame for every set of graphics, under <paramref name="folder"/>, and returns the one
    /// path that names them all. The picture's top-left corner is that of the frame's leftmost and topmost cell; a
    /// cell with no block shows nothing, and so does a block a set of graphics does not draw.
    /// </summary>
    /// <param name="directory">Where the installation keeps the frame.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="folder">The folder of the asset tree its pictures go under.</param>
    /// <param name="wanted">Which of its cells to draw, by how far across and down from the object's block; all of them when not given.</param>
    /// <param name="name">The picture's name, where it is not the frame's.</param>
    /// <param name="over">Something drawn over it: its pixels, their width and height, and where their corner lies in the picture.</param>
    /// <param name="rows">How many blocks tall to make the picture at least, where that is more than its cells are.</param>
    internal string Picture(
        string directory, string frame, string folder, Func<int, int, bool>? wanted = null, string? name = null,
        (RgbaPixel[] Pixels, int Width, int Height, int Left, int Top)? over = null, int rows = 0)
    {
        JsonArray cells = Cells(directory, frame);
        (int firstColumn, int firstRow, int columns, int rowsOfCells) = Bounds(directory, frame, wanted);
        rows = Math.Max(rows, rowsOfCells);
        name = EngineSheets.FileName(name ?? frame);
        foreach (string graphicsSet in _graphicsSets)
        {
            string path = $"{folder}/{GraphicsName(graphicsSet)}/{name}.png";
            if (_tree.Holds(path))
                continue;
            var canvas = new RgbaPixel[columns * Block * rows * Block];

            // The part of its blocks that the cartridge draws in front of what stands in the room is written beside
            // the picture, as a tile's is, for the Godot game to draw there.
            var front = new RgbaPixel[canvas.Length];
            bool hasFront = false;
            foreach (JsonNode? cell in cells)
            {
                if (wanted is not null && !wanted(cell!["x"]!.GetValue<int>(), cell["y"]!.GetValue<int>()))
                    continue;
                if (cell!["block"]?.GetValue<int>() is not int block)
                    continue;
                string mirroring = cell["mirroring"]!.GetValue<string>();
                bool across = mirroring is "Horizontal" or "Both", down = mirroring is "Vertical" or "Both";
                if (mirroring is not ("None" or "Horizontal" or "Vertical" or "Both"))
                    throw new InvalidDataException($"The room object frame {frame} mirrors a block {mirroring}, which this does not know.");
                if (BlockPixels(_sheets, graphicsSet, block, across, down) is not { } pixels)
                    continue;
                int left = (cell["x"]!.GetValue<int>() - firstColumn) * Block, top = (cell["y"]!.GetValue<int>() - firstRow) * Block;
                for (int y = 0; y < Block; y++)
                    pixels.AsSpan(y * Block, Block).CopyTo(canvas.AsSpan((top + y) * columns * Block + left, Block));
                if (NamedBlockFrontPixels(_sheets, graphicsSet, BlockFrame(block), across, down) is { } inFront)
                {
                    for (int y = 0; y < Block; y++)
                        inFront.AsSpan(y * Block, Block).CopyTo(front.AsSpan((top + y) * columns * Block + left, Block));
                    hasFront = true;
                }
            }

            if (over is { } drawn)
            {
                if (drawn.Left < 0 || drawn.Top < 0 || drawn.Left + drawn.Width > columns * Block || drawn.Top + drawn.Height > rows * Block)
                    throw new InvalidDataException($"What is drawn over the room object frame {frame} does not lie inside it.");
                for (int y = 0; y < drawn.Height; y++)
                {
                    for (int x = 0; x < drawn.Width; x++)
                    {
                        RgbaPixel pixel = drawn.Pixels[y * drawn.Width + x];
                        if (pixel.A == 0)
                            continue;
                        canvas[(drawn.Top + y) * columns * Block + drawn.Left + x] = pixel;

                        // What is drawn over the blocks is over their front part too.
                        front[(drawn.Top + y) * columns * Block + drawn.Left + x] = default;
                    }
                }
            }

            _tree.WritePicture(path, columns * Block, rows * Block, canvas);
            if (hasFront && front.Any(pixel => pixel.A != 0))
                _tree.WritePicture(EngineAssetTree.FrontPath(path), columns * Block, rows * Block, front);
        }

        return $"{folder}/{GraphicsPlaceholder}/{name}.png";
    }
}
