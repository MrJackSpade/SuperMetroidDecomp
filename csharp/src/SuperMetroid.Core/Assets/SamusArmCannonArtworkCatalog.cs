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
    /// <summary>$90:C9DB, ArmCannonDrawingData_FacingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingForward = 0xC9DB;

    /// <summary>$90:C9DD, ArmCannonDrawingData_FacingRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRight = 0xC9DD;

    /// <summary>$90:C9F1, ArmCannonDrawingData_FacingLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeft = 0xC9F1;

    /// <summary>$90:CA05, ArmCannonDrawingData_FacingRight_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightAimingUp = 0xCA05;

    /// <summary>$90:CA0D, ArmCannonDrawingData_FacingLeft_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftAimingUp = 0xCA0D;

    /// <summary>$90:CA15, ArmCannonDrawingData_FacingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightAimingUpRight = 0xCA15;

    /// <summary>$90:CA19, ArmCannonDrawingData_FacingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftAimingUpLeft = 0xCA19;

    /// <summary>$90:CA1D, ArmCannonDrawingData_FacingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightAimingDownRight = 0xCA1D;

    /// <summary>$90:CA21, ArmCannonDrawingData_FacingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftAimingDownLeft = 0xCA21;

    /// <summary>$90:C9D9, ArmCannonDrawingData_Default: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingDefault = 0xC9D9;

    /// <summary>$90:CA25, ArmCannonDrawingData_MovingRight_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingRightGunExtended = 0xCA25;

    /// <summary>$90:CA3B, ArmCannonDrawingData_MovingLeft_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingLeftGunExtended = 0xCA3B;

    /// <summary>$90:CA51, ArmCannonDrawingData_MovingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingRightAimingUpRight = 0xCA51;

    /// <summary>$90:CA67, ArmCannonDrawingData_MovingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingLeftAimingUpLeft = 0xCA67;

    /// <summary>$90:CA7D, ArmCannonDrawingData_MovingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingRightAimingDownRight = 0xCA7D;

    /// <summary>$90:CA93, ArmCannonDrawingData_MovingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingMovingLeftAimingDownLeft = 0xCA93;

    /// <summary>$90:CAA9, ArmCannonDrawingData_FacingRight_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpNotMovingGunExt = 0xCAA9;

    /// <summary>$90:CAAF, ArmCannonDrawingData_FacingLeft_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpNotMovingGunExt = 0xCAAF;

    /// <summary>$90:CAB5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpAimingUp = 0xCAB5;

    /// <summary>$90:CABD, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpAimingUp = 0xCABD;

    /// <summary>$90:CAC5, ArmCannonDrawingData_FacingRight_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpAimingDown = 0xCAC5;

    /// <summary>$90:CACB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpAimingDown = 0xCACB;

    /// <summary>$90:CB5D, ArmCannonDrawingData_FacingRight_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightCrouching = 0xCB5D;

    /// <summary>$90:CB71, ArmCannonDrawingData_FacingLeft_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftCrouching = 0xCB71;

    /// <summary>$90:CB1D, ArmCannonDrawingData_FacingRight_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightFallingAimingUp = 0xCB1D;

    /// <summary>$90:CB27, ArmCannonDrawingData_FacingLeft_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftFallingAimingUp = 0xCB27;

    /// <summary>$90:CB31, ArmCannonDrawingData_FacingRight_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightFallingAimingDown = 0xCB31;

    /// <summary>$90:CB37, ArmCannonDrawingData_FacingLeft_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftFallingAimingDown = 0xCB37;

    /// <summary>$90:CBA5, ArmCannonDrawingData_FacingLeft_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftMoonwalk = 0xCBA5;

    /// <summary>$90:CBB3, ArmCannonDrawingData_FacingRight_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightMoonwalk = 0xCBB3;

    /// <summary>$90:CAD1, ArmCannonDrawingData_FacingRight_NormalJumpTransition: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpTransition = 0xCAD1;

    /// <summary>$90:CAD9, ArmCannonDrawingData_FacingRight_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpMovingForward = 0xCAD9;

    /// <summary>$90:CADF, ArmCannonDrawingData_FacingLeft_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpMovingForward = 0xCADF;

    /// <summary>$90:CC15, ArmCannonDrawingData_FacingRight_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightTransitionAimingUp = 0xCC15;

    /// <summary>$90:CC1B, ArmCannonDrawingData_FacingLeft_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftTransitionAimingUp = 0xCC1B;

    /// <summary>$90:CAFD, ArmCannonDrawingData_FacingRight_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightFallingGunExtended = 0xCAFD;

    /// <summary>$90:CB0D, ArmCannonDrawingData_FacingLeft_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftFallingGunExtended = 0xCB0D;

    /// <summary>$90:CAE5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpAimingUpRight = 0xCAE5;

    /// <summary>$90:CAEB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpAimingUpLeft = 0xCAEB;

    /// <summary>$90:CAF1, ArmCannonDrawingData_FacingRight_NormalJump_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightNormalJumpAimingDownRight = 0xCAF1;

    /// <summary>$90:CAF7, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftNormalJumpAimingDownLeft = 0xCAF7;

    /// <summary>$90:CB3D, ArmCannonDrawingData_FacingRight_Falling_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightFallingAimingUpRight = 0xCB3D;

    /// <summary>$90:CB45, ArmCannonDrawingData_FacingLeft_Falling_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftFallingAimingUpLeft = 0xCB45;

    /// <summary>$90:CB4D, ArmCannonDrawingData_FacingRight_Falling_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightFallingAimingDownRight = 0xCB4D;

    /// <summary>$90:CB55, ArmCannonDrawingData_FacingLeft_Falling_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftFallingAimingDownLeft = 0xCB55;

    /// <summary>$90:CB85, ArmCannonDrawingData_FacingRight_Crouching_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightCrouchingAimingUpRight = 0xCB85;

    /// <summary>$90:CB89, ArmCannonDrawingData_FacingLeft_Crouching_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftCrouchingAimingUpLeft = 0xCB89;

    /// <summary>$90:CB8D, ArmCannonDrawingData_FacingRight_Crouching_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightCrouchingAimingDownRight = 0xCB8D;

    /// <summary>$90:CB91, ArmCannonDrawingData_FacingLeft_Crouching_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftCrouchingAimingDownLeft = 0xCB91;

    /// <summary>$90:CBC1, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftMoonwalkAimingUpLeft = 0xCBC1;

    /// <summary>$90:CBCF, ArmCannonDrawingData_FacingRight_Moonwalk_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightMoonwalkAimingUpRight = 0xCBCF;

    /// <summary>$90:CBDD, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftMoonwalkAimingDownLeft = 0xCBDD;

    /// <summary>$90:CBEB, ArmCannonDrawingData_FacingRight_Moonwalk_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightMoonwalkAimingDownRight = 0xCBEB;

    /// <summary>$90:CB95, ArmCannonDrawingData_FacingRight_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightCrouchingAimingUp = 0xCB95;

    /// <summary>$90:CB9D, ArmCannonDrawingData_FacingLeft_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingLeftCrouchingAimingUp = 0xCB9D;

    /// <summary>$90:CBF9, ArmCannonDrawingData_FacingRight_LandingFromNormalJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightLandingFromNormalJump = 0xCBF9;

    /// <summary>$90:CC05, ArmCannonDrawingData_FacingRight_LandingFromSpinJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
    private const ushort DrawingFacingRightLandingFromSpinJump = 0xCC05;

    /// <summary>$90:C7DF-$C9D8, ArmCannonDrawingData: named descriptor selection for the existing 253-pose installed domain.</summary>
    internal static ushort StockPoseDrawingData(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return (SamusPoseId)pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => DrawingFacingForward,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.UnusedPose47 or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.FiringLandingRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose => DrawingFacingRight,
            SamusPoseId.FacingLeftNormalPose or
            SamusPoseId.UnusedPose48 or
            SamusPoseId.RanIntoWallLeftPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.DraygonGrabbedFiringLeftPose or
            SamusPoseId.FiringLandingLeftPose => DrawingFacingLeft,
            SamusPoseId.StandingAimUpRightPose => DrawingFacingRightAimingUp,
            SamusPoseId.StandingAimUpLeftPose => DrawingFacingLeftAimingUp,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.LandingAimDiagonalUpRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => DrawingFacingRightAimingUpRight,
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.DraygonGrabbedAimUpLeftPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.LandingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => DrawingFacingLeftAimingUpLeft,
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.LandingAimDiagonalDownRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => DrawingFacingRightAimingDownRight,
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
            SamusPoseId.GrappleStandingDownLeftPose or
            SamusPoseId.DraygonGrabbedAimDownLeftPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.LandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => DrawingFacingLeftAimingDownLeft,
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
            SamusPoseId.DraygonGrabbedMovingRightPose => DrawingDefault,
            SamusPoseId.MovingRightGunExtendedPose => DrawingMovingRightGunExtended,
            SamusPoseId.MovingLeftGunExtendedPose => DrawingMovingLeftGunExtended,
            SamusPoseId.RunningAimDiagonalUpRightPose => DrawingMovingRightAimingUpRight,
            SamusPoseId.RunningAimDiagonalUpLeftPose => DrawingMovingLeftAimingUpLeft,
            SamusPoseId.RunningAimDiagonalDownRightPose => DrawingMovingRightAimingDownRight,
            SamusPoseId.RunningAimDiagonalDownLeftPose => DrawingMovingLeftAimingDownLeft,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.UnusedPoseAC => DrawingFacingRightNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.UnusedPoseAD => DrawingFacingLeftNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpAimUpRightPose => DrawingFacingRightNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimUpLeftPose => DrawingFacingLeftNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.UnusedPoseAE => DrawingFacingRightNormalJumpAimingDown,
            SamusPoseId.NormalJumpAimDownLeftPose or
            SamusPoseId.UnusedPoseAF => DrawingFacingLeftNormalJumpAimingDown,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.GrappleCrouchingRightPose => DrawingFacingRightCrouching,
            SamusPoseId.CrouchingLeftPose or
            SamusPoseId.GrappleCrouchingLeftPose => DrawingFacingLeftCrouching,
            SamusPoseId.FallingAimUpRightPose => DrawingFacingRightFallingAimingUp,
            SamusPoseId.FallingAimUpLeftPose => DrawingFacingLeftFallingAimingUp,
            SamusPoseId.FallingAimDownRightPose => DrawingFacingRightFallingAimingDown,
            SamusPoseId.FallingAimDownLeftPose => DrawingFacingLeftFallingAimingDown,
            SamusPoseId.MoonwalkFacingLeftPose => DrawingFacingLeftMoonwalk,
            SamusPoseId.MoonwalkFacingRightPose => DrawingFacingRightMoonwalk,
            SamusPoseId.NeutralJumpTransitionRightPose => DrawingFacingRightNormalJumpTransition,
            SamusPoseId.NormalJumpForwardRightPose => DrawingFacingRightNormalJumpMovingForward,
            SamusPoseId.NormalJumpForwardLeftPose => DrawingFacingLeftNormalJumpMovingForward,
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.LandingAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimUpRightPose or
            SamusPoseId.StandingTransitionAimUpRightPose => DrawingFacingRightTransitionAimingUp,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.LandingAimUpLeftPose or
            SamusPoseId.CrouchingTransitionAimUpLeftPose or
            SamusPoseId.StandingTransitionAimUpLeftPose => DrawingFacingLeftTransitionAimingUp,
            SamusPoseId.FallingGunExtendedRightPose => DrawingFacingRightFallingGunExtended,
            SamusPoseId.FallingGunExtendedLeftPose => DrawingFacingLeftFallingGunExtended,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose => DrawingFacingRightNormalJumpAimingUpRight,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose => DrawingFacingLeftNormalJumpAimingUpLeft,
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.UnusedPoseB0 => DrawingFacingRightNormalJumpAimingDownRight,
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
            SamusPoseId.UnusedPoseB1 => DrawingFacingLeftNormalJumpAimingDownLeft,
            SamusPoseId.FallingAimDiagonalUpRightPose => DrawingFacingRightFallingAimingUpRight,
            SamusPoseId.FallingAimDiagonalUpLeftPose => DrawingFacingLeftFallingAimingUpLeft,
            SamusPoseId.FallingAimDiagonalDownRightPose => DrawingFacingRightFallingAimingDownRight,
            SamusPoseId.FallingAimDiagonalDownLeftPose => DrawingFacingLeftFallingAimingDownLeft,
            SamusPoseId.CrouchingAimDiagonalUpRightPose => DrawingFacingRightCrouchingAimingUpRight,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose => DrawingFacingLeftCrouchingAimingUpLeft,
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.GrappleCrouchingDownRightPose => DrawingFacingRightCrouchingAimingDownRight,
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.GrappleCrouchingDownLeftPose => DrawingFacingLeftCrouchingAimingDownLeft,
            SamusPoseId.MoonwalkAimUpLeftPose => DrawingFacingLeftMoonwalkAimingUpLeft,
            SamusPoseId.MoonwalkAimUpRightPose => DrawingFacingRightMoonwalkAimingUpRight,
            SamusPoseId.MoonwalkAimDownLeftPose => DrawingFacingLeftMoonwalkAimingDownLeft,
            SamusPoseId.MoonwalkAimDownRightPose => DrawingFacingRightMoonwalkAimingDownRight,
            SamusPoseId.CrouchingAimUpRightPose => DrawingFacingRightCrouchingAimingUp,
            SamusPoseId.CrouchingAimUpLeftPose => DrawingFacingLeftCrouchingAimingUp,
            SamusPoseId.NormalLandingRightPose => DrawingFacingRightLandingFromNormalJump,
            SamusPoseId.SpinLandingRightPose => DrawingFacingRightLandingFromSpinJump,
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

    /// <summary>Calculates named descriptor controls and adjacent cost aliases; coordinate geometry is resolved separately from body artwork.</summary>
    internal static bool TryStockDrawingByte(ushort address, out byte value)
    {
        if (address is >= AdjacentCostStart and < DrawingDataEndExclusive)
        {
            int offset = address - AdjacentCostStart;
            ushort cost = SamusComboMechanicsDefinitions.GetPowerBombCost(offset / sizeof(ushort));
            value = (byte)(cost >> ((offset & 1) * 8));
            return true;
        }
        switch (address)
        {
            case DrawingFacingForward:
            case DrawingDefault:
                value = 0;
                return true;
            case DrawingFacingForward + 1:
                value = ForwardDrawingMode;
                return true;
            case DrawingFacingRight:
            case DrawingMovingRightGunExtended:
            case DrawingFacingRightNormalJumpNotMovingGunExt:
            case DrawingFacingRightCrouching:
            case DrawingFacingLeftMoonwalk:
            case DrawingFacingRightNormalJumpMovingForward:
            case DrawingFacingRightFallingGunExtended:
                value = (byte)SamusProjectileDirection.Right;
                return true;
            case DrawingFacingRight + 1:
            case DrawingFacingLeft + 1:
            case DrawingFacingRightAimingUp + 1:
            case DrawingFacingRightAimingUp + 3:
            case DrawingFacingLeftAimingUp + 1:
            case DrawingFacingLeftAimingUp + 3:
            case DrawingFacingRightAimingUpRight + 1:
            case DrawingFacingLeftAimingUpLeft + 1:
            case DrawingFacingRightAimingDownRight + 1:
            case DrawingFacingLeftAimingDownLeft + 1:
            case DrawingMovingRightGunExtended + 1:
            case DrawingMovingLeftGunExtended + 1:
            case DrawingMovingRightAimingUpRight + 1:
            case DrawingMovingLeftAimingUpLeft + 1:
            case DrawingMovingRightAimingDownRight + 1:
            case DrawingMovingLeftAimingDownLeft + 1:
            case DrawingFacingRightNormalJumpNotMovingGunExt + 1:
            case DrawingFacingLeftNormalJumpNotMovingGunExt + 1:
            case DrawingFacingRightNormalJumpAimingUp + 1:
            case DrawingFacingRightNormalJumpAimingUp + 3:
            case DrawingFacingLeftNormalJumpAimingUp + 1:
            case DrawingFacingLeftNormalJumpAimingUp + 3:
            case DrawingFacingRightNormalJumpAimingDown + 1:
            case DrawingFacingLeftNormalJumpAimingDown + 1:
            case DrawingFacingRightCrouching + 1:
            case DrawingFacingLeftCrouching + 1:
            case DrawingFacingRightFallingAimingUp + 1:
            case DrawingFacingRightFallingAimingUp + 3:
            case DrawingFacingLeftFallingAimingUp + 1:
            case DrawingFacingLeftFallingAimingUp + 3:
            case DrawingFacingRightFallingAimingDown + 1:
            case DrawingFacingLeftFallingAimingDown + 1:
            case DrawingFacingLeftMoonwalk + 1:
            case DrawingFacingRightMoonwalk + 1:
            case DrawingFacingRightNormalJumpTransition + 1:
            case DrawingFacingRightNormalJumpMovingForward + 1:
            case DrawingFacingLeftNormalJumpMovingForward + 1:
            case DrawingFacingRightTransitionAimingUp + 1:
            case DrawingFacingLeftTransitionAimingUp + 1:
            case DrawingFacingRightFallingGunExtended + 1:
            case DrawingFacingLeftFallingGunExtended + 1:
            case DrawingFacingRightNormalJumpAimingUpRight + 1:
            case DrawingFacingLeftNormalJumpAimingUpLeft + 1:
            case DrawingFacingRightNormalJumpAimingDownRight + 1:
            case DrawingFacingLeftNormalJumpAimingDownLeft + 1:
            case DrawingFacingRightFallingAimingUpRight + 1:
            case DrawingFacingLeftFallingAimingUpLeft + 1:
            case DrawingFacingRightFallingAimingDownRight + 1:
            case DrawingFacingLeftFallingAimingDownLeft + 1:
            case DrawingFacingRightCrouchingAimingUpRight + 1:
            case DrawingFacingLeftCrouchingAimingUpLeft + 1:
            case DrawingFacingRightCrouchingAimingDownRight + 1:
            case DrawingFacingLeftCrouchingAimingDownLeft + 1:
            case DrawingFacingLeftMoonwalkAimingUpLeft + 1:
            case DrawingFacingRightMoonwalkAimingUpRight + 1:
            case DrawingFacingLeftMoonwalkAimingDownLeft + 1:
            case DrawingFacingRightMoonwalkAimingDownRight + 1:
            case DrawingFacingRightCrouchingAimingUp + 1:
            case DrawingFacingRightCrouchingAimingUp + 3:
            case DrawingFacingLeftCrouchingAimingUp + 1:
            case DrawingFacingLeftCrouchingAimingUp + 3:
            case DrawingFacingRightLandingFromNormalJump + 1:
            case DrawingFacingRightLandingFromSpinJump + 1:
                value = NormalDrawingMode;
                return true;
            case DrawingFacingLeft:
            case DrawingMovingLeftGunExtended:
            case DrawingFacingLeftNormalJumpNotMovingGunExt:
            case DrawingFacingLeftCrouching:
            case DrawingFacingRightMoonwalk:
            case DrawingFacingLeftNormalJumpMovingForward:
            case DrawingFacingLeftFallingGunExtended:
                value = (byte)SamusProjectileDirection.Left;
                return true;
            case DrawingFacingRightAimingUp:
            case DrawingFacingRightNormalJumpAimingUp:
            case DrawingFacingRightFallingAimingUp:
            case DrawingFacingRightCrouchingAimingUp:
                value = (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpRight);
                return true;
            case DrawingFacingRightAimingUp + 2:
            case DrawingFacingRightNormalJumpAimingUp + 2:
            case DrawingFacingRightFallingAimingUp + 2:
            case DrawingFacingRightCrouchingAimingUp + 2:
                value = (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingRight);
                return true;
            case DrawingFacingLeftAimingUp:
            case DrawingFacingLeftNormalJumpAimingUp:
            case DrawingFacingLeftFallingAimingUp:
            case DrawingFacingLeftCrouchingAimingUp:
                value = (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpLeft);
                return true;
            case DrawingFacingLeftAimingUp + 2:
            case DrawingFacingLeftNormalJumpAimingUp + 2:
            case DrawingFacingLeftFallingAimingUp + 2:
            case DrawingFacingLeftCrouchingAimingUp + 2:
                value = (byte)(FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingLeft);
                return true;
            case DrawingFacingRightAimingUpRight:
            case DrawingMovingRightAimingUpRight:
            case DrawingFacingRightNormalJumpAimingUpRight:
            case DrawingFacingRightFallingAimingUpRight:
            case DrawingFacingRightCrouchingAimingUpRight:
            case DrawingFacingRightMoonwalkAimingUpRight:
                value = (byte)SamusProjectileDirection.UpRight;
                return true;
            case DrawingFacingLeftAimingUpLeft:
            case DrawingMovingLeftAimingUpLeft:
            case DrawingFacingLeftNormalJumpAimingUpLeft:
            case DrawingFacingLeftFallingAimingUpLeft:
            case DrawingFacingLeftCrouchingAimingUpLeft:
            case DrawingFacingLeftMoonwalkAimingUpLeft:
                value = (byte)SamusProjectileDirection.UpLeft;
                return true;
            case DrawingFacingRightAimingDownRight:
            case DrawingMovingRightAimingDownRight:
            case DrawingFacingRightNormalJumpTransition:
            case DrawingFacingRightNormalJumpAimingDownRight:
            case DrawingFacingRightFallingAimingDownRight:
            case DrawingFacingRightCrouchingAimingDownRight:
            case DrawingFacingRightMoonwalkAimingDownRight:
            case DrawingFacingRightLandingFromNormalJump:
            case DrawingFacingRightLandingFromSpinJump:
                value = (byte)SamusProjectileDirection.DownRight;
                return true;
            case DrawingFacingLeftAimingDownLeft:
            case DrawingMovingLeftAimingDownLeft:
            case DrawingFacingLeftNormalJumpAimingDownLeft:
            case DrawingFacingLeftFallingAimingDownLeft:
            case DrawingFacingLeftCrouchingAimingDownLeft:
            case DrawingFacingLeftMoonwalkAimingDownLeft:
                value = (byte)SamusProjectileDirection.DownLeft;
                return true;
            case DrawingDefault + 1:
                value = HiddenDrawingMode;
                return true;
            case DrawingFacingRightNormalJumpAimingDown:
            case DrawingFacingRightFallingAimingDown:
                value = (byte)SamusProjectileDirection.DownFacingRight;
                return true;
            case DrawingFacingLeftNormalJumpAimingDown:
            case DrawingFacingLeftFallingAimingDown:
                value = (byte)SamusProjectileDirection.DownFacingLeft;
                return true;
            case DrawingFacingRightTransitionAimingUp:
                value = (byte)SamusProjectileDirection.UpFacingRight;
                return true;
            case DrawingFacingLeftTransitionAimingUp:
                value = (byte)SamusProjectileDirection.UpFacingLeft;
                return true;
            default: value = 0; return false;
        }
    }

    /// <summary>Repeated cover origins and fixed-X/repeated-Y running and moonwalking profiles.</summary>
    /// <remarks>Later stationary pairs, fixed X positions and repeated vertical cycles alias earlier coordinates. Dependencies strictly decrease in address; first-cycle defaults resolve through installed body geometry and the reviewed overlay joins.</remarks>
    internal static bool TryStockCoordinateSource(ushort address, out ushort source)
    {
        int firstPair = address switch
        {
            >= (DrawingFacingRight + 4) and < DrawingFacingLeft => DrawingFacingRight + 2,
            >= (DrawingFacingLeft + 4) and < DrawingFacingRightAimingUp => DrawingFacingLeft + 2,
            >= (DrawingFacingRightNormalJumpNotMovingGunExt + 4) and < DrawingFacingLeftNormalJumpNotMovingGunExt => DrawingFacingRightNormalJumpNotMovingGunExt + 2,
            >= (DrawingFacingLeftNormalJumpNotMovingGunExt + 4) and < DrawingFacingRightNormalJumpAimingUp => DrawingFacingLeftNormalJumpNotMovingGunExt + 2,
            >= (DrawingFacingRightNormalJumpAimingDown + 4) and < DrawingFacingLeftNormalJumpAimingDown => DrawingFacingRightNormalJumpAimingDown + 2,
            >= (DrawingFacingLeftNormalJumpAimingDown + 4) and < DrawingFacingRightNormalJumpTransition => DrawingFacingLeftNormalJumpAimingDown + 2,
            >= (DrawingFacingRightNormalJumpMovingForward + 4) and < DrawingFacingLeftNormalJumpMovingForward => DrawingFacingRightNormalJumpMovingForward + 2,
            >= (DrawingFacingLeftNormalJumpMovingForward + 4) and < DrawingFacingRightNormalJumpAimingUpRight => DrawingFacingLeftNormalJumpMovingForward + 2,
            >= (DrawingFacingRightNormalJumpAimingUpRight + 4) and < DrawingFacingLeftNormalJumpAimingUpLeft => DrawingFacingRightNormalJumpAimingUpRight + 2,
            >= (DrawingFacingLeftNormalJumpAimingUpLeft + 4) and < DrawingFacingRightNormalJumpAimingDownRight => DrawingFacingLeftNormalJumpAimingUpLeft + 2,
            >= (DrawingFacingRightNormalJumpAimingDownRight + 4) and < DrawingFacingLeftNormalJumpAimingDownLeft => DrawingFacingRightNormalJumpAimingDownRight + 2,
            >= (DrawingFacingLeftNormalJumpAimingDownLeft + 4) and < DrawingFacingRightFallingGunExtended => DrawingFacingLeftNormalJumpAimingDownLeft + 2,
            >= (DrawingFacingRightFallingGunExtended + 4) and < DrawingFacingLeftFallingGunExtended => DrawingFacingRightFallingGunExtended + 2,
            >= (DrawingFacingLeftFallingGunExtended + 4) and < DrawingFacingRightFallingAimingUp => DrawingFacingLeftFallingGunExtended + 2,
            >= (DrawingFacingRightFallingAimingDown + 4) and < DrawingFacingLeftFallingAimingDown => DrawingFacingRightFallingAimingDown + 2,
            >= (DrawingFacingLeftFallingAimingDown + 4) and < DrawingFacingRightFallingAimingUpRight => DrawingFacingLeftFallingAimingDown + 2,
            >= (DrawingFacingRightFallingAimingUpRight + 4) and < DrawingFacingLeftFallingAimingUpLeft => DrawingFacingRightFallingAimingUpRight + 2,
            >= (DrawingFacingLeftFallingAimingUpLeft + 4) and < DrawingFacingRightFallingAimingDownRight => DrawingFacingLeftFallingAimingUpLeft + 2,
            >= (DrawingFacingRightFallingAimingDownRight + 4) and < DrawingFacingLeftFallingAimingDownLeft => DrawingFacingRightFallingAimingDownRight + 2,
            >= (DrawingFacingLeftFallingAimingDownLeft + 4) and < DrawingFacingRightCrouching => DrawingFacingLeftFallingAimingDownLeft + 2,
            >= (DrawingFacingRightCrouching + 4) and < DrawingFacingLeftCrouching => DrawingFacingRightCrouching + 2,
            >= (DrawingFacingLeftCrouching + 4) and < DrawingFacingRightCrouchingAimingUpRight => DrawingFacingLeftCrouching + 2,
            >= (DrawingFacingRightTransitionAimingUp + 4) and < DrawingFacingLeftTransitionAimingUp => DrawingFacingRightTransitionAimingUp + 2,
            >= (DrawingFacingLeftTransitionAimingUp + 4) and < AdjacentCostStart => DrawingFacingLeftTransitionAimingUp + 2,
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
            >= (DrawingMovingRightGunExtended + 4) and < DrawingMovingLeftGunExtended => DrawingMovingRightGunExtended + 2,
            >= (DrawingMovingLeftGunExtended + 4) and < DrawingMovingRightAimingUpRight => DrawingMovingLeftGunExtended + 2,
            >= (DrawingMovingRightAimingUpRight + 4) and < DrawingMovingLeftAimingUpLeft => DrawingMovingRightAimingUpRight + 2,
            >= (DrawingMovingLeftAimingUpLeft + 4) and < DrawingMovingRightAimingDownRight => DrawingMovingLeftAimingUpLeft + 2,
            >= (DrawingMovingRightAimingDownRight + 4) and < DrawingMovingLeftAimingDownLeft => DrawingMovingRightAimingDownRight + 2,
            >= (DrawingMovingLeftAimingDownLeft + 4) and < DrawingFacingRightNormalJumpNotMovingGunExt => DrawingMovingLeftAimingDownLeft + 2,
            >= (DrawingFacingLeftMoonwalk + 4) and < DrawingFacingRightMoonwalk => DrawingFacingLeftMoonwalk + 2,
            >= (DrawingFacingRightMoonwalk + 4) and < DrawingFacingLeftMoonwalkAimingUpLeft => DrawingFacingRightMoonwalk + 2,
            >= (DrawingFacingLeftMoonwalkAimingUpLeft + 4) and < DrawingFacingRightMoonwalkAimingUpRight => DrawingFacingLeftMoonwalkAimingUpLeft + 2,
            >= (DrawingFacingRightMoonwalkAimingUpRight + 4) and < DrawingFacingLeftMoonwalkAimingDownLeft => DrawingFacingRightMoonwalkAimingUpRight + 2,
            >= (DrawingFacingLeftMoonwalkAimingDownLeft + 4) and < DrawingFacingRightMoonwalkAimingDownRight => DrawingFacingLeftMoonwalkAimingDownLeft + 2,
            >= (DrawingFacingRightMoonwalkAimingDownRight + 4) and < DrawingFacingRightLandingFromNormalJump => DrawingFacingRightMoonwalkAimingDownRight + 2,
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
            >= (DrawingMovingRightAimingUpRight + 13) and < DrawingMovingLeftAimingUpLeft => 10,
            >= (DrawingMovingLeftAimingUpLeft + 13) and < DrawingMovingRightAimingDownRight => 10,
            >= (DrawingMovingRightAimingDownRight + 13) and < DrawingMovingLeftAimingDownLeft => 10,
            >= (DrawingMovingLeftAimingDownLeft + 13) and < DrawingFacingRightNormalJumpNotMovingGunExt => 10,
            >= (DrawingFacingLeftMoonwalk + 9) and < DrawingFacingRightMoonwalk => 6,
            >= (DrawingFacingRightMoonwalk + 9) and < DrawingFacingLeftMoonwalkAimingUpLeft => 6,
            >= (DrawingFacingLeftMoonwalkAimingUpLeft + 9) and < DrawingFacingRightMoonwalkAimingUpRight => 6,
            >= (DrawingFacingRightMoonwalkAimingUpRight + 9) and < DrawingFacingLeftMoonwalkAimingDownLeft => 6,
            >= (DrawingFacingLeftMoonwalkAimingDownLeft + 9) and < DrawingFacingRightMoonwalkAimingDownRight => 6,
            >= (DrawingFacingRightMoonwalkAimingDownRight + 9) and < DrawingFacingRightLandingFromNormalJump => 6,
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
            DrawingFacingLeft + 3 => DrawingFacingRight + 3,
            DrawingFacingLeftAimingDownLeft + 3 => DrawingFacingRightAimingDownRight + 3,
            DrawingMovingLeftAimingUpLeft + 3 => DrawingMovingRightAimingUpRight + 3,
            DrawingMovingLeftAimingUpLeft + 5 => DrawingMovingRightAimingUpRight + 5,
            DrawingMovingLeftAimingUpLeft + 7 => DrawingMovingRightAimingUpRight + 7,
            DrawingMovingLeftAimingUpLeft + 9 => DrawingMovingRightAimingUpRight + 9,
            DrawingMovingLeftAimingUpLeft + 11 => DrawingMovingRightAimingUpRight + 11,
            DrawingMovingLeftAimingDownLeft + 3 => DrawingMovingRightAimingDownRight + 3,
            DrawingMovingLeftAimingDownLeft + 5 => DrawingMovingRightAimingDownRight + 5,
            DrawingMovingLeftAimingDownLeft + 7 => DrawingMovingRightAimingDownRight + 7,
            DrawingMovingLeftAimingDownLeft + 9 => DrawingMovingRightAimingDownRight + 9,
            DrawingMovingLeftAimingDownLeft + 11 => DrawingMovingRightAimingDownRight + 11,
            DrawingFacingLeftNormalJumpNotMovingGunExt + 3 => DrawingFacingRightNormalJumpNotMovingGunExt + 3,
            DrawingFacingLeftNormalJumpAimingDown + 3 => DrawingFacingRightNormalJumpAimingDown + 3,
            DrawingFacingLeftNormalJumpMovingForward + 3 => DrawingFacingRightNormalJumpMovingForward + 3,
            DrawingFacingLeftNormalJumpAimingUpLeft + 3 => DrawingFacingRightNormalJumpAimingUpRight + 3,
            DrawingFacingLeftNormalJumpAimingDownLeft + 3 => DrawingFacingRightNormalJumpAimingDownRight + 3,
            DrawingFacingLeftFallingGunExtended + 3 => DrawingFacingRightFallingGunExtended + 3,
            DrawingFacingLeftFallingAimingDown + 3 => DrawingFacingRightFallingAimingDown + 3,
            DrawingFacingLeftFallingAimingUpLeft + 3 => DrawingFacingRightFallingAimingUpRight + 3,
            DrawingFacingLeftFallingAimingDownLeft + 3 => DrawingFacingRightFallingAimingDownRight + 3,
            DrawingFacingLeftCrouching + 3 => DrawingFacingRightCrouching + 3,
            DrawingFacingLeftCrouchingAimingDownLeft + 3 => DrawingFacingRightCrouchingAimingDownRight + 3,
            DrawingFacingRightMoonwalk + 3 => DrawingFacingLeftMoonwalk + 3,
            DrawingFacingRightMoonwalk + 5 => DrawingFacingLeftMoonwalk + 5,
            DrawingFacingRightMoonwalk + 7 => DrawingFacingLeftMoonwalk + 7,
            DrawingFacingRightMoonwalkAimingUpRight + 3 => DrawingFacingLeftMoonwalkAimingUpLeft + 3,
            DrawingFacingRightMoonwalkAimingUpRight + 5 => DrawingFacingLeftMoonwalkAimingUpLeft + 5,
            DrawingFacingRightMoonwalkAimingUpRight + 7 => DrawingFacingLeftMoonwalkAimingUpLeft + 7,
            DrawingFacingRightMoonwalkAimingDownRight + 3 => DrawingFacingLeftMoonwalkAimingDownLeft + 3,
            DrawingFacingRightMoonwalkAimingDownRight + 5 => DrawingFacingLeftMoonwalkAimingDownLeft + 5,
            DrawingFacingRightMoonwalkAimingDownRight + 7 => DrawingFacingLeftMoonwalkAimingDownLeft + 7,
            DrawingFacingLeftTransitionAimingUp + 3 => DrawingFacingRightTransitionAimingUp + 3,
            DrawingFacingLeftAimingUp + 7 => DrawingFacingRightAimingUp + 7,
            DrawingFacingLeftNormalJumpAimingUp + 7 => DrawingFacingRightNormalJumpAimingUp + 7,
            DrawingFacingLeftCrouchingAimingUp + 7 => DrawingFacingRightCrouchingAimingUp + 7,
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
            DrawingFacingLeft + 2 => DrawingFacingRight + 2,
            DrawingFacingLeftAimingUp + 4 => DrawingFacingRightAimingUp + 4,
            DrawingFacingLeftAimingUp + 6 => DrawingFacingRightAimingUp + 6,
            DrawingFacingLeftAimingUpLeft + 2 => DrawingFacingRightAimingUpRight + 2,
            DrawingFacingLeftAimingDownLeft + 2 => DrawingFacingRightAimingDownRight + 2,
            DrawingMovingLeftGunExtended + 2 => DrawingMovingRightGunExtended + 2,
            DrawingMovingLeftAimingUpLeft + 2 => DrawingMovingRightAimingUpRight + 2,
            DrawingMovingLeftAimingDownLeft + 2 => DrawingMovingRightAimingDownRight + 2,
            DrawingFacingLeftNormalJumpNotMovingGunExt + 2 => DrawingFacingRightNormalJumpNotMovingGunExt + 2,
            DrawingFacingLeftNormalJumpAimingUp + 4 => DrawingFacingRightNormalJumpAimingUp + 4,
            DrawingFacingLeftNormalJumpAimingUp + 6 => DrawingFacingRightNormalJumpAimingUp + 6,
            DrawingFacingLeftNormalJumpMovingForward + 2 => DrawingFacingRightNormalJumpMovingForward + 2,
            DrawingFacingLeftNormalJumpAimingUpLeft + 2 => DrawingFacingRightNormalJumpAimingUpRight + 2,
            DrawingFacingLeftNormalJumpAimingDownLeft + 2 => DrawingFacingRightNormalJumpAimingDownRight + 2,
            DrawingFacingLeftFallingGunExtended + 2 => DrawingFacingRightFallingGunExtended + 2,
            DrawingFacingLeftFallingAimingUp + 4 => DrawingFacingRightFallingAimingUp + 4,
            DrawingFacingLeftFallingAimingUp + 6 => DrawingFacingRightFallingAimingUp + 6,
            DrawingFacingLeftFallingAimingUp + 8 => DrawingFacingRightFallingAimingUp + 8,
            DrawingFacingLeftFallingAimingUpLeft + 2 => DrawingFacingRightFallingAimingUpRight + 2,
            DrawingFacingLeftFallingAimingDownLeft + 2 => DrawingFacingRightFallingAimingDownRight + 2,
            DrawingFacingLeftCrouching + 2 => DrawingFacingRightCrouching + 2,
            DrawingFacingLeftCrouchingAimingUpLeft + 2 => DrawingFacingRightCrouchingAimingUpRight + 2,
            DrawingFacingLeftCrouchingAimingDownLeft + 2 => DrawingFacingRightCrouchingAimingDownRight + 2,
            DrawingFacingLeftCrouchingAimingUp + 4 => DrawingFacingRightCrouchingAimingUp + 4,
            DrawingFacingLeftCrouchingAimingUp + 6 => DrawingFacingRightCrouchingAimingUp + 6,
            DrawingFacingRightMoonwalk + 2 => DrawingFacingLeftMoonwalk + 2,
            DrawingFacingRightMoonwalkAimingUpRight + 2 => DrawingFacingLeftMoonwalkAimingUpLeft + 2,
            DrawingFacingRightMoonwalkAimingDownRight + 2 => DrawingFacingLeftMoonwalkAimingDownLeft + 2,
            DrawingFacingLeftTransitionAimingUp + 2 => DrawingFacingRightTransitionAimingUp + 2,
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
