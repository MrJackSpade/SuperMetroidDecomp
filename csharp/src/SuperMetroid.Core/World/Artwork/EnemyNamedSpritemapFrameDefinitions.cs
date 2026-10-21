using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>The enemy spritemap frames named outside a schema's numbered list.</summary>
internal static class EnemyNamedSpritemapFrameDefinitions
{

    /// <summary>Enumerates the historical named-frame prefix in enum order for schema compatibility.</summary>
    internal static IEnumerable<EnemySpritemapDefinition> NamedFrames()
    {
        foreach (EnemyNamedSpritemapFrameId frame in Enum.GetValues<EnemyNamedSpritemapFrameId>())
            yield return NamedFrame(frame);
    }

    /// <summary>Maps an ordered named identity to its native bank, pointer, and stable installed-document name.</summary>
    internal static EnemySpritemapDefinition NamedFrame(EnemyNamedSpritemapFrameId frame)
    {
        (byte bank, ushort pointer) = frame switch
        {
            EnemyNamedSpritemapFrameId.boyon_idle_0 or
                EnemyNamedSpritemapFrameId.boyon_idle_1 or
                EnemyNamedSpritemapFrameId.boyon_idle_2 =>
                (EnemySpritemapDefinitions.BoyonBank, (ushort)(0x88da + ((int)frame - (int)EnemyNamedSpritemapFrameId.boyon_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.boyon_bounce_0 or
                EnemyNamedSpritemapFrameId.boyon_bounce_1 or
                EnemyNamedSpritemapFrameId.boyon_bounce_2 or
                EnemyNamedSpritemapFrameId.boyon_bounce_3 =>
                (EnemySpritemapDefinitions.BoyonBank, (ushort)(0x88ef + ((int)frame - (int)EnemyNamedSpritemapFrameId.boyon_bounce_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.cacatac_upright_idle_0 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_1 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_2 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_3 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_4 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_5 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_6 or
                EnemyNamedSpritemapFrameId.cacatac_upright_idle_7 =>
                (EnemySpritemapDefinitions.BoyonBank, (ushort)(0xa0bb + ((int)frame - (int)EnemyNamedSpritemapFrameId.cacatac_upright_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.cacatac_upright_attack_1 => (EnemySpritemapDefinitions.BoyonBank, (ushort)0xa1bb),
            EnemyNamedSpritemapFrameId.cacatac_upright_attack_2 => (EnemySpritemapDefinitions.BoyonBank, (ushort)0xa1ef),
            EnemyNamedSpritemapFrameId.cacatac_inverted_idle_0 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_1 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_2 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_3 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_4 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_5 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_6 or
                EnemyNamedSpritemapFrameId.cacatac_inverted_idle_7 =>
                (EnemySpritemapDefinitions.BoyonBank, (ushort)(0xa223 + ((int)frame - (int)EnemyNamedSpritemapFrameId.cacatac_inverted_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.cacatac_inverted_attack_1 => (EnemySpritemapDefinitions.BoyonBank, (ushort)0xa323),
            EnemyNamedSpritemapFrameId.cacatac_inverted_attack_2 => (EnemySpritemapDefinitions.BoyonBank, (ushort)0xa357),
            EnemyNamedSpritemapFrameId.boulder_roll_0 or
                EnemyNamedSpritemapFrameId.boulder_roll_1 or
                EnemyNamedSpritemapFrameId.boulder_roll_2 or
                EnemyNamedSpritemapFrameId.boulder_roll_3 or
                EnemyNamedSpritemapFrameId.boulder_roll_4 or
                EnemyNamedSpritemapFrameId.boulder_roll_5 or
                EnemyNamedSpritemapFrameId.boulder_roll_6 or
                EnemyNamedSpritemapFrameId.boulder_roll_7 =>
                (EnemySpritemapDefinitions.BoulderBank, (ushort)(0x8a59 + ((int)frame - (int)EnemyNamedSpritemapFrameId.boulder_roll_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.atomic_up_right_0 or
                EnemyNamedSpritemapFrameId.atomic_up_right_1 or
                EnemyNamedSpritemapFrameId.atomic_up_right_2 or
                EnemyNamedSpritemapFrameId.atomic_up_right_3 or
                EnemyNamedSpritemapFrameId.atomic_up_right_4 =>
                (EnemySpritemapDefinitions.AtomicBank, (ushort)(0xe489 + ((int)frame - (int)EnemyNamedSpritemapFrameId.atomic_up_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.atomic_up_right_5 => (EnemySpritemapDefinitions.AtomicBank, (ushort)0xe4f2),
            EnemyNamedSpritemapFrameId.atomic_up_left_0 or
                EnemyNamedSpritemapFrameId.atomic_up_left_1 or
                EnemyNamedSpritemapFrameId.atomic_up_left_2 or
                EnemyNamedSpritemapFrameId.atomic_up_left_3 or
                EnemyNamedSpritemapFrameId.atomic_up_left_4 =>
                (EnemySpritemapDefinitions.AtomicBank, (ushort)(0xe508 + ((int)frame - (int)EnemyNamedSpritemapFrameId.atomic_up_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.atomic_up_left_5 => (EnemySpritemapDefinitions.AtomicBank, (ushort)0xe571),
            EnemyNamedSpritemapFrameId.skultera_swim_left_0 or
                EnemyNamedSpritemapFrameId.skultera_swim_left_1 or
                EnemyNamedSpritemapFrameId.skultera_swim_left_2 =>
                (EnemySpritemapDefinitions.SkulteraBank, (ushort)(0x928a + ((int)frame - (int)EnemyNamedSpritemapFrameId.skultera_swim_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.skultera_turn_right_0 or
                EnemyNamedSpritemapFrameId.skultera_turn_right_1 or
                EnemyNamedSpritemapFrameId.skultera_turn_right_2 =>
                (EnemySpritemapDefinitions.SkulteraBank, (ushort)(0x92db + ((int)frame - (int)EnemyNamedSpritemapFrameId.skultera_turn_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.skultera_turn_right_3 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x9327),
            EnemyNamedSpritemapFrameId.skultera_turn_right_4 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x933d),
            EnemyNamedSpritemapFrameId.skultera_turn_right_5 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x934e),
            EnemyNamedSpritemapFrameId.skultera_turn_right_6 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x9364),
            EnemyNamedSpritemapFrameId.skultera_turn_right_7 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x937f),
            EnemyNamedSpritemapFrameId.skultera_swim_right_0 or
                EnemyNamedSpritemapFrameId.skultera_swim_right_1 or
                EnemyNamedSpritemapFrameId.skultera_swim_right_2 =>
                (EnemySpritemapDefinitions.SkulteraBank, (ushort)(0x939a + ((int)frame - (int)EnemyNamedSpritemapFrameId.skultera_swim_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.skultera_turn_left_0 or
                EnemyNamedSpritemapFrameId.skultera_turn_left_1 or
                EnemyNamedSpritemapFrameId.skultera_turn_left_2 =>
                (EnemySpritemapDefinitions.SkulteraBank, (ushort)(0x93eb + ((int)frame - (int)EnemyNamedSpritemapFrameId.skultera_turn_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.skultera_turn_left_3 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x9437),
            EnemyNamedSpritemapFrameId.skultera_turn_left_4 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x944d),
            EnemyNamedSpritemapFrameId.skultera_turn_left_5 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x945e),
            EnemyNamedSpritemapFrameId.skultera_turn_left_6 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x9474),
            EnemyNamedSpritemapFrameId.skultera_turn_left_7 => (EnemySpritemapDefinitions.SkulteraBank, (ushort)0x948f),
            EnemyNamedSpritemapFrameId.waver_steady_left => (EnemySpritemapDefinitions.WaverBank, (ushort)0x884a),
            EnemyNamedSpritemapFrameId.waver_steady_right => (EnemySpritemapDefinitions.WaverBank, (ushort)0x88b3),
            EnemyNamedSpritemapFrameId.waver_spin_left_0 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x885b),
            EnemyNamedSpritemapFrameId.waver_spin_left_1 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x8871),
            EnemyNamedSpritemapFrameId.waver_spin_left_2 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x881e),
            EnemyNamedSpritemapFrameId.waver_spin_left_3 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x8834),
            EnemyNamedSpritemapFrameId.waver_spin_right_0 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x88c4),
            EnemyNamedSpritemapFrameId.waver_spin_right_1 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x88da),
            EnemyNamedSpritemapFrameId.waver_spin_right_2 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x8887),
            EnemyNamedSpritemapFrameId.waver_spin_right_3 => (EnemySpritemapDefinitions.WaverBank, (ushort)0x889d),
            EnemyNamedSpritemapFrameId.metaree_idle_0 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0x8b65),
            EnemyNamedSpritemapFrameId.metaree_idle_1 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0x8bb9),
            EnemyNamedSpritemapFrameId.metaree_idle_2 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0x8bde),
            EnemyNamedSpritemapFrameId.metaree_idle_3 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0x8bea),
            EnemyNamedSpritemapFrameId.metaree_prepare_1 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0x8b94),
            EnemyNamedSpritemapFrameId.skree_idle_0 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0xc842),
            EnemyNamedSpritemapFrameId.skree_idle_1 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0xc878),
            EnemyNamedSpritemapFrameId.skree_idle_2 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0xc884),
            EnemyNamedSpritemapFrameId.skree_idle_3 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0xc89a),
            EnemyNamedSpritemapFrameId.skree_prepare_1 => (EnemySpritemapDefinitions.SkreeMetareeBank, (ushort)0xc862),
            EnemyNamedSpritemapFrameId.zoa_shoot_left_0 or
                EnemyNamedSpritemapFrameId.zoa_shoot_left_1 or
                EnemyNamedSpritemapFrameId.zoa_shoot_left_2 =>
                (EnemySpritemapDefinitions.ZoaBank, (ushort)(0xb55f + ((int)frame - (int)EnemyNamedSpritemapFrameId.zoa_shoot_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.zoa_rise_left_0 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb57b),
            EnemyNamedSpritemapFrameId.zoa_rise_left_1 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb574),
            EnemyNamedSpritemapFrameId.zoa_rise_left_2 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb582),
            EnemyNamedSpritemapFrameId.zoa_shoot_right_0 or
                EnemyNamedSpritemapFrameId.zoa_shoot_right_1 or
                EnemyNamedSpritemapFrameId.zoa_shoot_right_2 =>
                (EnemySpritemapDefinitions.ZoaBank, (ushort)(0xb589 + ((int)frame - (int)EnemyNamedSpritemapFrameId.zoa_shoot_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.zoa_rise_right_0 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb5a5),
            EnemyNamedSpritemapFrameId.zoa_rise_right_1 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb59e),
            EnemyNamedSpritemapFrameId.zoa_rise_right_2 => (EnemySpritemapDefinitions.ZoaBank, (ushort)0xb5ac),
            EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_2 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_3 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_4 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x89b7 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_normal_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_2 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_3 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_4 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x89da + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_normal_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_left_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_left_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_left_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8a6d + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_left_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_left_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_left_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8a82 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_right_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_right_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_right_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8a97 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_strong_shoot_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_right_0 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_right_1 or
                EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_right_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8aac + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_brinstar_strong_rise_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_norfair_left_0 or
                EnemyNamedSpritemapFrameId.pipe_norfair_left_1 or
                EnemyNamedSpritemapFrameId.pipe_norfair_left_2 or
                EnemyNamedSpritemapFrameId.pipe_norfair_left_3 or
                EnemyNamedSpritemapFrameId.pipe_norfair_left_4 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8e96 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_norfair_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_norfair_right_0 or
                EnemyNamedSpritemapFrameId.pipe_norfair_right_1 or
                EnemyNamedSpritemapFrameId.pipe_norfair_right_2 or
                EnemyNamedSpritemapFrameId.pipe_norfair_right_3 or
                EnemyNamedSpritemapFrameId.pipe_norfair_right_4 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x8eb9 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_norfair_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_yellow_fly_left_0 or
                EnemyNamedSpritemapFrameId.pipe_yellow_fly_left_1 or
                EnemyNamedSpritemapFrameId.pipe_yellow_fly_left_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x92ad + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_yellow_fly_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_yellow_arc_left_0 or
                EnemyNamedSpritemapFrameId.pipe_yellow_arc_left_1 or
                EnemyNamedSpritemapFrameId.pipe_yellow_arc_left_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x92c2 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_yellow_arc_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_yellow_fly_right_0 or
                EnemyNamedSpritemapFrameId.pipe_yellow_fly_right_1 or
                EnemyNamedSpritemapFrameId.pipe_yellow_fly_right_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x92d7 + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_yellow_fly_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.pipe_yellow_arc_right_0 or
                EnemyNamedSpritemapFrameId.pipe_yellow_arc_right_1 or
                EnemyNamedSpritemapFrameId.pipe_yellow_arc_right_2 =>
                (EnemySpritemapDefinitions.PipeBugBank, (ushort)(0x92ec + ((int)frame - (int)EnemyNamedSpritemapFrameId.pipe_yellow_arc_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.fake_kraid_walk_left_0 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_left_1 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_left_2 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_left_3 =>
                (EnemySpritemapDefinitions.FakeKraidBank, (ushort)(0x9c64 + ((int)frame - (int)EnemyNamedSpritemapFrameId.fake_kraid_walk_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(16))),
            EnemyNamedSpritemapFrameId.fake_kraid_spit_left_0 or
                EnemyNamedSpritemapFrameId.fake_kraid_spit_left_1 or
                EnemyNamedSpritemapFrameId.fake_kraid_spit_left_2 =>
                (EnemySpritemapDefinitions.FakeKraidBank, (ushort)(0x9dac + ((int)frame - (int)EnemyNamedSpritemapFrameId.fake_kraid_spit_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(16))),
            EnemyNamedSpritemapFrameId.fake_kraid_walk_right_0 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_right_1 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_right_2 or
                EnemyNamedSpritemapFrameId.fake_kraid_walk_right_3 =>
                (EnemySpritemapDefinitions.FakeKraidBank, (ushort)(0x9ea2 + ((int)frame - (int)EnemyNamedSpritemapFrameId.fake_kraid_walk_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(16))),
            EnemyNamedSpritemapFrameId.fake_kraid_spit_right_0 or
                EnemyNamedSpritemapFrameId.fake_kraid_spit_right_1 or
                EnemyNamedSpritemapFrameId.fake_kraid_spit_right_2 =>
                (EnemySpritemapDefinitions.FakeKraidBank, (ushort)(0x9fea + ((int)frame - (int)EnemyNamedSpritemapFrameId.fake_kraid_spit_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(16))),
            EnemyNamedSpritemapFrameId.kraid_nail_0 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa617),
            EnemyNamedSpritemapFrameId.kraid_nail_1 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa623),
            EnemyNamedSpritemapFrameId.kraid_nail_2 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa639),
            EnemyNamedSpritemapFrameId.kraid_nail_3 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa645),
            EnemyNamedSpritemapFrameId.kraid_nail_4 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa65b),
            EnemyNamedSpritemapFrameId.kraid_nail_5 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa667),
            EnemyNamedSpritemapFrameId.kraid_nail_6 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa67d),
            EnemyNamedSpritemapFrameId.kraid_nail_7 => (EnemySpritemapDefinitions.KraidNailBank, (ushort)0xa689),
            EnemyNamedSpritemapFrameId.owtch_left_0 or
                EnemyNamedSpritemapFrameId.owtch_left_1 or
                EnemyNamedSpritemapFrameId.owtch_left_2 =>
                (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)(0xa589 + ((int)frame - (int)EnemyNamedSpritemapFrameId.owtch_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.stoke_walk_left_0 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8aca),
            EnemyNamedSpritemapFrameId.stoke_walk_left_1 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8ad6),
            EnemyNamedSpritemapFrameId.stoke_walk_left_2 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8ae7),
            EnemyNamedSpritemapFrameId.stoke_walk_left_3 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8af3),
            EnemyNamedSpritemapFrameId.stoke_attack_left => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8aff),
            EnemyNamedSpritemapFrameId.stoke_walk_right_0 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8b15),
            EnemyNamedSpritemapFrameId.stoke_walk_right_1 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8b21),
            EnemyNamedSpritemapFrameId.stoke_walk_right_2 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8b32),
            EnemyNamedSpritemapFrameId.stoke_walk_right_3 => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8b3e),
            EnemyNamedSpritemapFrameId.stoke_attack_right => (EnemySpritemapDefinitions.OwtchStokeBank, (ushort)0x8b4a),
            EnemyNamedSpritemapFrameId.ripper_shared_left_0 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe3c5),
            EnemyNamedSpritemapFrameId.ripper_shared_left_1 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe3db),
            EnemyNamedSpritemapFrameId.ripper_shared_left_2 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe3ec),
            EnemyNamedSpritemapFrameId.ripper_shared_right_0 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe402),
            EnemyNamedSpritemapFrameId.ripper_shared_right_1 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe418),
            EnemyNamedSpritemapFrameId.ripper_shared_right_2 => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe429),
            EnemyNamedSpritemapFrameId.ripper_shared_frozen_left => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe43f),
            EnemyNamedSpritemapFrameId.ripper_shared_frozen_right => (EnemySpritemapDefinitions.RipperBank, (ushort)0xe44b),
            EnemyNamedSpritemapFrameId.ripper_left_0 or
                EnemyNamedSpritemapFrameId.ripper_left_1 or
                EnemyNamedSpritemapFrameId.ripper_left_2 =>
                (EnemySpritemapDefinitions.RipperBank, (ushort)(0xe527 + ((int)frame - (int)EnemyNamedSpritemapFrameId.ripper_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(2))),
            EnemyNamedSpritemapFrameId.ripper_right_0 or
                EnemyNamedSpritemapFrameId.ripper_right_1 or
                EnemyNamedSpritemapFrameId.ripper_right_2 =>
                (EnemySpritemapDefinitions.RipperBank, (ushort)(0xe54b + ((int)frame - (int)EnemyNamedSpritemapFrameId.ripper_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(2))),
            EnemyNamedSpritemapFrameId.fireflea_cycle_0 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_1 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_2 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_3 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_4 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_5 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_6 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_7 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_8 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_9 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_10 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_11 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_12 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_13 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_14 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_15 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_16 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_17 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_18 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_19 or
                EnemyNamedSpritemapFrameId.fireflea_cycle_20 =>
                (EnemySpritemapDefinitions.FirefleaBank, (ushort)(0x8ea5 + ((int)frame - (int)EnemyNamedSpritemapFrameId.fireflea_cycle_0) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.magdollite_left_idle_0 or
                EnemyNamedSpritemapFrameId.magdollite_left_idle_1 or
                EnemyNamedSpritemapFrameId.magdollite_left_idle_2 =>
                (EnemySpritemapDefinitions.MagdolliteBank, (ushort)(0xb448 + ((int)frame - (int)EnemyNamedSpritemapFrameId.magdollite_left_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.magdollite_left_throw_0 or
                EnemyNamedSpritemapFrameId.magdollite_left_throw_1 or
                EnemyNamedSpritemapFrameId.magdollite_left_throw_2 =>
                (EnemySpritemapDefinitions.MagdolliteBank, (ushort)(0xb47b + ((int)frame - (int)EnemyNamedSpritemapFrameId.magdollite_left_throw_0) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.magdollite_left_throw_3 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb4a9),
            EnemyNamedSpritemapFrameId.magdollite_pillar_cap => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb4b5),
            EnemyNamedSpritemapFrameId.magdollite_left_submerge_0 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb4c1),
            EnemyNamedSpritemapFrameId.magdollite_left_submerge_1 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb4cf),
            EnemyNamedSpritemapFrameId.magdollite_left_submerge_2 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb4e0),
            EnemyNamedSpritemapFrameId.magdollite_right_idle_0 or
                EnemyNamedSpritemapFrameId.magdollite_right_idle_1 or
                EnemyNamedSpritemapFrameId.magdollite_right_idle_2 =>
                (EnemySpritemapDefinitions.MagdolliteBank, (ushort)(0xb4f1 + ((int)frame - (int)EnemyNamedSpritemapFrameId.magdollite_right_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.magdollite_right_throw_0 or
                EnemyNamedSpritemapFrameId.magdollite_right_throw_1 or
                EnemyNamedSpritemapFrameId.magdollite_right_throw_2 =>
                (EnemySpritemapDefinitions.MagdolliteBank, (ushort)(0xb524 + ((int)frame - (int)EnemyNamedSpritemapFrameId.magdollite_right_throw_0) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.magdollite_right_throw_3 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb552),
            EnemyNamedSpritemapFrameId.magdollite_right_submerge_0 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb56a),
            EnemyNamedSpritemapFrameId.magdollite_right_submerge_1 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb578),
            EnemyNamedSpritemapFrameId.magdollite_right_submerge_2 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb589),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_0 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb59a),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_1 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb5a1),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_2 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb5ad),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_3 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb5be),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_4 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb5d4),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_5 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb5ef),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_6 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb60f),
            EnemyNamedSpritemapFrameId.magdollite_pillar_phase_7 => (EnemySpritemapDefinitions.MagdolliteBank, (ushort)0xb634),
            EnemyNamedSpritemapFrameId.ceres_door_rotating_overlay => (CeresDoorFrameDefinitions.Bank, (ushort)0xf921),
            EnemyNamedSpritemapFrameId.ceres_door_left_hold => (CeresDoorFrameDefinitions.Bank, (ushort)0xf95f),
            EnemyNamedSpritemapFrameId.ceres_door_left_transition_0 => (CeresDoorFrameDefinitions.Bank, (ushort)0xf989),
            EnemyNamedSpritemapFrameId.ceres_door_left_transition_1 or
                EnemyNamedSpritemapFrameId.ceres_door_left_transition_2 or
                EnemyNamedSpritemapFrameId.ceres_door_left_transition_3 =>
                (CeresDoorFrameDefinitions.Bank, (ushort)(0xf9b3 + ((int)frame - (int)EnemyNamedSpritemapFrameId.ceres_door_left_transition_1) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.ceres_door_right_hold => (CeresDoorFrameDefinitions.Bank, (ushort)0xfa13),
            EnemyNamedSpritemapFrameId.ceres_door_right_transition_0 => (CeresDoorFrameDefinitions.Bank, (ushort)0xfa3d),
            EnemyNamedSpritemapFrameId.ceres_door_right_transition_1 or
                EnemyNamedSpritemapFrameId.ceres_door_right_transition_2 or
                EnemyNamedSpritemapFrameId.ceres_door_right_transition_3 =>
                (CeresDoorFrameDefinitions.Bank, (ushort)(0xfa67 + ((int)frame - (int)EnemyNamedSpritemapFrameId.ceres_door_right_transition_1) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.ceres_door_initial => (CeresDoorFrameDefinitions.Bank, CeresDoorFrameDefinitions.InitialSpritemap),
            EnemyNamedSpritemapFrameId.ceres_door_mode7_left_wall => (CeresDoorFrameDefinitions.Bank, (ushort)0xface),
            EnemyNamedSpritemapFrameId.ceres_door_mode7_right_wall => (CeresDoorFrameDefinitions.Bank, (ushort)0xfb2f),
            EnemyNamedSpritemapFrameId.ceres_door_ridley_private_overlay => (CeresDoorFrameDefinitions.Bank, CeresDoorFrameDefinitions.RidleyPrivateOverlaySpritemap),
            EnemyNamedSpritemapFrameId.ceres_baby_horizontal => (CeresBabyFrameDefinitions.Bank, CeresBabyFrameDefinitions.HorizontalFrame),
            EnemyNamedSpritemapFrameId.ceres_baby_round => (CeresBabyFrameDefinitions.Bank, CeresBabyFrameDefinitions.RoundFrame),
            EnemyNamedSpritemapFrameId.ceres_baby_vertical => (CeresBabyFrameDefinitions.Bank, CeresBabyFrameDefinitions.VerticalFrame),
            EnemyNamedSpritemapFrameId.rio_bd6c => (EnemySpritemapDefinitions.RioBank, (ushort)0xbd6c),
            EnemyNamedSpritemapFrameId.rio_bd82 => (EnemySpritemapDefinitions.RioBank, (ushort)0xbd82),
            EnemyNamedSpritemapFrameId.rio_bd98 => (EnemySpritemapDefinitions.RioBank, (ushort)0xbd98),
            EnemyNamedSpritemapFrameId.rio_bdae => (EnemySpritemapDefinitions.RioBank, (ushort)0xbdae),
            EnemyNamedSpritemapFrameId.rio_bdc4 => (EnemySpritemapDefinitions.RioBank, (ushort)0xbdc4),
            EnemyNamedSpritemapFrameId.rio_bdda => (EnemySpritemapDefinitions.RioBank, (ushort)0xbdda),
            EnemyNamedSpritemapFrameId.rio_bdf0 => (EnemySpritemapDefinitions.RioBank, (ushort)0xbdf0),
            EnemyNamedSpritemapFrameId.rio_be06 => (EnemySpritemapDefinitions.RioBank, (ushort)0xbe06),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c8bd => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc8bd),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c8d3 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc8d3),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c8e9 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc8e9),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c8ff => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc8ff),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c915 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc915),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c92b => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc92b),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c941 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc941),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c957 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc957),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c96d => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc96d),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c983 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc983),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c999 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc999),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c9af => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc9af),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c9c5 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc9c5),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c9db => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc9db),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_c9f1 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xc9f1),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_ca07 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xca07),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_ca13 => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xca13),
            EnemyNamedSpritemapFrameId.lower_norfair_rio_ca1f => (EnemySpritemapDefinitions.LowerNorfairRioBank, (ushort)0xca1f),
            EnemyNamedSpritemapFrameId.norfair_rio_c427 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc427),
            EnemyNamedSpritemapFrameId.norfair_rio_c442 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc442),
            EnemyNamedSpritemapFrameId.norfair_rio_c45d => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc45d),
            EnemyNamedSpritemapFrameId.norfair_rio_c493 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc493),
            EnemyNamedSpritemapFrameId.norfair_rio_c49f => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc49f),
            EnemyNamedSpritemapFrameId.norfair_rio_c4ab => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc4ab),
            EnemyNamedSpritemapFrameId.norfair_rio_c4b7 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc4b7),
            EnemyNamedSpritemapFrameId.norfair_rio_c4d2 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc4d2),
            EnemyNamedSpritemapFrameId.norfair_rio_c4ed => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc4ed),
            EnemyNamedSpritemapFrameId.norfair_rio_c508 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc508),
            EnemyNamedSpritemapFrameId.norfair_rio_c523 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc523),
            EnemyNamedSpritemapFrameId.norfair_rio_c534 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc534),
            EnemyNamedSpritemapFrameId.norfair_rio_c54f => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc54f),
            EnemyNamedSpritemapFrameId.norfair_rio_c56a => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc56a),
            EnemyNamedSpritemapFrameId.norfair_rio_c585 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc585),
            EnemyNamedSpritemapFrameId.norfair_rio_c5a0 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc5a0),
            EnemyNamedSpritemapFrameId.norfair_rio_c5bb => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc5bb),
            EnemyNamedSpritemapFrameId.norfair_rio_c5d6 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc5d6),
            EnemyNamedSpritemapFrameId.norfair_rio_c5e2 => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc5e2),
            EnemyNamedSpritemapFrameId.norfair_rio_c5ee => (EnemySpritemapDefinitions.NorfairRioBank, (ushort)0xc5ee),
            EnemyNamedSpritemapFrameId.puyo_ground_0 or
                EnemyNamedSpritemapFrameId.puyo_ground_1 or
                EnemyNamedSpritemapFrameId.puyo_ground_2 =>
                (EnemySpritemapDefinitions.PuyoBank, (ushort)(0x9df6 + ((int)frame - (int)EnemyNamedSpritemapFrameId.puyo_ground_0) * EnemySpritemapDefinitions.NativeFrameBytes(2))),
            EnemyNamedSpritemapFrameId.puyo_air_0 => (EnemySpritemapDefinitions.PuyoBank, (ushort)0x9e1a),
            EnemyNamedSpritemapFrameId.puyo_air_1 => (EnemySpritemapDefinitions.PuyoBank, (ushort)0x9e26),
            EnemyNamedSpritemapFrameId.puyo_air_2 => (EnemySpritemapDefinitions.PuyoBank, (ushort)0x9e37),
            EnemyNamedSpritemapFrameId.puyo_air_3 => (EnemySpritemapDefinitions.PuyoBank, (ushort)0x9e4d),
            EnemyNamedSpritemapFrameId.puyo_air_4 => (EnemySpritemapDefinitions.PuyoBank, (ushort)0x9e5e),
            EnemyNamedSpritemapFrameId.bull_idle_0 or
                EnemyNamedSpritemapFrameId.bull_idle_1 or
                EnemyNamedSpritemapFrameId.bull_idle_2 =>
                (EnemySpritemapDefinitions.BullBank, (ushort)(0xdb76 + ((int)frame - (int)EnemyNamedSpritemapFrameId.bull_idle_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.alcoon_left_walk_0 or
                EnemyNamedSpritemapFrameId.alcoon_left_walk_1 or
                EnemyNamedSpritemapFrameId.alcoon_left_walk_2 =>
                (EnemySpritemapDefinitions.AlcoonBank, (ushort)(0xdfa2 + ((int)frame - (int)EnemyNamedSpritemapFrameId.alcoon_left_walk_0) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.alcoon_left_walk_3 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe007),
            EnemyNamedSpritemapFrameId.alcoon_left_fire_0 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe027),
            EnemyNamedSpritemapFrameId.alcoon_left_fire_1 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe047),
            EnemyNamedSpritemapFrameId.alcoon_left_fire_2 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe06c),
            EnemyNamedSpritemapFrameId.alcoon_left_fire_3 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe09b),
            EnemyNamedSpritemapFrameId.alcoon_left_air_up => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe0bb),
            EnemyNamedSpritemapFrameId.alcoon_right_walk_0 or
                EnemyNamedSpritemapFrameId.alcoon_right_walk_1 or
                EnemyNamedSpritemapFrameId.alcoon_right_walk_2 =>
                (EnemySpritemapDefinitions.AlcoonBank, (ushort)(0xe0db + ((int)frame - (int)EnemyNamedSpritemapFrameId.alcoon_right_walk_0) * EnemySpritemapDefinitions.NativeFrameBytes(6))),
            EnemyNamedSpritemapFrameId.alcoon_right_walk_3 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe140),
            EnemyNamedSpritemapFrameId.alcoon_right_fire_0 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe160),
            EnemyNamedSpritemapFrameId.alcoon_right_fire_1 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe180),
            EnemyNamedSpritemapFrameId.alcoon_right_fire_2 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe1a5),
            EnemyNamedSpritemapFrameId.alcoon_right_fire_3 => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe1d4),
            EnemyNamedSpritemapFrameId.alcoon_right_air_up => (EnemySpritemapDefinitions.AlcoonBank, (ushort)0xe1f4),
            EnemyNamedSpritemapFrameId.beetom_left_crawl_0 or
                EnemyNamedSpritemapFrameId.beetom_left_crawl_1 or
                EnemyNamedSpritemapFrameId.beetom_left_crawl_2 =>
                (EnemySpritemapDefinitions.BeetomBank, (ushort)(0xbed3 + ((int)frame - (int)EnemyNamedSpritemapFrameId.beetom_left_crawl_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.beetom_left_hop_0 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xbf24),
            EnemyNamedSpritemapFrameId.beetom_left_hop_1 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xbf3f),
            EnemyNamedSpritemapFrameId.beetom_left_drain_0 or
                EnemyNamedSpritemapFrameId.beetom_left_drain_1 or
                EnemyNamedSpritemapFrameId.beetom_left_drain_2 or
                EnemyNamedSpritemapFrameId.beetom_left_drain_3 =>
                (EnemySpritemapDefinitions.BeetomBank, (ushort)(0xbf5a + ((int)frame - (int)EnemyNamedSpritemapFrameId.beetom_left_drain_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.beetom_left_drain_4 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xbfcb),
            EnemyNamedSpritemapFrameId.beetom_left_drain_5 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xbfeb),
            EnemyNamedSpritemapFrameId.beetom_right_crawl_0 or
                EnemyNamedSpritemapFrameId.beetom_right_crawl_1 or
                EnemyNamedSpritemapFrameId.beetom_right_crawl_2 =>
                (EnemySpritemapDefinitions.BeetomBank, (ushort)(0xc00b + ((int)frame - (int)EnemyNamedSpritemapFrameId.beetom_right_crawl_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.beetom_right_hop_0 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xc05c),
            EnemyNamedSpritemapFrameId.beetom_right_hop_1 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xc077),
            EnemyNamedSpritemapFrameId.beetom_right_drain_0 or
                EnemyNamedSpritemapFrameId.beetom_right_drain_1 or
                EnemyNamedSpritemapFrameId.beetom_right_drain_2 or
                EnemyNamedSpritemapFrameId.beetom_right_drain_3 =>
                (EnemySpritemapDefinitions.BeetomBank, (ushort)(0xc092 + ((int)frame - (int)EnemyNamedSpritemapFrameId.beetom_right_drain_0) * EnemySpritemapDefinitions.NativeFrameBytes(5))),
            EnemyNamedSpritemapFrameId.beetom_right_drain_4 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xc103),
            EnemyNamedSpritemapFrameId.beetom_right_drain_5 => (EnemySpritemapDefinitions.BeetomBank, (ushort)0xc123),
            EnemyNamedSpritemapFrameId.sidehopper_jump_floor => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaf19),
            EnemyNamedSpritemapFrameId.sidehopper_land_floor_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaee3),
            EnemyNamedSpritemapFrameId.sidehopper_land_floor_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaefe),
            EnemyNamedSpritemapFrameId.sidehopper_jump_ceiling => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaf6a),
            EnemyNamedSpritemapFrameId.sidehopper_land_ceiling_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaf34),
            EnemyNamedSpritemapFrameId.sidehopper_land_ceiling_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaf4f),
            EnemyNamedSpritemapFrameId.dessgeega_jump_floor => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb019),
            EnemyNamedSpritemapFrameId.dessgeega_land_floor_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xafe3),
            EnemyNamedSpritemapFrameId.dessgeega_land_floor_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xaffe),
            EnemyNamedSpritemapFrameId.dessgeega_jump_ceiling => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb06a),
            EnemyNamedSpritemapFrameId.dessgeega_land_ceiling_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb034),
            EnemyNamedSpritemapFrameId.dessgeega_land_ceiling_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb04f),
            EnemyNamedSpritemapFrameId.large_sidehopper_jump_floor => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb15b),
            EnemyNamedSpritemapFrameId.large_sidehopper_land_floor_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb111),
            EnemyNamedSpritemapFrameId.large_sidehopper_land_floor_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb136),
            EnemyNamedSpritemapFrameId.large_sidehopper_jump_ceiling => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb1de),
            EnemyNamedSpritemapFrameId.large_sidehopper_land_ceiling_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb194),
            EnemyNamedSpritemapFrameId.large_sidehopper_land_ceiling_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb1b9),
            EnemyNamedSpritemapFrameId.large_dessgeega_jump_floor => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb2d1),
            EnemyNamedSpritemapFrameId.large_dessgeega_land_floor_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb273),
            EnemyNamedSpritemapFrameId.large_dessgeega_land_floor_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb2a2),
            EnemyNamedSpritemapFrameId.large_dessgeega_jump_ceiling => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb368),
            EnemyNamedSpritemapFrameId.large_dessgeega_land_ceiling_0 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb30a),
            EnemyNamedSpritemapFrameId.large_dessgeega_land_ceiling_1 => (EnemySpritemapDefinitions.HopperBank, (ushort)0xb339),
            EnemyNamedSpritemapFrameId.choot_idle => (EnemySpritemapDefinitions.ChootBank, (ushort)0xe146),
            EnemyNamedSpritemapFrameId.choot_jump => (EnemySpritemapDefinitions.ChootBank, (ushort)0xe15c),
            EnemyNamedSpritemapFrameId.choot_jump_apex => (EnemySpritemapDefinitions.ChootBank, (ushort)0xe168),
            EnemyNamedSpritemapFrameId.choot_fall_end => (EnemySpritemapDefinitions.ChootBank, (ushort)0xe16f),
            EnemyNamedSpritemapFrameId.hzoomer_upside_right_0 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_right_1 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_right_2 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_right_3 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_right_4 =>
                (EnemySpritemapDefinitions.HZoomerBank, (ushort)(0xe50e + ((int)frame - (int)EnemyNamedSpritemapFrameId.hzoomer_upside_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.hzoomer_upside_left_0 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_left_1 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_left_2 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_left_3 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_left_4 =>
                (EnemySpritemapDefinitions.HZoomerBank, (ushort)(0xe3c4 + ((int)frame - (int)EnemyNamedSpritemapFrameId.hzoomer_upside_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.hzoomer_upside_down_0 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_down_1 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_down_2 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_down_3 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_down_4 =>
                (EnemySpritemapDefinitions.HZoomerBank, (ushort)(0xe432 + ((int)frame - (int)EnemyNamedSpritemapFrameId.hzoomer_upside_down_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.hzoomer_upside_up_0 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_up_1 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_up_2 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_up_3 or
                EnemyNamedSpritemapFrameId.hzoomer_upside_up_4 =>
                (EnemySpritemapDefinitions.HZoomerBank, (ushort)(0xe2e8 + ((int)frame - (int)EnemyNamedSpritemapFrameId.hzoomer_upside_up_0) * EnemySpritemapDefinitions.NativeFrameBytes(4))),
            EnemyNamedSpritemapFrameId.sbug_right_0 or
                EnemyNamedSpritemapFrameId.sbug_right_1 or
                EnemyNamedSpritemapFrameId.sbug_right_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa67d + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_up_right_0 or
                EnemyNamedSpritemapFrameId.sbug_up_right_1 or
                EnemyNamedSpritemapFrameId.sbug_up_right_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa692 + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_up_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_up_0 or
                EnemyNamedSpritemapFrameId.sbug_up_1 or
                EnemyNamedSpritemapFrameId.sbug_up_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa6a7 + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_up_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_up_left_0 or
                EnemyNamedSpritemapFrameId.sbug_up_left_1 or
                EnemyNamedSpritemapFrameId.sbug_up_left_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa6bc + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_up_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_left_0 or
                EnemyNamedSpritemapFrameId.sbug_left_1 or
                EnemyNamedSpritemapFrameId.sbug_left_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa6d1 + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_down_left_0 or
                EnemyNamedSpritemapFrameId.sbug_down_left_1 or
                EnemyNamedSpritemapFrameId.sbug_down_left_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa6e6 + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_down_left_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_down_0 or
                EnemyNamedSpritemapFrameId.sbug_down_1 or
                EnemyNamedSpritemapFrameId.sbug_down_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa6fb + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_down_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.sbug_down_right_0 or
                EnemyNamedSpritemapFrameId.sbug_down_right_1 or
                EnemyNamedSpritemapFrameId.sbug_down_right_2 =>
                (EnemySpritemapDefinitions.SbugBank, (ushort)(0xa710 + ((int)frame - (int)EnemyNamedSpritemapFrameId.sbug_down_right_0) * EnemySpritemapDefinitions.NativeFrameBytes(1))),
            EnemyNamedSpritemapFrameId.fune_left_idle => (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)0x93f9),
            EnemyNamedSpritemapFrameId.fune_left_active_0 or
                EnemyNamedSpritemapFrameId.fune_left_active_1 or
                EnemyNamedSpritemapFrameId.fune_left_active_2 or
                EnemyNamedSpritemapFrameId.fune_left_active_3 =>
                (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)(0x9423 + ((int)frame - (int)EnemyNamedSpritemapFrameId.fune_left_active_0) * EnemySpritemapDefinitions.NativeFrameBytes(8))),
            EnemyNamedSpritemapFrameId.fune_right_idle => (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)0x94cb),
            EnemyNamedSpritemapFrameId.fune_right_active_0 or
                EnemyNamedSpritemapFrameId.fune_right_active_1 or
                EnemyNamedSpritemapFrameId.fune_right_active_2 or
                EnemyNamedSpritemapFrameId.fune_right_active_3 =>
                (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)(0x94f5 + ((int)frame - (int)EnemyNamedSpritemapFrameId.fune_right_active_0) * EnemySpritemapDefinitions.NativeFrameBytes(8))),
            EnemyNamedSpritemapFrameId.namihe_left_idle => (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)0x97b4),
            EnemyNamedSpritemapFrameId.namihe_left_active_0 or
                EnemyNamedSpritemapFrameId.namihe_left_active_1 or
                EnemyNamedSpritemapFrameId.namihe_left_active_2 or
                EnemyNamedSpritemapFrameId.namihe_left_active_3 or
                EnemyNamedSpritemapFrameId.namihe_left_active_4 =>
                (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)(0x97de + ((int)frame - (int)EnemyNamedSpritemapFrameId.namihe_left_active_0) * EnemySpritemapDefinitions.NativeFrameBytes(8))),
            EnemyNamedSpritemapFrameId.namihe_right_idle => (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)0x98b0),
            EnemyNamedSpritemapFrameId.namihe_right_active_0 or
                EnemyNamedSpritemapFrameId.namihe_right_active_1 or
                EnemyNamedSpritemapFrameId.namihe_right_active_2 or
                EnemyNamedSpritemapFrameId.namihe_right_active_3 or
                EnemyNamedSpritemapFrameId.namihe_right_active_4 =>
                (EnemySpritemapDefinitions.FuneNamiheBank, (ushort)(0x98da + ((int)frame - (int)EnemyNamedSpritemapFrameId.namihe_right_active_0) * EnemySpritemapDefinitions.NativeFrameBytes(8))),
            EnemyNamedSpritemapFrameId.kamer_platform_0 or
                EnemyNamedSpritemapFrameId.kamer_platform_1 or
                EnemyNamedSpritemapFrameId.kamer_platform_2 or
                EnemyNamedSpritemapFrameId.kamer_platform_3 =>
                (EnemySpritemapDefinitions.KamerPlatformBank, (ushort)(0xf468 + ((int)frame - (int)EnemyNamedSpritemapFrameId.kamer_platform_0) * EnemySpritemapDefinitions.NativeFrameBytes(2))),
            EnemyNamedSpritemapFrameId.elevator_platform_0 => (EnemySpritemapDefinitions.ElevatorBank, (ushort)0x962f),
            EnemyNamedSpritemapFrameId.elevator_platform_1 => (EnemySpritemapDefinitions.ElevatorBank, (ushort)0x9645),
            EnemyNamedSpritemapFrameId.draygon_intro_evir_0 or
                EnemyNamedSpritemapFrameId.draygon_intro_evir_1 or
                EnemyNamedSpritemapFrameId.draygon_intro_evir_2 or
                EnemyNamedSpritemapFrameId.draygon_intro_evir_3 =>
                (EnemySpritemapDefinitions.RoomSpriteObjectBank, (ushort)(0xdb42 + ((int)frame - (int)EnemyNamedSpritemapFrameId.draygon_intro_evir_0) * EnemySpritemapDefinitions.NativeFrameBytes(12))),
            EnemyNamedSpritemapFrameId.draygon_breath_bubble_0 => (EnemySpritemapDefinitions.RoomSpriteObjectBank, (ushort)0xc920),
            EnemyNamedSpritemapFrameId.draygon_breath_bubble_1 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_2 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_3 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_4 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_5 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_6 or
                EnemyNamedSpritemapFrameId.draygon_breath_bubble_7 =>
                (EnemySpritemapDefinitions.RoomSpriteObjectBank, (ushort)(0xc927 + ((int)frame - (int)EnemyNamedSpritemapFrameId.draygon_breath_bubble_1) * EnemySpritemapDefinitions.NativeFrameBytes(3))),
            EnemyNamedSpritemapFrameId.draygon_breath_bubble_8 => (EnemySpritemapDefinitions.RoomSpriteObjectBank, (ushort)0xc999),
            _ => throw new ArgumentOutOfRangeException(nameof(frame)),
        };
        return new(bank, pointer, frame.ToString());
    }
}
