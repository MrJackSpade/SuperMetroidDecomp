using static SuperMetroid.Core.Samus.Grapple.GrapplePointAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple point's animation advances.</summary>
internal static class GrapplePointAnimation
{
    /// <summary>Decrements the endpoint timer, advances one frame on expiry, and returns to the first frame past the last.</summary>
    internal static void Advance(SamusGrappleState state)
    {
        state.PointAnimationTimer = state.PointAnimationTimer.Decrement().Remaining;
        if (!state.PointAnimationTimer.IsNegative) return;
        state.PointAnimationTimer = WrappingCountdown.Start(Delay);
        state.PointAnimationFrame = (state.PointAnimationFrame + 1) % FrameCount;
    }
}
