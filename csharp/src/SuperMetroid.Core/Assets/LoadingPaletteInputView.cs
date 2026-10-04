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

    internal LoadingPaletteInputView(Dictionary<ushort, ushort> colors)
    {
        this.colors = colors;
        powerDim1 = new(colors[0xdc7e], Expected(0xdc7e));
        powerDim2 = new(colors[0xdc80], Expected(0xdc80));
        powerBright9 = new(colors[0xdba1], Expected(0xdba1));
        powerDim12 = new(colors[0xdc94], Expected(0xdc94));
        variaBright10 = new(colors[0xdd09], Expected(0xdd09));
        variaBright11 = new(colors[0xdd0b], Expected(0xdd0b));
        variaDim12 = new(colors[0xddfa], Expected(0xddfa));
        gravityDim2 = new(colors[0xdf4c], Expected(0xdf4c));
        colors.Remove(0xdc7e);
        colors.Remove(0xdc80);
        colors.Remove(0xdba1);
        colors.Remove(0xdc94);
        colors.Remove(0xdd09);
        colors.Remove(0xdd0b);
        colors.Remove(0xddfa);
        colors.Remove(0xdf4c);
    }

    private ushort Normal(ushort pointer) =>
        LoadingPaletteColorDefinitions.TryReadColor(pointer, colors, out ushort value)
            ? value : throw new InvalidDataException($"Missing loading base ${pointer:X4}.");

    private ushort Expected(ushort pointer) => pointer switch
    {
        0xdc7e => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6d), 2),
        0xdc80 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6f), 2),
        0xdba1 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb7d), 0),
        0xdc94 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb83), 2),
        0xdd09 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce5), 0),
        0xdd0b => (ushort)((LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce7), 0) & 0x03ff) |
            (variaBright10.Apply(Expected(0xdd09)) & 0x7c00)),
        0xddfa => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce9), 2),
        0xdf4c => LoadingPaletteColorDefinitions.TintColor(Normal(0xde3b), 2),
        _ => throw new ArgumentOutOfRangeException(nameof(pointer)),
    };

    internal readonly struct Channels
    {
        private readonly int? red;
        private readonly int? green;
        private readonly int? blue;
        /// <summary>Captures differing channels; independentMask bits0/1/2 always retain red/green/blue inputs.</summary>
        internal Channels(ushort supplied, ushort expected, int independentMask = 0)
        {
            red = (independentMask & 1) == 0 && (supplied & 31) == (expected & 31) ? null : supplied & 31;
            green = (independentMask & 2) == 0 && (supplied >> 5 & 31) == (expected >> 5 & 31) ? null : supplied >> 5 & 31;
            blue = (independentMask & 4) == 0 && (supplied >> 10 & 31) == (expected >> 10 & 31) ? null : supplied >> 10 & 31;
        }
        internal ushort Apply(ushort expected) => (ushort)((red ?? (expected & 31)) |
            (green ?? (expected >> 5 & 31)) << 5 | (blue ?? (expected >> 10 & 31)) << 10);
    }

    public bool TryGetValue(ushort pointer, out ushort value)
    {
        switch (pointer)
        {
            case 0xdc7e: value = powerDim1.Apply(Expected(pointer)); return true;
            case 0xdc80: value = powerDim2.Apply(Expected(pointer)); return true;
            case 0xdba1: value = powerBright9.Apply(Expected(pointer)); return true;
            case 0xdc94: value = powerDim12.Apply(Expected(pointer)); return true;
            case 0xdd09: value = variaBright10.Apply(Expected(pointer)); return true;
            case 0xdd0b: value = variaBright11.Apply(Expected(pointer)); return true;
            case 0xddfa: value = variaDim12.Apply(Expected(pointer)); return true;
            case 0xdf4c: value = gravityDim2.Apply(Expected(pointer)); return true;
            default: return colors.TryGetValue(pointer, out value);
        }
    }
    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();
    public int Count => colors.Count + 8;
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return 0xdc7e; yield return 0xdc80; yield return 0xdba1; yield return 0xdc94;
            yield return 0xdd09; yield return 0xdd0b; yield return 0xddfa; yield return 0xdf4c;
        }
    }
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
