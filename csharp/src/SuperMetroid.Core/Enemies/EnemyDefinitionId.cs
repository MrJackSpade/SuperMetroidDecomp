namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy species: the enemy definition a room population entry or spawn names. Every enemy definition of
/// the retail game is a member. The values are arbitrary and nothing reads them; the import pipeline maps
/// the game's enemy definitions to these members (#1412).
/// </summary>
public enum EnemyDefinitionId
{
    /// <summary>A cleared or inactive slot.</summary>
    None,
    /// <summary>The Boyon enemy definition.</summary>
    Boyon,
    /// <summary>The Mama Turtle enemy definition.</summary>
    MamaTurtle,
    /// <summary>The Baby Turtle enemy definition.</summary>
    BabyTurtle,
    /// <summary>The Puyo enemy definition.</summary>
    Puyo,
    /// <summary>The Cacatac enemy definition.</summary>
    Cacatac,
    /// <summary>The Owtch enemy definition.</summary>
    Owtch,
    /// <summary>The Ship Top enemy definition.</summary>
    ShipTop,
    /// <summary>The Ship Bottom Entrance enemy definition.</summary>
    ShipBottomEntrance,
    /// <summary>The Mellow enemy definition.</summary>
    Mellow,
    /// <summary>The Mella enemy definition.</summary>
    Mella,
    /// <summary>The Menu enemy definition.</summary>
    Menu,
    /// <summary>The Multiviola enemy definition.</summary>
    Multiviola,
    /// <summary>The Lava Rocks enemy definition.</summary>
    LavaRocks,
    /// <summary>The Rinka enemy definition.</summary>
    Rinka,
    /// <summary>The Rio enemy definition.</summary>
    Rio,
    /// <summary>The Squeept enemy definition.</summary>
    Squeept,
    /// <summary>The Geruta enemy definition.</summary>
    Geruta,
    /// <summary>The Holtz enemy definition.</summary>
    Holtz,
    /// <summary>The Oum enemy definition.</summary>
    Oum,
    /// <summary>The Choot enemy definition.</summary>
    Choot,
    /// <summary>The G Ripper enemy definition.</summary>
    GRipper,
    /// <summary>The Ripper2 enemy definition.</summary>
    Ripper2,
    /// <summary>The Ripper enemy definition.</summary>
    Ripper,
    /// <summary>The Dragon enemy definition.</summary>
    Dragon,
    /// <summary>The Shutter Growing enemy definition.</summary>
    ShutterGrowing,
    /// <summary>The Shutter Shootable enemy definition.</summary>
    ShutterShootable,
    /// <summary>The Shutter Destroyable enemy definition.</summary>
    ShutterDestroyable,
    /// <summary>The Kamer enemy definition.</summary>
    Kamer,
    /// <summary>The Waver enemy definition.</summary>
    Waver,
    /// <summary>The Metaree enemy definition.</summary>
    Metaree,
    /// <summary>The Fireflea enemy definition.</summary>
    Fireflea,
    /// <summary>The Skultera enemy definition.</summary>
    Skultera,
    /// <summary>The Elevator enemy definition.</summary>
    Elevator,
    /// <summary>The Sciser enemy definition.</summary>
    Sciser,
    /// <summary>The Zero enemy definition.</summary>
    Zero,
    /// <summary>The Tripper enemy definition.</summary>
    Tripper,
    /// <summary>The Kamer2 enemy definition.</summary>
    Kamer2,
    /// <summary>The Sbug enemy definition.</summary>
    Sbug,
    /// <summary>The Mochtroid enemy definition.</summary>
    Mochtroid,
    /// <summary>The Sidehopper enemy definition.</summary>
    Sidehopper,
    /// <summary>The Dessgeega enemy definition.</summary>
    Dessgeega,
    /// <summary>The Sidehopper Large enemy definition.</summary>
    SidehopperLarge,
    /// <summary>The Sidehopper Tourian enemy definition.</summary>
    SidehopperTourian,
    /// <summary>The Dessgeega Large enemy definition.</summary>
    DessgeegaLarge,
    /// <summary>The Zoa enemy definition.</summary>
    Zoa,
    /// <summary>The Viola enemy definition.</summary>
    Viola,
    /// <summary>The Respawn enemy definition.</summary>
    /// <remarks>Reserves a killed actor's slot until it respawns; its AI is inert.</remarks>
    Respawn,
    /// <summary>The Skree enemy definition.</summary>
    Skree,
    /// <summary>The Yard enemy definition.</summary>
    Yard,
    /// <summary>The H Zoomer enemy definition.</summary>
    HZoomer,
    /// <summary>The Zeela enemy definition.</summary>
    Zeela,
    /// <summary>The Sova enemy definition.</summary>
    Sova,
    /// <summary>The Zoomer enemy definition.</summary>
    Zoomer,
    /// <summary>The M Zoomer enemy definition.</summary>
    MZoomer,
    /// <summary>The Metroid enemy definition.</summary>
    Metroid,
    /// <summary>The Crocomire enemy definition.</summary>
    Crocomire,
    /// <summary>The Crocomire Tongue enemy definition.</summary>
    CrocomireTongue,
    /// <summary>The Draygon Body enemy definition.</summary>
    /// <remarks>The Draygon body and encounter owner.</remarks>
    DraygonBody,
    /// <summary>The Draygon Eye enemy definition.</summary>
    /// <remarks>Draygon's eye actor.</remarks>
    DraygonEye,
    /// <summary>The Draygon Tail enemy definition.</summary>
    /// <remarks>Draygon's tail actor.</remarks>
    DraygonTail,
    /// <summary>The Draygon Arms enemy definition.</summary>
    /// <remarks>Draygon's arms actor.</remarks>
    DraygonArms,
    /// <summary>The Spore Spawn enemy definition.</summary>
    /// <remarks>Spore Spawn's body, distinct from its stalk.</remarks>
    SporeSpawn,
    /// <summary>The Spore Spawn Stalk enemy definition.</summary>
    /// <remarks>Spore impacts roll their drops from this definition's table, not from the Spore Spawn body's.</remarks>
    SporeSpawnStalk,
    /// <summary>The Boulder enemy definition.</summary>
    Boulder,
    /// <summary>The Kzan Top enemy definition.</summary>
    KzanTop,
    /// <summary>The Kzan Bottom enemy definition.</summary>
    KzanBottom,
    /// <summary>The Hibashi enemy definition.</summary>
    Hibashi,
    /// <summary>The Puromi enemy definition.</summary>
    Puromi,
    /// <summary>The Mini Kraid enemy definition.</summary>
    MiniKraid,
    /// <summary>The Ridley Ceres enemy definition.</summary>
    /// <remarks>Ceres Ridley, which shares Ridley's implementation.</remarks>
    RidleyCeres,
    /// <summary>The Ridley enemy definition.</summary>
    /// <remarks>Selects the Lower Norfair boss branches of Ridley's shared implementation.</remarks>
    Ridley,
    /// <summary>The Ridley Explosion enemy definition.</summary>
    RidleyExplosion,
    /// <summary>The Steam enemy definition.</summary>
    /// <remarks>The Ceres steam.</remarks>
    Steam,
    /// <summary>The Ceres Door enemy definition.</summary>
    CeresDoor,
    /// <summary>The Zebetite enemy definition.</summary>
    Zebetite,
    /// <summary>The Kraid enemy definition.</summary>
    Kraid,
    /// <summary>The Kraid Arm enemy definition.</summary>
    KraidArm,
    /// <summary>The Kraid Lint Top enemy definition.</summary>
    KraidLintTop,
    /// <summary>The Kraid Lint Middle enemy definition.</summary>
    KraidLintMiddle,
    /// <summary>The Kraid Lint Bottom enemy definition.</summary>
    KraidLintBottom,
    /// <summary>The Kraid Foot enemy definition.</summary>
    KraidFoot,
    /// <summary>The Kraid Nail enemy definition.</summary>
    KraidNail,
    /// <summary>The Kraid Nail Bad enemy definition.</summary>
    KraidNailBad,
    /// <summary>The Phantoon Body enemy definition.</summary>
    PhantoonBody,
    /// <summary>The Phantoon Eye enemy definition.</summary>
    PhantoonEye,
    /// <summary>The Phantoon Tentacles enemy definition.</summary>
    PhantoonTentacles,
    /// <summary>The Phantoon Mouth enemy definition.</summary>
    PhantoonMouth,
    /// <summary>The Etecoon enemy definition.</summary>
    Etecoon,
    /// <summary>The Dachora enemy definition.</summary>
    Dachora,
    /// <summary>The Evir enemy definition.</summary>
    Evir,
    /// <summary>The Evir Projectile enemy definition.</summary>
    EvirProjectile,
    /// <summary>The Eye enemy definition.</summary>
    Eye,
    /// <summary>The Fune enemy definition.</summary>
    Fune,
    /// <summary>The Namihe enemy definition.</summary>
    Namihe,
    /// <summary>The Coven enemy definition.</summary>
    Coven,
    /// <summary>The Yapping Maw enemy definition.</summary>
    YappingMaw,
    /// <summary>The Kago enemy definition.</summary>
    Kago,
    /// <summary>The Magdollite enemy definition.</summary>
    Magdollite,
    /// <summary>The Beetom enemy definition.</summary>
    Beetom,
    /// <summary>The Powamp enemy definition.</summary>
    Powamp,
    /// <summary>The Robot enemy definition.</summary>
    Robot,
    /// <summary>The Robot No Power enemy definition.</summary>
    RobotNoPower,
    /// <summary>The Bull enemy definition.</summary>
    Bull,
    /// <summary>The Alcoon enemy definition.</summary>
    Alcoon,
    /// <summary>The Atomic enemy definition.</summary>
    Atomic,
    /// <summary>The Spark enemy definition.</summary>
    Spark,
    /// <summary>The Face Block enemy definition.</summary>
    FaceBlock,
    /// <summary>The Kihunter Green enemy definition.</summary>
    KihunterGreen,
    /// <summary>The Kihunter Green Wings enemy definition.</summary>
    KihunterGreenWings,
    /// <summary>The Kihunter Yellow enemy definition.</summary>
    KihunterYellow,
    /// <summary>The Kihunter Yellow Wings enemy definition.</summary>
    KihunterYellowWings,
    /// <summary>The Kihunter Red enemy definition.</summary>
    KihunterRed,
    /// <summary>The Kihunter Red Wings enemy definition.</summary>
    KihunterRedWings,
    /// <summary>The Mother Brain Head enemy definition.</summary>
    MotherBrainHead,
    /// <summary>The Mother Brain Body enemy definition.</summary>
    MotherBrainBody,
    /// <summary>The Baby Metroid Cutscene enemy definition.</summary>
    /// <remarks>The cutscene Baby Metroid.</remarks>
    BabyMetroidCutscene,
    /// <summary>The Mother Brain Tubes enemy definition.</summary>
    /// <remarks>Mother Brain's falling tube actor.</remarks>
    MotherBrainTubes,
    /// <summary>The Corpse Torizo enemy definition.</summary>
    /// <remarks>The Tourian Torizo corpse, which must own enemy slot zero.</remarks>
    CorpseTorizo,
    /// <summary>The Corpse Sidehopper enemy definition.</summary>
    /// <remarks>The dead-sidehopper family: an initially alive victim or an already dead Tourian corpse.</remarks>
    CorpseSidehopper,
    /// <summary>The Corpse Zoomer enemy definition.</summary>
    /// <remarks>The dead Zoomer, with its three corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseZoomer,
    /// <summary>The Corpse Ripper enemy definition.</summary>
    /// <remarks>The dead Ripper, with its two corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseRipper,
    /// <summary>The Corpse Skree enemy definition.</summary>
    /// <remarks>The dead Skree, with its three corpse variants sharing the corpse-rotting engine.</remarks>
    CorpseSkree,
    /// <summary>The Baby Metroid enemy definition.</summary>
    /// <remarks>The retail Shitroid actor.</remarks>
    BabyMetroid,
    /// <summary>The Bomb Torizo enemy definition.</summary>
    BombTorizo,
    /// <summary>The Bomb Torizo Orb enemy definition.</summary>
    BombTorizoOrb,
    /// <summary>The Golden Torizo enemy definition.</summary>
    GoldenTorizo,
    /// <summary>The Golden Torizo Orb enemy definition.</summary>
    GoldenTorizoOrb,
    /// <summary>The Tourian Statue enemy definition.</summary>
    TourianStatue,
    /// <summary>The Shaktool enemy definition.</summary>
    Shaktool,
    /// <summary>The Noob Tube Crack enemy definition.</summary>
    NoobTubeCrack,
    /// <summary>The Chozo enemy definition.</summary>
    /// <remarks>The shared Chozo statue definition.</remarks>
    Chozo,
    /// <summary>The Zeb enemy definition.</summary>
    /// <remarks>The normal Brinstar Pipe Bug.</remarks>
    Zeb,
    /// <summary>The Zebbo enemy definition.</summary>
    /// <remarks>The strong Brinstar Pipe Bug.</remarks>
    Zebbo,
    /// <summary>The Gamet enemy definition.</summary>
    /// <remarks>The Norfair Pipe Bug.</remarks>
    Gamet,
    /// <summary>The Geega enemy definition.</summary>
    /// <remarks>The yellow Brinstar Pipe Bug.</remarks>
    Geega,
    /// <summary>The Botwoon enemy definition.</summary>
    Botwoon,
    /// <summary>The Etecoon Escape enemy definition.</summary>
    EtecoonEscape,
    /// <summary>The Dachora Escape enemy definition.</summary>
    DachoraEscape,
    /// <summary>The Pirate Grey Wall enemy definition.</summary>
    PirateGreyWall,
    /// <summary>The Pirate Gold Wall enemy definition.</summary>
    /// <remarks>Its colours are the ones every Ninja Pirate's divekick flashes in, whatever the ninja's own colours.</remarks>
    PirateGoldWall,
    /// <summary>The Pirate Magenta Wall enemy definition.</summary>
    PirateMagentaWall,
    /// <summary>The Pirate Silver Wall enemy definition.</summary>
    PirateSilverWall,
    /// <summary>The Pirate Gold Ninja enemy definition.</summary>
    PirateGoldNinja,
    /// <summary>The Pirate Silver Ninja enemy definition.</summary>
    PirateSilverNinja,
    /// <summary>The Pirate Grey Walking enemy definition.</summary>
    PirateGreyWalking,
    /// <summary>The Pirate Green Walking enemy definition.</summary>
    PirateGreenWalking,
    /// <summary>The Pirate Red Walking enemy definition.</summary>
    PirateRedWalking,
    /// <summary>The Pirate Gold Walking enemy definition.</summary>
    PirateGoldWalking,
    /// <summary>The Pirate Magenta Walking enemy definition.</summary>
    PirateMagentaWalking,
    /// <summary>The Pirate Silver Walking enemy definition.</summary>
    PirateSilverWalking,
}
