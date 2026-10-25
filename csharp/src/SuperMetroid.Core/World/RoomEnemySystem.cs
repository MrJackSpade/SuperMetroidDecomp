using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Inert;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Grapple;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room-enemy loader, scheduler, instruction interpreter, and draw queues. Retail
/// gameplay definitions are compiled; installed artwork supplies pixels and colors.
/// </summary>
/// <remarks>
/// This deliberately retains the cartridge's 32 fixed slots and its native slot offsets
/// ($0000, $0040, ... $07C0). Those offsets are data: enemy AI uses them for multi-part
/// actors, collision lists publish them, and the gunship bottom initializer even relies on
/// WRAM-array aliasing to copy fields from earlier slots. A compact host list would appear
/// cleaner while destroying behavior that the original code expects.
/// </remarks>
[RoomLifetime]
public sealed class RoomEnemySystem : IEnemyProjectileHandlerSource, IEnemyFrameResults, IEnemyDisplaySources
{
    /// <summary>The <see cref="EnemyDrawList"/> owner of this RoomEnemySystem.</summary>
    internal EnemyDrawList Drawing { get; }

    /// <summary>The <see cref="EnemyInstructionInterpreter"/> owner of this RoomEnemySystem.</summary>
    internal EnemyInstructionInterpreter Instructions { get; }

    /// <summary>The <see cref="EnemyPopulationLoader"/> owner of this RoomEnemySystem.</summary>
    internal EnemyPopulationLoader PopulationLoader { get; }

    /// <summary>The <see cref="EnemyProcessingWindow"/> owner of this RoomEnemySystem.</summary>
    internal EnemyProcessingWindow ProcessingWindow { get; }

    /// <summary>The room's bank-$B4 enemy graphics set.</summary>
    internal EnemyGraphicsSet GraphicsSet { get; }

    /// <summary>Puts populations into slots and runs their banked AI.</summary>
    internal EnemySlotDispatch Dispatch { get; }

    /// <summary>The <see cref="GunshipBehavior"/> owner of this RoomEnemySystem.</summary>
    internal GunshipBehavior GunshipBehavior { get; }

    /// <summary>The <see cref="SamusScrollCap"/> owner of this RoomEnemySystem.</summary>
    internal SamusScrollCap ScrollCap { get; }

    /// <summary>The <see cref="EnemyRoomFx"/> owner of this RoomEnemySystem.</summary>
    internal EnemyRoomFx RoomFx { get; }

    /// <summary>The <see cref="EnemyRoomStatus"/> owner of this RoomEnemySystem.</summary>
    internal EnemyRoomStatus RoomStatus { get; }

    /// <summary>The <see cref="RidleyProjectiles"/> owner of this RoomEnemySystem.</summary>
    internal RidleyProjectiles RidleyProjectiles { get; }

    /// <summary>The <see cref="RidleyEncounter"/> owner of this RoomEnemySystem.</summary>
    internal RidleyEncounter RidleyEncounter { get; }
    /// <summary>Coordinates Ridley's multipart tail through the shared slot and projectile services.</summary>
    internal RidleyTail RidleyTail { get; }
    /// <summary>Publishes dead-monster room effects through the same frame as enemy processing.</summary>
    internal DeadMonsterFrame DeadMonsters { get; }
    /// <summary>Owns Mother Brain's room encounter and its cutscene state.</summary>
    internal MotherBrainEncounter MotherBrainEncounter { get; }

    /// <summary>The <see cref="EnemyCollision"/> owner of this RoomEnemySystem.</summary>
    internal EnemyCollision Collision { get; }

    /// <summary>The <see cref="EnemyDeathsAndDrops"/> owner of this RoomEnemySystem.</summary>
    internal EnemyDeathsAndDrops DeathsAndDrops { get; }

    /// <summary>The <see cref="EnemyFrameContext"/> owner of this RoomEnemySystem.</summary>
    internal EnemyFrameContext FrameContext { get; }

    /// <summary>The <see cref="EnemyPresentation"/> owner of this RoomEnemySystem.</summary>
    internal EnemyPresentation Presentation { get; }

    /// <summary>The <see cref="EnemySlotTable"/> owner of this RoomEnemySystem.</summary>
    internal EnemySlotTable SlotTable { get; }

    /// <summary>The <see cref="ZoaBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ZoaBehavior ZoaBehavior { get; }

    /// <summary>The <see cref="ZebetiteBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ZebetiteBehavior ZebetiteBehavior { get; }

    /// <summary>The <see cref="YardEnemyBehavior"/> owner of this RoomEnemySystem.</summary>
    internal YardEnemyBehavior YardBehavior { get; }

    /// <summary>The <see cref="YappingMawBehavior"/> owner of this RoomEnemySystem.</summary>
    internal YappingMawBehavior YappingMawBehavior { get; }

    /// <summary>The <see cref="WreckedShipGhostBehavior"/> owner of this RoomEnemySystem.</summary>
    internal WreckedShipGhostBehavior WreckedShipGhostBehavior { get; }

    /// <summary>The <see cref="WorkRobotBehavior"/> owner of this RoomEnemySystem.</summary>
    internal WorkRobotBehavior WorkRobotBehavior { get; }

    /// <summary>The <see cref="WaverBehavior"/> owner of this RoomEnemySystem.</summary>
    internal WaverBehavior WaverBehavior { get; }

    /// <summary>The <see cref="WallSpacePirateBehavior"/> owner of this RoomEnemySystem.</summary>
    internal WallSpacePirateBehavior WallSpacePirateBehavior { get; }

    /// <summary>The <see cref="WalkingSpacePirateBehavior"/> owner of this RoomEnemySystem.</summary>
    internal WalkingSpacePirateBehavior WalkingSpacePirateBehavior { get; }

    /// <summary>The <see cref="TourianUnlockBehavior"/> owner of this RoomEnemySystem.</summary>
    internal TourianUnlockBehavior TourianUnlockBehavior { get; }

    /// <summary>The <see cref="TourianEntranceStatueBehavior"/> owner of this RoomEnemySystem.</summary>
    internal TourianEntranceStatueBehavior TourianEntranceStatueBehavior { get; }

    /// <summary>The <see cref="StokeBehavior"/> owner of this RoomEnemySystem.</summary>
    internal StokeBehavior StokeBehavior { get; }

    /// <summary>The <see cref="SporeSpawnBehavior"/> owner of this RoomEnemySystem.</summary>
    internal SporeSpawnBehavior SporeSpawnBehavior { get; }

    /// <summary>The <see cref="SparkBehavior"/> owner of this RoomEnemySystem.</summary>
    internal SparkBehavior SparkBehavior { get; }

    /// <summary>The <see cref="SkulteraBehavior"/> owner of this RoomEnemySystem.</summary>
    internal SkulteraBehavior SkulteraBehavior { get; }

    /// <summary>The <see cref="SkreeBehavior"/> owner of this RoomEnemySystem.</summary>
    internal SkreeBehavior SkreeBehavior { get; }

    /// <summary>The <see cref="ShuttersBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ShuttersBehavior ShuttersBehavior { get; }

    /// <summary>The <see cref="ShitroidBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ShitroidBehavior ShitroidBehavior { get; }

    /// <summary>The <see cref="ShaktoolBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ShaktoolBehavior ShaktoolBehavior { get; }

    /// <summary>The <see cref="SbugBehavior"/> owner of this RoomEnemySystem.</summary>
    internal SbugBehavior SbugBehavior { get; }

    /// <summary>The <see cref="RipperBehavior"/> owner of this RoomEnemySystem.</summary>
    internal RipperBehavior RipperBehavior { get; }

    /// <summary>The <see cref="RioBehavior"/> owner of this RoomEnemySystem.</summary>
    internal RioBehavior RioBehavior { get; }

    /// <summary>The <see cref="RinkaBehavior"/> owner of this RoomEnemySystem.</summary>
    internal RinkaBehavior RinkaBehavior { get; }

    /// <summary>The <see cref="RidleyBehavior"/> owner of this RoomEnemySystem.</summary>
    internal RidleyBehavior RidleyBehavior { get; }

    /// <summary>The <see cref="PuyoBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PuyoBehavior PuyoBehavior { get; }

    /// <summary>The <see cref="PrePhantoonRoomBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PrePhantoonRoomBehavior PrePhantoonRoomBehavior { get; }

    /// <summary>The <see cref="PowampBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PowampBehavior PowampBehavior { get; }

    /// <summary>The <see cref="PolypBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PolypBehavior PolypBehavior { get; }

    /// <summary>The <see cref="PlatformsBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PlatformsBehavior PlatformsBehavior { get; }

    /// <summary>The <see cref="PipeBugsBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PipeBugsBehavior PipeBugsBehavior { get; }

    /// <summary>The <see cref="PhantoonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal PhantoonBehavior PhantoonBehavior { get; }

    /// <summary>The <see cref="OwtchBehavior"/> owner of this RoomEnemySystem.</summary>
    internal OwtchBehavior OwtchBehavior { get; }

    /// <summary>The <see cref="OldTourianEscapeShaftWallBehavior"/> owner of this RoomEnemySystem.</summary>
    internal OldTourianEscapeShaftWallBehavior OldTourianEscapeShaftWallBehavior { get; }

    /// <summary>The <see cref="NuclearWaffleBehavior"/> owner of this RoomEnemySystem.</summary>
    internal NuclearWaffleBehavior NuclearWaffleBehavior { get; }

    /// <summary>The <see cref="NorfairRioBehavior"/> owner of this RoomEnemySystem.</summary>
    internal NorfairRioBehavior NorfairRioBehavior { get; }

    /// <summary>The <see cref="NorfairLavaJumpingEnemyBehavior"/> owner of this RoomEnemySystem.</summary>
    internal NorfairLavaJumpingEnemyBehavior NorfairLavaJumpingEnemyBehavior { get; }

    /// <summary>The <see cref="NoobTubeBehavior"/> owner of this RoomEnemySystem.</summary>
    internal NoobTubeBehavior NoobTubeBehavior { get; }

    /// <summary>The <see cref="NinjaSpacePirateBehavior"/> owner of this RoomEnemySystem.</summary>
    internal NinjaSpacePirateBehavior NinjaSpacePirateBehavior { get; }

