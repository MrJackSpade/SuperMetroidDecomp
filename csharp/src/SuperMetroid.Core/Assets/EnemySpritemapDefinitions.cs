using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named visual frame selected by an otherwise compiled enemy program.</summary>
internal readonly record struct EnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);

/// <summary>
/// Stock identities for installed enemy compositions. These are visual frame selections,
/// not editable instruction timers, AI callbacks, collision or hitbox definitions.
/// </summary>
internal static class EnemySpritemapDefinitions
{
    /// <summary>Schema before Ridley's eight ordinary breakup body compositions.</summary>
    internal const int PreRidleyBreakupVersion = 68;
    internal const int PreRidleyBreakupFrameCount = 1404;
    /// <summary>Schema before the single-frame Kzan and Polyp compositions were installed.</summary>
    internal const int PreSingleFrameVersion = 67;
    internal const int PreSingleFrameFrameCount = 1402;
    /// <summary>Schema before Puromi/Nuclear Waffle's eight head frames were installed.</summary>
    internal const int PreNuclearWaffleVersion = 66;
    internal const int PreNuclearWaffleFrameCount = 1394;
    /// <summary>Schema before Kraid's two ordinary belly-lint frames were installed.</summary>
    internal const int PreKraidLintVersion = 65;
    internal const int PreKraidLintFrameCount = 1392;
    /// <summary>Schema before audit-identified ordinary environmental/Tourian artwork additions.</summary>
    internal const int PreAuditOrdinaryVersion = 64;
    internal const int PreAuditOrdinaryFrameCount = 1339;
    /// <summary>Schema before the friendly-animal compositions; existing edits remain valid.</summary>
    internal const int PreFriendlyAnimalVersion = 63;
    internal const int PreFriendlyAnimalFrameCount = 1257;
    /// <summary>Schema before the Zero crawler; existing edits and bindings remain valid.</summary>
    internal const int PreZeroVersion = 62;
    internal const int PreZeroFrameCount = 1241;
    /// <summary>Schema before the tatori family; previous compositions and bindings remain editable.</summary>
    internal const int PreMamaTurtleVersion = 61;
    internal const int PreMamaTurtleFrameCount = 1212;
    internal const int PreGunshipVersion = 60;
    internal const int PreGunshipFrameCount = 1199;
    internal const int PreBotwoonVersion = 59;
    internal const int PreBotwoonFrameCount = 1183;
    internal const int PreYardVersion = 58;
    internal const int PreYardFrameCount = 1079;
    internal const int PreWorkRobotVersion = 57;
    internal const int PreWorkRobotFrameCount = 1052;
    internal const int PreEvirVersion = 56;
    internal const int PreEvirFrameCount = 1028;
    internal const int PreMochtroidVersion = 55;
    internal const int PreMochtroidFrameCount = 1022;
    internal const int PreDeadTourianCorpseVersion = 54;
    internal const int PreDeadTourianCorpseFrameCount = 1009;
    internal const int PreDeadTorizoStationaryVersion = 53;
    internal const int PreDeadTorizoStationaryFrameCount = 1008;
    internal const int Version = 69;
    internal const int PreRinkaVersion = 52;
    internal const int PreRinkaFrameCount = 1003;
    internal const int PreViolaVersion = 51;
    internal const int PreViolaFrameCount = 995;
    internal const int PreChozoStatueVersion = 50;
    internal const int PreChozoStatueFrameCount = 969;
    internal const int PreNorfairLavaJumperVersion = 49;
    internal const int PreNorfairLavaJumperFrameCount = 958;
    internal const int PreMultiviolaVersion = 48;
    internal const int PreMultiviolaFrameCount = 950;
    internal const int PreDragonVersion = 47;
    internal const int PreDragonFrameCount = 938;
    internal const int PreTripperKamerVersion = 46;
    internal const int PreTripperKamerFrameCount = 920;
    internal const int PreShaktoolVersion = 45;
    internal const int PreShaktoolFrameCount = 905;
    internal const int PreMetroidVersion = 44;
    internal const int PreMetroidFrameCount = 901;
    internal const int PreShutterVersion = 43;
    internal const int PreShutterFrameCount = 896;
    internal const int PreMorphBallEyeVersion = 42;
    internal const int PreMorphBallEyeFrameCount = 874;
    internal const int PreFaceBlockVersion = 41;
    internal const int PreFaceBlockFrameCount = 869;
    internal const int PreKagoVersion = 40;
    internal const int PreKagoFrameCount = 866;
    internal const int PreFlyVersion = 39;
    internal const int PreFlyFrameCount = 862;
    internal const int PreSciserVersion = 38;
    internal const int PreSciserFrameCount = 850;
    internal const int PreRidleySupplementVersion = 37;
    internal const int PreRidleySupplementFrameCount = 819;
    internal const int PreDeadTorizoVersion = 36;
    internal const int PreDeadTorizoFrameCount = 818;
    internal const int PreMotherBrainVersion = 35;
    internal const int PreMotherBrainFrameCount = 800;
    internal const int PreKiHunterVersion = 34;
    internal const int PreKiHunterFrameCount = 759;
    internal const int PreYappingMawVersion = 33;
    internal const int PreYappingMawFrameCount = 735;
    internal const int PreRoomSpriteObjectVersion = 32;
    internal const int PreRoomSpriteObjectFrameCount = 472;
    internal const int PreDraygonBreathVersion = 31;
    internal const int PreDraygonBreathFrameCount = 463;
    internal const int PreDraygonIntroVersion = 30;
    internal const int PreDraygonIntroFrameCount = 459;
    internal const int PreElevatorVersion = 29;
    internal const int PreElevatorFrameCount = 457;
    internal const int PreKamerVersion = 28;
    internal const int PreKamerFrameCount = 453;
    internal const int PreFuneNamiheVersion = 27;
    internal const int PreFuneNamiheFrameCount = 431;
    internal const int PreSbugVersion = 26;
    internal const int PreSbugFrameCount = 407;
    internal const int PreHZoomerVersion = 25;
    internal const int PreHZoomerFrameCount = 387;
    internal const int PreChootVersion = 24;
    internal const int PreChootFrameCount = 383;
    internal const int PreHopperVersion = 23;
    internal const int PreHopperFrameCount = 359;
    internal const int PreBeetomVersion = 22;
    internal const int PreBeetomFrameCount = 337;
    internal const int PreAlcoonVersion = 21;
    internal const int PreAlcoonFrameCount = 319;
    internal const int PreBullVersion = 20;
    internal const int PreBullFrameCount = 316;
    internal const int PrePuyoVersion = 19;
    internal const int PrePuyoFrameCount = 308;
    internal const int PreNorfairRioVersion = 18;
    internal const int PreNorfairRioFrameCount = 288;
    internal const int PreLowerNorfairRioVersion = 17;
    internal const int PreLowerNorfairRioFrameCount = 270;
    internal const int PreRioVersion = 16;
    internal const int PreRioFrameCount = 262;
    internal const int PreCeresBabyVersion = 15;
    internal const int PreCeresBabyFrameCount = 259;
    internal const int PreCeresDoorVersion = 14;
    internal const int PreCeresDoorFrameCount = 244;
    /// <summary>Last art-only composition schema; later accepted schemas own editable display bindings.</summary>
    internal const int PreDisplayBindingsVersion = 13;
    internal const int PreDisplayBindingsFrameCount = 244;
    internal const int PreMagdolliteVersion = 12;
    internal const int PreMagdolliteFrameCount = 215;
    internal const int PreFirefleaVersion = 11;
    internal const int PreFirefleaFrameCount = 194;
    internal const int PreRipperVersion = 10;
    internal const int PreRipperFrameCount = 180;
    internal const int PreOwtchStokeVersion = 9;
    internal const int PreOwtchStokeFrameCount = 167;
    internal const int PreviousVersion = 8;
    internal const int PreviousFrameCount = 145;
    internal const int PriorVersion = 7;
    internal const int PriorFrameCount = 101;
    internal const int EarlierVersion = 6;
    internal const int EarlierFrameCount = 79;
    internal const int IntermediateVersion = 5;
    internal const int IntermediateFrameCount = 69;
    internal const int LegacyVersion = 4;
    internal const int LegacyFrameCount = 47;
    internal const string FileName = "enemy-compositions.json";
    internal const byte BoyonBank = 0xa2;
    internal const byte RioBank = 0xa2;
    internal const byte LowerNorfairRioBank = 0xa2;
    internal const byte NorfairRioBank = 0xa2;
    internal const byte PuyoBank = 0xa2;
    internal const byte BullBank = 0xa8;
    internal const byte AlcoonBank = 0xa8;
    internal const byte BeetomBank = 0xa8;
    internal const byte HopperBank = 0xa3;
    internal const byte ChootBank = 0xa2;
    internal const byte HZoomerBank = 0xa3;
    internal const byte SbugBank = 0xa3;
    internal const byte FuneNamiheBank = 0xa8;
    internal const byte KamerPlatformBank = 0xa2;
    internal const byte ElevatorBank = 0xa3;
    internal const byte RoomSpriteObjectBank = 0xb4;
    internal const byte SkulteraBank = 0xa3;
    internal const byte WaverBank = 0xa3;
    internal const byte ZoaBank = 0xa3;
    internal const byte SkreeMetareeBank = 0xa3;
    internal const byte PipeBugBank = 0xb3;
    internal const byte FakeKraidBank = 0xa6;
    internal const byte KraidNailBank = 0xa7;
    internal const byte OwtchStokeBank = 0xa2;
    internal const byte RipperBank = 0xa2;
    internal const byte FirefleaBank = 0xa3;
    internal const byte MagdolliteBank = 0xa8;
    internal const byte BoulderBank = 0xa6;
    internal const byte AtomicBank = 0xa8;
    internal const int MaximumParts = 128;
    internal const int TileColumns = 16;
    internal const int TileRows = 32;

