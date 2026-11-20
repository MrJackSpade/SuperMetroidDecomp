using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Elevator.ElevatorNativeListAddresses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>#641, #1600: the elevator's typed animations, each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class ElevatorAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyElevatorAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ElevatorAnimations.Segments, ElevatorAnimations.StartingAt, ElevatorFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  elevator animations: {Entries.Length} segments ({beats} beats) and {ElevatorAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
