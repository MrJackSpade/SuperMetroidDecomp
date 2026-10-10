using SuperMetroid.Core.Hardware;
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
internal sealed class HeatPaletteInputView : IReadOnlyDictionary<ushort, Bgr555>
{
    private readonly Dictionary<ushort, Bgr555> colors;
    private readonly int redTarget;
    private readonly int mixedRedTarget;
    private readonly int mixedGreenTarget;
    private readonly int? greenOverride;
    private readonly int? blueOverride;
    private readonly int? mixedBlueOverride;

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

    private Bgr555 ReadBase(ushort pointer)
    {
        if (colors.TryGetValue(pointer, out Bgr555 value)) return value;
        if (HeatPaletteColorDefinitions.TryBasePalettePointer(pointer, out ushort source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value)) return value;
        throw new InvalidDataException($"Missing installed heat base ${pointer:X4}.");
    }

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

    public Bgr555 this[ushort key] => TryGetValue(key, out Bgr555 value) ? value : throw new KeyNotFoundException();
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
    public IEnumerable<Bgr555> Values => Keys.Select(key => this[key]);
    public IEnumerator<KeyValuePair<ushort, Bgr555>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
