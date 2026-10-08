using System.Reflection;
using SuperMetroid.Core.Assets;
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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(rom));
    }

    private static void VerifyFuneNamiheInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFuneNamiheMechanicsMapping), () => VerifyFuneNamiheMechanicsMapping(rom));
        Suite(nameof(VerifyFuneNamihePresentationAddresses), () => VerifyFuneNamihePresentationAddresses());

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

        AssertEqual(0, guarded.ObservedPresentationWords.Count,
            "all Fune/Namihe visual selectors are compiled, not reread from cartridge");
        Suite(nameof(VerifyFuneNamiheVisualSelectors), () => VerifyFuneNamiheVisualSelectors(rom));
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
            $"{FuneNamiheInstructionProgramDefinitionsTooling.MechanicsWordCount} words, " +
            $"{FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount} compiled " +
            "spritemap selectors, and all eight production programs pass with mechanics " +
            "and visual-selector reads forbidden.");
    }

    private static void VerifyFuneNamiheVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x939b, 0x93a1, 0x93a5, 0x93a9, 0x93ad, 0x93b5, 0x93b9, 0x93bd,
            0x93c1, 0x93cb, 0x93d1, 0x93d5, 0x93d9, 0x93dd, 0x93e5, 0x93e9, 0x93ed, 0x93f1,
            0x95bf, 0x95c5, 0x95c9, 0x95cd, 0x95d1, 0x95d5, 0x95dd, 0x95e1, 0x95e5, 0x95e9,
            0x95f3, 0x95f9, 0x95fd, 0x9601, 0x9605, 0x9609, 0x9611, 0x9615, 0x9619, 0x961d];
        foreach (ushort address in addresses)
        {
            ushort expected = ReadFuneNamiheProgramWord(rom, address);
            AssertEqual(expected, EnemySpritemapDefinitions.FuneNamiheFrameAt(address),
                "Fune/Namihe calculated pose matches native visual word");
            AssertEqual((ushort)8, ReadFuneNamiheProgramWord(rom, expected),
                "Fune/Namihe original map has eight OAM entries");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, address, out ushort shared),
                "shared catalog dispatches Fune/Namihe operand");
            AssertEqual(expected, shared, "shared Fune/Namihe pointer matches native word");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa80000 | address),
                "Fune/Namihe selector is excluded from literal regeneration");
        }
        var known = addresses.ToHashSet();
        for (int address = 0x9397; address <= 0x9626; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.FuneNamiheFrameAt((ushort)address),
                    "Fune/Namihe visual resolver rejects control words and intervening artwork");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8, (ushort)address, out ushort missing),
                    "shared Fune/Namihe interval holes are not selectors");
                AssertEqual((ushort)0, missing, "missing shared selector clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.FuneNamiheFrameAt(address),
                "Fune/Namihe visual resolver rejects distant addresses");
    }
    private static void VerifyFuneNamiheMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x9399, 0x939d, 0x939f, 0x93a3, 0x93a7, 0x93ab, 0x93af, 0x93b1, 0x93b3, 0x93b7, 0x93bb, 0x93bf, 0x93c3, 0x93c5, 0x93c7, 0x93c9, 0x93cd, 0x93cf, 0x93d3, 0x93d7, 0x93db, 0x93df, 0x93e1, 0x93e3, 0x93e7, 0x93eb, 0x93ef, 0x93f3, 0x93f5, 0x93f7, 0x95bd, 0x95c1, 0x95c3, 0x95c7, 0x95cb, 0x95cf, 0x95d3, 0x95d7, 0x95d9, 0x95db, 0x95df, 0x95e3, 0x95e7, 0x95eb, 0x95ed, 0x95ef, 0x95f1, 0x95f5, 0x95f7, 0x95fb, 0x95ff, 0x9603, 0x9607, 0x960b, 0x960d, 0x960f, 0x9613, 0x9617, 0x961b, 0x961f, 0x9621, 0x9623];
        AssertEqual(addresses.Length, FuneNamiheInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Fune/Namihe independent native mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadFuneNamiheProgramWord(rom, address);
            var word = FuneNamiheInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, word.Address, "Fune/Namihe native mechanics address");
            AssertEqual(expected, word.Value, "Fune/Namihe enumerated native mechanics value");
            AssertEqual(expected, FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Fune/Namihe direct native mechanics value");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        var words = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), FuneNamiheInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address),
                "Fune/Namihe full native byte ownership");
            AssertEqual(bytes.Contains(address), FuneNamiheInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a80000 | address),
                "Fune/Namihe bank mask aliases");
            AssertTrue(!FuneNamiheInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa70000 | address),
                "Fune/Namihe other bank rejected");
            if (address >= 0x9397 && address <= 0x9626 && !words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Fune/Namihe visual operands, odd words, intervening art and adjacent instructions rejected");
        }
        foreach (int index in new[] { int.MinValue, -1, 62, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FuneNamiheInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Fune/Namihe mechanics ordinal bounds");
    }

    private static void VerifyFuneNamihePresentationAddresses()
    {
        ushort[] addresses = [0x939b, 0x93a1, 0x93a5, 0x93a9, 0x93ad, 0x93b5, 0x93b9, 0x93bd, 0x93c1, 0x93cb, 0x93d1, 0x93d5, 0x93d9, 0x93dd, 0x93e5, 0x93e9, 0x93ed, 0x93f1, 0x95bf, 0x95c5, 0x95c9, 0x95cd, 0x95d1, 0x95d5, 0x95dd, 0x95e1, 0x95e5, 0x95e9, 0x95f3, 0x95f9, 0x95fd, 0x9601, 0x9605, 0x9609, 0x9611, 0x9615, 0x9619, 0x961d];
        AssertEqual(addresses.Length, FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Fune/Namihe independent visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Fune/Namihe native visual operand address");
        var expected = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(expected.Contains((ushort)address), FuneNamiheInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Fune/Namihe full visual membership domain");
        foreach (int index in new[] { int.MinValue, -1, 38, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Fune/Namihe visual ordinal bounds");
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (FuneNamiheInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fune/Namihe mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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
