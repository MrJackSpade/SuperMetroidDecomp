using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Authored Yard instruction-list entries driven through the production enemy interpreter.</summary>
    private static readonly ushort[] YardInstructionEntries =
    [
        YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp,
        YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight,
        YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight,
        YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp,
        YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight,
        YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight,
        YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown,
        YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown,
        YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight,
        YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight,
        YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown,
        YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.HidingUpsideDownMovingRight,
        YardInstructionProgramDefinitions.HidingUpsideUpMovingRight,
        YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight,
        YardInstructionProgramDefinitions.HidingUpsideRightMovingUp,
        YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.HidingUpsideRightMovingDown,
        YardInstructionProgramDefinitions.AirborneFacingLeft,
        YardInstructionProgramDefinitions.AirborneFacingLeftLoop,
        YardInstructionProgramDefinitions.AirborneFacingRight,
        YardInstructionProgramDefinitions.AirborneFacingRightLoop,
    ];

    /// <summary>Loads the retail cartridge and runs the Yard instruction-program verification suite.</summary>
    private static void VerifyYardInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with cartridge words and executes every authored Yard entry under a read guard.</summary>
    /// <param name="rom">Cartridge address space used to check native instruction and visual selector words.</param>
    private static void VerifyYardInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        int mechanics = 0;
        int presentation = 0;
        for (int address = 0xc8c6; address < 0xcc36; address += 2)
        {
            if (YardInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address))
            {
                mechanics++;
                AssertEqual(ReadYardInstructionWord(rom, unchecked((ushort)address)),
                    YardInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Yard mechanics word $A3:{address:X4}");
            }
            else
            {
                presentation++;
                AssertThrows<InvalidDataException>(
                    () => YardInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Yard presentation word $A3:{address:X4} is rejected as mechanics");
            }
        }
        AssertEqual(YardInstructionProgramDefinitionsTooling.MechanicsWordCount, mechanics,
            "Yard compiled mechanics word count");
        AssertEqual(YardInstructionProgramDefinitions.PresentationWordCount, presentation,
            "Yard live presentation word count");

        var guard = new YardInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateYardInstructionSystem(guard, out RoomEnemySlot yard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (ushort entry in YardInstructionEntries)
        {
            yard.CurrentInstruction = entry;
            for (int call = 0; call < 80; call++)
            {
                yard.InstructionTimer = 1;
                process.Invoke(
                    enemies,
                    [yard, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            }
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Yard execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Yard production execution avoids compiled mechanics bytes");
        for (int index = 0; index < YardInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, RoomEnemySystem.YardDefinition,
                0xa3, address, $"Yard $A3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => YardInstructionProgramDefinitions.ReadMechanicsWord(0xc8c7),
            "odd Yard instruction pointer is rejected");
        AssertThrows<InvalidDataException>(
            () => YardInstructionProgramDefinitions.ReadMechanicsWord(0xcc36),
            "adjacent Yard callback code is rejected as instruction mechanics");

        _ = ProbeYardInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYardInstructionAllocation();
        AssertTrue(checksum != 0, "Yard instruction allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yard instruction mechanics lookups allocate no storage");

        Console.WriteLine(
            "Yard instruction mechanics: 328 compiled words, all 38 authored entries, " +
            "five private callbacks, and 112 compiled visual selectors pass.");
    }

    /// <summary>Creates a Yard enemy system with deterministic randomness and its private per-slot state initialized.</summary>
    /// <param name="bus">Address space installed as the enemy system's cartridge bus.</param>
    /// <param name="yard">Receives the configured Yard slot whose instruction programs will be executed.</param>
    /// <returns>The enemy system containing the initialized Yard state.</returns>
    private static RoomEnemySystem CreateYardInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot yard)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));

        yard = enemies.Slots[0];
        yard.EnemyDefinitionPointer = RoomEnemySystem.YardDefinition;
        yard.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        var state = new YardEnemyState(yard);
        var states = (YardEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_yardStates", flags)!.GetValue(enemies)!;
        states[0] = state;
        return enemies;
    }

    /// <summary>Consumes repeated compiled mechanics lookups for the warmed allocation measurement.</summary>
    /// <returns>A checksum of selected instruction words that keeps the lookup results observable.</returns>
    private static int ProbeYardInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YardInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft
                    : YardInstructionProgramDefinitions.AirborneFacingRightLoop);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from bank $A3 at a 16-bit instruction address.</summary>
    /// <param name="source">Cartridge address space containing the native program bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The native word formed from the addressed byte and its successor.</returns>
    private static ushort ReadYardInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Tracks presentation-selector reads and rejects runtime access to mechanics words supplied by compiled definitions.</summary>
    /// <param name="source">Underlying address space supplying permitted program bytes.</param>
    /// <param name="forbidPresentation">When true, visual-selector reads throw instead of being recorded.</param>
    private sealed class YardInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses observed while executing Yard programs when selector reads are allowed.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads from instruction bytes that should come from compiled mechanics definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the guard's checks for forbidden runtime data access.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The source byte when the read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records or rejects presentation selectors, and forwards other bytes.</summary>
        /// <param name="address">Cartridge byte address requested during instruction execution.</param>
        /// <returns>The source byte if the address is not forbidden.</returns>
        public byte ReadByte(int address)
        {
            if (YardInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yard mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YardInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        YardInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Yard read presentation byte ${address:X6}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged; this fixture observes reads from Yard instruction data.</summary>
        /// <param name="address">Destination byte address.</param>
        /// <param name="value">Byte written to the underlying address space.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
