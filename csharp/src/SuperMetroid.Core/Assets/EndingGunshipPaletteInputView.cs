using System.Collections;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Gunship base inks with shared channels and the middle hull shade calculated.
/// Original $8D:D8DC matches the Ceres/explosion gunship artwork: gold highlights use
/// green=red-1; three hull shades use green=red-3,blue0 and a nearest-rounded middle red;
/// cockpit shades use red0 and proportional blue/green; underside shades share red/green
/// and one blue offset. Only independent channels or differing player edits are stored.
/// Complete original words and independent RGB edits are checked by the gunship proof.</summary>
/// <remarks>The remaining independent hue/contrast parameters are painted artwork:
/// original Mode7 tile pixels select gold hull, green cockpit and blue-grey underside
/// inks, without lighting or material inputs determining their chosen colors. Retain
/// those parameters under #1165's nonsense exception; a numeric index fit would only
/// recite this drawing. Shared channel/shade rules and temporal fades are calculated.
/// The exact18 retained components and source-art evidence are recorded in the review
/// inventory's endingGunshipIndependentInkReview.</remarks>
internal sealed class EndingGunshipPaletteInputView : IReadOnlyDictionary<ushort, ushort>
{
    /// <summary>Source colors not consumed into the calculated frame-15 ink entries.</summary>
    private readonly Dictionary<ushort, ushort> colors;
    /// <summary>Independent channel overrides for the highlight and three hull-shade inks.</summary>
    private readonly Channels highlight, hullLight, hullShadeLight, hullShadeMiddle, hullShadeDark;
    /// <summary>Independent channel overrides for the deepest shadow and black-detail inks.</summary>
    private readonly Channels deepShadow, blackDetail;
    /// <summary>Independent channel overrides for the three cockpit inks.</summary>
    private readonly Channels cockpitLight, cockpitMiddle, cockpitDark;
    /// <summary>Independent channel overrides for the three underside inks.</summary>
    private readonly Channels undersideLight, undersideMiddle, undersideDark;
    /// <summary>Blue-minus-red offset shared by the calculated underside shades.</summary>
    private readonly int undersideBlueOffset;

    /// <summary>Consumes the original gunship ink words and retains only independent edits.</summary>
    /// <param name="colors">Palette words keyed by their native frame/color pointer; recognized frame-15 inks are removed after their exceptions are captured.</param>
    internal EndingGunshipPaletteInputView(Dictionary<ushort, ushort> colors)
    {
        this.colors = colors;
        ushort Word(EndingGunshipPaletteInk ink) => colors[Pointer(ink)];
        int Red(EndingGunshipPaletteInk ink) => Word(ink) & 31;
        Channels Gold(EndingGunshipPaletteInk ink) => new(Word(ink), null, Math.Max(0, Red(ink) - 1), null);
        Channels Hull(EndingGunshipPaletteInk ink, int? red = null) => new(Word(ink), red, Math.Max(0, Red(ink) - 3), 0);
        highlight = Gold(EndingGunshipPaletteInk.Highlight);
        hullLight = Gold(EndingGunshipPaletteInk.HullLight);
        deepShadow = Hull(EndingGunshipPaletteInk.DeepShadow);
        blackDetail = new(Word(EndingGunshipPaletteInk.BlackDetail), 0, 0, 0);
        hullShadeLight = Hull(EndingGunshipPaletteInk.HullShadeLight);
        hullShadeDark = Hull(EndingGunshipPaletteInk.HullShadeDark);
        hullShadeMiddle = Hull(EndingGunshipPaletteInk.HullShadeMiddle,
            (Red(EndingGunshipPaletteInk.HullShadeLight) + Red(EndingGunshipPaletteInk.HullShadeDark) + 1) / 2);

        ushort cockpit = Word(EndingGunshipPaletteInk.CockpitLight);
        int green = cockpit >> 5 & 31, blue = cockpit >> 10 & 31;
        Channels Cockpit(EndingGunshipPaletteInk ink) => new(Word(ink), 0, null,
            green == 0 ? null : Math.Min(31, (Word(ink) >> 5 & 31) * blue / green));
        cockpitLight = new(cockpit, 0, null, null);
        cockpitMiddle = Cockpit(EndingGunshipPaletteInk.CockpitMiddle);
        cockpitDark = Cockpit(EndingGunshipPaletteInk.CockpitDark);

        ushort underside = Word(EndingGunshipPaletteInk.UndersideLight);
        undersideBlueOffset = (underside >> 10 & 31) - (underside & 31);
        Channels Underside(EndingGunshipPaletteInk ink) => new(Word(ink), null, Red(ink), Math.Clamp(Red(ink) + undersideBlueOffset, 0, 31));
        undersideLight = Underside(EndingGunshipPaletteInk.UndersideLight);
        undersideMiddle = Underside(EndingGunshipPaletteInk.UndersideMiddle);
        undersideDark = Underside(EndingGunshipPaletteInk.UndersideDark);
        foreach (EndingGunshipPaletteInk ink in Enum.GetValues<EndingGunshipPaletteInk>()) colors.Remove(Pointer(ink));
    }

