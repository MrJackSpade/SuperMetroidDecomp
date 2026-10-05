using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Kraid RGB5 sources; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            Append(KraidPaletteSource.RoomBackdrop, roomBackdrop);
            Append(KraidPaletteSource.InitialTarget, initialTarget);
            Append(KraidPaletteSource.Health, health);
            Append(KraidPaletteSource.Secondary, secondary);
            Append(KraidPaletteSource.DeathArm, deathArm);
            void Append(KraidPaletteSource source, IReadOnlyList<ushort> row)
            {
                content.Append("source", (int)source);
                content.AppendWords("colors", row.ToArray());
            }
        });

    private readonly IReadOnlyList<ushort> roomBackdrop;
    private readonly IReadOnlyList<ushort> initialTarget;
    private readonly IReadOnlyList<ushort> health;
    private readonly IReadOnlyList<ushort> secondary;
    private readonly IReadOnlyList<ushort> deathArm;

    private KraidColorCatalog(KraidColorDocument document)
    {
        roomBackdrop = Compile(document.RoomBackdrop, KraidPaletteSource.RoomBackdrop);
        initialTarget = Compile(document.InitialTarget, KraidPaletteSource.InitialTarget);
        health = new HealthPalette(Compile(document.Health, KraidPaletteSource.Health));
        ushort[] suppliedSecondary = Compile(document.Secondary, KraidPaletteSource.Secondary);
        secondary = suppliedSecondary.SequenceEqual(health) ? health : new HealthPalette(suppliedSecondary);
        deathArm = Compile(document.DeathArm, KraidPaletteSource.DeathArm);
    }

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public ushort Resolve(KraidPaletteSource source, int index)
    {
        IReadOnlyList<ushort> band = source switch
        {
            KraidPaletteSource.RoomBackdrop => roomBackdrop,
            KraidPaletteSource.InitialTarget => initialTarget,
            KraidPaletteSource.Health => health,
            KraidPaletteSource.Secondary => secondary,
            KraidPaletteSource.DeathArm => deathArm,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
        if ((uint)index >= band.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return band[index];
    }

    /// <summary>
    /// The nine $A7:B3D3/$B513 bands comprise a white flash and eight RGB5
    /// health steps. Normal channels interpolate their endpoint colors to nearest
    /// integer. The transparent slot switches from the first to final backdrop
    /// after the first normal band. Exact supplied deviations remain independent.
    /// Endpoint artwork and the two original color-six deviations still require
    /// separate review under #1165; they are not retention exemptions.
    /// </summary>
    private sealed class HealthPalette : IReadOnlyList<ushort>
    {
        private readonly ushort[] first;
        private readonly ushort[] last;
        private readonly Dictionary<int, ushort> deviations = new();

        internal HealthPalette(ushort[] supplied)
        {
            first = supplied.AsSpan(KraidPaletteRomData.BandColors, KraidPaletteRomData.BandColors).ToArray();
            last = supplied.AsSpan((KraidPaletteRomData.HealthBandCount - 1) * KraidPaletteRomData.BandColors).ToArray();

            for (int index = 0; index < supplied.Length; index++)
                if (Calculate(index) != supplied[index])
                    deviations.Add(index, supplied[index]);
        }

        public int Count => KraidPaletteRomData.HealthBandCount * KraidPaletteRomData.BandColors;
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return deviations.TryGetValue(index, out ushort supplied) ? supplied : Calculate(index);
            }
        }

        private ushort Calculate(int index)
        {
            int band = index / KraidPaletteRomData.BandColors;
            int color = index % KraidPaletteRomData.BandColors;
            // Every channel saturates during the stock hit flash.
            if (band == 0)
                return 31 | 31 << 5 | 31 << 10;
            if (color == 0)
                return band == 1 ? first[color] : last[color];
            int result = 0;
            int intervals = KraidPaletteRomData.HealthBandCount - 2;
            for (int shift = 0; shift <= 10; shift += 5)
            {
                int start = first[color] >> shift & 31;
                int end = last[color] >> shift & 31;
                int delta = end - start;
                int channel = start + Math.Sign(delta) *
                    ((Math.Abs(delta) * (band - 1) + intervals / 2) / intervals);
                result |= channel << shift;
            }
            return (ushort)result;
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static KraidColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        KraidColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<KraidColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Kraid colors JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Kraid colors JSON.", error);
        }
        if (document.Version != KraidColorFormat.Version)
            throw new InvalidDataException("Kraid colors require the supported version.");
        return new(document);
    }

    public static byte[] Write(KraidColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, KraidPaletteSource kind)
    {
        int expected = KraidPaletteRomData.ColorCount(kind);
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Kraid {kind} requires {expected} colors.");
        var result = new ushort[expected];
        for (int index = 0; index < expected; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Kraid {kind} color {index} requires RGB5 channels 0..31.");
            result[index] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Kraid color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record KraidColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] RoomBackdrop { get; init; }
    public required PaletteRgb5[] InitialTarget { get; init; }
    public required PaletteRgb5[] Health { get; init; }
    public required PaletteRgb5[] Secondary { get; init; }
    public required PaletteRgb5[] DeathArm { get; init; }
}

public static class KraidColorFormat
{
    public const string FileName = "kraid-colors.json";
    public const int Version = 1;
}
