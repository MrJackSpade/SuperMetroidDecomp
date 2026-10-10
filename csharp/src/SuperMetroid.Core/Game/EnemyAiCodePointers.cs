namespace SuperMetroid.Core.Game;

/// <summary>
/// The 24-bit initialization and main AI routines the generic enemy runtime dispatches,
/// valued by their native long address (bank in bits 16..23).
/// </summary>
internal enum EnemyAiRoutine
{
    /// <summary><c>InitAI_Crocomire</c> at $A4:8A5A.</summary>
    InitAI_Crocomire = 0xa48a5a,

    /// <summary><c>InitAI_SporeSpawn</c> at $A5:EA2A.</summary>
    InitAI_SporeSpawn = 0xa5ea2a,

    /// <summary><c>InitAI_CrocomireTongue</c> at $A4:F67A.</summary>
    InitAI_CrocomireTongue = 0xa4f67a,

    /// <summary><c>InitAI_Magdollite</c> at $A8:AF8B.</summary>
    InitAI_Magdollite = 0xa8af8b,

    /// <summary><c>InitAI_ShipTop</c> at $A2:A644.</summary>
    InitAI_ShipTop = 0xa2a644,

    /// <summary><c>InitAI_Boyon</c> at $A2:871C.</summary>
    InitAI_Boyon = 0xa2871c,

    /// <summary><c>InitAI_Stoke</c> at $A2:89AD.</summary>
    InitAI_Stoke = 0xa289ad,

    /// <summary><c>InitAI_MamaTurtle</c> at $A2:8D6C.</summary>
    InitAI_MamaTurtle = 0xa28d6c,

    /// <summary><c>InitAI_BabyTurtle</c> at $A2:8D9D.</summary>
    InitAI_BabyTurtle = 0xa28d9d,

    /// <summary><c>InitAI_Puyo</c> at $A2:9A3F.</summary>
    InitAI_Puyo = 0xa29a3f,

    /// <summary><c>InitAI_Cacatac</c> at $A2:9F48.</summary>
    InitAI_Cacatac = 0xa29f48,

    /// <summary><c>InitAI_Owtch</c> at $A2:A3F9.</summary>
    InitAI_Owtch = 0xa2a3f9,

    /// <summary><c>InitAI_Multiviola</c> at $A2:B3E0.</summary>
    InitAI_Multiviola = 0xa2b3e0,

    /// <summary><c>InitAI_Polyp</c> at $A2:B570.</summary>
    InitAI_Polyp = 0xa2b570,

    /// <summary><c>InitAI_Rinka</c> at $A2:B602.</summary>
    InitAI_Rinka = 0xa2b602,

    /// <summary><c>InitAI_Rio</c> at $A2:BBCD.</summary>
    InitAI_Rio = 0xa2bbcd,

    /// <summary><c>InitAI_Squeept</c> at $A2:BE99.</summary>
    InitAI_Squeept = 0xa2be99,

    /// <summary><c>InitAI_Geruta</c> at $A2:C242.</summary>
    InitAI_Geruta = 0xa2c242,

    /// <summary><c>InitAI_Holtz</c> at $A2:C6F3.</summary>
    InitAI_Holtz = 0xa2c6f3,

    /// <summary><c>InitAI_Oum</c> at $A2:CCD4.</summary>
    InitAI_Oum = 0xa2ccd4,

    /// <summary><c>InitAI_GRipper</c> at $A2:E1D3.</summary>
    InitAI_GRipper = 0xa2e1d3,

    /// <summary><c>InitAI_Ripper2</c> at $A2:E318.</summary>
    InitAI_Ripper2 = 0xa2e318,

    /// <summary><c>InitAI_Dragon</c> at $A2:E606.</summary>
    InitAI_Dragon = 0xa2e606,

    /// <summary><c>InitAI_ShutterGrowing</c> at $A2:E9DA.</summary>
    InitAI_ShutterGrowing = 0xa2e9da,

    /// <summary><c>InitAI_ShutterShootable_ShutterDestroyable</c> at $A2:EE12.</summary>
    InitAI_ShutterShootable_ShutterDestroyable = 0xa2ee12,

    /// <summary><c>InitAI_Kamer</c> at $A2:EE05.</summary>
    InitAI_Kamer = 0xa2ee05,

    /// <summary><c>InitAI_ShutterHorizShootable</c> at $A2:F111.</summary>
    InitAI_ShutterHorizShootable = 0xa2f111,

    /// <summary><c>InitAI_Elevator</c> at $A3:94E6.</summary>
    InitAI_Elevator = 0xa394e6,

    /// <summary><c>InitAI_Fune_Namihe</c> at $A8:96E3.</summary>
    InitAI_Fune_Namihe = 0xa896e3,

    /// <summary><c>InitAI_ShipBottomEntrance</c> at $A2:A6D2.</summary>
    InitAI_ShipBottomEntrance = 0xa2a6d2,

    /// <summary><c>InitAI_CeresSteam</c> at $A6:EFB1.</summary>
    InitAI_CeresSteam = 0xa6efb1,

    /// <summary><c>InitAI_CeresDoor</c> at $A6:F6C5.</summary>
    InitAI_CeresDoor = 0xa6f6c5,

    /// <summary><c>InitAI_Ridley</c> at $A6:A0F5.</summary>
    InitAI_Ridley = 0xa6a0f5,

    /// <summary><c>InitAI_RidleyExplosion</c> at $A6:C696.</summary>
    InitAI_RidleyExplosion = 0xa6c696,

    /// <summary><c>InitAI_Boulder</c> at $A6:86F5.</summary>
    InitAI_Boulder = 0xa686f5,

    /// <summary><c>InitAI_Zebetite</c> at $A6:FB72.</summary>
    InitAI_Zebetite = 0xa6fb72,

    /// <summary><c>InitAI_Etecoon</c> at $A7:E912.</summary>
    InitAI_Etecoon = 0xa7e912,

    /// <summary><c>InitAI_Dachora</c> at $A7:F4DD.</summary>
    InitAI_Dachora = 0xa7f4dd,

    /// <summary><c>InitAI_Evir</c> at $A8:87E0.</summary>
    InitAI_Evir = 0xa887e0,

    /// <summary><c>InitAI_EvirProjectile</c> at $A8:88B0.</summary>
    InitAI_EvirProjectile = 0xa888b0,

    /// <summary><c>InitAI_Eye</c> at $A8:9058.</summary>
    InitAI_Eye = 0xa89058,

    /// <summary><c>InitAI_Coven</c> at $A8:9AEE.</summary>
    InitAI_Coven = 0xa89aee,

    /// <summary><c>InitAI_YappingMaw</c> at $A8:A148.</summary>
    InitAI_YappingMaw = 0xa8a148,

