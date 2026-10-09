using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the Kraid fingernail instruction checks against the retail cartridge.</summary>
    private static void VerifyKraidNailInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks native nail population identities and initial frames, executes both nail
    /// instruction loops through production dispatch, and rejects cartridge reads for
    /// compiled mechanics and installed visual selectors.
    /// </summary>
    /// <param name="rom">Retail address space used to compare native records and instruction words.</param>
    private static void VerifyKraidNailInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(10, KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            "Kraid fingernail compiled mechanics word count");
        AssertEqual(8, KraidNailInstructionProgramDefinitions.PresentationWordCount,
            "Kraid fingernail compiled visual operand count");
        AssertEqual(ReadKraidNailInstructionWord(rom, 0x8b0c),
            KraidVisualDefinitions.InitialNailFrame,
            "compiled Kraid fingernail initial frame matches its cartridge selector");
        foreach ((int slotIndex, ushort expectedDefinition) in new[]
                 {
                     (6, RoomEnemySystem.KraidGoodNailDefinition),
                     (7, RoomEnemySystem.KraidBadNailDefinition),
                 })
        {
            int record = EnemyRomTablePointers.Kraid.PopulationRecords + slotIndex * 16;
            ushort definition = (ushort)(rom.ReadByte(record) |
                rom.ReadByte(record + 1) << 8);
            ushort extraProperties = (ushort)(rom.ReadByte(record + 10) |
                rom.ReadByte(record + 11) << 8);
            AssertEqual(expectedDefinition, definition,
                $"retail Kraid nail slot {slotIndex} matches the visual family");
            AssertTrue((extraProperties &
                    (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
                $"retail Kraid nail slot {slotIndex} uses ordinary OAM composition");
        }
        Suite(nameof(VerifyKraidNailMechanicsMapping), () => VerifyKraidNailMechanicsMapping(rom));
        Suite(nameof(VerifyKraidNailPresentationMapping), () => VerifyKraidNailPresentationMapping());

        var guard = new KraidNailInstructionReadGuard(rom);
        foreach (ushort definitionPointer in new ushort[]
                 {
                     RoomEnemySystem.KraidGoodNailDefinition,
                     RoomEnemySystem.KraidBadNailDefinition,
                 })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField(
                "_bus",
                BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
            slot.InstructionTimer = 1;

            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            for (int frame = 0; frame < 9; frame++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(
                    enemies,
                    [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            }
            AssertEqual(unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 4)),
                slot.CurrentInstruction,
                $"Kraid fingernail ${definitionPointer:X4} loops to its first frame");
        }

        Suite(nameof(VerifyKraidNailVisualSelectors), () => VerifyKraidNailVisualSelectors(rom));
        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Kraid fingernail programs never read installed visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid fingernail production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidNailInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertThrows<InvalidDataException>(
                () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid fingernail spritemap $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                KraidNailInstructionProgramDefinitions.AdjacentPresentationData),
            "adjacent Kraid arm presentation data is rejected as fingernail mechanics");
        AssertThrows<InvalidDataException>(
            () => KraidVisualDefinitions.FrameAt(
                RoomEnemySystem.KraidGoodNailDefinition,
                KraidNailInstructionProgramDefinitions.AdjacentPresentationData),
            "adjacent Kraid visual operand is not a fingernail frame");

        foreach ((ushort definition, int slotIndex) in new[]
                 {
                     (RoomEnemySystem.KraidGoodNailDefinition, 6),
                     (RoomEnemySystem.KraidBadNailDefinition, 7),
                 })
        {
            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_kraidState", flags)!
                .SetValue(enemies, new KraidEnemyState());
            typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!
                .SetValue(enemies, (Func<bool>)(() => false));
            enemies.Slots[0].EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
            RoomEnemySlot slot = enemies.Slots[slotIndex];
            slot.EnemyDefinitionPointer = definition;
            typeof(RoomEnemySystem).GetMethod("InitializeKraidNail", flags)!
                .Invoke(enemies, [slot, slotIndex]);
            AssertEqual(KraidVisualDefinitions.InitialNailFrame,
                slot.SpritemapPointer,
                $"Kraid fingernail slot {slotIndex} installs the compiled initial frame");
        }
        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Kraid fingernail initialization never reads the initial frame from ROM");

        _ = ProbeKraidNailInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidNailInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid fingernail allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid fingernail mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid fingernail instruction mechanics: ten compiled words, both nail " +
            "definitions, eight compiled visual selectors, and both initial frames pass " +
            "with selector and mechanics ROM reads forbidden.");
    }

    /// <summary>Warms repeated compiled mechanics lookups and returns a checksum that consumes their values.</summary>
    /// <returns>The accumulated values read from alternating positions in the fingernail loop.</returns>
    private static int ProbeKraidNailInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidNailInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 0x20)));
        }
        return checksum;
    }

    /// <summary>Reads one little-endian word from bank $A7 at the given bank-local address.</summary>
    /// <param name="source">Address space containing the Kraid instruction bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadKraidNailInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Rejects runtime reads of compiled fingernail mechanics and installed selector words,
    /// while allowing unrelated cartridge access to reach the wrapped address space.
    /// </summary>
    /// <param name="source">Address space used for reads outside the protected mechanics and selector ranges.</param>
    private sealed class KraidNailInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from installed fingernail visual-selector words.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }

        /// <summary>Number of attempted reads from compiled fingernail mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the guard's mechanics and selector checks.</summary>
        /// <param name="address">Cartridge bus address requested by the importer.</param>
        /// <returns>The wrapped address space's byte when the read is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address is a protected mechanics or selector byte.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects protected mechanics and selector reads, then forwards unrelated addresses.</summary>
        /// <param name="address">Address requested by production execution.</param>
        /// <returns>The wrapped address space's byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics or an installed selector.</exception>
        public byte ReadByte(int address)
        {
            if (KraidNailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid fingernail mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidNailInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Kraid nail selector ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
