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
using SuperMetroid.Core.Enemies.Artwork;
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
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Enemies;

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
public sealed class RoomEnemySystem : IEnemyDispatch, IEnemyProjectileHandlerSource
{
    /// <summary>The <see cref="EnemyDrawList"/> owner of this RoomEnemySystem.</summary>
    internal EnemyDrawList Drawing { get; }

    /// <summary>The <see cref="EnemyInstructionInterpreter"/> owner of this RoomEnemySystem.</summary>
    internal EnemyInstructionInterpreter Instructions { get; }

    /// <summary>The <see cref="EnemyPopulationLoader"/> owner of this RoomEnemySystem.</summary>
    internal EnemyPopulationLoader PopulationLoader { get; }

    /// <summary>The <see cref="EnemyProcessingWindow"/> owner of this RoomEnemySystem.</summary>
    internal EnemyProcessingWindow ProcessingWindow { get; }

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

    /// <summary>The <see cref="GunshipHealthBehavior"/> owner of this RoomEnemySystem.</summary>
    internal GunshipHealthBehavior GunshipHealthBehavior { get; }

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
    /// Resolves native AI addresses to species handlers. The table contains delegates and is
    /// rebuilt from its owners on first access after a debugger restore.
    /// </summary>
    [field: NonSerialized]
    internal EnemyAiTable Ai => field ??= BuildEnemyAiTable();

    /// <summary>The loaded room's address space, services and artwork, shared with the composed owners.</summary>
    internal EnemyRoomBinding Room { get; } = new();

