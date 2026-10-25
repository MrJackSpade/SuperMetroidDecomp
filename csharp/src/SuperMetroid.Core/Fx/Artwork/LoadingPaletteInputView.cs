using System.Collections;
using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Loading tint inputs with derived endpoint channels removed.</summary>
/// <remarks>Original loading endpoint words preserve channels of
/// normal suit colors and the reviewed additive tint rules. Only differing
/// components are stored. The nine independent choices are Power dim1 red14/
/// green6, dim2 blue13, bright9 blue21, dim12 red22/green16; Varia bright10
/// blue29 and dim12 red13; Gravity dim2 blue27. These specify the chosen tint
/// of categorical sprite inks. Native $91:DD5B copies the corresponding bank9B
/// colors into fixed OBJ slots, while the loading program repeats the same art;
/// neither supplies illumination, temperature or another quantity deriving these
/// tint targets. Temporal shade and shared-channel relationships are calculated
/// separately. An index fit for these remaining choices would only encode the
/// painting, the #1165 nonsense exception. This narrow disposition does not
/// exempt other normal/speed-boost palettes or their unreviewed relationships.
/// Custom differences remain nullable overrides, never generated color caches.</remarks>
internal sealed class LoadingPaletteInputView : IReadOnlyDictionary<PaletteFxColorKey, RgbColor>
{
    /// <summary>The eight tinted endpoints, by the color each names.</summary>
    private static readonly Dictionary<PaletteFxColorKey, LoadingSuitColor> Endpoints =
        Enum.GetValues<LoadingSuitColor>().ToDictionary(LoadingPaletteColorDefinitions.Key);

    /// <summary>Ordinary stored suit colors after the eight selected tinted endpoints are removed.</summary>
    private readonly Dictionary<PaletteFxColorKey, RgbColor> colors;
    /// <summary>Power dim slot1.</summary>
    internal readonly Channels powerDim1;
    /// <summary>Power dim slot2.</summary>
    internal readonly Channels powerDim2;
    /// <summary>Power bright slot9.</summary>
    internal readonly Channels powerBright9;
    /// <summary>Power dim slot12.</summary>
    internal readonly Channels powerDim12;
    /// <summary>Varia bright slot10.</summary>
    internal readonly Channels variaBright10;
    /// <summary>Varia bright slot11; shares slot10 blue.</summary>
    internal readonly Channels variaBright11;
    /// <summary>Varia dim slot12.</summary>
    internal readonly Channels variaDim12;
    /// <summary>Gravity dim slot2.</summary>
    internal readonly Channels gravityDim2;

    /// <summary>Captures only channels differing from calculated tint endpoints, then removes those endpoints from ordinary storage.</summary>
    internal LoadingPaletteInputView(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        this.colors = colors;
        RgbColor Supplied(LoadingSuitColor slot) => colors[LoadingPaletteColorDefinitions.Key(slot)];
        powerDim1 = new(Supplied(LoadingSuitColor.PowerDim1), Expected(LoadingSuitColor.PowerDim1));
        powerDim2 = new(Supplied(LoadingSuitColor.PowerDim2), Expected(LoadingSuitColor.PowerDim2));
        powerBright9 = new(Supplied(LoadingSuitColor.PowerBright9), Expected(LoadingSuitColor.PowerBright9));
        powerDim12 = new(Supplied(LoadingSuitColor.PowerDim12), Expected(LoadingSuitColor.PowerDim12));
        variaBright10 = new(Supplied(LoadingSuitColor.VariaBright10), Expected(LoadingSuitColor.VariaBright10));
        variaBright11 = new(Supplied(LoadingSuitColor.VariaBright11), Expected(LoadingSuitColor.VariaBright11));
        variaDim12 = new(Supplied(LoadingSuitColor.VariaDim12), Expected(LoadingSuitColor.VariaDim12));
        gravityDim2 = new(Supplied(LoadingSuitColor.GravityDim2), Expected(LoadingSuitColor.GravityDim2));
        foreach (PaletteFxColorKey endpoint in Endpoints.Keys)
            colors.Remove(endpoint);
    }

    /// <summary>Requires the normal-suit source color used to derive a loading tint endpoint.</summary>
    private RgbColor Normal(PaletteFxColorKey key) =>
        LoadingPaletteColorDefinitions.TryReadColor(key, colors, out RgbColor value)
            ? value : throw new InvalidDataException($"Missing loading base {key}.");

