using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropChanceDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Enemies.EnemyRoomListsVerification;
using static SuperMetroid.Verification.Enemies.EnemyVulnerabilityDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyVulnerabilityDispatchVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

/// <summary>Confirms the compiled enemy catalog closes its references and initializes linked populations.</summary>
internal static class EnemyGameplayAcceptanceVerification
{
    /// <summary>
    /// Acceptance for the statically inventoried enemy definition/population boundary.
    /// Import bytes are an independent oracle, never the production memory provider.
    /// No controller sequence or frame probe is used to discover remaining reads.
    /// </summary>
    internal static void VerifyEnemyGameplayAcceptance()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledEnemyDefinitions), () => VerifyCompiledEnemyDefinitions());
        Suite(nameof(VerifyCompiledEnemyRoomLists), () => VerifyCompiledEnemyRoomLists());
        Suite(nameof(VerifyEnemyVulnerabilityDefinitions), () => VerifyEnemyVulnerabilityDefinitions(source));
        Suite(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(source));
        Suite(nameof(VerifyEnemyDefinitionReferenceClosure), () => VerifyEnemyDefinitionReferenceClosure());
        Suite(nameof(VerifyEnemyVulnerabilityDispatch), () => VerifyEnemyVulnerabilityDispatch(source));
        Suite(nameof(VerifyEnemyPopulationPresentationIsolation), () => VerifyEnemyPopulationPresentationIsolation());
        Suite(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops());
        Console.WriteLine("Enemy gameplay acceptance: complete stock definitions, reference closure, " +
            "vulnerability selection, linked population initialization pass.");
    }

    /// <summary>Resolves the vulnerabilities and drop chances of every primary and auxiliary enemy header.</summary>
    private static void VerifyEnemyDefinitionReferenceClosure()
    {
        foreach (EnemyDefinitionId pointer in RoomEnemyDefinitionCatalog.Pointers.Concat(RoomEnemyAuxiliaryDefinitionCatalog.Pointers))
        {
            RoomEnemyDefinition definition = RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer, out var auxiliary)
                ? auxiliary : RoomEnemyDefinitionLookup.Get(pointer);
            AssertTrue(EnemyVulnerabilities.Of(definition) is not null, $"{pointer} resolves its vulnerabilities");
            if (definition.DropChances is { } chances)
            {
                AssertEqual(byte.MaxValue, chances.SmallEnergy + chances.BigEnergy + chances.Missile + chances.Nothing +
                    chances.SuperMissile + chances.PowerBomb, $"{pointer}'s drop chances sum to 255");
            }
        }
        Console.WriteLine("Enemy reference closure: all headers resolve compiled vulnerabilities and drop chances.");
    }

    /// <summary>Loads a retail KiHunter body-wing population without cartridge memory and checks its slots, quota and wing placement.</summary>
    private static void VerifyEnemyPopulationPresentationIsolation()
    {
        ImportEnemyArtwork stock = InstalledFixtures.EnemyArtwork;

        // Body/wing adjacency is gameplay-visible: the wing initializer aliases the
        // immediately preceding physical slot. Choose one complete retail pair list,
        // not a hand-written visual-only approximation or a full room playthrough.
        RoomStateSettings state = RoomStateDefinitions.All
            .Where(candidate => IsKiHunterPairList(RoomEnemyPopulationDefinitions.PointerOf(candidate.EnemyPopulation)))
            .OrderBy(candidate => candidate.EnemyPopulation.Records.Length)
            .ThenBy(candidate => RoomEnemyPopulationDefinitions.PointerOf(candidate.EnemyPopulation)).First();
        RoomEnemyPopulationDefinition population = state.EnemyPopulation;
        (RoomEnemySystem baseline, _) = Load(stock);
        AssertEqual(population.Records.Length, baseline.SlotTable.EnemyCount, "compiled population publishes its exact count");
        AssertEqual(population.Records.Length < EnemySlotOrder.Capacity ? baseline.SlotTable.Slots[population.Records.Length] : null,
            baseline.SlotTable.FindFreeSlot(), "a spawn takes the slot after the compiled population");
        AssertEqual(population.DeathQuota, baseline.PopulationLoader.DeathQuota, "compiled population publishes its authored death quota");
        for (int index = 0; index < population.Records.Length; index++)
        {
            RoomEnemySlot slot = baseline.SlotTable.Slots[index];
            AssertEqual(index, slot.SlotIndex, "linked population slot order");
            AssertEqual(population.Records.Span[index], slot.Spawn.Population, "linked population preserves every spawn word");
            AssertEqual(RoomEnemyDefinitionLookup.Get(slot.DefinitionId), slot.Definition, "linked slot uses compiled header");
            AssertEqual(slot.Definition.Health, slot.Health, "linked slot starts at compiled health");
            if ((index & 1) != 0)
            {
                RoomEnemySlot body = baseline.SlotTable.Slots[index - 1];
                AssertEqual(body.Position.X.Whole, slot.Position.X.Whole, "wing initializer retains its preceding body's X");
                AssertEqual(body.Position.Y.Whole, slot.Position.Y.Whole, "wing initializer retains its preceding body's Y");
            }
        }

        Console.WriteLine($"Enemy population binding: {population.Records.Length} linked slots at {population.Name} " +
            "initialize with RAM-only memory.");

        LoadResult Load(ImportEnemyArtwork artwork)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 1)) { BossArtwork = artwork.Bosses };
            var vram = new SnesVram();
            enemies.Load(new RoomEnemyLoadRequest(state.EnemyPopulation, BgArtworkFixtures.For(vram)));
            // The room's enemy tile set as the import composes its pictures over it: each member's tiles where its load lands them.
            var characters = new byte[SpriteArtworkPlacements.TableByteCount];
            foreach (SuperMetroid.AssetExtraction.Enemies.GraphicsSetMemberCharacters entry in enemies.GraphicsSet.Entries)
            {
                ReadOnlySpan<byte> tiles = entry.Member.TilesIn(artwork.Enemies).Span;
                int first = LoadedFirstByte(entry);
                int shown = Math.Min(tiles.Length, characters.Length - first);
                if (shown > 0)
                    tiles[..shown].CopyTo(characters.AsSpan(first));
            }
            return new LoadResult(enemies, characters);
        }

        // The table byte the room load lands a member's tiles from.
        static int LoadedFirstByte(SuperMetroid.AssetExtraction.Enemies.GraphicsSetMemberCharacters entry) =>
            EncounterCharacterPlacementDefinitions.EnemyTileAreaFirstCharacter * SpriteArtworkPlacements.CharacterByteCount + entry.StagingOffset;

        static bool IsKiHunterPairList(ushort pointer)
        {
            ReadOnlySpan<RoomEnemyPopulationRecord> records = RoomEnemyPopulationDefinitions.Get(pointer).Records.Span;
            if (records.IsEmpty || (records.Length & 1) != 0) return false;
            for (int index = 0; index < records.Length; index++)
                if (records[index].DefinitionId != ((index & 1) == 0
                    ? EnemyDefinitionId.KihunterGreen : EnemyDefinitionId.KihunterGreenWings)) return false;
            return true;
        }
    }

    /// <summary>Runs the enemy definition, population, vulnerability, drop, and presentation isolation checks.</summary>
    [VerificationCommand("--enemy-gameplay-acceptance")]
    internal static int EnemyGameplayAcceptanceCommand()
    {
        Suite(nameof(VerifyEnemyGameplayAcceptance), () => VerifyEnemyGameplayAcceptance());
        return 0;
    }
}
