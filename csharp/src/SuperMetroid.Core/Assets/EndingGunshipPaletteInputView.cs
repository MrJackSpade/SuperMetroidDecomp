using SuperMetroid.Core.Hardware;
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
internal sealed class EndingGunshipPaletteInputView : IReadOnlyDictionary<ushort, Bgr555>
{
    /// <summary>Ordinary stored fade colors after the thirteen final-frame painted inks are separated.</summary>
    private readonly Dictionary<ushort, Bgr555> colors;
    /// <summary>Gold highlight inputs with green derived from red minus one.</summary>
    private readonly Channels highlight;
    /// <summary>Light hull gold inputs using the same one-step green offset.</summary>
    private readonly Channels hullLight;
    /// <summary>First dark hull shade with green derived three steps below red.</summary>
    private readonly Channels hullShadeLight;
    /// <summary>Middle hull shade whose default red is rounded from the outer shade reds.</summary>
    private readonly Channels hullShadeMiddle;
    /// <summary>Deepest of the three related hull shade inks.</summary>
    private readonly Channels hullShadeDark;
    /// <summary>Deep hull shadow with the three-step green offset.</summary>
    private readonly Channels deepShadow;
    /// <summary>Black detail ink with independently retained component edits.</summary>
    private readonly Channels blackDetail;
    /// <summary>Light cockpit ink supplying the blue-to-green ratio for darker cockpit shades.</summary>
    private readonly Channels cockpitLight;
    /// <summary>Middle cockpit ink sharing zero red and a proportional blue target.</summary>
    private readonly Channels cockpitMiddle;
    /// <summary>Dark cockpit ink sharing zero red and a proportional blue target.</summary>
    private readonly Channels cockpitDark;
    /// <summary>Light underside ink supplying the common blue-minus-red offset.</summary>
    private readonly Channels undersideLight;
    /// <summary>Middle underside ink using equal red/green and the shared blue offset.</summary>
    private readonly Channels undersideMiddle;
    /// <summary>Dark underside ink using equal red/green and the shared blue offset.</summary>
    private readonly Channels undersideDark;
    /// <summary>Blue-minus-red difference selected by the light underside paint.</summary>
    private readonly int undersideBlueOffset;

    /// <summary>Extracts independent channel edits and shared hue relationships from the final gunship palette frame.</summary>
    internal EndingGunshipPaletteInputView(Dictionary<ushort, Bgr555> colors)
    {
        this.colors = colors;
        Bgr555 Word(EndingGunshipPaletteInk ink) => colors[Pointer(ink)];
        int Red(EndingGunshipPaletteInk ink) => Word(ink).Red;
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

        Bgr555 cockpit = Word(EndingGunshipPaletteInk.CockpitLight);
        int green = cockpit.Green, blue = cockpit.Blue;
        Channels Cockpit(EndingGunshipPaletteInk ink) => new(Word(ink), 0, null,
            green == 0 ? null : Math.Min(31, (Word(ink).Green) * blue / green));
        cockpitLight = new(cockpit, 0, null, null);
        cockpitMiddle = Cockpit(EndingGunshipPaletteInk.CockpitMiddle);
        cockpitDark = Cockpit(EndingGunshipPaletteInk.CockpitDark);

        Bgr555 underside = Word(EndingGunshipPaletteInk.UndersideLight);
        undersideBlueOffset = (underside.Blue) - (underside.Red);
        Channels Underside(EndingGunshipPaletteInk ink) => new(Word(ink), null, Red(ink), Math.Clamp(Red(ink) + undersideBlueOffset, 0, 31));
        undersideLight = Underside(EndingGunshipPaletteInk.UndersideLight);
        undersideMiddle = Underside(EndingGunshipPaletteInk.UndersideMiddle);
        undersideDark = Underside(EndingGunshipPaletteInk.UndersideDark);
        foreach (EndingGunshipPaletteInk ink in Enum.GetValues<EndingGunshipPaletteInk>()) colors.Remove(Pointer(ink));
    }

