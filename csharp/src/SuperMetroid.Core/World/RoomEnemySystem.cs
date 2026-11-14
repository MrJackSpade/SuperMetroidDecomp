using SuperMetroid.Core.Rooms.Scrolling;
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
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
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
/// Room-enemy loader, scheduler, shared animation, and draw queues. Retail
/// gameplay definitions are compiled; installed artwork supplies pixels and colors.
/// </summary>
/// <remarks>
/// The room keeps a fixed set of enemy slots in <see cref="EnemySlotOrder"/>, each with a stable ordinal. The ordinals
/// are gameplay: multi-part enemies find their parts by neighbouring ordinals, Samus records the ordinal of the enemy she
/// meets, and the gunship's lower part copies fields from the slot before it. A compacted list would lose that.
/// </remarks>
[RoomLifetime]
public sealed class RoomEnemySystem : IEnemyFrameResults, IEnemyDisplaySources
{
    /// <summary>The <see cref="EnemyDrawList"/> owner of this RoomEnemySystem.</summary>
    internal EnemyDrawList Drawing { get; }

    /// <summary>The <see cref="EnemyAnimationPlayer"/> owner of this RoomEnemySystem.</summary>
    internal EnemyAnimationPlayer AnimationPlayer { get; }

    /// <summary>The <see cref="EnemyPopulationLoader"/> owner of this RoomEnemySystem.</summary>
    internal EnemyPopulationLoader PopulationLoader { get; }

    /// <summary>The <see cref="EnemyProcessingWindow"/> owner of this RoomEnemySystem.</summary>
    internal EnemyProcessingWindow ProcessingWindow { get; }

    /// <summary>Puts populations into slots and runs their AI.</summary>
    internal EnemyLifecycle Lifecycle { get; }

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

    /// <summary>The room camera's distance mode, which Kraid and Crocomire request (#1874).</summary>
    internal RoomCameraDistance CameraDistance { get; } = new();

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

    /// <summary>The <see cref="Areas.Ceres.CeresFallingTiles"/> owner of this RoomEnemySystem.</summary>
    internal CeresFallingTiles CeresFallingTiles { get; }

    /// <summary>The <see cref="Enemies.SaveStation.SaveStationElectricity"/> owner of this RoomEnemySystem.</summary>
    internal SaveStationElectricity SaveStationElectricity { get; }

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

