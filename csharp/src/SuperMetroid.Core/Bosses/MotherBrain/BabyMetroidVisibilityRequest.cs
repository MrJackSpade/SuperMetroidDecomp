namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of the Baby's cutscene asks of whether its place is drawn (#2310).</summary>
public enum BabyMetroidVisibilityRequest
{
    /// <summary>Leave the Baby shown or hidden as it is: every step outside its death.</summary>
    Unchanged,

    /// <summary>Show the Baby, as its death's flicker does on odd enemy frames.</summary>
    Show,

    /// <summary>Hide the Baby, as its death's flicker does on even enemy frames and its finished fade to black does.</summary>
    Hide,
}
