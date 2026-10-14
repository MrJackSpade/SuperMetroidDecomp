using System.Collections;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Installed palette inputs with calculated heat endpoint channels.</summary>
/// <remarks>
/// Native $8D:E55C preserves the base green/blue at $E46E; $E55E preserves the
/// base green-minus-blue difference at $E470. Only the shared red target and the
/// mixed target's red/green are independent stock inputs. They specify the chosen
/// heat tint, not samples indexed by time or distance. The native consumer supplies
/// no temperature/intensity parameter deriving those targets: it selects painted
/// palette colors. Inventing an index formula for these three choices would merely
/// encode the chosen tint, the #1165 nonsense exception. All temporal interpolation,
/// shared channels and repeated rows are calculated separately. This does not exempt
/// the underlying normal suit artwork from its own review. Nonmatching player channel
/// edits remain explicit overrides. No complete stock endpoint word is cached.
/// </remarks>
internal sealed class HeatPaletteInputView : IReadOnlyDictionary<ushort, Bgr555>
{
    /// <summary>Installed base and edited heat colors excluding the two calculated endpoints.</summary>
    private readonly Dictionary<ushort, Bgr555> colors;
    /// <summary>Selected red channel of the $E55C heat endpoint.</summary>
    private readonly int redTarget;
    /// <summary>Selected red channel of the mixed $E55E endpoint.</summary>
    private readonly int mixedRedTarget;
    /// <summary>Selected green channel of the mixed $E55E endpoint.</summary>
    private readonly int mixedGreenTarget;
    /// <summary>Player edit to $E55C green when it differs from the current base color.</summary>
    internal readonly int? greenOverride;
    /// <summary>Player edit to $E55C blue when it differs from the current base color.</summary>
    internal readonly int? blueOverride;
    /// <summary>Player edit to $E55E blue when it differs from the preserved base green-blue difference.</summary>
    internal readonly int? mixedBlueOverride;

    /// <summary>Separates selected heat-tint channels from base-dependent channels and retains nonmatching edits.</summary>
    internal HeatPaletteInputView(Dictionary<ushort, Bgr555> colors)
    {
        this.colors = colors;
        Bgr555 red = colors[0xe55c];
        Bgr555 mixed = colors[0xe55e];
        Bgr555 redBase = ReadBase(0xe46e);
        Bgr555 mixedBase = ReadBase(0xe470);
        redTarget = red.Red;
        mixedRedTarget = mixed.Red;
        mixedGreenTarget = mixed.Green;
        greenOverride = red.Green == redBase.Green ? null : red.Green;
        blueOverride = red.Blue == redBase.Blue ? null : red.Blue;
        int calculatedBlue = mixedBase.Blue + mixedGreenTarget - mixedBase.Green;
        mixedBlueOverride = mixed.Blue == calculatedBlue ? null : mixed.Blue;
        colors.Remove(0xe55c);
        colors.Remove(0xe55e);
    }

    /// <summary>Resolves a base suit color from installed entries or its compiled loading-palette alias.</summary>
    private Bgr555 ReadBase(ushort pointer)
    {
        if (colors.TryGetValue(pointer, out Bgr555 value)) return value;
        if (HeatPaletteColorDefinitions.TryBasePalettePointer(pointer, out ushort source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value)) return value;
        throw new InvalidDataException($"Missing installed heat base ${pointer:X4}.");
    }

    /// <summary>Reconstructs $E55C/$E55E from selected channels and live base colors, or reads another installed entry.</summary>
    public bool TryGetValue(ushort pointer, out Bgr555 value)
    {
        if (pointer == 0xe55c)
        {
            Bgr555 original = ReadBase(0xe46e);
            value = new(redTarget, greenOverride ?? original.Green, blueOverride ?? original.Blue);
            return true;
        }
        if (pointer == 0xe55e)
        {
            Bgr555 original = ReadBase(0xe470);
            int blue = mixedBlueOverride ?? (original.Blue + mixedGreenTarget - original.Green);
            value = new(mixedRedTarget, mixedGreenTarget, blue);
            return true;
        }
        return colors.TryGetValue(pointer, out value);
    }

    /// <summary>Resolves a heat color or rejects an absent native pointer.</summary>
    public Bgr555 this[ushort key] => TryGetValue(key, out Bgr555 value) ? value : throw new KeyNotFoundException();
    /// <summary>Installed entries plus the two calculated heat endpoints.</summary>
    public int Count => colors.Count + 2;
    /// <summary>Recognizes either calculated endpoint or a remaining installed pointer.</summary>
    public bool ContainsKey(ushort key) => key is 0xe55c or 0xe55e || colors.ContainsKey(key);
    /// <summary>Enumerates installed pointers followed by the two calculated endpoint pointers.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return 0xe55c;
            yield return 0xe55e;
        }
    }
    /// <summary>Resolves every current heat color in key-enumeration order.</summary>
    public IEnumerable<Bgr555> Values => Keys.Select(key => this[key]);
    /// <summary>Yields native pointers paired with their current derived or installed color.</summary>
    public IEnumerator<KeyValuePair<ushort, Bgr555>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Exposes the same pointer-color pairs to non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
