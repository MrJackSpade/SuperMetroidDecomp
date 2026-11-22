namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The timing of the Torizos' colour fades (#1512, #2236); their colours are the import's.</summary>
internal static partial class TorizoShadeDefinitions
{
    /// <summary>
    /// The shared fade counter's last value at which a Torizo's fade step moves his colours: the step there reaches the
    /// colouring faded toward, and the step after completes the fade and resets the counter. A fade's colours move in this
    /// many parts, one for each of its own steps after its first (#2302).
    /// </summary>
    internal const int FadeLastStep = 13;
}
