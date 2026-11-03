using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import pipeline holds it (#1465): the cartridge's definition-group starts, per-pose frame lists
/// and four-byte frame selections, from which the import resolves once the half pictures each pose shows at each
/// animation frame. A half picture is a numbered body definition record: a group's start plus a selection's position,
/// which may run into the next group. The numbering is the import's private knowledge; gameplay sees only
/// <see cref="SamusBodyPicture"/> identities.
/// </summary>
public sealed class SamusBodySources : InstalledSamusBodyArtwork
{
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
    private readonly int[] poseFirstFrames;
    /// <summary>The contiguous frame-selector interval.</summary>
    private readonly SamusBodyFrameSelection[] frames;

    /// <summary>Validates the body's native tables and resolves the half pictures each pose frame shows.</summary>
    /// <param name="parts">Samus's body tables, offsets and dependent artwork catalogs.</param>
    /// <param name="bodyCharactersContentIdentity">Identity of the body characters the frames select, composed into the body sheets at installation.</param>
    /// <exception cref="InvalidDataException">A table has an invalid length, a selector component is out of range, or a pose's frame list is outside the selectors.</exception>
    public SamusBodySources(SamusBodyArtworkParts parts, string bodyCharactersContentIdentity)
        : base(Content(Ensure.NotNull(parts)))
    {
        DeathPalettes = Ensure.NotNull(parts.DeathPalettes);
        DeathTilesContentIdentity = Ensure.NotNull(parts.DeathTilesContentIdentity);
        BodyCharactersContentIdentity = Ensure.NotNull(bodyCharactersContentIdentity);
        topSetStarts = (int[])parts.TopSetStarts.Clone();
        bottomSetStarts = (int[])parts.BottomSetStarts.Clone();
        poseFirstFrames = (int[])parts.PoseFirstFrames.Clone();
        frames = (SamusBodyFrameSelection[])parts.Frames.Clone();
    }

    /// <summary>Identity of the selected death-explosion characters, whose pre-coloured sheet is installed separately.</summary>
    public string DeathTilesContentIdentity { get; }

    /// <summary>
    /// The fatal-damage suit, suitless and whiteout colours her death shades are pre-coloured from (#1512); gameplay
    /// reads only the order of those steps, through <see cref="InstalledSamusBodyArtwork.DeathSteps"/>.
    /// </summary>
    public InstalledSamusDeathPalettes DeathPalettes { get; }

    /// <summary>Identity of the selected body characters, whose pre-coloured sheets are installed separately.</summary>
    public string BodyCharactersContentIdentity { get; }

    /// <summary>The resolved signed art-origin offset of every pose, in pose-number order.</summary>
    public ReadOnlySpan<int> GraphicsYOffsets => graphicsYOffsetsTable ??=
        [.. Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset(SamusPoseNumbering.FromNumber(index)))];
    /// <summary>The materialized <see cref="GraphicsYOffsets"/>; the installed tables never change after construction.</summary>
    private int[]? graphicsYOffsetsTable;

    /// <summary>The landing table: four unsigned bytes for each landing pose.</summary>
    public ReadOnlySpan<int> LandingYOffsets => landingYOffsetsTable ??=
        [.. Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount)
            .Select(index => TryLandingYOffset(index, out int value) ? value : throw new InvalidOperationException($"Landing byte {index} is outside the table."))];
    /// <summary>The materialized <see cref="LandingYOffsets"/>; the installed tables never change after construction.</summary>
    private int[]? landingYOffsetsTable;

    /// <summary>The installed posture-transition vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> PostureYOffsets => postureYOffsetsTable ??=
        [.. Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount)
            .Select(index => TryPostureYOffset(index, out int value) ? checked((sbyte)value) : throw new InvalidOperationException($"Posture byte {index} is outside the table."))];
    /// <summary>The materialized <see cref="PostureYOffsets"/>; the installed tables never change after construction.</summary>
    private sbyte[]? postureYOffsetsTable;

    /// <summary>The installed drained-state vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> DrainedYOffsets => drainedYOffsetsTable ??=
        [.. Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount)
            .Select(index => TryDrainedYOffset(index, out int value) ? checked((sbyte)value) : throw new InvalidOperationException($"Drained byte {index} is outside the table."))];
    /// <summary>The materialized <see cref="DrainedYOffsets"/>; the installed tables never change after construction.</summary>
    private sbyte[]? drainedYOffsetsTable;

    /// <summary>SHA-256 of all visual selectors, the body characters' identity and bundled Samus presentation catalogs.</summary>
    public string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-body-v3", content =>
    {
        foreach (int start in TopSetStarts) content.Append("top set start", start);
        foreach (int start in BottomSetStarts) content.Append("bottom set start", start);
        foreach (int firstFrame in PoseFirstFrames) content.Append("pose first frame", firstFrame);
        content.Append("graphics y offsets", GraphicsYOffsets.SelectToArray(value => unchecked((byte)value)));
        content.AppendWords("landing y offsets", LandingYOffsets);
        content.Append("posture y offsets", PostureYOffsets.SelectToArray(value => unchecked((byte)value)));
        content.Append("drained y offsets", DrainedYOffsets.SelectToArray(value => unchecked((byte)value)));
        foreach (SamusBodyFrameSelection frame in Frames)
        {
            content.Append("top set", frame.TopSet);
            content.Append("top position", frame.TopPosition);
            content.Append("bottom set", frame.BottomSet);
            content.Append("bottom position", frame.BottomPosition);
        }
        content.Append("body characters", Convert.FromHexString(BodyCharactersContentIdentity));
        content.Append("spritemaps", Convert.FromHexString(Spritemaps.ContentIdentity));
        content.Append("atmosphere", Convert.FromHexString(Atmosphere.ContentIdentity));
        content.Append("death palettes", Convert.FromHexString(DeathPalettes.ContentIdentity));
        content.Append("death tiles", Convert.FromHexString(DeathTilesContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });

    /// <summary>Each upper-body definition group's first record.</summary>
    public ReadOnlySpan<int> TopSetStarts => topSetStarts;

    /// <summary>Each lower-body definition group's first record.</summary>
    public ReadOnlySpan<int> BottomSetStarts => bottomSetStarts;

    /// <summary>Each pose's first frame selector.</summary>
    public ReadOnlySpan<int> PoseFirstFrames => poseFirstFrames;

    /// <summary>The contiguous frame-selector interval.</summary>
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => frames;

    /// <summary>The definition record <paramref name="picture"/> shows: the import numbers the pictures by their records.</summary>
    public static int Record(SamusBodyPicture picture) => picture.Ordinal;

    /// <summary>The picture that shows definition <paramref name="record"/>.</summary>
    public static SamusBodyPicture Picture(int record) => new(record);

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

    /// <summary>
    /// Validates the native tables and resolves, for every pose and every animation frame the body sheets cover that the
    /// selector table holds, the half pictures the frame's selector names, or none when either named record is not installed.
    /// </summary>
    private static SamusBodyArtworkContent Content(SamusBodyArtworkParts parts)
    {
        Ensure.NotNull(parts.TopSetStarts);
        Ensure.NotNull(parts.BottomSetStarts);
        Ensure.NotNull(parts.PoseFirstFrames);
        Ensure.NotNull(parts.Frames);
        if (parts.TopSetStarts.Length != TopSetCount || parts.BottomSetStarts.Length != BottomSetCount ||
            parts.PoseFirstFrames.Length != PoseCount || parts.Frames.Length != FrameCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");
        if (parts.Frames.Any(frame => !IsSelectorComponent(frame.TopSet) || !IsSelectorComponent(frame.TopPosition) ||
                !IsSelectorComponent(frame.BottomSet) || !IsSelectorComponent(frame.BottomPosition)))
            throw new InvalidDataException("Samus body frame selector components must be table indices from 0 through 255.");
        foreach (int firstFrame in parts.PoseFirstFrames)
            if ((uint)firstFrame >= FrameCount)
                throw new InvalidDataException($"Samus pose selects invalid frame list {firstFrame}.");
        int first = GroupLengths(parts.TopSetStarts, parts.BottomSetStarts).Keys.Min();
        SamusBodyFramePictures? Pictures(SamusBodyFrameSelection selection) =>
            TryDefinitionRecord(parts.TopSetStarts, parts.BottomSetStarts, first, SamusBodyHalf.Top, selection.TopSet, selection.TopPosition, out int top) &&
            (!selection.NamesBottom || TryDefinitionRecord(parts.TopSetStarts, parts.BottomSetStarts, first, SamusBodyHalf.Bottom,
                selection.BottomSet, selection.BottomPosition, out _))
                ? new SamusBodyFramePictures(Picture(top), selection.NamesBottom
                    ? Picture(parts.BottomSetStarts[selection.BottomSet] + selection.BottomPosition) : null)
                : null;
        return new SamusBodyArtworkContent
        {
            Frames = Enumerable.Range(0, PoseCount).ToDictionary(SamusPoseNumbering.FromNumber, pose =>
            {
                int firstFrame = parts.PoseFirstFrames[pose];
                return (IReadOnlyList<SamusBodyFramePictures?>)parts.Frames
                    .Skip(firstFrame).Take(Math.Min(SamusBodySheetDefinitions.AnimationFrames, FrameCount - firstFrame)).Select(Pictures).ToArray();
            }),
            GraphicsYOffsets = parts.GraphicsYOffsets,
            LandingYOffsets = parts.LandingYOffsets,
            PostureYOffsets = parts.PostureYOffsets,
            DrainedYOffsets = parts.DrainedYOffsets,
            Spritemaps = parts.Spritemaps,
            Atmosphere = parts.Atmosphere,
            DeathSteps = parts.DeathPalettes.Steps,
            ArmCannon = parts.ArmCannon,
        };
    }

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
