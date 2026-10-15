using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#641: Fune and Namihe's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FuneNamiheFireballAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFuneNamiheFireballAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, FuneNamiheFireballAnimations.Programs,
            FuneNamiheFireballFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        for (int pose = 0; pose < FuneNamiheFireballFrameDefinitions.PoseCount; pose++)
        {
            FuneNamiheFireballFrame frame = new(pose);
            ushort key = FuneNamiheFireballFrameDefinitions.PresentationKey(frame);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), FuneNamiheFireballFrameDefinitions.Spritemap(frame),
                $"Fune/Namihe fireball pose {pose} spritemap");
            AssertTrue(FuneNamiheFireballFrameDefinitions.TryFrame(key, out FuneNamiheFireballFrame found) && found == frame, $"Fune/Namihe fireball pose {pose} is found by its key");
        }
        Console.WriteLine($"  Fune/Namihe fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