    /// <summary>Calculates the selected loading tint from a normal-suit source and shared channel rules.</summary>
    private RgbColor Expected(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => LoadingPaletteColorDefinitions.TintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.PowerDim1)), 2),
        LoadingSuitColor.PowerDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.PowerDim2)), 2),
        LoadingSuitColor.PowerBright9 => LoadingPaletteColorDefinitions.TintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.PowerBright9)), 0),
        LoadingSuitColor.PowerDim12 => LoadingPaletteColorDefinitions.TintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.PowerDim12)), 2),
        LoadingSuitColor.VariaBright10 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.VariaBright10)), 0),
        LoadingSuitColor.VariaBright11 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.VariaBright11)), 0)
            .WithBlue(variaBright10.Apply(Expected(LoadingSuitColor.VariaBright10)).Blue),
        LoadingSuitColor.VariaDim12 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.VariaDim12)), 2),
        LoadingSuitColor.GravityDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(LoadingPaletteColorDefinitions.NormalSource(LoadingSuitColor.GravityDim2)), 2),
        _ => throw new InvalidOperationException($"Undefined loading suit color {slot}."),
    };

    /// <summary>Nullable channel overrides for a tint endpoint whose other channels follow the common rule.</summary>
    internal readonly struct Channels
    {
        /// <summary>Independent red value, or null when the calculated endpoint supplies it.</summary>
        internal readonly int? red;
        /// <summary>Independent green value, or null when the calculated endpoint supplies it.</summary>
        internal readonly int? green;
        /// <summary>Independent blue value, or null when the calculated endpoint supplies it.</summary>
        internal readonly int? blue;
        /// <summary>Captures differing channels; independentMask bits0/1/2 always retain red/green/blue inputs.</summary>
        internal Channels(RgbColor supplied, RgbColor expected, int independentMask = 0)
        {
            red = (independentMask & 1) == 0 && supplied.Red == expected.Red ? null : supplied.Red;
            green = (independentMask & 2) == 0 && supplied.Green == expected.Green ? null : supplied.Green;
            blue = (independentMask & 4) == 0 && supplied.Blue == expected.Blue ? null : supplied.Blue;
        }
        /// <summary>Overlays retained independent channels on the calculated normal-suit tint.</summary>
        internal RgbColor Apply(RgbColor expected) => new(red ?? expected.Red, green ?? expected.Green, blue ?? expected.Blue);
    }

    /// <summary>Resolves an ordinary stored color or reconstructs one of eight derived loading-tint endpoints.</summary>
    public bool TryGetValue(PaletteFxColorKey key, out RgbColor value)
    {
        // Colors other than the eight tinted endpoints are ordinary stored colors.
        if (!Endpoints.TryGetValue(key, out LoadingSuitColor slot))
            return colors.TryGetValue(key, out value);
        Channels channels = slot switch
        {
            LoadingSuitColor.PowerDim1 => powerDim1,
            LoadingSuitColor.PowerDim2 => powerDim2,
            LoadingSuitColor.PowerBright9 => powerBright9,
            LoadingSuitColor.PowerDim12 => powerDim12,
            LoadingSuitColor.VariaBright10 => variaBright10,
            LoadingSuitColor.VariaBright11 => variaBright11,
            LoadingSuitColor.VariaDim12 => variaDim12,
            LoadingSuitColor.GravityDim2 => gravityDim2,
            _ => throw new InvalidOperationException($"Undefined loading suit color {slot}."),
        };
        value = channels.Apply(Expected(slot));
        return true;
    }
    /// <summary>Requires a stored or derived loading color.</summary>
    public RgbColor this[PaletteFxColorKey key] => TryGetValue(key, out RgbColor value) ? value : throw new KeyNotFoundException();
    /// <summary>Includes ordinary stored colors and the eight reconstructed tint endpoints.</summary>
    public int Count => colors.Count + Endpoints.Count;
    /// <summary>Checks whether a color is stored or derived.</summary>
    public bool ContainsKey(PaletteFxColorKey key) => TryGetValue(key, out _);
    /// <summary>Enumerates stored colors followed by the eight derived tint endpoints.</summary>
    public IEnumerable<PaletteFxColorKey> Keys => colors.Keys.Concat(Endpoints.Keys);
    /// <summary>Resolves values in the same stored-then-derived order as <see cref="Keys"/>.</summary>
    public IEnumerable<RgbColor> Values => Keys.Select(key => this[key]);
    /// <summary>Enumerates stored and reconstructed colors.</summary>
    public IEnumerator<KeyValuePair<PaletteFxColorKey, RgbColor>> GetEnumerator()
    {
        foreach (PaletteFxColorKey key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Exposes the same loading color pairs to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
