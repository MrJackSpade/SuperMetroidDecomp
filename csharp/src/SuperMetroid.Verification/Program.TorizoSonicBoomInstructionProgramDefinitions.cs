using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyTorizoSonicBoomInstructionProgramDefinitions() =>
        VerifyTorizoSonicBoomInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyTorizoSonicBoomInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoSonicBoomInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoSonicBoomInstructionMechanicsWord definition =
                TorizoSonicBoomInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo sonic-boom mechanics word $86:{definition.Address:X4}");
        }

        var guard = new TorizoSonicBoomInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnBomb = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoSonicBoom", flags)!;
        MethodInfo spawnGolden = typeof(RoomEnemySystem).GetMethod(
            "SpawnGoldenTorizoSonicBoom", flags)!;
        MethodInfo runMovement = typeof(RoomEnemySystem).GetMethod(
            "RunBombTorizoSonicBoomPreInstruction", flags)!;

        foreach (bool golden in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        {
            var random = new Queue<ushort>([facingRight ? (ushort)1 : (ushort)0]);
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight);
            (golden ? spawnGolden : spawnBomb).Invoke(enemies, [torizo, (ushort)0]);
            RoomEnemyProjectileSlot boom = enemies.EnemyProjectiles[^1];
            ushort expectedProgram = facingRight
                ? TorizoSonicBoomInstructionProgramDefinitions.FiredRight
                : TorizoSonicBoomInstructionProgramDefinitions.FiredLeft;
            AssertEqual(expectedProgram, boom.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom program");
            AssertEqual(unchecked((ushort)(1000 + (facingRight ? 32 : -32))),
                boom.XPosition,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom X origin");
            AssertEqual(unchecked((ushort)(500 + (facingRight ? -12 : 20))),
                boom.YPosition,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom random Y origin");
            AssertEqual(facingRight ? (ushort)624 : unchecked((ushort)-624),
                boom.XVelocity,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom velocity");

            for (int frame = 1; frame <= 93; frame++)
            {
                process.Invoke(enemies, [boom, null, (ushort)0, (ushort)0]);
                AssertTrue(boom.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom frame {frame}");
            }
            ushort movingProgram = facingRight
                ? TorizoSonicBoomInstructionProgramDefinitions.MovingRight
                : TorizoSonicBoomInstructionProgramDefinitions.MovingLeft;
            AssertEqual((ushort)0x0050, boom.InstructionTimer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} reloads moving pose");
            AssertEqual(unchecked((ushort)(movingProgram + 4)), boom.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} moving loop cursor");
            AssertEqual(0, random.Count,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} launch RNG count");
        }

        ushort[] wallWords = new ushort[32 * 32];
        for (int row = 0; row < 32; row++)
        {
            wallWords[row * 32 + 10] = RoomLevelWord.Create(
                0,
                LevelBlockFlipFlags.None,
                RoomCollisionType.SolidBlock).Raw;
        }
        RoomLevelData wallRoom = CreateRoom(
            32,
            32,
            wallWords,
            new byte[wallWords.Length]);
        var impactRandom = new Queue<ushort>(
        [
            0,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
        ]);
        RoomEnemySystem impactEnemies = CreateSystem(guard, impactRandom);
        RoomEnemySlot impactTorizo = CreateTorizo(impactEnemies, facingRight: true);
        spawnBomb.Invoke(impactEnemies, [impactTorizo, (ushort)0]);
        RoomEnemyProjectileSlot impact = impactEnemies.EnemyProjectiles[^1];
        impact.XPosition = 157;
        impact.YPosition = 128;
        impact.XSubposition = 0;
        impact.XVelocity = 624;
        impact.BlocksSamusProjectiles = true;
        impact.CanDamageSamus = true;
        impact.DrawPriority = EnemyProjectileDrawPriority.Low;
        runMovement.Invoke(impactEnemies, [impact, wallRoom]);
        AssertEqual(TorizoSonicBoomInstructionProgramDefinitions.WallImpact,
            impact.InstructionPointer,
            "Torizo sonic boom wall collision selects compiled impact program");
        AssertEqual((ushort)1, impact.InstructionTimer,
            "Torizo sonic boom wall collision arms impact immediately");
        AssertEqual((ushort)157, impact.Variable0,
            "Torizo sonic boom retains wall-aligned impact X center");
        AssertEqual((ushort)128, impact.Variable1,
            "Torizo sonic boom retains impact Y center");

        for (int frame = 1; frame <= 60; frame++)
        {
            process.Invoke(impactEnemies, [impact, null, (ushort)0, (ushort)0]);
            AssertTrue(impact.IsActive, $"Torizo sonic-boom impact frame {frame}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, impact.PreInstruction,
                $"Torizo sonic-boom impact clears movement on frame {frame}");
            AssertTrue(!impact.BlocksSamusProjectiles,
                $"Torizo sonic-boom impact disables shot collision on frame {frame}");
            AssertTrue(!impact.CanDamageSamus,
                $"Torizo sonic-boom impact disables Samus collision on frame {frame}");
            AssertEqual(EnemyProjectileDrawPriority.High, impact.DrawPriority,
                $"Torizo sonic-boom impact is high priority on frame {frame}");
            if ((frame - 1) % 12 == 0)
            {
                int cycle = (frame - 1) / 12;
                AssertEqual((ushort)158, impact.XPosition,
                    $"Torizo sonic-boom impact cycle {cycle + 1} X jitter");
                AssertEqual((ushort)130, impact.YPosition,
                    $"Torizo sonic-boom impact cycle {cycle + 1} Y jitter");
                AssertEqual(unchecked((ushort)(5 - cycle)), impact.GeneralTimer,
                    $"Torizo sonic-boom impact cycle {cycle + 1} timer");
            }
        }
        process.Invoke(impactEnemies, [impact, null, (ushort)0, (ushort)0]);
        AssertTrue(!impact.IsActive,
            "Torizo sonic-boom impact deletes after five exact twelve-frame cycles");
        AssertEqual(0, impactRandom.Count,
            "Torizo sonic-boom launch and five jitter cycles consume exact RNG words");

        AssertEqual(TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Torizo sonic-boom spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo sonic-boom mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadc4),
            "Torizo sonic-boom spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadc1),
            "Torizo sonic-boom packed left sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadd4),
            "Torizo sonic-boom packed right sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xae15),
            "adjacent sonic-boom initialization AI is rejected as program mechanics");

        _ = ProbeTorizoSonicBoomInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoSonicBoomInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo sonic-boom allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo sonic-boom mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo sonic-boom instruction mechanics: thirty-one compiled words, four " +
            "real launch loops, the real wall handoff, and five exact impact cycles pass " +
            "with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus, Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            return enemies;
        }

        static RoomEnemySlot CreateTorizo(RoomEnemySystem enemies, bool facingRight)
        {
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
            return torizo;
        }
    }

    private static int ProbeTorizoSonicBoomInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoSonicBoomInstructionProgramDefinitions.FiredLeft
                    : TorizoSonicBoomInstructionProgramDefinitions.WallImpact);
        }
        return checksum;
    }

    private sealed class TorizoSonicBoomInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TorizoSonicBoomInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo sonic-boom byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoSonicBoomInstructionProgramDefinitions
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
