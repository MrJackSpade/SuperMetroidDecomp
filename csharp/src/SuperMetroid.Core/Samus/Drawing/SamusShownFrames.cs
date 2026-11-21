using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The animation frames Samus's body can show in each pose, and the picture frame each of them draws (#1513, #2048), as the
/// import decided them once and the installation holds them: the body sheet holds a whole-body picture for exactly the
/// picture frames, and the draw rejects any animation frame no path reaches.
/// </summary>
/// <remarks>
/// A frame the body shows that is not one of the pose's picture frames draws the pose's nearest picture frame at or before
/// it, a drawing decision authorised by the owner's ruling on #1513 that every Samus body frame is a whole Samus of its
/// pose; the animation frame itself, its timing and everything gameplay reads from it stay as the game computes them.
/// </remarks>
public sealed class SamusShownFrames
{
    /// <summary>The picture frame each frame each pose shows draws, by pose.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>> pictureFrames;

    /// <summary>Keeps the picture frame each shown frame of each pose draws.</summary>
    /// <param name="pictureFrames">Each pose's shown frames, each with the picture frame it draws, which the pose shows too.</param>
    /// <exception cref="InvalidDataException">A picture frame is not itself a frame its pose shows as its own picture.</exception>
    internal SamusShownFrames(IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>> pictureFrames)
    {
        Ensure.NotNull(pictureFrames);
        foreach ((SamusPoseId pose, IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame> frames) in pictureFrames)
        {
            if (frames is null)
                throw new InvalidDataException($"Samus pose {pose} has no shown frames.");
            foreach (SamusAnimationFrame picture in frames.Values)
            {
                if (!frames.TryGetValue(picture, out SamusAnimationFrame drawn) || drawn != picture)
                    throw new InvalidDataException($"Samus pose {pose} draws picture frame {picture}, which is not one of its pictures.");
            }
        }
        this.pictureFrames = pictureFrames.ToDictionary();
    }

    /// <summary>The poses the body can show, with the frames each shows and the picture frame each of them draws.</summary>
    internal IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>> PictureFrames => pictureFrames;

    /// <summary>
    /// The picture frame the body draws for <paramref name="pose"/> at animation frame <paramref name="frame"/>, when some
    /// path of the game shows that frame: the frame itself when it is one of the pose's picture frames, otherwise the
    /// pose's nearest picture frame at or before it.
    /// </summary>
    internal bool TryPictureFrame(SamusPoseId pose, SamusAnimationFrame frame, out SamusAnimationFrame picture)
    {
        picture = default;
        return pictureFrames.TryGetValue(pose, out IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>? frames) &&
            frames.TryGetValue(frame, out picture);
    }

    /// <summary>The picture frame the body draws for <paramref name="pose"/> at animation frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidOperationException">No path of the game shows that frame.</exception>
    internal SamusAnimationFrame PictureFrame(SamusPoseId pose, SamusAnimationFrame frame) =>
        TryPictureFrame(pose, frame, out SamusAnimationFrame picture)
            ? picture
            : throw new InvalidOperationException($"Samus's body cannot show pose {pose} at animation frame {frame}: no path of the game reaches it.");

    /// <summary>
    /// The picture frame the body draws for <paramref name="samus"/>: her pose at her animation frame as the pose's animation
    /// reads it (<see cref="SamusState.AnimationFrameOf"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">No path of the game shows her pose, or that frame of it.</exception>
    internal SamusAnimationFrame PictureFrame(SamusState samus) =>
        pictureFrames.TryGetValue(Ensure.NotNull(samus).Pose, out IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>? frames) && frames.Count > 0
            ? PictureFrame(samus.Pose, samus.AnimationFrameOf(samus.Pose))
            : throw new InvalidOperationException($"Samus's body cannot show pose {samus.Pose}: no path of the game gives it to her.");

    /// <summary>The picture frames of <paramref name="pose"/>, in its animation's order: the frames the pose shows as their own pictures.</summary>
    internal IEnumerable<SamusAnimationFrame> Pictures(SamusPoseId pose) =>
        pictureFrames.TryGetValue(pose, out IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>? frames)
            ? frames.Where(pair => pair.Key == pair.Value).Select(pair => pair.Key).Order(SamusAnimationDelays.AnimationForPose(pose).FrameOrder)
            : [];
}
