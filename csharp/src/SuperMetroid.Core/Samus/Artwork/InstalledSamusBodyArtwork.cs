using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which body half pictures each Samus pose frame shows, and where the body is placed.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. The import resolves, once, the half pictures each pose shows
/// at every animation frame the body sheets cover (#1465); their characters are composed into the pre-coloured sheets at
/// installation, and this catalog names the pictures a frame shows and carries their characters' identity.
/// </remarks>
public abstract class InstalledSamusBodyArtwork
{
    /// <summary>Number of native Samus pose entries with body-art selectors.</summary>
    public const int PoseCount = SamusPoseNumbering.RecordCount;

    /// <summary>Pose art-origin edits independent of physical collision offsets.</summary>
    internal readonly Dictionary<int, int> graphicsYOffsets;
    /// <summary>Edited landing visual bytes, including an adjacent unaligned-read byte.</summary>
    internal readonly Dictionary<int, int> landingYOffsets;
    /// <summary>The installed posture-transition art offsets.</summary>
    private readonly sbyte[] postureYOffsets;
    /// <summary>The installed drained-state art offsets.</summary>
    private readonly sbyte[] drainedYOffsets;
    /// <summary>The half pictures each pose shows at each covered animation frame, or none where they are not installed.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> poseFrames;

    /// <summary>Which body picture each pose half shows at each animation frame, for these body frames.</summary>
    public InstalledSamusSpritemaps Spritemaps { get; }

    /// <summary>The selected appearance of Samus's atmospheric effects.</summary>
    public InstalledSamusAtmosphericArtwork Atmosphere { get; }

    /// <summary>Editable fatal-damage suit, suitless, and whiteout colors.</summary>
    public InstalledSamusDeathPalettes DeathPalettes { get; }

    /// <summary>Identity of the selected death-explosion characters, whose pre-coloured sheet is installed separately.</summary>
    public string DeathTilesContentIdentity { get; }

    /// <summary>Identity of the selected body characters, whose pre-coloured sheets are installed separately.</summary>
    public string BodyCharactersContentIdentity { get; }

    /// <summary>Where each pose places the independent arm-cannon cover.</summary>
    public InstalledSamusArmCannonArtwork ArmCannon { get; }

