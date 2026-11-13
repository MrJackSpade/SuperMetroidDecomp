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
    internal const int Version = 5;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>The 42 body frames that carry BG2 components, in the compiled frame order.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.CrocomireBodyChargingFirst, "crocomire_body_charging_first"),
        new(EnemyFrameId.CrocomireBodyChargingSecond, "crocomire_body_charging_second"),
        new(EnemyFrameId.CrocomireBodyChargingThird, "crocomire_body_charging_third"),
        new(EnemyFrameId.CrocomireBodyChargingFourth, "crocomire_body_charging_fourth"),
        new(EnemyFrameId.CrocomireBodyChargingFifth, "crocomire_body_charging_fifth"),
        new(EnemyFrameId.CrocomireBodyChargingSixth, "crocomire_body_charging_sixth"),
        new(EnemyFrameId.CrocomireBodyChargingSeventh, "crocomire_body_charging_seventh"),
        new(EnemyFrameId.CrocomireBodyChargingEighth, "crocomire_body_charging_eighth"),
        new(EnemyFrameId.CrocomireBodyChargingNinth, "crocomire_body_charging_ninth"),
        new(EnemyFrameId.CrocomireBodyChargingTenth, "crocomire_body_charging_tenth"),
        new(EnemyFrameId.CrocomireBodyChargingEleventh, "crocomire_body_charging_eleventh"),
        new(EnemyFrameId.CrocomireBodyChargingTwelfth, "crocomire_body_charging_twelfth"),
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
        new(EnemyFrameId.CrocomireBodySteppingForwardFirst, "crocomire_body_stepping_forward_first"),
        new(EnemyFrameId.CrocomireBodySteppingForwardSecond, "crocomire_body_stepping_forward_second"),
        new(EnemyFrameId.CrocomireBodySteppingForwardThird, "crocomire_body_stepping_forward_third"),
        new(EnemyFrameId.CrocomireBodySteppingForwardFourth, "crocomire_body_stepping_forward_fourth"),
        new(EnemyFrameId.CrocomireBodySteppingForwardFifth, "crocomire_body_stepping_forward_fifth"),
        new(EnemyFrameId.CrocomireBodySteppingForwardSixth, "crocomire_body_stepping_forward_sixth"),
        new(EnemyFrameId.CrocomireBodySteppingForwardSeventh, "crocomire_body_stepping_forward_seventh"),
        new(EnemyFrameId.CrocomireBodySteppingForwardEighth, "crocomire_body_stepping_forward_eighth"),
        new(EnemyFrameId.CrocomireBodySteppingForwardNinth, "crocomire_body_stepping_forward_ninth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst, "crocomire_body_projectile_attack_loop_first"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond, "crocomire_body_projectile_attack_loop_second"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopThird, "crocomire_body_projectile_attack_loop_third"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth, "crocomire_body_projectile_attack_loop_fourth"),
        new(EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth, "crocomire_body_projectile_attack_loop_fifth"),
    ];
}
