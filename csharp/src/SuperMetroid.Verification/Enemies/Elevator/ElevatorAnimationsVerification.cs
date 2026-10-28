using static SuperMetroid.Verification.Enemies.Elevator.ElevatorNativeListAddresses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>#641: the elevator's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class ElevatorAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyElevatorAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ElevatorAnimations.Programs,
            ElevatorFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  elevator programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
