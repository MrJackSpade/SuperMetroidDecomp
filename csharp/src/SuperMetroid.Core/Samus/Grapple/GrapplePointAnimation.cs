using static SuperMetroid.Core.Samus.Grapple.GrapplePointAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple point's animation advances.</summary>
internal static class GrapplePointAnimation
{
    /// <summary>Decrements the endpoint timer, advances one frame on expiry, and returns to the first frame past the last.</summary>
    /// <remarks>
    /// A restored frame up to half the stored value's range past the last frame also returns to the first; one beyond that
    /// reads as before the start, so it keeps advancing until the stored value wraps back to the first frame.
    /// </remarks>
    internal static void Advance(SamusGrappleState state)
    {
        state.PointAnimationTimer = state.PointAnimationTimer.Decrement().Remaining;
        if (!state.PointAnimationTimer.IsNegative) return;
        state.PointAnimationTimer = WrappingCountdown.Start(Delay);
        int next = (state.PointAnimationFrame + 1) % SourceCycleFrames;
        state.PointAnimationFrame = next >= FrameCount && next - FrameCount < SourceCycleFrames / 2 ? 0 : next;
    }
}
