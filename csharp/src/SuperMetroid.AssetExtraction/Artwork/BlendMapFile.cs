using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed blend map of a sheet image (#1633), a <c>.blendmap</c> file in <c>game/blend-maps/</c>: which pixels blend
/// with what lies behind them where the sheet is drawn as sprites (<see cref="SpriteImage.Blending"/>). Only a sprite-drawn
/// sheet with a picture that blends in some opaque pixels and not others has one. It is JSON:
/// <c>{ "version": 1, "width": W, "height": H, "rows": [ [start, count, ...], ... ] }</c>, each row from the top listing
/// its runs of blending pixels from the left, each by its first pixel and its length, in order and apart.
/// </summary>
internal static class BlendMapFile
{
    /// <summary>The map format's revision.</summary>
    internal const int Version = 1;

    /// <summary>The map file's extension.</summary>
    internal const string Extension = ".blendmap";

    /// <summary>Encodes the map of an image of <paramref name="size"/> whose blending pixels <paramref name="blending"/> marks.</summary>
    /// <exception cref="ArgumentException">The marks are not one per pixel.</exception>
    internal static byte[] Encode(Extent16<ImagePixels> size, ReadOnlySpan<bool> blending)
    {
        Ensure.LengthEqual(blending, size.X * size.Y);
        var rows = new int[size.Y][];
        var runs = new List<int>();
        for (int y = 0; y < size.Y; y++)
        {
            runs.Clear();
            ReadOnlySpan<bool> row = blending.Slice(y * size.X, size.X);
            for (int x = 0; x < row.Length; x++)
            {
                if (!row[x])
                    continue;
                int start = x;
                while (x < row.Length && row[x])
                    x++;
                runs.Add(start);
                runs.Add(x - start);
            }
            rows[y] = [.. runs];
        }
        return JsonSerializer.SerializeToUtf8Bytes(new Document(Version, size, rows), Options);
    }

    /// <summary>Which pixels of an image of <paramref name="size"/> blend, as <paramref name="bytes"/>, its map, says.</summary>
    /// <param name="bytes">The map file.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="name">Names the map in a failure.</param>
    /// <exception cref="InvalidDataException">
    /// The map is malformed, of another revision or size, or a row's runs leave the image, overlap or are out of order.
    /// </exception>
    internal static bool[] Decode(byte[] bytes, Extent16<ImagePixels> size, string name)
    {
        Document document = JsonAssetDocument.Read<Document>(new MemoryStream(bytes, writable: false), Options, $"blend map {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Blend map {name} has revision {document.Version}, not {Version}.");
        if (document.Size != size || document.Rows is null || document.Rows.Length != size.Y)
            throw new InvalidDataException($"Blend map {name} is not {size.X}x{size.Y}, the size of its image.");
        var blending = new bool[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            int[] runs = document.Rows[y] ?? throw new InvalidDataException($"Blend map {name} has no row {y}.");
            if (runs.Length % 2 != 0)
                throw new InvalidDataException($"Blend map {name} row {y} ends inside a run.");
            int end = 0;
            for (int run = 0; run < runs.Length; run += 2)
            {
                int start = runs[run], count = runs[run + 1];
                if (start < end || count <= 0 || start + count > size.X)
                    throw new InvalidDataException($"Blend map {name} row {y} has run ({start}, {count}) outside its {size.X} pixels, overlapping or out of order.");
                blending.AsSpan(y * size.X + start, count).Fill(true);
                end = start + count;
            }
        }
        return blending;
    }

    /// <summary>Camel-case fields; unknown fields are rejected.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
    };

    /// <summary>The map document.</summary>
    /// <param name="Version">The format revision.</param>
    /// <param name="Size">The image's width and height.</param>
    /// <param name="Rows">Each row's runs of blending pixels, first pixel then length.</param>
    private sealed record Document(int Version, Extent16<ImagePixels> Size, int[][] Rows);
}
