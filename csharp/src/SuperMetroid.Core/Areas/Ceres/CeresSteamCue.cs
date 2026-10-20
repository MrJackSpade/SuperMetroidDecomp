namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres steam vents' programs, with the operands its native instruction carries.</summary>
internal abstract record CeresSteamCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresSteamCue()
    {
    }

    /// <summary>
    /// Counts the vent's activation timer down, continuing at <see cref="Target"/> while it runs and at
    /// <see cref="Alternate"/>, visible and tangible, once it expires (<c>DecrementActivationTimerGotoYOrY2</c>, $A6:F127).
    /// </summary>
    /// <param name="Target">The program the vent continues at while its timer runs.</param>
    /// <param name="Alternate">The program the vent continues at once its timer expires.</param>
    internal sealed record CountDownToActivation(CeresSteamProgram Target, CeresSteamProgram Alternate) : CeresSteamCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(CeresSteamCueKind Kind) : CeresSteamCue;
}
