using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions()
    {
        VerifyDeadTourianCorpseInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DeadTourianCorpseInstructionMechanicsWord definition =
                DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadTourianCorpseInstructionWord(rom, definition.Address),
                $"Dead Tourian corpse mechanics word $A9:{definition.Address:X4}");
        }

        (DeadTourianCorpseSpecies Species, ushort EnemyDefinition, int VariantCount)[] families =
        [
            (DeadTourianCorpseSpecies.Zoomer, RoomEnemySystem.DeadZoomerDefinition, 3),
            (DeadTourianCorpseSpecies.Ripper, RoomEnemySystem.DeadRipperDefinition, 2),
            (DeadTourianCorpseSpecies.Skree, RoomEnemySystem.DeadSkreeDefinition, 3),
        ];

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadTourianCorpseInstructionReadGuard(rom);
        int programIndex = 0;
        foreach (var family in families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                var enemies = new RoomEnemySystem();
                Type type = typeof(RoomEnemySystem);
                type.GetField("_bus", flags)!.SetValue(enemies, guard);
                var initialize = type.GetMethod("InitializeDeadTourianCorpse", flags)!
                    .CreateDelegate<Action<RoomEnemySlot>>(enemies);
                MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

                RoomEnemySlot corpse = enemies.Slots[0];
                corpse.EnemyDefinitionPointer = family.EnemyDefinition;
                corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
                corpse.Parameter1 = checked((ushort)(variantIndex * 2));
                initialize(corpse);

                ushort program = DeadTourianCorpseInstructionProgramDefinitions.Program(
                    programIndex);
                AssertEqual(program, corpse.CurrentInstruction,
                    $"real dead {family.Species} variant {variantIndex} initializer program");

                object?[] processArguments =
                    [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
                for (int call = 0; call < 2; call++)
                {
                    corpse.InstructionTimer = 1;
                    process.Invoke(enemies, processArguments);
                }
                AssertEqual(
                    DeadTourianCorpseInstructionProgramDefinitions.SleepWordAddress(programIndex),
                    corpse.CurrentInstruction,
                    $"dead {family.Species} variant {variantIndex} reaches terminal sleep");
                programIndex++;
            }
        }

        AssertEqual(DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
            programIndex,
            "every Dead Tourian corpse program executes");
        AssertEqual(DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
            guard.ObservedPresentationWords.Count,
            "all Dead Tourian corpse spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead Tourian corpse mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTourianCorpseInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Dead Tourian corpse spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTourianCorpseInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent corpse spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTourianCorpseInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTourianCorpseInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Dead Tourian corpse allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Tourian corpse mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Tourian corpse instruction mechanics: sixteen compiled words, all eight " +
            "real Zoomer/Ripper/Skree initializers and terminal sleeps, and eight live " +
            "spritemap reads pass with mechanics bytes forbidden.");
    }

    private static int ProbeDeadTourianCorpseInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTourianCorpseInstructionProgramDefinitions.Zoomer0
                    : DeadTourianCorpseInstructionProgramDefinitions.SleepWordAddress(7));
        }
        return checksum;
    }

    private static ushort ReadDeadTourianCorpseInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class DeadTourianCorpseInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DeadTourianCorpseInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead Tourian corpse mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DeadTourianCorpseInstructionProgramDefinitions.ProgramCount;
                     index++)
                {
                    ushort presentation =
                        DeadTourianCorpseInstructionProgramDefinitions.PresentationWordAddress(index);
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
