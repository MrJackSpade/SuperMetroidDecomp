using SuperMetroid.Core;
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
        var runFall = new Action<RoomEnemyPlace, ChootEnemyState>((fixturePlace, fixtureArg1) => ChootBehavior.RunChootFall(fixturePlace.HeldOccupant(), fixturePlace, fixtureArg1));
        int[] frameCounts = [74, 74, 74, 94, 134];

        for (ushort patternIndex = 0; patternIndex < 5; patternIndex++)
        {
            ChootFallingPath fallingPath = DecodedFallingPath(patternIndex);
            int loopYDistance = ChootFalls.LoopYDistance(fallingPath);
            ushort nativePointer = rom.ReadCartridgeWord((0xa2df5e + patternIndex * 2));
            ushort distancePointer = rom.ReadCartridgeWord((0xa2df6a + patternIndex * 2));
            AssertEqual(nativePointer, ChootNativeWords.ChootFallingPaths.Word(fallingPath),
                $"Choot pattern pointer {patternIndex}");
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | distancePointer)),
                loopYDistance,
                $"Choot pattern distance {patternIndex}");

            int pathAddress = 0xa20000 | ChootNativeWords.ChootFallingPaths.Word(fallingPath);
            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                Offset16<RoomPixels> point = ChootFallLoopAccess.OffsetAt(
                    fallingPath,
                    frameIndex) ?? throw new InvalidDataException($"Choot path {patternIndex} ends early at {frameIndex}.");
                AssertEqual(rom.ReadCartridgeWord((pathAddress + frameIndex * 4)),
                    unchecked((ushort)point.X),
                    $"Choot path X {patternIndex}/{frameIndex}");
                AssertEqual(rom.ReadCartridgeWord((pathAddress + frameIndex * 4 + 2)),
                    unchecked((ushort)point.Y),
                    $"Choot path Y {patternIndex}/{frameIndex}");
            }

            // The native path ends each loop with a (-32768, -32768) pair; the port's path ends at that frame.
            int endFrame = frameCounts[patternIndex] - 1;
            AssertEqual((ushort)0x8000, rom.ReadCartridgeWord((pathAddress + endFrame * 4)),
                $"Choot path {patternIndex} native loop end X");
            AssertEqual((ushort)0x8000, rom.ReadCartridgeWord((pathAddress + endFrame * 4 + 2)),
                $"Choot path {patternIndex} native loop end Y");
            AssertTrue(ChootFallLoopAccess.AtAge(fallingPath, endFrame) is ChootFallLoop.Over, $"Choot path {patternIndex} ends its loop at the native end frame");

            AssertThrows<InvalidOperationException>(
                () => ChootFallLoopAccess.OffsetAt(
                    fallingPath,
                    frameCounts[patternIndex]),
                $"Choot path {patternIndex} rejects the frame after its loop's end");

            foreach (byte loopCount in new byte[] { 0, 1, 2, 3 })
            {
                var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
                var initialize = new Action<RoomEnemyPlace>(fixturePlace => ChootBehavior.InitializeChoot(fixturePlace.HeldOccupant().SpawnSettings<ChootSpawnSettings>(), fixturePlace));
                RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
                place.Bind(EnemyDefinitionId.Choot, place.KeptDefinition);
                place.Position = place.Position.WithWhole(new(0x3456, 0x789a));
                place.SetSpawnParameter1(EnemyDefinitionId.Choot, (ushort)((patternIndex << 8) | loopCount));
                initialize(place);

                ChootEnemyState state = (enemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ChootStates[0]."));
                ushort jumpHeight = unchecked((ushort)(
                    loopCount * (LittleEndianBytes.LowByte(loopYDistance))));
                AssertEqual(fallingPath, place.HeldOccupant().SpawnSettings<ChootSpawnSettings>().FallingPath,
                    $"Choot production pattern {patternIndex}/{loopCount}");
                AssertEqual(place.Position.X.Whole, state.InitialFallingPosition.X,
                    $"Choot production initial falling X {patternIndex}/{loopCount}");
                AssertEqual(unchecked((ushort)(0x789a - jumpHeight)),
                    state.InitialFallingPosition.Y,
                    $"Choot production initial falling Y {patternIndex}/{loopCount}");
            }

            var fallEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var initializeFall = new Action<RoomEnemyPlace>(fixturePlace => ChootBehavior.InitializeChoot(fixturePlace.HeldOccupant().SpawnSettings<ChootSpawnSettings>(), fixturePlace));
            RoomEnemyPlace fallPlace = fallEnemies.Places.InUpdateOrder[0];
            fallPlace.Bind(EnemyDefinitionId.Choot, fallPlace.KeptDefinition);
            fallPlace.Position = new(new(0x3456, 0x1111), new(0x789a, 0x2222));
            fallPlace.SetSpawnParameter1(EnemyDefinitionId.Choot, (ushort)((patternIndex << 8) | 2));
            initializeFall(fallPlace);
            ChootEnemyState fallState = (fallEnemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires fallEnemies.ChootStates[0]."));
            fallState.FallingOrigin = new(0x4000, 0x5000);
            fallState.FallingLoop = ChootFallLoop.Starting(fallingPath);
            fallState.FallingPatternLoopCounter = 2;

            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                Offset16<RoomPixels> point = ChootFallLoopAccess.OffsetAt(
                    fallingPath,
                    frameIndex) ?? throw new InvalidDataException($"Choot path {patternIndex} ends early at {frameIndex}.");
                runFall(fallPlace, fallState);
                AssertEqual(unchecked((ushort)(0x4000 + point.X)),
                    fallPlace.Position.X.Whole,
                    $"Choot production fall X {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)(0x5000 + point.Y)),
                    fallPlace.Position.Y.Whole,
                    $"Choot production fall Y {patternIndex}/{frameIndex}");
                AssertEqual(ChootFallLoopAccess.OffsetAt(fallingPath, frameIndex + 1),
                    ChootFallLoopAccess.NextOffsetOf(fallState.FallingLoop),
                    $"Choot production fall moves on to its path's next update {patternIndex}/{frameIndex}");
            }

            runFall(fallPlace, fallState);
            AssertEqual(ChootFallLoopAccess.OffsetAt(fallingPath, 0), ChootFallLoopAccess.NextOffsetOf(fallState.FallingLoop),
                $"Choot production loop starts its path again {patternIndex}");
            AssertEqual(1, fallState.FallingPatternLoopCounter,
                $"Choot production loop counter {patternIndex}");
            AssertEqual(unchecked((ushort)(0x5000 + loopYDistance)),
                fallState.FallingOrigin.Y,
                $"Choot production loop Y advance {patternIndex}");

            fallState.FallingLoop = ChootFallLoopAccess.AtAge(fallingPath, frameCounts[patternIndex] - 1);
            runFall(fallPlace, fallState);
            AssertEqual((ushort)0x3456, fallPlace.Position.X.Whole,
                $"Choot production final spawn X {patternIndex}");
            AssertEqual((ushort)0x789a, fallPlace.Position.Y.Whole,
                $"Choot production final spawn Y {patternIndex}");
            AssertEqual((ushort)0, fallPlace.Position.X.Fraction,
                $"Choot production final spawn X fraction {patternIndex}");
            AssertEqual((ushort)0, fallPlace.Position.Y.Fraction,
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
