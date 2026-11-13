using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What the game binds of Samus's installed body (#1616, #1638): each pose's art origin, every body frame the body sheet
/// pictures with where that frame places the body, her death steps and the arm cannon's layout. The installed
/// <c>samus-body.json</c> holds the art origins and frames; the tables, compositions and sprite parts the import composed the
/// body sheet from are import data (<see cref="SamusBodySources"/>), and the body sheet's frames are named from this.
/// </summary>
public sealed class InstalledSamusBody
{
    /// <summary>The animation frames a pose's body pictures are covered for: the animation frame is a byte.</summary>
    internal const int AnimationFrames = SamusBodySheetDefinitions.AnimationFrames;

    /// <summary>Each pose's art origin.</summary>
    private readonly Dictionary<SamusPoseId, int> graphicsYOffsets;
    /// <summary>Each pose's pictured body frames: where each animation frame places the body.</summary>
    private readonly Dictionary<SamusPoseId, SortedDictionary<int, int>> frames;

    /// <summary>Validates and holds the body the game binds.</summary>
    /// <param name="graphicsYOffsets">Each pose with a record's art origin, -128 through 127.</param>
    /// <param name="frames">
    /// Each pose with a record's pictured body frames, by animation frame from 0 through 255: how many pixels above Samus's
    /// drawn point the frame's body origin sits. A pose the body sheet pictures no frame of has none.
    /// </param>
    /// <param name="deathSteps">The order her death sequence steps through and the room's whiteout colours.</param>
    /// <param name="armCannon">Each pose's arm-cannon descriptor within the drawing window.</param>
    /// <exception cref="InvalidDataException">
    /// The art origins or the frames do not cover exactly the poses with records, an animation frame is outside 0 through
    /// 255, or an art origin is outside -128 through 127.
    /// </exception>
    internal InstalledSamusBody(IReadOnlyDictionary<SamusPoseId, int> graphicsYOffsets,
        IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<int, int>> frames, SamusDeathSteps deathSteps, SamusArmCannonLayout armCannon)
    {
        Ensure.NotNull(graphicsYOffsets);
        Ensure.NotNull(frames);
        DeathSteps = Ensure.NotNull(deathSteps);
        ArmCannon = Ensure.NotNull(armCannon);
        if (!SamusPoseNumbers.CoversRecordedPoses(graphicsYOffsets) || !SamusPoseNumbers.CoversRecordedPoses(frames) ||
            frames.Values.Any(pose => pose is null))
            throw new InvalidDataException("Samus body art origins and frames do not cover exactly the poses with records.");
        if (frames.Values.Any(pose => pose.Keys.Any(frame => (uint)frame >= AnimationFrames)))
            throw new InvalidDataException($"Samus body frames must be animation frames from 0 through {AnimationFrames - 1}.");
        this.graphicsYOffsets = graphicsYOffsets.ToDictionary();
        this.frames = frames.ToDictionary(pose => pose.Key, pose => new SortedDictionary<int, int>(pose.Value.ToDictionary()));
        Frames = [.. SamusPoseNumbers.RecordedPoses.OrderBy(SamusPoseNumbers.Number)
            .SelectMany(pose => this.frames[pose].Keys.Select(frame => new SamusBodyFrame(pose, frame)))];
        Gameplay = new InstalledSamusBodyArtwork(new SamusBodyArtworkContent
        {
            OriginRises = this.frames.ToDictionary(pose => pose.Key, pose => (IReadOnlyList<int?>)OriginRises(pose.Value)),
            GraphicsYOffsets = this.graphicsYOffsets,
            DeathSteps = deathSteps,
            ArmCannon = armCannon.Gameplay,
        });
    }

    /// <summary>What the game reads of the body.</summary>
    public InstalledSamusBodyArtwork Gameplay { get; }

    /// <summary>
    /// Every body frame the body sheet pictures, in the cartridge's pose order and frame order: the frames the game can
    /// draw, which name the sheet's frames.
    /// </summary>
    public IReadOnlyList<SamusBodyFrame> Frames { get; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public SamusDeathSteps DeathSteps { get; }

    /// <summary>Each pose's arm-cannon descriptor within the drawing window.</summary>
    public SamusArmCannonLayout ArmCannon { get; }

    /// <summary>The art origin of <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal int GraphicsYOffset(SamusPoseId pose) => graphicsYOffsets.TryGetValue(pose, out int offset)
        ? offset
        : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose has no record in the pose tables.");

    /// <summary>The pictured body frames of <paramref name="pose"/>, by animation frame: where each places the body.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal IReadOnlyDictionary<int, int> FramesOf(SamusPoseId pose) => frames.TryGetValue(pose, out SortedDictionary<int, int>? shown)
        ? shown
        : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose has no record in the pose tables.");

    /// <summary>SHA-256 of each pose's art origin and pictured frames, the death steps and the arm cannon's layout.</summary>
    public string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-body-game-v1", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            content.Append("graphics y offset", graphicsYOffsets[pose]);
            content.Append("frame count", frames[pose].Count);
            foreach ((int frame, int rise) in frames[pose])
            {
                content.Append("frame", frame);
                content.Append("origin rise", rise);
            }
        }
        content.Append("death steps", Convert.FromHexString(DeathSteps.ContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });

    /// <summary>Each animation frame's origin rise up to the last pictured frame, none for a frame the sheet does not picture.</summary>
    private static int?[] OriginRises(SortedDictionary<int, int> frames)
    {
        var rises = new int?[frames.Count == 0 ? 0 : frames.Keys.Max() + 1];
        foreach ((int frame, int rise) in frames)
            rises[frame] = rise;
        return rises;
    }
}
