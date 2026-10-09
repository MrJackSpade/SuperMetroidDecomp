using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the cartridge-backed verification suite for the compiled Norfair lava-jumper programs.</summary>
    private static void VerifyNorfairLavaJumperInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words and drives parent and follower enemies through their production instruction programs.</summary>
    /// <param name="rom">Retail address space used to compare native mechanics words and provide permitted runtime reads.</param>
    private static void VerifyNorfairLavaJumperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairLavaJumperInstructionWord(rom, 0xa20000 | definition.Address),
                $"Norfair lava-jumper mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new NorfairLavaJumperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
        var initialize = type.GetMethod("InitializeNorfairLavaJumpingEnemy", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var runMain = type.GetMethod("RunNorfairLavaJumpingEnemyMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, NorfairLavaJumpingEnemyState>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = RoomEnemySystem.NorfairLavaJumpingEnemyDefinition;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        parent.XPosition = 0x0100;
        parent.YPosition = 0x01e0;
        initialize(parent);
        NorfairLavaJumpingEnemyState parentState =
            enemies.NorfairLavaJumpingEnemyStates[0]!;
        object?[] processArguments =
            [parent, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        RunNorfairLavaJumperProgram(
            enemies, process, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Hidden,
            NorfairLavaJumperInstructionProgramDefinitions.HiddenSleep,
            timedFrames: 1);

        runMain(parent, parentState);
        parentState.YVelocity = 0xfbc8;
        runMain(parent, parentState);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Jump,
            parent.CurrentInstruction,
            "real lava-jumper rise handoff installs jump program");
        RunNorfairLavaJumperProgram(
            enemies, process, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Jump,
            NorfairLavaJumperInstructionProgramDefinitions.JumpSleep,
            timedFrames: 7);
        AssertTrue(parentState.AnimationFinished,
            "jump program publishes its real animation handshake");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = RoomEnemySystem.NorfairLavaJumpingEnemyDefinition;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize(follower);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Follower,
            follower.CurrentInstruction,
            "real follower initializer installs compiled loop");
        processArguments[0] = follower;
        for (int frame = 0; frame < 7; frame++)
        {
            follower.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(
                NorfairLavaJumperInstructionProgramDefinitions.Follower + 4)),
            follower.CurrentInstruction,
            "follower program loops to its first timed frame");

        // EnemySpritemapDefinitions now supplies all fourteen selector operands from
        // installed artwork. Program.EnemySpritemapArtwork compares each selector
        // against the cartridge; this guard verifies that live execution never
        // falls back to either the mechanics or presentation ROM bytes.
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "lava-jumper execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled lava-jumper mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe44),
            "lava-jumper presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe86),
            "adjacent lava-jumper velocity table is rejected as mechanics");

        _ = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "lava-jumper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed lava-jumper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair lava-jumper instruction mechanics: 23 compiled words, all three " +
            "production programs, the real handshake, and compiled spritemap selectors pass.");
    }

    /// <summary>Advances one lava-jumper instruction list through its timed frames and confirms it reaches terminal sleep.</summary>
    /// <param name="enemies">Enemy system whose production instruction processor is invoked.</param>
    /// <param name="process">Bound instruction-processing method.</param>
    /// <param name="arguments">Reflection argument array passed to the processor, with <paramref name="slot"/> in its enemy slot position.</param>
    /// <param name="slot">Enemy whose instruction pointer and timer are advanced.</param>
    /// <param name="entry">Expected initial program pointer.</param>
    /// <param name="terminalSleep">Expected pointer after all timed frames have elapsed.</param>
    /// <param name="timedFrames">Number of authored timed frames before the terminal sleep instruction.</param>
    private static void RunNorfairLavaJumperProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort terminalSleep,
        int timedFrames)
    {
        AssertEqual(entry, slot.CurrentInstruction, "lava-jumper compiled program entry");
        for (int frame = 0; frame < timedFrames; frame++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        slot.InstructionTimer = 1;
        process.Invoke(enemies, arguments);
        AssertEqual(terminalSleep, slot.CurrentInstruction,
            "lava-jumper program reaches terminal sleep");
    }

    /// <summary>Repeatedly reads compiled program entry words for the warmed allocation probe.</summary>
    /// <returns>A checksum of the selected words so the caller observes the lookup results.</returns>
    private static int ProbeNorfairLavaJumperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 3) switch
                {
                    0 => NorfairLavaJumperInstructionProgramDefinitions.Hidden,
                    1 => NorfairLavaJumperInstructionProgramDefinitions.Jump,
                    _ => NorfairLavaJumperInstructionProgramDefinitions.Follower,
                });
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the retail cartridge image.</summary>
    /// <param name="bus">Cartridge address space containing the native word.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The combined sixteen-bit value.</returns>
    private static ushort ReadNorfairLavaJumperInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Address-space proxy that detects fallback reads of compiled Norfair mechanics or presentation selectors.</summary>
    /// <param name="source">Underlying bus for permitted cartridge accesses.</param>
    private sealed class NorfairLavaJumperInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation selector words observed through the generic cartridge read path.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads rejected because they targeted compiled mechanics data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes asset-import reads through the same mechanics guard as generic reads.</summary>
        /// <param name="address">Cartridge address being read.</param>
        /// <returns>The permitted byte from the wrapped source.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation-selector reads, and forwards permitted accesses.</summary>
        /// <param name="address">Address requested by the runtime.</param>
        /// <returns>The byte read from the underlying source when access is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (NorfairLavaJumperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled lava-jumper mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                        ObservedPresentationWords.Add(presentation);
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the underlying cartridge address space.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
