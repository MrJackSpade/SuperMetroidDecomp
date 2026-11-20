using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Doors;
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
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The composition of a room's species (#2571): the one place that names each species to build its owner from the room's
/// shared enemy services and register it. What it hands the room's enemy system are the contracts the shared phases
/// iterate (the species' frame resets, their factories, glows, late and pass draws, display latches, steps before
/// selection, after the pass and with the room's effects) and the capability interfaces through which the rest of the game
/// reaches the enemies; it keeps no member per species, so no phase reaches a species by name.
/// </summary>
/// <remarks>
/// The owners are built in the game's own order, after the shared services they use, and the frame participants are kept
/// in the game's reset order (<see cref="FrameParticipants"/>).
/// </remarks>
[SpeciesComposition]
[RoomLifetime]
internal sealed class RoomEnemySpecies
{
    /// <summary>Builds every owner the room's scenery lets it build and registers each with its contracts.</summary>
    /// <param name="enemies">
    /// The room's shared enemy services every owner receives, the enemy session, the room's compatibility services, its
    /// scenery and Samus's passage through a door.
    /// </param>
    internal RoomEnemySpecies(RoomEnemySpeciesDependencies enemies)
    {
        Ensure.NotNull(enemies);
        EnemySession session = enemies.Session;
        DoorPassage doorPassage = Ensure.NotNull(enemies.DoorPassage);
        var morphBallEyeBehavior = new MorphBallEyeBehavior(enemies.Audio);
        RidleyEncounter = new RidleyEncounter(session);
        var gunshipBehavior = new GunshipBehavior(enemies.Audio, enemies.Projectiles, enemies.Room, enemies.Places, enemies.PassEvents, enemies.Presentation);
        CorpseRowDust = new CorpseRowDust(enemies.Audio);
        MotherBrainEncounter = new MotherBrainEncounter(enemies.Audio, enemies.BossEncounters);
        RidleyTail = new RidleyTail(enemies.Projectiles, enemies.Room);
        RidleyProjectiles = new RidleyProjectiles(enemies.Room, enemies.Projectiles);
        RidleyBody = new RidleyBody(new RidleyFlight(enemies.Earthquake), new RidleyReveal(doorPassage, enemies.Audio), new RidleyBodyAnimation(enemies.Audio, RidleyProjectiles),
            new RidleyBodyContact(enemies.Collision), new RidleyDrawing(enemies.Presentation), RidleyTail);
        CeresEscape = new CeresEscape(new CeresEscapeDependencies { PassEvents = enemies.PassEvents, Room = enemies.Room, Audio = enemies.Audio, Presentation = enemies.Presentation, Status = RidleyEncounter, RoomStatus = enemies.RoomStatus, BossArtwork = enemies.BossArtworkBinding, UnauthoredBoostCadence = enemies.UnauthoredBoostCadence });
        var zoaBehavior = new ZoaBehavior();
        var elevatorBehavior = new ElevatorBehavior(enemies.Room, enemies.Audio, enemies.PassEvents, doorPassage);
        var zebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Audio = enemies.Audio, Lifecycle = enemies.Lifecycle, Room = enemies.Room, Earthquake = enemies.Earthquake, Places = enemies.Places, DoorPassage = doorPassage, RoomStatus = enemies.RoomStatus, Collision = enemies.Collision, FadeStepCount = enemies.FadeStepCount, DeathsAndDrops = enemies.DeathsAndDrops });
        var yardBehavior = new YardEnemyBehavior(enemies.Room, enemies.Terrain, enemies.Earthquake, enemies.Collision, enemies.Audio, enemies.YardKickDistance);
        var yappingMawBehavior = new YappingMawBehavior(enemies.Projectiles, enemies.EffectActors, enemies.ScrollCap, enemies.Audio);
        var wreckedShipGhostBehavior = new WreckedShipGhostBehavior();
        var workRobotBehavior = new WorkRobotBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.Audio);
        var waverBehavior = new WaverBehavior(enemies.Terrain);
        var walkingSpacePirateBehavior = new WalkingSpacePirateBehavior(enemies.Terrain, enemies.Projectiles, enemies.Audio, enemies.Collision, enemies.DeathsAndDrops);
        var wallSpacePirateBehavior = new WallSpacePirateBehavior(enemies.Room, enemies.Terrain, walkingSpacePirateBehavior);
        // The statue room alone builds the statue scene and its unlock effects, with its room (#2426, #2486).
        TourianEntranceStatueBehavior? tourianEntranceStatueBehavior = null;
        if (enemies.Scenery.TourianStatueRoom is { } statueRoom)
        {
            var statueScene = new TourianEntranceStatueBehavior(enemies.Projectiles, statueRoom);
            var unlockEffects = new TourianUnlockBehavior(enemies.Room, enemies.Projectiles, enemies.Earthquake, enemies.Audio, statueScene, statueRoom);
            tourianEntranceStatueBehavior = statueScene;
            TourianStatue = new TourianStatueActors(statueScene, unlockEffects);
        }
        var sporeSpawnBehavior = new SporeSpawnBehavior(new SporeSpawnBehaviorDependencies { Audio = enemies.Audio, Room = enemies.Room, Presentation = enemies.Presentation, Projectiles = enemies.Projectiles, EffectActors = enemies.EffectActors, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, PassEvents = enemies.PassEvents, CameraFloor = enemies.CameraFloor });
        var fallingSparksBehavior = new FallingSparksBehavior(enemies.Room, enemies.Projectiles, enemies.EffectActors, enemies.FallingSparkFloorProbe);
        var sparkBehavior = new SparkBehavior(fallingSparksBehavior);
        var skulteraBehavior = new SkulteraBehavior(enemies.Terrain);
        var skreeBehavior = new SkreeBehavior(enemies.Audio, enemies.Projectiles, enemies.DeathsAndDrops);
        var shuttersBehavior = new ShuttersBehavior(enemies.Audio, enemies.Places);
        var babyMetroidBehavior = new BabyMetroidBehavior(enemies.Room, enemies.Places, enemies.FadeStepCount, enemies.PassEvents, enemies.Audio);
        var deadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = enemies.Room, Terrain = enemies.Terrain, BabyMetroidFeeding = new BabyMetroidFeeding(enemies.Places), FrameContext = enemies.FrameContext, Projectiles = enemies.Projectiles, Collision = enemies.Collision, RowDust = CorpseRowDust });
        var shaktoolBehavior = new ShaktoolBehavior(enemies.Terrain, enemies.Places, enemies.Collision);
        var sbugBehavior = new SbugBehavior();
        var ripperBehavior = new RipperBehavior(enemies.Terrain);
        var rioBehavior = new RioBehavior(enemies.Room, enemies.Terrain, enemies.Audio);
        var rinkaBehavior = new RinkaBehavior(enemies.Collision, enemies.FrameContext, enemies.Projectiles, enemies.DeathsAndDrops);
        var ceresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { Lifecycle = enemies.Lifecycle, Room = enemies.Room, Earthquake = enemies.Earthquake, Audio = enemies.Audio, Places = enemies.Places, Presentation = enemies.Presentation, RidleyEncounter = RidleyEncounter, RidleyBody = RidleyBody, Escape = CeresEscape });
        var norfairRidleyBehavior = new NorfairRidleyBehavior(new NorfairRidleyBehaviorDependencies { Lifecycle = enemies.Lifecycle, Room = enemies.Room, Earthquake = enemies.Earthquake, Audio = enemies.Audio, DeathsAndDrops = enemies.DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyBody = RidleyBody, RidleyProjectiles = RidleyProjectiles, RoomFx = enemies.RoomFx, Presentation = enemies.Presentation });
        var puyoBehavior = new PuyoBehavior(enemies.Room, enemies.Terrain, enemies.PuyoCeilingTurn);
        var prePhantoonRoomBehavior = new PrePhantoonRoomBehavior(enemies.Projectiles);
        var powampBehavior = new PowampBehavior(enemies.Terrain, enemies.Projectiles, enemies.Places, enemies.Collision, enemies.DeathsAndDrops);
        var polypBehavior = new PolypBehavior(enemies.Room, enemies.Projectiles);
        var platformsBehavior = new PlatformsBehavior(enemies.Terrain);
        var pipeBugsBehavior = new PipeBugsBehavior(enemies.Places);
        var phantoonBehavior = new PhantoonBehavior(new PhantoonBehaviorDependencies { Room = enemies.Room, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Audio = enemies.Audio, Presentation = enemies.Presentation, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops , BossArtwork = enemies.BossArtworkBinding, PassEvents = enemies.PassEvents, Encounters = enemies.BossEncounters, Places = enemies.Places });
        var owtchBehavior = new OwtchBehavior(enemies.Room);
        var oldTourianEscapeShaftWallBehavior = new OldTourianEscapeShaftWallBehavior(enemies.Projectiles, enemies.Audio);
        var nuclearWaffleBehavior = new NuclearWaffleBehavior(enemies.Projectiles, enemies.EffectActors, enemies.Audio);
        var gerutaBehavior = new GerutaBehavior(enemies.Room, enemies.Terrain, enemies.Places, enemies.Audio);
        var squeeptBehavior = new SqueeptBehavior(enemies.Room, enemies.Places, enemies.Audio);
        var noobTubeBehavior = new NoobTubeBehavior(enemies.Room, enemies.Projectiles);
        var ninjaSpacePirateBehavior = new NinjaSpacePirateBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.EffectActors, walkingSpacePirateBehavior);
        var multiviolaBehavior = new MultiviolaBehavior(enemies.Terrain);
        var babyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Lifecycle = enemies.Lifecycle, Room = enemies.Room, Audio = enemies.Audio, Places = enemies.Places, FrameContext = enemies.FrameContext, Projectiles = enemies.Projectiles, MotherBrainEncounter = MotherBrainEncounter });
        var motherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { PassEvents = enemies.PassEvents, Lifecycle = enemies.Lifecycle, Places = enemies.Places, Room = enemies.Room, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Audio = enemies.Audio, Presentation = enemies.BossPresentation, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, RoomActors = new MotherBrainRoomActors(babyMetroidCutsceneBehavior, rinkaBehavior, walkingSpacePirateBehavior, RidleyProjectiles)  });
        var mochtroidBehavior = new MochtroidBehavior(enemies.Terrain, enemies.Audio, enemies.FrameContext, enemies.Collision);
        var metroidBehavior = new MetroidBehavior(enemies.Room, enemies.Terrain, enemies.Audio, enemies.EffectActors, enemies.FrameContext, enemies.DeathsAndDrops);
        var metareeBehavior = new MetareeBehavior(enemies.Projectiles, enemies.Audio);
        var oumBehavior = new OumBehavior(enemies.Terrain, enemies.Collision, enemies.Audio);
        var mamaTurtleBehavior = new MamaTurtleBehavior(enemies.Terrain, enemies.Earthquake, enemies.Places, enemies.Collision, enemies.Audio);
        var magdolliteBehavior = new MagdolliteBehavior(enemies.Room, enemies.Projectiles, enemies.Places, enemies.Collision, enemies.DeathsAndDrops, enemies.Audio);
        var holtzBehavior = new HoltzBehavior(enemies.Room, enemies.Terrain, enemies.Places, enemies.Audio);
        var kzanBehavior = new KzanBehavior(enemies.Places, enemies.Audio);
        var kraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { PassEvents = enemies.PassEvents, Room = enemies.Room, Terrain = enemies.Terrain, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Audio = enemies.Audio, EffectActors = enemies.EffectActors, Presentation = enemies.BossPresentation, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, CameraDistance = enemies.CameraDistance  });
        var kiHunterBehavior = new KiHunterBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.Audio, enemies.Places);
        var kagoBehavior = new KagoBehavior(enemies.Room, enemies.Projectiles, enemies.Earthquake, enemies.DeathsAndDrops, enemies.RoomStatus, enemies.Audio);
        var hoppersBehavior = new HoppersBehavior(enemies.Room, enemies.Terrain, enemies.Audio);
        var hibashiBehavior = new HibashiBehavior(enemies.Places, enemies.Audio);
        var funeNamiheBehavior = new FuneNamiheBehavior(enemies.Projectiles, enemies.Audio);
        var fliesBehavior = new FliesBehavior(enemies.Room);
        var firefleaBehavior = new FirefleaBehavior(enemies.Collision, enemies.DeathsAndDrops, enemies.RoomStatus);
        var fakeKraidBehavior = new FakeKraidBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.Collision, enemies.DeathsAndDrops, enemies.Audio);
        var eyeDoorBehavior = new EyeDoorBehavior(enemies.Room, enemies.Projectiles);
        var evirBehavior = new EvirBehavior(enemies.Places, enemies.Collision, enemies.Audio);
        var etecoonBehavior = new EtecoonBehavior(enemies.Terrain, enemies.Earthquake, doorPassage, enemies.Audio);
        var escapeExplosionsBehavior = new EscapeExplosionsBehavior(enemies.Audio, enemies.EffectActors);
        var escapeAnimalsBehavior = new EscapeAnimalsBehavior(enemies.Room, enemies.Terrain);
        var draygonBehavior = new DraygonBehavior(new DraygonBehaviorDependencies { PassEvents = enemies.PassEvents, Places = enemies.Places, Room = enemies.Room, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Audio = enemies.Audio, EffectActors = enemies.EffectActors, Presentation = enemies.Presentation, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, ScrollCap = enemies.ScrollCap , BossArtwork = enemies.BossArtworkBinding });
        var dragonBehavior = new DragonBehavior(enemies.Projectiles, enemies.Places, enemies.Collision, enemies.Audio);
        var downwardGateBehavior = new DownwardGateBehavior(enemies.Projectiles);
        var deadTourianBehavior = new DeadTourianBehavior(enemies.Room, enemies.FrameContext, enemies.Projectiles, CorpseRowDust);
        var deadTorizoBehavior = new DeadTorizoBehavior(enemies.Room, enemies.FrameContext, enemies.Projectiles, enemies.Presentation, enemies.Audio);
        var dachoraBehavior = new DachoraBehavior(enemies.Terrain, enemies.Places, enemies.Audio);
        var crocomireAccess = new CrocomireEncounterAccess(enemies.PassEvents, enemies.Room, enemies.Projectiles, enemies.Audio);
        var crocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { Access = crocomireAccess, Places = enemies.Places, BodyLayer = new CrocomireBodyLayer(enemies.EffectActors, enemies.BossPresentation.Surfaces, crocomireAccess, enemies.RoomFx), Room = enemies.Room, Terrain = enemies.Terrain, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Presentation = enemies.BossPresentation, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, CameraDistance = enemies.CameraDistance, RoomStatus = enemies.RoomStatus, Encounters = enemies.BossEncounters });
        var crawlersBehavior = new CrawlersBehavior(enemies.Terrain, enemies.Earthquake);
        var chozoStatueBehavior = new ChozoStatueBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.Audio, enemies.RoomFx, enemies.PassEvents);
        var chootBehavior = new ChootBehavior();
        var ceresSteamBehavior = new CeresSteamBehavior(enemies.Room, enemies.Collision);
        // The turning elevator shaft alone places vents that ride its turn (#2480).
        var steamVents = new CeresSteamFactory(ceresSteamBehavior, enemies.Scenery.ShaftTurn is { } shaft
            ? new CeresShaftSteamVentMotions(shaft)
            : CeresSteamVentsHoldingPlace.Instance);
        CeresFallingTiles = new CeresFallingTiles(enemies.Projectiles, enemies.Audio);
        SaveStationElectricity = new SaveStationElectricity(enemies.Projectiles);
        var ceresDoorBehavior = new CeresDoorBehavior(new CeresDoorBehaviorDependencies { Room = enemies.Room, Earthquake = enemies.Earthquake, Audio = enemies.Audio, Presentation = enemies.Presentation, Projectiles = enemies.Projectiles, CeresEscape = RidleyEncounter });
        // A room with a turning floor alone places the elevator room's blocked door, which cycles the floor's platform (#2426).
        CeresElevatorDoorFactory? elevatorDoors = enemies.Scenery.Floor is { } floor ? new CeresElevatorDoorFactory(ceresDoorBehavior, floor) : null;
        var cacatacBehavior = new CacatacBehavior(enemies.Room, enemies.Projectiles, enemies.Audio);
        var bullBehavior = new BullBehavior(enemies.BullInheritance);
        var boyonBehavior = new BoyonBehavior(enemies.Audio);
        var boulderBehavior = new BoulderBehavior(enemies.Terrain, enemies.Projectiles, enemies.Audio);
        var botwoonBehavior = new BotwoonBehavior(new BotwoonBehaviorDependencies { Audio = enemies.Audio, Room = enemies.Room, Projectiles = enemies.Projectiles, EffectActors = enemies.EffectActors, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, PassEvents = enemies.PassEvents });
        var torizoBehavior = new TorizoBehavior(new TorizoBehaviorDependencies { Room = enemies.Room, Terrain = enemies.Terrain, Projectiles = enemies.Projectiles, Earthquake = enemies.Earthquake, Audio = enemies.Audio, Presentation = enemies.Presentation, FrameContext = enemies.FrameContext, Collision = enemies.Collision, DeathsAndDrops = enemies.DeathsAndDrops, PassEvents = enemies.PassEvents });
        var blueBrinstarFaceBlockBehavior = new BlueBrinstarFaceBlockBehavior(doorPassage);
        var beetomBehavior = new BeetomBehavior(enemies.Room, enemies.Terrain, enemies.Audio, enemies.FrameContext, enemies.Collision, enemies.BeetomInheritance);
        var atomicBehavior = new AtomicBehavior();
        var alcoonBehavior = new AlcoonBehavior(enemies.Room, enemies.Terrain, enemies.Projectiles, enemies.Audio);
        // Every species with room or frame lifecycle state, in the game's reset order.
        FrameParticipants =
        [
            babyMetroidBehavior,
            boyonBehavior,
            mamaTurtleBehavior,
            metroidBehavior,
            ceresDoorBehavior,
            boulderBehavior,
            zebetiteBehavior,
            etecoonBehavior,
            dachoraBehavior,
            evirBehavior,
            morphBallEyeBehavior,
            yappingMawBehavior,
            funeNamiheBehavior,
            kagoBehavior,
            magdolliteBehavior,
            blueBrinstarFaceBlockBehavior,
            kiHunterBehavior,
            pipeBugsBehavior,
            botwoonBehavior,
            torizoBehavior,
            .. Present<IEnemySpecies>(tourianEntranceStatueBehavior),
            shaktoolBehavior,
            chozoStatueBehavior,
            escapeAnimalsBehavior,
            crocomireBehavior,
            sporeSpawnBehavior,
            rinkaBehavior,
            rioBehavior,
            squeeptBehavior,
            gerutaBehavior,
            holtzBehavior,
            oumBehavior,
            ripperBehavior,
            dragonBehavior,
            kraidBehavior,
            phantoonBehavior,
            draygonBehavior,
            motherBrainBehavior,
            deadTorizoBehavior,
            deadSidehopperBehavior,
            deadTourianBehavior,
            shuttersBehavior,
            elevatorBehavior,
            nuclearWaffleBehavior,
            fakeKraidBehavior,
            workRobotBehavior,
            norfairRidleyBehavior,
            puyoBehavior,
            cacatacBehavior,
            owtchBehavior,
            multiviolaBehavior,
            polypBehavior,
            crawlersBehavior,
            skreeBehavior,
            fliesBehavior,
            sbugBehavior,
            mochtroidBehavior,
            wreckedShipGhostBehavior,
            hoppersBehavior,
            zoaBehavior,
            yardBehavior,
            waverBehavior,
            metareeBehavior,
            firefleaBehavior,
            skulteraBehavior,
            chootBehavior,
            platformsBehavior,
            alcoonBehavior,
            beetomBehavior,
            powampBehavior,
            bullBehavior,
            atomicBehavior,
            sparkBehavior,
            kzanBehavior,
            hibashiBehavior,
            walkingSpacePirateBehavior,
            wallSpacePirateBehavior,
            ninjaSpacePirateBehavior,
        ];
        // The registry is the species above that create their own entities, with the owners that create entities but keep no
        // room or frame state (#2557): the Ceres doors' and the Ridleys' factories, the steam vents, the gunship, the Baby's
        // cutscene, the n00b tube and the turning floor's elevator door.
        Registry = EnemySpeciesRegistry.ForRoom(new EnemyCommonParticipation(() => enemies.Collision), [
            .. FrameParticipants.OfType<IEnemyEntityFactory>(),
            new CeresDoorFactory(ceresDoorBehavior), steamVents, gunshipBehavior, babyMetroidCutsceneBehavior, noobTubeBehavior,
            new RidleyEntityFactory(norfairRidleyBehavior, ceresRidleyBehavior, RidleyEncounter),
            .. Present<IEnemyEntityFactory>(elevatorDoors)],
            // What the owners the room's scenery did not let it build would have placed; the registry refuses a room that
            // builds an owner of one, or lacks an owner of any other.
            [
                .. tourianEntranceStatueBehavior is null ? [EnemyDefinitionId.TourianStatue] : Array.Empty<EnemyDefinitionId>(),
                .. elevatorDoors is null ? [EnemyDefinitionId.CeresElevatorDoor] : Array.Empty<EnemyDefinitionId>(),
            ]);
        BeforeSelection = [new MorphBallEyeBeamStep(morphBallEyeBehavior)];
        // The room-wide glows step in the game's order: the work robots', the Magdollites', the face blocks'.
        Glows = [workRobotBehavior, magdolliteBehavior, blueBrinstarFaceBlockBehavior];
        AfterPass = [new MotherBrainRainbowBeamStep(enemies.BossEncounters, enemies.BossArtworkBinding)];
        // Mother Brain's late parts draw before the dead Torizo's sand heap.
        LateDraws = [new MotherBrainLateParts(motherBrainBehavior, enemies.BossEncounters), new DeadTorizoSandHeap(deadTorizoBehavior)];
        var ceres = new CeresEncounter(RidleyEncounter, ceresRidleyBehavior, CeresEscape, ceresDoorBehavior);
        Ceres = ceres;
        PassScenes = [new CeresBabyPassScene(ceres)];
        DisplayLatches = [new BossBodyDisplayLatch(enemies.BossEncounters), morphBallEyeBehavior.MorphBallEyeBeam];
        RoomEffectSteps = [new PhantoonBodyDisplaySteps(enemies.BossEncounters)];
        Shadings = [ceresDoorBehavior, .. Present<IGameplayShadingSource>(tourianEntranceStatueBehavior)];
        Elevator = elevatorBehavior;
        Gunship = new GunshipSession(gunshipBehavior);
        LoaderHooks = new LoaderOperations(rinkaBehavior, prePhantoonRoomBehavior, RidleyEncounter);
        LoadPreparation = new RoomLoadPreparation(gunshipBehavior, rinkaBehavior);
        AttractDemoHooks = new AttractDemoOperations(kraidBehavior);
        RoomObjectSpawns = new ObjectSpawns(noobTubeBehavior, eyeDoorBehavior, oldTourianEscapeShaftWallBehavior, draygonBehavior,
            motherBrainBehavior, torizoBehavior, escapeExplosionsBehavior);
        RoomObjectRequests = new ObjectRequests(chozoStatueBehavior, downwardGateBehavior);
        EscapeShutterFlash = shuttersBehavior;
        Displays = new DisplayContributions(CeresEscape, MotherBrainEncounter, morphBallEyeBehavior.MorphBallEyeBeam);
    }

    /// <summary><paramref name="owner"/>, an owner the room's scenery may not let it build, when the room built it; else none.</summary>
    private static IEnumerable<T> Present<T>(T? owner) where T : class => owner is null ? [] : [owner];

    /// <summary>What the room sees of Ridley's encounters in Ceres and Lower Norfair, and the Ceres escape's progress.</summary>
    private RidleyEncounter RidleyEncounter { get; }

    /// <summary>Coordinates Ridley's multipart tail through the shared place and projectile services.</summary>
    private RidleyTail RidleyTail { get; }

    /// <summary>Ridley's fireballs and the afterburn that trails them, which Mother Brain's afterburn reuses.</summary>
    private RidleyProjectiles RidleyProjectiles { get; }

    /// <summary>The parts of Ridley's body both his encounters run (#2229).</summary>
    private RidleyBody RidleyBody { get; }

    /// <summary>The station's escape from Ceres Ridley's room: his getaway and the self-destruct warning (#2228).</summary>
    private CeresEscape CeresEscape { get; }

    /// <summary>Raises the dust of dead monsters' fallen corpse rows through the same frame as enemy processing.</summary>
    private CorpseRowDust CorpseRowDust { get; }

    /// <summary>Owns Mother Brain's room encounter and its cutscene state.</summary>
    private MotherBrainEncounter MotherBrainEncounter { get; }

    /// <summary>Every species with room or frame lifecycle state, in the game's reset order.</summary>
    internal IReadOnlyList<IEnemySpecies> FrameParticipants { get; }

    /// <summary>The registry that binds each installed occupant's entity through its species' factory.</summary>
    internal EnemySpeciesRegistry Registry { get; }

    /// <summary>The species' work that steps at the start of each update, before the enemy pass selects its enemies.</summary>
    internal IReadOnlyList<IStepBeforeEnemySelection> BeforeSelection { get; }

    /// <summary>The room-wide glows, which step after every enemy of the pass, in their order.</summary>
    internal IReadOnlyList<IRoomGlow> Glows { get; }

    /// <summary>The encounters' work that steps after the enemy pass's room effects on an unfrozen update.</summary>
    internal IReadOnlyList<IStepAfterEnemyPass> AfterPass { get; }

    /// <summary>The encounters' sprites drawn after every queued enemy, in their order.</summary>
    internal IReadOnlyList<IEnemyLateDraw> LateDraws { get; }

    /// <summary>The encounters' scenes that step beside the enemy pass and show among the actors, in their order.</summary>
    internal IReadOnlyList<IEnemyPassScene> PassScenes { get; }

    /// <summary>The enemies' displayed states, which the accepted frame's boundary latches.</summary>
    internal IReadOnlyList<IDisplayLatch> DisplayLatches { get; }

    /// <summary>The encounters' display effects, which step with the room's own effects.</summary>
    internal IReadOnlyList<IStepWithRoomEffects> RoomEffectSteps { get; }

    /// <summary>The species' owners of colour effects gameplay's pictures show, laid under the bosses' own.</summary>
    internal IReadOnlyList<IGameplayShadingSource> Shadings { get; }

    /// <summary>The elevator journey the runtime reads and resets.</summary>
    internal IElevatorJourney Elevator { get; }

    /// <summary>The gunship's save prompt and health check.</summary>
    internal IGunshipSession Gunship { get; }

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    internal ICeresEncounter Ceres { get; }

    /// <summary>The operations the room loader runs.</summary>
    internal IEnemyLoaderHooks LoaderHooks { get; }

    /// <summary>What the room's species take from the room's load before its population loads.</summary>
    internal IEnemyLoadPreparation LoadPreparation { get; }

    /// <summary>The enemy setup the attract demo's room setups perform.</summary>
    internal IEnemyAttractDemoHooks AttractDemoHooks { get; }

    /// <summary>The enemy-side effects room objects and room effects trigger.</summary>
    internal IEnemyObjectSpawns RoomObjectSpawns { get; }

    /// <summary>The room-object work the Chozo statues and downward gates exchange with their room objects each frame.</summary>
    internal IEnemyObjectRequests RoomObjectRequests { get; }

    /// <summary>The destroyable shutters' share of the escape's shutter flash.</summary>
    internal IEscapeShutterFlash EscapeShutterFlash { get; }

    /// <summary>What the encounters show over the room: written overlays, a flown turning floor and the beam cone.</summary>
    internal IEnemyDisplayContributions Displays { get; }

    /// <summary>
    /// The statue room's entrance statue and unlock effects as its statue sequence drives them; null in every other room
    /// (#2486).
    /// </summary>
    internal ITourianStatueActors? TourianStatue { get; }

    /// <summary>The ceiling tiles that fall in Ceres station during its escape.</summary>
    internal CeresFallingTiles CeresFallingTiles { get; }

    /// <summary>The electricity a save station shows while it saves.</summary>
    internal SaveStationElectricity SaveStationElectricity { get; }

    /// <summary>Mother Brain's rainbow beam, stepped after the enemy pass while her head is loaded.</summary>
    /// <param name="encounters">The room's boss encounters.</param>
    /// <param name="artwork">The bosses' installed artwork, whose rainbow colours the beam cycles.</param>
    private sealed class MotherBrainRainbowBeamStep(RoomBossEncounters encounters, BossArtworkBinding artwork) : IStepAfterEnemyPass
    {
        /// <inheritdoc/>
        public void StepAfterEnemyPass()
        {
            if (encounters.MotherBrain is { Head: { } head } brain)
                brain.StepRainbowBeam(head.Position.Whole, artwork.MotherBrainRainbowColors);
        }
    }

    /// <summary>Mother Brain's brain and neck, whose draw advances her brain's reads of the room's random numbers.</summary>
    /// <param name="motherBrain">Mother Brain.</param>
    /// <param name="encounters">The room's boss encounters.</param>
    private sealed class MotherBrainLateParts(MotherBrainBehavior motherBrain, RoomBossEncounters encounters) : IEnemyLateDraw
    {
        /// <inheritdoc/>
        public void DrawAfterEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus) =>
            motherBrain.Head.DrawMotherBrainBrainAndNeck(encounters.MotherBrain, sprites, camera, samus);
    }

    /// <summary>The dead Torizo's sand heap.</summary>
    /// <param name="deadTorizo">The dead Torizo.</param>
    private sealed class DeadTorizoSandHeap(DeadTorizoBehavior deadTorizo) : IEnemyLateDraw
    {
        /// <inheritdoc/>
        public void DrawAfterEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus) =>
            deadTorizo.DrawDeadTorizoSandHeap(sprites, camera);
    }

    /// <summary>
    /// Ceres Ridley's scene: the Baby Metroid's animation, which runs as the enemy pass ends until his animation starts and
    /// after the actors' drawing steps once it has (#2573), and the Baby and his room's door overlay, each shown once.
    /// </summary>
    /// <param name="ceres">The Ceres encounter.</param>
    private sealed class CeresBabyPassScene(ICeresEncounter ceres) : IEnemyPassScene
    {
        /// <inheritdoc/>
        public void StepWithEnemyPass() => ceres.StepCeresBabyWithEnemyPass();

        /// <inheritdoc/>
        public void StepAfterActors() => ceres.StepCeresBabyAfterActors();

        /// <inheritdoc/>
        public void Publish(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
        {
            ceres.PublishCeresBaby(sprites, camera);
            ceres.PublishRidleyRoomDoorOverlay(sprites, camera);
        }
    }

    /// <summary>The loaded bosses' displayed body wave, visibility and rainbow beam colour.</summary>
    /// <param name="encounters">The room's boss encounters.</param>
    private sealed class BossBodyDisplayLatch(RoomBossEncounters encounters) : IDisplayLatch
    {
        /// <inheritdoc/>
        public void LatchDisplay()
        {
            if (encounters.Phantoon is { } phantoon)
            {
                phantoon.BodyWave.LatchDisplay();
                phantoon.BodyVisibility.LatchDisplay(phantoon.Death.PixelationSize);
            }
            encounters.MotherBrain?.RainbowBeam.LatchDisplay();
        }
    }

    /// <summary>Phantoon's body wave and visibility, which step with the room's effects.</summary>
    /// <param name="encounters">The room's boss encounters.</param>
    private sealed class PhantoonBodyDisplaySteps(RoomBossEncounters encounters) : IStepWithRoomEffects
    {
        /// <inheritdoc/>
        public void StepWithRoomEffects()
        {
            if (encounters.Phantoon is not { } phantoon)
                return;
            phantoon.BodyWave.Step(phantoon);
            phantoon.BodyVisibility.Step(phantoon);
        }
    }

    /// <summary>The loader's operations over the Rinkas, the pre-Phantoon room and a present Ridley.</summary>
    /// <param name="rinka">The Rinkas, whose setup reads the camera as loading progresses.</param>
    /// <param name="prePhantoonRoom">The room before Phantoon's.</param>
    /// <param name="ridley">What the room sees of Ridley's encounters.</param>
    internal sealed class LoaderOperations(RinkaBehavior rinka, PrePhantoonRoomBehavior prePhantoonRoom, RidleyEncounter ridley) : IEnemyLoaderHooks
    {
        /// <summary>The room before Phantoon's.</summary>
        private PrePhantoonRoomBehavior PrePhantoonRoom { get; } = prePhantoonRoom;

        /// <inheritdoc/>
        public bool HasDeferredLoaderTimeCameraReads => rinka.HasDeferredLoaderTimeCameraReads;

        /// <inheritdoc/>
        public void CompleteLoaderTimeCameraReads(Func<int, bool> loaderInitializedPlace, Position16<RoomPixels> camera) =>
            rinka.CompleteLoaderTimeCameraReads(loaderInitializedPlace, camera);

        /// <inheritdoc/>
        public void SpawnPrePhantoonRoomProjectile() => PrePhantoonRoom.SpawnPrePhantoonRoomProjectile();

        /// <inheritdoc/>
        public bool BossHidesMinimap => ridley.RidleyPresent;

        /// <inheritdoc/>
        public int BossArenaMapRows => ridley.ExploredArenaMapRows;
    }

    /// <summary>The gunship's scenario and the Rinkas' camera, which the room's load hands the species first.</summary>
    /// <param name="gunship">Samus's gunship.</param>
    /// <param name="rinka">The Rinkas.</param>
    private sealed class RoomLoadPreparation(GunshipBehavior gunship, RinkaBehavior rinka) : IEnemyLoadPreparation
    {
        /// <inheritdoc/>
        public void PrepareRoomLoad(IRoomEnemyLoad request)
        {
            Ensure.NotNull(request);
            gunship.BeginRoomLoad(request.GunshipScenario);
            // Initialization AI reads the already-positioned destination camera before the first enemy frame captures its own.
            rinka.SetRinkaCamera(request.Camera);
        }
    }

    /// <summary>The attract demo's setup of Kraid's room.</summary>
    /// <param name="kraid">Kraid.</param>
    private sealed class AttractDemoOperations(KraidBehavior kraid) : IEnemyAttractDemoHooks
    {
        /// <inheritdoc/>
        public void StartKraidPhaseWait(int updates) => kraid.Parts.Encounter.BodyState.PhaseWait.LastFor(updates);
    }

    /// <summary>The enemy-side effects room objects and room effects trigger, carried out by the owners they belong to.</summary>
    internal sealed class ObjectSpawns(
        NoobTubeBehavior noobTube,
        EyeDoorBehavior eyeDoor,
        OldTourianEscapeShaftWallBehavior oldTourianWall,
        DraygonBehavior draygon,
        MotherBrainBehavior motherBrain,
        TorizoBehavior bombTorizo,
        EscapeExplosionsBehavior escapeExplosions) : IEnemyObjectSpawns
    {
        /// <summary>The eyes guarding some doors.</summary>
        private EyeDoorBehavior EyeDoor { get; } = eyeDoor;

        /// <summary>The wall of old Tourian's escape shaft.</summary>
        private OldTourianEscapeShaftWallBehavior OldTourianWall { get; } = oldTourianWall;

        /// <summary>The explosions that go off around Samus during the escape.</summary>
        private EscapeExplosionsBehavior EscapeExplosions { get; } = escapeExplosions;

        /// <inheritdoc/>
        public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) => noobTube.SpawnNoobTubeProjectile(request);

        /// <inheritdoc/>
        public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) => EyeDoor.SpawnEyeDoorProjectile(request);

        /// <inheritdoc/>
        public void SpawnEyeDoorAttack(EyeDoorAttackRequest request, SharedGameState system) => EyeDoor.SpawnEyeDoorAttack(request, system);

        /// <inheritdoc/>
        public void SpawnOldTourianEscapeShaftWallExplosion() => OldTourianWall.SpawnOldTourianEscapeShaftWallExplosion();

        /// <inheritdoc/>
        public void DisableDraygonCannon(DraygonWallCannon cannon) => draygon.DisableDraygonCannon(cannon);

        /// <inheritdoc/>
        public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request) =>
            motherBrain.Projectiles.SpawnMotherBrainGlassProjectile(request);

        /// <inheritdoc/>
        public void SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request) =>
            bombTorizo.Attacks.SpawnBombTorizoStatueBreakingProjectile(request);

        /// <inheritdoc/>
        public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, RoomEffectActorKind kindLeftOpen) =>
            EscapeExplosions.SpawnEscapeExplosion(position, random, kindLeftOpen);
    }

    /// <summary>The room-object work the Chozo statues and downward gates exchange with their room objects.</summary>
    internal sealed class ObjectRequests(ChozoStatueBehavior chozoStatue, DownwardGateBehavior downwardGate) : IEnemyObjectRequests
    {
        /// <summary>The gates that close downward over a passage.</summary>
        private DownwardGateBehavior DownwardGate { get; } = downwardGate;

        /// <inheritdoc/>
        public IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ActivateChozoStatueHandTrigger() =>
            chozoStatue.ActivateChozoStatueHandTrigger();

        /// <inheritdoc/>
        public void ApplyDownwardGateProjectileRequest(DownwardGateProjectileRequest request) =>
            DownwardGate.ApplyDownwardGateProjectileRequest(request);
    }

    /// <summary>The written overlays, the flown turning floor and the beam cone the encounters show.</summary>
    /// <param name="CeresEscape">The station's escape from Ceres Ridley's room.</param>
    /// <param name="motherBrain">Mother Brain's room encounter.</param>
    /// <param name="beam">The Morph Ball eye's beam.</param>
    private sealed class DisplayContributions(CeresEscape CeresEscape, MotherBrainEncounter motherBrain, MorphBallEyeBeamState beam)
        : IEnemyDisplayContributions
    {
        /// <inheritdoc/>
        public FloorView? FlownFloor => CeresEscape.ShownFloor;

        /// <inheritdoc/>
        public ImmutableArray<PerspectiveFloorScenery> FlownFloorWalls => CeresEscape.ChamberWalls();

        /// <inheritdoc/>
        public RoomWarningText ForegroundWarning => CeresEscape.Warning.Overlay;

        /// <inheritdoc/>
        public EscapeTypewriterState? ForegroundTypewriter => CeresEscape.Warning.Typewriter;

        /// <inheritdoc/>
        public RoomWarningText BackgroundWarning => motherBrain.ZebesWarning;

        /// <inheritdoc/>
        public MorphBallEyeBeamCone? LatchedBeamCone => beam.Latched;
    }
}
