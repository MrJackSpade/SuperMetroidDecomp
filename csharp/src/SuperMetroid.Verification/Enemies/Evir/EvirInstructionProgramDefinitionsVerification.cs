using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Evir;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Evir body's, arms' and projectile's typed animations.</summary>
internal static class EvirInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Evir animation checks.</summary>
    internal static void VerifyEvirInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The four body and arms loops run on time; the normal projectile holds its flight pose; regeneration follows the
    /// body's installed facing, moves the mouth offset back eight times, plays the spit sound and returns the projectile
    /// to idle; the program oracle matches bank $A8.
    /// </summary>
    internal static void VerifyEvirInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        foreach (EvirProgram loop in new[] { EvirProgram.BodyFacingLeft, EvirProgram.ArmsFacingLeft, EvirProgram.BodyFacingRight, EvirProgram.ArmsFacingRight })
        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(slot, EnemyDefinitionId.Evir, loop);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(EvirAnimations.Programs, loop, EvirFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(EvirAnimations.Programs, loop)),
                $"Evir {loop} loop");
        }

        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            EvirEnemyState state = PrepareEvirInstructionSlot(slot, EnemyDefinitionId.EvirProjectile, EvirProgram.ProjectileNormal);
            _ = EnemyInstructionRunner.ShownFrames(slot, 4);
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirProgram.ProjectileNormal).Single(), slot.SpritemapPointer,
                "normal Evir projectile shows its flight pose");
            AnimationProgramCursor<EvirProgram>? held = state.Program;
            _ = EnemyInstructionRunner.ShownFrames(slot, 4);
            AssertEqual(held, state.Program, "normal Evir projectile holds its flight pose");
        }

        {
            var enemies = NewEvirInstructionSystem();
            // The regeneration cues read Evir.instList-$80,X: the body record two slots before the projectile, as the
            // retail population lays them out.
            RoomEnemySlot slot = enemies.SlotTable.Slots[2];
            EvirEnemyState state = PrepareEvirInstructionSlot(slot, EnemyDefinitionId.EvirProjectile, EvirProgram.ProjectileRegenerating);
            // #1269: the projectile's own facing word disagrees with its body on purpose; $A8:879B/$87B6 follow the
            // body's installed list, so the offset starts at +8.
            state.Facing = EvirFacing.Right;
            state.InFlight = true;
            state.Regenerating = true;
            state.Function = EvirAiFunction.ProjectileRegenerating;
            enemies.SlotTable.Slots[0].SetSpeciesState(new EvirEnemyState { InstalledProgram = EvirProgram.BodyFacingLeft });
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            for (int call = 0; call < 10; call++)
            {
                slot.InstructionTimer = NativeCountdown.Start(1);
                EnemyInstructionRunner.Process(arguments);
                if (call == 0)
                    AssertEqual(new Offset16<RoomPixels>(8, 0), state.RegenerationOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirProgram.ProjectileNormal).Single(), slot.SpritemapPointer,
                "regenerating Evir projectile ends on its flight pose");
            AssertEqual(Offset16<RoomPixels>.Zero, state.RegenerationOffset, "regeneration loop advances the projectile mouth offset eight times");
            AssertTrue(!state.Regenerating, "regeneration finish cue clears regeneration ownership");
            AssertTrue(!state.InFlight, "regeneration finish cue leaves the projectile stationary");
            AssertEqual((EvirAiFunction?)EvirAiFunction.ProjectileIdle, state.Function, "regeneration finish cue restores idle AI");
            const ushort nativeSpitSound = 0x005e;
            AssertEqual((SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, nativeSpitSound),
                enemies.EvirBehavior.LastEvirSoundEffect, "regeneration publishes the native library-two spit sound");
        }

        Suite(nameof(EvirAnimationsVerification.VerifyEvirAnimations), () => EvirAnimationsVerification.VerifyEvirAnimations(rom));
        Console.WriteLine(
            "Evir: the four body and arms loops, the held flight pose and the body-relative regeneration cues pass.");
    }

    /// <summary>Creates an enemy system bound to the supplied address space.</summary>
    internal static RoomEnemySystem NewEvirInstructionSystem()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>Seeds an Evir body or projectile slot with its bank-$A8 definition and starting program.</summary>
    internal static EvirEnemyState PrepareEvirInstructionSlot(
        RoomEnemySlot slot,
        EnemyDefinitionId definition,
        EvirProgram program)
    {
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = EvirFrameToolingDefinitions.Bank });
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new EvirEnemyState
        {
            RequestedProgram = program,
            InstalledProgram = program,
            Program = AnimationProgramCursor<EvirProgram>.Start(program),
        };
        slot.SetSpeciesState(state);
        return state;
    }

    /// <summary>Runs retail-cartridge Evir animation verification.</summary>
    [VerificationCommand("--evir-instruction-program-definitions")]
    internal static int EvirInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions());
        return 0;
    }
}
