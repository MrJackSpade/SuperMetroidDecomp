namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Movement index of one Mother Brain neck joint: the word at <c>$7E:8064</c> (lower) or
/// <c>$7E:8066</c> (upper) that <c>$A9:9072</c> and <c>$A9:9109</c> use as an offset into
/// their five-entry jump tables. Both joints share the same table layout.
/// </summary>
public enum MotherBrainNeckMovement
{
    /// <summary>Index 0: the joint does not move (<c>RTS_A99084</c>/<c>RTS_A9911B</c>).</summary>
    None = 0,

    /// <summary>Index 2: bob down, reversing at the lower limit (<c>$A9:9085</c>/<c>$A9:911C</c>).</summary>
    BobDown = 2,

    /// <summary>Index 4: bob up, reversing at the upper limit (<c>$A9:90A2</c>/<c>$A9:9151</c>).</summary>
    BobUp = 4,

    /// <summary>Index 6: one-way lower that stops the joint at its limit (<c>$A9:90CF</c>/<c>$A9:9176</c>).</summary>
    Lower = 6,

    /// <summary>Index 8: one-way raise that stops the joint at its limit (<c>$A9:90EC</c>/<c>$A9:9193</c>).</summary>
    Raise = 8,
}
