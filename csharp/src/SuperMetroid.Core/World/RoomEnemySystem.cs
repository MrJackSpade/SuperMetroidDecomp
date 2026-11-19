using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.Doors;
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
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Grapple;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room-enemy loader, scheduler, shared animation, and draw queues. The game's
/// gameplay definitions are its own; installed artwork supplies pixels and colors.
/// </summary>
/// <remarks>
/// The room keeps a fixed set of enemy places in <see cref="EnemyPlaceOrder"/>, each with a stable ordinal. The ordinals
/// are gameplay: multi-part enemies find their parts by neighbouring ordinals, Samus records the ordinal of the enemy she
/// meets, and the gunship's lower part copies fields from the place before it. A compacted list would lose that.
/// </remarks>
[RoomLifetime]
public sealed class RoomEnemySystem : IEnemyFrameResults, IEnemyDisplaySources
{
    /// <summary>The enemies' draw phase: the live enemies the enemy pass queues to draw, and the frames each one shows.</summary>
    internal EnemyDrawList Drawing { get; }

    /// <summary>Advances each animating enemy's animation and shows the frames the bosses show on their bodies.</summary>
    internal EnemyAnimationPlayer AnimationPlayer { get; }

    /// <summary>Places the room's enemy population in the room's enemy places when the room loads, with its kill quota.</summary>
    internal EnemyPopulationLoader PopulationLoader { get; }

    /// <summary>Chooses the enemies the enemy pass runs this update: those near enough the screen, in their places' order.</summary>
    internal EnemyProcessingWindow ProcessingWindow { get; }

    /// <summary>Puts populations into places and runs their AI.</summary>
    internal EnemyLifecycle Lifecycle { get; }

    /// <summary>Samus's gunship: its landing, save prompt, boarding and exit, and its lift-offs after Ceres and the escape.</summary>
    internal GunshipBehavior GunshipBehavior { get; }

    /// <summary>Caps how fast the camera follows Samus while Draygon or a Yapping Maw carries her.</summary>
    internal SamusScrollCap ScrollCap { get; }

    /// <summary>The room's liquid, which Ridley and the Chozo statue raise or lower.</summary>
    internal EnemyRoomFx RoomFx { get; }

    /// <summary>What the room's enemies tell the rest of the game: the kills, the boss present, the Fireflea darkness and whether a door transition holds them.</summary>
    internal EnemyRoomStatus RoomStatus { get; }

    /// <summary>Ridley's fireballs and the afterburn that trails them, in Ceres and Norfair, which Mother Brain's afterburn reuses.</summary>
    internal RidleyProjectiles RidleyProjectiles { get; }

    /// <summary>What the room sees of Ridley's encounters in Ceres and Lower Norfair, and the Ceres escape's progress.</summary>
    internal RidleyEncounter RidleyEncounter { get; }
    /// <summary>Coordinates Ridley's multipart tail through the shared place and projectile services.</summary>
    internal RidleyTail RidleyTail { get; }
    /// <summary>The parts of Ridley's body both his encounters run (#2229).</summary>
    internal RidleyBody RidleyBody { get; }
    /// <summary>The station's escape from Ceres Ridley's room: his getaway and the self-destruct warning (#2228).</summary>
    internal CeresEscape CeresEscape { get; }
    /// <summary>Raises the dust of dead monsters' fallen corpse rows through the same frame as enemy processing.</summary>
    internal CorpseRowDust CorpseRowDust { get; }
    /// <summary>Owns Mother Brain's room encounter and its cutscene state.</summary>
    internal MotherBrainEncounter MotherBrainEncounter { get; }

    /// <summary>
    /// The encounters of the room's bosses the room itself reads (#2194), which each attaches as its body sets up: the
    /// room's main, display and colours read them here, never through a boss's species.
    /// </summary>
    internal RoomBossEncounters BossEncounters { get; } = new();

    /// <summary>Samus's touches, shots, bombs and power bombs against the room's enemies, and the enemies' reactions to them.</summary>
    internal EnemyCollision Collision { get; }

    /// <summary>Enemy deaths and their drops: the death explosions, the scattered pickups and Samus collecting them.</summary>
    internal EnemyDeathsAndDrops DeathsAndDrops { get; }

    /// <summary>What the current enemy pass reads: the random number, the frame counter and whether time is frozen.</summary>
    internal EnemyFrameContext FrameContext { get; }

    /// <summary>How the room's enemies look: the room's appearance, the boss body layer and the shading each draw shows.</summary>
    internal EnemyPresentation Presentation { get; }

    /// <summary>The room's enemy places, in their order, and how far the population and later spawns have filled them.</summary>
    internal EnemyPlaces Places { get; }

    /// <summary>The Zoas, which hide until Samus comes near, rise to her height and dart across the room.</summary>
    internal ZoaBehavior ZoaBehavior { get; }

    /// <summary>The Zebetites barring the way to Mother Brain, which Samus's missiles wear down.</summary>
    internal ZebetiteBehavior ZebetiteBehavior { get; }

    /// <summary>The room camera's distance mode, which Kraid and Crocomire request (#1874).</summary>
    internal RoomCameraDistance CameraDistance { get; } = new();

    /// <summary>The room camera's floor, which Spore Spawn's live fight holds (#2243).</summary>
    internal RoomCameraFloor CameraFloor { get; } = new();

    /// <summary>The Yards, Maridia's shelled crawlers that curl into a ball Samus can kick.</summary>
    internal YardEnemyBehavior YardBehavior { get; }

    /// <summary>The Yapping Maws, mouths on stalks that bite at Samus and carry her.</summary>
    internal YappingMawBehavior YappingMawBehavior { get; }

    /// <summary>The Wrecked Ship's ghosts, which flicker in and out.</summary>
    internal WreckedShipGhostBehavior WreckedShipGhostBehavior { get; }

    /// <summary>The Wrecked Ship's work robots, which patrol and fire lasers.</summary>
    internal WorkRobotBehavior WorkRobotBehavior { get; }

    /// <summary>The Wavers, flyers that weave up and down as they cross the room.</summary>
    internal WaverBehavior WaverBehavior { get; }

    /// <summary>The space pirates that cling to walls, climb and fire at Samus.</summary>
    internal WallSpacePirateBehavior WallSpacePirateBehavior { get; }

    /// <summary>The space pirates that walk the floors and fire at Samus.</summary>
    internal WalkingSpacePirateBehavior WalkingSpacePirateBehavior { get; }

    /// <summary>
    /// The Tourian entrance statue scene: its decorations, which follow the statue down, and its look as each boss statue
    /// releases. Only the statue room builds it, with that room (#2426); null in every other room, which places no statue.
    /// </summary>
    internal TourianEntranceStatueBehavior? TourianEntranceStatueBehavior { get; }

    /// <summary>
    /// The Tourian statues' unlock effects in the statue room: the eyes' glows, the released souls and particles, their
    /// splashes at its water and the descent's dust (#2382). Only the statue room builds them, with its statue scene; null in
    /// every other room (#2486).
    /// </summary>
    internal TourianUnlockBehavior? TourianUnlockEffects { get; }

    /// <summary>
    /// The statue room's entrance statue and unlock effects as its statue sequence drives them; null in every other room
    /// (#2486).
    /// </summary>
    internal ITourianStatueActors? TourianStatue { get; }

