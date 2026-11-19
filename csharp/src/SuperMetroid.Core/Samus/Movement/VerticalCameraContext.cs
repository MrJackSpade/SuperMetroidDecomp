namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which way Samus moves vertically and how far above her the camera aims its top for each, as the camera's vertical target reads them.</summary>
/// <param name="YDirection">Samus's vertical direction this frame.</param>
/// <param name="FallingCameraOffset">How many pixels above Samus the camera aims its top while she moves any way but up; zero or more.</param>
/// <param name="RisingCameraOffset">How many pixels above Samus the camera aims its top while she moves up; zero or more.</param>
public readonly record struct VerticalCameraContext(
    SamusVerticalDirection YDirection,
    int FallingCameraOffset,
    int RisingCameraOffset)
{
    /// <summary>
    /// How many pixels above Samus the camera's top edge sits while she is not
    /// moving up. A distance, so a negative one is rejected.
    /// </summary>
    public int FallingCameraOffset { get; } = Ensure.AtLeastZero(FallingCameraOffset);

    /// <summary>
    /// How many pixels above Samus the camera's top edge sits while she moves up.
    /// A distance, so a negative one is rejected.
    /// </summary>
    public int RisingCameraOffset { get; } = Ensure.AtLeastZero(RisingCameraOffset);
}
