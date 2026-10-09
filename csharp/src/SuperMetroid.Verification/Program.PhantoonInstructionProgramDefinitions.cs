using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs Phantoon's compiled instruction, callback, presentation, and allocation checks against the retail ROM.</summary>
    private static void VerifyPhantoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words and executes the reachable body, eye, tentacle, and mouth programs.</summary>
    /// <param name="rom">Retail address space supplying the independent native word and presentation references.</param>
    private static void VerifyPhantoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(58, PhantoonInstructionProgramDefinitions.MechanicsWordCount,
            "Phantoon compiled mechanics word count");
        AssertEqual(27, PhantoonInstructionProgramDefinitions.PresentationWordCount,
            "Phantoon presentation word count");
        for (int index = 0;
             index < PhantoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PhantoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPhantoonInstructionWord(rom, definition.Address),
                $"Phantoon mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new PhantoonInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreatePhantoonInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tentacles = enemies.Slots[2];
        RoomEnemySlot mouth = enemies.Slots[3];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (ushort entry in new ushort[]
                 {
                     PhantoonInstructionProgramDefinitions.InvulnerableBody,
                     PhantoonInstructionProgramDefinitions.FullHitboxBody,
                     PhantoonInstructionProgramDefinitions.EyeHitboxBody,
                 })
        {
            RunPhantoonInstructionProgram(rom, executedOperands, process, enemies, body, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), body.CurrentInstruction,
                $"Phantoon body program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeOpen,
            calls: 4);
        AssertEqual((ushort)0xcc67, eye.CurrentInstruction,
            "Phantoon open-eye program reaches terminal sleep");
        AssertEqual(PhantoonInstructionProgramDefinitions.EyeHitboxBody,
            body.CurrentInstruction,
            "Phantoon open-eye callback installs the eye-only body hitbox");
        AssertTrue(enemies.Phantoon!.LastMaterializationSound.HasValue,
            "Phantoon open-eye callback queues its materialization sound");

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClosed,
            calls: 2);
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeCloseAndPickNewPattern,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close-and-pick program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClose,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeballCentered,
            calls: 2);

        foreach (ushort entry in new ushort[]
                 {
                     PhantoonInstructionProgramDefinitions.EyeLookingUp,
                     PhantoonInstructionProgramDefinitions.EyeLookingUpRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingDownRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingDown,
                     PhantoonInstructionProgramDefinitions.EyeLookingDownLeft,
                     PhantoonInstructionProgramDefinitions.EyeLookingLeft,
                     PhantoonInstructionProgramDefinitions.EyeLookingUpLeft,
                 })
        {
            RunPhantoonInstructionProgram(rom, executedOperands, process, enemies, eye, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), eye.CurrentInstruction,
                $"Phantoon eye-direction program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            tentacles,
            PhantoonInstructionProgramDefinitions.InitialTentacles,
            calls: 5);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialTentacles + 4)),
            tentacles.CurrentInstruction,
            "Phantoon tentacle program loops to its first frame");

        int flamesBefore = enemies.EnemyProjectiles.Count(projectile => projectile.IsActive);
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.MouthFollowUp,
            calls: 3);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialMouth + 4)),
            mouth.CurrentInstruction,
            "Phantoon mouth flame program falls through into its initial frame");
        AssertEqual(flamesBefore + 1,
            enemies.EnemyProjectiles.Count(projectile => projectile.IsActive),
            "Phantoon mouth callback spawns one casual flame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.InitialMouth,
            calls: 2);

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(PhantoonInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Phantoon production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < PhantoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                PhantoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Phantoon presentation $A7:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa7, address, out ushort selected),
                "Every Phantoon visual operand has an installed selector");
            AssertEqual(ReadPhantoonInstructionWord(rom, address), selected,
                "Exact native Phantoon visual operand");
            AssertEqual(ReadPhantoonInstructionWord(rom, address),
                PhantoonInstructionProgramDefinitions.PresentationFrame(index),
                "Named Phantoon role selection equals the native operand");
            AssertEqual(selected, PhantoonInstructionProgramDefinitions.FrameAt(address),
                "Phantoon address dispatch equals indexed role selection");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa70000 | address),
                "Phantoon selector belongs to calculated dispatch");
            AssertThrows<InvalidDataException>(
                () => PhantoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Phantoon presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonInstructionProgramDefinitions.ReadMechanicsWord(
                PhantoonInstructionProgramDefinitions.AdjacentCasualFlameTimers),
            "adjacent Phantoon casual-flame timer data is rejected as instruction mechanics");

        _ = ProbePhantoonInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePhantoonInstructionAllocation();
        AssertTrue(checksum != 0, "Phantoon instruction allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Phantoon instruction mechanics lookups allocate no storage");

        Console.WriteLine(
            "Phantoon instruction mechanics: 58 compiled words, all 19 reachable " +
            "programs, four callbacks, and 27 native sprite selections pass with zero live reads.");
    }

    /// <summary>Builds an enemy system with Phantoon's four actor slots and deterministic private runtime state initialized.</summary>
    /// <param name="bus">Address space assigned to the enemy system for projectile and instruction processing.</param>
    /// <returns>The configured system containing body, eye, tentacle, and mouth actors in slots zero through three.</returns>
    private static RoomEnemySystem CreatePhantoonInstructionSystem(ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));

        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tentacles = enemies.Slots[2];
        RoomEnemySlot mouth = enemies.Slots[3];
        body.EnemyDefinitionPointer = RoomEnemySystem.PhantoonBodyDefinition;
        eye.EnemyDefinitionPointer = RoomEnemySystem.PhantoonEyeDefinition;
        tentacles.EnemyDefinitionPointer = RoomEnemySystem.PhantoonTentaclesDefinition;
        mouth.EnemyDefinitionPointer = RoomEnemySystem.PhantoonMouthDefinition;
        foreach (RoomEnemySlot slot in new[] { body, eye, tentacles, mouth })
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };

        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
            Mouth = mouth,
        };
        typeof(RoomEnemySystem).GetField("_phantoonState", flags)!.SetValue(enemies, state);
        return enemies;
    }

    /// <summary>Executes a selected Phantoon program for a fixed number of instruction calls and checks its native sprite selection.</summary>
    /// <param name="rom">Address space used to inspect each executed native instruction word.</param>
    /// <param name="executedOperands">Set updated with visual operand addresses reached by the program.</param>
    /// <param name="process">Reflected enemy instruction processor invoked for each call.</param>
    /// <param name="enemies">System owning the actor and Phantoon state under test.</param>
    /// <param name="slot">Actor slot whose instruction pointer is advanced.</param>
    /// <param name="entry">Program entry address assigned before execution begins.</param>
    /// <param name="calls">Number of instruction-processing calls to perform.</param>
    private static void RunPhantoonInstructionProgram(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        ushort entry,
        int calls)
    {
        slot.CurrentInstruction = entry;
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            ushort previousSprite = slot.SpritemapPointer;
            process.Invoke(
                enemies,
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            ushort record = unchecked((ushort)(slot.CurrentInstruction - 4));
            ushort nativeWord = (ushort)(rom.ReadByte(0xa70000 | record) |
                rom.ReadByte(0xa70000 | unchecked((ushort)(record + 1))) << 8);
            if ((nativeWord & 0x8000) == 0)
                VerifyExecutedEnemySelector(rom, slot, executedOperands);
            else
                AssertEqual(previousSprite, slot.SpritemapPointer,
                    "Phantoon callback-to-sleep retains the previously selected sprite");
        }
    }

    /// <summary>Warms and repeats compiled mechanics lookups so the caller can measure warmed allocation behavior.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbePhantoonInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PhantoonInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PhantoonInstructionProgramDefinitions.InvulnerableBody
                    : unchecked((ushort)(
                        PhantoonInstructionProgramDefinitions.InitialMouth + 4)));
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from Phantoon's bank-$A7 program data.</summary>
    /// <param name="source">Retail address space supplying the two instruction bytes.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadPhantoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects production reads of compiled Phantoon mechanics and records any live presentation-operand reads.</summary>
    /// <param name="source">Underlying address space used for reads permitted by the guard.</param>
    private sealed class PhantoonInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Addresses of presentation words observed while production enemy instructions execute.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of production reads rejected because the byte belongs to compiled Phantoon mechanics data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the same compiled-mechanics check as ordinary reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte returned by the guarded address-space read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics bytes, records presentation operands, and forwards permitted reads to the source.</summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The source byte when the requested address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Phantoon mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (PhantoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Phantoon mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PhantoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = PhantoonInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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

        /// <summary>Forwards a memory write unchanged; the guard monitors read access only.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte value forwarded to the source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
