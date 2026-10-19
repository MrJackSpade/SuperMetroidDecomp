using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>Checks the hoppers' animation selection and the side effects of their typed programs.</summary>
internal static class HopperAnimationDefinitionsVerification
{
    /// <summary>
    /// The $A3:AAC2-AAE1 selector tables pick the same lists as <see cref="HopperAnimationDefinitions.Program"/>. For
    /// every species and orientation the real initializer, jump and landing start the selected programs; the jump turns
    /// off-screen processing on and plays the airborne sound, and the landing turns it off, plays the landing sound,
    /// shows its four timed poses and then lets main AI hop again.
    /// </summary>
    internal static void VerifyHopperAnimationDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        int[] tables = [0xa3aac2, 0xa3aaca, 0xa3aad2, 0xa3aada];
        for (ushort variant = 0; variant < 4; variant++)
        for (int selector = 0; selector < tables.Length; selector++)
        {
            bool upsideDown = (selector & 1) != 0;
            bool jumping = (selector & 2) != 0;
            AssertEqual(ReadHopperAnimationWord(rom, tables[selector] + variant * 2),
                NativeEnemyProgramOracle.EntryOf(HopperNativeListAddresses.Entries,
                    HopperAnimationDefinitions.Program(variant, upsideDown, jumping)),
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
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definitions[variant], slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = HopperFrameDefinitionsBank, VariantIndex = variant });
            slot.SetSpawnParameter1(upsideDown ? (ushort)1 : (ushort)0);
            enemies.HoppersBehavior.InitializeHopper(slot);
            HopperEnemyState state = enemies.HopperStates[0] ?? throw new InvalidOperationException("Verification requires enemies.HopperStates[0].");
            HopperProgram landed = HopperAnimationDefinitions.Program(variant, upsideDown, jumping: false);
            HopperProgram jumping = HopperAnimationDefinitions.Program(variant, upsideDown, jumping: true);
            AssertEqual(AnimationProgramCursor<HopperProgram>.Start(landed), state.Program, $"hopper initial landed program {context}");

            state.XVelocity = 3;
            HoppersBehavior.StartHopperJump(slot, state, upsideDown, false);
            AssertEqual(AnimationProgramCursor<HopperProgram>.Start(jumping), state.Program, $"hopper jumping program {context}");
            ushort airborne = HopperAnimationsConstants.Spritemaps(jumping).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([airborne, airborne], [1, 1]),
                EnemyInstructionRunner.ShownFrames(slot, 2), $"hopper airborne pose {context}");
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper jumping program {context} enables off-screen processing");
            AssertEqual(variant is 0 or 1 ? (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x005d) : null, enemies.HoppersBehavior.LastHopperSoundEffect,
                $"hopper jumping sound {context}");

            HoppersBehavior.LandHopper(slot, state);
            AssertEqual(AnimationProgramCursor<HopperProgram>.Start(landed), state.Program, $"hopper landed handoff {context}");
            int[] holds = [2, 5, 2, 3];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(HopperAnimationsConstants.Spritemaps(landed), holds),
                EnemyInstructionRunner.ShownFrames(slot, holds.Sum()), $"hopper landing poses {context}");
            AssertTrue(!state.ReadyToHop, $"hopper landed program {context} holds its last pose before readiness");
            EnemyInstructionRunner.ShownFrames(slot, 1);
            AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper landed program {context} disables off-screen processing");
            AssertTrue(state.ReadyToHop, $"hopper landed program {context} publishes ready-to-hop");
            AssertEqual(variant is 0 or 1 ? (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x005e) : null, enemies.HoppersBehavior.LastHopperSoundEffect,
                $"hopper landed sound {context}");
        }

        Suite(nameof(VerifyTourianSidehopperInstructionAlias), () => VerifyTourianSidehopperInstructionAlias(artwork));
        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.Program(4, false, false),
            "hopper animation variant beyond authored table");
        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.Program(ushort.MaxValue, true, true),
            "restored hopper animation variant does not wrap into authored table");
        Console.WriteLine(artwork is null
            ? "Hopper animations: the native selector tables, five production definitions and their sound, processing and readiness cues pass."
            : "Installed Sidehopper/Dessgeega: sixteen floor/ceiling programs and the Tourian alias run with the installed artwork.");
    }

    /// <summary>The Sidehopper and Dessgeega animation bank.</summary>
    private const byte HopperFrameDefinitionsBank = SuperMetroid.Core.Enemies.Hoppers.Artwork.HopperFrameToolingDefinitions.Bank;

    /// <summary>Checks the Tourian Sidehopper uses the large-Sidehopper landed program and sound.</summary>
    private static void VerifyTourianSidehopperInstructionAlias(
        EnemyTileArtworkCatalog? artwork)
    {
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.SidehopperTourian, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = HopperFrameDefinitionsBank, VariantIndex = 1 });
        enemies.HoppersBehavior.InitializeHopper(slot);
        EnemyInstructionRunner.ShownFrames(slot, 1);
        AssertEqual((SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x005e), enemies.HoppersBehavior.LastHopperSoundEffect,
            "Tourian Sidehopper shares the large-Sidehopper landed program");
    }

    /// <summary>Reads a native hopper word for independent bank-A3 definition comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Hopper animation words")]
    internal static ushort ReadHopperAnimationWord(SuperMetroidAddressSpace source, int address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWord(address);

    /// <summary>Runs the hopper animation checks and the program oracle through the verification dispatcher.</summary>
    [VerificationCommand("--hopper-instruction-program-definitions")]
    internal static int HopperInstructionProgramDefinitionsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom));
        Suite(nameof(HopperAnimationsVerification.VerifyHopperAnimations), () => HopperAnimationsVerification.VerifyHopperAnimations(rom));
        return 0;
    }
}
