using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The whole pictures the import composes Samus's body sheet with (#1513, #1585): every installed picture frame each pose
/// shows, composed of the halves its own frame selects.
/// </summary>
internal static class SamusShownBodies
{
    /// <summary>The bodies a body's pictures are drawn as.</summary>
    extension(SamusShownFrames frames)
    {
        /// <summary>
        /// Every whole picture Samus's body can be drawn as with <paramref name="body"/>'s pictures: each picture frame of
        /// each pose that is installed, composed by the pose's own halves rule from the half pictures that frame selects, in
        /// the cartridge's pose order and frame order, which the body sheet is laid out in. What the body showed before never
        /// chooses a picture's halves.
        /// </summary>
        internal IEnumerable<SamusShownBody> Bodies(SamusBodySources body)
        {
            Ensure.NotNull(body);
            foreach (SamusPoseId pose in SamusShownFrames.Poses.OrderBy(SamusPoseNumbers.Number))
            {
                foreach (int frame in frames.pictured[pose])
                {
                    if (!body.TryPictures(pose, frame, out SamusBodyFramePictures pictures))
                        continue;
                    yield return new SamusShownBody(new SamusBodyFrame(pose, frame), SamusBodyCompositions.Of(pose, frame, body), pictures);
                }
            }
        }
    }
}
