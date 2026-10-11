using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadTourianCorpseInstructionProgramDefinitions), () => VerifyDeadTourianCorpseInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyDeadTourianCorpseInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadTourianCorpseInstructionWord(rom, definition.Address),
                $"Dead Tourian corpse mechanics word $A9:{definition.Address:X4}");
        }

        (DeadTourianCorpseSpecies Species, EnemyDefinitionId EnemyDefinition, int VariantCount)[] families =
        [
            (DeadTourianCorpseSpecies.Zoomer, EnemyDefinitionId.CorpseZoomer, 3),
            (DeadTourianCorpseSpecies.Ripper, EnemyDefinitionId.CorpseRipper, 2),
            (DeadTourianCorpseSpecies.Skree, EnemyDefinitionId.CorpseSkree, 3),
        ];

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewDeadTourianCorpseInstructionReadGuard(rom);
        int programIndex = 0;
        foreach (var family in families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                var enemies = new RoomEnemySystem { TileArtwork = LookupStream5CorpseFixtureArtwork(rom) };
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
                    [corpse, null, null, Position16<RoomPixels>.Zero, (ushort)0];
                for (int call = 0; call < 2; call++)
                {
                    corpse.InstructionTimer = 1;
                    process.Invoke(enemies, processArguments);
                }
                AssertEqual(
                    DeadTourianCorpseInstructionProgramDefinitions.SleepWordAddress(programIndex),
                    corpse.CurrentInstruction,
                    $"dead {family.Species} variant {variantIndex} reaches terminal sleep");
                ushort operand = DeadTourianCorpseInstructionProgramDefinitionsTooling
                    .PresentationWordAddress(programIndex);
                AssertCompiledEnemyVisualSelector(rom, family.EnemyDefinition, 0xa9,
                    operand, $"dead {family.Species} variant {variantIndex}");
                programIndex++;
            }
        }

        AssertEqual(DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
            programIndex,
            "every Dead Tourian corpse program executes");
        AssertEqual(0, guard.ObservedWords.Count,
            "Dead Tourian corpse execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead Tourian corpse mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTourianCorpseInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
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
            "real Zoomer/Ripper/Skree initializers and terminal sleeps, and eight compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");
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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Dead Tourian corpse instruction words")]
    private static ushort ReadDeadTourianCorpseInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadCartridgeByte(0xa90000 | address) |
            source.ReadCartridgeByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    private static CartridgeReadGuard NewDeadTourianCorpseInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(DeadTourianCorpseInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dead Tourian corpse mechanics byte ${address:X6}.")
            .ObserveWords(0xa90000, DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
                DeadTourianCorpseInstructionProgramDefinitionsTooling.PresentationWordAddress);
}
