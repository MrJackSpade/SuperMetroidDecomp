using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGunshipDustInstructionProgramDefinitions() =>
        VerifyGunshipDustInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyGunshipDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GunshipDustInstructionMechanicsWord definition =
                GunshipDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Gunship dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = new GunshipDustInstructionReadGuard(rom);
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnGunshipLiftoffDustCloud", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        for (int programIndex = 0;
             programIndex < GunshipDustInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort parameter = unchecked((ushort)(programIndex * 2));
            GunshipDustInstructionProgramDefinition program =
                GunshipDustInstructionProgramDefinitions.Program(programIndex);
            RoomEnemySystem enemies = NewSystem();
            var samus = new SamusState { XPosition = 0x1234, YPosition = 0x5678 };
            spawn.Invoke(enemies, [parameter, samus]);
            RoomEnemyProjectileSlot dust = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            AssertEqual(
                GunshipDustInstructionProgramDefinitions.InitialForParameter(parameter),
                dust.InstructionPointer,
                "real gunship dust producer selects its named program");
            AssertEqual(program.Initial, dust.InstructionPointer,
                "gunship dust parameter and program catalogs agree");

            for (int frame = 0; frame < program.Durations.Length; frame++)
            {
                RunForcedTick(enemies, dust);
                AssertEqual(program.Durations[frame], dust.InstructionTimer,
                    $"gunship dust program {programIndex} frame {frame} duration");
                AssertEqual(
                    unchecked((ushort)(program.FirstFrame + (frame + 1) * 4)),
                    dust.InstructionPointer,
                    $"gunship dust program {programIndex} frame {frame} continuation");
            }

            AssertEqual((ushort)1, dust.GeneralTimer,
                $"gunship dust program {programIndex} initializes one animation pass");
            RunForcedTick(enemies, dust);
            AssertTrue(!dust.IsActive,
                $"gunship dust program {programIndex} exits its decrement branch and deletes");
        }

        AssertEqual(GunshipDustInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all gunship dust spritemap operands remain cartridge reads");
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GunshipDustInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads gunship dust presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids gunship dust mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa19d),
            "gunship dust spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa2e2),
            "gunship dust selector-table data is rejected as mechanics");

        _ = ProbeGunshipDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGunshipDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "gunship dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed gunship dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Gunship dust instruction mechanics: seventy-six compiled words, all six " +
            "real producers, frame loops, deletions, and forty-six live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeGunshipDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GunshipDustInstructionProgramDefinitions.Index0
                    : GunshipDustInstructionProgramDefinitions.IndexA);
        }
        return checksum;
    }

    private sealed class GunshipDustInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GunshipDustInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled gunship dust mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GunshipDustInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = GunshipDustInstructionProgramDefinitions
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