    /// <summary><c>InitAI_Ripper</c> at $A2:E49F.</summary>
    InitAI_Ripper = 0xa2e49f,

    /// <summary><c>InitAI_Choot</c> at $A2:DF76.</summary>
    InitAI_Choot = 0xa2df76,

    /// <summary><c>InitAI_Sciser</c> at $A3:96E3.</summary>
    InitAI_Sciser = 0xa396e3,

    /// <summary><c>InitAI_Zero</c> at $A3:993B.</summary>
    InitAI_Zero = 0xa3993b,

    /// <summary><c>InitAI_Viola</c> at $A3:B66F.</summary>
    InitAI_Viola = 0xa3b66f,

    /// <summary><c>InitAI_Zeela</c> at $A3:E2D4.</summary>
    InitAI_Zeela = 0xa3e2d4,

    /// <summary><c>InitAI_Sova</c> at $A3:E59C.</summary>
    InitAI_Sova = 0xa3e59c,

    /// <summary><c>InitAI_Zoomer_MZoomer</c> at $A3:E669.</summary>
    InitAI_Zoomer_MZoomer = 0xa3e669,

    /// <summary><c>InitAI_HZoomer</c> at $A3:E043.</summary>
    InitAI_HZoomer = 0xa3e043,

    /// <summary><c>InitAI_Skree</c> at $A3:C6AE.</summary>
    InitAI_Skree = 0xa3c6ae,

    /// <summary><c>InitAI_Mellow_Mella_Menu</c> at $A2:B06B.</summary>
    InitAI_Mellow_Mella_Menu = 0xa2b06b,

    /// <summary><c>InitAI_Sbug</c> at $A3:A14D.</summary>
    InitAI_Sbug = 0xa3a14d,

    /// <summary><c>InitAI_Mochtroid</c> at $A3:A77D.</summary>
    InitAI_Mochtroid = 0xa3a77d,

    /// <summary><c>InitAI_Metroid</c> at $A3:EA4F.</summary>
    InitAI_Metroid = 0xa3ea4f,

    /// <summary><c>InitAI_Hopper</c> at $A3:AB09.</summary>
    InitAI_Hopper = 0xa3ab09,

    /// <summary><c>InitAI_Zoa</c> at $A3:B44A.</summary>
    InitAI_Zoa = 0xa3b44a,

    /// <summary><c>InitAI_Yard</c> at $A3:CDE2.</summary>
    InitAI_Yard = 0xa3cde2,

    /// <summary><c>InitAI_Waver</c> at $A3:86ED.</summary>
    InitAI_Waver = 0xa386ed,

    /// <summary><c>InitAI_Metaree</c> at $A3:8960.</summary>
    InitAI_Metaree = 0xa38960,

    /// <summary><c>InitAI_Fireflea</c> at $A3:8D2D.</summary>
    InitAI_Fireflea = 0xa38d2d,

    /// <summary><c>InitAI_Skultera</c> at $A3:90B5.</summary>
    InitAI_Skultera = 0xa390b5,

    /// <summary><c>InitAI_Kamer2</c> at $A3:9C9F.</summary>
    InitAI_Kamer2 = 0xa39c9f,

    /// <summary><c>InitAI_Tripper</c> at $A3:9CBA.</summary>
    InitAI_Tripper = 0xa39cba,

    /// <summary><c>InitAI_Alcoon</c> at $A8:DCCD.</summary>
    InitAI_Alcoon = 0xa8dccd,

    /// <summary><c>InitAI_Kago</c> at $A8:AB46.</summary>
    InitAI_Kago = 0xa8ab46,

    /// <summary><c>InitAI_Beetom</c> at $A8:B776.</summary>
    InitAI_Beetom = 0xa8b776,

    /// <summary><c>InitAI_Powamp</c> at $A8:C1C9.</summary>
    InitAI_Powamp = 0xa8c1c9,

    /// <summary><c>InitAI_Robot</c> at $A8:CB77.</summary>
    InitAI_Robot = 0xa8cb77,

    /// <summary><c>InitAI_RobotNoPower</c> at $A8:CBCC.</summary>
    InitAI_RobotNoPower = 0xa8cbcc,

    /// <summary><c>InitAI_Bull</c> at $A8:D8C9.</summary>
    InitAI_Bull = 0xa8d8c9,

    /// <summary><c>InitAI_Atomic</c> at $A8:E388.</summary>
    InitAI_Atomic = 0xa8e388,

    /// <summary><c>InitAI_Spark</c> at $A8:E637.</summary>
    InitAI_Spark = 0xa8e637,

    /// <summary><c>InitAI_FaceBlock</c> at $A8:E82E.</summary>
    InitAI_FaceBlock = 0xa8e82e,

    /// <summary><c>InitAI_Kihunter</c> at $A8:F188.</summary>
    InitAI_Kihunter = 0xa8f188,

    /// <summary><c>InitAI_KihunterWings</c> at $A8:F214.</summary>
    InitAI_KihunterWings = 0xa8f214,

    /// <summary><c>InitAI_Zeb_Zebbo</c> at $B3:883B.</summary>
    InitAI_Zeb_Zebbo = 0xb3883b,

    /// <summary><c>InitAI_Gamet</c> at $B3:8B61.</summary>
    InitAI_Gamet = 0xb38b61,

    /// <summary><c>InitAI_Geega</c> at $B3:8F4C.</summary>
    InitAI_Geega = 0xb38f4c,

    /// <summary><c>InitAI_Botwoon</c> at $B3:9583.</summary>
    InitAI_Botwoon = 0xb39583,

    /// <summary><c>InitAI_EtecoonEscape</c> at $B3:E6CB.</summary>
    InitAI_EtecoonEscape = 0xb3e6cb,

    /// <summary><c>InitAI_DachoraEscape</c> at $B3:EAE5.</summary>
    InitAI_DachoraEscape = 0xb3eae5,

    /// <summary><c>InitAI_KzanTop</c> at $A6:8B2F.</summary>
    InitAI_KzanTop = 0xa68b2f,

    /// <summary><c>InitAI_KzanBottom</c> at $A6:8B85.</summary>
    InitAI_KzanBottom = 0xa68b85,

    /// <summary><c>InitAI_Hibashi</c> at $A6:8FFC.</summary>
    InitAI_Hibashi = 0xa68ffc,

    /// <summary><c>InitAI_Puromi</c> at $A6:94C4.</summary>
    InitAI_Puromi = 0xa694c4,

    /// <summary><c>InitAI_MiniKraid</c> at $A6:9A58.</summary>
    InitAI_MiniKraid = 0xa69a58,

