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

    /// <summary>A tree rooted at <paramref name="root"/>, which is created if it is not there.</summary>
    internal EngineAssetTree(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
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

    /// <summary>Writes a data file.</summary>
    /// <exception cref="InvalidOperationException">Different data was already written at the path.</exception>
    internal void WriteData(string path, JsonObject data)
    {
        ArgumentNullException.ThrowIfNull(data);
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
    }
}