    /// <summary>Spore Spawn, Brinstar's mini-boss, which swings its core and releases spores.</summary>
    internal SporeSpawnBehavior SporeSpawnBehavior { get; }

    /// <summary>The Wrecked Ship's sparks, which flicker on and off and drop falling sparks.</summary>
    internal SparkBehavior SparkBehavior { get; }

    /// <summary>The Skulteras, skull-faced fish that swim back and forth in Maridia.</summary>
    internal SkulteraBehavior SkulteraBehavior { get; }

    /// <summary>The Skrees, which hang from ceilings, dive at Samus and burst on the ground.</summary>
    internal SkreeBehavior SkreeBehavior { get; }

    /// <summary>The shutters, the gates that rise, fall or grow out of the walls on their own.</summary>
    internal ShuttersBehavior ShuttersBehavior { get; }

    /// <summary>The Baby Metroid in Mother Brain's fight, which drains her and gives Samus its life.</summary>
    internal ShitroidBehavior ShitroidBehavior { get; }

    /// <summary>Shaktool, the Maridia digger that clears the sand from its tunnel.</summary>
    internal ShaktoolBehavior ShaktoolBehavior { get; }

    /// <summary>The Sbugs, small bugs that wake when Samus comes near and dart about her.</summary>
    internal SbugBehavior SbugBehavior { get; }

    /// <summary>The Rippers, armoured flyers that cross the room and turn at walls, the jet-powered ones among them.</summary>
    internal RipperBehavior RipperBehavior { get; }

    /// <summary>The Rios, Brinstar's flyers that swoop at Samus from the ceiling.</summary>
    internal RioBehavior RioBehavior { get; }

    /// <summary>The Rinkas, the rings that spawn in Tourian and fly at Samus.</summary>
    internal RinkaBehavior RinkaBehavior { get; }

    /// <summary>Ridley in Lower Norfair: his fight and his death.</summary>
    internal NorfairRidleyBehavior NorfairRidleyBehavior { get; }

    /// <summary>The Puyos, Norfair's small blobs that hop along floors and ceilings.</summary>
    internal PuyoBehavior PuyoBehavior { get; }

    /// <summary>The room before Phantoon's, whose setup holds one enemy-projectile place empty for its first updates.</summary>
    internal PrePhantoonRoomBehavior PrePhantoonRoomBehavior { get; }

    /// <summary>The Powamps, Maridia's balloon creatures that inflate and shoot spikes.</summary>
    internal PowampBehavior PowampBehavior { get; }

    /// <summary>The Polyps, which throw rocks out of Norfair's lava.</summary>
    internal PolypBehavior PolypBehavior { get; }

    /// <summary>The Trippers and suspensor platforms Samus rides.</summary>
    internal PlatformsBehavior PlatformsBehavior { get; }

    /// <summary>The pipe bugs, the Zebs, Zebbos, Gamets and Geegas that stream out of pipes at Samus.</summary>
    internal PipeBugsBehavior PipeBugsBehavior { get; }

    /// <summary>Phantoon, the Wrecked Ship's boss.</summary>
    internal PhantoonBehavior PhantoonBehavior { get; }

    /// <summary>The Owtches, spiky creatures that crawl over Maridia's sand.</summary>
    internal OwtchBehavior OwtchBehavior { get; }

    /// <summary>The wall of old Tourian's escape shaft, which the escape blows open.</summary>
    internal OldTourianEscapeShaftWallBehavior OldTourianEscapeShaftWallBehavior { get; }

    /// <summary>The Nuclear Waffles, Lower Norfair's chains of fire that sweep an arc.</summary>
    internal NuclearWaffleBehavior NuclearWaffleBehavior { get; }

    /// <summary>Norfair's Rios, which swoop at Samus from the ceiling.</summary>
    internal NorfairRioBehavior NorfairRioBehavior { get; }

    /// <summary>The enemies that leap out of Norfair's lava and fall back in.</summary>
    internal NorfairLavaJumpingEnemyBehavior NorfairLavaJumpingEnemyBehavior { get; }

    /// <summary>Maridia's glass tube, which a power bomb shatters.</summary>
    internal NoobTubeBehavior NoobTubeBehavior { get; }

    /// <summary>The ninja space pirates, which leap across their rooms and throw their claws.</summary>
    internal NinjaSpacePirateBehavior NinjaSpacePirateBehavior { get; }

    /// <summary>The Multiviolas, Norfair's fireballs that bounce around their rooms.</summary>
    internal MultiviolaBehavior MultiviolaBehavior { get; }

    /// <summary>Mother Brain, the final boss.</summary>
    internal MotherBrainBehavior MotherBrainBehavior { get; }

    /// <summary>The wall-mounted eyes that track Samus and shine their beam on her.</summary>
    internal MorphBallEyeBehavior MorphBallEyeBehavior { get; }

    /// <summary>The Mochtroids, Maridia's Metroid look-alikes.</summary>
    internal MochtroidBehavior MochtroidBehavior { get; }

    /// <summary>The Metroids of Tourian, which latch on to Samus and drain her energy.</summary>
    internal MetroidBehavior MetroidBehavior { get; }

    /// <summary>The Metarees, which drop from ceilings and drill into the floor.</summary>
    internal MetareeBehavior MetareeBehavior { get; }

    /// <summary>Maridia's large snails, which crawl over the room's surfaces.</summary>
    internal MaridiaLargeSnailBehavior MaridiaLargeSnailBehavior { get; }

    /// <summary>Maridia's mama turtle and her babies.</summary>
    internal MamaTurtleBehavior MamaTurtleBehavior { get; }

    /// <summary>The Magdollites, which rise out of Lower Norfair's lava and throw fire.</summary>
    internal MagdolliteBehavior MagdolliteBehavior { get; }

    /// <summary>Lower Norfair's Rios, which swoop at Samus from the ceiling.</summary>
    internal LowerNorfairRioBehavior LowerNorfairRioBehavior { get; }

    /// <summary>The Kzans, spiked crushers that drop and rise again, carrying Samus on top.</summary>
    internal KzanBehavior KzanBehavior { get; }

    /// <summary>Kraid, Brinstar's boss.</summary>
    internal KraidBehavior KraidBehavior { get; }

    /// <summary>The Ki-Hunters, winged insects that swoop at Samus, spit acid and fight on once their wings are shot off.</summary>
    internal KiHunterBehavior KiHunterBehavior { get; }

    /// <summary>The Kagos, hives that release swarms of bugs when shot.</summary>
    internal KagoBehavior KagoBehavior { get; }

    /// <summary>The Sidehoppers and Dessgeegas, which hop along floors, walls and ceilings.</summary>
    internal HoppersBehavior HoppersBehavior { get; }

    /// <summary>The Hibashis, Norfair's pillars of fire that erupt from the floor.</summary>
    internal HibashiBehavior HibashiBehavior { get; }

    /// <summary>The grapple beam's grip on enemies: which enemy its tip latches on to, and what a grappled enemy does.</summary>
    internal GrappleBehavior GrappleBehavior { get; }

    /// <summary>The Funes and Namihes, heads in Norfair's walls that spit fireballs.</summary>
    internal FuneNamiheBehavior FuneNamiheBehavior { get; }

    /// <summary>The Mellows, Mellas and Memus, swarms of flies that circle and dive at Samus.</summary>
    internal FliesBehavior FliesBehavior { get; }

    /// <summary>The Firefleas of Lower Norfair, whose deaths darken the room.</summary>
    internal FirefleaBehavior FirefleaBehavior { get; }

