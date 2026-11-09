namespace SuperMetroid.Core.Assets;

/// <summary>One frame of an animation and how long it is shown once selected.</summary>
/// <param name="Frame">The frame the animation selects.</param>
/// <param name="Duration">The updates the frame is shown for before the animation selects its next frame.</param>
public readonly record struct TimedArtworkFrame(ArtworkFrame Frame, int Duration);