    /// <summary><c>InitAI_PirateWalking</c> at $B2:FD02.</summary>
    InitAI_PirateWalking = 0xb2fd02,

    /// <summary><c>InitAI_PirateWall</c> at $B2:EF9F.</summary>
    InitAI_PirateWall = 0xb2ef9f,

    /// <summary><c>InitAI_PirateNinja</c> at $B2:F5DE.</summary>
    InitAI_PirateNinja = 0xb2f5de,

    /// <summary><c>InitAI_Torizo</c> at $AA:C87F.</summary>
    InitAI_Torizo = 0xaac87f,

    /// <summary><c>InitAI_Kraid</c> at $A7:A959.</summary>
    InitAI_Kraid = 0xa7a959,

    /// <summary><c>InitAI_KraidArm</c> at $A7:AB43.</summary>
    InitAI_KraidArm = 0xa7ab43,

    /// <summary><c>InitAI_KraidLintTop</c> at $A7:AB68.</summary>
    InitAI_KraidLintTop = 0xa7ab68,

    /// <summary><c>InitAI_KraidLintMiddle</c> at $A7:AB9C.</summary>
    InitAI_KraidLintMiddle = 0xa7ab9c,

    /// <summary><c>InitAI_KraidLintBottom</c> at $A7:ABCA.</summary>
    InitAI_KraidLintBottom = 0xa7abca,

    /// <summary><c>InitAI_KraidFoot</c> at $A7:ABF8.</summary>
    InitAI_KraidFoot = 0xa7abf8,

    /// <summary><c>InitAI_KraidNail</c> at $A7:BCEF.</summary>
    InitAI_KraidNail = 0xa7bcef,

    /// <summary><c>InitAI_KraidNailBad</c> at $A7:BD2D.</summary>
    InitAI_KraidNailBad = 0xa7bd2d,

    /// <summary><c>InitAI_PhantoonBody</c> at $A7:CDF3.</summary>
    InitAI_PhantoonBody = 0xa7cdf3,

    /// <summary><c>InitAI_Phantoon_Eye_Tentacles_Mouth</c> at $A7:CE55.</summary>
    InitAI_Phantoon_Eye_Tentacles_Mouth = 0xa7ce55,

    /// <summary><c>InitAI_DraygonBody</c> at $A5:8687.</summary>
    InitAI_DraygonBody = 0xa58687,

    /// <summary><c>InitAI_DraygonEye</c> at $A5:C46B.</summary>
    InitAI_DraygonEye = 0xa5c46b,

    /// <summary><c>InitAI_DraygonTail</c> at $A5:C599.</summary>
    InitAI_DraygonTail = 0xa5c599,

    /// <summary><c>InitAI_DraygonArms</c> at $A5:C5AD.</summary>
    InitAI_DraygonArms = 0xa5c5ad,

    /// <summary><c>InitAI_MotherBrainBody</c> at $A9:8687.</summary>
    InitAI_MotherBrainBody = 0xa98687,

    /// <summary><c>InitAI_MotherBrainHead</c> at $A9:8705.</summary>
    InitAI_MotherBrainHead = 0xa98705,

    /// <summary><c>InitAI_MotherBrainTubes</c> at $A9:8B35.</summary>
    InitAI_MotherBrainTubes = 0xa98b35,

    /// <summary><c>InitAI_BabyMetroidCutscene</c> at $A9:C710.</summary>
    InitAI_BabyMetroidCutscene = 0xa9c710,

    /// <summary><c>InitAI_CorpseTorizo</c> at $A9:D308.</summary>
    InitAI_CorpseTorizo = 0xa9d308,

    /// <summary><c>InitAI_CorpseSidehopper</c> at $A9:D7B6.</summary>
    InitAI_CorpseSidehopper = 0xa9d7b6,

    /// <summary><c>InitAI_CorpseZoomer</c> at $A9:D849.</summary>
    InitAI_CorpseZoomer = 0xa9d849,

    /// <summary><c>InitAI_CorpseRipper</c> at $A9:D876.</summary>
    InitAI_CorpseRipper = 0xa9d876,

    /// <summary><c>InitAI_CorpseSkree</c> at $A9:D89F.</summary>
    InitAI_CorpseSkree = 0xa9d89f,

    /// <summary><c>InitAI_BabyMetroid</c> at $A9:EF37.</summary>
    InitAI_BabyMetroid = 0xa9ef37,

    /// <summary><c>InitAI_TourianStatue</c> at $AA:D7C8.</summary>
    InitAI_TourianStatue = 0xaad7c8,

    /// <summary><c>InitAI_Shaktool</c> at $AA:DE43.</summary>
    InitAI_Shaktool = 0xaade43,

    /// <summary><c>InitAI_NoobTubeCrack</c> at $AA:E716.</summary>
    InitAI_NoobTubeCrack = 0xaae716,

    /// <summary><c>InitAI_Chozo</c> at $AA:E725.</summary>
    InitAI_Chozo = 0xaae725,

    /// <summary><c>RTL_A2804C</c> at $A2:804C.</summary>
    RTL_A2804C = 0xa2804c,

    /// <summary><c>RTL_A3804C</c> at $A3:804C; respawn-placeholder main/hurt/frozen AI.</summary>
    RTL_A3804C = 0xa3804c,

    /// <summary><c>MainAI_DraygonBody</c> at $A5:86FC.</summary>
    MainAI_DraygonBody = 0xa586fc,

    /// <summary><c>MainAI_DraygonEye</c> at $A5:C486.</summary>
    MainAI_DraygonEye = 0xa5c486,

    /// <summary><c>RTL_A5C5AA</c> at $A5:C5AA.</summary>
    RTL_A5C5AA = 0xa5c5aa,

    /// <summary><c>RTL_A5C5C4</c> at $A5:C5C4.</summary>
    RTL_A5C5C4 = 0xa5c5c4,

    /// <summary><c>MainAI_HurtAI_MotherBrainBody</c> at $A9:873E.</summary>
    MainAI_HurtAI_MotherBrainBody = 0xa9873e,

    /// <summary><c>MainAI_HurtAI_MotherBrainHead</c> at $A9:878B.</summary>
    MainAI_HurtAI_MotherBrainHead = 0xa9878b,

    /// <summary><c>MainAI_MotherBrainTubes</c> at $A9:8B85.</summary>
    MainAI_MotherBrainTubes = 0xa98b85,

    /// <summary><c>MainAI_BabyMetroidCutscene</c> at $A9:C779.</summary>
    MainAI_BabyMetroidCutscene = 0xa9c779,

    /// <summary><c>MainAI_CorpseTorizo</c> at $A9:D368.</summary>
    MainAI_CorpseTorizo = 0xa9d368,

