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

/// <summary>Checks compiled Nuclear Waffle body-link projectile loops against retail program words.</summary>
internal static class NuclearWaffleProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for body-link instruction and sprite comparison.</summary>
    internal static void VerifyNuclearWaffleProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs four spawned links through their twelve-frame loops and shared deletion under a read guard.</summary>
    internal static void VerifyNuclearWaffleProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NuclearWaffleProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NuclearWaffleProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Nuclear Waffle projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewNuclearWaffleProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemySlot> initialize = enemies.NuclearWaffleBehavior.InitializeNuclearWaffle;

        RoomEnemySlot owner = enemies.SlotTable.Slots[0];
        owner.Parameter1 = 0x2001;
        owner.Parameter2 = 0x2000;
        owner.Position = owner.Position.WithWhole(new(0x0180, 0x0200));
        initialize(owner);

        RoomEnemyProjectileSlot[] projectiles = enemies.NuclearWaffleStates[0]!
            .ProjectileSegments
            .OfType<RoomEnemyProjectileSlot>()
            .ToArray();
        AssertEqual(4, projectiles.Length,
            "real Nuclear Waffle initialization spawns four projectile body links");
        var installed = new HashSet<ushort>();
        foreach (RoomEnemyProjectileSlot projectile in projectiles)
        {
            AssertEqual(NuclearWaffleProjectileInstructionProgramDefinitions.Initial,
                projectile.InstructionPointer,
                "real Nuclear Waffle projectile producer selects the named body loop");
            RunForcedTicks(projectile, 13);
            AssertEqual(
                unchecked((ushort)(
                    NuclearWaffleProjectileInstructionProgramDefinitions.Initial + 4)),
                projectile.InstructionPointer,
                "Nuclear Waffle projectile completes all twelve frames and loops");
            AssertTrue(projectile.IsActive,
                "Nuclear Waffle projectile body loop remains active");
        }

        projectiles[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(projectiles[0], 1);
        AssertTrue(!projectiles[0].IsActive,
            "Nuclear Waffle projectile reaches the compiled shared delete program");

        AssertEqual(0, guard.ObservedWords.Count,
            "NuclearWaffleProjectile execution performs no live spritemap operand reads");
        AssertEqual(NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "NuclearWaffleProjectile executes every native visual operand");
        for (int index = 0; index < NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"NuclearWaffleProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "NuclearWaffleProjectile has a compiled visual selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address), selector,
                "NuclearWaffleProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Nuclear Waffle projectile and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbb60),
            "Nuclear Waffle projectile spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbb92),
            "adjacent Nuclear Waffle projectile initializer is rejected as mechanics");

        _ = ProbeNuclearWaffleProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNuclearWaffleProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Nuclear Waffle projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Nuclear Waffle projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Nuclear Waffle projectile instruction mechanics: fourteen compiled words, " +
            "four real articulated links, complete twelve-frame loops, shared deletion, " +
            "and all twelve installed sprite frames pass with mechanics and visual operand ROM reads forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
                if (projectile.IsActive)
                {
                    AssertEqual((ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + 2 + tick % 12 * 4),
                        projectile.PresentationOperandAddress, "actual Nuclear Waffle body selects exact native timed operand");
                    installed.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    /// <summary>Alternates two compiled instruction words to detect allocation on repeated mechanics reads.</summary>
    private static int ProbeNuclearWaffleProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NuclearWaffleProjectileInstructionProgramDefinitions.Initial
                    : NuclearWaffleProjectileInstructionProgramDefinitions.LoopCommand);
        }
        return checksum;
    }

    /// <summary>Blocks live reads of projectile and shared-delete mechanics while recording visual operand access.</summary>
    private static CartridgeReadGuard NewNuclearWaffleProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => NuclearWaffleProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address),
                address => $"Production read compiled Nuclear Waffle projectile mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount, NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs Nuclear Waffle projectile instruction verification from the command dispatcher.</summary>
    [VerificationCommand("--nuclear-waffle-projectile-instruction-mechanics")]
    internal static int NuclearWaffleProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions());
        return 0;
    }
}
