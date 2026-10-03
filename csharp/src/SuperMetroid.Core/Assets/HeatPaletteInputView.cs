using System.Collections;

namespace SuperMetroid.Core.Assets;

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
internal sealed class HeatPaletteInputView : IReadOnlyDictionary<ushort, ushort>
{
    private readonly Dictionary<ushort, ushort> colors;
    private readonly int redTarget;
    private readonly int mixedRedTarget;
    private readonly int mixedGreenTarget;
    private readonly int? greenOverride;
    private readonly int? blueOverride;
    private readonly int? mixedBlueOverride;

    internal HeatPaletteInputView(Dictionary<ushort, ushort> colors)
    {
        this.colors = colors;
        ushort red = colors[0xe55c];
        ushort mixed = colors[0xe55e];
        ushort redBase = ReadBase(0xe46e);
        ushort mixedBase = ReadBase(0xe470);
        redTarget = red & 31;
        mixedRedTarget = mixed & 31;
        mixedGreenTarget = mixed >> 5 & 31;
        greenOverride = (red >> 5 & 31) == (redBase >> 5 & 31) ? null : red >> 5 & 31;
        blueOverride = (red >> 10 & 31) == (redBase >> 10 & 31) ? null : red >> 10 & 31;
        int calculatedBlue = (mixedBase >> 10 & 31) + mixedGreenTarget - (mixedBase >> 5 & 31);
        mixedBlueOverride = (mixed >> 10 & 31) == calculatedBlue ? null : mixed >> 10 & 31;
        colors.Remove(0xe55c);
        colors.Remove(0xe55e);
    }

    private ushort ReadBase(ushort pointer)
    {
        if (colors.TryGetValue(pointer, out ushort value)) return value;
        if (HeatPaletteColorDefinitions.TryBasePalettePointer(pointer, out ushort source) &&
            colors.TryGetValue(source, out value)) return value;
        throw new InvalidDataException($"Missing installed heat base ${pointer:X4}.");
    }

    public bool TryGetValue(ushort pointer, out ushort value)
    {
        if (pointer == 0xe55c)
        {
            ushort original = ReadBase(0xe46e);
            int green = greenOverride ?? (original >> 5 & 31);
            int blue = blueOverride ?? (original >> 10 & 31);
            value = (ushort)(redTarget | green << 5 | blue << 10);
            return true;
        }
        if (pointer == 0xe55e)
        {
            ushort original = ReadBase(0xe470);
            int blue = mixedBlueOverride ?? ((original >> 10 & 31) + mixedGreenTarget - (original >> 5 & 31));
            value = (ushort)(mixedRedTarget | mixedGreenTarget << 5 | blue << 10);
            return true;
        }
        return colors.TryGetValue(pointer, out value);
    }

    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();
    public int Count => colors.Count + 2;
    public bool ContainsKey(ushort key) => key is 0xe55c or 0xe55e || colors.ContainsKey(key);
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return 0xe55c;
            yield return 0xe55e;
        }
    }
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