    /// <summary><c>MainAI_HurtAI_CorpseEnemies</c> at $A9:D8DB.</summary>
    MainAI_HurtAI_CorpseEnemies = 0xa9d8db,

    /// <summary><c>MainAI_BabyMetroid</c> at $A9:EFC5.</summary>
    MainAI_BabyMetroid = 0xa9efc5,

    /// <summary><c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    MainAI_Crocomire = 0xa48c04,

    /// <summary><c>MainAI_SporeSpawn</c> at $A5:EB13.</summary>
    MainAI_SporeSpawn = 0xa5eb13,

    /// <summary><c>MainAI_CrocomireTongue</c> at $A4:F6BB.</summary>
    MainAI_CrocomireTongue = 0xa4f6bb,

    /// <summary><c>MainAI_Magdollite</c> at $A8:B10A.</summary>
    MainAI_Magdollite = 0xa8b10a,

    /// <summary><c>MainAI_ShipTop</c> at $A2:A759.</summary>
    MainAI_ShipTop = 0xa2a759,

    /// <summary><c>MainAI_Boyon</c> at $A2:879C.</summary>
    MainAI_Boyon = 0xa2879c,

    /// <summary><c>MainAI_Stoke</c> at $A2:89F0.</summary>
    MainAI_Stoke = 0xa289f0,

    /// <summary><c>MainAI_MamaTurtle</c> at $A2:8DD2.</summary>
    MainAI_MamaTurtle = 0xa28dd2,

    /// <summary><c>MainAI_BabyTurtle</c> at $A2:912E.</summary>
    MainAI_BabyTurtle = 0xa2912e,

    /// <summary><c>MainAI_Puyo</c> at $A2:9A7D.</summary>
    MainAI_Puyo = 0xa29a7d,

    /// <summary><c>MainAI_Cacatac</c> at $A2:9FB3.</summary>
    MainAI_Cacatac = 0xa29fb3,

    /// <summary><c>MainAI_Owtch</c> at $A2:A47E.</summary>
    MainAI_Owtch = 0xa2a47e,

    /// <summary><c>MainAI_Multiviola</c> at $A2:B40F.</summary>
    MainAI_Multiviola = 0xa2b40f,

    /// <summary><c>MainAI_Polyp</c> at $A2:B58F.</summary>
    MainAI_Polyp = 0xa2b58f,

    /// <summary><c>MainAI_Rinka</c> at $A2:B7C4.</summary>
    MainAI_Rinka = 0xa2b7c4,

    /// <summary><c>MainAI_Rio</c> at $A2:BBE3.</summary>
    MainAI_Rio = 0xa2bbe3,

    /// <summary><c>MainAI_Squeept</c> at $A2:BED2.</summary>
    MainAI_Squeept = 0xa2bed2,

    /// <summary><c>MainAI_Geruta</c> at $A2:C277.</summary>
    MainAI_Geruta = 0xa2c277,

    /// <summary><c>MainAI_Holtz</c> at $A2:C724.</summary>
    MainAI_Holtz = 0xa2c724,

    /// <summary><c>MainAI_Oum</c> at $A2:CD13.</summary>
    MainAI_Oum = 0xa2cd13,

    /// <summary><c>MainAI_GRipper</c> at $A2:E221.</summary>
    MainAI_GRipper = 0xa2e221,

    /// <summary><c>MainAI_Ripper2</c> at $A2:E353.</summary>
    MainAI_Ripper2 = 0xa2e353,

    /// <summary><c>MainAI_Dragon</c> at $A2:E64E.</summary>
    MainAI_Dragon = 0xa2e64e,

    /// <summary><c>MainAI_ShutterGrowing</c> at $A2:EAB6.</summary>
    MainAI_ShutterGrowing = 0xa2eab6,

    /// <summary><c>MainAI_ShutterShootable_ShutterDestroyable_Kamer</c> at $A2:EED1.</summary>
    MainAI_ShutterShootable_ShutterDestroyable_Kamer = 0xa2eed1,

    /// <summary><c>MainAI_ShutterHorizShootable</c> at $A2:F1DE.</summary>
    MainAI_ShutterHorizShootable = 0xa2f1de,

    /// <summary><c>MainAI_GrappleAI_FrozenAI_Elevator</c> at $A3:952A.</summary>
    MainAI_GrappleAI_FrozenAI_Elevator = 0xa3952a,

    /// <summary><c>MainAI_Fune_Namihe</c> at $A8:9730.</summary>
    MainAI_Fune_Namihe = 0xa89730,

    /// <summary><c>MainAI_Kago</c> at $A8:AB75.</summary>
    MainAI_Kago = 0xa8ab75,

    /// <summary><c>RTL_A7804C</c> at $A7:804C.</summary>
    RTL_A7804C = 0xa7804c,

    /// <summary><c>MainAI_CeresSteam</c> at $A6:F00D.</summary>
    MainAI_CeresSteam = 0xa6f00d,

    /// <summary><c>MainAI_CeresDoor</c> at $A6:F765.</summary>
    MainAI_CeresDoor = 0xa6f765,

    /// <summary><c>MainAI_RidleyCeres</c> at $A6:A288.</summary>
    MainAI_RidleyCeres = 0xa6a288,

    /// <summary><c>MainAI_Ridley</c> at $A6:B227.</summary>
    MainAI_Ridley = 0xa6b227,

    /// <summary><c>MainAI_RidleyExplosion</c> at $A6:C8D4.</summary>
    MainAI_RidleyExplosion = 0xa6c8d4,

    /// <summary><c>MainAI_Boulder</c> at $A6:8793.</summary>
    MainAI_Boulder = 0xa68793,

    /// <summary><c>MainAI_Zebetite</c> at $A6:FC33.</summary>
    MainAI_Zebetite = 0xa6fc33,

    /// <summary><c>MainAI_Etecoon</c> at $A7:E940.</summary>
    MainAI_Etecoon = 0xa7e940,

    /// <summary><c>MainAI_Dachora</c> at $A7:F52E.</summary>
    MainAI_Dachora = 0xa7f52e,

    /// <summary><c>MainAI_Evir</c> at $A8:891B.</summary>
    MainAI_Evir = 0xa8891b,

    /// <summary><c>MainAI_EvirProjectile</c> at $A8:899E.</summary>
    MainAI_EvirProjectile = 0xa8899e,

    /// <summary><c>MainAI_Eye</c> at $A8:90E2.</summary>
    MainAI_Eye = 0xa890e2,

    /// <summary><c>MainAI_Coven</c> at $A8:9B3C.</summary>
    MainAI_Coven = 0xa89b3c,

