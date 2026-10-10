using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Loading tint inputs with derived endpoint channels removed.</summary>
/// <remarks>Original $8D:DB62/DCC8/DE2E endpoint words preserve channels of
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
internal sealed class LoadingPaletteInputView : IReadOnlyDictionary<ushort, ushort>
{
    /// <summary>Base color entries after the eight editable suit-color entries are extracted.</summary>
    private readonly Dictionary<ushort, ushort> colors;
    /// <summary>Power dim slot1, $8D:DC7E.</summary>
    private readonly Channels powerDim1;
    /// <summary>Power dim slot2, $8D:DC80.</summary>
    private readonly Channels powerDim2;
    /// <summary>Power bright slot9, $8D:DBA1.</summary>
    private readonly Channels powerBright9;
    /// <summary>Power dim slot12, $8D:DC94.</summary>
    private readonly Channels powerDim12;
    /// <summary>Varia bright slot10, $8D:DD09.</summary>
    private readonly Channels variaBright10;
    /// <summary>Varia bright slot11, $8D:DD0B; shares slot10 blue.</summary>
    private readonly Channels variaBright11;
    /// <summary>Varia dim slot12, $8D:DDFA.</summary>
    private readonly Channels variaDim12;
    /// <summary>Gravity dim slot2, $8D:DF4C.</summary>
    private readonly Channels gravityDim2;

    /// <summary>Extracts authored tint overrides while retaining the remaining base palette entries.</summary>
    /// <param name="colors">Mutable pointer-to-color map from which the editable targets are removed.</param>
    internal LoadingPaletteInputView(Dictionary<ushort, ushort> colors)
    {
        this.colors = colors;
        powerDim1 = new(colors[LoadingSuitColorPointers.PowerDim1], Expected(LoadingSuitColorPointers.PowerDim1));
        powerDim2 = new(colors[LoadingSuitColorPointers.PowerDim2], Expected(LoadingSuitColorPointers.PowerDim2));
        powerBright9 = new(colors[LoadingSuitColorPointers.PowerBright9], Expected(LoadingSuitColorPointers.PowerBright9));
        powerDim12 = new(colors[LoadingSuitColorPointers.PowerDim12], Expected(LoadingSuitColorPointers.PowerDim12));
        variaBright10 = new(colors[LoadingSuitColorPointers.VariaBright10], Expected(LoadingSuitColorPointers.VariaBright10));
        variaBright11 = new(colors[LoadingSuitColorPointers.VariaBright11], Expected(LoadingSuitColorPointers.VariaBright11));
        variaDim12 = new(colors[LoadingSuitColorPointers.VariaDim12], Expected(LoadingSuitColorPointers.VariaDim12));
        gravityDim2 = new(colors[LoadingSuitColorPointers.GravityDim2], Expected(LoadingSuitColorPointers.GravityDim2));
        colors.Remove(LoadingSuitColorPointers.PowerDim1);
        colors.Remove(LoadingSuitColorPointers.PowerDim2);
        colors.Remove(LoadingSuitColorPointers.PowerBright9);
        colors.Remove(LoadingSuitColorPointers.PowerDim12);
        colors.Remove(LoadingSuitColorPointers.VariaBright10);
        colors.Remove(LoadingSuitColorPointers.VariaBright11);
        colors.Remove(LoadingSuitColorPointers.VariaDim12);
        colors.Remove(LoadingSuitColorPointers.GravityDim2);
    }

    /// <summary>Reads a required untinted base color from the retained palette entries.</summary>
    /// <param name="pointer">Bank-$8D palette pointer of the normal-color source.</param>
    /// <exception cref="InvalidDataException">The required source color is absent.</exception>
    private ushort Normal(ushort pointer) =>
        LoadingPaletteColorDefinitions.TryReadColor(pointer, colors, out ushort value)
            ? value : throw new InvalidDataException($"Missing loading base ${pointer:X4}.");

