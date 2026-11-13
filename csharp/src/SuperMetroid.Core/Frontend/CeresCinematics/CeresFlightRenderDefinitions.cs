namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>How the opening Ceres flight shows its perspective floor.</summary>
internal static class CeresFlightRenderDefinitions
{
    /// <summary>The floor point, in floor pixels, that the opening flight's floor turns and zooms about.</summary>
    internal static Offset16<FloorPixels> FloorCenter => new(56, 24);

    /// <summary>The front approach's starting zoom: two floor pixels per screen pixel.</summary>
    internal static FloorZoom FrontInitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(2);

    /// <summary>The front approach keeps narrowing its zoom while it is at least this: an eighth of a floor pixel per screen pixel.</summary>
    internal static FloorZoom FrontNarrowingFloor { get; } = FloorZoom.FromTwoHundredFiftySixths(32);

    /// <summary>The rear view widens its zoom by the slow step while below this: twelve floor pixels per screen pixel.</summary>
    internal static FloorZoom RearSlowWideningLimit { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(12);

    /// <summary>The rear view widens its zoom by the fast step while below this, then ends: thirty-two floor pixels per screen pixel.</summary>
    internal static FloorZoom RearFastWideningLimit { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(32);

    /// <summary>The front approach's narrowing and the rear view's slow widening each update, in 256ths of a floor pixel per screen pixel.</summary>
    internal const int SlowZoomStep = 16;

    /// <summary>The rear view's fast widening each update, in 256ths of a floor pixel per screen pixel.</summary>
    internal const int FastZoomStep = 32;
}