    /// <summary><c>MainAI_YappingMaw</c> at $A8:A211.</summary>
    MainAI_YappingMaw = 0xa8a211,

    /// <summary><c>MainAI_Ripper</c> at $A2:E4DA.</summary>
    MainAI_Ripper = 0xa2e4da,

    /// <summary><c>MainAI_Choot</c> at $A2:E02E.</summary>
    MainAI_Choot = 0xa2e02e,

    /// <summary><c>MainAI_Crawlers</c> at $A3:E6C2.</summary>
    MainAI_Crawlers = 0xa3e6c2,

    /// <summary><c>MainAI_HZoomer</c> at $A3:E08B.</summary>
    MainAI_HZoomer = 0xa3e08b,

    /// <summary><c>MainAI_Skree</c> at $A3:C6C7.</summary>
    MainAI_Skree = 0xa3c6c7,

    /// <summary><c>MainAI_Mellow_Mella_Menu</c> at $A2:B11F.</summary>
    MainAI_Mellow_Mella_Menu = 0xa2b11f,

    /// <summary><c>MainAI_Sbug</c> at $A3:A2D0.</summary>
    MainAI_Sbug = 0xa3a2d0,

    /// <summary><c>MainAI_Mochtroid</c> at $A3:A790.</summary>
    MainAI_Mochtroid = 0xa3a790,

    /// <summary><c>MainAI_Metroid</c> at $A3:EB98.</summary>
    MainAI_Metroid = 0xa3eb98,

    /// <summary><c>MainAI_Hopper</c> at $A3:ABCF.</summary>
    MainAI_Hopper = 0xa3abcf,

    /// <summary><c>MainAI_Zoa</c> at $A3:B47C.</summary>
    MainAI_Zoa = 0xa3b47c,

    /// <summary><c>MainAI_Yard</c> at $A3:CE64.</summary>
    MainAI_Yard = 0xa3ce64,

    /// <summary><c>MainAI_Waver</c> at $A3:874C.</summary>
    MainAI_Waver = 0xa3874c,

    /// <summary><c>MainAI_Metaree</c> at $A3:8979.</summary>
    MainAI_Metaree = 0xa38979,

    /// <summary><c>MainAI_Fireflea</c> at $A3:8DEE.</summary>
    MainAI_Fireflea = 0xa38dee,

    /// <summary><c>MainAI_Skultera</c> at $A3:912B.</summary>
    MainAI_Skultera = 0xa3912b,

    /// <summary><c>MainAI_Tripper_Kamer2</c> at $A3:9D16.</summary>
    MainAI_Tripper_Kamer2 = 0xa39d16,

    /// <summary><c>MainAI_Alcoon</c> at $A8:DD6B.</summary>
    MainAI_Alcoon = 0xa8dd6b,

    /// <summary><c>MainAI_Beetom</c> at $A8:B80D.</summary>
    MainAI_Beetom = 0xa8b80d,

    /// <summary><c>MainAI_Powamp</c> at $A8:C21C.</summary>
    MainAI_Powamp = 0xa8c21c,

    /// <summary><c>MainAI_Robot</c> at $A8:CC36.</summary>
    MainAI_Robot = 0xa8cc36,

    /// <summary><c>RTL_A8CC66</c> at $A8:CC66.</summary>
    RTL_A8CC66 = 0xa8cc66,

    /// <summary><c>MainAI_Bull</c> at $A8:D90B.</summary>
    MainAI_Bull = 0xa8d90b,

    /// <summary><c>MainAI_Atomic</c> at $A8:E3C3.</summary>
    MainAI_Atomic = 0xa8e3c3,

    /// <summary><c>MainAI_Spark</c> at $A8:E68E.</summary>
    MainAI_Spark = 0xa8e68e,

    /// <summary><c>MainAI_FaceBlock</c> at $A8:E8AE.</summary>
    MainAI_FaceBlock = 0xa8e8ae,

    /// <summary><c>MainAI_Kihunter</c> at $A8:F25C.</summary>
    MainAI_Kihunter = 0xa8f25c,

    /// <summary><c>MainAI_KihunterWings</c> at $A8:F262.</summary>
    MainAI_KihunterWings = 0xa8f262,

    /// <summary><c>MainAI_Zeb_Zebbo</c> at $B3:887A.</summary>
    MainAI_Zeb_Zebbo = 0xb3887a,

    /// <summary><c>MainAI_Gamet</c> at $B3:8B9E.</summary>
    MainAI_Gamet = 0xb38b9e,

    /// <summary><c>MainAI_Geega</c> at $B3:8FAE.</summary>
    MainAI_Geega = 0xb38fae,

    /// <summary><c>MainAI_Botwoon</c> at $B3:9668.</summary>
    MainAI_Botwoon = 0xb39668,

    /// <summary><c>MainAI_EtecoonEscape</c> at $B3:E655.</summary>
    MainAI_EtecoonEscape = 0xb3e655,

    /// <summary><c>RTL_B3EB1A</c> at $B3:EB1A.</summary>
    RTL_B3EB1A = 0xb3eb1a,

    /// <summary><c>MainAI_KzanTop</c> at $A6:8BAD.</summary>
    MainAI_KzanTop = 0xa68bad,

    /// <summary><c>MainAI_KzanBottom</c> at $A6:8B99.</summary>
    MainAI_KzanBottom = 0xa68b99,

    /// <summary><c>MainAI_Hibashi</c> at $A6:9023.</summary>
    MainAI_Hibashi = 0xa69023,

    /// <summary><c>MainAI_Puromi</c> at $A6:960E.</summary>
    MainAI_Puromi = 0xa6960e,

    /// <summary><c>MainAI_MiniKraid</c> at $A6:9AC2.</summary>
    MainAI_MiniKraid = 0xa69ac2,

    /// <summary><c>MainAI_PirateWalking</c> at $B2:FD32.</summary>
    MainAI_PirateWalking = 0xb2fd32,

    /// <summary><c>MainAI_PirateWall</c> at $B2:F02D.</summary>
    MainAI_PirateWall = 0xb2f02d,

    /// <summary><c>MainAI_PirateNinja</c> at $B2:F6A2.</summary>
    MainAI_PirateNinja = 0xb2f6a2,

    /// <summary><c>MainAI_BombTorizo</c> at $AA:C6A4.</summary>
    MainAI_BombTorizo = 0xaac6a4,

    /// <summary><c>MainAI_GoldenTorizo</c> at $AA:D369.</summary>
    MainAI_GoldenTorizo = 0xaad369,

    /// <summary><c>MainAI_Kraid</c> at $A7:AC21.</summary>
    MainAI_Kraid = 0xa7ac21,

    /// <summary><c>MainAI_KraidArm</c> at $A7:B7BD.</summary>
    MainAI_KraidArm = 0xa7b7bd,

