namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The whole body one update draws as one picture (#1513): a pose's top half at an animation frame, its bottom half
/// unless the frame's top half is the complete body, and the chest cover the forward-facing Power Suit body shows between
/// them, in that drawing order.
/// </summary>
/// <param name="Top">The top half the body shows.</param>
/// <param name="Bottom">The bottom half the body shows, or null when the frame shows none.</param>
/// <param name="ChestCover">Where the chest cover sits relative to the body's origin, or null for a body without one.</param>
public readonly record struct SamusBodyFrame(SamusBodyHalfFrame Top, SamusBodyHalfFrame? Bottom, Offset16<ScreenPixels>? ChestCover);
