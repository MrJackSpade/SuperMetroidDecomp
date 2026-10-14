using static SuperMetroid.Core.Programs.InstructionItem;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Complete bounded Bomb/Golden Torizo instruction lists, compiled from the pinned NTSC cartridge.
/// Branches, callbacks, timers, and movement operands are engine definitions; tile pixels and
/// spritemap contents remain installed artwork. Packed DMA operands have their own typed catalog.
/// </summary>
internal abstract class TorizoInstructionProgramDefinitions
{
    /// <summary>Bank $AA holds the Bomb and Golden Torizo instruction programs cataloged here.</summary>
    internal const byte Bank = 0xaa;

    /// <summary><c>InstList_Torizo_SpecialCallable_BlowUpBombTorizosGut</c> at $AA:B0E5; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_SpecialCallable_BlowUpBombTorizosGut = 0xb0e5;

    /// <summary><c>InstList_Torizo_Callable_BlowUpBombTorizosFace</c> at $AA:B155; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_Callable_BlowUpBombTorizosFace = 0xb155;

    /// <summary><c>InstList_Torizo_DeathSequence_0</c> at $AA:B1C8; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_DeathSequence_0 = 0xb1c8;

    /// <summary><c>InstList_Torizo_DeathSequence_1</c> at $AA:B1D2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_DeathSequence_1 = 0xb1d2;

    /// <summary><c>InstList_Torizo_DeathSequence_2</c> at $AA:B1E2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_DeathSequence_2 = 0xb1e2;

    /// <summary><c>InstList_Torizo_BombTorizo_Initial_0</c> at $AA:B879; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_BombTorizo_Initial_0 = 0xb879;

    /// <summary><c>InstList_Torizo_BombTorizo_Initial_1</c> at $AA:B8C7; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_BombTorizo_Initial_1 = 0xb8c7;

    /// <summary><c>InstList_Torizo_BombTorizo_Initial_2</c> at $AA:B935; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_BombTorizo_Initial_2 = 0xb935;

    /// <summary><c>InstList_Torizo_FacingLeft_TurningLeft</c> at $AA:B962; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_TurningLeft = 0xb962;

    /// <summary><c>InstList_Torizo_FacingLeft_Walking_RightLegMoving</c> at $AA:B96C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Walking_RightLegMoving = 0xb96c;

    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c> at $AA:B9B6; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Walking_LeftLegMoving = 0xb9b6;

    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BA04; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0 = 0xba04;

    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1</c> at $AA:BA26; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1 = 0xba26;

    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BA46; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0 = 0xba46;

    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1</c> at $AA:BA68; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1 = 0xba68;

    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0</c> at $AA:BA88; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0 = 0xba88;

    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1</c> at $AA:BA90; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1 = 0xba90;

    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0</c> at $AA:BAF2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0 = 0xbaf2;

    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1</c> at $AA:BAFA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1 = 0xbafa;

    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward</c> at $AA:BB5C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward = 0xbb5c;

    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward</c> at $AA:BBDE; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward = 0xbbde;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_0</c> at $AA:BC60; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingForwards_0 = 0xbc60;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_1</c> at $AA:BC70; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingForwards_1 = 0xbc70;

    /// <summary><c>InstList_Torizo_FacingLeft_Falling_0</c> at $AA:BC78; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Falling_0 = 0xbc78;

    /// <summary><c>InstList_Torizo_FacingLeft_Falling_1</c> at $AA:BC80; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Falling_1 = 0xbc80;

    /// <summary><c>InstList_Torizo_FacingLeft_Falling_2</c> at $AA:BC88; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Falling_2 = 0xbc88;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0</c> at $AA:BC96; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0 = 0xbc96;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1</c> at $AA:BCA6; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1 = 0xbca6;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2</c> at $AA:BCAE; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2 = 0xbcae;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3</c> at $AA:BCB6; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3 = 0xbcb6;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4</c> at $AA:BCBE; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4 = 0xbcbe;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0</c> at $AA:BCD2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0 = 0xbcd2;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1</c> at $AA:BCE2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1 = 0xbce2;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2</c> at $AA:BCEA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2 = 0xbcea;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3</c> at $AA:BCF2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3 = 0xbcf2;

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4</c> at $AA:BCFA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4 = 0xbcfa;

    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_TurningLeft</c> at $AA:BD0E; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Faceless_TurningLeft = 0xbd0e;

    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving</c> at $AA:BD18; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving = 0xbd18;

    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c> at $AA:BD52; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving = 0xbd52;

