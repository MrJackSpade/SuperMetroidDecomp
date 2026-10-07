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
        roomBackdrop = new StockBand(KraidPaletteSource.RoomBackdrop,
            Compile(document.RoomBackdrop, KraidPaletteSource.RoomBackdrop));
        initialTarget = new StockBand(KraidPaletteSource.InitialTarget,
            Compile(document.InitialTarget, KraidPaletteSource.InitialTarget));
        health = new HealthPalette(Compile(document.Health, KraidPaletteSource.Health));
        ushort[] suppliedSecondary = Compile(document.Secondary, KraidPaletteSource.Secondary);
        secondary = suppliedSecondary.SequenceEqual(health) ? health : new HealthPalette(suppliedSecondary);
        deathArm = new StockBand(KraidPaletteSource.DeathArm,
            Compile(document.DeathArm, KraidPaletteSource.DeathArm));
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
    /// One sixteen-color source stored as its supplied deviations from the stock paint in
    /// <see cref="KraidPaintDefinitions"/>; an unedited stock band stores nothing.
    /// </summary>
    private sealed class StockBand : IReadOnlyList<ushort>
    {
        private readonly KraidPaletteSource source;
        private readonly Dictionary<int, ushort> deviations = new();

        internal StockBand(KraidPaletteSource source, ushort[] supplied)
        {
            this.source = source;
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.Color(source, index) != supplied[index])
                    deviations.Add(index, supplied[index]);
        }

        public int Count => KraidPaletteRomData.ColorCount(source);
        public ushort this[int index] => deviations.TryGetValue(index, out ushort supplied)
            ? supplied : KraidPaintDefinitions.Color(source, index);

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Interpolates one channelwise health color for <paramref name="band"/> between the first
    /// normal band and the final band, to nearest integer over the seven intervals.
    /// </summary>
    internal static ushort InterpolateHealth(ushort first, ushort last, int band)
    {
        int result = 0;
        int intervals = KraidPaletteRomData.HealthBandCount - 2;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int start = first >> shift & 31;
            int end = last >> shift & 31;
            int delta = end - start;
            int channel = start + Math.Sign(delta) *
                ((Math.Abs(delta) * (band - 1) + intervals / 2) / intervals);
            result |= channel << shift;
        }
        return (ushort)result;
    }

    /// <summary>
    /// The nine $A7:B3D3/$B513 bands comprise a white flash and eight RGB5 health steps.
    /// The flash, both endpoint bands and the two authored color-six steps come from
    /// <see cref="KraidPaintDefinitions"/> unless supplied differently; interior bands
    /// interpolate the current endpoints, and their transparent slot is the final band's.
    /// Only supplied deviations are stored, so edited endpoints still drive every step.
    /// </summary>
    private sealed class HealthPalette : IReadOnlyList<ushort>
    {
        private readonly Dictionary<int, ushort> deviations = new();

        internal HealthPalette(ushort[] supplied)
        {
            // Authored cells first: interior interpolation reads the (possibly edited) endpoints.
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.HealthAuthored(index) is ushort stock && stock != supplied[index])
                    deviations.Add(index, supplied[index]);
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.HealthAuthored(index) is null && Calculate(index) != supplied[index])
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
            if (KraidPaintDefinitions.HealthAuthored(index) is ushort authored)
                return authored;
            int band = index / KraidPaletteRomData.BandColors;
            int color = index % KraidPaletteRomData.BandColors;
            int finalBand = (KraidPaletteRomData.HealthBandCount - 1) * KraidPaletteRomData.BandColors;
            if (color == 0)
                return this[finalBand];
            return InterpolateHealth(this[KraidPaletteRomData.BandColors + color], this[finalBand + color], band);
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.OrdinalIgnoreCase,
            name => new InvalidDataException($"Duplicate Kraid color property {name}."));

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
