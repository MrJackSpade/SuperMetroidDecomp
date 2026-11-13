using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>Checks the hoppers' animation selection and the side effects of their typed programs.</summary>
internal static class HopperAnimationDefinitionsVerification
{
    /// <summary>
    /// The $A3:AAC2-AAE1 selector tables pick the same lists as <see cref="HopperPrograms.Program"/>. For
    /// every species and orientation the real initializer, jump and landing start the selected programs; the jump turns
    /// off-screen processing on and plays the airborne sound, and the landing turns it off, plays the landing sound,
    /// shows its four timed poses and then lets main AI hop again.
    /// </summary>
    internal static void VerifyHopperAnimationDefinitions(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork? artwork = null)
    {
        int[] tables = [0xa3aac2, 0xa3aaca, 0xa3aad2, 0xa3aada];
        for (ushort variant = 0; variant < 4; variant++)
        for (int selector = 0; selector < tables.Length; selector++)
        {
            bool upsideDown = (selector & 1) != 0;
            bool jumping = (selector & 2) != 0;
            AssertEqual(ReadHopperAnimationWord(rom, tables[selector] + variant * 2),
                NativeEnemyAnimationOracle.EntryOf(HopperNativeListAddresses.Entries,
                    HopperPrograms.Program(variant, upsideDown ? HopperSurface.Ceiling : HopperSurface.Floor, jumping ? HopperPhase.Jumping : HopperPhase.Landed)),
                $"hopper animation variant {variant}, selector {selector}");
        }

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
            string context = $"{variant}/{orientation}";
            var enemies = new RoomEnemySystem((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = artwork?.Bosses };
            enemies.VerificationServices().SeedRandom = _ => { };
            enemies.VerificationServices().NextRandom = () => 0;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definitions[variant], slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition) with { VariantIndex = variant });
            slot.SetSpawnParameter1(EnemyDefinitionId.Sidehopper, upsideDown ? (ushort)1 : (ushort)0);
            enemies.HoppersBehavior.InitializeHopper(slot);
            HopperEnemyState state = enemies.HopperStates[0] ?? throw new InvalidOperationException("Verification requires enemies.HopperStates[0].");
            HopperProgram landed = HopperPrograms.Program(variant, upsideDown ? HopperSurface.Ceiling : HopperSurface.Floor, HopperPhase.Landed);
            HopperProgram jumping = HopperPrograms.Program(variant, upsideDown ? HopperSurface.Ceiling : HopperSurface.Floor, HopperPhase.Jumping);
            AssertEqual(new AnimationPlayback<HopperProgram>(HopperAnimations.StartingWith(landed), AnimationPlace.StartOfAnimation), state.Playback, $"hopper initial landed program {context}");

            state.XVelocity = 3;
            HoppersBehavior.StartHopperJump(slot, state, upsideDown ? HopperSurface.Ceiling : HopperSurface.Floor, HopperHopDirection.Forward);
            AssertEqual(new AnimationPlayback<HopperProgram>(HopperAnimations.StartingWith(jumping), AnimationPlace.StartOfAnimation), state.Playback, $"hopper jumping program {context}");
            ushort airborne = HopperAnimationsConstants.Spritemaps(jumping).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([airborne, airborne], [1, 1]),
                EnemyInstructionRunner.ShownFrames(slot, 2), $"hopper airborne pose {context}");
            AssertTrue(slot.Processing.HasAny(EnemyProcessing.OffScreen),
                $"hopper jumping program {context} enables off-screen processing");
            AssertEqual(variant is 0 or 1 ? EnemySound.SidehopperJump : null, enemies.Audio.LatestSound,
                $"hopper jumping sound {context}");

            HoppersBehavior.LandHopper(slot, state);
            AssertEqual(new AnimationPlayback<HopperProgram>(HopperAnimations.StartingWith(landed), AnimationPlace.StartOfAnimation), state.Playback, $"hopper landed handoff {context}");
            int[] holds = [2, 5, 2, 3];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(HopperAnimationsConstants.Spritemaps(landed), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"hopper landing poses {context}");
            AssertTrue(!state.ReadyToHop, $"hopper landed program {context} holds its last pose before readiness");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(!slot.Processing.HasAny(EnemyProcessing.OffScreen),
                $"hopper landed program {context} disables off-screen processing");
            AssertTrue(state.ReadyToHop, $"hopper landed program {context} publishes ready-to-hop");
            AssertEqual(variant is 0 or 1 ? EnemySound.SidehopperLanding : null, enemies.Audio.LatestSound,
                $"hopper landed sound {context}");
        }

        Suite(nameof(VerifyTourianSidehopperInstructionAlias), () => VerifyTourianSidehopperInstructionAlias(artwork));
        AssertThrows<InvalidDataException>(
            () => HopperPrograms.Program(4, HopperSurface.Floor, HopperPhase.Landed),
            "hopper animation variant beyond authored table");
        AssertThrows<InvalidDataException>(
            () => HopperPrograms.Program(ushort.MaxValue, HopperSurface.Ceiling, HopperPhase.Jumping),
            "restored hopper animation variant does not wrap into authored table");
        Console.WriteLine(artwork is null
            ? "Hopper animations: the native selector tables, five production definitions and their sound, processing and readiness cues pass."
            : "Installed Sidehopper/Dessgeega: sixteen floor/ceiling programs and the Tourian alias run with the installed artwork.");
    }


    /// <summary>Checks the Tourian Sidehopper uses the large-Sidehopper landed program and sound.</summary>
    private static void VerifyTourianSidehopperInstructionAlias(
        ImportEnemyArtwork? artwork)
    {
        var enemies = new RoomEnemySystem((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = artwork?.Bosses };
        enemies.VerificationServices().SeedRandom = _ => { };
        enemies.VerificationServices().NextRandom = () => 0;

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.SidehopperTourian, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition) with { VariantIndex = 1 });
        slot.SetSpawnSettings(new HopperSpawnSettings(HopperMounting.Floor));
        enemies.HoppersBehavior.InitializeHopper(slot);
        EnemyInstructionRunner.ShownFrames(slot, 1);
        AssertEqual((SoundEffectId?)EnemySound.SidehopperLanding, enemies.Audio.LatestSound,
            "Tourian Sidehopper shares the large-Sidehopper landed program");
    }

    /// <summary>Reads a native hopper word for independent bank-A3 definition comparisons.</summary>
    internal static ushort ReadHopperAnimationWord(CartridgeImportAddressSpace source, int address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWord(address);

    /// <summary>Runs the hopper animation checks and the program oracle through the verification dispatcher.</summary>
    [VerificationCommand("--hopper-instruction-program-definitions")]
    internal static int HopperInstructionProgramDefinitionsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom));
        Suite(nameof(HopperAnimationsVerification.VerifyHopperAnimations), () => HopperAnimationsVerification.VerifyHopperAnimations(rom));
        return 0;
    }
}