    /// <summary><c>UNUSED_InstList_Torizo_FacingRight_StandUp_AABD90</c> at $AA:BD90; native instruction-list identity.</summary>
    internal const ushort UNUSED_InstList_Torizo_FacingRight_StandUp_AABD90 = 0xbd90;

    /// <summary><c>InstList_Torizo_FacingRight_TurningRight</c> at $AA:BDD8; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_TurningRight = 0xbdd8;

    /// <summary><c>InstList_Torizo_FacingRight_Walking_LeftLegMoving</c> at $AA:BDE2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Walking_LeftLegMoving = 0xbde2;

    /// <summary><c>InstList_Torizo_FacingRight_Walking_RightLegMoving</c> at $AA:BE30; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Walking_RightLegMoving = 0xbe30;

    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BE7E; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0 = 0xbe7e;

    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1</c> at $AA:BEA0; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1 = 0xbea0;

    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BEC0; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0 = 0xbec0;

    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1</c> at $AA:BEE2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1 = 0xbee2;

    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0</c> at $AA:BF02; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0 = 0xbf02;

    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1</c> at $AA:BF0A; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1 = 0xbf0a;

    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0</c> at $AA:BF6C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0 = 0xbf6c;

    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1</c> at $AA:BF74; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1 = 0xbf74;

    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward</c> at $AA:BFD6; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward = 0xbfd6;

    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward</c> at $AA:C058; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward = 0xc058;

    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_0</c> at $AA:C0DA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpingForwards_0 = 0xc0da;

    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_1</c> at $AA:C0EA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpingForwards_1 = 0xc0ea;

    /// <summary><c>InstList_Torizo_FacingRight_Falling_0</c> at $AA:C0F2; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Falling_0 = 0xc0f2;

    /// <summary><c>InstList_Torizo_FacingRight_Falling_1</c> at $AA:C0FA; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Falling_1 = 0xc0fa;

    /// <summary><c>InstList_Torizo_FacingRight_Falling_2</c> at $AA:C102; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Falling_2 = 0xc102;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0</c> at $AA:C110; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0 = 0xc110;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1</c> at $AA:C120; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1 = 0xc120;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2</c> at $AA:C128; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2 = 0xc128;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3</c> at $AA:C130; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3 = 0xc130;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4</c> at $AA:C138; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4 = 0xc138;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0</c> at $AA:C14C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0 = 0xc14c;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1</c> at $AA:C15C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1 = 0xc15c;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2</c> at $AA:C164; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2 = 0xc164;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3</c> at $AA:C16C; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3 = 0xc16c;

    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4</c> at $AA:C174; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4 = 0xc174;

    /// <summary><c>InstList_Torizo_FacingRight_Faceless_TurningRight</c> at $AA:C188; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Faceless_TurningRight = 0xc188;

    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving</c> at $AA:C192; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving = 0xc192;

    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving</c> at $AA:C1CC; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving = 0xc1cc;

    /// <summary><c>InstList_GoldenTorizo_Initial_0</c> at $AA:C9CB; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Initial_0 = 0xc9cb;

    /// <summary><c>InstList_GoldenTorizo_Initial_1</c> at $AA:C9E6; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Initial_1 = 0xc9e6;

    /// <summary><c>InstList_GoldenTorizo_Initial_2</c> at $AA:CA48; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Initial_2 = 0xca48;

