namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of the ending (<see cref="Progression.SuperMetroidGameState.Ending"/>) runs.</summary>
internal enum EndingStep
{
    /// <summary>The departing gunship's gameplay continues while the screen darkens after the Zebes escape.</summary>
    LeavingZebes,
    /// <summary>Zebes's destruction, the results, the staff credits and the completion reward.</summary>
    Credits,
}
