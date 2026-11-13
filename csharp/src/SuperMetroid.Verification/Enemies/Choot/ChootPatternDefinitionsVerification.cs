using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Choot path sources, initializers, fall loops, and invalid pattern indexes.</summary>
internal static class ChootPatternDefinitionsVerification
{
    /// <summary>Checks five Choot path sources, initializers, complete fall loops, and invalid pattern indexes.</summary>
    internal static void VerifyChootPatternDefinitions(CartridgeImportAddressSpace rom)
    {
        var runFall = new Action<RoomEnemySlot, ChootEnemyState>(ChootBehavior.RunChootFall);
        int[] frameCounts = [74, 74, 74, 94, 134];

        for (ushort patternIndex = 0; patternIndex < 5; patternIndex++)
        {
            ChootFallingPath fallingPath = DecodedFallingPath(patternIndex);
            int loopYDistance = ChootFalls.LoopYDistance(fallingPath);
            ushort nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df5e + patternIndex * 2));
            ushort distancePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df6a + patternIndex * 2));
            AssertEqual(nativePointer, ChootNativeWords.ChootFallingPaths.Word(fallingPath),
                $"Choot pattern pointer {patternIndex}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | distancePointer)),
                loopYDistance,
                $"Choot pattern distance {patternIndex}");

            int pathAddress = 0xa20000 | ChootNativeWords.ChootFallingPaths.Word(fallingPath);
            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                Offset16<RoomPixels> point = ChootFalls.At(
                    fallingPath,
                    frameIndex) ?? throw new InvalidDataException($"Choot path {patternIndex} ends early at {frameIndex}.");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + frameIndex * 4)),
                    unchecked((ushort)point.X),
                    $"Choot path X {patternIndex}/{frameIndex}");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + frameIndex * 4 + 2)),
                    unchecked((ushort)point.Y),
                    $"Choot path Y {patternIndex}/{frameIndex}");
            }

            // The native path ends each loop with a (-32768, -32768) pair; the port's path ends at that frame.
            int endFrame = frameCounts[patternIndex] - 1;
            AssertEqual((ushort)0x8000, CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + endFrame * 4)),
                $"Choot path {patternIndex} native loop end X");
            AssertEqual((ushort)0x8000, CartridgeImportSource.Require(rom).ReadCartridgeWord((pathAddress + endFrame * 4 + 2)),
                $"Choot path {patternIndex} native loop end Y");
            AssertTrue(ChootFalls.At(fallingPath, endFrame) is null, $"Choot path {patternIndex} ends its loop at the native end frame");

            AssertThrows<InvalidDataException>(
                () => ChootFalls.At(
                    fallingPath,
                    frameCounts[patternIndex]),
                $"Choot path {patternIndex} rejects the frame after its loop's end");

            foreach (byte loopCount in new byte[] { 0, 1, 2, 3 })
            {
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
                var initialize = new Action<RoomEnemySlot>(ChootBehavior.InitializeChoot);
                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                slot.Bind(EnemyDefinitionId.Choot, slot.Definition);
                slot.Position = slot.Position.WithWhole(new(0x3456, 0x789a));
                slot.SetSpawnParameter1(EnemyDefinitionId.Choot, (ushort)((patternIndex << 8) | loopCount));
                initialize(slot);

                ChootEnemyState state = (enemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ChootStates[0]."));
                ushort jumpHeight = unchecked((ushort)(
                    loopCount * (LittleEndianBytes.LowByte(loopYDistance))));
                AssertEqual<ChootFallingPath?>(fallingPath, state.FallingPath,
                    $"Choot production pattern {patternIndex}/{loopCount}");
                AssertEqual(loopYDistance, state.FallingPatternYDistance,
                    $"Choot production pattern distance {patternIndex}/{loopCount}");
                AssertEqual(slot.Position.X.Whole, state.InitialFallingPosition.X,
                    $"Choot production initial falling X {patternIndex}/{loopCount}");
                AssertEqual(unchecked((ushort)(0x789a - jumpHeight)),
                    state.InitialFallingPosition.Y,
                    $"Choot production initial falling Y {patternIndex}/{loopCount}");
            }

            var fallEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var initializeFall = new Action<RoomEnemySlot>(ChootBehavior.InitializeChoot);
            RoomEnemySlot fallSlot = fallEnemies.SlotTable.Slots[0];
            fallSlot.Bind(EnemyDefinitionId.Choot, fallSlot.Definition);
            fallSlot.Position = new(new(0x3456, 0x1111), new(0x789a, 0x2222));
            fallSlot.SetSpawnParameter1(EnemyDefinitionId.Choot, (ushort)((patternIndex << 8) | 2));
            initializeFall(fallSlot);
            ChootEnemyState fallState = (fallEnemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires fallEnemies.ChootStates[0]."));
            fallState.FallingOrigin = new(0x4000, 0x5000);
            fallState.FallingPatternFrame = 0;
            fallState.FallingPatternLoopCounter = 1;

            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                Offset16<RoomPixels> point = ChootFalls.At(
                    fallingPath,
                    frameIndex) ?? throw new InvalidDataException($"Choot path {patternIndex} ends early at {frameIndex}.");
                runFall(fallSlot, fallState);
                AssertEqual(unchecked((ushort)(0x4000 + point.X)),
                    fallSlot.Position.X.Whole,
                    $"Choot production fall X {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)(0x5000 + point.Y)),
                    fallSlot.Position.Y.Whole,
                    $"Choot production fall Y {patternIndex}/{frameIndex}");
                AssertEqual(frameIndex + 1,
                    fallState.FallingPatternFrame,
                    $"Choot production fall cursor {patternIndex}/{frameIndex}");
            }

            runFall(fallSlot, fallState);
            AssertEqual(0, fallState.FallingPatternFrame,
                $"Choot production loop cursor reset {patternIndex}");
            AssertEqual(0, fallState.FallingPatternLoopCounter,
                $"Choot production loop counter {patternIndex}");
            AssertEqual(unchecked((ushort)(0x5000 + loopYDistance)),
                fallState.FallingOrigin.Y,
                $"Choot production loop Y advance {patternIndex}");

            fallState.FallingPatternFrame = frameCounts[patternIndex] - 1;
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
            () => DecodedFallingPath(5),
            "the import rejects the sixth Choot pointer-table alias, which is not an authored pattern");
        AssertThrows<InvalidDataException>(
            () => DecodedFallingPath(byte.MaxValue),
            "the import rejects a Choot pattern selector that does not wrap into the catalog");
        AssertThrows<InvalidDataException>(
            () => ChootFallingPathDefinitionsConstants.At(0xffff, 0),
            "restored Choot falling pointer does not read arbitrary bank-A2 data");
        Console.WriteLine(
            "Choot pattern definitions: five selectors/distances, all 450 physical path frames, twenty initializers, and five complete fall loops pass with every source read forbidden.");
    }


    /// <summary>The falling path the import decodes from a Choot's pattern selector, the first parameter's high byte.</summary>
    private static ChootFallingPath DecodedFallingPath(int patternIndex) =>
        ((ChootSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Choot, PopulationWords.Parameters(patternIndex << 8, 0))).FallingPath;

    /// <summary>Runs the Choot pattern definition verification suite.</summary>
    [VerificationCommand("--choot-pattern-definitions")]
    internal static int ChootPatternDefinitionsCommand()
    {
        Suite(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
