using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#641: Fune and Namihe's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FuneNamiheFireballAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, FuneNamiheFireballAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFuneNamiheFireballAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        for (int pose = 0; pose < FuneNamiheFireballFrameToolingDefinitions.PoseCount; pose++)
        {
            FuneNamiheFireballFrame frame = new(pose);
            ushort key = EnemyProjectileFrames.ProgramFrameKey(FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[frame]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), FuneNamiheFireballFrameToolingDefinitions.Spritemap(frame),
                $"Fune/Namihe fireball pose {pose} spritemap");
            AssertTrue(FuneNamiheFireballFrameToolingDefinitions.TryFrame(key, out FuneNamiheFireballFrame found) && found == frame, $"Fune/Namihe fireball pose {pose} is found by its key");
        }
        Console.WriteLine($"  Fune/Namihe fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
