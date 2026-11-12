using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed recolour-region maps the picture store's pictures are shown with (#1633, #1644), one <c>.regions</c> file
/// in <c>game/recolour-regions/</c> (<see cref="Installation.PictureStoreFiles.RecolourRegionMapsFile"/>): each map once,
/// by its tag, giving each pixel of a picture the recolour region it lies in among its sheet's pixels of its colour
/// (<see cref="ColourKey.Region"/>), so a shade can recolour pixels of one colour differently. A sheet frame naming a picture
/// names the map it shows the picture with, and only a picture shown with a pixel outside region zero is shown with one.
/// It is JSON: <c>{ "version": 2, "maps": { "R0000": { "size": { "x": W, "y": H }, "rows": [ [region, count, ...], ... ] } } }</c>,
/// each map's rows from the top a run-length list of regions from the left whose counts cover exactly the picture's width.
/// </summary>
internal static class RecolourRegionMapFile
{
    /// <summary>
    /// The map format's revision: 1 held one page's map; 2 holds every map the pictures are shown with, each once by its tag,
    /// the size of the picture it is for (#1644).
    /// </summary>
    internal const int Version = 2;

    /// <summary>The map file's extension.</summary>
    internal const string Extension = ".regions";

    /// <summary>Encodes <paramref name="maps"/>, each by its tag, in the order given.</summary>
    /// <exception cref="ArgumentException">A tag is named twice, or a map holds a region below zero.</exception>
    internal static byte[] Encode(List<KeyValuePair<string, PictureMap<int>>> maps)
    {
        Ensure.NotNull(maps);
        var documents = new Dictionary<string, MapDocument>(StringComparer.Ordinal);
        foreach ((string tag, PictureMap<int> map) in maps)
        {
            Ensure.NotNull(map);
            if (!documents.TryAdd(tag, Encoded(tag, map)))
                throw new ArgumentException($"Recolour-region map {tag} is named twice.", nameof(maps));
        }
        return JsonSerializer.SerializeToUtf8Bytes(new Document(Version, documents), Options);
    }

    /// <summary>One map's runs.</summary>
    /// <exception cref="ArgumentException">A pixel lies in a region below zero.</exception>
    private static MapDocument Encoded(string tag, PictureMap<int> map)
    {
        Extent16<ImagePixels> size = map.Size;
        ReadOnlySpan<int> regions = map.Values;
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
                    throw new ArgumentException($"Pixel ({start}, {y}) of recolour-region map {tag} lies in region {row[start]}, below zero.", nameof(map));
                runs.Add(row[start]);
                runs.Add(x - start);
                start = x;
            }
            rows[y] = [.. runs];
        }
        return new MapDocument(size, rows);
    }

    /// <summary>Every map <paramref name="bytes"/>, the map file, holds, by tag.</summary>
    /// <param name="bytes">The map file.</param>
    /// <param name="name">Names the file in a failure.</param>
    /// <exception cref="InvalidDataException">
    /// The file is malformed or of another revision, or a map's row is missing or its runs do not cover exactly its width.
    /// </exception>
    internal static Dictionary<string, PictureMap<int>> Decode(byte[] bytes, string name)
    {
        Document document = JsonAssetDocument.Read<Document>(new MemoryStream(bytes, writable: false), Options, $"recolour-region maps {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Recolour-region maps {name} have revision {document.Version}, not {Version}.");
        if (document.Maps is null)
            throw new InvalidDataException($"Recolour-region maps {name} list no maps.");
        var maps = new Dictionary<string, PictureMap<int>>(StringComparer.Ordinal);
        foreach ((string tag, MapDocument? map) in document.Maps)
            maps.Add(tag, Decoded($"{name} map {tag}", map ?? throw new InvalidDataException($"Recolour-region maps {name} map {tag} is null.")));
        return maps;
    }

    /// <summary>One map's regions.</summary>
    /// <exception cref="InvalidDataException">A row is missing or its runs do not cover exactly the map's width.</exception>
    private static PictureMap<int> Decoded(string name, MapDocument map)
    {
        Extent16<ImagePixels> size = map.Size;
        if (map.Rows is null || map.Rows.Length != size.Y)
            throw new InvalidDataException($"Recolour-region map {name} does not hold {size.Y} rows, the height of its picture.");
        var regions = new int[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            int[] runs = map.Rows[y] ?? throw new InvalidDataException($"Recolour-region map {name} has no row {y}.");
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
        return new PictureMap<int>(size, regions);
    }

    /// <summary>Camel-case fields; unknown fields are rejected.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
    };

    /// <summary>The map file.</summary>
    /// <param name="Version">The format revision.</param>
    /// <param name="Maps">Every map, by tag.</param>
    private sealed record Document(int Version, Dictionary<string, MapDocument> Maps);

    /// <summary>One map.</summary>
    /// <param name="Size">The width and height of the picture it is for.</param>
    /// <param name="Rows">Each row's runs, region then count.</param>
    private sealed record MapDocument(Extent16<ImagePixels> Size, int[][] Rows);
}
