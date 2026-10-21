using SuperMetroid.Core.Rendering;

namespace SuperMetroid.RenderVerification.Samus;

/// <summary>A frame to render and its context.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Context">The context.</param>
internal readonly record struct ContextFrame(RenderFrameSnapshot Frame, string Context);
