using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Brinstar rooms.</summary>
internal static class BrinstarRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Brinstar Pre Map room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BrinstarPreMap { get; } = new("BrinstarPreMap",
    [
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(580, 59)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(376, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(424, 88)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(73, 64)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
    ], 4);

    /// <summary>The enemy population of the Green Brinstar Map room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition GreenBrinstarMap { get; } = new("GreenBrinstarMap");

    /// <summary>The enemy population of the Green Brinstar Missile Refill room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition GreenBrinstarMissileRefill { get; } = new("GreenBrinstarMissileRefill");

    /// <summary>The enemy population of the Spore Spawn Kihunters room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition SporeSpawnKihunters { get; } = new("SporeSpawnKihunters",
    [
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(217, 104)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(217, 104)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(725, 98)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(725, 98)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(983, 104)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(983, 104)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
    ], 3);

    /// <summary>The enemy population of the Etecoon Super room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition EtecoonSuper { get; } = new("EtecoonSuper");

    /// <summary>The enemy population of the Dachora Energy Refill room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition DachoraEnergyRefill { get; } = new("DachoraEnergyRefill");

    /// <summary>The enemy population of the Spore Spawn Farming room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition SporeSpawnFarming { get; } = new("SporeSpawnFarming",
    [
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(384, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(272, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(224, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(128, 66)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: null)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(176, 66)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: null)),
    ], 5);

    /// <summary>The enemy population of the Hopper Energy Tank room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition HopperEnergyTank { get; } = new("HopperEnergyTank");

    /// <summary>The enemy population of the Beta Power Bomb room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BetaPowerBomb { get; } = new("BetaPowerBomb",
    [
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(368, 208)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 54, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(240, 208)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 54, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(240, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(368, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(120, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
    ], 3);

    /// <summary>The enemy population of the Waterway Energy Tank room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WaterwayEnergyTank { get; } = new("WaterwayEnergyTank",
    [
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(320, 174)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 1, AngleStep: RotationAngle.FromSteps(16))),
        new(EnemyDefinitionId.Puyo, EnemyPlacing.At(new(361, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 2, HopDistance: 1)),
        new(EnemyDefinitionId.Puyo, EnemyPlacing.At(new(503, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 2, HopDistance: 1)),
        new(EnemyDefinitionId.Puyo, EnemyPlacing.At(new(548, 181)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new PuyoSpawnSettings(HopCooldown: 2, HopDistance: 1)),
        new(EnemyDefinitionId.Skultera, EnemyPlacing.At(new(587, 174)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new SkulteraSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), StartDirection: HorizontalDirection.Right, Radius: 1, AngleStep: RotationAngle.FromSteps(16))),
        new(EnemyDefinitionId.Zero, EnemyPlacing.At(new(1072, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zero, EnemyPlacing.At(new(1520, 88)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
    ], 7);

    /// <summary>The enemy population of the Construction Zone room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ConstructionZoneWhenZebesAwake { get; } = new("ConstructionZoneWhenZebesAwake",
    [
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(88, 120)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(168, 148)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(72, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(184, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(232, 278)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(232, 358)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(24, 278)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ], 2);

    /// <summary>The enemy population of the Blue Brinstar Energy Tank room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BlueBrinstarEnergyTankWhenZebesAwake { get; } = new("BlueBrinstarEnergyTankWhenZebesAwake",
    [
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(168, 712)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zoomer, EnemyPlacing.At(new(216, 712)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(222, 587)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(254, 571)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(496, 576)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(56, 552)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(56, 680)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(744, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(616, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(648, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ], 5);

    /// <summary>The enemy population of the Noob Bridge room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition NoobBridge { get; } = new("NoobBridge",
    [
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(192, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 64, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(432, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 64, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(1392, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Right, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 64, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(976, 179)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 64, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(584, 118)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(680, 118)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(768, 118)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(872, 118)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnCeiling, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
    ], 8);

    /// <summary>The enemy population of the Morph Ball room once Zebes is awake.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MorphBallWhenZebesAwake { get; } = new("MorphBallWhenZebesAwake",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(1408, 706)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(152, 678)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(380, 678)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(263, 620)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1352, 576)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1464, 576)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1160, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1064, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ], 3);

    /// <summary>The enemy population of the Warehouse Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition WarehouseSave { get; } = new("WarehouseSave");

    /// <summary>The enemy population of the Morph Ball room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MorphBallDefault { get; } = new("MorphBallDefault",
    [
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(1032, 584)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeMountSpawnSettings(Side: MorphBallEyeMountSide.Left)),
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(1032, 584)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeBodySpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(1408, 706)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1352, 576)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 64, Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1464, 576)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 64, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1160, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 64, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(1064, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 64, Facing: HorizontalDirection.Right)),
    ], 0);

    /// <summary>The enemy population of the Warehouse Zeela room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WarehouseZeela { get; } = new("WarehouseZeela",
    [
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(106, 64)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(106, 104)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(43, 232)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
    ], 3);

    /// <summary>The enemy population of the Red Tower room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition RedTower { get; } = new("RedTower",
    [
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(152, 520)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(96, 232)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(152, 320)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(104, 416)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(207, 1624)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(152, 2376)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(95, 2280)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(157, 2184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Ripper, EnemyPlacing.At(new(104, 2048)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromWholePixels(1), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(24, 1328)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Right, Speed: SignedSixteenSixteen.FromWholePixels(2))),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(200, 1360)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromWholePixels(2))),
    ], 11);

    /// <summary>The enemy population of the Blue Brinstar Boulders room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BlueBrinstarBoulders { get; } = new("BlueBrinstarBoulders",
    [
        new(EnemyDefinitionId.Boulder, EnemyPlacing.At(new(344, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 128, TriggerReach: 80, Start: new BoulderStart.FallsFrom(160), Roll: HorizontalDirection.Left, FirstImpact: BoulderImpact.Breaks)),
        new(EnemyDefinitionId.Boulder, EnemyPlacing.At(new(240, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 128, TriggerReach: 80, Start: new BoulderStart.FallsFrom(160), Roll: HorizontalDirection.Left, FirstImpact: BoulderImpact.Breaks)),
        new(EnemyDefinitionId.Boulder, EnemyPlacing.At(new(144, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoulderSpawnSettings(TriggerHeight: 128, TriggerReach: 80, Start: new BoulderStart.FallsFrom(160), Roll: HorizontalDirection.Left, FirstImpact: BoulderImpact.Breaks)),
    ], 0);

    /// <summary>The enemy population of the Billy Mays room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition BillyMays { get; } = new("BillyMays");

    /// <summary>The enemy population of the Big Pink Save Room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition BigPinkSaveRoom { get; } = new("BigPinkSaveRoom");

    /// <summary>The enemy population of the Big Pink room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BigPink { get; } = new("BigPink",
    [
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(744, 1144)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(808, 1144)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(821, 1449)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(569, 252)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(760, 237)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(592, 1048)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(688, 1048)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(704, 152)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(688, 552)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(800, 1353)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
    ], 10);

    /// <summary>The enemy population of the Etecoon Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition EtecoonSave { get; } = new("EtecoonSave");

    /// <summary>The enemy population of the Below Spazer room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BelowSpazer { get; } = new("BelowSpazer",
    [
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(400, 456)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 48, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(128, 456)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 48, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(264, 403)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.FromPixelFraction(1, 8))),
    ], 1);

    /// <summary>The enemy population of the Green Brinstar Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition GreenBrinstarSave { get; } = new("GreenBrinstarSave");

    /// <summary>The enemy population of the Spazer room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition Spazer { get; } = new("Spazer");

    /// <summary>The enemy population of the Pink Brinstar Power Bombs room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PinkBrinstarPowerBombs { get; } = new("PinkBrinstarPowerBombs",
    [
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(56, 400)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.RepeatableReaction, TravelDistance: 96, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(408, 496)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.RepeatableReaction, TravelDistance: 64, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(241, 166)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(104, 100)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
    ], 2);

    /// <summary>The enemy population of the Red Tower Refill room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition RedTowerRefill { get; } = new("RedTowerRefill");

    /// <summary>The enemy population of the Kraid Refill room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition KraidRefill { get; } = new("KraidRefill");

    /// <summary>The enemy population of the Varia Suit room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition VariaSuit { get; } = new("VariaSuit");

    /// <summary>The enemy population of the Red Brinstar Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition RedBrinstarSave { get; } = new("RedBrinstarSave");

    /// <summary>The enemy population of the Blue Brinstar Energy Tank room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BlueBrinstarEnergyTankDefault { get; } = new("BlueBrinstarEnergyTankDefault",
    [
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(552, 616)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeMountSpawnSettings(Side: MorphBallEyeMountSide.Right)),
        new(EnemyDefinitionId.MorphBallEye, EnemyPlacing.At(new(552, 616)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new MorphBallEyeBodySpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(56, 552)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(56, 680)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(744, 696)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(616, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(648, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Green Brinstar Firefleas room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenBrinstarFirefleas { get; } = new("GreenBrinstarFirefleas",
    [
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(384, 95)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-1)), Radius: 24)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(225, 106)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(1)), Radius: 24)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(69, 91)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-1)), Radius: 24)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(112, 280)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Bobbing(SignedSixteenSixteen.FromPixelFraction(1, 2), VerticalDirection.Up), Radius: 32)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(160, 312)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Bobbing(SignedSixteenSixteen.FromPixelFraction(1, 2), VerticalDirection.Up), Radius: 40)),
    ], 5);

    /// <summary>The enemy population of the Green Brinstar Beetoms room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenBrinstarBeetoms { get; } = new("GreenBrinstarBeetoms",
    [
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(80, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(112, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(144, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(176, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Etecoon Energy Tank room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition EtecoonEnergyTank { get; } = new("EtecoonEnergyTank",
    [
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(592, 472)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(752, 472)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(1072, 472)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(472, 408)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(632, 408)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(864, 424)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(984, 424)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(1112, 424)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 8);

    /// <summary>The enemy population of the Pink Brinstar Hoppers room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition PinkBrinstarHoppers { get; } = new("PinkBrinstarHoppers",
    [
        new(EnemyDefinitionId.SidehopperLarge, EnemyPlacing.At(new(181, 437)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(248, 416)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(106, 449)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(344, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.OneShotReaction, TravelDistance: 64, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(344, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.OneShotReaction, TravelDistance: 128, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(344, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.OneShotReaction, TravelDistance: 192, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(344, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.OneShotReaction, TravelDistance: 240, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(328, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.Zero, PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(0), TravelDistance: 0, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(360, 480)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.Zero, PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(0), TravelDistance: 0, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.For(0))),
    ], 3);

    /// <summary>The enemy population of the Warehouse Energy Tank room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WarehouseEnergyTank { get; } = new("WarehouseEnergyTank",
    [
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(45, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(71, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(139, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Beetom, EnemyPlacing.At(new(96, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Warehouse Entrance room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WarehouseEntrance { get; } = new("WarehouseEntrance",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 160)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Down, ArrivalY: 320)),
    ], 0);

    /// <summary>The enemy population of the Warehouse Kihunter room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition WarehouseKihunter { get; } = new("WarehouseKihunter",
    [
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(361, 112)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(361, 112)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(649, 89)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(649, 89)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(510, 99)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(510, 99)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
        new(EnemyDefinitionId.KihunterGreen, EnemyPlacing.At(new(578, 122)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterSpawnSettings(Wings: KiHunterWings.Winged)),
        new(EnemyDefinitionId.KihunterGreenWings, EnemyPlacing.At(new(578, 122)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new KiHunterWingsSpawnSettings(DetachHealth: 32)),
    ], 4);

    /// <summary>The enemy population of the Green Brinstar Main Shaft room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenBrinstarMainShaft { get; } = new("GreenBrinstarMainShaft",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 706)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(218, 715)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(112, 920)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.JetRipper, EnemyPlacing.At(new(108, 1014)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(3, 2), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(121, 1768)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.JetRipper, EnemyPlacing.At(new(67, 1352)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new RipperSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(3, 2), Direction: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(123, 1528)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(39, 972)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 2))),
        new(EnemyDefinitionId.Etecoon, EnemyPlacing.At(new(607, 2968)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen, EnemyPresence.None, false, new EtecoonSpawnSettings(Route: EtecoonRoute.RunLeftToTeachingStart)),
        new(EnemyDefinitionId.Etecoon, EnemyPlacing.At(new(623, 2968)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen, EnemyPresence.None, false, new EtecoonSpawnSettings(Route: EtecoonRoute.RunRightToTeachingStart)),
        new(EnemyDefinitionId.Etecoon, EnemyPlacing.At(new(639, 2968)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen, EnemyPresence.None, false, new EtecoonSpawnSettings(Route: EtecoonRoute.TeachAtOnce)),
    ], 7);

    /// <summary>The enemy population of the Spore Spawn Super room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition SporeSpawnSuper { get; } = new("SporeSpawnSuper",
    [
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(192, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
    ], 1);

    /// <summary>The enemy population of the Early Supers room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition EarlySupers { get; } = new("EarlySupers",
    [
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(442, 424)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(512, 424)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.ShutterGrowing, EnemyPlacing.At(new(408, 296)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrowingShutterSpawnSettings(Wait: new GrowingShutterWait.SamusNear(144), Growth: VerticalDirection.Down, GrowthSpeed: SignedSixteenSixteen.FromPixelFraction(17, 16))),
        new(EnemyDefinitionId.ShutterGrowing, EnemyPlacing.At(new(552, 296)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrowingShutterSpawnSettings(Wait: new GrowingShutterWait.SamusNear(144), Growth: VerticalDirection.Down, GrowthSpeed: SignedSixteenSixteen.FromPixelFraction(17, 16))),
        new(EnemyDefinitionId.ShutterGrowing, EnemyPlacing.At(new(648, 296)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new GrowingShutterSpawnSettings(Wait: new GrowingShutterWait.SamusNear(144), Growth: VerticalDirection.Down, GrowthSpeed: SignedSixteenSixteen.FromPixelFraction(17, 16))),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(171, 75)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(264, 107)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(529, 101)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(321, 86)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(144, 440)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(322, 133)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(451, 132)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(387, 61)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
    ], 10);

    /// <summary>The enemy population of the Red Brinstar Firefleas room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition RedBrinstarFirefleas { get; } = new("RedBrinstarFirefleas",
    [
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(1744, 144)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Bobbing(SignedSixteenSixteen.FromPixelFraction(3, 8), VerticalDirection.Up), Radius: 32)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(1808, 112)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromStepFraction(-3, 2)), Radius: 48)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(1672, 96)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromStepFraction(3, 2)), Radius: 40)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(1248, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(-1)), Radius: 24)),
        new(EnemyDefinitionId.Fireflea, EnemyPlacing.At(new(1584, 128)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new FirefleaSpawnSettings(Flight: new FirefleaFlight.Circling(AngularSpeed.FromSteps(2)), Radius: 24)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(1861, 80)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(1680, 96)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.Waver, EnemyPlacing.At(new(1536, 96)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new WaverSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(408, 200)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 80, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(600, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 80, Attack: VerticalDirection.Up)),
        new(EnemyDefinitionId.YappingMaw, EnemyPlacing.At(new(936, 192)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new YappingMawSpawnSettings(ActivationDistance: 80, Attack: VerticalDirection.Up)),
    ], 8);

    /// <summary>The enemy population of the Construction Zone room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition ConstructionZoneDefault { get; } = new("ConstructionZoneDefault",
    [
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(46, 192)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(138), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(222, 179)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(219, 183)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(248), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(217, 177)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(236), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(215, 193)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(244), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(179, 467)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(196), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(185, 475)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(208), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(187, 490)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(220), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(174, 476)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(187), ActivationRadius: 80, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(213, 496)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(3), ActivationRadius: 128, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(72, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(184, 136)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(232, 278)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(232, 358)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.FaceBlock, EnemyPlacing.At(new(24, 278)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new FaceBlockSpawnSettings(TriggerDistance: 0, Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Green Hill Zone room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition GreenHillZone { get; } = new("GreenHillZone",
    [
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(1024, 672)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(1280, 768)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(1671, 950)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(368, 288)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromPixelFraction(9, 4))),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(560, 480)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Right, Speed: SignedSixteenSixteen.FromPixelFraction(9, 4))),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(896, 544)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromPixelFraction(9, 4))),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(1376, 736)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Left, Speed: SignedSixteenSixteen.FromPixelFraction(9, 4))),
        new(EnemyDefinitionId.Geega, EnemyPlacing.At(new(1280, 832)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new YellowPipeBugSpawnSettings(Facing: HorizontalDirection.Right, Speed: SignedSixteenSixteen.FromPixelFraction(9, 4))),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(1744, 848)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.Sidehopper, EnemyPlacing.At(new(1816, 848)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
    ], 10);

    /// <summary>The enemy population of the Dachora room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Dachora { get; } = new("Dachora",
    [
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(224, 184)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(264, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnFloor, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Metaree, EnemyPlacing.At(new(1581, 108)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Metaree, EnemyPlacing.At(new(1624, 76)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Metaree, EnemyPlacing.At(new(1651, 76)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(304, 96)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(432, 96)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Rio, EnemyPlacing.At(new(744, 96)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        .. DachoraTrail.Placed(new(EnemyDefinitionId.Dachora, EnemyPlacing.At(new(96, 1704)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen, EnemyPresence.None, false, new DachoraSpawnSettings(Facing: HorizontalDirection.Right, Role: DachoraRole.Runner))),
    ], 8);

    /// <summary>The enemy population of the Hellway room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Hellway { get; } = new("Hellway",
    [
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(160, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(295, 136)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(368, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zeela, EnemyPlacing.At(new(447, 152)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(3, 4))),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(416, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(256, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(480, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
        new(EnemyDefinitionId.Zebbo, EnemyPlacing.At(new(128, 216)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Strong)),
    ], 8);

    /// <summary>The enemy population of the First Missile room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition FirstMissile { get; } = new("FirstMissile");

    /// <summary>The enemy population of the Kraid room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Kraid { get; } = new("Kraid",
    [
        new(EnemyDefinitionId.Kraid, EnemyPlacing.At(new(256, 536)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen, EnemyPresence.Hidden, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidArm, EnemyPlacing.At(new(232, 488)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidLintTop, EnemyPlacing.At(new(200, 528)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidLintMiddle, EnemyPlacing.At(new(176, 592)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidLintBottom, EnemyPlacing.At(new(178, 648)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidFoot, EnemyPlacing.At(new(256, 632)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidFirstNail, EnemyPlacing.At(new(232, 488)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.KraidSecondNail, EnemyPlacing.At(new(232, 488)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Brinstar Reserve Tank room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition BrinstarReserveTank { get; } = new("BrinstarReserveTank");

    /// <summary>The enemy population of the X-Ray Scope room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition XrayScope { get; } = new("XrayScope",
    [
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(360, 160)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.RepeatableReaction, TravelDistance: 64, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterShootable, EnemyPlacing.At(new(184, 224)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: ShutterTrigger.RepeatableReaction, TravelDistance: 64, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.Permanent)),
    ], 0);

    /// <summary>The enemy population of the Alpha Power Bomb room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition AlphaPowerBomb { get; } = new("AlphaPowerBomb",
    [
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(600, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 48, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(616, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 48, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(504, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 48, TriggerDistance: 32)),
        new(EnemyDefinitionId.Boyon, EnemyPlacing.At(new(520, 168)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new BoyonSpawnSettings(SpeedMultiplier: 8, JumpHeight: 48, TriggerDistance: 32)),
    ], 0);

    /// <summary>The enemy population of the Kraid Eye Door room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition KraidEyeDoor { get; } = new("KraidEyeDoor",
    [
        new(EnemyDefinitionId.Zeb, EnemyPlacing.At(new(144, 456)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.Hidden | EnemyPresence.RespawnsIfKilled, false, new BrinstarPipeBugSpawnSettings(Kind: BrinstarPipeBugKind.Weak)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(310, 413)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(311, 420)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(186), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(317, 427)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(168), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(307, 428)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(180), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(315, 414)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(202), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(322, 420)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(201), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(325, 423)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(190), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(323, 431)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(194), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(311, 432)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 3, InitialAngle: RotationAngle.FromSteps(144), ActivationRadius: 48, Behavior: SbugActivationBehavior.MoveForward)),
        new(EnemyDefinitionId.Sbug, EnemyPlacing.At(new(334, 428)), EnemyContact.IgnoresSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new SbugSpawnSettings(Speed: 2, InitialAngle: RotationAngle.FromSteps(200), ActivationRadius: 32, Behavior: SbugActivationBehavior.MoveForward)),
    ], 1);

    /// <summary>The enemy population of the Caterpillar room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Caterpillar { get; } = new("Caterpillar",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 706)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Up, ArrivalY: 24)),
        new(EnemyDefinitionId.Zero, EnemyPlacing.At(new(95, 1928)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnFloor, BlockSide: CrawlerSurfaceOrientation.OnRightWall, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zero, EnemyPlacing.At(new(23, 1758)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnLeftWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Zero, EnemyPlacing.At(new(120, 1679)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CrawlerSpawnSettings(Orientation: CrawlerSurfaceOrientation.OnRightWall, BlockSide: CrawlerSurfaceOrientation.OnCeiling, CrawlSpeed: SignedEightEight.FromPixelFraction(1, 4))),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(113, 1139)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Right, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
        new(EnemyDefinitionId.Cacatac, EnemyPlacing.At(new(192, 1283)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CacatacSpawnSettings(Direction: HorizontalDirection.Left, Start: CacatacAction.Patrolling, Mounting: CacatacMounting.Floor, TravelDistance: 16, Speed: SignedSixteenSixteen.FromPixelFraction(3, 16))),
    ], 5);

    /// <summary>The enemy population of the Mini Kraid room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MiniKraid { get; } = new("MiniKraid",
    [
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(217, 160)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 80)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(288, 160)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 80)),
        new(EnemyDefinitionId.PirateGreenWalking, EnemyPlacing.At(new(500, 160)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Slow, ShotWatch: WalkingSpacePirateShotWatch.FlinchesFromShots, PatrolReach: 80)),
        new(EnemyDefinitionId.FakeKraid, EnemyPlacing.At(new(1328, 160)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 4);

    /// <summary>The enemy population of the Spore Spawn room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition SporeSpawn { get; } = new("SporeSpawn",
    [
        new(EnemyDefinitionId.SporeSpawn, EnemyPlacing.At(new(128, 624)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Skree Boost room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition SkreeBoost { get; } = new("SkreeBoost",
    [
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(328, 60)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Skree, EnemyPlacing.At(new(368, 60)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 2);

    /// <summary>The Brinstar rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        BrinstarPreMap,
        GreenBrinstarMap,
        GreenBrinstarMissileRefill,
        SporeSpawnKihunters,
        EtecoonSuper,
        DachoraEnergyRefill,
        SporeSpawnFarming,
        HopperEnergyTank,
        BetaPowerBomb,
        WaterwayEnergyTank,
        ConstructionZoneWhenZebesAwake,
        BlueBrinstarEnergyTankWhenZebesAwake,
        NoobBridge,
        MorphBallWhenZebesAwake,
        WarehouseSave,
        MorphBallDefault,
        WarehouseZeela,
        RedTower,
        BlueBrinstarBoulders,
        BillyMays,
        BigPinkSaveRoom,
        BigPink,
        EtecoonSave,
        BelowSpazer,
        GreenBrinstarSave,
        Spazer,
        PinkBrinstarPowerBombs,
        RedTowerRefill,
        KraidRefill,
        VariaSuit,
        RedBrinstarSave,
        BlueBrinstarEnergyTankDefault,
        GreenBrinstarFirefleas,
        GreenBrinstarBeetoms,
        EtecoonEnergyTank,
        PinkBrinstarHoppers,
        WarehouseEnergyTank,
        WarehouseEntrance,
        WarehouseKihunter,
        GreenBrinstarMainShaft,
        SporeSpawnSuper,
        EarlySupers,
        RedBrinstarFirefleas,
        ConstructionZoneDefault,
        GreenHillZone,
        Dachora,
        Hellway,
        FirstMissile,
        Kraid,
        BrinstarReserveTank,
        XrayScope,
        AlphaPowerBomb,
        KraidEyeDoor,
        Caterpillar,
        MiniKraid,
        SporeSpawn,
        SkreeBoost,
    ];
}