    /// <summary>Current host-owned escape-warning text, rebound after debugger restoration.</summary>
    public EscapeTypewriterPresentation? EscapeTypewriterPresentation
    {
        get => Presentation.EscapeTypewriterPresentation;
        set
        {
            Presentation.EscapeTypewriterPresentation = value;
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

    /// <summary>Thirty-two fixed physical enemy records in the native bank-$A0 scheduler, including unused and multipart slots.</summary>
    public const int MaximumEnemyCount = 32;
    /// <summary>Native enemy-record stride in bytes ($0040), used to translate host slot numbers into cartridge indexes $0000..$07C0.</summary>
    public const int NativeSlotSize = 0x40;
    /// <summary>Maximum four enemy graphics-set entries accepted by the native room tileset arrays.</summary>
    public const int MaximumGraphicsSetCount = 4;
    /// <summary>Post-AI collision snapshots for active physical enemy slots.</summary>
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();

    /// <summary>Creates all 32 stable physical slot objects without loading a room or binding memory, graphics, random-number, or gameplay services.</summary>
    public RoomEnemySystem()
    {
        // Shared collaborators first: every species owner receives the ones it uses.
        ScrollCap = new SamusScrollCap();
        RoomFx = new EnemyRoomFx();
        RoomStatus = new EnemyRoomStatus();
        FrameContext = new EnemyFrameContext();
        Presentation = new EnemyPresentation(Room);
        SlotTable = new EnemySlotTable();
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, SlotTable, this);
        RidleyEncounter = new RidleyEncounter(SlotTable);
        Earthquake = new RoomEarthquake();
        SpriteObjects = new RoomSpriteObjectPool(Room);
        Audio = new EnemyAudioQueue();
        Projectiles = new EnemyProjectileEngine(Room, SpriteObjects, Earthquake, Audio);
        Projectiles.AddHandlerSource(this);
        GunshipBehavior = new GunshipBehavior(Audio, Projectiles, Room, SlotTable);
        Terrain = new EnemyTerrain();
        DeadMonsters = new DeadMonsterFrame(Room);
        MotherBrainEncounter = new MotherBrainEncounter(Presentation, Room);
        RidleyTail = new RidleyTail(FrameContext, Projectiles, RidleyEncounter, Room, SlotTable);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, SlotTable, this);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Dispatch = this, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, FrameContext = FrameContext, Presentation = Presentation, Projectiles = Projectiles, Room = Room, SlotTable = SlotTable });
        RidleyProjectiles = new RidleyProjectiles(DeathsAndDrops, Projectiles, RidleyEncounter);
        ZoaBehavior = new ZoaBehavior();
        ElevatorBehavior = new ElevatorBehavior(Room, Audio, MotherBrainEncounter);
        ZebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Dispatch = this, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, SlotTable = SlotTable, Presentation = Presentation, ElevatorBehavior = ElevatorBehavior, RoomStatus = RoomStatus, Collision = Collision });
        YardBehavior = new YardEnemyBehavior(Room, Terrain, Earthquake, Collision);
        YappingMawBehavior = new YappingMawBehavior(Room, Projectiles, SpriteObjects, ScrollCap);
        WreckedShipGhostBehavior = new WreckedShipGhostBehavior(Presentation);
        WorkRobotBehavior = new WorkRobotBehavior(Room, Terrain, Projectiles, Presentation);
        WaverBehavior = new WaverBehavior(Terrain);
        WalkingSpacePirateBehavior = new WalkingSpacePirateBehavior(Terrain, Projectiles);
        WallSpacePirateBehavior = new WallSpacePirateBehavior(Room, Terrain, Collision, DeathsAndDrops, WalkingSpacePirateBehavior);
        TourianUnlockBehavior = new TourianUnlockBehavior(Room, Projectiles, Earthquake, Audio, Presentation);
        TourianEntranceStatueBehavior = new TourianEntranceStatueBehavior(Room, Projectiles, Presentation);
        StokeBehavior = new StokeBehavior(Room, Terrain, Projectiles);
        SporeSpawnBehavior = new SporeSpawnBehavior(Room, Projectiles, SpriteObjects, Presentation, FrameContext, Collision, DeathsAndDrops);
        FallingSparksBehavior = new FallingSparksBehavior(Room, Projectiles, SpriteObjects);
        SparkBehavior = new SparkBehavior(Room, FallingSparksBehavior);
        SkulteraBehavior = new SkulteraBehavior(Terrain);
        SkreeBehavior = new SkreeBehavior(Audio, Projectiles, DeathsAndDrops);
        ShuttersBehavior = new ShuttersBehavior();
        DeadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = Room, Terrain = Terrain, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Projectiles = Projectiles, Collision = Collision, DeadMonsters = DeadMonsters });
        ShitroidBehavior = new ShitroidBehavior(Room, SlotTable, Presentation, DeadSidehopperBehavior, ZebetiteBehavior);
        ShaktoolBehavior = new ShaktoolBehavior(Terrain, Projectiles, SlotTable, Collision);
        SbugBehavior = new SbugBehavior(Room, Terrain);
        RipperBehavior = new RipperBehavior(Terrain);
        RioBehavior = new RioBehavior(Room, Terrain);
        RinkaBehavior = new RinkaBehavior(this, SlotTable, Collision, FrameContext, Projectiles, Room, DeathsAndDrops);
        CeresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { Dispatch = this, Room = Room, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, Collision = Collision, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, RoomStatus = RoomStatus });
        RidleyBehavior = new RidleyBehavior(new RidleyBehaviorDependencies { Dispatch = this, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, DeathsAndDrops = DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, CeresRidleyBehavior = CeresRidleyBehavior, RidleyProjectiles = RidleyProjectiles, RoomStatus = RoomStatus, RoomFx = RoomFx });
        PuyoBehavior = new PuyoBehavior(Room, Terrain);
        PrePhantoonRoomBehavior = new PrePhantoonRoomBehavior(Room, Projectiles);
        PowampBehavior = new PowampBehavior(Terrain, Projectiles, SlotTable, Collision, DeathsAndDrops);
        PolypBehavior = new PolypBehavior(Room, Projectiles);
        PlatformsBehavior = new PlatformsBehavior(Terrain);
        PipeBugsBehavior = new PipeBugsBehavior(SlotTable);
        PhantoonBehavior = new PhantoonBehavior(new PhantoonBehaviorDependencies { Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops });
        OwtchBehavior = new OwtchBehavior(Room);
        OldTourianEscapeShaftWallBehavior = new OldTourianEscapeShaftWallBehavior(Room, Projectiles, Audio);
        NuclearWaffleBehavior = new NuclearWaffleBehavior(Projectiles, SpriteObjects);
        NorfairRioBehavior = new NorfairRioBehavior(Room, Terrain, SlotTable);
        NorfairLavaJumpingEnemyBehavior = new NorfairLavaJumpingEnemyBehavior(Room, SlotTable);
        NoobTubeBehavior = new NoobTubeBehavior(Room, Projectiles);
        NinjaSpacePirateBehavior = new NinjaSpacePirateBehavior(Room, Terrain, Projectiles, SpriteObjects, Presentation, WalkingSpacePirateBehavior);
        MultiviolaBehavior = new MultiviolaBehavior(Terrain);
        BabyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Dispatch = this, Room = Room, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Projectiles = Projectiles, MotherBrainEncounter = MotherBrainEncounter });
        MotherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { Dispatch = this, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, BabyMetroidCutsceneBehavior = BabyMetroidCutsceneBehavior, RinkaBehavior = RinkaBehavior, WalkingSpacePirateBehavior = WalkingSpacePirateBehavior, RidleyProjectiles = RidleyProjectiles, RoomStatus = RoomStatus });
        MorphBallEyeBehavior = new MorphBallEyeBehavior(SlotTable);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, MorphBallEyeBehavior, Room, SlotTable);
        MochtroidBehavior = new MochtroidBehavior(Terrain, Audio, FrameContext, Collision);
        MetroidBehavior = new MetroidBehavior(Room, Terrain, Audio, SpriteObjects, FrameContext, DeathsAndDrops);
        MetareeBehavior = new MetareeBehavior(Projectiles);
        MaridiaLargeSnailBehavior = new MaridiaLargeSnailBehavior(Terrain, Collision);
        MamaTurtleBehavior = new MamaTurtleBehavior(Room, Terrain, Earthquake, SlotTable, Collision);
        MagdolliteBehavior = new MagdolliteBehavior(Room, Projectiles, SlotTable, Presentation, Collision, DeathsAndDrops);
        LowerNorfairRioBehavior = new LowerNorfairRioBehavior(Room, Terrain, SlotTable);
        KzanBehavior = new KzanBehavior(SlotTable);
        KraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, SlotTable = SlotTable, Presentation = Presentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior });
        KiHunterBehavior = new KiHunterBehavior(Room, Terrain, Projectiles, Audio, SlotTable);
        KagoBehavior = new KagoBehavior(Room, Projectiles, Earthquake, SlotTable, DeathsAndDrops, RoomStatus);
        HoppersBehavior = new HoppersBehavior(Room, Terrain);
        HibashiBehavior = new HibashiBehavior(SlotTable);
        GunshipHealthBehavior = new GunshipHealthBehavior(SlotTable);
        GrappleBehavior = new GrappleBehavior(this, Room, SlotTable, DeathsAndDrops);
        FuneNamiheBehavior = new FuneNamiheBehavior(Projectiles);
        FliesBehavior = new FliesBehavior(Room);
        FirefleaBehavior = new FirefleaBehavior(Collision, DeathsAndDrops, RoomStatus);
        FakeKraidBehavior = new FakeKraidBehavior(Room, Terrain, Projectiles, Collision, DeathsAndDrops);
        EyeDoorBehavior = new EyeDoorBehavior(Room, Projectiles);
        EvirBehavior = new EvirBehavior(SlotTable, Collision);
        EtecoonBehavior = new EtecoonBehavior(Terrain, Earthquake, ElevatorBehavior);
        EscapeExplosionsBehavior = new EscapeExplosionsBehavior(Audio, SpriteObjects);
        EscapeAnimalsBehavior = new EscapeAnimalsBehavior(Room, Terrain);
        DraygonBehavior = new DraygonBehavior(new DraygonBehaviorDependencies { Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, RoomStatus = RoomStatus, ScrollCap = ScrollCap });
        DragonBehavior = new DragonBehavior(Projectiles, SlotTable, Collision);
        DownwardGateBehavior = new DownwardGateBehavior(Room, Projectiles);
        DeadTourianBehavior = new DeadTourianBehavior(Room, FrameContext, Projectiles, DeadMonsters);
        DeadTorizoBehavior = new DeadTorizoBehavior(Room, FrameContext, Projectiles, Presentation);
        DachoraBehavior = new DachoraBehavior(Room, Terrain, SlotTable, Presentation);
        CrocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior, RoomStatus = RoomStatus });
        CrawlersBehavior = new CrawlersBehavior(Terrain, Earthquake);
        ChozoStatueBehavior = new ChozoStatueBehavior(Room, Terrain, Projectiles, Audio, SlotTable, Presentation, RoomFx);
        ChootBehavior = new ChootBehavior();
        CeresSteamBehavior = new CeresSteamBehavior(Room, Collision);
        CeresDoorBehavior = new CeresDoorBehavior(Room, Earthquake, Audio, SlotTable, Presentation, Projectiles, RidleyEncounter);
        CacatacBehavior = new CacatacBehavior(Room, Projectiles);
        BullBehavior = new BullBehavior();
        BoyonBehavior = new BoyonBehavior();
        BoulderBehavior = new BoulderBehavior(Terrain, Projectiles);
        BotwoonBehavior = new BotwoonBehavior(Room, Projectiles, SpriteObjects, SlotTable, Presentation, Collision, DeathsAndDrops);
        Instructions = new EnemyInstructionInterpreter(CrocomireBehavior, MotherBrainEncounter, Presentation, Room, this);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { CeresRidleyBehavior = CeresRidleyBehavior, DeadTorizoBehavior = DeadTorizoBehavior, FrameContext = FrameContext, Instructions = Instructions, MotherBrainBehavior = MotherBrainBehavior, Presentation = Presentation, RidleyEncounter = RidleyEncounter, Room = Room, SlotTable = SlotTable });
        BombTorizoBehavior = new BombTorizoBehavior(new BombTorizoBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops });
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
        for (int slotIndex = 0; slotIndex < SlotTable._slots.Length; slotIndex++)
            SlotTable._slots[slotIndex] = new RoomEnemySlot(slotIndex);
    }

    /// <summary>Collision words read from the current slots after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its
    /// corresponding catalog; null does not enable a cartridge-backed fallback.
    /// </summary>
    public EnemyTileArtworkCatalog? TileArtwork
    {
        get => Room.Artwork;
        set => Room.Artwork = value;
    }
    /// <summary>Whether an address-space dependency has been bound by <see cref="Load"/>; this reports binding, not successful completion of every initializer or artwork validation.</summary>
    public bool IsLoaded => Room.IsLoaded;

    /// <summary>
    /// Ports the data-producing parts of <c>LoadEnemies</c>,
    /// <c>ProcessEnemyTilesets</c>, and <c>InitializeEnemies</c> at $A0:8A1E-$8C6C.
    /// </summary>
    public void Load(IRoomEnemyLoad request)
    {
        Ensure.NotNull(request);
        ISnesAddressSpace bus = request.Bus;
        Room.Bus = bus;
        GunshipBehavior._gunshipLoadScenario = request.GunshipScenario;
        FrameContext._deferLoaderTimeCameraReads = request.DeferLoaderTimeCameraReads;
        GunshipBehavior._samusAtEnemyInitialization = request.Samus;
        FrameContext._samusProjectilesForEnemyFrame = null;
        DeathsAndDrops._samusForEnemyDrops = request.Samus;
        // Some enemy routines call GenerateRandomNumber while others, including Alcoon's
        // post-volley bytecode, merely sample the existing WRAM seed: IEnemyRandomNumbers keeps
        // Generate and Read distinct so a sample never silently advances the cartridge RNG.
        Room.Services = request.Services;
        Presentation._vram = request.Vram;
        Presentation._cgram = request.Cgram;
        PopulationLoader.PopulationPointer = request.PopulationPointer;
        PopulationLoader.TilesetPointer = request.TilesetPointer;
        SlotTable.EnemyCount = 0;
        RoomStatus.EnemiesKilled = 0;
        RoomStatus.BossId = 0;
        GunshipBehavior.LastGunshipEvent = GunshipFrameEvent.None;
        BeginEnemySoundRequestFrame();
        // Each species resets its own room state, in the original order.
        foreach (IEnemySpecies species in _species)
            species.ResetForRoomLoad(request);
        GunshipBehavior.GunshipSavePromptPending = false;
        GunshipBehavior.GunshipSaveRequested = false;
        RoomStatus.BeginFrame();
        RoomStatus.EnemyDoorTransitionActive = false;
        Projectiles.LastEnemyProjectileDudSoundEffect = null;
        RoomStatus.FirefleaDarknessLevel = 0;
        Earthquake.EarthquakeTimer = NativeCountdown.Start(0);
        Earthquake.EarthquakeType = 0;
        Earthquake.LastRoomShake = default;
        RidleyEncounter.Ridley = null;
        FrameContext._processAllEnemies = false;
        SpriteObjects.ResetForRoom();
        // Enemy projectiles live in a separate native bank-$86 pool, but room loading
        // destroys them just as decisively as it clears bank-$A0 enemy slots. Without
        // this reset, leaving Ridley's room could carry a fireball (and its stale room
        // collision coordinates) into the destination room.
        Projectiles.ResetForRoom();
        ProcessingWindow._activeEnemyIndexes.Clear();
        ProcessingWindow._interactiveEnemyIndexes.Clear();
        _interactiveCollisionBodies.Clear();
        PopulationLoader._graphicsSet.Clear();
        Drawing.Clear();
        foreach (RoomEnemySlot slot in SlotTable._slots)
            slot.Clear();

        // $A0:8A6D does not even inspect the room's enemy set when the first population
        // word is the terminator. This is observable: an empty room must not overwrite
        // CGRAM or VRAM merely because its state happens to retain a non-empty set pointer.
        RoomEnemyPopulationDefinition population =
            EnemyPopulationLoader.ResolveRoomEnemyPopulation(bus, request.PopulationPointer);
        if (!population.Records.IsEmpty)
            PopulationLoader.LoadGraphicsSet(bus, request.TilesetPointer, request.Vram, request.Cgram);
        PopulationLoader.LoadPopulation(
            bus,
            population,
            request.Level,
            request.Samus,
            request.ControllerInput,
            request.Camera.X);
    }

    /// <summary>
    /// Runs one native enemy pass: executes selected AI and instruction lists, publishes
    /// live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    public void StepFrame(RoomEnemyFrame frame)
    {
        using var terrainScope = new EnemyTerrain.EnemyTerrainScope(Terrain, frame.CollisionPlms);
        Room.EnsureLoaded();
        DeathsAndDrops._samusForEnemyDrops = frame.Samus;
        ScrollCap._samusPreviousPositionCheckpoint = frame.SamusPreviousPositionCheckpoint;
        FrameContext._enemyFrameTimeIsFrozen = frame.TimeIsFrozen;
        FrameContext._samusProjectilesForEnemyFrame = frame.SamusProjectiles;
        Audio.PowerBomb = frame.SharedProjectiles?.PowerBombExplosion;
        // Standalone audits do not own the runtime NMI clock. In that case the enemy-frame
        // counter begins at zero and advances at the same end-of-frame point, which gives
        // Mama Turtle's even-frame shell jitter the same initial phase as retail room load.
        byte enemyNmiFrameCounter8 = frame.NmiFrameCounter8 ?? unchecked((byte)FrameContext._randomEnemyCounter);
        FrameContext._enemyFrameNmiFrameCounter = frame.NmiFrameCounter ?? FrameContext._randomEnemyCounter;
        GunshipBehavior.LastGunshipEvent = GunshipFrameEvent.None;
        BeginEnemySoundRequestFrame();
        foreach (IEnemySpecies species in _species)
            species.BeginEnemyFrame(frame);
        RoomStatus.BeginFrame();
        Projectiles.LastEnemyProjectileDudSoundEffect = null;
        if (!frame.ProcessingListPrepared)
            ProcessingWindow.PrepareEnemyProcessingList(frame.Camera);
        Drawing.Clear();

        foreach (ushort nativeIndex in ProcessingWindow._activeEnemyIndexes)
        {
            RoomEnemySlot slot = SlotTable.SlotFromNativeIndex(nativeIndex);
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
                if (slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
                    frame.SamusProjectiles is not null && frame.SharedProjectiles is not null)
                    PhantoonBehavior.ResolvePhantoonProjectileHits(Room.LoadedBus, frame.SamusProjectiles, frame.SharedProjectiles);
                if (frame.SamusProjectiles is not null && frame.SharedProjectiles is not null)
                    Collision.ResolveOrdinaryProjectileHits(Room.LoadedBus, frame.SamusProjectiles, frame.SharedProjectiles,
                        frame.Samus, nativeIndex);
                // Alpha already updated the bomb slots. Native EnemyMain checks
                // them before this actor's touch and AI, so escape starts now.
                if (frame.SharedProjectiles is not null && frame.SamusProjectiles is not null)
                    Collision.ResolveOrdinaryBombHits(frame.SharedProjectiles, frame.SamusProjectiles, frame.Samus, nativeIndex);
                if (slot.EnemyDefinitionPointer == 0)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.ControllerInput, frame.Level, nativeIndex);
                if (RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer))
                    CeresRidleyBehavior.ResolveRidleyBodySamusContact(frame.Samus);
                if (slot.EnemyDefinitionPointer == 0)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(slot, frame, enemyNmiFrameCounter8);
                // Hurt AI replaces main AI while the hurt bit is set; the first registered match runs.
                // Native selects the lowest AI bit even when its handler is just
                // RTL. Hurt therefore delays frozen AI until flash housekeeping
                // clears bit two; falling through here prematurely clears flash.
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    Ai.FindHurt(slot) is { } hurt)
                {
                    hurt(slot, frame);
                    ranActorAi = true;
                }
                // $A0:9044 selects the handler from the lowest set AI bit alone. A frozen
                // clock without bit four (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (slot.AiHandlerBits & 0x0004) != 0)
                {
                    // Rinka's termination tail clears/replaces the current actor record.
                    // The active-index array was frozen before AI, so skip the remainder
                    // of this stale entry exactly as the private routine's death call does.
                    if (Ai.TryPreemptFrozen(slot))
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // dispatch for this call. Ice removal instead takes the thaw branch
                    // immediately. That branch stores the remaining AI bits into BOTH
                    // words; replacing the clock with literal zero loses retail behavior
                    // when hurt and frozen dispatch coexist.
                    slot.FlashTimer = NativeCountdown.Start(0);
                    bool thaw = slot.FrozenTimer.IsZero ||
                        (frame.Samus is not null &&
                            (frame.Samus.EquippedBeams & (ushort)SamusBeamFlags.Ice) == 0);
                    if (!thaw)
                        slot.FrozenTimer = slot.FrozenTimer.Decrement().Remaining;
                    else
                    {
                        slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0004));
                        slot.FrozenTimer = NativeCountdown.FromRaw(slot.AiHandlerBits);
                    }
                    Ai.RunFrozen(slot);
                    // The native frame clock advances after every selected AI call,
                    // including frozen AI. Instruction gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    RunMainAi(slot, frame, enemyNmiFrameCounter8);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    slot.FrameCounter = unchecked((ushort)(slot.FrameCounter + 1));
                    // `$A0:8FF7` skips the instruction interpreter whenever frozen bit
                    // four is present, even if lower-priority hurt bit two selected an
                    // enemy-specific hurt callback above. Testing only `ranActorAi` here
                    // incorrectly advanced a Metroid's body animation while its frozen
                    // callback still owned the two composited outer sprite objects.
                    if ((slot.AiHandlerBits & 0x0004) == 0 &&
                        slot.Properties.HasAny(EnemyProperties.ProcessInstructions))
                        Instructions.ProcessInstructions(
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
                Drawing.Enqueue(slot.Layer & 7, nativeIndex);

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
                    slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0002));
            }
        }

        // DetermineWhichEnemiesToProcess freezes only the index list. Later bank-$94
        // collision routines dereference the live slot words, so publish bodies after AI
        // has had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (ushort nativeIndex in ProcessingWindow._interactiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotTable.SlotFromNativeIndex(nativeIndex);
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                nativeIndex,
                slot.Position.Whole,
                new(slot.Radius.X, slot.Radius.Y),
                slot.FrozenTimer.RawValue,
                slot.Properties));
        }
        FrameContext._randomEnemyCounter = unchecked((ushort)(FrameContext._randomEnemyCounter + 1));
        DeadTorizoBehavior.QueueDeadTorizoFrameVramTransfers(frame.VramWriteQueue);
        DeadSidehopperBehavior.QueueDeadSidehopperFrameVramTransfers(frame.VramWriteQueue);
        frame.Samus?.Kinematics.ClearSolidEnemyCollisionIndexes();
        WorkRobotBehavior.StepWorkRobotPaletteAnimation();
        MagdolliteBehavior.StepMagdollitePaletteAnimation();
        BlueBrinstarFaceBlockBehavior.StepBlueBrinstarFaceBlockPaletteAnimation();
        if (!frame.TimeIsFrozen)
            SpriteObjects.StepRoomSpriteObjects();
        CollectLegacyEnemyAudioRequests();
    }

    /// <summary>
    /// Copies population and definition fields into a physical slot, resolving room graphics
    /// indexes and recording a spawn snapshot before initialization AI runs.
    /// </summary>
    internal void InitializeSlotFromDefinition(
        RoomEnemySlot slot,
        RoomEnemyPopulationRecord population,
        RoomEnemyDefinition definition)
    {
        (ushort tileIndex, ushort paletteIndex) = PopulationLoader.FindGraphicsIndexes(population.DefinitionPointer);

        slot.EnemyDefinitionPointer = population.DefinitionPointer;
        slot.Definition = definition;
        slot.Radius = new(definition.Radius.X, definition.Radius.Y);
        slot.Health = definition.Health;
        slot.Layer = definition.Layer;
        slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
        slot.CurrentInstruction = population.InitializationParameter;
        _ = population.Properties.ReadFlagsChecked();
        slot.Properties = population.Properties;
        slot.ExtraProperties = population.ExtraProperties;
        slot.AiHandlerBits = 0;
        slot.Parameter1 = population.Parameter1;
        slot.Parameter2 = population.Parameter2;
        slot.Timer = NativeCountdown.Start(0);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.FrameCounter = 0;
        slot.AiBank = definition.Bank;
        slot.HurtAiTime = definition.HurtAiTime;
        slot.PaletteIndex = paletteIndex;
        slot.VramTilesIndex = tileIndex;
        slot.Spawn = new RoomEnemySpawnSnapshot(
            population,
            tileIndex);
    }

    /// <summary>Dispatches the definition's banked initialization pointer for the newly loaded slot.</summary>
    internal void RunInitializationAi(
        RoomEnemySlot slot,
        RoomLevelData? level = null,
        SamusState? samus = null,
        ushort controllerInput = 0,
        ushort cameraX = 0)
    {
        int address = ((int)new SnesAddress(slot.Definition.Bank, slot.Definition.InitializationAiPointer));
        var routine = (EnemyAiRoutine)address;
        if (!Enum.IsDefined(routine))
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        if (!Ai.TryGetInitialization(routine, slot, out EnemyInitializationAiHandler? handler))
        {
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");

        }
        handler(slot, level, samus, controllerInput, cameraX);
    }

    /// <summary>Exposes physical-slot population setup to the enemy loader.</summary>
    void IEnemyDispatch.InitializeSlotFromDefinition(
        RoomEnemySlot slot, RoomEnemyPopulationRecord population, RoomEnemyDefinition definition) =>
        InitializeSlotFromDefinition(slot, population, definition);

    /// <summary>Lets the population loader invoke the slot's compiled initialization handler.</summary>
    void IEnemyDispatch.RunInitializationAi(RoomEnemySlot slot, RoomLevelData? level, SamusState? samus,
        ushort controllerInput, ushort cameraX) =>
        RunInitializationAi(slot, level, samus, controllerInput, cameraX);

    /// <summary>Routes a scheduled enemy update through its banked main AI handler.</summary>
    void IEnemyDispatch.RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte nmiFrameCounter8) =>
        RunMainAi(slot, frame, nmiFrameCounter8);

    /// <summary>Shares the lazily rebuilt handler table with the enemy scheduler.</summary>
    EnemyAiTable IEnemyDispatch.Ai => Ai;

    /// <summary>Exposes native slot offsets selected for Samus collision checks this frame.</summary>
    IReadOnlyList<ushort> IEnemyDispatch.InteractiveEnemyIndexes => ProcessingWindow._interactiveEnemyIndexes;

    /// <summary>Dispatches a loaded slot's banked main AI pointer through the compiled table.</summary>
    internal void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte nmiFrameCounter8)
    {
        int address = ((int)new SnesAddress(slot.Definition.Bank, slot.Definition.MainAiPointer));
        var routine = (EnemyAiRoutine)address;
        if (!Enum.IsDefined(routine))
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        if (!Ai.TryGetMain(routine, slot, out EnemyMainAiHandler? handler))
        {
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");

        }
        handler(slot, frame, nmiFrameCounter8);
    }

    /// <summary>Ports <c>Handle_Room_Shaking</c> at $A0:8687-$8711: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        Room.EnsureLoaded();
        // This runs after drawing on the cartridge, so the two-frame actor timer becomes visible
        // on the following frame rather than retroactively changing already-emitted OAM.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (ushort nativeIndex in ProcessingWindow._activeEnemyIndexes)
                SlotTable.SlotFromNativeIndex(nativeIndex).ShakeTimer = NativeCountdown.Start(RoomFxRomData.Earthquake.EnemyShakeDuration);
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
        HibashiBehavior.QueueLegacySounds(Audio);
        NuclearWaffleBehavior.QueueLegacySounds(Audio);
        FakeKraidBehavior.QueueLegacySounds(Audio);
        WorkRobotBehavior.QueueLegacySounds(Audio);
        WalkingSpacePirateBehavior.QueueLegacySounds(Audio);
        RioBehavior.QueueLegacySounds(Audio);
        NorfairLavaJumpingEnemyBehavior.QueueLegacySounds(Audio);
        NorfairRioBehavior.QueueLegacySounds(Audio);
        LowerNorfairRioBehavior.QueueLegacySounds(Audio);
        MaridiaLargeSnailBehavior.QueueLegacySounds(Audio);
        DragonBehavior.QueueLegacySounds(Audio);
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
    /// Rebuilds the handler table after construction or debugger restore. Species handlers
    /// precede shared handlers; owned instructions preserve first-match registration order.
    /// </summary>
    private EnemyAiTable BuildEnemyAiTable()
    {
        var table = new EnemyAiTable();
        RegisterInitializationAi(table);
        RegisterMainAi(table);
        RegisterOwnedInstructions(table);
        RegisterTouchHandlers(table);
        RegisterShotReactions(table);
        RegisterBombReactions(table);
        RegisterPowerBombReactions(table);
        RegisterHurtAi(table);
        RegisterFrozenAi(table);
        return table;
    }

    /// <summary>Registers species initializers before generic and cross-species room-load handlers.</summary>
    private void RegisterInitializationAi(EnemyAiTable table)
    {
        // Species register first; generic and cross-species registrations follow.
        AlcoonBehavior.RegisterAlcoonInitializationAi(table);
        AtomicBehavior.RegisterAtomicInitializationAi(table);
        BabyMetroidCutsceneBehavior.RegisterBabyMetroidCutsceneInitializationAi(table);
        BeetomBehavior.RegisterBeetomInitializationAi(table);
        BlueBrinstarFaceBlockBehavior.RegisterBlueBrinstarFaceBlockInitializationAi(table);
        BombTorizoBehavior.RegisterBombTorizoInitializationAi(table);
        BotwoonBehavior.RegisterBotwoonInitializationAi(table);
        BoulderBehavior.RegisterBoulderInitializationAi(table);
        BoyonBehavior.RegisterBoyonInitializationAi(table);
        BullBehavior.RegisterBullInitializationAi(table);
        CacatacBehavior.RegisterCacatacInitializationAi(table);
        CeresDoorBehavior.RegisterCeresDoorInitializationAi(table);
        CeresRidleyBehavior.RegisterCeresRidleyInitializationAi(table);
        CeresSteamBehavior.RegisterCeresSteamInitializationAi(table);
        ChootBehavior.RegisterChootInitializationAi(table);
        ChozoStatueBehavior.RegisterChozoStatueInitializationAi(table);
        CrawlersBehavior.RegisterCrawlersInitializationAi(table);
        CrocomireBehavior.RegisterCrocomireInitializationAi(table);
        DachoraBehavior.RegisterDachoraInitializationAi(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperInitializationAi(table);
        DeadTorizoBehavior.RegisterDeadTorizoInitializationAi(table);
        DeadTourianBehavior.RegisterDeadTourianInitializationAi(table);
        DragonBehavior.RegisterDragonInitializationAi(table);
        DraygonBehavior.RegisterDraygonInitializationAi(table);
        ElevatorBehavior.RegisterElevatorInitializationAi(table);
        EscapeAnimalsBehavior.RegisterEscapeAnimalsInitializationAi(table);
        EtecoonBehavior.RegisterEtecoonInitializationAi(table);
        EvirBehavior.RegisterEvirInitializationAi(table);
        FakeKraidBehavior.RegisterFakeKraidInitializationAi(table);
        FirefleaBehavior.RegisterFirefleaInitializationAi(table);
        FliesBehavior.RegisterFliesInitializationAi(table);
        FuneNamiheBehavior.RegisterFuneNamiheInitializationAi(table);
        HibashiBehavior.RegisterHibashiInitializationAi(table);
        HoppersBehavior.RegisterHoppersInitializationAi(table);
        KagoBehavior.RegisterKagoInitializationAi(table);
        KiHunterBehavior.RegisterKiHunterInitializationAi(table);
        KraidBehavior.RegisterKraidInitializationAi(table);
        KzanBehavior.RegisterKzanInitializationAi(table);
        LowerNorfairRioBehavior.RegisterLowerNorfairRioInitializationAi(table);
        MagdolliteBehavior.RegisterMagdolliteInitializationAi(table);
        MamaTurtleBehavior.RegisterMamaTurtleInitializationAi(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailInitializationAi(table);
        MetareeBehavior.RegisterMetareeInitializationAi(table);
        MetroidBehavior.RegisterMetroidInitializationAi(table);
        MochtroidBehavior.RegisterMochtroidInitializationAi(table);
        MorphBallEyeBehavior.RegisterMorphBallEyeInitializationAi(table);
        MotherBrainBehavior.RegisterMotherBrainInitializationAi(table);
        MultiviolaBehavior.RegisterMultiviolaInitializationAi(table);
        NinjaSpacePirateBehavior.RegisterNinjaSpacePirateInitializationAi(table);
        NorfairLavaJumpingEnemyBehavior.RegisterNorfairLavaJumpingEnemyInitializationAi(table);
        NorfairRioBehavior.RegisterNorfairRioInitializationAi(table);
        NuclearWaffleBehavior.RegisterNuclearWaffleInitializationAi(table);
        OwtchBehavior.RegisterOwtchInitializationAi(table);
        PhantoonBehavior.RegisterPhantoonInitializationAi(table);
        PipeBugsBehavior.RegisterPipeBugsInitializationAi(table);
        PlatformsBehavior.RegisterPlatformsInitializationAi(table);
        PolypBehavior.RegisterPolypInitializationAi(table);
        PowampBehavior.RegisterPowampInitializationAi(table);
        PuyoBehavior.RegisterPuyoInitializationAi(table);
        RidleyBehavior.RegisterRidleyInitializationAi(table);
        RinkaBehavior.RegisterRinkaInitializationAi(table);
        RioBehavior.RegisterRioInitializationAi(table);
        RipperBehavior.RegisterRipperInitializationAi(table);
        SbugBehavior.RegisterSbugInitializationAi(table);
        ShaktoolBehavior.RegisterShaktoolInitializationAi(table);
        ShitroidBehavior.RegisterShitroidInitializationAi(table);
        ShuttersBehavior.RegisterShuttersInitializationAi(table);
        SkreeBehavior.RegisterSkreeInitializationAi(table);
        SkulteraBehavior.RegisterSkulteraInitializationAi(table);
        SparkBehavior.RegisterSparkInitializationAi(table);
        SporeSpawnBehavior.RegisterSporeSpawnInitializationAi(table);
        StokeBehavior.RegisterStokeInitializationAi(table);
        TourianEntranceStatueBehavior.RegisterTourianEntranceStatueInitializationAi(table);
        WalkingSpacePirateBehavior.RegisterWalkingSpacePirateInitializationAi(table);
        WallSpacePirateBehavior.RegisterWallSpacePirateInitializationAi(table);
        WaverBehavior.RegisterWaverInitializationAi(table);
        WorkRobotBehavior.RegisterWorkRobotInitializationAi(table);
        WreckedShipGhostBehavior.RegisterWreckedShipGhostInitializationAi(table);
        YappingMawBehavior.RegisterYappingMawInitializationAi(table);
        YardBehavior.RegisterYardInitializationAi(table);
        ZebetiteBehavior.RegisterZebetiteInitializationAi(table);
        ZoaBehavior.RegisterZoaInitializationAi(table);

        GunshipBehavior.RegisterGunshipTopInitializationAi(table);

        GunshipBehavior.RegisterGunshipBottomInitializationAi(table);

        table.RegisterInitialization(EnemyAiRoutine.RTL_A2804C, null, (slot, level, samus, controllerInput, cameraX) =>
        { });
    }

    /// <summary>Registers the per-frame routines addressed by enemy definitions.</summary>
    private void RegisterMainAi(EnemyAiTable table)
    {
        // Species register first; generic and cross-species registrations follow.
        AlcoonBehavior.RegisterAlcoonMainAi(table);
        AtomicBehavior.RegisterAtomicMainAi(table);
        BabyMetroidCutsceneBehavior.RegisterBabyMetroidCutsceneMainAi(table);
        BeetomBehavior.RegisterBeetomMainAi(table);
        BlueBrinstarFaceBlockBehavior.RegisterBlueBrinstarFaceBlockMainAi(table);
        BombTorizoBehavior.RegisterBombTorizoMainAi(table);
        BotwoonBehavior.RegisterBotwoonMainAi(table);
        BoulderBehavior.RegisterBoulderMainAi(table);
        BoyonBehavior.RegisterBoyonMainAi(table);
        BullBehavior.RegisterBullMainAi(table);
        CacatacBehavior.RegisterCacatacMainAi(table);
        CeresDoorBehavior.RegisterCeresDoorMainAi(table);
        CeresRidleyBehavior.RegisterCeresRidleyMainAi(table);
        CeresSteamBehavior.RegisterCeresSteamMainAi(table);
        ChootBehavior.RegisterChootMainAi(table);
        ChozoStatueBehavior.RegisterChozoStatueMainAi(table);
        CrawlersBehavior.RegisterCrawlersMainAi(table);
        CrocomireBehavior.RegisterCrocomireMainAi(table);
        DachoraBehavior.RegisterDachoraMainAi(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperMainAi(table);
        DeadTorizoBehavior.RegisterDeadTorizoMainAi(table);
        DeadTourianBehavior.RegisterDeadTourianMainAi(table);
        DragonBehavior.RegisterDragonMainAi(table);
        DraygonBehavior.RegisterDraygonMainAi(table);
        ElevatorBehavior.RegisterElevatorMainAi(table);
        EscapeAnimalsBehavior.RegisterEscapeAnimalsMainAi(table);
        EtecoonBehavior.RegisterEtecoonMainAi(table);
        EvirBehavior.RegisterEvirMainAi(table);
        FakeKraidBehavior.RegisterFakeKraidMainAi(table);
        FirefleaBehavior.RegisterFirefleaMainAi(table);
        FliesBehavior.RegisterFliesMainAi(table);
        FuneNamiheBehavior.RegisterFuneNamiheMainAi(table);
        HibashiBehavior.RegisterHibashiMainAi(table);
        HoppersBehavior.RegisterHoppersMainAi(table);
        KagoBehavior.RegisterKagoMainAi(table);
        KiHunterBehavior.RegisterKiHunterMainAi(table);
        KraidBehavior.RegisterKraidMainAi(table);
        KzanBehavior.RegisterKzanMainAi(table);
        LowerNorfairRioBehavior.RegisterLowerNorfairRioMainAi(table);
        MagdolliteBehavior.RegisterMagdolliteMainAi(table);
        MamaTurtleBehavior.RegisterMamaTurtleMainAi(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailMainAi(table);
        MetareeBehavior.RegisterMetareeMainAi(table);
        MetroidBehavior.RegisterMetroidMainAi(table);
        MochtroidBehavior.RegisterMochtroidMainAi(table);
        MorphBallEyeBehavior.RegisterMorphBallEyeMainAi(table);
        MotherBrainBehavior.RegisterMotherBrainMainAi(table);
        MultiviolaBehavior.RegisterMultiviolaMainAi(table);
        NinjaSpacePirateBehavior.RegisterNinjaSpacePirateMainAi(table);
        NorfairLavaJumpingEnemyBehavior.RegisterNorfairLavaJumpingEnemyMainAi(table);
        NorfairRioBehavior.RegisterNorfairRioMainAi(table);
        NuclearWaffleBehavior.RegisterNuclearWaffleMainAi(table);
        OwtchBehavior.RegisterOwtchMainAi(table);
        PhantoonBehavior.RegisterPhantoonMainAi(table);
        PipeBugsBehavior.RegisterPipeBugsMainAi(table);
        PlatformsBehavior.RegisterPlatformsMainAi(table);
        PolypBehavior.RegisterPolypMainAi(table);
        PowampBehavior.RegisterPowampMainAi(table);
        PuyoBehavior.RegisterPuyoMainAi(table);
        RidleyBehavior.RegisterRidleyMainAi(table);
        RinkaBehavior.RegisterRinkaMainAi(table);
        RioBehavior.RegisterRioMainAi(table);
        RipperBehavior.RegisterRipperMainAi(table);
        SbugBehavior.RegisterSbugMainAi(table);
        ShaktoolBehavior.RegisterShaktoolMainAi(table);
        ShitroidBehavior.RegisterShitroidMainAi(table);
        ShuttersBehavior.RegisterShuttersMainAi(table);
        SkreeBehavior.RegisterSkreeMainAi(table);
        SkulteraBehavior.RegisterSkulteraMainAi(table);
        SparkBehavior.RegisterSparkMainAi(table);
        SporeSpawnBehavior.RegisterSporeSpawnMainAi(table);
        StokeBehavior.RegisterStokeMainAi(table);
        WalkingSpacePirateBehavior.RegisterWalkingSpacePirateMainAi(table);
        WallSpacePirateBehavior.RegisterWallSpacePirateMainAi(table);
        WaverBehavior.RegisterWaverMainAi(table);
        WorkRobotBehavior.RegisterWorkRobotMainAi(table);
        WreckedShipGhostBehavior.RegisterWreckedShipGhostMainAi(table);
        YappingMawBehavior.RegisterYappingMawMainAi(table);
        YardBehavior.RegisterYardMainAi(table);
        ZebetiteBehavior.RegisterZebetiteMainAi(table);
        ZebetiteBehavior.RegisterZebetiteTypedAnimations(table);
        ZoaBehavior.RegisterZoaMainAi(table);
        table.RegisterMain(EnemyAiRoutine.RTL_A3804C, null, (slot, frame, nmiFrameCounter8) =>
        { });

        CrocomireBehavior.RegisterCrocomireTongueMainAi(table);

        GunshipBehavior.RegisterGunshipMainAi(table);
        GunshipBehavior.RegisterGunshipTypedAnimations(table);

        table.RegisterMain(EnemyAiRoutine.RTL_A2804C, null, (slot, frame, nmiFrameCounter8) =>
        { });

        PhantoonBehavior.RegisterPhantoonEyeMainAi(table);

        WorkRobotBehavior.RegisterUnpoweredWorkRobotMainAi(table);

        EscapeAnimalsBehavior.RegisterEscapeDachoraMainAi(table);

        PhantoonBehavior.RegisterPhantoonMouthAndTentaclesMainAi(table);

        TourianEntranceStatueBehavior.RegisterTourianStatueMainAi(table);
    }

    // Owners accept disjoint words; the order mirrors the original single-switch interpreter.
    /// <summary>Installs species instruction callbacks before shared interpreter callbacks.</summary>
    private void RegisterOwnedInstructions(EnemyAiTable table)
    {
        BabyMetroidCutsceneBehavior.RegisterBabyMetroidCutsceneTypedAnimations(table);
        BoyonBehavior.RegisterBoyonOwnedInstructions(table);
        StokeBehavior.RegisterStokeOwnedInstructions(table);
        MamaTurtleBehavior.RegisterMamaTurtleOwnedInstructions(table);
        DragonBehavior.RegisterDragonOwnedInstructions(table);
        KraidBehavior.RegisterKraidTypedAnimations(table);
        PhantoonBehavior.RegisterPhantoonTypedAnimations(table);
        MotherBrainBehavior.RegisterMotherBrainTypedAnimations(table);
        MotherBrainBehavior.RegisterMotherBrainTubeTypedAnimations(table);
        DraygonBehavior.RegisterDraygonTypedAnimations(table);
        BotwoonBehavior.RegisterBotwoonTypedAnimations(table);
        CrocomireBehavior.RegisterCrocomireTypedAnimations(table);
        WallSpacePirateBehavior.RegisterWallSpacePirateOwnedInstructions(table);
        NinjaSpacePirateBehavior.RegisterNinjaSpacePirateOwnedInstructions(table);
        WorkRobotBehavior.RegisterWorkRobotOwnedInstructions(table);
        BeetomBehavior.RegisterBeetomOwnedInstructions(table);
        YappingMawBehavior.RegisterYappingMawOwnedInstructions(table);
        CacatacBehavior.RegisterCacatacOwnedInstructions(table);
        OwtchBehavior.RegisterOwtchOwnedInstructions(table);
        FuneNamiheBehavior.RegisterFuneNamiheOwnedInstructions(table);
        EvirBehavior.RegisterEvirOwnedInstructions(table);
        YardBehavior.RegisterYardOwnedInstructions(table);
        HoppersBehavior.RegisterHoppersOwnedInstructions(table);
        ZoaBehavior.RegisterZoaTypedAnimations(table);
        MetroidBehavior.RegisterMetroidOwnedInstructions(table);
        CrawlersBehavior.RegisterCrawlersOwnedInstructions(table);
        SkreeBehavior.RegisterSkreeOwnedInstructions(table);
        WaverBehavior.RegisterWaverOwnedInstructions(table);
        MetareeBehavior.RegisterMetareeOwnedInstructions(table);
        SkulteraBehavior.RegisterSkulteraOwnedInstructions(table);
        PlatformsBehavior.RegisterPlatformsOwnedInstructions(table);
        AlcoonBehavior.RegisterAlcoonOwnedInstructions(table);
        KiHunterBehavior.RegisterKiHunterOwnedInstructions(table);
        SparkBehavior.RegisterSparkOwnedInstructions(table);
        HibashiBehavior.RegisterHibashiOwnedInstructions(table);
        FakeKraidBehavior.RegisterFakeKraidOwnedInstructions(table);
        WalkingSpacePirateBehavior.RegisterWalkingSpacePirateOwnedInstructions(table);
        RidleyBehavior.RegisterRidleyTypedAnimations(table);
        CeresSteamBehavior.RegisterCeresSteamTypedAnimations(table);
        CeresDoorBehavior.RegisterCeresDoorTypedAnimations(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperOwnedInstructions(table);
        ShitroidBehavior.RegisterShitroidTypedAnimations(table);
        SporeSpawnBehavior.RegisterSporeSpawnTypedAnimations(table);
        RinkaBehavior.RegisterRinkaOwnedInstructions(table);
        RioBehavior.RegisterRioOwnedInstructions(table);
        NorfairLavaJumpingEnemyBehavior.RegisterNorfairLavaJumpingEnemyOwnedInstructions(table);
        NorfairRioBehavior.RegisterNorfairRioOwnedInstructions(table);
        LowerNorfairRioBehavior.RegisterLowerNorfairRioOwnedInstructions(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailOwnedInstructions(table);
        MagdolliteBehavior.RegisterMagdolliteOwnedInstructions(table);
        ShaktoolBehavior.RegisterShaktoolOwnedInstructions(table);
        ChozoStatueBehavior.RegisterChozoStatueOwnedInstructions(table);
        EscapeAnimalsBehavior.RegisterEscapeAnimalsTypedAnimations(table);
        RipperBehavior.RegisterRipperTypedAnimations(table);
        PolypBehavior.RegisterPolypTypedAnimations(table);
        KzanBehavior.RegisterKzanTypedAnimations(table);
        KagoBehavior.RegisterKagoTypedAnimations(table);
        FliesBehavior.RegisterFliesTypedAnimations(table);
        NuclearWaffleBehavior.RegisterNuclearWaffleTypedAnimations(table);
        BombTorizoBehavior.RegisterBombTorizoTypedAnimations(table);
}

    // Each species registers its own run of reactions; first match wins, so the interleaving below is the
    // established precedence between species and shared registrations.
    /// <summary>Maps enemy-definition bomb reaction pointers to translated handlers.</summary>
    private void RegisterBombReactions(EnemyAiTable table)
    {
        CrocomireBehavior.RegisterCrocomireBombReactions(table);
        KraidBehavior.RegisterKraidBombReactions(table);

        NinjaSpacePirateBehavior.RegisterGoldNinjaPirateBombReactions(table);
        ShuttersBehavior.RegisterShuttersBombReactions(table);
        MetroidBehavior.RegisterMetroidBombReactions(table);
        ShitroidBehavior.RegisterShitroidBombReactions(table);
        CeresRidleyBehavior.RegisterCeresRidleyBombReactions(table);
        BombTorizoBehavior.RegisterBombTorizoBombReactions(table);

        // `$A9:B503` is `CreateDudShot`. The multibox bomb walker has already marked the physical explosion, and the
        // body callback performs no health, phase, or multipart-state work of its own.
        MotherBrainBehavior.RegisterMotherBrainBodyBombReactions(table);
        MotherBrainBehavior.RegisterMotherBrainBombReactions(table);
        DeadTorizoBehavior.RegisterDeadTorizoBombReactions(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperBombReactions(table);
        DeadTourianBehavior.RegisterDeadTourianBombReactions(table);
        // `$A5:8046` is CreateDudShot. The native extended walker has already marked the physical bomb; no
        // vulnerability, health, or boss state follows. Bomb-owned dud sprite/sound presentation remains a shared
        // projectile-system seam, so the enemy-visible contract ends here.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.DraygonBody or EnemyDefinitionId.SporeSpawn &&
                                           c.SelectedShotAi == EnemyInteractionCallback.DudShot,
            static _ => { });
        // `$A8:E70E/$E91D` do not call common shot AI. Their entire callback clears direction bit $10, undoing the
        // mark installed immediately above and allowing the physical bomb actor to remain untouched.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Spark &&
                                           c.SelectedShotAi == EnemyInteractionCallback.SparkShot ||
                                           c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock &&
                                           c.SelectedShotAi == EnemyInteractionCallback.BlueBrinstarFaceBlockShot,
            c => c.Bomb.Direction = unchecked((ushort)(c.Bomb.Direction & 0xffef)));
        PhantoonBehavior.RegisterPhantoonBombReactions(table);
        MamaTurtleBehavior.RegisterMamaTurtleBombReactions(table);
        RinkaBehavior.RegisterRinkaBombReactions(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailBombReactions(table);
        RipperBehavior.RegisterRipperBombReactions(table);
        DragonBehavior.RegisterDragonBombReactions(table);
        // `$A3:8B0F` restores the dying actor's graphics long enough to seed four debris actors, then clears both
        // words. The host death flag does not erase the record, so the exact source remains here.
        MetareeBehavior.RegisterMetareeBombReactions(table);
        FirefleaBehavior.RegisterFirefleaBombReactions(table);
        FakeKraidBehavior.RegisterFakeKraidBombReactions(table);
        PlatformsBehavior.RegisterPlatformsBombReactions(table);
        // Skree's callback owns its four-piece burst and death animation because it called
        // `NormalEnemyShotAiSkipDeathAnim`. A normal bomb selects death variant zero (only missiles select two).
        SkreeBehavior.RegisterSkreeBombReactions(table);
        ZebetiteBehavior.RegisterZebetiteBombReactions(table);
        SporeSpawnBehavior.RegisterSporeSpawnBombReactions(table);
        DraygonBehavior.RegisterDraygonBombReactions(table);
        BotwoonBehavior.RegisterBotwoonBombReactions(table);
        RidleyBehavior.RegisterRidleyBombReactions(table);
        ShaktoolBehavior.RegisterShaktoolBombReactions(table);
        ShuttersBehavior.RegisterDestroyableShutterBombReactions(table);
        EvirBehavior.RegisterEvirBombReactions(table);
        YappingMawBehavior.RegisterYappingMawBombReactions(table);
        KagoBehavior.RegisterKagoBombReactions(table);
        MagdolliteBehavior.RegisterMagdolliteBombReactions(table);
        BeetomBehavior.RegisterBeetomBombReactions(table);
        PowampBehavior.RegisterPowampBombReactions(table);
        WorkRobotBehavior.RegisterWorkRobotBombReactions(table);
        BullBehavior.RegisterBullBombReactions(table);
        KiHunterBehavior.RegisterKiHunterBombReactions(table);
        OwtchBehavior.RegisterOwtchBombReactions(table);
}

    // Each species registers its own run of reactions; first match wins, so the interleaving below is the
    // established precedence between species and shared registrations.
    // Matches are mutually exclusive: each names its definition and header reaction callback.
    /// <summary>Maps banked power-bomb reaction pointers for each supported species.</summary>
    private void RegisterPowerBombReactions(EnemyAiTable table)
    {
        // These headers install private callbacks instead of falling through common power-bomb damage. A literal RTL
        // intentionally does nothing; the shutter callbacks only run their trigger state machines. All still
        // receive the native process-off-screen bit after a qualifying ellipse overlap. Literal no-ops are
        // classified by their compiled bank-qualified identities without fetching executable ROM bytes.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => EnemyPowerBombCallbackDefinitions.IsLiteralNoOp(
            enemy.Definition.Bank, enemy.Definition.PowerBombReactionPointer))
        {
            ReplacesCommon = static (_, _) => { },
        });
        ShuttersBehavior.RegisterShuttersPowerBombReactions(table);
        CrocomireBehavior.RegisterCrocomirePowerBombReactions(table);
        DeadTorizoBehavior.RegisterDeadTorizoPowerBombReactions(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperPowerBombReactions(table);
        DeadTourianBehavior.RegisterDeadTourianPowerBombReactions(table);
        ShitroidBehavior.RegisterShitroidPowerBombReactions(table);
        FirefleaBehavior.RegisterFirefleaPowerBombReactions(table);
        MetroidBehavior.RegisterMetroidPowerBombReactions(table);
        FakeKraidBehavior.RegisterFakeKraidPowerBombReactions(table);
        PowampBehavior.RegisterPowampPowerBombReactions(table);
        MagdolliteBehavior.RegisterMagdollitePowerBombReactions(table);
        RinkaBehavior.RegisterRinkaPowerBombReactions(table);
        DragonBehavior.RegisterDragonPowerBombReactions(table);
        EvirBehavior.RegisterEvirPowerBombReactions(table);
        KiHunterBehavior.RegisterKiHunterPowerBombReactions(table);
        BotwoonBehavior.RegisterBotwoonPowerBombReactions(table);
        WallSpacePirateBehavior.RegisterWallSpacePiratePowerBombReactions(table);
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.RidleyPowerBomb)
        {
            ExemptFromGenericDeath = true,
        });
        DraygonBehavior.RegisterDraygonPowerBombReactions(table);
}

    /// <summary>Advances enemy-projectile actors after Samus movement and before PLMs.</summary>
    public void StepEnemyProjectileInstructions(
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera = default,
        byte? nmiFrameCounter8 = null,
        SamusBombProjectileSystem? samusBombs = null,
        BackgroundScrollState? backgroundScroll = null,
        ushort? nmiFrameCounter = null)
    {
        Room.EnsureLoaded();
        DeathsAndDrops._samusForEnemyDrops = samus;
        PrePhantoonRoomBehavior._enemyProjectileBackgroundScroll = backgroundScroll;
        DeathsAndDrops.LastEnemyPickupSoundEffect = null;
        DeathsAndDrops.LastCollectedEnemyPickup = null;
        DeathsAndDrops.LastEnemyDeathSoundEffectLibrary2 = null;
        Projectiles.RunPass(level, samus, camera, nmiFrameCounter8, samusBombs, nmiFrameCounter);
    }

    /// <summary>
    /// Registers species projectile handlers before generic and cross-species handlers,
    /// preserving the keyed pre-instruction and ordered instruction-interceptor dispatch.
    /// </summary>
    void IEnemyProjectileHandlerSource.RegisterEnemyProjectileHandlers(EnemyProjectileHandlerTable table)
    {
        // Species register first; generic and cross-species registrations follow.
        AlcoonBehavior.RegisterAlcoonProjectiles(table);
        BombTorizoBehavior.RegisterBombTorizoProjectiles(table);
        BotwoonBehavior.RegisterBotwoonProjectiles(table);
        CacatacBehavior.RegisterCacatacProjectiles(table);
        CrocomireBehavior.RegisterCrocomireProjectiles(table);
        DownwardGateBehavior.RegisterDownwardGateProjectiles(table);
        DragonBehavior.RegisterDragonProjectiles(table);
        DraygonBehavior.RegisterDraygonProjectiles(table);
        EyeDoorBehavior.RegisterEyeDoorProjectiles(table);
        FakeKraidBehavior.RegisterFakeKraidProjectiles(table);
        FallingSparksBehavior.RegisterFallingSparksProjectiles(table);
        FuneNamiheBehavior.RegisterFuneNamiheProjectiles(table);
        KagoBehavior.RegisterKagoProjectiles(table);
        KiHunterBehavior.RegisterKiHunterProjectiles(table);
        KraidBehavior.RegisterKraidProjectiles(table);
        KraidBehavior.RegisterKraidRockAnimations(table);
        MagdolliteBehavior.RegisterMagdolliteProjectiles(table);
        MotherBrainBehavior.RegisterMotherBrainProjectiles(table);
        NinjaSpacePirateBehavior.RegisterNinjaSpacePirateProjectiles(table);
        NoobTubeBehavior.RegisterNoobTubeProjectiles(table);
        PhantoonBehavior.RegisterPhantoonProjectiles(table);
        PolypBehavior.RegisterPolypProjectiles(table);
        PowampBehavior.RegisterPowampProjectiles(table);
        PrePhantoonRoomBehavior.RegisterPrePhantoonRoomProjectiles(table);
        ShaktoolBehavior.RegisterShaktoolProjectiles(table);
        SporeSpawnBehavior.RegisterSporeSpawnProjectiles(table);
        StokeBehavior.RegisterStokeProjectiles(table);
        TourianEntranceStatueBehavior.RegisterTourianEntranceStatueProjectiles(table);
        TourianUnlockBehavior.RegisterTourianUnlockProjectiles(table);
        WalkingSpacePirateBehavior.RegisterWalkingSpacePirateProjectiles(table);
        WorkRobotBehavior.RegisterWorkRobotProjectiles(table);

        DeathsAndDrops.RegisterPickupPreInstruction(table);

        RinkaBehavior.RegisterMiscDustPreInstruction(table);

        RidleyProjectiles.RegisterFireballPreInstructions(table);

        EnemyProjectileEngine.RegisterMetalSkreeParticlePreInstruction(table);

        KraidBehavior.RegisterKraidRockSpitPreInstruction(table);

        Projectiles.RegisterCeresFallingTilePreInstruction(table);

        EnemyProjectileEngine.RegisterGunshipDustCues(table);


        OldTourianEscapeShaftWallBehavior.RegisterWallExplosionSoundInstruction(table);

        NoobTubeBehavior.RegisterFallingAngleInstructions(table);


        RidleyProjectiles.RegisterFireballCues(table);

        CrocomireBehavior.RegisterCrocomireDropProjectiles(table);

        DeathsAndDrops.RegisterDeathExplosionInstructions(table);
    }

    // Each species registers its own run of reactions; first match wins, so the interleaving below is the
    // established precedence between species and shared registrations.
    // Matches are mutually exclusive. Kraid's parts precede the generic literal no-op header and the ordinary
    // normal-shot reaction is last, because Crocomire's tongue also uses NormalEnemyShot.
    /// <summary>Maps shot-response pointers, including species-specific damage behavior.</summary>
    private void RegisterShotReactions(EnemyAiTable table)
    {
        DeadTorizoBehavior.RegisterDeadTorizoShotReactions(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperShotReactions(table);
        DeadTourianBehavior.RegisterDeadTourianShotReactions(table);
        ShitroidBehavior.RegisterShitroidShotReactions(table);
        MotherBrainBehavior.RegisterMotherBrainShotReactions(table);

        RidleyBehavior.RegisterRidleyShotReactions(table);

        DraygonBehavior.RegisterDraygonShotReactions(table);
        BombTorizoBehavior.RegisterBombTorizoShotReactions(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailShotReactions(table);
        SporeSpawnBehavior.RegisterSporeSpawnShotReactions(table);
        KraidBehavior.RegisterKraidShotReactions(table);
        CrocomireBehavior.RegisterCrocomireShotReactions(table);
        OwtchBehavior.RegisterOwtchShotReactions(table);
        WallSpacePirateBehavior.RegisterWallSpacePirateShotReactions(table);

        // Spark's private `$A8:E70E` handler never enters common shot AI. It only clears direction bit $10 on the
        // colliding Samus projectile, allowing the beam/missile actor to continue rather than becoming an impact
        // animation. This must occur before TryStartEnemyImpact, which would irreversibly rewrite the projectile
        // family to explosion `$0700`. Blue Brinstar's face block shares that behavior.
        EnemyShotStage passThrough = (in EnemyShotContext c) =>
        {
            c.Projectile.Direction = unchecked((ushort)(c.Projectile.Direction & 0xffef));
            return true;
        };
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Spark &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SparkShot)
        {
            AfterSuperMissileQuake = passThrough,
        });
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BlueBrinstarFaceBlockShot)
        {
            AfterSuperMissileQuake = passThrough,
        });
        ShuttersBehavior.RegisterShuttersShotReactions(table);

        MetroidBehavior.RegisterMetroidShotReactions(table);
        YardBehavior.RegisterYardShotReactions(table);

        // `$A3:8B0F` saves these graphics words around common shot AI, uses them for four metal debris actors on
        // death, then clears the dead body. Spawning before the clears preserves that exact graphics index.
        MetareeBehavior.RegisterMetareeShotReactions(table);

        // $A3:C7F5 adds Skree's four debris actors after the shared normal shot handler reports death, before the
        // common death animation releases the enemy slot.
        SkreeBehavior.RegisterSkreeShotReactions(table);
        FirefleaBehavior.RegisterFirefleaShotReactions(table);
        PlatformsBehavior.RegisterPlatformsShotReactions(table);
        BeetomBehavior.RegisterBeetomShotReactions(table);
        PowampBehavior.RegisterPowampShotReactions(table);
        WorkRobotBehavior.RegisterWorkRobotShotReactions(table);
        BullBehavior.RegisterBullShotReactions(table);
        FakeKraidBehavior.RegisterFakeKraidShotReactions(table);
        MamaTurtleBehavior.RegisterMamaTurtleShotReactions(table);
        KagoBehavior.RegisterKagoShotReactions(table);
        MagdolliteBehavior.RegisterMagdolliteShotReactions(table);
        RinkaBehavior.RegisterRinkaShotReactions(table);
        RipperBehavior.RegisterRipperShotReactions(table);
        DragonBehavior.RegisterDragonShotReactions(table);
        ShuttersBehavior.RegisterDestroyableAndHorizontalShutterShotReactions(table);
        ZebetiteBehavior.RegisterZebetiteShotReactions(table);
        EvirBehavior.RegisterEvirShotReactions(table);
        YappingMawBehavior.RegisterYappingMawShotReactions(table);
        KiHunterBehavior.RegisterKiHunterShotReactions(table);
        BotwoonBehavior.RegisterBotwoonShotReactions(table);
        ShaktoolBehavior.RegisterShaktoolShotReactions(table);

        // Ordinary common shot AI with no species stage.
        CeresSteamBehavior.RegisterCeresSteamShotReactions(table);
        MochtroidBehavior.RegisterMochtroidShotReactions(table);
        // Several retail helper/projectile definitions point their shot callback at a literal RTL in their own enemy
        // bank. Radius collision still dispatches those callbacks after its projectile prelude.
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.IsLiteralNoOpEnemyAi(enemy.Definition.Bank, enemy.Definition.ShotAiPointer)));
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot));
}

    // Each species registers its own run of reactions; first match wins, so the interleaving below is the
    // established precedence between species and shared registrations.
    // Matches are mutually exclusive (each names its definition and header callback); the ordinary normal-touch
    // handler is last because Crocomire's tongue also uses NormalEnemyTouch.
    /// <summary>Registers Samus-contact handlers after species instruction registrations.</summary>
    private void RegisterTouchHandlers(EnemyAiTable table)
    {
        KraidBehavior.RegisterKraidNailTouch(table);
        KraidBehavior.RegisterKraidTouch(table);

        MotherBrainBehavior.RegisterMotherBrainHeadTouch(table);
        ShitroidBehavior.RegisterShitroidTouch(table);
        DeadSidehopperBehavior.RegisterDeadSidehopperTouch(table);
        DeadTourianBehavior.RegisterDeadTourianTouch(table);
        DeadTorizoBehavior.RegisterDeadTorizoTouch(table);
        WallSpacePirateBehavior.RegisterWallSpacePirateTouch(table);
        BombTorizoBehavior.RegisterBombTorizoTouch(table);

        PhantoonBehavior.RegisterPhantoonBodyTouch(table);
        DraygonBehavior.RegisterDraygonTouch(table);
        SporeSpawnBehavior.RegisterSporeSpawnTouch(table);
        CeresSteamBehavior.RegisterCeresSteamTouch(table);
        CrocomireBehavior.RegisterCrocomireTouch(table);
        MaridiaLargeSnailBehavior.RegisterMaridiaLargeSnailTouch(table);
        PlatformsBehavior.RegisterPlatformsTouch(table);

        // Fifteen inventoried header/hitbox callbacks are literal RTL routines. Classify their bank-qualified
        // identities rather than probing executable ROM bytes at runtime. The collision is still reported, but the
        // callback performs no damage, knockback, or actor reaction.
        table.AddTouch(slot => EnemyTouchCallbackDefinitions.IsLiteralNoOp(slot.Definition.Bank, slot.Definition.TouchAiPointer),
            static _ => { });
        ShuttersBehavior.RegisterShuttersTouch(table);
        MochtroidBehavior.RegisterMochtroidTouch(table);
        MetroidBehavior.RegisterMetroidTouch(table);
        YappingMawBehavior.RegisterYappingMawTouch(table);
        YardBehavior.RegisterYardTouch(table);
        BeetomBehavior.RegisterBeetomTouch(table);
        PowampBehavior.RegisterPowampTouch(table);
        WorkRobotBehavior.RegisterWorkRobotTouch(table);
        FirefleaBehavior.RegisterFirefleaTouch(table);
        MamaTurtleBehavior.RegisterMamaTurtleTouch(table);
        MagdolliteBehavior.RegisterMagdolliteTouch(table);
        RinkaBehavior.RegisterRinkaTouch(table);
        DragonBehavior.RegisterDragonTouch(table);
        EvirBehavior.RegisterEvirTouch(table);
        BotwoonBehavior.RegisterBotwoonTouch(table);

        ZebetiteBehavior.RegisterZebetiteTouch(table);
        FakeKraidBehavior.RegisterFakeKraidTouch(table);
        ShaktoolBehavior.RegisterShaktoolTouch(table);

        table.AddTouch(slot => EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.NormalEnemyTouch,
            context => Collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
}

    /// <summary>Binds every enemy-owned color, HUD and escape asset from one host map catalog.</summary>
    public void BindMapPresentation(IGameplayPresentationContent? maps)
    {
        Presentation.MotherBrainHealthColors = maps?.MotherBrainHealthPalette;
        Presentation.MotherBrainRainbowColors = maps?.MotherBrainRainbowPalette;
        Presentation.MotherBrainRoomColors = maps?.MotherBrainRoomColors;
        Presentation.CeresRidleyColors = maps?.CeresRidleyColors;
        Presentation.CeresRidleyMode7Colors = maps?.CeresRidleyMode7Colors;
        Presentation.EscapeTimerArtwork = maps?.EscapeTimerTiles;
        EscapeTypewriterPresentation = maps?.EscapeTypewriter;
        Presentation.HudTileArtwork = maps?.HudTiles;
    }

    /// <summary>
    /// Publishes enemy-authored target colors to the current room-fade owner. Only written
    /// entries are transferred: untouched HUD, suit, and room rows retain their own targets.
    /// Current CGRAM must not be changed by instructions which target the fade buffer.
    /// </summary>
    internal void ConsumeTargetPaletteWrites(Action<int, Bgr555> write)
    {
        Ensure.NotNull(write);
        SporeSpawnBehavior._sporeSpawn?.ConsumeTargetColors(write);
    }

    // Hurt AI is a first-match list in EnemyMain's original dispatch order; the shared literal no-op hurt pointer is last.
    /// <summary>Maps hurt-state AI pointers used while enemies are flashing or reacting.</summary>
    private void RegisterHurtAi(EnemyAiTable table)
    {
        MetroidBehavior.RegisterMetroidHurtAi(table);
        BombTorizoBehavior.RegisterBombTorizoHurtAi(table);
        RidleyBehavior.RegisterRidleyHurtAi(table);
        PhantoonBehavior.RegisterPhantoonHurtAi(table);
        DraygonBehavior.RegisterDraygonHurtAi(table);
        table.AddHurt(slot => slot.Definition.HurtAiPointer == EnemyAiCodePointers.BankA0.NoOp, (_, _) => { });
    }

    // Rinka's frozen tail runs before the common thaw and can consume the frame; the others run after it.
    /// <summary>Maps frozen-state AI pointers used while ice suspends normal behavior.</summary>
    private void RegisterFrozenAi(EnemyAiTable table)
    {
        RinkaBehavior.RegisterRinkaFrozenPreemption(table);
        MetroidBehavior.RegisterMetroidFrozenAi(table);
        YappingMawBehavior.RegisterYappingMawFrozenAi(table);
    }
}
