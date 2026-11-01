namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Movement index of one Mother Brain neck joint: the value the lower and upper neck
/// motion dispatchers use as an index into
/// their five-entry jump tables. Both joints share the same table layout.
/// </summary>
public enum MotherBrainNeckMovement
{
    /// <summary>Index 0: the joint does not move (<c>RTS_A99084</c>/<c>RTS_A9911B</c>).</summary>
    None = 0,

    /// <summary>Index 2: bob down, reversing at the lower limit.</summary>
    BobDown = 2,

    /// <summary>Index 4: bob up, reversing at the upper limit.</summary>
    BobUp = 4,

    /// <summary>Index 6: one-way lower that stops the joint at its limit.</summary>
    Lower = 6,

    /// <summary>Index 8: one-way raise that stops the joint at its limit.</summary>
    Raise = 8,
}
