namespace SuperMetroid.Core.Fx;

/// <summary>
/// One drift of the scrolling sky's bands (#1687): how far the bands that move with it slide sideways each frame. The
/// bands start together when the room loads and move together every frame time runs, so every band of one drift shows
/// the same offset and the sky keeps one phase per drift.
/// </summary>
/// <param name="speed">How far the drift's bands slide each frame, in pixels and 65536ths of a pixel.</param>
public sealed class SkyDrift(SignedSixteenSixteen speed)
{
    /// <summary>How far the drift's bands slide each frame.</summary>
    public SignedSixteenSixteen Speed { get; } = speed;
}
