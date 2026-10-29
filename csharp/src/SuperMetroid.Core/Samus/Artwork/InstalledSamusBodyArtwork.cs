using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which body half pictures each Samus pose frame shows, and where the body is placed.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. The body's characters are composed into its pre-coloured
/// sheets at installation; this catalog names the half pictures a frame shows and carries their characters' identity.
/// </remarks>
public sealed class InstalledSamusBodyArtwork
{
    /// <summary>Number of four-byte frame selectors, stored one after another.</summary>
    public const int FrameCount = 1143;
    /// <summary>Number of native Samus pose entries with body-art selectors.</summary>
    public const int PoseCount = SamusPoseNumbering.RecordCount;
    /// <summary>Number of upper-body definition groups.</summary>
    public const int TopSetCount = 13;
    /// <summary>Number of lower-body definition groups.</summary>
    public const int BottomSetCount = 11;

    /// <summary>Edited upper-body set starts keyed by native set index.</summary>
    internal readonly Dictionary<int, int> topSetStarts;
    /// <summary>Edited lower-body set starts keyed by native set index.</summary>
    internal readonly Dictionary<int, int> bottomSetStarts;
    /// <summary>Edited per-pose first frames, with stock frames calculated on demand.</summary>
    internal readonly Dictionary<int, int> poseFirstFrames;
    /// <summary>Pose art-origin edits independent of physical collision offsets.</summary>
    internal readonly Dictionary<int, sbyte> graphicsYOffsets;
    /// <summary>Edited landing visual bytes, including an adjacent unaligned-read byte.</summary>
    internal readonly Dictionary<int, ushort> landingYOffsets;
    /// <summary>The installed posture-transition art offsets.</summary>
    private readonly sbyte[] postureYOffsets;
    /// <summary>The installed drained-state art offsets.</summary>
    private readonly sbyte[] drainedYOffsets;
    /// <summary>Edited components of four-byte native frame selectors.</summary>
    internal readonly Dictionary<int, byte> frames;
    /// <summary>The first numbered half picture any set holds; every record from it to the last is installed.</summary>
    private readonly int firstRecord;

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

