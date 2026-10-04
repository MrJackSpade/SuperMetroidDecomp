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
    private readonly Dictionary<ushort, ushort> colors;
    private readonly Channels highlight, hullLight, hullShadeLight, hullShadeMiddle, hullShadeDark;
    private readonly Channels deepShadow, blackDetail;
    private readonly Channels cockpitLight, cockpitMiddle, cockpitDark;
    private readonly Channels undersideLight, undersideMiddle, undersideDark;
    private readonly int undersideBlueOffset;

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

    private readonly struct Channels
    {
        private readonly int? red, green, blue;
        internal Channels(ushort supplied, int? expectedRed, int? expectedGreen, int? expectedBlue)
        {
            red = (supplied & 31) == expectedRed ? null : supplied & 31;
            green = (supplied >> 5 & 31) == expectedGreen ? null : supplied >> 5 & 31;
            blue = (supplied >> 10 & 31) == expectedBlue ? null : supplied >> 10 & 31;
        }
        internal int Red(int calculated) => red ?? calculated;
        internal int Green => green ?? 0;
        internal int Blue => blue ?? 0;
        internal ushort Apply(int r, int g, int b) => (ushort)((red ?? r) | (green ?? g) << 5 | (blue ?? b) << 10);
    }

    private static ushort Gold(Channels channels, int red, int greenOffset) =>
        channels.Apply(red, Math.Max(0, red - greenOffset), 0);
    private ushort Cockpit(Channels channels)
    {
        int blue = cockpitLight.Green == 0 ? 0 : Math.Min(31, channels.Green * cockpitLight.Blue / cockpitLight.Green);
        return channels.Apply(0, channels.Green, blue);
    }
    private ushort Underside(Channels channels)
    {
        int red = channels.Red(0);
        return channels.Apply(red, red, Math.Clamp(red + undersideBlueOffset, 0, 31));
    }

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

    private static ushort Pointer(EndingGunshipPaletteInk ink) => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(15, (int)ink);
    public ushort this[ushort key] => TryGetValue(key, out ushort value) ? value : throw new KeyNotFoundException();
    public int Count => colors.Count + 13;
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            foreach (EndingGunshipPaletteInk ink in Enum.GetValues<EndingGunshipPaletteInk>()) yield return Pointer(ink);
        }
    }
    public IEnumerable<ushort> Values => Keys.Select(key => this[key]);
    public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
