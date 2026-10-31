namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The two Speed Booster steps, inherited from a Screw Attack cycle, at which Gravity's read runs past the end of its own
/// cycle list; each copies the colours <see cref="SpeedBoosterOverrunColours{TPhase, TColour}"/> holds for it.
/// </summary>
internal enum SpeedBoosterPaletteOverrun
{
    /// <summary>Step four, the first past Gravity's list, which copies black.</summary>
    FirstStepPastCycle,
    /// <summary>Step five, the second past Gravity's list, which copies the colours the Grapple handler's code reads as.</summary>
    SecondStepPastCycle,
}
