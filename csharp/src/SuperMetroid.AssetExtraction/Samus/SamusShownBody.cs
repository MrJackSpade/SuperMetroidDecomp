using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One full body Samus's body can be drawn as (#1513): the halves and chest cover drawn, and the half pictures shown.</summary>
/// <param name="Frame">The body halves and chest cover the draw shows.</param>
/// <param name="Pictures">The half pictures the body shows them with.</param>
internal readonly record struct SamusShownBody(SamusBodyFrame Frame, SamusBodyPictures Pictures);