    /// <summary>The <see cref="MultiviolaBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MultiviolaBehavior MultiviolaBehavior { get; }

    /// <summary>The <see cref="MotherBrainBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MotherBrainBehavior MotherBrainBehavior { get; }

    /// <summary>The <see cref="MorphBallEyeBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MorphBallEyeBehavior MorphBallEyeBehavior { get; }

    /// <summary>The <see cref="MochtroidBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MochtroidBehavior MochtroidBehavior { get; }

    /// <summary>The <see cref="MetroidBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MetroidBehavior MetroidBehavior { get; }

    /// <summary>The <see cref="MetareeBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MetareeBehavior MetareeBehavior { get; }

    /// <summary>The <see cref="MaridiaLargeSnailBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MaridiaLargeSnailBehavior MaridiaLargeSnailBehavior { get; }

    /// <summary>The <see cref="MamaTurtleBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MamaTurtleBehavior MamaTurtleBehavior { get; }

    /// <summary>The <see cref="MagdolliteBehavior"/> owner of this RoomEnemySystem.</summary>
    internal MagdolliteBehavior MagdolliteBehavior { get; }

    /// <summary>The <see cref="LowerNorfairRioBehavior"/> owner of this RoomEnemySystem.</summary>
    internal LowerNorfairRioBehavior LowerNorfairRioBehavior { get; }

    /// <summary>The <see cref="KzanBehavior"/> owner of this RoomEnemySystem.</summary>
    internal KzanBehavior KzanBehavior { get; }

    /// <summary>The <see cref="KraidBehavior"/> owner of this RoomEnemySystem.</summary>
    internal KraidBehavior KraidBehavior { get; }

    /// <summary>The <see cref="KiHunterBehavior"/> owner of this RoomEnemySystem.</summary>
    internal KiHunterBehavior KiHunterBehavior { get; }

    /// <summary>The <see cref="KagoBehavior"/> owner of this RoomEnemySystem.</summary>
    internal KagoBehavior KagoBehavior { get; }

    /// <summary>The <see cref="HoppersBehavior"/> owner of this RoomEnemySystem.</summary>
    internal HoppersBehavior HoppersBehavior { get; }

    /// <summary>The <see cref="HibashiBehavior"/> owner of this RoomEnemySystem.</summary>
    internal HibashiBehavior HibashiBehavior { get; }

    /// <summary>The <see cref="GrappleBehavior"/> owner of this RoomEnemySystem.</summary>
    internal GrappleBehavior GrappleBehavior { get; }

    /// <summary>The <see cref="FuneNamiheBehavior"/> owner of this RoomEnemySystem.</summary>
    internal FuneNamiheBehavior FuneNamiheBehavior { get; }

    /// <summary>The <see cref="FliesBehavior"/> owner of this RoomEnemySystem.</summary>
    internal FliesBehavior FliesBehavior { get; }

    /// <summary>The <see cref="FirefleaBehavior"/> owner of this RoomEnemySystem.</summary>
    internal FirefleaBehavior FirefleaBehavior { get; }

    /// <summary>The <see cref="FallingSparksBehavior"/> owner of this RoomEnemySystem.</summary>
    internal FallingSparksBehavior FallingSparksBehavior { get; }

    /// <summary>The <see cref="FakeKraidBehavior"/> owner of this RoomEnemySystem.</summary>
    internal FakeKraidBehavior FakeKraidBehavior { get; }

    /// <summary>The <see cref="EyeDoorBehavior"/> owner of this RoomEnemySystem.</summary>
    internal EyeDoorBehavior EyeDoorBehavior { get; }

    /// <summary>The <see cref="EvirBehavior"/> owner of this RoomEnemySystem.</summary>
    internal EvirBehavior EvirBehavior { get; }

    /// <summary>The <see cref="EtecoonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal EtecoonBehavior EtecoonBehavior { get; }

    /// <summary>The <see cref="EscapeExplosionsBehavior"/> owner of this RoomEnemySystem.</summary>
    internal EscapeExplosionsBehavior EscapeExplosionsBehavior { get; }

    /// <summary>The <see cref="EscapeAnimalsBehavior"/> owner of this RoomEnemySystem.</summary>
    internal EscapeAnimalsBehavior EscapeAnimalsBehavior { get; }

    /// <summary>The <see cref="ElevatorBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ElevatorBehavior ElevatorBehavior { get; }

    /// <summary>The <see cref="DraygonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DraygonBehavior DraygonBehavior { get; }

    /// <summary>The <see cref="DragonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DragonBehavior DragonBehavior { get; }

    /// <summary>The <see cref="DownwardGateBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DownwardGateBehavior DownwardGateBehavior { get; }

    /// <summary>The <see cref="DeadTourianBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DeadTourianBehavior DeadTourianBehavior { get; }

    /// <summary>The <see cref="DeadTorizoBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DeadTorizoBehavior DeadTorizoBehavior { get; }

    /// <summary>The <see cref="DeadSidehopperBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DeadSidehopperBehavior DeadSidehopperBehavior { get; }

    /// <summary>The <see cref="DachoraBehavior"/> owner of this RoomEnemySystem.</summary>
    internal DachoraBehavior DachoraBehavior { get; }

    /// <summary>The <see cref="CrocomireBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CrocomireBehavior CrocomireBehavior { get; }

    /// <summary>The <see cref="CrawlersBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CrawlersBehavior CrawlersBehavior { get; }

    /// <summary>The <see cref="ChozoStatueBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ChozoStatueBehavior ChozoStatueBehavior { get; }

    /// <summary>The <see cref="ChootBehavior"/> owner of this RoomEnemySystem.</summary>
    internal ChootBehavior ChootBehavior { get; }

    /// <summary>The <see cref="CeresSteamBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CeresSteamBehavior CeresSteamBehavior { get; }

    /// <summary>The <see cref="CeresRidleyBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CeresRidleyBehavior CeresRidleyBehavior { get; }

    /// <summary>The <see cref="CeresDoorBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CeresDoorBehavior CeresDoorBehavior { get; }

    /// <summary>The <see cref="CacatacBehavior"/> owner of this RoomEnemySystem.</summary>
    internal CacatacBehavior CacatacBehavior { get; }

    /// <summary>The <see cref="BullBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BullBehavior BullBehavior { get; }

    /// <summary>The <see cref="BoyonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BoyonBehavior BoyonBehavior { get; }

    /// <summary>The <see cref="BoulderBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BoulderBehavior BoulderBehavior { get; }

    /// <summary>The <see cref="BotwoonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BotwoonBehavior BotwoonBehavior { get; }

    /// <summary>The <see cref="BombTorizoBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BombTorizoBehavior BombTorizoBehavior { get; }

    /// <summary>The <see cref="BlueBrinstarFaceBlockBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BlueBrinstarFaceBlockBehavior BlueBrinstarFaceBlockBehavior { get; }

    /// <summary>The <see cref="BeetomBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BeetomBehavior BeetomBehavior { get; }

    /// <summary>The <see cref="BabyMetroidCutsceneBehavior"/> owner of this RoomEnemySystem.</summary>
    internal BabyMetroidCutsceneBehavior BabyMetroidCutsceneBehavior { get; }

    /// <summary>The <see cref="AtomicBehavior"/> owner of this RoomEnemySystem.</summary>
    internal AtomicBehavior AtomicBehavior { get; }

    /// <summary>The <see cref="AlcoonBehavior"/> owner of this RoomEnemySystem.</summary>
    internal AlcoonBehavior AlcoonBehavior { get; }

    /// <summary>The <see cref="RoomEarthquake"/> owner of this RoomEnemySystem.</summary>
    internal RoomEarthquake Earthquake { get; }

    /// <summary>
    /// The reactions several species share. The lists contain delegates and are rebuilt from their owners on first
    /// access after a debugger restore.
    /// </summary>
    [field: NonSerialized]
    internal EnemySharedReactions SharedReactions => field ??= BuildSharedReactions();

    /// <summary>The shared reactions, for the common participation to resolve at use.</summary>
    private EnemySharedReactions GetSharedReactions() => SharedReactions;

    /// <summary>The species registry that binds each installed occupant's entity.</summary>
    internal EnemySpeciesRegistry Species { get; }

    /// <summary>The loaded room's address space, services and artwork, shared with the composed owners.</summary>
    internal EnemyRoomBinding Room { get; }

    /// <summary>
    /// Rebinds the room's active escape typewriters to the host's current escape-warning text, after the host installs
    /// it (as after a debugger restore).
    /// </summary>
    /// <param name="value">The installed escape-warning text, or null when none is installed.</param>
    internal void RebindEscapeTypewriters(EscapeTypewriterPresentation? value)
    {
        EscapeTypewriterState? active = MotherBrainEncounter.MotherBrain?.EscapeTypewriter;
        if (active?.ProgramId == EscapeTypewriterProgramId.Zebes)
        {
            if (value is null) active.UnbindProgram();
            else active.BindProgram(value.Get(EscapeTypewriterProgramId.Zebes));
        }
        EscapeTypewriterState? ceres = RidleyEncounter.Ridley?.CeresEscapeTypewriter;
        if (ceres?.ProgramId == EscapeTypewriterProgramId.Ceres)
        {
            if (value is null) ceres.UnbindProgram();
            else ceres.BindProgram(value.Get(EscapeTypewriterProgramId.Ceres));
        }
    }

    /// <summary>Species owners visited during room load and each enemy frame.</summary>
    private readonly IEnemySpecies[] _species;

    /// <summary>The <see cref="RoomSpriteObjectPool"/> owner of this RoomEnemySystem.</summary>
    internal RoomSpriteObjectPool SpriteObjects { get; }

    /// <summary>The <see cref="EnemyProjectileEngine"/> owner of this RoomEnemySystem.</summary>
    internal EnemyProjectileEngine Projectiles { get; }

    /// <summary>The <see cref="EnemyAudioQueue"/> owner of this RoomEnemySystem.</summary>
    internal EnemyAudioQueue Audio { get; }

