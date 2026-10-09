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
    /// <summary>Installed palette entries other than the two heat endpoints calculated by this view.</summary>
    private readonly Dictionary<ushort, ushort> colors;
    /// <summary>Red channel selected for the shared heat endpoint.</summary>
    private readonly int redTarget;
    /// <summary>Red channel selected for the mixed heat endpoint.</summary>
    private readonly int mixedRedTarget;
    /// <summary>Green channel selected for the mixed endpoint, which is retained from its installed value.</summary>
    private readonly int mixedGreenTarget;
    /// <summary>Explicit shared-endpoint green channel when it differs from the preserved base palette.</summary>
    private readonly int? greenOverride;
    /// <summary>Explicit shared-endpoint blue channel when it differs from the preserved base palette.</summary>
    private readonly int? blueOverride;
    /// <summary>Explicit mixed-endpoint blue channel when it differs from the calculated base relationship.</summary>
    private readonly int? mixedBlueOverride;

    /// <summary>Captures the authored heat tint and exposes its two endpoints as calculated palette entries.</summary>
    /// <param name="colors">Mutable installed palette entries containing both heat endpoint words and any available base colors; the endpoint entries are removed after capture.</param>
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

    /// <summary>Reads a preserved base color from installed entries or resolves it through the base-palette mappings.</summary>
    /// <param name="pointer">Pointer of the heat base color, such as the shared or mixed endpoint source.</param>
    /// <returns>The installed base palette word.</returns>
    /// <exception cref="InvalidDataException">No installed color or mapped source supplies the requested base word.</exception>
    private ushort ReadBase(ushort pointer)
    {
        if (colors.TryGetValue(pointer, out ushort value)) return value;
        if (HeatPaletteColorDefinitions.TryBasePalettePointer(pointer, out ushort source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value)) return value;
        throw new InvalidDataException($"Missing installed heat base ${pointer:X4}.");
    }

    /// <summary>Looks up an installed palette color or calculates one of the two heat endpoint colors.</summary>
    /// <param name="pointer">Palette pointer to resolve.</param>
    /// <param name="value">Receives the installed or calculated color when found; otherwise receives the dictionary's default value.</param>
    /// <returns><see langword="true"/> when the pointer names an installed entry or a synthesized heat endpoint.</returns>
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

    /// <summary>Gets an installed or calculated palette color by pointer.</summary>
    /// <param name="key">Palette pointer to resolve.</param>
    /// <exception cref="KeyNotFoundException">The pointer is neither installed nor one of the synthesized heat endpoints.</exception>
    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();
    /// <summary>Gets the number of installed entries plus the two synthesized heat endpoint entries.</summary>
    public int Count => colors.Count + 2;
    /// <summary>Determines whether a pointer is installed or is one of the synthesized heat endpoints.</summary>
    /// <param name="key">Palette pointer to test.</param>
    /// <returns><see langword="true"/> if the view can return a color for <paramref name="key"/>.</returns>
    public bool ContainsKey(ushort key) => key is 0xe55c or 0xe55e || colors.ContainsKey(key);
    /// <summary>Enumerates installed palette pointers followed by the two synthesized heat endpoint pointers.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            yield return 0xe55c;
            yield return 0xe55e;
        }
    }
    /// <summary>Enumerates colors corresponding to <see cref="Keys"/>, including calculated heat endpoints.</summary>
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);
    /// <summary>Enumerates each visible palette pointer and its installed or calculated color.</summary>
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
