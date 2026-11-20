namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy species: the enemy definition a room population entry or spawn names. Every enemy definition of
/// the game is a member, named for what the enemy is. The values are arbitrary and nothing reads them; the
/// import pipeline maps the game's enemy definitions to these members (#1412).
/// </summary>
public enum EnemyDefinitionId
{
    /// <summary>No enemy: a cleared or inactive place.</summary>
    None,
    /// <summary>A Boyon, the jelly that bounces in place.</summary>
    Boyon,
    /// <summary>The Mama Turtle, the giant Maridia turtle whose shell Samus can ride.</summary>
    MamaTurtle,
    /// <summary>A Baby Turtle, one of the Mama Turtle's four young.</summary>
    BabyTurtle,
    /// <summary>A Puyo, the small blob that hops along the floor.</summary>
    Puyo,
    /// <summary>A Cacatac, the Maridia cactus that shoots its spikes.</summary>
    Cacatac,
    /// <summary>An Owtch, the spiked shell that creeps across Maridia's sand.</summary>
    Owtch,
    /// <summary>The top of Samus's gunship, which carries her on landing and takeoff.</summary>
    ShipTop,
    /// <summary>The gunship's bottom hull and its entrance pad.</summary>
    ShipBottomEntrance,
    /// <summary>A Mellow, a fly of Brinstar's swarms.</summary>
    Mellow,
    /// <summary>A Mella, a fly of Norfair's swarms.</summary>
    Mella,
    /// <summary>A Menu, a fly of Maridia's swarms.</summary>
    MaridiaFly,
    /// <summary>A Multiviola, the fireball that bounces around Norfair's rooms.</summary>
    Multiviola,
    /// <summary>A Polyp, the pool of lava that throws up rocks.</summary>
    Polyp,
    /// <summary>A Rinka, the ring of fire that flies at Samus in Tourian.</summary>
    Rinka,
    /// <summary>A Rio, the flyer that swoops down at Samus.</summary>
    Rio,
    /// <summary>A Squeept, the creature that leaps out of Norfair's lava.</summary>
    Squeept,
    /// <summary>A Geruta, the Norfair flyer that dives at Samus.</summary>
    Geruta,
    /// <summary>A Holtz, the Lower Norfair flyer that dives at Samus.</summary>
    Holtz,
    /// <summary>Oum, the large Maridia snail that rolls and bounces.</summary>
    Oum,
    /// <summary>A Choot, the flat creature that jumps and floats back down.</summary>
    Choot,
    /// <summary>A grapple Ripper: a jet-powered Ripper that patrols between two bounds and holds the Grapple Beam.</summary>
    GrappleRipper,
    /// <summary>A jet Ripper: a jet-powered Ripper that flies from wall to wall and does not hold the Grapple Beam.</summary>
    JetRipper,
    /// <summary>A Ripper, the armoured flyer that crosses a room from wall to wall.</summary>
    Ripper,
    /// <summary>A Dragon, the serpent that rises from Norfair's lava and spits fireballs.</summary>
    Dragon,
    /// <summary>A shutter that grows out of its block to close a passage.</summary>
    ShutterGrowing,
    /// <summary>A shutter that a shot opens.</summary>
    ShutterShootable,
    /// <summary>A shutter that Samus's weapons destroy.</summary>
    ShutterDestroyable,
    /// <summary>A Kamer, the shutter-like platform that moves up and down.</summary>
    Kamer,
    /// <summary>A Waver, the flyer that weaves up and down as it crosses a room.</summary>
    Waver,
    /// <summary>A Metaree, which drops from the ceiling at Samus and burrows into the floor.</summary>
    Metaree,
    /// <summary>A Fireflea, the glowing insect whose deaths darken Lower Norfair's rooms.</summary>
    Fireflea,
    /// <summary>A Skultera, the Maridia fish that swims back and forth.</summary>
    Skultera,
    /// <summary>An elevator platform, which carries Samus between rooms.</summary>
    Elevator,
    /// <summary>A Sciser, the Maridia crab that crawls along walls and ceilings.</summary>
    Sciser,
    /// <summary>A Zero, the slug that creeps along surfaces.</summary>
    Zero,
    /// <summary>A Tripper, the flying platform that carries Samus.</summary>
    Tripper,
    /// <summary>
    /// The suspensor platform: the Tripper's carrying platform with its suspensor art, which sinks under Samus and
    /// floats back up.
    /// </summary>
    SuspensorPlatform,
    /// <summary>An Sbug, one of the tiny bugs that dart about.</summary>
    Sbug,
    /// <summary>A Mochtroid, the imitation Metroid of Maridia that latches onto Samus.</summary>
    Mochtroid,
    /// <summary>A Sidehopper, the four-legged hopper.</summary>
    Sidehopper,
    /// <summary>A Dessgeega, Norfair's four-legged hopper.</summary>
    Dessgeega,
    /// <summary>A large Sidehopper.</summary>
    SidehopperLarge,
    /// <summary>A Sidehopper of Tourian, which its Metroids drain.</summary>
    SidehopperTourian,
    /// <summary>A large Dessgeega.</summary>
    DessgeegaLarge,
    /// <summary>A Zoa, which waits until Samus comes near, rises and darts across at her.</summary>
    Zoa,
    /// <summary>A Viola, the fiery crawler of Norfair.</summary>
    Viola,

