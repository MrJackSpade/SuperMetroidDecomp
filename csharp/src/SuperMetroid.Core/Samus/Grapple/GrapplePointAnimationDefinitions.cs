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
    private const int SourceWordFrames = 128;
    /// <summary>$9B:BFC6/C645 NTSC delay: five, decremented through zero for a six-call interval.</summary>
    internal const ushort Delay = 5;

    /// <summary>Decrements the endpoint timer, advances one frame on expiry, and returns to the first frame past the last.</summary>
    /// <remarks>
    /// A restored frame up to half the source word's span past the last frame also returns to the first; one beyond that
    /// reads as before the start, so it keeps advancing until the source word wraps back to the first frame.
    /// </remarks>
    internal static void Advance(SamusGrappleState state)
    {
        state.PointAnimationTimer = state.PointAnimationTimer.Decrement().Remaining;
        if (!state.PointAnimationTimer.IsNegative) return;
        state.PointAnimationTimer = WrappingCountdown.Start(Delay);
        int next = (state.PointAnimationFrame + 1) % SourceWordFrames;
        state.PointAnimationFrame = next >= FrameCount && next - FrameCount < SourceWordFrames / 2 ? 0 : next;
    }
}
