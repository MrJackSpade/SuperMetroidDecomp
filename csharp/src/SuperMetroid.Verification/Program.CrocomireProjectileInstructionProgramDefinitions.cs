using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireProjectileInstructionProgramDefinitions), () => VerifyCrocomireProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyCrocomireProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyCrocomireProjectileMechanicsDispatch), () => VerifyCrocomireProjectileMechanicsDispatch(rom));
        Suite(nameof(VerifyCrocomireProjectilePresentationPositions), () => VerifyCrocomireProjectilePresentationPositions(rom));

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "CrocomireProjectile execution performs no live spritemap operand reads");
        AssertEqual(CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "CrocomireProjectile executes every native visual operand");
        for (int index = 0; index < CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"CrocomireProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "CrocomireProjectile has a compiled visual selector");
            AssertEqual(ReadCrocomireProjectileInstructionWord(rom, address), selector,
                "CrocomireProjectile compiled selector matches the cartridge");
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
            "shared deletion, and thirteen live sprite frames match native OAM without live operand reads.");

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
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    private static void VerifyCrocomireProjectileMechanicsDispatch(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0x8fcf, 0x8fd3, 0x8fd7, 0x8fdb, 0x8fdf, 0x8fe3, 0x8fe7, 0x8fe9,
            0x8feb, 0x8fef, 0x8ff1, 0x8ff3, 0x8ff7, 0x8ff9,
            0x9007, 0x900b, 0x900f, 0x9013, 0x9017, 0x901b, 0x901d, 0x901f,
        ];
        AssertEqual(addresses.Length, CrocomireProjectileInstructionProgramDefinitions.MechanicsWordCount, "Crocomire projectile mechanics count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort address = addresses[i];
            var definition = CrocomireProjectileInstructionProgramDefinitions.MechanicsWord(i);
            ushort expected = ReadCrocomireProjectileInstructionWord(rom, address);
            AssertEqual(address, definition.Address, "Crocomire projectile independent mechanics address");
            AssertEqual(expected, definition.Value, "Crocomire projectile enumerated native word");
            AssertEqual(expected, CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(address), "Crocomire projectile native control dispatch");
            AssertThrows<InvalidDataException>(() => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)(address + 1)), "Crocomire projectile misaligned mechanics word");
        }
        // Confirm the complete byte-ownership contract against native addresses,
        // including the unused program gap and the skipped delete instruction.
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = Array.Exists(addresses, word => address == word || address == word + 1);
            AssertEqual(expected, CrocomireProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address), "Crocomire projectile mechanics byte domain");
        }
        AssertTrue(!CrocomireProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x878fcf), "Crocomire projectile rejects another bank");
        foreach (ushort address in new ushort[] { 0, 0x8fcd, 0x8fd1, 0x8fed, 0x8ff5, 0x8ffb, 0x9003, 0x9005, 0x9009, 0x9021, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(address), "Crocomire projectile rejects nonmechanics address");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.MechanicsWord(-1), "negative Crocomire projectile mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.MechanicsWord(22), "Crocomire projectile mechanics index past end");
    }

    private static void VerifyCrocomireProjectilePresentationPositions(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0x8fd1, 0x8fd5, 0x8fd9, 0x8fdd, 0x8fe1, 0x8fe5, 0x8fed, 0x8ff5,
            0x9009, 0x900d, 0x9011, 0x9015, 0x9019,
        ];
        AssertEqual(addresses.Length, CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount, "Crocomire projectile presentation count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort actual = CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(i);
            AssertEqual(addresses[i], actual, "Crocomire projectile independent sprite operand address");
            AssertEqual(ReadCrocomireProjectileInstructionWord(rom, addresses[i]), ReadCrocomireProjectileInstructionWord(rom, actual), "Crocomire projectile original presentation operand");
            AssertThrows<InvalidDataException>(() => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(actual), "Crocomire projectile presentation excluded from mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(-1), "negative Crocomire projectile presentation index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(13), "Crocomire projectile presentation index past end");
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
