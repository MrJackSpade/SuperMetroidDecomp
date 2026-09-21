using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyDeathInstructionProgramDefinitions() =>
        VerifyEnemyDeathInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyEnemyDeathInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EnemyDeathInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EnemyDeathInstructionMechanicsWord definition =
                EnemyDeathInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"generic enemy-death mechanics word $86:{definition.Address:X4}");
        }

        var guard = new EnemyDeathInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        (ushort Variant, ushort Initial, int CallsUntilTail, ushort Sound)[] programs =
        [
            (0, EnemyDeathInstructionProgramDefinitions.SmallExplosion, 7, 0x0009),
            (1, EnemyDeathInstructionProgramDefinitions.KilledBySamusContact, 17, 0x000b),
            (2, EnemyDeathInstructionProgramDefinitions.NormalExplosion, 7, 0x0009),
            (3, EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion, 17, 0x0024),
            (4, EnemyDeathInstructionProgramDefinitions.BigExplosion, 6, 0x0024),
        ];

        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
                .SetValue(enemies, (Func<ushort>)(() => 0x1234));
            typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!
                .SetValue(enemies, new SamusState { Health = 99, MaxHealth = 99 });

            var enemy = new RoomEnemySlot(0)
            {
                XPosition = 0x0080,
                YPosition = 0x0070,
            };
            enemies.StartGenericEnemyDeath(enemy, program.Variant);
            RoomEnemyProjectileSlot death = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
            AssertEqual(program.Initial, death.InstructionPointer,
                $"real death producer selects variant {program.Variant} named program");

            for (int call = 1; call <= program.CallsUntilTail; call++)
            {
                death.InstructionTimer = 1;
                Process(enemies, death);
                AssertTrue(death.IsActive,
                    $"death variant {program.Variant} remains active through call {call}");
                if (call < program.CallsUntilTail)
                {
                    AssertTrue(death.InstructionPointer !=
                        EnemyDeathInstructionProgramDefinitions.RespawnTail + 4,
                        $"death variant {program.Variant} does not convert early at call {call}");
                }
            }

            AssertEqual(
                unchecked((ushort)(EnemyDeathInstructionProgramDefinitions.RespawnTail + 4)),
                death.InstructionPointer,
                $"death variant {program.Variant} converts to the shared blank tail exactly");
            AssertEqual(program.Sound, enemies.LastEnemyDeathSoundEffectLibrary2!.Value,
                $"death variant {program.Variant} queues its authored sound");

            if (program.Variant is 3 or 4)
            {
                AssertTrue(enemies.RoomSpriteObjects.Any(sprite => sprite.IsActive),
                    $"death variant {program.Variant} executes its random sprite callbacks");
            }

            death.InstructionTimer = 1;
            Process(enemies, death);
            AssertTrue(!death.IsActive,
                $"death variant {program.Variant} executes the compiled respawn/delete tail");
        }

        AssertEqual(EnemyDeathInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all generic enemy-death spritemap operands remain cartridge reads");
        for (int index = 0;
             index < EnemyDeathInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EnemyDeathInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads generic enemy-death presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids all compiled generic enemy-death mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(0xed4d),
            "generic enemy-death spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(0xed87),
            "unused enemy-death list is not silently catalogued");

        _ = ProbeEnemyDeathInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEnemyDeathInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "generic enemy-death allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed generic enemy-death mechanics lookups allocate no storage");

        Console.WriteLine(
            "Generic enemy-death instruction mechanics: all five real death variants, " +
            "the shared respawn tail, 66 compiled words, and 31 live spritemap reads pass " +
            "with mechanics bytes forbidden.");

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile) =>
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
    }

    private static int ProbeEnemyDeathInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EnemyDeathInstructionProgramDefinitions.SmallExplosion
                    : EnemyDeathInstructionProgramDefinitions.BigExplosion);
        }
        return checksum;
    }

    private sealed class EnemyDeathInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EnemyDeathInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled generic enemy-death mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EnemyDeathInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        EnemyDeathInstructionProgramDefinitions.PresentationWordAddress(index);
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
