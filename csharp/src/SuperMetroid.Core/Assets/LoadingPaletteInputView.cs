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
    /// <summary>Ordinary stored suit colors after the eight selected tinted endpoints are removed.</summary>
    private readonly Dictionary<ushort, Bgr555> colors;
    /// <summary>Power dim slot1, $8D:DC7E.</summary>
    internal readonly Channels powerDim1;
    /// <summary>Power dim slot2, $8D:DC80.</summary>
    internal readonly Channels powerDim2;
    /// <summary>Power bright slot9, $8D:DBA1.</summary>
    internal readonly Channels powerBright9;
    /// <summary>Power dim slot12, $8D:DC94.</summary>
    internal readonly Channels powerDim12;
    /// <summary>Varia bright slot10, $8D:DD09.</summary>
    internal readonly Channels variaBright10;
    /// <summary>Varia bright slot11, $8D:DD0B; shares slot10 blue.</summary>
    internal readonly Channels variaBright11;
    /// <summary>Varia dim slot12, $8D:DDFA.</summary>
    internal readonly Channels variaDim12;
    /// <summary>Gravity dim slot2, $8D:DF4C.</summary>
    internal readonly Channels gravityDim2;

    /// <summary>Captures only channels differing from calculated tint endpoints, then removes those endpoints from ordinary storage.</summary>
    internal LoadingPaletteInputView(Dictionary<ushort, Bgr555> colors)
    {
        this.colors = colors;
        powerDim1 = new(colors[(ushort)LoadingSuitColor.PowerDim1], Expected(LoadingSuitColor.PowerDim1));
        powerDim2 = new(colors[(ushort)LoadingSuitColor.PowerDim2], Expected(LoadingSuitColor.PowerDim2));
        powerBright9 = new(colors[(ushort)LoadingSuitColor.PowerBright9], Expected(LoadingSuitColor.PowerBright9));
        powerDim12 = new(colors[(ushort)LoadingSuitColor.PowerDim12], Expected(LoadingSuitColor.PowerDim12));
        variaBright10 = new(colors[(ushort)LoadingSuitColor.VariaBright10], Expected(LoadingSuitColor.VariaBright10));
        variaBright11 = new(colors[(ushort)LoadingSuitColor.VariaBright11], Expected(LoadingSuitColor.VariaBright11));
        variaDim12 = new(colors[(ushort)LoadingSuitColor.VariaDim12], Expected(LoadingSuitColor.VariaDim12));
        gravityDim2 = new(colors[(ushort)LoadingSuitColor.GravityDim2], Expected(LoadingSuitColor.GravityDim2));
        colors.Remove((ushort)LoadingSuitColor.PowerDim1);
        colors.Remove((ushort)LoadingSuitColor.PowerDim2);
        colors.Remove((ushort)LoadingSuitColor.PowerBright9);
        colors.Remove((ushort)LoadingSuitColor.PowerDim12);
        colors.Remove((ushort)LoadingSuitColor.VariaBright10);
        colors.Remove((ushort)LoadingSuitColor.VariaBright11);
        colors.Remove((ushort)LoadingSuitColor.VariaDim12);
        colors.Remove((ushort)LoadingSuitColor.GravityDim2);
    }

    /// <summary>Requires the normal-suit source color used to derive a loading tint endpoint.</summary>
    private Bgr555 Normal(ushort pointer) =>
        LoadingPaletteColorDefinitions.TryReadColor(pointer, colors, out Bgr555 value)
            ? value : throw new InvalidDataException($"Missing loading base ${pointer:X4}.");

    /// <summary>Calculates the selected loading tint from a normal-suit source and shared channel rules.</summary>
    private Bgr555 Expected(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6d), 2),
        LoadingSuitColor.PowerDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb6f), 2),
        LoadingSuitColor.PowerBright9 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb7d), 0),
        LoadingSuitColor.PowerDim12 => LoadingPaletteColorDefinitions.TintColor(Normal(0xdb83), 2),
        LoadingSuitColor.VariaBright10 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce5), 0),
        LoadingSuitColor.VariaBright11 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce7), 0)
            .WithBlue(variaBright10.Apply(Expected(LoadingSuitColor.VariaBright10)).Blue),
        LoadingSuitColor.VariaDim12 => LoadingPaletteColorDefinitions.VariaTintColor(Normal(0xdce9), 2),
        LoadingSuitColor.GravityDim2 => LoadingPaletteColorDefinitions.TintColor(Normal(0xde3b), 2),
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
        internal Channels(Bgr555 supplied, Bgr555 expected, int independentMask = 0)
        {
            red = (independentMask & 1) == 0 && supplied.Red == expected.Red ? null : supplied.Red;
            green = (independentMask & 2) == 0 && supplied.Green == expected.Green ? null : supplied.Green;
            blue = (independentMask & 4) == 0 && supplied.Blue == expected.Blue ? null : supplied.Blue;
        }
        /// <summary>Overlays retained independent channels on the calculated normal-suit tint.</summary>
        internal Bgr555 Apply(Bgr555 expected) => new(red ?? expected.Red, green ?? expected.Green, blue ?? expected.Blue);
    }

    /// <summary>Resolves an ordinary stored color or reconstructs one of eight derived loading-tint endpoints.</summary>
    public bool TryGetValue(ushort pointer, out Bgr555 value)
    {
        // Pointers outside the eight tinted slots are ordinary stored colors.
        if (!Enum.IsDefined((LoadingSuitColor)pointer))
            return colors.TryGetValue(pointer, out value);
        var slot = (LoadingSuitColor)pointer;
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
    /// <summary>Requires a stored or derived loading color at its native pointer.</summary>
    public Bgr555 this[ushort key] => TryGetValue(key, out Bgr555 value) ? value : throw new KeyNotFoundException();
    /// <summary>Includes ordinary stored colors and the eight reconstructed tint endpoints.</summary>
    public int Count => colors.Count + 8;
    /// <summary>Checks whether a native pointer identifies a stored or derived loading color.</summary>
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    /// <summary>Enumerates stored pointers followed by the eight derived tint addresses.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return (ushort)LoadingSuitColor.PowerDim1; yield return (ushort)LoadingSuitColor.PowerDim2; yield return (ushort)LoadingSuitColor.PowerBright9; yield return (ushort)LoadingSuitColor.PowerDim12;
            yield return (ushort)LoadingSuitColor.VariaBright10; yield return (ushort)LoadingSuitColor.VariaBright11; yield return (ushort)LoadingSuitColor.VariaDim12; yield return (ushort)LoadingSuitColor.GravityDim2;
        }
    }
    /// <summary>Resolves values in the same stored-then-derived order as <see cref="Keys"/>.</summary>
    public IEnumerable<Bgr555> Values => Keys.Select(key => this[key]);
    /// <summary>Enumerates stored and reconstructed colors by their native pointers.</summary>
    public IEnumerator<KeyValuePair<ushort, Bgr555>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Exposes the same loading color pairs to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
