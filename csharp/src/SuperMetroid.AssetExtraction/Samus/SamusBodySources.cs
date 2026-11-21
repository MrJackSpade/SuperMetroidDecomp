using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import pipeline holds it (#1465, #2556): each pose's art origin and, for each picture frame the
/// import decoded from the cartridge's tables, the half pictures it selects and where it places the body (#1517). The half
/// pictures and the compositions are the import's private knowledge, read from the component's import data (#1638): it
/// composes the whole picture of each pose frame from them (#1585), and decides once which frames the body sheet pictures,
/// which the installation holds with where each places the body (<see cref="InstalledBody"/>); gameplay receives only that.
/// </summary>
public sealed class SamusBodySources
{
    /// <summary>Each pose's art origin.</summary>
    private readonly Dictionary<SamusPoseId, int> artOrigins;
    /// <summary>Each pose's decoded picture frames, by their place in the game's layout of the pose's animation.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>> frames;

    /// <summary>Validates the decoded body and binds its catalogs.</summary>
    /// <param name="parts">Samus's decoded body, its compositions and dependent artwork catalogs.</param>
    /// <exception cref="InvalidDataException">The art origins or frames do not cover exactly the poses with records, or a frame's halves are not decoded compositions.</exception>
    public SamusBodySources(SamusBodyArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.ArtOrigins);
        Ensure.NotNull(parts.Frames);
        if (!SamusPoseNumbers.CoversRecordedPoses(parts.ArtOrigins) || !SamusPoseNumbers.CoversRecordedPoses(parts.Frames) ||
            parts.Frames.Values.Any(pose => pose is null || pose.Values.Any(frame => frame is null)))
            throw new InvalidDataException("Samus body art origins and frames do not cover exactly the poses with records.");
        Spritemaps = Ensure.NotNull(parts.Spritemaps);
        foreach ((SamusPoseId pose, IReadOnlyDictionary<int, SamusBodyDecodedFrame> decoded) in parts.Frames)
            foreach (int frame in decoded.Keys)
                if (!Spritemaps.Covers(new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, frame)))
                    throw new InvalidDataException($"Samus body frame {frame} of {pose} has no decoded top half.");
        artOrigins = parts.ArtOrigins.ToDictionary();
        frames = parts.Frames.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<int, SamusBodyDecodedFrame>)pose.Value.ToDictionary());
        Atmosphere = Ensure.NotNull(parts.Atmosphere);
        ArmCannon = Ensure.NotNull(parts.ArmCannon);
        DeathSteps = Ensure.NotNull(parts.DeathSteps);
        Gameplay = new InstalledSamusBodyArtwork(new SamusBodyArtworkContent
        {
            OriginRises = frames.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<SamusAnimationFrame, int>)NamedRises(pose.Key, pose.Value)),
            ArtOrigins = artOrigins,
            DeathSteps = DeathSteps,
            ArmCannon = ArmCannon.Gameplay,
        });
    }

    /// <summary>
    /// What the game would read of this body: where each decoded frame places it, each pose's art origin, her death steps
    /// and the arm cannon's placements. The installation holds the frames the body sheet pictures of these
    /// (<see cref="InstalledBody"/>), which are every frame the game draws.
    /// </summary>
    public InstalledSamusBodyArtwork Gameplay { get; }

    /// <summary>The appearances of the atmospheric effects, which the atmosphere sheet is composed from.</summary>
    public SamusAtmosphericSourceArtwork Atmosphere { get; }

    /// <summary>The arm cannon's descriptors, appearances and tiles, which the overlay sheet is composed from.</summary>
    public SamusArmCannonSourceArtwork ArmCannon { get; }

    /// <summary>The compositions each pose half shows, which the import composes the whole body pictures from.</summary>
    public SamusSpritemapSources Spritemaps { get; }

    /// <summary>
    /// The fatal-damage suit, suitless and whiteout colours her death shades are pre-coloured from (#1512); gameplay
    /// reads only the order of those steps, through <see cref="InstalledSamusBodyArtwork.DeathSteps"/>.
    /// </summary>
    private SuperMetroid.Core.Samus.Death.SamusDeathSteps DeathSteps { get; }

    /// <summary>Each pose's decoded picture frames, by their place in the game's layout of the pose's animation.</summary>
    internal IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>> Frames => frames;

    /// <summary>
    /// The half pictures <paramref name="pose"/>'s <paramref name="animationFrame"/> selects, when the pose has a record and
    /// the frame is decoded; false for any other pair.
    /// </summary>
    private bool TryPictures(SamusPoseId pose, int animationFrame, out SamusBodyFramePictures pictures)
    {
        pictures = default;
        if (!frames.TryGetValue(pose, out IReadOnlyDictionary<int, SamusBodyDecodedFrame>? decoded) ||
            !decoded.TryGetValue(animationFrame, out SamusBodyDecodedFrame? selected))
            return false;
        pictures = selected.Pictures;
        return true;
    }

    /// <summary>
    /// <paramref name="decoded"/>, where each of <paramref name="pose"/>'s decoded frames places the body, by the frame of
    /// the animation each lies at.
    /// </summary>
    private static Dictionary<SamusAnimationFrame, int> NamedRises(SamusPoseId pose, IReadOnlyDictionary<int, SamusBodyDecodedFrame> decoded)
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        return decoded.ToDictionary(frame => SamusAnimationLayout.FrameAt(animation, frame.Key), frame => frame.Value.OriginRise);
    }

    /// <summary>
    /// What the installation holds of this body for the game (#1638): each pose's art origin, every body frame the body sheet
    /// pictures with where it places the body, her death steps and the arm cannon's placement at every frame each pose shows.
    /// </summary>
    /// <param name="shown">The frames her body can show in each pose and the picture each draws, as the import decided them.</param>
    internal InstalledSamusBody InstalledBody(SamusShownFrames shown) =>
        new(artOrigins, SheetFrames(Ensure.NotNull(shown)), shown, DeathSteps, ArmCannon.Installed(shown));

    /// <summary>
    /// Every whole picture the body sheet holds, in the installation's pose order and frame order: each frame
    /// <paramref name="installed"/> pictures, composed of the halves its own frame selects (#1513, #1585).
    /// </summary>
    /// <exception cref="InvalidDataException">A pictured frame is not decoded.</exception>
    internal IEnumerable<SamusShownBody> SheetBodies(InstalledSamusBody installed)
    {
        foreach (SamusBodyFrame frame in Ensure.NotNull(installed).Frames)
        {
            int place = SamusAnimationLayout.Offset(SamusAnimationDelays.AnimationForPose(frame.Pose), frame.PictureFrame);
            if (!TryPictures(frame.Pose, place, out SamusBodyFramePictures pictures))
                throw new InvalidDataException($"Samus body frame {frame.Name} is pictured but selects no installed half pictures.");
            yield return new SamusShownBody(frame, SamusBodyCompositions.Of(frame.Pose, place, this), pictures);
        }
    }

    /// <summary>
    /// The body frames the body sheet pictures (#1513, #1585, #1638), and where each places the body: each picture frame of
    /// each pose some path gives Samus, as <paramref name="shown"/> decided her animation shows it, that the import decoded,
    /// by the frame of the pose's animation.
    /// </summary>
    private Dictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, int>> SheetFrames(SamusShownFrames shown)
    {
        Dictionary<SamusPoseId, Dictionary<SamusAnimationFrame, int>> pictured =
            SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => new Dictionary<SamusAnimationFrame, int>());
        foreach (SamusPoseId pose in shown.PictureFrames.Keys)
        {
            if (!frames.TryGetValue(pose, out IReadOnlyDictionary<int, SamusBodyDecodedFrame>? decoded))
                continue;
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            foreach (int place in shown.Pictures(pose).Select(picture => SamusAnimationLayout.Offset(animation, picture)))
            {
                if (decoded.TryGetValue(place, out SamusBodyDecodedFrame? frame))
                    pictured[pose].Add(SamusAnimationLayout.FrameAt(animation, place), frame.OriginRise);
            }
        }
        return pictured.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<SamusAnimationFrame, int>)pose.Value);
    }
}
