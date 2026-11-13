using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
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
using SuperMetroid.Core.Enemies.BossBodies;
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
/// The room keeps a fixed set of enemy slots in <see cref="EnemySlotOrder"/>, each with a stable ordinal. The ordinals
/// are gameplay: multi-part enemies find their parts by neighbouring ordinals, Samus records the ordinal of the enemy she
/// meets, and the gunship's lower part copies fields from the slot before it. A compact host list would lose that.
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

    /// <summary>Puts populations into slots and runs their AI.</summary>
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
    /// <summary>Raises the dust of dead monsters' fallen corpse rows through the same frame as enemy processing.</summary>
    internal CorpseRowDust CorpseRowDust { get; }
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
    /// The reactions several species share, built from their owners on first access.
    /// </summary>
    internal EnemySharedReactions SharedReactions => field ??= BuildSharedReactions();

    /// <summary>The shared reactions, for the common participation to resolve at use.</summary>
    private EnemySharedReactions GetSharedReactions() => SharedReactions;

    /// <summary>The species registry that binds each installed occupant's entity.</summary>
    internal EnemySpeciesRegistry Species { get; }

    /// <summary>The loaded room's enemy definitions, services and artwork, shared with the composed owners.</summary>
    internal EnemyRoomBinding Room { get; }

    /// <summary>
    /// Rebinds the room's active escape typewriters to the host's current escape-warning text, after the host installs
    /// it (as when a saved game state is restored).
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

    /// <summary>The room's sprite objects.</summary>
    internal RoomSpriteObjects SpriteObjects { get; }

    /// <summary>
    /// Whether a Puyo striking a ceiling turns back: told of the room's draws, the charge flare's and the HUD's redraws,
    /// and read by the Puyos.
    /// </summary>
    internal PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; }

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
    internal IGunshipSession Gunship => field ??= new GunshipSession(GunshipBehavior);

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    internal ICeresEncounter Ceres => field ??= new CeresEncounter(RidleyEncounter, CeresRidleyBehavior);

    /// <summary>The state and hooks the room main routines and scrolling use.</summary>
    internal IEnemyRoomMainHooks RoomMainHooks => field ??= new EnemyRoomMainHooks(CrocomireBehavior, SporeSpawnBehavior, ZebetiteBehavior);

    /// <summary>The operations the room loader runs.</summary>
    internal IEnemyLoaderHooks LoaderHooks => field ??= new EnemyLoaderHooks(RinkaBehavior, PrePhantoonRoomBehavior);

    /// <summary>The enemy setup the attract demo's room setups perform.</summary>
    internal IEnemyAttractDemoHooks AttractDemoHooks => field ??= new EnemyAttractDemoHooks(KraidBehavior);

    /// <summary>The enemy-side effects room objects and room effects trigger.</summary>
    internal IEnemyObjectSpawns RoomObjectSpawns => field ??= new EnemyObjectSpawns(NoobTubeBehavior, EyeDoorBehavior, OldTourianEscapeShaftWallBehavior,
        DraygonBehavior, MotherBrainBehavior, BombTorizoBehavior, EscapeExplosionsBehavior);

    /// <summary>The room-object work the Chozo statues and downward gates exchange with their room objects each frame.</summary>
    internal IEnemyObjectRequests RoomObjectRequests => field ??= new EnemyObjectRequests(ChozoStatueBehavior, DownwardGateBehavior);

    /// <summary>The Tourian statue sequence's operations.</summary>
    internal ITourianStatueActors TourianStatue => field ??= new TourianStatueActors(TourianEntranceStatueBehavior, TourianUnlockBehavior);

    /// <summary>The enemies' palette-FX glows and flashes the room's palette-FX handler shows.</summary>
    internal IEnemyPaletteFxShows PaletteFxShows => field ??= new EnemyPaletteFxShows(BombTorizoBehavior, TourianEntranceStatueBehavior,
        SlotTable);

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
    RoomWrittenOverlay IEnemyDisplaySources.CeresWarning => RidleyEncounter.CeresWarning;

    /// <inheritdoc/>
    RoomWrittenOverlay IEnemyDisplaySources.ZebesWarning => MotherBrainEncounter.ZebesWarning;
    /// <inheritdoc/>
    ImmutableArray<PerspectiveFloorScenery> IEnemyDisplaySources.CeresGetawayChamberWalls => CeresRidleyBehavior.Getaway.ChamberWalls();
    /// <inheritdoc/>
    BossBodyLayer? IEnemyDisplaySources.BossBody => KraidBehavior.BodyLayer.CaptureBodyLayer()
        ?? (Presentation.BossBody is { IsShown: true } body ? body.Publish() : null);
    /// <inheritdoc/>
    Position16<RoomPixels>? IEnemyDisplaySources.KraidBodyOrigin => KraidBehavior.BodyLayer.BodyOrigin;
    /// <inheritdoc/>
    ScreenPlacedBossBody? IEnemyDisplaySources.CrocomireBody =>
        CrocomireBehavior.Access.Crocomire is not null ? new(CrocomireBehavior.BodyLayer.Placement, CrocomireBehavior.BodyLayer.ShowsBody) : null;
    /// <inheritdoc/>
    MorphBallEyeBeamState IEnemyDisplaySources.MorphBallEyeBeam => MorphBallEyeBehavior.MorphBallEyeBeam;

    /// <inheritdoc />
    IEnumerable<IGameplayShadingSource> IEnemyDisplaySources.ShadingSources
    {
        get
        {
            yield return CeresDoorBehavior;
            yield return TourianEntranceStatueBehavior;
            if (RidleyEncounter.Ridley is { } ridley)
                yield return ridley.Colours;
            if (MotherBrainEncounter.MotherBrain is { } motherBrain)
                yield return motherBrain.Colours;
            if (SporeSpawnBehavior.SporeSpawn is { } sporeSpawn)
                yield return sporeSpawn.Colours;
            if (PhantoonBehavior.Combat.Phantoon is { } phantoon)
                yield return phantoon.Colours;
            if (DraygonBehavior.Parts.Draygon is { } draygon)
                yield return draygon.Colours;
            if (CrocomireBehavior.Access.Crocomire is { } crocomire)
                yield return crocomire.Colours;
            if (KraidBehavior.Access.Kraid is { } kraid)
            {
                yield return KraidBehavior.Access.Backdrop;
                yield return kraid.Colours;
            }
        }
    }

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
        BossFrameResults.DraygonDiscardedSamusPoseRequests, BossFrameResults.CeresEscapeStarted, BossFrameResults.CameraXOverride,
        BossFrameResults.RemoveLiquidEffects, FrameResults.Gunship, BossFrameResults.RoomEffects());

    /// <summary>
    /// Creates a room's enemy system: every enemy slot, each with its stable ordinal, with the door-transition gates
    /// clear, without loading a population or binding memory, graphics, random-number, or gameplay services.
    /// </summary>
    /// <param name="session">The enemy words and installed artwork that outlive each room.</param>
    /// <param name="compatibility">
    /// The compatibility services the room's enemies consult: the room's own, created with the system by its builder, and
    /// those that outlive every room, which the builder hands on.
    /// </param>
    /// <param name="definitions">The room's enemy definition source, or null for an enemy system with no population.</param>
    /// <param name="services">The original game-state services the room's AI calls, or null when none is supplied.</param>
    /// <param name="bossArtwork">The session's installed boss artwork, or null for a system with its own, empty binding.</param>
    internal RoomEnemySystem(EnemySession session, RoomEnemyCompatibility compatibility,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null,
        BossArtworkBinding? bossArtwork = null)
    {
        Ensure.NotNull(compatibility);
        BossArtworkBinding = bossArtwork ?? new();
        Room = new EnemyRoomBinding(session, definitions, services);
        // Shared collaborators first: every species owner receives the ones it uses.
        ScrollCap = new SamusScrollCap();
        RoomFx = new EnemyRoomFx();
        RoomStatus = new EnemyRoomStatus(session);
        FrameContext = new EnemyFrameContext(session);
        Presentation = new EnemyPresentation(Room, compatibility.FadeStepCount);
        BossPresentation = new BossPresentation(Presentation, BossArtworkBinding);
        SlotTable = new EnemySlotTable();
        Audio = new EnemyAudioQueue();
        MorphBallEyeBehavior = new MorphBallEyeBehavior(Audio);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, MorphBallEyeBehavior, Room, SlotTable);
        Dispatch = new EnemySlotDispatch(Room, SlotTable, ProcessingWindow);
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, SlotTable, Dispatch);
        RidleyEncounter = new RidleyEncounter(SlotTable, session);
        Earthquake = new RoomEarthquake();
        PuyoCeilingTurn = compatibility.PuyoCeilingTurn;
        SpriteObjects = new RoomSpriteObjects(Presentation, compatibility.SpriteObjectPlaces, PuyoCeilingTurn);
        Projectiles = new EnemyProjectileEngine(Room, Presentation, SpriteObjects, Earthquake, Audio);
        Projectiles.AddHandlerSource(this);
        GunshipBehavior = new GunshipBehavior(Audio, Projectiles, Room, SlotTable, FrameResults);
        Terrain = new EnemyTerrain();
        CorpseRowDust = new CorpseRowDust(Audio);
        MotherBrainEncounter = new MotherBrainEncounter(Audio);
        RidleyTail = new RidleyTail(FrameContext, Projectiles, RidleyEncounter, Room);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, Dispatch, compatibility.PowerBombRadius, Audio);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Dispatch = Dispatch, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, FrameContext = FrameContext, Presentation = Presentation, Projectiles = Projectiles, Room = Room, SlotTable = SlotTable, PowerBombRadius = compatibility.PowerBombRadius });
        RidleyProjectiles = new RidleyProjectiles(DeathsAndDrops, Projectiles, RidleyEncounter);
        ZoaBehavior = new ZoaBehavior();
        ElevatorBehavior = new ElevatorBehavior(Room, Audio, FrameResults);
        ZebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Audio = Audio, Dispatch = Dispatch, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, SlotTable = SlotTable, ElevatorBehavior = ElevatorBehavior, RoomStatus = RoomStatus, Collision = Collision });
        YardBehavior = new YardEnemyBehavior(Room, Terrain, Earthquake, Collision, Audio);
        YappingMawBehavior = new YappingMawBehavior(Projectiles, SpriteObjects, ScrollCap, Audio);
        WreckedShipGhostBehavior = new WreckedShipGhostBehavior();
        WorkRobotBehavior = new WorkRobotBehavior(Room, Terrain, Projectiles, SlotTable, Audio);
        WaverBehavior = new WaverBehavior(Terrain);
        WalkingSpacePirateBehavior = new WalkingSpacePirateBehavior(Terrain, Projectiles, Audio);
        WallSpacePirateBehavior = new WallSpacePirateBehavior(Room, Terrain, Collision, DeathsAndDrops, WalkingSpacePirateBehavior);
        TourianEntranceStatueBehavior = new TourianEntranceStatueBehavior(Projectiles, SlotTable);
        TourianUnlockBehavior = new TourianUnlockBehavior(Room, Projectiles, Earthquake, Audio, TourianEntranceStatueBehavior);
        StokeBehavior = new StokeBehavior(Room, Terrain, Projectiles);
        SporeSpawnBehavior = new SporeSpawnBehavior(new SporeSpawnBehaviorDependencies { Audio = Audio, Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults , BossArtwork = BossArtworkBinding });
        FallingSparksBehavior = new FallingSparksBehavior(Room, Projectiles, SpriteObjects);
        SparkBehavior = new SparkBehavior(FallingSparksBehavior);
        SkulteraBehavior = new SkulteraBehavior(Terrain);
        SkreeBehavior = new SkreeBehavior(Audio, Projectiles, DeathsAndDrops);
        ShuttersBehavior = new ShuttersBehavior(Audio);
        DeadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = Room, Terrain = Terrain, SlotTable = SlotTable, FrameContext = FrameContext, Projectiles = Projectiles, Collision = Collision, RowDust = CorpseRowDust });
        ShitroidBehavior = new ShitroidBehavior(Room, SlotTable, ZebetiteBehavior, BossFrameResults, Audio);
        ShaktoolBehavior = new ShaktoolBehavior(Terrain, Projectiles, SlotTable, Collision, compatibility.KilledShaktoolSegment);
        SbugBehavior = new SbugBehavior(Room, Terrain);
        RipperBehavior = new RipperBehavior(Terrain);
        RioBehavior = new RioBehavior(Room, Terrain, Audio);
        RinkaBehavior = new RinkaBehavior(Collision, FrameContext, Projectiles, DeathsAndDrops);
        CeresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { FrameResults = BossFrameResults, Dispatch = Dispatch, Room = Room, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = BossPresentation, Collision = Collision, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, RoomStatus = RoomStatus, UnauthoredBoostCadence = compatibility.UnauthoredBoostCadence });
        RidleyBehavior = new RidleyBehavior(new RidleyBehaviorDependencies { Dispatch = Dispatch, Room = Room, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, DeathsAndDrops = DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, CeresRidleyBehavior = CeresRidleyBehavior, RidleyProjectiles = RidleyProjectiles, RoomStatus = RoomStatus, RoomFx = RoomFx  });
        PuyoBehavior = new PuyoBehavior(Room, Terrain, PuyoCeilingTurn);
        PrePhantoonRoomBehavior = new PrePhantoonRoomBehavior(Room, Projectiles);
        PowampBehavior = new PowampBehavior(Terrain, Projectiles, SlotTable, Collision, DeathsAndDrops);
        PolypBehavior = new PolypBehavior(Room, Projectiles);
        PlatformsBehavior = new PlatformsBehavior(Terrain);
        PipeBugsBehavior = new PipeBugsBehavior(SlotTable);
        PhantoonBehavior = new PhantoonBehavior(new PhantoonBehaviorDependencies { Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops , BossArtwork = BossArtworkBinding, FrameResults = BossFrameResults });
        OwtchBehavior = new OwtchBehavior(Room);
        OldTourianEscapeShaftWallBehavior = new OldTourianEscapeShaftWallBehavior(Room, Projectiles, Audio);
        NuclearWaffleBehavior = new NuclearWaffleBehavior(Projectiles, SpriteObjects, Audio);
        NorfairRioBehavior = new NorfairRioBehavior(Room, Terrain, SlotTable, Audio);
        NorfairLavaJumpingEnemyBehavior = new NorfairLavaJumpingEnemyBehavior(Room, SlotTable, Audio);
        NoobTubeBehavior = new NoobTubeBehavior(Room, Projectiles);
        NinjaSpacePirateBehavior = new NinjaSpacePirateBehavior(Room, Terrain, Projectiles, SpriteObjects, WalkingSpacePirateBehavior);
        MultiviolaBehavior = new MultiviolaBehavior(Terrain);
        BabyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Dispatch = Dispatch, Room = Room, Audio = Audio, SlotTable = SlotTable, FrameContext = FrameContext, Projectiles = Projectiles, MotherBrainEncounter = MotherBrainEncounter });
        MotherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { FrameResults = BossFrameResults, Dispatch = Dispatch, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, RoomActors = new MotherBrainRoomActors(BabyMetroidCutsceneBehavior, RinkaBehavior, WalkingSpacePirateBehavior, RidleyProjectiles)  });
        MochtroidBehavior = new MochtroidBehavior(Terrain, Audio, FrameContext, Collision);
        MetroidBehavior = new MetroidBehavior(Room, Terrain, Audio, SpriteObjects, FrameContext, DeathsAndDrops);
        MetareeBehavior = new MetareeBehavior(Projectiles, Audio);
        MaridiaLargeSnailBehavior = new MaridiaLargeSnailBehavior(Terrain, Collision, Audio);
        MamaTurtleBehavior = new MamaTurtleBehavior(Terrain, Earthquake, SlotTable, Collision, Audio);
        MagdolliteBehavior = new MagdolliteBehavior(Room, Projectiles, SlotTable, Collision, DeathsAndDrops, Audio);
        LowerNorfairRioBehavior = new LowerNorfairRioBehavior(Room, Terrain, SlotTable, Audio);
        KzanBehavior = new KzanBehavior(SlotTable, Audio);
        KraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, SlotTable = SlotTable, Presentation = BossPresentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior  });
        KiHunterBehavior = new KiHunterBehavior(Room, Terrain, Projectiles, Audio, SlotTable);
        KagoBehavior = new KagoBehavior(Room, Projectiles, Earthquake, DeathsAndDrops, RoomStatus, Audio);
        HoppersBehavior = new HoppersBehavior(Room, Terrain, Audio);
        HibashiBehavior = new HibashiBehavior(SlotTable, Audio);
        GrappleBehavior = new GrappleBehavior(Dispatch, Room, DeathsAndDrops);
        FuneNamiheBehavior = new FuneNamiheBehavior(Projectiles, Audio);
        FliesBehavior = new FliesBehavior(Room);
        FirefleaBehavior = new FirefleaBehavior(Collision, DeathsAndDrops, RoomStatus);
        FakeKraidBehavior = new FakeKraidBehavior(Room, Terrain, Projectiles, Collision, DeathsAndDrops, Audio);
        EyeDoorBehavior = new EyeDoorBehavior(Room, Projectiles);
        EvirBehavior = new EvirBehavior(SlotTable, Collision, Audio);
        EtecoonBehavior = new EtecoonBehavior(Terrain, Earthquake, ElevatorBehavior, Audio);
        EscapeExplosionsBehavior = new EscapeExplosionsBehavior(Audio, SpriteObjects);
        EscapeAnimalsBehavior = new EscapeAnimalsBehavior(Room, Terrain);
        DraygonBehavior = new DraygonBehavior(new DraygonBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ScrollCap = ScrollCap , BossArtwork = BossArtworkBinding });
        DragonBehavior = new DragonBehavior(Projectiles, SlotTable, Collision, Audio);
        DownwardGateBehavior = new DownwardGateBehavior(Room, Projectiles);
        DeadTourianBehavior = new DeadTourianBehavior(Room, FrameContext, Projectiles, CorpseRowDust);
        DeadTorizoBehavior = new DeadTorizoBehavior(Room, FrameContext, Projectiles, Presentation, Audio);
        DachoraBehavior = new DachoraBehavior(Terrain, SlotTable, Audio);
        CrocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { Access = new CrocomireEncounterAccess(BossFrameResults, Room, Projectiles, Audio), Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, SpriteObjects = SpriteObjects, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ZebetiteBehavior = ZebetiteBehavior, RoomStatus = RoomStatus  });
        CrawlersBehavior = new CrawlersBehavior(Terrain, Earthquake);
        ChozoStatueBehavior = new ChozoStatueBehavior(Room, Terrain, Projectiles, Audio, RoomFx);
        ChootBehavior = new ChootBehavior();
        CeresSteamBehavior = new CeresSteamBehavior(Room, Collision);
        CeresDoorBehavior = new CeresDoorBehavior(new CeresDoorBehaviorDependencies { Room = Room, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, Projectiles = Projectiles, CeresEscape = RidleyEncounter });
        CacatacBehavior = new CacatacBehavior(Room, Projectiles, Audio);
        BullBehavior = new BullBehavior(compatibility.BullInheritance);
        BoyonBehavior = new BoyonBehavior(Audio);
        BoulderBehavior = new BoulderBehavior(Terrain, Projectiles, Audio);
        BotwoonBehavior = new BotwoonBehavior(new BotwoonBehaviorDependencies { Audio = Audio, Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, Presentation = Presentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults , BossArtwork = BossArtworkBinding });
        Instructions = new EnemyInstructionInterpreter(Presentation);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { DeadTorizoBehavior = DeadTorizoBehavior, FrameContext = FrameContext, Instructions = Instructions, MotherBrainBehavior = MotherBrainBehavior, Presentation = Presentation, Room = Room, PuyoCeilingTurn = PuyoCeilingTurn });
        BombTorizoBehavior = new BombTorizoBehavior(new BombTorizoBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults });
        BlueBrinstarFaceBlockBehavior = new BlueBrinstarFaceBlockBehavior(SlotTable, ElevatorBehavior);
        BeetomBehavior = new BeetomBehavior(Room, Terrain, Audio, FrameContext, Collision, compatibility.BeetomInheritance);
        AtomicBehavior = new AtomicBehavior();
        AlcoonBehavior = new AlcoonBehavior(Room, Terrain, Projectiles, Audio);
        // Every species with room or frame lifecycle state, in the original game reset order.
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
    }

    /// <summary>
    /// Starts each enemy projectile place from the whole position and velocity the room this one replaces left
    /// (<see cref="RoomHandOver"/>), before the population loads.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver) => Projectiles.TakeOverFrom(handOver);

    /// <summary>Leaves each enemy projectile place's whole position and velocity for the room that replaces this one (<see cref="RoomHandOver"/>).</summary>
    internal void HandOverTo(RoomHandOver handOver) => Projectiles.HandOverTo(handOver);

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
    /// <summary>Whether <see cref="Load"/> has loaded this room's population; a room loads it once.</summary>
    private bool _populationLoaded;

    /// <summary>Whether the room's enemy definitions have been bound by <see cref="Load"/>; this reports binding, not successful completion of every initializer or artwork validation.</summary>
    public bool IsLoaded => Room.IsLoaded;

    /// <summary>
    /// Loads the room's enemies: the artwork their population draws from, their population and each one's initialization.
    /// </summary>
    public void Load(IRoomEnemyLoad request)
    {
        Ensure.NotNull(request);
        if (_populationLoaded)
            throw new InvalidOperationException("A room's enemy system loads its population once; each room builds its own.");
        _populationLoaded = true;
        // Every encounter of the new room shows the look its room's load gives it until it shows another (#1538), its
        // initialization among them.
        Room.Session.Looks.Restart();
        IRoomEnemyDefinitionSource definitions = Room.LoadedDefinitions;
        GunshipBehavior.BeginRoomLoad(request.GunshipScenario, request.Samus);
        FrameContext.BeginRoomLoad(request.DeferLoaderTimeCameraReads);
        DeathsAndDrops.BindSamus(request.Samus);
        Presentation.BindBackgrounds(request.Appearance);
        FrameResults.Begin();
        BossFrameResults.Begin();
        // Initialization AI reads the already-positioned destination camera before the first enemy frame captures its own.
        RinkaBehavior.SetRinkaCamera(request.Camera);
        ShuttersBehavior.BeginShutterFrame(request.Camera);

        // A room that places no enemies keeps drawing its spawned objects from the standard artwork (#1599): an empty
        // population loads no enemy artwork, so the objects such a room shows appear in their standard variants.
        RoomEnemyPopulationDefinition population = request.Population;
        if (!population.Records.IsEmpty)
            Presentation.DrawPopulation(population);
        PopulationLoader.LoadPopulation(
            definitions,
            population,
            new EnemySpawnSurroundings(request.Level, request.Samus, request.ControllerInput, request.Camera));
    }

    /// <summary>
    /// Runs one enemy pass over the processing list the frame already built: executes selected AI and animation scripts,
    /// publishes live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    internal EnemyFrameResult StepFrame(RoomEnemyFrame frame)
    {
        using var terrainScope = new EnemyTerrain.EnemyTerrainScope(Terrain, frame.CollisionObjects);
        Room.EnsureLoaded();
        DeathsAndDrops.BindSamus(frame.Samus);
        ScrollCap.BeginFrame(frame.SamusPreviousPositionCheckpoint);
        FrameContext.BeginFrame(frame.TimeIsFrozen, frame.SamusProjectiles, frame.SamusColours, frame.FrameCounter);
        Audio.PowerBomb = frame.SharedProjectiles.PowerBombExplosion;
        var aiFrame = new ClockedEnemyAiFrame(frame, new FrameCounters(frame.ShortCounter));
        FrameResults.Begin();
        BossFrameResults.Begin();
        foreach (IEnemySpecies species in _species)
            species.BeginEnemyFrame(frame);
        Drawing.Clear();

        foreach (RoomEnemySlot slot in ProcessingWindow.ActiveEnemies)
        {
            // The enemy pass decrements tangible actors' invincibility
            // before checking time freeze. X-ray therefore stops their movement but
            // not this clock. Remember the entry value: the call that reaches zero
            // still skips collision, and a new AI-owned timer must not lose a tick.
            bool invincibleAtEntry = slot.InvincibilityTimer != 0;
            if (!slot.Contact.HasAny(EnemyContact.IgnoresSamus) && invincibleAtEntry)
                slot.InvincibilityTimer--;
            // The enemy pass checks contact against the pre-movement actor, not the position
            // just moved into by AI. A rising support temporarily overlaps its rider until
            // Samus's movement consumes the carry; a late touch pass would retrigger that support.
            if (!frame.TimeIsFrozen && !invincibleAtEntry && frame.Samus is not null)
            {
                // Extended collision runs projectiles before bombs/touch and
                // before selecting hurt/main AI. In particular, a retained Plasma
                // hit must publish Phantoon's hurt clock before this frame ticks it.
                if (slot.Entity is IEnemyContactHooks hooks)
                    hooks.ResolveBeforeOrdinaryHits(frame);
                Collision.ResolveOrdinaryProjectileHits(frame.SamusProjectiles, frame.SharedProjectiles,
                    frame.Samus, slot);
                // Alpha already updated the bomb slots. The enemy pass checks
                // them before this actor's touch and AI, so escape starts now.
                Collision.ResolveOrdinaryBombHits(frame.SharedProjectiles, frame.SamusProjectiles, frame.Samus, slot);
                if (slot.DefinitionId == EnemyDefinitionId.None)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.ControllerInput, frame.Level, slot);
                if (slot.Entity is IEnemyContactHooks contact)
                    contact.ResolveAfterSamusContact(frame.Samus);
                if (slot.DefinitionId == EnemyDefinitionId.None)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(slot, aiFrame);
                // Hurt AI replaces main AI while the hurt bit is set; the first registered match runs.
                // Selects the lowest AI bit even when its handler is just
                // a bare return. Hurt therefore delays frozen AI until flash housekeeping
                // clears bit two; falling through here prematurely clears flash.
                if (!ranActorAi &&
                    (slot.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None &&
                    (slot.Entity as IEnemyHurtAi)?.FindHurt() is { } hurt)
                {
                    hurt(slot, frame);
                    ranActorAi = true;
                }
                // The enemy pass selects the handler from the lowest set AI bit alone. A frozen
                // clock without bit four (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (slot.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None)
                {
                    // Rinka's termination tail clears/replaces the current actor record.
                    // The active-index array was frozen before AI, so skip the remainder
                    // of this stale entry exactly as the private routine's death call does.
                    var frozen = slot.Entity as IEnemyFrozenAi;
                    if (frozen?.FindFrozenPreemption() is { } preemption && preemption(slot))
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // dispatch for this call. Ice removal instead takes the thaw branch
                    // immediately. That branch leaves the freeze clock at the remainder
                    // ThawedFreezeLeftover names rather than zero, which matters when hurt
                    // and frozen dispatch coexist.
                    slot.FlashTimer = 0;
                    bool thaw = slot.FrozenTimer == 0 ||
                        (frame.Samus is not null &&
                            (frame.Samus.EquippedBeams & SamusBeamFlags.Ice) == SamusBeamFlags.None);
                    if (!thaw)
                        slot.FrozenTimer--;
                    else
                    {
                        slot.AiStates = slot.AiStates & ~EnemyAiStates.Frozen;
                        bool stillGrappled = (slot.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
                        bool stillHurt = (slot.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None;
                        ThawedFreezeLeftover.Hold hold = stillGrappled
                            ? stillHurt ? ThawedFreezeLeftover.Hold.GrappledAndHurt : ThawedFreezeLeftover.Hold.Grappled
                            : stillHurt ? ThawedFreezeLeftover.Hold.Hurt : ThawedFreezeLeftover.Hold.None;
                        slot.FrozenTimer = ThawedFreezeLeftover.Of(hold);
                    }
                    foreach (Action<RoomEnemySlot> handler in frozen?.FrozenHandlers() ?? [])
                        handler(slot);
                    // The frame clock advances after every selected AI call,
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
                    slot.FrameCounter = WrappingCounter.IncrementCount(slot.FrameCounter);
                    // The enemy pass skips the animation script whenever frozen bit
                    // four is present, even if lower-priority hurt bit two selected an
                    // enemy-specific hurt callback above. Testing only `ranActorAi` here
                    // incorrectly advanced a Metroid's body animation while its frozen
                    // callback still owned the two composited outer sprite objects.
                    if ((slot.AiStates & EnemyAiStates.Frozen) == EnemyAiStates.None &&
                        slot.Processing.HasAny(EnemyProcessing.AnimationScript))
                        EnemyInstructionInterpreter.RunsAnimationScript(
                            slot,
                            frame.Samus,
                            frame.Level,
                            frame.Camera,
                            frame.ControllerInput);
                }
            }

            // The enemy pass queues each live enemy to show only after its AI and animation work; the two
            // hide-drawing properties keep it from being shown. The draw pass reaches an enemy its radius puts on
            // screen, and every enemy that collides by its frames' hitboxes wherever it is. Only a reached enemy's draw
            // clears the value a Puyo's later ceiling strike reads, consumes its shake tick and shows its boss body
            // frame, so which enemies are reached stays the game's; what is off screen is culled by the display.
            // The gunship uses the radius test.
            bool reached = slot.CollidesByFrameHitboxes ||
                !EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, frame.Camera);
            if (!slot.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted))
                Drawing.Enqueue(slot, reached);

            // The enemy pass performs this after the enemy has been admitted to its draw queue.
            // Keeping it here also makes the boss's latched draw palettes describe the
            // frame just processed rather than the already-decremented following frame.
            if (!frame.TimeIsFrozen && slot.FlashTimer != 0)
            {
                slot.FlashTimer--;
                // The enemy pass clears hurt dispatch once the post-decrement timer is below
                // eight. This is a signed comparison and applies even when another AI bit
                // remains set; omitting it would strand Metroid in custom hurt forever.
                if (slot.FlashTimer - 8 < 0)
                    slot.AiStates = slot.AiStates & ~EnemyAiStates.Hurt;
            }
        }

        // The processing window fixes only which enemies interact this frame. Samus's
        // collision reads their live position and size, so publish bodies after AI has
        // had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (RoomEnemySlot slot in ProcessingWindow.InteractiveEnemies)
        {
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                slot.SlotIndex,
                slot.Position.Whole,
                new(slot.Radius.X, slot.Radius.Y),
                // A frozen enemy blocks Samus whatever its properties say.
                slot.FrozenTimer != 0 || slot.Contact.HasAny(EnemyContact.SolidToSamus)));
        }
        FrameContext.AdvanceRandomEnemyCounter();
        frame.Samus?.Kinematics.ClearSolidEnemyCollisions();
        // The room-wide glows step after every enemy has updated, whether or not time is frozen.
        WorkRobotBehavior.StepGlow();
        MagdolliteBehavior.StepGlow();
        BlueBrinstarFaceBlockBehavior.StepGlow();
        if (!frame.TimeIsFrozen)
            SpriteObjects.StepRoomSpriteObjects();
        return BuildFrameResult();
    }

    /// <summary>Runs the room shaking: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        Room.EnsureLoaded();
        // This runs after drawing on the original game, so the two-frame actor timer becomes visible
        // on the following frame rather than retroactively changing already-emitted sprites.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (RoomEnemySlot slot in ProcessingWindow.ActiveEnemies)
                slot.ShakeTimer = RoomEarthquakeDefinitions.EnemyShakeDuration;
        }
        return Earthquake.LastRoomShake;
    }

    /// <summary>
    /// Builds the shared reactions in the established first-match order.
    /// </summary>
    private EnemySharedReactions BuildSharedReactions()
    {
        var table = new EnemySharedReactions();
        AddTouchHandlers(table);
        AddShotReactions(table);
        AddBombReactions(table);
        AddPowerBombReactions(table);
        AddHurtAi(table);
        AddFrozenAi(table);
        return table;
    }

    /// <summary>Maps each enemy definition's bomb reaction to its translated handler.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations.</remarks>
    private void AddBombReactions(EnemySharedReactions table)
    {
        CrocomireBehavior.Combat.AddCrocomireBombReactions(table);
        KraidBehavior.Combat.AddKraidBombReactions(table);

        NinjaSpacePirateBehavior.AddGoldNinjaPirateBombReactions(table);
        ShuttersBehavior.AddShuttersBombReactions(table);
        MetroidBehavior.AddMetroidBombReactions(table);
        ShitroidBehavior.AddShitroidBombReactions(table);
        CeresRidleyBehavior.Hits.AddCeresRidleyBombReactions(table);
        BombTorizoBehavior.Combat.AddBombTorizoBombReactions(table);

        // The body's reaction makes a dud shot. The frame-hitbox bomb walker has already marked the physical explosion, and the
        // body callback performs no health, phase, or multipart-state work of its own.
        MotherBrainCombat.AddMotherBrainBodyBombReactions(table);
        MotherBrainBehavior.Combat.AddMotherBrainBombReactions(table);
        DeadTorizoBehavior.AddDeadTorizoBombReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperBombReactions(table);
        DeadTourianBehavior.AddDeadTourianBombReactions(table);
        PhantoonBehavior.Combat.AddPhantoonBombReactions(table);
        MamaTurtleBehavior.AddMamaTurtleBombReactions(table);
        RinkaBehavior.AddRinkaBombReactions(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailBombReactions(table);
        RipperBehavior.AddRipperBombReactions(table);
        DragonBehavior.AddDragonBombReactions(table);
        // The metal-debris death restores the dying actor's graphics long enough to seed four debris actors, then clears both
        // values. The host death flag does not erase the record, so the exact source remains here.
        MetareeBehavior.AddMetareeBombReactions(table);
        FirefleaBehavior.AddFirefleaBombReactions(table);
        FakeKraidBehavior.AddFakeKraidBombReactions(table);
        PlatformsBehavior.AddPlatformsBombReactions(table);
        // Skree's callback owns its four-piece burst and death animation because its shot reaction skips the
        // ordinary death animation. A normal bomb selects death variant zero (only missiles select two).
        SkreeBehavior.AddSkreeBombReactions(table);
        ZebetiteBehavior.AddZebetiteBombReactions(table);
        SporeSpawnBehavior.AddSporeSpawnBombReactions(table);
        DraygonBehavior.Combat.AddDraygonBombReactions(table);
        BotwoonBehavior.AddBotwoonBombReactions(table);
        RidleyInteractions.AddRidleyBombReactions(table);
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

    /// <summary>Maps the species' own power-bomb reactions for each supported species.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive: each names its
    /// definition and header reaction.</remarks>
    private void AddPowerBombReactions(EnemySharedReactions table)
    {
        // These headers name their own reactions instead of falling through common power-bomb damage. The ignoring
        // reaction intentionally does nothing; the shutter reactions only run their trigger state machines. All still
        // receive the process-off-screen bit after a qualifying ellipse overlap.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy =>
            EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.Ignore)
        {
            ReplacesCommon = static (_, _) => { },
        });
        ShuttersBehavior.AddShuttersPowerBombReactions(table);
        CrocomireBehavior.Combat.AddCrocomirePowerBombReactions(table);
        DeadTorizoBehavior.AddDeadTorizoPowerBombReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperPowerBombReactions(table);
        DeadTourianBehavior.AddDeadTourianPowerBombReactions(table);
        ShitroidBehavior.AddShitroidPowerBombReactions(table);
        FirefleaBehavior.AddFirefleaPowerBombReactions(table);
        MetroidBehavior.AddMetroidPowerBombReactions(table);
        FakeKraidBehavior.AddFakeKraidPowerBombReactions(table);
        PowampBehavior.AddPowampPowerBombReactions(table);
        MagdolliteBehavior.AddMagdollitePowerBombReactions(table);
        RinkaBehavior.AddRinkaPowerBombReactions(table);
        DragonBehavior.AddDragonPowerBombReactions(table);
        EvirBehavior.AddEvirPowerBombReactions(table);
        KiHunterBehavior.AddKiHunterPowerBombReactions(table);
        BotwoonBehavior.AddBotwoonPowerBombReactions(table);
        WallSpacePirateBehavior.AddWallSpacePiratePowerBombReactions(table);
        RidleyInteractions.AddRidleyPowerBombReactions(table);
        DraygonBehavior.Combat.AddDraygonPowerBombReactions(table);
}

    /// <summary>Advances enemy-projectile actors after Samus movement and before room objects.</summary>
    public void StepEnemyProjectileInstructions(
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera,
        int shortFrameCounter,
        SamusBombProjectileSystem? samusBombs,
        int frameCounter)
    {
        Room.EnsureLoaded();
        DeathsAndDrops.BindSamus(samus);
        Projectiles.RunPass(level, samus, camera, shortFrameCounter, samusBombs, frameCounter);
    }

    /// <summary>
    /// Registers species projectile handlers before generic and cross-species handlers,
    /// preserving the keyed per-frame step and ordered script-interceptor dispatch.
    /// </summary>
    void IEnemyProjectileHandlerSource.AddEnemyProjectileHandlers(EnemyProjectileHandlerTable table)
    {
        // Species register first; generic and cross-species registrations follow.
        AlcoonBehavior.AddAlcoonProjectiles(table);
        BombTorizoBehavior.Attacks.AddBombTorizoProjectiles(table);
        BotwoonBehavior.AddBotwoonProjectiles(table);
        CacatacBehavior.AddCacatacProjectiles(table);
        CrocomireBehavior.Combat.AddCrocomireProjectiles(table);
        DownwardGateBehavior.AddDownwardGateProjectiles(table);
        DragonBehavior.AddDragonProjectiles(table);
        DraygonBehavior.Goop.AddDraygonProjectiles(table);
        EyeDoorBehavior.AddEyeDoorProjectiles(table);
        FakeKraidBehavior.AddFakeKraidProjectiles(table);
        FallingSparksBehavior.AddFallingSparksProjectiles(table);
        FuneNamiheBehavior.AddFuneNamiheProjectiles(table);
        KagoBehavior.AddKagoProjectiles(table);
        KiHunterBehavior.AddKiHunterProjectiles(table);
        KraidProjectiles.AddKraidProjectiles(table);
        KraidProjectiles.AddKraidRockAnimations(table);
        MagdolliteBehavior.AddMagdolliteProjectiles(table);
        MotherBrainBehavior.Projectiles.AddMotherBrainProjectiles(table);
        NinjaSpacePirateBehavior.AddNinjaSpacePirateProjectiles(table);
        NoobTubeBehavior.AddNoobTubeProjectiles(table);
        PhantoonBehavior.Combat.AddPhantoonProjectiles(table);
        PolypBehavior.AddPolypProjectiles(table);
        PowampBehavior.AddPowampProjectiles(table);
        PrePhantoonRoomBehavior.AddPrePhantoonRoomProjectiles(table);
        ShaktoolBehavior.AddShaktoolProjectiles(table);
        SporeSpawnBehavior.AddSporeSpawnProjectiles(table);
        StokeBehavior.AddStokeProjectiles(table);
        TourianEntranceStatueBehavior.AddTourianEntranceStatueProjectiles(table);
        TourianUnlockBehavior.AddTourianUnlockProjectiles(table);
        WalkingSpacePirateBehavior.AddWalkingSpacePirateProjectiles(table);
        WorkRobotBehavior.AddWorkRobotProjectiles(table);

        DeathsAndDrops.ConnectPickupFrameBehavior(table);

        RinkaBehavior.AddMiscDustFrameBehavior(table);

        RidleyProjectiles.AddFireballFrameBehaviors(table);

        EnemyProjectileEngine.AddMetalSkreeParticleFrameBehavior(table);

        KraidProjectiles.AddKraidRockSpitFrameBehavior(table);

        Projectiles.AddCeresFallingTileFrameBehavior(table);

        EnemyProjectileEngine.AddGunshipDustCues(table);


        OldTourianEscapeShaftWallBehavior.AddWallExplosionCues(table);

        NoobTubeBehavior.AddNoobTubeProjectileCues(table);


        RidleyProjectiles.AddFireballCues(table);

        CrocomireBehavior.Combat.AddCrocomireDropProjectiles(table);

        DeathsAndDrops.ConnectDeathExplosionCues(table);
    }

    /// <summary>Maps shot responses, including species-specific damage behavior.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive. Kraid's parts
    /// precede the generic ignoring header and the ordinary normal-shot reaction is last, because Crocomire's
    /// tongue also uses NormalEnemyShot.</remarks>
    private void AddShotReactions(EnemySharedReactions table)
    {
        DeadTorizoBehavior.AddDeadTorizoShotReactions(table);
        DeadSidehopperBehavior.AddDeadSidehopperShotReactions(table);
        DeadTourianBehavior.AddDeadTourianShotReactions(table);
        ShitroidBehavior.AddShitroidShotReactions(table);
        MotherBrainBehavior.Combat.AddMotherBrainShotReactions(table);

        RidleyInteractions.AddRidleyShotReactions(table);

        DraygonBehavior.Combat.AddDraygonShotReactions(table);
        BombTorizoBehavior.Combat.AddBombTorizoShotReactions(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailShotReactions(table);
        SporeSpawnBehavior.AddSporeSpawnShotReactions(table);
        KraidBehavior.Combat.AddKraidShotReactions(table);
        CrocomireBehavior.Combat.AddCrocomireShotReactions(table);
        OwtchBehavior.AddOwtchShotReactions(table);
        WallSpacePirateBehavior.AddWallSpacePirateShotReactions(table);
        ShuttersBehavior.AddShuttersShotReactions(table);

        MetroidBehavior.AddMetroidShotReactions(table);
        YardBehavior.AddYardShotReactions(table);

        // The metal-debris shot handler saves these graphics words around common shot AI, uses them for four metal debris actors on
        // death, then clears the dead body. Spawning before the clears preserves that exact graphics index.
        MetareeBehavior.AddMetareeShotReactions(table);

        // Skree's shot handler adds its four debris actors after the shared normal shot handler reports death, before the
        // common death animation releases the enemy slot.
        SkreeBehavior.AddSkreeShotReactions(table);
        FirefleaBehavior.AddFirefleaShotReactions(table);
        PlatformsBehavior.AddPlatformsShotReactions(table);
        BeetomBehavior.AddBeetomShotReactions(table);
        PowampBehavior.AddPowampShotReactions(table);
        WorkRobotBehavior.AddWorkRobotShotReactions(table);
        FakeKraidBehavior.AddFakeKraidShotReactions(table);
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
        BotwoonBehavior.AddBotwoonShotReactions(table);
        ShaktoolBehavior.AddShaktoolShotReactions(table);

        // Ordinary common shot AI with no species stage.
        CeresSteamBehavior.AddCeresSteamShotReactions(table);
        MochtroidBehavior.AddMochtroidShotReactions(table);
        // Several retail helper/projectile definitions ignore shots. Radius collision still dispatches that reaction
        // after its projectile prelude.
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.ShotAi(enemy) == EnemyReaction.Ignore));
        table.AddShot(new EnemyShotReaction(enemy => EnemyCollision.ShotAi(enemy) == EnemyReaction.NormalEnemyShot));
}

    /// <summary>Registers Samus-contact handlers after species instruction registrations.</summary>
    /// <remarks>Each species registers its own run of reactions; first match wins, so the interleaving below is the
    /// established precedence between species and shared registrations. Matches are mutually exclusive (each names its
    /// definition and header callback); the ordinary normal-touch handler is last because Crocomire's tongue also uses
    /// NormalEnemyTouch.</remarks>
    private void AddTouchHandlers(EnemySharedReactions table)
    {
        KraidBehavior.Projectiles.AddKraidNailTouch(table);
        KraidBehavior.Combat.AddKraidTouch(table);

        MotherBrainCombat.AddMotherBrainHeadTouch(table);
        ShitroidBehavior.AddShitroidTouch(table);
        DeadSidehopperBehavior.AddDeadSidehopperTouch(table);
        DeadTourianBehavior.AddDeadTourianTouch(table);
        DeadTorizoBehavior.AddDeadTorizoTouch(table);
        WallSpacePirateBehavior.AddWallSpacePirateTouch(table);
        BombTorizoBehavior.Combat.AddBombTorizoTouch(table);

        PhantoonBehavior.Combat.AddPhantoonBodyTouch(table);
        DraygonBehavior.Combat.AddDraygonTouch(table);
        SporeSpawnBehavior.AddSporeSpawnTouch(table);
        CeresSteamBehavior.AddCeresSteamTouch(table);
        CrocomireBehavior.Combat.AddCrocomireTouch(table);
        MaridiaLargeSnailBehavior.AddMaridiaLargeSnailTouch(table);
        PlatformsBehavior.AddPlatformsTouch(table);

        // Headers that ignore Samus's touch: the collision is still reported, but the reaction performs no damage,
        // knockback, or actor reaction.
        table.AddTouch(slot => EnemyCollision.TouchAi(slot) == EnemyReaction.Ignore,
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
        BotwoonBehavior.AddBotwoonTouch(table);

        ZebetiteBehavior.AddZebetiteTouch(table);
        FakeKraidBehavior.AddFakeKraidTouch(table);
        ShaktoolBehavior.AddShaktoolTouch(table);

        table.AddTouch(slot => EnemyCollision.TouchAi(slot) == EnemyReaction.NormalEnemyTouch,
            context => Collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
}


    /// <summary>Maps hurt-state AI used while enemies are flashing or reacting.</summary>
    /// <remarks>Hurt AI is a first-match list in the enemy pass's dispatch order; the shared no-op hurt
    /// AI is last.</remarks>
    private void AddHurtAi(EnemySharedReactions table)
    {
        MetroidBehavior.AddMetroidHurtAi(table);
        BombTorizoBehavior.Combat.AddBombTorizoHurtAi(table);
        RidleyBehavior.AddRidleyHurtAi(table);
        PhantoonBehavior.AddPhantoonHurtAi(table);
        DraygonBehavior.AddDraygonHurtAi(table);
        table.AddHurt(slot => slot.Definition.HurtAi == EnemyAiRoutine.Inert, (_, _) => { });
    }

    /// <summary>Maps frozen-state AI used while ice suspends normal behavior.</summary>
    /// <remarks>Rinka's frozen tail runs before the common thaw and can consume the frame; the others run after
    /// it.</remarks>
    private void AddFrozenAi(EnemySharedReactions table)
    {
        RinkaBehavior.AddRinkaFrozenPreemption(table);
        MetroidBehavior.AddMetroidFrozenAi(table);
        YappingMawBehavior.AddYappingMawFrozenAi(table);
    }
}
