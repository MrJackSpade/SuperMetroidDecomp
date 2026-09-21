using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidRockProjectileInstructionProgramDefinitions()
    {
        VerifyKraidRockProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKraidRockProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KraidRockProjectileInstructionMechanicsWord definition =
                KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadKraidRockProjectileInstructionWord(rom, definition.Address),
                $"Kraid-rock projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new KraidRockProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        ushort selectedRandom = 0;
        typeof(RoomEnemySystem).GetField("_readRandomNumber", instanceFlags)!
            .SetValue(enemies, (Func<ushort>)(() => selectedRandom));

        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        MethodInfo spawnSpit = typeof(RoomEnemySystem).GetMethod(
            "SpawnKraidSpitRock",
            instanceFlags)!;
        MethodInfo spawnCeiling = typeof(RoomEnemySystem).GetMethod(
            "SpawnKraidCeilingRock",
            instanceFlags)!;
        MethodInfo requestRising = typeof(RoomEnemySystem).GetMethod(
            "RequestKraidRisingRock",
            instanceFlags)!;
        MethodInfo spawnKago = typeof(RoomEnemySystem).GetMethod(
            "SpawnKagoBug",
            instanceFlags)!;

        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 224,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };

        selectedRandom = 0x0020;
        AssertTrue((bool)spawnSpit.Invoke(enemies, [source])!,
            "real Kraid spit producer allocates its rock");
        selectedRandom = 0x0020;
        AssertTrue((bool)spawnCeiling.Invoke(enemies, [(ushort)96])!,
            "real Kraid ceiling producer allocates its rock");

        var kraidState = new KraidEnemyState();
        selectedRandom = 0x0000;
        requestRising.Invoke(enemies, [source, kraidState]);
        selectedRandom = 0x0010;
        requestRising.Invoke(enemies, [source, kraidState]);

        selectedRandom = 0x0002;
        AssertTrue((bool)spawnKago.Invoke(enemies, [source])!,
            "real Kago producer allocates its bug");

        RoomEnemyProjectileSlot spit = Find(RoomEnemyProjectileKind.KraidSpitRock);
        RoomEnemyProjectileSlot ceiling = Find(RoomEnemyProjectileKind.KraidCeilingRock);
        RoomEnemyProjectileSlot risingLeft = Find(
            RoomEnemyProjectileKind.KraidRisingRockLeft);
        RoomEnemyProjectileSlot risingRight = Find(
            RoomEnemyProjectileKind.KraidRisingRockRight);
        RoomEnemyProjectileSlot kago = Find(RoomEnemyProjectileKind.KagoBug);

        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            spit.InstructionPointer,
            "Kraid spit rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            ceiling.InstructionPointer,
            "Kraid ceiling rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            risingLeft.InstructionPointer,
            "left rising rock starts in the named shared pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.RisingRockRight,
            risingRight.InstructionPointer,
            "right rising rock starts in its named mirrored pose");
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            kago.InstructionPointer,
            "Kago bug starts in the cartridge's shared Kraid-rock pose");

        foreach (RoomEnemyProjectileSlot projectile in
                 new[] { spit, ceiling, risingLeft, risingRight, kago })
        {
            ushort expectedSleep = projectile.Kind ==
                    RoomEnemyProjectileKind.KraidRisingRockRight
                ? KraidRockProjectileInstructionProgramDefinitions.RisingRockRightSleep
                : KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBugSleep;
            RunForcedTick(projectile);
            AssertEqual(expectedSleep, projectile.InstructionPointer,
                $"{projectile.Kind} schedules its terminal sleep");
            RunForcedTick(projectile);
            AssertEqual(expectedSleep, projectile.InstructionPointer,
                $"{projectile.Kind} sleeps at the authored opcode");
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{projectile.Kind} sleep leaves the timer stopped");
        }

        spit.InstructionPointer =
            KraidRockProjectileInstructionProgramDefinitions.SpitRockShot;
        RunForcedTick(spit);
        AssertEqual(
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0,
            spit.PreInstruction,
            "Kraid spit shot program installs the native palette-zero pre-instruction");
        AssertEqual((ushort)0x9c91, spit.InstructionPointer,
            "Kraid spit shot program schedules its first explosion frame");
        RunForcedTicks(spit, 4);
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SpitRockShotDelete,
            spit.InstructionPointer,
            "Kraid spit shot program reaches deletion after all five explosion frames");
        RunForcedTick(spit);
        AssertTrue(!spit.IsActive,
            "Kraid spit shot program deletes after its complete explosion");

        ceiling.InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(ceiling);
        AssertTrue(!ceiling.IsActive,
            "non-spit Kraid rock shot reaction reaches the compiled shared delete program");

        AssertEqual(
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Kraid-rock projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KraidRockProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Kraid-rock presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Kraid-rock and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9c7f),
            "Kraid-rock spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9ca3),
            "adjacent Kraid-rock initializer is rejected as mechanics");
        AssertTrue(KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KagoBug,
                KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug),
            "Kago bug owns the one shared initial pose");
        AssertTrue(!KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KagoBug,
                KraidRockProjectileInstructionProgramDefinitions.SpitRockShot),
            "Kago bug does not claim Kraid's shot program");
        AssertTrue(!KraidRockProjectileInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.KraidRisingRockRight,
                KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug),
            "right rising rock does not claim the mirrored left/shared pose");

        _ = ProbeKraidRockProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidRockProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kraid-rock allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid-rock projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid-rock projectile instruction mechanics: twelve compiled words, all " +
            "four real Kraid rock producers, the shared Kago pose, both sleeps, the " +
            "complete five-frame shot explosion, and seven live spritemap reads pass " +
            "with mechanics bytes forbidden.");

        RoomEnemyProjectileSlot Find(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.Single(projectile => projectile.Kind == kind);

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
                RunForcedTick(projectile);
        }

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeKraidRockProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug
                    : KraidRockProjectileInstructionProgramDefinitions.SpitRockShot);
        }
        return checksum;
    }

    private static ushort ReadKraidRockProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class KraidRockProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KraidRockProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid-rock mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = KraidRockProjectileInstructionProgramDefinitions
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
