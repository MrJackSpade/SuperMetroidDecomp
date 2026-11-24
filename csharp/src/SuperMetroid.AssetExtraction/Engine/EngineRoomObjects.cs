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

    /// <summary>How many blocks wide and tall a frame is.</summary>
    internal (int Columns, int Rows) Size(string directory, string frame)
    {
        JsonArray cells = Cells(directory, frame);
        return (cells.Max(cell => cell!["x"]!.GetValue<int>()) + 1, cells.Max(cell => cell!["y"]!.GetValue<int>()) + 1);
    }

    /// <summary>
    /// Writes the picture of one frame for every set of graphics, under <paramref name="folder"/>, and returns the one
    /// path that names them all. The picture's top-left corner is the frame's first cell's; a cell with no block shows
    /// nothing, and so does a block a set of graphics does not draw.
    /// </summary>
    internal string Picture(string directory, string frame, string folder)
    {
        JsonArray cells = Cells(directory, frame);
        (int columns, int rows) = Size(directory, frame);
        string name = EngineSheets.FileName(frame);
        foreach (string graphicsSet in _graphicsSets)
        {
            string path = $"{folder}/{GraphicsName(graphicsSet)}/{name}.png";
            if (_tree.Holds(path))
                continue;
            var canvas = new RgbaPixel[columns * Block * rows * Block];
            foreach (JsonNode? cell in cells)
            {
                if (cell!["block"]?.GetValue<int>() is not int block)
                    continue;
                string mirroring = cell["mirroring"]!.GetValue<string>();
                bool across = mirroring is "Horizontal" or "Both", down = mirroring is "Vertical" or "Both";
                if (mirroring is not ("None" or "Horizontal" or "Vertical" or "Both"))
                    throw new InvalidDataException($"The room object frame {frame} mirrors a block {mirroring}, which this does not know.");
                if (BlockPixels(_sheets, graphicsSet, block, across, down) is not { } pixels)
                    continue;
                int left = cell["x"]!.GetValue<int>() * Block, top = cell["y"]!.GetValue<int>() * Block;
                for (int y = 0; y < Block; y++)
                    pixels.AsSpan(y * Block, Block).CopyTo(canvas.AsSpan((top + y) * columns * Block + left, Block));
            }

            _tree.WritePicture(path, columns * Block, rows * Block, canvas);
        }

        return $"{folder}/{GraphicsPlaceholder}/{name}.png";
    }
}
