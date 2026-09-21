using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonProjectileInstructionProgramDefinitions() =>
        VerifyBotwoonProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyBotwoonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < BotwoonProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BotwoonProjectileInstructionMechanicsWord definition =
                BotwoonProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Botwoon projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new BotwoonProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawnBody = typeof(RoomEnemySystem).GetMethod(
            "SpawnBotwoonBodySegment", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState, ushort>>(enemies);
        var spawnSpit = typeof(RoomEnemySystem).GetMethod(
            "SpawnBotwoonSpit", flags)!
            .CreateDelegate<Action<RoomEnemySlot, byte, ushort>>(enemies);
        var runBodyPreInstruction = typeof(RoomEnemySystem).GetMethod(
            "RunBotwoonBodyPreInstruction", flags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, byte>>(enemies);

        RoomEnemySlot head = enemies.Slots[0];
        head.XPosition = 128;
        head.YPosition = 96;
        var state = new BotwoonEnemyState(head);
        typeof(RoomEnemySystem).GetField("_botwoonState", flags)!.SetValue(enemies, state);
        spawnBody(head, state, 2);
        RoomEnemyProjectileSlot body = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonBody);
        AssertEqual(BotwoonProjectileInstructionProgramDefinitions.Hidden,
            body.InstructionPointer,
            "real Botwoon body producer starts a non-tail segment hidden");
        body.YPosition = 200;
        body.XVelocity = BotwoonProjectileCodePointers.BodyFallingFunction;
        runBodyPreInstruction(body, 0);
        AssertEqual(BotwoonProjectileCodePointers.BodyLandedFunction, body.XVelocity,
            "landed Botwoon body installs the cartridge RTS state");
        body.XVelocity = BotwoonProjectileCodePointers.LegacyBodyLandedFunction;
        runBodyPreInstruction(body, 0);
        AssertEqual(BotwoonProjectileCodePointers.LegacyBodyLandedFunction, body.XVelocity,
            "older debugger-state Botwoon landed sentinel remains restorable");

        for (int index = 0;
             index < BotwoonProjectileInstructionProgramDefinitions.BodyProgramCount;
             index++)
        {
            ushort program = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(index);
            body.InstructionPointer = program;
            body.InstructionTimer = 1;
            Run(body, 5);
        }

        spawnSpit(head, 0x40, 0x0180);
        RoomEnemyProjectileSlot spit = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonSpit);
        AssertEqual(BotwoonProjectileInstructionProgramDefinitions.Spit,
            spit.InstructionPointer,
            "real Botwoon spit producer selects the compiled animation loop");
        Run(spit, 6);
        AssertEqual((ushort)0x0003, spit.InstructionTimer,
            "Botwoon spit loop retains its three-frame cadence");

        AssertEqual(BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Botwoon body, tail, hidden, and spit spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Botwoon projectile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe811),
            "Botwoon body spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe84b),
            "unused adjacent Botwoon body program is rejected as mechanics");

        _ = ProbeBotwoonProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBotwoonProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Botwoon projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Botwoon projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Botwoon projectile instruction mechanics: seventy-three compiled words, " +
            "all seventeen body/tail programs and the real spit producer pass with " +
            "forty-six live spritemap reads and mechanics bytes forbidden.");

        void Run(RoomEnemyProjectileSlot projectile, int steps)
        {
            for (int step = 0; step < steps; step++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeBotwoonProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft
                    : BotwoonProjectileInstructionProgramDefinitions.Spit);
        }
        return checksum;
    }

    private sealed class BotwoonProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BotwoonProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Botwoon projectile mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = BotwoonProjectileInstructionProgramDefinitions
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
