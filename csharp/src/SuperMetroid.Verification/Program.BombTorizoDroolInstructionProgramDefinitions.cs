using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBombTorizoDroolVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xa46c,0xa470,0xa47c,0xa484,0xa492,0xa496,0xa49a];
        foreach (ushort operand in operands)
        {
            int address = 0x860000 | operand;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(operand), "drool native visual pointer");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86,operand,out ushort shared), "drool shared selector found");
            AssertEqual(native, shared, "drool shared native pointer");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(address), "drool excluded from literal regeneration");
        }
        foreach (ushort pointer in new ushort[] {0x8c54,0x8c5b,0x8c62,0x8c69})
            AssertEqual((ushort)1, (ushort)(rom.ReadByte(0x8d0000 | pointer) | rom.ReadByte(0x8d0000 | (pointer + 1)) << 8), "drool native single-entry sprite record");
        AssertEqual((byte)0, rom.ReadByte(0x8d8000), "drool blank map count low byte");
        AssertEqual((byte)0, rom.ReadByte(0x8d8001), "drool blank map count high byte");
        var known = operands.ToHashSet();
        for (int address = 0xa468; address <= 0xa49e; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt((ushort)address), "drool rejects controls, odd and adjacent visual operands");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x86,(ushort)address,out ushort missing), "drool shared holes rejected");
                AssertEqual((ushort)0, missing, "drool missing output cleared");
            }
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(address), "drool distant invalid visual operand");
    }
    private static void VerifyBombTorizoDroolMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = [0xa46a,0xa46e,0xa472,0xa474,0xa476,0xa478,0xa47a,0xa47e,0xa480,
            0xa482,0xa486,0xa488,0xa48a,0xa48c,0xa48e,0xa490,0xa494,0xa498,0xa49c];
        AssertEqual(expected.Length, BombTorizoDroolInstructionProgramDefinitions.MechanicsWordCount, "drool native control count");
        var bytes = new HashSet<int>();
        for (int i = 0; i < expected.Length; i++)
        {
            var actual = BombTorizoDroolInstructionProgramDefinitions.MechanicsWord(i);
            AssertEqual(expected[i], actual.Address, "drool native control order");
            int address = 0x860000 | expected[i];
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, actual.Value, "drool enumerated native control");
            AssertEqual(native, BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(expected[i]), "drool direct native control");
            bytes.Add(expected[i]); bytes.Add(expected[i] + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), BombTorizoDroolInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address), "drool full byte ownership");
            AssertEqual(bytes.Contains(address), BombTorizoDroolInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1860000 | address), "drool existing high-bit alias");
            AssertTrue(!BombTorizoDroolInstructionProgramDefinitions.IsCompiledMechanicsByte(0x870000 | address), "drool other bank rejected");
        }
        var words = expected.ToHashSet();
        for (int address = 0xa468; address <= 0xa4a0; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "drool rejects visual, odd and adjacent words");
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(address), "drool distant invalid words");
        foreach (int index in new[] {int.MinValue,-1,19,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BombTorizoDroolInstructionProgramDefinitions.MechanicsWord(index), "drool mechanics ordinal bounds");
    }
    private static void VerifyBombTorizoDroolPresentationMapping()
    {
        ushort[] expected = [0xa46c,0xa470,0xa47c,0xa484,0xa492,0xa496,0xa49a];
        AssertEqual(expected.Length, BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount, "drool native operand count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(i), "drool native operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)address), "drool full visual membership");
        foreach (int index in new[] {int.MinValue,-1,7,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index), "drool operand ordinal bounds");
    }
    private static void VerifyBombTorizoDroolInitialSelection(SuperMetroidAddressSpace rom)
    {
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            // Native LSR, AND #000E produces the byte offset, independently of the modulo conversion.
            int address = 0x86a64d + ((random >> 1) & 0x000e);
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, BombTorizoDroolInstructionProgramDefinitions.SelectLowHealthInitialProgram((ushort)random),
                "Bomb Torizo drool native delay selection for all RNG words");
        }
    }
    private static void VerifyBombTorizoDroolInstructionProgramDefinitions() =>
        Suite(nameof(VerifyBombTorizoDroolInstructionProgramDefinitions), () => VerifyBombTorizoDroolInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    private static void VerifyBombTorizoDroolInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBombTorizoDroolInitialSelection), () => VerifyBombTorizoDroolInitialSelection(rom));
        Suite(nameof(VerifyBombTorizoDroolMechanicsMapping), () => VerifyBombTorizoDroolMechanicsMapping(rom));
        Suite(nameof(VerifyBombTorizoDroolPresentationMapping), () => VerifyBombTorizoDroolPresentationMapping());
        Suite(nameof(VerifyBombTorizoDroolVisualMapping), () => VerifyBombTorizoDroolVisualMapping(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < BombTorizoDroolInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BombTorizoDroolInstructionMechanicsWord definition =
                BombTorizoDroolInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Bomb Torizo drool mechanics word $86:{definition.Address:X4}");
        }

        var guard = new BombTorizoDroolInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        ushort[] expectedPrograms =
        [
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
        ];

        for (int selection = 0; selection < expectedPrograms.Length; selection++)
        {
            var random = new Queue<ushort>(
                [unchecked((ushort)(selection << 2)), 0x0037]);
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 0x0400;
            torizo.YPosition = 0x0200;
            torizo.Parameter1 = 0x8000;
            typeof(RoomEnemySystem).GetMethod(
                "SpawnBombTorizoLowHealthDrool", flags)!.Invoke(enemies, [torizo]);

            RoomEnemyProjectileSlot drool = enemies.EnemyProjectiles[^1];
            AssertEqual(expectedPrograms[selection], drool.InstructionPointer,
                $"drool selector {selection} initial program");
            AssertEqual(0, random.Count,
                $"drool selector {selection} consumes selection and trajectory RNG");
            AssertEqual((ushort)0x0408, drool.XPosition,
                $"drool selector {selection} X origin");
            AssertEqual((ushort)0x01fb, drool.YPosition,
                $"drool selector {selection} Y origin");

            int delay = expectedPrograms[selection] switch
            {
                BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay => 4,
                BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay => 2,
                _ => 0,
            };
            for (int frame = 0; frame < delay; frame++)
            {
                process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
                AssertEqual((ushort)(delay == 4 && frame < 2 ? 0xa46c : 0xa470), drool.PresentationOperandAddress,
                    "drool delay installs its original presentation operand" );
                AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                    $"drool selector {selection} stays high priority during delay {frame + 1}");
            }
            process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual((ushort)0xa47c, drool.PresentationOperandAddress, "drool first falling pose operand" );
            AssertEqual(BombTorizoDroolInstructionProgramDefinitions.FallingPreInstruction,
                drool.PreInstruction,
                $"drool selector {selection} installs falling callback after delay");
            AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                $"drool selector {selection} first five-frame pose is high priority");

            for (int frame = 1; frame < 5; frame++)
                process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                $"drool selector {selection} keeps priority through five visible frames");
            process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual(EnemyProjectileDrawPriority.Low, drool.DrawPriority,
                $"drool selector {selection} clears priority before 64-frame loop");
            AssertEqual((ushort)0xa484, drool.PresentationOperandAddress, "drool loop pose operand" );
            AssertEqual((ushort)0x0040, drool.InstructionTimer,
                $"drool selector {selection} installs exact loop duration");
        }

        var initialRandom = new Queue<ushort>([0x001f, 0x0003]);
        RoomEnemySystem initialEnemies = CreateSystem(guard, initialRandom);
        RoomEnemySlot initialTorizo = initialEnemies.Slots[0];
        initialTorizo.XPosition = 0x0500;
        initialTorizo.YPosition = 0x0300;
        initialTorizo.Parameter1 = 0x4000;
        typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoInitialDrool", flags)!.Invoke(initialEnemies, [initialTorizo]);
        RoomEnemyProjectileSlot initial = initialEnemies.EnemyProjectiles[^1];
        AssertEqual(BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            initial.InstructionPointer, "initial gut-break drool bypasses delay selector");
        AssertEqual((ushort)0x02fe, initial.YPosition, "initial drool random Y origin");
        AssertEqual((ushort)79, initial.YVelocity, "initial drool random Y velocity");
        AssertEqual((ushort)0x0503, initial.XPosition, "initial drool turning X origin");
        AssertEqual(0, initialRandom.Count, "initial drool consumes exactly two RNG words");

        initial.InstructionPointer = BombTorizoDroolInstructionProgramDefinitions.WallImpact;
        initial.InstructionTimer = 1;
        process.Invoke(initialEnemies, [initial, null, (ushort)0, (ushort)0]);
        AssertTrue(!initial.IsActive, "drool wall impact clears callback and deletes immediately");

        var floorRandom = new Queue<ushort>([0, 0]);
        RoomEnemySystem floorEnemies = CreateSystem(guard, floorRandom);
        RoomEnemySlot floorTorizo = floorEnemies.Slots[0];
        floorTorizo.XPosition = 0x0500;
        floorTorizo.YPosition = 0x0300;
        typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoInitialDrool", flags)!.Invoke(floorEnemies, [floorTorizo]);
        RoomEnemyProjectileSlot floor = floorEnemies.EnemyProjectiles[^1];
        floor.PreInstruction = BombTorizoDroolInstructionProgramDefinitions.FallingPreInstruction;
        floor.InstructionPointer = BombTorizoDroolInstructionProgramDefinitions.FloorImpact;
        floor.InstructionTimer = 1;
        for (int frame = 0; frame < 24; frame++)
        {
            process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
            AssertEqual((ushort)(0xa492 + 4 * (frame / 8)), floor.PresentationOperandAddress, "drool impact pose follows native eight-tick sequence" );
            AssertTrue(floor.IsActive,
                $"drool floor impact remains active through frame {frame + 1}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, floor.PreInstruction,
                $"drool floor impact callback is clear on frame {frame + 1}");
        }
        process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
        AssertTrue(!floor.IsActive,
            "drool floor impact deletes on the tick after three eight-frame poses");

        AssertEqual(0,
            guard.ObservedPresentationWords.Count,
            "Bomb Torizo drool uses installed presentation operands without ROM reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Bomb Torizo drool mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(0xa47c),
            "Bomb Torizo drool spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(0xa49e),
            "adjacent unused Torizo program is rejected as drool mechanics");

        _ = ProbeBombTorizoDroolInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBombTorizoDroolInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bomb Torizo drool allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bomb Torizo drool mechanics lookups allocate no storage");

        Console.WriteLine(
            "Bomb Torizo drool instruction mechanics: nineteen compiled words, all " +
            "eight native delay selections, exact two-word RNG sequencing, priority " +
            "handoff, and both impact lifetimes pass with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(
            ISnesAddressSpace bus,
            Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            return enemies;
        }
    }

    private static int ProbeBombTorizoDroolInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? BombTorizoDroolInstructionProgramDefinitions.NoDelay
                    : BombTorizoDroolInstructionProgramDefinitions.FloorImpact);
        }
        return checksum;
    }

    private sealed class BombTorizoDroolInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BombTorizoDroolInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Bomb Torizo drool byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = BombTorizoDroolInstructionProgramDefinitions
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