    /// <summary>Creates and validates a complete editable Samus body-art catalog.</summary>
    /// <param name="parts">Samus's body pointer tables, frame selections, offsets and dependent artwork catalogs (#633).</param>
    /// <param name="bodyCharactersContentIdentity">Identity of the body characters the frames select, composed into the body sheets at installation.</param>
    public InstalledSamusBodyArtwork(SamusBodyArtworkParts parts, string bodyCharactersContentIdentity)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(bodyCharactersContentIdentity);
        Ensure.NotNull(parts.TopSetStarts);
        Ensure.NotNull(parts.BottomSetStarts);
        Ensure.NotNull(parts.PoseFirstFrames);
        Ensure.NotNull(parts.GraphicsYOffsets);
        Ensure.NotNull(parts.Frames);
        Ensure.NotNull(parts.Spritemaps);
        Ensure.NotNull(parts.Atmosphere);
        Ensure.NotNull(parts.DeathPalettes);
        Ensure.NotNull(parts.DeathTilesContentIdentity);
        Ensure.NotNull(parts.ArmCannon);
        Ensure.NotNull(parts.LandingYOffsets);
        Ensure.NotNull(parts.PostureYOffsets);
        Ensure.NotNull(parts.DrainedYOffsets);
        if (parts.TopSetStarts.Length != TopSetCount || parts.BottomSetStarts.Length != BottomSetCount ||
            parts.PoseFirstFrames.Length != PoseCount || parts.GraphicsYOffsets.Length != PoseCount ||
            parts.LandingYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount ||
            parts.PostureYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount ||
            parts.DrainedYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount ||
            parts.Frames.Length != FrameCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");

        this.topSetStarts = Enumerable.Range(0, parts.TopSetStarts.Length)
            .Where(index => parts.TopSetStarts[index] != SamusBodyDefinitionStarts.DefaultTopStart(index))
            .ToDictionary(index => index, index => parts.TopSetStarts[index]);
        this.bottomSetStarts = Enumerable.Range(0, parts.BottomSetStarts.Length)
            .Where(index => parts.BottomSetStarts[index] != SamusBodyDefinitionStarts.DefaultBottomStart(index))
            .ToDictionary(index => index, index => parts.BottomSetStarts[index]);
        this.poseFirstFrames = Enumerable.Range(0, parts.PoseFirstFrames.Length)
            .Where(pose => parts.PoseFirstFrames[pose] != SamusBodyPoseFrames.DefaultFirstFrame(SamusPoseNumbering.FromNumber(pose)))
            .ToDictionary(pose => pose, pose => parts.PoseFirstFrames[pose]);
        this.graphicsYOffsets = Enumerable.Range(0, parts.GraphicsYOffsets.Length)
            .Where(index => parts.GraphicsYOffsets[index] != SamusBodyPlacements.DefaultGraphicsYOffset(SamusPoseNumbering.FromNumber(index)))
            .ToDictionary(index => index, index => parts.GraphicsYOffsets[index]);
        if (parts.LandingYOffsets.Any(value => value > byte.MaxValue))
            throw new InvalidDataException("Samus landing visual bytes must fit in one byte.");
        this.landingYOffsets = Enumerable.Range(0, parts.LandingYOffsets.Length)
            .Where(index => parts.LandingYOffsets[index] != (SamusBodyPlacements.LandingSourceIndex(index) == index
                ? SamusBodyPlacements.DefaultLandingByte(index)
                : parts.LandingYOffsets[SamusBodyPlacements.LandingSourceIndex(index)]))
            .ToDictionary(index => index, index => parts.LandingYOffsets[index]);

        if (parts.Frames.Any(frame => !IsSelectorComponent(frame.TopSet) || !IsSelectorComponent(frame.TopPosition) ||
                !IsSelectorComponent(frame.BottomSet) || !IsSelectorComponent(frame.BottomPosition)))
            throw new InvalidDataException("Samus body frame selector components must be table indices from 0 through 255.");
        byte[] components = parts.Frames.SelectMany(frame => new[] { (byte)frame.TopSet, (byte)frame.TopPosition,
            (byte)frame.BottomSet, (byte)frame.BottomPosition }).ToArray();
        this.frames = Enumerable.Range(0, components.Length)
            .Where(index => SamusBodyFrameSharing.SourceComponent(index) != index
                ? components[index] != components[SamusBodyFrameSharing.SourceComponent(index)]
                : !SamusBodyFrameSharing.TryComponent(index, out int calculated) || components[index] != calculated)
            .ToDictionary(index => index, index => components[index]);
        this.postureYOffsets = (sbyte[])parts.PostureYOffsets.Clone();
        this.drainedYOffsets = (sbyte[])parts.DrainedYOffsets.Clone();
        Spritemaps = parts.Spritemaps;
        Atmosphere = parts.Atmosphere;
        DeathPalettes = parts.DeathPalettes;
        DeathTilesContentIdentity = parts.DeathTilesContentIdentity;
        BodyCharactersContentIdentity = bodyCharactersContentIdentity;
        ArmCannon = parts.ArmCannon;
        // Each set's half pictures run from its start to the next start in sorted order, so together they number every
        // record from the lowest start to the last.
        firstRecord = SamusBodyDefinitionStarts.GroupLengths(parts.TopSetStarts, parts.BottomSetStarts).Keys.Min();
        foreach (int firstFrame in parts.PoseFirstFrames)
            if ((uint)firstFrame >= FrameCount)
                throw new InvalidDataException($"Samus pose selects invalid frame list {firstFrame}.");
    }

    /// <summary>SHA-256 of all visual selectors, the body characters' identity and every bundled Samus catalog.</summary>
    public string ContentIdentity => CreateContentIdentity();