    /// <summary><c>MainAI_KraidLintTop</c> at $A7:B801.</summary>
    MainAI_KraidLintTop = 0xa7b801,

    /// <summary><c>MainAI_KraidLintMiddle</c> at $A7:B80D.</summary>
    MainAI_KraidLintMiddle = 0xa7b80d,

    /// <summary><c>MainAI_KraidLintBottom</c> at $A7:B819.</summary>
    MainAI_KraidLintBottom = 0xa7b819,

    /// <summary><c>MainAI_KraidFoot</c> at $A7:B9F6.</summary>
    MainAI_KraidFoot = 0xa7b9f6,

    /// <summary><c>MainAI_KraidNail</c> at $A7:BD32.</summary>
    MainAI_KraidNail = 0xa7bd32,

    /// <summary><c>MainAI_KraidNailBad</c> at $A7:BD49.</summary>
    MainAI_KraidNailBad = 0xa7bd49,

    /// <summary><c>MainAI_Phantoon</c> at $A7:CEA6.</summary>
    MainAI_Phantoon = 0xa7cea6,

    /// <summary><c>RTL_A7E011</c> at $A7:E011.</summary>
    RTL_A7E011 = 0xa7e011,

    /// <summary><c>MainAI_TourianStatue</c> at $AA:D7C7.</summary>
    MainAI_TourianStatue = 0xaad7c7,

    /// <summary><c>MainAI_HurtAI_Shaktool</c> at $AA:DCA3.</summary>
    MainAI_HurtAI_Shaktool = 0xaadca3,

    /// <summary><c>MainAI_Chozo</c> at $AA:E7A7.</summary>
    MainAI_Chozo = 0xaae7a7,
}

/// <summary>The bank-$A4 shot callbacks named by Crocomire's extended hitbox records.</summary>
internal enum CrocomireHitboxShotCallback : ushort
{
    /// <summary>Crocomire no-op hitbox shot callback at $A4:B951; it still counts the hit.</summary>
    NoOp = EnemyAiCodePointers.BankA4.NoOpHitboxShot,
    /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
    Dust = EnemyAiCodePointers.BankA4.DustHitboxShot,
    /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
    Mouth = EnemyAiCodePointers.BankA4.MouthShot,
    /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
    AlternateDust = EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
    /// <summary>Records that deliberately point at the body header's RTL at $A4:B950.</summary>
    HeaderReturn = EnemyAiCodePointers.BankA4.HeaderTouch,
}

/// <summary>Named cartridge enemy interaction callbacks, grouped by their native bank.</summary>
internal static class EnemyAiCodePointers
{
    /// <summary>Shared bank-$A0 enemy interaction callbacks.</summary>
    public static class BankA0
    {
        /// <summary><c>NormalEnemyTouchAI</c> at $A0:8023.</summary>
        public const ushort NormalEnemyTouch = 0x8023;
        /// <summary><c>NormalEnemyShotAI</c> at $A0:802D.</summary>
        public const ushort NormalEnemyShot = 0x802d;
        /// <summary>Shared dud-shot callback at $A0:8046.</summary>
        public const ushort DudShot = 0x8046;
        /// <summary>Shared no-op interaction callback at $A0:804C.</summary>
        public const ushort NoOp = 0x804c;

        /// <summary>RTS_A0804B at $A0:804B, the shared short-return no-op identity rejected by collision dispatch.</summary>
        public const ushort NoOpShortReturn = 0x804b;
    }

    /// <summary>Bank-$A2 enemy interaction callbacks.</summary>
    public static class BankA2
    {
        /// <summary>Owtch shot callback at $A2:A579.</summary>
        public const ushort OwtchShot = 0xa579;
        /// <summary>Dragon touch callback at $A2:E7C8.</summary>
        public const ushort DragonTouch = 0xe7c8;
        /// <summary>Dragon shot callback at $A2:E7CE.</summary>
        public const ushort DragonShot = 0xe7ce;
        /// <summary>Dragon Power Bomb callback at $A2:E7D4.</summary>
        public const ushort DragonPowerBomb = 0xe7d4;
        /// <summary>Shared G/Ripper 2 shot callback at $A2:E3A9.</summary>
        public const ushort GRipperRipper2Shot = 0xe3a9;
        /// <summary>Mama Turtle touch callback at $A2:9281.</summary>
        public const ushort MamaTurtleTouch = 0x9281;
        /// <summary>Baby Turtle touch callback at $A2:929F.</summary>
        public const ushort BabyTurtleTouch = 0x929f;
        /// <summary>Baby Turtle shot callback at $A2:930F.</summary>
        public const ushort BabyTurtleShot = 0x930f;
        /// <summary><c>MaridiaLargeSnailDamagingTouchAI</c> at $A2:D388.</summary>
        public const ushort MaridiaLargeSnailDamagingTouch = 0xd388;
        /// <summary><c>MaridiaLargeSnailNonDamagingTouchAI</c> at $A2:D38C.</summary>
        public const ushort MaridiaLargeSnailNonDamagingTouch = 0xd38c;
        /// <summary><c>MaridiaLargeSnailShotAI</c> at $A2:D3B4.</summary>
        public const ushort MaridiaLargeSnailShot = 0xd3b4;
        /// <summary>Rinka touch callback at $A2:B947.</summary>
        public const ushort RinkaTouch = 0xb947;
        /// <summary>Rinka shot callback at $A2:B94D.</summary>
        public const ushort RinkaShot = 0xb94d;
        /// <summary>Rinka Power Bomb callback at $A2:B953.</summary>
        public const ushort RinkaPowerBomb = 0xb953;
        /// <summary>Vertical shutter touch callback at $A2:F09D.</summary>
        public const ushort VerticalShutterTouch = 0xf09d;
        /// <summary>Shootable vertical shutter shot callback at $A2:F0A2.</summary>
        public const ushort ShootableVerticalShutterShot = 0xf0a2;
        /// <summary>Destroyable vertical shutter shot callback at $A2:F0AA.</summary>
        public const ushort DestroyableVerticalShutterShot = 0xf0aa;
        /// <summary>Vertical shutter Power Bomb callback at $A2:F0B6.</summary>
        public const ushort VerticalShutterPowerBomb = 0xf0b6;
        /// <summary>Horizontal shutter touch callback at $A2:F3D8.</summary>
        public const ushort HorizontalShutterTouch = 0xf3d8;
        /// <summary>Horizontal shutter shot callback at $A2:F40E.</summary>
        public const ushort HorizontalShutterShot = 0xf40e;
        /// <summary>Horizontal shutter Power Bomb callback at $A2:F41A.</summary>
        public const ushort HorizontalShutterPowerBomb = 0xf41a;
    }

