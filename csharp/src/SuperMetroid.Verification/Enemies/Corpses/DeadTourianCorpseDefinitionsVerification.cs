using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.LookupStream5Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks dead Tourian Zoomer, Ripper, and Skree variant metadata against cartridge tables and live setup.</summary>
internal static class DeadTourianCorpseDefinitionsVerification
{
    /// <summary>The bank-$A9 configuration record of the initially living Shitroid victim's corpse.</summary>
    private const ushort SidehopperInitiallyAliveConfiguration = 0xdd68;

    /// <summary>The bank-$A9 configuration record of the already dead Tourian sidehopper's corpse.</summary>
    private const ushort SidehopperInitiallyDeadConfiguration = 0xdd78;

    /// <summary>Each species' corpse, variant count and native instruction and configuration selector tables.</summary>
    private static readonly DeadTourianCorpseDefinitionsFamily[] Families =
        [
            new DeadTourianCorpseDefinitionsFamily(DeadTourianCorpseSpecies.Zoomer, EnemyDefinitionId.CorpseZoomer,
                3, 0xa9d86a, 0xa9d870),
            new DeadTourianCorpseDefinitionsFamily(DeadTourianCorpseSpecies.Ripper, EnemyDefinitionId.CorpseRipper,
                2, 0xa9d897, 0xa9d89b),
            new DeadTourianCorpseDefinitionsFamily(DeadTourianCorpseSpecies.Skree, EnemyDefinitionId.CorpseSkree,
                3, 0xa9d8c0, 0xa9d8c6),
        ];

    /// <summary>The bank-$A9 configuration record the cartridge selects for <paramref name="species"/> variant <paramref name="variantIndex"/>.</summary>
    internal static ushort ConfigurationPointer(ISnesAddressSpace rom, DeadTourianCorpseSpecies species, int variantIndex) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWord(Families.Single(family => family.Species == species).ConfigurationTable + variantIndex * 2);

