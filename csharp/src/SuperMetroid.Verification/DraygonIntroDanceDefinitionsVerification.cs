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
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Draygon intro latency and movement definitions against ROM and the full Evir trajectory.</summary>
internal static class DraygonIntroDanceDefinitionsVerification
{
    /// <summary>Runs latency, movement, and complete production trajectory comparisons.</summary>
    internal static void VerifyDraygonIntroDanceDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(rom));
        Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(rom));
        Suite(nameof(VerifyCompleteDraygonIntroTrajectory), () => VerifyCompleteDraygonIntroTrajectory(rom));

        Console.WriteLine(
            "Draygon intro dance definitions: all four latency words and all 1,104 " +
            "reachable movement records match the pinned ROM; the complete 1,232-frame " +
            "production trajectory passes with migrated ROM reads forbidden.");
    }

    /// <summary>Compares all four Evir slot delays with native words and checks slot bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon intro latency definitions")]
    internal static void VerifyDraygonIntroLatencyDefinitions(SuperMetroidAddressSpace rom)
    {
        const int source = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            int address = source + (slotIndex - 28) * 2;
            short expected = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            AssertEqual(expected,
                DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex),
                $"Draygon intro Evir slot {slotIndex} latency");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(27),
            "Draygon intro Evir slot before native range");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(32),
            "Draygon intro Evir slot after native range");
        Console.WriteLine("Evir latency algorithm: all four signed native delays and slot bounds match.");
    }

    /// <summary>Compares reachable motion deltas and rejection of unaligned or out-of-range stream indices.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon intro movement definitions")]
    internal static void VerifyDraygonIntroMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDraygonIntroDeleteSelection), () => VerifyDraygonIntroDeleteSelection(rom));
        const int source = DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress;
        for (ushort offset = 0;
             offset <= DraygonIntroDanceDefinitions.LastMovementStreamOffset;
             offset += DraygonIntroDanceDefinitions.StreamIndexAdvance)
        {
            byte nativeX = rom.ReadCartridgeByte(source + offset);
            byte nativeY = rom.ReadCartridgeByte(source + offset + 1);
            DraygonIntroMovement actual =
                DraygonIntroDanceDefinitions.ResolveMovement(offset);

            bool expectedDelete = nativeX == 0x80 && nativeY == 0x80;
            AssertEqual(expectedDelete ? (short)0 : unchecked((sbyte)nativeX), actual.Delta.X,
                $"Draygon intro movement ${offset:X4} X delta");
            AssertEqual(expectedDelete ? (short)0 : unchecked((sbyte)nativeY), actual.Delta.Y,
                $"Draygon intro movement ${offset:X4} Y delta");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonIntroDanceDefinitions.ResolveMovement(1),
            "Draygon intro rejects a non-native unaligned restored stream index");
        AssertThrows<InvalidDataException>(
            () => DraygonIntroDanceDefinitions.ResolveMovement(
                unchecked((ushort)(DraygonIntroDanceDefinitions.LastMovementStreamOffset + 4))),
            "Draygon intro rejects a restored stream index beyond the compiled route");
    }

    /// <summary>Checks that the two native sentinel records delete sprites at the authored aligned offsets.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon intro delete selection")]
    private static void VerifyDraygonIntroDeleteSelection(SuperMetroidAddressSpace rom)
    {
        int commands = 0;
        for (int offset = 0; offset <= 0x113c; offset += 4)
        {
            bool expected = rom.ReadCartridgeByte(0xa5ce07 + offset) == 0x80 &&
                rom.ReadCartridgeByte(0xa5ce08 + offset) == 0x80;
            DraygonIntroMovement actual = DraygonIntroDanceDefinitions.ResolveMovement((ushort)offset);
            AssertEqual(expected, actual.DeletesSprite, "Draygon original intro delete command selection");
            if (expected) commands++;
        }
        AssertEqual(2, commands, "Draygon aligned intro has two native delete commands");
        foreach (ushort offset in new ushort[] { 1, 0x0fb9, 0x0fba, 0x0fbb, 0x0fbd, 0x1140, 0xffff })
            AssertThrows<InvalidDataException>(() => DraygonIntroDanceDefinitions.ResolveMovement(offset),
                "Draygon command selection preserves alignment and outer bounds");
    }

    /// <summary>Compares every Evir slot's position and lifetime across the full intro, including the fight handoff.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for complete Draygon intro trajectory")]
    private static void VerifyCompleteDraygonIntroTrajectory(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewDraygonIntroDefinitionReadGuard(rom));
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        Action<DraygonEnemyState, SamusState?, byte> run = enemies.DraygonBehavior.RunDraygonIntroDance;
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);

        ushort[] expectedX = new ushort[4];
        ushort[] expectedY = new ushort[4];
        bool[] expectedActive = new bool[4];
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[slotIndex];
            sprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
            sprite.InstructionPointer = 1;
            expectedX[slotIndex - 28] = 0x0100;
            sprite.Position = sprite.Position.WithWholeX(0x0100);
            expectedY[slotIndex - 28] = 0x0180;
            sprite.Position = sprite.Position.WithWholeY(0x0180);
            expectedActive[slotIndex - 28] = true;
        }

        ushort expectedStreamIndex = 0;
        for (int frame = 0; frame < DraygonIntroDanceDefinitions.DurationFrames; frame++)
        {
            for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
            {
                int actor = slotIndex - 28;
                ushort streamIndex = unchecked((ushort)(
                    expectedStreamIndex +
                    DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex)));
                if (unchecked((short)streamIndex) < 0 || !expectedActive[actor])
                    continue;

                byte xDelta = rom.ReadCartridgeByte(
                    DraygonIntroDanceDefinitions.NativeMovementStreamAddress + streamIndex);
                byte yDelta = rom.ReadCartridgeByte(
                    DraygonIntroDanceDefinitions.NativeMovementStreamAddress + streamIndex + 1);
                if (xDelta == 0x80 && yDelta == 0x80)
                {
                    expectedActive[actor] = false;
                    expectedX[actor] = 0;
                    expectedY[actor] = 0;
                    continue;
                }

                expectedX[actor] = unchecked((ushort)(
                    expectedX[actor] + unchecked((sbyte)xDelta)));
                expectedY[actor] = unchecked((ushort)(
                    expectedY[actor] + unchecked((sbyte)yDelta)));
            }

            run(state, null, 1);
            expectedStreamIndex = unchecked((ushort)(
                expectedStreamIndex + DraygonIntroDanceDefinitions.StreamIndexAdvance));

            AssertEqual(expectedStreamIndex, state.FightIntroDanceIndex,
                $"Draygon intro frame {frame} stream index");
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

        // A restored index can be corrupt while the owner is still inside its native
        // duration. Exercise the production dispatcher so this cannot regress into an
        // arbitrary bank-$A5 read hidden behind a catalog-only unit test.
        var malformedEnemies = new RoomEnemySystem();
        malformedEnemies.BindFixtureBus(NewDraygonIntroDefinitionReadGuard(rom));
        Action<DraygonEnemyState, SamusState?, byte> malformedRun = malformedEnemies.DraygonBehavior.RunDraygonIntroDance;
        var malformedState = new DraygonEnemyState(malformedEnemies.SlotTable.Slots[0])
        {
            // Slot 31 subtracts $0200, selecting invalid offset one immediately.
            FightIntroDanceIndex = 0x0201,
        };
        RoomSpriteObjectSlot malformedSprite = malformedEnemies.RoomSpriteObjects[31];
        malformedSprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
        malformedSprite.InstructionPointer = 1;
        AssertThrows<InvalidDataException>(
            () => malformedRun(malformedState, null, 1),
            "Production Draygon intro rejects malformed restored movement without a ROM fallback");
    }

    /// <summary>Rejects runtime reads from the compiled Evir latency and movement ranges.</summary>
    private static CartridgeReadGuard NewDraygonIntroDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    address is >= 0xa5a19f and < 0xa5a1a7 ||
                    address >= DraygonIntroDanceDefinitions.NativeMovementStreamAddress &&
                    address <= DraygonIntroDanceDefinitions.NativeMovementStreamAddress +
                        DraygonIntroDanceDefinitions.LastMovementStreamOffset + 1,
                address => $"Draygon intro attempted migrated definition read ${address:X6}.");

    /// <summary>Runs the Draygon intro dance suite through the verification command dispatcher.</summary>
    [VerificationCommand("--draygon-intro-dance-definitions")]
    internal static int DraygonIntroDanceDefinitionsCommand()
    {
        Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
