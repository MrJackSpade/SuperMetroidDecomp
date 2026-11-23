using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installed dissolve maps of the boss body pictures a dissolve erases (#2478), one <c>.dissolve</c> file in
/// <c>game/dissolve-maps/</c> apart from the pictures: each map once, by its tag, giving each pixel of a picture the band of
/// the dissolve that shows it and the steps after which the dissolve erases it, as Crocomire's melt images dissolve
/// (<see cref="PictureDissolve"/>). A boss body frame whose picture dissolves names its map's tag. It is JSON:
/// <c>{ "version": 2, "maps": { "Crocomire MeltingFirst": { "size": { "x": W, "y": H }, "rows": [ [ [count], [band, steps, count], ... ], ... ] } } }</c>,
/// each map's rows from the top a run-length list from the left whose counts cover exactly the picture's width: a run of
/// pixels the dissolve keeps holds only its count, and a run it erases its band, its steps and its count.
/// </summary>
internal static partial class BossBodyDissolveMapFile
{
    /// <summary>
    /// The map format's revision: 1 held each dissolving boss body picture's map once, by its tag (#2478); 2 spells a kept
    /// run by its count alone (#2558).
    /// </summary>
    internal const int Version = 2;

    /// <summary>The map file's extension.</summary>
    internal const string Extension = ".dissolve";

    /// <summary>The map file's name, in the dissolve map directory.</summary>
    internal const string FileName = "boss-bodies" + Extension;

    /// <summary>The map file under the content directory <paramref name="content"/>.</summary>
    internal static string PathIn(string content) =>
        Path.Combine(content, GameInstallationLayout.DissolveMapDirectoryName, FileName);

    /// <summary>Encodes <paramref name="maps"/>, each by its tag, in the order given.</summary>
    /// <exception cref="ArgumentException">A tag is named twice.</exception>
    internal static byte[] Encode(IEnumerable<KeyValuePair<string, PictureDissolve>> maps)
    {
        Ensure.NotNull(maps);
        var documents = new Dictionary<string, MapDocument>(StringComparer.Ordinal);
        foreach ((string tag, PictureDissolve map) in maps)
        {
            if (!documents.TryAdd(tag, Encoded(Ensure.NotNull(map))))
                throw new ArgumentException($"Dissolve map {tag} is named twice.", nameof(maps));
        }
        return JsonSerializer.SerializeToUtf8Bytes(new Document(Version, documents), global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Installation.BossBodyDissolveMapFile.Document>(Options));
    }

    /// <summary>One map's runs.</summary>
    private static MapDocument Encoded(PictureDissolve map)
    {
        Extent16<ImagePixels> size = map.Size;
        ImmutableArray<DissolvingPixel?> pixels = map.Pixels;
        var rows = new int[size.Y][][];
        var runs = new List<int[]>();
        for (int y = 0; y < size.Y; y++)
        {
            runs.Clear();
            int start = 0;
            for (int x = 1; x <= size.X; x++)
            {
                if (x < size.X && pixels[(y * size.X) + x] == pixels[(y * size.X) + start])
                    continue;
                runs.Add(pixels[(y * size.X) + start] is { } pixel ? [pixel.Band, pixel.ErasedAfterSteps, x - start] : [x - start]);
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
    /// The file is malformed or of another revision, or a map's row is missing, its runs do not cover exactly its width or a
    /// run names a band and steps no dissolving or kept pixel holds.
    /// </exception>
    internal static Dictionary<string, PictureDissolve> Decode(byte[] bytes, string name)
    {
        Document document = JsonAssetDocument.Read<Document>(new MemoryStream(bytes, writable: false), Options, $"dissolve maps {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Dissolve maps {name} have revision {document.Version}, not {Version}.");
        if (document.Maps is null)
            throw new InvalidDataException($"Dissolve maps {name} list no maps.");
        var maps = new Dictionary<string, PictureDissolve>(StringComparer.Ordinal);
        foreach ((string tag, MapDocument map) in document.Maps)
            maps.Add(tag, Decoded($"{name} map {tag}", map));
        return maps;
    }

    /// <summary>One map's pixels.</summary>
    /// <exception cref="InvalidDataException">A row is missing, its runs do not cover exactly the map's width, or a run is invalid.</exception>
    private static PictureDissolve Decoded(string name, MapDocument map)
    {
        Extent16<ImagePixels> size = map.Size;
        if (map.Rows is null || map.Rows.Length != size.Y)
            throw new InvalidDataException($"Dissolve map {name} does not hold {size.Y} rows, the height of its picture.");
        var pixels = new DissolvingPixel?[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            int[][] runs = map.Rows[y] ?? throw new InvalidDataException($"Dissolve map {name} has no row {y}.");
            int x = 0;
            foreach (int[] run in runs)
            {
                DissolvingPixel? pixel;
                int count;
                switch (run)
                {
                    case [int kept]:
                        pixel = null;
                        count = kept;
                        break;
                    case [int band, int steps, int erased] when band >= 0 && steps >= 1:
                        pixel = new DissolvingPixel(band, steps);
                        count = erased;
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Dissolve map {name} row {y} has run [{string.Join(", ", run)}] at {x}: not a kept run or a band and steps erasing it.");
                }
                if (count <= 0 || x + count > size.X)
                    throw new InvalidDataException($"Dissolve map {name} row {y} has a run of {count} pixels at {x}, outside its {size.X} pixels.");
                pixels.AsSpan((y * size.X) + x, count).Fill(pixel);
                x += count;
            }
            if (x != size.X)
                throw new InvalidDataException($"Dissolve map {name} row {y} covers {x} of its {size.X} pixels.");
        }
        return new PictureDissolve(size, [.. pixels]);
    }

    /// <summary>Camel-case fields; unknown fields are rejected.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
        RespectNullableAnnotations = true,
        TypeInfoResolver = InstalledDocumentElements.Resolver,
    };

    /// <summary>The map file.</summary>
    /// <param name="Version">The format revision.</param>
    /// <param name="Maps">Every map, by tag.</param>
    internal sealed partial record Document(int Version, Dictionary<string, MapDocument> Maps);

    /// <summary>One map.</summary>
    /// <param name="Size">The width and height of the picture it is for.</param>
    /// <param name="Rows">Each row's runs: a kept run's count, or an erased run's band, steps, then count.</param>
    internal sealed partial record MapDocument(Extent16<ImagePixels> Size, int[][][] Rows);
}
