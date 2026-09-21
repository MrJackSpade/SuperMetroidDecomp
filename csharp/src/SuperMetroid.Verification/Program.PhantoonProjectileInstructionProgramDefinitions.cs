using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPhantoonProjectileInstructionProgramDefinitions() =>
        VerifyPhantoonProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyPhantoonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PhantoonProjectileInstructionMechanicsWord definition =
                PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value, ReadWord(rom, definition.Address),
                $"Phantoon projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new PhantoonProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnStarting = typeof(RoomEnemySystem).GetMethod(
            "SpawnPhantoonStartingFlame", flags)!;
        MethodInfo spawnDestroyable = typeof(RoomEnemySystem).GetMethod(
            "SpawnPhantoonDestroyableFlame", flags)!;

        RoomEnemySystem startingSystem = NewSystem();
        RoomEnemySlot startingOwner = startingSystem.Slots[0];
        startingOwner.XPosition = 128;
        startingOwner.YPosition = 96;
        spawnStarting.Invoke(startingSystem, [startingOwner, (byte)0]);
        RoomEnemyProjectileSlot starting = startingSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.StartingFlame,
            starting.InstructionPointer, "real Phantoon starting-flame producer");
        RunForcedTicks(startingSystem, starting, 4);
        AssertEqual((ushort)0x97ec, starting.InstructionPointer,
            "Phantoon starting flame completes three frames and loops");

        RoomEnemySystem fallingSystem = NewSystem();
        RoomEnemyProjectileSlot falling = SpawnDestroyable(fallingSystem, 0);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.CasualFalling,
            falling.InstructionPointer, "real Phantoon casual-flame producer");
        RunForcedTicks(fallingSystem, falling, 25);
        AssertEqual((ushort)0x97bc, falling.InstructionPointer,
            "Phantoon casual flame completes all three blink phases and loops");

        RoomEnemySystem idleSystem = NewSystem();
        RoomEnemyProjectileSlot idle = SpawnDestroyable(idleSystem, 0x0200);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle,
            idle.InstructionPointer, "real Phantoon rage-flame producer");
        RunForcedTicks(idleSystem, idle, 4);
        AssertEqual((ushort)0x9760, idle.InstructionPointer,
            "Phantoon idle flame completes three frames and loops");

        RoomEnemySystem hitGroundSystem = NewSystem();
        RoomEnemyProjectileSlot hitGround = NewDestroyable(
            hitGroundSystem, PhantoonProjectileInstructionProgramDefinitions.CasualHitGround);
        RunForcedTicks(hitGroundSystem, hitGround, 2);
        AssertEqual((ushort)0x9770, hitGround.InstructionPointer,
            "Phantoon ground-impact pose reaches terminal sleep");
        AssertEqual((ushort)0, hitGround.InstructionTimer,
            "Phantoon terminal sleep preserves the native zero timer");

        RoomEnemySystem bouncingSystem = NewSystem();
        RoomEnemyProjectileSlot bouncing = NewDestroyable(
            bouncingSystem, PhantoonProjectileInstructionProgramDefinitions.CasualBouncing);
        RunForcedTicks(bouncingSystem, bouncing, 4);
        AssertEqual((ushort)0x9776, bouncing.InstructionPointer,
            "Phantoon bouncing flame completes three frames and loops");

        RoomEnemySystem restingSystem = NewSystem();
        RoomEnemyProjectileSlot resting = NewDestroyable(
            restingSystem, PhantoonProjectileInstructionProgramDefinitions.CasualResting);
        RunForcedTicks(restingSystem, resting, 11);
        AssertTrue(!resting.IsActive,
            "Phantoon resting flame falls through resting and dying frames to delete");

        RoomEnemySystem rainSystem = NewSystem();
        RoomEnemyProjectileSlot rain = NewDestroyable(
            rainSystem, PhantoonProjectileInstructionProgramDefinitions.RainImpact);
        RunForcedTicks(rainSystem, rain, 6);
        AssertTrue(!rain.IsActive,
            "Phantoon rain impact enters the dying program and deletes");

        RoomEnemySystem deleteSystem = NewSystem();
        RoomEnemyProjectileSlot deleting = NewDestroyable(
            deleteSystem, PhantoonProjectileInstructionProgramDefinitions.Delete);
        RunForcedTicks(deleteSystem, deleting, 1);
        AssertTrue(!deleting.IsActive, "Phantoon orbit expiry reaches private deletion");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = NewDestroyable(
            shotSystem, PhantoonProjectileInstructionProgramDefinitions.DestroyableShot);
        RunForcedTicks(shotSystem, shot, 5);
        AssertTrue(!shot.IsActive,
            "Phantoon shot flame completes four frames, requests a drop, and deletes");
        AssertEqual(1, shotSystem.PhantoonFlameDropRequests.Count,
            "Phantoon shot callback publishes exactly one drop request");

        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Phantoon spritemap operands remain cartridge reads");
        for (int index = 0;
             index < PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = PhantoonProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads Phantoon presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Phantoon mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x975e),
            "Phantoon spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x980e),
            "Phantoon drop callback body is rejected as mechanics");

        _ = ProbePhantoonProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePhantoonProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Phantoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Phantoon mechanics lookups allocate no storage");

        Console.WriteLine(
            "Phantoon projectile instruction mechanics: fifty-eight compiled words, " +
            "both real producers, every flame program, callbacks, and thirty-one live " +
            "spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies, (Func<ushort>)(() => 0x0080));
            typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
                enemies, new SamusState { Health = 99, MaxHealth = 99 });
            return enemies;
        }

        RoomEnemyProjectileSlot SpawnDestroyable(RoomEnemySystem enemies, ushort parameter)
        {
            RoomEnemySlot body = enemies.Slots[0];
            body.XPosition = 128;
            body.YPosition = 96;
            spawnDestroyable.Invoke(enemies, [body, parameter]);
            return enemies.EnemyProjectiles.Single(projectile => projectile.Kind ==
                RoomEnemyProjectileKind.PhantoonDestroyableFlame);
        }

        RoomEnemyProjectileSlot NewDestroyable(RoomEnemySystem enemies, ushort program)
        {
            RoomEnemyProjectileSlot projectile = SpawnDestroyable(enemies, 0x0200);
            projectile.InstructionPointer = program;
            projectile.InstructionTimer = 1;
            return projectile;
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            }
        }

        static ushort ReadWord(SuperMetroidAddressSpace source, ushort address) =>
            unchecked((ushort)(
                source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
                source.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(address + 1))) << 8));
    }

    private static int ProbePhantoonProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle
                    : PhantoonProjectileInstructionProgramDefinitions.DestroyableShot);
        }
        return checksum;
    }

    private sealed class PhantoonProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PhantoonProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Phantoon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = PhantoonProjectileInstructionProgramDefinitions
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
