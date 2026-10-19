using static SuperMetroid.Verification.Enemies.Powamp.PowampSpikeNativeListAddresses;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#641: the Powamp spike's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class PowampSpikeAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, PowampSpikeAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(PowampSpikeFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPowampSpikeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Powamp spike programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
