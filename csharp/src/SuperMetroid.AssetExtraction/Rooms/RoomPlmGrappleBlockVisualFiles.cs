using SuperMetroid.Core.Rooms.LevelData;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs editable Grapple-block visual references while verifying every native
/// source draw record against the immutable collision/shape catalog.
/// </summary>
public static class RoomPlmGrappleBlockVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for the five initial, breakup, and blank breakable-Grapple frames.</summary>
    public const string VisualFileName = "grapple-blocks.json";
    /// <summary>Stock manifest filename recording format version one, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema revision shared by Grapple-block visual JSON and its stock manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Readable camel-case encoding for the visual document and provenance manifest.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Checks every six-byte bank-$84 Grapple-block draw record and exports its pointer-keyed twelve-bit appearance.</summary>
    /// <param name="bus">Cartridge source for the five compiled one-cell draws, including full collision and terminating offsets.</param>
    /// <param name="directory">Family directory, created before native validation; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, not recomputed here; stock load requires the supported identity.</param>
    /// <remarks>Only metatile/flip bits are editable. Existing files are not overwritten, and separate JSON/manifest writes can leave a partial installation.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data differs from compiled cartridge definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmVisualBlockEntry[] entries = ReadAndVerifyNative(bus);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new VisualDocument(FormatVersion, entries), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, VisualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new VisualManifest(FormatVersion, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json))), JsonOptions);
    }

    /// <summary>Validates the five stock Grapple-block frames before selecting an optional complete artwork replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or missing visual JSON selects validated stock.</param>
    /// <returns>A ROM-independent pointer-to-visual-word catalog; Grapple collision, breakup timing, and one-cell geometry remain compiled.</returns>
    /// <remarks>Stock provenance, byte hash, and compiled visual equality are mandatory even with an override. Override JSON needs each compiled draw pointer exactly once and twelve-bit words, but no manifest.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, document version, pointer coverage/uniqueness, or visual-only bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static InstalledRoomPlmGrappleBlockVisuals Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Grapple-block visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock Grapple-block visuals {stockPath} failed their manifest hash.");
        InstalledRoomPlmGrappleBlockVisuals stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 All)
        {
            RoomBlockAppearance expected = draw.Terrain.Appearance;
            if (stock.GetAppearance(PlmDraw.Of(draw.Frame)) != expected)
                throw new InvalidDataException(
                    $"Stock Grapple-block visuals {stockPath} differ from {draw.Frame}.");
        }

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="Load"/> without overrides, cartridge access, or writes.</summary>
    /// <param name="directory">Grapple-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Each one-block Grapple-block run writes along its row.</summary>
    private const PlmRunDirection RunDirection = PlmRunDirection.Horizontal;
    /// <summary>Each Grapple-block run writes its one block.</summary>
    private const int RunLength = 1;
    /// <summary>The run ends with a zero horizontal offset, leaving the next draw in its column.</summary>
    private const int RunNextX = 0;
    /// <summary>The run ends with a zero vertical offset, leaving the next draw in its row.</summary>
    private const int RunNextY = 0;

    /// <summary>Validates all five compiled one-block frames and builds the visuals from the authored appearances that differ from the compiled frames.</summary>
    /// <param name="entries">Exactly one visual-only entry for each compiled initial, breakup, and blank draw identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry changes an identity, repeats a frame, or leaves coverage incomplete.</exception>
    internal static InstalledRoomPlmGrappleBlockVisuals Create(IEnumerable<RoomPlmVisualBlockEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<GrappleBlockDraw, RoomBlockAppearance>();
        var seen = new HashSet<GrappleBlockDraw>();
        foreach (RoomPlmVisualBlockEntry entry in entries)
        {
            if (entry is null || !TryGetByVisualId(entry.Id, out var draw))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity.");
            if (!seen.Add(draw.Frame))
                throw new InvalidDataException($"Grapple-block visuals repeat draw {entry.Id}.");
            if (entry.Appearance != draw.Terrain.Appearance)
                selected.Add(draw.Frame, entry.Appearance);
        }

        if (seen.Count != All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        return new(selected);
    }

    /// <summary>The published artwork identity of a Grapple-block draw.</summary>
    internal static string VisualId(GrappleBlockDraw frame) => frame switch
    {
        GrappleBlockDraw.Intact => "intact",
        GrappleBlockDraw.Break0 => "break-0",
        GrappleBlockDraw.Break1 => "break-1",
        GrappleBlockDraw.Break2 => "break-2",
        GrappleBlockDraw.Blank => "blank",
        _ => throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}."),
    };

    /// <summary>Finds the Grapple-block draw whose artwork identity is <paramref name="id"/>.</summary>
    internal static bool TryGetByVisualId(string? id, out RoomPlmGrappleBlockDrawDefinitions.DrawList draw) =>
        RoomPlmDrawListExport.TryFindByVisualId(All, candidate => VisualId(candidate.Frame), id, out draw);

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in authored order.</summary>
    internal static IEnumerable<RoomPlmGrappleBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (GrappleBlockDraw frame in Enum.GetValues<GrappleBlockDraw>())
            {
                GrappleBlockPlmDraws.TryGet(PlmDraw.Of(frame), out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Checks every native one-cell draw's direction, block, and terminal offsets before exporting visuals.</summary>
    private static RoomPlmVisualBlockEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmVisualBlockEntry>();
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 All.OrderBy(item => PlmDrawAddressCodec.Of(PlmDraw.Of(item.Frame))))
        {
            int pointer = PlmDrawAddressCodec.Of(PlmDraw.Of(draw.Frame));
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, pointer)) !=
                    PlmRunHeaderCodec.Encode(RunDirection,
                        RunLength) ||
                RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (unchecked((ushort)(pointer + 2)))))) != draw.Terrain ||
                bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, unchecked((ushort)(pointer + 4)))) !=
                    RunNextX ||
                bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, unchecked((ushort)(pointer + 5)))) !=
                    RunNextY)
                throw new InvalidDataException(
                    $"Grapple-block draw list ${pointer:X4} differs from compiled cartridge data.");
            entries.Add(new RoomPlmVisualBlockEntry(VisualId(draw.Frame),
                draw.Terrain.Appearance));
        }

        return entries.ToArray();
    }

    /// <summary>Validates a document's version and pointer-to-word entries with its source path in any error.</summary>
    private static InstalledRoomPlmGrappleBlockVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Grapple-block visuals {path} have an incompatible format.");
        try { return Create(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Grapple-block visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a required Grapple-block JSON file and preserves its path in validation failures.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes JSON bytes, rejecting empty or malformed documents with file context.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Grapple-block JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Grapple-block JSON {path}.", error);
        }
    }

    /// <summary>Stock provenance and visual-file hash required even when an override is selected.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned five-entry pointer-to-visual-word document.</summary>
    private sealed record VisualDocument(int Version, RoomPlmVisualBlockEntry[] Entries);
}
