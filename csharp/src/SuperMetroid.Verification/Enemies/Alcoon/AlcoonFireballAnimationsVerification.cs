using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#641: Alcoon's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class AlcoonFireballAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, AlcoonFireballAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(AlcoonFireballFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyAlcoonFireballAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        for (int pose = 0; pose < AlcoonFireballFrameToolingDefinitions.PoseCount; pose++)
        {
            AlcoonFireballFrame frame = new(pose);
            ushort key = EnemyProjectileProgramFrameSources.Operand(AlcoonFireballFrameDefinitions.ShownProjectileFrames[frame]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), AlcoonFireballFrameToolingDefinitions.Spritemap(frame),
                $"Alcoon fireball pose {pose} spritemap");
            AssertTrue(AlcoonFireballFrameToolingDefinitions.TryFrame(key, out AlcoonFireballFrame found) && found == frame, $"Alcoon fireball pose {pose} is found by its key");
        }
        Console.WriteLine($"  Alcoon fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
