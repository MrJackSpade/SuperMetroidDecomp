using SuperMetroid.Core.Hardware;
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
internal sealed class LoadingPaletteInputView : IReadOnlyDictionary<ushort, Bgr555>
{
    private readonly Dictionary<ushort, Bgr555> colors;
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

    internal LoadingPaletteInputView(Dictionary<ushort, Bgr555> colors)
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

    private Bgr555 Normal(ushort pointer) =>
        LoadingPaletteColorDefinitions.TryReadColor(pointer, colors, out Bgr555 value)
            ? value : throw new InvalidDataException($"Missing loading base ${pointer:X4}.");

    private Bgr555 Expected(ushort pointer) => pointer switch
    {
        LoadingSuitColorPointers.PowerDim1 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6d), 2),
        LoadingSuitColorPointers.PowerDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6f), 2),
        LoadingSuitColorPointers.PowerBright9 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb7d), 0),
        LoadingSuitColorPointers.PowerDim12 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb83), 2),
        LoadingSuitColorPointers.VariaBright10 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce5), 0),
        LoadingSuitColorPointers.VariaBright11 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce7), 0)
            .WithBlue(variaBright10.Apply(Expected(LoadingSuitColorPointers.VariaBright10)).Blue),
        LoadingSuitColorPointers.VariaDim12 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce9), 2),
        LoadingSuitColorPointers.GravityDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xde3b), 2),
        _ => throw new ArgumentOutOfRangeException(nameof(pointer)),
    };

    internal readonly struct Channels
    {
        private readonly int? red;
        private readonly int? green;
        private readonly int? blue;
        /// <summary>Captures differing channels; independentMask bits0/1/2 always retain red/green/blue inputs.</summary>
        internal Channels(Bgr555 supplied, Bgr555 expected, int independentMask = 0)
        {
            red = (independentMask & 1) == 0 && supplied.Red == expected.Red ? null : supplied.Red;
            green = (independentMask & 2) == 0 && supplied.Green == expected.Green ? null : supplied.Green;
            blue = (independentMask & 4) == 0 && supplied.Blue == expected.Blue ? null : supplied.Blue;
        }
        internal Bgr555 Apply(Bgr555 expected) => new(red ?? expected.Red, green ?? expected.Green, blue ?? expected.Blue);
    }

    public bool TryGetValue(ushort pointer, out Bgr555 value)
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
    public Bgr555 this[ushort key] => TryGetValue(key, out Bgr555 value) ? value : throw new KeyNotFoundException();
    public int Count => colors.Count + 8;
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return LoadingSuitColorPointers.PowerDim1; yield return LoadingSuitColorPointers.PowerDim2; yield return LoadingSuitColorPointers.PowerBright9; yield return LoadingSuitColorPointers.PowerDim12;
            yield return LoadingSuitColorPointers.VariaBright10; yield return LoadingSuitColorPointers.VariaBright11; yield return LoadingSuitColorPointers.VariaDim12; yield return LoadingSuitColorPointers.GravityDim2;
        }
    }
    public IEnumerable<Bgr555> Values => Keys.Select(key => this[key]);
    public IEnumerator<KeyValuePair<ushort, Bgr555>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
