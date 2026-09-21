using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySaveStationElectricityInstructionProgramDefinitions() =>
        VerifySaveStationElectricityInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifySaveStationElectricityInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SaveStationElectricityInstructionMechanicsWord definition =
                SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"save-station electricity mechanics word $86:{definition.Address:X4}");
        }

        var guard = new SaveStationElectricityInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        enemies.SpawnSaveStationElectricity(plmBlockIndex: 42, roomWidthInBlocks: 16);
        RoomEnemyProjectileSlot electricity = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(SaveStationElectricityInstructionProgramDefinitions.Initial,
            electricity.InstructionPointer,
            "real save-station producer selects the named initial program");
        AssertEqual((ushort)0x00b0, electricity.XPosition,
            "save-station producer derives X from one block right of its PLM");
        AssertEqual((ushort)0x0000, electricity.YPosition,
            "save-station producer derives Y from two blocks above its PLM");

        RunForcedTick();
        AssertEqual((ushort)0x0014, electricity.GeneralTimer,
            "save-station electricity initializes twenty animation cycles");
        AssertEqual((ushort)0xe68b, electricity.InstructionPointer,
            "save-station electricity displays the first loop frame");

        for (int tick = 1; tick < 160; tick++)
        {
            AssertTrue(electricity.IsActive,
                $"save-station electricity remains active before displayed frame {tick + 1}");
            RunForcedTick();
        }

        AssertTrue(electricity.IsActive,
            "save-station electricity remains active after all 160 displayed frames");
        AssertEqual((ushort)1, electricity.GeneralTimer,
            "save-station electricity retains one count before its terminal branch");
        AssertEqual((ushort)0xe6a7, electricity.InstructionPointer,
            "save-station electricity reaches the terminal decrement branch");
        RunForcedTick();
        AssertTrue(!electricity.IsActive,
            "save-station electricity exits its twentieth cycle through private deletion");

        RoomEnemySystem shotSystem = NewSystem();
        shotSystem.SpawnSaveStationElectricity(plmBlockIndex: 42, roomWidthInBlocks: 16);
        RoomEnemyProjectileSlot shot = shotSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = 1;
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "save-station electricity shot reaction reaches shared compiled deletion");

        AssertEqual(SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all save-station electricity spritemap operands remain cartridge reads");
        for (int index = 0;
             index < SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = SaveStationElectricityInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads save-station electricity presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared save-station electricity mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(0xe689),
            "save-station electricity spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(0xe6ad),
            "save-station electricity initializer body is rejected as mechanics");

        _ = ProbeSaveStationElectricityInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSaveStationElectricityInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "save-station electricity allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed save-station electricity mechanics lookups allocate no storage");

        Console.WriteLine(
            "Save-station electricity instruction mechanics: thirteen compiled words, the " +
            "real producer, 160 displayed frames, both delete paths, and eight live " +
            "spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            return system;
        }

        void RunForcedTick()
        {
            electricity.InstructionTimer = 1;
            Process(enemies, electricity);
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeSaveStationElectricityInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SaveStationElectricityInstructionProgramDefinitions.Initial
                    : SaveStationElectricityInstructionProgramDefinitions.Loop);
        }
        return checksum;
    }

    private sealed class SaveStationElectricityInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SaveStationElectricityInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled save-station electricity mechanics byte " +
                    $"${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SaveStationElectricityInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = SaveStationElectricityInstructionProgramDefinitions
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
