namespace SuperMetroid.Core.Game;

/// <summary>
/// Enemy species: the bank-$A0 enemy header a room population entry or spawn names.
/// Values are the native header pointers; every labelled header in the pinned
/// disassembly is a member, generated from its EnemyHeaders_* labels.
/// </summary>
public enum EnemyDefinitionId : ushort
{
    /// <summary>Enemy.ID zero: a cleared or inactive slot.</summary>
    None = 0,
    /// <summary><c>EnemyHeaders_Boyon</c> at $A0:CEBF.</summary>
    Boyon = 0xcebf,
    /// <summary><c>EnemyHeaders_Stoke</c> at $A0:CEFF.</summary>
    Stoke = 0xceff,
    /// <summary><c>EnemyHeaders_MamaTurtle</c> at $A0:CF3F.</summary>
    /// <remarks><c>EnemyHeaders_MamaTurtle</c> at <c>$A0:CF3F</c>.</remarks>
    MamaTurtle = 0xcf3f,
    /// <summary><c>EnemyHeaders_BabyTurtle</c> at $A0:CF7F.</summary>
    /// <remarks><c>EnemyHeaders_BabyTurtle</c> at <c>$A0:CF7F</c>.</remarks>
    BabyTurtle = 0xcf7f,
    /// <summary><c>EnemyHeaders_Puyo</c> at $A0:CFBF.</summary>
    Puyo = 0xcfbf,
    /// <summary><c>EnemyHeaders_Cacatac</c> at $A0:CFFF.</summary>
    Cacatac = 0xcfff,
    /// <summary><c>EnemyHeaders_Owtch</c> at $A0:D03F.</summary>
    Owtch = 0xd03f,
    /// <summary><c>EnemyHeaders_ShipTop</c> at $A0:D07F.</summary>
    /// <remarks>Enemy definition <c>EnemyDefs_ShipTop</c> at $A0:D07F.</remarks>
    ShipTop = 0xd07f,
    /// <summary><c>EnemyHeaders_ShipBottomEntrance</c> at $A0:D0BF.</summary>
    /// <remarks>Enemy definition <c>EnemyDefs_ShipBottomEntrance</c> at $A0:D0BF.</remarks>
    ShipBottomEntrance = 0xd0bf,
    /// <summary><c>EnemyHeaders_Mellow</c> at $A0:D0FF.</summary>
    Mellow = 0xd0ff,
    /// <summary><c>EnemyHeaders_Mella</c> at $A0:D13F.</summary>
    Mella = 0xd13f,
    /// <summary><c>EnemyHeaders_Menu</c> at $A0:D17F.</summary>
    Menu = 0xd17f,
    /// <summary><c>EnemyHeaders_Multiviola</c> at $A0:D1BF.</summary>
    Multiviola = 0xd1bf,
    /// <summary><c>EnemyHeaders_LavaRocks</c> at $A0:D1FF.</summary>
    LavaRocks = 0xd1ff,
    /// <summary><c>EnemyHeaders_Rinka</c> at $A0:D23F.</summary>
    Rinka = 0xd23f,
    /// <summary><c>EnemyHeaders_Rio</c> at $A0:D27F.</summary>
    Rio = 0xd27f,
    /// <summary><c>EnemyHeaders_Squeept</c> at $A0:D2BF.</summary>
    Squeept = 0xd2bf,
    /// <summary><c>EnemyHeaders_Geruta</c> at $A0:D2FF.</summary>
    Geruta = 0xd2ff,
    /// <summary><c>EnemyHeaders_Holtz</c> at $A0:D33F.</summary>
    Holtz = 0xd33f,
    /// <summary><c>EnemyHeaders_Oum</c> at $A0:D37F.</summary>
    Oum = 0xd37f,
    /// <summary><c>EnemyHeaders_Choot</c> at $A0:D3BF.</summary>
    Choot = 0xd3bf,
    /// <summary><c>EnemyHeaders_GRipper</c> at $A0:D3FF.</summary>
    GRipper = 0xd3ff,
    /// <summary><c>EnemyHeaders_Ripper2</c> at $A0:D43F.</summary>
    Ripper2 = 0xd43f,
    /// <summary><c>EnemyHeaders_Ripper</c> at $A0:D47F.</summary>
    Ripper = 0xd47f,
    /// <summary><c>EnemyHeaders_Dragon</c> at $A0:D4BF.</summary>
    Dragon = 0xd4bf,
    /// <summary><c>EnemyHeaders_ShutterGrowing</c> at $A0:D4FF.</summary>
    ShutterGrowing = 0xd4ff,
    /// <summary><c>EnemyHeaders_ShutterShootable</c> at $A0:D53F.</summary>
    ShutterShootable = 0xd53f,
    /// <summary><c>EnemyHeaders_ShutterHorizShootable</c> at $A0:D57F.</summary>
    ShutterHorizShootable = 0xd57f,
    /// <summary><c>EnemyHeaders_ShutterDestroyable</c> at $A0:D5BF.</summary>
    ShutterDestroyable = 0xd5bf,
    /// <summary><c>EnemyHeaders_Kamer</c> at $A0:D5FF.</summary>
    Kamer = 0xd5ff,
    /// <summary><c>EnemyHeaders_Waver</c> at $A0:D63F.</summary>
    Waver = 0xd63f,
    /// <summary><c>EnemyHeaders_Metaree</c> at $A0:D67F.</summary>
    Metaree = 0xd67f,
    /// <summary><c>EnemyHeaders_Fireflea</c> at $A0:D6BF.</summary>
    Fireflea = 0xd6bf,
    /// <summary><c>EnemyHeaders_Skultera</c> at $A0:D6FF.</summary>
    Skultera = 0xd6ff,
    /// <summary><c>EnemyHeaders_Elevator</c> at $A0:D73F.</summary>
    Elevator = 0xd73f,
    /// <summary><c>EnemyHeaders_Sciser</c> at $A0:D77F.</summary>
    Sciser = 0xd77f,
    /// <summary><c>EnemyHeaders_Zero</c> at $A0:D7BF.</summary>
    Zero = 0xd7bf,
    /// <summary><c>EnemyHeaders_Tripper</c> at $A0:D7FF.</summary>
    Tripper = 0xd7ff,
    /// <summary><c>EnemyHeaders_Kamer2</c> at $A0:D83F.</summary>
    Kamer2 = 0xd83f,
    /// <summary><c>EnemyHeaders_Sbug</c> at $A0:D87F.</summary>
    Sbug = 0xd87f,
    /// <summary><c>EnemyHeaders_Sbug2</c> at $A0:D8BF.</summary>
    Sbug2 = 0xd8bf,
    /// <summary><c>EnemyHeaders_Mochtroid</c> at $A0:D8FF.</summary>
    /// <remarks>EnemyHeader_Mochtroid at $A3:D8FF.</remarks>
    Mochtroid = 0xd8ff,
    /// <summary><c>EnemyHeaders_Sidehopper</c> at $A0:D93F.</summary>
    Sidehopper = 0xd93f,
    /// <summary><c>EnemyHeaders_Dessgeega</c> at $A0:D97F.</summary>
    Dessgeega = 0xd97f,
    /// <summary><c>EnemyHeaders_SidehopperLarge</c> at $A0:D9BF.</summary>
    SidehopperLarge = 0xd9bf,
    /// <summary><c>EnemyHeaders_SidehopperTourian</c> at $A0:D9FF.</summary>
    SidehopperTourian = 0xd9ff,
    /// <summary><c>EnemyHeaders_DessgeegaLarge</c> at $A0:DA3F.</summary>
    DessgeegaLarge = 0xda3f,
    /// <summary><c>EnemyHeaders_Zoa</c> at $A0:DA7F.</summary>
    Zoa = 0xda7f,
    /// <summary><c>EnemyHeaders_Viola</c> at $A0:DABF.</summary>
    Viola = 0xdabf,
    /// <summary><c>EnemyHeaders_Respawn</c> at $A0:DAFF.</summary>
    /// <remarks>$A0:DAFF, EnemyHeaders_Respawn: reserves a killed actor's slot until respawn; its AI is inert.</remarks>
    Respawn = 0xdaff,
    /// <summary><c>EnemyHeaders_Bang</c> at $A0:DB3F.</summary>
    Bang = 0xdb3f,
    /// <summary><c>EnemyHeaders_Skree</c> at $A0:DB7F.</summary>
    Skree = 0xdb7f,
    /// <summary><c>EnemyHeaders_Yard</c> at $A0:DBBF.</summary>
    Yard = 0xdbbf,
    /// <summary><c>EnemyHeaders_Reflec</c> at $A0:DBFF.</summary>
    Reflec = 0xdbff,
    /// <summary><c>EnemyHeaders_HZoomer</c> at $A0:DC3F.</summary>
    HZoomer = 0xdc3f,
    /// <summary><c>EnemyHeaders_Zeela</c> at $A0:DC7F.</summary>
    Zeela = 0xdc7f,
    /// <summary><c>EnemyHeaders_Sova</c> at $A0:DCBF.</summary>
    Sova = 0xdcbf,
    /// <summary><c>EnemyHeaders_Zoomer</c> at $A0:DCFF.</summary>
    Zoomer = 0xdcff,
    /// <summary><c>EnemyHeaders_MZoomer</c> at $A0:DD3F.</summary>
    MZoomer = 0xdd3f,
    /// <summary><c>EnemyHeaders_Metroid</c> at $A0:DD7F.</summary>
    Metroid = 0xdd7f,
    /// <summary><c>EnemyHeaders_Crocomire</c> at $A0:DDBF.</summary>
    Crocomire = 0xddbf,
    /// <summary><c>EnemyHeaders_CrocomireTongue</c> at $A0:DDFF.</summary>
    CrocomireTongue = 0xddff,
    /// <summary><c>EnemyHeaders_DraygonBody</c> at $A0:DE3F.</summary>
    /// <remarks>$A0:DE3F, the Draygon body and encounter owner.</remarks>
    DraygonBody = 0xde3f,
    /// <summary><c>EnemyHeaders_DraygonEye</c> at $A0:DE7F.</summary>
    /// <remarks>$A0:DE7F, Draygon's eye actor.</remarks>
    DraygonEye = 0xde7f,
    /// <summary><c>EnemyHeaders_DraygonTail</c> at $A0:DEBF.</summary>
    /// <remarks>$A0:DEBF, Draygon's tail actor.</remarks>
    DraygonTail = 0xdebf,
    /// <summary><c>EnemyHeaders_DraygonArms</c> at $A0:DEFF.</summary>
    /// <remarks>$A0:DEFF, Draygon's arms actor.</remarks>
    DraygonArms = 0xdeff,
    /// <summary><c>EnemyHeaders_SporeSpawn</c> at $A0:DF3F.</summary>
    /// <remarks><c>EnemyHeaders_SporeSpawn</c> at <c>$A0:DF3F</c>; bank-relative body definition identity, distinct from stalk header $DF7F.</remarks>
    SporeSpawn = 0xdf3f,
    /// <summary><c>EnemyHeaders_SporeSpawnStalk</c> at $A0:DF7F.</summary>
    /// <remarks>EnemyHeaders_SporeSpawnStalk at $A0:DF7F. Spore impacts ($86:DC6D) roll their drops from this header's table, not from the Spore Spawn body header at $A0:DF3F.</remarks>
    SporeSpawnStalk = 0xdf7f,
    /// <summary><c>EnemyHeaders_Boulder</c> at $A0:DFBF.</summary>
    Boulder = 0xdfbf,
    /// <summary><c>EnemyHeaders_KzanTop</c> at $A0:DFFF.</summary>
    KzanTop = 0xdfff,
    /// <summary><c>EnemyHeaders_KzanBottom</c> at $A0:E03F.</summary>
    KzanBottom = 0xe03f,
    /// <summary><c>EnemyHeaders_Hibashi</c> at $A0:E07F.</summary>
    /// <remarks>Enemy definition $E07F (Hibashi) in bank $A6.</remarks>
    Hibashi = 0xe07f,
    /// <summary><c>EnemyHeaders_Puromi</c> at $A0:E0BF.</summary>
    /// <remarks>Enemy definition $E0BF (Puromi) in bank $A6.</remarks>
    Puromi = 0xe0bf,
    /// <summary><c>EnemyHeaders_MiniKraid</c> at $A0:E0FF.</summary>
    MiniKraid = 0xe0ff,
    /// <summary><c>EnemyHeaders_RidleyCeres</c> at $A0:E13F.</summary>
    /// <remarks>Ceres Ridley's shared bank-$A6 enemy header at $A0:E13F.</remarks>
    RidleyCeres = 0xe13f,
    /// <summary><c>EnemyHeaders_Ridley</c> at $A0:E17F.</summary>
    /// <remarks>Enemy header $A0:E17F, selecting the Lower Norfair boss branches of Ridley's shared bank-$A6 implementation.</remarks>
    Ridley = 0xe17f,
    /// <summary><c>EnemyHeaders_RidleyExplosion</c> at $A0:E1BF.</summary>
    /// <remarks><c>EnemyHeaders_RidleyExplosion</c> at <c>$A0:E1BF</c>.</remarks>
    RidleyExplosion = 0xe1bf,
    /// <summary><c>EnemyHeaders_Steam</c> at $A0:E1FF.</summary>
    /// <remarks>The Ceres steam enemy header at <c>$A0:E1FF</c>.</remarks>
    Steam = 0xe1ff,
    /// <summary><c>EnemyHeaders_CeresDoor</c> at $A0:E23F.</summary>
    /// <remarks><c>Enemy_CeresDoor</c>, the bank-$A6 enemy definition at $A6:E23F.</remarks>
    CeresDoor = 0xe23f,
    /// <summary><c>EnemyHeaders_Zebetite</c> at $A0:E27F.</summary>
    Zebetite = 0xe27f,
    /// <summary><c>EnemyHeaders_Kraid</c> at $A0:E2BF.</summary>
    Kraid = 0xe2bf,
    /// <summary><c>EnemyHeaders_KraidArm</c> at $A0:E2FF.</summary>
    KraidArm = 0xe2ff,
    /// <summary><c>EnemyHeaders_KraidLintTop</c> at $A0:E33F.</summary>
    KraidLintTop = 0xe33f,
    /// <summary><c>EnemyHeaders_KraidLintMiddle</c> at $A0:E37F.</summary>
    KraidLintMiddle = 0xe37f,
    /// <summary><c>EnemyHeaders_KraidLintBottom</c> at $A0:E3BF.</summary>
    KraidLintBottom = 0xe3bf,
    /// <summary><c>EnemyHeaders_KraidFoot</c> at $A0:E3FF.</summary>
    KraidFoot = 0xe3ff,
    /// <summary><c>EnemyHeaders_KraidNail</c> at $A0:E43F.</summary>
    KraidNail = 0xe43f,
    /// <summary><c>EnemyHeaders_KraidNailBad</c> at $A0:E47F.</summary>
    KraidNailBad = 0xe47f,
    /// <summary><c>EnemyHeaders_PhantoonBody</c> at $A0:E4BF.</summary>
    PhantoonBody = 0xe4bf,
    /// <summary><c>EnemyHeaders_PhantoonEye</c> at $A0:E4FF.</summary>
    PhantoonEye = 0xe4ff,
    /// <summary><c>EnemyHeaders_PhantoonTentacles</c> at $A0:E53F.</summary>
    PhantoonTentacles = 0xe53f,
    /// <summary><c>EnemyHeaders_PhantoonMouth</c> at $A0:E57F.</summary>
    PhantoonMouth = 0xe57f,
    /// <summary><c>EnemyHeaders_Etecoon</c> at $A0:E5BF.</summary>
    Etecoon = 0xe5bf,
    /// <summary><c>EnemyHeaders_Dachora</c> at $A0:E5FF.</summary>
    Dachora = 0xe5ff,
    /// <summary><c>EnemyHeaders_Evir</c> at $A0:E63F.</summary>
    Evir = 0xe63f,
    /// <summary><c>EnemyHeaders_EvirProjectile</c> at $A0:E67F.</summary>
    EvirProjectile = 0xe67f,
    /// <summary><c>EnemyHeaders_Eye</c> at $A0:E6BF.</summary>
    Eye = 0xe6bf,
    /// <summary><c>EnemyHeaders_Fune</c> at $A0:E6FF.</summary>
    /// <remarks>Enemy definition $E6FF (Fune) in bank $A0.</remarks>
    Fune = 0xe6ff,
    /// <summary><c>EnemyHeaders_Namihe</c> at $A0:E73F.</summary>
    /// <remarks>Enemy definition $E73F (Namihe) in bank $A0.</remarks>
    Namihe = 0xe73f,
    /// <summary><c>EnemyHeaders_Coven</c> at $A0:E77F.</summary>
    Coven = 0xe77f,
    /// <summary><c>EnemyHeaders_YappingMaw</c> at $A0:E7BF.</summary>
    YappingMaw = 0xe7bf,
    /// <summary><c>EnemyHeaders_Kago</c> at $A0:E7FF.</summary>
    Kago = 0xe7ff,
    /// <summary><c>EnemyHeaders_Magdollite</c> at $A0:E83F.</summary>
    Magdollite = 0xe83f,
    /// <summary><c>EnemyHeaders_Beetom</c> at $A0:E87F.</summary>
    Beetom = 0xe87f,
    /// <summary><c>EnemyHeaders_Powamp</c> at $A0:E8BF.</summary>
    Powamp = 0xe8bf,
    /// <summary><c>EnemyHeaders_Robot</c> at $A0:E8FF.</summary>
    Robot = 0xe8ff,
    /// <summary><c>EnemyHeaders_RobotNoPower</c> at $A0:E93F.</summary>
    RobotNoPower = 0xe93f,
    /// <summary><c>EnemyHeaders_Bull</c> at $A0:E97F.</summary>
    Bull = 0xe97f,
    /// <summary><c>EnemyHeaders_Alcoon</c> at $A0:E9BF.</summary>
    Alcoon = 0xe9bf,
    /// <summary><c>EnemyHeaders_Atomic</c> at $A0:E9FF.</summary>
    Atomic = 0xe9ff,
    /// <summary><c>EnemyHeaders_Spark</c> at $A0:EA3F.</summary>
    Spark = 0xea3f,
    /// <summary><c>EnemyHeaders_FaceBlock</c> at $A0:EA7F.</summary>
    FaceBlock = 0xea7f,
    /// <summary><c>EnemyHeaders_KihunterGreen</c> at $A0:EABF.</summary>
    KihunterGreen = 0xeabf,
    /// <summary><c>EnemyHeaders_KihunterGreenWings</c> at $A0:EAFF.</summary>
    KihunterGreenWings = 0xeaff,
    /// <summary><c>EnemyHeaders_KihunterYellow</c> at $A0:EB3F.</summary>
    KihunterYellow = 0xeb3f,
    /// <summary><c>EnemyHeaders_KihunterYellowWings</c> at $A0:EB7F.</summary>
    KihunterYellowWings = 0xeb7f,
    /// <summary><c>EnemyHeaders_KihunterRed</c> at $A0:EBBF.</summary>
    KihunterRed = 0xebbf,
    /// <summary><c>EnemyHeaders_KihunterRedWings</c> at $A0:EBFF.</summary>
    KihunterRedWings = 0xebff,
    /// <summary><c>EnemyHeaders_MotherBrainHead</c> at $A0:EC3F.</summary>
    MotherBrainHead = 0xec3f,
    /// <summary><c>EnemyHeaders_MotherBrainBody</c> at $A0:EC7F.</summary>
    MotherBrainBody = 0xec7f,
    /// <summary><c>EnemyHeaders_BabyMetroidCutscene</c> at $A0:ECBF.</summary>
    /// <remarks>The cutscene Baby Metroid enemy header at <c>$A0:ECBF</c>.</remarks>
    BabyMetroidCutscene = 0xecbf,
    /// <summary><c>EnemyHeaders_MotherBrainTubes</c> at $A0:ECFF.</summary>
    /// <remarks>Mother Brain's falling tube actor header at $A0:ECFF.</remarks>
    MotherBrainTubes = 0xecff,
    /// <summary><c>EnemyHeaders_CorpseTorizo</c> at $A0:ED3F.</summary>
    /// <remarks>Bank-$A0 enemy definition $ED3F for the Tourian Torizo corpse, initialized by $A9:D308 and required to own native enemy slot zero.</remarks>
    CorpseTorizo = 0xed3f,
    /// <summary><c>EnemyHeaders_CorpseSidehopper</c> at $A0:ED7F.</summary>
    /// <remarks>Bank-$A0 enemy definition $ED7F for the dead-sidehopper family, initialized by $A9:D7B6 as an initially alive victim or an already dead Tourian corpse.</remarks>
    CorpseSidehopper = 0xed7f,
    /// <summary><c>EnemyHeaders_CorpseSidehopper2</c> at $A0:EDBF.</summary>
    CorpseSidehopper2 = 0xedbf,
    /// <summary><c>EnemyHeaders_CorpseZoomer</c> at $A0:EDFF.</summary>
    /// <remarks>$A0:EDFF, EnemyHeaders_CorpseZoomer: dead Zoomer header initialized by $A9:D849, with parameter-1 variants 0, 2, and 4 sharing the corpse-rotting engine.</remarks>
    CorpseZoomer = 0xedff,
    /// <summary><c>EnemyHeaders_CorpseRipper</c> at $A0:EE3F.</summary>
    /// <remarks>$A0:EE3F, EnemyHeaders_CorpseRipper: dead Ripper header initialized by $A9:D876, with parameter-1 variants 0 and 2 sharing the corpse-rotting engine.</remarks>
    CorpseRipper = 0xee3f,
    /// <summary><c>EnemyHeaders_CorpseSkree</c> at $A0:EE7F.</summary>
    /// <remarks>$A0:EE7F, EnemyHeaders_CorpseSkree: dead Skree header initialized by $A9:D89F, with parameter-1 variants 0, 2, and 4 sharing the corpse-rotting engine.</remarks>
    CorpseSkree = 0xee7f,
    /// <summary><c>EnemyHeaders_BabyMetroid</c> at $A0:EEBF.</summary>
    /// <remarks>Bank-$A9 enemy-definition pointer identifying the retail Shitroid actor.</remarks>
    BabyMetroid = 0xeebf,
    /// <summary><c>EnemyHeaders_BombTorizo</c> at $A0:EEFF.</summary>
    BombTorizo = 0xeeff,
    /// <summary><c>EnemyHeaders_BombTorizoOrb</c> at $A0:EF3F.</summary>
    /// <remarks><c>EnemyHeaders_BombTorizoOrb</c> at $A0:EF3F.</remarks>
    BombTorizoOrb = 0xef3f,
    /// <summary><c>EnemyHeaders_GoldenTorizo</c> at $A0:EF7F.</summary>
    GoldenTorizo = 0xef7f,
    /// <summary><c>EnemyHeaders_GoldenTorizoOrb</c> at $A0:EFBF.</summary>
    /// <remarks><c>EnemyHeaders_GoldenTorizoOrb</c> at $A0:EFBF.</remarks>
    GoldenTorizoOrb = 0xefbf,
    /// <summary><c>EnemyHeaders_TourianStatue</c> at $A0:EFFF.</summary>
    TourianStatue = 0xefff,
    /// <summary><c>EnemyHeaders_TourianStatueGhost</c> at $A0:F03F.</summary>
    TourianStatueGhost = 0xf03f,
    /// <summary><c>EnemyHeaders_Shaktool</c> at $A0:F07F.</summary>
    Shaktool = 0xf07f,
    /// <summary><c>EnemyHeaders_NoobTubeCrack</c> at $A0:F0BF.</summary>
    NoobTubeCrack = 0xf0bf,
    /// <summary><c>EnemyHeaders_Chozo</c> at $A0:F0FF.</summary>
    /// <remarks><c>$A0:F0FF</c>, the shared Chozo statue enemy definition.</remarks>
    Chozo = 0xf0ff,
    /// <summary><c>UNUSED_EnemyHeaders_SpinningTurtleEye_A0F153</c> at $A0:F153.</summary>
    UnusedSpinningTurtleEye = 0xf153,
    /// <summary><c>EnemyHeaders_Zeb</c> at $A0:F193.</summary>
    /// <remarks>Normal Brinstar Pipe Bug enemy header at <c>$A0:F193</c>.</remarks>
    Zeb = 0xf193,
    /// <summary><c>EnemyHeaders_Zebbo</c> at $A0:F1D3.</summary>
    /// <remarks>Strong Brinstar Pipe Bug enemy header at <c>$A0:F1D3</c>.</remarks>
    Zebbo = 0xf1d3,
    /// <summary><c>EnemyHeaders_Gamet</c> at $A0:F213.</summary>
    /// <remarks>Norfair Pipe Bug enemy header at <c>$A0:F213</c>.</remarks>
    Gamet = 0xf213,
    /// <summary><c>EnemyHeaders_Geega</c> at $A0:F253.</summary>
    /// <remarks>Yellow Brinstar Pipe Bug enemy header at <c>$A0:F253</c>.</remarks>
    Geega = 0xf253,
    /// <summary><c>EnemyHeaders_Botwoon</c> at $A0:F293.</summary>
    Botwoon = 0xf293,
    /// <summary><c>EnemyHeaders_EtecoonEscape</c> at $A0:F2D3.</summary>
    EtecoonEscape = 0xf2d3,
    /// <summary><c>EnemyHeaders_DachoraEscape</c> at $A0:F313.</summary>
    DachoraEscape = 0xf313,
    /// <summary><c>EnemyHeaders_PirateGreyWall</c> at $A0:F353.</summary>
    PirateGreyWall = 0xf353,
    /// <summary><c>EnemyHeaders_PirateGreenWall</c> at $A0:F393.</summary>
    PirateGreenWall = 0xf393,
    /// <summary><c>EnemyHeaders_PirateRedWall</c> at $A0:F3D3.</summary>
    PirateRedWall = 0xf3d3,
    /// <summary><c>EnemyHeaders_PirateGoldWall</c> at $A0:F413.</summary>
    /// <remarks>Enemy $F413's palette header resolves to $B2:8727. Ninja Pirate initialization copies this same 16-color source to its target OBJ palette, irrespective of the ninja actor's own definition or starting color.</remarks>
    PirateGoldWall = 0xf413,
    /// <summary><c>EnemyHeaders_PirateMagentaWall</c> at $A0:F453.</summary>
    PirateMagentaWall = 0xf453,
    /// <summary><c>EnemyHeaders_PirateSilverWall</c> at $A0:F493.</summary>
    PirateSilverWall = 0xf493,
    /// <summary><c>EnemyHeaders_PirateGreyNinja</c> at $A0:F4D3.</summary>
    PirateGreyNinja = 0xf4d3,
    /// <summary><c>EnemyHeaders_PirateGreenNinja</c> at $A0:F513.</summary>
    PirateGreenNinja = 0xf513,
    /// <summary><c>EnemyHeaders_PirateRedNinja</c> at $A0:F553.</summary>
    PirateRedNinja = 0xf553,
    /// <summary><c>EnemyHeaders_PirateGoldNinja</c> at $A0:F593.</summary>
    PirateGoldNinja = 0xf593,
    /// <summary><c>EnemyHeaders_PirateMagentaNinja</c> at $A0:F5D3.</summary>
    PirateMagentaNinja = 0xf5d3,
    /// <summary><c>EnemyHeaders_PirateSilverNinja</c> at $A0:F613.</summary>
    PirateSilverNinja = 0xf613,
    /// <summary><c>EnemyHeaders_PirateGreyWalking</c> at $A0:F653.</summary>
    PirateGreyWalking = 0xf653,
    /// <summary><c>EnemyHeaders_PirateGreenWalking</c> at $A0:F693.</summary>
    PirateGreenWalking = 0xf693,
    /// <summary><c>EnemyHeaders_PirateRedWalking</c> at $A0:F6D3.</summary>
    PirateRedWalking = 0xf6d3,
    /// <summary><c>EnemyHeaders_PirateGoldWalking</c> at $A0:F713.</summary>
    PirateGoldWalking = 0xf713,
    /// <summary><c>EnemyHeaders_PirateMagentaWalking</c> at $A0:F753.</summary>
    PirateMagentaWalking = 0xf753,
    /// <summary><c>EnemyHeaders_PirateSilverWalking</c> at $A0:F793.</summary>
    PirateSilverWalking = 0xf793,
}

/// <summary>Decoding for <see cref="EnemyDefinitionId"/>.</summary>
public static class EnemyDefinitionIds
{
    /// <summary>The species a stored bank-$A0 header pointer names.</summary>
    /// <exception cref="InvalidDataException">The pointer is not a labelled enemy header.</exception>
    public static EnemyDefinitionId FromHeaderPointer(ushort pointer)
    {
        var id = (EnemyDefinitionId)pointer;
        return id != EnemyDefinitionId.None && Enum.IsDefined(id)
            ? id
            : throw new InvalidDataException($"${pointer:X4} is not a bank-$A0 enemy header.");
    }
}
