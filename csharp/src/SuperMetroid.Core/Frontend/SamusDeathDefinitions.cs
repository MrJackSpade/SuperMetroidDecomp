namespace SuperMetroid.Core.Frontend;

/// <summary>What the frontend plays when Samus dies.</summary>
internal static class SamusDeathDefinitions
{
    /// <summary>The death fanfare, selected after every sound and the current music stop.</summary>
    public const MusicTrack FanfareTrack = MusicTrack.SamusDeath;

    /// <summary>The steps of the gradual colour change in which the display fades to black as Samus dies.</summary>
    public const int FadeDenominator = 6;
}
