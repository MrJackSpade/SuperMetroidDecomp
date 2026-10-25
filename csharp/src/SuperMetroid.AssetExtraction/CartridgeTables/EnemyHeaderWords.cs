using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The bank-$A0 header pointers that name the enemy definitions (#1412): the words a room population entry, a graphics
/// set record, a spawn and an enemy slot's identity hold, decoded once here. Each entry's comment is the header's label in
/// the pinned disassembly; zero names the cleared slot.
/// </summary>
[CpuBusBoundary("catalog: enemy header pointers")]
internal static class EnemyHeaderWords
{
    /// <summary>The bank-$A0 pointer of each enemy definition's 64-byte header.</summary>
    internal static readonly NativeWordIdentities<EnemyDefinitionId> Headers = new(new Dictionary<EnemyDefinitionId, ushort>
    {
        [EnemyDefinitionId.None] = 0x0000,
        [EnemyDefinitionId.Boyon] = 0xcebf, // EnemyHeaders_Boyon
        [EnemyDefinitionId.Stoke] = 0xceff, // EnemyHeaders_Stoke
        [EnemyDefinitionId.MamaTurtle] = 0xcf3f, // EnemyHeaders_MamaTurtle
        [EnemyDefinitionId.BabyTurtle] = 0xcf7f, // EnemyHeaders_BabyTurtle
        [EnemyDefinitionId.Puyo] = 0xcfbf, // EnemyHeaders_Puyo
        [EnemyDefinitionId.Cacatac] = 0xcfff, // EnemyHeaders_Cacatac
        [EnemyDefinitionId.Owtch] = 0xd03f, // EnemyHeaders_Owtch
        [EnemyDefinitionId.ShipTop] = 0xd07f, // EnemyHeaders_ShipTop
        [EnemyDefinitionId.ShipBottomEntrance] = 0xd0bf, // EnemyHeaders_ShipBottomEntrance
        [EnemyDefinitionId.Mellow] = 0xd0ff, // EnemyHeaders_Mellow
        [EnemyDefinitionId.Mella] = 0xd13f, // EnemyHeaders_Mella
        [EnemyDefinitionId.Menu] = 0xd17f, // EnemyHeaders_Menu
        [EnemyDefinitionId.Multiviola] = 0xd1bf, // EnemyHeaders_Multiviola
        [EnemyDefinitionId.LavaRocks] = 0xd1ff, // EnemyHeaders_LavaRocks
        [EnemyDefinitionId.Rinka] = 0xd23f, // EnemyHeaders_Rinka
        [EnemyDefinitionId.Rio] = 0xd27f, // EnemyHeaders_Rio
        [EnemyDefinitionId.Squeept] = 0xd2bf, // EnemyHeaders_Squeept
        [EnemyDefinitionId.Geruta] = 0xd2ff, // EnemyHeaders_Geruta
        [EnemyDefinitionId.Holtz] = 0xd33f, // EnemyHeaders_Holtz
        [EnemyDefinitionId.Oum] = 0xd37f, // EnemyHeaders_Oum
        [EnemyDefinitionId.Choot] = 0xd3bf, // EnemyHeaders_Choot
        [EnemyDefinitionId.GRipper] = 0xd3ff, // EnemyHeaders_GRipper
        [EnemyDefinitionId.Ripper2] = 0xd43f, // EnemyHeaders_Ripper2
        [EnemyDefinitionId.Ripper] = 0xd47f, // EnemyHeaders_Ripper
        [EnemyDefinitionId.Dragon] = 0xd4bf, // EnemyHeaders_Dragon
        [EnemyDefinitionId.ShutterGrowing] = 0xd4ff, // EnemyHeaders_ShutterGrowing
        [EnemyDefinitionId.ShutterShootable] = 0xd53f, // EnemyHeaders_ShutterShootable
        [EnemyDefinitionId.ShutterHorizShootable] = 0xd57f, // EnemyHeaders_ShutterHorizShootable
        [EnemyDefinitionId.ShutterDestroyable] = 0xd5bf, // EnemyHeaders_ShutterDestroyable
        [EnemyDefinitionId.Kamer] = 0xd5ff, // EnemyHeaders_Kamer
        [EnemyDefinitionId.Waver] = 0xd63f, // EnemyHeaders_Waver
        [EnemyDefinitionId.Metaree] = 0xd67f, // EnemyHeaders_Metaree
        [EnemyDefinitionId.Fireflea] = 0xd6bf, // EnemyHeaders_Fireflea
        [EnemyDefinitionId.Skultera] = 0xd6ff, // EnemyHeaders_Skultera
        [EnemyDefinitionId.Elevator] = 0xd73f, // EnemyHeaders_Elevator
        [EnemyDefinitionId.Sciser] = 0xd77f, // EnemyHeaders_Sciser
        [EnemyDefinitionId.Zero] = 0xd7bf, // EnemyHeaders_Zero
        [EnemyDefinitionId.Tripper] = 0xd7ff, // EnemyHeaders_Tripper
        [EnemyDefinitionId.Kamer2] = 0xd83f, // EnemyHeaders_Kamer2
        [EnemyDefinitionId.Sbug] = 0xd87f, // EnemyHeaders_Sbug
        [EnemyDefinitionId.Sbug2] = 0xd8bf, // EnemyHeaders_Sbug2
        [EnemyDefinitionId.Mochtroid] = 0xd8ff, // EnemyHeaders_Mochtroid
        [EnemyDefinitionId.Sidehopper] = 0xd93f, // EnemyHeaders_Sidehopper
        [EnemyDefinitionId.Dessgeega] = 0xd97f, // EnemyHeaders_Dessgeega
        [EnemyDefinitionId.SidehopperLarge] = 0xd9bf, // EnemyHeaders_SidehopperLarge
        [EnemyDefinitionId.SidehopperTourian] = 0xd9ff, // EnemyHeaders_SidehopperTourian
        [EnemyDefinitionId.DessgeegaLarge] = 0xda3f, // EnemyHeaders_DessgeegaLarge
        [EnemyDefinitionId.Zoa] = 0xda7f, // EnemyHeaders_Zoa
        [EnemyDefinitionId.Viola] = 0xdabf, // EnemyHeaders_Viola
        [EnemyDefinitionId.Respawn] = 0xdaff, // EnemyHeaders_Respawn
        [EnemyDefinitionId.Bang] = 0xdb3f, // EnemyHeaders_Bang
        [EnemyDefinitionId.Skree] = 0xdb7f, // EnemyHeaders_Skree
        [EnemyDefinitionId.Yard] = 0xdbbf, // EnemyHeaders_Yard
        [EnemyDefinitionId.Reflec] = 0xdbff, // EnemyHeaders_Reflec
        [EnemyDefinitionId.HZoomer] = 0xdc3f, // EnemyHeaders_HZoomer
        [EnemyDefinitionId.Zeela] = 0xdc7f, // EnemyHeaders_Zeela
        [EnemyDefinitionId.Sova] = 0xdcbf, // EnemyHeaders_Sova
        [EnemyDefinitionId.Zoomer] = 0xdcff, // EnemyHeaders_Zoomer
        [EnemyDefinitionId.MZoomer] = 0xdd3f, // EnemyHeaders_MZoomer
        [EnemyDefinitionId.Metroid] = 0xdd7f, // EnemyHeaders_Metroid
        [EnemyDefinitionId.Crocomire] = 0xddbf, // EnemyHeaders_Crocomire
        [EnemyDefinitionId.CrocomireTongue] = 0xddff, // EnemyHeaders_CrocomireTongue
        [EnemyDefinitionId.DraygonBody] = 0xde3f, // EnemyHeaders_DraygonBody
        [EnemyDefinitionId.DraygonEye] = 0xde7f, // EnemyHeaders_DraygonEye
        [EnemyDefinitionId.DraygonTail] = 0xdebf, // EnemyHeaders_DraygonTail
        [EnemyDefinitionId.DraygonArms] = 0xdeff, // EnemyHeaders_DraygonArms
        [EnemyDefinitionId.SporeSpawn] = 0xdf3f, // EnemyHeaders_SporeSpawn
        [EnemyDefinitionId.SporeSpawnStalk] = 0xdf7f, // EnemyHeaders_SporeSpawnStalk
        [EnemyDefinitionId.Boulder] = 0xdfbf, // EnemyHeaders_Boulder
        [EnemyDefinitionId.KzanTop] = 0xdfff, // EnemyHeaders_KzanTop
        [EnemyDefinitionId.KzanBottom] = 0xe03f, // EnemyHeaders_KzanBottom
        [EnemyDefinitionId.Hibashi] = 0xe07f, // EnemyHeaders_Hibashi
        [EnemyDefinitionId.Puromi] = 0xe0bf, // EnemyHeaders_Puromi
        [EnemyDefinitionId.MiniKraid] = 0xe0ff, // EnemyHeaders_MiniKraid
        [EnemyDefinitionId.RidleyCeres] = 0xe13f, // EnemyHeaders_RidleyCeres
        [EnemyDefinitionId.Ridley] = 0xe17f, // EnemyHeaders_Ridley
        [EnemyDefinitionId.RidleyExplosion] = 0xe1bf, // EnemyHeaders_RidleyExplosion
        [EnemyDefinitionId.Steam] = 0xe1ff, // EnemyHeaders_Steam
        [EnemyDefinitionId.CeresDoor] = 0xe23f, // EnemyHeaders_CeresDoor
        [EnemyDefinitionId.Zebetite] = 0xe27f, // EnemyHeaders_Zebetite
        [EnemyDefinitionId.Kraid] = 0xe2bf, // EnemyHeaders_Kraid
        [EnemyDefinitionId.KraidArm] = 0xe2ff, // EnemyHeaders_KraidArm
        [EnemyDefinitionId.KraidLintTop] = 0xe33f, // EnemyHeaders_KraidLintTop
        [EnemyDefinitionId.KraidLintMiddle] = 0xe37f, // EnemyHeaders_KraidLintMiddle
        [EnemyDefinitionId.KraidLintBottom] = 0xe3bf, // EnemyHeaders_KraidLintBottom
        [EnemyDefinitionId.KraidFoot] = 0xe3ff, // EnemyHeaders_KraidFoot
        [EnemyDefinitionId.KraidNail] = 0xe43f, // EnemyHeaders_KraidNail
        [EnemyDefinitionId.KraidNailBad] = 0xe47f, // EnemyHeaders_KraidNailBad
        [EnemyDefinitionId.PhantoonBody] = 0xe4bf, // EnemyHeaders_PhantoonBody
        [EnemyDefinitionId.PhantoonEye] = 0xe4ff, // EnemyHeaders_PhantoonEye
        [EnemyDefinitionId.PhantoonTentacles] = 0xe53f, // EnemyHeaders_PhantoonTentacles
        [EnemyDefinitionId.PhantoonMouth] = 0xe57f, // EnemyHeaders_PhantoonMouth
        [EnemyDefinitionId.Etecoon] = 0xe5bf, // EnemyHeaders_Etecoon
        [EnemyDefinitionId.Dachora] = 0xe5ff, // EnemyHeaders_Dachora
        [EnemyDefinitionId.Evir] = 0xe63f, // EnemyHeaders_Evir
        [EnemyDefinitionId.EvirProjectile] = 0xe67f, // EnemyHeaders_EvirProjectile
        [EnemyDefinitionId.Eye] = 0xe6bf, // EnemyHeaders_Eye
        [EnemyDefinitionId.Fune] = 0xe6ff, // EnemyHeaders_Fune
        [EnemyDefinitionId.Namihe] = 0xe73f, // EnemyHeaders_Namihe
        [EnemyDefinitionId.Coven] = 0xe77f, // EnemyHeaders_Coven
        [EnemyDefinitionId.YappingMaw] = 0xe7bf, // EnemyHeaders_YappingMaw
        [EnemyDefinitionId.Kago] = 0xe7ff, // EnemyHeaders_Kago
        [EnemyDefinitionId.Magdollite] = 0xe83f, // EnemyHeaders_Magdollite
        [EnemyDefinitionId.Beetom] = 0xe87f, // EnemyHeaders_Beetom
        [EnemyDefinitionId.Powamp] = 0xe8bf, // EnemyHeaders_Powamp
        [EnemyDefinitionId.Robot] = 0xe8ff, // EnemyHeaders_Robot
        [EnemyDefinitionId.RobotNoPower] = 0xe93f, // EnemyHeaders_RobotNoPower
        [EnemyDefinitionId.Bull] = 0xe97f, // EnemyHeaders_Bull
        [EnemyDefinitionId.Alcoon] = 0xe9bf, // EnemyHeaders_Alcoon
        [EnemyDefinitionId.Atomic] = 0xe9ff, // EnemyHeaders_Atomic
        [EnemyDefinitionId.Spark] = 0xea3f, // EnemyHeaders_Spark
        [EnemyDefinitionId.FaceBlock] = 0xea7f, // EnemyHeaders_FaceBlock
        [EnemyDefinitionId.KihunterGreen] = 0xeabf, // EnemyHeaders_KihunterGreen
        [EnemyDefinitionId.KihunterGreenWings] = 0xeaff, // EnemyHeaders_KihunterGreenWings
        [EnemyDefinitionId.KihunterYellow] = 0xeb3f, // EnemyHeaders_KihunterYellow
        [EnemyDefinitionId.KihunterYellowWings] = 0xeb7f, // EnemyHeaders_KihunterYellowWings
        [EnemyDefinitionId.KihunterRed] = 0xebbf, // EnemyHeaders_KihunterRed
        [EnemyDefinitionId.KihunterRedWings] = 0xebff, // EnemyHeaders_KihunterRedWings
        [EnemyDefinitionId.MotherBrainHead] = 0xec3f, // EnemyHeaders_MotherBrainHead
        [EnemyDefinitionId.MotherBrainBody] = 0xec7f, // EnemyHeaders_MotherBrainBody
        [EnemyDefinitionId.BabyMetroidCutscene] = 0xecbf, // EnemyHeaders_BabyMetroidCutscene
        [EnemyDefinitionId.MotherBrainTubes] = 0xecff, // EnemyHeaders_MotherBrainTubes
        [EnemyDefinitionId.CorpseTorizo] = 0xed3f, // EnemyHeaders_CorpseTorizo
        [EnemyDefinitionId.CorpseSidehopper] = 0xed7f, // EnemyHeaders_CorpseSidehopper
        [EnemyDefinitionId.CorpseSidehopper2] = 0xedbf, // EnemyHeaders_CorpseSidehopper2
        [EnemyDefinitionId.CorpseZoomer] = 0xedff, // EnemyHeaders_CorpseZoomer
        [EnemyDefinitionId.CorpseRipper] = 0xee3f, // EnemyHeaders_CorpseRipper
        [EnemyDefinitionId.CorpseSkree] = 0xee7f, // EnemyHeaders_CorpseSkree
        [EnemyDefinitionId.BabyMetroid] = 0xeebf, // EnemyHeaders_BabyMetroid
        [EnemyDefinitionId.BombTorizo] = 0xeeff, // EnemyHeaders_BombTorizo
        [EnemyDefinitionId.BombTorizoOrb] = 0xef3f, // EnemyHeaders_BombTorizoOrb
        [EnemyDefinitionId.GoldenTorizo] = 0xef7f, // EnemyHeaders_GoldenTorizo
        [EnemyDefinitionId.GoldenTorizoOrb] = 0xefbf, // EnemyHeaders_GoldenTorizoOrb
        [EnemyDefinitionId.TourianStatue] = 0xefff, // EnemyHeaders_TourianStatue
        [EnemyDefinitionId.TourianStatueGhost] = 0xf03f, // EnemyHeaders_TourianStatueGhost
        [EnemyDefinitionId.Shaktool] = 0xf07f, // EnemyHeaders_Shaktool
        [EnemyDefinitionId.NoobTubeCrack] = 0xf0bf, // EnemyHeaders_NoobTubeCrack
        [EnemyDefinitionId.Chozo] = 0xf0ff, // EnemyHeaders_Chozo
        [EnemyDefinitionId.UnusedSpinningTurtleEye] = 0xf153, // UNUSED_EnemyHeaders_SpinningTurtleEye_A0F153
        [EnemyDefinitionId.Zeb] = 0xf193, // EnemyHeaders_Zeb
        [EnemyDefinitionId.Zebbo] = 0xf1d3, // EnemyHeaders_Zebbo
        [EnemyDefinitionId.Gamet] = 0xf213, // EnemyHeaders_Gamet
        [EnemyDefinitionId.Geega] = 0xf253, // EnemyHeaders_Geega
        [EnemyDefinitionId.Botwoon] = 0xf293, // EnemyHeaders_Botwoon
        [EnemyDefinitionId.EtecoonEscape] = 0xf2d3, // EnemyHeaders_EtecoonEscape
        [EnemyDefinitionId.DachoraEscape] = 0xf313, // EnemyHeaders_DachoraEscape
        [EnemyDefinitionId.PirateGreyWall] = 0xf353, // EnemyHeaders_PirateGreyWall
        [EnemyDefinitionId.PirateGreenWall] = 0xf393, // EnemyHeaders_PirateGreenWall
        [EnemyDefinitionId.PirateRedWall] = 0xf3d3, // EnemyHeaders_PirateRedWall
        [EnemyDefinitionId.PirateGoldWall] = 0xf413, // EnemyHeaders_PirateGoldWall
        [EnemyDefinitionId.PirateMagentaWall] = 0xf453, // EnemyHeaders_PirateMagentaWall
        [EnemyDefinitionId.PirateSilverWall] = 0xf493, // EnemyHeaders_PirateSilverWall
        [EnemyDefinitionId.PirateGreyNinja] = 0xf4d3, // EnemyHeaders_PirateGreyNinja
        [EnemyDefinitionId.PirateGreenNinja] = 0xf513, // EnemyHeaders_PirateGreenNinja
        [EnemyDefinitionId.PirateRedNinja] = 0xf553, // EnemyHeaders_PirateRedNinja
        [EnemyDefinitionId.PirateGoldNinja] = 0xf593, // EnemyHeaders_PirateGoldNinja
        [EnemyDefinitionId.PirateMagentaNinja] = 0xf5d3, // EnemyHeaders_PirateMagentaNinja
        [EnemyDefinitionId.PirateSilverNinja] = 0xf613, // EnemyHeaders_PirateSilverNinja
        [EnemyDefinitionId.PirateGreyWalking] = 0xf653, // EnemyHeaders_PirateGreyWalking
        [EnemyDefinitionId.PirateGreenWalking] = 0xf693, // EnemyHeaders_PirateGreenWalking
        [EnemyDefinitionId.PirateRedWalking] = 0xf6d3, // EnemyHeaders_PirateRedWalking
        [EnemyDefinitionId.PirateGoldWalking] = 0xf713, // EnemyHeaders_PirateGoldWalking
        [EnemyDefinitionId.PirateMagentaWalking] = 0xf753, // EnemyHeaders_PirateMagentaWalking
        [EnemyDefinitionId.PirateSilverWalking] = 0xf793, // EnemyHeaders_PirateSilverWalking
    });
}
