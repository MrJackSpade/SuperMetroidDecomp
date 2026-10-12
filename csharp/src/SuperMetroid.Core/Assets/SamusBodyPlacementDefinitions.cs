using SuperMetroid.Core.Game;
namespace SuperMetroid.Core.Assets;

/// <summary>Calculated pose-origin and selected-glyph support relationships for Samus landing, posture and drained transitions.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>
    /// $91:B62D + pose*8: the stock visual origin shares the native real-pose byte
    /// with the already calculated physical projectile correction. Installed visual
    /// overrides remain independent. FD..FF adjacent instruction observations are excluded.
    /// </summary>
    internal static sbyte DefaultGraphicsYOffset(SamusPoseId pose)
    {
        if ((int)pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return unchecked((sbyte)Game.SamusPoseProjectileOriginDefinitions.ReadYOffset(pose));
    }
    /// <summary>$90:8D28..8D37: normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>$90:8D28..8D37: sixteen true landing bytes; $90:8D38 PLB is the separate narrowly retained observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;
    /// <summary>$90:8D80..8D97: twelve transition poses with two bytes each, paired by facing.</summary>
    internal const int PosturePoseBytes = 2;
    /// <summary>$90:8D80..8D97: six right/left transition pairs, including two unused pairs.</summary>
    internal const int PostureDataBytes = 12 * PosturePoseBytes;
    /// <summary>The twelve $35-$40 posture poses whose rows $90:8D80..8D97 hold, in table order.</summary>
    private static readonly Game.SamusPoseId[] PostureBlockPoses =
    [
        Game.SamusPoseId.CrouchingTransitionRightPose, Game.SamusPoseId.CrouchingTransitionLeftPose,
        Game.SamusPoseId.MorphingTransitionRightPose, Game.SamusPoseId.MorphingTransitionLeftPose,
        Game.SamusPoseId.UnusedPose39, Game.SamusPoseId.UnusedPose3A,
        Game.SamusPoseId.StandingTransitionRightPose, Game.SamusPoseId.StandingTransitionLeftPose,
        Game.SamusPoseId.UnmorphingTransitionRightPose, Game.SamusPoseId.UnmorphingTransitionLeftPose,
        Game.SamusPoseId.UnusedPose3F, Game.SamusPoseId.UnusedPose40,
    ];

    /// <summary>Landing left-facing copies use their right-facing row; each source default follows the named landing/recovery phase.</summary>
    internal static int LandingSourceIndex(int index)
    {
        if ((uint)index > LandingDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index == LandingDataBytes ? index : index / (2 * LandingPoseBytes) * (2 * LandingPoseBytes) + index % LandingPoseBytes;
    }

    /// <summary>$90:8D38: exact PLB instruction encoding observed as the high byte of the final admitted unaligned word.</summary>
    /// <remarks>Only this one byte is retained as unrelated native instruction content; no memory beyond the17-byte window is modeled.</remarks>
    private const byte AdjacentLandingPlb = 0xAB;

    /// <summary>$90:8D28..8D38: named landing compression, standing recovery, inactive padding and adjacent instruction observation.</summary>
    internal static byte DefaultLandingByte(int index)
    {
        int source = LandingSourceIndex(index);
        if (source == LandingDataBytes) return AdjacentLandingPlb;
        bool spin = source >= 2 * LandingPoseBytes;
        int frame = source % LandingPoseBytes;
        int recoveryFrame = spin ? 2 : 1;
        if (frame > recoveryFrame) return 0;
        Game.SamusPoseId pose = frame == recoveryFrame ? Game.SamusPoseId.FacingRightNormalPose
            : spin ? Game.SamusPoseId.SpinLandingRightPose : Game.SamusPoseId.NormalLandingRightPose;
        return unchecked((byte)DefaultGraphicsYOffset(pose));
    }
    /// <summary>$90:8D8C: selected standing-transition join one pixel above its destination support; narrowly retained visual composition.</summary>
    private const int StandingSupportJoin = -1;
    /// <summary>$90:8D91: selected second-unmorph join one pixel below crouching support; narrowly retained visual composition.</summary>
    private const int UnmorphSupportJoin = 1;

    /// <summary>$91:B378: eight ordinary Morph Ball rolling frames before the loop command; both facing sequences share this domain.</summary>
    private const int RollingSupportFrames = 8;
    /// <summary>$90:8D80..8D97: align each selected transition's opaque support to its destination pose.</summary>
    /// <remarks>Unavailable or independently edited non-body geometry remains supplied content rather than narrowing the editable schema.</remarks>
    internal static bool TryDefaultPostureByte(SamusBodyArtworkCatalog art, int index, out sbyte value)
    {
        int source = PostureSourceIndex(index);
        Game.SamusPoseId pose = PostureBlockPoses[source / PosturePoseBytes];
        int frame = source % PosturePoseBytes;
        Game.SamusPoseId target;
        bool sourceBottom;
        int join = 0;
        switch (pose)
        {
            case Game.SamusPoseId.CrouchingTransitionRightPose:
                if (frame != 0) { value = 0; return true; }
                target = Game.SamusPoseId.CrouchingRightPose; sourceBottom = true;
                break;
            case Game.SamusPoseId.StandingTransitionRightPose:
                if (frame != 0) { value = 0; return true; }
                target = Game.SamusPoseId.FacingRightNormalPose; sourceBottom = true; join = StandingSupportJoin;
                break;
            case Game.SamusPoseId.MorphingTransitionRightPose:
                target = Game.SamusPoseId.MorphBallGroundRightPose; sourceBottom = false;
                break;
            case Game.SamusPoseId.UnmorphingTransitionRightPose:
                target = Game.SamusPoseId.CrouchingRightPose; sourceBottom = false;
                join = frame == 1 ? UnmorphSupportJoin : 0;
                break;
            // The rest of the $35-$40 posture block derives no support row.
            case Game.SamusPoseId.CrouchingTransitionLeftPose:
            case Game.SamusPoseId.MorphingTransitionLeftPose:
            case Game.SamusPoseId.UnusedPose39:
            case Game.SamusPoseId.UnusedPose3A:
            case Game.SamusPoseId.StandingTransitionLeftPose:
            case Game.SamusPoseId.UnmorphingTransitionLeftPose:
            case Game.SamusPoseId.UnusedPose3F:
            case Game.SamusPoseId.UnusedPose40:
                value = 0; return true;
            default:
                throw new InvalidOperationException($"{pose} is outside the posture source block.");
        }
        value = 0;
        try
        {
            bool targetBottom = target != Game.SamusPoseId.MorphBallGroundRightPose;
            if (!TryOpaqueBottom(art, pose, (ushort)frame, sourceBottom, out int sourceY) ||
                !TryOpaqueBottom(art, target, 0, targetBottom, out int targetY)) return false;
            if (!targetBottom)
            {
                // Rolling frames bob by one pixel in either direction. Align the
                // incoming morph to the support envelope, independent of facing's
                // initial rolling phase, rather than selecting a sampled target Y.
                for (ushort phase = 1; phase < RollingSupportFrames; phase++)
                {
                    if (!TryOpaqueBottom(art, target, phase, false, out int phaseBottom)) return false;
                    targetY = Math.Max(targetY, phaseBottom);
                }
            }
            int offset = targetY - art.GraphicsYOffset(target) - sourceY + join;
            if (offset is < sbyte.MinValue or > sbyte.MaxValue) return false;
            value = (sbyte)offset;
            return true;
        }
        catch (InvalidDataException)
        {
            // Edited frame identities may be admitted by the document yet unavailable
            // until selected for drawing. Preserve the supplied offset independently.
            return false;
        }
    }

    /// <summary>$90:8DEF/8DF0: narrowly retained common support of the two upper-half-only compact drained poses; selected scene composition.</summary>
    private const int DrainedCompactSupport = 17;
    /// <summary>$90:8DF6: narrowly retained impact support before the kneeling cycle; selected scene composition.</summary>
    private const int DrainedImpactSupport = 18;

    /// <summary>$90:8DEF..8E0E: shared drained placement, indexed by byte position in the complete $91:B268 left-facing program.</summary>
    internal static bool TryDefaultDrainedByte(SamusBodyArtworkCatalog art, int index, out sbyte value)
    {
        if ((uint)index >= Game.SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        value = 0;
        const SamusPoseId pose = Game.SamusPoseId.DrainedCrouchingLeftPose;
        ushort frame;
        switch (index)
        {
            // Repeated kneeling, tucked recovery, hit and Hyper Beam poses use
            // the drained pose's ordinary drawing origin. Command-only slots
            // remain zero even when a controller briefly publishes their index.
            case 8 or 9 or 10 or 11 or 14 or 19 or 23 or 26 or 29:
                int usual = -art.GraphicsYOffset(pose);
                if (usual > sbyte.MaxValue) return false;
                value = (sbyte)usual; return true;
            case 0 or 1: frame = (ushort)index; break;
            case >= 2 and <= 6: frame = 2; break;
            case 7: frame = 7; break;
            case 15 or 20 or 22: frame = 15; break;
            case 16 or 21: frame = 16; break;
            default: return true;
        }
        try
        {
            if (!TryOpaqueBottom(art, pose, frame, frame >= 2, out int sourceY)) return false;
            int targetY;
            if (frame < 2) targetY = DrainedCompactSupport;
            else if (frame == 7) targetY = DrainedImpactSupport;
            else if (frame == 16)
                // This final rise selects the collision boundary itself, while
                // falling/intermediate rise select standing's visible support.
                targetY = Game.SamusPoseCollisionDefinitions.ReadVerticalRadius(pose);
            else
            {
                const SamusPoseId standing = Game.SamusPoseId.FacingLeftNormalPose;
                if (!TryOpaqueBottom(art, standing, 0, true, out targetY)) return false;
                targetY -= art.GraphicsYOffset(standing);
            }
            int offset = targetY - sourceY;
            if (offset is < sbyte.MinValue or > sbyte.MaxValue) return false;
            value = (sbyte)offset;
            return true;
        }
        catch (InvalidDataException) { return false; }
    }

    private static bool TryOpaqueBottom(SamusBodyArtworkCatalog art, SamusPoseId pose, ushort frame,
        bool drawBottom, out int bottom)
    {
        int windowBytes = 2 * (Game.SamusRenderingRomData.TileTransfers.BottomDestinations.Second -
            Game.SamusRenderingRomData.TileTransfers.TopDestinations.First) + SamusBodyArtworkCatalog.BytesPerDefinitionSlot;
        Span<byte> pixels = stackalloc byte[windowBytes];
        Span<bool> defined = stackalloc bool[windowBytes];
        pixels.Clear(); defined.Clear();
        SamusBodyFrameSelection selected = art.Frame(pose, frame);
        Upload(art.GetDefinition(true, selected.TopSet, selected.TopPosition),
            Game.SamusRenderingRomData.TileTransfers.TopDestinations, pixels, defined);
        if (selected.BottomSet != Game.SamusRenderingRomData.TileTransfers.NoBottomTransferSet)
            Upload(art.GetDefinition(false, selected.BottomSet, selected.BottomPosition),
                Game.SamusRenderingRomData.TileTransfers.BottomDestinations, pixels, defined);
        bottom = int.MinValue;
        for (int half = 0; half < (drawBottom ? 2 : 1); half++)
        {
            int baseIndex = half == 0 ? art.Spritemaps.TopBase(pose) : art.Spritemaps.BottomBase(pose);
            int index = baseIndex + frame;
            if ((uint)index >= SamusSpritemapArtworkCatalog.PointerCount ||
                !art.Spritemaps.TryGet((ushort)index, out SamusSpritemapDefinition? map)) return false;
            foreach (SamusSpritePart part in map!.Parts)
            {
                int size = (part.X & 0x8000) != 0 ? 16 : 8;
                var attributes = new Hardware.SnesObjAttributeWord(part.Attributes);
                if (attributes.TileNumber >= 256) return false;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int sx = attributes.FlipHorizontally ? size - 1 - x : x;
                    int sy = attributes.FlipVertically ? size - 1 - y : y;
                    int tile = (attributes.TileNumber + (sy / 8) * 16 + sx / 8) & 255;
                    int row = tile * 32 + (sy % 8) * 2;
                    int mask = 128 >> (sx % 8);
                    bool opaque = false;
                    for (int plane = 0; plane < 4; plane++)
                    {
                        int at = row + plane % 2 + plane / 2 * 16;
                        if ((uint)at >= pixels.Length || !defined[at]) return false;
                        opaque |= (pixels[at] & mask) != 0;
                    }
                    if (opaque) bottom = Math.Max(bottom, unchecked((sbyte)part.Y) + y);
                }
            }
        }
        return bottom != int.MinValue;

        static void Upload(SamusBodyTileDefinition definition,
            Game.SamusRenderingRomData.TileTransfers.SplitVramDestinations destination,
            Span<byte> data, Span<bool> coverage)
        {
            ReadOnlySpan<byte> source = definition.Planar.Span;
            int firstByte = Game.SamusRenderingRomData.TileTransfers.TopDestinations.First * 2;
            int first = destination.First * 2 - firstByte, second = destination.Second * 2 - firstByte;
            source[..definition.FirstSize].CopyTo(data[first..]);
            coverage.Slice(first, definition.FirstSize).Fill(true);
            source[definition.FirstSize..].CopyTo(data[second..]);
            coverage.Slice(second, definition.SecondSize).Fill(true);
        }
    }
    /// <summary>Transition left-facing copies use their right-facing row; source defaults derive from selected support geometry.</summary>
    internal static int PostureSourceIndex(int index)
    {
        if ((uint)index >= PostureDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index / (2 * PosturePoseBytes) * (2 * PosturePoseBytes) + index % PosturePoseBytes;
    }
}
