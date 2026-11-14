namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The planet explosion's shared colour and camera transition constants.</summary>
internal static class EndingExplosionFadeDefinitions
{
    /// <summary>The explosion's countdown starts at 63; its hold begins once the countdown passes zero.</summary>
    public const int InitialCountdown = 63;
    /// <summary>The explosion widens the turning floor's zoom by four 256ths of a floor pixel per screen pixel each call.</summary>
    public const int ZoomStep = 4;
    /// <summary>There are thirty-two palette steps over sixty-four calls.</summary>
    public const int Steps = 32;
    /// <summary>
    /// The updates the explosion holds after its crossfade, with the escape backdrop off, before its finale begins; the
    /// flyaway's floor is ready once the hold ends.
    /// </summary>
    public const int HoldUpdates = 16;
}
