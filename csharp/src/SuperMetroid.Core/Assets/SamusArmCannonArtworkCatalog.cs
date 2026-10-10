using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed arm-cannon cover placement, OAM attributes, and indexed 8×8 tiles.</summary>
public sealed class SamusArmCannonArtworkCatalog
{
    private readonly Dictionary<int, ushort> posePointers = new();
    private readonly Dictionary<int, byte> drawingData = new();
    private readonly Dictionary<int, ushort> attributes = new();
    private readonly Dictionary<int, ushort> tileSources = new();
    private readonly RoomCharacterAtlas tiles;
    private readonly SamusBodyArtworkCatalog? bodyGeometry;

    private SamusArmCannonArtworkCatalog(ushort[] posePointers, byte[] drawingData,
        ushort[] attributes, ushort[][] tileSources, RoomCharacterAtlas tiles,
        SamusBodyArtworkCatalog? bodyGeometry = null)
    {
        this.bodyGeometry = bodyGeometry;
        for (int pose = 0; pose < posePointers.Length; pose++)
            if (posePointers[pose] != SamusArmCannonArtworkFormat.StockPoseDrawingData(pose))
                this.posePointers.Add(pose, posePointers[pose]);
        for (int index = 0; index < drawingData.Length; index++)
        {
            ushort address = (ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index);
            bool derived = SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte calculated);
            if (!derived && SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source))
            {
                calculated = drawingData[source - SamusArmCannonArtworkFormat.DrawingDataStart];
                derived = true;
            }
            if (!derived && SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out source))
            {
                calculated = SamusArmCannonArtworkFormat.ReflectCoverX(drawingData[source - SamusArmCannonArtworkFormat.DrawingDataStart]);
                derived = true;
            }
            if (!derived && bodyGeometry is not null)
                derived = SamusArmCannonPlacementDefinitions.TryCoordinate(bodyGeometry, address, out calculated);
            if (!derived || drawingData[index] != calculated) this.drawingData.Add(index, drawingData[index]);
        }
        for (int direction = 0; direction < attributes.Length; direction++)
        {
            if (attributes[direction] != SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction))
                this.attributes.Add(direction, attributes[direction]);
            for (int frame = 0; frame < SamusArmCannonArtworkFormat.FramesPerDirection; frame++)
                if (tileSources[direction][frame] != SamusArmCannonArtworkFormat.StockTileSource((SamusProjectileDirection)direction, frame))
                    this.tileSources.Add(direction * SamusArmCannonArtworkFormat.FramesPerDirection + frame, tileSources[direction][frame]);
        }
        this.tiles = tiles;
    }

    internal SamusArmCannonArtworkCatalog WithBodyGeometry(SamusBodyArtworkCatalog body) => new(
        Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount).Select(PoseDrawingData).ToArray(),
        Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => ReadDrawingByte((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index))).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(SpriteAttributes).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(direction =>
            Enumerable.Range(0, SamusArmCannonArtworkFormat.FramesPerDirection).Select(frame => TileSource(direction, frame)).ToArray()).ToArray(),
        tiles, body);

    /// <summary>SHA-256 of selected cannon placement, OBJ attributes, tile selectors and characters.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), content =>
    {
        Span<ushort> selectedPoses = stackalloc ushort[SamusBodyArtworkCatalog.PoseCount];
        for (int pose = 0; pose < selectedPoses.Length; pose++) selectedPoses[pose] = PoseDrawingData(pose);
        content.AppendWords("pose pointers", selectedPoses);
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index));
        content.Append("drawing data", selectedDrawing);
        Span<ushort> selectedAttributes = stackalloc ushort[SamusRenderingRomData.ArmCannon.DirectionCount];
        for (int direction = 0; direction < selectedAttributes.Length; direction++) selectedAttributes[direction] = SpriteAttributes(direction);
        content.AppendWords("attributes", selectedAttributes);
        Span<ushort> selectedSources = stackalloc ushort[SamusArmCannonArtworkFormat.FramesPerDirection];
        for (int direction = 0; direction < SamusRenderingRomData.ArmCannon.DirectionCount; direction++)
        {
            for (int frame = 0; frame < selectedSources.Length; frame++) selectedSources[frame] = TileSource(direction, frame);
            content.AppendWords("tile sources", selectedSources);
        }
        content.Append("characters", this.tiles.Transfer.Span);
    });

    // Placement and PNG admission are distinct file boundaries. The installer can
    // identify the failing file without attributing JSON errors to the tile sheet.
    internal sealed record Placement(ushort[] PosePointers, byte[] DrawingData,
        ushort[] Attributes, ushort[][] TileSources);

    internal static SamusArmCannonArtworkCatalog FromPlacement(Placement placement, RoomCharacterAtlas tiles) =>
        new(placement.PosePointers, placement.DrawingData, placement.Attributes, placement.TileSources, tiles);

    internal static Placement LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusArmCannonArtworkDocument>(Options)
                ?? throw new InvalidDataException("Arm-cannon artwork JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid arm-cannon artwork JSON.", error);
        }
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.PosePointers is null ||
            document.PosePointers.Length != SamusBodyArtworkCatalog.PoseCount ||
            document.DrawingData is null ||
            document.DrawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount ||
            document.SpriteAttributes is null ||
            document.SpriteAttributes.Length != SamusRenderingRomData.ArmCannon.DirectionCount ||
            document.TileSources is null ||
            document.TileSources.Length != SamusRenderingRomData.ArmCannon.DirectionCount)
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        ushort[] pointers = document.PosePointers.Select((value, pose) =>
            RequireWord(value, $"pose {pose} pointer")).ToArray();
        foreach (ushort pointer in pointers)
            if (pointer < SamusArmCannonArtworkFormat.DrawingDataStart ||
                pointer + 2 >= SamusArmCannonArtworkFormat.DrawingDataEndExclusive)
                throw new InvalidDataException(
                    $"Arm-cannon pose points outside drawing data: ${pointer:X4}.");
        byte[] data = document.DrawingData.Select((value, index) =>
            RequireByte(value, $"drawing byte {index}")).ToArray();
        ushort[] attributes = document.SpriteAttributes.Select((value, index) =>
            RequireWord(value, $"attribute {index}")).ToArray();
        ushort[][] sources = new ushort[document.TileSources.Length][];
        for (int direction = 0; direction < sources.Length; direction++)
        {
            int[]? native = document.TileSources[direction];
            if (native is null || native.Length != SamusArmCannonArtworkFormat.FramesPerDirection)
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} needs four tile-source slots.");
            sources[direction] = native.Select((value, frame) =>
                RequireWord(value, $"tile source {direction}/{frame}")).ToArray();
            if (sources[direction][0] != 0 ||
                sources[direction].Skip(1).Any(source =>
                    !SamusArmCannonArtworkFormat.TileSourcePointers.Contains(source)))
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} selects a missing tile.");
        }
        return new Placement(pointers, data, attributes, sources);
    }

    /// <summary>Serializes the placement document as indented camel-case UTF-8 JSON for <c>samus-arm-cannon.json</c>; table bounds and selector validation occur when placement is loaded, not here.</summary>
    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    /// <summary>Gets the bank-$90 drawing-descriptor pointer for pose 0–252, using an independent supplied edit or the stock $90:C7DF pose-table mapping.</summary>
    public ushort PoseDrawingData(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return posePointers.TryGetValue(pose, out ushort selected) ? selected
            : SamusArmCannonArtworkFormat.StockPoseDrawingData(pose);
    }

    /// <summary>Resolves one installed descriptor byte at bank-$90 address $C9D9–$CC38, preferring edits over stock selector/mode bytes and derived body-relative or reflected coordinate bytes.</summary>
    /// <param name="address">Bank-relative descriptor byte address; coordinate bytes are interpreted as signed pixel offsets by the renderer.</param>
    public byte ReadDrawingByte(ushort address)
    {
        int index = address - SamusArmCannonArtworkFormat.DrawingDataStart;
        if ((uint)index >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte $90:{address:X4} is not installed.");
        if (drawingData.TryGetValue(index, out byte supplied)) return supplied;
        if (SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte calculated)) return calculated;
        if (SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source)) return ReadDrawingByte(source);
        if (SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out source))
            return SamusArmCannonArtworkFormat.ReflectCoverX(ReadDrawingByte(source));
        if (bodyGeometry is not null && SamusArmCannonPlacementDefinitions.TryCoordinate(bodyGeometry, address, out byte coordinate))
            return coordinate;
        throw new InvalidDataException("Arm-cannon coordinate basis is incomplete.");
    }

    /// <summary>Gets the packed small-OBJ character, palette, priority, and flip attributes for native direction 0–9; supplied words override the stock $90:C791 table.</summary>
    public ushort SpriteAttributes(int direction)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount) throw new IndexOutOfRangeException();
        return attributes.TryGetValue(direction, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction);
    }

    /// <summary>Gets the bank-$9A tile-source identity for native direction 0–9 and cover frame 0–3; frame 0 is the closed no-transfer sentinel, while frames 1–3 select installed opening artwork.</summary>
    public ushort TileSource(int direction, int frame)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount ||
            (uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection) throw new IndexOutOfRangeException();
        return tileSources.TryGetValue(direction * SamusArmCannonArtworkFormat.FramesPerDirection + frame, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockTileSource((SamusProjectileDirection)direction, frame);
    }

    /// <summary>Resolves the queued 32-byte VRAM DMA from the editable indexed PNG.</summary>
    public bool TryResolveTile(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (byteCount == SamusRenderingRomData.ArmCannon.TileUploadByteCount)
        {
            int source = sourceAddress & 0xffff;
            if ((sourceAddress & 0xff0000) == SamusRenderingRomData.Banks.CharacterData &&
                SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(
                    unchecked((ushort)source)) is int index and >= 0)
            {
                data = tiles.Transfer.Slice(index * byteCount, byteCount);
                return true;
            }
        }
        data = default;
        return false;
    }

    private static byte RequireByte(int value, string name) => (uint)value <= byte.MaxValue
        ? (byte)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a byte.");

    private static ushort RequireWord(int value, string name) => (uint)value <= ushort.MaxValue
        ? (ushort)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a word.");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Versioned <c>samus-arm-cannon.json</c> placement, OBJ-attribute, and tile-selection tables; pixels are supplied separately by <c>samus-arm-cannon-tiles.png</c>, and opening/closing mechanics remain runtime-owned.</summary>
