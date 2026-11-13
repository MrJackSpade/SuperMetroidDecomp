namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-credits wave that ripples Samus sideways while the backdrop waits, and how it moves.</summary>
internal static class EndingWavySamusDefinitions
{
    /// <summary>The wavy Samus effect: the wave's peak displacement, in 1/256ths of a pixel (64 pixels).</summary>
    internal const int Amplitude = 16384;
    /// <summary>
    /// The wavy Samus effect: the sine steps the wave's angle advances each frame, so the ripple travels up
    /// the screen.
    /// </summary>
    internal const int AngleStepPerFrame = 8;
    /// <summary>
    /// The wavy-Samus setup: the wave's angle at screen line zero before its first advance, one step short of a whole turn,
    /// so the first advance carries it past the turn's start (#1627: an angle on the turn, not a minus-one marker).
    /// </summary>
    internal static TrigonometryAngle InitialAngle { get; } = TrigonometryAngle.Wrapped(-1);
    /// <summary>
    /// The wavy-Samus frame behaviour: the screen lines of one whole cycle of the wave, a half cycle of 64 lines
    /// and its sign-inverted mirror.
    /// </summary>
    internal const int Period = 128;
    /// <summary>
    /// The frame the wave first shows: the frame it starts on only sets it up (the wavy-Samus setup), and the
    /// following frame is its first advance.
    /// </summary>
    internal const int FirstWaveAge = 2;
    /// <summary>The wavy Samus scene advances the wave's vertical scroll by two per call.</summary>
    internal const int VerticalStep = 2;
}
