using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGunshipInstructionProgramDefinitions()
    {
        VerifyGunshipInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyGunshipInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GunshipInstructionMechanicsWord definition =
                GunshipInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadGunshipWord(rom, 0xa20000 | definition.Address),
                $"gunship mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new GunshipInstructionReadGuard(rom);
        VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadOpening,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 123,
            expectedCursor: 0xa5ea,
            expectedTimer: 4,
            expectedSpritemap: 0xaf9d,
            "opening pad reaches its open loop");
        VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadClosing,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 79,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closing pad falls through to its closed loop");
        VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomEntrancePad,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 9,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closed pad loops");
        VerifyProgram(
            GunshipInstructionProgramDefinitions.TopHull,
            GunshipEnemyDefinitions.Top,
            frames: 2,
            expectedCursor: 0xa61a,
            expectedTimer: 0,
            expectedSpritemap: 0xad81,
            "top hull sleeps on its static frame");
        VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomHull,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 2,
            expectedCursor: 0xa620,
            expectedTimer: 0,
            expectedSpritemap: 0xaddd,
            "bottom hull sleeps on its static frame");

        VerifyInitializers();

        AssertEqual(
            GunshipInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all gunship spritemap words remain live cartridge reads");
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads gunship presentation $A2:{address:X4}");
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
            "program paths, both real initializer identities, and 22 live spritemap " +
            "reads pass with mechanics bytes forbidden.");

        void VerifyProgram(
            ushort program,
            ushort enemyDefinition,
            int frames,
            ushort expectedCursor,
            ushort expectedTimer,
            ushort expectedSpritemap,
            string assertion)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = enemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;

            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);

            AssertEqual(expectedCursor, slot.CurrentInstruction, assertion + " cursor");
            AssertEqual(expectedTimer, slot.InstructionTimer, assertion + " timer");
            AssertEqual(expectedSpritemap, slot.SpritemapPointer,
                assertion + " spritemap");
        }

        static void VerifyInitializers()
        {
            var enemies = new RoomEnemySystem();
            RoomEnemySlot top = enemies.Slots[0];
            top.EnemyDefinitionPointer = GunshipEnemyDefinitions.Top;
            top.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            top.YPosition = 0x0200;
            typeof(RoomEnemySystem).GetMethod("InitializeGunshipTop", flags)!
                .Invoke(enemies, [top]);
            AssertEqual(GunshipInstructionProgramDefinitions.TopHull, top.CurrentInstruction,
                "real gunship-top initializer installs compiled top program");

            RoomEnemySlot bottom = enemies.Slots[1];
            bottom.EnemyDefinitionPointer = GunshipEnemyDefinitions.BottomEntrance;
            bottom.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            bottom.YPosition = 0x0200;
            bottom.Parameter2 = 0;
            top.VramTilesIndex = 0x1234;
            typeof(RoomEnemySystem).GetMethod("InitializeGunshipBottom", flags)!
                .Invoke(enemies, [bottom]);
            AssertEqual(GunshipInstructionProgramDefinitions.BottomHull,
                bottom.CurrentInstruction,
                "real gunship-bottom initializer installs compiled bottom program");

            RoomEnemySlot pad = enemies.Slots[2];
            pad.EnemyDefinitionPointer = GunshipEnemyDefinitions.BottomEntrance;
            pad.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            pad.Parameter2 = 1;
            typeof(RoomEnemySystem).GetMethod("InitializeGunshipBottom", flags)!
                .Invoke(enemies, [pad]);
            AssertEqual(GunshipInstructionProgramDefinitions.BottomEntrancePad,
                pad.CurrentInstruction,
                "real entrance-pad initializer installs compiled closed program");
        }
    }

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

    private static ushort ReadGunshipWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class GunshipInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GunshipInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled gunship mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GunshipInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