    /// <summary>Compact record of RGB components that must be retained as edits.</summary>
    private readonly struct Channels
    {
        /// <summary>Explicit component values retained when an input ink differs from its shared rule.</summary>
        private readonly int? red, green, blue;

        /// <summary>Stores only channel components that cannot be reconstructed by the shared palette rule.</summary>
        /// <param name="supplied">Original 15-bit palette word.</param>
        /// <param name="expectedRed">Calculated red component, or null when red is independently retained.</param>
        /// <param name="expectedGreen">Calculated green component, or null when green is independently retained.</param>
        /// <param name="expectedBlue">Calculated blue component, or null when blue is independently retained.</param>
        internal Channels(ushort supplied, int? expectedRed, int? expectedGreen, int? expectedBlue)
        {
            red = (supplied & 31) == expectedRed ? null : supplied & 31;
            green = (supplied >> 5 & 31) == expectedGreen ? null : supplied >> 5 & 31;
            blue = (supplied >> 10 & 31) == expectedBlue ? null : supplied >> 10 & 31;
        }
        /// <summary>Uses the captured red edit when present, otherwise the calculated channel.</summary>
        /// <param name="calculated">Red component produced by the shared palette rule.</param>
        /// <returns>The effective red component.</returns>
        internal int Red(int calculated) => red ?? calculated;

        /// <summary>Explicit green edit, or zero when green follows the shared rule.</summary>
        internal int Green => green ?? 0;

        /// <summary>Explicit blue edit, or zero when blue follows the shared rule.</summary>
        internal int Blue => blue ?? 0;

        /// <summary>Combines calculated RGB components with any captured per-channel edits.</summary>
        /// <param name="r">Calculated five-bit red component.</param>
        /// <param name="g">Calculated five-bit green component.</param>
        /// <param name="b">Calculated five-bit blue component.</param>
        /// <returns>Packed SNES 15-bit RGB palette word.</returns>
        internal ushort Apply(int r, int g, int b) => (ushort)((red ?? r) | (green ?? g) << 5 | (blue ?? b) << 10);
    }

    /// <summary>Calculates a gold ink using its retained red and shared red-to-green offset.</summary>
    /// <param name="channels">Independent edits captured for this ink.</param>
    /// <param name="red">Effective red component.</param>
    /// <param name="greenOffset">Amount subtracted from red to derive green.</param>
    /// <returns>Packed palette word with a zero blue component unless explicitly edited.</returns>
    private static ushort Gold(Channels channels, int red, int greenOffset) =>
        channels.Apply(red, Math.Max(0, red - greenOffset), 0);

    /// <summary>Calculates a cockpit ink using the light ink's green-to-blue proportion.</summary>
    /// <param name="channels">Independent edits captured for the selected cockpit shade.</param>
    /// <returns>Packed palette word with red fixed at zero.</returns>
    private ushort Cockpit(Channels channels)
    {
        int blue = cockpitLight.Green == 0 ? 0 : Math.Min(31, channels.Green * cockpitLight.Blue / cockpitLight.Green);
        return channels.Apply(0, channels.Green, blue);
    }
    /// <summary>Calculates an underside shade from its effective red and the shared blue offset.</summary>
    /// <param name="channels">Independent edits captured for the selected underside shade.</param>
    /// <returns>Packed palette word whose red and green components match.</returns>
    private ushort Underside(Channels channels)
    {
        int red = channels.Red(0);
        return channels.Apply(red, red, Math.Clamp(red + undersideBlueOffset, 0, 31));
    }

