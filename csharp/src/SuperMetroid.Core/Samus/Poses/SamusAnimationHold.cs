namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Holds the animation on this frame without reloading its timer: the timer stays expired, so the next tick wraps it
/// and moves on to the frame after. The aimed falling poses use it to spend one extra tick before their held frame.
/// </summary>
internal sealed record SamusAnimationHold : SamusAnimationInstruction;
