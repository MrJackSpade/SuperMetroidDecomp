using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireProjectileInstructionProgramDefinitions()
    {
        VerifyCrocomireProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCrocomireProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CrocomireProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CrocomireProjectileInstructionMechanicsWord definition =
                CrocomireProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCrocomireProjectileInstructionWord(rom, definition.Address),
                $"Crocomire projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CrocomireProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        RoomEnemySystem mouthSystem = NewSystem();
        MethodInfo spawnMouth = typeof(RoomEnemySystem).GetMethod(
            "SpawnCrocomireProjectile", flags)!;
        RoomEnemySlot mouthOwner = mouthSystem.Slots[0];
        mouthOwner.XPosition = 224;
        mouthOwner.YPosition = 128;
        spawnMouth.Invoke(mouthSystem, [mouthOwner, (ushort)2]);
        RoomEnemyProjectileSlot mouth = mouthSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        AssertEqual(CrocomireProjectileInstructionProgramDefinitions.MouthProjectile,
            mouth.InstructionPointer,
            "real Crocomire mouth producer selects the named animation loop");
        RunForcedTicks(mouthSystem, mouth, 7);
        AssertEqual(unchecked((ushort)(
                CrocomireProjectileInstructionProgramDefinitions.MouthProjectile + 4)),
            mouth.InstructionPointer,
            "Crocomire mouth projectile completes all six frames and loops");

        mouth.InstructionPointer =
            CrocomireProjectileInstructionProgramDefinitions.MouthProjectileShot;
        RunForcedTicks(mouthSystem, mouth, 5);
        AssertEqual((ushort)0x901b, mouth.InstructionPointer,
            "Crocomire mouth shot program displays all five explosion frames");
        RunForcedTicks(mouthSystem, mouth, 1);
        AssertTrue(!mouth.IsActive,
            "Crocomire mouth shot reaches shared deletion after requesting its drop");
        AssertTrue(mouthSystem.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "Crocomire mouth shot invokes the production drop allocator before deletion");

        RoomEnemySystem bridgeSystem = NewSystem();
        var bridgeState = new CrocomireEnemyState(bridgeSystem.Slots[0]);
        typeof(RoomEnemySystem).GetField("_crocomire", flags)!
            .SetValue(bridgeSystem, bridgeState);
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!
            .SetValue(bridgeSystem, new CrocomireDeathState());
        MethodInfo spawnBridge = typeof(RoomEnemySystem).GetMethod(
            "SpawnNextCrocomireBridgeFragment", flags)!;
        spawnBridge.Invoke(bridgeSystem, [bridgeState]);
        RoomEnemyProjectileSlot bridge = bridgeSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.CrocomireBridgeCrumbling);
        AssertEqual(CrocomireProjectileInstructionProgramDefinitions.BridgeFragment,
            bridge.InstructionPointer,
            "real Crocomire bridge producer selects the named fragment loop");
        RunForcedTicks(bridgeSystem, bridge, 2);
        AssertEqual(unchecked((ushort)(
                CrocomireProjectileInstructionProgramDefinitions.BridgeFragment + 4)),
            bridge.InstructionPointer,
            "Crocomire bridge fragment loops its persistent pose");

        RoomEnemySystem spikeSystem = NewSystem();
        RoomEnemySlot spikeOwner = spikeSystem.Slots[0];
        spikeOwner.VramTilesIndex = 0x0200;
        spikeOwner.PaletteIndex = 0x0c00;
        typeof(RoomEnemySystem).GetField("_crocomire", flags)!
            .SetValue(spikeSystem, new CrocomireEnemyState(spikeOwner));
        MethodInfo spawnSpikes = typeof(RoomEnemySystem).GetMethod(
            "SpawnCrocomireSpikeWallPieces", flags)!;
        spawnSpikes.Invoke(spikeSystem, null);
        RoomEnemyProjectileSlot[] spikes = spikeSystem.EnemyProjectiles.Where(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces)
            .ToArray();
        AssertEqual(8, spikes.Length,
            "real Crocomire spike-wall producer allocates all eight pieces");
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            AssertEqual(CrocomireProjectileInstructionProgramDefinitions.SpikeWallPiece,
                spike.InstructionPointer,
                "real Crocomire spike-wall piece selects the named loop");
            RunForcedTicks(spikeSystem, spike, 2);
            AssertEqual(unchecked((ushort)(
                    CrocomireProjectileInstructionProgramDefinitions.SpikeWallPiece + 4)),
                spike.InstructionPointer,
                "Crocomire spike-wall piece loops its persistent pose");
        }

        bridge.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(bridgeSystem, bridge, 1);
        AssertTrue(!bridge.IsActive,
            "Crocomire bridge shot reaction reaches the compiled shared delete program");

        AssertEqual(CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Crocomire projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = CrocomireProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Crocomire projectile presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Crocomire and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x8fd1),
            "Crocomire projectile spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9021),
            "unreachable word before the initializer is rejected as mechanics");

        _ = ProbeCrocomireProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCrocomireProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Crocomire projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Crocomire projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Crocomire projectile instruction mechanics: twenty-two compiled words, " +
            "the real mouth/bridge/spike producers, complete mouth and shot loops, " +
            "shared deletion, and thirteen live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)(() => 0x0080));
            typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
                enemies,
                new SamusState { Health = 99, MaxHealth = 99 });
            return enemies;
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
    }

    private static int ProbeCrocomireProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CrocomireProjectileInstructionProgramDefinitions.MouthProjectile
                    : CrocomireProjectileInstructionProgramDefinitions.MouthProjectileShot);
        }
        return checksum;
    }

    private static ushort ReadCrocomireProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class CrocomireProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CrocomireProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Crocomire projectile mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CrocomireProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = CrocomireProjectileInstructionProgramDefinitions
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
