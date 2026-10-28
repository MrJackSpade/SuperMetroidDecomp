using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Rooms.Plms.NoobTubeProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.NoobTube.Artwork;
using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#641: the n00b tube's crack, shards and bubbles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NoobTubeProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, NoobTubeProjectileAnimations.Programs, frame => EnemyProjectileProgramFrameSources.Operand(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(frame.Pose)]), NoobTubeProjectileCueRules.Rules, reflectedPresentationKey: frame => frame.ReflectedPose is int pose ? EnemyProjectileProgramFrameSources.Operand(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(pose)]) : null);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNoobTubeProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  n00b tube projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
