using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>Checks all 62 room sprite-object selectors and their executed bank $B4 instruction programs.</summary>
internal static class RoomSpriteObjectDefinitionsVerification
{
    /// <summary>Runs every native object identity until deletion, sleep, or loop while guarding compiled reads.</summary>
    internal static void VerifyRoomSpriteObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = NewRoomSpriteObjectDefinitionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        Func<RoomEnemySystem, Position16<RoomPixels>, RoomSpriteObjectKind, ushort, RoomSpriteObjectSlot?> spawn =
            (owner, position, kind, graphicsIndex) => owner.SpriteObjects.SpawnRoomSpriteObject(position, kind, graphicsIndex);
        Action<RoomEnemySystem> step = owner => owner.SpriteObjects.StepRoomSpriteObjects();

        for (int index = 0;
             index < RoomSpriteObjectInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RoomSpriteObjectInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb40000 | definition.Address)),
                definition.Value,
                $"room sprite-object mechanics word $B4:{definition.Address:X4}");
        }

        for (ushort objectNumber = 0; objectNumber <= 0x003d; objectNumber++)
        {
            var kind = (RoomSpriteObjectKind)objectNumber;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb4bda8 + objectNumber * 2));
            AssertEqual(expected, RoomSpriteObjectDefinitions.InstructionPointer(kind),
                $"room sprite object selector ${objectNumber:X2}");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            RoomSpriteObjectSlot? slot = spawn(
                enemies,
                new(unchecked((ushort)(0x1000 + objectNumber)), unchecked((ushort)(0x2000 + objectNumber))),
                kind,
                0x0600);
            AssertTrue(slot is not null,
                $"production room sprite object ${objectNumber:X2} allocated");
            AssertEqual(expected, (slot ?? throw new InvalidOperationException("Verification requires slot.")).InstructionPointer,
                $"production room sprite object ${objectNumber:X2} instruction");
            AssertTrue(slot.InstructionTimer != 0,
                $"production room sprite object ${objectNumber:X2} loaded first frame");
            AssertEqual(kind, slot.Kind,
                $"production room sprite object ${objectNumber:X2} identity");

            VerifySelectedFrame(slot);
            var observedStates = new HashSet<(ushort Pointer, ushort Timer)>();
            for (int frame = 0; frame < 8192 && slot.IsActive; frame++)
            {
                if (!observedStates.Add((slot.InstructionPointer, slot.InstructionTimer)))
                    break;
                step(enemies);
                VerifySelectedFrame(slot);
                if (slot.InstructionTimer == 0x7fff)
                    break;
            }
            AssertTrue(!slot.IsActive || slot.InstructionTimer == 0x7fff ||
                observedStates.Contains((slot.InstructionPointer, slot.InstructionTimer)),
                $"room sprite object ${objectNumber:X2} terminates or reaches its authored loop");
        }

        AssertEqual(0, guarded.ObservedWords.Count,
            "room sprite-object visuals require no runtime cartridge reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "room sprite-object execution avoids compiled mechanics bytes");
        AssertEqual(
            RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "every executed room sprite-object selector matches the cartridge");
        for (int index = 0;
             index < RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers room sprite-object presentation $B4:{address:X4}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(
                (RoomSpriteObjectKind)0x003e),
            "room sprite object selector past definitions");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(RoomSpriteObjectKind.None),
            "room sprite object none selector");
        AssertThrows<InvalidDataException>(
            () => RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(0)),
            "room sprite-object spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(0x8000),
            "foreign bank-B4 data is outside room sprite-object programs");

        _ = ProbeRoomSpriteObjectInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRoomSpriteObjectInstructionAllocation();
        AssertTrue(checksum != 0,
            "room sprite-object allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed room sprite-object mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Room sprite object definitions: all 62 native selectors and complete " +
            $"production programs pass with {RoomSpriteObjectInstructionProgramDefinitionsTooling.MechanicsWordCount} " +
            "compiled mechanics words, " +
            $"{RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount} executed " +
            "cartridge-matching selectors, strict rejection, and allocation-free lookup.");
        void VerifySelectedFrame(RoomSpriteObjectSlot selected)
        {
            if (!selected.IsActive || (selected.InstructionTimer & 0x8000) != 0)
                return;
            ushort operand = unchecked((ushort)(selected.InstructionPointer + 2));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb40000 | operand)),
                selected.SpritemapPointer,
                $"executed room sprite-object selector $B4:{operand:X4}");
            executedOperands.Add(operand);
        }
    }

    /// <summary>Repeats warmed sprite-object mechanics lookups for the caller's allocation measurement.</summary>
    private static int ProbeRoomSpriteObjectInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                RoomSpriteObjectDefinitions.InstructionPointer(RoomSpriteObjectKind.DustCloud));
        }
        return checksum;
    }

    /// <summary>Forbids migrated selector and mechanics reads while observing presentation operands.</summary>
    private static CartridgeReadGuard NewRoomSpriteObjectDefinitionReadGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xb4bda8 and < 0xb4be24,
                address => $"Room sprite object attempted migrated selector read ${address:X6}.")
            .Forbid(RoomSpriteObjectInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Room sprite object attempted compiled mechanics read ${address:X6}.");
        return guard.Observe(address => RoomSpriteObjectInstructionProgramDefinitions.TryGetPresentationWord(address, out _),
            address =>
            {
                RoomSpriteObjectInstructionProgramDefinitions.TryGetPresentationWord(address, out ushort presentation);
                guard.ObservedWords.Add(presentation);
            });
    }

    /// <summary>Dispatches the cartridge-backed room sprite-object verification command.</summary>
    [VerificationCommand("--room-sprite-object-definitions")]
    internal static int RoomSpriteObjectDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomSpriteObjectDefinitions), () => VerifyRoomSpriteObjectDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
