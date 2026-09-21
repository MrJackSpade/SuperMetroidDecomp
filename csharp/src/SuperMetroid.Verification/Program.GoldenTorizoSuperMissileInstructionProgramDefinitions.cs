using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions() =>
        VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GoldenTorizoSuperMissileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GoldenTorizoSuperMissileInstructionMechanicsWord definition =
                GoldenTorizoSuperMissileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Golden Torizo Super Missile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new GoldenTorizoSuperMissileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnGoldenTorizoSuperMissile", flags)!;
        MethodInfo runFlight = typeof(RoomEnemySystem).GetMethod(
            "RunGoldenTorizoSuperMissileFlight", flags)!;

        foreach (bool facingRight in new[] { false, true })
        {
            RoomEnemySystem enemies = CreateSystem(guard);
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
            spawn.Invoke(enemies, [torizo]);
            RoomEnemyProjectileSlot missile = enemies.EnemyProjectiles[^1];
            ushort initial = facingRight
                ? GoldenTorizoSuperMissileInstructionProgramDefinitions.RightInitial
                : GoldenTorizoSuperMissileInstructionProgramDefinitions.LeftInitial;
            ushort loop = facingRight
                ? GoldenTorizoSuperMissileInstructionProgramDefinitions.RightLoop
                : GoldenTorizoSuperMissileInstructionProgramDefinitions.LeftLoop;
            AssertEqual(initial, missile.InstructionPointer,
                $"Golden Torizo {facingRight} Super Missile initial program");
            AssertEqual(unchecked((ushort)(1000 + (facingRight ? 30 : -30))),
                missile.XPosition,
                $"Golden Torizo {facingRight} Super Missile X origin");
            AssertEqual((ushort)448, missile.YPosition,
                $"Golden Torizo {facingRight} Super Missile Y origin");

            var samus = new SamusState
            {
                XPosition = 1200,
                YPosition = 448,
            };
            for (int frame = 1; frame <= 49; frame++)
            {
                process.Invoke(enemies, [missile, samus, (ushort)0, (ushort)0]);
                AssertTrue(missile.IsActive,
                    $"Golden Torizo {facingRight} Super Missile held frame {frame}");
            }
            AssertEqual(
                EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown,
                missile.PreInstruction,
                $"Golden Torizo {facingRight} Super Missile installs flight callback");
            AssertEqual((ushort)2, missile.InstructionTimer,
                $"Golden Torizo {facingRight} Super Missile enters two-frame loop");
            AssertEqual(unchecked((ushort)(loop + 4)), missile.InstructionPointer,
                $"Golden Torizo {facingRight} Super Missile loop cursor");
            AssertTrue(facingRight
                    ? unchecked((short)missile.XVelocity) > 0
                    : unchecked((short)missile.XVelocity) < 0,
                $"Golden Torizo {facingRight} Super Missile aim direction");

            for (int frame = 1; frame <= 16; frame++)
                process.Invoke(enemies, [missile, samus, (ushort)0, (ushort)0]);
            AssertEqual((ushort)2, missile.InstructionTimer,
                $"Golden Torizo {facingRight} Super Missile reloads sixteen-frame loop");
            AssertEqual(unchecked((ushort)(loop + 4)), missile.InstructionPointer,
                $"Golden Torizo {facingRight} Super Missile stable loop cursor");
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
        RoomEnemySystem impactEnemies = CreateSystem(guard);
        RoomEnemySlot impactTorizo = impactEnemies.Slots[0];
        impactTorizo.XPosition = 1000;
        impactTorizo.YPosition = 500;
        impactTorizo.Parameter1 = 0x8000;
        spawn.Invoke(impactEnemies, [impactTorizo]);
        RoomEnemyProjectileSlot impact = impactEnemies.EnemyProjectiles[^1];
        impact.XPosition = 156;
        impact.YPosition = 128;
        impact.XSubposition = 0;
        impact.XVelocity = 256;
        impact.YVelocity = 0;
        runFlight.Invoke(impactEnemies, [impact, wallRoom]);
        AssertEqual(GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact,
            impact.InstructionPointer,
            "Golden Torizo Super Missile wall collision selects compiled impact");
        AssertEqual((ushort)1, impact.InstructionTimer,
            "Golden Torizo Super Missile impact is immediately armed");
        AssertEqual(GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact,
            EnemyProjectileDefinitionCatalog.Get(
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile).ShotInstructionList,
            "Golden Torizo Super Missile shot path shares compiled impact");

        for (int frame = 1; frame <= 30; frame++)
        {
            process.Invoke(impactEnemies, [impact, null, (ushort)0, (ushort)0]);
            AssertTrue(impact.IsActive,
                $"Golden Torizo Super Missile impact frame {frame}");
            AssertEqual((ushort)16, impact.XRadius,
                $"Golden Torizo Super Missile impact X radius frame {frame}");
            AssertEqual((ushort)16, impact.YRadius,
                $"Golden Torizo Super Missile impact Y radius frame {frame}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, impact.PreInstruction,
                $"Golden Torizo Super Missile impact clears movement frame {frame}");
            AssertTrue(impact.CanDamageSamus,
                $"Golden Torizo Super Missile impact enables Samus collision frame {frame}");
            AssertTrue(impact.PersistsOnSamusContact,
                $"Golden Torizo Super Missile impact persists on contact frame {frame}");
            AssertTrue(!impact.BlocksSamusProjectiles,
                $"Golden Torizo Super Missile impact disables shot collision frame {frame}");
            AssertEqual(EnemyProjectileDrawPriority.High, impact.DrawPriority,
                $"Golden Torizo Super Missile impact priority frame {frame}");
        }
        process.Invoke(impactEnemies, [impact, null, (ushort)0, (ushort)0]);
        AssertTrue(!impact.IsActive,
            "Golden Torizo Super Missile impact deletes after six five-frame poses");

        AssertEqual(
            GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Golden Torizo Super Missile spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Golden Torizo Super Missile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoSuperMissileInstructionProgramDefinitions.ReadMechanicsWord(0xb295),
            "Golden Torizo Super Missile spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoSuperMissileInstructionProgramDefinitions.ReadMechanicsWord(0xb2ff),
            "Golden Torizo Super Missile packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoSuperMissileInstructionProgramDefinitions.ReadMechanicsWord(0xb31a),
            "adjacent Super Missile definition is rejected as program mechanics");

        _ = ProbeGoldenTorizoSuperMissileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGoldenTorizoSuperMissileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Golden Torizo Super Missile allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Golden Torizo Super Missile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Golden Torizo Super Missile instruction mechanics: forty-three compiled " +
            "words, both real held/aim/flight loops, and the real wall/shot impact pass " +
            "with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            return enemies;
        }
    }

    private static int ProbeGoldenTorizoSuperMissileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GoldenTorizoSuperMissileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GoldenTorizoSuperMissileInstructionProgramDefinitions.RightInitial
                    : GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact);
        }
        return checksum;
    }

    private sealed class GoldenTorizoSuperMissileInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GoldenTorizoSuperMissileInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Golden Torizo Super Missile byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = GoldenTorizoSuperMissileInstructionProgramDefinitions
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
