namespace SuperMetroid.Core.Game;

/// <summary>One simulation-owned bank-$AA instruction word; sprite operands are deliberately separate.</summary>
internal readonly record struct TorizoMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Complete bounded Bomb/Golden Torizo instruction lists, compiled from the pinned NTSC cartridge.
/// Branches, callbacks, timers, and movement operands are engine definitions; tile pixels and
/// spritemap contents remain installed artwork. Packed DMA operands have their own typed catalog.
/// </summary>
internal static partial class TorizoInstructionProgramDefinitions
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

    private static readonly TorizoMechanicsWord[] Words =
        [.. Shared, .. BombLeft, .. BombRight, .. GoldenLeft, .. GoldenRight];

    private static readonly ushort[] PresentationWords =
    [
        0xb87f, 0xb88d, 0xb89a, 0xb8a7, 0xb8b4, 0xb8c1, 0xb8c9, 0xb8d6,
        0xb8e3, 0xb8f0, 0xb901, 0xb909, 0xb911, 0xb919, 0xb921, 0xb929,
        0xb937, 0xb947, 0xb96a, 0xb97c, 0xb98c, 0xb998, 0xb9a4, 0xb9b0,
        0xb9c6, 0xb9d6, 0xb9e2, 0xb9ee, 0xb9fa, 0xba0a, 0xba0e, 0xba12,
        0xba16, 0xba1a, 0xba1e, 0xba32, 0xba36, 0xba3a, 0xba3e, 0xba4c,
        0xba50, 0xba54, 0xba58, 0xba5c, 0xba60, 0xba74, 0xba78, 0xba7c,
        0xba80, 0xba92, 0xba96, 0xba9a, 0xba9e, 0xbaa2, 0xbaa6, 0xbaaa,
        0xbaae, 0xbab6, 0xbaba, 0xbabe, 0xbac2, 0xbac6, 0xbaca, 0xbace,
        0xbad2, 0xbad6, 0xbada, 0xbae2, 0xbae6, 0xbafc, 0xbb00, 0xbb04,
        0xbb08, 0xbb0c, 0xbb10, 0xbb14, 0xbb18, 0xbb20, 0xbb24, 0xbb28,
        0xbb2c, 0xbb30, 0xbb34, 0xbb38, 0xbb3c, 0xbb40, 0xbb44, 0xbb4c,
        0xbb50, 0xbb62, 0xbb66, 0xbb6a, 0xbb6e, 0xbb72, 0xbb7a, 0xbb82,
        0xbb8a, 0xbb92, 0xbb9a, 0xbba2, 0xbba6, 0xbbaa, 0xbbae, 0xbbb2,
        0xbbba, 0xbbc2, 0xbbca, 0xbbd2, 0xbbd6, 0xbbe4, 0xbbe8, 0xbbec,
        0xbbf0, 0xbbf4, 0xbbfc, 0xbc04, 0xbc0c, 0xbc14, 0xbc1c, 0xbc24,
        0xbc28, 0xbc2c, 0xbc30, 0xbc34, 0xbc3c, 0xbc44, 0xbc4c, 0xbc54,
        0xbc58, 0xbc6a, 0xbc6e, 0xbc72, 0xbc82, 0xbca0, 0xbca4, 0xbca8,
        0xbcb8, 0xbcdc, 0xbce0, 0xbce4, 0xbcf4, 0xbd16, 0xbd28, 0xbd34,
        0xbd3c, 0xbd44, 0xbd4c, 0xbd62, 0xbd6e, 0xbd76, 0xbd7e, 0xbd86,
        0xbd94, 0xbda2, 0xbdaa, 0xbdb2, 0xbdba, 0xbdc2, 0xbdca, 0xbdd2,
        0xbde0, 0xbdf2, 0xbe02, 0xbe0e, 0xbe1a, 0xbe26, 0xbe40, 0xbe50,
        0xbe5c, 0xbe68, 0xbe74, 0xbe84, 0xbe88, 0xbe8c, 0xbe90, 0xbe94,
        0xbe98, 0xbeac, 0xbeb0, 0xbeb4, 0xbeb8, 0xbec6, 0xbeca, 0xbece,
        0xbed2, 0xbed6, 0xbeda, 0xbeee, 0xbef2, 0xbef6, 0xbefa, 0xbf0c,
        0xbf10, 0xbf14, 0xbf18, 0xbf1c, 0xbf20, 0xbf24, 0xbf28, 0xbf30,
        0xbf34, 0xbf38, 0xbf3c, 0xbf40, 0xbf44, 0xbf48, 0xbf4c, 0xbf50,
        0xbf54, 0xbf5c, 0xbf60, 0xbf76, 0xbf7a, 0xbf7e, 0xbf82, 0xbf86,
        0xbf8a, 0xbf8e, 0xbf92, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa,
        0xbfae, 0xbfb2, 0xbfb6, 0xbfba, 0xbfbe, 0xbfc6, 0xbfca, 0xbfdc,
        0xbfe0, 0xbfe4, 0xbfe8, 0xbfec, 0xbff4, 0xbffc, 0xc004, 0xc00c,
        0xc014, 0xc01c, 0xc020, 0xc024, 0xc028, 0xc02c, 0xc034, 0xc03c,
        0xc044, 0xc04c, 0xc050, 0xc05e, 0xc062, 0xc066, 0xc06a, 0xc06e,
        0xc076, 0xc07e, 0xc086, 0xc08e, 0xc096, 0xc09e, 0xc0a2, 0xc0a6,
        0xc0aa, 0xc0ae, 0xc0b6, 0xc0be, 0xc0c6, 0xc0ce, 0xc0d2, 0xc0e4,
        0xc0e8, 0xc0ec, 0xc0fc, 0xc11a, 0xc11e, 0xc122, 0xc132, 0xc156,
        0xc15a, 0xc15e, 0xc16e, 0xc190, 0xc1a2, 0xc1ae, 0xc1b6, 0xc1be,
        0xc1c6, 0xc1dc, 0xc1e8, 0xc1f0, 0xc1f8, 0xc200, 0xc9de, 0xc9e8,
        0xc9f2, 0xc9fa, 0xca02, 0xca0e, 0xca1b, 0xca28, 0xca35, 0xca42,
        0xca4a, 0xca57, 0xca64, 0xca71, 0xca82, 0xca8a, 0xca92, 0xca9a,
        0xcaa2, 0xcaaa, 0xcab8, 0xcac8, 0xcb05, 0xcb09, 0xcb0d, 0xcb11,
        0xcb15, 0xcb19, 0xcb2d, 0xcb31, 0xcb35, 0xcb39, 0xcb47, 0xcb4b,
        0xcb4f, 0xcb53, 0xcb57, 0xcb5b, 0xcb6f, 0xcb73, 0xcb77, 0xcb7b,
        0xcb8d, 0xcb91, 0xcb95, 0xcb99, 0xcb9d, 0xcba1, 0xcba5, 0xcba9,
        0xcbb1, 0xcbb5, 0xcbb9, 0xcbbd, 0xcbc1, 0xcbc5, 0xcbc9, 0xcbcd,
        0xcbd1, 0xcbd5, 0xcbdd, 0xcbe1, 0xcbf7, 0xcbfb, 0xcbff, 0xcc03,
        0xcc07, 0xcc0b, 0xcc0f, 0xcc13, 0xcc1b, 0xcc1f, 0xcc23, 0xcc27,
        0xcc2b, 0xcc2f, 0xcc33, 0xcc37, 0xcc3b, 0xcc3f, 0xcc47, 0xcc4b,
        0xcc5d, 0xcc61, 0xcc65, 0xcc69, 0xcc6d, 0xcc71, 0xcc85, 0xcc89,
        0xcc8d, 0xcc91, 0xcc9f, 0xcca3, 0xcca7, 0xccab, 0xccaf, 0xccb3,
        0xccc7, 0xcccb, 0xcccf, 0xccd3, 0xcce5, 0xcce9, 0xcced, 0xccf1,
        0xccf5, 0xccf9, 0xccfd, 0xcd01, 0xcd09, 0xcd0d, 0xcd11, 0xcd15,
        0xcd19, 0xcd1d, 0xcd21, 0xcd25, 0xcd29, 0xcd2d, 0xcd35, 0xcd39,
        0xcd4f, 0xcd53, 0xcd57, 0xcd5b, 0xcd5f, 0xcd63, 0xcd67, 0xcd6b,
        0xcd73, 0xcd77, 0xcd7b, 0xcd7f, 0xcd83, 0xcd87, 0xcd8b, 0xcd8f,
        0xcd93, 0xcd97, 0xcd9f, 0xcda3, 0xcde9, 0xcded, 0xcdf1, 0xcdf5,
        0xcdf9, 0xcdfd, 0xce01, 0xce05, 0xce09, 0xce0d, 0xce11, 0xce15,
        0xce19, 0xce1d, 0xce23, 0xce27, 0xce2b, 0xce2f, 0xce33, 0xce37,
        0xce3b, 0xce4b, 0xce4f, 0xce53, 0xce57, 0xce5b, 0xce5f, 0xce63,
        0xce67, 0xce6b, 0xce6f, 0xce73, 0xce77, 0xce7b, 0xce7f, 0xce85,
        0xce89, 0xce8d, 0xce91, 0xce95, 0xce99, 0xce9d, 0xcead, 0xceb1,
        0xceb5, 0xceb9, 0xcebd, 0xcec1, 0xcec5, 0xcec9, 0xcecd, 0xced1,
        0xced5, 0xced9, 0xcedf, 0xcee3, 0xcee7, 0xceeb, 0xceef, 0xcef3,
        0xcef7, 0xcf07, 0xcf0b, 0xcf0f, 0xcf13, 0xcf17, 0xcf1b, 0xcf1f,
        0xcf23, 0xcf27, 0xcf2b, 0xcf2f, 0xcf33, 0xcf39, 0xcf3d, 0xcf41,
        0xcf45, 0xcf49, 0xcf4d, 0xcf51, 0xcf5f, 0xcf67, 0xcf6f, 0xcf77,
        0xcf7f, 0xcf87, 0xcf8f, 0xcf97, 0xcf9f, 0xcfa7, 0xcfaf, 0xcfb7,
        0xcfcb, 0xcfd3, 0xcfdb, 0xcfe3, 0xcfeb, 0xcff3, 0xcffb, 0xd003,
        0xd00b, 0xd013, 0xd01b, 0xd023, 0xd1fb, 0xd20b, 0xd21b, 0xd233,
        0xd23b, 0xd24b, 0xd253, 0xd267, 0xd283, 0xd28b, 0xd29b, 0xd2a3,
        0xd2b7, 0xd2c7, 0xd2d7, 0xd2ef, 0xd2f7, 0xd303, 0xd30f, 0xd323,
        0xd33f, 0xd347, 0xd357, 0xd35f,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static TorizoMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address) =>
        TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException($"Torizo instruction word $AA:{address:X4} has no compiled mechanics definition.");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoMechanicsWord word = Words[middle];
            if (word.Address == address) { value = word.Value; return true; }
            if (word.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address) =>
        (address >> 16) == Bank &&
        (TryReadMechanicsWord(unchecked((ushort)address), out _) ||
         TryReadMechanicsWord(unchecked((ushort)(address - 1)), out _));
}
