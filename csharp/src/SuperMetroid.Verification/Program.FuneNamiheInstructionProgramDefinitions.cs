using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies all eight mixed Fune/Namihe streams and their strict mechanics/presentation
    /// ownership boundary against the pinned cartridge.
    /// </summary>
    private static void VerifyFuneNamiheInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Fune/Namihe instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyFuneNamiheInstructionProgramDefinitions(rom);
    }

    private static void VerifyFuneNamiheInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < FuneNamiheInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FuneNamiheInstructionMechanicsWord definition =
                FuneNamiheInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadFuneNamiheProgramWord(rom, definition.Address),
                $"Fune/Namihe mechanics word $A8:{definition.Address:X4}");
        }

        var guarded = new FuneNamiheInstructionReadGuard(rom);
        FuneNamiheProgramCase[] programs =
        [
            new(FuneNamiheInstructionProgramDefinitions.FuneActiveLeft, 0x96d3, false, false, true),
            new(FuneNamiheInstructionProgramDefinitions.FuneActiveRight, 0x96d5, false, true, true),
            new(FuneNamiheInstructionProgramDefinitions.FuneIdleLeft, 0x96d7, false, false, false),
            new(FuneNamiheInstructionProgramDefinitions.FuneIdleRight, 0x96d9, false, true, false),
            new(FuneNamiheInstructionProgramDefinitions.NamiheActiveLeft, 0x96db, true, false, true),
            new(FuneNamiheInstructionProgramDefinitions.NamiheActiveRight, 0x96dd, true, true, true),
            new(FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft, 0x96df, true, false, false),
            new(FuneNamiheInstructionProgramDefinitions.NamiheIdleRight, 0x96e1, true, true, false),
        ];

        foreach (FuneNamiheProgramCase program in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = RunFuneNamiheProgram(
                guarded,
                program);
            int activeProjectiles = enemies.EnemyProjectiles.Count(projectile => projectile.IsActive);
            AssertEqual(program.Active ? 1 : 0, activeProjectiles,
                $"Fune/Namihe program $A8:{program.Entry:X4} projectile count");

            if (!program.Active)
                continue;

            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(program.Right ? (ushort)1 : (ushort)0, projectile.DirectionParameter,
                $"Fune/Namihe program $A8:{program.Entry:X4} projectile direction");
            AssertEqual(FuneNamiheDefinitions.SpitSoundEffect, enemies.LastFuneNamiheSoundEffect,
                $"Fune/Namihe program $A8:{program.Entry:X4} sound callback");
            AssertEqual(
                program.Namihe
                    ? FuneNamiheEnemyFunction.NamiheWaitForSamus
                    : FuneNamiheEnemyFunction.FuneWaitForCooldown,
                enemies.FuneNamiheStates[slot.SlotIndex]!.Function,
                $"Fune/Namihe program $A8:{program.Entry:X4} returns main-AI ownership");
        }

        AssertEqual(
            FuneNamiheInstructionProgramDefinitions.PresentationWordCount,
            guarded.ObservedPresentationWords.Count,
            "all live Fune/Namihe spritemap words remain cartridge reads");
        for (int index = 0;
             index < FuneNamiheInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FuneNamiheInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guarded.ObservedPresentationWords.Contains(address),
                $"production execution reads presentation word $A8:{address:X4}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids every compiled Fune/Namihe mechanics byte");

        AssertThrows<InvalidDataException>(
            () => FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(0x939b),
            "interleaved Fune spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(0x95bf),
            "interleaved Namihe spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside all Fune/Namihe programs fails loudly");

        _ = FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(
            FuneNamiheInstructionProgramDefinitions.FuneActiveLeft);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(
                FuneNamiheInstructionProgramDefinitions.FuneActiveLeft);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Fune/Namihe allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Fune/Namihe mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Fune/Namihe instruction mechanics: " +
            $"{FuneNamiheInstructionProgramDefinitions.MechanicsWordCount} words, " +
            $"{FuneNamiheInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap words, and all eight production programs pass with mechanics reads " +
            "forbidden.");
    }

    private static (RoomEnemySystem System, RoomEnemySlot Slot) RunFuneNamiheProgram(
        FuneNamiheInstructionReadGuard bus,
        FuneNamiheProgramCase program)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = program.Namihe
            ? FuneNamiheDefinitions.NamiheEnemyDefinition
            : FuneNamiheDefinitions.FuneEnemyDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.Parameter1 = unchecked((ushort)(
            (program.Namihe ? 1 : 0) | (program.Right ? 0x10 : 0)));
        slot.Parameter2 = 0;
        slot.XPosition = 128;
        slot.YPosition = 128;
        slot.CurrentInstruction = program.Entry;
        slot.InstructionTimer = 1;

        var state = new FuneNamiheEnemyState(slot)
        {
            InstructionListPointerTableCursor = program.SelectorCursor,
            Function = program.Active
                ? program.Namihe
                    ? FuneNamiheEnemyFunction.NamiheActivityNoOp
                    : FuneNamiheEnemyFunction.FuneActivityNoOp
                : program.Namihe
                    ? FuneNamiheEnemyFunction.NamiheWaitForSamus
                    : FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = program.Namihe ? (ushort)1 : (ushort)0,
        };
        var states = (FuneNamiheEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_funeNamiheStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments = [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        int frames = program.Active ? 140 : 4;
        for (int frame = 0; frame < frames; frame++)
            process.Invoke(enemies, arguments);

        return (enemies, slot);
    }

    private static ushort ReadFuneNamiheProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private readonly record struct FuneNamiheProgramCase(
        ushort Entry,
        ushort SelectorCursor,
        bool Namihe,
        bool Right,
        bool Active);

    private sealed class FuneNamiheInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FuneNamiheInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fune/Namihe mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FuneNamiheInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FuneNamiheInstructionProgramDefinitions.PresentationWordAddress(index);
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
