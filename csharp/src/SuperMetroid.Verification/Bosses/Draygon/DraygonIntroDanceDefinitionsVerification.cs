using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon intro start delays and the traced path against ROM and the full Evir trajectory.</summary>
/// <remarks>
/// The cartridge keeps each Evir's delay as a negative byte distance into a path of four-byte records; the game holds
/// delays in updates and the path as typed steps. This oracle owns that translation.
/// </remarks>
internal static class DraygonIntroDanceDefinitionsVerification
{
    /// <summary>The cartridge record of path step <paramref name="step"/>: its X and Y bytes.</summary>
    private static NativeIntroRecord ReadRecord(SuperMetroidAddressSpace rom, int step)
    {
        int address = DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress +
            step * DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes;
        return new(rom.ReadCartridgeByte(address), rom.ReadCartridgeByte(address + 1));
    }

    /// <summary>Runs delay, path, and complete production trajectory comparisons.</summary>
    internal static void VerifyDraygonIntroDanceDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(rom));
        Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(rom));
        Suite(nameof(VerifyCompleteDraygonIntroTrajectory), () => VerifyCompleteDraygonIntroTrajectory(rom));

        Console.WriteLine(
            "Draygon intro dance definitions: all four start delays and all 1,104 " +
            "reachable path steps match the pinned ROM; the complete 1,232-frame " +
            "production trajectory passes.");
    }

    /// <summary>Compares all four Evir start delays with the native byte latencies and checks slot bounds.</summary>
    internal static void VerifyDraygonIntroLatencyDefinitions(SuperMetroidAddressSpace rom)
    {
        const int source = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            int address = source + (slotIndex - 28) * 2;
            short native = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            AssertEqual(native % DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes, 0,
                $"Draygon intro Evir slot {slotIndex} latency is whole records");
            AssertEqual(-native / DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes,
                DraygonIntroDance.StartDelayForSlot(slotIndex),
                $"Draygon intro Evir slot {slotIndex} start delay");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDance.StartDelayForSlot(27),
            "Draygon intro Evir slot before native range");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDance.StartDelayForSlot(32),
            "Draygon intro Evir slot after native range");
        Console.WriteLine("Evir start delays: all four native latencies and slot bounds match.");
    }

    /// <summary>Compares every step of the traced path, deletes included, and rejects steps outside it.</summary>
    internal static void VerifyDraygonIntroMovementDefinitions(SuperMetroidAddressSpace rom)
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
        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames - 1 - DraygonIntroDance.StartDelayForSlot(31),
            DraygonIntroDanceDefinitions.TracedPath.Length - 1, "Draygon intro path ends at the last reachable step");
        foreach (int step in new[] { -1, DraygonIntroDanceDefinitions.TracedPath.Length, int.MaxValue })
            AssertThrows<InvalidDataException>(() => DraygonIntroDance.Movement(step),
                "Draygon intro rejects a step outside the traced path");
    }

    /// <summary>Compares every Evir slot's position and lifetime across the full intro, including the fight handoff.</summary>
    private static void VerifyCompleteDraygonIntroTrajectory(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        Action<DraygonEnemyState, SamusState?, byte> run = (danceState, danceSamus, frameCounterLow) =>
            enemies.DraygonBehavior.Intro.RunDraygonIntroDance(danceState, danceSamus, frameCounterLow);
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);

        ushort[] expectedX = new ushort[4];
        ushort[] expectedY = new ushort[4];
        bool[] expectedActive = new bool[4];
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[slotIndex];
            sprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
            sprite.Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectProgram.EvirFacingLeft);
            expectedX[slotIndex - 28] = 0x0100;
            sprite.Position = sprite.Position.WithWholeX(0x0100);
            expectedY[slotIndex - 28] = 0x0180;
            sprite.Position = sprite.Position.WithWholeY(0x0180);
            expectedActive[slotIndex - 28] = true;
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
                    expectedX[actor] = 0;
                    expectedY[actor] = 0;
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
                RoomSpriteObjectSlot actual = enemies.RoomSpriteObjects[slotIndex];
                AssertEqual(expectedActive[actor], actual.IsActive,
                    $"Draygon intro frame {frame} slot {slotIndex} active state");
                AssertEqual(expectedX[actor], actual.Position.X.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} X");
                AssertEqual(expectedY[actor], actual.Position.Y.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} Y");
            }
        }

        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames, state.FunctionTimer.RawValue,
            "Draygon intro exact native duration");
        run(state, null, 1);
        AssertEqual(DraygonAiFunction.SwoopRightSetup, state.Function,
            "Draygon intro transitions to right-swoop setup after the native duration");
        AssertEqual((ushort)0, state.FunctionTimer.RawValue,
            "Draygon intro clears the function timer at the native handoff");

        // A restored dance step can be corrupt while the owner is still inside its native duration. Exercise the
        // production dispatcher so this cannot regress into reading past the traced path.
        var malformedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<DraygonEnemyState, SamusState?, byte> malformedRun = (danceState, danceSamus, frameCounterLow) =>
            malformedEnemies.DraygonBehavior.Intro.RunDraygonIntroDance(danceState, danceSamus, frameCounterLow);
        var malformedState = new DraygonEnemyState(malformedEnemies.SlotTable.Slots[0])
        {
            // Slot 31 waits the shortest, so it reaches one step past the path's end first.
            IntroDanceStep = DraygonIntroDanceDefinitions.TracedPath.Length + DraygonIntroDance.StartDelayForSlot(31),
        };
        RoomSpriteObjectSlot malformedSprite = malformedEnemies.RoomSpriteObjects[31];
        malformedSprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
        malformedSprite.Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectProgram.EvirFacingLeft);
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
