namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Mutually exclusive original gunship palette slots with shared channel rules.</summary>
internal enum EndingGunshipPaletteInk
{
    /// <summary>$8D:D8DE, upper gold highlight.</summary>
    Highlight = 1,
    /// <summary>$8D:D8E2, deep gold shadow with the hull ramp's shared hue.</summary>
    DeepShadow = 3,
    /// <summary>$8D:D8E4, black outline/detail ink.</summary>
    BlackDetail = 4,
    /// <summary>$8D:D8E6, light gold hull ink.</summary>
    HullLight = 5,
    /// <summary>$8D:D8E8, light member of the three-shade gold ramp.</summary>
    HullShadeLight = 6,
    /// <summary>$8D:D8EA, middle member of the three-shade gold ramp.</summary>
    HullShadeMiddle = 7,
    /// <summary>$8D:D8EC, dark member of the three-shade gold ramp.</summary>
    HullShadeDark = 8,
    /// <summary>$8D:D8EE, green cockpit light.</summary>
    CockpitLight = 9,
    /// <summary>$8D:D8F0, middle cockpit shade.</summary>
    CockpitMiddle = 10,
    /// <summary>$8D:D8F2, dark cockpit shade.</summary>
    CockpitDark = 11,
    /// <summary>$8D:D8F4, light blue-grey underside.</summary>
    UndersideLight = 12,
    /// <summary>$8D:D8F6, middle blue-grey underside.</summary>
    UndersideMiddle = 13,
    /// <summary>$8D:D8F8, dark blue-grey underside.</summary>
    UndersideDark = 14,
}