    /// <summary>Calculates the selected tint for one extracted loading-palette target.</summary>
    /// <param name="pointer">Bank-$8D pointer identifying a supported editable target.</param>
    /// <exception cref="ArgumentOutOfRangeException">The pointer is not one of the extracted targets.</exception>
    private ushort Expected(ushort pointer) => pointer switch
    {
        LoadingSuitColorPointers.PowerDim1 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6d), 2),
        LoadingSuitColorPointers.PowerDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6f), 2),
        LoadingSuitColorPointers.PowerBright9 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb7d), 0),
        LoadingSuitColorPointers.PowerDim12 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb83), 2),
        LoadingSuitColorPointers.VariaBright10 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce5), 0),
        LoadingSuitColorPointers.VariaBright11 => (ushort)((LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce7), 0) & 0x03ff) |
            (variaBright10.Apply(Expected(LoadingSuitColorPointers.VariaBright10)) & 0x7c00)),
        LoadingSuitColorPointers.VariaDim12 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce9), 2),
        LoadingSuitColorPointers.GravityDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xde3b), 2),
        _ => throw new ArgumentOutOfRangeException(nameof(pointer)),
    };

    /// <summary>Stores only independently authored RGB5 channels so shared tint relationships remain derived.</summary>
    internal readonly struct Channels
    {
        /// <summary>Optional authored five-bit red value; null means use the calculated tint channel.</summary>
        private readonly int? red;
        /// <summary>Optional authored five-bit green value; null means use the calculated tint channel.</summary>
        private readonly int? green;
        /// <summary>Optional authored five-bit blue value; null means use the calculated tint channel.</summary>
        private readonly int? blue;
        /// <summary>Captures differing channels; independentMask bits0/1/2 always retain red/green/blue inputs.</summary>
        /// <param name="supplied">Authored packed BGR555 color from the editable source.</param>
        /// <param name="expected">Calculated color against which dependent channels are compared.</param>
        /// <param name="independentMask">Bits that force the corresponding red, green, or blue input to remain explicit.</param>
        internal Channels(ushort supplied, ushort expected, int independentMask = 0)
        {
            red = (independentMask & 1) == 0 && (supplied & 31) == (expected & 31) ? null : supplied & 31;
            green = (independentMask & 2) == 0 && (supplied >> 5 & 31) == (expected >> 5 & 31) ? null : supplied >> 5 & 31;
            blue = (independentMask & 4) == 0 && (supplied >> 10 & 31) == (expected >> 10 & 31) ? null : supplied >> 10 & 31;
        }
        /// <summary>Combines explicit channels with the calculated values for all dependent channels.</summary>
        /// <param name="expected">Calculated packed BGR555 color supplying channels without overrides.</param>
        /// <returns>The packed color after applying this record's authored channel differences.</returns>
        internal ushort Apply(ushort expected) => (ushort)((red ?? (expected & 31)) |
            (green ?? (expected >> 5 & 31)) << 5 | (blue ?? (expected >> 10 & 31)) << 10);
    }

    /// <summary>Looks up either an editable tint target or a retained normal palette entry.</summary>
    /// <param name="pointer">Bank-$8D palette pointer to resolve.</param>
    /// <param name="value">Receives the packed color when the pointer is present.</param>
    /// <returns><see langword="true"/> if the pointer resolves in either part of the view.</returns>
    public bool TryGetValue(ushort pointer, out ushort value)
    {
        switch (pointer)
        {
            case LoadingSuitColorPointers.PowerDim1: value = powerDim1.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.PowerDim2: value = powerDim2.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.PowerBright9: value = powerBright9.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.PowerDim12: value = powerDim12.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.VariaBright10: value = variaBright10.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.VariaBright11: value = variaBright11.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.VariaDim12: value = variaDim12.Apply(Expected(pointer)); return true;
            case LoadingSuitColorPointers.GravityDim2: value = gravityDim2.Apply(Expected(pointer)); return true;
            default: return colors.TryGetValue(pointer, out value);
        }
    }
    /// <summary>Gets the packed color associated with a palette pointer.</summary>
    /// <param name="key">Bank-$8D palette pointer to resolve.</param>
    /// <exception cref="KeyNotFoundException">No retained color or extracted tint target has this pointer.</exception>
    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();
    /// <summary>Gets the number of retained palette entries plus the eight extracted tint targets.</summary>
    public int Count => colors.Count + 8;
    /// <summary>Determines whether either the retained entries or extracted targets contain a pointer.</summary>
    /// <param name="key">Bank-$8D palette pointer to test.</param>
    /// <returns><see langword="true"/> when lookup can resolve the pointer.</returns>
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    /// <summary>Enumerates retained pointers followed by the eight extracted loading-tint targets.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return LoadingSuitColorPointers.PowerDim1; yield return LoadingSuitColorPointers.PowerDim2; yield return LoadingSuitColorPointers.PowerBright9; yield return LoadingSuitColorPointers.PowerDim12;
            yield return LoadingSuitColorPointers.VariaBright10; yield return LoadingSuitColorPointers.VariaBright11; yield return LoadingSuitColorPointers.VariaDim12; yield return LoadingSuitColorPointers.GravityDim2;
        }
    }
    /// <summary>Enumerates packed colors in the same order as <see cref="Keys"/>.</summary>
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);
    /// <summary>Enumerates pointer-color pairs in key order, resolving each value through this view.</summary>
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
