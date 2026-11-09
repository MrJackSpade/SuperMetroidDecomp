using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One whole picture Samus's body can be drawn as (#1513, #1585): the body frame that names it, the halves and chest cover
/// it is composed of, and the half pictures its frame selects.
/// </summary>
/// <param name="Frame">The pose and picture frame the picture is named by.</param>
/// <param name="Composition">The halves and chest cover the picture is composed of.</param>
/// <param name="Pictures">The half pictures the frame selects.</param>
internal readonly record struct SamusShownBody(SamusBodyFrame Frame, SamusBodyComposition Composition, SamusBodyFramePictures Pictures);