    // These mutually exclusive identities preserve the historical installed-schema order.
    private enum NamedFrameId
    {
        /// <summary>$A2:88DA: boyon_idle_0 visual identity in the installed frame schema.</summary>
        boyon_idle_0,
        /// <summary>$A2:88E1: boyon_idle_1 visual identity in the installed frame schema.</summary>
        boyon_idle_1,
        /// <summary>$A2:88E8: boyon_idle_2 visual identity in the installed frame schema.</summary>
        boyon_idle_2,
        /// <summary>$A2:88EF: boyon_bounce_0 visual identity in the installed frame schema.</summary>
        boyon_bounce_0,
        /// <summary>$A2:88F6: boyon_bounce_1 visual identity in the installed frame schema.</summary>
        boyon_bounce_1,
        /// <summary>$A2:88FD: boyon_bounce_2 visual identity in the installed frame schema.</summary>
        boyon_bounce_2,
        /// <summary>$A2:8904: boyon_bounce_3 visual identity in the installed frame schema.</summary>
        boyon_bounce_3,
        /// <summary>$A2:A0BB: cacatac_upright_idle_0 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_0,
        /// <summary>$A2:A0DB: cacatac_upright_idle_1 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_1,
        /// <summary>$A2:A0FB: cacatac_upright_idle_2 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_2,
        /// <summary>$A2:A11B: cacatac_upright_idle_3 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_3,
        /// <summary>$A2:A13B: cacatac_upright_idle_4 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_4,
        /// <summary>$A2:A15B: cacatac_upright_idle_5 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_5,
        /// <summary>$A2:A17B: cacatac_upright_idle_6 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_6,
        /// <summary>$A2:A19B: cacatac_upright_idle_7 visual identity in the installed frame schema.</summary>
        cacatac_upright_idle_7,
        /// <summary>$A2:A1BB: cacatac_upright_attack_1 visual identity in the installed frame schema.</summary>
        cacatac_upright_attack_1,
        /// <summary>$A2:A1EF: cacatac_upright_attack_2 visual identity in the installed frame schema.</summary>
        cacatac_upright_attack_2,
        /// <summary>$A2:A223: cacatac_inverted_idle_0 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_0,
        /// <summary>$A2:A243: cacatac_inverted_idle_1 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_1,
        /// <summary>$A2:A263: cacatac_inverted_idle_2 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_2,
        /// <summary>$A2:A283: cacatac_inverted_idle_3 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_3,
        /// <summary>$A2:A2A3: cacatac_inverted_idle_4 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_4,
        /// <summary>$A2:A2C3: cacatac_inverted_idle_5 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_5,
        /// <summary>$A2:A2E3: cacatac_inverted_idle_6 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_6,
        /// <summary>$A2:A303: cacatac_inverted_idle_7 visual identity in the installed frame schema.</summary>
        cacatac_inverted_idle_7,
        /// <summary>$A2:A323: cacatac_inverted_attack_1 visual identity in the installed frame schema.</summary>
        cacatac_inverted_attack_1,
        /// <summary>$A2:A357: cacatac_inverted_attack_2 visual identity in the installed frame schema.</summary>
        cacatac_inverted_attack_2,
        /// <summary>$A6:8A59: boulder_roll_0 visual identity in the installed frame schema.</summary>
        boulder_roll_0,
        /// <summary>$A6:8A6F: boulder_roll_1 visual identity in the installed frame schema.</summary>
        boulder_roll_1,
        /// <summary>$A6:8A85: boulder_roll_2 visual identity in the installed frame schema.</summary>
        boulder_roll_2,
        /// <summary>$A6:8A9B: boulder_roll_3 visual identity in the installed frame schema.</summary>
        boulder_roll_3,
        /// <summary>$A6:8AB1: boulder_roll_4 visual identity in the installed frame schema.</summary>
        boulder_roll_4,
        /// <summary>$A6:8AC7: boulder_roll_5 visual identity in the installed frame schema.</summary>
        boulder_roll_5,
        /// <summary>$A6:8ADD: boulder_roll_6 visual identity in the installed frame schema.</summary>
        boulder_roll_6,
        /// <summary>$A6:8AF3: boulder_roll_7 visual identity in the installed frame schema.</summary>
        boulder_roll_7,
        /// <summary>$A8:E489: atomic_up_right_0 visual identity in the installed frame schema.</summary>
        atomic_up_right_0,
        /// <summary>$A8:E49F: atomic_up_right_1 visual identity in the installed frame schema.</summary>
        atomic_up_right_1,
        /// <summary>$A8:E4B5: atomic_up_right_2 visual identity in the installed frame schema.</summary>
        atomic_up_right_2,
        /// <summary>$A8:E4CB: atomic_up_right_3 visual identity in the installed frame schema.</summary>
        atomic_up_right_3,
        /// <summary>$A8:E4E1: atomic_up_right_4 visual identity in the installed frame schema.</summary>
        atomic_up_right_4,
        /// <summary>$A8:E4F2: atomic_up_right_5 visual identity in the installed frame schema.</summary>
        atomic_up_right_5,
        /// <summary>$A8:E508: atomic_up_left_0 visual identity in the installed frame schema.</summary>
        atomic_up_left_0,
        /// <summary>$A8:E51E: atomic_up_left_1 visual identity in the installed frame schema.</summary>
        atomic_up_left_1,
        /// <summary>$A8:E534: atomic_up_left_2 visual identity in the installed frame schema.</summary>
        atomic_up_left_2,
        /// <summary>$A8:E54A: atomic_up_left_3 visual identity in the installed frame schema.</summary>
        atomic_up_left_3,
        /// <summary>$A8:E560: atomic_up_left_4 visual identity in the installed frame schema.</summary>
        atomic_up_left_4,
        /// <summary>$A8:E571: atomic_up_left_5 visual identity in the installed frame schema.</summary>
        atomic_up_left_5,
        /// <summary>$A3:928A: skultera_swim_left_0 visual identity in the installed frame schema.</summary>
        skultera_swim_left_0,
        /// <summary>$A3:92A5: skultera_swim_left_1 visual identity in the installed frame schema.</summary>
        skultera_swim_left_1,
        /// <summary>$A3:92C0: skultera_swim_left_2 visual identity in the installed frame schema.</summary>
        skultera_swim_left_2,
        /// <summary>$A3:92DB: skultera_turn_right_0 visual identity in the installed frame schema.</summary>
        skultera_turn_right_0,
        /// <summary>$A3:92F6: skultera_turn_right_1 visual identity in the installed frame schema.</summary>
        skultera_turn_right_1,
        /// <summary>$A3:9311: skultera_turn_right_2 visual identity in the installed frame schema.</summary>
        skultera_turn_right_2,
        /// <summary>$A3:9327: skultera_turn_right_3 visual identity in the installed frame schema.</summary>
        skultera_turn_right_3,
        /// <summary>$A3:933D: skultera_turn_right_4 visual identity in the installed frame schema.</summary>
        skultera_turn_right_4,
        /// <summary>$A3:934E: skultera_turn_right_5 visual identity in the installed frame schema.</summary>
        skultera_turn_right_5,
        /// <summary>$A3:9364: skultera_turn_right_6 visual identity in the installed frame schema.</summary>
        skultera_turn_right_6,
        /// <summary>$A3:937F: skultera_turn_right_7 visual identity in the installed frame schema.</summary>
        skultera_turn_right_7,
        /// <summary>$A3:939A: skultera_swim_right_0 visual identity in the installed frame schema.</summary>
        skultera_swim_right_0,
        /// <summary>$A3:93B5: skultera_swim_right_1 visual identity in the installed frame schema.</summary>
        skultera_swim_right_1,
        /// <summary>$A3:93D0: skultera_swim_right_2 visual identity in the installed frame schema.</summary>
        skultera_swim_right_2,
        /// <summary>$A3:93EB: skultera_turn_left_0 visual identity in the installed frame schema.</summary>
        skultera_turn_left_0,
        /// <summary>$A3:9406: skultera_turn_left_1 visual identity in the installed frame schema.</summary>
        skultera_turn_left_1,
        /// <summary>$A3:9421: skultera_turn_left_2 visual identity in the installed frame schema.</summary>
        skultera_turn_left_2,
        /// <summary>$A3:9437: skultera_turn_left_3 visual identity in the installed frame schema.</summary>
        skultera_turn_left_3,
        /// <summary>$A3:944D: skultera_turn_left_4 visual identity in the installed frame schema.</summary>
        skultera_turn_left_4,
        /// <summary>$A3:945E: skultera_turn_left_5 visual identity in the installed frame schema.</summary>
        skultera_turn_left_5,
        /// <summary>$A3:9474: skultera_turn_left_6 visual identity in the installed frame schema.</summary>
        skultera_turn_left_6,
        /// <summary>$A3:948F: skultera_turn_left_7 visual identity in the installed frame schema.</summary>
        skultera_turn_left_7,
        /// <summary>$A3:884A: waver_steady_left visual identity in the installed frame schema.</summary>
        waver_steady_left,
        /// <summary>$A3:88B3: waver_steady_right visual identity in the installed frame schema.</summary>
        waver_steady_right,
        /// <summary>$A3:885B: waver_spin_left_0 visual identity in the installed frame schema.</summary>
        waver_spin_left_0,
        /// <summary>$A3:8871: waver_spin_left_1 visual identity in the installed frame schema.</summary>
        waver_spin_left_1,
        /// <summary>$A3:881E: waver_spin_left_2 visual identity in the installed frame schema.</summary>
        waver_spin_left_2,
        /// <summary>$A3:8834: waver_spin_left_3 visual identity in the installed frame schema.</summary>
        waver_spin_left_3,
        /// <summary>$A3:88C4: waver_spin_right_0 visual identity in the installed frame schema.</summary>
        waver_spin_right_0,
        /// <summary>$A3:88DA: waver_spin_right_1 visual identity in the installed frame schema.</summary>
        waver_spin_right_1,
        /// <summary>$A3:8887: waver_spin_right_2 visual identity in the installed frame schema.</summary>
        waver_spin_right_2,
        /// <summary>$A3:889D: waver_spin_right_3 visual identity in the installed frame schema.</summary>
        waver_spin_right_3,
        /// <summary>$A3:8B65: metaree_idle_0 visual identity in the installed frame schema.</summary>
        metaree_idle_0,
        /// <summary>$A3:8BB9: metaree_idle_1 visual identity in the installed frame schema.</summary>
        metaree_idle_1,
        /// <summary>$A3:8BDE: metaree_idle_2 visual identity in the installed frame schema.</summary>
        metaree_idle_2,
        /// <summary>$A3:8BEA: metaree_idle_3 visual identity in the installed frame schema.</summary>
        metaree_idle_3,
        /// <summary>$A3:8B94: metaree_prepare_1 visual identity in the installed frame schema.</summary>
        metaree_prepare_1,
        /// <summary>$A3:C842: skree_idle_0 visual identity in the installed frame schema.</summary>
        skree_idle_0,
        /// <summary>$A3:C878: skree_idle_1 visual identity in the installed frame schema.</summary>
        skree_idle_1,
        /// <summary>$A3:C884: skree_idle_2 visual identity in the installed frame schema.</summary>
        skree_idle_2,
        /// <summary>$A3:C89A: skree_idle_3 visual identity in the installed frame schema.</summary>
        skree_idle_3,
        /// <summary>$A3:C862: skree_prepare_1 visual identity in the installed frame schema.</summary>
        skree_prepare_1,
        /// <summary>$A3:B55F: zoa_shoot_left_0 visual identity in the installed frame schema.</summary>
        zoa_shoot_left_0,
        /// <summary>$A3:B566: zoa_shoot_left_1 visual identity in the installed frame schema.</summary>
        zoa_shoot_left_1,
        /// <summary>$A3:B56D: zoa_shoot_left_2 visual identity in the installed frame schema.</summary>
        zoa_shoot_left_2,
        /// <summary>$A3:B57B: zoa_rise_left_0 visual identity in the installed frame schema.</summary>
        zoa_rise_left_0,
        /// <summary>$A3:B574: zoa_rise_left_1 visual identity in the installed frame schema.</summary>
        zoa_rise_left_1,
        /// <summary>$A3:B582: zoa_rise_left_2 visual identity in the installed frame schema.</summary>
        zoa_rise_left_2,
        /// <summary>$A3:B589: zoa_shoot_right_0 visual identity in the installed frame schema.</summary>
        zoa_shoot_right_0,
        /// <summary>$A3:B590: zoa_shoot_right_1 visual identity in the installed frame schema.</summary>
        zoa_shoot_right_1,
        /// <summary>$A3:B597: zoa_shoot_right_2 visual identity in the installed frame schema.</summary>
        zoa_shoot_right_2,
        /// <summary>$A3:B5A5: zoa_rise_right_0 visual identity in the installed frame schema.</summary>
        zoa_rise_right_0,
        /// <summary>$A3:B59E: zoa_rise_right_1 visual identity in the installed frame schema.</summary>
        zoa_rise_right_1,
        /// <summary>$A3:B5AC: zoa_rise_right_2 visual identity in the installed frame schema.</summary>
        zoa_rise_right_2,
        /// <summary>$B3:89B7: pipe_brinstar_normal_left_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_left_0,
        /// <summary>$B3:89BE: pipe_brinstar_normal_left_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_left_1,
        /// <summary>$B3:89C5: pipe_brinstar_normal_left_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_left_2,
        /// <summary>$B3:89CC: pipe_brinstar_normal_left_3 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_left_3,
        /// <summary>$B3:89D3: pipe_brinstar_normal_left_4 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_left_4,
        /// <summary>$B3:89DA: pipe_brinstar_normal_right_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_right_0,
        /// <summary>$B3:89E1: pipe_brinstar_normal_right_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_right_1,
        /// <summary>$B3:89E8: pipe_brinstar_normal_right_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_right_2,
        /// <summary>$B3:89EF: pipe_brinstar_normal_right_3 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_right_3,
        /// <summary>$B3:89F6: pipe_brinstar_normal_right_4 visual identity in the installed frame schema.</summary>
        pipe_brinstar_normal_right_4,
        /// <summary>$B3:8A6D: pipe_brinstar_strong_shoot_left_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_left_0,
        /// <summary>$B3:8A74: pipe_brinstar_strong_shoot_left_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_left_1,
        /// <summary>$B3:8A7B: pipe_brinstar_strong_shoot_left_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_left_2,
        /// <summary>$B3:8A82: pipe_brinstar_strong_rise_left_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_left_0,
        /// <summary>$B3:8A89: pipe_brinstar_strong_rise_left_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_left_1,
        /// <summary>$B3:8A90: pipe_brinstar_strong_rise_left_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_left_2,
        /// <summary>$B3:8A97: pipe_brinstar_strong_shoot_right_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_right_0,
        /// <summary>$B3:8A9E: pipe_brinstar_strong_shoot_right_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_right_1,
        /// <summary>$B3:8AA5: pipe_brinstar_strong_shoot_right_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_shoot_right_2,
        /// <summary>$B3:8AAC: pipe_brinstar_strong_rise_right_0 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_right_0,
        /// <summary>$B3:8AB3: pipe_brinstar_strong_rise_right_1 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_right_1,
        /// <summary>$B3:8ABA: pipe_brinstar_strong_rise_right_2 visual identity in the installed frame schema.</summary>
        pipe_brinstar_strong_rise_right_2,
        /// <summary>$B3:8E96: pipe_norfair_left_0 visual identity in the installed frame schema.</summary>
        pipe_norfair_left_0,
        /// <summary>$B3:8E9D: pipe_norfair_left_1 visual identity in the installed frame schema.</summary>
        pipe_norfair_left_1,
        /// <summary>$B3:8EA4: pipe_norfair_left_2 visual identity in the installed frame schema.</summary>
        pipe_norfair_left_2,
        /// <summary>$B3:8EAB: pipe_norfair_left_3 visual identity in the installed frame schema.</summary>
        pipe_norfair_left_3,
        /// <summary>$B3:8EB2: pipe_norfair_left_4 visual identity in the installed frame schema.</summary>
        pipe_norfair_left_4,
        /// <summary>$B3:8EB9: pipe_norfair_right_0 visual identity in the installed frame schema.</summary>
        pipe_norfair_right_0,
        /// <summary>$B3:8EC0: pipe_norfair_right_1 visual identity in the installed frame schema.</summary>
        pipe_norfair_right_1,
        /// <summary>$B3:8EC7: pipe_norfair_right_2 visual identity in the installed frame schema.</summary>
        pipe_norfair_right_2,
        /// <summary>$B3:8ECE: pipe_norfair_right_3 visual identity in the installed frame schema.</summary>
        pipe_norfair_right_3,
        /// <summary>$B3:8ED5: pipe_norfair_right_4 visual identity in the installed frame schema.</summary>
        pipe_norfair_right_4,
        /// <summary>$B3:92AD: pipe_yellow_fly_left_0 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_left_0,
        /// <summary>$B3:92B4: pipe_yellow_fly_left_1 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_left_1,
        /// <summary>$B3:92BB: pipe_yellow_fly_left_2 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_left_2,
        /// <summary>$B3:92C2: pipe_yellow_arc_left_0 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_left_0,
        /// <summary>$B3:92C9: pipe_yellow_arc_left_1 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_left_1,
        /// <summary>$B3:92D0: pipe_yellow_arc_left_2 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_left_2,
        /// <summary>$B3:92D7: pipe_yellow_fly_right_0 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_right_0,
        /// <summary>$B3:92DE: pipe_yellow_fly_right_1 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_right_1,
        /// <summary>$B3:92E5: pipe_yellow_fly_right_2 visual identity in the installed frame schema.</summary>
        pipe_yellow_fly_right_2,
        /// <summary>$B3:92EC: pipe_yellow_arc_right_0 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_right_0,
        /// <summary>$B3:92F3: pipe_yellow_arc_right_1 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_right_1,
        /// <summary>$B3:92FA: pipe_yellow_arc_right_2 visual identity in the installed frame schema.</summary>
        pipe_yellow_arc_right_2,
        /// <summary>$A6:9C64: fake_kraid_walk_left_0 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_left_0,
        /// <summary>$A6:9CB6: fake_kraid_walk_left_1 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_left_1,
        /// <summary>$A6:9D08: fake_kraid_walk_left_2 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_left_2,
        /// <summary>$A6:9D5A: fake_kraid_walk_left_3 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_left_3,
        /// <summary>$A6:9DAC: fake_kraid_spit_left_0 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_left_0,
        /// <summary>$A6:9DFE: fake_kraid_spit_left_1 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_left_1,
        /// <summary>$A6:9E50: fake_kraid_spit_left_2 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_left_2,
        /// <summary>$A6:9EA2: fake_kraid_walk_right_0 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_right_0,
        /// <summary>$A6:9EF4: fake_kraid_walk_right_1 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_right_1,
        /// <summary>$A6:9F46: fake_kraid_walk_right_2 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_right_2,
        /// <summary>$A6:9F98: fake_kraid_walk_right_3 visual identity in the installed frame schema.</summary>
        fake_kraid_walk_right_3,
        /// <summary>$A6:9FEA: fake_kraid_spit_right_0 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_right_0,
        /// <summary>$A6:A03C: fake_kraid_spit_right_1 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_right_1,
        /// <summary>$A6:A08E: fake_kraid_spit_right_2 visual identity in the installed frame schema.</summary>
        fake_kraid_spit_right_2,
        /// <summary>$A7:A617: kraid_nail_0 visual identity in the installed frame schema.</summary>
        kraid_nail_0,
        /// <summary>$A7:A623: kraid_nail_1 visual identity in the installed frame schema.</summary>
        kraid_nail_1,
        /// <summary>$A7:A639: kraid_nail_2 visual identity in the installed frame schema.</summary>
        kraid_nail_2,
        /// <summary>$A7:A645: kraid_nail_3 visual identity in the installed frame schema.</summary>
        kraid_nail_3,
        /// <summary>$A7:A65B: kraid_nail_4 visual identity in the installed frame schema.</summary>
        kraid_nail_4,
        /// <summary>$A7:A667: kraid_nail_5 visual identity in the installed frame schema.</summary>
        kraid_nail_5,
        /// <summary>$A7:A67D: kraid_nail_6 visual identity in the installed frame schema.</summary>
        kraid_nail_6,
        /// <summary>$A7:A689: kraid_nail_7 visual identity in the installed frame schema.</summary>
        kraid_nail_7,
        /// <summary>$A2:A589: owtch_left_0 visual identity in the installed frame schema.</summary>
        owtch_left_0,
        /// <summary>$A2:A590: owtch_left_1 visual identity in the installed frame schema.</summary>
        owtch_left_1,
        /// <summary>$A2:A597: owtch_left_2 visual identity in the installed frame schema.</summary>
        owtch_left_2,
        /// <summary>$A2:8ACA: stoke_walk_left_0 visual identity in the installed frame schema.</summary>
        stoke_walk_left_0,
        /// <summary>$A2:8AD6: stoke_walk_left_1 visual identity in the installed frame schema.</summary>
        stoke_walk_left_1,
        /// <summary>$A2:8AE7: stoke_walk_left_2 visual identity in the installed frame schema.</summary>
        stoke_walk_left_2,
        /// <summary>$A2:8AF3: stoke_walk_left_3 visual identity in the installed frame schema.</summary>
        stoke_walk_left_3,
        /// <summary>$A2:8AFF: stoke_attack_left visual identity in the installed frame schema.</summary>
        stoke_attack_left,
        /// <summary>$A2:8B15: stoke_walk_right_0 visual identity in the installed frame schema.</summary>
        stoke_walk_right_0,
        /// <summary>$A2:8B21: stoke_walk_right_1 visual identity in the installed frame schema.</summary>
        stoke_walk_right_1,
        /// <summary>$A2:8B32: stoke_walk_right_2 visual identity in the installed frame schema.</summary>
        stoke_walk_right_2,
        /// <summary>$A2:8B3E: stoke_walk_right_3 visual identity in the installed frame schema.</summary>
        stoke_walk_right_3,
        /// <summary>$A2:8B4A: stoke_attack_right visual identity in the installed frame schema.</summary>
        stoke_attack_right,
        /// <summary>$A2:E3C5: ripper_shared_left_0 visual identity in the installed frame schema.</summary>
        ripper_shared_left_0,
        /// <summary>$A2:E3DB: ripper_shared_left_1 visual identity in the installed frame schema.</summary>
        ripper_shared_left_1,
        /// <summary>$A2:E3EC: ripper_shared_left_2 visual identity in the installed frame schema.</summary>
        ripper_shared_left_2,
        /// <summary>$A2:E402: ripper_shared_right_0 visual identity in the installed frame schema.</summary>
        ripper_shared_right_0,
        /// <summary>$A2:E418: ripper_shared_right_1 visual identity in the installed frame schema.</summary>
        ripper_shared_right_1,
        /// <summary>$A2:E429: ripper_shared_right_2 visual identity in the installed frame schema.</summary>
        ripper_shared_right_2,
        /// <summary>$A2:E43F: ripper_shared_frozen_left visual identity in the installed frame schema.</summary>
        ripper_shared_frozen_left,
        /// <summary>$A2:E44B: ripper_shared_frozen_right visual identity in the installed frame schema.</summary>
        ripper_shared_frozen_right,
        /// <summary>$A2:E527: ripper_left_0 visual identity in the installed frame schema.</summary>
        ripper_left_0,
        /// <summary>$A2:E533: ripper_left_1 visual identity in the installed frame schema.</summary>
        ripper_left_1,
        /// <summary>$A2:E53F: ripper_left_2 visual identity in the installed frame schema.</summary>
        ripper_left_2,
        /// <summary>$A2:E54B: ripper_right_0 visual identity in the installed frame schema.</summary>
        ripper_right_0,
        /// <summary>$A2:E557: ripper_right_1 visual identity in the installed frame schema.</summary>
        ripper_right_1,
        /// <summary>$A2:E563: ripper_right_2 visual identity in the installed frame schema.</summary>
        ripper_right_2,
        /// <summary>$A3:8EA5: fireflea_cycle_0 visual identity in the installed frame schema.</summary>
        fireflea_cycle_0,
        /// <summary>$A3:8EB6: fireflea_cycle_1 visual identity in the installed frame schema.</summary>
        fireflea_cycle_1,
        /// <summary>$A3:8EC7: fireflea_cycle_2 visual identity in the installed frame schema.</summary>
        fireflea_cycle_2,
        /// <summary>$A3:8ED8: fireflea_cycle_3 visual identity in the installed frame schema.</summary>
        fireflea_cycle_3,
        /// <summary>$A3:8EE9: fireflea_cycle_4 visual identity in the installed frame schema.</summary>
        fireflea_cycle_4,
        /// <summary>$A3:8EFA: fireflea_cycle_5 visual identity in the installed frame schema.</summary>
        fireflea_cycle_5,
        /// <summary>$A3:8F0B: fireflea_cycle_6 visual identity in the installed frame schema.</summary>
        fireflea_cycle_6,
        /// <summary>$A3:8F1C: fireflea_cycle_7 visual identity in the installed frame schema.</summary>
        fireflea_cycle_7,
        /// <summary>$A3:8F2D: fireflea_cycle_8 visual identity in the installed frame schema.</summary>
        fireflea_cycle_8,
        /// <summary>$A3:8F3E: fireflea_cycle_9 visual identity in the installed frame schema.</summary>
        fireflea_cycle_9,
        /// <summary>$A3:8F4F: fireflea_cycle_10 visual identity in the installed frame schema.</summary>
        fireflea_cycle_10,
        /// <summary>$A3:8F60: fireflea_cycle_11 visual identity in the installed frame schema.</summary>
        fireflea_cycle_11,
        /// <summary>$A3:8F71: fireflea_cycle_12 visual identity in the installed frame schema.</summary>
        fireflea_cycle_12,
        /// <summary>$A3:8F82: fireflea_cycle_13 visual identity in the installed frame schema.</summary>
        fireflea_cycle_13,
        /// <summary>$A3:8F93: fireflea_cycle_14 visual identity in the installed frame schema.</summary>
        fireflea_cycle_14,
        /// <summary>$A3:8FA4: fireflea_cycle_15 visual identity in the installed frame schema.</summary>
        fireflea_cycle_15,
        /// <summary>$A3:8FB5: fireflea_cycle_16 visual identity in the installed frame schema.</summary>
        fireflea_cycle_16,
        /// <summary>$A3:8FC6: fireflea_cycle_17 visual identity in the installed frame schema.</summary>
        fireflea_cycle_17,
        /// <summary>$A3:8FD7: fireflea_cycle_18 visual identity in the installed frame schema.</summary>
        fireflea_cycle_18,
        /// <summary>$A3:8FE8: fireflea_cycle_19 visual identity in the installed frame schema.</summary>
        fireflea_cycle_19,
        /// <summary>$A3:8FF9: fireflea_cycle_20 visual identity in the installed frame schema.</summary>
        fireflea_cycle_20,
        /// <summary>$A8:B448: magdollite_left_idle_0 visual identity in the installed frame schema.</summary>
        magdollite_left_idle_0,
        /// <summary>$A8:B459: magdollite_left_idle_1 visual identity in the installed frame schema.</summary>
        magdollite_left_idle_1,
        /// <summary>$A8:B46A: magdollite_left_idle_2 visual identity in the installed frame schema.</summary>
        magdollite_left_idle_2,
        /// <summary>$A8:B47B: magdollite_left_throw_0 visual identity in the installed frame schema.</summary>
        magdollite_left_throw_0,
        /// <summary>$A8:B48C: magdollite_left_throw_1 visual identity in the installed frame schema.</summary>
        magdollite_left_throw_1,
        /// <summary>$A8:B49D: magdollite_left_throw_2 visual identity in the installed frame schema.</summary>
        magdollite_left_throw_2,
        /// <summary>$A8:B4A9: magdollite_left_throw_3 visual identity in the installed frame schema.</summary>
        magdollite_left_throw_3,
        /// <summary>$A8:B4B5: magdollite_pillar_cap visual identity in the installed frame schema.</summary>
        magdollite_pillar_cap,
        /// <summary>$A8:B4C1: magdollite_left_submerge_0 visual identity in the installed frame schema.</summary>
        magdollite_left_submerge_0,
        /// <summary>$A8:B4CF: magdollite_left_submerge_1 visual identity in the installed frame schema.</summary>
        magdollite_left_submerge_1,
        /// <summary>$A8:B4E0: magdollite_left_submerge_2 visual identity in the installed frame schema.</summary>
        magdollite_left_submerge_2,
        /// <summary>$A8:B4F1: magdollite_right_idle_0 visual identity in the installed frame schema.</summary>
        magdollite_right_idle_0,
        /// <summary>$A8:B502: magdollite_right_idle_1 visual identity in the installed frame schema.</summary>
        magdollite_right_idle_1,
        /// <summary>$A8:B513: magdollite_right_idle_2 visual identity in the installed frame schema.</summary>
        magdollite_right_idle_2,
        /// <summary>$A8:B524: magdollite_right_throw_0 visual identity in the installed frame schema.</summary>
        magdollite_right_throw_0,
        /// <summary>$A8:B535: magdollite_right_throw_1 visual identity in the installed frame schema.</summary>
        magdollite_right_throw_1,
        /// <summary>$A8:B546: magdollite_right_throw_2 visual identity in the installed frame schema.</summary>
        magdollite_right_throw_2,
        /// <summary>$A8:B552: magdollite_right_throw_3 visual identity in the installed frame schema.</summary>
        magdollite_right_throw_3,
        /// <summary>$A8:B56A: magdollite_right_submerge_0 visual identity in the installed frame schema.</summary>
        magdollite_right_submerge_0,
        /// <summary>$A8:B578: magdollite_right_submerge_1 visual identity in the installed frame schema.</summary>
        magdollite_right_submerge_1,
        /// <summary>$A8:B589: magdollite_right_submerge_2 visual identity in the installed frame schema.</summary>
        magdollite_right_submerge_2,
        /// <summary>$A8:B59A: magdollite_pillar_phase_0 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_0,
        /// <summary>$A8:B5A1: magdollite_pillar_phase_1 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_1,
        /// <summary>$A8:B5AD: magdollite_pillar_phase_2 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_2,
        /// <summary>$A8:B5BE: magdollite_pillar_phase_3 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_3,
        /// <summary>$A8:B5D4: magdollite_pillar_phase_4 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_4,
        /// <summary>$A8:B5EF: magdollite_pillar_phase_5 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_5,
        /// <summary>$A8:B60F: magdollite_pillar_phase_6 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_6,
        /// <summary>$A8:B634: magdollite_pillar_phase_7 visual identity in the installed frame schema.</summary>
        magdollite_pillar_phase_7,
        /// <summary>$A6:F921: ceres_door_rotating_overlay visual identity in the installed frame schema.</summary>
        ceres_door_rotating_overlay,
        /// <summary>$A6:F95F: ceres_door_left_hold visual identity in the installed frame schema.</summary>
        ceres_door_left_hold,
        /// <summary>$A6:F989: ceres_door_left_transition_0 visual identity in the installed frame schema.</summary>
        ceres_door_left_transition_0,
        /// <summary>$A6:F9B3: ceres_door_left_transition_1 visual identity in the installed frame schema.</summary>
        ceres_door_left_transition_1,
        /// <summary>$A6:F9D3: ceres_door_left_transition_2 visual identity in the installed frame schema.</summary>
        ceres_door_left_transition_2,
        /// <summary>$A6:F9F3: ceres_door_left_transition_3 visual identity in the installed frame schema.</summary>
        ceres_door_left_transition_3,
        /// <summary>$A6:FA13: ceres_door_right_hold visual identity in the installed frame schema.</summary>
        ceres_door_right_hold,
        /// <summary>$A6:FA3D: ceres_door_right_transition_0 visual identity in the installed frame schema.</summary>
        ceres_door_right_transition_0,
        /// <summary>$A6:FA67: ceres_door_right_transition_1 visual identity in the installed frame schema.</summary>
        ceres_door_right_transition_1,
        /// <summary>$A6:FA87: ceres_door_right_transition_2 visual identity in the installed frame schema.</summary>
        ceres_door_right_transition_2,
        /// <summary>$A6:FAA7: ceres_door_right_transition_3 visual identity in the installed frame schema.</summary>
        ceres_door_right_transition_3,
        /// <summary>$A6:FAC7: ceres_door_initial visual identity in the installed frame schema.</summary>
        ceres_door_initial,
        /// <summary>$A6:FACE: ceres_door_mode7_left_wall visual identity in the installed frame schema.</summary>
        ceres_door_mode7_left_wall,
        /// <summary>$A6:FB2F: ceres_door_mode7_right_wall visual identity in the installed frame schema.</summary>
        ceres_door_mode7_right_wall,
        /// <summary>$A6:A329: ceres_door_ridley_private_overlay visual identity in the installed frame schema.</summary>
        ceres_door_ridley_private_overlay,
        /// <summary>$A6:BFFD: ceres_baby_horizontal visual identity in the installed frame schema.</summary>
        ceres_baby_horizontal,
        /// <summary>$A6:C018: ceres_baby_round visual identity in the installed frame schema.</summary>
        ceres_baby_round,
        /// <summary>$A6:C033: ceres_baby_vertical visual identity in the installed frame schema.</summary>
        ceres_baby_vertical,
        /// <summary>$A2:BD6C: rio_bd6c visual identity in the installed frame schema.</summary>
        rio_bd6c,
        /// <summary>$A2:BD82: rio_bd82 visual identity in the installed frame schema.</summary>
        rio_bd82,
        /// <summary>$A2:BD98: rio_bd98 visual identity in the installed frame schema.</summary>
        rio_bd98,
        /// <summary>$A2:BDAE: rio_bdae visual identity in the installed frame schema.</summary>
        rio_bdae,
        /// <summary>$A2:BDC4: rio_bdc4 visual identity in the installed frame schema.</summary>
        rio_bdc4,
        /// <summary>$A2:BDDA: rio_bdda visual identity in the installed frame schema.</summary>
        rio_bdda,
        /// <summary>$A2:BDF0: rio_bdf0 visual identity in the installed frame schema.</summary>
        rio_bdf0,
        /// <summary>$A2:BE06: rio_be06 visual identity in the installed frame schema.</summary>
        rio_be06,
        /// <summary>$A2:C8BD: lower_norfair_rio_c8bd visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c8bd,
        /// <summary>$A2:C8D3: lower_norfair_rio_c8d3 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c8d3,
        /// <summary>$A2:C8E9: lower_norfair_rio_c8e9 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c8e9,
        /// <summary>$A2:C8FF: lower_norfair_rio_c8ff visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c8ff,
        /// <summary>$A2:C915: lower_norfair_rio_c915 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c915,
        /// <summary>$A2:C92B: lower_norfair_rio_c92b visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c92b,
        /// <summary>$A2:C941: lower_norfair_rio_c941 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c941,
        /// <summary>$A2:C957: lower_norfair_rio_c957 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c957,
        /// <summary>$A2:C96D: lower_norfair_rio_c96d visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c96d,
        /// <summary>$A2:C983: lower_norfair_rio_c983 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c983,
        /// <summary>$A2:C999: lower_norfair_rio_c999 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c999,
        /// <summary>$A2:C9AF: lower_norfair_rio_c9af visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c9af,
        /// <summary>$A2:C9C5: lower_norfair_rio_c9c5 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c9c5,
        /// <summary>$A2:C9DB: lower_norfair_rio_c9db visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c9db,
        /// <summary>$A2:C9F1: lower_norfair_rio_c9f1 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_c9f1,
        /// <summary>$A2:CA07: lower_norfair_rio_ca07 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_ca07,
        /// <summary>$A2:CA13: lower_norfair_rio_ca13 visual identity in the installed frame schema.</summary>
        lower_norfair_rio_ca13,
        /// <summary>$A2:CA1F: lower_norfair_rio_ca1f visual identity in the installed frame schema.</summary>
        lower_norfair_rio_ca1f,
        /// <summary>$A2:C427: norfair_rio_c427 visual identity in the installed frame schema.</summary>
        norfair_rio_c427,
        /// <summary>$A2:C442: norfair_rio_c442 visual identity in the installed frame schema.</summary>
        norfair_rio_c442,
        /// <summary>$A2:C45D: norfair_rio_c45d visual identity in the installed frame schema.</summary>
        norfair_rio_c45d,
        /// <summary>$A2:C493: norfair_rio_c493 visual identity in the installed frame schema.</summary>
        norfair_rio_c493,
        /// <summary>$A2:C49F: norfair_rio_c49f visual identity in the installed frame schema.</summary>
        norfair_rio_c49f,
        /// <summary>$A2:C4AB: norfair_rio_c4ab visual identity in the installed frame schema.</summary>
        norfair_rio_c4ab,
        /// <summary>$A2:C4B7: norfair_rio_c4b7 visual identity in the installed frame schema.</summary>
        norfair_rio_c4b7,
        /// <summary>$A2:C4D2: norfair_rio_c4d2 visual identity in the installed frame schema.</summary>
        norfair_rio_c4d2,
        /// <summary>$A2:C4ED: norfair_rio_c4ed visual identity in the installed frame schema.</summary>
        norfair_rio_c4ed,
        /// <summary>$A2:C508: norfair_rio_c508 visual identity in the installed frame schema.</summary>
        norfair_rio_c508,
        /// <summary>$A2:C523: norfair_rio_c523 visual identity in the installed frame schema.</summary>
        norfair_rio_c523,
        /// <summary>$A2:C534: norfair_rio_c534 visual identity in the installed frame schema.</summary>
        norfair_rio_c534,
        /// <summary>$A2:C54F: norfair_rio_c54f visual identity in the installed frame schema.</summary>
        norfair_rio_c54f,
        /// <summary>$A2:C56A: norfair_rio_c56a visual identity in the installed frame schema.</summary>
        norfair_rio_c56a,
        /// <summary>$A2:C585: norfair_rio_c585 visual identity in the installed frame schema.</summary>
        norfair_rio_c585,
        /// <summary>$A2:C5A0: norfair_rio_c5a0 visual identity in the installed frame schema.</summary>
        norfair_rio_c5a0,
        /// <summary>$A2:C5BB: norfair_rio_c5bb visual identity in the installed frame schema.</summary>
        norfair_rio_c5bb,
        /// <summary>$A2:C5D6: norfair_rio_c5d6 visual identity in the installed frame schema.</summary>
        norfair_rio_c5d6,
        /// <summary>$A2:C5E2: norfair_rio_c5e2 visual identity in the installed frame schema.</summary>
        norfair_rio_c5e2,
        /// <summary>$A2:C5EE: norfair_rio_c5ee visual identity in the installed frame schema.</summary>
        norfair_rio_c5ee,
        /// <summary>$A2:9DF6: puyo_ground_0 visual identity in the installed frame schema.</summary>
        puyo_ground_0,
        /// <summary>$A2:9E02: puyo_ground_1 visual identity in the installed frame schema.</summary>
        puyo_ground_1,
        /// <summary>$A2:9E0E: puyo_ground_2 visual identity in the installed frame schema.</summary>
        puyo_ground_2,
        /// <summary>$A2:9E1A: puyo_air_0 visual identity in the installed frame schema.</summary>
        puyo_air_0,
        /// <summary>$A2:9E26: puyo_air_1 visual identity in the installed frame schema.</summary>
        puyo_air_1,
        /// <summary>$A2:9E37: puyo_air_2 visual identity in the installed frame schema.</summary>
        puyo_air_2,
        /// <summary>$A2:9E4D: puyo_air_3 visual identity in the installed frame schema.</summary>
        puyo_air_3,
        /// <summary>$A2:9E5E: puyo_air_4 visual identity in the installed frame schema.</summary>
        puyo_air_4,
        /// <summary>$A8:DB76: bull_idle_0 visual identity in the installed frame schema.</summary>
        bull_idle_0,
        /// <summary>$A8:DB8C: bull_idle_1 visual identity in the installed frame schema.</summary>
        bull_idle_1,
        /// <summary>$A8:DBA2: bull_idle_2 visual identity in the installed frame schema.</summary>
        bull_idle_2,
        /// <summary>$A8:DFA2: alcoon_left_walk_0 visual identity in the installed frame schema.</summary>
        alcoon_left_walk_0,
        /// <summary>$A8:DFC2: alcoon_left_walk_1 visual identity in the installed frame schema.</summary>
        alcoon_left_walk_1,
        /// <summary>$A8:DFE2: alcoon_left_walk_2 visual identity in the installed frame schema.</summary>
        alcoon_left_walk_2,
        /// <summary>$A8:E007: alcoon_left_walk_3 visual identity in the installed frame schema.</summary>
        alcoon_left_walk_3,
        /// <summary>$A8:E027: alcoon_left_fire_0 visual identity in the installed frame schema.</summary>
        alcoon_left_fire_0,
        /// <summary>$A8:E047: alcoon_left_fire_1 visual identity in the installed frame schema.</summary>
        alcoon_left_fire_1,
        /// <summary>$A8:E06C: alcoon_left_fire_2 visual identity in the installed frame schema.</summary>
        alcoon_left_fire_2,
        /// <summary>$A8:E09B: alcoon_left_fire_3 visual identity in the installed frame schema.</summary>
        alcoon_left_fire_3,
        /// <summary>$A8:E0BB: alcoon_left_air_up visual identity in the installed frame schema.</summary>
        alcoon_left_air_up,
        /// <summary>$A8:E0DB: alcoon_right_walk_0 visual identity in the installed frame schema.</summary>
        alcoon_right_walk_0,
        /// <summary>$A8:E0FB: alcoon_right_walk_1 visual identity in the installed frame schema.</summary>
        alcoon_right_walk_1,
        /// <summary>$A8:E11B: alcoon_right_walk_2 visual identity in the installed frame schema.</summary>
        alcoon_right_walk_2,
        /// <summary>$A8:E140: alcoon_right_walk_3 visual identity in the installed frame schema.</summary>
        alcoon_right_walk_3,
        /// <summary>$A8:E160: alcoon_right_fire_0 visual identity in the installed frame schema.</summary>
        alcoon_right_fire_0,
        /// <summary>$A8:E180: alcoon_right_fire_1 visual identity in the installed frame schema.</summary>
        alcoon_right_fire_1,
        /// <summary>$A8:E1A5: alcoon_right_fire_2 visual identity in the installed frame schema.</summary>
        alcoon_right_fire_2,
        /// <summary>$A8:E1D4: alcoon_right_fire_3 visual identity in the installed frame schema.</summary>
        alcoon_right_fire_3,
        /// <summary>$A8:E1F4: alcoon_right_air_up visual identity in the installed frame schema.</summary>
        alcoon_right_air_up,
        /// <summary>$A8:BED3: beetom_left_crawl_0 visual identity in the installed frame schema.</summary>
        beetom_left_crawl_0,
        /// <summary>$A8:BEEE: beetom_left_crawl_1 visual identity in the installed frame schema.</summary>
        beetom_left_crawl_1,
        /// <summary>$A8:BF09: beetom_left_crawl_2 visual identity in the installed frame schema.</summary>
        beetom_left_crawl_2,
        /// <summary>$A8:BF24: beetom_left_hop_0 visual identity in the installed frame schema.</summary>
        beetom_left_hop_0,
        /// <summary>$A8:BF3F: beetom_left_hop_1 visual identity in the installed frame schema.</summary>
        beetom_left_hop_1,
        /// <summary>$A8:BF5A: beetom_left_drain_0 visual identity in the installed frame schema.</summary>
        beetom_left_drain_0,
        /// <summary>$A8:BF75: beetom_left_drain_1 visual identity in the installed frame schema.</summary>
        beetom_left_drain_1,
        /// <summary>$A8:BF90: beetom_left_drain_2 visual identity in the installed frame schema.</summary>
        beetom_left_drain_2,
        /// <summary>$A8:BFAB: beetom_left_drain_3 visual identity in the installed frame schema.</summary>
        beetom_left_drain_3,
        /// <summary>$A8:BFCB: beetom_left_drain_4 visual identity in the installed frame schema.</summary>
        beetom_left_drain_4,
        /// <summary>$A8:BFEB: beetom_left_drain_5 visual identity in the installed frame schema.</summary>
        beetom_left_drain_5,
        /// <summary>$A8:C00B: beetom_right_crawl_0 visual identity in the installed frame schema.</summary>
        beetom_right_crawl_0,
        /// <summary>$A8:C026: beetom_right_crawl_1 visual identity in the installed frame schema.</summary>
        beetom_right_crawl_1,
        /// <summary>$A8:C041: beetom_right_crawl_2 visual identity in the installed frame schema.</summary>
        beetom_right_crawl_2,
        /// <summary>$A8:C05C: beetom_right_hop_0 visual identity in the installed frame schema.</summary>
        beetom_right_hop_0,
        /// <summary>$A8:C077: beetom_right_hop_1 visual identity in the installed frame schema.</summary>
        beetom_right_hop_1,
        /// <summary>$A8:C092: beetom_right_drain_0 visual identity in the installed frame schema.</summary>
        beetom_right_drain_0,
        /// <summary>$A8:C0AD: beetom_right_drain_1 visual identity in the installed frame schema.</summary>
        beetom_right_drain_1,
        /// <summary>$A8:C0C8: beetom_right_drain_2 visual identity in the installed frame schema.</summary>
        beetom_right_drain_2,
        /// <summary>$A8:C0E3: beetom_right_drain_3 visual identity in the installed frame schema.</summary>
        beetom_right_drain_3,
        /// <summary>$A8:C103: beetom_right_drain_4 visual identity in the installed frame schema.</summary>
        beetom_right_drain_4,
        /// <summary>$A8:C123: beetom_right_drain_5 visual identity in the installed frame schema.</summary>
        beetom_right_drain_5,
        /// <summary>$A3:AF19: sidehopper_jump_floor visual identity in the installed frame schema.</summary>
        sidehopper_jump_floor,
        /// <summary>$A3:AEE3: sidehopper_land_floor_0 visual identity in the installed frame schema.</summary>
        sidehopper_land_floor_0,
        /// <summary>$A3:AEFE: sidehopper_land_floor_1 visual identity in the installed frame schema.</summary>
        sidehopper_land_floor_1,
        /// <summary>$A3:AF6A: sidehopper_jump_ceiling visual identity in the installed frame schema.</summary>
        sidehopper_jump_ceiling,
        /// <summary>$A3:AF34: sidehopper_land_ceiling_0 visual identity in the installed frame schema.</summary>
        sidehopper_land_ceiling_0,
        /// <summary>$A3:AF4F: sidehopper_land_ceiling_1 visual identity in the installed frame schema.</summary>
        sidehopper_land_ceiling_1,
        /// <summary>$A3:B019: dessgeega_jump_floor visual identity in the installed frame schema.</summary>
        dessgeega_jump_floor,
        /// <summary>$A3:AFE3: dessgeega_land_floor_0 visual identity in the installed frame schema.</summary>
        dessgeega_land_floor_0,
        /// <summary>$A3:AFFE: dessgeega_land_floor_1 visual identity in the installed frame schema.</summary>
        dessgeega_land_floor_1,
        /// <summary>$A3:B06A: dessgeega_jump_ceiling visual identity in the installed frame schema.</summary>
        dessgeega_jump_ceiling,
        /// <summary>$A3:B034: dessgeega_land_ceiling_0 visual identity in the installed frame schema.</summary>
        dessgeega_land_ceiling_0,
        /// <summary>$A3:B04F: dessgeega_land_ceiling_1 visual identity in the installed frame schema.</summary>
        dessgeega_land_ceiling_1,
        /// <summary>$A3:B15B: large_sidehopper_jump_floor visual identity in the installed frame schema.</summary>
        large_sidehopper_jump_floor,
        /// <summary>$A3:B111: large_sidehopper_land_floor_0 visual identity in the installed frame schema.</summary>
        large_sidehopper_land_floor_0,
        /// <summary>$A3:B136: large_sidehopper_land_floor_1 visual identity in the installed frame schema.</summary>
        large_sidehopper_land_floor_1,
        /// <summary>$A3:B1DE: large_sidehopper_jump_ceiling visual identity in the installed frame schema.</summary>
        large_sidehopper_jump_ceiling,
        /// <summary>$A3:B194: large_sidehopper_land_ceiling_0 visual identity in the installed frame schema.</summary>
        large_sidehopper_land_ceiling_0,
        /// <summary>$A3:B1B9: large_sidehopper_land_ceiling_1 visual identity in the installed frame schema.</summary>
        large_sidehopper_land_ceiling_1,
        /// <summary>$A3:B2D1: large_dessgeega_jump_floor visual identity in the installed frame schema.</summary>
        large_dessgeega_jump_floor,
        /// <summary>$A3:B273: large_dessgeega_land_floor_0 visual identity in the installed frame schema.</summary>
        large_dessgeega_land_floor_0,
        /// <summary>$A3:B2A2: large_dessgeega_land_floor_1 visual identity in the installed frame schema.</summary>
        large_dessgeega_land_floor_1,
        /// <summary>$A3:B368: large_dessgeega_jump_ceiling visual identity in the installed frame schema.</summary>
        large_dessgeega_jump_ceiling,
        /// <summary>$A3:B30A: large_dessgeega_land_ceiling_0 visual identity in the installed frame schema.</summary>
        large_dessgeega_land_ceiling_0,
        /// <summary>$A3:B339: large_dessgeega_land_ceiling_1 visual identity in the installed frame schema.</summary>
        large_dessgeega_land_ceiling_1,
        /// <summary>$A2:E146: choot_idle visual identity in the installed frame schema.</summary>
        choot_idle,
        /// <summary>$A2:E15C: choot_jump visual identity in the installed frame schema.</summary>
        choot_jump,
        /// <summary>$A2:E168: choot_jump_apex visual identity in the installed frame schema.</summary>
        choot_jump_apex,
        /// <summary>$A2:E16F: choot_fall_end visual identity in the installed frame schema.</summary>
        choot_fall_end,
        /// <summary>$A3:E50E: hzoomer_upside_right_0 visual identity in the installed frame schema.</summary>
        hzoomer_upside_right_0,
        /// <summary>$A3:E524: hzoomer_upside_right_1 visual identity in the installed frame schema.</summary>
        hzoomer_upside_right_1,
        /// <summary>$A3:E53A: hzoomer_upside_right_2 visual identity in the installed frame schema.</summary>
        hzoomer_upside_right_2,
        /// <summary>$A3:E550: hzoomer_upside_right_3 visual identity in the installed frame schema.</summary>
        hzoomer_upside_right_3,
        /// <summary>$A3:E566: hzoomer_upside_right_4 visual identity in the installed frame schema.</summary>
        hzoomer_upside_right_4,
        /// <summary>$A3:E3C4: hzoomer_upside_left_0 visual identity in the installed frame schema.</summary>
        hzoomer_upside_left_0,
        /// <summary>$A3:E3DA: hzoomer_upside_left_1 visual identity in the installed frame schema.</summary>
        hzoomer_upside_left_1,
        /// <summary>$A3:E3F0: hzoomer_upside_left_2 visual identity in the installed frame schema.</summary>
        hzoomer_upside_left_2,
        /// <summary>$A3:E406: hzoomer_upside_left_3 visual identity in the installed frame schema.</summary>
        hzoomer_upside_left_3,
        /// <summary>$A3:E41C: hzoomer_upside_left_4 visual identity in the installed frame schema.</summary>
        hzoomer_upside_left_4,
        /// <summary>$A3:E432: hzoomer_upside_down_0 visual identity in the installed frame schema.</summary>
        hzoomer_upside_down_0,
        /// <summary>$A3:E448: hzoomer_upside_down_1 visual identity in the installed frame schema.</summary>
        hzoomer_upside_down_1,
        /// <summary>$A3:E45E: hzoomer_upside_down_2 visual identity in the installed frame schema.</summary>
        hzoomer_upside_down_2,
        /// <summary>$A3:E474: hzoomer_upside_down_3 visual identity in the installed frame schema.</summary>
        hzoomer_upside_down_3,
        /// <summary>$A3:E48A: hzoomer_upside_down_4 visual identity in the installed frame schema.</summary>
        hzoomer_upside_down_4,
        /// <summary>$A3:E2E8: hzoomer_upside_up_0 visual identity in the installed frame schema.</summary>
        hzoomer_upside_up_0,
        /// <summary>$A3:E2FE: hzoomer_upside_up_1 visual identity in the installed frame schema.</summary>
        hzoomer_upside_up_1,
        /// <summary>$A3:E314: hzoomer_upside_up_2 visual identity in the installed frame schema.</summary>
        hzoomer_upside_up_2,
        /// <summary>$A3:E32A: hzoomer_upside_up_3 visual identity in the installed frame schema.</summary>
        hzoomer_upside_up_3,
        /// <summary>$A3:E340: hzoomer_upside_up_4 visual identity in the installed frame schema.</summary>
        hzoomer_upside_up_4,
        /// <summary>$A3:A67D: sbug_right_0 visual identity in the installed frame schema.</summary>
        sbug_right_0,
        /// <summary>$A3:A684: sbug_right_1 visual identity in the installed frame schema.</summary>
        sbug_right_1,
        /// <summary>$A3:A68B: sbug_right_2 visual identity in the installed frame schema.</summary>
        sbug_right_2,
        /// <summary>$A3:A692: sbug_up_right_0 visual identity in the installed frame schema.</summary>
        sbug_up_right_0,
        /// <summary>$A3:A699: sbug_up_right_1 visual identity in the installed frame schema.</summary>
        sbug_up_right_1,
        /// <summary>$A3:A6A0: sbug_up_right_2 visual identity in the installed frame schema.</summary>
        sbug_up_right_2,
        /// <summary>$A3:A6A7: sbug_up_0 visual identity in the installed frame schema.</summary>
        sbug_up_0,
        /// <summary>$A3:A6AE: sbug_up_1 visual identity in the installed frame schema.</summary>
        sbug_up_1,
        /// <summary>$A3:A6B5: sbug_up_2 visual identity in the installed frame schema.</summary>
        sbug_up_2,
        /// <summary>$A3:A6BC: sbug_up_left_0 visual identity in the installed frame schema.</summary>
        sbug_up_left_0,
        /// <summary>$A3:A6C3: sbug_up_left_1 visual identity in the installed frame schema.</summary>
        sbug_up_left_1,
        /// <summary>$A3:A6CA: sbug_up_left_2 visual identity in the installed frame schema.</summary>
        sbug_up_left_2,
        /// <summary>$A3:A6D1: sbug_left_0 visual identity in the installed frame schema.</summary>
        sbug_left_0,
        /// <summary>$A3:A6D8: sbug_left_1 visual identity in the installed frame schema.</summary>
        sbug_left_1,
        /// <summary>$A3:A6DF: sbug_left_2 visual identity in the installed frame schema.</summary>
        sbug_left_2,
        /// <summary>$A3:A6E6: sbug_down_left_0 visual identity in the installed frame schema.</summary>
        sbug_down_left_0,
        /// <summary>$A3:A6ED: sbug_down_left_1 visual identity in the installed frame schema.</summary>
        sbug_down_left_1,
        /// <summary>$A3:A6F4: sbug_down_left_2 visual identity in the installed frame schema.</summary>
        sbug_down_left_2,
        /// <summary>$A3:A6FB: sbug_down_0 visual identity in the installed frame schema.</summary>
        sbug_down_0,
        /// <summary>$A3:A702: sbug_down_1 visual identity in the installed frame schema.</summary>
        sbug_down_1,
        /// <summary>$A3:A709: sbug_down_2 visual identity in the installed frame schema.</summary>
        sbug_down_2,
        /// <summary>$A3:A710: sbug_down_right_0 visual identity in the installed frame schema.</summary>
        sbug_down_right_0,
        /// <summary>$A3:A717: sbug_down_right_1 visual identity in the installed frame schema.</summary>
        sbug_down_right_1,
        /// <summary>$A3:A71E: sbug_down_right_2 visual identity in the installed frame schema.</summary>
        sbug_down_right_2,
        /// <summary>$A8:93F9: fune_left_idle visual identity in the installed frame schema.</summary>
        fune_left_idle,
        /// <summary>$A8:9423: fune_left_active_0 visual identity in the installed frame schema.</summary>
        fune_left_active_0,
        /// <summary>$A8:944D: fune_left_active_1 visual identity in the installed frame schema.</summary>
        fune_left_active_1,
        /// <summary>$A8:9477: fune_left_active_2 visual identity in the installed frame schema.</summary>
        fune_left_active_2,
        /// <summary>$A8:94A1: fune_left_active_3 visual identity in the installed frame schema.</summary>
        fune_left_active_3,
        /// <summary>$A8:94CB: fune_right_idle visual identity in the installed frame schema.</summary>
        fune_right_idle,
        /// <summary>$A8:94F5: fune_right_active_0 visual identity in the installed frame schema.</summary>
        fune_right_active_0,
        /// <summary>$A8:951F: fune_right_active_1 visual identity in the installed frame schema.</summary>
        fune_right_active_1,
        /// <summary>$A8:9549: fune_right_active_2 visual identity in the installed frame schema.</summary>
        fune_right_active_2,
        /// <summary>$A8:9573: fune_right_active_3 visual identity in the installed frame schema.</summary>
        fune_right_active_3,
        /// <summary>$A8:97B4: namihe_left_idle visual identity in the installed frame schema.</summary>
        namihe_left_idle,
        /// <summary>$A8:97DE: namihe_left_active_0 visual identity in the installed frame schema.</summary>
        namihe_left_active_0,
        /// <summary>$A8:9808: namihe_left_active_1 visual identity in the installed frame schema.</summary>
        namihe_left_active_1,
        /// <summary>$A8:9832: namihe_left_active_2 visual identity in the installed frame schema.</summary>
        namihe_left_active_2,
        /// <summary>$A8:985C: namihe_left_active_3 visual identity in the installed frame schema.</summary>
        namihe_left_active_3,
        /// <summary>$A8:9886: namihe_left_active_4 visual identity in the installed frame schema.</summary>
        namihe_left_active_4,
        /// <summary>$A8:98B0: namihe_right_idle visual identity in the installed frame schema.</summary>
        namihe_right_idle,
        /// <summary>$A8:98DA: namihe_right_active_0 visual identity in the installed frame schema.</summary>
        namihe_right_active_0,
        /// <summary>$A8:9904: namihe_right_active_1 visual identity in the installed frame schema.</summary>
        namihe_right_active_1,
        /// <summary>$A8:992E: namihe_right_active_2 visual identity in the installed frame schema.</summary>
        namihe_right_active_2,
        /// <summary>$A8:9958: namihe_right_active_3 visual identity in the installed frame schema.</summary>
        namihe_right_active_3,
        /// <summary>$A8:9982: namihe_right_active_4 visual identity in the installed frame schema.</summary>
        namihe_right_active_4,
        /// <summary>$A2:F468: kamer_platform_0 visual identity in the installed frame schema.</summary>
        kamer_platform_0,
        /// <summary>$A2:F474: kamer_platform_1 visual identity in the installed frame schema.</summary>
        kamer_platform_1,
        /// <summary>$A2:F480: kamer_platform_2 visual identity in the installed frame schema.</summary>
        kamer_platform_2,
        /// <summary>$A2:F48C: kamer_platform_3 visual identity in the installed frame schema.</summary>
        kamer_platform_3,
        /// <summary>$A3:962F: elevator_platform_0 visual identity in the installed frame schema.</summary>
        elevator_platform_0,
        /// <summary>$A3:9645: elevator_platform_1 visual identity in the installed frame schema.</summary>
        elevator_platform_1,
        /// <summary>$B4:DB42: draygon_intro_evir_0 visual identity in the installed frame schema.</summary>
        draygon_intro_evir_0,
        /// <summary>$B4:DB80: draygon_intro_evir_1 visual identity in the installed frame schema.</summary>
        draygon_intro_evir_1,
        /// <summary>$B4:DBBE: draygon_intro_evir_2 visual identity in the installed frame schema.</summary>
        draygon_intro_evir_2,
        /// <summary>$B4:DBFC: draygon_intro_evir_3 visual identity in the installed frame schema.</summary>
        draygon_intro_evir_3,
        /// <summary>$B4:C920: draygon_breath_bubble_0 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_0,
        /// <summary>$B4:C927: draygon_breath_bubble_1 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_1,
        /// <summary>$B4:C938: draygon_breath_bubble_2 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_2,
        /// <summary>$B4:C949: draygon_breath_bubble_3 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_3,
        /// <summary>$B4:C95A: draygon_breath_bubble_4 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_4,
        /// <summary>$B4:C96B: draygon_breath_bubble_5 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_5,
        /// <summary>$B4:C97C: draygon_breath_bubble_6 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_6,
        /// <summary>$B4:C98D: draygon_breath_bubble_7 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_7,
        /// <summary>$B4:C999: draygon_breath_bubble_8 visual identity in the installed frame schema.</summary>
        draygon_breath_bubble_8,
    }