    /// <summary>Resolves a palette pointer from either retained source colors or calculated frame-15 inks.</summary>
    /// <param name="pointer">Native palette color pointer being read.</param>
    /// <param name="value">Receives the resolved 15-bit palette word when found.</param>
    /// <returns>True when the pointer belongs to a retained color or supported calculated ink.</returns>
    public bool TryGetValue(ushort pointer, out ushort value)
    {
        if (EndingGunshipPaletteColorDefinitions.TryCoordinates(pointer, out int frame, out int color) && frame == 15)
        {
            ushort? calculated = (EndingGunshipPaletteInk)color switch
            {
                EndingGunshipPaletteInk.Highlight => Gold(highlight, highlight.Red(0), 1),
                EndingGunshipPaletteInk.DeepShadow => Gold(deepShadow, deepShadow.Red(0), 3),
                EndingGunshipPaletteInk.BlackDetail => blackDetail.Apply(0, 0, 0),
                EndingGunshipPaletteInk.HullLight => Gold(hullLight, hullLight.Red(0), 1),
                EndingGunshipPaletteInk.HullShadeLight => Gold(hullShadeLight, hullShadeLight.Red(0), 3),
                EndingGunshipPaletteInk.HullShadeMiddle => Gold(hullShadeMiddle,
                    hullShadeMiddle.Red((hullShadeLight.Red(0) + hullShadeDark.Red(0) + 1) / 2), 3),
                EndingGunshipPaletteInk.HullShadeDark => Gold(hullShadeDark, hullShadeDark.Red(0), 3),
                EndingGunshipPaletteInk.CockpitLight => Cockpit(cockpitLight),
                EndingGunshipPaletteInk.CockpitMiddle => Cockpit(cockpitMiddle),
                EndingGunshipPaletteInk.CockpitDark => Cockpit(cockpitDark),
                EndingGunshipPaletteInk.UndersideLight => Underside(undersideLight),
                EndingGunshipPaletteInk.UndersideMiddle => Underside(undersideMiddle),
                EndingGunshipPaletteInk.UndersideDark => Underside(undersideDark),
                _ => null,
            };
            if (calculated.HasValue) { value = calculated.Value; return true; }
        }
        return colors.TryGetValue(pointer, out value);
    }

    /// <summary>Gets the native palette pointer for one frame-15 gunship ink identity.</summary>
    /// <param name="ink">Color identity whose palette address is required.</param>
    /// <returns>The compiled bank-relative palette pointer.</returns>
    private static ushort Pointer(EndingGunshipPaletteInk ink) => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(15, (int)ink);

    /// <summary>Gets a palette word using calculated ink values when available.</summary>
    /// <param name="key">Native palette pointer to resolve.</param>
    /// <returns>The resolved palette word.</returns>
    /// <exception cref="KeyNotFoundException">The pointer is not present in the view.</exception>
    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();

    /// <summary>Number of retained source words plus the thirteen calculated frame-15 ink entries.</summary>
    public int Count => colors.Count + 13;

    /// <summary>Checks whether a retained or calculated palette entry exists.</summary>
    /// <param name="key">Native palette pointer to test.</param>
    /// <returns>True when the view can resolve the pointer.</returns>
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);

    /// <summary>Enumerates retained source pointers followed by the calculated ink pointers.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            foreach (EndingGunshipPaletteInk ink in Enum.GetValues<EndingGunshipPaletteInk>()) yield return Pointer(ink);
        }
    }
    /// <summary>Enumerates palette words in the same order as <see cref="Keys"/>.</summary>
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);

    /// <summary>Enumerates resolved pointer/value pairs in key order.</summary>
    /// <returns>An enumerator over every retained and calculated palette entry.</returns>
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
