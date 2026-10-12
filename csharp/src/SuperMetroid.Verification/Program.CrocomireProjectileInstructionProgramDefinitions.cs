using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Represents crocomire projectile instruction program definitions verification.</summary>
internal static class CrocomireProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies crocomire projectile instruction program definitions.</summary>
    internal static void VerifyCrocomireProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireProjectileInstructionProgramDefinitions), () => VerifyCrocomireProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies crocomire projectile instruction program definitions.</summary>
    internal static void VerifyCrocomireProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyCrocomireProjectileMechanicsDispatch), () => VerifyCrocomireProjectileMechanicsDispatch(rom));
        Suite(nameof(VerifyCrocomireProjectilePresentationPositions), () => VerifyCrocomireProjectilePresentationPositions(rom));

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewCrocomireProjectileInstructionReadGuard(rom);

        RoomEnemySystem mouthSystem = NewSystem();
        RoomEnemySlot mouthOwner = mouthSystem.SlotTable.Slots[0];
        mouthOwner.Position = mouthOwner.Position.WithWhole(new(224, 128));
        mouthSystem.CrocomireBehavior.SpawnCrocomireProjectile(mouthOwner, (ushort)2);
        RoomEnemyProjectileSlot mouth = mouthSystem.Projectiles.EnemyProjectiles.Single(
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
        AssertTrue(mouthSystem.Projectiles.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "Crocomire mouth shot invokes the production drop allocator before deletion");

        RoomEnemySystem bridgeSystem = NewSystem();
        var bridgeState = new CrocomireEnemyState(bridgeSystem.SlotTable.Slots[0]);
        bridgeSystem.CrocomireBehavior.Crocomire = bridgeState;
        bridgeSystem.CrocomireBehavior.CrocomireDeath = new CrocomireDeathState();
        bridgeSystem.CrocomireBehavior.SpawnNextCrocomireBridgeFragment();
        RoomEnemyProjectileSlot bridge = bridgeSystem.Projectiles.EnemyProjectiles.Single(
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
        RoomEnemySlot spikeOwner = spikeSystem.SlotTable.Slots[0];
        spikeOwner.VramTilesIndex = 0x0200;
        spikeOwner.PaletteIndex = 0x0c00;
        spikeSystem.CrocomireBehavior.Crocomire = new CrocomireEnemyState(spikeOwner);
        spikeSystem.CrocomireBehavior.SpawnCrocomireSpikeWallPieces();
        RoomEnemyProjectileSlot[] spikes = spikeSystem.Projectiles.EnemyProjectiles.Where(projectile =>
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

        AssertEqual(0, guard.ObservedWords.Count,
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
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address), selector,
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
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x0080);
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0x0080);
            enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };
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
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    /// <summary>Verifies crocomire projectile mechanics dispatch.</summary>
    internal static void VerifyCrocomireProjectileMechanicsDispatch(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0x8fcf, 0x8fd3, 0x8fd7, 0x8fdb, 0x8fdf, 0x8fe3, 0x8fe7, 0x8fe9,
            0x8feb, 0x8fef, 0x8ff1, 0x8ff3, 0x8ff7, 0x8ff9,
            0x9007, 0x900b, 0x900f, 0x9013, 0x9017, 0x901b, 0x901d, 0x901f,
        ];
        AssertEqual(addresses.Length, CrocomireProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount, "Crocomire projectile mechanics count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort address = addresses[i];
            var definition = CrocomireProjectileInstructionProgramDefinitionsTooling.MechanicsWord(i);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);
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
            AssertEqual(expected, CrocomireProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address), "Crocomire projectile mechanics byte domain");
        }
        AssertTrue(!CrocomireProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x878fcf), "Crocomire projectile rejects another bank");
        foreach (ushort address in new ushort[] { 0, 0x8fcd, 0x8fd1, 0x8fed, 0x8ff5, 0x8ffb, 0x9003, 0x9005, 0x9009, 0x9021, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(address), "Crocomire projectile rejects nonmechanics address");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitionsTooling.MechanicsWord(-1), "negative Crocomire projectile mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitionsTooling.MechanicsWord(22), "Crocomire projectile mechanics index past end");
    }

    /// <summary>Verifies crocomire projectile presentation positions.</summary>
    internal static void VerifyCrocomireProjectilePresentationPositions(SuperMetroidAddressSpace rom)
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
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | addresses[i]), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | actual), "Crocomire projectile original presentation operand");
            AssertThrows<InvalidDataException>(() => CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(actual), "Crocomire projectile presentation excluded from mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(-1), "negative Crocomire projectile presentation index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(13), "Crocomire projectile presentation index past end");
    }
    /// <summary>Probes crocomire projectile instruction mechanics allocation.</summary>
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

    /// <summary>New crocomire projectile instruction read guard.</summary>
    private static CartridgeReadGuard NewCrocomireProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => CrocomireProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Crocomire projectile mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount, CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Crocomire projectile instruction mechanics command.</summary>
    [VerificationCommand("--crocomire-projectile-instruction-mechanics")]
    internal static int CrocomireProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireProjectileInstructionProgramDefinitions), () => VerifyCrocomireProjectileInstructionProgramDefinitions());
        return 0;
    }
}
