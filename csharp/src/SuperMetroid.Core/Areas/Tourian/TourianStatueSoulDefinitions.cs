namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>How a boss's soul, released from its Tourian statue's eye, rises out of the room.</summary>
internal static class TourianStatueSoulDefinitions
{
    /// <summary>The speed a soul leaves the statue's eye with: four pixels per update, upward.</summary>
    internal static readonly SignedEightEight LaunchSpeed = SignedEightEight.FromPixels(-4);

    /// <summary>The upward speed a rising soul gains each update: half a pixel per update.</summary>
    internal static readonly SignedEightEight RiseAcceleration = SignedEightEight.FromPixelFraction(1, 2);
}
