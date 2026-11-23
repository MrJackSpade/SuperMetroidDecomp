namespace SuperMetroid.Core.Assets;

/// <summary>One frame of an animation and how long it is shown once selected.</summary>
/// <param name="Frame">The frame the animation selects.</param>
public readonly partial record struct TimedArtworkFrame(ArtworkFrame Frame);
