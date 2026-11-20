namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The two parts of a Ridley encounter (#2560): the reveal both encounters open with, then the encounter's own fight from his lift-off.</summary>
internal enum RidleyEncounterPart
{
    /// <summary>The reveal, which runs a <see cref="RidleyRevealPhase"/>.</summary>
    Reveal,

    /// <summary>The fight from his lift-off, which runs the encounter's own phase.</summary>
    Fight,
}
