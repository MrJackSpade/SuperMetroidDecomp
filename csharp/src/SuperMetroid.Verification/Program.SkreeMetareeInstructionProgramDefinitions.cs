using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySkreeMetareeInstructionProgramDefinitions()
    {
        VerifySkreeMetareeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifySkreeMetareeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitions.MechanicsWordCount(metaree);
                 index++)
            {
                SkreeMetareeInstructionMechanicsWord definition =
                    SkreeMetareeInstructionProgramDefinitions.MechanicsWord(metaree, index);
                AssertEqual(definition.Value,
                    ReadSkreeMetareeInstructionWord(rom, 0xa30000 | definition.Address),
                    $"{(metaree ? "Metaree" : "Skree")} mechanics $A3:{definition.Address:X4}");
            }
        }

        var guard = new SkreeMetareeInstructionProgramReadGuard(rom);
        VerifySpecies(metaree: true);
        VerifySpecies(metaree: false);

        AssertEqual(22, guard.ObservedPresentationWords.Count,
            "all live Skree/Metaree spritemap words remain cartridge reads");
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitions.PresentationWordCount(metaree);
                 index++)
            {
                ushort address =
                    SkreeMetareeInstructionProgramDefinitions.PresentationWordAddress(
                        metaree, index);
                AssertTrue(guard.ObservedPresentationWords.Contains(address),
                    $"production reads {(metaree ? "Metaree" : "Skree")} presentation $A3:{address:X4}");
            }
        }
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
            "programs, property/completion callbacks, and twenty-two live spritemap " +
            "reads pass with mechanics bytes forbidden.");

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

    private static string SpeciesName(bool metaree) => metaree ? "Metaree" : "Skree";

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

    private static ushort ReadSkreeMetareeInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class SkreeMetareeInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

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
                         index < SkreeMetareeInstructionProgramDefinitions.PresentationWordCount(metaree);
                         index++)
                    {
                        ushort presentation =
                            SkreeMetareeInstructionProgramDefinitions.PresentationWordAddress(
                                metaree, index);
                        if (bankAddress == presentation ||
                            bankAddress == unchecked((ushort)(presentation + 1)))
                        {
                            ObservedPresentationWords.Add(presentation);
                        }
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
