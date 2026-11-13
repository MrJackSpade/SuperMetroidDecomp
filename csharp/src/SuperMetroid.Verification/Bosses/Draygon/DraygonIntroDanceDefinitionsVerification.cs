using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon intro start delays and the traced path against ROM and the full Evir trajectory.</summary>
/// <remarks>
/// The cartridge keeps each Evir's delay as a negative byte distance into a path of four-byte records; the game holds
/// delays in updates and the path as typed steps. This oracle owns that translation.
/// </remarks>
internal static class DraygonIntroDanceDefinitionsVerification
{
    /// <summary>The cartridge record of path step <paramref name="step"/>: its X and Y bytes.</summary>
    private static NativeIntroRecord ReadRecord(CartridgeImportAddressSpace rom, int step)
    {
        int address = DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress +
            step * DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes;
        return new(rom.ReadCartridgeByte(address), rom.ReadCartridgeByte(address + 1));
    }

    /// <summary>Runs delay, path, and complete production trajectory comparisons.</summary>
    internal static void VerifyDraygonIntroDanceDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(rom));
        Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(rom));
        Suite(nameof(VerifyCompleteDraygonIntroTrajectory), () => VerifyCompleteDraygonIntroTrajectory(rom));

        Console.WriteLine(
            "Draygon intro dance definitions: all four start delays and all 1,104 " +
            "reachable path steps match the pinned ROM; the complete 1,232-frame " +
            "production trajectory passes.");
    }

    /// <summary>
    /// The native sprite slot the Evir spawned <paramref name="dancer"/>-th takes, which indexes its native start
    /// latency: spawns fill the free slots from slot 31 down.
    /// </summary>
    private static int NativeDancerSlot(int dancer) => 31 - dancer;

    /// <summary>Compares all four Evir start delays with the native byte latencies and checks the dancer bounds.</summary>
    internal static void VerifyDraygonIntroLatencyDefinitions(CartridgeImportAddressSpace rom)
    {
        const int source = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        for (int dancer = 0; dancer < DraygonIntroDanceDefinitions.EvirCount; dancer++)
        {
            int slotIndex = NativeDancerSlot(dancer);
            int address = source + (slotIndex - 28) * 2;
            short native = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            AssertEqual(native % DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes, 0,
                $"Draygon intro Evir slot {slotIndex} latency is whole records");
            AssertEqual(-native / DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes,
                DraygonIntroDance.StartDelay(dancer),
                $"Draygon intro Evir {dancer} (native slot {slotIndex}) start delay");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDance.StartDelay(-1),
            "Draygon intro Evir before the first spawned");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDance.StartDelay(DraygonIntroDanceDefinitions.EvirCount),
            "Draygon intro Evir after the last spawned");
        Console.WriteLine("Evir start delays: all four native latencies and dancer bounds match.");
    }

    /// <summary>Compares every step of the traced path, deletes included, and rejects steps outside it.</summary>
    internal static void VerifyDraygonIntroMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        int deletes = 0;
        for (int step = 0; step < DraygonIntroDanceDefinitions.TracedPath.Length; step++)
        {
            NativeIntroRecord native = ReadRecord(rom, step);
            DraygonIntroMovement actual = DraygonIntroDance.Movement(step);
            bool expectedDelete = native.X == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte &&
                native.Y == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte;
            AssertEqual(expectedDelete, actual.DeletesSprite, $"Draygon intro step {step} delete");
            AssertEqual(expectedDelete ? (short)0 : unchecked((sbyte)native.X), actual.Delta.X,
                $"Draygon intro step {step} X delta");
            AssertEqual(expectedDelete ? (short)0 : unchecked((sbyte)native.Y), actual.Delta.Y,
                $"Draygon intro step {step} Y delta");
            if (expectedDelete)
                deletes++;
        }
        AssertEqual(2, deletes, "Draygon intro path has two native delete records");

        // The last Evir's delay and the dance's duration reach exactly the path's last step.
        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames - 1 - DraygonIntroDance.StartDelay(0),
            DraygonIntroDanceDefinitions.TracedPath.Length - 1, "Draygon intro path ends at the last reachable step");
        foreach (int step in new[] { -1, DraygonIntroDanceDefinitions.TracedPath.Length, int.MaxValue })
            AssertThrows<InvalidDataException>(() => DraygonIntroDance.Movement(step),
                "Draygon intro rejects a step outside the traced path");
    }

    /// <summary>Compares every Evir slot's position and lifetime across the full intro, including the fight handoff.</summary>
    private static void VerifyCompleteDraygonIntroTrajectory(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        Action<DraygonEnemyState, SamusState?, byte> run = (danceState, danceSamus, frameCounter) =>
            enemies.DraygonBehavior.Intro.RunDraygonIntroDance(danceState, danceSamus, frameCounter);
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);

        ushort[] expectedX = new ushort[4];
        ushort[] expectedY = new ushort[4];
        bool[] expectedActive = new bool[4];
        // Each dancer is the Evir spawned in its turn; the native slot it would take indexes its expected values.
        var dancers = new RoomSpriteObject[DraygonIntroDanceDefinitions.EvirCount];
        for (int dancer = 0; dancer < DraygonIntroDanceDefinitions.EvirCount; dancer++)
        {
            dancers[dancer] = enemies.SpriteObjects.SpawnRoomSpriteObject(new(0x0100, 0x0180), RoomSpriteObjectKind.DraygonIntroEvir,
                SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(DraygonColours.Evirs));
            int actor = NativeDancerSlot(dancer) - 28;
            expectedX[actor] = 0x0100;
            expectedY[actor] = 0x0180;
            expectedActive[actor] = true;
            state.IntroDancers.Add(new DraygonIntroDancer(dancers[dancer], DraygonIntroDance.StartDelay(dancer)));
        }

        const int latencySource = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        int expectedStreamOffset = 0;
        for (int frame = 0; frame < DraygonIntroDanceDefinitions.DurationFrames; frame++)
        {
            for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
            {
                int actor = slotIndex - 28;
                short latency = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(latencySource + actor * 2));
                int streamOffset = expectedStreamOffset + latency;
                if (streamOffset < 0 || !expectedActive[actor])
                    continue;

                NativeIntroRecord record = ReadRecord(rom, streamOffset / DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes);
                if (record.X == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte &&
                    record.Y == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte)
                {
                    expectedActive[actor] = false;
                    continue;
                }

                expectedX[actor] = unchecked((ushort)(expectedX[actor] + unchecked((sbyte)record.X)));
                expectedY[actor] = unchecked((ushort)(expectedY[actor] + unchecked((sbyte)record.Y)));
            }

            run(state, null, 1);
            expectedStreamOffset += DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes;

            AssertEqual(frame + 1, state.IntroDanceStep, $"Draygon intro frame {frame} dance step");
            for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
            {
                int actor = slotIndex - 28;
                RoomSpriteObject actual = dancers[31 - slotIndex];
                AssertEqual(expectedActive[actor], actual.IsActive,
                    $"Draygon intro frame {frame} slot {slotIndex} active state");
                // An Evir the dance deleted has ended; where it stood no longer shows.
                if (!expectedActive[actor])
                    continue;
                AssertEqual(expectedX[actor], actual.Position.X.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} X");
                AssertEqual(expectedY[actor], actual.Position.Y.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} Y");
            }
        }

        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames, state.IntroFrames,
            "Draygon intro exact native duration");
        run(state, null, 1);
        AssertEqual(DraygonAiFunction.SwoopRightSetup, state.Function,
            "Draygon intro transitions to right-swoop setup after the native duration");
        AssertEqual(0, state.IntroFrames,
            "Draygon intro clears its frame count at the native handoff");

        // A restored dance step can be corrupt while the owner is still inside its native duration. Exercise the
        // production dispatcher so this cannot regress into reading past the traced path.
        var malformedEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<DraygonEnemyState, SamusState?, byte> malformedRun = (danceState, danceSamus, frameCounter) =>
            malformedEnemies.DraygonBehavior.Intro.RunDraygonIntroDance(danceState, danceSamus, frameCounter);
        var malformedState = new DraygonEnemyState(malformedEnemies.SlotTable.Slots[0])
        {
            // The first Evir waits the shortest, so it reaches one step past the path's end first.
            IntroDanceStep = DraygonIntroDanceDefinitions.TracedPath.Length + DraygonIntroDance.StartDelay(0),
        };
        RoomSpriteObject malformedSprite = malformedEnemies.SpriteObjects.SpawnRoomSpriteObject(new(0x0100, 0x0180),
            RoomSpriteObjectKind.DraygonIntroEvir, SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(DraygonColours.Evirs));
        malformedState.IntroDancers.Add(new DraygonIntroDancer(malformedSprite, DraygonIntroDance.StartDelay(0)));
        AssertThrows<InvalidDataException>(
            () => malformedRun(malformedState, null, 1),
            "Production Draygon intro rejects a restored step past the traced path");
    }


    /// <summary>Runs the Draygon intro dance suite through the verification command dispatcher.</summary>
    [VerificationCommand("--draygon-intro-dance-definitions")]
    internal static int DraygonIntroDanceDefinitionsCommand()
    {
        Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>The two movement bytes of one cartridge intro path record.</summary>
    /// <param name="X">The signed X movement byte.</param>
    /// <param name="Y">The signed Y movement byte.</param>
    private readonly record struct NativeIntroRecord(byte X, byte Y);
}
