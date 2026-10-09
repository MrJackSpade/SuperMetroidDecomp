using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and verifies that gunship instruction mechanics match the compiled definitions.
    /// </summary>
    private static void VerifyGunshipInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled mechanics and visual selectors with ROM data, then exercises hull, pad, and initializer paths under a read guard.
    /// </summary>
    /// <param name="rom">The retail cartridge address space used to verify original mechanics and presentation operands.</param>
    private static void VerifyGunshipInstructionProgramDefinitions(
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
                ReadGunshipWord(rom, 0xa20000 | definition.Address),
                $"gunship mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new GunshipInstructionReadGuard(rom);
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadOpening,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 123,
            expectedCursor: 0xa5ea,
            expectedTimer: 4,
            expectedSpritemap: 0xaf9d,
            "opening pad reaches its open loop"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.EntrancePadClosing,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 79,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closing pad falls through to its closed loop"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomEntrancePad,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 9,
            expectedCursor: 0xa612,
            expectedTimer: 8,
            expectedSpritemap: 0xafdd,
            "closed pad loops"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.TopHull,
            GunshipEnemyDefinitions.Top,
            frames: 2,
            expectedCursor: 0xa61a,
            expectedTimer: 0,
            expectedSpritemap: 0xad81,
            "top hull sleeps on its static frame"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            GunshipInstructionProgramDefinitions.BottomHull,
            GunshipEnemyDefinitions.BottomEntrance,
            frames: 2,
            expectedCursor: 0xa620,
            expectedTimer: 0,
            expectedSpritemap: 0xaddd,
            "bottom hull sleeps on its static frame"));

        Suite(nameof(VerifyInitializers), () => VerifyInitializers());

        AssertEqual(0, guard.ObservedPresentationWords.Count, "compiled gunship visuals require no cartridge reads");
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

    /// <summary>
    /// Repeatedly reads a compiled gunship mechanics word to warm the lookup path for the allocation measurement.
    /// </summary>
    /// <returns>A non-zero checksum that consumes the lookup results.</returns>
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

    /// <summary>
    /// Reads two adjacent cartridge bytes and combines them as one little-endian instruction word.
    /// </summary>
    /// <param name="bus">The cartridge address space containing the reference word.</param>
    /// <param name="address">The bus address of the word's low byte.</param>
    /// <returns>The unsigned 16-bit value stored at that address.</returns>
    private static ushort ReadGunshipWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Audits gunship execution by rejecting reads of compiled mechanics bytes and recording reads of presentation operands.
    /// </summary>
    /// <param name="source">The underlying address space used for all permitted cartridge accesses.</param>
    private sealed class GunshipInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Gets the compiled presentation-word addresses read while production instruction programs are executing.
        /// </summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>
        /// Gets the number of attempted reads from the compiled gunship mechanics range.
        /// </summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Routes an importer cartridge read through the gunship mechanics guard.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte at the address if the access is not a forbidden mechanics read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads of compiled mechanics bytes, records presentation-word reads, and forwards permitted accesses.
        /// </summary>
        /// <param name="address">The bus address being read.</param>
        /// <returns>The byte supplied by the wrapped address space for a permitted access.</returns>
        public byte ReadByte(int address)
        {
            if (GunshipInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>
        /// Forwards a write to the wrapped address space without changing its address or value.
        /// </summary>
        /// <param name="address">The destination bus address.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
