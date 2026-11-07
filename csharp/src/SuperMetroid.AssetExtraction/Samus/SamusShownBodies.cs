using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The full bodies the import pictures Samus's body sheet with (#1513): every picture frame each pose shows, with every
/// bottom picture the frame can show.
/// </summary>
internal static class SamusShownBodies
{
    /// <summary>The bodies a body's pictures are drawn as.</summary>
    extension(SamusShownFrames frames)
    {
        /// <summary>
        /// Every full body Samus's body can be drawn as with <paramref name="body"/>'s pictures: each picture frame of each
        /// pose that shows installed pictures, drawn by the pose's own halves rule, with every bottom picture the frame can
        /// show, in pose, frame and bottom order.
        /// </summary>
        internal IEnumerable<SamusShownBody> Bodies(InstalledSamusBodyArtwork body)
        {
            Ensure.NotNull(body);
            Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>> bottoms = frames.BottomsShown(body);
            foreach (SamusPoseId pose in SamusShownFrames.Poses)
            {
                foreach (int frame in frames.pictured[pose])
                {
                    if (!body.TryFrame(pose, frame, out SamusBodyFramePictures pictures))
                        continue;
                    SamusBodyFrame drawn = SamusBodyDrawing.Frame(pose, frame, body);
                    foreach (SamusBodyPicture? bottom in bottoms[new SamusPoseFrame(pose, frame)].OrderBy(bottom => bottom?.Ordinal ?? -1))
                        yield return new SamusShownBody(drawn, new SamusBodyPictures(pictures.Top, bottom));
                }
            }
        }

        /// <summary>
        /// The bottom pictures each picture frame with installed pictures can show (#1513): its own when it names one;
        /// otherwise the bottom picture the picture before it showed, for the picture frames of its own pose it follows, and
        /// for a picture frame a pose is started or published on any bottom picture a frame names, or none before any frame
        /// has named one.
        /// </summary>
        private Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>> BottomsShown(InstalledSamusBodyArtwork body)
        {
            var pictures = new Dictionary<SamusPoseFrame, SamusBodyFramePictures>();
            foreach (SamusPoseId pose in SamusShownFrames.Poses)
            {
                foreach (int frame in frames.pictured[pose])
                {
                    if (body.TryFrame(pose, frame, out SamusBodyFramePictures framePictures))
                        pictures.Add(new SamusPoseFrame(pose, frame), framePictures);
                }
            }
            var named = new HashSet<SamusBodyPicture?> { null };
            foreach (SamusBodyFramePictures framePictures in pictures.Values)
            {
                if (framePictures.Bottom is { } bottom)
                    named.Add(bottom);
            }
            var entered = new HashSet<SamusPoseFrame>();
            foreach (KeyValuePair<SamusPoseId, HashSet<int>> pose in frames.entries)
            {
                foreach (int frame in pose.Value)
                    entered.Add(new SamusPoseFrame(pose.Key, frames.PictureFrame(pose.Key, frame)));
            }
            // The pictures shown one after another: the picture frames of the animation frames each shown frame follows.
            var follows = new Dictionary<SamusPoseFrame, HashSet<int>>();
            foreach (KeyValuePair<SamusPoseFrame, HashSet<int>> frame in frames.predecessors)
            {
                var picture = new SamusPoseFrame(frame.Key.Pose, frames.PictureFrame(frame.Key.Pose, frame.Key.Frame));
                if (!follows.TryGetValue(picture, out HashSet<int>? before))
                {
                    before = [];
                    follows.Add(picture, before);
                }
                foreach (int earlier in frame.Value)
                    before.Add(frames.PictureFrame(frame.Key.Pose, earlier));
            }
            var bottoms = new Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>>();
            foreach (KeyValuePair<SamusPoseFrame, SamusBodyFramePictures> frame in pictures)
            {
                var frameBottoms = new HashSet<SamusBodyPicture?>();
                if (frame.Value.Bottom is { } own)
                    frameBottoms.Add(own);
                else if (entered.Contains(frame.Key))
                    frameBottoms.UnionWith(named);
                bottoms.Add(frame.Key, frameBottoms);
            }
            // A picture that names no bottom keeps the bottom of the picture before it, which may itself have kept an earlier one.
            bool changed;
            do
            {
                changed = false;
                foreach (KeyValuePair<SamusPoseFrame, HashSet<SamusBodyPicture?>> frame in bottoms)
                {
                    if (pictures[frame.Key].Bottom is not null || !follows.TryGetValue(frame.Key, out HashSet<int>? before))
                        continue;
                    foreach (int earlier in before)
                    {
                        // A picture that follows itself keeps what it already shows.
                        if (earlier == frame.Key.Frame ||
                            !bottoms.TryGetValue(new SamusPoseFrame(frame.Key.Pose, earlier), out HashSet<SamusBodyPicture?>? earlierBottoms))
                            continue;
                        foreach (SamusBodyPicture? bottom in earlierBottoms)
                            changed |= frame.Value.Add(bottom);
                    }
                }
            }
            while (changed);
            return bottoms;
        }
    }
}
