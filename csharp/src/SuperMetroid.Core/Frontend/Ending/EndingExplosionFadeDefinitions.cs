namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The planet explosion's shared colour and camera transition constants.</summary>
internal static partial class EndingExplosionFadeDefinitions
{
    /// <summary>How far the explosion widens the turning floor's zoom each update.</summary>
    public static FloorZoomStep ZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 64);
    /// <summary>The crossfade's colour steps.</summary>
    public const int Steps = 32;
    /// <summary>
    /// The updates the explosion holds after its crossfade, with the escape backdrop off, before its finale begins; the
    /// flyaway's floor is ready once the hold ends.
    /// </summary>
    public const int HoldUpdates = 16;
}
