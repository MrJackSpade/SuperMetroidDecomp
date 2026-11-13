using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Evir;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
    internal static void VerifyEvirInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        foreach (EvirSegment loop in new[] { EvirSegment.BodyFacingLeft, EvirSegment.ArmsFacingLeft, EvirSegment.BodyFacingRight, EvirSegment.ArmsFacingRight })
        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(slot, EnemyDefinitionId.Evir, loop);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(EvirAnimations.Segments, loop, EvirFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(EvirAnimations.Segments, loop)),
                $"Evir {loop} loop");
        }

        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            EvirEnemyState state = PrepareEvirInstructionSlot(slot, EnemyDefinitionId.EvirProjectile, EvirSegment.ProjectileNormal);
            _ = EnemyInstructionRunner.ShownFrames(slot, 4);
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileNormal).Single(), slot.SpritemapPointer,
                "normal Evir projectile shows its flight pose");
            AnimationPlayback<EvirSegment>? held = state.Playback;
            _ = EnemyInstructionRunner.ShownFrames(slot, 4);
            AssertEqual(held, state.Playback, "normal Evir projectile holds its flight pose");
        }

        {
            var enemies = NewEvirInstructionSystem();
            // The regeneration cues read Evir.instList-$80,X: the body record two slots before the projectile, as the
            // retail population lays them out.
            RoomEnemySlot slot = enemies.SlotTable.Slots[2];
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Bind(EnemyDefinitionId.Evir, body.Definition);
            body.SetSpeciesState(new EvirEnemyState { InstalledAnimation = EvirSegment.BodyFacingLeft });
            // The spit finds its body as production places it, two places after the body.
            slot.Bind(EnemyDefinitionId.EvirProjectile, slot.Definition);
            enemies.EvirBehavior.InitializeEvirProjectile(slot);
            EvirEnemyState state = PrepareEvirInstructionSlot(slot, EnemyDefinitionId.EvirProjectile, EvirSegment.ProjectileRegenerating);
            // #1269: the projectile's own facing word disagrees with its body on purpose; $A8:879B/$87B6 follow the
            // body's installed list, so the offset starts at +8.
            state.Facing = EvirFacing.Right;
            state.InFlight = true;
            state.Regenerating = true;
            state.Function = EvirAiFunction.ProjectileRegenerating;
            enemies.SlotTable.Slots[0].SetSpeciesState(new EvirEnemyState { InstalledAnimation = EvirSegment.BodyFacingLeft });
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int call = 0; call < 10; call++)
            {
                slot.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
                if (call == 0)
                    AssertEqual(new Offset16<RoomPixels>(8, 0), state.RegenerationOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileNormal).Single(), slot.SpritemapPointer,
                "regenerating Evir projectile ends on its flight pose");
            AssertEqual(Offset16<RoomPixels>.Zero, state.RegenerationOffset, "regeneration loop advances the projectile mouth offset eight times");
            AssertTrue(!state.Regenerating, "regeneration finish cue clears regeneration ownership");
            AssertTrue(!state.InFlight, "regeneration finish cue leaves the projectile stationary");
            AssertEqual((EvirAiFunction?)EvirAiFunction.ProjectileIdle, state.Function, "regeneration finish cue restores idle AI");
            const ushort nativeSpitSound = 0x005e;
            AssertEqual((SoundEffectId?)SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, nativeSpitSound),
                enemies.Audio.LatestSound, "regeneration publishes the native library-two spit sound");
        }

        Suite(nameof(EvirAnimationsVerification.VerifyEvirAnimations), () => EvirAnimationsVerification.VerifyEvirAnimations(rom));
        Console.WriteLine(
            "Evir: the four body and arms loops, the held flight pose and the body-relative regeneration cues pass.");
    }

    /// <summary>Creates an enemy system bound to the supplied address space.</summary>
    internal static RoomEnemySystem NewEvirInstructionSystem()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>Seeds an Evir body or projectile slot with its bank-$A8 definition and starting program.</summary>
    internal static EvirEnemyState PrepareEvirInstructionSlot(
        RoomEnemySlot slot,
        EnemyDefinitionId definition,
        EvirSegment program)
    {
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.AnimationClock.StepOnNextUpdate();
        var state = new EvirEnemyState
        {
            RequestedAnimation = program,
            InstalledAnimation = program,
            Playback = new(EvirAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
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
