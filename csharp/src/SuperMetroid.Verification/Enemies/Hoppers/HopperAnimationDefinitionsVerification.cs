using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.LookupStream3Verification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Hoppers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Sidehopper and Dessgeega animation tables and live floor and ceiling program handoffs.</summary>
internal static class HopperAnimationDefinitionsVerification
{
    /// <summary>Compares native selector and mechanics words, then exercises four variants in both orientations.</summary>
    internal static void VerifyHopperAnimationDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        int[] tables = [0xa3aac2, 0xa3aaca, 0xa3aad2, 0xa3aada];

        for (int index = 0;
             index < HopperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                HopperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadHopperAnimationWord(rom, 0xa30000 | definition.Address),
                $"hopper mechanics word $A3:{definition.Address:X4}");
        }

        for (ushort variant = 0; variant < 4; variant++)
        for (int selector = 0; selector < tables.Length; selector++)
        {
            bool upsideDown = (selector & 1) != 0;
            bool jumping = (selector & 2) != 0;
            ushort native = ReadHopperAnimationWord(rom, tables[selector] + variant * 2);
            AssertEqual(native,
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping),
                $"hopper animation variant {variant}, selector {selector}");
        }

        var guard = NewHopperAnimationGuard(rom);
        EnemyDefinitionId[] definitions =
        [
            EnemyDefinitionId.Sidehopper,
            EnemyDefinitionId.SidehopperLarge,
            EnemyDefinitionId.DessgeegaLarge,
            EnemyDefinitionId.Dessgeega,
        ];

        for (ushort variant = 0; variant < 4; variant++)
        for (int orientation = 0; orientation < 2; orientation++)
        {
            bool upsideDown = orientation != 0;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

            Action<RoomEnemySlot> initialize = enemies.HoppersBehavior.InitializeHopper;
            Action<RoomEnemySlot, HopperEnemyState, bool, bool> startJump = HoppersBehavior.StartHopperJump;
            Action<RoomEnemySlot, HopperEnemyState> land = HoppersBehavior.LandHopper;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = definitions[variant];
            slot.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xa3,
                VariantIndex = variant,
            };
            slot.Parameter1 = upsideDown ? (ushort)1 : (ushort)0;
            initialize(slot);
            HopperEnemyState state = (enemies.HopperStates[0] ?? throw new InvalidOperationException("Verification requires enemies.HopperStates[0]."));
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production initial landed list {variant}/{orientation}");

            (state ?? throw new InvalidOperationException("Verification requires state.")).XVelocity = 3;
            startJump(slot, state, upsideDown, false);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: true),
                slot.CurrentInstruction,
                $"hopper production jumping list {variant}/{orientation}");

            ExecuteHopperInstructionProgram(enemies, slot, callCount: 1);
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper jumping program {variant}/{orientation} enables off-screen processing");
            AssertEqual(
                variant is 0 or 1 ? (ushort?)0x005d : null,
                enemies.HoppersBehavior.LastHopperSoundEffect,
                $"hopper jumping sound {variant}/{orientation}");
            ExecuteHopperInstructionProgram(enemies, slot, callCount: 1);

            land(slot, state);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production landed handoff {variant}/{orientation}");

            ExecuteHopperInstructionProgram(enemies, slot, callCount: 5);
            AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper landed program {variant}/{orientation} disables off-screen processing");
            AssertTrue(state.ReadyToHop,
                $"hopper landed program {variant}/{orientation} publishes ready-to-hop");
            AssertEqual(
                variant is 0 or 1 ? (ushort?)0x005e : null,
                enemies.HoppersBehavior.LastHopperSoundEffect,
                $"hopper landed sound {variant}/{orientation}");
        }

        Suite(nameof(VerifyTourianSidehopperInstructionAlias), () => VerifyTourianSidehopperInstructionAlias(rom, artwork));

        AssertEqual(0,
            guard.ObservedWords.Count,
            "hopper production programs avoid all cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled hopper mechanics byte");

        AssertThrows<InvalidDataException>(
            () => HopperInstructionProgramDefinitions.ReadMechanicsWord(
                HopperInstructionProgramDefinitions.PresentationWordAddress(0)),
            "hopper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HopperInstructionProgramDefinitions.ReadMechanicsWord(
                HopperInstructionProgramDefinitions.LastAdjacentPhysicsWord),
            "adjacent hopper physics data is rejected as mechanics");

        _ = ProbeHopperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHopperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "hopper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed hopper mechanics lookups allocate no per-frame storage");

        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(4, false, false),
            "hopper animation variant beyond authored table");
        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(ushort.MaxValue, true, true),
            "restored hopper animation variant does not wrap into authored table");
        Console.WriteLine(artwork is null
            ? "Hopper instruction mechanics: ninety-six compiled words, sixteen native " +
              "floor/ceiling programs, five production definitions, and forty compiled " +
              "visual selectors preserve sound/processing/ready side effects."
            : "Installed Sidehopper/Dessgeega: sixteen floor/ceiling programs and " +
              "Tourian alias execute with cartridge visual reads forbidden.");
    }

    /// <summary>Checks the Tourian Sidehopper uses the compiled large-Sidehopper landed program and sound.</summary>
    private static void VerifyTourianSidehopperInstructionAlias(
        SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog? artwork)
    {
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        enemies.BindFixtureBus(NewHopperAnimationGuard(rom));
        enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.SidehopperTourian;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3, VariantIndex = 1 };
        enemies.HoppersBehavior.InitializeHopper(slot);
        ExecuteHopperInstructionProgram(enemies, slot, callCount: 1);
        AssertEqual((ushort?)0x005e, enemies.HoppersBehavior.LastHopperSoundEffect,
            "Tourian Sidehopper shares compiled large-Sidehopper landed program");
    }

    /// <summary>Forces the selected hopper instruction list through a bounded number of interpreter calls.</summary>
    private static void ExecuteHopperInstructionProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Consumes repeated floor and ceiling control lookups for the warmed allocation check.</summary>
    private static int ProbeHopperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HopperInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? HopperInstructionProgramDefinitions.SidehopperJumpingFloor
                    : HopperInstructionProgramDefinitions.LargeDessgeegaLandedCeiling);
        }
        return checksum;
    }

    /// <summary>Reads a native hopper word for independent bank-A3 definition comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Hopper animation words")]
    internal static ushort ReadHopperAnimationWord(SuperMetroidAddressSpace source, int address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWord(address);

    /// <summary>Rejects runtime reads from migrated hopper selector, control, and visual ranges.</summary>
    private static CartridgeReadGuard NewHopperAnimationGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(0xa3aac2, 0xa3aae2 - 0xa3aac2,
                address => $"Hopper attempted migrated animation-selector read ${address:X6}.")
            .Forbid(HopperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled hopper mechanics byte ${address:X6}.")
            .ForbidWords(0xa30000, HopperInstructionProgramDefinitions.PresentationWordCount,
                HopperInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Hopper read visual-selector byte ${address:X6}.");

    /// <summary>Runs hopper animation and operand-location checks through the verification dispatcher.</summary>
    [VerificationCommand("--hopper-instruction-program-definitions")]
    internal static int HopperInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyStream3HopperOperandPositions), () => VerifyStream3HopperOperandPositions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
