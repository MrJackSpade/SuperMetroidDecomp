using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Choot path sources, initializers, fall loops, and invalid pattern indexes.</summary>
internal static class ChootPatternDefinitionsVerification
{
    /// <summary>Checks five Choot path sources, initializers, complete fall loops, and invalid pattern indexes.</summary>
    internal static void VerifyChootPatternDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        var runFall = new Action<RoomEnemySlot, ChootEnemyState>(ChootBehavior.RunChootFall);
        int[] frameCounts = [74, 74, 74, 94, 134];

        for (ushort patternIndex = 0; patternIndex < 5; patternIndex++)
        {
            ChootPatternDefinition definition = ChootPatternDefinitions.ForIndex(patternIndex);
            ushort nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df5e + patternIndex * 2));
            ushort distancePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df6a + patternIndex * 2));
            AssertEqual(nativePointer, (ushort)definition.FallingPatternPointer,
                $"Choot pattern pointer {patternIndex}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | distancePointer)),
                definition.FallingPatternYDistance,
                $"Choot pattern distance {patternIndex}");

            int pathAddress = 0xa20000 | (ushort)definition.FallingPatternPointer;
            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex]; frameIndex++)
            {
                ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameIndex);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + frameIndex * 4)),
                    unchecked((ushort)point.Offset.X),
                    $"Choot path X {patternIndex}/{frameIndex}");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + frameIndex * 4 + 2)),
                    unchecked((ushort)point.Offset.Y),
                    $"Choot path Y {patternIndex}/{frameIndex}");
            }

            AssertThrows<InvalidDataException>(
                () => ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameCounts[patternIndex]),
                $"Choot path {patternIndex} rejects its first post-terminator frame");

            foreach (byte loopCount in new byte[] { 0, 1, 2, 3 })
            {
                var enemies = new RoomEnemySystem();
                enemies.BindFixtureBus(NewChootPatternReadGuard(rom));
                var initialize = new Action<RoomEnemySlot>(enemies.ChootBehavior.InitializeChoot);
                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                slot.Position = slot.Position.WithWhole(new(0x3456, 0x789a));
                slot.Parameter1 = (ushort)((patternIndex << 8) | loopCount);
                initialize(slot);

                ChootEnemyState state = (enemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ChootStates[0]."));
                ushort jumpHeight = unchecked((ushort)(
                    loopCount * (SnesLittleEndian.LowByte(definition.FallingPatternYDistance))));
                AssertEqual((ushort)definition.FallingPatternPointer, (state ?? throw new InvalidOperationException("Verification requires state.")).FallingPatternPointer,
                    $"Choot production pattern pointer {patternIndex}/{loopCount}");
                AssertEqual(definition.FallingPatternYDistance, state.FallingPatternYDistance,
                    $"Choot production pattern distance {patternIndex}/{loopCount}");
                AssertEqual(slot.Position.X.Whole, state.InitialFallingPosition.X,
                    $"Choot production initial falling X {patternIndex}/{loopCount}");
                AssertEqual(unchecked((ushort)(0x789a - jumpHeight)),
                    state.InitialFallingPosition.Y,
                    $"Choot production initial falling Y {patternIndex}/{loopCount}");
            }

            var fallEnemies = new RoomEnemySystem();
            fallEnemies.BindFixtureBus(NewChootPatternReadGuard(rom));
            var initializeFall = new Action<RoomEnemySlot>(fallEnemies.ChootBehavior.InitializeChoot);
            RoomEnemySlot fallSlot = fallEnemies.SlotTable.Slots[0];
            fallSlot.Position = new(new(0x3456, 0x1111), new(0x789a, 0x2222));
            fallSlot.Parameter1 = (ushort)((patternIndex << 8) | 2);
            initializeFall(fallSlot);
            ChootEnemyState fallState = (fallEnemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires fallEnemies.ChootStates[0]."));
            (fallState ?? throw new InvalidOperationException("Verification requires fallState.")).FallingOrigin = new(0x4000, 0x5000);
            fallState.FallingPatternIndex = 0;
            fallState.FallingPatternLoopCounter = NativeCountdown.Start(1);

            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameIndex);
                runFall(fallSlot, fallState);
                AssertEqual(unchecked((ushort)(0x4000 + point.Offset.X)),
                    fallSlot.Position.X.Whole,
                    $"Choot production fall X {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)(0x5000 + point.Offset.Y)),
                    fallSlot.Position.Y.Whole,
                    $"Choot production fall Y {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)((frameIndex + 1) << 8)),
                    fallState.FallingPatternIndex,
                    $"Choot production fall cursor {patternIndex}/{frameIndex}");
            }

            runFall(fallSlot, fallState);
            AssertEqual((ushort)0, fallState.FallingPatternIndex,
                $"Choot production loop cursor reset {patternIndex}");
            AssertEqual((ushort)0, fallState.FallingPatternLoopCounter.RawValue,
                $"Choot production loop counter {patternIndex}");
            AssertEqual(unchecked((ushort)(0x5000 + definition.FallingPatternYDistance)),
                fallState.FallingOrigin.Y,
                $"Choot production loop Y advance {patternIndex}");

            fallState.FallingPatternIndex = unchecked((ushort)(
                (frameCounts[patternIndex] - 1) << 8));
            runFall(fallSlot, fallState);
            AssertEqual((ushort)0x3456, fallSlot.Position.X.Whole,
                $"Choot production final spawn X {patternIndex}");
            AssertEqual((ushort)0x789a, fallSlot.Position.Y.Whole,
                $"Choot production final spawn Y {patternIndex}");
            AssertEqual((ushort)0, fallSlot.Position.X.Fraction,
                $"Choot production final spawn X fraction {patternIndex}");
            AssertEqual((ushort)0, fallSlot.Position.Y.Fraction,
                $"Choot production final spawn Y fraction {patternIndex}");
        }

        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(5),
            "sixth Choot pointer-table alias is not an authored pattern");
        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(ushort.MaxValue),
            "restored Choot pattern index does not wrap into the catalog");
        AssertThrows<InvalidDataException>(
            () => ChootFallingPathDefinitions.At(0xffff, 0),
            "restored Choot falling pointer does not read arbitrary bank-A2 data");
        Console.WriteLine(
            "Choot pattern definitions: five selectors/distances, all 450 physical path frames, twenty initializers, and five complete fall loops pass with every source read forbidden.");
    }

    /// <summary>Forbids Choot's bank-$A2 falling paths and pattern selector tables during live behavior.</summary>
    private static CartridgeReadGuard NewChootPatternReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa2d84c and < 0xa2df74,
                address => $"Choot attempted migrated pattern-definition read ${address:X6}.");

    /// <summary>Runs the Choot pattern definition verification suite.</summary>
    [VerificationCommand("--choot-pattern-definitions")]
    internal static int ChootPatternDefinitionsCommand()
    {
        Suite(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
