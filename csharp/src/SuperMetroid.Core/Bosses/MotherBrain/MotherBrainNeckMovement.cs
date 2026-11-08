namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How one Mother Brain neck joint moves on each enabled update. Both joints use the same movements. Its values are
/// arbitrary.
/// </summary>
public enum MotherBrainNeckMovement
{
    /// <summary>The joint does not move.</summary>
    None,

    /// <summary>Bob down, reversing at the lower limit.</summary>
    BobDown,

    /// <summary>Bob up, reversing at the upper limit.</summary>
    BobUp,

    /// <summary>One-way lower that stops the joint at its limit.</summary>
    Lower,

    /// <summary>One-way raise that stops the joint at its limit.</summary>
    Raise,
}
