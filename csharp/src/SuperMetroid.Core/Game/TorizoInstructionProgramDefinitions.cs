using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Complete bounded Bomb/Golden Torizo instruction lists, compiled from the pinned NTSC cartridge.
/// Branches, callbacks, timers, and movement operands are engine definitions; tile pixels and
/// spritemap contents remain installed artwork. Packed DMA operands have their own typed catalog.
/// </summary>
internal abstract partial class TorizoInstructionProgramDefinitions
{
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

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        [.. SharedItems, .. BombLeftItems, .. BombRightItems, .. GoldenLeftItems, .. GoldenRightItems]);
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo instruction word $AA:{address:X4} has no compiled mechanics definition.");
}
