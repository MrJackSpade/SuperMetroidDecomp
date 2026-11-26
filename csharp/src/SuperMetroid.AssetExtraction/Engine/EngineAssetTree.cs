using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The tree of files the Godot game loads its assets from: pictures as PNG and data as JSON, each named by its path
/// within the tree, with forward slashes, as the game names it.
/// </summary>
internal sealed partial class EngineAssetTree
{
    /// <summary>The buffers every picture this tree writes is encoded with.</summary>
    private readonly RgbaPngBuffers _buffers = new();

    /// <summary>The paths written so far, so that writing one twice with different contents is caught.</summary>
    private readonly Dictionary<string, int> _written = new(StringComparer.Ordinal);

    /// <summary>Told how many files have been written, after each; null when nobody watches.</summary>
    private readonly Action<int>? _onWritten;

    /// <summary>A tree rooted at <paramref name="root"/>, which is created if it is not there.</summary>
    /// <param name="root">The directory the tree is in.</param>
    /// <param name="written">Told how many files have been written, after each; null when nobody watches.</param>
    internal EngineAssetTree(string root, Action<int>? written = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        _onWritten = written;
        Root = Path.GetFullPath(root);
        Directory.CreateDirectory(Root);
    }

    /// <summary>The directory the tree is in.</summary>
    internal string Root { get; }

    /// <summary>Whether a file has been written at <paramref name="path"/>.</summary>
    internal bool Holds(string path) => _written.ContainsKey(path);

    /// <summary>Writes a picture of <paramref name="width"/> by <paramref name="height"/> pixels, given row by row.</summary>
    /// <exception cref="InvalidOperationException">A different picture was already written at the path.</exception>
    internal void WritePicture(string path, int width, int height, ReadOnlySpan<RgbaPixel> pixels)
    {
        if (pixels.Length != width * height)
            throw new ArgumentException($"The picture {path} is {width} by {height} but has {pixels.Length} pixels.", nameof(pixels));
        Write(path, RgbaPng.Encode(new Extent16<ImagePixels>(width, height), pixels, _buffers));
    }

    /// <summary>
    /// The path of the part of the picture at <paramref name="picture"/> that lies in front of the things standing in a
    /// room, which lies beside it: a picture of a room's tiles has one where some of it is drawn over what stands there.
    /// </summary>
    internal static string FrontPath(string picture) => picture.EndsWith(".png", StringComparison.Ordinal)
        ? picture[..^4] + ".front.png"
        : throw new ArgumentException($"A front part lies beside a picture, which {picture} is not.", nameof(picture));

    /// <summary>The most regions a picture's map of regions tells apart.</summary>
    internal const int MostRegions = 256;

    /// <summary>The path of the map of regions that lies beside the picture at <paramref name="picture"/>.</summary>
    internal static string RegionMapPath(string picture) => picture.EndsWith(".png", StringComparison.Ordinal)
        ? picture[..^4] + ".regions.png"
        : throw new ArgumentException($"A map of regions lies beside a picture, which {picture} is not.", nameof(picture));

    /// <summary>
    /// Writes the map of regions of the picture at <paramref name="picture"/>, beside it: a picture of the same size
    /// in greys, each pixel as far from white as the number of the region the picture's pixel there lies in, so the
    /// first region is white. What changes a picture's colours where it is drawn reads it to tell apart two parts of
    /// the picture that have one colour and are changed differently.
    /// </summary>
    /// <exception cref="InvalidDataException">A region's number is not one a map can hold.</exception>
    internal void WriteRegionMap(string picture, int width, int height, ReadOnlySpan<int> regions)
    {
        if (regions.Length != width * height)
            throw new ArgumentException($"The picture {picture} is {width} by {height} but has {regions.Length} regions.", nameof(regions));
        var pixels = new RgbaPixel[regions.Length];
        for (int pixel = 0; pixel < regions.Length; pixel++)
        {
            int region = regions[pixel];
            if (region < 0 || region >= MostRegions)
                throw new InvalidDataException($"The picture {picture} has a pixel in the region {region}; a map of regions holds 0 through {MostRegions - 1}.");
            byte grey = (byte)(MostRegions - 1 - region);
            pixels[pixel] = new RgbaPixel(grey, grey, grey, 255);
        }

        WritePicture(RegionMapPath(picture), width, height, pixels);
    }

    /// <summary>Writes a data file.</summary>
    /// <exception cref="InvalidOperationException">Different data was already written at the path.</exception>
    internal void WriteData(string path, JsonObject data)
    {
        ArgumentNullException.ThrowIfNull(data);
        EngineDefaults.Check(path, data);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            data.WriteTo(writer);
        Write(path, stream.ToArray());
    }

    /// <summary>Writes a file's bytes, once; writing the same bytes again changes nothing.</summary>
    private void Write(string path, byte[] bytes)
    {
        if (path.Contains('\\') || path.StartsWith('/') || path.Contains(".."))
            throw new ArgumentException($"An asset is named by its path within the tree, with forward slashes: {path}", nameof(path));
        int sum = 17;
        foreach (byte value in bytes)
            sum = unchecked(sum * 31 + value);
        if (_written.TryGetValue(path, out int before))
        {
            if (before != sum)
                throw new InvalidOperationException($"Two different files were written as the asset {path}.");
            return;
        }

        _written[path] = sum;
        string full = Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllBytes(full, bytes);
        _onWritten?.Invoke(_written.Count);
    }
}
