using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What the import composes one whole Samus body picture of (#1513, #1585): a pose's top half at a picture frame, its
/// bottom half unless the frame's top half is the complete body, and the chest cover the forward-facing Power Suit body
/// shows between them, in that drawing order.
/// </summary>
/// <param name="Top">The top half the picture shows.</param>
/// <param name="Bottom">The bottom half the picture shows, or null when the frame shows none.</param>
/// <param name="ChestCover">Where the chest cover sits relative to the body's origin, or null for a body without one.</param>
internal readonly record struct SamusBodyComposition(SamusBodyHalfFrame Top, SamusBodyHalfFrame? Bottom, Offset16<ScreenPixels>? ChestCover);