    /// <summary><c>InstList_GoldenTorizo_Initial_3</c> at $AA:CAB6; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Initial_3 = 0xcab6;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0</c> at $AA:CAFF; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0 = 0xcaff;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1</c> at $AA:CB1F; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1 = 0xcb1f;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0</c> at $AA:CB41; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0 = 0xcb41;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1</c> at $AA:CB61; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1 = 0xcb61;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0</c> at $AA:CB83; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0 = 0xcb83;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1</c> at $AA:CB8B; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1 = 0xcb8b;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0</c> at $AA:CBED; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0 = 0xcbed;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1</c> at $AA:CBF5; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1 = 0xcbf5;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0</c> at $AA:CC57; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0 = 0xcc57;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1</c> at $AA:CC77; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1 = 0xcc77;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0</c> at $AA:CC99; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0 = 0xcc99;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1</c> at $AA:CCB9; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1 = 0xccb9;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0</c> at $AA:CCDB; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0 = 0xccdb;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1</c> at $AA:CCE3; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1 = 0xcce3;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0</c> at $AA:CD45; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0 = 0xcd45;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1</c> at $AA:CD4D; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1 = 0xcd4d;

    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd</c> at $AA:CDAF; native instruction-list identity.</summary>
    internal const ushort InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd = 0xcdaf;

    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd</c> at $AA:CDB9; native instruction-list identity.</summary>
    internal const ushort InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd = 0xcdb9;

    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd</c> at $AA:CDC3; native instruction-list identity.</summary>
    internal const ushort InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd = 0xcdc3;

    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd</c> at $AA:CDCD; native instruction-list identity.</summary>
    internal const ushort InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd = 0xcdcd;

    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd</c> at $AA:CDE1; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd = 0xcde1;

    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd</c> at $AA:CE43; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd = 0xce43;

    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd</c> at $AA:CEA5; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd = 0xcea5;

    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd</c> at $AA:CEFF; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd = 0xceff;

    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingLeft</c> at $AA:CF59; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SitDownAttack_FacingLeft = 0xcf59;

    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingRight</c> at $AA:CFC5; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_SitDownAttack_FacingRight = 0xcfc5;

    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0</c> at $AA:D031; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0 = 0xd031;

    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1</c> at $AA:D07B; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1 = 0xd07b;

    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2</c> at $AA:D087; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2 = 0xd087;

    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_EyeBeamAttack_0 = 0xd10d;

    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_1</c> at $AA:D11F; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_EyeBeamAttack_1 = 0xd11f;

    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_2</c> at $AA:D133; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_EyeBeamAttack_2 = 0xd133;

    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_Stunned_0 = 0xd193;

    /// <summary><c>InstList_Torizo_Stunned_1</c> at $AA:D1A1; native instruction-list identity.</summary>
    internal const ushort InstList_Torizo_Stunned_1 = 0xd1a1;

    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningLeft</c> at $AA:D1F1; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Dodge_TurningLeft = 0xd1f1;

    /// <summary><c>InstList_GoldenTorizo_TurningLeft</c> at $AA:D203; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_TurningLeft = 0xd203;

    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c> at $AA:D20D; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_WalkingLeft_RightLegMoving = 0xd20d;

    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_LeftLegMoving</c> at $AA:D259; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_WalkingLeft_LeftLegMoving = 0xd259;

    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningRight</c> at $AA:D2AD; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_Dodge_TurningRight = 0xd2ad;

    /// <summary><c>InstList_GoldenTorizo_TurningRight</c> at $AA:D2BF; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_TurningRight = 0xd2bf;

    /// <summary><c>InstList_GoldenTorizo_WalkingRight_LeftLegMoving</c> at $AA:D2C9; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_WalkingRight_LeftLegMoving = 0xd2c9;

    /// <summary><c>InstList_GoldenTorizo_WalkingRight_RightLegMoving</c> at $AA:D315; native instruction-list identity.</summary>
    internal const ushort InstList_GoldenTorizo_WalkingRight_RightLegMoving = 0xd315;

    /// <summary><c>RTS_AAC6AB</c> at $AA:C6AB; native instruction callback operand.</summary>
    private const ushort RTS_AAC6AB = 0xc6ab;

    /// <summary><c>Function_Torizo_NormalMovement</c> at $AA:C6FF; native instruction callback operand.</summary>
    private const ushort Function_Torizo_NormalMovement = 0xc6ff;

    /// <summary><c>Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling</c> at $AA:C6C6; native instruction callback operand.</summary>
    private const ushort Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling = 0xc6c6;

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF; native instruction callback operand.</summary>
    private const ushort Function_Torizo_SimpleMovement = 0xc6bf;

    /// <summary><c>Function_Torizo_Movement_Walking</c> at $AA:C752; native instruction callback operand.</summary>
    private const ushort Function_Torizo_Movement_Walking = 0xc752;

    /// <summary><c>Function_Torizo_Movement_Attacking</c> at $AA:C828; native instruction callback operand.</summary>
    private const ushort Function_Torizo_Movement_Attacking = 0xc828;

    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C; native instruction callback operand.</summary>
    private const ushort Function_Torizo_Movement_Jumping_Falling = 0xc82c;