    /// <summary>Bank-$A3 enemy interaction callbacks.</summary>
    public static class BankA3
    {
        /// <summary>Skree shot callback at $A3:C7F5.</summary>
        public const ushort SkreeShot = 0xc7f5;
        /// <summary>Metaree shot callback at $A3:8B0F.</summary>
        public const ushort MetareeShot = 0x8b0f;
        /// <summary>Fireflea touch callback at $A3:8E6B.</summary>
        public const ushort FirefleaTouch = 0x8e6b;
        /// <summary>Fireflea Power Bomb callback at $A3:8E83.</summary>
        public const ushort FirefleaPowerBomb = 0x8e83;
        /// <summary>Fireflea shot callback at $A3:8E89.</summary>
        public const ushort FirefleaShot = 0x8e89;
        /// <summary>Mochtroid touch callback at $A3:A953.</summary>
        public const ushort MochtroidTouch = 0xa953;
        /// <summary>Mochtroid shot callback at $A3:A9A8.</summary>
        public const ushort MochtroidShot = 0xa9a8;
        /// <summary>Metroid touch callback at $A3:EDEB.</summary>
        public const ushort MetroidTouch = 0xedeb;
        /// <summary>Metroid shot callback at $A3:EF07.</summary>
        public const ushort MetroidShot = 0xef07;
        /// <summary>Metroid Power Bomb callback at $A3:F042.</summary>
        public const ushort MetroidPowerBomb = 0xf042;
        /// <summary>Yard touch callback at $A3:D3B0.</summary>
        public const ushort YardTouch = 0xd3b0;
        /// <summary>Yard shot callback at $A3:D469.</summary>
        public const ushort YardShot = 0xd469;
        /// <summary>Platform no-op touch callback at $A3:9F07.</summary>
        public const ushort PlatformNoOpTouch = 0x9f07;
        /// <summary>Tripper shot callback at $A3:9F08.</summary>
        public const ushort TripperShot = 0x9f08;
    }

    /// <summary>Bank-$A4 Crocomire interaction callbacks.</summary>
    public static class BankA4
    {
        /// <summary>Crocomire header touch callback at $A4:B950.</summary>
        public const ushort HeaderTouch = 0xb950;
        /// <summary>Crocomire claw touch callback at $A4:B93D.</summary>
        public const ushort ClawTouch = 0xb93d;
        /// <summary>Crocomire no-op hitbox shot callback at $A4:B951.</summary>
        public const ushort NoOpHitboxShot = 0xb951;
        /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
        public const ushort DustHitboxShot = 0xb968;
        /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
        public const ushort MouthShot = 0xba05;
        /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
        public const ushort AlternateDustHitboxShot = 0xbab4;
        /// <summary>Crocomire Power Bomb callback at $A4:B992.</summary>
        public const ushort PowerBomb = 0xb992;
    }

    /// <summary>Bank-$A5 boss interaction callbacks.</summary>
    public static class BankA5
    {
        /// <summary>Spore Spawn touch callback at $A5:EDEC.</summary>
        public const ushort SporeSpawnTouch = 0xedec;
        /// <summary>Spore Spawn shot callback at $A5:ED5A.</summary>
        public const ushort SporeSpawnShot = 0xed5a;
        /// <summary>Draygon touch callback at $A5:95EA.</summary>
        public const ushort DraygonTouch = 0x95ea;
        /// <summary>Draygon shot callback at $A5:95F0.</summary>
        public const ushort DraygonShot = 0x95f0;
        /// <summary>Draygon Power Bomb callback at $A5:9607.</summary>
        public const ushort DraygonPowerBomb = 0x9607;
    }

    /// <summary>Bank-$A6 Ceres, Norfair boss, and Zebetite callbacks.</summary>
    public static class BankA6
    {
        /// <summary>Fake Kraid touch callback at $A6:9C22.</summary>
        public const ushort FakeKraidTouch = 0x9c22;
        /// <summary>Fake Kraid shot callback at $A6:9C39.</summary>
        public const ushort FakeKraidShot = 0x9c39;
        /// <summary>Ceres steam touch callback at $A6:F03F.</summary>
        public const ushort CeresSteamTouch = 0xf03f;
        /// <summary>Ridley extended-spritemap touch callback at $A6:DF59.</summary>
        public const ushort RidleyExtendedTouch = 0xdf59;
        /// <summary>Ridley shot callback at $A6:DF8A.</summary>
        public const ushort RidleyShot = 0xdf8a;
        /// <summary>Ridley Power Bomb callback at $A6:DFB2.</summary>
        public const ushort RidleyPowerBomb = 0xdfb2;
        /// <summary>Zebetite touch callback at $A6:FDA7.</summary>
        public const ushort ZebetiteTouch = 0xfda7;
        /// <summary>Zebetite shot callback at $A6:FDAC.</summary>
        public const ushort ZebetiteShot = 0xfdac;
    }

    /// <summary>Bank-$A7 Kraid interaction callbacks.</summary>
    public static class BankA7
    {
        /// <summary>Kraid background/foot touch callback at $A7:948B.</summary>
        public const ushort KraidBackgroundTouch = 0x948b;
        /// <summary>Kraid arm touch callback at $A7:9490.</summary>
        public const ushort KraidArmTouch = 0x9490;
        /// <summary>Kraid no-op shot callback at $A7:94B5.</summary>
        public const ushort KraidNoOpShot = 0x94b5;
        /// <summary>Kraid arm shot callback at $A7:94B6.</summary>
        public const ushort KraidArmShot = 0x94b6;
        /// <summary>EnemyTouch_KraidNail at $A7:BCCF: normal touch followed by enemy death.</summary>
        public const ushort KraidNailTouch = 0xbccf;
        /// <summary>EnemyTouch_KraidNailBad at $A7:BCDE: normal touch followed by enemy death.</summary>
        public const ushort KraidBadNailTouch = 0xbcde;
    }