    /// <summary>A Skree, which hangs from the ceiling, dives at Samus and bursts on the floor.</summary>
    Skree,
    /// <summary>A Yard, the Maridia crawler that curls up into a ball.</summary>
    Yard,
    /// <summary>The orange Zoomer, a crawler that turns along its surface to follow Samus.</summary>
    OrangeZoomer,
    /// <summary>A Zeela, a crawler that creeps around surfaces.</summary>
    Zeela,
    /// <summary>A Sova, Norfair's fiery crawler.</summary>
    Sova,
    /// <summary>A Zoomer, the spiked crawler.</summary>
    Zoomer,
    /// <summary>The armoured Zoomer, a crawler that most of Samus's beams do not hurt.</summary>
    ArmoredZoomer,
    /// <summary>A Metroid, which latches onto Samus and drains her energy.</summary>
    Metroid,
    /// <summary>Crocomire, Norfair's boss, pushed back toward the lava by Samus's shots.</summary>
    Crocomire,
    /// <summary>Crocomire's tongue, the target its mouth opens to show.</summary>
    CrocomireTongue,
    /// <summary>Draygon's body, which runs the fight.</summary>
    /// <remarks>The Draygon body and encounter owner.</remarks>
    DraygonBody,
    /// <summary>Draygon's eye.</summary>
    /// <remarks>Draygon's eye actor.</remarks>
    DraygonEye,
    /// <summary>Draygon's tail.</summary>
    /// <remarks>Draygon's tail actor.</remarks>
    DraygonTail,
    /// <summary>Draygon's arms.</summary>
    /// <remarks>Draygon's arms actor.</remarks>
    DraygonArms,
    /// <summary>Spore Spawn, Brinstar's plant boss.</summary>
    SporeSpawn,
    /// <summary>A Boulder, the rock that falls and rolls along the floor.</summary>
    Boulder,
    /// <summary>The top of a Kzan, the block that crushes down and rises back, carrying Samus.</summary>
    KzanTop,
    /// <summary>The bottom of a Kzan, which follows its top.</summary>
    KzanBottom,
    /// <summary>A Hibashi, the pillar of flame that erupts from Norfair's floor.</summary>
    Hibashi,
    /// <summary>The Nuclear Waffle, which sweeps around its centre; the game's own name for it is Puromi.</summary>
    NuclearWaffle,
    /// <summary>Fake Kraid, the small Kraid of Brinstar.</summary>
    FakeKraid,
    /// <summary>Ridley at Ceres station.</summary>
    /// <remarks>Ceres Ridley, which shares Ridley's implementation.</remarks>
    RidleyCeres,
    /// <summary>Ridley, the boss of Lower Norfair.</summary>
    /// <remarks>Ridley as the Lower Norfair boss, where Ceres Ridley's behaviour differs.</remarks>
    Ridley,
    /// <summary>One piece of Ridley's body as it breaks apart at his death.</summary>
    RidleyBreakupPiece,
    /// <summary>A jet of steam in Ceres station.</summary>
    CeresSteam,
    /// <summary>An ordinary Ceres door, facing either way, which opens as Samus comes near (#1989).</summary>
    CeresDoor,
    /// <summary>The door of Ceres Ridley's room, which his scene draws while he holds the room (#1989).</summary>
    CeresRidleyRoomDoor,
    /// <summary>The Ceres elevator room's blocked door, which blows apart once the station starts to self-destruct (#1989).</summary>
    CeresElevatorDoor,
    /// <summary>The Ceres elevator room's left door, an ordinary door until Ridley escapes and an unseen wall after (#1989).</summary>
    CeresElevatorRoomWall,
    /// <summary>A wall of Ceres Ridley's getaway chamber, beside its turning floor, which his getaway places (#1989).</summary>
    CeresGetawayChamberWall,
    /// <summary>A Zebetite, one of the barriers that guard Mother Brain.</summary>
    Zebetite,
    /// <summary>Kraid, Brinstar's giant boss.</summary>
    Kraid,
    /// <summary>Kraid's arm.</summary>
    KraidArm,
    /// <summary>The top spike Kraid fires from his belly, which Samus can stand on.</summary>
    KraidLintTop,
    /// <summary>The middle spike Kraid fires from his belly.</summary>
    KraidLintMiddle,
    /// <summary>The bottom spike Kraid fires from his belly.</summary>
    KraidLintBottom,
    /// <summary>Kraid's foot.</summary>
    KraidFoot,
    /// <summary>The first of the two fingernails Kraid throws.</summary>
    KraidFirstNail,
    /// <summary>The second of the two fingernails Kraid throws, launched opposite the first's vertical travel.</summary>
    KraidSecondNail,
    /// <summary>Phantoon's body, the Wrecked Ship's boss.</summary>
    PhantoonBody,
    /// <summary>Phantoon's eye.</summary>
    PhantoonEye,
    /// <summary>Phantoon's tentacles.</summary>
    PhantoonTentacles,
    /// <summary>Phantoon's mouth.</summary>
    PhantoonMouth,
    /// <summary>An Etecoon, one of the small green creatures who show Samus the wall jump.</summary>
    Etecoon,
    /// <summary>The Dachora, the long-legged creature who shows Samus the shinespark.</summary>
    Dachora,
    /// <summary>An Evir, the Maridia creature that circles its place and spits at Samus.</summary>
    Evir,
    /// <summary>An Evir's spit.</summary>
    EvirProjectile,
    /// <summary>The Morph Ball eye, set in the wall, which turns to watch Samus and shines its beam on her.</summary>
    MorphBallEye,
    /// <summary>A Fune, the head in a Norfair wall that spits fireballs.</summary>
    Fune,
    /// <summary>A Namihe, the head in a Norfair wall that spits fireballs.</summary>
    Namihe,
    /// <summary>A Coven, the ghost that fades in and drifts toward Samus.</summary>
    Coven,
    /// <summary>A Yapping Maw, the snapping mouth on a jointed stalk that grabs Samus.</summary>
    YappingMaw,
    /// <summary>A Kago, the hive that sends out its bugs when shot.</summary>
    Kago,
    /// <summary>A Magdollite, the Norfair lava creature that stretches up and throws fire.</summary>
    Magdollite,
    /// <summary>A Beetom, the bug that latches onto Samus and drains her.</summary>
    Beetom,
    /// <summary>A Powamp, the Maridia puffer that rises and sinks and fires spikes.</summary>
    Powamp,
    /// <summary>A Work Robot of the Wrecked Ship with the power on, which walks and fires its laser.</summary>
    WorkRobot,
    /// <summary>A Work Robot of the Wrecked Ship before its power returns.</summary>
    UnpoweredWorkRobot,
    /// <summary>A Bull, the floating creature that drifts after Samus.</summary>
    Bull,
    /// <summary>An Alcoon, the Norfair lizard that leaps out of the ground and spits fireballs.</summary>
    Alcoon,
    /// <summary>An Atomic, the Wrecked Ship's floating atom.</summary>
    Atomic,
    /// <summary>A Spark, the Wrecked Ship's electric spark.</summary>
    Spark,
    /// <summary>A face block, the Brinstar wall face that glows and wakes once Samus has the item it guards.</summary>
    FaceBlock,
    /// <summary>A green Ki-Hunter's body.</summary>
    KihunterGreen,
    /// <summary>A green Ki-Hunter's wings.</summary>
    KihunterGreenWings,
    /// <summary>A yellow Ki-Hunter's body.</summary>
    KihunterYellow,
    /// <summary>A yellow Ki-Hunter's wings.</summary>
    KihunterYellowWings,
    /// <summary>A red Ki-Hunter's body.</summary>
    KihunterRed,
    /// <summary>A red Ki-Hunter's wings.</summary>
    KihunterRedWings,
    /// <summary>Mother Brain's head, the brain of her glass tank and later of her standing body.</summary>
    MotherBrainHead,
    /// <summary>Mother Brain's body.</summary>
    MotherBrainBody,
    /// <summary>The Baby Metroid of Mother Brain's fight.</summary>
    /// <remarks>The cutscene Baby Metroid.</remarks>
    BabyMetroidCutscene,
    /// <summary>The glass tubes around Mother Brain's tank, which fall as she rises.</summary>
    /// <remarks>Mother Brain's falling tube actor.</remarks>
    MotherBrainTubes,
    /// <summary>The Torizo corpse in Tourian.</summary>
    /// <remarks>The Tourian Torizo corpse.</remarks>
    CorpseTorizo,
    /// <summary>A dead Sidehopper.</summary>
    /// <remarks>The dead-sidehopper family: an initially alive victim or an already dead Tourian corpse.</remarks>
    CorpseSidehopper,
    /// <summary>A dead Zoomer.</summary>
    /// <remarks>The dead Zoomer, with its three corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseZoomer,
    /// <summary>A dead Ripper.</summary>
    /// <remarks>The dead Ripper, with its two corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseRipper,
    /// <summary>A dead Skree.</summary>
    /// <remarks>The dead Skree, with its three corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseSkree,
    /// <summary>The Baby Metroid in Tourian, which drains Samus and then leaves her.</summary>
    BabyMetroid,
    /// <summary>Bomb Torizo, the Chozo statue that wakes in Crateria.</summary>
    BombTorizo,
    /// <summary>Golden Torizo, the Chozo statue that wakes in Lower Norfair.</summary>
    GoldenTorizo,
    /// <summary>The boss statues before Tourian's entrance.</summary>
    TourianStatue,
    /// <summary>Shaktool, the chain of segments that digs through Maridia's sand.</summary>
    Shaktool,
    /// <summary>The crack in Maridia's glass tube, which a Power Bomb shatters.</summary>
    NoobTubeCrack,
    /// <summary>A Chozo statue that holds an item or carries Samus.</summary>
    /// <remarks>The shared Chozo statue definition.</remarks>
    Chozo,
    /// <summary>A Zeb, the bug that flies out of a Brinstar pipe at Samus.</summary>
    /// <remarks>The normal Brinstar Pipe Bug.</remarks>
    Zeb,
    /// <summary>A Zebbo, the stronger bug that flies out of a Brinstar pipe.</summary>
    /// <remarks>The strong Brinstar Pipe Bug.</remarks>
    Zebbo,
    /// <summary>A Gamet, the bug that flies out of a Norfair pipe.</summary>
    /// <remarks>The Norfair Pipe Bug.</remarks>
    Gamet,
    /// <summary>A Geega, the yellow bug that flies out of a Brinstar pipe.</summary>
    /// <remarks>The yellow Brinstar Pipe Bug.</remarks>
    Geega,
    /// <summary>Botwoon, the serpent boss that weaves through Maridia's holes.</summary>
    Botwoon,
    /// <summary>The Etecoons as they escape Zebes.</summary>
    EtecoonEscape,
    /// <summary>The Dachora as it escapes Zebes.</summary>
    DachoraEscape,
    /// <summary>A grey Space Pirate that clings to walls and fires.</summary>
    PirateGreyWall,
    /// <summary>A gold Space Pirate that clings to walls and fires.</summary>
    /// <remarks>Its colours are the ones every Ninja Pirate's divekick flashes in, whatever the ninja's own colours.</remarks>
    PirateGoldWall,
    /// <summary>A magenta Space Pirate that clings to walls and fires.</summary>
    PirateMagentaWall,
    /// <summary>A silver Space Pirate that clings to walls and fires.</summary>
    PirateSilverWall,
    /// <summary>A gold Ninja Pirate, the leaping, divekicking Space Pirate.</summary>
    PirateGoldNinja,
    /// <summary>A silver Ninja Pirate, the leaping, divekicking Space Pirate.</summary>
    PirateSilverNinja,
    /// <summary>A grey Space Pirate that walks the floor and fires.</summary>
    PirateGreyWalking,
    /// <summary>A green Space Pirate that walks the floor and fires.</summary>
    PirateGreenWalking,
    /// <summary>A red Space Pirate that walks the floor and fires.</summary>
    PirateRedWalking,
    /// <summary>A gold Space Pirate that walks the floor and fires.</summary>
    PirateGoldWalking,
    /// <summary>A magenta Space Pirate that walks the floor and fires.</summary>
    PirateMagentaWalking,
    /// <summary>A silver Space Pirate that walks the floor and fires.</summary>
    PirateSilverWalking,
}
