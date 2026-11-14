namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres steam vents' animations: what it does, with the values it acts on.</summary>
internal abstract record CeresSteamCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresSteamCue()
    {
    }

    /// <summary>
    /// Counts the vent's activation timer down: while it runs the vent waits again in <see cref="Dormant"/>; once it expires
    /// the vent appears, harmful, and plays <see cref="Puffing"/>.
    /// </summary>
    /// <param name="Dormant">The vent's dormant wait, which it plays again while its timer runs.</param>
    /// <param name="Puffing">The vent's puff, which it plays once its timer expires.</param>
    internal sealed record CountDownToActivation(CeresSteamSegment Dormant, CeresSteamSegment Puffing) : CeresSteamCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CeresSteamCueKind Kind) : CeresSteamCue;
}
