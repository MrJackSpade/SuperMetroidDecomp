using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.LookupStream5Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks dead Tourian Zoomer, Ripper, and Skree variant metadata against cartridge tables and live setup.</summary>
internal static class DeadTourianCorpseDefinitionsVerification
{
    /// <summary>Each species' corpse, variant count and native instruction and configuration selector tables.</summary>
    private static readonly (DeadTourianCorpseSpecies Species, EnemyDefinitionId EnemyDefinition, int VariantCount,
        int InstructionTable, int ConfigurationTable)[] Families =
        [
            (DeadTourianCorpseSpecies.Zoomer, EnemyDefinitionId.CorpseZoomer,
                3, 0xa9d86a, 0xa9d870),
            (DeadTourianCorpseSpecies.Ripper, EnemyDefinitionId.CorpseRipper,
                2, 0xa9d897, 0xa9d89b),
            (DeadTourianCorpseSpecies.Skree, EnemyDefinitionId.CorpseSkree,
                3, 0xa9d8c0, 0xa9d8c6),
        ];

    /// <summary>The bank-$A9 configuration record the cartridge selects for <paramref name="species"/> variant <paramref name="variantIndex"/>.</summary>
    internal static ushort ConfigurationPointer(ISnesAddressSpace rom, DeadTourianCorpseSpecies species, int variantIndex) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWord(Families.Single(family => family.Species == species).ConfigurationTable + variantIndex * 2);

    /// <summary>Compares eight variant selectors and configurations and exercises their production initializers.</summary>
    internal static void VerifyDeadTourianCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        foreach (var family in Families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                DeadTourianCorpseDefinition definition =
                    DeadTourianCorpseDefinitions.For(family.Species, variantIndex);
                AssertEqual(
                    CartridgeImportSource.Require(rom).ReadCartridgeWord((family.InstructionTable + variantIndex * 2)),
                    NativeEnemyProgramOracle.EntryOf(DeadTourianNativeListAddresses.Entries, definition.InitialProgram),
                    $"dead {family.Species} variant {variantIndex} instruction selector");
                VerifyDeadTourianCorpseConfiguration(
                    rom, family.Species, variantIndex, definition);
                VerifyDeadTourianCorpseProductionInitialization(
                    rom, family.Species, family.EnemyDefinition, variantIndex, definition);
            }

            AssertThrows<ArgumentOutOfRangeException>(
                () => DeadTourianCorpseDefinitions.For(family.Species, family.VariantCount),
                $"dead {family.Species} selector past definitions");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => DeadTourianCorpseDefinitions.For((DeadTourianCorpseSpecies)3, 0),
            "unknown dead Tourian corpse species");
        Console.WriteLine(
            "Dead Tourian corpse definitions: 16 selectors, 64 configuration words, eight derived wrap offsets, and all eight real initializers pass with migrated metadata reads forbidden.");
    }

    /// <summary>Checks the consumed configuration words and derives the wrap offset from the rotation table.</summary>
    private static void VerifyDeadTourianCorpseConfiguration(
        SuperMetroidAddressSpace rom,
        DeadTourianCorpseSpecies species,
        int variantIndex,
        DeadTourianCorpseDefinition definition)
    {
        int address = 0xa90000 | ConfigurationPointer(rom, species, variantIndex);
        ushort Word(int index) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
        ushort[] actual = [definition.RottingTablePointer, definition.VramTransferPointer, definition.EntryCount];
        AssertTrue(actual.SequenceEqual(new[] { Word(0), Word(1), Word(4) }),
            $"dead {species} variant {variantIndex} configuration words");

        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(Word(6) + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead {species} variant {variantIndex} wrap offset");
    }

    /// <summary>Checks one corpse initializer's installed instruction, rot table, callbacks, and row limits.</summary>
    private static void VerifyDeadTourianCorpseProductionInitialization(
        SuperMetroidAddressSpace rom,
        DeadTourianCorpseSpecies species,
        EnemyDefinitionId enemyDefinition,
        int variantIndex,
        DeadTourianCorpseDefinition expected)
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = LookupStream5CorpseFixtureArtwork(rom) };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = enemies.DeadTourianBehavior.InitializeDeadTourianCorpse;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(enemyDefinition, slot.Definition);
        slot.SetSpawnParameter1(checked((ushort)(variantIndex * 2)));

        initialize(slot);

        DeadTourianCorpseEnemyState state = (enemies.DeadTourianCorpses[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadTourianCorpses[0]."));
        AssertEqual(species, (state ?? throw new InvalidOperationException("Verification requires state.")).Species,
            $"dead {species} variant {variantIndex} species");
        AssertEqual(variantIndex, state.VariantIndex,
            $"dead {species} variant {variantIndex} index");
        AssertEqual(AnimationProgramCursor<DeadTourianProgram>.Start(expected.InitialProgram),
            enemies.DeadTourianBehavior.Extensions[slot.SlotIndex].Program,
            $"dead {species} variant {variantIndex} started animation");
        CorpseRotTableChecks.AssertConfiguredRotTable(enemies, expected.RottingTablePointer, expected.EntryCount,
            state.RotTable, $"dead {species} variant {variantIndex}");
        AssertTrue(DeadMonsterRottingDefinitions.ForTransferTable(expected.VramTransferPointer).SequenceEqual(state.VramTransfers),
            $"dead {species} variant {variantIndex} uploads the configured VRAM transfer list");
        AssertEqual((ushort)state.Profile.RowLayout,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa90000 | (ConfigurationPointer(rom, species, variantIndex) + 12)),
            $"dead {species} variant {variantIndex} configuration names the tile-row layout production copies with");
        AssertEqual(expected.EntryCount - 1, state.YLimit,
            $"dead {species} variant {variantIndex} Y limit");
        AssertEqual(expected.EntryCount - 2, state.LateMoveEntryIndex,
            $"dead {species} variant {variantIndex} late-move row");
        AssertEqual(expected.WrapOffset, state.WrapOffset,
            $"dead {species} variant {variantIndex} wrap offset");
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
