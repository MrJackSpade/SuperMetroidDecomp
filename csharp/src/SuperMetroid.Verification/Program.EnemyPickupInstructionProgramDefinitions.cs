using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyPickupInstructionProgramDefinitions() =>
        VerifyEnemyPickupInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyEnemyPickupInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < EnemyPickupInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EnemyPickupInstructionMechanicsWord definition =
                EnemyPickupInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"enemy-pickup mechanics word $86:{definition.Address:X4}");
        }

        var guard = new EnemyPickupInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo begin = typeof(RoomEnemySystem).GetMethod(
            "BeginEnemyPickup", staticFlags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;

        (EnemyPickupKind Kind, ushort Initial, int FrameCount, ushort Branch, ushort Sleep)[]
            programs =
            [
                (EnemyPickupKind.SmallEnergy,
                    EnemyPickupInstructionProgramDefinitions.SmallEnergy,
                    4, 0xed9d, 0xeda1),
                (EnemyPickupKind.BigEnergy,
                    EnemyPickupInstructionProgramDefinitions.BigEnergy,
                    4, 0xedb3, 0xedb7),
                (EnemyPickupKind.Missile,
                    EnemyPickupInstructionProgramDefinitions.Missiles,
                    2, 0xedc1, 0xedc5),
                (EnemyPickupKind.SuperMissile,
                    EnemyPickupInstructionProgramDefinitions.SuperMissiles,
                    2, 0xede5, 0xede9),
                (EnemyPickupKind.PowerBomb,
                    EnemyPickupInstructionProgramDefinitions.PowerBombs,
                    4, 0xedfb, 0),
            ];

        foreach (var program in programs)
        {
            var pickup = new RoomEnemyProjectileSlot(1)
            {
                Kind = RoomEnemyProjectileKind.EnemyDeathPickup,
            };
            begin.Invoke(null, [pickup, program.Kind]);
            AssertEqual(program.Initial, pickup.InstructionPointer,
                $"real {program.Kind} pickup initializer selects its named program");

            for (int frame = 0; frame < program.FrameCount; frame++)
            {
                pickup.InstructionTimer = 1;
                Process(pickup);
                AssertTrue(pickup.IsActive,
                    $"{program.Kind} remains active through animation frame {frame + 1}");
            }

            AssertEqual(program.Branch, pickup.InstructionPointer,
                $"{program.Kind} reaches its authored loop branch");
            pickup.InstructionTimer = 1;
            Process(pickup);
            AssertEqual(unchecked((ushort)(program.Initial + 4)), pickup.InstructionPointer,
                $"{program.Kind} branch displays its first frame in the same tick");

            if (program.Sleep != 0)
            {
                pickup.InstructionPointer = program.Sleep;
                pickup.InstructionTimer = 1;
                Process(pickup);
                AssertEqual(program.Sleep, pickup.InstructionPointer,
                    $"{program.Kind} terminal sleep retains its own opcode");
                AssertEqual((ushort)0, pickup.InstructionTimer,
                    $"{program.Kind} terminal sleep leaves the timer dormant");
            }
        }

        var convertedDeath = new RoomEnemyProjectileSlot(2)
        {
            Kind = RoomEnemyProjectileKind.EnemyDeathExplosion,
        };
        begin.Invoke(null, [convertedDeath, EnemyPickupKind.SmallEnergy]);
        convertedDeath.InstructionTimer = 1;
        Process(convertedDeath);
        AssertEqual(
            unchecked((ushort)(EnemyPickupInstructionProgramDefinitions.SmallEnergy + 4)),
            convertedDeath.InstructionPointer,
            "death actor converted in place uses the same compiled pickup program");

        AssertEqual(EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all enemy-pickup spritemap operands remain cartridge reads");
        for (int index = 0;
             index < EnemyPickupInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads enemy-pickup presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids all compiled enemy-pickup mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(0xed8f),
            "enemy-pickup spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(0xedc7),
            "unused bomb-pickup program is not silently catalogued");

        _ = ProbeEnemyPickupInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEnemyPickupInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "enemy-pickup allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed enemy-pickup mechanics lookups allocate no storage");

        Console.WriteLine(
            "Enemy-pickup instruction mechanics: thirty compiled words, all five live " +
            "programs, direct and converted-death owners, and sixteen live spritemap " +
            "reads pass with mechanics bytes forbidden.");

        void Process(RoomEnemyProjectileSlot projectile) =>
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
    }

    private static int ProbeEnemyPickupInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EnemyPickupInstructionProgramDefinitions.SmallEnergy
                    : EnemyPickupInstructionProgramDefinitions.PowerBombs);
        }
        return checksum;
    }

    private sealed class EnemyPickupInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EnemyPickupInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled enemy-pickup mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EnemyPickupInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
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
