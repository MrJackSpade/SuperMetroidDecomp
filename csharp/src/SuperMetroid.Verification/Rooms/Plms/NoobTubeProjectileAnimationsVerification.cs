using static SuperMetroid.Verification.Rooms.Plms.NoobTubeProjectileNativeListAddresses;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#641: the n00b tube's crack, shards and bubbles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NoobTubeProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNoobTubeProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, NoobTubeProjectileAnimations.Programs,
            NoobTubeProjectileFrameDefinitions.PresentationKey, NoobTubeProjectileCueRules.Rules,
            reflectedPresentationKey: frame => frame.ReflectedPose is int pose ? NoobTubeProjectileFrameDefinitions.PresentationKey(new(pose)) : null);
        Console.WriteLine($"  n00b tube projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
