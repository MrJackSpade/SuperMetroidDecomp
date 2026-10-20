using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

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
            void Append(KraidPaletteSource source, IReadOnlyList<Bgr555> row)
            {
                content.Append("source", (int)source);
                content.AppendColors("colors", row.ToArray());
            }
        });

    /// <summary>Target colors for Kraid's room-backdrop fade.</summary>
    internal readonly IReadOnlyList<Bgr555> roomBackdrop;
    /// <summary>Rock sprite target colors staged during initial setup.</summary>
    internal readonly IReadOnlyList<Bgr555> initialTarget;
    /// <summary>White hurt flash and eight body health bands.</summary>
    internal readonly IReadOnlyList<Bgr555> health;
    /// <summary>Independently editable sprite flash and health bands.</summary>
    internal readonly IReadOnlyList<Bgr555> secondary;
    /// <summary>Arm and body death colors installed before the fade.</summary>
    internal readonly IReadOnlyList<Bgr555> deathArm;

    /// <summary>Compiles five authored sources, sharing health-band storage only when secondary colors match exactly.</summary>
    private KraidColorCatalog(KraidColorDocument document)
    {
        roomBackdrop = new StockBand(KraidPaletteSource.RoomBackdrop,
            Compile(document.RoomBackdrop, KraidPaletteSource.RoomBackdrop));
        initialTarget = new StockBand(KraidPaletteSource.InitialTarget,
            Compile(document.InitialTarget, KraidPaletteSource.InitialTarget));
        health = new HealthPalette(Compile(document.Health, KraidPaletteSource.Health));
        Bgr555[] suppliedSecondary = Compile(document.Secondary, KraidPaletteSource.Secondary);
        secondary = suppliedSecondary.SequenceEqual(health) ? health : new HealthPalette(suppliedSecondary);
        deathArm = new StockBand(KraidPaletteSource.DeathArm,
            Compile(document.DeathArm, KraidPaletteSource.DeathArm));
    }

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public Bgr555 Resolve(KraidPaletteSource source, int index)
    {
        IReadOnlyList<Bgr555> band = source switch
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
    internal sealed class StockBand : IReadOnlyList<Bgr555>
    {
        /// <summary>Named native source that supplies calculated unedited colors.</summary>
        private readonly KraidPaletteSource source;
        /// <summary>Only supplied colors that differ from the reviewed stock paint.</summary>
        internal readonly Dictionary<int, Bgr555> deviations = new();

        /// <summary>Records independent edits to one sixteen-color source.</summary>
        internal StockBand(KraidPaletteSource source, Bgr555[] supplied)
        {
            this.source = source;
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.Color(source, index) != supplied[index])
                    deviations.Add(index, supplied[index]);
        }

        /// <summary>Native color count of this named source.</summary>
        public int Count => KraidPaletteRomData.ColorCount(source);
        /// <summary>Edited color at an index, or its calculated stock paint.</summary>
        public Bgr555 this[int index] => deviations.TryGetValue(index, out Bgr555 supplied)
            ? supplied : KraidPaintDefinitions.Color(source, index);

        /// <summary>Visits source colors in their native palette order.</summary>
        public IEnumerator<Bgr555> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same palette order to non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Interpolates one channelwise health color for <paramref name="band"/> between the first
    /// normal band and the final band, to nearest integer over the seven intervals.
    /// </summary>
    internal static Bgr555 InterpolateHealth(Bgr555 first, Bgr555 last, int band)
    {
        int intervals = KraidPaletteRomData.HealthBandCount - 2;
        return first.Zip(last, (_, start, end) =>
        {
            int delta = end - start;
            return start + Math.Sign(delta) * ((Math.Abs(delta) * (band - 1) + intervals / 2) / intervals);
        });
    }

    /// <summary>
    /// The nine $A7:B3D3/$B513 bands comprise a white flash and eight RGB5 health steps.
    /// The flash, both endpoint bands and the two authored color-six steps come from
    /// <see cref="KraidPaintDefinitions"/> unless supplied differently; interior bands
    /// interpolate the current endpoints, and their transparent slot is the final band's.
    /// Only supplied deviations are stored, so edited endpoints still drive every step.
    /// </summary>
    internal sealed class HealthPalette : IReadOnlyList<Bgr555>
    {
        /// <summary>Edited authored endpoints or interior samples that differ from calculated interpolation.</summary>
        internal readonly Dictionary<int, Bgr555> deviations = new();

        /// <summary>Records endpoint edits before comparing interior colors against those selected endpoints.</summary>
        internal HealthPalette(Bgr555[] supplied)
        {
            // Authored cells first: interior interpolation reads the (possibly edited) endpoints.
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.HealthAuthored(index) is Bgr555 stock && stock != supplied[index])
                    deviations.Add(index, supplied[index]);
            for (int index = 0; index < supplied.Length; index++)
                if (KraidPaintDefinitions.HealthAuthored(index) is null && Calculate(index) != supplied[index])
                    deviations.Add(index, supplied[index]);
        }

        /// <summary>White flash plus eight native sixteen-color health bands.</summary>
        public int Count => KraidPaletteRomData.HealthBandCount * KraidPaletteRomData.BandColors;
        /// <summary>Selected health color, preserving independent edits over calculated bands.</summary>
        public Bgr555 this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return deviations.TryGetValue(index, out Bgr555 supplied) ? supplied : Calculate(index);
            }
        }

        /// <summary>Uses authored flash/endpoints or interpolates an interior band from the current selected endpoints.</summary>
        private Bgr555 Calculate(int index)
        {
            if (KraidPaintDefinitions.HealthAuthored(index) is Bgr555 authored)
                return authored;
            int band = index / KraidPaletteRomData.BandColors;
            int color = index % KraidPaletteRomData.BandColors;
            int finalBand = (KraidPaletteRomData.HealthBandCount - 1) * KraidPaletteRomData.BandColors;
            if (color == 0)
                return this[finalBand];
            return InterpolateHealth(this[KraidPaletteRomData.BandColors + color], this[finalBand + color], band);
        }

        /// <summary>Visits flash and health bands in their native contiguous order.</summary>
        public IEnumerator<Bgr555> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same health-band order to non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Validates and compiles all five Kraid RGB5 sources, preserving independent authored edits while leaving health-band selection and fade timing to the boss mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open; property names are matched case-insensitively.</param>
    /// <returns>Compiled packed-color catalog detached from the document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON has duplicate or unknown properties, an unsupported version, incorrect source lengths, null colors, or RGB5 channels outside 0..31.</exception>
    public static KraidColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        KraidColorDocument document;
        document = JsonAssetDocument.Read<KraidColorDocument>(json, JsonOptions, "Kraid colors");
        if (document.Version != KraidColorFormat.Version)
            throw new InvalidDataException("Kraid colors require the supported version.");
        return new(document);
    }

    /// <summary>Serializes the five editable sources to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Complete palette sources to serialize; the writer does not retain their arrays.</param>
    /// <returns>Validated JSON bytes for <see cref="KraidColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, palette-length, or RGB5-channel validation.</exception>
    public static byte[] Write(KraidColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, JsonOptions, json => _ = Load(json));

    /// <summary>Validates one source's native length and RGB5 channels before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, KraidPaletteSource kind)
    {
        int expected = KraidPaletteRomData.ColorCount(kind);
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Kraid {kind} requires {expected} colors.");
        var result = new Bgr555[expected];
        for (int index = 0; index < expected; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Kraid {kind} color {index} requires RGB5 channels 0..31.");
            result[index] = rgb.ToBgr555();
        }
        return result;
    }

    /// <summary>Writes indented camel-case JSON while accepting case-insensitive property names and rejecting unknown fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
