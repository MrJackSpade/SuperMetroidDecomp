using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGoldenTorizoEggInstructionProgramDefinitions() =>
        VerifyGoldenTorizoEggInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyGoldenTorizoEggInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoEggInstructionMechanicsWord definition =
                GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Golden Torizo egg mechanics word $86:{definition.Address:X4}");
        }

        var guard = new GoldenTorizoEggInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnGoldenTorizoEgg", flags)!;
        MethodInfo runBounce = typeof(RoomEnemySystem).GetMethod(
            "RunGoldenTorizoEggPreInstruction", flags)!;
        MethodInfo runFall = typeof(RoomEnemySystem).GetMethod(
            "RunGoldenTorizoEggFall", flags)!;
        RoomLevelData emptyRoom = CreateEmptyRoom(32, 32);
        ushort[] floorWords = new ushort[32 * 32];
        for (int column = 0; column < 32; column++)
        {
            floorWords[10 * 32 + column] = RoomLevelWord.Create(
                0,
                LevelBlockFlipFlags.None,
                RoomCollisionType.SolidBlock).Raw;
        }
        RoomLevelData floorRoom = CreateRoom(
            32,
            32,
            floorWords,
            new byte[floorWords.Length]);

        foreach (bool movingRight in new[] { false, true })
        {
            var random = new Queue<ushort>([0x0080, 0x0080]);
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            torizo.Parameter1 = movingRight ? (ushort)0x8000 : (ushort)0;
            spawn.Invoke(enemies, [torizo]);
            RoomEnemyProjectileSlot egg = enemies.EnemyProjectiles[^1];
            ushort bouncing = movingRight
                ? GoldenTorizoEggInstructionProgramDefinitions.BouncingRight
                : GoldenTorizoEggInstructionProgramDefinitions.BouncingLeft;
            AssertEqual(bouncing, egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg bouncing program");
            AssertEqual((ushort)66, egg.Variable1,
                $"Golden Torizo {movingRight} egg preserves native fixed timer bug");

            for (int frame = 1; frame <= 49; frame++)
            {
                process.Invoke(enemies, [egg, null, (ushort)0, (ushort)0]);
                AssertTrue(egg.IsActive,
                    $"Golden Torizo {movingRight} egg bounce wait frame {frame}");
            }
            AssertEqual(unchecked((ushort)(bouncing + 4)), egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg sleeps after first pose");
            AssertEqual((ushort)0, egg.InstructionTimer,
                $"Golden Torizo {movingRight} egg sleep timer");

            egg.Variable1 = 0;
            runBounce.Invoke(enemies, [egg, emptyRoom]);
            AssertEqual(unchecked((ushort)(bouncing + 6)), egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg hatch skips sleep");
            AssertEqual((ushort)1, egg.InstructionTimer,
                $"Golden Torizo {movingRight} egg hatch arms instruction stream");
            AssertEqual(movingRight ? (ushort)256 : unchecked((ushort)-256), egg.XVelocity,
                $"Golden Torizo {movingRight} egg hatch horizontal launch");

            for (int frame = 1; frame <= 13; frame++)
            {
                process.Invoke(enemies, [egg, null, (ushort)0, (ushort)0]);
                AssertTrue(egg.IsActive,
                    $"Golden Torizo {movingRight} egg hatch frame {frame}");
            }
            ushort hatchedLoop = movingRight
                ? GoldenTorizoEggInstructionProgramDefinitions.HatchedRightLoop
                : GoldenTorizoEggInstructionProgramDefinitions.HatchedLeftLoop;
            AssertEqual(EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched,
                egg.PreInstruction,
                $"Golden Torizo {movingRight} egg installs horizontal-charge callback");
            AssertTrue(egg.CanDamageSamus,
                $"Golden Torizo {movingRight} egg hatch enables Samus collision");
            AssertTrue(egg.BlocksSamusProjectiles,
                $"Golden Torizo {movingRight} egg hatch enables shot collision");
            AssertEqual((ushort)6, egg.InstructionTimer,
                $"Golden Torizo {movingRight} egg enters hatched loop");
            AssertEqual(unchecked((ushort)(hatchedLoop + 4)), egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg hatched loop cursor");

            for (int frame = 1; frame <= 24; frame++)
                process.Invoke(enemies, [egg, null, (ushort)0, (ushort)0]);
            AssertEqual((ushort)6, egg.InstructionTimer,
                $"Golden Torizo {movingRight} egg reloads 24-frame hatched loop");
            AssertEqual(unchecked((ushort)(hatchedLoop + 4)), egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg stable hatched-loop cursor");

            egg.PreInstruction =
                EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall;
            egg.XPosition = 128;
            egg.YPosition = 153;
            egg.YSubposition = 0;
            egg.YVelocity = 256;
            runFall.Invoke(enemies, [egg, floorRoom]);
            ushort breakProgram = movingRight
                ? GoldenTorizoEggInstructionProgramDefinitions.BreakRight
                : GoldenTorizoEggInstructionProgramDefinitions.BreakLeft;
            AssertEqual(breakProgram, egg.InstructionPointer,
                $"Golden Torizo {movingRight} egg floor collision selects break program");
            AssertEqual((ushort)1, egg.InstructionTimer,
                $"Golden Torizo {movingRight} egg floor collision arms break program");

            int breakFrames = movingRight ? 24 : 26;
            for (int frame = 1; frame <= breakFrames; frame++)
            {
                process.Invoke(enemies, [egg, null, (ushort)0, (ushort)0]);
                AssertTrue(egg.IsActive,
                    $"Golden Torizo {movingRight} egg break frame {frame}");
                AssertEqual(EnemyProjectileCodePointers.RTS_868170, egg.PreInstruction,
                    $"Golden Torizo {movingRight} egg break clears callback");
            }
            process.Invoke(enemies, [egg, null, (ushort)0, (ushort)0]);
            AssertTrue(!egg.IsActive,
                $"Golden Torizo {movingRight} egg deletes after exact break lifetime");
            AssertEqual(0, random.Count,
                $"Golden Torizo {movingRight} egg consumes two launch RNG words");
        }

        var shotRandom = new Queue<ushort>(
        [
            0x0080, 0x0080,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
        ]);
        RoomEnemySystem shotEnemies = CreateSystem(guard, shotRandom);
        RoomEnemySlot shotTorizo = shotEnemies.Slots[0];
        shotTorizo.XPosition = 1000;
        shotTorizo.YPosition = 500;
        spawn.Invoke(shotEnemies, [shotTorizo]);
        RoomEnemyProjectileSlot shot = shotEnemies.EnemyProjectiles[^1];
        shot.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.WallImpact;
        shot.InstructionTimer = 1;
        shot.Variable0 = shot.XPosition;
        shot.Variable1 = shot.YPosition;
        for (int frame = 1; frame <= 20; frame++)
        {
            process.Invoke(shotEnemies, [shot, null, (ushort)0, (ushort)0]);
            AssertTrue(shot.IsActive, $"Golden Torizo egg shot-break frame {frame}");
        }
        process.Invoke(shotEnemies, [shot, null, (ushort)0, (ushort)0]);
        AssertTrue(!shot.IsActive,
            "Golden Torizo egg shot path completes shared Chozo-orb break program");

        AssertEqual(GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Golden Torizo egg spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Golden Torizo egg mechanics byte");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEggInstructionProgramDefinitions.ReadMechanicsWord(0xb106),
            "Golden Torizo egg spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEggInstructionProgramDefinitions.ReadMechanicsWord(0xb14d),
            "Golden Torizo egg packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEggInstructionProgramDefinitions.ReadMechanicsWord(0xb181),
            "unused adjacent egg program is rejected as live mechanics");

        _ = ProbeGoldenTorizoEggInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGoldenTorizoEggInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Golden Torizo egg allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Golden Torizo egg mechanics lookups allocate no storage");

        Console.WriteLine(
            "Golden Torizo egg instruction mechanics: fifty-three private words, both " +
            "real producers through bounce, hatch, charge, floor break, and the shared " +
            "shot break pass with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus, Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            return enemies;
        }
    }

    private static int ProbeGoldenTorizoEggInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GoldenTorizoEggInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GoldenTorizoEggInstructionProgramDefinitions.BouncingLeft
                    : GoldenTorizoEggInstructionProgramDefinitions.BreakRight);
        }
        return checksum;
    }

    private sealed class GoldenTorizoEggInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GoldenTorizoEggInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Golden Torizo egg byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = GoldenTorizoEggInstructionProgramDefinitions
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
