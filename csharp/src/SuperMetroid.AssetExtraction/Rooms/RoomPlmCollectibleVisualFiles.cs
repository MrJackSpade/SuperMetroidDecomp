using System.Text.Json.Serialization;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmCollectibleDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs named collectible block art while checking all 24 native draw records
/// and the eight dynamic-slot selectors against compiled physical definitions, and names each frame.
/// </summary>
public static class RoomPlmCollectibleVisualFiles
{
    /// <summary>Editable JSON filename containing the complete twenty-four-frame collectible visual set.</summary>
    public const string VisualFileName = "collectibles.json";
    /// <summary>Stock provenance manifest filename containing the format, source-cartridge identity, and visual-file digest.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema revision required by stock collectible visuals and their manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Reads and writes collectible documents with stable camel-case JSON names.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Verifies the native collectible selectors/draw lists and creates a new stock visual JSON plus provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge address space used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">SHA-256 identity recorded in the stock manifest.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">A path or cartridge identity is blank.</exception>
    /// <exception cref="InvalidDataException">Native selectors, draw geometry, or block differ from the compiled definitions.</exception>
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
            new VisualManifest(FormatVersion, sourceCartridgeSha256), JsonOptions);
    }

    /// <summary>Validates the installed collectible manifest's provenance and loads the installed collectible JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed family directory containing the version-one collectible JSON and its manifest.</param>
    /// <returns>The immutable collectible visual catalog.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is blank.</exception>
    /// <exception cref="InvalidDataException">The manifest provenance or the installed JSON is invalid.</exception>
    public static InstalledRoomPlmCollectibleVisuals Load(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Collectible visual manifest {manifestPath} is incompatible.");
        string path = Path.Combine(directory, VisualFileName);
        return CreateCatalog(ReadJson<VisualDocument>(path), path);
    }

    /// <summary>Validates all twenty-four compiled one-cell frames and builds the visuals from the authored appearances that differ from the compiled frames.</summary>
    /// <param name="entries">Exactly one appearance for each compiled collectible frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, or leaves compiled frame coverage incomplete.</exception>
    internal static InstalledRoomPlmCollectibleVisuals Create(IEnumerable<RoomPlmVisualBlockEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<Draw, RoomBlockAppearance>();
        var seen = new HashSet<Draw>();
        foreach (RoomPlmVisualBlockEntry entry in entries)
        {
            if (entry is null || !TryGetById(entry.Id, out var frame))
                throw new InvalidDataException(
                    "Collectible visuals changed a compiled frame identity.");
            if (!seen.Add(frame.Draw))
                throw new InvalidDataException($"Collectible visuals repeat frame {entry.Id}.");
            if (entry.Appearance != frame.Terrain.Appearance)
                selected.Add(frame.Draw, entry.Appearance);
        }
        if (seen.Count != All.Count())
            throw new InvalidDataException("Collectible visuals do not cover all compiled frames.");
        return new(selected);
    }

    /// <summary>The editable artwork identifier of a draw.</summary>
    internal static string VisualId(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => "empty",
        DrawGroup.Orb => $"chozo-orb-{draw.Index}",
        DrawGroup.OrbBurst => "chozo-orb-burst",
        DrawGroup.Tank => $"{TankKindName(draw.Index / 2)}-tank-{draw.Index % 2}",
        DrawGroup.Dynamic => $"dynamic-slot-{draw.Index / 2}-frame-{draw.Index % 2}",
        DrawGroup.ShotReveal => $"shot-reveal-{draw.Index}",
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
    };

    /// <summary>Gets the stable visual ID stem for one of the four tank pickup kinds.</summary>
    private static string TankKindName(int kind) => kind switch
    {
        0 => "energy",
        1 => "missile",
        2 => "super-missile",
        _ => "power-bomb",
    };

    /// <summary>Resolves an exact artwork identifier to its compiled collectible draw frame.</summary>
    internal static bool TryGetById(string id, out RoomPlmCollectibleDrawFrame frame)
    {
        foreach (var candidate in All)
        {
            if (!string.Equals(candidate.Id, id, StringComparison.Ordinal)) continue;
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    /// <summary>Enumerates all 24 collectible one-cell draws in their published order.</summary>
    internal static IEnumerable<RoomPlmCollectibleDrawFrame> All
    {
        get
        {
            foreach (DrawGroup group in Enum.GetValues<DrawGroup>())
            {
                for (int index = 0; index < CollectiblePlmDraws.CountOf(group); index++)
                {
                    var draw = new Draw(group, index);
                    CollectiblePlmDraws.TryGetBlock(PlmDraw.Of(draw), out RoomTerrainBlock block);
                    yield return new(draw, block, VisualId(draw));
                }
            }
        }
    }

    /// <summary>Checks eight dynamic selectors and all 24 one-cell native draws before extracting appearance.</summary>
    private static RoomPlmVisualBlockEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        for (int slot = 0; slot < 4; slot++)
        for (int animation = 0; animation < 2; animation++)
        {
            int table = animation == 0
                ? PlmDrawSourceDefinitions.CollectibleDynamicFrame0Table.Offset
                : PlmDrawSourceDefinitions.CollectibleDynamicFrame1Table.Offset;
            int native = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(table + slot * 2)))));
            int compiled = PlmDrawAddressCodec.Of(CollectiblePlmDraws.VisibleFrame(
                InWorldCollectibleKind.Bombs, animation, slot));
            if (native != compiled)
                throw new InvalidDataException(
                    $"Dynamic collectible selector $84:{table + slot * 2:X4} differs from the cartridge.");
        }

        var entries = new List<RoomPlmVisualBlockEntry>();
        foreach (RoomPlmCollectibleDrawFrame frame in All)
        {
            int address = PlmDrawAddressCodec.Of(PlmDraw.Of(frame.Draw));
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, address)) != 1 ||
                RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(address + 2)))))) != frame.Terrain ||
                CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(address + 4))))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw {frame.Id} at $84:{address:X4} differs from compiled geometry or level word.");
            entries.Add(new RoomPlmVisualBlockEntry(frame.Id,
                frame.Terrain.Appearance));
        }
        return entries.ToArray();
    }

    /// <summary>Validates document version and entries, attaching the source path to any catalog error.</summary>
    private static InstalledRoomPlmCollectibleVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Collectible visuals {path} have an incompatible format.");
        try { return Create(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid collectible visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a collectible document from an installed JSON path.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes a collectible document and identifies malformed or empty content by path.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Collectible JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid collectible JSON {path}.", error);
        }
    }

    /// <summary>Records the supported cartridge revision and digest of the stock visual JSON.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256);
    /// <summary>Stores a versioned, complete set of named collectible visual references.</summary>
    private sealed record VisualDocument(int Version,
        RoomPlmVisualBlockEntry[] Entries);
}
