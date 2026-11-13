using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Resolves a PLM draw to where the cartridge stores it (#1402). Enumerated families look their draw up in
/// <see cref="RoomObjectDrawSourceDefinitions"/>; families whose draws the cartridge lays out at a fixed stride compute it from the
/// family's first draw.
/// </summary>
internal static class RoomObjectDrawAddressCodec
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
    internal static ushort Of(RoomObjectDraw draw) => Source(draw).Offset;

    /// <summary>Where the cartridge stores <paramref name="draw"/>.</summary>
    private static SnesAddress Source(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.BombBlockRestore[frame],
        RoomObjectFrameDraw<BombTorizoHandDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.BombTorizoHand[frame],
        RoomObjectFrameDraw<BotwoonWallDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.BotwoonWall[frame],
        RoomObjectFrameDraw<ChozoStatueDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.ChozoStatue[frame],
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.ContactCrumbleRestore[frame],
        RoomObjectFrameDraw<CrocomireArenaDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.CrocomireArena[frame],
        RoomObjectFrameDraw<DownwardGateDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.DownwardGate[frame],
        RoomObjectFrameDraw<DraygonCannonDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.DraygonCannon[frame],
        RoomObjectFrameDraw<ElevatorPlatformDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.ElevatorPlatform[frame],
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.EscapeAnimal[frame],
        RoomObjectFrameDraw<KraidRoomDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.KraidRoom[frame],
        RoomObjectFrameDraw<MotherBrainEscapeGateDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.MotherBrainEscapeGate[frame],
        RoomObjectFrameDraw<MotherBrainFakeDeathDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.MotherBrainFakeDeath[frame],
        RoomObjectFrameDraw<MotherBrainGlassDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.MotherBrainGlass[frame],
        RoomObjectFrameDraw<NoobTubeDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.NoobTube[frame],
        RoomObjectFrameDraw<PhantoonDoorDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.PhantoonDoor[frame],
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.RoomObjectBombedReveal[frame],
        RoomObjectFrameDraw<SamusEaterDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.SamusEater[frame],
        RoomObjectFrameDraw<SporeSpawnCeilingDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.SporeSpawnCeiling[frame],
        RoomObjectFrameDraw<StationDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.Station[frame],
        RoomObjectFrameDraw<TourianAccessDraw> { Frame: var frame } => RoomObjectDrawSourceDefinitions.TourianAccess[frame],
        RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame: var shot } => ShotBlock(shot),
        RoomObjectFrameDraw<GrappleBlockDraw> { Frame: var grapple } => Offset(RoomObjectDrawSourceDefinitions.GrappleBlockFirst, GrappleIndex(grapple) * OneBlockStride),
        RoomObjectFrameDraw<RoomObjectCollectibleDrawDefinitions.Draw> { Frame: var item } => Collectible(item),
        RoomObjectFrameDraw<BlueDoorCaps.Draw> { Frame: var blue } => Offset(RoomObjectDrawSourceDefinitions.BlueDoorLeftAlias,
            FacingIndex(blue.Orientation) * BlueFacingStride + (blue.Frame - BlueDoorObjectDrawDefinitions.AliasFrame) * CapStride),
        RoomObjectFrameDraw<GreyDoorCaps.Draw> { Frame: { Frame: GreyDoorObjectDrawDefinitions.ClearFrame } clear } =>
            Offset(RoomObjectDrawSourceDefinitions.DoorClearLeft, FacingIndex(clear.Orientation) * CapStride),
        RoomObjectFrameDraw<GreyDoorCaps.Draw> { Frame: var grey } => Offset(RoomObjectDrawSourceDefinitions.GreyDoorLeft,
            FacingIndex(grey.Orientation) * CapFacingStride + grey.Frame * CapStride),
        RoomObjectFrameDraw<ColoredDoorCaps.Draw> { Frame: var colored } => Offset(RoomObjectDrawSourceDefinitions.ColoredDoorYellowLeft,
            ColorIndex(colored.Color) * ColorStride + FacingIndex(colored.Orientation) * CapFacingStride + colored.Frame * CapStride),
        RoomObjectFrameDraw<EyeDoorDraws.Draw> { Frame: var eye } => EyeDoor(eye),
        RoomObjectFrameDraw<CrateriaMainstreetEscapePassageObjectDefinitions.Draw> => RoomObjectDrawSourceDefinitions.CrateriaPassageClearPair,
        RoomObjectFrameDraw<OldTourianEscapeShaftWallObjectDefinitions.Draw> => RoomObjectDrawSourceDefinitions.OldTourianShaftWallOpen,
        RoomObjectFrameDraw<MaridiaElevatubeObjectDefinitions.Draw> => RoomObjectDrawSourceDefinitions.MaridiaElevatubeBlock,
        RoomObjectFrameDraw<SpeedBoosterBlockObjectDrawDefinitions.Draw> => RoomObjectDrawSourceDefinitions.SpeedBoosterBombReveal,
        _ => throw new InvalidDataException($"{draw} has no cartridge address."),
    };

    /// <summary>A shot-block breakup frame at its shape's stride, or the shape's restoration.</summary>
    private static SnesAddress ShotBlock(ShotBlockObjectDraws.Draw shot) =>
        shot.Restore ? ShotBlockRestoration(shot) : ShotBlockBreakup(shot);

    /// <summary>A shot-block breakup frame, at its shape's stride from the shape's first frame.</summary>
    private static SnesAddress ShotBlockBreakup(ShotBlockObjectDraws.Draw shot) => shot.Layout switch
    {
        RoomObjectShotBlockDrawDefinitions.Shape.Single => Offset(RoomObjectDrawSourceDefinitions.SingleBreakupFirst, shot.Frame * OneBlockStride),
        RoomObjectShotBlockDrawDefinitions.Shape.Horizontal => Offset(RoomObjectDrawSourceDefinitions.HorizontalBreakupFirst, shot.Frame * PairStride),
        RoomObjectShotBlockDrawDefinitions.Shape.Vertical => Offset(RoomObjectDrawSourceDefinitions.VerticalBreakupFirst, shot.Frame * PairStride),
        RoomObjectShotBlockDrawDefinitions.Shape.Square => Offset(RoomObjectDrawSourceDefinitions.SquareBreakupFirst, shot.Frame * SquareStride),
        _ => throw new InvalidDataException($"Shot-block draw {shot} has no cartridge address."),
    };

    /// <summary>A shot-block shape's restoration draw.</summary>
    private static SnesAddress ShotBlockRestoration(ShotBlockObjectDraws.Draw shot) => shot.Layout switch
    {
        RoomObjectShotBlockDrawDefinitions.Shape.Horizontal => RoomObjectDrawSourceDefinitions.ShotBlockRestoreHorizontal,
        RoomObjectShotBlockDrawDefinitions.Shape.Vertical => RoomObjectDrawSourceDefinitions.ShotBlockRestoreVertical,
        RoomObjectShotBlockDrawDefinitions.Shape.Square => RoomObjectDrawSourceDefinitions.ShotBlockRestoreSquare,
        RoomObjectShotBlockDrawDefinitions.Shape.Single => throw new InvalidDataException($"A single shot block has no restoration draw: {shot}."),
        _ => throw new InvalidDataException($"Shot-block draw {shot} has no cartridge address."),
    };

    /// <summary>A collectible draw: its group's first draw, then one six-byte record per frame.</summary>
    private static SnesAddress Collectible(RoomObjectCollectibleDrawDefinitions.Draw item) => item.Group switch
    {
        RoomObjectCollectibleDrawDefinitions.DrawGroup.Empty => RoomObjectDrawSourceDefinitions.CollectibleEmpty,
        RoomObjectCollectibleDrawDefinitions.DrawGroup.Orb => Offset(RoomObjectDrawSourceDefinitions.CollectibleOrbFirst, item.Index * OneBlockStride),
        RoomObjectCollectibleDrawDefinitions.DrawGroup.OrbBurst => RoomObjectDrawSourceDefinitions.CollectibleOrbBurst,
        RoomObjectCollectibleDrawDefinitions.DrawGroup.Tank => Offset(RoomObjectDrawSourceDefinitions.CollectibleTankFirst, item.Index * OneBlockStride),
        RoomObjectCollectibleDrawDefinitions.DrawGroup.OwnPicture => Offset(RoomObjectDrawSourceDefinitions.CollectibleDynamicFirst, item.Index * OneBlockStride),
        RoomObjectCollectibleDrawDefinitions.DrawGroup.ShotReveal => Offset(RoomObjectDrawSourceDefinitions.CollectibleShotRevealFirst, item.Index * OneBlockStride),
        _ => throw new InvalidDataException($"Collectible draw {item} has no cartridge address."),
    };

    /// <summary>An eye-door draw: the right door's draws follow the left's at a fixed offset.</summary>
    private static SnesAddress EyeDoor(EyeDoorDraws.Draw eye)
    {
        int side = eye.Left ? 0 : RoomObjectDrawSourceDefinitions.EyeDoorRightOffset;
        return eye.Part switch
        {
            EyeDoorDraws.Component.Clear => Offset(RoomObjectDrawSourceDefinitions.EyeDoorMirroredOpeningClear, side),
            EyeDoorDraws.Component.Eye => Offset(RoomObjectDrawSourceDefinitions.EyeDoorLeftEyeFirst, side + eye.Frame * EyeStride),
            EyeDoorDraws.Component.Middle => Offset(RoomObjectDrawSourceDefinitions.EyeDoorLeftMiddleFirst, side + eye.Frame * OneBlockStride),
            EyeDoorDraws.Component.Bottom => Offset(RoomObjectDrawSourceDefinitions.EyeDoorLeftBottomFirst, side + eye.Frame * OneBlockStride),
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
