using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-data and guarded-execution checks for escape Etecoon initialization and animation.</summary>
internal static class EscapeEtecoonDefinitionsVerification
{
    /// <summary>Checks three native setup roles and all six masked selector values against production initialization.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for escape Etecoon definitions")]
    internal static void VerifyEscapeEtecoonDefinitions(SuperMetroidAddressSpace rom)
    {
        const int xTable = 0xb3e718;
        const int yTable = 0xb3e71e;
        const int preInstructionTable = 0xb3e724;
        const int instructionTable = 0xb3e72a;
        const int speedTable = 0xb3e730;

        for (int role = 0; role < 3; role++)
        {
            ushort parameter = unchecked((ushort)(role * 2));
            EscapeEtecoonInitialization initialization =
                EscapeEtecoonDefinitions.Initialization(parameter);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((xTable + parameter)), initialization.Position.X,
                $"escape Etecoon role {role} X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((yTable + parameter)), initialization.Position.Y,
                $"escape Etecoon role {role} Y");
            AssertEqual((EscapeEtecoonPreInstruction)CartridgeImportSource.Require(rom).ReadCartridgeWord((preInstructionTable + parameter)),
                initialization.PreInstruction,
                $"escape Etecoon role {role} pre-instruction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + parameter)),
                NativeEnemyProgramOracle.EntryOf(EscapeEtecoonNativeListAddresses.Entries, initialization.Program),
                $"escape Etecoon role {role} instruction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((speedTable + parameter)),
                initialization.Velocity.X.RawValue,
                $"escape Etecoon role {role} speed");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(6),
            "escape Etecoon selector after authored roles");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(0xffff),
            "escape Etecoon wrapping restored selector");

        var guarded = NewEscapeEtecoonDefinitionGuard(rom);
        for (ushort parameter = 0; parameter < 6; parameter++)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().HasEvent = _ => false;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.SetSpawnParameter1(parameter);
            slot.Position = slot.Position.WithWhole(new(0xaaaa, 0xbbbb));
            slot.Bind(EnemyDefinitionId.EtecoonEscape, slot.Definition);
            enemies.EscapeAnimalsBehavior.InitializeEscapeEtecoon(slot);

            int role = parameter >> 1;
            EscapeEtecoonInitialization expected =
                EscapeEtecoonDefinitions.Initialization(parameter);
            EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
                throw new InvalidDataException("Escape Etecoon initializer omitted typed state.");
            AssertEqual(expected.Position.X, slot.Position.X.Whole,
                $"production escape Etecoon selector {parameter} X");
            AssertEqual(expected.Position.Y, slot.Position.Y.Whole,
                $"production escape Etecoon selector {parameter} Y");
            AssertEqual(expected.PreInstruction, state.PreInstruction,
                $"production escape Etecoon selector {parameter} pre-instruction");
            AssertEqual(AnimationProgramCursor<EscapeEtecoonProgram>.Start(expected.Program), state.Program,
                $"production escape Etecoon selector {parameter} program");
            AssertEqual(expected.Velocity, state.Velocity,
                $"production escape Etecoon selector {parameter} speed");
            AssertEqual((EscapeEtecoonRole)(role * 2), (EscapeEtecoonRole)(parameter & 0xfffe),
                $"escape Etecoon selector {parameter} role mask");
        }

        Console.WriteLine(
            "Escape Etecoon definitions: fifteen native values and all six masked retail selectors pass with the authored tables forbidden during production initialization; malformed restored selectors fail explicitly.");
    }

    /// <summary>Loads retail data for the escape Etecoon program behavior check.</summary>
    internal static void VerifyEscapeEtecoonProgramBehavior() =>
        VerifyEscapeEtecoonProgramBehavior(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    /// <summary>
    /// Runs every Etecoon program through the production instruction runner with its native lists forbidden: together
    /// they show all of the escape Etecoon's poses, and the gratitude program moves the Etecoon left before handing off
    /// to the rightward escape.
    /// </summary>
    internal static void VerifyEscapeEtecoonProgramBehavior(SuperMetroidAddressSpace rom)
    {
        CartridgeReadGuard guard = NewEscapeEtecoonDefinitionGuard(rom);
        RoomEnemySystem enemies = CreateEscapeEtecoonProgramSystem();
        RoomEnemySlot etecoon = enemies.SlotTable.Slots[0];
        EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
            throw new InvalidDataException("Escape Etecoon test lost typed state.");
        var shown = new HashSet<ushort>();
        EscapeEtecoonProgram[] programs =
        [
            EscapeEtecoonProgram.RunningLeftLowTide,
            EscapeEtecoonProgram.RunningLeftHighTide,
            EscapeEtecoonProgram.RunningRightLowTide,
            EscapeEtecoonProgram.RunningRightHighTide,
            EscapeEtecoonProgram.RunningForEscape,
            EscapeEtecoonProgram.Stationary,
        ];
        foreach (EscapeEtecoonProgram program in programs)
        {
            state.Program = AnimationProgramCursor<EscapeEtecoonProgram>.Start(program);
            RunForcedEscapeEtecoonUpdates(etecoon, 7, shown);
        }

        ushort xBeforeGratitude = etecoon.Position.X.Whole;
        state.Program = AnimationProgramCursor<EscapeEtecoonProgram>.Start(EscapeEtecoonProgram.ExpressGratitudeThenEscape);
        RunForcedEscapeEtecoonUpdates(etecoon, 40, shown);
        AssertTrue(unchecked((short)(etecoon.Position.X.Whole - xBeforeGratitude)) < 0,
            "escape Etecoon gratitude program applies its repeated left displacement");
        AssertEqual(EscapeEtecoonPreInstruction.EscapeRight, state.PreInstruction,
            "escape Etecoon gratitude program hands off to rightward escape");

        var poses = Enumerable.Range(0, EscapeEtecoonFrameDefinitionsConstants.PoseCount)
            .Select(pose => EscapeEtecoonFrameDefinitions.Spritemap(new(pose))).ToHashSet();
        AssertTrue(shown.SetEquals(poses), "the escape Etecoon programs show every pose");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production avoids every native escape-Etecoon list byte");
        Console.WriteLine($"Escape Etecoon programs: all seven run through production, showing {shown.Count} poses, with native lists forbidden.");
    }

    /// <summary>Rejects production reads of the escape Etecoon's migrated setup tables and native lists.</summary>
    private static CartridgeReadGuard NewEscapeEtecoonDefinitionGuard(ISnesAddressSpace rom) =>
        new CartridgeReadGuard(rom)
            .Forbid(address => address is >= 0xb3e556 and < 0xb3e718,
                address => $"Production read a native escape-Etecoon list byte ${address:X6}.")
            .Forbid(address => address is >= 0xb3e718 and < 0xb3e736,
                address => $"Escape Etecoon attempted migrated definition read ${address:X6}.");

    /// <summary>Initializes an Etecoon in a guarded bank-$B3 enemy system.</summary>
    private static RoomEnemySystem CreateEscapeEtecoonProgramSystem(
)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().HasEvent = _ => false;
        RoomEnemySlot etecoon = enemies.SlotTable.Slots[0];
        etecoon.Bind(EnemyDefinitionId.EtecoonEscape, etecoon.Definition);
        etecoon.Bind(etecoon.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xb3 });
        etecoon.SetSpawnParameter1(0);
        enemies.EscapeAnimalsBehavior.InitializeEscapeEtecoon(etecoon);
        return enemies;
    }

    /// <summary>Forces <paramref name="steps"/> program updates, recording each pose shown.</summary>
    private static void RunForcedEscapeEtecoonUpdates(RoomEnemySlot etecoon,
        int steps,
        HashSet<ushort> shown)
    {
        object?[] arguments =
            [etecoon, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        for (int step = 0; step < steps; step++)
        {
            etecoon.InstructionTimer = WrappingCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
            shown.Add(etecoon.SpritemapPointer);
        }
    }

    /// <summary>Runs the retail-backed Etecoon setup-definition command.</summary>
    [VerificationCommand("--escape-etecoon-definitions")]
    internal static int EscapeEtecoonDefinitionsCommand()
    {
        Suite(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the focused escape Etecoon owned-state check.</summary>
    [VerificationCommand("--escape-etecoon-owned-state")]
    internal static int EscapeEtecoonOwnedStateCommand()
    {
        Suite(nameof(VerifyEscapeEtecoonOwnedState), () => VerifyEscapeEtecoonOwnedState());
        return 0;
    }

    /// <summary>
    /// Confirms an initialized escape Etecoon owns a pre-instruction, and that a kill empties its velocity and
    /// pre-instruction as the cleared common record does.
    /// </summary>
    internal static void VerifyEscapeEtecoonOwnedState()
    {
        RoomEnemySystem enemies = CreateEscapeEtecoonProgramSystem();
        RoomEnemySlot etecoon = enemies.SlotTable.Slots[0];
        EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
            throw new InvalidDataException("Escape Etecoon test lost typed state.");
        AssertTrue(state.PreInstruction is not null, "an initialized escape Etecoon has a pre-instruction");
        state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0x0380), default);

        etecoon.Clear();
        AssertEqual<EscapeEtecoonPreInstruction?>(null, state.PreInstruction, "a kill empties the pre-instruction");
        AssertEqual(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties the velocity");
        Console.WriteLine("  Escape Etecoon owned state: the cleared record matches.");
    }

    /// <summary>Runs the retail-backed Etecoon instruction migration command.</summary>
    [VerificationCommand("--escape-etecoon-instruction-mechanics")]
    internal static int EscapeEtecoonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEscapeEtecoonProgramBehavior), () => VerifyEscapeEtecoonProgramBehavior());
        return 0;
    }
}
