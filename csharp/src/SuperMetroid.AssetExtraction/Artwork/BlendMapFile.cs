using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed blend maps the picture store's pictures are shown with (#1633, #1644), one <c>.blendmap</c> file in
/// <c>game/blend-maps/</c> (<see cref="Installation.PictureStoreFiles.BlendMapsFile"/>): each map once, by its tag, saying
/// which pixels of a picture blend with what lies behind them where its sheet is drawn as sprites
/// (<see cref="SpriteImage.Blending"/>). A sheet frame naming a picture names the map it shows the picture with, and only a
/// picture of a sprite-drawn sheet with a frame that blends in some opaque pixels and not others is shown with one. It is
/// JSON: <c>{ "version": 2, "maps": { "B0000": { "size": { "x": W, "y": H }, "rows": [ [start, count, ...], ... ] } } }</c>,
/// each map's rows from the top listing its runs of blending pixels from the left, each by its first pixel and its length,
/// in order and apart.
/// </summary>
internal static class BlendMapFile
{
    /// <summary>
    /// The map format's revision: 1 held one page's map; 2 holds every map the pictures are shown with, each once by its tag,
    /// the size of the picture it is for (#1644).
    /// </summary>
    internal const int Version = 2;

    /// <summary>The map file's extension.</summary>
    internal const string Extension = ".blendmap";

    /// <summary>Encodes <paramref name="maps"/>, each by its tag, in the order given.</summary>
    /// <exception cref="ArgumentException">A tag is named twice.</exception>
    internal static byte[] Encode(List<KeyValuePair<string, PictureMap<bool>>> maps)
    {
        Ensure.NotNull(maps);
        var documents = new Dictionary<string, MapDocument>(StringComparer.Ordinal);
        foreach ((string tag, PictureMap<bool> map) in maps)
        {
            if (!documents.TryAdd(tag, Encoded(Ensure.NotNull(map))))
                throw new ArgumentException($"Blend map {tag} is named twice.", nameof(maps));
        }
        return JsonSerializer.SerializeToUtf8Bytes(new Document(Version, documents), Options);
    }

    /// <summary>One map's runs of blending pixels.</summary>
    private static MapDocument Encoded(PictureMap<bool> map)
    {
        Extent16<ImagePixels> size = map.Size;
        ReadOnlySpan<bool> blending = map.Values;
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
        return new MapDocument(size, rows);
    }

    /// <summary>Every map <paramref name="bytes"/>, the map file, holds, by tag.</summary>
    /// <param name="bytes">The map file.</param>
    /// <param name="name">Names the file in a failure.</param>
    /// <exception cref="InvalidDataException">
    /// The file is malformed or of another revision, or a map's row is missing or its runs leave the picture, overlap or are
    /// out of order.
    /// </exception>
    internal static Dictionary<string, PictureMap<bool>> Decode(byte[] bytes, string name)
    {
        Document document = JsonAssetDocument.Read<Document>(new MemoryStream(bytes, writable: false), Options, $"blend maps {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Blend maps {name} have revision {document.Version}, not {Version}.");
        if (document.Maps is null)
            throw new InvalidDataException($"Blend maps {name} list no maps.");
        var maps = new Dictionary<string, PictureMap<bool>>(StringComparer.Ordinal);
        foreach ((string tag, MapDocument? map) in document.Maps)
            maps.Add(tag, Decoded($"{name} map {tag}", map ?? throw new InvalidDataException($"Blend maps {name} map {tag} is null.")));
        return maps;
    }

    /// <summary>One map's blending pixels.</summary>
    /// <exception cref="InvalidDataException">A row is missing or its runs leave the picture, overlap or are out of order.</exception>
    private static PictureMap<bool> Decoded(string name, MapDocument map)
    {
        Extent16<ImagePixels> size = map.Size;
        if (map.Rows is null || map.Rows.Length != size.Y)
            throw new InvalidDataException($"Blend map {name} does not hold {size.Y} rows, the height of its picture.");
        var blending = new bool[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            int[] runs = map.Rows[y] ?? throw new InvalidDataException($"Blend map {name} has no row {y}.");
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
        return new PictureMap<bool>(size, blending);
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
    /// <param name="Rows">Each row's runs of blending pixels, first pixel then length.</param>
    private sealed record MapDocument(Extent16<ImagePixels> Size, int[][] Rows);
}
