namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the camera follows Samus's grapple swing (<see cref="SamusGrappleState.CameraScroll"/>).</summary>
public enum GrappleCameraScroll
{
    /// <summary>The camera scrolls as it does without the grapple.</summary>
    Ordinary,

    /// <summary>A fast swing: the camera moves three pixels an update toward its dead zone (<see cref="GrappleSlowScrollDefinitions.Step"/>).</summary>
    Slow,
}