    /// <summary>Two-byte part count followed by five bytes per native OAM part.</summary>
    private static int NativeFrameBytes(int parts) => 2 + 5 * parts;

    private static EnemySpritemapDefinition NamedFrame(NamedFrameId frame)
    {
        (byte bank, ushort pointer) = frame switch
        {
            >= NamedFrameId.boyon_idle_0 and <= NamedFrameId.boyon_idle_2 =>
                (BoyonBank, (ushort)(0x88da + ((int)frame - (int)NamedFrameId.boyon_idle_0) * NativeFrameBytes(1))),
            >= NamedFrameId.boyon_bounce_0 and <= NamedFrameId.boyon_bounce_3 =>
                (BoyonBank, (ushort)(0x88ef + ((int)frame - (int)NamedFrameId.boyon_bounce_0) * NativeFrameBytes(1))),
            >= NamedFrameId.cacatac_upright_idle_0 and <= NamedFrameId.cacatac_upright_idle_7 =>
                (BoyonBank, (ushort)(0xa0bb + ((int)frame - (int)NamedFrameId.cacatac_upright_idle_0) * NativeFrameBytes(6))),
            NamedFrameId.cacatac_upright_attack_1 => (BoyonBank, (ushort)0xa1bb),
            NamedFrameId.cacatac_upright_attack_2 => (BoyonBank, (ushort)0xa1ef),
            >= NamedFrameId.cacatac_inverted_idle_0 and <= NamedFrameId.cacatac_inverted_idle_7 =>
                (BoyonBank, (ushort)(0xa223 + ((int)frame - (int)NamedFrameId.cacatac_inverted_idle_0) * NativeFrameBytes(6))),
            NamedFrameId.cacatac_inverted_attack_1 => (BoyonBank, (ushort)0xa323),
            NamedFrameId.cacatac_inverted_attack_2 => (BoyonBank, (ushort)0xa357),
            >= NamedFrameId.boulder_roll_0 and <= NamedFrameId.boulder_roll_7 =>
                (BoulderBank, (ushort)(0x8a59 + ((int)frame - (int)NamedFrameId.boulder_roll_0) * NativeFrameBytes(4))),
            >= NamedFrameId.atomic_up_right_0 and <= NamedFrameId.atomic_up_right_4 =>
                (AtomicBank, (ushort)(0xe489 + ((int)frame - (int)NamedFrameId.atomic_up_right_0) * NativeFrameBytes(4))),
            NamedFrameId.atomic_up_right_5 => (AtomicBank, (ushort)0xe4f2),
            >= NamedFrameId.atomic_up_left_0 and <= NamedFrameId.atomic_up_left_4 =>
                (AtomicBank, (ushort)(0xe508 + ((int)frame - (int)NamedFrameId.atomic_up_left_0) * NativeFrameBytes(4))),
            NamedFrameId.atomic_up_left_5 => (AtomicBank, (ushort)0xe571),
            >= NamedFrameId.skultera_swim_left_0 and <= NamedFrameId.skultera_swim_left_2 =>
                (SkulteraBank, (ushort)(0x928a + ((int)frame - (int)NamedFrameId.skultera_swim_left_0) * NativeFrameBytes(5))),
            >= NamedFrameId.skultera_turn_right_0 and <= NamedFrameId.skultera_turn_right_2 =>
                (SkulteraBank, (ushort)(0x92db + ((int)frame - (int)NamedFrameId.skultera_turn_right_0) * NativeFrameBytes(5))),
            NamedFrameId.skultera_turn_right_3 => (SkulteraBank, (ushort)0x9327),
            NamedFrameId.skultera_turn_right_4 => (SkulteraBank, (ushort)0x933d),
            NamedFrameId.skultera_turn_right_5 => (SkulteraBank, (ushort)0x934e),
            NamedFrameId.skultera_turn_right_6 => (SkulteraBank, (ushort)0x9364),
            NamedFrameId.skultera_turn_right_7 => (SkulteraBank, (ushort)0x937f),
            >= NamedFrameId.skultera_swim_right_0 and <= NamedFrameId.skultera_swim_right_2 =>
                (SkulteraBank, (ushort)(0x939a + ((int)frame - (int)NamedFrameId.skultera_swim_right_0) * NativeFrameBytes(5))),
            >= NamedFrameId.skultera_turn_left_0 and <= NamedFrameId.skultera_turn_left_2 =>
                (SkulteraBank, (ushort)(0x93eb + ((int)frame - (int)NamedFrameId.skultera_turn_left_0) * NativeFrameBytes(5))),
            NamedFrameId.skultera_turn_left_3 => (SkulteraBank, (ushort)0x9437),
            NamedFrameId.skultera_turn_left_4 => (SkulteraBank, (ushort)0x944d),
            NamedFrameId.skultera_turn_left_5 => (SkulteraBank, (ushort)0x945e),
            NamedFrameId.skultera_turn_left_6 => (SkulteraBank, (ushort)0x9474),
            NamedFrameId.skultera_turn_left_7 => (SkulteraBank, (ushort)0x948f),
            NamedFrameId.waver_steady_left => (WaverBank, (ushort)0x884a),
            NamedFrameId.waver_steady_right => (WaverBank, (ushort)0x88b3),
            NamedFrameId.waver_spin_left_0 => (WaverBank, (ushort)0x885b),
            NamedFrameId.waver_spin_left_1 => (WaverBank, (ushort)0x8871),
            NamedFrameId.waver_spin_left_2 => (WaverBank, (ushort)0x881e),
            NamedFrameId.waver_spin_left_3 => (WaverBank, (ushort)0x8834),
            NamedFrameId.waver_spin_right_0 => (WaverBank, (ushort)0x88c4),
            NamedFrameId.waver_spin_right_1 => (WaverBank, (ushort)0x88da),
            NamedFrameId.waver_spin_right_2 => (WaverBank, (ushort)0x8887),
            NamedFrameId.waver_spin_right_3 => (WaverBank, (ushort)0x889d),
            NamedFrameId.metaree_idle_0 => (SkreeMetareeBank, (ushort)0x8b65),
            NamedFrameId.metaree_idle_1 => (SkreeMetareeBank, (ushort)0x8bb9),
            NamedFrameId.metaree_idle_2 => (SkreeMetareeBank, (ushort)0x8bde),
            NamedFrameId.metaree_idle_3 => (SkreeMetareeBank, (ushort)0x8bea),
            NamedFrameId.metaree_prepare_1 => (SkreeMetareeBank, (ushort)0x8b94),
            NamedFrameId.skree_idle_0 => (SkreeMetareeBank, (ushort)0xc842),
            NamedFrameId.skree_idle_1 => (SkreeMetareeBank, (ushort)0xc878),
            NamedFrameId.skree_idle_2 => (SkreeMetareeBank, (ushort)0xc884),
            NamedFrameId.skree_idle_3 => (SkreeMetareeBank, (ushort)0xc89a),
            NamedFrameId.skree_prepare_1 => (SkreeMetareeBank, (ushort)0xc862),
            >= NamedFrameId.zoa_shoot_left_0 and <= NamedFrameId.zoa_shoot_left_2 =>
                (ZoaBank, (ushort)(0xb55f + ((int)frame - (int)NamedFrameId.zoa_shoot_left_0) * NativeFrameBytes(1))),
            NamedFrameId.zoa_rise_left_0 => (ZoaBank, (ushort)0xb57b),
            NamedFrameId.zoa_rise_left_1 => (ZoaBank, (ushort)0xb574),
            NamedFrameId.zoa_rise_left_2 => (ZoaBank, (ushort)0xb582),
            >= NamedFrameId.zoa_shoot_right_0 and <= NamedFrameId.zoa_shoot_right_2 =>
                (ZoaBank, (ushort)(0xb589 + ((int)frame - (int)NamedFrameId.zoa_shoot_right_0) * NativeFrameBytes(1))),
            NamedFrameId.zoa_rise_right_0 => (ZoaBank, (ushort)0xb5a5),
            NamedFrameId.zoa_rise_right_1 => (ZoaBank, (ushort)0xb59e),
            NamedFrameId.zoa_rise_right_2 => (ZoaBank, (ushort)0xb5ac),
            >= NamedFrameId.pipe_brinstar_normal_left_0 and <= NamedFrameId.pipe_brinstar_normal_left_4 =>
                (PipeBugBank, (ushort)(0x89b7 + ((int)frame - (int)NamedFrameId.pipe_brinstar_normal_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_brinstar_normal_right_0 and <= NamedFrameId.pipe_brinstar_normal_right_4 =>
                (PipeBugBank, (ushort)(0x89da + ((int)frame - (int)NamedFrameId.pipe_brinstar_normal_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_brinstar_strong_shoot_left_0 and <= NamedFrameId.pipe_brinstar_strong_shoot_left_2 =>
                (PipeBugBank, (ushort)(0x8a6d + ((int)frame - (int)NamedFrameId.pipe_brinstar_strong_shoot_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_brinstar_strong_rise_left_0 and <= NamedFrameId.pipe_brinstar_strong_rise_left_2 =>
                (PipeBugBank, (ushort)(0x8a82 + ((int)frame - (int)NamedFrameId.pipe_brinstar_strong_rise_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_brinstar_strong_shoot_right_0 and <= NamedFrameId.pipe_brinstar_strong_shoot_right_2 =>
                (PipeBugBank, (ushort)(0x8a97 + ((int)frame - (int)NamedFrameId.pipe_brinstar_strong_shoot_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_brinstar_strong_rise_right_0 and <= NamedFrameId.pipe_brinstar_strong_rise_right_2 =>
                (PipeBugBank, (ushort)(0x8aac + ((int)frame - (int)NamedFrameId.pipe_brinstar_strong_rise_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_norfair_left_0 and <= NamedFrameId.pipe_norfair_left_4 =>
                (PipeBugBank, (ushort)(0x8e96 + ((int)frame - (int)NamedFrameId.pipe_norfair_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_norfair_right_0 and <= NamedFrameId.pipe_norfair_right_4 =>
                (PipeBugBank, (ushort)(0x8eb9 + ((int)frame - (int)NamedFrameId.pipe_norfair_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_yellow_fly_left_0 and <= NamedFrameId.pipe_yellow_fly_left_2 =>
                (PipeBugBank, (ushort)(0x92ad + ((int)frame - (int)NamedFrameId.pipe_yellow_fly_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_yellow_arc_left_0 and <= NamedFrameId.pipe_yellow_arc_left_2 =>
                (PipeBugBank, (ushort)(0x92c2 + ((int)frame - (int)NamedFrameId.pipe_yellow_arc_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_yellow_fly_right_0 and <= NamedFrameId.pipe_yellow_fly_right_2 =>
                (PipeBugBank, (ushort)(0x92d7 + ((int)frame - (int)NamedFrameId.pipe_yellow_fly_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.pipe_yellow_arc_right_0 and <= NamedFrameId.pipe_yellow_arc_right_2 =>
                (PipeBugBank, (ushort)(0x92ec + ((int)frame - (int)NamedFrameId.pipe_yellow_arc_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.fake_kraid_walk_left_0 and <= NamedFrameId.fake_kraid_walk_left_3 =>
                (FakeKraidBank, (ushort)(0x9c64 + ((int)frame - (int)NamedFrameId.fake_kraid_walk_left_0) * NativeFrameBytes(16))),
            >= NamedFrameId.fake_kraid_spit_left_0 and <= NamedFrameId.fake_kraid_spit_left_2 =>
                (FakeKraidBank, (ushort)(0x9dac + ((int)frame - (int)NamedFrameId.fake_kraid_spit_left_0) * NativeFrameBytes(16))),
            >= NamedFrameId.fake_kraid_walk_right_0 and <= NamedFrameId.fake_kraid_walk_right_3 =>
                (FakeKraidBank, (ushort)(0x9ea2 + ((int)frame - (int)NamedFrameId.fake_kraid_walk_right_0) * NativeFrameBytes(16))),
            >= NamedFrameId.fake_kraid_spit_right_0 and <= NamedFrameId.fake_kraid_spit_right_2 =>
                (FakeKraidBank, (ushort)(0x9fea + ((int)frame - (int)NamedFrameId.fake_kraid_spit_right_0) * NativeFrameBytes(16))),
            NamedFrameId.kraid_nail_0 => (KraidNailBank, (ushort)0xa617),
            NamedFrameId.kraid_nail_1 => (KraidNailBank, (ushort)0xa623),
            NamedFrameId.kraid_nail_2 => (KraidNailBank, (ushort)0xa639),
            NamedFrameId.kraid_nail_3 => (KraidNailBank, (ushort)0xa645),
            NamedFrameId.kraid_nail_4 => (KraidNailBank, (ushort)0xa65b),
            NamedFrameId.kraid_nail_5 => (KraidNailBank, (ushort)0xa667),
            NamedFrameId.kraid_nail_6 => (KraidNailBank, (ushort)0xa67d),
            NamedFrameId.kraid_nail_7 => (KraidNailBank, (ushort)0xa689),
            >= NamedFrameId.owtch_left_0 and <= NamedFrameId.owtch_left_2 =>
                (OwtchStokeBank, (ushort)(0xa589 + ((int)frame - (int)NamedFrameId.owtch_left_0) * NativeFrameBytes(1))),
            NamedFrameId.stoke_walk_left_0 => (OwtchStokeBank, (ushort)0x8aca),
            NamedFrameId.stoke_walk_left_1 => (OwtchStokeBank, (ushort)0x8ad6),
            NamedFrameId.stoke_walk_left_2 => (OwtchStokeBank, (ushort)0x8ae7),
            NamedFrameId.stoke_walk_left_3 => (OwtchStokeBank, (ushort)0x8af3),
            NamedFrameId.stoke_attack_left => (OwtchStokeBank, (ushort)0x8aff),
            NamedFrameId.stoke_walk_right_0 => (OwtchStokeBank, (ushort)0x8b15),
            NamedFrameId.stoke_walk_right_1 => (OwtchStokeBank, (ushort)0x8b21),
            NamedFrameId.stoke_walk_right_2 => (OwtchStokeBank, (ushort)0x8b32),
            NamedFrameId.stoke_walk_right_3 => (OwtchStokeBank, (ushort)0x8b3e),
            NamedFrameId.stoke_attack_right => (OwtchStokeBank, (ushort)0x8b4a),
            NamedFrameId.ripper_shared_left_0 => (RipperBank, (ushort)0xe3c5),
            NamedFrameId.ripper_shared_left_1 => (RipperBank, (ushort)0xe3db),
            NamedFrameId.ripper_shared_left_2 => (RipperBank, (ushort)0xe3ec),
            NamedFrameId.ripper_shared_right_0 => (RipperBank, (ushort)0xe402),
            NamedFrameId.ripper_shared_right_1 => (RipperBank, (ushort)0xe418),
            NamedFrameId.ripper_shared_right_2 => (RipperBank, (ushort)0xe429),
            NamedFrameId.ripper_shared_frozen_left => (RipperBank, (ushort)0xe43f),
            NamedFrameId.ripper_shared_frozen_right => (RipperBank, (ushort)0xe44b),
            >= NamedFrameId.ripper_left_0 and <= NamedFrameId.ripper_left_2 =>
                (RipperBank, (ushort)(0xe527 + ((int)frame - (int)NamedFrameId.ripper_left_0) * NativeFrameBytes(2))),
            >= NamedFrameId.ripper_right_0 and <= NamedFrameId.ripper_right_2 =>
                (RipperBank, (ushort)(0xe54b + ((int)frame - (int)NamedFrameId.ripper_right_0) * NativeFrameBytes(2))),
            >= NamedFrameId.fireflea_cycle_0 and <= NamedFrameId.fireflea_cycle_20 =>
                (FirefleaBank, (ushort)(0x8ea5 + ((int)frame - (int)NamedFrameId.fireflea_cycle_0) * NativeFrameBytes(3))),
            >= NamedFrameId.magdollite_left_idle_0 and <= NamedFrameId.magdollite_left_idle_2 =>
                (MagdolliteBank, (ushort)(0xb448 + ((int)frame - (int)NamedFrameId.magdollite_left_idle_0) * NativeFrameBytes(3))),
            >= NamedFrameId.magdollite_left_throw_0 and <= NamedFrameId.magdollite_left_throw_2 =>
                (MagdolliteBank, (ushort)(0xb47b + ((int)frame - (int)NamedFrameId.magdollite_left_throw_0) * NativeFrameBytes(3))),
            NamedFrameId.magdollite_left_throw_3 => (MagdolliteBank, (ushort)0xb4a9),
            NamedFrameId.magdollite_pillar_cap => (MagdolliteBank, (ushort)0xb4b5),
            NamedFrameId.magdollite_left_submerge_0 => (MagdolliteBank, (ushort)0xb4c1),
            NamedFrameId.magdollite_left_submerge_1 => (MagdolliteBank, (ushort)0xb4cf),
            NamedFrameId.magdollite_left_submerge_2 => (MagdolliteBank, (ushort)0xb4e0),
            >= NamedFrameId.magdollite_right_idle_0 and <= NamedFrameId.magdollite_right_idle_2 =>
                (MagdolliteBank, (ushort)(0xb4f1 + ((int)frame - (int)NamedFrameId.magdollite_right_idle_0) * NativeFrameBytes(3))),
            >= NamedFrameId.magdollite_right_throw_0 and <= NamedFrameId.magdollite_right_throw_2 =>
                (MagdolliteBank, (ushort)(0xb524 + ((int)frame - (int)NamedFrameId.magdollite_right_throw_0) * NativeFrameBytes(3))),
            NamedFrameId.magdollite_right_throw_3 => (MagdolliteBank, (ushort)0xb552),
            NamedFrameId.magdollite_right_submerge_0 => (MagdolliteBank, (ushort)0xb56a),
            NamedFrameId.magdollite_right_submerge_1 => (MagdolliteBank, (ushort)0xb578),
            NamedFrameId.magdollite_right_submerge_2 => (MagdolliteBank, (ushort)0xb589),
            NamedFrameId.magdollite_pillar_phase_0 => (MagdolliteBank, (ushort)0xb59a),
            NamedFrameId.magdollite_pillar_phase_1 => (MagdolliteBank, (ushort)0xb5a1),
            NamedFrameId.magdollite_pillar_phase_2 => (MagdolliteBank, (ushort)0xb5ad),
            NamedFrameId.magdollite_pillar_phase_3 => (MagdolliteBank, (ushort)0xb5be),
            NamedFrameId.magdollite_pillar_phase_4 => (MagdolliteBank, (ushort)0xb5d4),
            NamedFrameId.magdollite_pillar_phase_5 => (MagdolliteBank, (ushort)0xb5ef),
            NamedFrameId.magdollite_pillar_phase_6 => (MagdolliteBank, (ushort)0xb60f),
            NamedFrameId.magdollite_pillar_phase_7 => (MagdolliteBank, (ushort)0xb634),
            NamedFrameId.ceres_door_rotating_overlay => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xf921),
            NamedFrameId.ceres_door_left_hold => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xf95f),
            NamedFrameId.ceres_door_left_transition_0 => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xf989),
            >= NamedFrameId.ceres_door_left_transition_1 and <= NamedFrameId.ceres_door_left_transition_3 =>
                (CeresDoorInstructionProgramDefinitions.Bank, (ushort)(0xf9b3 + ((int)frame - (int)NamedFrameId.ceres_door_left_transition_1) * NativeFrameBytes(6))),
            NamedFrameId.ceres_door_right_hold => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xfa13),
            NamedFrameId.ceres_door_right_transition_0 => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xfa3d),
            >= NamedFrameId.ceres_door_right_transition_1 and <= NamedFrameId.ceres_door_right_transition_3 =>
                (CeresDoorInstructionProgramDefinitions.Bank, (ushort)(0xfa67 + ((int)frame - (int)NamedFrameId.ceres_door_right_transition_1) * NativeFrameBytes(6))),
            NamedFrameId.ceres_door_initial => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)CeresDoorInstructionProgramDefinitions.InitialSpritemap),
            NamedFrameId.ceres_door_mode7_left_wall => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xface),
            NamedFrameId.ceres_door_mode7_right_wall => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)0xfb2f),
            NamedFrameId.ceres_door_ridley_private_overlay => (CeresDoorInstructionProgramDefinitions.Bank, (ushort)CeresDoorInstructionProgramDefinitions.RidleyPrivateOverlaySpritemap),
            NamedFrameId.ceres_baby_horizontal => (CeresBabyInstructionProgramDefinitions.Bank, (ushort)CeresBabyInstructionProgramDefinitions.HorizontalFrame),
            NamedFrameId.ceres_baby_round => (CeresBabyInstructionProgramDefinitions.Bank, (ushort)CeresBabyInstructionProgramDefinitions.RoundFrame),
            NamedFrameId.ceres_baby_vertical => (CeresBabyInstructionProgramDefinitions.Bank, (ushort)CeresBabyInstructionProgramDefinitions.VerticalFrame),
            NamedFrameId.rio_bd6c => (RioBank, (ushort)0xbd6c),
            NamedFrameId.rio_bd82 => (RioBank, (ushort)0xbd82),
            NamedFrameId.rio_bd98 => (RioBank, (ushort)0xbd98),
            NamedFrameId.rio_bdae => (RioBank, (ushort)0xbdae),
            NamedFrameId.rio_bdc4 => (RioBank, (ushort)0xbdc4),
            NamedFrameId.rio_bdda => (RioBank, (ushort)0xbdda),
            NamedFrameId.rio_bdf0 => (RioBank, (ushort)0xbdf0),
            NamedFrameId.rio_be06 => (RioBank, (ushort)0xbe06),
            NamedFrameId.lower_norfair_rio_c8bd => (LowerNorfairRioBank, (ushort)0xc8bd),
            NamedFrameId.lower_norfair_rio_c8d3 => (LowerNorfairRioBank, (ushort)0xc8d3),
            NamedFrameId.lower_norfair_rio_c8e9 => (LowerNorfairRioBank, (ushort)0xc8e9),
            NamedFrameId.lower_norfair_rio_c8ff => (LowerNorfairRioBank, (ushort)0xc8ff),
            NamedFrameId.lower_norfair_rio_c915 => (LowerNorfairRioBank, (ushort)0xc915),
            NamedFrameId.lower_norfair_rio_c92b => (LowerNorfairRioBank, (ushort)0xc92b),
            NamedFrameId.lower_norfair_rio_c941 => (LowerNorfairRioBank, (ushort)0xc941),
            NamedFrameId.lower_norfair_rio_c957 => (LowerNorfairRioBank, (ushort)0xc957),
            NamedFrameId.lower_norfair_rio_c96d => (LowerNorfairRioBank, (ushort)0xc96d),
            NamedFrameId.lower_norfair_rio_c983 => (LowerNorfairRioBank, (ushort)0xc983),
            NamedFrameId.lower_norfair_rio_c999 => (LowerNorfairRioBank, (ushort)0xc999),
            NamedFrameId.lower_norfair_rio_c9af => (LowerNorfairRioBank, (ushort)0xc9af),
            NamedFrameId.lower_norfair_rio_c9c5 => (LowerNorfairRioBank, (ushort)0xc9c5),
            NamedFrameId.lower_norfair_rio_c9db => (LowerNorfairRioBank, (ushort)0xc9db),
            NamedFrameId.lower_norfair_rio_c9f1 => (LowerNorfairRioBank, (ushort)0xc9f1),
            NamedFrameId.lower_norfair_rio_ca07 => (LowerNorfairRioBank, (ushort)0xca07),
            NamedFrameId.lower_norfair_rio_ca13 => (LowerNorfairRioBank, (ushort)0xca13),
            NamedFrameId.lower_norfair_rio_ca1f => (LowerNorfairRioBank, (ushort)0xca1f),
            NamedFrameId.norfair_rio_c427 => (NorfairRioBank, (ushort)0xc427),
            NamedFrameId.norfair_rio_c442 => (NorfairRioBank, (ushort)0xc442),
            NamedFrameId.norfair_rio_c45d => (NorfairRioBank, (ushort)0xc45d),
            NamedFrameId.norfair_rio_c493 => (NorfairRioBank, (ushort)0xc493),
            NamedFrameId.norfair_rio_c49f => (NorfairRioBank, (ushort)0xc49f),
            NamedFrameId.norfair_rio_c4ab => (NorfairRioBank, (ushort)0xc4ab),
            NamedFrameId.norfair_rio_c4b7 => (NorfairRioBank, (ushort)0xc4b7),
            NamedFrameId.norfair_rio_c4d2 => (NorfairRioBank, (ushort)0xc4d2),
            NamedFrameId.norfair_rio_c4ed => (NorfairRioBank, (ushort)0xc4ed),
            NamedFrameId.norfair_rio_c508 => (NorfairRioBank, (ushort)0xc508),
            NamedFrameId.norfair_rio_c523 => (NorfairRioBank, (ushort)0xc523),
            NamedFrameId.norfair_rio_c534 => (NorfairRioBank, (ushort)0xc534),
            NamedFrameId.norfair_rio_c54f => (NorfairRioBank, (ushort)0xc54f),
            NamedFrameId.norfair_rio_c56a => (NorfairRioBank, (ushort)0xc56a),
            NamedFrameId.norfair_rio_c585 => (NorfairRioBank, (ushort)0xc585),
            NamedFrameId.norfair_rio_c5a0 => (NorfairRioBank, (ushort)0xc5a0),
            NamedFrameId.norfair_rio_c5bb => (NorfairRioBank, (ushort)0xc5bb),
            NamedFrameId.norfair_rio_c5d6 => (NorfairRioBank, (ushort)0xc5d6),
            NamedFrameId.norfair_rio_c5e2 => (NorfairRioBank, (ushort)0xc5e2),
            NamedFrameId.norfair_rio_c5ee => (NorfairRioBank, (ushort)0xc5ee),
            >= NamedFrameId.puyo_ground_0 and <= NamedFrameId.puyo_ground_2 =>
                (PuyoBank, (ushort)(0x9df6 + ((int)frame - (int)NamedFrameId.puyo_ground_0) * NativeFrameBytes(2))),
            NamedFrameId.puyo_air_0 => (PuyoBank, (ushort)0x9e1a),
            NamedFrameId.puyo_air_1 => (PuyoBank, (ushort)0x9e26),
            NamedFrameId.puyo_air_2 => (PuyoBank, (ushort)0x9e37),
            NamedFrameId.puyo_air_3 => (PuyoBank, (ushort)0x9e4d),
            NamedFrameId.puyo_air_4 => (PuyoBank, (ushort)0x9e5e),
            >= NamedFrameId.bull_idle_0 and <= NamedFrameId.bull_idle_2 =>
                (BullBank, (ushort)(0xdb76 + ((int)frame - (int)NamedFrameId.bull_idle_0) * NativeFrameBytes(4))),
            >= NamedFrameId.alcoon_left_walk_0 and <= NamedFrameId.alcoon_left_walk_2 =>
                (AlcoonBank, (ushort)(0xdfa2 + ((int)frame - (int)NamedFrameId.alcoon_left_walk_0) * NativeFrameBytes(6))),
            NamedFrameId.alcoon_left_walk_3 => (AlcoonBank, (ushort)0xe007),
            NamedFrameId.alcoon_left_fire_0 => (AlcoonBank, (ushort)0xe027),
            NamedFrameId.alcoon_left_fire_1 => (AlcoonBank, (ushort)0xe047),
            NamedFrameId.alcoon_left_fire_2 => (AlcoonBank, (ushort)0xe06c),
            NamedFrameId.alcoon_left_fire_3 => (AlcoonBank, (ushort)0xe09b),
            NamedFrameId.alcoon_left_air_up => (AlcoonBank, (ushort)0xe0bb),
            >= NamedFrameId.alcoon_right_walk_0 and <= NamedFrameId.alcoon_right_walk_2 =>
                (AlcoonBank, (ushort)(0xe0db + ((int)frame - (int)NamedFrameId.alcoon_right_walk_0) * NativeFrameBytes(6))),
            NamedFrameId.alcoon_right_walk_3 => (AlcoonBank, (ushort)0xe140),
            NamedFrameId.alcoon_right_fire_0 => (AlcoonBank, (ushort)0xe160),
            NamedFrameId.alcoon_right_fire_1 => (AlcoonBank, (ushort)0xe180),
            NamedFrameId.alcoon_right_fire_2 => (AlcoonBank, (ushort)0xe1a5),
            NamedFrameId.alcoon_right_fire_3 => (AlcoonBank, (ushort)0xe1d4),
            NamedFrameId.alcoon_right_air_up => (AlcoonBank, (ushort)0xe1f4),
            >= NamedFrameId.beetom_left_crawl_0 and <= NamedFrameId.beetom_left_crawl_2 =>
                (BeetomBank, (ushort)(0xbed3 + ((int)frame - (int)NamedFrameId.beetom_left_crawl_0) * NativeFrameBytes(5))),
            NamedFrameId.beetom_left_hop_0 => (BeetomBank, (ushort)0xbf24),
            NamedFrameId.beetom_left_hop_1 => (BeetomBank, (ushort)0xbf3f),
            >= NamedFrameId.beetom_left_drain_0 and <= NamedFrameId.beetom_left_drain_3 =>
                (BeetomBank, (ushort)(0xbf5a + ((int)frame - (int)NamedFrameId.beetom_left_drain_0) * NativeFrameBytes(5))),
            NamedFrameId.beetom_left_drain_4 => (BeetomBank, (ushort)0xbfcb),
            NamedFrameId.beetom_left_drain_5 => (BeetomBank, (ushort)0xbfeb),
            >= NamedFrameId.beetom_right_crawl_0 and <= NamedFrameId.beetom_right_crawl_2 =>
                (BeetomBank, (ushort)(0xc00b + ((int)frame - (int)NamedFrameId.beetom_right_crawl_0) * NativeFrameBytes(5))),
            NamedFrameId.beetom_right_hop_0 => (BeetomBank, (ushort)0xc05c),
            NamedFrameId.beetom_right_hop_1 => (BeetomBank, (ushort)0xc077),
            >= NamedFrameId.beetom_right_drain_0 and <= NamedFrameId.beetom_right_drain_3 =>
                (BeetomBank, (ushort)(0xc092 + ((int)frame - (int)NamedFrameId.beetom_right_drain_0) * NativeFrameBytes(5))),
            NamedFrameId.beetom_right_drain_4 => (BeetomBank, (ushort)0xc103),
            NamedFrameId.beetom_right_drain_5 => (BeetomBank, (ushort)0xc123),
            NamedFrameId.sidehopper_jump_floor => (HopperBank, (ushort)0xaf19),
            NamedFrameId.sidehopper_land_floor_0 => (HopperBank, (ushort)0xaee3),
            NamedFrameId.sidehopper_land_floor_1 => (HopperBank, (ushort)0xaefe),
            NamedFrameId.sidehopper_jump_ceiling => (HopperBank, (ushort)0xaf6a),
            NamedFrameId.sidehopper_land_ceiling_0 => (HopperBank, (ushort)0xaf34),
            NamedFrameId.sidehopper_land_ceiling_1 => (HopperBank, (ushort)0xaf4f),
            NamedFrameId.dessgeega_jump_floor => (HopperBank, (ushort)0xb019),
            NamedFrameId.dessgeega_land_floor_0 => (HopperBank, (ushort)0xafe3),
            NamedFrameId.dessgeega_land_floor_1 => (HopperBank, (ushort)0xaffe),
            NamedFrameId.dessgeega_jump_ceiling => (HopperBank, (ushort)0xb06a),
            NamedFrameId.dessgeega_land_ceiling_0 => (HopperBank, (ushort)0xb034),
            NamedFrameId.dessgeega_land_ceiling_1 => (HopperBank, (ushort)0xb04f),
            NamedFrameId.large_sidehopper_jump_floor => (HopperBank, (ushort)0xb15b),
            NamedFrameId.large_sidehopper_land_floor_0 => (HopperBank, (ushort)0xb111),
            NamedFrameId.large_sidehopper_land_floor_1 => (HopperBank, (ushort)0xb136),
            NamedFrameId.large_sidehopper_jump_ceiling => (HopperBank, (ushort)0xb1de),
            NamedFrameId.large_sidehopper_land_ceiling_0 => (HopperBank, (ushort)0xb194),
            NamedFrameId.large_sidehopper_land_ceiling_1 => (HopperBank, (ushort)0xb1b9),
            NamedFrameId.large_dessgeega_jump_floor => (HopperBank, (ushort)0xb2d1),
            NamedFrameId.large_dessgeega_land_floor_0 => (HopperBank, (ushort)0xb273),
            NamedFrameId.large_dessgeega_land_floor_1 => (HopperBank, (ushort)0xb2a2),
            NamedFrameId.large_dessgeega_jump_ceiling => (HopperBank, (ushort)0xb368),
            NamedFrameId.large_dessgeega_land_ceiling_0 => (HopperBank, (ushort)0xb30a),
            NamedFrameId.large_dessgeega_land_ceiling_1 => (HopperBank, (ushort)0xb339),
            NamedFrameId.choot_idle => (ChootBank, (ushort)0xe146),
            NamedFrameId.choot_jump => (ChootBank, (ushort)0xe15c),
            NamedFrameId.choot_jump_apex => (ChootBank, (ushort)0xe168),
            NamedFrameId.choot_fall_end => (ChootBank, (ushort)0xe16f),
            >= NamedFrameId.hzoomer_upside_right_0 and <= NamedFrameId.hzoomer_upside_right_4 =>
                (HZoomerBank, (ushort)(0xe50e + ((int)frame - (int)NamedFrameId.hzoomer_upside_right_0) * NativeFrameBytes(4))),
            >= NamedFrameId.hzoomer_upside_left_0 and <= NamedFrameId.hzoomer_upside_left_4 =>
                (HZoomerBank, (ushort)(0xe3c4 + ((int)frame - (int)NamedFrameId.hzoomer_upside_left_0) * NativeFrameBytes(4))),
            >= NamedFrameId.hzoomer_upside_down_0 and <= NamedFrameId.hzoomer_upside_down_4 =>
                (HZoomerBank, (ushort)(0xe432 + ((int)frame - (int)NamedFrameId.hzoomer_upside_down_0) * NativeFrameBytes(4))),
            >= NamedFrameId.hzoomer_upside_up_0 and <= NamedFrameId.hzoomer_upside_up_4 =>
                (HZoomerBank, (ushort)(0xe2e8 + ((int)frame - (int)NamedFrameId.hzoomer_upside_up_0) * NativeFrameBytes(4))),
            >= NamedFrameId.sbug_right_0 and <= NamedFrameId.sbug_right_2 =>
                (SbugBank, (ushort)(0xa67d + ((int)frame - (int)NamedFrameId.sbug_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_up_right_0 and <= NamedFrameId.sbug_up_right_2 =>
                (SbugBank, (ushort)(0xa692 + ((int)frame - (int)NamedFrameId.sbug_up_right_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_up_0 and <= NamedFrameId.sbug_up_2 =>
                (SbugBank, (ushort)(0xa6a7 + ((int)frame - (int)NamedFrameId.sbug_up_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_up_left_0 and <= NamedFrameId.sbug_up_left_2 =>
                (SbugBank, (ushort)(0xa6bc + ((int)frame - (int)NamedFrameId.sbug_up_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_left_0 and <= NamedFrameId.sbug_left_2 =>
                (SbugBank, (ushort)(0xa6d1 + ((int)frame - (int)NamedFrameId.sbug_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_down_left_0 and <= NamedFrameId.sbug_down_left_2 =>
                (SbugBank, (ushort)(0xa6e6 + ((int)frame - (int)NamedFrameId.sbug_down_left_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_down_0 and <= NamedFrameId.sbug_down_2 =>
                (SbugBank, (ushort)(0xa6fb + ((int)frame - (int)NamedFrameId.sbug_down_0) * NativeFrameBytes(1))),
            >= NamedFrameId.sbug_down_right_0 and <= NamedFrameId.sbug_down_right_2 =>
                (SbugBank, (ushort)(0xa710 + ((int)frame - (int)NamedFrameId.sbug_down_right_0) * NativeFrameBytes(1))),
            NamedFrameId.fune_left_idle => (FuneNamiheBank, (ushort)0x93f9),
            >= NamedFrameId.fune_left_active_0 and <= NamedFrameId.fune_left_active_3 =>
                (FuneNamiheBank, (ushort)(0x9423 + ((int)frame - (int)NamedFrameId.fune_left_active_0) * NativeFrameBytes(8))),
            NamedFrameId.fune_right_idle => (FuneNamiheBank, (ushort)0x94cb),
            >= NamedFrameId.fune_right_active_0 and <= NamedFrameId.fune_right_active_3 =>
                (FuneNamiheBank, (ushort)(0x94f5 + ((int)frame - (int)NamedFrameId.fune_right_active_0) * NativeFrameBytes(8))),
            NamedFrameId.namihe_left_idle => (FuneNamiheBank, (ushort)0x97b4),
            >= NamedFrameId.namihe_left_active_0 and <= NamedFrameId.namihe_left_active_4 =>
                (FuneNamiheBank, (ushort)(0x97de + ((int)frame - (int)NamedFrameId.namihe_left_active_0) * NativeFrameBytes(8))),
            NamedFrameId.namihe_right_idle => (FuneNamiheBank, (ushort)0x98b0),
            >= NamedFrameId.namihe_right_active_0 and <= NamedFrameId.namihe_right_active_4 =>
                (FuneNamiheBank, (ushort)(0x98da + ((int)frame - (int)NamedFrameId.namihe_right_active_0) * NativeFrameBytes(8))),
            >= NamedFrameId.kamer_platform_0 and <= NamedFrameId.kamer_platform_3 =>
                (KamerPlatformBank, (ushort)(0xf468 + ((int)frame - (int)NamedFrameId.kamer_platform_0) * NativeFrameBytes(2))),
            NamedFrameId.elevator_platform_0 => (ElevatorBank, (ushort)0x962f),
            NamedFrameId.elevator_platform_1 => (ElevatorBank, (ushort)0x9645),
            >= NamedFrameId.draygon_intro_evir_0 and <= NamedFrameId.draygon_intro_evir_3 =>
                (RoomSpriteObjectBank, (ushort)(0xdb42 + ((int)frame - (int)NamedFrameId.draygon_intro_evir_0) * NativeFrameBytes(12))),
            NamedFrameId.draygon_breath_bubble_0 => (RoomSpriteObjectBank, (ushort)0xc920),
            >= NamedFrameId.draygon_breath_bubble_1 and <= NamedFrameId.draygon_breath_bubble_7 =>
                (RoomSpriteObjectBank, (ushort)(0xc927 + ((int)frame - (int)NamedFrameId.draygon_breath_bubble_1) * NativeFrameBytes(3))),
            NamedFrameId.draygon_breath_bubble_8 => (RoomSpriteObjectBank, (ushort)0xc999),
            _ => throw new IndexOutOfRangeException(),
        };
        return new(bank, pointer, frame.ToString());
    }

    private static IEnumerable<EnemySpritemapDefinition> NamedFrames()
    {
        for (var frame = NamedFrameId.boyon_idle_0; frame <= NamedFrameId.draygon_breath_bubble_8; frame++)
            yield return NamedFrame(frame);
    }


    // All other bank-$B4 presentation targets are shared by the 62 compiled
    // sprite-object programs. Build their stable, address-named art identities
    // from the compiled selectors; do not duplicate their pointer list here.
    private static IEnumerable<EnemySpritemapDefinition> EnumerateFrames()
    {
        foreach (var frame in NamedFrames())
            yield return frame;
        foreach (var frame in RoomSpriteObjectVisualDefinitions.AdditionalFrames(NamedFrames()))
            yield return frame;
        foreach (var frame in YappingMawVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KiHunterVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MotherBrainVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DeadTorizoArtworkDefinitions.Frames())
            yield return frame;
        foreach (var frame in RidleySupplementalVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in SciserVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in FlyVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KagoVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in BlueBrinstarFaceBlockVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MorphBallEyeVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShutterVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MetroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShaktoolVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in TripperKamerVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DragonVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MultiviolaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in NorfairLavaJumperVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ChozoStatueVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ViolaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in RinkaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DeadTorizoArtworkDefinitions.StationaryFrames())
            yield return frame;
        foreach (var frame in DeadTourianCorpseVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MochtroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in EvirVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in WorkRobotVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in YardVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in BotwoonVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in GunshipVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MamaTurtleVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ZeroVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in FriendlyAnimalVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in HibashiVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ZebetiteVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in WreckedShipGhostVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in PowampVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in SparkVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShitroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KraidLintVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in NuclearWaffleVisualDefinitions.Frames())
            yield return frame;
        yield return SingleFrameEnemyVisualDefinitions.Kzan;
        yield return SingleFrameEnemyVisualDefinitions.Polyp;
        foreach (var frame in RidleyBreakupVisualDefinitions.Legs) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Torso) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Head) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Claw) yield return frame;
    }

    internal static FrameList Frames { get; } = new();

    internal sealed class FrameList : IReadOnlyList<EnemySpritemapDefinition>
    {
        // The frame list is a fixed derivation of the definitions; derive it once, not per access.
        private readonly Lazy<EnemySpritemapDefinition[]> frames = new(() => EnumerateFrames().ToArray());
        public int Count => frames.Value.Length;
        internal int Length => Count;
        public EnemySpritemapDefinition this[int index] => (uint)index < Count
            ? frames.Value[index] : throw new IndexOutOfRangeException();
        internal EnemySpritemapDefinition[] this[Range range] => frames.Value[range];
        public IEnumerator<EnemySpritemapDefinition> GetEnumerator() => ((IEnumerable<EnemySpritemapDefinition>)frames.Value).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }


    /// <summary>
    /// Selects only families whose fixed instruction visual operands are compiled.
    /// Other families use the compiled selector catalog at the interpreter boundary.
    /// Known families reject an unlisted operand rather than reading adjacent data.
    /// </summary>
    internal static bool TryFrameAt(ushort enemyDefinition, ushort operandAddress,
        out ushort frame)
    {
        frame = enemyDefinition switch
        {
            MamaTurtleEnemyDefinitionCatalog.MamaPointer or MamaTurtleEnemyDefinitionCatalog.BabyPointer =>
                MamaTurtleVisualDefinitions.FrameAt(operandAddress),
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer =>
                CeresDoorInstructionProgramDefinitions.ReadPresentationFrame(operandAddress),
            RoomEnemySystem.BoyonDefinition => BoyonFrameAt(operandAddress),
            RoomEnemySystem.SciserDefinition => SciserVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.ZeroDefinition => ZeroVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.MellowDefinition or RoomEnemySystem.MellaDefinition or
            RoomEnemySystem.MemuDefinition => FlyVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.KagoDefinition => KagoVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.BlueBrinstarFaceBlockDefinition =>
                BlueBrinstarFaceBlockVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.MorphBallEyeDefinition =>
                MorphBallEyeVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.MetroidDefinition =>
                MetroidVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.ShaktoolDefinition =>
                ShaktoolVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.TripperDefinition or RoomEnemySystem.KamerDefinition =>
                TripperKamerVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.DragonDefinition =>
                DragonVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.MultiviolaDefinition =>
                MultiviolaVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.NorfairLavaJumpingEnemyDefinition =>
                NorfairLavaJumperVisualDefinitions.FrameAt(operandAddress),
            ChozoStatueEnemyDefinitions.EnemyDefinitionPointer =>
                ChozoStatueVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.ViolaDefinition =>
                ViolaVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.RinkaDefinition =>
                RinkaVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.DeadTorizoDefinition =>
                DeadTorizoArtworkDefinitions.StationaryFrameAt(operandAddress),
            RoomEnemySystem.DeadZoomerDefinition or
                RoomEnemySystem.DeadRipperDefinition or
                RoomEnemySystem.DeadSkreeDefinition =>
                DeadTourianCorpseVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.DeadSidehopperDefinition =>
                DeadTourianCorpseVisualDefinitions.SidehopperFrameAt(operandAddress),
            EnemyDefinitionPointers.Mochtroid =>
                MochtroidVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.EvirDefinition or RoomEnemySystem.EvirProjectileDefinition =>
                EvirVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.WorkRobotDefinition or
                RoomEnemySystem.WorkRobotNoPowerDefinition =>
                WorkRobotVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.YardDefinition =>
                YardVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.BotwoonDefinition =>
                BotwoonVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.GrowingShutterDefinition or
                RoomEnemySystem.ShootableVerticalShutterDefinition or
                RoomEnemySystem.DestroyableVerticalShutterDefinition or
                RoomEnemySystem.ShootableHorizontalShutterDefinition =>
                ShutterVisualDefinitions.FrameAt(enemyDefinition, operandAddress),
            RoomEnemySystem.RioDefinition => RioFrameAt(operandAddress),
            RoomEnemySystem.LowerNorfairRioDefinition =>
                LowerNorfairRioFrameAt(operandAddress),
            RoomEnemySystem.NorfairRioDefinition => NorfairRioFrameAt(operandAddress),
            RoomEnemySystem.PuyoDefinition => PuyoFrameAt(operandAddress),
            RoomEnemySystem.BullDefinition => BullFrameAt(operandAddress),
            RoomEnemySystem.AlcoonDefinition => AlcoonFrameAt(operandAddress),
            RoomEnemySystem.BeetomDefinition => BeetomFrameAt(operandAddress),
            RoomEnemySystem.SidehopperDefinition or
                RoomEnemySystem.DessgeegaDefinition or
                RoomEnemySystem.LargeSidehopperDefinition or
                RoomEnemySystem.TourianSidehopperDefinition or
                RoomEnemySystem.LargeDessgeegaDefinition => HopperFrameAt(operandAddress),
            RoomEnemySystem.ChootDefinition => ChootFrameAt(operandAddress),
            RoomEnemySystem.HZoomerDefinition => HZoomerFrameAt(operandAddress),
            RoomEnemySystem.SbugDefinition or RoomEnemySystem.Sbug2Definition =>
                SbugFrameAt(operandAddress),
            FuneNamiheDefinitions.FuneEnemyDefinition or
                FuneNamiheDefinitions.NamiheEnemyDefinition =>
                FuneNamiheFrameAt(operandAddress),
            RoomEnemySystem.KamerVerticalPlatformDefinition =>
                KamerPlatformFrameAt(operandAddress),
            RoomEnemySystem.ElevatorDefinition => ElevatorFrameAt(operandAddress),
            RoomEnemySystem.YappingMawDefinition =>
                YappingMawVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.KiHunterDefinition or
                RoomEnemySystem.KiHunterWingsDefinition or
                RoomEnemySystem.RedKiHunterDefinition or
                RoomEnemySystem.RedKiHunterWingsDefinition or
                RoomEnemySystem.GoldKiHunterDefinition or
                RoomEnemySystem.GoldKiHunterWingsDefinition =>
                KiHunterVisualDefinitions.FrameAt(operandAddress),
            RoomEnemySystem.ZeelaDefinition or
                RoomEnemySystem.SovaDefinition or
                RoomEnemySystem.ZoomerDefinition or
                RoomEnemySystem.StoneZoomerDefinition =>
                SharedCrawlerFrameAt(operandAddress),
            RoomEnemySystem.CacatacDefinition => CacatacFrameAt(operandAddress),
            RoomEnemySystem.FirefleaDefinition => FirefleaFrameAt(operandAddress),
            RoomEnemySystem.MagdolliteDefinition => MagdolliteFrameAt(operandAddress),
            RoomEnemySystem.BoulderDefinition => BoulderFrameAt(operandAddress),
            RoomEnemySystem.AtomicDefinition => AtomicFrameAt(operandAddress),
            RoomEnemySystem.SkulteraDefinition => SkulteraFrameAt(operandAddress),
            RoomEnemySystem.WaverDefinition => WaverFrameAt(operandAddress),
            RoomEnemySystem.ZoaDefinition => ZoaFrameAt(operandAddress),
            RoomEnemySystem.MetareeDefinition => SkreeMetareeFrameAt(true, operandAddress),
            RoomEnemySystem.SkreeDefinition => SkreeMetareeFrameAt(false, operandAddress),
            PipeBugDefinitions.BrinstarEnemyDefinition or
                PipeBugDefinitions.StrongBrinstarEnemyDefinition or
                PipeBugDefinitions.NorfairEnemyDefinition or
                PipeBugDefinitions.YellowEnemyDefinition =>
                PipeBugVisualDefinitions.FrameAt(enemyDefinition, operandAddress),
            RoomEnemySystem.FakeKraidDefinition or
                RoomEnemySystem.KraidGoodNailDefinition or
                RoomEnemySystem.KraidBadNailDefinition =>
                KraidVisualDefinitions.FrameAt(enemyDefinition, operandAddress),
            RoomEnemySystem.OwtchDefinition or RoomEnemySystem.StokeDefinition =>
                OwtchStokeVisualDefinitions.FrameAt(enemyDefinition, operandAddress),
            RoomEnemySystem.GRipperDefinition or RoomEnemySystem.Ripper2Definition or
                RoomEnemySystem.RipperDefinition =>
                RipperVisualDefinitions.FrameAt(enemyDefinition, operandAddress),
            _ => 0,
        };
        return enemyDefinition is
            MamaTurtleEnemyDefinitionCatalog.MamaPointer or MamaTurtleEnemyDefinitionCatalog.BabyPointer or
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer or
            RoomEnemySystem.BoyonDefinition or
            RoomEnemySystem.SciserDefinition or
            RoomEnemySystem.ZeroDefinition or
            RoomEnemySystem.MellowDefinition or RoomEnemySystem.MellaDefinition or
            RoomEnemySystem.MemuDefinition or
            RoomEnemySystem.KagoDefinition or
            RoomEnemySystem.BlueBrinstarFaceBlockDefinition or
            RoomEnemySystem.MorphBallEyeDefinition or
            RoomEnemySystem.MetroidDefinition or
            RoomEnemySystem.ShaktoolDefinition or
            RoomEnemySystem.TripperDefinition or RoomEnemySystem.KamerDefinition or
            RoomEnemySystem.DragonDefinition or
            RoomEnemySystem.MultiviolaDefinition or
            RoomEnemySystem.NorfairLavaJumpingEnemyDefinition or
            ChozoStatueEnemyDefinitions.EnemyDefinitionPointer or
            RoomEnemySystem.ViolaDefinition or
            RoomEnemySystem.RinkaDefinition or
            RoomEnemySystem.DeadTorizoDefinition or
            RoomEnemySystem.DeadZoomerDefinition or
            RoomEnemySystem.DeadRipperDefinition or
            RoomEnemySystem.DeadSkreeDefinition or
            RoomEnemySystem.DeadSidehopperDefinition or
            EnemyDefinitionPointers.Mochtroid or
            RoomEnemySystem.EvirDefinition or
            RoomEnemySystem.EvirProjectileDefinition or
            RoomEnemySystem.WorkRobotDefinition or
            RoomEnemySystem.WorkRobotNoPowerDefinition or
            RoomEnemySystem.YardDefinition or
            RoomEnemySystem.BotwoonDefinition or
            RoomEnemySystem.GrowingShutterDefinition or
            RoomEnemySystem.ShootableVerticalShutterDefinition or
            RoomEnemySystem.DestroyableVerticalShutterDefinition or
            RoomEnemySystem.ShootableHorizontalShutterDefinition or
            RoomEnemySystem.RioDefinition or
            RoomEnemySystem.LowerNorfairRioDefinition or
            RoomEnemySystem.NorfairRioDefinition or
            RoomEnemySystem.PuyoDefinition or
            RoomEnemySystem.BullDefinition or
            RoomEnemySystem.AlcoonDefinition or
            RoomEnemySystem.BeetomDefinition or
            RoomEnemySystem.SidehopperDefinition or
            RoomEnemySystem.DessgeegaDefinition or
            RoomEnemySystem.LargeSidehopperDefinition or
            RoomEnemySystem.TourianSidehopperDefinition or
            RoomEnemySystem.LargeDessgeegaDefinition or
            RoomEnemySystem.ChootDefinition or
            RoomEnemySystem.HZoomerDefinition or
            RoomEnemySystem.SbugDefinition or RoomEnemySystem.Sbug2Definition or
            FuneNamiheDefinitions.FuneEnemyDefinition or
            FuneNamiheDefinitions.NamiheEnemyDefinition or
            RoomEnemySystem.KamerVerticalPlatformDefinition or
            RoomEnemySystem.ElevatorDefinition or
            RoomEnemySystem.YappingMawDefinition or
            RoomEnemySystem.KiHunterDefinition or
            RoomEnemySystem.KiHunterWingsDefinition or
            RoomEnemySystem.RedKiHunterDefinition or
            RoomEnemySystem.RedKiHunterWingsDefinition or
            RoomEnemySystem.GoldKiHunterDefinition or
            RoomEnemySystem.GoldKiHunterWingsDefinition or
            RoomEnemySystem.ZeelaDefinition or
            RoomEnemySystem.SovaDefinition or
            RoomEnemySystem.ZoomerDefinition or
            RoomEnemySystem.StoneZoomerDefinition or
            RoomEnemySystem.CacatacDefinition or RoomEnemySystem.FirefleaDefinition or
            RoomEnemySystem.MagdolliteDefinition or
            RoomEnemySystem.BoulderDefinition or
            RoomEnemySystem.AtomicDefinition or RoomEnemySystem.SkulteraDefinition or
            RoomEnemySystem.WaverDefinition or RoomEnemySystem.ZoaDefinition or
            RoomEnemySystem.MetareeDefinition or RoomEnemySystem.SkreeDefinition or
            PipeBugDefinitions.BrinstarEnemyDefinition or
            PipeBugDefinitions.StrongBrinstarEnemyDefinition or
            PipeBugDefinitions.NorfairEnemyDefinition or
            PipeBugDefinitions.YellowEnemyDefinition or
            RoomEnemySystem.FakeKraidDefinition or
            RoomEnemySystem.KraidGoodNailDefinition or
            RoomEnemySystem.KraidBadNailDefinition or
            RoomEnemySystem.OwtchDefinition or RoomEnemySystem.StokeDefinition or
            RoomEnemySystem.GRipperDefinition or RoomEnemySystem.Ripper2Definition or
            RoomEnemySystem.RipperDefinition;
    }

    /// <summary>Selects opening/recovery poses in the two directional sprite strips.
    /// Every native map has eight five-byte OAM entries and a two-byte count.</summary>
    internal static ushort FuneNamiheFrameAt(ushort operandAddress)
    {
        if (!FuneNamiheInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Fune/Namihe visual operand $A8:{operandAddress:X4} is not compiled.");
        bool namihe = operandAddress >= FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft;
        int stride = namihe ? 52 : 48;
        int offset = operandAddress - (namihe ? FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft
            : FuneNamiheInstructionProgramDefinitions.FuneIdleLeft);
        int local = offset % stride;
        int peak = namihe ? 5 : 4;
        int recovery = 12 + 4 * peak;
        int pose = local == 2 ? 0 : local < recovery ? (local - 8) / 4
            : peak - (local - recovery) / 4;
        return (ushort)((namihe ? 0x97b4 : 0x93f9) + 42 * ((peak + 1) * (offset / stride) + pose));
    }
    /// <summary>Four consecutive Kamer maps, each holding two five-byte OAM
    /// records after its two-byte count, selected in forward animation order.</summary>
    internal static ushort KamerPlatformFrameAt(ushort operandAddress)
    {
        if (!VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Kamer platform visual operand $A2:{operandAddress:X4} is not compiled.");
        int frame = (operandAddress - (VerticalShutterInstructionProgramDefinitions.KamerPlatform + 2)) / 4;
        return (ushort)(0xf468 + 12 * frame);
    }
    /// <summary>Two elevator maps in forward animation order. Each native map
    /// contains four five-byte OAM entries after its two-byte count.</summary>
    internal static ushort ElevatorFrameAt(ushort operandAddress)
    {
        if (!ElevatorInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Elevator visual operand $A3:{operandAddress:X4} is not compiled.");
        int frame = (operandAddress - (ElevatorInstructionProgramDefinitions.Loop + 2)) / 4;
        return (ushort)(0x962f + 22 * frame);
    }
    /// <summary>
    /// Rio's twenty-four fixed presentation operands at $A2:BB4D..BBB5 select
    /// eight distinct extracted OAM compositions. The instruction timing, swoop
    /// callbacks, and hitboxes remain compiled gameplay behavior.
    /// </summary>
    internal static ushort RioFrameAt(ushort operandAddress)
    {
        if (RioInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(RioBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Rio visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Holtz's thirty-two fixed presentation operands at $A2:C61E..C6BA select
    /// eighteen extracted parent/flame OAM compositions. Swoop callbacks,
    /// durations, follower visibility, and hitboxes remain engine-owned.
    /// </summary>
    internal static ushort LowerNorfairRioFrameAt(ushort operandAddress)
    {
        if (LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(LowerNorfairRioBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Lower Norfair Rio visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Geruta's thirty-four fixed presentation operands at $A2:C0F5..C1B1
    /// select twenty extracted parent/flame OAM compositions. Swoop
    /// callbacks, durations, follower offsets, and hitboxes stay compiled.
    /// </summary>
    internal static ushort NorfairRioFrameAt(ushort operandAddress)
    {
        if (NorfairRioInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(NorfairRioBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Norfair Rio visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Puyo's seventeen presentation operands at $A2:99AF..9A03 select eight
    /// editable OAM compositions; hop timing and movement remain compiled.
    /// </summary>
    internal static ushort PuyoFrameAt(ushort operandAddress)
    {
        if (PuyoInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(PuyoBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Puyo visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Bull's eight fixed visual operands at $A8:D843..D867 select three
    /// editable OAM compositions. Its immune-shot loop remains engine-owned.
    /// </summary>
    internal static ushort BullFrameAt(ushort operandAddress)
    {
        if (BullInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(BullBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Bull visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Alcoon's 44 left/right walking, fire-volley, and airborne visual operands
    /// at $A8:DBEB..DCC3 select eighteen editable OAM compositions. Movement,
    /// fireball callbacks, and instruction timing remain compiled gameplay rules.
    /// </summary>
    internal static ushort AlcoonFrameAt(ushort operandAddress)
    {
        if (AlcoonInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            int offset = operandAddress - AlcoonInstructionProgramDefinitions.WalkingLeft;
            int local = offset % 112;
            int pose = local switch
            {
                < 28 => (local - 4) / 6,
                < 94 => ((local - 28) % 22) switch
                {
                    2 => 4,             // Wing extended before the shot.
                    6 or 14 => 5,       // Mouth opening, also used for windup.
                    10 => 6,            // Ready to spit.
                    _ => 7,             // Recovery after firing.
                },
                98 => 7,                // Trailing recovery pose after StartWalking.
                102 => 8,               // Airborne, looking up.
                _ => 3,                 // Airborne, looking forward reuses walking pose.
            };
            // Nine records per facing. Each has a two-byte count and six five-byte
            // OAM entries, except poses2/5 have seven and pose6 has nine:313 bytes.
            return (ushort)(0xdfa2 + 313 * (offset / 112) + 32 * pose +
                (pose > 2 ? 5 : 0) + (pose > 5 ? 5 : 0) + (pose > 6 ? 15 : 0));
        }
        throw new InvalidDataException(
            $"Alcoon visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Beetom's thirty-two crawl, hop, and drain operands select twenty-two
    /// editable compositions; physical attachment and drain cadence stay compiled.
    /// </summary>
    internal static ushort BeetomFrameAt(ushort operandAddress)
    {
        if (BeetomInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            int stride = BeetomInstructionProgramDefinitions.CrawlingRight - BeetomInstructionProgramDefinitions.CrawlingLeft;
            int offset = operandAddress - BeetomInstructionProgramDefinitions.CrawlingLeft;
            int local = offset % stride;
            int pose;
            if (local < BeetomInstructionProgramDefinitions.HopLeft - BeetomInstructionProgramDefinitions.CrawlingLeft)
            {
                int frame = (local - 4) / 4;
                pose = 2 - Math.Abs(2 - frame);
            }
            else if (local < BeetomInstructionProgramDefinitions.DrainingLeft - BeetomInstructionProgramDefinitions.CrawlingLeft)
            {
                int frame = (local - (BeetomInstructionProgramDefinitions.HopLeft - BeetomInstructionProgramDefinitions.CrawlingLeft) - 4) / 4;
                pose = frame == 3 ? 0 : 4 - Math.Abs(1 - frame);
            }
            else
            {
                int drain = local - (BeetomInstructionProgramDefinitions.DrainingLeft - BeetomInstructionProgramDefinitions.CrawlingLeft);
                bool loop = drain >= 20;
                int frame = (drain - (loop ? 20 : 2)) / 4;
                pose = (loop ? 8 : 5) + 2 - Math.Abs(2 - frame);
            }
            // Each facing owns eight five-entry maps (27 bytes) then three six-entry
            // maps (32 bytes). Account for the extra entry only after the eighth map.
            return (ushort)(0xbed3 + 312 * (offset / stride) + 27 * pose + 5 * Math.Max(0, pose - 8));
        }
        throw new InvalidDataException(
            $"Beetom visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The forty small/large Sidehopper and Dessgeega floor/ceiling operands
    /// select twenty-four compositions. Tourian Sidehoppers share the large
    /// Sidehopper instruction lists and visual identities.
    /// </summary>
    internal static ushort HopperFrameAt(ushort operandAddress)
    {
        if (HopperInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(HopperBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Hopper visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The five <c>$A2:D830-D848</c> Choot visual operands select four
    /// installed idle, jump, apex, and falling compositions.
    /// </summary>
    internal static ushort ChootFrameAt(ushort operandAddress)
    {
        if (ChootInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(ChootBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Choot visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The twenty <c>$A3:DFD1-E035</c> HZoomer visual operands select
    /// installed five-frame compositions for each of four surface orientations.
    /// </summary>
    internal static ushort HZoomerFrameAt(ushort operandAddress)
    {
        if (HZoomerInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(HZoomerBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"HZoomer visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Sbug's 32 visual operands in eight four-frame direction loops select
    /// 24 distinct OAM compositions; the compiled bank-$A3 instruction program
    /// owns cadence and goto commands, not the editable sprite resource.
    /// </summary>
    internal static ushort SbugFrameAt(ushort operandAddress)
    {
        for (int index = 0; index <
             SbugInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            if (SbugInstructionProgramDefinitions.PresentationWordAddress(index) !=
                operandAddress)
                continue;
            if (CompiledEnemyVisualSelectors.TryGet(SbugBank, operandAddress,
                    out ushort frame))
                return frame;
            break;
        }
        throw new InvalidDataException(
            $"Sbug visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The twenty <c>$A3:E262-E2C6</c> shared-crawler visual operands select
    /// the same bank-$A3 compositions as HZoomer's four surface loops.
    /// </summary>
    internal static ushort SharedCrawlerFrameAt(ushort operandAddress)
    {
        if (SharedCrawlerInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(HZoomerBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Shared-crawler visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>Compiled Zoa visual operands; shot speed changes remain gameplay callbacks.</summary>
    internal static ushort ZoaFrameAt(ushort operandAddress) => operandAddress switch
    {
        0xb3c5 => 0xb55f,
        0xb3cb => 0xb566,
        0xb3d1 => 0xb56d,
        0xb3d9 => 0xb57b,
        0xb3dd => 0xb574,
        0xb3e1 => 0xb582,
        0xb3eb => 0xb589,
        0xb3f1 => 0xb590,
        0xb3f7 => 0xb597,
        0xb3ff => 0xb5a5,
        0xb403 => 0xb59e,
        0xb407 => 0xb5ac,
        _ => throw new InvalidDataException(
            $"Zoa visual operand $A3:{operandAddress:X4} is not compiled."),
    };

    /// <summary>Compiled Skree and Metaree visual operands; attack phases remain code.</summary>
    internal static ushort SkreeMetareeFrameAt(bool metaree, ushort operandAddress) =>
        (ushort)(metaree ? operandAddress switch
        {
            0x8912 or 0x8926 or 0x8940 or 0x894a => 0x8b65,
            0x8916 or 0x8934 => 0x8bb9,
            0x891a or 0x8938 => 0x8bde,
            0x891e or 0x893c => 0x8bea,
            0x892a => 0x8b94,
            _ => throw new InvalidDataException(
                $"Metaree visual operand $A3:{operandAddress:X4} is not compiled."),
        } : operandAddress switch
        {
            0xc660 or 0xc674 or 0xc68e or 0xc698 => 0xc842,
            0xc664 or 0xc682 => 0xc878,
            0xc668 or 0xc686 => 0xc884,
            0xc66c or 0xc68a => 0xc89a,
            0xc678 => 0xc862,
            _ => throw new InvalidDataException(
                $"Skree visual operand $A3:{operandAddress:X4} is not compiled."),
        });

    /// <summary>
    /// Waver's two steady and eight spinning frames from the interleaved visual
    /// operands at $A3:86A9-$86D5. Spin completion and delays remain mechanics.
    /// </summary>
    internal static ushort WaverFrameAt(ushort operandAddress) => operandAddress switch
    {
        0x86a9 => 0x884a,
        0x86af => 0x88b3,
        0x86b5 => 0x885b,
        0x86b9 => 0x8871,
        0x86bd => 0x881e,
        0x86c1 => 0x8834,
        0x86c9 => 0x88c4,
        0x86cd => 0x88da,
        0x86d1 => 0x8887,
        0x86d5 => 0x889d,
        _ => throw new InvalidDataException(
            $"Waver visual operand $A3:{operandAddress:X4} is not compiled."),
    };

    /// <summary>
    /// The twenty-two Skultera frame operands are fixed visual identities; control
    /// durations, turn callbacks, and layer changes remain in the instruction catalog.
    /// </summary>
    internal static ushort SkulteraFrameAt(ushort operandAddress)
    {
        if (SkulteraInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(SkulteraBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Skultera visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The ten fixed pointer operands interleaved with Boyon's compiled idle and bounce
    /// instructions. Repeated frames retain the cartridge's exact visual sequence.
    /// </summary>
    internal static ushort BoyonFrameAt(ushort operandAddress)
    {
        if (BoyonInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            bool bouncing = operandAddress >= BoyonInstructionProgramDefinitions.Bouncing;
            int firstOperand = (bouncing ? BoyonInstructionProgramDefinitions.Bouncing : BoyonInstructionProgramDefinitions.Idle) + 6;
            int frame = (operandAddress - firstOperand) / 4;
            int peak = bouncing ? 3 : 2;
            int pose = peak - Math.Abs(peak - frame);
            // One five-byte OAM entry and a two-byte count per frame; both sequences
            // advance to their peak pose then reverse without repeating the endpoints.
            return (ushort)((bouncing ? 0x88ef : 0x88da) + 7 * pose);
        }
        throw new InvalidDataException(
            $"Boyon visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Four native Cacatac programs: eight idle frames and four attack selectors
    /// per orientation. Attack poses zero and three reuse idle zero and attack one.
    /// The lookup cannot alter attack timings or spike-spawn callbacks.
    /// </summary>
    internal static ushort CacatacFrameAt(ushort operandAddress)
    {
        if (operandAddress >= 0x9e8e && operandAddress <= 0x9eaa &&
            (operandAddress - 0x9e8e) % 4 == 0)
            return unchecked((ushort)(0xa0bb + (operandAddress - 0x9e8e) * 8));
        if (operandAddress >= 0x9ede && operandAddress <= 0x9efa &&
            (operandAddress - 0x9ede) % 4 == 0)
            return unchecked((ushort)(0xa223 + (operandAddress - 0x9ede) * 8));
        return operandAddress switch
        {
            0x9eb2 => 0xa0bb,
            0x9eb6 or 0x9ebe => 0xa1bb,
            0x9eba => 0xa1ef,
            0x9f02 => 0xa223,
            0x9f06 or 0x9f0e => 0xa323,
            0x9f0a => 0xa357,
            _ => throw new InvalidDataException(
                $"Cacatac visual operand $A2:{operandAddress:X4} is not compiled."),
        };
    }

    /// <summary>
    /// Fireflea's 52 interleaved frame selectors at $A3:8C31..8CFD. The 2/1-frame
    /// cadence and loop instruction remain compiled mechanics, not visual assets.
    /// </summary>
    internal static ushort FirefleaFrameAt(ushort operandAddress)
    {
        if (FirefleaInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(FirefleaBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Fireflea visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Magdollite's 53 head, throwing-hand, and pillar selectors at $A8:AC9E..AE0E.
    /// Their attack callbacks, timing, and physical movement remain engine-owned.
    /// </summary>
    internal static ushort MagdolliteFrameAt(ushort operandAddress)
    {
        if (MagdolliteInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(MagdolliteBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Magdollite visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Boulder rolls through eight four-part frames. Its right-moving list plays the
    /// same eight frames in reverse after the first frame; direction and duration
    /// remain gameplay-owned.
    /// </summary>
    internal static ushort BoulderFrameAt(ushort operandAddress)
    {
        if (operandAddress >= 0x86a9 && operandAddress <= 0x86c5 &&
            (operandAddress - 0x86a9) % 4 == 0)
            return unchecked((ushort)(0x8a59 + (operandAddress - 0x86a9) / 4 * 0x16));
        if (operandAddress >= 0x86cd && operandAddress <= 0x86e9 &&
            (operandAddress - 0x86cd) % 4 == 0)
        {
            int index = (operandAddress - 0x86cd) / 4;
            return unchecked((ushort)(0x8a59 + (index == 0 ? 0 : 8 - index) * 0x16));
        }
        throw new InvalidDataException(
            $"Boulder visual operand $A6:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Atomic has two authored six-frame spirals; the down-left/down-right programs
    /// replay the matching up spiral in reverse. Directional movement remains compiled.
    /// </summary>
    internal static ushort AtomicFrameAt(ushort operandAddress)
    {
        int offset = operandAddress - 0xe312;
        if (AtomicInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            int direction = offset / 28;
            int frame = offset % 28 / 4;
            if (direction >= 2) frame = 5 - frame;
            // Each spiral has six records: a two-byte count and four five-byte
            // OAM entries, except frame four has three entries. Its shorter record
            // moves frame five back five bytes and makes the spiral span127 bytes.
            return (ushort)(0xe489 + (direction & 1) * 127 + frame * 22 - (frame == 5 ? 5 : 0));
        }
        throw new InvalidDataException(
            $"Atomic visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
