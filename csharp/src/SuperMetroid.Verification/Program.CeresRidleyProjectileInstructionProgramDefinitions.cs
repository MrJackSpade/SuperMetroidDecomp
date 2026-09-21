using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresRidleyProjectileInstructionProgramDefinitions() =>
        VerifyCeresRidleyProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyCeresRidleyProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresRidleyProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresRidleyProjectileInstructionMechanicsWord definition =
                CeresRidleyProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Ceres Ridley projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CeresRidleyProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;
        MethodInfo spawnFireball = typeof(RoomEnemySystem).GetMethod(
            "SpawnRidleyFireball", instanceFlags)!;
        MethodInfo spawnCenter = typeof(RoomEnemySystem).GetMethod(
            "SpawnAfterburnCenter", instanceFlags)!;
        MethodInfo spawnDirectional = typeof(RoomEnemySystem).GetMethod(
            "SpawnDirectionalAfterburn", instanceFlags)!;
        MethodInfo beginFinal = typeof(RoomEnemySystem).GetMethod(
            "BeginAfterburnFinalAnimation", staticFlags)!;
        ushort ceresRidleyDefinition = (ushort)typeof(RoomEnemySystem).GetField(
            "CeresRidleyDefinition", staticFlags)!.GetRawConstantValue()!;

        RoomEnemySystem fireballSystem = NewSystem();
        RoomEnemySlot ridley = fireballSystem.Slots[0];
        ridley.EnemyDefinitionPointer = ceresRidleyDefinition;
        ridley.XPosition = 128;
        ridley.YPosition = 96;
        typeof(RoomEnemySystem).GetField("_ridleyState", instanceFlags)!.SetValue(
            fireballSystem,
            new RidleyEnemyState
            {
                FacingDirection = 0,
                FireballXVelocity = 0xfe00,
                FireballYVelocity = 0x0100,
            });
        spawnFireball.Invoke(fireballSystem, [ridley, true]);
        RoomEnemyProjectileSlot fireball = fireballSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CeresRidleyFireball);
        AssertEqual(CeresRidleyProjectileInstructionProgramDefinitions.Fireball,
            fireball.InstructionPointer, "real Ceres Ridley fireball producer");
        RunForcedTick(fireballSystem, fireball);
        AssertEqual(EnemyProjectileCodePointers.RTS_868170, fireball.PreInstruction,
            "fireball first command clears its pre-instruction before the first pose");
        RunForcedTick(fireballSystem, fireball);
        AssertEqual(EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_RidleyFireball,
            fireball.PreInstruction,
            "fireball program reinstalls its movement pre-instruction");
        RunForcedTicks(fireballSystem, fireball, 5);
        AssertEqual((ushort)0x9564, fireball.InstructionPointer,
            "fireball completes all authored frames and loops to its second frame");

        foreach ((RoomEnemyProjectileKind centerKind, ushort program,
                  RoomEnemyProjectileKind firstChild,
                  RoomEnemyProjectileKind secondChild) in new[]
                 {
                     (RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
                         CeresRidleyProjectileInstructionProgramDefinitions.HorizontalCenter,
                         RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight,
                         RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft),
                     (RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter,
                         CeresRidleyProjectileInstructionProgramDefinitions.VerticalCenter,
                         RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp,
                         RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            spawnCenter.Invoke(enemies, [centerKind, (ushort)120, (ushort)88, (ushort)3]);
            RoomEnemyProjectileSlot center = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == centerKind);
            AssertEqual(program, center.InstructionPointer,
                "real center-afterburn producer selects its named program");
            RunForcedTicks(enemies, center, 2);
            AssertTrue(enemies.EnemyProjectiles.Any(
                    projectile => projectile.Kind == firstChild),
                "center-afterburn callback spawns its first directional child");
            AssertTrue(enemies.EnemyProjectiles.Any(
                    projectile => projectile.Kind == secondChild),
                "center-afterburn callback spawns its second directional child");
            RunForcedTicks(enemies, center, 4);
            AssertTrue(!center.IsActive,
                "center-afterburn completes five frames and deletes");
        }

        foreach (RoomEnemyProjectileKind kind in new[]
                 {
                     RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight,
                     RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft,
                     RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp,
                     RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown,
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyProjectileSlot source = enemies.EnemyProjectiles[0];
            source.XPosition = 120;
            source.YPosition = 88;
            source.RemainingAfterburns = 1;
            spawnDirectional.Invoke(
                enemies,
                [source, kind, (ushort)0x0100, (ushort)0xff00]);
            RoomEnemyProjectileSlot afterburn = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == kind);
            AssertEqual(
                CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn,
                afterburn.InstructionPointer,
                "real directional-afterburn producer selects its named program");
            RunForcedTicks(enemies, afterburn, 2);
            AssertEqual(2, enemies.EnemyProjectiles.Count(
                    projectile => projectile.Kind == kind),
                "directional callback spawns the next same-direction afterburn");
            RunForcedTicks(enemies, afterburn, 4);
            AssertTrue(!afterburn.IsActive,
                "directional afterburn completes five frames and deletes");
        }

        RoomEnemySystem finalSystem = NewSystem();
        RoomEnemyProjectileSlot finalSource = finalSystem.EnemyProjectiles[0];
        finalSource.RemainingAfterburns = 0;
        spawnDirectional.Invoke(
            finalSystem,
            [finalSource,
             RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight,
             (ushort)0x0100,
             (ushort)0]);
        RoomEnemyProjectileSlot final = finalSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight);
        beginFinal.Invoke(null, [final]);
        AssertEqual(CeresRidleyProjectileInstructionProgramDefinitions.AfterburnFinal,
            final.InstructionPointer, "production collision handoff selects final impact");
        RunForcedTicks(finalSystem, final, 6);
        AssertTrue(!final.IsActive,
            "final afterburn impact completes all five frames and deletes");

        RoomEnemySystem sharedDeleteSystem = NewSystem();
        RoomEnemyProjectileSlot sharedDeleteSource = sharedDeleteSystem.EnemyProjectiles[0];
        spawnDirectional.Invoke(
            sharedDeleteSystem,
            [sharedDeleteSource,
             RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown,
             (ushort)0,
             (ushort)0x0100]);
        RoomEnemyProjectileSlot sharedDelete = sharedDeleteSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown);
        sharedDelete.InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(sharedDeleteSystem, sharedDelete);
        AssertTrue(!sharedDelete.IsActive,
            "Ceres Ridley projectile shot reaction reaches shared compiled deletion");

        AssertEqual(CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Ceres Ridley projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = CeresRidleyProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads Ceres Ridley presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared Ceres Ridley mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9556),
            "Ceres Ridley spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x95ba),
            "Ceres Ridley callback body is rejected as mechanics");

        _ = ProbeCeresRidleyProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresRidleyProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres Ridley allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres Ridley mechanics lookups allocate no storage");

        Console.WriteLine(
            "Ceres Ridley projectile instruction mechanics: forty-two compiled words, " +
            "all seven owners, callback handoffs, deletion paths, and twenty-six live " +
            "spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
                RunForcedTick(enemies, projectile);
        }
    }

    private static int ProbeCeresRidleyProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CeresRidleyProjectileInstructionProgramDefinitions.Fireball
                    : CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn);
        }
        return checksum;
    }

    private sealed class CeresRidleyProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresRidleyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres Ridley mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresRidleyProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = CeresRidleyProjectileInstructionProgramDefinitions
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