    /// <summary>Nullable independent RGB5 components over a calculated gunship ink.</summary>
    private readonly struct Channels
    {
        /// <summary>Independent red component, or null when supplied by a shared rule.</summary>
        private readonly int? red;
        /// <summary>Independent green component, or null when supplied by a shared rule.</summary>
        private readonly int? green;
        /// <summary>Independent blue component, or null when supplied by a shared rule.</summary>
        private readonly int? blue;
        /// <summary>Retains supplied RGB5 components only where a calculated target does not explain them.</summary>
        internal Channels(Bgr555 supplied, int? expectedRed, int? expectedGreen, int? expectedBlue)
        {
            red = (supplied.Red) == expectedRed ? null : supplied.Red;
            green = (supplied.Green) == expectedGreen ? null : supplied.Green;
            blue = (supplied.Blue) == expectedBlue ? null : supplied.Blue;
        }
        /// <summary>Selects an authored red component or its calculated shade relationship.</summary>
        internal int Red(int calculated) => red ?? calculated;
        /// <summary>Selected cockpit green channel, defaulting to black when absent.</summary>
        internal int Green => green ?? 0;
        /// <summary>Selected cockpit blue channel, defaulting to black when absent.</summary>
        internal int Blue => blue ?? 0;
        /// <summary>Overlays independent components onto a calculated gold, cockpit, or underside target.</summary>
        internal Bgr555 Apply(int r, int g, int b) => new(red ?? r, green ?? g, blue ?? b);
    }

    /// <summary>Calculates a gold or hull shade's green offset from red before applying independent edits.</summary>
    private static Bgr555 Gold(Channels channels, int red, int greenOffset) =>
        channels.Apply(red, Math.Max(0, red - greenOffset), 0);
    /// <summary>Derives blue from the light cockpit ink's blue/green proportion.</summary>
    private Bgr555 Cockpit(Channels channels)
    {
        int blue = cockpitLight.Green == 0 ? 0 : Math.Min(31, channels.Green * cockpitLight.Blue / cockpitLight.Green);
        return channels.Apply(0, channels.Green, blue);
    }
    /// <summary>Uses equal red and green with the common blue offset, clamped to RGB5.</summary>
    private Bgr555 Underside(Channels channels)
    {
        int red = channels.Red(0);
        return channels.Apply(red, red, Math.Clamp(red + undersideBlueOffset, 0, 31));
    }

    /// <summary>Reconstructs thirteen final-frame gunship inks while leaving other fade and non-ink words stored.</summary>
    public bool TryGetValue(ushort pointer, out Bgr555 value)
    {
        // Colors 0, 2 and 15 of the final frame are not inks; they stay stored values.
        if (EndingGunshipPaletteColorDefinitions.TryCoordinates(pointer, out int frame, out int color) && frame == 15 &&
            Enum.IsDefined((EndingGunshipPaletteInk)color))
        {
            Bgr555 calculated = (EndingGunshipPaletteInk)color switch
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
                var ink => throw new InvalidOperationException($"Undefined gunship ink {ink}."),
            };
            value = calculated;
            return true;
        }
        return colors.TryGetValue(pointer, out value);
    }

    /// <summary>Finds the native final-frame color address for one selected gunship ink.</summary>
    private static ushort Pointer(EndingGunshipPaletteInk ink) => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(15, (int)ink);
    /// <summary>Requires a stored fade color or reconstructed final-frame ink at its native address.</summary>
    public Bgr555 this[ushort key] => TryGetValue(key, out Bgr555 value) ? value : throw new KeyNotFoundException();
    /// <summary>Includes ordinary stored colors and thirteen reconstructed painted inks.</summary>
    public int Count => colors.Count + 13;
    /// <summary>Checks membership among ordinary colors and reconstructed final-frame inks.</summary>
    public bool ContainsKey(ushort key) => TryGetValue(key, out _);
    /// <summary>Enumerates ordinary color addresses followed by the thirteen final-frame inks.</summary>
    public IEnumerable<ushort> Keys
    {
        get
        {
            foreach (ushort key in colors.Keys) yield return key;
            foreach (EndingGunshipPaletteInk ink in Enum.GetValues<EndingGunshipPaletteInk>()) yield return Pointer(ink);
        }
    }
    /// <summary>Resolves values in the same stored-then-derived order as <see cref="Keys"/>.</summary>
    public IEnumerable<Bgr555> Values => Keys.Select(key => this[key]);
    /// <summary>Enumerates every ordinary or reconstructed gunship color by native pointer.</summary>
    public IEnumerator<KeyValuePair<ushort, Bgr555>> GetEnumerator()
    {
        foreach (ushort key in Keys) yield return new(key, this[key]);
    }
    /// <summary>Exposes the same gunship color pairs to non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
