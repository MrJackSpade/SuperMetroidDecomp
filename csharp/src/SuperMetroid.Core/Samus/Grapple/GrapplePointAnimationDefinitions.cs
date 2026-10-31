namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Native endpoint animation at $9B:BFBD, distinct from the rope's angle selection.</summary>
internal static class GrapplePointAnimationDefinitions
{
    /// <summary>$9B:C342/C344: the endpoint animation shows four frames before returning to the first.</summary>
    internal const int FrameCount = 4;
    /// <summary>
    /// $9B:BFD0 advances a 16-bit tile-source word by one frame's tiles, so the frame it names repeats every 128 frames
    /// and the end test compares the signed distance past the last frame.
    /// </summary>
    internal const int SourceWordFrames = 128;
    /// <summary>$9B:BFC6/C645 NTSC delay: five, decremented through zero for a six-call interval.</summary>
    internal const int Delay = 5;
}
