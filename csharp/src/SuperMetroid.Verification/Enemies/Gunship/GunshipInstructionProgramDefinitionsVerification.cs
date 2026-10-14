using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;

/// <summary>Checks gunship hull and entrance-pad instruction programs against ROM and production initializers.</summary>
internal static class GunshipInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the gunship instruction suite.</summary>
    internal static void VerifyGunshipInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled control words, five hull and pad paths, initializer selection, and visual operands.</summary>
    internal static void VerifyGunshipInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                GunshipInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"gunship mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewGunshipInstructionGuard(rom);
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadOpening,
            EnemyDefinitionId.ShipBottomEntrance,
            frames: 123,
            expectedCursor: 0xa5ea,
            expectedTimer: 4,
            expectedSpritemap: 0xaf9d,
            "opening pad reaches its open loop"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadClosing,
            EnemyDefinitionId.ShipBottomEntrance,
            frames: 79,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closing pad falls through to its closed loop"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomEntrancePad,
            EnemyDefinitionId.ShipBottomEntrance,
            frames: 9,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closed pad loops"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.TopHull,
            EnemyDefinitionId.ShipTop,
            frames: 2,
            expectedCursor: 0xa61a,
            expectedTimer: 0,
            expectedSpritemap: 0xad81,
            "top hull sleeps on its static frame"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomHull,
            EnemyDefinitionId.ShipBottomEntrance,
            frames: 2,
            expectedCursor: 0xa620,
            expectedTimer: 0,
            expectedSpritemap: 0xaddd,
            "bottom hull sleeps on its static frame"));

        Suite(nameof(VerifyInitializers), () => VerifyInitializers());

        AssertEqual(0, guard.ObservedWords.Count, "compiled gunship visuals require no cartridge reads");
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa2, address, out ushort selector),
                "compiled gunship selector exists");
            AssertEqual(ReadVerificationWord(rom, (0xa2 << 16) | address), selector,
                "compiled gunship selector matches original operand");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled gunship mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GunshipInstructionProgramDefinitions.ReadMechanicsWord(0xa5c0),
            "gunship spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GunshipInstructionProgramDefinitions.ReadMechanicsWord(0xa622),
            "adjacent gunship brake table is rejected as mechanics");

        _ = ProbeGunshipInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGunshipInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "gunship allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed gunship mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Gunship instruction mechanics: 28 compiled words, all five hull/pad " +
            "program paths, both real initializer identities, and 22 native compiled spritemap " +
            "operands pass with mechanics bytes forbidden.");

        void VerifyProgram(
            ushort program,
            EnemyDefinitionId enemyDefinition,
            int frames,
            ushort expectedCursor,
            ushort expectedTimer,
            ushort expectedSpritemap,
            string assertion)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = enemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);

            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);

            AssertEqual(expectedCursor, slot.CurrentInstruction, assertion + " cursor");
            AssertEqual(expectedTimer, slot.InstructionTimer.RawValue, assertion + " timer");
            AssertEqual(expectedSpritemap, slot.SpritemapPointer,
                assertion + " spritemap");
        }

        static void VerifyInitializers()
        {
            var enemies = new RoomEnemySystem();
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            top.EnemyDefinitionPointer = EnemyDefinitionId.ShipTop;
            top.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            top.Position = top.Position.WithWholeY(0x0200);
            enemies.GunshipBehavior.InitializeGunshipTop(top);
            AssertEqual(GunshipInstructionProgramDefinitions.TopHull, top.CurrentInstruction,
                "real gunship-top initializer installs compiled top program");

            RoomEnemySlot bottom = enemies.SlotTable.Slots[1];
            bottom.EnemyDefinitionPointer = EnemyDefinitionId.ShipBottomEntrance;
            bottom.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            bottom.Position = bottom.Position.WithWholeY(0x0200);
            bottom.Parameter2 = 0;
            top.VramTilesIndex = 0x1234;
            enemies.GunshipBehavior.InitializeGunshipBottom(bottom);
            AssertEqual(GunshipInstructionProgramDefinitions.BottomHull,
                bottom.CurrentInstruction,
                "real gunship-bottom initializer installs compiled bottom program");

            RoomEnemySlot pad = enemies.SlotTable.Slots[2];
            pad.EnemyDefinitionPointer = EnemyDefinitionId.ShipBottomEntrance;
            pad.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            pad.Parameter2 = 1;
            enemies.GunshipBehavior.InitializeGunshipBottom(pad);
            AssertEqual(GunshipInstructionProgramDefinitions.BottomEntrancePad,
                pad.CurrentInstruction,
                "real entrance-pad initializer installs compiled closed program");
        }
    }

    /// <summary>Consumes repeated entrance-pad control lookups for the warmed allocation check.</summary>
    private static int ProbeGunshipInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GunshipInstructionProgramDefinitions.ReadMechanicsWord(
                GunshipInstructionProgramDefinitions.EntrancePadOpening);
        }
        return checksum;
    }

    /// <summary>Rejects compiled gunship mechanics reads and observes visual selector addresses.</summary>
    private static CartridgeReadGuard NewGunshipInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(GunshipInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled gunship mechanics ${address:X6}.")
            .ObserveWords(0xa20000, GunshipInstructionProgramDefinitions.PresentationWordCount, GunshipInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the gunship instruction suite through the verification command dispatcher.</summary>
    [VerificationCommand("--gunship-instruction-mechanics")]
    internal static int GunshipInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions());
        return 0;
    }
}
