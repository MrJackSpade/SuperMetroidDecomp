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
    internal const int Version = 12;
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
    internal const byte BoulderBank = 0xa6;
    internal const byte AtomicBank = 0xa8;
    internal const int MaximumParts = 128;
    internal const int TileColumns = 16;
    internal const int TileRows = 32;

    private static readonly EnemySpritemapDefinition[] FrameDefinitions =
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
    ];

    private static readonly ushort[] AtomicUpRightFrames =
        [0xe489, 0xe49f, 0xe4b5, 0xe4cb, 0xe4e1, 0xe4f2];
    private static readonly ushort[] AtomicUpLeftFrames =
        [0xe508, 0xe51e, 0xe534, 0xe54a, 0xe560, 0xe571];

    internal static ReadOnlySpan<EnemySpritemapDefinition> Frames => FrameDefinitions;

    /// <summary>
    /// Selects only families whose fixed instruction visual operands are compiled.
    /// Unknown families retain the existing cartridge route until separately migrated.
    /// Known families reject an unlisted operand rather than reading adjacent data.
    /// </summary>
    internal static bool TryFrameAt(ushort enemyDefinition, ushort operandAddress,
        out ushort frame)
    {
        frame = enemyDefinition switch
        {
            RoomEnemySystem.BoyonDefinition => BoyonFrameAt(operandAddress),
            RoomEnemySystem.CacatacDefinition => CacatacFrameAt(operandAddress),
            RoomEnemySystem.FirefleaDefinition => FirefleaFrameAt(operandAddress),
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
        return enemyDefinition is RoomEnemySystem.BoyonDefinition or
            RoomEnemySystem.CacatacDefinition or RoomEnemySystem.FirefleaDefinition or
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