    /// <summary>Bank-$A8 ordinary enemy interaction callbacks.</summary>
    public static class BankA8
    {
        /// <summary>Evir touch callback at $A8:8B06.</summary>
        public const ushort EvirTouch = 0x8b06;
        /// <summary>Evir Power Bomb callback at $A8:8B0C.</summary>
        public const ushort EvirPowerBomb = 0x8b0c;
        /// <summary>Evir shot callback at $A8:8B12.</summary>
        public const ushort EvirShot = 0x8b12;
        /// <summary>Magdollite Power Bomb callback at $A8:B400.</summary>
        public const ushort MagdollitePowerBomb = 0xb400;
        /// <summary>Magdollite touch callback at $A8:B406.</summary>
        public const ushort MagdolliteTouch = 0xb406;
        /// <summary>Magdollite shot callback at $A8:B40C.</summary>
        public const ushort MagdolliteShot = 0xb40c;
        /// <summary>Beetom touch callback at $A8:BE2E.</summary>
        public const ushort BeetomTouch = 0xbe2e;
        /// <summary>Beetom shot callback at $A8:BEAC.</summary>
        public const ushort BeetomShot = 0xbeac;
        /// <summary>Powamp touch callback at $A8:C5BE.</summary>
        public const ushort PowampTouch = 0xc5be;
        /// <summary>Powamp shot callback at $A8:C5EF.</summary>
        public const ushort PowampShot = 0xc5ef;
        /// <summary>Powamp Power Bomb callback at $A8:C63F.</summary>
        public const ushort PowampPowerBomb = 0xc63f;
        /// <summary>Work robot touch callback at $A8:D174.</summary>
        public const ushort WorkRobotTouch = 0xd174;
        /// <summary>Work robot no-power shot callback at $A8:D18D.</summary>
        public const ushort WorkRobotNoPowerShot = 0xd18d;
        /// <summary>Work robot powered shot callback at $A8:D192.</summary>
        public const ushort WorkRobotShot = 0xd192;
        /// <summary>Bull shot callback at $A8:DB14.</summary>
        public const ushort BullShot = 0xdb14;
        /// <summary>Kago shot callback at $A8:AB83.</summary>
        public const ushort KagoShot = 0xab83;
        /// <summary>KiHunter shot callback at $A8:F701.</summary>
        public const ushort KiHunterShot = 0xf701;
        /// <summary>Spark shot callback at $A8:E70E.</summary>
        public const ushort SparkShot = 0xe70e;
        /// <summary><c>BlueBrinstarFaceBlockShotAI</c> at $A8:E91D.</summary>
        public const ushort BlueBrinstarFaceBlockShot = 0xe91d;
        /// <summary>Yapping Maw touch callback at $A8:A799.</summary>
        public const ushort YappingMawTouch = 0xa799;
        /// <summary>Yapping Maw shot callback at $A8:A7BD.</summary>
        public const ushort YappingMawShot = 0xa7bd;
    }

    /// <summary>Bank-$A9 Mother Brain and dead-Tourian interaction callbacks.</summary>
    public static class BankA9
    {
        /// <summary>Mother Brain body shot callback at $A9:B503.</summary>
        public const ushort MotherBrainBodyShot = 0xb503;
        /// <summary>Mother Brain head shot callback at $A9:B507.</summary>
        public const ushort MotherBrainHeadShot = 0xb507;
        /// <summary>Mother Brain body touch callback at $A9:B5C5, a bare RTL.</summary>
        public const ushort MotherBrainBodyTouch = 0xb5c5;
        /// <summary>Mother Brain head touch callback at $A9:B5C6.</summary>
        public const ushort MotherBrainHeadTouch = 0xb5c6;
        /// <summary>Dead Torizo touch/shot callback at $A9:D433.</summary>
        public const ushort DeadTorizoTouchAndShot = 0xd433;
        /// <summary>Dead Torizo Power Bomb callback at $A9:D42A.</summary>
        public const ushort DeadTorizoPowerBomb = 0xd42a;
        /// <summary>Dead Sidehopper touch callback at $A9:DD44.</summary>
        public const ushort DeadSidehopperTouch = 0xdd44;
        /// <summary>Dead Sidehopper shot callback at $A9:DD1D.</summary>
        public const ushort DeadSidehopperShot = 0xdd1d;
        /// <summary>Dead Sidehopper Power Bomb callback at $A9:D8CC.</summary>
        public const ushort DeadSidehopperPowerBomb = 0xd8cc;
        /// <summary>Shitroid touch callback at $A9:F789.</summary>
        public const ushort ShitroidTouch = 0xf789;
        /// <summary>Shitroid shot callback at $A9:F842.</summary>
        public const ushort ShitroidShot = 0xf842;
        /// <summary>Shitroid Power Bomb callback at $A9:EFBA.</summary>
        public const ushort ShitroidPowerBomb = 0xefba;
    }

    /// <summary>Bank-$AA Torizo/Shaktool interaction callbacks.</summary>
    public static class BankAA
    {
        /// <summary>Bomb Torizo touch callback at $AA:C977.</summary>
        public const ushort BombTorizoTouch = 0xc977;
        /// <summary>Bomb Torizo shot callback at $AA:C97C.</summary>
        public const ushort BombTorizoShot = 0xc97c;
        /// <summary>Torizo stand-up/sit-down shot callback at $AA:C9C2.</summary>
        public const ushort TorizoStandUpSitDownShot = 0xc9c2;
        /// <summary>Golden Torizo shot callback at $AA:D667.</summary>
        public const ushort GoldenTorizoShot = 0xd667;
        /// <summary>Shaktool touch callback at $AA:DF2F.</summary>
        public const ushort ShaktoolTouch = 0xdf2f;
        /// <summary>Shaktool shot callback at $AA:DF34.</summary>
        public const ushort ShaktoolShot = 0xdf34;
    }

    /// <summary>Bank-$B2 Space Pirate interaction callbacks.</summary>
    public static class BankB2
    {
        /// <summary>CommonB2_NormalEnemyShotAI at $B2:802D; calls common damage directly, without the gold Ninja's private death tail.</summary>
        public const ushort CommonShot = 0x802d;
        /// <summary>Space Pirate Power Bomb callback at $B2:8767.</summary>
        public const ushort PowerBomb = 0x8767;
        /// <summary>Space Pirate touch callback at $B2:876C.</summary>
        public const ushort Touch = 0x876c;
        /// <summary>Space Pirate shot callback at $B2:8779.</summary>
        public const ushort Shot = 0x8779;
        /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot callback at $B2:87C8.</summary>
        public const ushort GoldNinjaVulnerableHitboxShot = 0x87c8;
        /// <summary>Gold ninja Space Pirate invincible-hitbox shot callback at $B2:883E.</summary>
        public const ushort GoldNinjaInvincibleHitboxShot = 0x883e;
    }

    /// <summary>Bank-$B3 Botwoon interaction callbacks.</summary>
    public static class BankB3
    {
        /// <summary>Botwoon touch callback at $B3:9FFF.</summary>
        public const ushort BotwoonTouch = 0x9fff;
        /// <summary>Botwoon shot callback at $B3:A016.</summary>
        public const ushort BotwoonShot = 0xa016;
        /// <summary>Botwoon Power Bomb callback at $B3:A041.</summary>
        public const ushort BotwoonPowerBomb = 0xa041;
    }

}