    /// <summary>The enemies' collision, for the shared responses to resolve at use.</summary>
    private EnemyCollision GetCollision() => Collision;

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
        if (active?.MessageId == EscapeTypewriterMessageId.Zebes)
        {
            if (value is null) active.UnbindMessage();
            else active.BindMessage(value.Get(EscapeTypewriterMessageId.Zebes));
        }
        EscapeTypewriterState? ceres = RidleyEncounter.Ridley?.CeresEscapeTypewriter;
        if (ceres?.MessageId == EscapeTypewriterMessageId.Ceres)
        {
            if (value is null) ceres.UnbindMessage();
            else ceres.BindMessage(value.Get(EscapeTypewriterMessageId.Ceres));
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

    /// <summary>Post-AI collision snapshots for the room's active enemies.</summary>
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();

    /// <summary>The elevator journey the runtime reads and resets.</summary>
    internal IElevatorJourney Elevator => ElevatorBehavior;

    /// <summary>The gunship's save prompt and health check.</summary>
    internal IGunshipSession Gunship => field ??= new GunshipSession(GunshipBehavior);

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    internal ICeresEncounter Ceres => field ??= new CeresEncounter(RidleyEncounter, CeresRidleyBehavior);

    /// <summary>The state and hooks the room main behaviours and scrolling use.</summary>
    internal IEnemyRoomMainHooks RoomMainHooks => field ??= new EnemyRoomMainHooks(CrocomireBehavior, SporeSpawnBehavior, CameraDistance);

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

    /// <summary>The enemies' colour effect glows and flashes the room's colour effects show.</summary>
    internal IEnemyColourEffectShows ColourEffectShows => field ??= new EnemyColourEffectShows(BombTorizoBehavior, TourianEntranceStatueBehavior,
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
        ?? (Presentation.BossBody is not { IsShown: true } body ? null
            : CrocomireBehavior.Access.Crocomire is not null && !CrocomireBehavior.BodyLayer.ShowsBody ? BossBodyLayer.Empty
            : body.Publish());
    /// <inheritdoc/>
    Position16<RoomPixels>? IEnemyDisplaySources.KraidBodyOrigin => KraidBehavior.BodyLayer.BodyOrigin;
    /// <inheritdoc/>
    Position16<RoomPixels>? IEnemyDisplaySources.CrocomireBodyOrigin =>
        CrocomireBehavior.Access.Crocomire is not null ? CrocomireBehavior.BodyLayer.BodyOrigin : null;
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
        BossFrameResults.RemoveLiquidEffects, FrameResults.Gunship, BossFrameResults.RoomEffects(), FrameResults.Glow);

    /// <summary>
    /// Creates a room's enemy system: every enemy slot, each with its stable ordinal, with the door-transition gates
    /// clear, without loading a population or binding memory, graphics, random-number, or gameplay services.
    /// </summary>
    /// <param name="session">The enemy state and installed artwork that outlive each room.</param>
    /// <param name="compatibility">
    /// The compatibility services the room's enemies consult: the room's own, created with the system by its builder, and
    /// those that outlive every room, which the builder hands on.
    /// </param>
    /// <param name="definitions">The room's enemy definition source, or null for an enemy system with no population.</param>
    /// <param name="services">The game-state services the room's AI calls, or null when none is supplied.</param>
    /// <param name="bossArtwork">The session's installed boss artwork, or null for a system with its own, empty binding.</param>
    /// <param name="breakableTerrain">
    /// The room's room objects that show its enemy-breakable blocks breaking, which live as long as the room; null for a
    /// system whose enemies never break a block.
    /// </param>
    internal RoomEnemySystem(EnemySession session, RoomEnemyCompatibility compatibility,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null,
        BossArtworkBinding? bossArtwork = null, EnemyBreakableTerrainObjects? breakableTerrain = null)
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
        MorphBallEyeBehavior = new MorphBallEyeBehavior(Audio, SlotTable);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, new MorphBallEyeBeamStep(MorphBallEyeBehavior), Room, SlotTable);
        Lifecycle = new EnemyLifecycle(Room, SlotTable, ProcessingWindow);
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, SlotTable, Lifecycle);
        RidleyEncounter = new RidleyEncounter(session);
        Earthquake = new RoomEarthquake();
        PuyoCeilingTurn = compatibility.PuyoCeilingTurn;
        SpriteObjects = new RoomSpriteObjects(Presentation, compatibility.SpriteObjectPlaces, PuyoCeilingTurn);
        Projectiles = new EnemyProjectileEngine(Room, Presentation, SpriteObjects, Earthquake, Audio);
        GunshipBehavior = new GunshipBehavior(Audio, Projectiles, Room, SlotTable, FrameResults);
        Terrain = new EnemyTerrain(breakableTerrain);
        CorpseRowDust = new CorpseRowDust(Audio);
        MotherBrainEncounter = new MotherBrainEncounter(Audio);
        RidleyTail = new RidleyTail(FrameContext, Projectiles, RidleyEncounter, Room);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, Lifecycle, compatibility.PowerBombRadius, Audio);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Lifecycle = Lifecycle, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, FrameContext = FrameContext, Projectiles = Projectiles, SpriteObjects = SpriteObjects, Room = Room, SlotTable = SlotTable, PowerBombRadius = compatibility.PowerBombRadius });
        RidleyProjectiles = new RidleyProjectiles(DeathsAndDrops, Projectiles, RidleyEncounter);
        ZoaBehavior = new ZoaBehavior();
        ElevatorBehavior = new ElevatorBehavior(Room, Audio, FrameResults);
        ZebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Audio = Audio, Lifecycle = Lifecycle, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, SlotTable = SlotTable, ElevatorBehavior = ElevatorBehavior, RoomStatus = RoomStatus, Collision = Collision, FadeStepCount = compatibility.FadeStepCount, DeathsAndDrops = DeathsAndDrops });
        YardBehavior = new YardEnemyBehavior(Room, Terrain, Earthquake, Collision, Audio);
        YappingMawBehavior = new YappingMawBehavior(Projectiles, SpriteObjects, ScrollCap, Audio);
        WreckedShipGhostBehavior = new WreckedShipGhostBehavior();
        WorkRobotBehavior = new WorkRobotBehavior(Room, Terrain, Projectiles, SlotTable, Audio);
        WaverBehavior = new WaverBehavior(Terrain);
        WalkingSpacePirateBehavior = new WalkingSpacePirateBehavior(Terrain, Projectiles, Audio, Collision, DeathsAndDrops);
        WallSpacePirateBehavior = new WallSpacePirateBehavior(Room, Terrain, WalkingSpacePirateBehavior);
        TourianEntranceStatueBehavior = new TourianEntranceStatueBehavior(Projectiles, SlotTable);
        TourianUnlockBehavior = new TourianUnlockBehavior(Room, Projectiles, Earthquake, Audio, TourianEntranceStatueBehavior);
        SporeSpawnBehavior = new SporeSpawnBehavior(new SporeSpawnBehaviorDependencies { Audio = Audio, Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults });
        FallingSparksBehavior = new FallingSparksBehavior(Room, Projectiles, SpriteObjects);
        SparkBehavior = new SparkBehavior(FallingSparksBehavior);
        SkulteraBehavior = new SkulteraBehavior(Terrain);
        SkreeBehavior = new SkreeBehavior(Audio, Projectiles, DeathsAndDrops);
        ShuttersBehavior = new ShuttersBehavior(Audio, SlotTable);
        ShitroidBehavior = new ShitroidBehavior(Room, SlotTable, compatibility.FadeStepCount, BossFrameResults, Audio);
        DeadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = Room, Terrain = Terrain, Shitroid = new ShitroidFeeding(ShitroidBehavior), FrameContext = FrameContext, Projectiles = Projectiles, Collision = Collision, RowDust = CorpseRowDust });
        ShaktoolBehavior = new ShaktoolBehavior(Terrain, Projectiles, SlotTable, Collision, compatibility.KilledShaktoolSegment);
        SbugBehavior = new SbugBehavior(Room, Terrain);
        RipperBehavior = new RipperBehavior(Terrain);
        RioBehavior = new RioBehavior(Room, Terrain, Audio);
        RinkaBehavior = new RinkaBehavior(Collision, FrameContext, Projectiles, DeathsAndDrops);
        CeresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { FrameResults = BossFrameResults, Lifecycle = Lifecycle, Room = Room, Earthquake = Earthquake, Audio = Audio, SlotTable = SlotTable, Presentation = BossPresentation, Collision = Collision, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, RoomStatus = RoomStatus, UnauthoredBoostCadence = compatibility.UnauthoredBoostCadence });
        RidleyBehavior = new RidleyBehavior(new RidleyBehaviorDependencies { Lifecycle = Lifecycle, Room = Room, Earthquake = Earthquake, Audio = Audio, DeathsAndDrops = DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyTail = RidleyTail, CeresRidleyBehavior = CeresRidleyBehavior, RidleyProjectiles = RidleyProjectiles, RoomStatus = RoomStatus, RoomFx = RoomFx, FrameContext = FrameContext });
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
        BabyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Lifecycle = Lifecycle, Room = Room, Audio = Audio, SlotTable = SlotTable, FrameContext = FrameContext, Projectiles = Projectiles, MotherBrainEncounter = MotherBrainEncounter });
        MotherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { FrameResults = BossFrameResults, Lifecycle = Lifecycle, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, RoomActors = new MotherBrainRoomActors(BabyMetroidCutsceneBehavior, RinkaBehavior, WalkingSpacePirateBehavior, RidleyProjectiles)  });
        MochtroidBehavior = new MochtroidBehavior(Terrain, Audio, FrameContext, Collision);
        MetroidBehavior = new MetroidBehavior(Room, Terrain, Audio, SpriteObjects, FrameContext, DeathsAndDrops);
        MetareeBehavior = new MetareeBehavior(Projectiles, Audio);
        MaridiaLargeSnailBehavior = new MaridiaLargeSnailBehavior(Terrain, Collision, Audio);
        MamaTurtleBehavior = new MamaTurtleBehavior(Terrain, Earthquake, SlotTable, Collision, Audio);
        MagdolliteBehavior = new MagdolliteBehavior(Room, Projectiles, SlotTable, Collision, DeathsAndDrops, Audio);
        LowerNorfairRioBehavior = new LowerNorfairRioBehavior(Room, Terrain, SlotTable, Audio);
        KzanBehavior = new KzanBehavior(SlotTable, Audio);
        KraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { FrameResults = BossFrameResults, Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, SpriteObjects = SpriteObjects, Presentation = BossPresentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, CameraDistance = CameraDistance  });
        KiHunterBehavior = new KiHunterBehavior(Room, Terrain, Projectiles, Audio, SlotTable);
        KagoBehavior = new KagoBehavior(Room, Projectiles, Earthquake, DeathsAndDrops, RoomStatus, Audio);
        HoppersBehavior = new HoppersBehavior(Room, Terrain, Audio);
        HibashiBehavior = new HibashiBehavior(SlotTable, Audio);
        GrappleBehavior = new GrappleBehavior(Lifecycle, Room, DeathsAndDrops);
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
        DeadTorizoBehavior = new DeadTorizoBehavior(Room, FrameContext, Projectiles, Presentation, Audio, SlotTable);
        DachoraBehavior = new DachoraBehavior(Terrain, SlotTable, Audio);
        CrocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { Access = new CrocomireEncounterAccess(BossFrameResults, Room, Projectiles, Audio), Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, SpriteObjects = SpriteObjects, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, CameraDistance = CameraDistance, RoomStatus = RoomStatus  });
        CrawlersBehavior = new CrawlersBehavior(Terrain, Earthquake);
        ChozoStatueBehavior = new ChozoStatueBehavior(Room, Terrain, Projectiles, Audio, RoomFx);
        ChootBehavior = new ChootBehavior();
        CeresSteamBehavior = new CeresSteamBehavior(Room, Collision);
        CeresFallingTiles = new CeresFallingTiles(Room, Projectiles, Audio);
        SaveStationElectricity = new SaveStationElectricity(Room, Projectiles);
        CeresDoorBehavior = new CeresDoorBehavior(new CeresDoorBehaviorDependencies { Room = Room, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, Projectiles = Projectiles, CeresEscape = RidleyEncounter, RidleyScene = RidleyEncounter });
        CacatacBehavior = new CacatacBehavior(Room, Projectiles, Audio);
        BullBehavior = new BullBehavior(compatibility.BullInheritance);
        BoyonBehavior = new BoyonBehavior(Audio);
        BoulderBehavior = new BoulderBehavior(Terrain, Projectiles, Audio);
        BotwoonBehavior = new BotwoonBehavior(new BotwoonBehaviorDependencies { Audio = Audio, Room = Room, Projectiles = Projectiles, SpriteObjects = SpriteObjects, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults });
        AnimationPlayer = new EnemyAnimationPlayer(Presentation);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { DeadTorizoBehavior = DeadTorizoBehavior, FrameContext = FrameContext, AnimationPlayer = AnimationPlayer, MotherBrainBehavior = MotherBrainBehavior, Presentation = Presentation, Room = Room, PuyoCeilingTurn = PuyoCeilingTurn });
        BombTorizoBehavior = new BombTorizoBehavior(new BombTorizoBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, FrameResults = BossFrameResults });
        BlueBrinstarFaceBlockBehavior = new BlueBrinstarFaceBlockBehavior(SlotTable, ElevatorBehavior);
        BeetomBehavior = new BeetomBehavior(Room, Terrain, Audio, FrameContext, Collision, compatibility.BeetomInheritance);
        AtomicBehavior = new AtomicBehavior();
        AlcoonBehavior = new AlcoonBehavior(Room, Terrain, Projectiles, Audio);
        // Every species with room or frame lifecycle state, in the game's reset order.
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
        Species = EnemySpeciesRegistry.ForRetailRooms(new EnemyCommonParticipation(GetCollision), [BoyonBehavior, BullBehavior, ZoaBehavior, WaverBehavior, ChootBehavior, AtomicBehavior, SkulteraBehavior, PuyoBehavior, RioBehavior, FliesBehavior, SparkBehavior, SbugBehavior, MultiviolaBehavior, CacatacBehavior, AlcoonBehavior, BoulderBehavior, NuclearWaffleBehavior, HibashiBehavior, WreckedShipGhostBehavior, KzanBehavior, RipperBehavior, MochtroidBehavior, SkreeBehavior, OwtchBehavior, BeetomBehavior, KagoBehavior, FirefleaBehavior, MetareeBehavior, PolypBehavior, HoppersBehavior, CrawlersBehavior, PlatformsBehavior, YardBehavior, YappingMawBehavior, EtecoonBehavior, DachoraBehavior, MaridiaLargeSnailBehavior, WorkRobotBehavior, BlueBrinstarFaceBlockBehavior, DragonBehavior, FuneNamiheBehavior, PowampBehavior, NorfairRioBehavior, LowerNorfairRioBehavior, NorfairLavaJumpingEnemyBehavior, ShaktoolBehavior, MetroidBehavior, WalkingSpacePirateBehavior, WallSpacePirateBehavior, NinjaSpacePirateBehavior, ChozoStatueBehavior, TourianEntranceStatueBehavior, DeadSidehopperBehavior, DeadTorizoBehavior, DeadTourianBehavior, ShitroidBehavior, FakeKraidBehavior, ElevatorBehavior, MagdolliteBehavior, EscapeAnimalsBehavior, RinkaBehavior, ZebetiteBehavior, ShuttersBehavior, EvirBehavior, KiHunterBehavior, PipeBugsBehavior, MamaTurtleBehavior, MorphBallEyeBehavior, CeresDoorBehavior, CeresSteamBehavior, GunshipBehavior, BotwoonBehavior, SporeSpawnBehavior, BombTorizoBehavior, KraidBehavior, CrocomireBehavior, DraygonBehavior, PhantoonBehavior, MotherBrainBehavior, BabyMetroidCutsceneBehavior,
            new RidleyEntityFactory(RidleyBehavior, CeresRidleyBehavior, RidleyEncounter)]);
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

    /// <summary>Collision values read from the current slots after their AI has run.</summary>
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

        // A room that places no enemies keeps drawing its spawned objects from the standard artwork (#1599): an empty
        // population loads no enemy artwork, so the objects such a room shows appear in their standard variants.
        RoomEnemyPopulationDefinition population = request.Population;
        if (!population.Placements.IsEmpty)
            Presentation.DrawPopulation(population);
        PopulationLoader.LoadPopulation(
            definitions,
            population,
            new EnemySpawnSurroundings(request.Level, request.Samus, request.ControllerInput, request.Camera));
    }

    /// <summary>
    /// Runs one enemy pass over the processing list the frame already built: executes selected AI and animations,
    /// publishes live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    internal EnemyFrameResult StepFrame(RoomEnemyFrame frame)
    {
        Room.EnsureLoaded();
        DeathsAndDrops.BindSamus(frame.Samus);
        ScrollCap.BeginFrame(frame.SamusPreviousPositionCheckpoint);
        FrameContext.BeginFrame(frame.TimeIsFrozen, frame.SamusProjectiles, frame.SamusColours, frame.SharedProjectiles.PowerBombExplosion, frame.FrameCounter);
        var aiFrame = new ClockedEnemyAiFrame(frame, new FrameCounters(frame.ShortCounter));
        FrameResults.Begin();
        BossFrameResults.Begin();
        foreach (IEnemySpecies species in _species)
            species.BeginEnemyFrame(frame);
        Drawing.Clear();
        ShakeWallJumpLaunchEnemy(frame.Samus);

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
                Collision.ResolveOrdinaryProjectileHits(frame.SamusProjectiles, frame.SharedProjectiles, frame.RoomArea,
                    frame.Samus, slot, frame.Camera);
                // The input phase already updated the bomb slots. The enemy pass checks
                // them before this actor's touch and AI, so escape starts now.
                Collision.ResolveOrdinaryBombHits(frame.SharedProjectiles, frame.SamusProjectiles, frame.Samus, slot, frame.Camera);
                if (slot.IsFree)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.ControllerInput, frame.Level, slot, frame.Camera);
                if (slot.Entity is IEnemyContactHooks contact)
                    contact.ResolveAfterSamusContact(frame.Samus);
                if (slot.IsFree)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(slot, aiFrame);
                // The hurt behaviour replaces main AI while the enemy is hurt: the species' own, or the inert one its
                // definition names. The grappled, hurt and frozen states are tried in that order, and the first one the
                // enemy is in selects its behaviour even when that behaviour does nothing. Hurt therefore delays frozen
                // AI until the flash's end clears the hurt state; falling through here prematurely clears flash.
                if (!ranActorAi &&
                    (slot.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None &&
                    slot.Entity is IEnemyHurtReaction hurt &&
                    hurt.RunHurt(frame))
                {
                    ranActorAi = true;
                }
                // The enemy pass selects the AI from the first state the enemy is in alone. A frozen
                // clock without the frozen state (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (slot.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None)
                {
                    // Rinka's termination tail clears/replaces the current actor record.
                    // The active-index array was frozen before AI, so skip the remainder
                    // of this stale entry exactly as its own death call does.
                    var frozen = slot.Entity as IEnemyFrozenAi;
                    if (frozen?.PreemptFrozenTurn() == true)
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // AI for this call. Ice removal instead takes the thaw branch
                    // immediately. That branch leaves the freeze clock at the remainder
                    // ThawedFreezeLeftover names rather than zero, which matters when hurt
                    // and frozen AI coexist.
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
                    frozen?.RunFrozen();
                    // The frame clock advances after every selected AI call,
                    // including frozen AI. Animation gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    EnemyLifecycle.RunMainAi(slot, aiFrame);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    slot.FrameCounter = WrappingCounter.IncrementCount(slot.FrameCounter);
                    // The enemy pass skips the animation whenever the enemy is
                    // frozen, even if the hurt state, tried first, selected an
                    // enemy-specific hurt behaviour above. Testing only `ranActorAi` here
                    // incorrectly advanced a Metroid's body animation while its frozen
                    // behaviour still owned the two composited outer sprite objects.
                    if ((slot.AiStates & EnemyAiStates.Frozen) == EnemyAiStates.None &&
                        slot.Processing.HasAny(EnemyProcessing.Animates))
                        EnemyAnimationPlayer.AdvanceAnimation(
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
                EnemyProcessingWindow.IsWithinProcessingWindow(slot, frame.Camera);
            if (!slot.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted))
                Drawing.Enqueue(slot, reached);

            // The enemy pass performs this after the enemy has been admitted to its draw queue.
            // Keeping it here also makes the boss's latched draw palettes describe the
            // frame just processed rather than the already-decremented following frame.
            if (!frame.TimeIsFrozen && slot.FlashTimer != 0)
            {
                slot.FlashTimer--;
                // The enemy pass clears the hurt AI once the post-decrement timer is below
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
                slot,
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

    /// <summary>
    /// Shakes the solid or frozen enemy Samus's latest wall jump launched from, as the enemy pass does before any enemy
    /// updates. While no enemy place is filled the request waits.
    /// </summary>
    internal void ShakeWallJumpLaunchEnemy(SamusState? samus)
    {
        if (SlotTable.EnemyCount == 0 || samus?.Kinematics.TakeWallJumpLaunchShake() is not { } launchedFrom)
            return;
        launchedFrom.ShakeFromWallJumpLaunch();
    }

    /// <summary>Runs the room shaking: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        Room.EnsureLoaded();
        // This runs after drawing in the game, so the two-frame actor timer becomes visible
        // on the following frame rather than retroactively changing already-emitted sprites.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (RoomEnemySlot slot in ProcessingWindow.ActiveEnemies)
                slot.ShakeTimer = RoomEarthquakeDefinitions.EnemyShakeDuration;
        }
        return Earthquake.LastRoomShake;
    }

    /// <summary>Advances enemy-projectile actors after Samus movement and before room objects.</summary>
    public void AdvanceEnemyProjectileAnimations(
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
}
