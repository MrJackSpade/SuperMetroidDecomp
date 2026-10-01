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
    internal const int Version = 65;
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

    private static readonly EnemySpritemapDefinition[] NamedFrameDefinitions =
    [
        new(BoyonBank, 0x88da, "boyon_idle_0"),
        new(BoyonBank, 0x88e1, "boyon_idle_1"),
        new(BoyonBank, 0x88e8, "boyon_idle_2"),
        new(BoyonBank, 0x88ef, "boyon_bounce_0"),
        new(BoyonBank, 0x88f6, "boyon_bounce_1"),
        new(BoyonBank, 0x88fd, "boyon_bounce_2"),
        new(BoyonBank, 0x8904, "boyon_bounce_3"),
        new(BoyonBank, 0xa0bb, "cacatac_upright_idle_0"),
        new(BoyonBank, 0xa0db, "cacatac_upright_idle_1"),
        new(BoyonBank, 0xa0fb, "cacatac_upright_idle_2"),
        new(BoyonBank, 0xa11b, "cacatac_upright_idle_3"),
        new(BoyonBank, 0xa13b, "cacatac_upright_idle_4"),
        new(BoyonBank, 0xa15b, "cacatac_upright_idle_5"),
        new(BoyonBank, 0xa17b, "cacatac_upright_idle_6"),
        new(BoyonBank, 0xa19b, "cacatac_upright_idle_7"),
        new(BoyonBank, 0xa1bb, "cacatac_upright_attack_1"),
        new(BoyonBank, 0xa1ef, "cacatac_upright_attack_2"),
        new(BoyonBank, 0xa223, "cacatac_inverted_idle_0"),
        new(BoyonBank, 0xa243, "cacatac_inverted_idle_1"),
        new(BoyonBank, 0xa263, "cacatac_inverted_idle_2"),
        new(BoyonBank, 0xa283, "cacatac_inverted_idle_3"),
        new(BoyonBank, 0xa2a3, "cacatac_inverted_idle_4"),
        new(BoyonBank, 0xa2c3, "cacatac_inverted_idle_5"),
        new(BoyonBank, 0xa2e3, "cacatac_inverted_idle_6"),
        new(BoyonBank, 0xa303, "cacatac_inverted_idle_7"),
        new(BoyonBank, 0xa323, "cacatac_inverted_attack_1"),
        new(BoyonBank, 0xa357, "cacatac_inverted_attack_2"),
        new(BoulderBank, 0x8a59, "boulder_roll_0"),
        new(BoulderBank, 0x8a6f, "boulder_roll_1"),
        new(BoulderBank, 0x8a85, "boulder_roll_2"),
        new(BoulderBank, 0x8a9b, "boulder_roll_3"),
        new(BoulderBank, 0x8ab1, "boulder_roll_4"),
        new(BoulderBank, 0x8ac7, "boulder_roll_5"),
        new(BoulderBank, 0x8add, "boulder_roll_6"),
        new(BoulderBank, 0x8af3, "boulder_roll_7"),
        new(AtomicBank, 0xe489, "atomic_up_right_0"),
        new(AtomicBank, 0xe49f, "atomic_up_right_1"),
        new(AtomicBank, 0xe4b5, "atomic_up_right_2"),
        new(AtomicBank, 0xe4cb, "atomic_up_right_3"),
        new(AtomicBank, 0xe4e1, "atomic_up_right_4"),
        new(AtomicBank, 0xe4f2, "atomic_up_right_5"),
        new(AtomicBank, 0xe508, "atomic_up_left_0"),
        new(AtomicBank, 0xe51e, "atomic_up_left_1"),
        new(AtomicBank, 0xe534, "atomic_up_left_2"),
        new(AtomicBank, 0xe54a, "atomic_up_left_3"),
        new(AtomicBank, 0xe560, "atomic_up_left_4"),
        new(AtomicBank, 0xe571, "atomic_up_left_5"),
        new(SkulteraBank, 0x928a, "skultera_swim_left_0"),
        new(SkulteraBank, 0x92a5, "skultera_swim_left_1"),
        new(SkulteraBank, 0x92c0, "skultera_swim_left_2"),
        new(SkulteraBank, 0x92db, "skultera_turn_right_0"),
        new(SkulteraBank, 0x92f6, "skultera_turn_right_1"),
        new(SkulteraBank, 0x9311, "skultera_turn_right_2"),
        new(SkulteraBank, 0x9327, "skultera_turn_right_3"),
        new(SkulteraBank, 0x933d, "skultera_turn_right_4"),
        new(SkulteraBank, 0x934e, "skultera_turn_right_5"),
        new(SkulteraBank, 0x9364, "skultera_turn_right_6"),
        new(SkulteraBank, 0x937f, "skultera_turn_right_7"),
        new(SkulteraBank, 0x939a, "skultera_swim_right_0"),
        new(SkulteraBank, 0x93b5, "skultera_swim_right_1"),
        new(SkulteraBank, 0x93d0, "skultera_swim_right_2"),
        new(SkulteraBank, 0x93eb, "skultera_turn_left_0"),
        new(SkulteraBank, 0x9406, "skultera_turn_left_1"),
        new(SkulteraBank, 0x9421, "skultera_turn_left_2"),
        new(SkulteraBank, 0x9437, "skultera_turn_left_3"),
        new(SkulteraBank, 0x944d, "skultera_turn_left_4"),
        new(SkulteraBank, 0x945e, "skultera_turn_left_5"),
        new(SkulteraBank, 0x9474, "skultera_turn_left_6"),
        new(SkulteraBank, 0x948f, "skultera_turn_left_7"),
        new(WaverBank, 0x884a, "waver_steady_left"),
        new(WaverBank, 0x88b3, "waver_steady_right"),
        new(WaverBank, 0x885b, "waver_spin_left_0"),
        new(WaverBank, 0x8871, "waver_spin_left_1"),
        new(WaverBank, 0x881e, "waver_spin_left_2"),
        new(WaverBank, 0x8834, "waver_spin_left_3"),
        new(WaverBank, 0x88c4, "waver_spin_right_0"),
        new(WaverBank, 0x88da, "waver_spin_right_1"),
        new(WaverBank, 0x8887, "waver_spin_right_2"),
        new(WaverBank, 0x889d, "waver_spin_right_3"),
        new(SkreeMetareeBank, 0x8b65, "metaree_idle_0"),
        new(SkreeMetareeBank, 0x8bb9, "metaree_idle_1"),
        new(SkreeMetareeBank, 0x8bde, "metaree_idle_2"),
        new(SkreeMetareeBank, 0x8bea, "metaree_idle_3"),
        new(SkreeMetareeBank, 0x8b94, "metaree_prepare_1"),
        new(SkreeMetareeBank, 0xc842, "skree_idle_0"),
        new(SkreeMetareeBank, 0xc878, "skree_idle_1"),
        new(SkreeMetareeBank, 0xc884, "skree_idle_2"),
        new(SkreeMetareeBank, 0xc89a, "skree_idle_3"),
        new(SkreeMetareeBank, 0xc862, "skree_prepare_1"),
        new(ZoaBank, 0xb55f, "zoa_shoot_left_0"),
        new(ZoaBank, 0xb566, "zoa_shoot_left_1"),
        new(ZoaBank, 0xb56d, "zoa_shoot_left_2"),
        new(ZoaBank, 0xb57b, "zoa_rise_left_0"),
        new(ZoaBank, 0xb574, "zoa_rise_left_1"),
        new(ZoaBank, 0xb582, "zoa_rise_left_2"),
        new(ZoaBank, 0xb589, "zoa_shoot_right_0"),
        new(ZoaBank, 0xb590, "zoa_shoot_right_1"),
        new(ZoaBank, 0xb597, "zoa_shoot_right_2"),
        new(ZoaBank, 0xb5a5, "zoa_rise_right_0"),
        new(ZoaBank, 0xb59e, "zoa_rise_right_1"),
        new(ZoaBank, 0xb5ac, "zoa_rise_right_2"),
        new(PipeBugBank, 0x89b7, "pipe_brinstar_normal_left_0"),
        new(PipeBugBank, 0x89be, "pipe_brinstar_normal_left_1"),
        new(PipeBugBank, 0x89c5, "pipe_brinstar_normal_left_2"),
        new(PipeBugBank, 0x89cc, "pipe_brinstar_normal_left_3"),
        new(PipeBugBank, 0x89d3, "pipe_brinstar_normal_left_4"),
        new(PipeBugBank, 0x89da, "pipe_brinstar_normal_right_0"),
        new(PipeBugBank, 0x89e1, "pipe_brinstar_normal_right_1"),
        new(PipeBugBank, 0x89e8, "pipe_brinstar_normal_right_2"),
        new(PipeBugBank, 0x89ef, "pipe_brinstar_normal_right_3"),
        new(PipeBugBank, 0x89f6, "pipe_brinstar_normal_right_4"),
        new(PipeBugBank, 0x8a6d, "pipe_brinstar_strong_shoot_left_0"),
        new(PipeBugBank, 0x8a74, "pipe_brinstar_strong_shoot_left_1"),
        new(PipeBugBank, 0x8a7b, "pipe_brinstar_strong_shoot_left_2"),
        new(PipeBugBank, 0x8a82, "pipe_brinstar_strong_rise_left_0"),
        new(PipeBugBank, 0x8a89, "pipe_brinstar_strong_rise_left_1"),
        new(PipeBugBank, 0x8a90, "pipe_brinstar_strong_rise_left_2"),
        new(PipeBugBank, 0x8a97, "pipe_brinstar_strong_shoot_right_0"),
        new(PipeBugBank, 0x8a9e, "pipe_brinstar_strong_shoot_right_1"),
        new(PipeBugBank, 0x8aa5, "pipe_brinstar_strong_shoot_right_2"),
        new(PipeBugBank, 0x8aac, "pipe_brinstar_strong_rise_right_0"),
        new(PipeBugBank, 0x8ab3, "pipe_brinstar_strong_rise_right_1"),
        new(PipeBugBank, 0x8aba, "pipe_brinstar_strong_rise_right_2"),
        new(PipeBugBank, 0x8e96, "pipe_norfair_left_0"),
        new(PipeBugBank, 0x8e9d, "pipe_norfair_left_1"),
        new(PipeBugBank, 0x8ea4, "pipe_norfair_left_2"),
        new(PipeBugBank, 0x8eab, "pipe_norfair_left_3"),
        new(PipeBugBank, 0x8eb2, "pipe_norfair_left_4"),
        new(PipeBugBank, 0x8eb9, "pipe_norfair_right_0"),
        new(PipeBugBank, 0x8ec0, "pipe_norfair_right_1"),
        new(PipeBugBank, 0x8ec7, "pipe_norfair_right_2"),
        new(PipeBugBank, 0x8ece, "pipe_norfair_right_3"),
        new(PipeBugBank, 0x8ed5, "pipe_norfair_right_4"),
        new(PipeBugBank, 0x92ad, "pipe_yellow_fly_left_0"),
        new(PipeBugBank, 0x92b4, "pipe_yellow_fly_left_1"),
        new(PipeBugBank, 0x92bb, "pipe_yellow_fly_left_2"),
        new(PipeBugBank, 0x92c2, "pipe_yellow_arc_left_0"),
        new(PipeBugBank, 0x92c9, "pipe_yellow_arc_left_1"),
        new(PipeBugBank, 0x92d0, "pipe_yellow_arc_left_2"),
        new(PipeBugBank, 0x92d7, "pipe_yellow_fly_right_0"),
        new(PipeBugBank, 0x92de, "pipe_yellow_fly_right_1"),
        new(PipeBugBank, 0x92e5, "pipe_yellow_fly_right_2"),
        new(PipeBugBank, 0x92ec, "pipe_yellow_arc_right_0"),
        new(PipeBugBank, 0x92f3, "pipe_yellow_arc_right_1"),
        new(PipeBugBank, 0x92fa, "pipe_yellow_arc_right_2"),
        new(FakeKraidBank, 0x9c64, "fake_kraid_walk_left_0"),
        new(FakeKraidBank, 0x9cb6, "fake_kraid_walk_left_1"),
        new(FakeKraidBank, 0x9d08, "fake_kraid_walk_left_2"),
        new(FakeKraidBank, 0x9d5a, "fake_kraid_walk_left_3"),
        new(FakeKraidBank, 0x9dac, "fake_kraid_spit_left_0"),
        new(FakeKraidBank, 0x9dfe, "fake_kraid_spit_left_1"),
        new(FakeKraidBank, 0x9e50, "fake_kraid_spit_left_2"),
        new(FakeKraidBank, 0x9ea2, "fake_kraid_walk_right_0"),
        new(FakeKraidBank, 0x9ef4, "fake_kraid_walk_right_1"),
        new(FakeKraidBank, 0x9f46, "fake_kraid_walk_right_2"),
        new(FakeKraidBank, 0x9f98, "fake_kraid_walk_right_3"),
        new(FakeKraidBank, 0x9fea, "fake_kraid_spit_right_0"),
        new(FakeKraidBank, 0xa03c, "fake_kraid_spit_right_1"),
        new(FakeKraidBank, 0xa08e, "fake_kraid_spit_right_2"),
        new(KraidNailBank, 0xa617, "kraid_nail_0"),
        new(KraidNailBank, 0xa623, "kraid_nail_1"),
        new(KraidNailBank, 0xa639, "kraid_nail_2"),
        new(KraidNailBank, 0xa645, "kraid_nail_3"),
        new(KraidNailBank, 0xa65b, "kraid_nail_4"),
        new(KraidNailBank, 0xa667, "kraid_nail_5"),
        new(KraidNailBank, 0xa67d, "kraid_nail_6"),
        new(KraidNailBank, 0xa689, "kraid_nail_7"),
        new(OwtchStokeBank, 0xa589, "owtch_left_0"),
        new(OwtchStokeBank, 0xa590, "owtch_left_1"),
        new(OwtchStokeBank, 0xa597, "owtch_left_2"),
        new(OwtchStokeBank, 0x8aca, "stoke_walk_left_0"),
        new(OwtchStokeBank, 0x8ad6, "stoke_walk_left_1"),
        new(OwtchStokeBank, 0x8ae7, "stoke_walk_left_2"),
        new(OwtchStokeBank, 0x8af3, "stoke_walk_left_3"),
        new(OwtchStokeBank, 0x8aff, "stoke_attack_left"),
        new(OwtchStokeBank, 0x8b15, "stoke_walk_right_0"),
        new(OwtchStokeBank, 0x8b21, "stoke_walk_right_1"),
        new(OwtchStokeBank, 0x8b32, "stoke_walk_right_2"),
        new(OwtchStokeBank, 0x8b3e, "stoke_walk_right_3"),
        new(OwtchStokeBank, 0x8b4a, "stoke_attack_right"),
        new(RipperBank, 0xe3c5, "ripper_shared_left_0"),
        new(RipperBank, 0xe3db, "ripper_shared_left_1"),
        new(RipperBank, 0xe3ec, "ripper_shared_left_2"),
        new(RipperBank, 0xe402, "ripper_shared_right_0"),
        new(RipperBank, 0xe418, "ripper_shared_right_1"),
        new(RipperBank, 0xe429, "ripper_shared_right_2"),
        new(RipperBank, 0xe43f, "ripper_shared_frozen_left"),
        new(RipperBank, 0xe44b, "ripper_shared_frozen_right"),
        new(RipperBank, 0xe527, "ripper_left_0"),
        new(RipperBank, 0xe533, "ripper_left_1"),
        new(RipperBank, 0xe53f, "ripper_left_2"),
        new(RipperBank, 0xe54b, "ripper_right_0"),
        new(RipperBank, 0xe557, "ripper_right_1"),
        new(RipperBank, 0xe563, "ripper_right_2"),
        new(FirefleaBank, 0x8ea5, "fireflea_cycle_0"),
        new(FirefleaBank, 0x8eb6, "fireflea_cycle_1"),
        new(FirefleaBank, 0x8ec7, "fireflea_cycle_2"),
        new(FirefleaBank, 0x8ed8, "fireflea_cycle_3"),
        new(FirefleaBank, 0x8ee9, "fireflea_cycle_4"),
        new(FirefleaBank, 0x8efa, "fireflea_cycle_5"),
        new(FirefleaBank, 0x8f0b, "fireflea_cycle_6"),
        new(FirefleaBank, 0x8f1c, "fireflea_cycle_7"),
        new(FirefleaBank, 0x8f2d, "fireflea_cycle_8"),
        new(FirefleaBank, 0x8f3e, "fireflea_cycle_9"),
        new(FirefleaBank, 0x8f4f, "fireflea_cycle_10"),
        new(FirefleaBank, 0x8f60, "fireflea_cycle_11"),
        new(FirefleaBank, 0x8f71, "fireflea_cycle_12"),
        new(FirefleaBank, 0x8f82, "fireflea_cycle_13"),
        new(FirefleaBank, 0x8f93, "fireflea_cycle_14"),
        new(FirefleaBank, 0x8fa4, "fireflea_cycle_15"),
        new(FirefleaBank, 0x8fb5, "fireflea_cycle_16"),
        new(FirefleaBank, 0x8fc6, "fireflea_cycle_17"),
        new(FirefleaBank, 0x8fd7, "fireflea_cycle_18"),
        new(FirefleaBank, 0x8fe8, "fireflea_cycle_19"),
        new(FirefleaBank, 0x8ff9, "fireflea_cycle_20"),
        new(MagdolliteBank, 0xb448, "magdollite_left_idle_0"),
        new(MagdolliteBank, 0xb459, "magdollite_left_idle_1"),
        new(MagdolliteBank, 0xb46a, "magdollite_left_idle_2"),
        new(MagdolliteBank, 0xb47b, "magdollite_left_throw_0"),
        new(MagdolliteBank, 0xb48c, "magdollite_left_throw_1"),
        new(MagdolliteBank, 0xb49d, "magdollite_left_throw_2"),
        new(MagdolliteBank, 0xb4a9, "magdollite_left_throw_3"),
        new(MagdolliteBank, 0xb4b5, "magdollite_pillar_cap"),
        new(MagdolliteBank, 0xb4c1, "magdollite_left_submerge_0"),
        new(MagdolliteBank, 0xb4cf, "magdollite_left_submerge_1"),
        new(MagdolliteBank, 0xb4e0, "magdollite_left_submerge_2"),
        new(MagdolliteBank, 0xb4f1, "magdollite_right_idle_0"),
        new(MagdolliteBank, 0xb502, "magdollite_right_idle_1"),
        new(MagdolliteBank, 0xb513, "magdollite_right_idle_2"),
        new(MagdolliteBank, 0xb524, "magdollite_right_throw_0"),
        new(MagdolliteBank, 0xb535, "magdollite_right_throw_1"),
        new(MagdolliteBank, 0xb546, "magdollite_right_throw_2"),
        new(MagdolliteBank, 0xb552, "magdollite_right_throw_3"),
        new(MagdolliteBank, 0xb56a, "magdollite_right_submerge_0"),
        new(MagdolliteBank, 0xb578, "magdollite_right_submerge_1"),
        new(MagdolliteBank, 0xb589, "magdollite_right_submerge_2"),
        new(MagdolliteBank, 0xb59a, "magdollite_pillar_phase_0"),
        new(MagdolliteBank, 0xb5a1, "magdollite_pillar_phase_1"),
        new(MagdolliteBank, 0xb5ad, "magdollite_pillar_phase_2"),
        new(MagdolliteBank, 0xb5be, "magdollite_pillar_phase_3"),
        new(MagdolliteBank, 0xb5d4, "magdollite_pillar_phase_4"),
        new(MagdolliteBank, 0xb5ef, "magdollite_pillar_phase_5"),
        new(MagdolliteBank, 0xb60f, "magdollite_pillar_phase_6"),
        new(MagdolliteBank, 0xb634, "magdollite_pillar_phase_7"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf921, "ceres_door_rotating_overlay"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf95f, "ceres_door_left_hold"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf989, "ceres_door_left_transition_0"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf9b3, "ceres_door_left_transition_1"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf9d3, "ceres_door_left_transition_2"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xf9f3, "ceres_door_left_transition_3"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfa13, "ceres_door_right_hold"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfa3d, "ceres_door_right_transition_0"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfa67, "ceres_door_right_transition_1"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfa87, "ceres_door_right_transition_2"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfaa7, "ceres_door_right_transition_3"),
        new(CeresDoorInstructionProgramDefinitions.Bank,
            CeresDoorInstructionProgramDefinitions.InitialSpritemap, "ceres_door_initial"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xface, "ceres_door_mode7_left_wall"),
        new(CeresDoorInstructionProgramDefinitions.Bank, 0xfb2f, "ceres_door_mode7_right_wall"),
        new(CeresDoorInstructionProgramDefinitions.Bank,
            CeresDoorInstructionProgramDefinitions.RidleyPrivateOverlaySpritemap,
            "ceres_door_ridley_private_overlay"),
        new(CeresBabyInstructionProgramDefinitions.Bank,
            CeresBabyInstructionProgramDefinitions.HorizontalFrame,
            "ceres_baby_horizontal"),
        new(CeresBabyInstructionProgramDefinitions.Bank,
            CeresBabyInstructionProgramDefinitions.RoundFrame,
            "ceres_baby_round"),
        new(CeresBabyInstructionProgramDefinitions.Bank,
            CeresBabyInstructionProgramDefinitions.VerticalFrame,
            "ceres_baby_vertical"),
        new(RioBank, 0xbd6c, "rio_bd6c"),
        new(RioBank, 0xbd82, "rio_bd82"),
        new(RioBank, 0xbd98, "rio_bd98"),
        new(RioBank, 0xbdae, "rio_bdae"),
        new(RioBank, 0xbdc4, "rio_bdc4"),
        new(RioBank, 0xbdda, "rio_bdda"),
        new(RioBank, 0xbdf0, "rio_bdf0"),
        new(RioBank, 0xbe06, "rio_be06"),
        new(LowerNorfairRioBank, 0xc8bd, "lower_norfair_rio_c8bd"),
        new(LowerNorfairRioBank, 0xc8d3, "lower_norfair_rio_c8d3"),
        new(LowerNorfairRioBank, 0xc8e9, "lower_norfair_rio_c8e9"),
        new(LowerNorfairRioBank, 0xc8ff, "lower_norfair_rio_c8ff"),
        new(LowerNorfairRioBank, 0xc915, "lower_norfair_rio_c915"),
        new(LowerNorfairRioBank, 0xc92b, "lower_norfair_rio_c92b"),
        new(LowerNorfairRioBank, 0xc941, "lower_norfair_rio_c941"),
        new(LowerNorfairRioBank, 0xc957, "lower_norfair_rio_c957"),
        new(LowerNorfairRioBank, 0xc96d, "lower_norfair_rio_c96d"),
        new(LowerNorfairRioBank, 0xc983, "lower_norfair_rio_c983"),
        new(LowerNorfairRioBank, 0xc999, "lower_norfair_rio_c999"),
        new(LowerNorfairRioBank, 0xc9af, "lower_norfair_rio_c9af"),
        new(LowerNorfairRioBank, 0xc9c5, "lower_norfair_rio_c9c5"),
        new(LowerNorfairRioBank, 0xc9db, "lower_norfair_rio_c9db"),
        new(LowerNorfairRioBank, 0xc9f1, "lower_norfair_rio_c9f1"),
        new(LowerNorfairRioBank, 0xca07, "lower_norfair_rio_ca07"),
        new(LowerNorfairRioBank, 0xca13, "lower_norfair_rio_ca13"),
        new(LowerNorfairRioBank, 0xca1f, "lower_norfair_rio_ca1f"),
        new(NorfairRioBank, 0xc427, "norfair_rio_c427"),
        new(NorfairRioBank, 0xc442, "norfair_rio_c442"),
        new(NorfairRioBank, 0xc45d, "norfair_rio_c45d"),
        new(NorfairRioBank, 0xc493, "norfair_rio_c493"),
        new(NorfairRioBank, 0xc49f, "norfair_rio_c49f"),
        new(NorfairRioBank, 0xc4ab, "norfair_rio_c4ab"),
        new(NorfairRioBank, 0xc4b7, "norfair_rio_c4b7"),
        new(NorfairRioBank, 0xc4d2, "norfair_rio_c4d2"),
        new(NorfairRioBank, 0xc4ed, "norfair_rio_c4ed"),
        new(NorfairRioBank, 0xc508, "norfair_rio_c508"),
        new(NorfairRioBank, 0xc523, "norfair_rio_c523"),
        new(NorfairRioBank, 0xc534, "norfair_rio_c534"),
        new(NorfairRioBank, 0xc54f, "norfair_rio_c54f"),
        new(NorfairRioBank, 0xc56a, "norfair_rio_c56a"),
        new(NorfairRioBank, 0xc585, "norfair_rio_c585"),
        new(NorfairRioBank, 0xc5a0, "norfair_rio_c5a0"),
        new(NorfairRioBank, 0xc5bb, "norfair_rio_c5bb"),
        new(NorfairRioBank, 0xc5d6, "norfair_rio_c5d6"),
        new(NorfairRioBank, 0xc5e2, "norfair_rio_c5e2"),
        new(NorfairRioBank, 0xc5ee, "norfair_rio_c5ee"),
        new(PuyoBank, 0x9df6, "puyo_ground_0"),
        new(PuyoBank, 0x9e02, "puyo_ground_1"),
        new(PuyoBank, 0x9e0e, "puyo_ground_2"),
        new(PuyoBank, 0x9e1a, "puyo_air_0"),
        new(PuyoBank, 0x9e26, "puyo_air_1"),
        new(PuyoBank, 0x9e37, "puyo_air_2"),
        new(PuyoBank, 0x9e4d, "puyo_air_3"),
        new(PuyoBank, 0x9e5e, "puyo_air_4"),
        new(BullBank, 0xdb76, "bull_idle_0"),
        new(BullBank, 0xdb8c, "bull_idle_1"),
        new(BullBank, 0xdba2, "bull_idle_2"),
        new(AlcoonBank, 0xdfa2, "alcoon_left_walk_0"),
        new(AlcoonBank, 0xdfc2, "alcoon_left_walk_1"),
        new(AlcoonBank, 0xdfe2, "alcoon_left_walk_2"),
        new(AlcoonBank, 0xe007, "alcoon_left_walk_3"),
        new(AlcoonBank, 0xe027, "alcoon_left_fire_0"),
        new(AlcoonBank, 0xe047, "alcoon_left_fire_1"),
        new(AlcoonBank, 0xe06c, "alcoon_left_fire_2"),
        new(AlcoonBank, 0xe09b, "alcoon_left_fire_3"),
        new(AlcoonBank, 0xe0bb, "alcoon_left_air_up"),
        new(AlcoonBank, 0xe0db, "alcoon_right_walk_0"),
        new(AlcoonBank, 0xe0fb, "alcoon_right_walk_1"),
        new(AlcoonBank, 0xe11b, "alcoon_right_walk_2"),
        new(AlcoonBank, 0xe140, "alcoon_right_walk_3"),
        new(AlcoonBank, 0xe160, "alcoon_right_fire_0"),
        new(AlcoonBank, 0xe180, "alcoon_right_fire_1"),
        new(AlcoonBank, 0xe1a5, "alcoon_right_fire_2"),
        new(AlcoonBank, 0xe1d4, "alcoon_right_fire_3"),
        new(AlcoonBank, 0xe1f4, "alcoon_right_air_up"),
        new(BeetomBank, 0xbed3, "beetom_left_crawl_0"),
        new(BeetomBank, 0xbeee, "beetom_left_crawl_1"),
        new(BeetomBank, 0xbf09, "beetom_left_crawl_2"),
        new(BeetomBank, 0xbf24, "beetom_left_hop_0"),
        new(BeetomBank, 0xbf3f, "beetom_left_hop_1"),
        new(BeetomBank, 0xbf5a, "beetom_left_drain_0"),
        new(BeetomBank, 0xbf75, "beetom_left_drain_1"),
        new(BeetomBank, 0xbf90, "beetom_left_drain_2"),
        new(BeetomBank, 0xbfab, "beetom_left_drain_3"),
        new(BeetomBank, 0xbfcb, "beetom_left_drain_4"),
        new(BeetomBank, 0xbfeb, "beetom_left_drain_5"),
        new(BeetomBank, 0xc00b, "beetom_right_crawl_0"),
        new(BeetomBank, 0xc026, "beetom_right_crawl_1"),
        new(BeetomBank, 0xc041, "beetom_right_crawl_2"),
        new(BeetomBank, 0xc05c, "beetom_right_hop_0"),
        new(BeetomBank, 0xc077, "beetom_right_hop_1"),
        new(BeetomBank, 0xc092, "beetom_right_drain_0"),
        new(BeetomBank, 0xc0ad, "beetom_right_drain_1"),
        new(BeetomBank, 0xc0c8, "beetom_right_drain_2"),
        new(BeetomBank, 0xc0e3, "beetom_right_drain_3"),
        new(BeetomBank, 0xc103, "beetom_right_drain_4"),
        new(BeetomBank, 0xc123, "beetom_right_drain_5"),
        new(HopperBank, 0xaf19, "sidehopper_jump_floor"),
        new(HopperBank, 0xaee3, "sidehopper_land_floor_0"),
        new(HopperBank, 0xaefe, "sidehopper_land_floor_1"),
        new(HopperBank, 0xaf6a, "sidehopper_jump_ceiling"),
        new(HopperBank, 0xaf34, "sidehopper_land_ceiling_0"),
        new(HopperBank, 0xaf4f, "sidehopper_land_ceiling_1"),
        new(HopperBank, 0xb019, "dessgeega_jump_floor"),
        new(HopperBank, 0xafe3, "dessgeega_land_floor_0"),
        new(HopperBank, 0xaffe, "dessgeega_land_floor_1"),
        new(HopperBank, 0xb06a, "dessgeega_jump_ceiling"),
        new(HopperBank, 0xb034, "dessgeega_land_ceiling_0"),
        new(HopperBank, 0xb04f, "dessgeega_land_ceiling_1"),
        new(HopperBank, 0xb15b, "large_sidehopper_jump_floor"),
        new(HopperBank, 0xb111, "large_sidehopper_land_floor_0"),
        new(HopperBank, 0xb136, "large_sidehopper_land_floor_1"),
        new(HopperBank, 0xb1de, "large_sidehopper_jump_ceiling"),
        new(HopperBank, 0xb194, "large_sidehopper_land_ceiling_0"),
        new(HopperBank, 0xb1b9, "large_sidehopper_land_ceiling_1"),
        new(HopperBank, 0xb2d1, "large_dessgeega_jump_floor"),
        new(HopperBank, 0xb273, "large_dessgeega_land_floor_0"),
        new(HopperBank, 0xb2a2, "large_dessgeega_land_floor_1"),
        new(HopperBank, 0xb368, "large_dessgeega_jump_ceiling"),
        new(HopperBank, 0xb30a, "large_dessgeega_land_ceiling_0"),
        new(HopperBank, 0xb339, "large_dessgeega_land_ceiling_1"),
        new(ChootBank, 0xe146, "choot_idle"),
        new(ChootBank, 0xe15c, "choot_jump"),
        new(ChootBank, 0xe168, "choot_jump_apex"),
        new(ChootBank, 0xe16f, "choot_fall_end"),
        new(HZoomerBank, 0xe50e, "hzoomer_upside_right_0"),
        new(HZoomerBank, 0xe524, "hzoomer_upside_right_1"),
        new(HZoomerBank, 0xe53a, "hzoomer_upside_right_2"),
        new(HZoomerBank, 0xe550, "hzoomer_upside_right_3"),
        new(HZoomerBank, 0xe566, "hzoomer_upside_right_4"),
        new(HZoomerBank, 0xe3c4, "hzoomer_upside_left_0"),
        new(HZoomerBank, 0xe3da, "hzoomer_upside_left_1"),
        new(HZoomerBank, 0xe3f0, "hzoomer_upside_left_2"),
        new(HZoomerBank, 0xe406, "hzoomer_upside_left_3"),
        new(HZoomerBank, 0xe41c, "hzoomer_upside_left_4"),
        new(HZoomerBank, 0xe432, "hzoomer_upside_down_0"),
        new(HZoomerBank, 0xe448, "hzoomer_upside_down_1"),
        new(HZoomerBank, 0xe45e, "hzoomer_upside_down_2"),
        new(HZoomerBank, 0xe474, "hzoomer_upside_down_3"),
        new(HZoomerBank, 0xe48a, "hzoomer_upside_down_4"),
        new(HZoomerBank, 0xe2e8, "hzoomer_upside_up_0"),
        new(HZoomerBank, 0xe2fe, "hzoomer_upside_up_1"),
        new(HZoomerBank, 0xe314, "hzoomer_upside_up_2"),
        new(HZoomerBank, 0xe32a, "hzoomer_upside_up_3"),
        new(HZoomerBank, 0xe340, "hzoomer_upside_up_4"),
        new(SbugBank, 0xa67d, "sbug_right_0"),
        new(SbugBank, 0xa684, "sbug_right_1"),
        new(SbugBank, 0xa68b, "sbug_right_2"),
        new(SbugBank, 0xa692, "sbug_up_right_0"),
        new(SbugBank, 0xa699, "sbug_up_right_1"),
        new(SbugBank, 0xa6a0, "sbug_up_right_2"),
        new(SbugBank, 0xa6a7, "sbug_up_0"),
        new(SbugBank, 0xa6ae, "sbug_up_1"),
        new(SbugBank, 0xa6b5, "sbug_up_2"),
        new(SbugBank, 0xa6bc, "sbug_up_left_0"),
        new(SbugBank, 0xa6c3, "sbug_up_left_1"),
        new(SbugBank, 0xa6ca, "sbug_up_left_2"),
        new(SbugBank, 0xa6d1, "sbug_left_0"),
        new(SbugBank, 0xa6d8, "sbug_left_1"),
        new(SbugBank, 0xa6df, "sbug_left_2"),
        new(SbugBank, 0xa6e6, "sbug_down_left_0"),
        new(SbugBank, 0xa6ed, "sbug_down_left_1"),
        new(SbugBank, 0xa6f4, "sbug_down_left_2"),
        new(SbugBank, 0xa6fb, "sbug_down_0"),
        new(SbugBank, 0xa702, "sbug_down_1"),
        new(SbugBank, 0xa709, "sbug_down_2"),
        new(SbugBank, 0xa710, "sbug_down_right_0"),
        new(SbugBank, 0xa717, "sbug_down_right_1"),
        new(SbugBank, 0xa71e, "sbug_down_right_2"),
        new(FuneNamiheBank, 0x93f9, "fune_left_idle"),
        new(FuneNamiheBank, 0x9423, "fune_left_active_0"),
        new(FuneNamiheBank, 0x944d, "fune_left_active_1"),
        new(FuneNamiheBank, 0x9477, "fune_left_active_2"),
        new(FuneNamiheBank, 0x94a1, "fune_left_active_3"),
        new(FuneNamiheBank, 0x94cb, "fune_right_idle"),
        new(FuneNamiheBank, 0x94f5, "fune_right_active_0"),
        new(FuneNamiheBank, 0x951f, "fune_right_active_1"),
        new(FuneNamiheBank, 0x9549, "fune_right_active_2"),
        new(FuneNamiheBank, 0x9573, "fune_right_active_3"),
        new(FuneNamiheBank, 0x97b4, "namihe_left_idle"),
        new(FuneNamiheBank, 0x97de, "namihe_left_active_0"),
        new(FuneNamiheBank, 0x9808, "namihe_left_active_1"),
        new(FuneNamiheBank, 0x9832, "namihe_left_active_2"),
        new(FuneNamiheBank, 0x985c, "namihe_left_active_3"),
        new(FuneNamiheBank, 0x9886, "namihe_left_active_4"),
        new(FuneNamiheBank, 0x98b0, "namihe_right_idle"),
        new(FuneNamiheBank, 0x98da, "namihe_right_active_0"),
        new(FuneNamiheBank, 0x9904, "namihe_right_active_1"),
        new(FuneNamiheBank, 0x992e, "namihe_right_active_2"),
        new(FuneNamiheBank, 0x9958, "namihe_right_active_3"),
        new(FuneNamiheBank, 0x9982, "namihe_right_active_4"),
        new(KamerPlatformBank, 0xf468, "kamer_platform_0"),
        new(KamerPlatformBank, 0xf474, "kamer_platform_1"),
        new(KamerPlatformBank, 0xf480, "kamer_platform_2"),
        new(KamerPlatformBank, 0xf48c, "kamer_platform_3"),
        new(ElevatorBank, 0x962f, "elevator_platform_0"),
        new(ElevatorBank, 0x9645, "elevator_platform_1"),
        new(RoomSpriteObjectBank, 0xdb42, "draygon_intro_evir_0"),
        new(RoomSpriteObjectBank, 0xdb80, "draygon_intro_evir_1"),
        new(RoomSpriteObjectBank, 0xdbbe, "draygon_intro_evir_2"),
        new(RoomSpriteObjectBank, 0xdbfc, "draygon_intro_evir_3"),
        new(RoomSpriteObjectBank, 0xc920, "draygon_breath_bubble_0"),
        new(RoomSpriteObjectBank, 0xc927, "draygon_breath_bubble_1"),
        new(RoomSpriteObjectBank, 0xc938, "draygon_breath_bubble_2"),
        new(RoomSpriteObjectBank, 0xc949, "draygon_breath_bubble_3"),
        new(RoomSpriteObjectBank, 0xc95a, "draygon_breath_bubble_4"),
        new(RoomSpriteObjectBank, 0xc96b, "draygon_breath_bubble_5"),
        new(RoomSpriteObjectBank, 0xc97c, "draygon_breath_bubble_6"),
        new(RoomSpriteObjectBank, 0xc98d, "draygon_breath_bubble_7"),
        new(RoomSpriteObjectBank, 0xc999, "draygon_breath_bubble_8"),
    ];

    // All other bank-$B4 presentation targets are shared by the 62 compiled
    // sprite-object programs. Build their stable, address-named art identities
    // from the compiled selectors; do not duplicate their pointer list here.
    private static readonly EnemySpritemapDefinition[] FrameDefinitions =
    [
        .. NamedFrameDefinitions,
        .. RoomSpriteObjectVisualDefinitions.AdditionalFrames(NamedFrameDefinitions),
        .. YappingMawVisualDefinitions.Frames(),
        .. KiHunterVisualDefinitions.Frames(),
        .. MotherBrainVisualDefinitions.Frames(),
        .. DeadTorizoArtworkDefinitions.Frames(),
        .. RidleySupplementalVisualDefinitions.Frames(),
        .. SciserVisualDefinitions.Frames(),
        .. FlyVisualDefinitions.Frames(),
        .. KagoVisualDefinitions.Frames(),
        .. BlueBrinstarFaceBlockVisualDefinitions.Frames(),
        .. MorphBallEyeVisualDefinitions.Frames(),
        .. ShutterVisualDefinitions.Frames(),
        .. MetroidVisualDefinitions.Frames(),
        .. ShaktoolVisualDefinitions.Frames(),
        .. TripperKamerVisualDefinitions.Frames(),
        .. DragonVisualDefinitions.Frames(),
        .. MultiviolaVisualDefinitions.Frames(),
        .. NorfairLavaJumperVisualDefinitions.Frames(),
        .. ChozoStatueVisualDefinitions.Frames(),
        .. ViolaVisualDefinitions.Frames(),
        .. RinkaVisualDefinitions.Frames(),
        .. DeadTorizoArtworkDefinitions.StationaryFrames(),
        .. DeadTourianCorpseVisualDefinitions.Frames(),
        .. MochtroidVisualDefinitions.Frames(),
        .. EvirVisualDefinitions.Frames(),
        .. WorkRobotVisualDefinitions.Frames(),
        .. YardVisualDefinitions.Frames(),
        .. BotwoonVisualDefinitions.Frames(),
        .. GunshipVisualDefinitions.Frames(),
        .. MamaTurtleVisualDefinitions.Frames(),
        .. ZeroVisualDefinitions.Frames(),
        .. FriendlyAnimalVisualDefinitions.Frames(),
        .. HibashiVisualDefinitions.Frames(),
        .. ZebetiteVisualDefinitions.Frames(),
        .. WreckedShipGhostVisualDefinitions.Frames(),
        .. PowampVisualDefinitions.Frames(),
        .. SparkVisualDefinitions.Frames(),
        .. ShitroidVisualDefinitions.Frames(),
    ];

    private static readonly ushort[] AtomicUpRightFrames =
        [0xe489, 0xe49f, 0xe4b5, 0xe4cb, 0xe4e1, 0xe4f2];
    private static readonly ushort[] AtomicUpLeftFrames =
        [0xe508, 0xe51e, 0xe534, 0xe54a, 0xe560, 0xe571];

    internal static ReadOnlySpan<EnemySpritemapDefinition> Frames => FrameDefinitions;

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

    /// <summary>Reads only the 38 Fune/Namihe presentation operands from their eight native programs.</summary>
    internal static ushort FuneNamiheFrameAt(ushort operandAddress)
    {
        if (FuneNamiheInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(FuneNamiheBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Fune/Namihe visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>Four cartridge selectors in Kamer platform's $A2:EDE7 loop.</summary>
    internal static ushort KamerPlatformFrameAt(ushort operandAddress)
    {
        if (operandAddress is (0xede9 or 0xeded or 0xedf1 or 0xedf5) &&
            CompiledEnemyVisualSelectors.TryGet(KamerPlatformBank,
                operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Kamer platform visual operand $A2:{operandAddress:X4} is not compiled.");
    }

    /// <summary>The two visual operands in the native $A3:94D6 elevator loop.</summary>
    internal static ushort ElevatorFrameAt(ushort operandAddress)
    {
        if (operandAddress is (0x94d8 or 0x94dc) &&
            CompiledEnemyVisualSelectors.TryGet(ElevatorBank,
                operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Elevator visual operand $A3:{operandAddress:X4} is not compiled.");
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
        if (AlcoonInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(AlcoonBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Alcoon visual operand $A8:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Beetom's thirty-two crawl, hop, and drain operands select twenty-two
    /// editable compositions; physical attachment and drain cadence stay compiled.
    /// </summary>
    internal static ushort BeetomFrameAt(ushort operandAddress)
    {
        if (BeetomInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(BeetomBank, operandAddress,
                out ushort frame))
            return frame;
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
    internal static ushort BoyonFrameAt(ushort operandAddress) => operandAddress switch
    {
        0x86ad => 0x88da,
        0x86b1 => 0x88e1,
        0x86b5 => 0x88e8,
        0x86b9 => 0x88e1,
        0x86c5 => 0x88ef,
        0x86c9 => 0x88f6,
        0x86cd => 0x88fd,
        0x86d1 => 0x8904,
        0x86d5 => 0x88fd,
        0x86d9 => 0x88f6,
        _ => throw new InvalidDataException(
            $"Boyon visual operand $A2:{operandAddress:X4} is not compiled."),
    };

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
        if (operandAddress >= 0xe312 && operandAddress <= 0xe326 &&
            (operandAddress - 0xe312) % 4 == 0)
            return AtomicUpRightFrames[(operandAddress - 0xe312) / 4];
        if (operandAddress >= 0xe32e && operandAddress <= 0xe342 &&
            (operandAddress - 0xe32e) % 4 == 0)
            return AtomicUpLeftFrames[(operandAddress - 0xe32e) / 4];
        if (operandAddress >= 0xe34a && operandAddress <= 0xe35e &&
            (operandAddress - 0xe34a) % 4 == 0)
            return AtomicUpRightFrames[5 - (operandAddress - 0xe34a) / 4];
        if (operandAddress >= 0xe366 && operandAddress <= 0xe37a &&
            (operandAddress - 0xe366) % 4 == 0)
            return AtomicUpLeftFrames[5 - (operandAddress - 0xe366) / 4];
        throw new InvalidDataException(
            $"Atomic visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
