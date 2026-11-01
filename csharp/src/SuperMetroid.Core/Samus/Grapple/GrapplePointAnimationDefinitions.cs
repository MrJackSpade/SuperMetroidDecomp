namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Endpoint animation, distinct from the rope's angle selection.</summary>
internal static class GrapplePointAnimationDefinitions
{
    /// <summary>The endpoint animation shows four frames before returning to the first.</summary>
    internal const int FrameCount = 4;
    /// <summary>
    /// The endpoint animation advances a 16-bit tile-source word by one frame's tiles, so the frame it names repeats every 128 frames
    /// and the end test compares the signed distance past the last frame.
    /// </summary>
    internal const int SourceWordFrames = 128;
    /// <summary>NTSC delay: five, decremented through zero for a six-call interval.</summary>
    internal const int Delay = 5;
}