    /// <summary>Validates a complete Samus body-art catalog.</summary>
    /// <param name="content">The pictures each pose frame shows, the body's placement and the catalogs it binds.</param>
    /// <exception cref="InvalidDataException">A table has an invalid length, or the pictures do not cover exactly the poses with records.</exception>
    protected InstalledSamusBodyArtwork(SamusBodyArtworkContent content)
    {
        Ensure.NotNull(content);
        Ensure.NotNull(content.Frames);
        Ensure.NotNull(content.GraphicsYOffsets);
        Ensure.NotNull(content.Spritemaps);
        Ensure.NotNull(content.Atmosphere);
        Ensure.NotNull(content.DeathPalettes);
        Ensure.NotNull(content.DeathTilesContentIdentity);
        Ensure.NotNull(content.ArmCannon);
        Ensure.NotNull(content.LandingYOffsets);
        Ensure.NotNull(content.PostureYOffsets);
        Ensure.NotNull(content.DrainedYOffsets);
        Ensure.NotNull(content.BodyCharactersContentIdentity);
        if (content.GraphicsYOffsets.Length != PoseCount ||
            content.LandingYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount ||
            content.PostureYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount ||
            content.DrainedYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");
        if (content.Frames.Count != PoseCount || content.Frames.Keys.Any(pose => !SamusPoseNumbering.HasRecord(pose)) ||
            content.Frames.Values.Any(frames => frames is null))
            throw new InvalidDataException("Samus body pictures do not cover exactly the poses with records.");

        if (content.GraphicsYOffsets.Any(value => value is < sbyte.MinValue or > sbyte.MaxValue))
            throw new InvalidDataException("Samus art origins must fit in one signed byte.");
        this.graphicsYOffsets = Enumerable.Range(0, content.GraphicsYOffsets.Length)
            .Where(index => content.GraphicsYOffsets[index] != SamusBodyPlacements.DefaultGraphicsYOffset(SamusPoseNumbering.FromNumber(index)))
            .ToDictionary(index => index, index => content.GraphicsYOffsets[index]);
        if (content.LandingYOffsets.Any(value => value is < 0 or > byte.MaxValue))
            throw new InvalidDataException("Samus landing visual bytes must fit in one byte.");
        this.landingYOffsets = Enumerable.Range(0, content.LandingYOffsets.Length)
            .Where(index => content.LandingYOffsets[index] != (SamusBodyPlacements.LandingSourceIndex(index) == index
                ? SamusBodyPlacements.DefaultLandingByte(index)
                : content.LandingYOffsets[SamusBodyPlacements.LandingSourceIndex(index)]))
            .ToDictionary(index => index, index => content.LandingYOffsets[index]);
        this.postureYOffsets = (sbyte[])content.PostureYOffsets.Clone();
        this.drainedYOffsets = (sbyte[])content.DrainedYOffsets.Clone();
        this.poseFrames = content.Frames.ToDictionary();
        Spritemaps = content.Spritemaps;
        Atmosphere = content.Atmosphere;
        DeathPalettes = content.DeathPalettes;
        DeathTilesContentIdentity = content.DeathTilesContentIdentity;
        BodyCharactersContentIdentity = content.BodyCharactersContentIdentity;
        ArmCannon = content.ArmCannon;
    }

    /// <summary>SHA-256 of all visual selectors, the body characters' identity and every bundled Samus catalog.</summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Gets the resolved signed art-origin offset for every pose.</summary>
    public ReadOnlySpan<int> GraphicsYOffsets => graphicsYOffsetsTable ??= Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset(SamusPoseNumbering.FromNumber(index))).ToArray();
    /// <summary>The materialized <see cref="GraphicsYOffsets"/>; the installed tables never change after construction.</summary>
    private int[]? graphicsYOffsetsTable;
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<int> LandingYOffsets => landingYOffsetsTable ??= Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount).Select(LandingByte).ToArray();
    /// <summary>The materialized <see cref="LandingYOffsets"/>; the installed tables never change after construction.</summary>
    private int[]? landingYOffsetsTable;
    /// <summary>Gets the installed posture-transition vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> PostureYOffsets => postureYOffsets;
    /// <summary>Gets the installed drained-state vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> DrainedYOffsets => drainedYOffsets;
    /// <summary>Resolves an edited landing byte or its native shared-source alias.</summary>
    private int LandingByte(int index) => landingYOffsets.ContainsKey(index)
        ? landingYOffsets[index] : SamusBodyPlacements.LandingSourceIndex(index) == index
            ? SamusBodyPlacements.DefaultLandingByte(index) : LandingByte(SamusBodyPlacements.LandingSourceIndex(index));
    /// <summary>Attempts to read the native unaligned landing-offset word at a byte index.</summary>
    /// <param name="index">The starting byte index.</param>
    /// <param name="value">Receives the little-endian landing-offset word.</param>
    /// <returns><see langword="true"/> when two bytes are available.</returns>
    public bool TryLandingYOffset(int index, out int value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount - 1)
        {
            value = 0;
            return false;
        }
        value = LittleEndianWord.Unsigned(LandingByte(index), LandingByte(index + 1));
        return true;
    }
    /// <summary>Attempts to read a signed posture-transition art offset.</summary>
    /// <param name="index">The table byte index.</param>
    /// <param name="value">Receives the signed vertical offset.</param>
    /// <returns><see langword="true"/> when the index is in range.</returns>
    public bool TryPostureYOffset(int index, out int value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount) { value = 0; return false; }
        value = postureYOffsets[index];
        return true;
    }
    /// <summary>Attempts to read a signed drained-state art offset.</summary>
    /// <param name="index">The table byte index.</param>
    /// <param name="value">Receives the signed vertical offset.</param>
    /// <returns><see langword="true"/> when the index is in range.</returns>
    public bool TryDrainedYOffset(int index, out int value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount) { value = 0; return false; }
        value = drainedYOffsets[index];
        return true;
    }
    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    public int GraphicsYOffset(SamusPoseId pose) =>
        SamusPoseNumbering.HasRecord(pose) ? graphicsYOffsets.ContainsKey(SamusPoseNumbering.Number(pose)) ? graphicsYOffsets[SamusPoseNumbering.Number(pose)] : SamusBodyPlacements.DefaultGraphicsYOffset(pose) :
            throw new InvalidDataException($"Pose {pose} has no authored graphics Y offset.");

    /// <summary>The half pictures <paramref name="pose"/> shows at <paramref name="animationFrame"/>.</summary>
    /// <exception cref="InvalidDataException">The pose has no record, or the frame's pictures are not installed.</exception>
    public SamusBodyFramePictures Frame(SamusPoseId pose, int animationFrame) =>
        TryFrame(pose, animationFrame, out SamusBodyFramePictures pictures)
            ? pictures
            : throw new InvalidDataException($"Samus pose {pose} animation frame {animationFrame} shows no installed body pictures.");

    /// <summary>
    /// The half pictures <paramref name="pose"/> shows at <paramref name="animationFrame"/>, when the pose has a record and
    /// the frame's pictures are installed; false for any other pair, which <see cref="Frame"/> rejects.
    /// </summary>
    public bool TryFrame(SamusPoseId pose, int animationFrame, out SamusBodyFramePictures pictures)
    {
        pictures = default;
        if (!poseFrames.TryGetValue(pose, out IReadOnlyList<SamusBodyFramePictures?>? frames) ||
            (uint)animationFrame >= (uint)frames.Count || frames[animationFrame] is not { } shown)
            return false;
        pictures = shown;
        return true;
    }
}
