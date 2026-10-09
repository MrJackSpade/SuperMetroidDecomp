using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for Fake Kraid projectile instruction programs.</summary>
    private static void VerifyFakeKraidProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with cartridge words and exercises the production spit and spike producers, projectile instruction loops, terminal sleeps, and shared deletion path under guarded reads.</summary>
    /// <param name="rom">Retail address space supplying reference instruction durations and words.</param>
    private static void VerifyFakeKraidProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(rom));
        Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());

        var guard = new FakeKraidProjectileInstructionReadGuard(rom);
        var selectedPresentationWords = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        MethodInfo spawnSpit = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpit",
            instanceFlags)!;
        MethodInfo spawnSpike = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpike",
            instanceFlags)!;

        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new FakeKraidEnemyState(source);

        AssertTrue((bool)spawnSpit.Invoke(
                enemies,
                [source, (short)12, (ushort)0x0180, unchecked((ushort)-0x0200)])!,
            "real Fake Kraid spit producer allocates its projectile");
        source.VariableC = unchecked((ushort)-4);
        AssertTrue((bool)spawnSpike.Invoke(enemies, [source, state, 0])!,
            "real Fake Kraid left-spike producer allocates its projectile");
        source.VariableC = 4;
        AssertTrue((bool)spawnSpike.Invoke(enemies, [source, state, 1])!,
            "real Fake Kraid right-spike producer allocates its projectile");

        var cases = new[]
        {
            (Kind: RoomEnemyProjectileKind.FakeKraidSpit,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.Spit,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpitSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeLeft,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeftSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeRight,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeRightSleep),
        };

        foreach (var testCase in cases)
        {
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                candidate => candidate.Kind == testCase.Kind);
            AssertEqual(testCase.Initial, projectile.InstructionPointer,
                $"{testCase.Kind} selects its named cartridge program");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} schedules its terminal sleep");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} sleeps at the authored opcode");
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{testCase.Kind} sleep leaves the timer stopped");

            projectile.InstructionPointer =
                CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            RunForcedTick(projectile);
            AssertTrue(!projectile.IsActive,
                $"{testCase.Kind} shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(
            FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentationWords.Count,
            "all live Fake Kraid projectile frames select installed operands");
        for (int index = 0;
             index < FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = FakeKraidProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(selectedPresentationWords.Contains(address),
                $"production execution selects Fake Kraid presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Fake Kraid frames never read presentation operands from ROM");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fake Kraid and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9ddc),
            "Fake Kraid spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9dec),
            "adjacent Fake Kraid initializer is rejected as mechanics");

        _ = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fake Kraid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fake Kraid projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Fake Kraid projectile instruction mechanics: six compiled words, all three " +
            "real spit/spike producers, three terminal sleeps, shared shot deletion, and " +
            "three installed frame selectors pass without instruction ROM reads.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            if (projectile.IsActive && projectile.InstructionTimer != 0)
            {
                ushort operand = (ushort)(projectile.InstructionPointer - 2);
                AssertEqual(operand, projectile.PresentationOperandAddress,
                    "Fake Kraid selects the just-executed native frame operand");
                AssertEqual(ReadFakeKraidProjectileInstructionWord(rom, (ushort)(operand - 2)),
                    projectile.InstructionTimer, "Fake Kraid frame retains the native duration");
                selectedPresentationWords.Add(projectile.PresentationOperandAddress);
            }
        }
    }

    /// <summary>Warms and repeatedly reads compiled Fake Kraid projectile mechanics so the caller can measure steady-state lookup allocations.</summary>
    /// <returns>A checksum that keeps the repeated reads observable.</returns>
    private static int ProbeFakeKraidProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FakeKraidProjectileInstructionProgramDefinitions.Spit
                    : FakeKraidProjectileInstructionProgramDefinitions.SpikeRight);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian projectile instruction word from the enemy-projectile bank.</summary>
    /// <param name="source">Retail address space containing the original projectile program.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The adjacent cartridge bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadFakeKraidProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps address-space access to reject reads of compiled Fake Kraid and shared-delete mechanics while observing presentation operand reads.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class FakeKraidProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique presentation operand addresses requested by production projectile execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected for targeting compiled instruction mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the same mechanics guard and presentation tracking as ordinary byte reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when it is not a compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled Fake Kraid or shared-delete mechanics, records presentation accesses, and forwards other byte requests.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is outside guarded mechanics ranges.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Fake Kraid or shared-delete mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fake Kraid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = FakeKraidProjectileInstructionProgramDefinitions
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

        /// <summary>Forwards writes unchanged; this wrapper restricts reads of compiled mechanics only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte value passed through to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
