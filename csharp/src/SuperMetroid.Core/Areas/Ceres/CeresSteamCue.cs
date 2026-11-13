namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres steam vents' animations: what it does, with the values it acts on.</summary>
internal abstract record CeresSteamCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresSteamCue()
    {
    }

    /// <summary>
    /// Counts the vent's activation timer down, continuing at <see cref="Target"/> while it runs and at
    /// <see cref="Alternate"/>, visible and tangible, once it expires.
    /// </summary>
    /// <param name="Target">The segment the vent continues at while its timer runs.</param>
    /// <param name="Alternate">The segment the vent continues at once its timer expires.</param>
    internal sealed record CountDownToActivation(CeresSteamSegment Target, CeresSteamSegment Alternate) : CeresSteamCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CeresSteamCueKind Kind) : CeresSteamCue;
}