    /// <summary>Gets each upper-body definition group's resolved first record.</summary>
    public ReadOnlySpan<int> TopSetStarts => topSetStartsTable ??= Enumerable.Range(0, TopSetCount).Select(index => SetStart(true, index)).ToArray();
    /// <summary>The materialized <see cref="TopSetStarts"/>; the installed tables never change after construction.</summary>
    private int[]? topSetStartsTable;
    /// <summary>Gets each lower-body definition group's resolved first record.</summary>
    public ReadOnlySpan<int> BottomSetStarts => bottomSetStartsTable ??= Enumerable.Range(0, BottomSetCount).Select(index => SetStart(false, index)).ToArray();
    /// <summary>The materialized <see cref="BottomSetStarts"/>; the installed tables never change after construction.</summary>
    private int[]? bottomSetStartsTable;
    /// <summary>Gets each pose's resolved first frame.</summary>
    public ReadOnlySpan<int> PoseFirstFrames => poseFirstFramesTable ??= Enumerable.Range(0, PoseCount).Select(pose => PoseFirstFrame(SamusPoseNumbering.FromNumber(pose))).ToArray();
    /// <summary>The materialized <see cref="PoseFirstFrames"/>; the installed tables never change after construction.</summary>
    private int[]? poseFirstFramesTable;
    /// <summary>Gets the resolved signed art-origin offset for every pose.</summary>
    public ReadOnlySpan<sbyte> GraphicsYOffsets => graphicsYOffsetsTable ??= Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset(SamusPoseNumbering.FromNumber(index))).ToArray();
    /// <summary>The materialized <see cref="GraphicsYOffsets"/>; the installed tables never change after construction.</summary>
    private sbyte[]? graphicsYOffsetsTable;
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<ushort> LandingYOffsets => landingYOffsetsTable ??= Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount).Select(LandingByte).ToArray();
    /// <summary>The materialized <see cref="LandingYOffsets"/>; the installed tables never change after construction.</summary>
    private ushort[]? landingYOffsetsTable;
    /// <summary>Gets the installed posture-transition vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> PostureYOffsets => postureYOffsets;
    /// <summary>Gets the installed drained-state vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> DrainedYOffsets => drainedYOffsets;
    /// <summary>Resolves an edited landing byte or its native shared-source alias.</summary>
    private ushort LandingByte(int index) => landingYOffsets.ContainsKey(index)
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
        value = BinaryPrimitives.ReadUInt16LittleEndian([(byte)LandingByte(index), (byte)LandingByte(index + 1)]);
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
    public sbyte GraphicsYOffset(SamusPoseId pose) =>
        SamusPoseNumbering.HasRecord(pose) ? graphicsYOffsets.ContainsKey(SamusPoseNumbering.Number(pose)) ? graphicsYOffsets[SamusPoseNumbering.Number(pose)] : SamusBodyPlacements.DefaultGraphicsYOffset(pose) :
            throw new InvalidDataException($"Pose {pose} has no authored graphics Y offset.");
    /// <summary>Gets the resolved contiguous bank-$92 frame-selector interval.</summary>
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => framesTable ??= Enumerable.Range(0, FrameCount).Select(FrameAt).ToArray();
    /// <summary>The materialized <see cref="Frames"/>; the installed tables never change after construction.</summary>
    private SamusBodyFrameSelection[]? framesTable;
    /// <summary>Resolves a frame-selector byte through edited values, aliases, or compiled defaults.</summary>
    private byte FrameComponent(int index) => frames.ContainsKey(index)
        ? frames[index] : SamusBodyFrameSharing.SourceComponent(index) != index
            ? FrameComponent(SamusBodyFrameSharing.SourceComponent(index))
            : SamusBodyFrameSharing.TryComponent(index, out int calculated) ? (byte)calculated
                : throw new InvalidDataException("Installed body frame has no selected component.");
    /// <summary>Assembles one native four-byte upper/lower body selector.</summary>
    private SamusBodyFrameSelection FrameAt(int index) => new(FrameComponent(index * 4), FrameComponent(index * 4 + 1),
        FrameComponent(index * 4 + 2), FrameComponent(index * 4 + 3));

    /// <summary>Resolves an edited pose's first frame or the compiled stock one.</summary>
    private int PoseFirstFrame(SamusPoseId pose) => poseFirstFrames.TryGetValue(SamusPoseNumbering.Number(pose), out int value)
        ? value : SamusBodyPoseFrames.DefaultFirstFrame(pose);

    /// <summary>The frame <paramref name="animationFrame"/> places after the pose's first.</summary>
    public SamusBodyFrameSelection Frame(SamusPoseId pose, int animationFrame)
    {
        if (!SamusPoseNumbering.HasRecord(pose))
            throw new InvalidDataException($"Pose {pose} has no authored Samus body frame list.");
        int index = PoseFirstFrame(pose) + animationFrame;
        if ((uint)index >= FrameCount)
            throw new InvalidDataException($"Samus frame {index} is outside extracted visual selectors.");
        SamusBodyFrameSelection frame = FrameAt(index);
        _ = DefinitionRecord(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition);
        if (frame.NamesBottom)
            _ = DefinitionRecord(SamusBodyHalf.Bottom, frame.BottomSet, frame.BottomPosition);
        return frame;
    }

    /// <summary>Resolves the selected upper or lower definition group's first record.</summary>
    private int SetStart(bool upperHalf, int set)
    {
        Dictionary<int, int> overrides = upperHalf ? topSetStarts : bottomSetStarts;
        return overrides.TryGetValue(set, out int value) ? value : upperHalf
            ? SamusBodyDefinitionStarts.DefaultTopStart(set) : SamusBodyDefinitionStarts.DefaultBottomStart(set);
    }
    /// <summary>Whether a frame-selector component is a table index the selector can hold (0 through 255).</summary>
    private static bool IsSelectorComponent(int component) => component is >= 0 and <= byte.MaxValue;

    /// <summary>
    /// The frame <paramref name="animationFrame"/> places after the pose's first, when it lies in the frame table and both
    /// of its definitions are installed; false for any other pair, which <see cref="Frame"/> rejects.
    /// </summary>
    public bool TryFrame(SamusPoseId pose, int animationFrame, out SamusBodyFrameSelection frame)
    {
        frame = default;
        int index = SamusPoseNumbering.HasRecord(pose) ? PoseFirstFrame(pose) + animationFrame : -1;
        if ((uint)index >= FrameCount)
            return false;
        frame = FrameAt(index);
        return HasDefinition(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition) &&
            (!frame.NamesBottom || HasDefinition(SamusBodyHalf.Bottom, frame.BottomSet, frame.BottomPosition));
    }

    /// <summary>Whether a native set and position resolve to an installed definition.</summary>
    private bool HasDefinition(SamusBodyHalf half, int set, int position) =>
        set >= 0 && set < (half == SamusBodyHalf.Top ? TopSetCount : BottomSetCount) && IsSelectorComponent(position) &&
        IsInstalledRecord(SetStart(half == SamusBodyHalf.Top, set) + position);

    /// <summary>Whether a numbered half picture lies in an installed set.</summary>
    private bool IsInstalledRecord(int record) => record >= firstRecord && record < SamusBodyDefinitionLayout.RecordCount;

    /// <summary>Returns the record selected by a native body set and position.</summary>
    /// <exception cref="InvalidDataException">The set and position name no installed half picture.</exception>
    public int DefinitionRecord(SamusBodyHalf half, int set, int position)
    {
        bool upperHalf = half == SamusBodyHalf.Top;
        if (set < 0 || set >= (upperHalf ? TopSetCount : BottomSetCount) || !IsSelectorComponent(position))
            throw new InvalidDataException(
                $"Samus {(upperHalf ? "top" : "bottom")} definition {set:X2}/{position:X2} is absent.");
        // The native selector steps position records past the chosen set's first without a
        // per-set bounds check. Several authored frames intentionally land in the
        // next definition group; resolve the physical record, not a per-set index.
        int record = SetStart(upperHalf, set) + position;
        if (!IsInstalledRecord(record))
            throw new InvalidDataException($"Samus body definition record {record} is absent from installed art.");
        return record;
    }

    /// <summary>Hashes all visual selectors, the body characters' identity and bundled Samus presentation catalogs.</summary>
    private string CreateContentIdentity() => SelectedPresentationHash.Create("samus-body-v3", content =>
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
}