    /// <summary>The sparks that fall from the Wrecked Ship's sparks and skitter along the floor.</summary>
    internal FallingSparksBehavior FallingSparksBehavior { get; }

    /// <summary>Fake Kraid, the small Kraid in the room before Kraid's.</summary>
    internal FakeKraidBehavior FakeKraidBehavior { get; }

    /// <summary>The eyes guarding some doors, which fire at Samus until she destroys them.</summary>
    internal EyeDoorBehavior EyeDoorBehavior { get; }

    /// <summary>The Evirs, Maridia's shrimp that throw their arms at Samus.</summary>
    internal EvirBehavior EvirBehavior { get; }

    /// <summary>The Etecoons, the friendly animals that show Samus the wall jump.</summary>
    internal EtecoonBehavior EtecoonBehavior { get; }

    /// <summary>The explosions that go off around Samus during the escape.</summary>
    internal EscapeExplosionsBehavior EscapeExplosionsBehavior { get; }

    /// <summary>The Dachora and Etecoons escaping Zebes, and the wall Samus can free them through.</summary>
    internal EscapeAnimalsBehavior EscapeAnimalsBehavior { get; }

    /// <summary>The elevators that carry Samus between areas.</summary>
    internal ElevatorBehavior ElevatorBehavior { get; }

    /// <summary>Draygon, Maridia's boss, with the turrets on its room's walls.</summary>
    internal DraygonBehavior DraygonBehavior { get; }

    /// <summary>The Dragons, which rise out of Norfair's lava and spit fireballs.</summary>
    internal DragonBehavior DragonBehavior { get; }

    /// <summary>The gates that close downward over a passage and the gates already closed.</summary>
    internal DownwardGateBehavior DownwardGateBehavior { get; }

    /// <summary>The drained corpses lying in Tourian, which crumble away when disturbed.</summary>
    internal DeadTourianBehavior DeadTourianBehavior { get; }

    /// <summary>The drained Torizo lying in Tourian, which crumbles away when disturbed.</summary>
    internal DeadTorizoBehavior DeadTorizoBehavior { get; }

    /// <summary>The giant Sidehopper the Baby Metroid drains in Tourian, and the corpse it leaves.</summary>
    internal DeadSidehopperBehavior DeadSidehopperBehavior { get; }

    /// <summary>The Dachora, the friendly animal that shows Samus the shinespark.</summary>
    internal DachoraBehavior DachoraBehavior { get; }

    /// <summary>Crocomire, Norfair's boss.</summary>
    internal CrocomireBehavior CrocomireBehavior { get; }

    /// <summary>The Geemers, Zeelas and Sovas that crawl around the room's surfaces.</summary>
    internal CrawlersBehavior CrawlersBehavior { get; }

    /// <summary>The Chozo statues that come to life: the one that carries Samus and the one that drains the acid.</summary>
    internal ChozoStatueBehavior ChozoStatueBehavior { get; }

    /// <summary>The Choots, Maridia's flat fish that leap when Samus comes near and glide back down.</summary>
    internal ChootBehavior ChootBehavior { get; }

    /// <summary>The steam that vents from Ceres station's walls.</summary>
    internal CeresSteamBehavior CeresSteamBehavior { get; }

    /// <summary>The ceiling tiles that fall in Ceres station during its escape.</summary>
    internal CeresFallingTiles CeresFallingTiles { get; }

    /// <summary>The electricity a save station shows while it saves.</summary>
    internal SaveStationElectricity SaveStationElectricity { get; }

    /// <summary>Ridley in Ceres station: his fight over the Baby Metroid and his getaway.</summary>
    internal CeresRidleyBehavior CeresRidleyBehavior { get; }

    /// <summary>Ceres station's doors and walls, and the quake that shakes them during the escape.</summary>
    internal CeresDoorBehavior CeresDoorBehavior { get; }

    /// <summary>The Cacatacs, Maridia's cacti that shoot spikes.</summary>
    internal CacatacBehavior CacatacBehavior { get; }

    /// <summary>The Bulls, floating creatures that drift after Samus and recoil from shots.</summary>
    internal BullBehavior BullBehavior { get; }

    /// <summary>The Boyons, Norfair's jelly blobs that bounce in place.</summary>
    internal BoyonBehavior BoyonBehavior { get; }

    /// <summary>The boulders that roll and bounce down at Samus.</summary>
    internal BoulderBehavior BoulderBehavior { get; }

    /// <summary>Botwoon, Maridia's mini-boss, with its body and spit.</summary>
    internal BotwoonBehavior BotwoonBehavior { get; }

    /// <summary>The Torizos, Bomb Torizo and Golden Torizo.</summary>
    internal TorizoBehavior TorizoBehavior { get; }

    /// <summary>The face blocks in Blue Brinstar and their turning animation.</summary>
    internal BlueBrinstarFaceBlockBehavior BlueBrinstarFaceBlockBehavior { get; }

    /// <summary>The Beetoms, which leap at Samus, latch on and drain her energy.</summary>
    internal BeetomBehavior BeetomBehavior { get; }

    /// <summary>The Baby Metroid's meeting with Samus in Tourian, where it latches on and lets her go.</summary>
    internal BabyMetroidCutsceneBehavior BabyMetroidCutsceneBehavior { get; }

    /// <summary>The Atomics, the Wrecked Ship's spinning atoms that drift after Samus.</summary>
    internal AtomicBehavior AtomicBehavior { get; }

    /// <summary>The Alcoons, Norfair's lizards that leap out at Samus and spit fireballs.</summary>
    internal AlcoonBehavior AlcoonBehavior { get; }

    /// <summary>The room's earthquake, which every enemy, room object and effect that shakes the room starts or sustains.</summary>
    internal RoomEarthquake Earthquake { get; }

    /// <summary>The enemies' collision, for the shared responses to resolve at use.</summary>
    private EnemyCollision GetCollision() => Collision;

    /// <summary>The species registry that binds each installed occupant's entity.</summary>
    internal EnemySpeciesRegistry Species { get; }

    /// <summary>The loaded room's enemy definitions, services and artwork, shared with the composed owners.</summary>
    internal EnemyRoomBinding Room { get; }

    /// <summary>
    /// Rebinds the room's active escape typewriters to the host's current escape-warning text, after the host binds new
    /// presentation content.
    /// </summary>
    /// <param name="value">The installed escape-warning text.</param>
    internal void RebindEscapeTypewriters(EscapeTypewriterPresentation value)
    {
        Ensure.NotNull(value);
        EscapeTypewriterState? active = BossEncounters.MotherBrain?.EscapeTypewriter;
        if (active?.MessageId == EscapeTypewriterMessageId.Zebes)
            active.BindMessage(value.Get(EscapeTypewriterMessageId.Zebes));
        EscapeTypewriterState? ceres = CeresEscape.Warning.Typewriter;
        if (ceres?.MessageId == EscapeTypewriterMessageId.Ceres)
            ceres.BindMessage(value.Get(EscapeTypewriterMessageId.Ceres));
    }

    /// <summary>Species owners visited during room load and each enemy frame.</summary>
    private readonly IEnemySpecies[] _species;

    /// <summary>The room's effect actors.</summary>
    internal RoomEffectActors EffectActors { get; }

