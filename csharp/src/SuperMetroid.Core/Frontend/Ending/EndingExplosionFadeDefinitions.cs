namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Func115–117's shared palette/camera transition constants.</summary>
internal static class EndingExplosionFadeDefinitions
{
    /// <summary>Func115 seeds var4 to 63; Func117 exits when its decrement becomes negative.</summary>
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
