using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoChozoOrbInstructionProgramDefinitions() =>
        VerifyTorizoChozoOrbInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyTorizoChozoOrbInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoChozoOrbInstructionMechanicsWord definition =
                TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo Chozo-orb mechanics word $86:{definition.Address:X4}");
        }

        var guard = new TorizoChozoOrbInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnBomb = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoChozoOrb", flags)!;
        MethodInfo spawnGolden = typeof(RoomEnemySystem).GetMethod(
            "SpawnGoldenTorizoChozoOrb", flags)!;

        foreach (bool golden in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        {
            RoomEnemySystem enemies = CreateSystem(guard);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight);
            (golden ? spawnGolden : spawnBomb).Invoke(enemies, [torizo]);
            RoomEnemyProjectileSlot orb = enemies.EnemyProjectiles[^1];
            ushort expectedProgram = facingRight
                ? TorizoChozoOrbInstructionProgramDefinitions.MovingRight
                : TorizoChozoOrbInstructionProgramDefinitions.MovingLeft;
            AssertEqual(expectedProgram, orb.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb program");

            for (int frame = 1; frame <= 86; frame++)
            {
                process.Invoke(enemies, [orb, null, (ushort)0, (ushort)0]);
                AssertTrue(orb.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb loop frame {frame}");
            }
            AssertEqual((ushort)0x0055, orb.InstructionTimer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb reloads 85-frame pose");
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), orb.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb loop cursor");
        }

        RoomEnemySystem wallEnemies = CreateSystem(guard);
        RoomEnemySlot wallTorizo = CreateTorizo(wallEnemies, facingRight: false);
        spawnBomb.Invoke(wallEnemies, [wallTorizo]);
        RoomEnemyProjectileSlot wall = wallEnemies.EnemyProjectiles[^1];
        wall.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.WallImpact;
        wall.InstructionTimer = 1;
        for (int frame = 1; frame <= 20; frame++)
        {
            process.Invoke(wallEnemies, [wall, null, (ushort)0, (ushort)0]);
            AssertTrue(wall.IsActive, $"Torizo orb wall impact frame {frame}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, wall.PreInstruction,
                $"Torizo orb wall impact clears callback on frame {frame}");
            AssertTrue(wall.PersistsOnSamusContact,
                $"Torizo orb wall impact persists on Samus contact on frame {frame}");
        }
        process.Invoke(wallEnemies, [wall, null, (ushort)0, (ushort)0]);
        AssertTrue(!wall.IsActive,
            "Torizo orb wall impact deletes after five four-frame poses");

        RoomEnemySystem floorEnemies = CreateSystem(guard);
        RoomEnemySlot floorTorizo = CreateTorizo(floorEnemies, facingRight: true);
        spawnGolden.Invoke(floorEnemies, [floorTorizo]);
        RoomEnemyProjectileSlot floor = floorEnemies.EnemyProjectiles[^1];
        AssertTrue(!floor.CanDamageSamus,
            "Golden Torizo airborne orb starts unable to damage Samus");
        floor.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
        floor.InstructionTimer = 1;
        for (int frame = 1; frame <= 39; frame++)
        {
            process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
            AssertTrue(floor.IsActive, $"Torizo orb floor impact frame {frame}");
            AssertTrue(floor.CanDamageSamus,
                $"Torizo orb floor impact clears no-contact bit on frame {frame}");
            AssertTrue(floor.PersistsOnSamusContact,
                $"Torizo orb floor impact persists on contact on frame {frame}");
            AssertEqual(EnemyProjectileDrawPriority.High, floor.DrawPriority,
                $"Torizo orb floor impact remains high priority on frame {frame}");
        }
        process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
        AssertTrue(!floor.IsActive,
            "Torizo orb floor impact deletes after exact 4/5/6/7/8/9-frame poses");

        VerifyShotDrop(golden: false);
        VerifyShotDrop(golden: true);

        AssertEqual(TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Torizo Chozo-orb spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo Chozo-orb mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab17),
            "Torizo Chozo-orb spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab4d),
            "Torizo Chozo-orb packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab8a),
            "adjacent drop callback body is rejected as orb mechanics");

        _ = ProbeTorizoChozoOrbInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoChozoOrbInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo Chozo-orb allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo Chozo-orb mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo Chozo-orb instruction mechanics: forty compiled words, four real " +
            "producer loops, exact wall/floor impacts, and both shot/drop operands pass " +
            "with mechanics bytes forbidden.");

        void VerifyShotDrop(bool golden)
        {
            RoomEnemySystem enemies = CreateSystem(guard);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight: false);
            (golden ? spawnGolden : spawnBomb).Invoke(enemies, [torizo]);
            if (golden)
            {
                typeof(RoomEnemySystem).GetField("_torizoState", flags)!.SetValue(
                    enemies,
                    new TorizoEnemyState(torizo, isGolden: true));
            }

            RoomEnemyProjectileSlot orb = enemies.EnemyProjectiles[^1];
            orb.XPosition = 0x0350;
            orb.YPosition = 0x0260;
            orb.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.Shot;
            orb.InstructionTimer = 1;
            for (int frame = 1; frame <= 20; frame++)
            {
                process.Invoke(enemies, [orb, null, (ushort)0, (ushort)0]);
                AssertTrue(orb.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo orb shot frame {frame}");
            }
            process.Invoke(enemies, [orb, null, (ushort)0, (ushort)0]);
            AssertTrue(!orb.IsActive,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot deletes after drop callback");
            AssertEqual(1, enemies.TorizoOrbDropRequests.Count,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot publishes one drop");
            TorizoOrbDropRequest request = enemies.TorizoOrbDropRequests[0];
            AssertEqual(golden
                    ? TorizoChozoOrbInstructionProgramDefinitions.GoldenOrbEnemyHeader
                    : TorizoChozoOrbInstructionProgramDefinitions.BombOrbEnemyHeader,
                request.EnemyDefinitionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop header");
            AssertEqual((ushort)0x0350, request.X,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop X");
            AssertEqual((ushort)0x0260, request.Y,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop Y");
        }

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)(() => 0x0080));
            typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
                enemies,
                new SamusState());
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

    private static int ProbeTorizoChozoOrbInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoChozoOrbInstructionProgramDefinitions.MovingLeft
                    : TorizoChozoOrbInstructionProgramDefinitions.Shot);
        }
        return checksum;
    }

    private sealed class TorizoChozoOrbInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TorizoChozoOrbInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo Chozo-orb byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoChozoOrbInstructionProgramDefinitions
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
