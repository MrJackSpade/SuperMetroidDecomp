using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The encounter that owns Ridley's body, Ceres's or Lower Norfair's (#2230): what it does with the cues his shared body
/// animation leaves to it, his lift-off into its flight and the chance to cut his fireballing short.
/// </summary>
internal interface IRidleyBodyOwner
{
    /// <summary>Starts the encounter's flight as Ridley lifts off at the end of his take-off; the rise itself is the body's.</summary>
    void LiftOff();

    /// <summary>
    /// Whether the encounter cuts Ridley's fireballing short at this cue: the segment his body animation continues in, or
    /// null to play on.
    /// </summary>
    /// <param name="samus">Samus, whose energy the encounter may read.</param>
    RidleyBodySegment? CutFireballingShort(SamusState samus);
}
