namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The ten pictures the pause screen's reserve-tank strip draws.</summary>
public enum PauseReserveTankVisual
{
    /// <summary>A full tank.</summary>
    Full,
    /// <summary>The strip's final end cap.</summary>
    EndCap,
    /// <summary>An empty tank.</summary>
    Empty,
    /// <summary>Partial tank, one fill step.</summary>
    Fill1,
    /// <summary>Partial tank, two fill steps.</summary>
    Fill2,
    /// <summary>Partial tank, three fill steps.</summary>
    Fill3,
    /// <summary>Partial tank, four fill steps.</summary>
    Fill4,
    /// <summary>Partial tank, five fill steps.</summary>
    Fill5,
    /// <summary>Partial tank, six fill steps.</summary>
    Fill6,
    /// <summary>Partial tank, seven fill steps; drawn with the full-tank picture.</summary>
    Fill7,
}
