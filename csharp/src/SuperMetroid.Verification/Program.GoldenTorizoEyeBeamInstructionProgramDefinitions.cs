using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions() =>
        VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoEyeBeamInstructionMechanicsWord definition =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Golden Torizo eye-beam mechanics word $86:{definition.Address:X4}");
        }

        var guard = new GoldenTorizoEyeBeamInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnGoldenTorizoEyeBeam", flags)!;
        MethodInfo runMovement = typeof(RoomEnemySystem).GetMethod(
            "RunGoldenTorizoEyeBeamPreInstruction", flags)!;

        foreach (bool facingRight in new[] { false, true })
        {
            var random = new Queue<ushort>([0x0080, 0x0080, 0x0010]);
            RoomEnemySystem enemies = CreateSystem(guard, random, eyeBeamExplosions: false);
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
            spawn.Invoke(enemies, [torizo, (ushort)0]);
            RoomEnemyProjectileSlot beam = enemies.EnemyProjectiles[^1];
            AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal,
                beam.InstructionPointer,
                $"Golden Torizo {facingRight} eye beam normal program");
            AssertEqual(unchecked((ushort)(1000 + (facingRight ? 20 : -20))),
                beam.XPosition,
                $"Golden Torizo {facingRight} eye beam X origin");
            AssertEqual((ushort)470, beam.YPosition,
                $"Golden Torizo {facingRight} eye beam Y origin");
            AssertEqual(
                EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEyeBeam,
                beam.PreInstruction,
                $"Golden Torizo {facingRight} eye beam movement callback");

            for (int frame = 1; frame <= 6; frame++)
            {
                process.Invoke(enemies, [beam, null, (ushort)0, (ushort)0]);
                AssertTrue(beam.IsActive,
                    $"Golden Torizo {facingRight} eye beam normal frame {frame}");
            }
            AssertEqual((ushort)1, beam.InstructionTimer,
                $"Golden Torizo {facingRight} eye beam reloads five-frame loop");
            AssertEqual(unchecked((ushort)(
                    GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal + 4)),
                beam.InstructionPointer,
                $"Golden Torizo {facingRight} eye beam stable loop cursor");
            AssertEqual(0, random.Count,
                $"Golden Torizo {facingRight} eye beam consumes three RNG words");
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
        RoomEnemySystem wallEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: false);
        RoomEnemyProjectileSlot wallBeam = SpawnBeam(wallEnemies, spawn);
        wallBeam.XPosition = 157;
        wallBeam.YPosition = 128;
        wallBeam.XSubposition = 0;
        wallBeam.XVelocity = 256;
        wallBeam.YVelocity = 0;
        runMovement.Invoke(wallEnemies, [wallBeam, wallRoom]);
        AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact,
            wallBeam.InstructionPointer,
            "Golden Torizo eye beam wall collision selects wall impact");
        AssertEqual((ushort)1, wallBeam.InstructionTimer,
            "Golden Torizo eye beam wall impact is immediately armed");
        for (int frame = 1; frame <= 20; frame++)
        {
            process.Invoke(wallEnemies, [wallBeam, null, (ushort)0, (ushort)0]);
            AssertTrue(wallBeam.IsActive,
                $"Golden Torizo eye beam wall impact frame {frame}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, wallBeam.PreInstruction,
                $"Golden Torizo eye beam wall impact clears movement frame {frame}");
        }
        process.Invoke(wallEnemies, [wallBeam, null, (ushort)0, (ushort)0]);
        AssertTrue(!wallBeam.IsActive,
            "Golden Torizo eye beam wall impact deletes after five four-frame poses");

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

        RoomEnemySystem disabledEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: false);
        RoomEnemyProjectileSlot disabledBeam = SpawnBeam(disabledEnemies, spawn);
        CollideWithFloor(disabledEnemies, disabledBeam);
        for (int frame = 1; frame <= 9; frame++)
        {
            process.Invoke(disabledEnemies, [disabledBeam, null, (ushort)0, (ushort)0]);
            AssertTrue(disabledBeam.IsActive,
                $"disabled Golden Torizo eye-beam floor loop frame {frame}");
        }
        AssertEqual((ushort)8, disabledBeam.InstructionTimer,
            "disabled Golden Torizo eye-beam explosion reloads blank wait");
        AssertEqual(unchecked((ushort)(
                GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpactLoop + 4)),
            disabledBeam.InstructionPointer,
            "disabled Golden Torizo eye-beam explosion stays in blank loop");
        AssertTrue(!disabledBeam.CanDamageSamus,
            "disabled Golden Torizo eye-beam explosion remains harmless");

        RoomEnemySystem enabledEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: true);
        RoomEnemyProjectileSlot enabledBeam = SpawnBeam(enabledEnemies, spawn);
        CollideWithFloor(enabledEnemies, enabledBeam);
        for (int frame = 1; frame <= 8; frame++)
        {
            process.Invoke(enabledEnemies, [enabledBeam, null, (ushort)0, (ushort)0]);
            AssertTrue(enabledBeam.IsActive,
                $"enabled Golden Torizo eye-beam blank floor frame {frame}");
            AssertTrue(!enabledBeam.CanDamageSamus,
                $"enabled Golden Torizo eye-beam blank floor frame {frame} is harmless");
        }
        for (int frame = 1; frame <= 39; frame++)
        {
            process.Invoke(enabledEnemies, [enabledBeam, null, (ushort)0, (ushort)0]);
            AssertTrue(enabledBeam.IsActive,
                $"enabled Golden Torizo eye-beam explosion frame {frame}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, enabledBeam.PreInstruction,
                $"enabled Golden Torizo eye-beam explosion clears movement frame {frame}");
            AssertEqual(frame >= 10, enabledBeam.CanDamageSamus,
                $"enabled Golden Torizo eye-beam collision transition frame {frame}");
        }
        process.Invoke(enabledEnemies, [enabledBeam, null, (ushort)0, (ushort)0]);
        AssertTrue(!enabledBeam.IsActive,
            "enabled Golden Torizo eye-beam floor impact deletes after exact lifetime");

        AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Golden Torizo eye-beam spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Golden Torizo eye-beam mechanics byte");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb3d1),
            "Golden Torizo eye-beam spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb3f1),
            "Golden Torizo eye-beam packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb428),
            "adjacent eye-beam definition is rejected as program mechanics");

        _ = ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Golden Torizo eye-beam allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Golden Torizo eye-beam mechanics lookups allocate no storage");

        Console.WriteLine(
            "Golden Torizo eye-beam instruction mechanics: twenty-eight compiled words, " +
            "both real producers, wall impact, and both floor branches pass with " +
            "mechanics bytes forbidden.");

        RoomEnemyProjectileSlot SpawnBeam(RoomEnemySystem enemies, MethodInfo spawnMethod)
        {
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            torizo.Parameter1 = 0x8000;
            spawnMethod.Invoke(enemies, [torizo, (ushort)0]);
            return enemies.EnemyProjectiles[^1];
        }

        void CollideWithFloor(RoomEnemySystem enemies, RoomEnemyProjectileSlot beam)
        {
            beam.XPosition = 128;
            beam.YPosition = 157;
            beam.XVelocity = 0;
            beam.YSubposition = 0;
            beam.YVelocity = 256;
            runMovement.Invoke(enemies, [beam, floorRoom]);
            AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpact,
                beam.InstructionPointer,
                "Golden Torizo eye beam floor collision selects floor impact");
            AssertEqual((ushort)1, beam.InstructionTimer,
                "Golden Torizo eye beam floor impact is immediately armed");
            AssertEqual((ushort)6, unchecked((ushort)(beam.YPosition & 0x000f)),
                "Golden Torizo eye beam floor impact uses native low-nibble alignment");
        }

        RoomEnemySystem CreateSystem(
            ISnesAddressSpace bus,
            Queue<ushort> random,
            bool eyeBeamExplosions)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            RoomEnemySlot torizo = enemies.Slots[0];
            var state = new TorizoEnemyState(torizo, isGolden: true)
            {
                AttackFlags = eyeBeamExplosions ? (ushort)0x8000 : (ushort)0,
            };
            typeof(RoomEnemySystem).GetField("_torizoState", flags)!.SetValue(enemies, state);
            return enemies;
        }
    }

    private static int ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact
                    : GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal);
        }
        return checksum;
    }

    private sealed class GoldenTorizoEyeBeamInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GoldenTorizoEyeBeamInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Golden Torizo eye-beam byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = GoldenTorizoEyeBeamInstructionProgramDefinitions
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
