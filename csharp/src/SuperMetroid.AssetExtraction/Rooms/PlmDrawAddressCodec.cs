using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Resolves a PLM draw to where the cartridge stores it (#1402). Enumerated families look their draw up in
/// <see cref="PlmDrawSourceDefinitions"/>; families whose draws the cartridge lays out at a fixed stride compute it from the
/// family's first draw.
/// </summary>
internal static class PlmDrawAddressCodec
{
    /// <summary>Bytes between successive one-block draws (shot-block single frames, collectibles, Grapple blocks).</summary>
    private const int OneBlockStride = 6;

    /// <summary>Bytes between successive two-block shot-block breakup frames.</summary>
    private const int PairStride = 8;

    /// <summary>Bytes between successive square shot-block breakup frames.</summary>
    private const int SquareStride = 16;

    /// <summary>Bytes between successive door-cap draws.</summary>
    private const int CapStride = 12;

    /// <summary>Bytes between one facing's grey or coloured caps and the next facing's.</summary>
    private const int CapFacingStride = 48;

    /// <summary>Bytes between one colour's caps and the next colour's.</summary>
    private const int ColorStride = 192;

    /// <summary>Bytes between one facing's blue caps (alias and four frames) and the next facing's.</summary>
    private const int BlueFacingStride = 60;

    /// <summary>Bytes between successive eye frames of an eye door.</summary>
    private const int EyeStride = 8;

    /// <summary>The bank-$84 pointer to <paramref name="draw"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> belongs to no family the cartridge stores.</exception>
    internal static ushort Of(PlmDraw draw) => Source(draw).Offset;

    /// <summary>Where the cartridge stores <paramref name="draw"/>.</summary>
    private static SnesAddress Source(PlmDraw draw) => draw switch
    {
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: var frame } => PlmDrawSourceDefinitions.BombBlockRestore[frame],
        PlmFrameDraw<BombTorizoHandDraw> { Frame: var frame } => PlmDrawSourceDefinitions.BombTorizoHand[frame],
        PlmFrameDraw<BotwoonWallDraw> { Frame: var frame } => PlmDrawSourceDefinitions.BotwoonWall[frame],
        PlmFrameDraw<ChozoStatueDraw> { Frame: var frame } => PlmDrawSourceDefinitions.ChozoStatue[frame],
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: var frame } => PlmDrawSourceDefinitions.ContactCrumbleRestore[frame],
        PlmFrameDraw<CrocomireArenaDraw> { Frame: var frame } => PlmDrawSourceDefinitions.CrocomireArena[frame],
        PlmFrameDraw<DownwardGateDraw> { Frame: var frame } => PlmDrawSourceDefinitions.DownwardGate[frame],
        PlmFrameDraw<DraygonCannonDraw> { Frame: var frame } => PlmDrawSourceDefinitions.DraygonCannon[frame],
        PlmFrameDraw<ElevatorPlatformDraw> { Frame: var frame } => PlmDrawSourceDefinitions.ElevatorPlatform[frame],
        PlmFrameDraw<EscapeAnimalDraw> { Frame: var frame } => PlmDrawSourceDefinitions.EscapeAnimal[frame],
        PlmFrameDraw<KraidRoomDraw> { Frame: var frame } => PlmDrawSourceDefinitions.KraidRoom[frame],
        PlmFrameDraw<MotherBrainEscapeGateDraw> { Frame: var frame } => PlmDrawSourceDefinitions.MotherBrainEscapeGate[frame],
        PlmFrameDraw<MotherBrainFakeDeathDraw> { Frame: var frame } => PlmDrawSourceDefinitions.MotherBrainFakeDeath[frame],
        PlmFrameDraw<MotherBrainGlassDraw> { Frame: var frame } => PlmDrawSourceDefinitions.MotherBrainGlass[frame],
        PlmFrameDraw<NoobTubeDraw> { Frame: var frame } => PlmDrawSourceDefinitions.NoobTube[frame],
        PlmFrameDraw<RoomPlmBombedRevealDraw> { Frame: var frame } => PlmDrawSourceDefinitions.RoomPlmBombedReveal[frame],
        PlmFrameDraw<SamusEaterDraw> { Frame: var frame } => PlmDrawSourceDefinitions.SamusEater[frame],
        PlmFrameDraw<SporeSpawnCeilingDraw> { Frame: var frame } => PlmDrawSourceDefinitions.SporeSpawnCeiling[frame],
        PlmFrameDraw<StationDraw> { Frame: var frame } => PlmDrawSourceDefinitions.Station[frame],
        PlmFrameDraw<TourianAccessDraw> { Frame: var frame } => PlmDrawSourceDefinitions.TourianAccess[frame],
        PlmFrameDraw<RoomPlmShotBlockDrawDefinitions.Draw> { Frame: var shot } => ShotBlock(shot),
        PlmFrameDraw<GrappleBlockDraw> { Frame: var grapple } => Offset(PlmDrawSourceDefinitions.GrappleBlockFirst, GrappleIndex(grapple) * OneBlockStride),
        PlmFrameDraw<RoomPlmCollectibleDrawDefinitions.Draw> { Frame: var item } => Collectible(item),
        PlmFrameDraw<BlueDoorPlmDrawDefinitions.Draw> { Frame: var blue } => Offset(PlmDrawSourceDefinitions.BlueDoorLeftAlias,
            FacingIndex(blue.Orientation) * BlueFacingStride + (blue.Frame - BlueDoorPlmDrawDefinitions.AliasFrame) * CapStride),
        PlmFrameDraw<GreyDoorPlmDrawDefinitions.Draw> { Frame: { Frame: GreyDoorPlmDrawDefinitions.ClearFrame } clear } =>
            Offset(PlmDrawSourceDefinitions.DoorClearLeft, FacingIndex(clear.Orientation) * CapStride),
        PlmFrameDraw<GreyDoorPlmDrawDefinitions.Draw> { Frame: var grey } => Offset(PlmDrawSourceDefinitions.GreyDoorLeft,
            FacingIndex(grey.Orientation) * CapFacingStride + grey.Frame * CapStride),
        PlmFrameDraw<ColoredDoorPlmDrawDefinitions.Draw> { Frame: var colored } => Offset(PlmDrawSourceDefinitions.ColoredDoorYellowLeft,
            ColorIndex(colored.Color) * ColorStride + FacingIndex(colored.Orientation) * CapFacingStride + colored.Frame * CapStride),
        PlmFrameDraw<EyeDoorPlmDrawDefinitions.Draw> { Frame: var eye } => EyeDoor(eye),
        PlmFrameDraw<CrateriaMainstreetEscapePassagePlmDefinitions.Draw> => PlmDrawSourceDefinitions.CrateriaPassageClearPair,
        PlmFrameDraw<OldTourianEscapeShaftWallPlmDefinitions.Draw> => PlmDrawSourceDefinitions.OldTourianShaftWallOpen,
        PlmFrameDraw<MaridiaElevatubePlmDefinitions.Draw> => PlmDrawSourceDefinitions.MaridiaElevatubeBlock,
        PlmFrameDraw<SpeedBoosterBlockPlmDrawDefinitions.Draw> => PlmDrawSourceDefinitions.SpeedBoosterBombReveal,
        _ => throw new InvalidDataException($"{draw} has no cartridge address."),
    };

    /// <summary>A shot-block breakup frame at its shape's stride, or the shape's restoration.</summary>
    private static SnesAddress ShotBlock(RoomPlmShotBlockDrawDefinitions.Draw shot) =>
        shot.Restore ? ShotBlockRestoration(shot) : ShotBlockBreakup(shot);

    /// <summary>A shot-block breakup frame, at its shape's stride from the shape's first frame.</summary>
    private static SnesAddress ShotBlockBreakup(RoomPlmShotBlockDrawDefinitions.Draw shot) => shot.Layout switch
    {
        RoomPlmShotBlockDrawDefinitions.Shape.Single => Offset(PlmDrawSourceDefinitions.SingleBreakupFirst, shot.Frame * OneBlockStride),
        RoomPlmShotBlockDrawDefinitions.Shape.Horizontal => Offset(PlmDrawSourceDefinitions.HorizontalBreakupFirst, shot.Frame * PairStride),
        RoomPlmShotBlockDrawDefinitions.Shape.Vertical => Offset(PlmDrawSourceDefinitions.VerticalBreakupFirst, shot.Frame * PairStride),
        RoomPlmShotBlockDrawDefinitions.Shape.Square => Offset(PlmDrawSourceDefinitions.SquareBreakupFirst, shot.Frame * SquareStride),
        _ => throw new InvalidDataException($"Shot-block draw {shot} has no cartridge address."),
    };

    /// <summary>A shot-block shape's restoration draw.</summary>
    private static SnesAddress ShotBlockRestoration(RoomPlmShotBlockDrawDefinitions.Draw shot) => shot.Layout switch
    {
        RoomPlmShotBlockDrawDefinitions.Shape.Horizontal => PlmDrawSourceDefinitions.ShotBlockRestoreHorizontal,
        RoomPlmShotBlockDrawDefinitions.Shape.Vertical => PlmDrawSourceDefinitions.ShotBlockRestoreVertical,
        RoomPlmShotBlockDrawDefinitions.Shape.Square => PlmDrawSourceDefinitions.ShotBlockRestoreSquare,
        RoomPlmShotBlockDrawDefinitions.Shape.Single => throw new InvalidDataException($"A single shot block has no restoration draw: {shot}."),
        _ => throw new InvalidDataException($"Shot-block draw {shot} has no cartridge address."),
    };

    /// <summary>A collectible draw: its group's first draw, then one six-byte record per frame.</summary>
    private static SnesAddress Collectible(RoomPlmCollectibleDrawDefinitions.Draw item) => item.Group switch
    {
        RoomPlmCollectibleDrawDefinitions.DrawGroup.Empty => PlmDrawSourceDefinitions.CollectibleEmpty,
        RoomPlmCollectibleDrawDefinitions.DrawGroup.Orb => Offset(PlmDrawSourceDefinitions.CollectibleOrbFirst, item.Index * OneBlockStride),
        RoomPlmCollectibleDrawDefinitions.DrawGroup.OrbBurst => PlmDrawSourceDefinitions.CollectibleOrbBurst,
        RoomPlmCollectibleDrawDefinitions.DrawGroup.Tank => Offset(PlmDrawSourceDefinitions.CollectibleTankFirst, item.Index * OneBlockStride),
        RoomPlmCollectibleDrawDefinitions.DrawGroup.Dynamic => Offset(PlmDrawSourceDefinitions.CollectibleDynamicFirst, item.Index * OneBlockStride),
        RoomPlmCollectibleDrawDefinitions.DrawGroup.ShotReveal => Offset(PlmDrawSourceDefinitions.CollectibleShotRevealFirst, item.Index * OneBlockStride),
        _ => throw new InvalidDataException($"Collectible draw {item} has no cartridge address."),
    };

    /// <summary>An eye-door draw: the right door's draws follow the left's at a fixed offset.</summary>
    private static SnesAddress EyeDoor(EyeDoorPlmDrawDefinitions.Draw eye)
    {
        int side = eye.Left ? 0 : PlmDrawSourceDefinitions.EyeDoorRightOffset;
        return eye.Part switch
        {
            EyeDoorPlmDrawDefinitions.Component.Clear => Offset(PlmDrawSourceDefinitions.EyeDoorMirroredOpeningClear, side),
            EyeDoorPlmDrawDefinitions.Component.Eye => Offset(PlmDrawSourceDefinitions.EyeDoorLeftEyeFirst, side + eye.Frame * EyeStride),
            EyeDoorPlmDrawDefinitions.Component.Middle => Offset(PlmDrawSourceDefinitions.EyeDoorLeftMiddleFirst, side + eye.Frame * OneBlockStride),
            EyeDoorPlmDrawDefinitions.Component.Bottom => Offset(PlmDrawSourceDefinitions.EyeDoorLeftBottomFirst, side + eye.Frame * OneBlockStride),
            _ => throw new InvalidDataException($"Eye-door draw {eye} has no cartridge address."),
        };
    }

    /// <summary>The position of a Grapple-block draw among the family's five records.</summary>
    private static int GrappleIndex(GrappleBlockDraw grapple) => grapple switch
    {
        GrappleBlockDraw.Intact => 0,
        GrappleBlockDraw.Break0 => 1,
        GrappleBlockDraw.Break1 => 2,
        GrappleBlockDraw.Break2 => 3,
        GrappleBlockDraw.Blank => 4,
        _ => throw new InvalidDataException($"Undefined {nameof(GrappleBlockDraw)} {grapple}."),
    };

    /// <summary>The position of a cap facing in the cartridge's cap tables: left, right, up, down.</summary>
    private static int FacingIndex(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left => 0,
        ColoredDoorOrientation.Right => 1,
        ColoredDoorOrientation.Up => 2,
        ColoredDoorOrientation.Down => 3,
        _ => throw new InvalidDataException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
    };

    /// <summary>The position of a cap colour in the cartridge's coloured-cap table: yellow, green, red.</summary>
    private static int ColorIndex(ColoredDoorColor color) => color switch
    {
        ColoredDoorColor.Yellow => 0,
        ColoredDoorColor.Green => 1,
        ColoredDoorColor.Red => 2,
        _ => throw new InvalidDataException($"Undefined {nameof(ColoredDoorColor)} {color}."),
    };

    /// <summary>An address <paramref name="bytes"/> past <paramref name="first"/>.</summary>
    private static SnesAddress Offset(SnesAddress first, int bytes) => new(first.Bank, checked((ushort)(first.Offset + bytes)));
}
