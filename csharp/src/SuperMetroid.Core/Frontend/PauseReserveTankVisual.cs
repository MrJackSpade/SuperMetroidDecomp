namespace SuperMetroid.Core.Frontend;

/// <summary>The ten bank-$82 menu spritemaps the reserve-tank strip draws, valued by their $82:C569 ordinal.</summary>
public enum PauseReserveTankVisual : ushort
{
    /// <summary>$82:B305 full-tank spritemap ID.</summary>
    Full = 0x1b,
    /// <summary>$82:B396 final end-cap spritemap ID.</summary>
    EndCap = 0x1f,
    /// <summary>$82:B37D empty-tank spritemap ID.</summary>
    Empty = 0x20,
    /// <summary>Partial tank, one fill step.</summary>
    Fill1 = 0x21,
    /// <summary>Partial tank, two fill steps.</summary>
    Fill2 = 0x22,
    /// <summary>Partial tank, three fill steps.</summary>
    Fill3 = 0x23,
    /// <summary>Partial tank, four fill steps.</summary>
    Fill4 = 0x24,
    /// <summary>Partial tank, five fill steps.</summary>
    Fill5 = 0x25,
    /// <summary>Partial tank, six fill steps.</summary>
    Fill6 = 0x26,
    /// <summary>Partial tank, seven fill steps; drawn with the full-tank tile.</summary>
    Fill7 = 0x27,
}
