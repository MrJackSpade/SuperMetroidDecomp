using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;

/// <summary>Production-execution checks for the Fune's and Namihe's eight typed programs.</summary>
internal static class FuneNamiheInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Fune/Namihe animation checks.</summary>
    internal static void VerifyFuneNamiheInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Fune/Namihe instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(rom));
    }

    /// <summary>
    /// The selector table names each of the eight programs; every active program spits one fireball in its facing, plays
    /// the spit sound and hands main AI back to waiting, while an idle program spits nothing; the program oracle matches
    /// bank $A8.
    /// </summary>
    internal static void VerifyFuneNamiheInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        FuneNamiheProgramCase[] programs =
        [
            new(FuneNamiheProgram.FuneActiveLeft, 0x96d3, false, false, true),
            new(FuneNamiheProgram.FuneActiveRight, 0x96d5, false, true, true),
            new(FuneNamiheProgram.FuneIdleLeft, 0x96d7, false, false, false),
            new(FuneNamiheProgram.FuneIdleRight, 0x96d9, false, true, false),
            new(FuneNamiheProgram.NamiheActiveLeft, 0x96db, true, false, true),
            new(FuneNamiheProgram.NamiheActiveRight, 0x96dd, true, true, true),
            new(FuneNamiheProgram.NamiheIdleLeft, 0x96df, true, false, false),
            new(FuneNamiheProgram.NamiheIdleRight, 0x96e1, true, true, false),
        ];

        foreach (FuneNamiheProgramCase program in programs)
        {
            AssertEqual(program.Entry, FuneNamiheSelectorOracle.ProgramAt(program.SelectorCursor),
                $"Fune/Namihe selector $A8:{program.SelectorCursor:X4} names {program.Entry}");
            (RoomEnemySystem enemies, RoomEnemySlot slot) = RunFuneNamiheProgram(program);
            int activeProjectiles = enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
            AssertEqual(program.Active ? 1 : 0, activeProjectiles, $"Fune/Namihe {program.Entry} projectile count");

            if (!program.Active)
                continue;

            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(program.Right ? (ushort)1 : (ushort)0, projectile.DirectionParameter, $"Fune/Namihe {program.Entry} projectile direction");
            AssertEqual(FuneNamiheDefinitions.SpitSoundEffect, enemies.FuneNamiheBehavior.LastFuneNamiheSoundEffect,
                $"Fune/Namihe {program.Entry} sound cue");
            AssertEqual(
                program.Namihe ? FuneNamiheEnemyFunction.NamiheWaitForSamus : FuneNamiheEnemyFunction.FuneWaitForCooldown,
                (enemies.FuneNamiheStates[slot.SlotIndex] ?? throw new InvalidOperationException("Verification requires enemies.FuneNamiheStates[slot.SlotIndex].")).Function,
                $"Fune/Namihe {program.Entry} returns main-AI ownership");
        }
        AssertThrows<InvalidDataException>(() => FuneNamiheSelectorOracle.ProgramAt(0x96e3), "Fune/Namihe selector beyond the table fails loudly");

        Suite(nameof(FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations), () => FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations(rom));
        Console.WriteLine(
            "  Fune/Namihe: the selector table, all eight programs' fireballs, spit sound and hand-back to waiting pass.");
    }

    /// <summary>Seeds one variant and direction into an enemy slot, then advances enough updates to observe projectile and AI handoff behavior.</summary>
    private static (RoomEnemySystem System, RoomEnemySlot Slot) RunFuneNamiheProgram(
        FuneNamiheProgramCase program)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(program.Namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
        slot.SetSpawnParameter1(unchecked((ushort)((program.Namihe ? 1 : 0) | (program.Right ? 0x10 : 0))));
        slot.SetSpawnParameter2(0);
        slot.Position = slot.Position.WithWhole(new(128, 128));
        slot.InstructionTimer = NativeCountdown.Start(1);

        var state = new FuneNamiheEnemyState
        {
            ShowsActiveAnimation = program.Active,
            FacingRight = program.Right,
            Function = program.Active
                ? program.Namihe ? FuneNamiheEnemyFunction.NamiheActivityNoOp : FuneNamiheEnemyFunction.FuneActivityNoOp
                : program.Namihe ? FuneNamiheEnemyFunction.NamiheWaitForSamus : FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = program.Namihe ? (ushort)1 : (ushort)0,
            Program = AnimationProgramCursor<FuneNamiheProgram>.Start(program.Entry),
        };
        enemies.SlotTable.Slots[0].SetSpeciesState(state);

        object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        int frames = program.Active ? 140 : 4;
        for (int frame = 0; frame < frames; frame++)
            EnemyInstructionRunner.Process(arguments);

        return (enemies, slot);
    }

    /// <summary>Program, selector cursor, variant, facing, and active state for one of the eight production programs.</summary>
    internal readonly record struct FuneNamiheProgramCase(
        FuneNamiheProgram Entry,
        ushort SelectorCursor,
        bool Namihe,
        bool Right,
        bool Active);

    /// <summary>Runs the Fune/Namihe animation checks through their dedicated command-line switch.</summary>
    [VerificationCommand("--fune-namihe-instruction-mechanics")]
    internal static int FuneNamiheInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions());
        return 0;
    }
}
