using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import pipeline holds it (#1465): the cartridge's definition-group starts, per-pose frame lists
/// and four-byte frame selections, from which the import resolves once the half pictures each pose shows at each
/// animation frame, and the placement each of those frames carries (#1517). A half picture is a numbered body definition
/// record: a group's start plus a selection's position, which may run into the next group. The half pictures and the
/// compositions are the import's private knowledge, read from the component's import data (#1638): it composes the whole
/// picture of each pose frame from them (#1585), and decides once which frames the body sheet pictures, which the
/// installation holds with where each places the body (<see cref="InstalledBody"/>); gameplay receives only that.
/// </summary>
public sealed class SamusBodySources
{
    /// <summary>Number of records in the cartridge's pose tables, which the import reads them by.</summary>
    public const int PoseCount = SamusPoseNumbering.RecordCount;
    /// <summary>Number of four-byte frame selectors, stored one after another.</summary>
    public const int FrameCount = 1143;
    /// <summary>Number of upper-body definition groups.</summary>
    public const int TopSetCount = 13;
    /// <summary>Number of lower-body definition groups.</summary>
    public const int BottomSetCount = 11;
    /// <summary>Number of numbered body definition records, top and bottom groups together.</summary>
    public const int RecordCount = 435;

    /// <summary>Each upper-body definition group's first record.</summary>
    private readonly int[] topSetStarts;
    /// <summary>Each lower-body definition group's first record.</summary>
    private readonly int[] bottomSetStarts;
    /// <summary>Each pose's first frame selector.</summary>
    internal readonly Dictionary<SamusPoseId, int> poseFirstFrames;
    /// <summary>The contiguous frame-selector interval.</summary>
    private readonly SamusBodyFrameSelection[] frames;
    /// <summary>Each pose's installed animation frames' half pictures, or none where not installed.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> posePictures;

    /// <summary>Validates the body's native tables and resolves the half pictures each pose frame shows.</summary>
    /// <param name="parts">Samus's body tables, offsets and dependent artwork catalogs.</param>
    /// <exception cref="InvalidDataException">A table has an invalid length, a selector component is out of range, or a pose's frame list is outside the selectors.</exception>
    public SamusBodySources(SamusBodyArtworkParts parts)
        : this(parts, Installed(Ensure.NotNull(parts)))
    {
    }

    /// <summary>Binds the frames <paramref name="installed"/> resolved from <paramref name="parts"/>.</summary>
    private SamusBodySources(SamusBodyArtworkParts parts, InstalledFrames installed)
    {
        Gameplay = new InstalledSamusBodyArtwork(Content(parts, installed));
        Atmosphere = Ensure.NotNull(parts.Atmosphere);
        ArmCannon = Ensure.NotNull(parts.ArmCannon);
        posePictures = installed.Pictures;
        originRises = installed.OriginRises;
        Spritemaps = Ensure.NotNull(parts.Spritemaps);
        DeathSteps = Ensure.NotNull(parts.DeathSteps);
        topSetStarts = (int[])parts.TopSetStarts.Clone();
        bottomSetStarts = (int[])parts.BottomSetStarts.Clone();
        poseFirstFrames = parts.PoseFirstFrames.ToDictionary();
        frames = (SamusBodyFrameSelection[])parts.Frames.Clone();
        Placements = parts.Placements.ToDictionary();
    }

    /// <summary>
    /// What the game would read of this body: where each frame whose half pictures and placement resolve places it, each
    /// pose's art origin, her death steps and the arm cannon's placements. The installation holds the frames the body sheet
    /// pictures of these (<see cref="InstalledBody"/>), which are every frame the game draws.
    /// </summary>
    public InstalledSamusBodyArtwork Gameplay { get; }

    /// <summary>Where each pose's resolved animation frames place the body, or none where not resolved.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyList<int?>> originRises;

    /// <summary>The appearances of the atmospheric effects, which the atmosphere sheet is composed from.</summary>
    public SamusAtmosphericSourceArtwork Atmosphere { get; }

    /// <summary>The arm cannon's descriptors, appearances and tiles, which the overlay sheet is composed from.</summary>
    public SamusArmCannonSourceArtwork ArmCannon { get; }

    /// <summary>The compositions each pose half shows, which the import composes the whole body pictures from.</summary>
    public SamusSpritemapSources Spritemaps { get; }

    /// <summary>
    /// The half pictures <paramref name="pose"/>'s <paramref name="animationFrame"/> selects, when the pose has a record and
    /// the frame is installed; false for any other pair.
    /// </summary>
    public bool TryPictures(SamusPoseId pose, int animationFrame, out SamusBodyFramePictures pictures)
    {
        pictures = default;
        if (!posePictures.TryGetValue(pose, out IReadOnlyList<SamusBodyFramePictures?>? shown) ||
            (uint)animationFrame >= (uint)shown.Count || shown[animationFrame] is not { } selected)
            return false;
        pictures = selected;
        return true;
    }

    /// <summary>
    /// The fatal-damage suit, suitless and whiteout colours her death shades are pre-coloured from (#1512); gameplay
    /// reads only the order of those steps, through <see cref="InstalledSamusBodyArtwork.DeathSteps"/>.
    /// </summary>
    public SuperMetroid.Core.Samus.Death.SamusDeathSteps DeathSteps { get; }

    /// <summary>Where each pose with a record places the body: its art origin and, where they differ, each frame's own placement.</summary>
    public IReadOnlyDictionary<SamusPoseId, SamusBodyPosePlacement> Placements { get; }

    /// <summary>
    /// SHA-256 of all visual selectors and bundled Samus presentation catalogs the import composes the sheets from. The body
    /// and death-explosion characters are import data the installation never holds (#1636); the sheets composed from them
    /// carry their identity, and what the installation holds of the body for the game carries its own
    /// (<see cref="InstalledSamusBody.ContentIdentity"/>).
    /// </summary>
    public string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-body-v6", content =>
    {
        foreach (int start in TopSetStarts) content.Append("top set start", start);
        foreach (int start in BottomSetStarts) content.Append("bottom set start", start);
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose first frame", poseFirstFrames[pose]);
        AppendPlacements(content, Placements);
        foreach (SamusBodyFrameSelection frame in Frames)
        {
            content.Append("top set", frame.TopSet);
            content.Append("top position", frame.TopPosition);
            content.Append("bottom set", frame.BottomSet);
            content.Append("bottom position", frame.BottomPosition);
        }
        content.Append("spritemaps", Convert.FromHexString(Spritemaps.ContentIdentity));
        content.Append("atmosphere", Convert.FromHexString(Atmosphere.ContentIdentity));
        content.Append("death steps", Convert.FromHexString(DeathSteps.ContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });

    /// <summary>Each upper-body definition group's first record.</summary>
    public ReadOnlySpan<int> TopSetStarts => topSetStarts;

    /// <summary>Each lower-body definition group's first record.</summary>
    public ReadOnlySpan<int> BottomSetStarts => bottomSetStarts;

    /// <summary>The contiguous frame-selector interval.</summary>
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => frames;

    /// <summary>The definition record <paramref name="picture"/> shows: the import numbers the pictures by their records.</summary>
    public static int Record(SamusBodyPicture picture) => picture.Ordinal;

    /// <summary>The picture that shows definition <paramref name="record"/>.</summary>
    public static SamusBodyPicture Picture(int record) => new(record);

    /// <summary>
    /// Adds every pose's placement to <paramref name="content"/> in the cartridge's pose order, which the import keeps
    /// the identity in: its art origin, then its frames' own placements when it has them.
    /// </summary>
    private static void AppendPlacements(SelectedPresentationHash content, IReadOnlyDictionary<SamusPoseId, SamusBodyPosePlacement> placements)
    {
        Ensure.NotNull(content);
        Ensure.NotNull(placements);
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            SamusBodyPosePlacement placement = placements[pose];
            content.Append("graphics y offset", placement.GraphicsYOffset);
            if (placement.FrameRises is not { } rises)
                continue;
            content.Append("frame rise count", rises.Length);
            foreach (int rise in rises)
                content.Append("frame rise", rise);
        }
    }

    /// <summary>
    /// The number of definitions each group start holds: every record between it and the next start in sorted order (or
    /// the end of the records). It does not restrict a frame's cross-group selection.
    /// </summary>
    /// <exception cref="InvalidDataException">A start is negative, repeated or past the end of the records.</exception>
    internal static IReadOnlyDictionary<int, int> GroupLengths(IReadOnlyList<int> topStarts, IReadOnlyList<int> bottomStarts)
    {
        Ensure.NotNull(topStarts);
        Ensure.NotNull(bottomStarts);
        int[] sorted = [.. topStarts.Concat(bottomStarts).Order()];
        var counts = new Dictionary<int, int>();
        for (int index = 0; index < sorted.Length; index++)
        {
            int start = sorted[index], end = index + 1 == sorted.Length ? RecordCount : sorted[index + 1];
            if (start < 0 || end <= start)
                throw new InvalidDataException($"Samus definition group at record {start} has invalid physical bounds.");
            counts.Add(start, end - start);
        }
        return counts;
    }

    /// <summary>What the installed body holds for gameplay: where each resolved frame places the body, and its catalogs.</summary>
    private static SamusBodyArtworkContent Content(SamusBodyArtworkParts parts, InstalledFrames installed) => new()
    {
        OriginRises = installed.OriginRises,
        GraphicsYOffsets = parts.Placements.ToDictionary(pose => pose.Key, pose => pose.Value.GraphicsYOffset),
        DeathSteps = Ensure.NotNull(parts.DeathSteps),
        ArmCannon = Ensure.NotNull(parts.ArmCannon).Gameplay,
    };

    /// <summary>
    /// What the installation holds of this body for the game (#1638): each pose's art origin, every body frame the body sheet
    /// pictures with where it places the body, her death steps and the arm cannon's placement at every frame each pose shows.
    /// </summary>
    /// <param name="framesBeyondAnimation">What an animation frame past a pose's program reads, which decides the frames her body can show.</param>
    internal InstalledSamusBody InstalledBody(FramesBeyondAnimation<SamusPoseId, SamusPoseAnimationStep> framesBeyondAnimation) =>
        new(Placements.ToDictionary(pose => pose.Key, pose => pose.Value.GraphicsYOffset),
            SheetFrames(originRises, Spritemaps, Ensure.NotNull(framesBeyondAnimation)), DeathSteps, ArmCannon.Installed(framesBeyondAnimation));

    /// <summary>
    /// Every whole picture the body sheet holds, in the cartridge's pose order and frame order: each frame
    /// <paramref name="installed"/> pictures, composed of the halves its own frame selects (#1513, #1585).
    /// </summary>
    /// <exception cref="InvalidDataException">A pictured frame selects no half pictures of this body.</exception>
    internal IEnumerable<SamusShownBody> SheetBodies(InstalledSamusBody installed)
    {
        foreach (SamusBodyFrame frame in Ensure.NotNull(installed).Frames)
        {
            if (!TryPictures(frame.Pose, frame.PictureFrame, out SamusBodyFramePictures pictures))
                throw new InvalidDataException($"Samus body frame {frame.Name} is pictured but selects no installed half pictures.");
            yield return new SamusShownBody(frame, SamusBodyCompositions.Of(frame.Pose, frame.PictureFrame, this), pictures);
        }
    }

    /// <summary>
    /// The body frames the body sheet pictures (#1513, #1585, #1638), and where each places the body: each picture frame of
    /// each pose some path gives Samus, as her own animation shows it with <paramref name="framesBeyondAnimation"/>, whose half
    /// pictures and placement are installed and whose halves' animation frames the pose-half table holds. A body whose
    /// halves reach past the table draws nothing the sheet could hold: the draw rejects it.
    /// </summary>
    private static Dictionary<SamusPoseId, IReadOnlyDictionary<int, int>> SheetFrames(Dictionary<SamusPoseId, IReadOnlyList<int?>> installed,
        SamusSpritemapSources spritemaps, FramesBeyondAnimation<SamusPoseId, SamusPoseAnimationStep> framesBeyondAnimation)
    {
        SamusShownFrames shown = SamusShownFrames.Of(framesBeyondAnimation);
        Dictionary<SamusPoseId, Dictionary<int, int>> pictured = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => new Dictionary<int, int>());
        foreach (SamusPoseId pose in SamusShownFrames.Poses)
        {
            if (!installed.TryGetValue(pose, out IReadOnlyList<int?>? rises))
                continue;
            foreach (int frame in shown.pictured[pose])
            {
                if ((uint)frame >= (uint)rises.Count || rises[frame] is not { } rise ||
                    !Covers(spritemaps, new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, frame)) ||
                    (SamusBodyCompositions.ShowsBottomHalf(pose, frame) &&
                     !Covers(spritemaps, new SamusBodyHalfFrame(pose, SamusBodyHalf.Bottom, frame))))
                    continue;
                pictured[pose].Add(frame, rise);
            }
        }
        return pictured.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<int, int>)pose.Value);
    }

    /// <summary>Whether <paramref name="half"/>'s animation frame is one the table holds for its pose half.</summary>
    private static bool Covers(SamusSpritemapSources spritemaps, SamusBodyHalfFrame half) =>
        (uint)half.AnimationFrame < (uint)spritemaps.Frames(half).Count;

    /// <summary>
    /// Validates the native tables and resolves, for every pose and every animation frame the body sheets cover that the
    /// selector table holds, the half pictures the frame's selector names and where the frame places the body, or none
    /// when either named record or the placement is not installed.
    /// </summary>
    private static InstalledFrames Installed(SamusBodyArtworkParts parts)
    {
        Ensure.NotNull(parts.TopSetStarts);
        Ensure.NotNull(parts.BottomSetStarts);
        Ensure.NotNull(parts.PoseFirstFrames);
        Ensure.NotNull(parts.Frames);
        if (parts.TopSetStarts.Length != TopSetCount || parts.BottomSetStarts.Length != BottomSetCount ||
            parts.Frames.Length != FrameCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");
        if (!SamusPoseNumbers.CoversRecordedPoses(parts.PoseFirstFrames))
            throw new InvalidDataException("Samus body frame lists do not cover exactly the poses with records.");
        if (parts.Frames.Any(frame => !IsSelectorComponent(frame.TopSet) || !IsSelectorComponent(frame.TopPosition) ||
                !IsSelectorComponent(frame.BottomSet) || !IsSelectorComponent(frame.BottomPosition)))
            throw new InvalidDataException("Samus body frame selector components must be table indices from 0 through 255.");
        foreach (int firstFrame in parts.PoseFirstFrames.Values)
            if ((uint)firstFrame >= FrameCount)
                throw new InvalidDataException($"Samus pose selects invalid frame list {firstFrame}.");
        Ensure.NotNull(parts.Placements);
        if (!SamusPoseNumbers.CoversRecordedPoses(parts.Placements) || parts.Placements.Values.Any(placement => placement is null))
            throw new InvalidDataException("Samus body placements do not cover exactly the poses with records.");
        int first = GroupLengths(parts.TopSetStarts, parts.BottomSetStarts).Keys.Min();
        // A frame is installed when both its pictures and its placement resolve.
        bool TryInstall(SamusBodyFrameSelection selection, SamusBodyPosePlacement placement, int animationFrame,
            out SamusBodyFramePictures selected, out int frameRise)
        {
            selected = default;
            if (!placement.TryFrameRise(animationFrame, out frameRise) ||
                !TryDefinitionRecord(parts.TopSetStarts, parts.BottomSetStarts, first, SamusBodyHalf.Top, selection.TopSet, selection.TopPosition, out int top) ||
                (selection.NamesBottom && !TryDefinitionRecord(parts.TopSetStarts, parts.BottomSetStarts, first, SamusBodyHalf.Bottom,
                    selection.BottomSet, selection.BottomPosition, out _)))
                return false;
            selected = new SamusBodyFramePictures(Picture(top), selection.NamesBottom
                ? Picture(parts.BottomSetStarts[selection.BottomSet] + selection.BottomPosition) : null);
            return true;
        }
        var pictures = new Dictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>>();
        var rises = new Dictionary<SamusPoseId, IReadOnlyList<int?>>();
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            int firstFrame = parts.PoseFirstFrames[pose];
            SamusBodyPosePlacement placement = parts.Placements[pose];
            int count = Math.Min(SamusBodySheetDefinitions.AnimationFrames, FrameCount - firstFrame);
            var framePictures = new SamusBodyFramePictures?[count];
            var frameRises = new int?[count];
            for (int frame = 0; frame < count; frame++)
            {
                if (!TryInstall(parts.Frames[firstFrame + frame], placement, frame, out SamusBodyFramePictures shown, out int rise))
                    continue;
                framePictures[frame] = shown;
                frameRises[frame] = rise;
            }
            pictures.Add(pose, framePictures);
            rises.Add(pose, frameRises);
        }
        return new InstalledFrames(pictures, rises);
    }

    /// <summary>The frames an installed body resolves from its tables.</summary>
    /// <param name="Pictures">Each pose's installed animation frames' half pictures, or none where not installed.</param>
    /// <param name="OriginRises">Where each pose's installed animation frames place the body, or none where not installed.</param>
    private sealed record InstalledFrames(Dictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> Pictures,
        Dictionary<SamusPoseId, IReadOnlyList<int?>> OriginRises);

    /// <summary>
    /// The record a native set and position select: the selected group's first record plus the position. The native
    /// selector steps past the chosen group's first without a per-group bound, so several authored frames land in the
    /// next group; the physical record is resolved, not a per-group index.
    /// </summary>
    private static bool TryDefinitionRecord(int[] topStarts, int[] bottomStarts, int firstRecord, SamusBodyHalf half, int set, int position, out int record)
    {
        int[] starts = half == SamusBodyHalf.Top ? topStarts : bottomStarts;
        record = set >= 0 && set < starts.Length && IsSelectorComponent(position) ? starts[set] + position : -1;
        return record >= firstRecord && record < RecordCount;
    }

    /// <summary>Whether a frame-selector component is a table index the selector can hold (0 through 255).</summary>
    private static bool IsSelectorComponent(int component) => component is >= 0 and <= byte.MaxValue;
}