public sealed record SamusArmCannonArtworkDocument
{
    /// <summary>Placement schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 253 unsigned 16-bit bank-$90 descriptor pointers in pose order; loading requires room for a descriptor within $C9D9–$CC38.</summary>
    public required int[] PosePointers { get; init; }
    /// <summary>Exactly 608 byte values for $90:C9D9–$CC38: direction and drawing-mode bytes, optional alternate direction/mode, then signed X/Y pixel-offset pairs indexed by body animation frame.</summary>
    public required int[] DrawingData { get; init; }
    /// <summary>Ten unsigned 16-bit OAM attribute words in native direction order, retaining character index, OBJ palette, priority, and mirror bits.</summary>
    public required int[] SpriteAttributes { get; init; }
    /// <summary>Ten direction rows of four unsigned 16-bit bank-$9A source identities; each row begins with zero for the closed cover, followed by three entries from the installed twelve-tile source set.</summary>
    public required int[][] TileSources { get; init; }
}

/// <summary>Bounded retail arm-cannon visual geometry, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{
    /// <summary>Bank-$90 arm-cannon drawing descriptors selected by Samus pose.</summary>
    private enum ArmCannonDrawing : ushort
    {
        /// <summary>$90:C9DB, ArmCannonDrawingData_FacingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingForward = 0xc9db,
        /// <summary>$90:C9DD, ArmCannonDrawingData_FacingRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRight = 0xc9dd,
        /// <summary>$90:C9F1, ArmCannonDrawingData_FacingLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeft = 0xc9f1,
        /// <summary>$90:CA05, ArmCannonDrawingData_FacingRight_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightAimingUp = 0xca05,
        /// <summary>$90:CA0D, ArmCannonDrawingData_FacingLeft_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftAimingUp = 0xca0d,
        /// <summary>$90:CA15, ArmCannonDrawingData_FacingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightAimingUpRight = 0xca15,
        /// <summary>$90:CA19, ArmCannonDrawingData_FacingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftAimingUpLeft = 0xca19,
        /// <summary>$90:CA1D, ArmCannonDrawingData_FacingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightAimingDownRight = 0xca1d,
        /// <summary>$90:CA21, ArmCannonDrawingData_FacingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftAimingDownLeft = 0xca21,
        /// <summary>$90:C9D9, ArmCannonDrawingData_Default: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        Default = 0xc9d9,
        /// <summary>$90:CA25, ArmCannonDrawingData_MovingRight_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingRightGunExtended = 0xca25,
        /// <summary>$90:CA3B, ArmCannonDrawingData_MovingLeft_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingLeftGunExtended = 0xca3b,
        /// <summary>$90:CA51, ArmCannonDrawingData_MovingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingRightAimingUpRight = 0xca51,
        /// <summary>$90:CA67, ArmCannonDrawingData_MovingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingLeftAimingUpLeft = 0xca67,
        /// <summary>$90:CA7D, ArmCannonDrawingData_MovingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingRightAimingDownRight = 0xca7d,
        /// <summary>$90:CA93, ArmCannonDrawingData_MovingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        MovingLeftAimingDownLeft = 0xca93,
        /// <summary>$90:CAA9, ArmCannonDrawingData_FacingRight_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpNotMovingGunExt = 0xcaa9,
        /// <summary>$90:CAAF, ArmCannonDrawingData_FacingLeft_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpNotMovingGunExt = 0xcaaf,
        /// <summary>$90:CAB5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpAimingUp = 0xcab5,
        /// <summary>$90:CABD, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpAimingUp = 0xcabd,
        /// <summary>$90:CAC5, ArmCannonDrawingData_FacingRight_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpAimingDown = 0xcac5,
        /// <summary>$90:CACB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpAimingDown = 0xcacb,
        /// <summary>$90:CB5D, ArmCannonDrawingData_FacingRight_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightCrouching = 0xcb5d,
        /// <summary>$90:CB71, ArmCannonDrawingData_FacingLeft_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftCrouching = 0xcb71,
        /// <summary>$90:CB1D, ArmCannonDrawingData_FacingRight_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightFallingAimingUp = 0xcb1d,
        /// <summary>$90:CB27, ArmCannonDrawingData_FacingLeft_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftFallingAimingUp = 0xcb27,
        /// <summary>$90:CB31, ArmCannonDrawingData_FacingRight_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightFallingAimingDown = 0xcb31,
        /// <summary>$90:CB37, ArmCannonDrawingData_FacingLeft_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftFallingAimingDown = 0xcb37,
        /// <summary>$90:CBA5, ArmCannonDrawingData_FacingLeft_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftMoonwalk = 0xcba5,
        /// <summary>$90:CBB3, ArmCannonDrawingData_FacingRight_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightMoonwalk = 0xcbb3,
        /// <summary>$90:CAD1, ArmCannonDrawingData_FacingRight_NormalJumpTransition: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpTransition = 0xcad1,
        /// <summary>$90:CAD9, ArmCannonDrawingData_FacingRight_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpMovingForward = 0xcad9,
        /// <summary>$90:CADF, ArmCannonDrawingData_FacingLeft_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpMovingForward = 0xcadf,
        /// <summary>$90:CC15, ArmCannonDrawingData_FacingRight_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightTransitionAimingUp = 0xcc15,
        /// <summary>$90:CC1B, ArmCannonDrawingData_FacingLeft_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftTransitionAimingUp = 0xcc1b,
        /// <summary>$90:CAFD, ArmCannonDrawingData_FacingRight_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightFallingGunExtended = 0xcafd,
        /// <summary>$90:CB0D, ArmCannonDrawingData_FacingLeft_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftFallingGunExtended = 0xcb0d,
        /// <summary>$90:CAE5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpAimingUpRight = 0xcae5,
        /// <summary>$90:CAEB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpAimingUpLeft = 0xcaeb,
        /// <summary>$90:CAF1, ArmCannonDrawingData_FacingRight_NormalJump_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightNormalJumpAimingDownRight = 0xcaf1,
        /// <summary>$90:CAF7, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftNormalJumpAimingDownLeft = 0xcaf7,
        /// <summary>$90:CB3D, ArmCannonDrawingData_FacingRight_Falling_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightFallingAimingUpRight = 0xcb3d,
        /// <summary>$90:CB45, ArmCannonDrawingData_FacingLeft_Falling_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftFallingAimingUpLeft = 0xcb45,
        /// <summary>$90:CB4D, ArmCannonDrawingData_FacingRight_Falling_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightFallingAimingDownRight = 0xcb4d,
        /// <summary>$90:CB55, ArmCannonDrawingData_FacingLeft_Falling_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftFallingAimingDownLeft = 0xcb55,
        /// <summary>$90:CB85, ArmCannonDrawingData_FacingRight_Crouching_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightCrouchingAimingUpRight = 0xcb85,
        /// <summary>$90:CB89, ArmCannonDrawingData_FacingLeft_Crouching_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftCrouchingAimingUpLeft = 0xcb89,
        /// <summary>$90:CB8D, ArmCannonDrawingData_FacingRight_Crouching_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightCrouchingAimingDownRight = 0xcb8d,
        /// <summary>$90:CB91, ArmCannonDrawingData_FacingLeft_Crouching_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftCrouchingAimingDownLeft = 0xcb91,
        /// <summary>$90:CBC1, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftMoonwalkAimingUpLeft = 0xcbc1,
        /// <summary>$90:CBCF, ArmCannonDrawingData_FacingRight_Moonwalk_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightMoonwalkAimingUpRight = 0xcbcf,
        /// <summary>$90:CBDD, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftMoonwalkAimingDownLeft = 0xcbdd,
        /// <summary>$90:CBEB, ArmCannonDrawingData_FacingRight_Moonwalk_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightMoonwalkAimingDownRight = 0xcbeb,
        /// <summary>$90:CB95, ArmCannonDrawingData_FacingRight_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightCrouchingAimingUp = 0xcb95,
        /// <summary>$90:CB9D, ArmCannonDrawingData_FacingLeft_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingLeftCrouchingAimingUp = 0xcb9d,
        /// <summary>$90:CBF9, ArmCannonDrawingData_FacingRight_LandingFromNormalJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightLandingFromNormalJump = 0xcbf9,
        /// <summary>$90:CC05, ArmCannonDrawingData_FacingRight_LandingFromSpinJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        FacingRightLandingFromSpinJump = 0xcc05,
    }

    /// <summary>Supported placement JSON schema revision, checked with the fixed pose, descriptor, direction, and frame table dimensions.</summary>
    public const int Version = 1;
    /// <summary>Installed editable placement, OBJ-attribute, and tile-selector JSON filename, separate from cover pixels and animation mechanics.</summary>
    public const string JsonFileName = "samus-arm-cannon.json";
    /// <summary>Installed indexed PNG filename holding the twelve 8-by-8 cover characters in native source-identity order.</summary>
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>First pose descriptor at $90:C9D9.</summary>
    public const ushort DrawingDataStart = 0xc9d9;
    /// <summary>Descriptor bytes end immediately before the $90:CC39 code entry.</summary>
    public const ushort DrawingDataEndExclusive = 0xcc39;
    /// <summary>608 installed bytes spanning the bank-$90 drawing descriptors from $C9D9 through $CC38.</summary>
    public const int DrawingDataByteCount = DrawingDataEndExclusive - DrawingDataStart;
    /// <summary>Four tile-selector slots per native aim direction: closed sentinel 0 and three successive nonclosed cover-art frames.</summary>
    public const int FramesPerDirection = 4;
    /// <summary>$90:C7DF-$C9D8, ArmCannonDrawingData: named descriptor selection for the existing 253-pose installed domain.</summary>
    internal static ushort StockPoseDrawingData(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return (SamusPoseId)pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => (ushort)ArmCannonDrawing.FacingForward,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.UnusedPose47 or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.FiringLandingRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose => (ushort)ArmCannonDrawing.FacingRight,
            SamusPoseId.FacingLeftNormalPose or
            SamusPoseId.UnusedPose48 or
            SamusPoseId.RanIntoWallLeftPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.DraygonGrabbedFiringLeftPose or
            SamusPoseId.FiringLandingLeftPose => (ushort)ArmCannonDrawing.FacingLeft,
            SamusPoseId.StandingAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightAimingUp,
            SamusPoseId.StandingAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftAimingUp,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.LandingAimDiagonalUpRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => (ushort)ArmCannonDrawing.FacingRightAimingUpRight,
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.DraygonGrabbedAimUpLeftPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.LandingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftAimingUpLeft,
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.LandingAimDiagonalDownRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => (ushort)ArmCannonDrawing.FacingRightAimingDownRight,
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
            SamusPoseId.GrappleStandingDownLeftPose or
            SamusPoseId.DraygonGrabbedAimDownLeftPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.LandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => (ushort)ArmCannonDrawing.FacingLeftAimingDownLeft,
            SamusPoseId.MovingRightNormalPose or
            SamusPoseId.MovingLeftNormalPose or
            SamusPoseId.RunningAimUpRightPose or
            SamusPoseId.RunningAimUpLeftPose or
            SamusPoseId.SpinJumpRightPose or
            SamusPoseId.SpinJumpLeftPose or
            SamusPoseId.SpaceJumpRightPose or
            SamusPoseId.SpaceJumpLeftPose or
            SamusPoseId.MorphBallGroundRightPose or
            SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or
            SamusPoseId.UnusedPose20 or
            SamusPoseId.UnusedPose21 or
            SamusPoseId.UnusedPose22 or
            SamusPoseId.UnusedPose23 or
            SamusPoseId.UnusedPose24 or
            SamusPoseId.TurningRightToLeftPose or
            SamusPoseId.TurningLeftToRightPose or
            SamusPoseId.FallingRightPose or
            SamusPoseId.FallingLeftPose or
            SamusPoseId.TurningRightToLeftJumpPose or
            SamusPoseId.TurningLeftToRightJumpPose or
            SamusPoseId.MorphBallFallingRightPose or
            SamusPoseId.MorphBallFallingLeftPose or
            SamusPoseId.UnusedKnockbackRightPose or
            SamusPoseId.UnusedKnockbackLeftPose or
            SamusPoseId.CrouchingTransitionRightPose or
            SamusPoseId.CrouchingTransitionLeftPose or
            SamusPoseId.MorphingTransitionRightPose or
            SamusPoseId.MorphingTransitionLeftPose or
            SamusPoseId.UnusedPose39 or
            SamusPoseId.UnusedPose3A or
            SamusPoseId.StandingTransitionRightPose or
            SamusPoseId.StandingTransitionLeftPose or
            SamusPoseId.UnmorphingTransitionRightPose or
            SamusPoseId.UnmorphingTransitionLeftPose or
            SamusPoseId.UnusedPose3F or
            SamusPoseId.UnusedPose40 or
            SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.UnusedPose42 or
            SamusPoseId.TurningRightToLeftCrouchingPose or
            SamusPoseId.TurningLeftToRightCrouchingPose or
            SamusPoseId.UnusedPose45 or
            SamusPoseId.UnusedPose46 or
            SamusPoseId.NeutralJumpTransitionLeftPose or
            SamusPoseId.NeutralJumpRightPose or
            SamusPoseId.NeutralJumpLeftPose or
            SamusPoseId.DamageBoostLeftPose or
            SamusPoseId.DamageBoostRightPose or
            SamusPoseId.KnockbackRightPose or
            SamusPoseId.KnockbackLeftPose or
            SamusPoseId.UnusedPose5B or
            SamusPoseId.UnusedPose5C or
            SamusPoseId.UnusedPose5D or
            SamusPoseId.UnusedPose5E or
            SamusPoseId.UnusedPose5F or
            SamusPoseId.UnusedPose60 or
            SamusPoseId.UnusedPose61 or
            SamusPoseId.UnusedPose62 or
            SamusPoseId.UnusedPose63 or
            SamusPoseId.UnusedPose64 or
            SamusPoseId.UnusedPose65 or
            SamusPoseId.UnusedPose66 or
            SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or
            SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose or
            SamusPoseId.SpringBallFallingRightPose or
            SamusPoseId.SpringBallFallingLeftPose or
            SamusPoseId.SpringBallJumpRightPose or
            SamusPoseId.SpringBallJumpLeftPose or
            SamusPoseId.ScrewAttackRightPose or
            SamusPoseId.ScrewAttackLeftPose or
            SamusPoseId.WallJumpRightPose or
            SamusPoseId.WallJumpLeftPose or
            SamusPoseId.TurningRightToLeftFallingPose or
            SamusPoseId.TurningLeftToRightFallingPose or
            SamusPoseId.TurningRightToLeftAimUpPose or
            SamusPoseId.TurningLeftToRightAimUpPose or
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
            SamusPoseId.TurningRightToLeftJumpAimUpPose or
            SamusPoseId.TurningLeftToRightJumpAimUpPose or
            SamusPoseId.TurningRightToLeftJumpAimDownPose or
            SamusPoseId.TurningLeftToRightJumpAimDownPose or
            SamusPoseId.TurningRightToLeftFallingAimUpPose or
            SamusPoseId.TurningLeftToRightFallingAimUpPose or
            SamusPoseId.TurningRightToLeftFallingAimDownPose or
            SamusPoseId.TurningLeftToRightFallingAimDownPose or
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
            SamusPoseId.NormalLandingLeftPose or
            SamusPoseId.SpinLandingLeftPose or
            SamusPoseId.GrappleSwingRightPose or
            SamusPoseId.GrappleSwingLeftPose or
            SamusPoseId.GrappleWallContactLeftPose or
            SamusPoseId.GrappleWallContactRightPose or
            SamusPoseId.DraygonGrabbedNeutralLeftPose or
            SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.MoonwalkTurnJumpLeftPose or
            SamusPoseId.MoonwalkTurnJumpRightPose or
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
            SamusPoseId.UnusedPoseC5 or
            SamusPoseId.UnusedPoseC6 or
            SamusPoseId.ShinesparkWindupRightPose or
            SamusPoseId.ShinesparkWindupLeftPose or
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose or
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose or
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose or
            SamusPoseId.CrystalFlashRightPose or
            SamusPoseId.CrystalFlashLeftPose or
            SamusPoseId.XrayingStandingRightPose or
            SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.DeathSequenceRightPose or
            SamusPoseId.DeathSequenceLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or
            SamusPoseId.XrayingCrouchingLeftPose or
            SamusPoseId.UnusedPoseDb or
            SamusPoseId.UnusedPoseDc or
            SamusPoseId.UnusedPoseDd or
            SamusPoseId.UnusedPoseDe or
            SamusPoseId.UnusedPoseDf or
            SamusPoseId.DrainedCrouchingRightPose or
            SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or
            SamusPoseId.DrainedStandingLeftPose or
            SamusPoseId.DraygonGrabbedNeutralRightPose or
            SamusPoseId.DraygonGrabbedMovingRightPose => (ushort)ArmCannonDrawing.Default,
            SamusPoseId.MovingRightGunExtendedPose => (ushort)ArmCannonDrawing.MovingRightGunExtended,
            SamusPoseId.MovingLeftGunExtendedPose => (ushort)ArmCannonDrawing.MovingLeftGunExtended,
            SamusPoseId.RunningAimDiagonalUpRightPose => (ushort)ArmCannonDrawing.MovingRightAimingUpRight,
            SamusPoseId.RunningAimDiagonalUpLeftPose => (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft,
            SamusPoseId.RunningAimDiagonalDownRightPose => (ushort)ArmCannonDrawing.MovingRightAimingDownRight,
            SamusPoseId.RunningAimDiagonalDownLeftPose => (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.UnusedPoseAC => (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.UnusedPoseAD => (ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.UnusedPoseAE => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDown,
            SamusPoseId.NormalJumpAimDownLeftPose or
            SamusPoseId.UnusedPoseAF => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDown,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.GrappleCrouchingRightPose => (ushort)ArmCannonDrawing.FacingRightCrouching,
            SamusPoseId.CrouchingLeftPose or
            SamusPoseId.GrappleCrouchingLeftPose => (ushort)ArmCannonDrawing.FacingLeftCrouching,
            SamusPoseId.FallingAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightFallingAimingUp,
            SamusPoseId.FallingAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftFallingAimingUp,
            SamusPoseId.FallingAimDownRightPose => (ushort)ArmCannonDrawing.FacingRightFallingAimingDown,
            SamusPoseId.FallingAimDownLeftPose => (ushort)ArmCannonDrawing.FacingLeftFallingAimingDown,
            SamusPoseId.MoonwalkFacingLeftPose => (ushort)ArmCannonDrawing.FacingLeftMoonwalk,
            SamusPoseId.MoonwalkFacingRightPose => (ushort)ArmCannonDrawing.FacingRightMoonwalk,
            SamusPoseId.NeutralJumpTransitionRightPose => (ushort)ArmCannonDrawing.FacingRightNormalJumpTransition,
            SamusPoseId.NormalJumpForwardRightPose => (ushort)ArmCannonDrawing.FacingRightNormalJumpMovingForward,
            SamusPoseId.NormalJumpForwardLeftPose => (ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward,
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.LandingAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimUpRightPose or
            SamusPoseId.StandingTransitionAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightTransitionAimingUp,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.LandingAimUpLeftPose or
            SamusPoseId.CrouchingTransitionAimUpLeftPose or
            SamusPoseId.StandingTransitionAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp,
            SamusPoseId.FallingGunExtendedRightPose => (ushort)ArmCannonDrawing.FacingRightFallingGunExtended,
            SamusPoseId.FallingGunExtendedLeftPose => (ushort)ArmCannonDrawing.FacingLeftFallingGunExtended,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft,
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.UnusedPoseB0 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight,
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
            SamusPoseId.UnusedPoseB1 => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft,
            SamusPoseId.FallingAimDiagonalUpRightPose => (ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight,
            SamusPoseId.FallingAimDiagonalUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft,
            SamusPoseId.FallingAimDiagonalDownRightPose => (ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight,
            SamusPoseId.FallingAimDiagonalDownLeftPose => (ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft,
            SamusPoseId.CrouchingAimDiagonalUpRightPose => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUpRight,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft,
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.GrappleCrouchingDownRightPose => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingDownRight,
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.GrappleCrouchingDownLeftPose => (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft,
            SamusPoseId.MoonwalkAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft,
            SamusPoseId.MoonwalkAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight,
            SamusPoseId.MoonwalkAimDownLeftPose => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft,
            SamusPoseId.MoonwalkAimDownRightPose => (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight,
            SamusPoseId.CrouchingAimUpRightPose => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUp,
            SamusPoseId.CrouchingAimUpLeftPose => (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUp,
            SamusPoseId.NormalLandingRightPose => (ushort)ArmCannonDrawing.FacingRightLandingFromNormalJump,
            SamusPoseId.SpinLandingRightPose => (ushort)ArmCannonDrawing.FacingRightLandingFromSpinJump,
            _ => throw new ArgumentOutOfRangeException(nameof(pose)),
        };
    }

    /// <summary>$90:C9D9, ArmCannonDrawingData_Default: no cover is drawn.</summary>
    private const byte HiddenDrawingMode = 0;
    /// <summary>$90:C9DE and other visible descriptor mode bytes: draw the cover normally.</summary>
    private const byte NormalDrawingMode = 1;
    /// <summary>$90:C9DC, ArmCannonDrawingData_FacingForward: forward-facing cover mode.</summary>
    private const byte ForwardDrawingMode = 2;
    /// <summary>$90:CA05 and vertical-aim descriptors: initial diagonal selector changes to the next direction word after frame zero.</summary>
    private const byte FrameDependentDirectionFlag = 0x80;
    /// <summary>$90:CC21, CostOfSBAsInPowerBombs: the final24 installed drawing-window bytes alias this independent mechanics owner.</summary>
    private const int AdjacentCostStart = SamusComboRomData.Costs & 0xffff;

    /// <summary>The $90:C9D9-$CC20 pose descriptors, ordered by native address.</summary>
    private static readonly ArmCannonDrawing[] DrawingsByAddress = Enum.GetValues<ArmCannonDrawing>();

    /// <summary>Finds the descriptor whose bytes contain <paramref name="address"/>.</summary>
    private static bool TryLocateDrawing(ushort address, out ArmCannonDrawing drawing, out int offset)
    {
        for (int index = DrawingsByAddress.Length - 1; index >= 0; index--)
        {
            if ((ushort)DrawingsByAddress[index] <= address)
            {
                drawing = DrawingsByAddress[index];
                offset = address - (ushort)drawing;
                return true;
            }
        }
        drawing = default;
        offset = 0;
        return false;
    }

    /// <summary>Calculates named descriptor controls and adjacent cost aliases; coordinate geometry is resolved separately from body artwork.</summary>
    internal static bool TryStockDrawingByte(ushort address, out byte value)
    {
        if (address is >= AdjacentCostStart and < DrawingDataEndExclusive)
        {
            int costOffset = address - AdjacentCostStart;
            ushort cost = SamusComboMechanicsDefinitions.GetPowerBombCost(
                SamusBeamCombinations.FromTableIndex(costOffset / sizeof(ushort)));
            value = (byte)(cost >> ((costOffset & 1) * 8));
            return true;
        }
        if (!TryLocateDrawing(address, out ArmCannonDrawing drawing, out int byteOffset))
        {
            value = 0;
            return false;
        }
        // Control bytes by descriptor and byte position; coordinate bytes are resolved separately.
        byte? control = (drawing, byteOffset) switch
        {
            (ArmCannonDrawing.FacingForward, 0) or
            (ArmCannonDrawing.Default, 0) => 0,
            (ArmCannonDrawing.FacingForward, 1) => ForwardDrawingMode,
            (ArmCannonDrawing.FacingRight, 0) or
            (ArmCannonDrawing.MovingRightGunExtended, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDrawing.FacingRightCrouching, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalk, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpMovingForward, 0) or
            (ArmCannonDrawing.FacingRightFallingGunExtended, 0) => (byte)SamusProjectileDirection.Right,
            (ArmCannonDrawing.FacingRight, 1) or
            (ArmCannonDrawing.FacingLeft, 1) or
            (ArmCannonDrawing.FacingRightAimingUp, 1) or
            (ArmCannonDrawing.FacingRightAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftAimingUp, 3) or
            (ArmCannonDrawing.FacingRightAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftAimingDownLeft, 1) or
            (ArmCannonDrawing.MovingRightGunExtended, 1) or
            (ArmCannonDrawing.MovingLeftGunExtended, 1) or
            (ArmCannonDrawing.MovingRightAimingUpRight, 1) or
            (ArmCannonDrawing.MovingLeftAimingUpLeft, 1) or
            (ArmCannonDrawing.MovingRightAimingDownRight, 1) or
            (ArmCannonDrawing.MovingLeftAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 3) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDown, 1) or
            (ArmCannonDrawing.FacingRightCrouching, 1) or
            (ArmCannonDrawing.FacingLeftCrouching, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 3) or
            (ArmCannonDrawing.FacingRightFallingAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalk, 1) or
            (ArmCannonDrawing.FacingRightMoonwalk, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpTransition, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpMovingForward, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpMovingForward, 1) or
            (ArmCannonDrawing.FacingRightTransitionAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftTransitionAimingUp, 1) or
            (ArmCannonDrawing.FacingRightFallingGunExtended, 1) or
            (ArmCannonDrawing.FacingLeftFallingGunExtended, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 3) or
            (ArmCannonDrawing.FacingRightLandingFromNormalJump, 1) or
            (ArmCannonDrawing.FacingRightLandingFromSpinJump, 1) => NormalDrawingMode,
            (ArmCannonDrawing.FacingLeft, 0) or
            (ArmCannonDrawing.MovingLeftGunExtended, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDrawing.FacingLeftCrouching, 0) or
            (ArmCannonDrawing.FacingRightMoonwalk, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpMovingForward, 0) or
            (ArmCannonDrawing.FacingLeftFallingGunExtended, 0) => (byte)SamusProjectileDirection.Left,
            (ArmCannonDrawing.FacingRightAimingUp, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 0) => (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpRight),
            (ArmCannonDrawing.FacingRightAimingUp, 2) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 2) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 2) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 2) => (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingRight),
            (ArmCannonDrawing.FacingLeftAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 0) => (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpLeft),
            (ArmCannonDrawing.FacingLeftAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 2) => (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingLeft),
            (ArmCannonDrawing.FacingRightAimingUpRight, 0) or
            (ArmCannonDrawing.MovingRightAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight, 0) => (byte)SamusProjectileDirection.UpRight,
            (ArmCannonDrawing.FacingLeftAimingUpLeft, 0) or
            (ArmCannonDrawing.MovingLeftAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft, 0) => (byte)SamusProjectileDirection.UpLeft,
            (ArmCannonDrawing.FacingRightAimingDownRight, 0) or
            (ArmCannonDrawing.MovingRightAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpTransition, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightLandingFromNormalJump, 0) or
            (ArmCannonDrawing.FacingRightLandingFromSpinJump, 0) => (byte)SamusProjectileDirection.DownRight,
            (ArmCannonDrawing.FacingLeftAimingDownLeft, 0) or
            (ArmCannonDrawing.MovingLeftAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft, 0) => (byte)SamusProjectileDirection.DownLeft,
            (ArmCannonDrawing.Default, 1) => HiddenDrawingMode,
            (ArmCannonDrawing.FacingRightNormalJumpAimingDown, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingDown, 0) => (byte)SamusProjectileDirection.DownFacingRight,
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDown, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingDown, 0) => (byte)SamusProjectileDirection.DownFacingLeft,
            (ArmCannonDrawing.FacingRightTransitionAimingUp, 0) => (byte)SamusProjectileDirection.UpFacingRight,
            (ArmCannonDrawing.FacingLeftTransitionAimingUp, 0) => (byte)SamusProjectileDirection.UpFacingLeft,
            _ => null,
        };
        value = control ?? 0;
        return control.HasValue;
    }

    /// <summary>Repeated cover origins and fixed-X/repeated-Y running and moonwalking profiles.</summary>
    /// <remarks>Later stationary pairs, fixed X positions and repeated vertical cycles alias earlier coordinates. Dependencies strictly decrease in address; first-cycle defaults resolve through installed body geometry and the reviewed overlay joins.</remarks>
    internal static bool TryStockCoordinateSource(ushort address, out ushort source)
    {
        int firstPair = address switch
        {
            >= ((ushort)ArmCannonDrawing.FacingRight + 4) and < (ushort)ArmCannonDrawing.FacingLeft => (ushort)ArmCannonDrawing.FacingRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightAimingUp => (ushort)ArmCannonDrawing.FacingLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 4) and < (ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt => (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 4) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUp => (ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDown + 4) and < (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDown => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDown + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 4) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpTransition => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightNormalJumpMovingForward + 4) and < (ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward => (ushort)ArmCannonDrawing.FacingRightNormalJumpMovingForward + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 4) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight => (ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 4) and < (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 4) and < (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightFallingGunExtended => (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightFallingGunExtended + 4) and < (ushort)ArmCannonDrawing.FacingLeftFallingGunExtended => (ushort)ArmCannonDrawing.FacingRightFallingGunExtended + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftFallingGunExtended + 4) and < (ushort)ArmCannonDrawing.FacingRightFallingAimingUp => (ushort)ArmCannonDrawing.FacingLeftFallingGunExtended + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightFallingAimingDown + 4) and < (ushort)ArmCannonDrawing.FacingLeftFallingAimingDown => (ushort)ArmCannonDrawing.FacingRightFallingAimingDown + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftFallingAimingDown + 4) and < (ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight => (ushort)ArmCannonDrawing.FacingLeftFallingAimingDown + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight + 4) and < (ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft => (ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight => (ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight + 4) and < (ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft => (ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightCrouching => (ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightCrouching + 4) and < (ushort)ArmCannonDrawing.FacingLeftCrouching => (ushort)ArmCannonDrawing.FacingRightCrouching + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftCrouching + 4) and < (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUpRight => (ushort)ArmCannonDrawing.FacingLeftCrouching + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightTransitionAimingUp + 4) and < (ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp => (ushort)ArmCannonDrawing.FacingRightTransitionAimingUp + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp + 4) and < AdjacentCostStart => (ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp + 2,
            _ => -1,
        };
        if (firstPair >= 0)
        {
            source = (ushort)(firstPair + ((address - firstPair) & 1));
            return true;
        }
        // Running/moonwalking keeps its horizontal origin while the vertical
        // component follows separately supplied animation motion.
        int firstX = address switch
        {
            >= ((ushort)ArmCannonDrawing.MovingRightGunExtended + 4) and < (ushort)ArmCannonDrawing.MovingLeftGunExtended => (ushort)ArmCannonDrawing.MovingRightGunExtended + 2,
            >= ((ushort)ArmCannonDrawing.MovingLeftGunExtended + 4) and < (ushort)ArmCannonDrawing.MovingRightAimingUpRight => (ushort)ArmCannonDrawing.MovingLeftGunExtended + 2,
            >= ((ushort)ArmCannonDrawing.MovingRightAimingUpRight + 4) and < (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 2,
            >= ((ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 4) and < (ushort)ArmCannonDrawing.MovingRightAimingDownRight => (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 2,
            >= ((ushort)ArmCannonDrawing.MovingRightAimingDownRight + 4) and < (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 2,
            >= ((ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt => (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalk + 4) and < (ushort)ArmCannonDrawing.FacingRightMoonwalk => (ushort)ArmCannonDrawing.FacingLeftMoonwalk + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalk + 4) and < (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft => (ushort)ArmCannonDrawing.FacingRightMoonwalk + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 4) and < (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft => (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 2,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 4) and < (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 2,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 4) and < (ushort)ArmCannonDrawing.FacingRightLandingFromNormalJump => (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 2,
            _ => -1,
        };
        bool fixedX = firstX >= 0 && ((address - firstX) & 1) == 0;
        if (fixedX)
        {
            source = (ushort)firstX;
            return true;
        }
        // These vertical cycles repeat once per half of the native animation.
        // First-cycle positions resolve separately, preserving independent supplied edits.
        int cycleBytes = address switch
        {
            >= ((ushort)ArmCannonDrawing.MovingRightAimingUpRight + 13) and < (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft => 10,
            >= ((ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 13) and < (ushort)ArmCannonDrawing.MovingRightAimingDownRight => 10,
            >= ((ushort)ArmCannonDrawing.MovingRightAimingDownRight + 13) and < (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft => 10,
            >= ((ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 13) and < (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt => 10,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalk + 9) and < (ushort)ArmCannonDrawing.FacingRightMoonwalk => 6,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalk + 9) and < (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft => 6,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 9) and < (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight => 6,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 9) and < (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft => 6,
            >= ((ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 9) and < (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight => 6,
            >= ((ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 9) and < (ushort)ArmCannonDrawing.FacingRightLandingFromNormalJump => 6,
            _ => 0,
        };
        bool repeatedY = cycleBytes != 0 && firstX >= 0 && ((address - firstX) & 1) != 0;
        if (repeatedY)
        {
            source = (ushort)(address - cycleBytes);
            return true;
        }
        // Horizontal reflection preserves Y only in these exact native pose/frame
        // pairs. Other vertical profiles keep their independently chosen offsets.
        int pairedYSource = address switch
        {
            (ushort)ArmCannonDrawing.FacingLeft + 3 => (ushort)ArmCannonDrawing.FacingRight + 3,
            (ushort)ArmCannonDrawing.FacingLeftAimingDownLeft + 3 => (ushort)ArmCannonDrawing.FacingRightAimingDownRight + 3,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 3 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 3,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 5 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 5,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 7 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 7,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 9 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 9,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 11 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 11,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 3 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 3,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 5 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 5,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 7 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 7,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 9 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 9,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 11 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 11,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 3 => (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 3,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 3 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDown + 3,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 3 => (ushort)ArmCannonDrawing.FacingRightNormalJumpMovingForward + 3,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 3 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 3,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 3 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 3,
            (ushort)ArmCannonDrawing.FacingLeftFallingGunExtended + 3 => (ushort)ArmCannonDrawing.FacingRightFallingGunExtended + 3,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingDown + 3 => (ushort)ArmCannonDrawing.FacingRightFallingAimingDown + 3,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 3 => (ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight + 3,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 3 => (ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight + 3,
            (ushort)ArmCannonDrawing.FacingLeftCrouching + 3 => (ushort)ArmCannonDrawing.FacingRightCrouching + 3,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft + 3 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingDownRight + 3,
            (ushort)ArmCannonDrawing.FacingRightMoonwalk + 3 => (ushort)ArmCannonDrawing.FacingLeftMoonwalk + 3,
            (ushort)ArmCannonDrawing.FacingRightMoonwalk + 5 => (ushort)ArmCannonDrawing.FacingLeftMoonwalk + 5,
            (ushort)ArmCannonDrawing.FacingRightMoonwalk + 7 => (ushort)ArmCannonDrawing.FacingLeftMoonwalk + 7,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 3 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 3,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 5 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 5,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 7 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 7,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 3 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 3,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 5 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 5,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 7 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 7,
            (ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp + 3 => (ushort)ArmCannonDrawing.FacingRightTransitionAimingUp + 3,
            (ushort)ArmCannonDrawing.FacingLeftAimingUp + 7 => (ushort)ArmCannonDrawing.FacingRightAimingUp + 7,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 7 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUp + 7,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUp + 7 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUp + 7,
            _ => -1,
        };
        source = pairedYSource >= 0 ? (ushort)pairedYSource : (ushort)0;
        return pairedYSource >= 0;
    }

    /// <summary>$90:C663 DrawArmCannon emits one small OBJ and transfers one32-byte4bpp tile: its horizontal footprint is eight pixels.</summary>
    private const int CoverWidthPixels = 8;

    internal static byte ReflectCoverX(byte coordinate) => unchecked((byte)(-unchecked((sbyte)coordinate) - CoverWidthPixels));

    /// <summary>Opposite-facing cover origins reflect the eight-pixel footprint around Samus's origin.</summary>
    /// <remarks>Only exact named facing pairs are included. Other coordinates resolve through
    /// the body attachment calculation, including asymmetric downward joins. Each source
    /// precedes its reflected result in the native window.</remarks>
    internal static bool TryStockReflectedXSource(ushort address, out ushort source)
    {
        int selected = address switch
        {
            (ushort)ArmCannonDrawing.FacingLeft + 2 => (ushort)ArmCannonDrawing.FacingRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftAimingUp + 4 => (ushort)ArmCannonDrawing.FacingRightAimingUp + 4,
            (ushort)ArmCannonDrawing.FacingLeftAimingUp + 6 => (ushort)ArmCannonDrawing.FacingRightAimingUp + 6,
            (ushort)ArmCannonDrawing.FacingLeftAimingUpLeft + 2 => (ushort)ArmCannonDrawing.FacingRightAimingUpRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftAimingDownLeft + 2 => (ushort)ArmCannonDrawing.FacingRightAimingDownRight + 2,
            (ushort)ArmCannonDrawing.MovingLeftGunExtended + 2 => (ushort)ArmCannonDrawing.MovingRightGunExtended + 2,
            (ushort)ArmCannonDrawing.MovingLeftAimingUpLeft + 2 => (ushort)ArmCannonDrawing.MovingRightAimingUpRight + 2,
            (ushort)ArmCannonDrawing.MovingLeftAimingDownLeft + 2 => (ushort)ArmCannonDrawing.MovingRightAimingDownRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 2 => (ushort)ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 2,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 4 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUp + 4,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 6 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUp + 6,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 2 => (ushort)ArmCannonDrawing.FacingRightNormalJumpMovingForward + 2,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 2 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 2 => (ushort)ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftFallingGunExtended + 2 => (ushort)ArmCannonDrawing.FacingRightFallingGunExtended + 2,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingUp + 4 => (ushort)ArmCannonDrawing.FacingRightFallingAimingUp + 4,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingUp + 6 => (ushort)ArmCannonDrawing.FacingRightFallingAimingUp + 6,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingUp + 8 => (ushort)ArmCannonDrawing.FacingRightFallingAimingUp + 8,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 2 => (ushort)ArmCannonDrawing.FacingRightFallingAimingUpRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 2 => (ushort)ArmCannonDrawing.FacingRightFallingAimingDownRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftCrouching + 2 => (ushort)ArmCannonDrawing.FacingRightCrouching + 2,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft + 2 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUpRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft + 2 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingDownRight + 2,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUp + 4 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUp + 4,
            (ushort)ArmCannonDrawing.FacingLeftCrouchingAimingUp + 6 => (ushort)ArmCannonDrawing.FacingRightCrouchingAimingUp + 6,
            (ushort)ArmCannonDrawing.FacingRightMoonwalk + 2 => (ushort)ArmCannonDrawing.FacingLeftMoonwalk + 2,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 2 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 2,
            (ushort)ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 2 => (ushort)ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 2,
            (ushort)ArmCannonDrawing.FacingLeftTransitionAimingUp + 2 => (ushort)ArmCannonDrawing.FacingRightTransitionAimingUp + 2,
            _ => -1,
        };
        source = selected >= 0 ? (ushort)selected : (ushort)0;
        return selected >= 0;
    }

    private enum TileOrientation
    {
        /// <summary>$90:C7B9 selects vertical frames from $9A:9A00.</summary>
        Vertical,
        /// <summary>$90:C7C1 selects horizontal frames from $9A:A000.</summary>
        Horizontal,
        /// <summary>$90:C7C9 selects downward-diagonal frames from $9A:A600.</summary>
        DownwardDiagonal,
        /// <summary>$90:C7D1 selects upward-diagonal frames from $9A:AC00.</summary>
        UpwardDiagonal,
    }
    /// <summary>$90:C791..C7A4 assigns OBJ character $1F, palette4 and priority2 to the cannon cover.</summary>
    private const ushort CoverSpriteIdentity = 0x001f | (4 << 9) | (2 << 12);
    /// <summary>$90:C791..C7A4 uses bit14 to reflect the selected cover artwork horizontally.</summary>
    private const ushort HorizontalReflection = 1 << 14;
    /// <summary>$90:C791..C7A4 uses bit15 to reflect the vertical source for downward aim.</summary>
    private const ushort VerticalReflection = 1 << 15;

    /// <summary>$90:C7A5..C7D8 chooses a physical artwork orientation and three successive opening frames; frame0 is the closed no-transfer sentinel.</summary>
    internal static ushort StockTileSource(SamusProjectileDirection direction, int frame)
    {
        if (frame == 0) return 0;
        TileOrientation orientation = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => TileOrientation.UpwardDiagonal,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => TileOrientation.Horizontal,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => TileOrientation.DownwardDiagonal,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft or
                SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => TileOrientation.Vertical,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined projectile direction."),
        };
        return TileSourcePointers[(int)orientation * (FramesPerDirection - 1) + frame - 1];
    }

    /// <summary>$90:C791..C7A4 reflects the selected vertical/horizontal/diagonal artwork according to native aiming direction.</summary>
    internal static ushort StockSpriteAttributes(SamusProjectileDirection direction)
    {
        bool horizontal = direction is SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingLeft
            or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft;
        bool vertical = direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft;
        return (ushort)(CoverSpriteIdentity | (horizontal ? HorizontalReflection : 0) | (vertical ? VerticalReflection : 0));
    }
    /// <summary>$9A:9A00, Tiles_NonClosed_ArmCannon_Vertical_0, first of twelve cover tile sources.</summary>
    private const int FirstTileSource = 0x9a00;
    /// <summary>The twelve native cover sources from $9A:9A00 through $9A:B000 occupy successive $200-byte character blocks.</summary>
    private const int TileSourceStride = 0x0200;
    /// <summary>Twelve source identities selected by the four three-frame lists at $90:C7B9..C7D8.</summary>
    public static TileSourceSequence TileSourcePointers => new();

    /// <summary>Calculated source identities in the original exported tile order; no tile pixels are inferred.</summary>
    public readonly record struct TileSourceSequence : IReadOnlyList<ushort>
    {
        /// <summary>Twelve source identities: three cover frames for each vertical, horizontal, downward-diagonal, and upward-diagonal artwork orientation.</summary>
        public int Count => 12;
        /// <summary>Array-style length of the twelve-entry native source sequence.</summary>
        public int Length => Count;
        /// <summary>Gets bank-$9A source identity $9A00 plus index times $200 for zero-based index 0–11; each identity resolves to an installed 32-byte character.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstTileSource + index * TileSourceStride)
            : throw new IndexOutOfRangeException();
        /// <summary>Finds the exact bank-relative native tile identity, returning its exported character index 0–11 or -1 if the address is absent or unaligned.</summary>
        public int IndexOf(ushort source)
        {
            int index = (source - FirstTileSource) / TileSourceStride;
            return (uint)index < Count && this[index] == source ? index : -1;
        }
        /// <summary>Whether a bank-relative source address exactly matches one of the twelve installed cover characters; the closed zero sentinel is not a character.</summary>
        public bool Contains(ushort source) => IndexOf(source) >= 0;
        /// <summary>Enumerates source identities in exported PNG character order, with the three frames of each native artwork orientation kept together.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