    /// <summary>
    /// Whether a Puyo striking a ceiling turns back: told of the room's draws, the charge flare's and the HUD's redraws,
    /// and read by the Puyos.
    /// </summary>
    internal PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; }

    /// <summary>The room's enemy projectiles: their eighteen places, their spawns, their motion and their touches on Samus.</summary>
    internal EnemyProjectileEngine Projectiles { get; }

    /// <summary>The sound and music requests the room's enemies queue, and the music their room starts when it loads.</summary>
    internal EnemyAudioQueue Audio { get; }

    /// <summary>How enemies move against the room's terrain: steps over flat ground and slopes, the solid ahead, and the blocks they break.</summary>
    internal EnemyTerrain Terrain { get; }

    /// <summary>Post-AI collision snapshots for the room's active enemies.</summary>
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();

    /// <summary>The elevator journey the runtime reads and resets.</summary>
    internal IElevatorJourney Elevator => ElevatorBehavior;

    /// <summary>The gunship's save prompt and health check.</summary>
    internal IGunshipSession Gunship => field ??= new GunshipSession(GunshipBehavior);

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    internal ICeresEncounter Ceres => field ??= new CeresEncounter(RidleyEncounter, CeresRidleyBehavior, CeresEscape);

    /// <summary>The state and hooks the room main behaviours and scrolling use.</summary>
    internal IEnemyRoomMainHooks RoomMainHooks => field ??= new EnemyRoomMainHooks(BossEncounters, CameraDistance, CameraFloor);

    /// <summary>The operations the room loader runs.</summary>
    internal IEnemyLoaderHooks LoaderHooks => field ??= new EnemyLoaderHooks(RinkaBehavior, PrePhantoonRoomBehavior);

    /// <summary>The enemy setup the attract demo's room setups perform.</summary>
    internal IEnemyAttractDemoHooks AttractDemoHooks => field ??= new EnemyAttractDemoHooks(KraidBehavior);

    /// <summary>The enemy-side effects room objects and room effects trigger.</summary>
    internal IEnemyObjectSpawns RoomObjectSpawns => field ??= new EnemyObjectSpawns(NoobTubeBehavior, EyeDoorBehavior, OldTourianEscapeShaftWallBehavior,
        DraygonBehavior, MotherBrainBehavior, TorizoBehavior, EscapeExplosionsBehavior);

    /// <summary>The room-object work the Chozo statues and downward gates exchange with their room objects each frame.</summary>
    internal IEnemyObjectRequests RoomObjectRequests => field ??= new EnemyObjectRequests(ChozoStatueBehavior, DownwardGateBehavior);

    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's shutter flash on the destroyable shutters the room holds, and on
    /// nothing once both are destroyed (#1512, #2288). Mother Brain's exploded door's fragments are pre-coloured in their
    /// own colours and do not show it (#1544).
    /// </summary>
    internal void ShowEscapeShutterFlash(ColourEffectFrame frame) => ShuttersBehavior.ShowEscapeShutterFlash(frame);

    /// <summary>The live enemy state the display latch and drawing read.</summary>
    internal IEnemyDisplaySources DisplaySources => this;

    /// <inheritdoc/>
    MotherBrainEnemyState? IEnemyDisplaySources.MotherBrain => BossEncounters.MotherBrain;
    /// <inheritdoc/>
    PhantoonEnemyState? IEnemyDisplaySources.Phantoon => BossEncounters.Phantoon;
    /// <inheritdoc/>
    CrocomireMeltDistortion? IEnemyDisplaySources.CrocomireMelt =>
        BossEncounters.Crocomire?.Death is { MeltingScrollActive: true } melting ? melting.MeltDistortion : null;
    /// <inheritdoc/>
    FloorView? IEnemyDisplaySources.CeresGetawayFloor => CeresEscape.ShownFloor;

    /// <inheritdoc/>
    RoomWrittenOverlay IEnemyDisplaySources.CeresWarning => CeresEscape.Warning.Overlay;

    /// <inheritdoc/>
    RoomWrittenOverlay IEnemyDisplaySources.ZebesWarning => MotherBrainEncounter.ZebesWarning;
    /// <inheritdoc/>
    ImmutableArray<PerspectiveFloorScenery> IEnemyDisplaySources.CeresGetawayChamberWalls => CeresEscape.ChamberWalls();
    /// <inheritdoc/>
    IBossBodyDisplay? IEnemyDisplaySources.BossBody => Presentation.BossBodyDisplay;
    /// <inheritdoc/>
    MorphBallEyeBeamState IEnemyDisplaySources.MorphBallEyeBeam => MorphBallEyeBehavior.MorphBallEyeBeam;

    /// <inheritdoc />
    IEnumerable<IGameplayShadingSource> IEnemyDisplaySources.ShadingSources
    {
        get
        {
            yield return CeresDoorBehavior;
            if (TourianEntranceStatueBehavior is { } statue)
                yield return statue;
            if (BossEncounters.MotherBrain is { } motherBrain)
                yield return motherBrain.Colours;
            foreach (IGameplayShadingSource boss in Presentation.BossShadings.InOrder)
                yield return boss;
        }
    }

    /// <summary>Grapple endpoint resolution against the room's enemies.</summary>
    internal IGrappleTargets GrappleTargets => GrappleBehavior;

    /// <summary>The events the enemies have published since the current pass or the room's load began, in publication order.</summary>
    internal EnemyPassEvents PassEvents { get; } = new();

    /// <inheritdoc/>
    public EnemyFrameResult LastEnemyFrame => new(PassEvents.Published);

    /// <summary>
    /// Creates a room's enemy system: every enemy place, each with its stable ordinal, without loading a population or binding artwork, random-number, or gameplay services.
    /// </summary>
    /// <param name="dependencies">
    /// The enemy session, the room's compatibility services, enemy definitions, game-state services, area, boss artwork
    /// and breakable terrain, and Samus's passage through a door.
    /// </param>
    internal RoomEnemySystem(RoomEnemySystemDependencies dependencies)
    {
        Ensure.NotNull(dependencies);
        EnemySession session = dependencies.Session;
        RoomEnemyCompatibility compatibility = Ensure.NotNull(dependencies.Compatibility);
        DoorPassage doorPassage = Ensure.NotNull(dependencies.DoorPassage);
        EnemyBreakableTerrainObjects breakableTerrain = dependencies.BreakableTerrain;
        BossArtworkBinding = Ensure.NotNull(dependencies.BossArtwork);
        Room = new EnemyRoomBinding(session, dependencies.Definitions, dependencies.Services, dependencies.Area);
        // Shared collaborators first: every species owner receives the ones it uses.
        ScrollCap = new SamusScrollCap();
        RoomFx = new EnemyRoomFx(dependencies.RoomFx);
        RoomStatus = new EnemyRoomStatus(session);
        FrameContext = new EnemyFrameContext(session);
        Presentation = new EnemyPresentation(session, Ensure.NotNull(dependencies.Looks), compatibility.FadeStepCount);
        BossPresentation = new BossPresentation(Presentation, BossArtworkBinding);
        Places = new EnemyPlaces();
        Audio = new EnemyAudioQueue(session.AudioRequests);
        MorphBallEyeBehavior = new MorphBallEyeBehavior(Audio);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, new MorphBallEyeBeamStep(MorphBallEyeBehavior), Places);
        Lifecycle = new EnemyLifecycle(Room, Places, ProcessingWindow);
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, Places, Lifecycle);
        RidleyEncounter = new RidleyEncounter(session);
        Earthquake = Ensure.NotNull(dependencies.Earthquake);
        PuyoCeilingTurn = compatibility.PuyoCeilingTurn;
        EffectActors = new RoomEffectActors(Presentation, compatibility.EffectActorPlaces, PuyoCeilingTurn);
        Projectiles = new EnemyProjectileEngine(Room, Presentation, EffectActors, Earthquake, Audio);
        GunshipBehavior = new GunshipBehavior(Audio, Projectiles, Room, Places, PassEvents, Presentation);
        Terrain = new EnemyTerrain(breakableTerrain);
        CorpseRowDust = new CorpseRowDust(Audio);
        MotherBrainEncounter = new MotherBrainEncounter(Audio, BossEncounters);
        RidleyTail = new RidleyTail(Projectiles, Room);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, Lifecycle, compatibility.PowerBombRadius, Audio);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Lifecycle = Lifecycle, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, Projectiles = Projectiles, EffectActors = EffectActors, Places = Places, PowerBombRadius = compatibility.PowerBombRadius });
        RidleyProjectiles = new RidleyProjectiles(Room, Projectiles);
        RidleyBody = new RidleyBody(new RidleyFlight(Earthquake), new RidleyReveal(doorPassage, Audio), new RidleyBodyAnimation(Audio, RidleyProjectiles),
            new RidleyBodyContact(Collision), new RidleyDrawing(Presentation), RidleyTail);
        CeresEscape = new CeresEscape(new CeresEscapeDependencies { PassEvents = PassEvents, Room = Room, Audio = Audio, Presentation = Presentation, Status = RidleyEncounter, RoomStatus = RoomStatus, BossArtwork = BossArtworkBinding, UnauthoredBoostCadence = compatibility.UnauthoredBoostCadence });
        ZoaBehavior = new ZoaBehavior();
        ElevatorBehavior = new ElevatorBehavior(Room, Audio, PassEvents, doorPassage);
        ZebetiteBehavior = new ZebetiteBehavior(new ZebetiteBehaviorDependencies { Audio = Audio, Lifecycle = Lifecycle, Room = Room, Earthquake = Earthquake, Places = Places, DoorPassage = doorPassage, RoomStatus = RoomStatus, Collision = Collision, FadeStepCount = compatibility.FadeStepCount, DeathsAndDrops = DeathsAndDrops });
        YardBehavior = new YardEnemyBehavior(Room, Terrain, Earthquake, Collision, Audio, compatibility.YardKickDistance);
        YappingMawBehavior = new YappingMawBehavior(Projectiles, EffectActors, ScrollCap, Audio);
        WreckedShipGhostBehavior = new WreckedShipGhostBehavior();
        WorkRobotBehavior = new WorkRobotBehavior(Room, Terrain, Projectiles, Audio);
        WaverBehavior = new WaverBehavior(Terrain);
        WalkingSpacePirateBehavior = new WalkingSpacePirateBehavior(Terrain, Projectiles, Audio, Collision, DeathsAndDrops);
        WallSpacePirateBehavior = new WallSpacePirateBehavior(Room, Terrain, WalkingSpacePirateBehavior);
        // The statue room alone builds the statue scene and its unlock effects, with its room (#2426, #2486).
        if (dependencies.Scenery.TourianStatueRoom is { } statueRoom)
        {
            var statueScene = new TourianEntranceStatueBehavior(Projectiles, statueRoom);
            var unlockEffects = new TourianUnlockBehavior(Room, Projectiles, Earthquake, Audio, statueScene, statueRoom);
            TourianEntranceStatueBehavior = statueScene;
            TourianUnlockEffects = unlockEffects;
            TourianStatue = new TourianStatueActors(statueScene, unlockEffects);
        }
        SporeSpawnBehavior = new SporeSpawnBehavior(new SporeSpawnBehaviorDependencies { Audio = Audio, Room = Room, Presentation = Presentation, Projectiles = Projectiles, EffectActors = EffectActors, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, PassEvents = PassEvents, CameraFloor = CameraFloor });
        FallingSparksBehavior = new FallingSparksBehavior(Room, Projectiles, EffectActors, compatibility.FallingSparkFloorProbe);
        SparkBehavior = new SparkBehavior(FallingSparksBehavior);
        SkulteraBehavior = new SkulteraBehavior(Terrain);
        SkreeBehavior = new SkreeBehavior(Audio, Projectiles, DeathsAndDrops);
        ShuttersBehavior = new ShuttersBehavior(Audio, Places);
        ShitroidBehavior = new ShitroidBehavior(Room, Places, compatibility.FadeStepCount, PassEvents, Audio);
        DeadSidehopperBehavior = new DeadSidehopperBehavior(new DeadSidehopperBehaviorDependencies { Room = Room, Terrain = Terrain, Shitroid = new ShitroidFeeding(Places), FrameContext = FrameContext, Projectiles = Projectiles, Collision = Collision, RowDust = CorpseRowDust });
        ShaktoolBehavior = new ShaktoolBehavior(Terrain, Places, Collision);
        SbugBehavior = new SbugBehavior();
        RipperBehavior = new RipperBehavior(Terrain);
        RioBehavior = new RioBehavior(Room, Terrain, Audio);
        RinkaBehavior = new RinkaBehavior(Collision, FrameContext, Projectiles, DeathsAndDrops);
        CeresRidleyBehavior = new CeresRidleyBehavior(new CeresRidleyBehaviorDependencies { Lifecycle = Lifecycle, Room = Room, Earthquake = Earthquake, Audio = Audio, Places = Places, Presentation = Presentation, RidleyEncounter = RidleyEncounter, RidleyBody = RidleyBody, Escape = CeresEscape });
        NorfairRidleyBehavior = new NorfairRidleyBehavior(new NorfairRidleyBehaviorDependencies { Lifecycle = Lifecycle, Room = Room, Earthquake = Earthquake, Audio = Audio, DeathsAndDrops = DeathsAndDrops, RidleyEncounter = RidleyEncounter, RidleyBody = RidleyBody, RidleyProjectiles = RidleyProjectiles, RoomFx = RoomFx, Presentation = Presentation });
        PuyoBehavior = new PuyoBehavior(Room, Terrain, PuyoCeilingTurn);
        PrePhantoonRoomBehavior = new PrePhantoonRoomBehavior(Projectiles);
        PowampBehavior = new PowampBehavior(Terrain, Projectiles, Places, Collision, DeathsAndDrops);
        PolypBehavior = new PolypBehavior(Room, Projectiles);
        PlatformsBehavior = new PlatformsBehavior(Terrain);
        PipeBugsBehavior = new PipeBugsBehavior(Places);
        PhantoonBehavior = new PhantoonBehavior(new PhantoonBehaviorDependencies { Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops , BossArtwork = BossArtworkBinding, PassEvents = PassEvents, Encounters = BossEncounters, Places = Places });
        OwtchBehavior = new OwtchBehavior(Room);
        OldTourianEscapeShaftWallBehavior = new OldTourianEscapeShaftWallBehavior(Projectiles, Audio);
        NuclearWaffleBehavior = new NuclearWaffleBehavior(Projectiles, EffectActors, Audio);
        NorfairRioBehavior = new NorfairRioBehavior(Room, Terrain, Places, Audio);
        NorfairLavaJumpingEnemyBehavior = new NorfairLavaJumpingEnemyBehavior(Room, Places, Audio);
        NoobTubeBehavior = new NoobTubeBehavior(Room, Projectiles);
        NinjaSpacePirateBehavior = new NinjaSpacePirateBehavior(Room, Terrain, Projectiles, EffectActors, WalkingSpacePirateBehavior);
        MultiviolaBehavior = new MultiviolaBehavior(Terrain);
        BabyMetroidCutsceneBehavior = new BabyMetroidCutsceneBehavior(new BabyMetroidCutsceneBehaviorDependencies { Lifecycle = Lifecycle, Room = Room, Audio = Audio, Places = Places, FrameContext = FrameContext, Projectiles = Projectiles, MotherBrainEncounter = MotherBrainEncounter });
        MotherBrainBehavior = new MotherBrainBehavior(new MotherBrainBehaviorDependencies { PassEvents = PassEvents, Lifecycle = Lifecycle, Places = Places, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, MotherBrainEncounter = MotherBrainEncounter, RoomActors = new MotherBrainRoomActors(BabyMetroidCutsceneBehavior, RinkaBehavior, WalkingSpacePirateBehavior, RidleyProjectiles)  });
        MochtroidBehavior = new MochtroidBehavior(Terrain, Audio, FrameContext, Collision);
        MetroidBehavior = new MetroidBehavior(Room, Terrain, Audio, EffectActors, FrameContext, DeathsAndDrops);
        MetareeBehavior = new MetareeBehavior(Projectiles, Audio);
        MaridiaLargeSnailBehavior = new MaridiaLargeSnailBehavior(Terrain, Collision, Audio);
        MamaTurtleBehavior = new MamaTurtleBehavior(Terrain, Earthquake, Places, Collision, Audio);
        MagdolliteBehavior = new MagdolliteBehavior(Room, Projectiles, Places, Collision, DeathsAndDrops, Audio);
        LowerNorfairRioBehavior = new LowerNorfairRioBehavior(Room, Terrain, Places, Audio);
        KzanBehavior = new KzanBehavior(Places, Audio);
        KraidBehavior = new KraidBehavior(new KraidBehaviorDependencies { PassEvents = PassEvents, Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, EffectActors = EffectActors, Presentation = BossPresentation, Collision = Collision, DeathsAndDrops = DeathsAndDrops, CameraDistance = CameraDistance  });
        KiHunterBehavior = new KiHunterBehavior(Room, Terrain, Projectiles, Audio, Places);
        KagoBehavior = new KagoBehavior(Room, Projectiles, Earthquake, DeathsAndDrops, RoomStatus, Audio);
        HoppersBehavior = new HoppersBehavior(Room, Terrain, Audio);
        HibashiBehavior = new HibashiBehavior(Places, Audio);
        GrappleBehavior = new GrappleBehavior(Lifecycle, DeathsAndDrops);
        FuneNamiheBehavior = new FuneNamiheBehavior(Projectiles, Audio);
        FliesBehavior = new FliesBehavior(Room);
        FirefleaBehavior = new FirefleaBehavior(Collision, DeathsAndDrops, RoomStatus);
        FakeKraidBehavior = new FakeKraidBehavior(Room, Terrain, Projectiles, Collision, DeathsAndDrops, Audio);
        EyeDoorBehavior = new EyeDoorBehavior(Room, Projectiles);
        EvirBehavior = new EvirBehavior(Places, Collision, Audio);
        EtecoonBehavior = new EtecoonBehavior(Terrain, Earthquake, doorPassage, Audio);
        EscapeExplosionsBehavior = new EscapeExplosionsBehavior(Audio, EffectActors);
        EscapeAnimalsBehavior = new EscapeAnimalsBehavior(Room, Terrain);
        DraygonBehavior = new DraygonBehavior(new DraygonBehaviorDependencies { PassEvents = PassEvents, Places = Places, Room = Room, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, EffectActors = EffectActors, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, ScrollCap = ScrollCap , BossArtwork = BossArtworkBinding });
        DragonBehavior = new DragonBehavior(Projectiles, Places, Collision, Audio);
        DownwardGateBehavior = new DownwardGateBehavior(Projectiles);
        DeadTourianBehavior = new DeadTourianBehavior(Room, FrameContext, Projectiles, CorpseRowDust);
        DeadTorizoBehavior = new DeadTorizoBehavior(Room, FrameContext, Projectiles, Presentation, Audio);
        DachoraBehavior = new DachoraBehavior(Terrain, Places, Audio);
        var crocomireAccess = new CrocomireEncounterAccess(PassEvents, Room, Projectiles, Audio);
        CrocomireBehavior = new CrocomireBehavior(new CrocomireBehaviorDependencies { Access = crocomireAccess, Places = Places, BodyLayer = new CrocomireBodyLayer(EffectActors, BossPresentation.Surfaces, crocomireAccess, RoomFx), Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Presentation = BossPresentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, CameraDistance = CameraDistance, RoomStatus = RoomStatus, Encounters = BossEncounters });
        CrawlersBehavior = new CrawlersBehavior(Terrain, Earthquake);
        ChozoStatueBehavior = new ChozoStatueBehavior(Room, Terrain, Projectiles, Audio, RoomFx, PassEvents);
        ChootBehavior = new ChootBehavior();
        CeresSteamBehavior = new CeresSteamBehavior(Room, Collision);
        // The turning elevator shaft alone places vents that ride its turn (#2480).
        var steamVents = new CeresSteamFactory(CeresSteamBehavior, dependencies.Scenery.ShaftTurn is { } shaft
            ? new CeresShaftSteamVentMotions(shaft)
            : CeresSteamVentsHoldingPlace.Instance);
        CeresFallingTiles = new CeresFallingTiles(Projectiles, Audio);
        SaveStationElectricity = new SaveStationElectricity(Projectiles);
        CeresDoorBehavior = new CeresDoorBehavior(new CeresDoorBehaviorDependencies { Room = Room, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, Projectiles = Projectiles, CeresEscape = RidleyEncounter, RidleyScene = RidleyEncounter });
        // A room with a turning floor alone places the elevator room's blocked door, which cycles the floor's platform (#2426).
        CeresElevatorDoorFactory? elevatorDoors = dependencies.Scenery.Floor is { } floor ? new CeresElevatorDoorFactory(CeresDoorBehavior, floor) : null;
        CacatacBehavior = new CacatacBehavior(Room, Projectiles, Audio);
        BullBehavior = new BullBehavior(compatibility.BullInheritance);
        BoyonBehavior = new BoyonBehavior(Audio);
        BoulderBehavior = new BoulderBehavior(Terrain, Projectiles, Audio);
        BotwoonBehavior = new BotwoonBehavior(new BotwoonBehaviorDependencies { Audio = Audio, Room = Room, Projectiles = Projectiles, EffectActors = EffectActors, Collision = Collision, DeathsAndDrops = DeathsAndDrops, PassEvents = PassEvents });
        AnimationPlayer = new EnemyAnimationPlayer(Presentation);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { BossEncounters = BossEncounters, DeadTorizoBehavior = DeadTorizoBehavior, FrameContext = FrameContext, AnimationPlayer = AnimationPlayer, MotherBrainBehavior = MotherBrainBehavior, Presentation = Presentation, Room = Room, PuyoCeilingTurn = PuyoCeilingTurn });
        TorizoBehavior = new TorizoBehavior(new TorizoBehaviorDependencies { Room = Room, Terrain = Terrain, Projectiles = Projectiles, Earthquake = Earthquake, Audio = Audio, Presentation = Presentation, FrameContext = FrameContext, Collision = Collision, DeathsAndDrops = DeathsAndDrops, PassEvents = PassEvents });
        BlueBrinstarFaceBlockBehavior = new BlueBrinstarFaceBlockBehavior(doorPassage);
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
            TorizoBehavior,
            .. Present<IEnemySpecies>(TourianEntranceStatueBehavior),
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
            NorfairRidleyBehavior,
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
        Species = EnemySpeciesRegistry.ForRoom(new EnemyCommonParticipation(GetCollision), [BoyonBehavior, BullBehavior, ZoaBehavior, WaverBehavior, ChootBehavior, AtomicBehavior, SkulteraBehavior, PuyoBehavior, RioBehavior, FliesBehavior, SparkBehavior, SbugBehavior, MultiviolaBehavior, CacatacBehavior, AlcoonBehavior, BoulderBehavior, NuclearWaffleBehavior, HibashiBehavior, WreckedShipGhostBehavior, KzanBehavior, RipperBehavior, MochtroidBehavior, SkreeBehavior, OwtchBehavior, BeetomBehavior, KagoBehavior, FirefleaBehavior, MetareeBehavior, PolypBehavior, HoppersBehavior, CrawlersBehavior, PlatformsBehavior, YardBehavior, YappingMawBehavior, EtecoonBehavior, DachoraBehavior, MaridiaLargeSnailBehavior, WorkRobotBehavior, BlueBrinstarFaceBlockBehavior, DragonBehavior, FuneNamiheBehavior, PowampBehavior, NorfairRioBehavior, LowerNorfairRioBehavior, NorfairLavaJumpingEnemyBehavior, ShaktoolBehavior, MetroidBehavior, WalkingSpacePirateBehavior, WallSpacePirateBehavior, NinjaSpacePirateBehavior, ChozoStatueBehavior, DeadSidehopperBehavior, DeadTorizoBehavior, DeadTourianBehavior, ShitroidBehavior, FakeKraidBehavior, ElevatorBehavior, MagdolliteBehavior, EscapeAnimalsBehavior, RinkaBehavior, ZebetiteBehavior, ShuttersBehavior, EvirBehavior, KiHunterBehavior, PipeBugsBehavior, MamaTurtleBehavior, MorphBallEyeBehavior, new CeresDoorFactory(CeresDoorBehavior), steamVents, GunshipBehavior, BotwoonBehavior, SporeSpawnBehavior, TorizoBehavior, KraidBehavior, CrocomireBehavior, DraygonBehavior, PhantoonBehavior, MotherBrainBehavior, BabyMetroidCutsceneBehavior, NoobTubeBehavior,
            new RidleyEntityFactory(NorfairRidleyBehavior, CeresRidleyBehavior, RidleyEncounter),
            .. Present<IEnemyEntityFactory>(TourianEntranceStatueBehavior), .. Present<IEnemyEntityFactory>(elevatorDoors)],
            // What the owners the room's scenery did not let it build would have placed; the registry refuses a room that
            // builds an owner of one, or lacks an owner of any other.
            [
                .. TourianEntranceStatueBehavior is null ? [EnemyDefinitionId.TourianStatue] : Array.Empty<EnemyDefinitionId>(),
                .. elevatorDoors is null ? [EnemyDefinitionId.CeresElevatorDoor] : Array.Empty<EnemyDefinitionId>(),
            ]);
        Places.InstallPlaces(Species);
    }

    /// <summary><paramref name="owner"/>, an owner the room's scenery may not let it build, when the room built it; else none.</summary>
    private static IEnumerable<T> Present<T>(T? owner) where T : class => owner is null ? [] : [owner];

    /// <summary>
    /// Starts each enemy projectile place from the whole position and velocity the room this one replaces left
    /// (<see cref="RoomHandOver"/>), before the population loads.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver) => Projectiles.TakeOverFrom(handOver);

    /// <summary>Leaves each enemy projectile place's whole position and velocity for the room that replaces this one (<see cref="RoomHandOver"/>).</summary>
    internal void HandOverTo(RoomHandOver handOver) => Projectiles.HandOverTo(handOver);

    /// <summary>Collision values read from the current places after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>What the room's bosses draw with: its presentation surfaces and the installed boss artwork.</summary>
    internal BossPresentation BossPresentation { get; }

    /// <summary>The bosses' installed artwork, read through the session's binding.</summary>
    internal BossArtworkBinding BossArtworkBinding { get; }
    /// <summary>Whether <see cref="Load"/> has loaded this room's population; a room loads it once.</summary>
    private bool _populationLoaded;

    /// <summary>
    /// Loads the room's enemies: the artwork their population draws from, their population and each one's initialization.
    /// </summary>
    public void Load(IRoomEnemyLoad request)
    {
        Ensure.NotNull(request);
        if (_populationLoaded)
            throw new InvalidOperationException("A room's enemy system loads its population once; each room builds its own.");
        _populationLoaded = true;
        IRoomEnemyDefinitionSource definitions = Room.Definitions;
        GunshipBehavior.BeginRoomLoad(request.GunshipScenario);
        FrameContext.BeginRoomLoad(request.DeferLoaderTimeCameraReads);
        Presentation.BindBackgrounds(request.Appearance);
        PassEvents.Begin();
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
            new EnemyPopulationSurroundings(request.Level, request.Samus, request.ControllerInput, request.Camera));
    }

    /// <summary>
    /// Runs one enemy pass over the processing list the frame already built: executes selected AI and animations,
    /// publishes live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    internal EnemyFrameResult StepFrame(RoomEnemyFrame frame)
    {
        ScrollCap.BeginFrame(frame.SamusPreviousPositionCheckpoint);
        FrameContext.BeginFrame(frame.TimeIsFrozen, frame.FrameCounter);
        var aiFrame = new ClockedEnemyAiFrame(frame, new FrameCounters(frame.ShortCounter));
        PassEvents.Begin();
        foreach (IEnemySpecies species in _species)
            species.BeginEnemyFrame(frame);
        Drawing.Clear();
        ShakeWallJumpLaunchEnemy(frame.Samus);

        foreach (RoomEnemyPlace place in ProcessingWindow.ActiveEnemies)
        {
            // The enemy pass counts tangible actors' invincibility
            // before checking time freeze. X-ray therefore stops their movement but
            // not this wait. Remember whether it ran at entry: the update that ends it
            // still skips collision, and a new AI-owned wait must not lose an update.
            bool invincibleAtEntry = place.Invincibility.IsRunning;
            if (!place.Contact.HasAny(EnemyContact.IgnoresSamus) && invincibleAtEntry)
                place.Invincibility.Count();
            // The enemy pass checks contact against the pre-movement actor, not the position
            // just moved into by AI. A rising support temporarily overlaps its rider until
            // Samus's movement consumes the carry; a late touch pass would retrigger that support.
            if (!frame.TimeIsFrozen && !invincibleAtEntry)
            {
                // Extended collision runs projectiles before bombs/touch and
                // before selecting hurt/main AI. In particular, a retained Plasma
                // hit must publish Phantoon's hurt clock before this frame ticks it.
                if (place.Entity is IEnemyContactHooks hooks)
                    hooks.ResolveBeforeOrdinaryHits(frame);
                Collision.ResolveOrdinaryProjectileHit(frame.SamusProjectiles, frame.SharedProjectiles, Room.Area,
                    frame.Samus, place, frame.Camera);
                // Samus's input phase has already moved her bombs. The enemy pass checks
                // them before this enemy's touch and AI, so an escape a bomb starts begins this update.
                Collision.ResolveOrdinaryBombHit(frame.SharedProjectiles, frame.SamusProjectiles, frame.Samus, place, frame.Camera);
                if (place.IsFree)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.PseudoScrew, frame.ControllerInput, frame.Level, place, frame.Camera);
                if (place.Entity is IEnemyAfterSamusContact contact)
                    contact.ResolveAfterSamusContact(frame.Samus, frame.PseudoScrew);
                if (place.IsFree)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(place, aiFrame);
                // The hurt behaviour replaces main AI while the enemy is hurt: the species' own, or the inert one its
                // definition names. The grappled, hurt and frozen states are tried in that order, and the first one the
                // enemy is in selects its behaviour even when that behaviour does nothing. Hurt therefore delays frozen
                // AI until the flash's end clears the hurt state; falling through here prematurely clears flash.
                if (!ranActorAi &&
                    (place.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None &&
                    place.Entity is IEnemyHurtReaction hurt &&
                    hurt.RunHurt(frame))
                {
                    ranActorAi = true;
                }
                // The enemy pass selects the AI from the first state the enemy is in alone. A frozen
                // clock without the frozen state (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (place.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None)
                {
                    // A Rinka's termination clears or replaces the enemy in this place.
                    // The pass's list of active enemies was fixed before AI, so skip the rest
                    // of this departed enemy's turn exactly as its own death does.
                    var frozen = place.Entity as IEnemyFrozenAi;
                    if (frozen?.PreemptFrozenTurn() == true)
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // AI for this update. Removing the ice instead thaws the enemy
                    // immediately, leaving the freeze clock at the remainder
                    // ThawedFreezeLeftover names rather than zero, which matters when an enemy
                    // is hurt and frozen at once.
                    place.HurtFlash.End();
                    bool thaw = place.Freeze.IsOver ||
                        (frame.Samus.EquippedBeams & SamusBeamFlags.Ice) == SamusBeamFlags.None;
                    if (!thaw)
                        place.Freeze.Count();
                    else
                    {
                        place.AiStates = place.AiStates & ~EnemyAiStates.Frozen;
                        bool stillGrappled = (place.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
                        bool stillHurt = (place.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None;
                        ThawedFreezeLeftover.Hold hold = stillGrappled
                            ? stillHurt ? ThawedFreezeLeftover.Hold.GrappledAndHurt : ThawedFreezeLeftover.Hold.Grappled
                            : stillHurt ? ThawedFreezeLeftover.Hold.Hurt : ThawedFreezeLeftover.Hold.None;
                        place.Freeze.Restart(ThawedFreezeLeftover.Of(hold));
                    }
                    frozen?.RunFrozen();
                    // The frame clock advances after every enemy whose AI runs,
                    // a frozen enemy's included. Animation gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    EnemyLifecycle.RunMainAi(place, aiFrame);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    place.FrameCounter++;
                    // The enemy pass skips the animation whenever the enemy is
                    // frozen, even if the hurt state, tried first, selected an
                    // enemy-specific hurt behaviour above: a frozen Metroid's body holds its frame while its frozen
                    // behaviour runs its two outer effect actors.
                    if ((place.AiStates & EnemyAiStates.Frozen) == EnemyAiStates.None &&
                        place.Processing.HasAny(EnemyProcessing.Animates))
                        EnemyAnimationPlayer.AdvanceAnimation(
                            place,
                            frame.Samus,
                            frame.Level,
                            frame.Camera,
                            frame.ControllerInput);
                }
            }

            // The enemy pass queues each live enemy to show only after its AI and animation work; a hidden
            // or removed enemy is not shown. The draw pass reaches an enemy its radius puts on
            // screen, and every enemy that collides by its frames' hitboxes wherever it is. Only a reached enemy's draw
            // clears the value a Puyo's later ceiling strike reads, consumes its shake tick and shows its boss body
            // frame, so which enemies are reached stays the game's; what is off screen is culled by the display.
            // The gunship uses the radius test.
            bool reached = place.CollidesByFrameHitboxes ||
                EnemyProcessingWindow.IsWithinProcessingWindow(place, frame.Camera);
            if (!place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted))
                Drawing.Enqueue(place, reached);

            // The enemy pass counts the flash down after the enemy joins its draw queue, so a boss's draw colours show
            // the flash of the update just processed.
            if (!frame.TimeIsFrozen && place.HurtFlash.IsRunning)
            {
                place.HurtFlash.Count();
                // The enemy pass ends the hurt state once too few flash updates remain, even while another AI state stays
                // set, so a Metroid's own hurt behaviour ends.
                if (place.HurtFlash.UpdatesLeft < EnemyCollisionDefinitions.HurtStateMinimumFlashUpdates)
                    place.AiStates = place.AiStates & ~EnemyAiStates.Hurt;
            }
        }

        // The processing window fixes only which enemies interact this frame. Samus's
        // collision reads their live position and size, so publish bodies after AI has
        // had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (RoomEnemyPlace place in ProcessingWindow.InteractiveEnemies)
        {
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                place,
                place.Position.Whole,
                new(place.Radius.X, place.Radius.Y),
                // A frozen enemy blocks Samus whatever its properties say.
                place.Freeze.IsRunning || place.Contact.HasAny(EnemyContact.SolidToSamus)));
        }
        FrameContext.CountEnemyPass();
        frame.Samus.Kinematics.ClearSolidEnemyCollisions();
        // The room-wide glows step after every enemy has updated, whether or not time is frozen.
        WorkRobotBehavior.StepGlow();
        MagdolliteBehavior.StepGlow();
        BlueBrinstarFaceBlockBehavior.StepGlow();
        if (!frame.TimeIsFrozen)
            EffectActors.StepRoomEffectActors();
        return LastEnemyFrame;
    }

    /// <summary>
    /// Shakes the solid or frozen enemy Samus's latest wall jump launched from, as the enemy pass does before any enemy
    /// updates. While no enemy place is filled the request waits.
    /// </summary>
    internal void ShakeWallJumpLaunchEnemy(SamusState samus)
    {
        if (Places.EnemyCount == 0 || samus.Kinematics.WallJumpLaunch.Take() is not { } launchedFrom)
            return;
        launchedFrom.ShakeFromWallJumpLaunch();
    }

    /// <summary>Runs the room shaking: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        // This runs after drawing in the game, so the actors' shake (RoomEarthquakeDefinitions.EnemyShakeDuration) becomes visible
        // on the following frame rather than retroactively changing already-emitted sprites.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (RoomEnemyPlace place in ProcessingWindow.ActiveEnemies)
                place.Shake.Restart(RoomEarthquakeDefinitions.EnemyShakeDuration);
        }
        return Earthquake.LastRoomShake;
    }

    /// <summary>Advances enemy-projectile actors after Samus movement and before room objects.</summary>
    public void AdvanceEnemyProjectileAnimations(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> camera,
        int shortFrameCounter,
        SamusBombProjectileSystem samusBombs,
        int frameCounter)
    {
        Projectiles.RunPass(level, samus, camera, shortFrameCounter, samusBombs, frameCounter);
    }
}