    /// <summary>The <see cref="EnemyTerrain"/> owner of this RoomEnemySystem.</summary>
    internal EnemyTerrain Terrain { get; }

    /// <summary>Post-AI collision snapshots for active physical enemy slots.</summary>
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();

    /// <summary>The elevator journey the runtime reads and resets.</summary>
    internal IElevatorJourney Elevator => ElevatorBehavior;

    /// <summary>The gunship's save prompt and health handler.</summary>
    [field: NonSerialized]
    internal IGunshipSession Gunship => field ??= new GunshipSession(GunshipBehavior);

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    [field: NonSerialized]
    internal ICeresEncounter Ceres => field ??= new CeresEncounter(RidleyEncounter, CeresRidleyBehavior);

    /// <summary>The state and hooks the room main routines and scrolling use.</summary>
    [field: NonSerialized]
    internal IEnemyRoomMainHooks RoomMainHooks => field ??= new EnemyRoomMainHooks(CrocomireBehavior, SporeSpawnBehavior, ZebetiteBehavior);

    /// <summary>The operations the room loader runs.</summary>
    [field: NonSerialized]
    internal IEnemyLoaderHooks LoaderHooks => field ??= new EnemyLoaderHooks(RinkaBehavior, PrePhantoonRoomBehavior);

    /// <summary>The enemy setup the attract demo's room setups perform.</summary>
    [field: NonSerialized]
    internal IEnemyAttractDemoHooks AttractDemoHooks => field ??= new EnemyAttractDemoHooks(KraidBehavior);

    /// <summary>The enemy-side effects room PLMs and room effects trigger.</summary>
    [field: NonSerialized]
    internal IEnemyPlmSpawns PlmSpawns => field ??= new EnemyPlmSpawns(NoobTubeBehavior, EyeDoorBehavior, OldTourianEscapeShaftWallBehavior,
        DraygonBehavior, MotherBrainBehavior, BombTorizoBehavior, EscapeExplosionsBehavior);

    /// <summary>The PLM work the Chozo statues and downward gates exchange with their PLMs each frame.</summary>
    internal IEnemyPlmRequests PlmRequests => field ??= new EnemyPlmRequests(ChozoStatueBehavior, DownwardGateBehavior);

    /// <summary>The Tourian statue sequence's operations.</summary>
    [field: NonSerialized]
    internal ITourianStatueActors TourianStatue => field ??= new TourianStatueActors(TourianEntranceStatueBehavior, TourianUnlockBehavior);

    /// <summary>The live enemy state the display latch and drawing read.</summary>
    internal IEnemyDisplaySources DisplaySources => this;

    /// <inheritdoc/>
    MotherBrainEnemyState? IEnemyDisplaySources.MotherBrain => MotherBrainEncounter.MotherBrain;
    /// <inheritdoc/>
    DraygonEnemyState? IEnemyDisplaySources.Draygon => DraygonBehavior.Parts.Draygon;
    /// <inheritdoc/>
    PhantoonEnemyState? IEnemyDisplaySources.Phantoon => PhantoonBehavior.Combat.Phantoon;
    /// <inheritdoc/>
    RidleyEnemyState? IEnemyDisplaySources.CeresRidley => RidleyEncounter.CeresRidley;
    /// <inheritdoc/>
    BossBg2Layer? IEnemyDisplaySources.KraidBody => KraidBehavior.BodyLayer.CaptureBodyLayer();
    /// <inheritdoc/>
    EnemyBg2ImageSnapshot? IEnemyDisplaySources.EnemyBg2 => Presentation.Bg2Image?.Publish();
    /// <inheritdoc/>
    Position16<Bg2Pixels>? IEnemyDisplaySources.CrocomireBg2Scroll =>
        CrocomireBehavior.Access.Crocomire is not null ? CrocomireBehavior.Bg2.CrocomireBg2Scroll : null;
    /// <inheritdoc/>
    MorphBallEyeBeamState IEnemyDisplaySources.MorphBallEyeBeam => MorphBallEyeBehavior.MorphBallEyeBeam;
    /// <inheritdoc/>
    IReadOnlyList<MorphBallEyeEnemyState?> IEnemyDisplaySources.MorphBallEyeStates => MorphBallEyeBehavior.MorphBallEyeStates;

    /// <summary>Grapple endpoint resolution against the room's enemies.</summary>
    internal IGrappleTargets GrappleTargets => GrappleBehavior;

    /// <summary>The current pass's results, written by their producers and read back until the next pass.</summary>
    internal EnemyFrameResultBuilder FrameResults { get; } = new();

    /// <summary>The bosses' results from the enemy pass.</summary>
    internal BossFrameResultBuilder BossFrameResults { get; } = new();

    /// <inheritdoc/>
    public EnemyFrameResult LastEnemyFrame => BuildFrameResult();

    /// <summary>The enemy pass's results so far, the bosses' included.</summary>
    private EnemyFrameResult BuildFrameResult() => new(FrameResults.Elevator, FrameResults.ElevatorClearedProjectileData,
        BossFrameResults.DraygonDiscardedSamusPoseRequests, BossFrameResults.CeresEscapeStarted, BossFrameResults.LayerOneXOverride,
        BossFrameResults.RemoveLiquidEffects, FrameResults.Gunship, BossFrameResults.RoomEffects());

