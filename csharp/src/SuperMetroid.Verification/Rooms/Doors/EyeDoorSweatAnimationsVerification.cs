using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorSweatNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1296: the eye-door sweat's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EyeDoorSweatAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EyeDoorSweatAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(EyeDoorSweatFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEyeDoorSweatAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  eye-door sweat programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
