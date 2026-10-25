using System.Collections;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Installed palette inputs with calculated heat endpoint channels.</summary>
/// <remarks>
/// The red endpoint preserves the base green/blue of its phase-zero color; the mixed endpoint preserves the
/// base green-minus-blue difference of its phase-zero color. Only the shared red target and the
/// mixed target's red/green are independent stock inputs. They specify the chosen
/// heat tint, not samples indexed by time or distance. The native consumer supplies
/// no temperature/intensity parameter deriving those targets: it selects painted
/// palette colors. Inventing an index formula for these three choices would merely
/// encode the chosen tint, the #1165 nonsense exception. All temporal interpolation,
/// shared channels and repeated rows are calculated separately. This does not exempt
/// the underlying normal suit artwork from its own review. Nonmatching player channel
/// edits remain explicit overrides. No complete stock endpoint word is cached.
/// </remarks>
internal sealed class HeatPaletteInputView : IReadOnlyDictionary<PaletteFxColorKey, RgbColor>
{
    /// <summary>Installed base and edited heat colors excluding the two calculated endpoints.</summary>
    private readonly Dictionary<PaletteFxColorKey, RgbColor> colors;
    /// <summary>Selected red channel of the red heat endpoint.</summary>
    private readonly int redTarget;
    /// <summary>Selected red channel of the mixed endpoint.</summary>
    private readonly int mixedRedTarget;
    /// <summary>Selected green channel of the mixed endpoint.</summary>
    private readonly int mixedGreenTarget;
    /// <summary>Player edit to the red endpoint's green when it differs from the current base color.</summary>
    internal readonly int? greenOverride;
    /// <summary>Player edit to the red endpoint's blue when it differs from the current base color.</summary>
    internal readonly int? blueOverride;
    /// <summary>Player edit to the mixed endpoint's blue when it differs from the preserved base green-blue difference.</summary>
    internal readonly int? mixedBlueOverride;

    /// <summary>Separates selected heat-tint channels from base-dependent channels and retains nonmatching edits.</summary>
    internal HeatPaletteInputView(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        this.colors = colors;
        RgbColor red = colors[HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Red)];
        RgbColor mixed = colors[HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Mixed)];
        RgbColor redBase = ReadBase(HeatPaletteColorDefinitions.RedEndpointBase);
        RgbColor mixedBase = ReadBase(HeatPaletteColorDefinitions.MixedEndpointBase);
        redTarget = red.Red;
        mixedRedTarget = mixed.Red;
        mixedGreenTarget = mixed.Green;
        greenOverride = red.Green == redBase.Green ? null : red.Green;
        blueOverride = red.Blue == redBase.Blue ? null : red.Blue;
        int calculatedBlue = mixedBase.Blue + mixedGreenTarget - mixedBase.Green;
        mixedBlueOverride = mixed.Blue == calculatedBlue ? null : mixed.Blue;
        colors.Remove(HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Red));
        colors.Remove(HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Mixed));
    }

    /// <summary>Resolves a base suit color from installed entries or its compiled loading-palette alias.</summary>
    private RgbColor ReadBase(PaletteFxColorKey key)
    {
        if (colors.TryGetValue(key, out RgbColor value)) return value;
        if (HeatPaletteColorDefinitions.TryBasePalette(key, out PaletteFxColorKey source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value)) return value;
        throw new InvalidDataException($"Missing installed heat base {key}.");
    }

    /// <summary>Reconstructs the two endpoints from selected channels and live base colors, or reads another installed entry.</summary>
    public bool TryGetValue(PaletteFxColorKey key, out RgbColor value)
    {
        if (key == HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Red))
        {
            RgbColor original = ReadBase(HeatPaletteColorDefinitions.RedEndpointBase);
            value = new(redTarget, greenOverride ?? original.Green, blueOverride ?? original.Blue);
            return true;
        }
        if (key == HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Mixed))
        {
            RgbColor original = ReadBase(HeatPaletteColorDefinitions.MixedEndpointBase);
            int blue = mixedBlueOverride ?? (original.Blue + mixedGreenTarget - original.Green);
            value = new(mixedRedTarget, mixedGreenTarget, blue);
            return true;
        }
        return colors.TryGetValue(key, out value);
    }

    /// <summary>Resolves a heat color or rejects an absent one.</summary>
    public RgbColor this[PaletteFxColorKey key] => TryGetValue(key, out RgbColor value) ? value : throw new KeyNotFoundException();
    /// <summary>Installed entries plus the two calculated heat endpoints.</summary>
    public int Count => colors.Count + 2;
    /// <summary>Recognizes either calculated endpoint or a remaining installed color.</summary>
    public bool ContainsKey(PaletteFxColorKey key) =>
        key == HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Red) ||
        key == HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Mixed) ||
        colors.ContainsKey(key);
    /// <summary>Enumerates installed colors followed by the two calculated endpoints.</summary>
    public IEnumerable<PaletteFxColorKey> Keys
    {
        get
        {
            foreach (PaletteFxColorKey key in colors.Keys) yield return key;
            yield return HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Red);
            yield return HeatPaletteColorDefinitions.Key(HeatPaletteEndpoint.Mixed);
        }
    }
    /// <summary>Resolves every current heat color in key-enumeration order.</summary>
    public IEnumerable<RgbColor> Values => Keys.Select(key => this[key]);
    /// <summary>Yields colors paired with their current derived or installed value.</summary>
    public IEnumerator<KeyValuePair<PaletteFxColorKey, RgbColor>> GetEnumerator()
    {
        foreach (PaletteFxColorKey key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Exposes the same color pairs to non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
