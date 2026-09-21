using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEyeDoorProjectileInstructionProgramDefinitions()
    {
        VerifyEyeDoorProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyEyeDoorProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EyeDoorProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EyeDoorProjectileInstructionMechanicsWord definition =
                EyeDoorProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadEyeDoorProjectileInstructionWord(rom, definition.Address),
                $"Eye Door projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new EyeDoorProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        var runPreInstruction = typeof(RoomEnemySystem).GetMethod(
                "RunEyeDoorProjectilePreInstruction",
                instanceFlags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, RoomLevelData>>(enemies);
        RoomLevelData level = CreateRoom(
            16,
            16,
            new ushort[16 * 16],
            new byte[16 * 16],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState
        {
            XPosition = 220,
            YPosition = 80,
        };
        var system = new Bank80SystemState();
        var request = new EyeDoorProjectileRequest(
            EyeDoorEnemyProjectileRomData.ProjectileDefinition,
            Parameter: 0,
            PlmBlockIndex: 4 * 16 + 7,
            DoorBit: 5);

        enemies.SpawnEyeDoorProjectile(request, roomWidthInBlocks: 16, system);
        RoomEnemyProjectileSlot impact = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorProjectile);
        AssertEqual(
            EyeDoorProjectileInstructionProgramDefinitions.Initial,
            impact.InstructionPointer,
            "Eye Door attack definition selects the named initial program");

        RunForcedTicks(impact, 4);
        AssertEqual(
            EyeDoorProjectileInstructionProgramDefinitions.FlyingLoop + 4,
            impact.InstructionPointer,
            "Eye Door initial program reaches its flying loop frame");
        AssertEqual(EyeDoorEnemyProjectileRomData.ProjectilePreInstruction,
            impact.PreInstruction,
            "Eye Door setup installs the native flight pre-instruction operand");
        impact.InstructionTimer = 1;
        process.Invoke(enemies, [impact, samus, (ushort)0, (ushort)0]);
        AssertEqual(
            EyeDoorProjectileInstructionProgramDefinitions.FlyingLoop + 4,
            impact.InstructionPointer,
            "Eye Door flying loop returns and schedules its frame again");

        system.SetOpenedDoorBit(5);
        runPreInstruction(impact, level);
        AssertEqual(
            EyeDoorProjectileInstructionProgramDefinitions.Impact,
            impact.InstructionPointer,
            "opened door selects the named Eye Door impact program");
        AssertEqual((ushort)1, impact.InstructionTimer,
            "Eye Door impact handoff requests a same-pass animation tick");
        RunForcedTicks(impact, 4);
        AssertTrue(!impact.IsActive,
            "Eye Door impact program deletes after all three presentation frames");

        enemies.SpawnEyeDoorProjectile(request, roomWidthInBlocks: 16, system);
        RoomEnemyProjectileSlot shot = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorProjectile);
        shot.InstructionPointer = EyeDoorProjectileInstructionProgramDefinitions.Shot;
        RunForcedTicks(shot, 5);
        AssertTrue(!shot.IsActive,
            "Eye Door shot program deletes after all four presentation frames");

        AssertEqual(
            EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Eye Door projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EyeDoorProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Eye Door presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Eye Door projectile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EyeDoorProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xb5db),
            "Eye Door spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EyeDoorProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xb615),
            "adjacent Eye Door sweat program is rejected as projectile mechanics");

        _ = ProbeEyeDoorProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEyeDoorProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Eye Door allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Eye Door projectile mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Eye Door projectile instruction mechanics: nineteen compiled words, complete " +
            "aim/flying/impact/shot execution, the real opened-door handoff, and eleven " +
            "live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeEyeDoorProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EyeDoorProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EyeDoorProjectileInstructionProgramDefinitions.Initial
                    : EyeDoorProjectileInstructionProgramDefinitions.Shot);
        }
        return checksum;
    }

    private static ushort ReadEyeDoorProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class EyeDoorProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EyeDoorProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Eye Door mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = EyeDoorProjectileInstructionProgramDefinitions
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