    /// <summary><c>Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos</c> at $AA:D5C2; native instruction callback operand.</summary>
    private const ushort Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos = 0xd5c2;

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED; native instruction callback operand.</summary>
    private const ushort Function_GoldenTorizo_Movement_Attacking = 0xd5ed;

    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1; native instruction callback operand.</summary>
    private const ushort Function_GoldenTorizo_Movement_Walking = 0xd5f1;

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c> at $AA:D5DF; native instruction callback operand.</summary>
    private const ushort Function_GoldenTorizo_SimpleMovement = 0xd5df;

    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6; native instruction callback operand.</summary>
    private const ushort Function_GoldenTorizo_NormalMovement = 0xd5e6;

    /// <summary>Combines shared, Bomb Torizo, and Golden Torizo control words while reserving art pointers.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        [.. SharedItems, .. BombLeftItems, .. BombRightItems, .. GoldenLeftItems, .. GoldenRightItems]);
    /// <summary>Number of interleaved spritemap words that remain presentation inputs.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    /// <summary>Returns the native address of a Torizo spritemap word outside the mechanics layout.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Reads a compiled Torizo instruction or operand, rejecting pointers outside its authored control lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo instruction word $AA:{address:X4} has no compiled mechanics definition.");

    /// <summary>BombLeft instruction items at $AA:B879..BC5F; native list names identify each group.</summary>
    private static InstructionItem[] BombLeftItems =>
    [
        Origin(0xb879),
        Entry(InstList_Torizo_BombTorizo_Initial_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Frame(48),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(32),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(32),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0002),
        Entry(InstList_Torizo_BombTorizo_Initial_1),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_BombTorizo_Initial_1),
        Frame(48),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0000),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0002),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0004),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0006),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0008),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x000a),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0010),
        Entry(InstList_Torizo_BombTorizo_Initial_2),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_BombTorizo_Initial_2, (ushort)TorizoInstruction.RTL_AAC2C8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Walking_LeftLegMoving),
        Origin(0xb962),
        Entry(InstList_Torizo_FacingLeft_TurningLeft),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Entry(InstList_Torizo_FacingLeft_Walking_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0002),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0004),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0006),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0008),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x000a),
        Entry(InstList_Torizo_FacingLeft_Walking_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x000c),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x000e),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0010),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0012),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0000),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Walking_RightLegMoving),
        Entry(InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(16),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0003),
        Entry(InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizosChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(72),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(16),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0003),
        Entry(InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizosChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(72),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0000),
        Frame(2),
        Frame(24),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0001),
        Frame(2),
        Frame(56),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0000),
        Frame(2),
        Frame(24),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0001),
        Frame(2),
        Frame(56),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0000),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0002),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0004),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0006),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0008),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000a),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000c),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000e),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0010),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0012),
        Frame(1),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0000),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0002),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0004),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0006),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0008),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000a),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000c),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000e),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0010),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0012),
        Frame(1),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
    ];

    /// <summary>BombRight instruction items at $AA:BC60..C209; native list names identify each group.</summary>
    private static InstructionItem[] BombRightItems =>
    [
        Origin(0xbc60),
        Entry(InstList_Torizo_FacingLeft_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_Falling_0),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingLeft_JumpingForwards_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingLeft_JumpingForwards_1),
        Entry(InstList_Torizo_FacingLeft_Falling_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_Falling_2),
        Entry(InstList_Torizo_FacingLeft_Falling_1),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Falling_1),
        Entry(InstList_Torizo_FacingLeft_Falling_2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving, InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Walking_LeftLegMoving),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving, InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0, InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Walking_RightLegMoving),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3),
        Entry(InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving, InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0, InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Walking_LeftLegMoving),
        Entry(InstList_Torizo_FacingLeft_Faceless_TurningLeft),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8),
        Entry(InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0002),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0004),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0006),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0008),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x000a),
        Entry(InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x000c),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x000e),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0010),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0012),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0000),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving),
        Entry(UNUSED_InstList_Torizo_FacingRight_StandUp_AABD90),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Frame(32),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0010),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0012),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0014),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0016),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0018),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x001a),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Walking_RightLegMoving),
        Entry(InstList_Torizo_FacingRight_TurningRight),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Entry(InstList_Torizo_FacingRight_Walking_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0016),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0018),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x001a),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x001c),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x001e),
        Entry(InstList_Torizo_FacingRight_Walking_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0020),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0022),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0024),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0026),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY, 0x0014),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Walking_LeftLegMoving),
        Entry(InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(16),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0003),
        Entry(InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizosChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(72),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(16),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0003),
        Entry(InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizosChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(72),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0000),
        Frame(2),
        Frame(24),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0001),
        Frame(2),
        Frame(56),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0000),
        Frame(2),
        Frame(24),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY, 0x0001),
        Frame(2),
        Frame(56),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0000),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0002),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0004),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0006),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0008),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000a),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000c),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000e),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0010),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0012),
        Frame(1),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Attacking),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0000),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0002),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0004),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0006),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0008),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000a),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000c),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x000e),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0010),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY, 0x0012),
        Frame(1),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_Torizo_FacingRight_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_Falling_0),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingRight_JumpingForwards_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingRight_JumpingForwards_1),
        Entry(InstList_Torizo_FacingRight_Falling_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_Falling_2),
        Entry(InstList_Torizo_FacingRight_Falling_1),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Falling_1),
        Entry(InstList_Torizo_FacingRight_Falling_2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving, InstList_GoldenTorizo_WalkingRight_RightLegMoving),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Walking_RightLegMoving),
        Entry(InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1),
        Entry(InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4),
        Entry(InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3),
        Entry(InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving, InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0, InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Walking_LeftLegMoving),
        Entry(InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2),
        Frame(5),
        Frame(5),
        Entry(InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1),
        Entry(InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Jumping_Falling),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4),
        Entry(InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3),
        Entry(InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving, InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0, InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Walking_RightLegMoving),
        Entry(InstList_Torizo_FacingRight_Faceless_TurningRight),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8),
        Entry(InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0016),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0018),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x001a),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x001c),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x001e),
        Entry(InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_Torizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront, InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0020),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0022),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0024),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0026),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY, 0x0014),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving),
    ];

    /// <summary>GoldenLeft instruction items at $AA:C9CB..CDD6; native list names identify each group.</summary>
    private static InstructionItem[] GoldenLeftItems =>
    [
        Origin(0xc9cb),
        Entry(InstList_GoldenTorizo_Initial_0),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Entry(InstList_GoldenTorizo_Initial_1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfNotHitGround, InstList_GoldenTorizo_Initial_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0004),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0002),
        Frame(5),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0000),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, RTS_AAC6AB),
        Frame(48),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(32),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(32),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0002),
        Entry(InstList_GoldenTorizo_Initial_2),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_Initial_2),
        Frame(32),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0000),
        Frame(12),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0002),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0004),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0006),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0008),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x000a),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LoadGoldenTorizoPalettes),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0010),
        Entry(InstList_GoldenTorizo_Initial_3),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_Initial_3, (ushort)TorizoInstruction.RTL_AAC2C8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock, (ushort)TorizoInstruction.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Origin(0xcaff),
        Entry(InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0006),
        Entry(InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0006),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0006),
        Entry(InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0006),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0006),
        Entry(InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0006),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0006),
        Entry(InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0006),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Entry(InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Entry(InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack, InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0, InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_RightLegMoving),
        Entry(InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack, InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0, InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Entry(InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack, InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0, InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_LeftLegMoving),
        Entry(InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack, InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0, InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_RightLegMoving),
        Skip(10),
    ];

    /// <summary>GoldenRight instruction items at $AA:CDE1..D368; native list names identify each group.</summary>
    private static InstructionItem[] GoldenRightItems =>
    [
        Origin(0xcde1),
        Entry(InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_QueueSonicBoomSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_SimpleMovement),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnSuperMissile),
        Frame(48),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_RightLegMoving),
        Entry(InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_QueueSonicBoomSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_SimpleMovement),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnSuperMissile),
        Frame(48),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Entry(InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_QueueSonicBoomSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_SimpleMovement),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnSuperMissile),
        Frame(48),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_LeftLegMoving),
        Entry(InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_QueueSonicBoomSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_SimpleMovement),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnSuperMissile),
        Frame(48),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(16),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_RightLegMoving),
        Entry(InstList_GoldenTorizo_SitDownAttack_FacingLeft),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x000a),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0008),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0006),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0004),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0002),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0000),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0000),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0002),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0004),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0006),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0008),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x000a),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Entry(InstList_GoldenTorizo_SitDownAttack_FacingRight),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x001a),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0018),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0016),
        Frame(6),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0014),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0012),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY, 0x0010),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0010),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0012),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0014),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0016),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x0018),
        Frame(10),
        Op((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY, 0x001a),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_RightLegMoving),
        Entry(InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0006),
        Entry(InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_QueueEggReleasedSFX),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnGoldenTorizoEgg),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1),
        Entry(InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0001),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_EyeBeamAttack_0, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Origin(0xd10d),
        Entry(InstList_GoldenTorizo_EyeBeamAttack_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_DisableEyeBeamExplosions),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_QueueLaserSFX),
        Entry(InstList_GoldenTorizo_EyeBeamAttack_1),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_SpawnEyeBeam, 0x0000),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0004),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_EyeBeamAttack_1),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0002),
        Entry(InstList_GoldenTorizo_EyeBeamAttack_2),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_GoldenTorizo_EyeBeamAttack_2),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_EnableEyeBeamExplosions),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0008),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_DisableEyeBeamExplosions),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Origin(0xd193),
        Entry(InstList_Torizo_Stunned_0),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Attacking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0018),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0002),
        Entry(InstList_Torizo_Stunned_1),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0003),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_Stunned_1),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0010),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_UnmarkStunned),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Skip(10),
        Entry(InstList_GoldenTorizo_Dodge_TurningLeft),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_RightLegMoving),
        Entry(InstList_GoldenTorizo_TurningLeft),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8),
        Entry(InstList_GoldenTorizo_WalkingLeft_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, InstList_Torizo_FacingLeft_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, InstList_Torizo_Stunned_0),
        Op((ushort)TorizoInstruction.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, InstList_GoldenTorizo_EyeBeamAttack_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0002),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0004),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0006),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0008),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000a),
        Entry(InstList_GoldenTorizo_WalkingLeft_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo, InstList_GoldenTorizo_SitDownAttack_FacingLeft),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, InstList_Torizo_FacingLeft_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, InstList_Torizo_Stunned_0),
        Op((ushort)TorizoInstruction.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, InstList_GoldenTorizo_EyeBeamAttack_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000c),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000e),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0010),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0012),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0000),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingLeft_RightLegMoving),
        Entry(InstList_GoldenTorizo_Dodge_TurningRight),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_LeftLegMoving),
        Entry(InstList_GoldenTorizo_TurningRight),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8),
        Entry(InstList_GoldenTorizo_WalkingRight_LeftLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithRightFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, InstList_Torizo_FacingRight_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, InstList_Torizo_Stunned_0),
        Op((ushort)TorizoInstruction.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, InstList_GoldenTorizo_EyeBeamAttack_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0016),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0018),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001a),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001c),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001e),
        Entry(InstList_GoldenTorizo_WalkingRight_RightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_GoldenTorizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, Function_GoldenTorizo_Movement_Walking),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo, InstList_GoldenTorizo_SitDownAttack_FacingRight),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, InstList_Torizo_FacingRight_JumpingForwards_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, InstList_Torizo_Stunned_0),
        Op((ushort)TorizoInstruction.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, InstList_GoldenTorizo_EyeBeamAttack_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0020),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0022),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0024),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0026),
        Frame(4),
        Op((ushort)TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0014),
        Op((ushort)CommonEnemyInstruction.Goto, InstList_GoldenTorizo_WalkingRight_LeftLegMoving),
    ];

    /// <summary>Shared instruction items at $AA:B0E5..B223; native list names identify each group.</summary>
    private static InstructionItem[] SharedItems =>
    [
        Origin(0xb0e5),
        Entry(InstList_Torizo_SpecialCallable_BlowUpBombTorizosGut),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, RTS_AAC6AB),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames, 0x0000),
        Op((ushort)TorizoInstruction.Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoGutExplosionLinkInstruction),
        Origin(0xb155),
        Entry(InstList_Torizo_Callable_BlowUpBombTorizosFace),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, RTS_AAC6AB),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames, 0x0006),
        Op((ushort)TorizoInstruction.Instruction_Torizo_MarkBombTorizoFaceBlownUp),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0001),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)CommonEnemyInstruction.CopyToVram),
        Skip(7),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, Function_Torizo_NormalMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_Return),
        Skip(10),
        Entry(InstList_Torizo_DeathSequence_0),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, RTS_AAC6AB),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0008),
        Entry(InstList_Torizo_DeathSequence_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0006),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_DeathSequence_1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToBlack),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x000e),
        Entry(InstList_Torizo_DeathSequence_2),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsVisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsInvisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsVisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsInvisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsVisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsInvisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsVisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAsInvisible),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0002),
        Op((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, InstList_Torizo_DeathSequence_2),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0040),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops),
        Op((ushort)CommonEnemyInstruction.StopScript),
    ];
}
