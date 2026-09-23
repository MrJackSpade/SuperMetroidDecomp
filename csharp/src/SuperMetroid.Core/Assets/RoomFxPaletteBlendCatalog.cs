using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable three-color room-FX blends selected by native FX records.</summary>
public sealed class RoomFxPaletteBlendCatalog
{
    private readonly Dictionary<byte, ushort[]> blends;

    private RoomFxPaletteBlendCatalog(Dictionary<byte, ushort[]> blends) => this.blends = blends;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static RoomFxPaletteBlendCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        RoomFxPaletteBlendDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<RoomFxPaletteBlendDocument>(JsonOptions)
                ?? throw new InvalidDataException("Room-FX blend palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room-FX blend palette JSON.", error);
        }
        if (document.Version != RoomFxPaletteBlendDefinitions.Version ||
            document.Blends is null || document.Blends.Count != RoomFxPaletteBlendDefinitions.Ids.Count)
            throw new InvalidDataException("Room-FX blend palettes require the supported version and all eight selections.");

        var blends = new Dictionary<byte, ushort[]>();
        foreach (byte id in RoomFxPaletteBlendDefinitions.Ids)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDefinitions.Key(id), out PaletteRgb5[]? colors) ||
                colors is null || colors.Length != RoomFxRomData.Layer3.PaletteBlendColorCount)
                throw new InvalidDataException($"Room-FX blend {id:X2} requires three colors.");
            var words = new ushort[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5? color = colors[index];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Room-FX blend {id:X2} color {index} requires RGB components from zero through 31.");
                words[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            }
            blends.Add(id, words);
        }
        return new(blends);
    }

    public static byte[] Write(RoomFxPaletteBlendDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Applies the native three-color write, or clears only color 27 for selection zero.</summary>
    public void Apply(SnesCgram cgram, byte selection)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (selection == 0)
        {
            cgram.SetColor(RoomFxRomData.Layer3.EmptyPaletteColorIndex, 0);
            return;
        }
        if (!blends.TryGetValue(selection, out ushort[]? colors))
            throw new InvalidDataException($"Room-FX palette blend ${selection:X2} is not an authored retail selection.");
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(RoomFxRomData.Layer3.PaletteBlendDestinationIndex + index, colors[index]);
    }

    public ReadOnlySpan<ushort> Resolve(byte selection) => blends.TryGetValue(selection, out ushort[]? colors)
        ? colors : throw new InvalidDataException($"Room-FX palette blend ${selection:X2} is not an authored retail selection.");

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate room-FX blend property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record RoomFxPaletteBlendDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, PaletteRgb5[]> Blends { get; init; }
}

/// <summary>Native bank-$89 room-FX blend selectors, distinct from editable colors.</summary>
public static class RoomFxPaletteBlendDefinitions
{
    public const string FileName = "room-fx-blend-palettes.json";
    public const int Version = 1;

    /// <summary>FX-record selector $02, used primarily for lava/acid.</summary>
    public const byte Lava = 0x02;
    /// <summary>FX-record selector $22, used by Landing Site rain.</summary>
    public const byte LandingSiteRain = 0x22;
    /// <summary>FX-record selector $42, used by several Maridia water rooms.</summary>
    public const byte MaridiaWaterA = 0x42;
    /// <summary>FX-record selector $48, used by Ceres and other water/acid rooms.</summary>
    public const byte WaterAndAcid = 0x48;
    /// <summary>FX-record selector $62, used by fog and nonliquid room states.</summary>
    public const byte Fog = 0x62;
    /// <summary>FX-record selector $E2, used by western Maridia water rooms.</summary>
    public const byte MaridiaWaterB = 0xe2;
    /// <summary>FX-record selector $E8, used by central Maridia water rooms.</summary>
    public const byte MaridiaWaterC = 0xe8;
    /// <summary>FX-record selector $EE, used by eastern Maridia water rooms.</summary>
    public const byte MaridiaWaterD = 0xee;

    private static readonly byte[] AuthoredIds =
    [
        Lava, LandingSiteRain, MaridiaWaterA, WaterAndAcid,
        Fog, MaridiaWaterB, MaridiaWaterC, MaridiaWaterD,
    ];
    public static IReadOnlyList<byte> Ids { get; } = Array.AsReadOnly(AuthoredIds);

    public static string Key(byte id)
    {
        if (!AuthoredIds.Contains(id))
            throw new InvalidDataException($"Room-FX palette blend ${id:X2} is not catalogued.");
        return $"blend-{id:X2}";
    }

    /// <summary>Native byte address for the first of three adjacent BGR555 colors.</summary>
    public static int SourceAddress(byte id)
    {
        _ = Key(id);
        return RoomFxRomData.Tables.PaletteBlendColors + id;
    }
}
