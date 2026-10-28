namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy species: the enemy definition a room population entry or spawn names. Every labelled enemy definition of
/// the pinned disassembly is a member. The values are arbitrary and nothing reads them; the import pipeline translates
/// the cartridge's header pointers to these members (#1412).
/// </summary>
public enum EnemyDefinitionId
{
    /// <summary>A cleared or inactive slot.</summary>
    None,
    /// <summary>The <c>Boyon</c> enemy definition.</summary>
    Boyon,
    /// <summary>The <c>Stoke</c> enemy definition.</summary>
    Stoke,
    /// <summary>The <c>MamaTurtle</c> enemy definition.</summary>
    MamaTurtle,
    /// <summary>The <c>BabyTurtle</c> enemy definition.</summary>
    BabyTurtle,
    /// <summary>The <c>Puyo</c> enemy definition.</summary>
    Puyo,
    /// <summary>The <c>Cacatac</c> enemy definition.</summary>
    Cacatac,
    /// <summary>The <c>Owtch</c> enemy definition.</summary>
    Owtch,
    /// <summary>The <c>ShipTop</c> enemy definition.</summary>
    ShipTop,
    /// <summary>The <c>ShipBottomEntrance</c> enemy definition.</summary>
    ShipBottomEntrance,
    /// <summary>The <c>Mellow</c> enemy definition.</summary>
    Mellow,
    /// <summary>The <c>Mella</c> enemy definition.</summary>
    Mella,
    /// <summary>The <c>Menu</c> enemy definition.</summary>
    Menu,
    /// <summary>The <c>Multiviola</c> enemy definition.</summary>
    Multiviola,
    /// <summary>The <c>LavaRocks</c> enemy definition.</summary>
    LavaRocks,
    /// <summary>The <c>Rinka</c> enemy definition.</summary>
    Rinka,
    /// <summary>The <c>Rio</c> enemy definition.</summary>
    Rio,
    /// <summary>The <c>Squeept</c> enemy definition.</summary>
    Squeept,
    /// <summary>The <c>Geruta</c> enemy definition.</summary>
    Geruta,
    /// <summary>The <c>Holtz</c> enemy definition.</summary>
    Holtz,
    /// <summary>The <c>Oum</c> enemy definition.</summary>
    Oum,
    /// <summary>The <c>Choot</c> enemy definition.</summary>
    Choot,
    /// <summary>The <c>GRipper</c> enemy definition.</summary>
    GRipper,
    /// <summary>The <c>Ripper2</c> enemy definition.</summary>
    Ripper2,
    /// <summary>The <c>Ripper</c> enemy definition.</summary>
    Ripper,
    /// <summary>The <c>Dragon</c> enemy definition.</summary>
    Dragon,
    /// <summary>The <c>ShutterGrowing</c> enemy definition.</summary>
    ShutterGrowing,
    /// <summary>The <c>ShutterShootable</c> enemy definition.</summary>
    ShutterShootable,
    /// <summary>The <c>ShutterHorizShootable</c> enemy definition.</summary>
    ShutterHorizShootable,
    /// <summary>The <c>ShutterDestroyable</c> enemy definition.</summary>
    ShutterDestroyable,
    /// <summary>The <c>Kamer</c> enemy definition.</summary>
    Kamer,
    /// <summary>The <c>Waver</c> enemy definition.</summary>
    Waver,
    /// <summary>The <c>Metaree</c> enemy definition.</summary>
    Metaree,
    /// <summary>The <c>Fireflea</c> enemy definition.</summary>
    Fireflea,
    /// <summary>The <c>Skultera</c> enemy definition.</summary>
    Skultera,
    /// <summary>The <c>Elevator</c> enemy definition.</summary>
    Elevator,
    /// <summary>The <c>Sciser</c> enemy definition.</summary>
    Sciser,
    /// <summary>The <c>Zero</c> enemy definition.</summary>
    Zero,
    /// <summary>The <c>Tripper</c> enemy definition.</summary>
    Tripper,
    /// <summary>The <c>Kamer2</c> enemy definition.</summary>
    Kamer2,
    /// <summary>The <c>Sbug</c> enemy definition.</summary>
    Sbug,
    /// <summary>The <c>Sbug2</c> enemy definition.</summary>
    Sbug2,
    /// <summary>The <c>Mochtroid</c> enemy definition.</summary>
    Mochtroid,
    /// <summary>The <c>Sidehopper</c> enemy definition.</summary>
    Sidehopper,
    /// <summary>The <c>Dessgeega</c> enemy definition.</summary>
    Dessgeega,
    /// <summary>The <c>SidehopperLarge</c> enemy definition.</summary>
    SidehopperLarge,
    /// <summary>The <c>SidehopperTourian</c> enemy definition.</summary>
    SidehopperTourian,
    /// <summary>The <c>DessgeegaLarge</c> enemy definition.</summary>
    DessgeegaLarge,
    /// <summary>The <c>Zoa</c> enemy definition.</summary>
    Zoa,
    /// <summary>The <c>Viola</c> enemy definition.</summary>
    Viola,
    /// <summary>The <c>Respawn</c> enemy definition.</summary>
    /// <remarks>Reserves a killed actor's slot until it respawns; its AI is inert.</remarks>
    Respawn,
    /// <summary>The <c>Bang</c> enemy definition.</summary>
    Bang,
    /// <summary>The <c>Skree</c> enemy definition.</summary>
    Skree,
    /// <summary>The <c>Yard</c> enemy definition.</summary>
    Yard,
    /// <summary>The <c>Reflec</c> enemy definition.</summary>
    Reflec,
    /// <summary>The <c>HZoomer</c> enemy definition.</summary>
    HZoomer,
    /// <summary>The <c>Zeela</c> enemy definition.</summary>
    Zeela,
    /// <summary>The <c>Sova</c> enemy definition.</summary>
    Sova,
    /// <summary>The <c>Zoomer</c> enemy definition.</summary>
    Zoomer,
    /// <summary>The <c>MZoomer</c> enemy definition.</summary>
    MZoomer,
    /// <summary>The <c>Metroid</c> enemy definition.</summary>
    Metroid,
    /// <summary>The <c>Crocomire</c> enemy definition.</summary>
    Crocomire,
    /// <summary>The <c>CrocomireTongue</c> enemy definition.</summary>
    CrocomireTongue,
    /// <summary>The <c>DraygonBody</c> enemy definition.</summary>
    /// <remarks>The Draygon body and encounter owner.</remarks>
    DraygonBody,
    /// <summary>The <c>DraygonEye</c> enemy definition.</summary>
    /// <remarks>Draygon's eye actor.</remarks>
    DraygonEye,
    /// <summary>The <c>DraygonTail</c> enemy definition.</summary>
    /// <remarks>Draygon's tail actor.</remarks>
    DraygonTail,
    /// <summary>The <c>DraygonArms</c> enemy definition.</summary>
    /// <remarks>Draygon's arms actor.</remarks>
    DraygonArms,
    /// <summary>The <c>SporeSpawn</c> enemy definition.</summary>
    /// <remarks>Spore Spawn's body, distinct from its stalk.</remarks>
    SporeSpawn,
    /// <summary>The <c>SporeSpawnStalk</c> enemy definition.</summary>
    /// <remarks>Spore impacts roll their drops from this definition's table, not from the Spore Spawn body's.</remarks>
    SporeSpawnStalk,
    /// <summary>The <c>Boulder</c> enemy definition.</summary>
    Boulder,
    /// <summary>The <c>KzanTop</c> enemy definition.</summary>
    KzanTop,
    /// <summary>The <c>KzanBottom</c> enemy definition.</summary>
    KzanBottom,
    /// <summary>The <c>Hibashi</c> enemy definition.</summary>
    Hibashi,
    /// <summary>The <c>Puromi</c> enemy definition.</summary>
    Puromi,
    /// <summary>The <c>MiniKraid</c> enemy definition.</summary>
    MiniKraid,
    /// <summary>The <c>RidleyCeres</c> enemy definition.</summary>
    /// <remarks>Ceres Ridley, which shares Ridley's implementation.</remarks>
    RidleyCeres,
    /// <summary>The <c>Ridley</c> enemy definition.</summary>
    /// <remarks>Selects the Lower Norfair boss branches of Ridley's shared implementation.</remarks>
    Ridley,
    /// <summary>The <c>RidleyExplosion</c> enemy definition.</summary>
    RidleyExplosion,
    /// <summary>The <c>Steam</c> enemy definition.</summary>
    /// <remarks>The Ceres steam.</remarks>
    Steam,
    /// <summary>The <c>CeresDoor</c> enemy definition.</summary>
    CeresDoor,
    /// <summary>The <c>Zebetite</c> enemy definition.</summary>
    Zebetite,
    /// <summary>The <c>Kraid</c> enemy definition.</summary>
    Kraid,
    /// <summary>The <c>KraidArm</c> enemy definition.</summary>
    KraidArm,
    /// <summary>The <c>KraidLintTop</c> enemy definition.</summary>
    KraidLintTop,
    /// <summary>The <c>KraidLintMiddle</c> enemy definition.</summary>
    KraidLintMiddle,
    /// <summary>The <c>KraidLintBottom</c> enemy definition.</summary>
    KraidLintBottom,
    /// <summary>The <c>KraidFoot</c> enemy definition.</summary>
    KraidFoot,
    /// <summary>The <c>KraidNail</c> enemy definition.</summary>
    KraidNail,
    /// <summary>The <c>KraidNailBad</c> enemy definition.</summary>
    KraidNailBad,
    /// <summary>The <c>PhantoonBody</c> enemy definition.</summary>
    PhantoonBody,
    /// <summary>The <c>PhantoonEye</c> enemy definition.</summary>
    PhantoonEye,
    /// <summary>The <c>PhantoonTentacles</c> enemy definition.</summary>
    PhantoonTentacles,
    /// <summary>The <c>PhantoonMouth</c> enemy definition.</summary>
    PhantoonMouth,
    /// <summary>The <c>Etecoon</c> enemy definition.</summary>
    Etecoon,
    /// <summary>The <c>Dachora</c> enemy definition.</summary>
    Dachora,
    /// <summary>The <c>Evir</c> enemy definition.</summary>
    Evir,
    /// <summary>The <c>EvirProjectile</c> enemy definition.</summary>
    EvirProjectile,
    /// <summary>The <c>Eye</c> enemy definition.</summary>
    Eye,
    /// <summary>The <c>Fune</c> enemy definition.</summary>
    Fune,
    /// <summary>The <c>Namihe</c> enemy definition.</summary>
    Namihe,
    /// <summary>The <c>Coven</c> enemy definition.</summary>
    Coven,
    /// <summary>The <c>YappingMaw</c> enemy definition.</summary>
    YappingMaw,
    /// <summary>The <c>Kago</c> enemy definition.</summary>
    Kago,
    /// <summary>The <c>Magdollite</c> enemy definition.</summary>
    Magdollite,
    /// <summary>The <c>Beetom</c> enemy definition.</summary>
    Beetom,
    /// <summary>The <c>Powamp</c> enemy definition.</summary>
    Powamp,
    /// <summary>The <c>Robot</c> enemy definition.</summary>
    Robot,
    /// <summary>The <c>RobotNoPower</c> enemy definition.</summary>
    RobotNoPower,
    /// <summary>The <c>Bull</c> enemy definition.</summary>
    Bull,
    /// <summary>The <c>Alcoon</c> enemy definition.</summary>
    Alcoon,
    /// <summary>The <c>Atomic</c> enemy definition.</summary>
    Atomic,
    /// <summary>The <c>Spark</c> enemy definition.</summary>
    Spark,
    /// <summary>The <c>FaceBlock</c> enemy definition.</summary>
    FaceBlock,
    /// <summary>The <c>KihunterGreen</c> enemy definition.</summary>
    KihunterGreen,
    /// <summary>The <c>KihunterGreenWings</c> enemy definition.</summary>
    KihunterGreenWings,
    /// <summary>The <c>KihunterYellow</c> enemy definition.</summary>
    KihunterYellow,
    /// <summary>The <c>KihunterYellowWings</c> enemy definition.</summary>
    KihunterYellowWings,
    /// <summary>The <c>KihunterRed</c> enemy definition.</summary>
    KihunterRed,
    /// <summary>The <c>KihunterRedWings</c> enemy definition.</summary>
    KihunterRedWings,
    /// <summary>The <c>MotherBrainHead</c> enemy definition.</summary>
    MotherBrainHead,
    /// <summary>The <c>MotherBrainBody</c> enemy definition.</summary>
    MotherBrainBody,
    /// <summary>The <c>BabyMetroidCutscene</c> enemy definition.</summary>
    /// <remarks>The cutscene Baby Metroid.</remarks>
    BabyMetroidCutscene,
    /// <summary>The <c>MotherBrainTubes</c> enemy definition.</summary>
    /// <remarks>Mother Brain's falling tube actor.</remarks>
    MotherBrainTubes,
    /// <summary>The <c>CorpseTorizo</c> enemy definition.</summary>
    /// <remarks>The Tourian Torizo corpse, which must own enemy slot zero.</remarks>
    CorpseTorizo,
    /// <summary>The <c>CorpseSidehopper</c> enemy definition.</summary>
    /// <remarks>The dead-sidehopper family: an initially alive victim or an already dead Tourian corpse.</remarks>
    CorpseSidehopper,
    /// <summary>The <c>CorpseSidehopper2</c> enemy definition.</summary>
    CorpseSidehopper2,
    /// <summary>The <c>CorpseZoomer</c> enemy definition.</summary>
    /// <remarks>The dead Zoomer, with parameter-1 variants 0, 2 and 4 sharing the corpse-rotting engine.</remarks>
    CorpseZoomer,
    /// <summary>The <c>CorpseRipper</c> enemy definition.</summary>
    /// <remarks>The dead Ripper, with parameter-1 variants 0 and 2 sharing the corpse-rotting engine.</remarks>
    CorpseRipper,
    /// <summary>The <c>CorpseSkree</c> enemy definition.</summary>
    /// <remarks>The dead Skree, with parameter-1 variants 0, 2 and 4 sharing the corpse-rotting engine.</remarks>
    CorpseSkree,
    /// <summary>The <c>BabyMetroid</c> enemy definition.</summary>
    /// <remarks>The retail Shitroid actor.</remarks>
    BabyMetroid,
    /// <summary>The <c>BombTorizo</c> enemy definition.</summary>
    BombTorizo,
    /// <summary>The <c>BombTorizoOrb</c> enemy definition.</summary>
    BombTorizoOrb,
    /// <summary>The <c>GoldenTorizo</c> enemy definition.</summary>
    GoldenTorizo,
    /// <summary>The <c>GoldenTorizoOrb</c> enemy definition.</summary>
    GoldenTorizoOrb,
    /// <summary>The <c>TourianStatue</c> enemy definition.</summary>
    TourianStatue,
    /// <summary>The <c>TourianStatueGhost</c> enemy definition.</summary>
    TourianStatueGhost,
    /// <summary>The <c>Shaktool</c> enemy definition.</summary>
    Shaktool,
    /// <summary>The <c>NoobTubeCrack</c> enemy definition.</summary>
    NoobTubeCrack,
    /// <summary>The <c>Chozo</c> enemy definition.</summary>
    /// <remarks>The shared Chozo statue definition.</remarks>
    Chozo,
    /// <summary>The unused spinning turtle eye.</summary>
    UnusedSpinningTurtleEye,
    /// <summary>The <c>Zeb</c> enemy definition.</summary>
    /// <remarks>The normal Brinstar Pipe Bug.</remarks>
    Zeb,
    /// <summary>The <c>Zebbo</c> enemy definition.</summary>
    /// <remarks>The strong Brinstar Pipe Bug.</remarks>
    Zebbo,
    /// <summary>The <c>Gamet</c> enemy definition.</summary>
    /// <remarks>The Norfair Pipe Bug.</remarks>
    Gamet,
    /// <summary>The <c>Geega</c> enemy definition.</summary>
    /// <remarks>The yellow Brinstar Pipe Bug.</remarks>
    Geega,
    /// <summary>The <c>Botwoon</c> enemy definition.</summary>
    Botwoon,
    /// <summary>The <c>EtecoonEscape</c> enemy definition.</summary>
    EtecoonEscape,
    /// <summary>The <c>DachoraEscape</c> enemy definition.</summary>
    DachoraEscape,
    /// <summary>The <c>PirateGreyWall</c> enemy definition.</summary>
    PirateGreyWall,
    /// <summary>The <c>PirateGreenWall</c> enemy definition.</summary>
    PirateGreenWall,
    /// <summary>The <c>PirateRedWall</c> enemy definition.</summary>
    PirateRedWall,
    /// <summary>The <c>PirateGoldWall</c> enemy definition.</summary>
    /// <remarks>Ninja Pirate initialization copies this definition's sixteen colours to its target palette, irrespective of the ninja actor's own definition or starting colour.</remarks>
    PirateGoldWall,
    /// <summary>The <c>PirateMagentaWall</c> enemy definition.</summary>
    PirateMagentaWall,
    /// <summary>The <c>PirateSilverWall</c> enemy definition.</summary>
    PirateSilverWall,
    /// <summary>The <c>PirateGreyNinja</c> enemy definition.</summary>
    PirateGreyNinja,
    /// <summary>The <c>PirateGreenNinja</c> enemy definition.</summary>
    PirateGreenNinja,
    /// <summary>The <c>PirateRedNinja</c> enemy definition.</summary>
    PirateRedNinja,
    /// <summary>The <c>PirateGoldNinja</c> enemy definition.</summary>
    PirateGoldNinja,
    /// <summary>The <c>PirateMagentaNinja</c> enemy definition.</summary>
    PirateMagentaNinja,
    /// <summary>The <c>PirateSilverNinja</c> enemy definition.</summary>
    PirateSilverNinja,
    /// <summary>The <c>PirateGreyWalking</c> enemy definition.</summary>
    PirateGreyWalking,
    /// <summary>The <c>PirateGreenWalking</c> enemy definition.</summary>
    PirateGreenWalking,
    /// <summary>The <c>PirateRedWalking</c> enemy definition.</summary>
    PirateRedWalking,
    /// <summary>The <c>PirateGoldWalking</c> enemy definition.</summary>
    PirateGoldWalking,
    /// <summary>The <c>PirateMagentaWalking</c> enemy definition.</summary>
    PirateMagentaWalking,
    /// <summary>The <c>PirateSilverWalking</c> enemy definition.</summary>
    PirateSilverWalking,
}
