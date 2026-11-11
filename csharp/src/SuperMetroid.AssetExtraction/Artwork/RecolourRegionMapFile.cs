using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed recolour-region map of a sheet image (#1633), a <c>.regions</c> file in <c>game/recolour-regions/</c>: for
/// each pixel, the recolour region it lies in among the image's pixels of its colour (<see cref="ColourKey.Region"/>), so a
/// shade can recolour pixels of one colour differently. Only a sheet whose pixels of one colour recolour differently under
/// some shade has one. It is JSON: <c>{ "version": 1, "width": W, "height": H, "rows": [ [region, count, ...], ... ] }</c>,
/// each row from the top a run-length list of regions from the left whose counts cover exactly the image's width.
/// </summary>
internal static class RecolourRegionMapFile
{
    /// <summary>The map format's revision.</summary>
    internal const int Version = 1;

    /// <summary>The map file's extension.</summary>
    internal const string Extension = ".regions";

    /// <summary>Encodes the map of an image of <paramref name="size"/> whose pixels lie in <paramref name="regions"/>.</summary>
    /// <exception cref="ArgumentException">The regions are not one per pixel or one is negative.</exception>
    internal static byte[] Encode(Extent16<ImagePixels> size, ReadOnlySpan<int> regions)
    {
        Ensure.LengthEqual(regions, size.X * size.Y);
        var rows = new int[size.Y][];
        var runs = new List<int>();
        for (int y = 0; y < size.Y; y++)
        {
            runs.Clear();
            ReadOnlySpan<int> row = regions.Slice(y * size.X, size.X);
            int start = 0;
            for (int x = 1; x <= row.Length; x++)
            {
                if (x < row.Length && row[x] == row[start])
                    continue;
                if (row[start] < 0)
                    throw new ArgumentException($"Pixel ({start}, {y}) lies in recolour region {row[start]}, below zero.", nameof(regions));
                runs.Add(row[start]);
                runs.Add(x - start);
                start = x;
            }
            rows[y] = [.. runs];
        }
        return JsonSerializer.SerializeToUtf8Bytes(new Document(Version, size, rows), Options);
    }

    /// <summary>The region of each pixel of an image of <paramref name="size"/> that <paramref name="bytes"/>, its map, holds.</summary>
    /// <param name="bytes">The map file.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="name">Names the map in a failure.</param>
    /// <exception cref="InvalidDataException">
    /// The map is malformed, of another revision or size, or a row's runs do not cover exactly the image's width.
    /// </exception>
    internal static int[] Decode(byte[] bytes, Extent16<ImagePixels> size, string name)
    {
        Document document = JsonAssetDocument.Read<Document>(new MemoryStream(bytes, writable: false), Options, $"recolour-region map {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Recolour-region map {name} has revision {document.Version}, not {Version}.");
        if (document.Size != size || document.Rows is null || document.Rows.Length != size.Y)
            throw new InvalidDataException($"Recolour-region map {name} is not {size.X}x{size.Y}, the size of its image.");
        var regions = new int[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            int[] runs = document.Rows[y] ?? throw new InvalidDataException($"Recolour-region map {name} has no row {y}.");
            if (runs.Length % 2 != 0)
                throw new InvalidDataException($"Recolour-region map {name} row {y} ends inside a run.");
            int x = 0;
            for (int run = 0; run < runs.Length; run += 2)
            {
                int region = runs[run], count = runs[run + 1];
                if (region < 0 || count <= 0 || x + count > size.X)
                    throw new InvalidDataException($"Recolour-region map {name} row {y} has run ({region}, {count}) at {x}, outside its {size.X} pixels or regions.");
                regions.AsSpan(y * size.X + x, count).Fill(region);
                x += count;
            }
            if (x != size.X)
                throw new InvalidDataException($"Recolour-region map {name} row {y} covers {x} of its {size.X} pixels.");
        }
        return regions;
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
    /// <param name="Rows">Each row's runs, region then count.</param>
    private sealed record Document(int Version, Extent16<ImagePixels> Size, int[][] Rows);
}
