using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEyeDoorSweatInstructionProgramDefinitions()
    {
        VerifyEyeDoorSweatInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyEyeDoorSweatInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EyeDoorSweatInstructionMechanicsWord definition =
                EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadEyeDoorSweatInstructionWord(rom, definition.Address),
                $"Eye Door sweat mechanics word $86:{definition.Address:X4}");
        }

        var guard = new EyeDoorSweatInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        var runPreInstruction = typeof(RoomEnemySystem).GetMethod(
                "RunEyeDoorSweatPreInstruction",
                BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, RoomLevelData>>();

        const int roomWidth = 16;
        var foreground = new ushort[roomWidth * 16];
        foreground[5 * roomWidth + 6] =
            (ushort)((int)RoomCollisionType.SolidBlock << 12);
        RoomLevelData level = CreateRoom(
            roomWidth,
            16,
            foreground,
            new byte[foreground.Length],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState();
        var system = new Bank80SystemState();
        var request = new EyeDoorProjectileRequest(
            EyeDoorEnemyProjectileRomData.SweatDefinition,
            Parameter: 4,
            PlmBlockIndex: 4 * roomWidth + 7,
            DoorBit: 0);

        enemies.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot sweat = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial,
            sweat.InstructionPointer,
            "Eye Door sweat definition selects the named initial program");

        RunForcedTicks(sweat, 2);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial + 4,
            sweat.InstructionPointer,
            "Eye Door sweat falling loop returns and schedules its frame again");

        runPreInstruction(sweat, level);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Impact,
            sweat.InstructionPointer,
            "real floor collision selects the named Eye Door sweat impact program");
        AssertEqual(EyeDoorEnemyProjectileRomData.SmokeInertPreInstruction,
            sweat.PreInstruction,
            "Eye Door sweat impact disables movement with the native inert pre-instruction");
        AssertEqual((ushort)1, sweat.InstructionTimer,
            "Eye Door sweat impact requests a same-pass animation tick");
        AssertEqual((ushort)76, sweat.YPosition,
            "Eye Door sweat applies the native four-pixel floor-impact correction");

        RunForcedTicks(sweat, 4);
        AssertTrue(!sweat.IsActive,
            "Eye Door sweat impact program deletes after all three presentation frames");

        enemies.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot shot = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat &&
                projectile.IsActive);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(shot, 1);
        AssertTrue(!shot.IsActive,
            "Eye Door sweat shot reaction reaches the compiled shared delete program");

        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Eye Door sweat spritemap operands remain cartridge reads");
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Eye Door sweat presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Eye Door sweat and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb617),
            "Eye Door sweat spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb62d),
            "adjacent Eye Door origin table is rejected as sweat mechanics");

        _ = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Eye Door sweat allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Eye Door sweat mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Eye Door sweat instruction mechanics: eight compiled words, the complete " +
            "falling loop, real floor-impact handoff, three impact frames, shared shot " +
            "deletion, and four live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeEyeDoorSweatInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EyeDoorSweatInstructionProgramDefinitions.Initial
                    : EyeDoorSweatInstructionProgramDefinitions.Impact);
        }
        return checksum;
    }

    private static ushort ReadEyeDoorSweatInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class EyeDoorSweatInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EyeDoorSweatInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Eye Door sweat mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = EyeDoorSweatInstructionProgramDefinitions
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
