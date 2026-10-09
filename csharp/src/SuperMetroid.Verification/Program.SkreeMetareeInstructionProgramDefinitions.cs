using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs Skree and Metaree mechanics, presentation, and program-phase checks against the retail ROM.</summary>
    private static void VerifySkreeMetareeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled instruction words and executes every animation phase for both species behind a read guard.</summary>
    /// <param name="rom">Retail address space supplying the native instruction and visual-operand references.</param>
    private static void VerifySkreeMetareeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitionsTooling.MechanicsWordCount(metaree);
                 index++)
            {
                InstructionMechanicsWord definition =
                    SkreeMetareeInstructionProgramDefinitions.MechanicsWord(metaree, index);
                AssertEqual(definition.Value,
                    ReadSkreeMetareeInstructionWord(rom, 0xa30000 | definition.Address),
                    $"{(metaree ? "Metaree" : "Skree")} mechanics $A3:{definition.Address:X4}");
            }
        }

        var guard = new SkreeMetareeInstructionProgramReadGuard(rom);
        Suite(nameof(VerifySpecies), () => VerifySpecies(metaree: true));
        Suite(nameof(VerifySpecies), () => VerifySpecies(metaree: false));

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Skree/Metaree programs never read installed visual selectors");
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(metaree);
                 index++)
            {
                ushort address =
                    SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(
                        metaree, index);
                AssertEqual(ReadSkreeMetareeInstructionWord(rom, 0xa30000 | address),
                    EnemySpritemapDefinitions.SkreeMetareeFrameAt(metaree, address),
                    $"compiled {(metaree ? "Metaree" : "Skree")} frame $A3:{address:X4}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SkreeMetareeFrameAt(true, 0xc660),
            "Metaree rejects a Skree visual operand");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SkreeMetareeFrameAt(false, 0x8912),
            "Skree rejects a Metaree visual operand");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Skree/Metaree mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(0x8912),
            "Metaree spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadSkreeMechanicsWord(0xc660),
            "Skree spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(0xc65e),
            "Metaree cannot enter Skree's program domain");

        _ = ProbeSkreeMetareeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkreeMetareeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Skree/Metaree allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Skree/Metaree lookups allocate no per-frame storage");

        Console.WriteLine(
            "Skree/Metaree instruction mechanics: forty compiled words, all eight " +
            "programs and property/completion callbacks pass with all twenty-two " +
            "visual-selector and mechanics source words forbidden.");

        void VerifySpecies(bool metaree)
        {
            foreach (SkreeMetareeAnimationPhase phase in Enum.GetValues<SkreeMetareeAnimationPhase>())
            {
                RoomEnemySystem enemies = CreateSystem(metaree, phase, out RoomEnemySlot slot);
                RunProgram(enemies, slot, phase switch
                {
                    SkreeMetareeAnimationPhase.Idling => 45,
                    SkreeMetareeAnimationPhase.PreparingAttack => 26,
                    SkreeMetareeAnimationPhase.Diving => 10,
                    SkreeMetareeAnimationPhase.StopAnimating => 2,
                    _ => throw new InvalidDataException(),
                });

                if (phase == SkreeMetareeAnimationPhase.PreparingAttack)
                {
                    bool ready = metaree
                        ? enemies.MetareeStates[0]!.AttackReady
                        : enemies.SkreeStates[0]!.AttackReady;
                    AssertTrue(ready, $"{SpeciesName(metaree)} preparation publishes ready flag");
                }
                if (phase == SkreeMetareeAnimationPhase.Diving)
                    AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                        $"{SpeciesName(metaree)} diving enables off-screen processing");
                if (phase == SkreeMetareeAnimationPhase.StopAnimating)
                    AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                        $"{SpeciesName(metaree)} stop program disables off-screen processing");
            }
        }

        RoomEnemySystem CreateSystem(
            bool metaree,
            SkreeMetareeAnimationPhase phase,
            out RoomEnemySlot slot)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            slot = enemies.Slots[0];
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };

            if (metaree)
            {
                slot.EnemyDefinitionPointer = RoomEnemySystem.MetareeDefinition;
                typeof(RoomEnemySystem).GetMethod("InitializeMetaree", flags)!
                    .Invoke(enemies, [slot]);
                MetareeEnemyState state = enemies.MetareeStates[0]!;
                state.InstalledInstructionListIndex = phase == SkreeMetareeAnimationPhase.Idling
                    ? SkreeMetareeAnimationPhase.PreparingAttack
                    : SkreeMetareeAnimationPhase.Idling;
                state.RequestedInstructionListIndex = phase;
                state.AttackReady = false;
                typeof(RoomEnemySystem).GetMethod("InstallRequestedMetareeInstruction", flags)!
                    .Invoke(null, [slot, state]);
            }
            else
            {
                slot.EnemyDefinitionPointer = RoomEnemySystem.SkreeDefinition;
                typeof(RoomEnemySystem).GetMethod("InitializeSkree", flags)!
                    .Invoke(enemies, [slot]);
                SkreeEnemyState state = enemies.SkreeStates[0]!;
                state.InstalledInstructionIndex = phase == SkreeMetareeAnimationPhase.Idling
                    ? SkreeMetareeAnimationPhase.PreparingAttack
                    : SkreeMetareeAnimationPhase.Idling;
                state.RequestedInstructionIndex = phase;
                state.AttackReady = false;
                typeof(RoomEnemySystem).GetMethod("InstallRequestedSkreeInstruction", flags)!
                    .Invoke(null, [slot, state]);
            }
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Returns the display label used to identify one member of the Skree/Metaree pair in verification messages.</summary>
    /// <param name="metaree"><see langword="true"/> to select Metaree; otherwise, select Skree.</param>
    /// <returns>The species label used by assertions.</returns>
    private static string SpeciesName(bool metaree) => metaree ? "Metaree" : "Skree";

    /// <summary>Warms and repeats one compiled mechanics lookup for each species so the caller can measure allocation behavior.</summary>
    /// <returns>A checksum that keeps both lookup results observable.</returns>
    private static int ProbeSkreeMetareeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(
                SkreeMetareeInstructionProgramDefinitions.MetareeIdling);
            checksum += SkreeMetareeInstructionProgramDefinitions.ReadSkreeMechanicsWord(
                SkreeMetareeInstructionProgramDefinitions.SkreeIdling);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction or presentation word from the supplied address space.</summary>
    /// <param name="bus">Address space containing the word bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadSkreeMetareeInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects reads of compiled Skree/Metaree mechanics and installed visual selectors, forwarding other requests.</summary>
    /// <param name="source">Underlying address space used for reads outside the guarded ranges and for writes.</param>
    private sealed class SkreeMetareeInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of read attempts targeting installed Skree or Metaree visual-selector words.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }

        /// <summary>Number of read attempts targeting bytes represented by compiled mechanics definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge imports through the same forbidden-address checks as ordinary reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte returned by the guarded address-space read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics and presentation bytes, forwarding unrelated reads to the source.</summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics or an installed presentation selector.</exception>
        public byte ReadByte(int address)
        {
            if (SkreeMetareeInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Skree/Metaree mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int species = 0; species < 2; species++)
                {
                    bool metaree = species == 0;
                    for (int index = 0;
                         index < SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(metaree);
                         index++)
                    {
                        ushort presentation =
                            SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(
                                metaree, index);
                        if (bankAddress == presentation ||
                            bankAddress == unchecked((ushort)(presentation + 1)))
                        {
                            ForbiddenPresentationReadAttempts++;
                            throw new InvalidOperationException(
                                $"Production read installed Skree/Metaree selector ${address:X6}.");
                        }
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged; the guard monitors read accesses only.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte value forwarded to the source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
