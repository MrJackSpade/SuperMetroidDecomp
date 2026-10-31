namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The ten bank-$82 menu spritemaps the reserve-tank strip draws, valued by their $82:C569 ordinal.</summary>
public enum PauseReserveTankVisual
{
    /// <summary>$82:B305 full-tank spritemap ID.</summary>
    Full = 27,
    /// <summary>$82:B396 final end-cap spritemap ID.</summary>
    EndCap = 31,
    /// <summary>$82:B37D empty-tank spritemap ID.</summary>
    Empty = 32,
    /// <summary>Partial tank, one fill step.</summary>
    Fill1 = 33,
    /// <summary>Partial tank, two fill steps.</summary>
    Fill2 = 34,
    /// <summary>Partial tank, three fill steps.</summary>
    Fill3 = 35,
    /// <summary>Partial tank, four fill steps.</summary>
    Fill4 = 36,
    /// <summary>Partial tank, five fill steps.</summary>
    Fill5 = 37,
    /// <summary>Partial tank, six fill steps.</summary>
    Fill6 = 38,
    /// <summary>Partial tank, seven fill steps; drawn with the full-tank tile.</summary>
    Fill7 = 39,
}