    /// <summary>
    /// Creates a room's enemy system: all 32 stable physical slot objects, starting from the words the replaced room's
    /// enemies left, with the door-transition gates clear, without loading a population or binding memory, graphics,
    /// random-number, or gameplay services.
    /// </summary>
    /// <param name="session">The enemy words and installed artwork that outlive each room.</param>
    /// <param name="residue">The words the replaced room's enemies leave behind, or null for the session's first room.</param>
    /// <param name="definitions">The room's enemy definition source, or null for an enemy system with no population.</param>
    /// <param name="services">The cartridge-state services the room's AI calls, or null when none is supplied.</param>
    /// <param name="bossArtwork">The session's installed boss artwork, or null for a system with its own, empty binding.</param>
    /// <param name="areaArtwork">The session's installed area artwork, or null for a system with its own, empty binding.</param>
    internal RoomEnemySystem(EnemySession session, EnemyRoomResidue? residue = null,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null,
        BossArtworkBinding? bossArtwork = null, AreaArtworkBinding? areaArtwork = null)
    {
        BossArtworkBinding = bossArtwork ?? new();
        AreaArtworkBinding = areaArtwork ?? new();
        Room = new EnemyRoomBinding(session, definitions, services);
        // Shared collaborators first: every species owner receives the ones it uses.
        ScrollCap = new SamusScrollCap();
        RoomFx = new EnemyRoomFx();
        RoomStatus = new EnemyRoomStatus(session);
        FrameContext = new EnemyFrameContext(session);
        GraphicsSet = new EnemyGraphicsSet(Room);
        Presentation = new EnemyPresentation(Room, GraphicsSet);
        BossPresentation = new BossPresentation(Presentation, BossArtworkBinding);
        SlotTable = new EnemySlotTable();
        MorphBallEyeBehavior = new MorphBallEyeBehavior(SlotTable);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, MorphBallEyeBehavior, Room, SlotTable);
        Dispatch = new EnemySlotDispatch(Room, SlotTable, GraphicsSet, ProcessingWindow);
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, SlotTable, Dispatch);
        RidleyEncounter = new RidleyEncounter(SlotTable, session);
        Earthquake = new RoomEarthquake();
        SpriteObjects = new RoomSpriteObjectPool(Presentation);
        Audio = new EnemyAudioQueue();
        Projectiles = new EnemyProjectileEngine(Room, Presentation, SpriteObjects, Earthquake, Audio);
        Projectiles.AddHandlerSource(this);
        GunshipBehavior = new GunshipBehavior(Audio, Projectiles, Room, SlotTable, FrameResults);
        Terrain = new EnemyTerrain();
        DeadMonsters = new DeadMonsterFrame();
        MotherBrainEncounter = new MotherBrainEncounter(Presentation);
        RidleyTail = new RidleyTail(FrameContext, Projectiles, RidleyEncounter, Room, SlotTable);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, SlotTable, Dispatch);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Dispatch = Dispatch, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, FrameContext = FrameContext, Presentation = Presentation, Projectiles = Projectiles, Room = Room, SlotTable = SlotTable });
        RidleyProjectiles = new RidleyProjectiles(DeathsAndDrops, Projectiles, RidleyEncounter);
        ZoaBehavior = new ZoaBehavior();
        ElevatorBehavior = new ElevatorBehavior(Room, Audio, FrameResults);
        ZebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Dispatch = Dispatch, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, SlotTable = SlotTable, Presentation = Presentation, ElevatorBehavior = ElevatorBehavior, RoomStatus = RoomStatus, Collision = Collision , BossArtwork = BossArtworkBinding });
        YardBehavior = new YardEnemyBehavior(Room, Terrain, Earthquake, Collision);
        YappingMawBehavior = new YappingMawBehavior(Projectiles, SpriteObjects, ScrollCap);
        WreckedShipGhostBehavior = new WreckedShipGhostBehavior(Presentation);
        WorkRobotBehavior = new WorkRobotBehavior(Room, Terrain, Projectiles, Presentation);
        WaverBehavior = new WaverBehavior(Terrain);
        WalkingSpacePirateBehavior = new WalkingSpacePirateBehavior(Terrain, Projectiles);
        WallSpacePirateBehavior = new WallSpacePirateBehavior(Room, Terrain, Collision, DeathsAndDrops, WalkingSpacePirateBehavior);
        TourianUnlockBehavior = new TourianUnlockBehavior(Room, Projectiles, Earthquake, Audio, Presentation, AreaArtworkBinding);
        TourianEntranceStatueBehavior = new TourianEntranceStatueBehavior(Projectiles, Presentation, AreaArtworkBinding);
        StokeBehavior = new StokeBehavior(Room, Terrain, Projectiles);
        SporeSpawnBehavior = new SporeSpawnBehavior(new SporeSpawnBehaviorDependencies { Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults , BossArtwork = BossArtworkBinding });
        FallingSparksBehavior = new FallingSparksBehavior(Room, Projectiles, SpriteObjects);
        SparkBehavior = new SparkBehavior(Room, FallingSparksBehavior);
        SkulteraBehavior = new SkulteraBehavior(Terrain);
        SkreeBehavior = new SkreeBehavior(Audio, Projectiles, DeathsAndDrops);
        ShuttersBehavior = new ShuttersBehavior();
        DeadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = Room, Terrain = Terrain, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Projectiles = Projectiles, Collision = Collision, DeadMonsters = DeadMonsters });
        ShitroidBehavior = new ShitroidBehavior(Room, SlotTable, Presentation, ZebetiteBehavior, BossFrameResults, BossArtworkBinding);
        ShaktoolBehavior = new ShaktoolBehavior(Terrain, Projectiles, SlotTable, Collision);
        SbugBehavior = new SbugBehavior(Room, Terrain);
        RipperBehavior = new RipperBehavior(Terrain);
        RioBehavior = new RioBehavior(Room, Terrain);
        RinkaBehavior = new RinkaBehavior(SlotTable, Collision, FrameContext, Projectiles, DeathsAndDrops);
        CeresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { FrameResults = BossFrameResults, Dispatch = Dispatch, Room = Room, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = BossPresentation, Collision = Collision, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, RoomStatus = RoomStatus , AreaArtwork = AreaArtworkBinding  });
        RidleyBehavior = new RidleyBehavior(new RidleyBehaviorDependencies { Dispatch = Dispatch, Room = Room, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, DeathsAndDrops = DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, CeresRidleyBehavior = CeresRidleyBehavior, RidleyProjectiles = RidleyProjectiles, RoomStatus = RoomStatus, RoomFx = RoomFx  });
        PuyoBehavior = new PuyoBehavior(Room, Terrain);
        PrePhantoonRoomBehavior = new PrePhantoonRoomBehavior(Room, Projectiles);
        PowampBehavior = new PowampBehavior(Terrain, Projectiles, SlotTable, Collision, DeathsAndDrops);
        PolypBehavior = new PolypBehavior(Room, Projectiles);
        PlatformsBehavior = new PlatformsBehavior(Terrain);
        PipeBugsBehavior = new PipeBugsBehavior(SlotTable);
        PhantoonBehavior = new PhantoonBehavior(new PhantoonBehaviorDependencies { Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops , BossArtwork = BossArtworkBinding });
        OwtchBehavior = new OwtchBehavior(Room);
        OldTourianEscapeShaftWallBehavior = new OldTourianEscapeShaftWallBehavior(Room, Projectiles, Audio);
        NuclearWaffleBehavior = new NuclearWaffleBehavior(Projectiles, SpriteObjects);
        NorfairRioBehavior = new NorfairRioBehavior(Room, Terrain, SlotTable);
        NorfairLavaJumpingEnemyBehavior = new NorfairLavaJumpingEnemyBehavior(Room, SlotTable);
        NoobTubeBehavior = new NoobTubeBehavior(Room, Projectiles);
        NinjaSpacePirateBehavior = new NinjaSpacePirateBehavior(Room, Terrain, Projectiles, SpriteObjects, Presentation, WalkingSpacePirateBehavior);
        MultiviolaBehavior = new MultiviolaBehavior(Terrain);
        BabyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Dispatch = Dispatch, Room = Room, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Projectiles = Projectiles, MotherBrainEncounter = MotherBrainEncounter , BossArtwork = BossArtworkBinding });
        MotherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { FrameResults = BossFrameResults, Dispatch = Dispatch, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, RoomActors = new MotherBrainRoomActors(BabyMetroidCutsceneBehavior, RinkaBehavior, WalkingSpacePirateBehavior, RidleyProjectiles)  });
        MochtroidBehavior = new MochtroidBehavior(Terrain, Audio, FrameContext, Collision);
        MetroidBehavior = new MetroidBehavior(Room, Terrain, Audio, SpriteObjects, FrameContext, DeathsAndDrops);
        MetareeBehavior = new MetareeBehavior(Projectiles);
        MaridiaLargeSnailBehavior = new MaridiaLargeSnailBehavior(Terrain, Collision);
        MamaTurtleBehavior = new MamaTurtleBehavior(Terrain, Earthquake, SlotTable, Collision);
        MagdolliteBehavior = new MagdolliteBehavior(Room, Projectiles, SlotTable, Presentation, Collision, DeathsAndDrops);
        LowerNorfairRioBehavior = new LowerNorfairRioBehavior(Room, Terrain, SlotTable);
        KzanBehavior = new KzanBehavior(SlotTable);
        KraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, SlotTable = SlotTable, Presentation = BossPresentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior  });
        KiHunterBehavior = new KiHunterBehavior(Room, Terrain, Projectiles, Audio, SlotTable);
        KagoBehavior = new KagoBehavior(Room, Projectiles, Earthquake, SlotTable, DeathsAndDrops, RoomStatus);
        HoppersBehavior = new HoppersBehavior(Room, Terrain);
        HibashiBehavior = new HibashiBehavior(SlotTable, Audio);
        GrappleBehavior = new GrappleBehavior(Dispatch, Room, SlotTable, DeathsAndDrops);
        FuneNamiheBehavior = new FuneNamiheBehavior(Projectiles);
        FliesBehavior = new FliesBehavior(Room);
        FirefleaBehavior = new FirefleaBehavior(Collision, DeathsAndDrops, RoomStatus);
        FakeKraidBehavior = new FakeKraidBehavior(Room, Terrain, Projectiles, Collision, DeathsAndDrops);
        EyeDoorBehavior = new EyeDoorBehavior(Room, Projectiles);
        EvirBehavior = new EvirBehavior(SlotTable, Collision);
        EtecoonBehavior = new EtecoonBehavior(Terrain, Earthquake, ElevatorBehavior);
        EscapeExplosionsBehavior = new EscapeExplosionsBehavior(Audio, SpriteObjects);
        EscapeAnimalsBehavior = new EscapeAnimalsBehavior(Room, Terrain);
        DraygonBehavior = new DraygonBehavior(new DraygonBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ScrollCap = ScrollCap , BossArtwork = BossArtworkBinding });
        DragonBehavior = new DragonBehavior(Projectiles, SlotTable, Collision, Audio);
        DownwardGateBehavior = new DownwardGateBehavior(Room, Projectiles);
        DeadTourianBehavior = new DeadTourianBehavior(Room, FrameContext, Projectiles, DeadMonsters);
        DeadTorizoBehavior = new DeadTorizoBehavior(Room, FrameContext, Projectiles, Presentation);
        DachoraBehavior = new DachoraBehavior(Room, Terrain, SlotTable, Presentation);
        CrocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, SpriteObjects = SpriteObjects, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior, RoomStatus = RoomStatus  });
        CrawlersBehavior = new CrawlersBehavior(Terrain, Earthquake);
        ChozoStatueBehavior = new ChozoStatueBehavior(Room, Terrain, Projectiles, Audio, SlotTable, Presentation, RoomFx);
        ChootBehavior = new ChootBehavior();
        CeresSteamBehavior = new CeresSteamBehavior(Room, Collision);
        CeresDoorBehavior = new CeresDoorBehavior(new CeresDoorBehaviorDependencies { Room = Room, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, Projectiles = Projectiles, CeresEscape = RidleyEncounter, AreaArtwork = AreaArtworkBinding });
        CacatacBehavior = new CacatacBehavior(Room, Projectiles);
        BullBehavior = new BullBehavior();
        BoyonBehavior = new BoyonBehavior();
        BoulderBehavior = new BoulderBehavior(Terrain, Projectiles);
        BotwoonBehavior = new BotwoonBehavior(new BotwoonBehaviorDependencies { Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, SlotTable = SlotTable, Presentation = Presentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults , BossArtwork = BossArtworkBinding });
        Instructions = new EnemyInstructionInterpreter(new BossBg2Copies(CrocomireBehavior, MotherBrainEncounter, Room), Presentation, Room);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { DeadTorizoBehavior = DeadTorizoBehavior, FrameContext = FrameContext, Instructions = Instructions, MotherBrainBehavior = MotherBrainBehavior, Presentation = Presentation, Room = Room, SlotTable = SlotTable });
        BombTorizoBehavior = new BombTorizoBehavior(new BombTorizoBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops , BossArtwork = BossArtworkBinding });
        BlueBrinstarFaceBlockBehavior = new BlueBrinstarFaceBlockBehavior(Room, Presentation, ElevatorBehavior);
        BeetomBehavior = new BeetomBehavior(Room, Terrain, Audio, FrameContext, Collision);
        AtomicBehavior = new AtomicBehavior();
        AlcoonBehavior = new AlcoonBehavior(Room, Terrain, Projectiles);
        // Every species with room or frame lifecycle state, in the cartridge reset order.
        _species =
        [
            ShitroidBehavior,
            BoyonBehavior,
            MamaTurtleBehavior,
            MetroidBehavior,
            CeresDoorBehavior,
            BoulderBehavior,
            ZebetiteBehavior,
            EtecoonBehavior,
            DachoraBehavior,
            EvirBehavior,
            MorphBallEyeBehavior,
            YappingMawBehavior,
            FuneNamiheBehavior,
            KagoBehavior,
            MagdolliteBehavior,
            BlueBrinstarFaceBlockBehavior,
            KiHunterBehavior,
            PipeBugsBehavior,
            BotwoonBehavior,
            BombTorizoBehavior,
            TourianEntranceStatueBehavior,
            ShaktoolBehavior,
            ChozoStatueBehavior,
            EscapeAnimalsBehavior,
            CrocomireBehavior,
            SporeSpawnBehavior,
            RinkaBehavior,
            RioBehavior,
            NorfairLavaJumpingEnemyBehavior,
            NorfairRioBehavior,
            LowerNorfairRioBehavior,
            MaridiaLargeSnailBehavior,
            RipperBehavior,
            DragonBehavior,
            KraidBehavior,
            PhantoonBehavior,
            DraygonBehavior,
            MotherBrainBehavior,
            DeadTorizoBehavior,
            DeadSidehopperBehavior,
            DeadTourianBehavior,
            ShuttersBehavior,
            ElevatorBehavior,
            NuclearWaffleBehavior,
            FakeKraidBehavior,
            WorkRobotBehavior,
            RidleyBehavior,
            StokeBehavior,
            PuyoBehavior,
            CacatacBehavior,
            OwtchBehavior,
            MultiviolaBehavior,
            PolypBehavior,
            CrawlersBehavior,
            SkreeBehavior,
            FliesBehavior,
            SbugBehavior,
            MochtroidBehavior,
            WreckedShipGhostBehavior,
            HoppersBehavior,
            ZoaBehavior,
            YardBehavior,
            WaverBehavior,
            MetareeBehavior,
            FirefleaBehavior,
            SkulteraBehavior,
            ChootBehavior,
            PlatformsBehavior,
            AlcoonBehavior,
            BeetomBehavior,
            PowampBehavior,
            BullBehavior,
            AtomicBehavior,
            SparkBehavior,
            KzanBehavior,
            HibashiBehavior,
            WalkingSpacePirateBehavior,
            WallSpacePirateBehavior,
            NinjaSpacePirateBehavior,
        ];
        Species = EnemySpeciesRegistry.ForRetailRooms(new EnemyCommonParticipation(GetSharedReactions), [BoyonBehavior, BullBehavior, ZoaBehavior, WaverBehavior, ChootBehavior, AtomicBehavior, SkulteraBehavior, PuyoBehavior, RioBehavior, FliesBehavior, SparkBehavior, StokeBehavior, SbugBehavior, MultiviolaBehavior, CacatacBehavior, AlcoonBehavior, BoulderBehavior, NuclearWaffleBehavior, HibashiBehavior, WreckedShipGhostBehavior, KzanBehavior, RipperBehavior, MochtroidBehavior, SkreeBehavior, OwtchBehavior, BeetomBehavior, KagoBehavior, FirefleaBehavior, MetareeBehavior, PolypBehavior, HoppersBehavior, CrawlersBehavior, PlatformsBehavior, YardBehavior, YappingMawBehavior, EtecoonBehavior, DachoraBehavior, MaridiaLargeSnailBehavior, WorkRobotBehavior, BlueBrinstarFaceBlockBehavior, DragonBehavior, FuneNamiheBehavior, PowampBehavior, NorfairRioBehavior, LowerNorfairRioBehavior, NorfairLavaJumpingEnemyBehavior, ShaktoolBehavior, MetroidBehavior, WalkingSpacePirateBehavior, WallSpacePirateBehavior, NinjaSpacePirateBehavior, ChozoStatueBehavior, TourianEntranceStatueBehavior, DeadSidehopperBehavior, DeadTorizoBehavior, DeadTourianBehavior, ShitroidBehavior, FakeKraidBehavior, ElevatorBehavior, MagdolliteBehavior, EscapeAnimalsBehavior, RinkaBehavior, ZebetiteBehavior, ShuttersBehavior, EvirBehavior, KiHunterBehavior, PipeBugsBehavior, MamaTurtleBehavior, MorphBallEyeBehavior, CeresDoorBehavior, CeresSteamBehavior, GunshipBehavior, BotwoonBehavior, SporeSpawnBehavior, BombTorizoBehavior, KraidBehavior, CrocomireBehavior, DraygonBehavior, PhantoonBehavior, MotherBrainBehavior, BabyMetroidCutsceneBehavior,
            new RidleyEntityFactory(RidleyBehavior, CeresRidleyBehavior, RidleyEncounter), new InertEnemyFactory()]);
        SlotTable.InstallSlots(Species);
        (residue ?? EnemyRoomResidue.None).ApplyTo(this);
    }

    /// <summary>
    /// Re-asserts the door-transition gates over the loaded room: the population initialized with them clear, and the
    /// door-transition code that owns them keeps them for the rest of the transition.
    /// </summary>
    /// <param name="gates">The gates the room the load replaced held.</param>
    internal void ResumeDoorTransitionGates(EnemyDoorTransitionGates gates)
    {
        RoomStatus.EnemyDoorTransitionActive = gates.EnemyDoorTransitionActive;
        ElevatorBehavior.ElevatorDoorTransitionActive = gates.ElevatorDoorTransitionActive;
    }

    /// <summary>Collision words read from the current slots after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>What the room\'s bosses draw with: its presentation surfaces and the installed boss artwork.</summary>
    internal BossPresentation BossPresentation { get; }

    /// <summary>The bosses' installed artwork, read through the session's binding.</summary>
    internal BossArtworkBinding BossArtworkBinding { get; }

    /// <summary>The areas' installed artwork, read through the session's binding.</summary>
    internal AreaArtworkBinding AreaArtworkBinding { get; }

    /// <summary>The areas' installed artwork; setting it rebinds the session's area artwork.</summary>
    public AreaArtworkCatalog? AreaArtwork
    {
        get => AreaArtworkBinding.Artwork;
        set => AreaArtworkBinding.Artwork = value;
    }
    /// <summary>Whether <see cref="Load"/> has loaded this room's population; a room loads it once.</summary>
    private bool _populationLoaded;

    /// <summary>Whether an address-space dependency has been bound by <see cref="Load"/>; this reports binding, not successful completion of every initializer or artwork validation.</summary>
    public bool IsLoaded => Room.IsLoaded;

    /// <summary>
    /// Ports the data-producing parts of <c>LoadEnemies</c>,
    /// <c>ProcessEnemyTilesets</c>, and <c>InitializeEnemies</c> at $A0:8A1E-$8C6C.
    /// </summary>
    public void Load(IRoomEnemyLoad request)
    {
        Ensure.NotNull(request);
        if (_populationLoaded)
            throw new InvalidOperationException("A room's enemy system loads its population once; each room builds its own.");
        _populationLoaded = true;
        IRoomEnemyDefinitionSource definitions = Room.LoadedDefinitions;
        GunshipBehavior.BeginRoomLoad(request.GunshipScenario, request.Samus);
        FrameContext.BeginRoomLoad(request.DeferLoaderTimeCameraReads);
        DeathsAndDrops.BindSamus(request.Samus);
        Presentation.BindBackgrounds(request.Backgrounds, request.ArtworkFrames);
        Presentation.BindColours(request.Colours);
        FrameResults.Begin();
        BossFrameResults.Begin();
        BeginEnemySoundRequestFrame();
        // Initialization AI reads the already-positioned destination camera before the first enemy frame captures its own.
        RinkaBehavior.SetRinkaCamera(request.Camera);
        ShuttersBehavior.BeginShutterFrame(request.Camera);

        // $A0:8A6D does not even inspect the room's enemy set when the first population
        // word is the terminator. This is observable: an empty room must not overwrite
        // CGRAM or VRAM merely because its state happens to retain a non-empty set pointer.
        RoomEnemyPopulationDefinition population = request.Population;
        if (!population.Records.IsEmpty)
            GraphicsSet.LoadGraphicsSet(definitions, request.GraphicsSet, request.Colours.SpriteSets);
        PopulationLoader.LoadPopulation(
            definitions,
            population,
            new EnemySpawnSurroundings(request.Level, request.Samus, request.ControllerInput, request.Camera));
    }

    /// <summary>
    /// Runs one native enemy pass: executes selected AI and instruction lists, publishes
    /// live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    public EnemyFrameResult StepFrame(RoomEnemyFrame frame)
    {
        using var terrainScope = new EnemyTerrain.EnemyTerrainScope(Terrain, frame.CollisionPlms);
        Room.EnsureLoaded();
        DeathsAndDrops.BindSamus(frame.Samus);
        ScrollCap.BeginFrame(frame.SamusPreviousPositionCheckpoint);
        // Standalone audits do not own the runtime NMI clock. In that case the enemy-frame
        // counter begins at zero and advances at the same end-of-frame point, which gives
        // Mama Turtle's even-frame shell jitter the same initial phase as retail room load.
        FrameContext.BeginFrame(frame.TimeIsFrozen, frame.SamusProjectiles, frame.SamusColours, frame.FrameCounter);
        Audio.PowerBomb = frame.SharedProjectiles?.PowerBombExplosion;
        var aiFrame = new ClockedEnemyAiFrame(frame, new FrameCounters(
            frame.FrameCounterLowByte ?? unchecked((byte)FrameContext.RandomEnemyCounter)));
        FrameResults.Begin();
        BossFrameResults.Begin();
        BeginEnemySoundRequestFrame();
        foreach (IEnemySpecies species in _species)
            species.BeginEnemyFrame(frame);
        Projectiles.LastEnemyProjectileDudSoundEffect = null;
        if (!frame.ProcessingListPrepared)
            ProcessingWindow.PrepareEnemyProcessingList(frame.Camera);
        Drawing.Clear();

        foreach (ushort enemyIndex in ProcessingWindow.ActiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotTable.SlotAtEnemyIndex(enemyIndex);
            // EnemyMain $A0:9021-$902E decrements tangible actors' invincibility
            // before checking time freeze. X-ray therefore stops their movement but
            // not this clock. Remember the entry value: the call that reaches zero
            // still skips collision, and a new AI-owned timer must not lose a tick.
            bool invincibleAtEntry = !slot.InvincibilityTimer.IsZero;
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) && invincibleAtEntry)
                slot.InvincibilityTimer = slot.InvincibilityTimer.Decrement().Remaining;
            // EnemyMain checks contact against the pre-movement actor, not the position
            // just moved into by AI. A rising support temporarily overlaps its rider until
            // bank $90 consumes the carry; a late touch pass would retrigger that support.
            if (!frame.TimeIsFrozen && !invincibleAtEntry && frame.ResolveSamusContactBeforeAi && frame.Samus is not null)
            {
                // Native extended collision runs projectiles before bombs/touch and
                // before selecting hurt/main AI. In particular, a retained Plasma
                // hit must publish Phantoon's hurt clock before this frame ticks it.
                if (slot.Entity is IEnemyContactHooks hooks)
                    hooks.ResolveBeforeOrdinaryHits(frame);
                if (frame.SamusProjectiles is not null && frame.SharedProjectiles is not null)
                    Collision.ResolveOrdinaryProjectileHits(frame.SamusProjectiles, frame.SharedProjectiles,
                        frame.Samus, enemyIndex);
                // Alpha already updated the bomb slots. Native EnemyMain checks
                // them before this actor's touch and AI, so escape starts now.
                if (frame.SharedProjectiles is not null && frame.SamusProjectiles is not null)
                    Collision.ResolveOrdinaryBombHits(frame.SharedProjectiles, frame.SamusProjectiles, frame.Samus, enemyIndex);
                if (slot.DefinitionId == 0)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.ControllerInput, frame.Level, enemyIndex);
                if (slot.Entity is IEnemyContactHooks contact)
                    contact.ResolveAfterSamusContact(frame.Samus);
                if (slot.DefinitionId == 0)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(slot, aiFrame);
                // Hurt AI replaces main AI while the hurt bit is set; the first registered match runs.
                // Native selects the lowest AI bit even when its handler is just
                // RTL. Hurt therefore delays frozen AI until flash housekeeping
                // clears bit two; falling through here prematurely clears flash.
                if (!ranActorAi &&
                    (slot.AiStates & EnemyAiStates.Hurt) != 0 &&
                    (slot.Entity as IEnemyHurtAi)?.FindHurt() is { } hurt)
                {
                    hurt(slot, frame);
                    ranActorAi = true;
                }
                // $A0:9044 selects the handler from the lowest set AI bit alone. A frozen
                // clock without bit four (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (slot.AiStates & EnemyAiStates.Frozen) != 0)
                {
                    // Rinka's termination tail clears/replaces the current actor record.
                    // The active-index array was frozen before AI, so skip the remainder
                    // of this stale entry exactly as the private routine's death call does.
                    var frozen = slot.Entity as IEnemyFrozenAi;
                    if (frozen?.FindFrozenPreemption() is { } preemption && preemption(slot))
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // dispatch for this call. Ice removal instead takes the thaw branch
                    // immediately. That branch stores the remaining AI bits into BOTH
                    // words; replacing the clock with literal zero loses retail behavior
                    // when hurt and frozen dispatch coexist.
                    slot.FlashTimer = WrappingCountdown.Start(0);
                    bool thaw = slot.FrozenTimer.IsZero ||
                        (frame.Samus is not null &&
                            (frame.Samus.EquippedBeams & SamusBeamFlags.Ice) == 0);
                    if (!thaw)
                        slot.FrozenTimer = slot.FrozenTimer.Decrement().Remaining;
                    else
                    {
                        slot.AiStates = slot.AiStates & ~EnemyAiStates.Frozen;
                        slot.FrozenTimer = WrappingCountdown.FromRaw(slot.AiStates.NativeWord);
                    }
                    foreach (Action<RoomEnemySlot> handler in frozen?.FrozenHandlers() ?? [])
                        handler(slot);
                    // The native frame clock advances after every selected AI call,
                    // including frozen AI. Instruction gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    EnemySlotDispatch.RunMainAi(slot, aiFrame);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    slot.FrameCounter = WrappingCounter.IncrementWord(slot.FrameCounter);
                    // `$A0:8FF7` skips the instruction interpreter whenever frozen bit
                    // four is present, even if lower-priority hurt bit two selected an
                    // enemy-specific hurt callback above. Testing only `ranActorAi` here
                    // incorrectly advanced a Metroid's body animation while its frozen
                    // callback still owned the two composited outer sprite objects.
                    if ((slot.AiStates & EnemyAiStates.Frozen) == 0 &&
                        slot.Properties.HasAny(EnemyProperties.ProcessInstructions))
                        EnemyInstructionInterpreter.ProcessInstructions(
                            slot,
                            frame.Samus,
                            frame.Level,
                            frame.Camera,
                            frame.ControllerInput);
                }
            }

            // EnemyMain queues ordinary-sprite actors only after AI and instruction work.
            // Properties $0100/$0200 suppress drawing, while extra-property bit $0004 can
            // force an otherwise off-screen actor into the queue. Gunship uses the normal
            // radius-aware visibility test.
            bool visible = slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
                !EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, frame.Camera);
            if (visible && !slot.Properties.HasAny(EnemyProperties.Invisible | EnemyProperties.Deleted))
                Drawing.Enqueue(slot.Layer & 7, enemyIndex);

            // $A0:9128 performs this after the enemy has been admitted to its draw queue.
            // Keeping it here also makes the boss's latched draw palettes describe the
            // frame just processed rather than the already-decremented following frame.
            if (!frame.TimeIsFrozen && !slot.FlashTimer.IsZero)
            {
                slot.FlashTimer = slot.FlashTimer.Decrement().Remaining;
                // `$A0:9128` clears hurt dispatch once the post-decrement timer is below
                // eight. This is a signed comparison and applies even when another AI bit
                // remains set; omitting it would strand Metroid in custom hurt forever.
                if (unchecked((short)(slot.FlashTimer.RawValue - 8)) < 0)
                    slot.AiStates = slot.AiStates & ~EnemyAiStates.Hurt;
            }
        }

        // DetermineWhichEnemiesToProcess freezes only the index list. Later bank-$94
        // collision routines dereference the live slot words, so publish bodies after AI
        // has had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (ushort enemyIndex in ProcessingWindow.InteractiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotTable.SlotAtEnemyIndex(enemyIndex);
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                slot.SlotIndex,
                enemyIndex,
                slot.Position.Whole,
                new(slot.Radius.X, slot.Radius.Y),
                // `$A0:A9D2-$A0:A9E8`: a frozen enemy blocks Samus whatever its properties say.
                !slot.FrozenTimer.IsZero || slot.Properties.HasAny(EnemyProperties.SolidToSamus)));
        }
        FrameContext.AdvanceRandomEnemyCounter();
        frame.Samus?.Kinematics.ClearSolidEnemyCollisionIndexes();
        WorkRobotBehavior.StepWorkRobotPaletteAnimation();
        MagdolliteBehavior.StepMagdollitePaletteAnimation();
        BlueBrinstarFaceBlockBehavior.StepBlueBrinstarFaceBlockPaletteAnimation();
        if (!frame.TimeIsFrozen)
            SpriteObjects.StepRoomSpriteObjects();
        CollectLegacyEnemyAudioRequests();
        return BuildFrameResult();
    }

    /// <summary>Ports <c>Handle_Room_Shaking</c> at $A0:8687-$8711: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        Room.EnsureLoaded();
        // This runs after drawing on the cartridge, so the two-frame actor timer becomes visible
        // on the following frame rather than retroactively changing already-emitted OAM.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (ushort enemyIndex in ProcessingWindow.ActiveEnemyIndexes)
                SlotTable.SlotAtEnemyIndex(enemyIndex).ShakeTimer = WrappingCountdown.Start(RoomFxRomData.Earthquake.EnemyShakeDuration);
        }
        return Earthquake.LastRoomShake;
    }

    /// <summary>Begins the native EnemyMain publication window.</summary>
    internal void BeginEnemySoundRequestFrame()
    {
        // Species clear their own legacy publications in their frame and room-load hooks, which run next.
        Audio.BeginFrame();
    }

    /// <summary>
    /// Converts the earlier one-value-per-family debugger seams into audible bank-$80
    /// requests after all active actors have run. The library and queue limits below come
    /// from the corresponding retail <c>QueueSfxN_MaxM</c> call, not from sound-ID meaning.
    /// </summary>
    /// <remarks>
    /// New translations should call <see cref="EnemyAudioQueue.QueueEnemySound"/> at the translated call
    /// site. This adapter exists so the already-large enemy translation can become audible
    /// without rewriting unrelated, verified AI in one risky mechanical pass. A legacy
    /// nullable field can retain only its final same-family call in a frame; that historical
    /// debugger limitation cannot be recovered here. Distinct families still append distinct
    /// requests, and every new/direct producer is lossless.
    /// </remarks>
    internal void CollectLegacyEnemyAudioRequests()
    {
        // Ordinary bank-$A2/$A3/$A8/$B2 actors overwhelmingly call QueueSfx2_Max6.
        BoyonBehavior.QueueLegacySounds(Audio);
        MamaTurtleBehavior.QueueLegacySounds(Audio);
        CacatacBehavior.QueueLegacySounds(Audio);
        BoulderBehavior.QueueLegacySounds(Audio);
        EtecoonBehavior.QueueLegacySounds(Audio);
        DachoraBehavior.QueueLegacySounds(Audio);
        EvirBehavior.QueueLegacySounds(Audio);
        MorphBallEyeBehavior.QueueLegacySounds(Audio);
        YappingMawBehavior.QueueLegacySounds(Audio);
        HoppersBehavior.QueueLegacySounds(Audio);
        MetareeBehavior.QueueLegacySounds(Audio);
        AlcoonBehavior.QueueLegacySounds(Audio);
        FuneNamiheBehavior.QueueLegacySounds(Audio);
        KagoBehavior.QueueLegacySounds(Audio);
        KzanBehavior.QueueLegacySounds(Audio);
        NuclearWaffleBehavior.QueueLegacySounds(Audio);
        FakeKraidBehavior.QueueLegacySounds(Audio);
        WorkRobotBehavior.QueueLegacySounds(Audio);
        WalkingSpacePirateBehavior.QueueLegacySounds(Audio);
        RioBehavior.QueueLegacySounds(Audio);
        NorfairLavaJumpingEnemyBehavior.QueueLegacySounds(Audio);
        NorfairRioBehavior.QueueLegacySounds(Audio);
        LowerNorfairRioBehavior.QueueLegacySounds(Audio);
        MaridiaLargeSnailBehavior.QueueLegacySounds(Audio);
        MagdolliteBehavior.QueueLegacySounds(Audio);
        ShuttersBehavior.QueueLegacySounds(Audio);

        // These actors deliberately use a different library or the tighter Max3 entry.
        YardBehavior.QueueLegacySounds(Audio);
        ZebetiteBehavior.QueueLegacySounds(Audio);
        MetroidBehavior.QueueLegacySounds(Audio);

        // Bank-$86 pickup/death opcodes and the common rejected-shot path have explicit,
        // smaller admission limits. Their fields are shared by several projectile actors.
        DeathsAndDrops.QueueLegacySounds(Audio);
        Projectiles.QueueLegacySounds(Audio);

        // Encounter-specific banks. Ceres doors, elevators, Skree, and Bomb Torizo sound
        // opcodes already publish directly and are intentionally absent to avoid duplicates.
        BotwoonBehavior.QueueLegacySounds(Audio);
        CrocomireBehavior.QueueLegacySounds(Audio);
        DeadTorizoBehavior.QueueLegacySounds(Audio);
        DeadMonsters.QueueLegacySounds(Audio);
        ShitroidBehavior.QueueLegacySounds(Audio);
        SporeSpawnBehavior.QueueLegacySounds(Audio);

        KraidBehavior.QueueLegacySounds(Audio);
        RidleyEncounter.QueueLegacySounds(Audio);
        DraygonBehavior.QueueLegacySounds(Audio);
        PhantoonBehavior.QueueLegacySounds(Audio);

        MotherBrainEncounter.QueueLegacySounds(Audio);

        BotwoonBehavior.QueueLegacyMusic(Audio);
        BombTorizoBehavior.QueueLegacyMusic(Audio);
        CrocomireBehavior.QueueLegacyMusic(Audio);
        KraidBehavior.QueueLegacyMusic(Audio);
        DraygonBehavior.QueueLegacyMusic(Audio);
        PhantoonBehavior.QueueLegacyMusic(Audio);
        RidleyEncounter.QueueLegacyMusic(Audio);
        ShitroidBehavior.QueueLegacyMusic(Audio);
        MotherBrainEncounter.QueueLegacyMusic(Audio);
    }

    /// <summary>
    /// Rebuilds the shared reactions after construction or debugger restore, in the established first-match order.
    /// </summary>
    private EnemySharedReactions BuildSharedReactions()
    {
        var table = new EnemySharedReactions();
        RegisterTouchHandlers(table);
        RegisterShotReactions(table);
        RegisterBombReactions(table);
        RegisterPowerBombReactions(table);
        RegisterHurtAi(table);
        RegisterFrozenAi(table);
        return table;
    }

    /// <summary>Maps enemy-definition bomb reaction pointers to translated handlers.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations.</remarks>
    private void RegisterBombReactions(EnemySharedReactions table)
    {
        CrocomireBehavior.Combat.RegisterCrocomireBombReactions(table);
        KraidBehavior.Combat.RegisterKraidBombReactions(table);

        NinjaSpacePirateBehavior.AddGoldNinjaPirateBombReactions(table);
        ShuttersBehavior.AddShuttersBombReactions(table);
        MetroidBehavior.AddMetroidBombReactions(table);
        ShitroidBehavior.AddShitroidBombReactions(table);
        CeresRidleyBehavior.Hits.RegisterCeresRidleyBombReactions(table);
        BombTorizoBehavior.Combat.RegisterBombTorizoBombReactions(table);

        // `$A9:B503` is `CreateDudShot`. The multibox bomb walker has already marked the physical explosion, and the
        // body callback performs no health, phase, or multipart-state work of its own.
        MotherBrainCombat.AddMotherBrainBodyBombReactions(table);
        MotherBrainBehavior.Combat.AddMotherBrainBombReactions(table);
        DeadTorizoBehavior.AddDeadTorizoBombReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperBombReactions(table);
        DeadTourianBehavior.AddDeadTourianBombReactions(table);
        PhantoonBehavior.Combat.RegisterPhantoonBombReactions(table);
        MamaTurtleBehavior.AddMamaTurtleBombReactions(table);
        RinkaBehavior.AddRinkaBombReactions(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailBombReactions(table);
        RipperBehavior.AddRipperBombReactions(table);
        DragonBehavior.AddDragonBombReactions(table);
        // `$A3:8B0F` restores the dying actor's graphics long enough to seed four debris actors, then clears both
        // words. The host death flag does not erase the record, so the exact source remains here.
        MetareeBehavior.AddMetareeBombReactions(table);
        FirefleaBehavior.AddFirefleaBombReactions(table);
        FakeKraidBehavior.RegisterFakeKraidBombReactions(table);
        PlatformsBehavior.AddPlatformsBombReactions(table);
        // Skree's callback owns its four-piece burst and death animation because it called
        // `NormalEnemyShotAiSkipDeathAnim`. A normal bomb selects death variant zero (only missiles select two).
        SkreeBehavior.AddSkreeBombReactions(table);
        ZebetiteBehavior.AddZebetiteBombReactions(table);
        SporeSpawnBehavior.AddSporeSpawnBombReactions(table);
        DraygonBehavior.Combat.RegisterDraygonBombReactions(table);
        BotwoonBehavior.RegisterBotwoonBombReactions(table);
        RidleyInteractions.RegisterRidleyBombReactions(table);
        ShaktoolBehavior.AddShaktoolBombReactions(table);
        ShuttersBehavior.AddDestroyableShutterBombReactions(table);
        EvirBehavior.AddEvirBombReactions(table);
        YappingMawBehavior.AddYappingMawBombReactions(table);
        KagoBehavior.AddKagoBombReactions(table);
        MagdolliteBehavior.AddMagdolliteBombReactions(table);
        BeetomBehavior.AddBeetomBombReactions(table);
        PowampBehavior.AddPowampBombReactions(table);
        WorkRobotBehavior.AddWorkRobotBombReactions(table);
        KiHunterBehavior.AddKiHunterBombReactions(table);
        OwtchBehavior.AddOwtchBombReactions(table);
}

    /// <summary>Maps banked power-bomb reaction pointers for each supported species.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive: each names its
    /// definition and header reaction callback.</remarks>
    private void RegisterPowerBombReactions(EnemySharedReactions table)
    {
        // These headers install private callbacks instead of falling through common power-bomb damage. A literal RTL
        // intentionally does nothing; the shutter callbacks only run their trigger state machines. All still
        // receive the native process-off-screen bit after a qualifying ellipse overlap. Literal no-ops are
        // classified by their compiled bank-qualified identities without fetching executable ROM bytes.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy =>
            enemy.Definition.PowerBombReactionBody == EnemyCallbackBody.ImmediateReturn)
        {
            ReplacesCommon = static (_, _) => { },
        });
        ShuttersBehavior.AddShuttersPowerBombReactions(table);
        CrocomireBehavior.Combat.RegisterCrocomirePowerBombReactions(table);
        DeadTorizoBehavior.AddDeadTorizoPowerBombReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperPowerBombReactions(table);
        DeadTourianBehavior.AddDeadTourianPowerBombReactions(table);
        ShitroidBehavior.AddShitroidPowerBombReactions(table);
        FirefleaBehavior.AddFirefleaPowerBombReactions(table);
        MetroidBehavior.AddMetroidPowerBombReactions(table);
        FakeKraidBehavior.RegisterFakeKraidPowerBombReactions(table);
        PowampBehavior.AddPowampPowerBombReactions(table);
        MagdolliteBehavior.AddMagdollitePowerBombReactions(table);
        RinkaBehavior.AddRinkaPowerBombReactions(table);
        DragonBehavior.AddDragonPowerBombReactions(table);
        EvirBehavior.AddEvirPowerBombReactions(table);
        KiHunterBehavior.AddKiHunterPowerBombReactions(table);
        BotwoonBehavior.RegisterBotwoonPowerBombReactions(table);
        WallSpacePirateBehavior.AddWallSpacePiratePowerBombReactions(table);
        RidleyInteractions.RegisterRidleyPowerBombReactions(table);
        DraygonBehavior.Combat.RegisterDraygonPowerBombReactions(table);
}

    /// <summary>Advances enemy-projectile actors after Samus movement and before PLMs.</summary>
    public void StepEnemyProjectileInstructions(
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera = default,
        int? frameCounterByte = null,
        SamusBombProjectileSystem? samusBombs = null,
        BackgroundScrollState? backgroundScroll = null,
        int? nmiFrameCounter = null)
    {
        Room.EnsureLoaded();
        DeathsAndDrops.BindSamus(samus);
        PrePhantoonRoomBehavior.BindBackgroundScroll(backgroundScroll);
        DeathsAndDrops.LastEnemyPickupSoundEffect = null;
        DeathsAndDrops.LastEnemyDeathSoundEffectLibrary2 = null;
        Projectiles.RunPass(level, samus, camera, frameCounterByte, samusBombs, nmiFrameCounter);
    }

    /// <summary>
    /// Registers species projectile handlers before generic and cross-species handlers,
    /// preserving the keyed pre-instruction and ordered instruction-interceptor dispatch.
    /// </summary>
    void IEnemyProjectileHandlerSource.AddEnemyProjectileHandlers(EnemyProjectileHandlerTable table)
    {
        // Species register first; generic and cross-species registrations follow.
        AlcoonBehavior.AddAlcoonProjectiles(table);
        BombTorizoBehavior.Attacks.RegisterBombTorizoProjectiles(table);
        BotwoonBehavior.RegisterBotwoonProjectiles(table);
        CacatacBehavior.AddCacatacProjectiles(table);
        CrocomireBehavior.Combat.RegisterCrocomireProjectiles(table);
        DownwardGateBehavior.AddDownwardGateProjectiles(table);
        DragonBehavior.AddDragonProjectiles(table);
        DraygonBehavior.Goop.RegisterDraygonProjectiles(table);
        EyeDoorBehavior.AddEyeDoorProjectiles(table);
        FakeKraidBehavior.RegisterFakeKraidProjectiles(table);
        FallingSparksBehavior.AddFallingSparksProjectiles(table);
        FuneNamiheBehavior.AddFuneNamiheProjectiles(table);
        KagoBehavior.AddKagoProjectiles(table);
        KiHunterBehavior.AddKiHunterProjectiles(table);
        KraidProjectiles.RegisterKraidProjectiles(table);
        KraidProjectiles.RegisterKraidRockAnimations(table);
        MagdolliteBehavior.AddMagdolliteProjectiles(table);
        MotherBrainBehavior.Projectiles.AddMotherBrainProjectiles(table);
        NinjaSpacePirateBehavior.AddNinjaSpacePirateProjectiles(table);
        NoobTubeBehavior.AddNoobTubeProjectiles(table);
        PhantoonBehavior.Combat.RegisterPhantoonProjectiles(table);
        PolypBehavior.AddPolypProjectiles(table);
        PowampBehavior.AddPowampProjectiles(table);
        PrePhantoonRoomBehavior.RegisterPrePhantoonRoomProjectiles(table);
        ShaktoolBehavior.AddShaktoolProjectiles(table);
        SporeSpawnBehavior.AddSporeSpawnProjectiles(table);
        StokeBehavior.AddStokeProjectiles(table);
        TourianEntranceStatueBehavior.RegisterTourianEntranceStatueProjectiles(table);
        TourianUnlockBehavior.RegisterTourianUnlockProjectiles(table);
        WalkingSpacePirateBehavior.AddWalkingSpacePirateProjectiles(table);
        WorkRobotBehavior.AddWorkRobotProjectiles(table);

        DeathsAndDrops.ConnectPickupPreInstruction(table);

        RinkaBehavior.AddMiscDustPreInstruction(table);

        RidleyProjectiles.RegisterFireballPreInstructions(table);

        EnemyProjectileEngine.AddMetalSkreeParticlePreInstruction(table);

        KraidProjectiles.RegisterKraidRockSpitPreInstruction(table);

        Projectiles.AddCeresFallingTilePreInstruction(table);

        EnemyProjectileEngine.AddGunshipDustCues(table);


        OldTourianEscapeShaftWallBehavior.AddWallExplosionCues(table);

        NoobTubeBehavior.AddNoobTubeProjectileCues(table);


        RidleyProjectiles.RegisterFireballCues(table);

        CrocomireBehavior.Combat.RegisterCrocomireDropProjectiles(table);

        DeathsAndDrops.ConnectDeathExplosionCues(table);
    }

    /// <summary>Maps shot-response pointers, including species-specific damage behavior.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive. Kraid's parts
    /// precede the generic literal no-op header and the ordinary normal-shot reaction is last, because Crocomire's
    /// tongue also uses NormalEnemyShot.</remarks>
    private void RegisterShotReactions(EnemySharedReactions table)
    {
        DeadTorizoBehavior.AddDeadTorizoShotReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperShotReactions(table);
        DeadTourianBehavior.AddDeadTourianShotReactions(table);
        ShitroidBehavior.AddShitroidShotReactions(table);
        MotherBrainBehavior.Combat.AddMotherBrainShotReactions(table);

        RidleyInteractions.RegisterRidleyShotReactions(table);

        DraygonBehavior.Combat.RegisterDraygonShotReactions(table);
        BombTorizoBehavior.Combat.RegisterBombTorizoShotReactions(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailShotReactions(table);
        SporeSpawnBehavior.AddSporeSpawnShotReactions(table);
        KraidBehavior.Combat.RegisterKraidShotReactions(table);
        CrocomireBehavior.Combat.RegisterCrocomireShotReactions(table);
        OwtchBehavior.AddOwtchShotReactions(table);
        WallSpacePirateBehavior.AddWallSpacePirateShotReactions(table);
        ShuttersBehavior.AddShuttersShotReactions(table);

        MetroidBehavior.AddMetroidShotReactions(table);
        YardBehavior.AddYardShotReactions(table);

        // `$A3:8B0F` saves these graphics words around common shot AI, uses them for four metal debris actors on
        // death, then clears the dead body. Spawning before the clears preserves that exact graphics index.
        MetareeBehavior.AddMetareeShotReactions(table);

        // $A3:C7F5 adds Skree's four debris actors after the shared normal shot handler reports death, before the
        // common death animation releases the enemy slot.
        SkreeBehavior.AddSkreeShotReactions(table);
        FirefleaBehavior.AddFirefleaShotReactions(table);
        PlatformsBehavior.AddPlatformsShotReactions(table);
        BeetomBehavior.AddBeetomShotReactions(table);
        PowampBehavior.AddPowampShotReactions(table);
        WorkRobotBehavior.AddWorkRobotShotReactions(table);
        FakeKraidBehavior.RegisterFakeKraidShotReactions(table);
        MamaTurtleBehavior.AddMamaTurtleShotReactions(table);
        KagoBehavior.AddKagoShotReactions(table);
        MagdolliteBehavior.AddMagdolliteShotReactions(table);
        RinkaBehavior.AddRinkaShotReactions(table);
        RipperBehavior.AddRipperShotReactions(table);
        DragonBehavior.AddDragonShotReactions(table);
        ShuttersBehavior.AddDestroyableAndHorizontalShutterShotReactions(table);
        ZebetiteBehavior.AddZebetiteShotReactions(table);
        EvirBehavior.AddEvirShotReactions(table);
        YappingMawBehavior.AddYappingMawShotReactions(table);
        KiHunterBehavior.AddKiHunterShotReactions(table);
        BotwoonBehavior.RegisterBotwoonShotReactions(table);
        ShaktoolBehavior.AddShaktoolShotReactions(table);

        // Ordinary common shot AI with no species stage.
        CeresSteamBehavior.RegisterCeresSteamShotReactions(table);
        MochtroidBehavior.AddMochtroidShotReactions(table);
        // Several retail helper/projectile definitions point their shot callback at a literal RTL in their own enemy
        // bank. Radius collision still dispatches those callbacks after its projectile prelude.
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.IsLiteralNoOpShotAi(enemy.Definition, EnemyCollision.ShotAi(enemy))));
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot));
}

    /// <summary>Registers Samus-contact handlers after species instruction registrations.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive (each names its
    /// definition and header callback); the ordinary normal-touch handler is last because Crocomire's tongue also uses
    /// NormalEnemyTouch.</remarks>
    private void RegisterTouchHandlers(EnemySharedReactions table)
    {
        KraidBehavior.Projectiles.RegisterKraidNailTouch(table);
        KraidBehavior.Combat.RegisterKraidTouch(table);

        MotherBrainCombat.AddMotherBrainHeadTouch(table);
        ShitroidBehavior.AddShitroidTouch(table);
        DeadSidehopperBehavior.AddDeadSidehopperTouch(table);
        DeadTourianBehavior.AddDeadTourianTouch(table);
        DeadTorizoBehavior.AddDeadTorizoTouch(table);
        WallSpacePirateBehavior.AddWallSpacePirateTouch(table);
        BombTorizoBehavior.Combat.RegisterBombTorizoTouch(table);

        PhantoonBehavior.Combat.RegisterPhantoonBodyTouch(table);
        DraygonBehavior.Combat.RegisterDraygonTouch(table);
        SporeSpawnBehavior.AddSporeSpawnTouch(table);
        CeresSteamBehavior.RegisterCeresSteamTouch(table);
        CrocomireBehavior.Combat.RegisterCrocomireTouch(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailTouch(table);
        PlatformsBehavior.AddPlatformsTouch(table);

        // Fifteen inventoried header/hitbox callbacks are literal RTL routines. Classify their bank-qualified
        // identities rather than probing executable ROM bytes at runtime. The collision is still reported, but the
        // callback performs no damage, knockback, or actor reaction.
        table.AddTouch(slot => slot.Definition.TouchBody == EnemyCallbackBody.ImmediateReturn,
            static _ => { });
        ShuttersBehavior.AddShuttersTouch(table);
        MochtroidBehavior.AddMochtroidTouch(table);
        MetroidBehavior.AddMetroidTouch(table);
        YappingMawBehavior.AddYappingMawTouch(table);
        YardBehavior.AddYardTouch(table);
        BeetomBehavior.AddBeetomTouch(table);
        PowampBehavior.AddPowampTouch(table);
        WorkRobotBehavior.AddWorkRobotTouch(table);
        FirefleaBehavior.AddFirefleaTouch(table);
        MamaTurtleBehavior.AddMamaTurtleTouch(table);
        MagdolliteBehavior.AddMagdolliteTouch(table);
        RinkaBehavior.AddRinkaTouch(table);
        DragonBehavior.AddDragonTouch(table);
        EvirBehavior.AddEvirTouch(table);
        BotwoonBehavior.RegisterBotwoonTouch(table);

        ZebetiteBehavior.AddZebetiteTouch(table);
        FakeKraidBehavior.RegisterFakeKraidTouch(table);
        ShaktoolBehavior.AddShaktoolTouch(table);

        table.AddTouch(slot => EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.NormalEnemyTouch,
            context => Collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
}


    /// <summary>
    /// Publishes enemy-authored target colors to the current room-fade owner. Only written
    /// entries are transferred: untouched HUD, suit, and room rows retain their own targets.
    /// Current CGRAM must not be changed by instructions which target the fade buffer.
    /// </summary>
    internal void ConsumeTargetPaletteWrites(Action<SporeSpawnStagedColour> write)
    {
        Ensure.NotNull(write);
        SporeSpawnBehavior.ConsumeTargetColors(write);
    }

    /// <summary>Maps hurt-state AI pointers used while enemies are flashing or reacting.</summary>
    /// <remarks>Hurt AI is a first-match list in EnemyMain's original dispatch order; the shared literal no-op hurt
    /// pointer is last.</remarks>
    private void RegisterHurtAi(EnemySharedReactions table)
    {
        MetroidBehavior.AddMetroidHurtAi(table);
        BombTorizoBehavior.Combat.RegisterBombTorizoHurtAi(table);
        RidleyBehavior.RegisterRidleyHurtAi(table);
        PhantoonBehavior.RegisterPhantoonHurtAi(table);
        DraygonBehavior.RegisterDraygonHurtAi(table);
        table.AddHurt(slot => slot.Definition.HurtAi == EnemyAiRoutine.CommonNoOp, (_, _) => { });
    }

    /// <summary>Maps frozen-state AI pointers used while ice suspends normal behavior.</summary>
    /// <remarks>Rinka's frozen tail runs before the common thaw and can consume the frame; the others run after
    /// it.</remarks>
    private void RegisterFrozenAi(EnemySharedReactions table)
    {
        RinkaBehavior.AddRinkaFrozenPreemption(table);
        MetroidBehavior.AddMetroidFrozenAi(table);
        YappingMawBehavior.AddYappingMawFrozenAi(table);
    }
}