    /// <summary>Compares eight variant selectors and configurations and exercises their production initializers.</summary>
    internal static void VerifyDeadTourianCorpseDefinitions(CartridgeImportAddressSpace rom)
    {
        foreach (var family in Families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                DeadTourianCorpseDefinition definition =
                    DeadTourianCorpses.For(family.Species, variantIndex);
                VerifyVariantPlaysItsCorpse(rom, family, variantIndex, definition);
                VerifyDeadTourianCorpseConfiguration(
                    rom, family.Species, variantIndex, definition);
                VerifyDeadTourianCorpseProductionInitialization(
                    rom, family.Species, family.EnemyDefinition, variantIndex, definition);
            }

            AssertThrows<ArgumentOutOfRangeException>(
                () => DeadTourianCorpses.For(family.Species, family.VariantCount),
                $"dead {family.Species} selector past definitions");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => DeadTourianCorpses.For((DeadTourianCorpseSpecies)3, 0),
            "unknown dead Tourian corpse species");
        VerifyCoexistingDeadMonsterSchedulesShareNoHeapBytes(rom);
        Console.WriteLine(
            "Dead Tourian corpse definitions: 16 selectors, eight decoded configuration records' upload lists and heights, all eight real initializers' own fresh rot schedules, and disjoint retail heap placement pass.");
    }

    /// <summary>
    /// The variant's native list plays the corpse the import decodes it to (#1810). The cartridge selects a list per
    /// variant; the game knows a corpse by how it lies, and a variant that shows the same picture as an earlier one is that
    /// corpse. Its list is read as the corpse's segment, with the spritemap its own held frame names in place of the
    /// corpse's: the same beats, timing and sleep, so the variant plays the decoded corpse.
    /// </summary>
    private static void VerifyVariantPlaysItsCorpse(
        CartridgeImportAddressSpace rom,
        DeadTourianCorpseDefinitionsFamily family,
        int variantIndex,
        DeadTourianCorpseDefinition definition)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort variantEntry = cartridge.ReadCartridgeWord(family.InstructionTable + variantIndex * 2);
        ushort variantSpritemap = cartridge.ReadCartridgeWord(DeadTourianNativeListAddresses.Bank << 16 | variantEntry + 2);
        EnemyFrameId corpseFrame = DeadTourianAnimations.Segments.Beats(definition.InitialAnimation)
            .OfType<AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame>().Single().Shown;
        NativeProgramEntry<DeadTourianSegment>[] entries =
        [
            .. DeadTourianNativeListAddresses.Entries.Select(entry => entry.Program == definition.InitialAnimation
                ? entry with { Entry = variantEntry }
                : entry),
        ];
        NativeEnemyAnimationOracle.Lists(cartridge, DeadTourianNativeListAddresses.Bank, entries, DeadTourianAnimations.Segments,
            frame => frame == corpseFrame ? variantSpritemap : DeadTourianFrameToolingDefinitions.Spritemap(frame),
            new NativeEnemyCueRules<DeadTourianCue>(
                word => DeadTourianNativeListAddresses.Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) => AssertEqual(DeadTourianNativeListAddresses.Instructions[word].Kind, cue.Kind, $"{at}: cue")));
    }

    /// <summary>
    /// Confirms the finding that lets each corpse own its rot schedule (#1472): the cartridge keeps every dead monster's
    /// schedule in one shared heap, but the ten records of the dead monsters, which the Shitroid room holds together, place
    /// their schedules in pairwise disjoint heap bytes, so no corpse's rot reaches another's.
    /// </summary>
    private static void VerifyCoexistingDeadMonsterSchedulesShareNoHeapBytes(CartridgeImportAddressSpace rom)
    {
        var schedules = new List<CorpseRottingConfiguration>
        {
            CorpseRottingConfigurationCodec.Decode(rom, SidehopperInitiallyAliveConfiguration),
            CorpseRottingConfigurationCodec.Decode(rom, SidehopperInitiallyDeadConfiguration),
        };
        foreach (var family in Families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
                schedules.Add(CorpseRottingConfigurationCodec.Decode(rom, ConfigurationPointer(rom, family.Species, variantIndex)));
        }

        for (int first = 0; first < schedules.Count; first++)
        {
            for (int second = first + 1; second < schedules.Count; second++)
            {
                AssertTrue(!schedules[first].ScheduleHeapBytes.Overlaps(schedules[second].ScheduleHeapBytes),
                    $"{schedules[first].UploadList} and {schedules[second].UploadList} rot schedules share no heap byte");
            }
        }
    }

    /// <summary>
    /// Compares one decoded configuration record with the typed definition and the extraction's row movers: the kind whose
    /// upload list it names, the corpse's height and its wrap.
    /// </summary>
    private static void VerifyDeadTourianCorpseConfiguration(
        CartridgeImportAddressSpace rom,
        DeadTourianCorpseSpecies species,
        int variantIndex,
        DeadTourianCorpseDefinition definition)
    {
        CorpseRottingConfiguration configuration = CorpseRottingConfigurationCodec.Decode(rom, ConfigurationPointer(rom, species, variantIndex));
        AssertEqual<CorpseRowUploadList?>(DeadTourianCorpses.RowUploadList(species, variantIndex), configuration.UploadList,
            $"dead {species} variant {variantIndex}'s record names the row upload list the extraction finds its corpse's characters by");
        AssertEqual(definition.RowCount, configuration.RowCount, $"dead {species} variant {variantIndex} corpse height");
    }

    /// <summary>Checks one corpse initializer's installed instruction, rot table, callbacks, and row limits.</summary>
    private static void VerifyDeadTourianCorpseProductionInitialization(
        CartridgeImportAddressSpace rom,
        DeadTourianCorpseSpecies species,
        EnemyDefinitionId enemyDefinition,
        int variantIndex,
        DeadTourianCorpseDefinition expected)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(LookupStream5CorpseFixtureArtwork(rom)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = DeadTourianBehavior.InitializeDeadTourianCorpse;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(enemyDefinition, slot.Definition);
        slot.SetSpawnParameter1(enemyDefinition, checked((ushort)(variantIndex * 2)));

        initialize(slot);

        DeadTourianCorpseEnemyState state = (enemies.DeadTourianCorpses[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadTourianCorpses[0]."));
        AssertEqual(species, state.Profile.Species,
            $"dead {species} variant {variantIndex} species");
        AssertEqual(new AnimationPlayback<DeadTourianSegment>(DeadTourianAnimations.StartingWith(expected.InitialAnimation), AnimationPlace.StartOfAnimation),
            state.Playback,
            $"dead {species} variant {variantIndex} started animation");
        CorpseRotScheduleChecks.AssertFreshSchedule(expected.RowCount, state.Rot, $"dead {species} variant {variantIndex}");
    }


    /// <summary>Runs the corpse definition comparison against the retail ROM.</summary>
    [VerificationCommand("--dead-tourian-corpse-definitions")]
    internal static int DeadTourianCorpseDefinitionsCommand()
    {
        Suite(nameof(VerifyDeadTourianCorpseDefinitions), () => VerifyDeadTourianCorpseDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
