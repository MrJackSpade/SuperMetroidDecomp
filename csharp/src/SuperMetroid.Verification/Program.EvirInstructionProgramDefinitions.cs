using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and verifies the compiled Evir instruction data against live execution.
    /// </summary>
    private static void VerifyEvirInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks compiled Evir mechanics words and instruction-loop behavior using the supplied ROM.
    /// </summary>
    /// <param name="rom">Retail address space containing the Evir instruction and presentation data.</param>
    private static void VerifyEvirInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        (ushort Entry, int Frames, ushort Definition, string Name)[] loops =
        [
            (EvirInstructionProgramDefinitions.BodyFacingLeft,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                RoomEnemySystem.EvirDefinition,
                "left body"),
            (EvirInstructionProgramDefinitions.ArmsFacingLeft,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                RoomEnemySystem.EvirDefinition,
                "left arms"),
            (EvirInstructionProgramDefinitions.BodyFacingRight,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                RoomEnemySystem.EvirDefinition,
                "right body"),
            (EvirInstructionProgramDefinitions.ArmsFacingRight,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                RoomEnemySystem.EvirDefinition,
                "right arms"),
        ];

        AssertEqual(67, EvirInstructionProgramDefinitions.MechanicsWordCount,
            "Evir compiled mechanics word count");
        AssertEqual(49, EvirInstructionProgramDefinitions.PresentationWordCount,
            "Evir live presentation word count");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EvirInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadEvirInstructionWord(rom, definition.Address),
                $"Evir mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new EvirInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach ((ushort entry, int frameCount, ushort definition, string name) in loops)
        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(slot, definition, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0; call < frameCount + 1; call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Evir {name} completes its loop and loads the first frame");
        }

        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(
                slot,
                RoomEnemySystem.EvirProjectileDefinition,
                EvirInstructionProgramDefinitions.ProjectileNormal);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            process.Invoke(enemies, arguments);
            process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileNormal + 4)),
                slot.CurrentInstruction,
                "normal Evir projectile reaches terminal sleep");
        }

        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            // The regeneration instructions read Evir.instList-$80,X: the body record two
            // slots before the projectile, as the retail population lays them out.
            RoomEnemySlot slot = enemies.Slots[2];
            PrepareEvirInstructionSlot(
                slot,
                RoomEnemySystem.EvirProjectileDefinition,
                EvirInstructionProgramDefinitions.ProjectileRegenerating);
            // #1269: the projectile's own facing word disagrees with its body on purpose;
            // $A8:879B/$87B6 follow the body's installed list, so the offset starts at +8.
            var state = new EvirEnemyState(slot)
            {
                FacingDirection = 1,
                MovingFlag = 1,
                RegenerationFlag = 1,
                Function = EvirAiFunction.ProjectileRegenerating,
            };
            var states = (EvirEnemyState?[])typeof(RoomEnemySystem)
                .GetField("_evirStates", flags)!
                .GetValue(enemies)!;
            states[2] = state;
            states[0] = new EvirEnemyState(enemies.Slots[0])
            {
                InstalledInstructionList = EvirInstructionProgramDefinitions.BodyFacingLeft,
            };
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0; call < 10; call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
                if (call == 0)
                    AssertEqual((ushort)8, state.RegenerationXOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileRegenerationLoop + 16)),
                slot.CurrentInstruction,
                "regenerating Evir projectile reaches terminal sleep");
            AssertEqual((ushort)0, state.RegenerationXOffset,
                "regeneration loop advances the projectile mouth offset eight times");
            AssertEqual((ushort)0, state.RegenerationFlag,
                "regeneration finish callback clears regeneration ownership");
            AssertEqual((ushort)0, state.MovingFlag,
                "regeneration finish callback leaves the projectile stationary");
            AssertEqual(EvirAiFunction.ProjectileIdle, state.Function,
                "regeneration finish callback restores idle AI");
            AssertEqual((ushort)0x005e, enemies.LastEvirSoundEffect,
                "regeneration script publishes the native spit sound");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Evir execution uses compiled spritemap selectors");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = EvirInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, (byte)0xa8,
                address, $"Evir $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Evir mechanics byte");

        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Evir spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.AdjacentCallbackCode),
            "adjacent Evir callback code is rejected as mechanics");

        _ = ProbeEvirInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEvirInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Evir allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Evir mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Evir instruction mechanics: 67 compiled words, all six body/arms/projectile " +
            "programs, complete regeneration callbacks, and 49 compiled spritemap selectors pass.");
    }

    /// <summary>
    /// Creates an enemy system whose private address-space dependency uses the supplied guarded bus.
    /// </summary>
    /// <param name="bus">Address space used to observe or reject Evir data reads.</param>
    /// <param name="flags">Reflection visibility used to install the system's private bus.</param>
    /// <returns>A room enemy system configured with <paramref name="bus"/>.</returns>
    private static RoomEnemySystem NewEvirInstructionSystem(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        return enemies;
    }

    /// <summary>
    /// Initializes a test slot to execute an Evir instruction list in bank A8.
    /// </summary>
    /// <param name="slot">Enemy slot that will execute the selected instruction list.</param>
    /// <param name="definition">Enemy definition pointer used to select the Evir mechanics.</param>
    /// <param name="entry">Address of the first instruction to execute.</param>
    private static void PrepareEvirInstructionSlot(
        RoomEnemySlot slot,
        ushort definition,
        ushort entry)
    {
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
    }

    /// <summary>
    /// Repeatedly reads representative compiled mechanics words to measure warmed lookup allocation.
    /// </summary>
    /// <returns>A checksum that keeps the mechanics reads observable.</returns>
    private static int ProbeEvirInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EvirInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EvirInstructionProgramDefinitions.BodyFacingLeft
                    : EvirInstructionProgramDefinitions.ProjectileRegenerating);
        }
        return checksum;
    }

    /// <summary>
    /// Reads one little-endian word from the Evir instruction bank.
    /// </summary>
    /// <param name="source">ROM address space containing bank A8.</param>
    /// <param name="address">Offset of the low byte within bank A8.</param>
    /// <returns>The word formed by the addressed byte and its following byte.</returns>
    private static ushort ReadEvirInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Wraps an address space to detect production reads of compiled Evir mechanics and presentation data.
    /// </summary>
    /// <param name="source">Underlying address space that receives permitted reads and all writes.</param>
    /// <param name="forbidPresentation">Whether presentation-data reads should throw instead of being recorded.</param>
    private sealed class EvirInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the presentation-word offsets whose bytes were read through this guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Gets the number of attempts to read bytes owned by compiled Evir mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte read through the guard's read checks.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte returned by the guarded address space.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads from compiled mechanics, records or rejects presentation reads, and forwards allowed reads.
        /// </summary>
        /// <param name="address">Address of the byte requested from the wrapped address space.</param>
        /// <returns>The requested byte when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (EvirInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Evir mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EvirInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        EvirInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Evir read presentation byte ${address:X6}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte stored at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
