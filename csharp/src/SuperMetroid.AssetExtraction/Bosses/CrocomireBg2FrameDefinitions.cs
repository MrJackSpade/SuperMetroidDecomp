using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Named background-layer pictures of the 42 Crocomire fight-body frames drawn
/// partly as sprites and partly on the background layer. Their sprite components
/// are authored among the extended frames; hitboxes and animation timing remain
/// gameplay data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file; version 3 names each frame for its pose (#1465), 4 for the animation that first shows it (#1623).</summary>
    internal const int Version = 4;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>The 42 body frames that carry BG2 components, in the compiled frame order.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_first"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSecond, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_second"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopThird, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_third"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFourth, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_fourth"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFifth, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_fifth"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSixth, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_sixth"),
        new(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSeventh, "crocomire_body_power_bomb_reaction_mouth_not_open_loop_seventh"),
        new(EnemyFrameId.CrocomireBodySteppingBackFifth, "crocomire_body_stepping_back_fifth"),
        new(EnemyFrameId.CrocomireBodySteppingBackFourth, "crocomire_body_stepping_back_fourth"),
        new(EnemyFrameId.CrocomireBodySteppingBackThird, "crocomire_body_stepping_back_third"),
        new(EnemyFrameId.CrocomireBodySteppingBackSecond, "crocomire_body_stepping_back_second"),
        new(EnemyFrameId.CrocomireBodySteppingBackFirst, "crocomire_body_stepping_back_first"),
        new(EnemyFrameId.CrocomireBodyInitial, "crocomire_body_initial"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSixth, "crocomire_body_wait_for_first_second_damage_sixth"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFifth, "crocomire_body_wait_for_first_second_damage_fifth"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFourth, "crocomire_body_wait_for_first_second_damage_fourth"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageThird, "crocomire_body_wait_for_first_second_damage_third"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSecond, "crocomire_body_wait_for_first_second_damage_second"),
        new(EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFirst, "crocomire_body_wait_for_first_second_damage_first"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackFirst, "crocomire_body_projectile_attack_first"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackSecond, "crocomire_body_projectile_attack_second"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackThird, "crocomire_body_projectile_attack_third"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackFourth, "crocomire_body_projectile_attack_fourth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackFifth, "crocomire_body_projectile_attack_fifth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackSixth, "crocomire_body_projectile_attack_sixth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackSeventh, "crocomire_body_projectile_attack_seventh"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackEighth, "crocomire_body_projectile_attack_eighth"),
        new(EnemyFrameId.CrocomireBodyStepForwardAfterDelay, "crocomire_body_step_forward_after_delay"),
        new(EnemyFrameId.CrocomireBodyStepForwardFourth, "crocomire_body_step_forward_fourth"),
        new(EnemyFrameId.CrocomireBodyStepForwardFifth, "crocomire_body_step_forward_fifth"),
        new(EnemyFrameId.CrocomireBodyStepForwardSixth, "crocomire_body_step_forward_sixth"),
        new(EnemyFrameId.CrocomireBodyStepForwardSeventh, "crocomire_body_step_forward_seventh"),
        new(EnemyFrameId.CrocomireBodyStepForwardEighth, "crocomire_body_step_forward_eighth"),
        new(EnemyFrameId.CrocomireBodyStepForwardNinth, "crocomire_body_step_forward_ninth"),
        new(EnemyFrameId.CrocomireBodyStepForwardTenth, "crocomire_body_step_forward_tenth"),
        new(EnemyFrameId.CrocomireBodyStepForwardEleventh, "crocomire_body_step_forward_eleventh"),
        new(EnemyFrameId.CrocomireBodyStepForwardTwelfth, "crocomire_body_step_forward_twelfth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst, "crocomire_body_projectile_attack_loop_first"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond, "crocomire_body_projectile_attack_loop_second"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopThird, "crocomire_body_projectile_attack_loop_third"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth, "crocomire_body_projectile_attack_loop_fourth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth, "crocomire_body_projectile_attack_loop_fifth"),
    ];
}
