using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed arm-cannon cover placement, OAM attributes, and indexed 8×8 tiles.</summary>
public sealed class SamusArmCannonArtworkCatalog
{
    private readonly Dictionary<int, ushort> posePointers = new();
    private readonly byte[] drawingData;
    private readonly Dictionary<int, ushort> attributes = new();
    private readonly Dictionary<int, ushort> tileSources = new();
    private readonly RoomCharacterAtlas tiles;

    private SamusArmCannonArtworkCatalog(ushort[] posePointers, byte[] drawingData,
        ushort[] attributes, ushort[][] tileSources, RoomCharacterAtlas tiles)
    {
        for (int pose = 0; pose < posePointers.Length; pose++)
            if (posePointers[pose] != SamusArmCannonArtworkFormat.StockPoseDrawingData(pose))
                this.posePointers.Add(pose, posePointers[pose]);
        this.drawingData = drawingData;
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

    /// <summary>SHA-256 of selected cannon placement, OBJ attributes, tile selectors and characters.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), content =>
    {
        Span<ushort> selectedPoses = stackalloc ushort[SamusBodyArtworkCatalog.PoseCount];
        for (int pose = 0; pose < selectedPoses.Length; pose++) selectedPoses[pose] = PoseDrawingData(pose);
        content.AppendWords("pose pointers", selectedPoses);
        content.Append("drawing data", this.drawingData);
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

    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(tilePng);
        Placement placement = LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return FromPlacement(placement, tiles);
    }

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

    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    public ushort PoseDrawingData(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return posePointers.TryGetValue(pose, out ushort selected) ? selected
            : SamusArmCannonArtworkFormat.StockPoseDrawingData(pose);
    }

    public byte ReadDrawingByte(ushort address)
    {
        int index = address - SamusArmCannonArtworkFormat.DrawingDataStart;
        if ((uint)index >= drawingData.Length)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte $90:{address:X4} is not installed.");
        return drawingData[index];
    }

    public ushort SpriteAttributes(int direction)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount) throw new IndexOutOfRangeException();
        return attributes.TryGetValue(direction, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction);
    }

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

public sealed record SamusArmCannonArtworkDocument
{
    public required int Version { get; init; }
    public required int[] PosePointers { get; init; }
    public required int[] DrawingData { get; init; }
    public required int[] SpriteAttributes { get; init; }
    public required int[][] TileSources { get; init; }
}

/// <summary>Bounded retail arm-cannon visual geometry, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{
    public const int Version = 1;
    public const string JsonFileName = "samus-arm-cannon.json";
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>First pose descriptor at $90:C9D9.</summary>
    public const ushort DrawingDataStart = 0xc9d9;
    /// <summary>Descriptor bytes end immediately before the $90:CC39 code entry.</summary>
    public const ushort DrawingDataEndExclusive = 0xcc39;
    public const int DrawingDataByteCount = DrawingDataEndExclusive - DrawingDataStart;
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
            _ => TileOrientation.Vertical,
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
        public int Count => 12;
        public int Length => Count;
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstTileSource + index * TileSourceStride)
            : throw new IndexOutOfRangeException();
        public int IndexOf(ushort source)
        {
            int index = (source - FirstTileSource) / TileSourceStride;
            return (uint)index < Count && this[index] == source ? index : -1;
        }
        public bool Contains(ushort source) => IndexOf(source) >= 0;
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
