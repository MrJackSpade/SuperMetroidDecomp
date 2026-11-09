namespace SuperMetroid.Core.Frontend;

/// <summary>What the frontend plays when Samus dies.</summary>
internal static class SamusDeathDefinitions
{
    /// <summary>The death fanfare, the death music set's first own song, selected after every sound and the current music stop.</summary>
    public const MusicTrack FanfareTrack = MusicTrack.FirstSetSong;

    /// <summary>The steps of the gradual colour change in which the display fades to black as Samus dies.</summary>
    public const int FadeDenominator = 6;
}
