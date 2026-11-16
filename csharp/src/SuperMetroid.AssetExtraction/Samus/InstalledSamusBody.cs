using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What the game binds of Samus's installed body (#1616, #1638): each pose's art origin, every body frame the body sheet
/// pictures with where that frame places the body, the frames her body can show with the picture each draws, her death
/// steps and the arm cannon's layout. The installed <c>samus-body.json</c> holds the art origins, frames and shown frames,
/// each frame by its name in the pose's animation (#2048); the tables, compositions and sprite parts the import composed the
/// body sheet from are import data (<see cref="SamusBodySources"/>), and the body sheet's frames are named from this.
/// </summary>
public sealed class InstalledSamusBody
{
    /// <summary>The animation frames a pose's body pictures are covered for: the animation frame is a byte.</summary>
    internal const int AnimationFrames = SamusBodySheetDefinitions.AnimationFrames;

    /// <summary>Each pose's art origin.</summary>
    private readonly Dictionary<SamusPoseId, int> graphicsYOffsets;
    /// <summary>Each pose's pictured body frames, in animation order: where each animation frame places the body.</summary>
    private readonly Dictionary<SamusPoseId, SortedDictionary<SamusAnimationFrame, int>> frames;

    /// <summary>Validates and holds the body the game binds.</summary>
    /// <param name="graphicsYOffsets">Each pose with a record's art origin.</param>
    /// <param name="frames">
    /// Each pose with a record's pictured body frames, by frame of the pose's animation: how many pixels above Samus's drawn
    /// point the frame's body origin sits. A pose the body sheet pictures no frame of has none.
    /// </param>
    /// <param name="shownFrames">The frames her body can show in each pose and the picture frame each draws.</param>
    /// <param name="deathSteps">The order her death sequence steps through and the room's whiteout colours.</param>
    /// <param name="armCannon">Each pose's arm-cannon placement.</param>
    /// <exception cref="InvalidDataException">The art origins or the frames do not cover exactly the poses with records.</exception>
    internal InstalledSamusBody(IReadOnlyDictionary<SamusPoseId, int> graphicsYOffsets,
        IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, int>> frames, SamusShownFrames shownFrames,
        SamusDeathSteps deathSteps, SamusArmCannonLayout armCannon)
    {
        Ensure.NotNull(graphicsYOffsets);
        Ensure.NotNull(frames);
        ShownFrames = Ensure.NotNull(shownFrames);
        DeathSteps = Ensure.NotNull(deathSteps);
        ArmCannon = Ensure.NotNull(armCannon);
        if (!SamusPoseNumbers.CoversRecordedPoses(graphicsYOffsets) || !SamusPoseNumbers.CoversRecordedPoses(frames) ||
            !SamusPoseNumbers.CoversRecordedPoses(shownFrames.PictureFrames) || frames.Values.Any(pose => pose is null))
            throw new InvalidDataException("Samus body art origins, frames and shown frames do not cover exactly the poses with records.");
        this.graphicsYOffsets = graphicsYOffsets.ToDictionary();
        this.frames = frames.ToDictionary(pose => pose.Key, pose => new SortedDictionary<SamusAnimationFrame, int>(pose.Value.ToDictionary()));
        Frames = [.. SamusPoseNumbers.RecordedPoses.OrderBy(SamusPoseNumbers.Number)
            .SelectMany(pose => this.frames[pose].Keys.Select(frame => new SamusBodyFrame(pose, frame)))];
        Gameplay = new InstalledSamusBodyArtwork(new SamusBodyArtworkContent
        {
            OriginRises = this.frames.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<SamusAnimationFrame, int>)pose.Value),
            GraphicsYOffsets = this.graphicsYOffsets,
            DeathSteps = deathSteps,
            ArmCannon = armCannon.Gameplay,
        });
    }

    /// <summary>What the game reads of the body.</summary>
    public InstalledSamusBodyArtwork Gameplay { get; }

    /// <summary>The frames her body can show in each pose and the picture frame each draws, which the game draws by.</summary>
    public SamusShownFrames ShownFrames { get; }

    /// <summary>
    /// Every body frame the body sheet pictures, in the cartridge's pose order and frame order: the frames the game can
    /// draw, which name the sheet's frames.
    /// </summary>
    public IReadOnlyList<SamusBodyFrame> Frames { get; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public SamusDeathSteps DeathSteps { get; }

    /// <summary>Each pose's arm-cannon placement.</summary>
    public SamusArmCannonLayout ArmCannon { get; }

    /// <summary>The art origin of <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal int GraphicsYOffset(SamusPoseId pose) => graphicsYOffsets.TryGetValue(pose, out int offset)
        ? offset
        : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose has no record in the pose tables.");

    /// <summary>The pictured body frames of <paramref name="pose"/>, by frame of its animation: where each places the body.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal IReadOnlyDictionary<SamusAnimationFrame, int> FramesOf(SamusPoseId pose) =>
        frames.TryGetValue(pose, out SortedDictionary<SamusAnimationFrame, int>? shown)
            ? shown
            : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose has no record in the pose tables.");

    /// <summary>
    /// SHA-256 of each pose's art origin, pictured frames and shown frames with their pictures, the death steps and the arm
    /// cannon's layout.
    /// </summary>
    public string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-body-game-v2", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            content.Append("graphics y offset", graphicsYOffsets[pose]);
            content.Append("frame count", frames[pose].Count);
            foreach ((SamusAnimationFrame frame, int rise) in frames[pose])
            {
                content.AppendName("frame", animation.NameOf(frame));
                content.Append("origin rise", rise);
            }
            IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame> shown = ShownFrames.PictureFrames[pose];
            content.Append("shown frame count", shown.Count);
            foreach ((SamusAnimationFrame frame, SamusAnimationFrame picture) in shown.OrderBy(pair => pair.Key))
            {
                content.AppendName("shown frame", animation.NameOf(frame));
                content.AppendName("picture frame", animation.NameOf(picture));
            }
        }
        content.Append("death steps", Convert.FromHexString(DeathSteps.ContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });
}
