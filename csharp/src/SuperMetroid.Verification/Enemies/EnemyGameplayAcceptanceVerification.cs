using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Enemies.EnemyDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropChanceDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Enemies.EnemyRoomListsVerification;
using static SuperMetroid.Verification.Enemies.EnemyVulnerabilityDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyVulnerabilityDispatchVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms the compiled enemy catalog closes its references and keeps editable enemy art outside gameplay state.</summary>
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
        Suite(nameof(VerifyEnemyPopulationPresentationIsolation), () => VerifyEnemyPopulationPresentationIsolation(source));
        Suite(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops());
        Console.WriteLine("Enemy gameplay acceptance: complete stock definitions, reference closure, " +
            "vulnerability selection, linked population initialization and editable-art isolation pass.");
    }

    /// <summary>Resolves vulnerability and drop records referenced by every primary and auxiliary enemy header.</summary>
    private static void VerifyEnemyDefinitionReferenceClosure()
    {
        Span<byte> drop = stackalloc byte[EnemyDropChanceDefinitions.RecordSize];
        foreach (EnemyDefinitionId pointer in RoomEnemyDefinitionCatalog.Pointers.Concat(RoomEnemyAuxiliaryDefinitionCatalog.Pointers))
        {
            RoomEnemyDefinition definition = RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer, out var auxiliary)
                ? auxiliary : RoomEnemyDefinitionCatalog.Get(pointer);
            ushort vulnerability = definition.VulnerabilityPointer == 0
                ? EnemyVulnerabilityDefinitions.DefaultPointer : definition.VulnerabilityPointer;
            for (int field = 0; field < EnemyVulnerabilityDefinitions.RecordSize; field++)
                _ = EnemyVulnerabilityDefinitions.Read(vulnerability, field);
            if (definition.ItemDropChancesPointer != 0)
                EnemyDropChanceDefinitions.Copy(definition.ItemDropChancesPointer, drop);
        }
        Console.WriteLine("Enemy reference closure: all 153 headers resolve compiled vulnerabilities and drops.");
    }

    /// <summary>Loads a retail KiHunter body-wing population without cartridge memory and checks a PNG edit affects VRAM but no enemy slot or quota.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyEnemyPopulationPresentationIsolation(CartridgeImportAddressSpace source)
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        string stockPath = Path.Combine(temporary.Root, "stock");
        string overridePath = Path.Combine(temporary.Root, "overrides");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(stockPath, null);

        // Body/wing adjacency is gameplay-visible: the wing initializer aliases the
        // immediately preceding physical slot. Choose one complete retail pair list,
        // not a hand-written visual-only approximation or a full room playthrough.
        CartridgeRoomState state = RoomStateDefinitions.All
            .Where(candidate => IsKiHunterPairList(candidate.EnemyPopulationPointer))
            .OrderBy(candidate => RoomEnemyPopulationDefinitions.Get(candidate.EnemyPopulationPointer).Records.Length)
            .ThenBy(candidate => candidate.EnemyPopulationPointer).First();
        RoomEnemyPopulationDefinition population = RoomEnemyPopulationDefinitions.Get(state.EnemyPopulationPointer);
        (RoomEnemySystem baseline, SnesVram beforeVram, SnesCgram beforeColors) = Load(stock);
        AssertEqual(population.Records.Length, baseline.SlotTable.EnemyCount, "compiled population publishes its exact count");
        AssertEqual(population.Records.Length * RoomEnemySystem.NativeSlotSize, baseline.SlotTable.FirstFreeEnemyIndex,
            "compiled population publishes its physical first-free offset");
        AssertEqual(population.DeathQuota, baseline.PopulationLoader.DeathQuota, "compiled population publishes its authored death quota");
        for (int index = 0; index < population.Records.Length; index++)
        {
            RoomEnemySlot slot = baseline.SlotTable.Slots[index];
            AssertEqual(index * RoomEnemySystem.NativeSlotSize, slot.NativeIndex, "linked population physical slot order");
            AssertEqual(population.Records.Span[index], slot.Spawn.Population, "linked population preserves every spawn word");
            AssertEqual(RoomEnemyDefinitionCatalog.Get(slot.EnemyDefinitionPointer), slot.Definition, "linked slot uses compiled header");
            AssertEqual(slot.Definition.Health, slot.Health, "linked slot starts at compiled health");
            if ((index & 1) != 0)
            {
                RoomEnemySlot body = baseline.SlotTable.Slots[index - 1];
                AssertEqual(body.Position.X.Whole, slot.Position.X.Whole, "wing initializer retains its preceding body's X");
                AssertEqual(body.Position.Y.Whole, slot.Position.Y.Whole, "wing initializer retains its preceding body's Y");
                AssertEqual(body.VramTilesIndex, slot.VramTilesIndex, "wing initializer retains its preceding body's tile binding");
            }
        }

        // All three body variants intentionally share one native DMA source. Edit
        // that source's aliases consistently, as a real artist must; conflicting
        // alias files are a loud validation error, not independent gameplay tuning.
        EnemyDefinitionId editedPointer = EnemyDefinitionId.KihunterGreen;
        RoomEnemyDefinition editedDefinition = RoomEnemyDefinitionCatalog.Get(editedPointer);
        Directory.CreateDirectory(overridePath);
        foreach (EnemyDefinitionId alias in RoomEnemyGraphicsSetDefinitions.Pointers
                     .SelectMany(pointer => RoomEnemyGraphicsSetDefinitions.Get(pointer).Records.ToArray())
                     .Select(record => record.DefinitionPointer).Distinct()
                     .Where(pointer => RoomEnemyDefinitionCatalog.Get(pointer).TileDataAddress == editedDefinition.TileDataAddress &&
                         RoomEnemyDefinitionCatalog.Get(pointer).TileDataSize == editedDefinition.TileDataSize))
        {
            int tiles = (editedDefinition.TileDataSize & 0x7fff) / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            using var input = File.OpenRead(Path.Combine(stockPath, EnemyTileArtworkFormat.FileName(alias)));
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)((tiles + columns - 1) / columns * 8))));
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overridePath, EnemyTileArtworkFormat.FileName(alias)));
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        }
        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockPath, EnemyTileArtworkFormat.FileName(editedPointer)));
        string stockHash = Convert.ToHexString(SHA256.HashData(stockBytes));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockPath, overridePath);
        (RoomEnemySystem changed, SnesVram afterVram, SnesCgram afterColors) = Load(edited);
        byte[] expectedVram = beforeVram.Bytes.ToArray();
        foreach (RoomEnemyGraphicsSetEntry entry in baseline.GraphicsSet
                     .Where(entry => entry.Definition.TileDataAddress == editedDefinition.TileDataAddress &&
                         entry.Definition.TileDataSize == editedDefinition.TileDataSize))
            expectedVram[RoomEnemyRomLayout.VramByteBase + entry.StagingOffset] ^= 0x80;
        AssertTrue(!beforeVram.ImageEquals(afterVram) && afterVram.Bytes.SequenceEqual(expectedVram),
            "PNG replacement changes exactly the authored pixel at its production VRAM destination");
        AssertTrue(new CgramPalettes(beforeColors).ImageEquals(new CgramPalettes(afterColors)), "PNG replacement does not change enemy colors");
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "PNG replacement changes selected presentation identity");
        AssertEqual(edited.ContentIdentity, EnemyTileArtworkFiles.Load(stockPath, overridePath).ContentIdentity,
            "enemy replacement persists through a fresh catalog load");
        AssertEqual(stockHash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(
            Path.Combine(stockPath, EnemyTileArtworkFormat.FileName(editedPointer))))), "enemy replacement leaves stock untouched");
        foreach (PropertyInfo property in MetadataShape.Properties(typeof(RoomEnemySlot), BindingFlags.Instance | BindingFlags.Public))
        for (int index = 0; index < baseline.SlotTable.Slots.Count; index++)
            AssertEqual(property.GetValue(baseline.SlotTable.Slots[index]), property.GetValue(changed.SlotTable.Slots[index]),
                $"enemy artwork cannot change slot {index} {property.Name}");
        AssertEqual(baseline.PopulationLoader.DeathQuota, changed.PopulationLoader.DeathQuota, "enemy artwork cannot change death quota");
        AssertEqual(baseline.SlotTable.FirstFreeEnemyIndex, changed.SlotTable.FirstFreeEnemyIndex, "enemy artwork cannot change slot allocation");
        AssertEqual(baseline.RoomStatus.BossId, changed.RoomStatus.BossId, "enemy artwork cannot change boss selection");
        Console.WriteLine($"Enemy population binding: {population.Records.Length} linked slots at $A1:{population.Pointer:X4} " +
            "initialize with RAM-only memory; PNG edits change VRAM, not any slot field or quota.");

        (RoomEnemySystem, SnesVram, SnesCgram) Load(EnemyTileArtworkCatalog artwork)
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            var vram = new SnesVram(); var colors = new SnesCgram();
            enemies.Load(new RoomEnemyLoadRequest(CompiledRoomEnemyDefinitions.Instance, state.EnemyPopulationPointer, state.EnemyTilesetPointer, vram, colors, new StandaloneRoomEnemyServices(() => 1)));
            return (enemies, vram, colors);
        }

        static bool IsKiHunterPairList(ushort pointer)
        {
            ReadOnlySpan<RoomEnemyPopulationRecord> records = RoomEnemyPopulationDefinitions.Get(pointer).Records.Span;
            if (records.IsEmpty || (records.Length & 1) != 0) return false;
            for (int index = 0; index < records.Length; index++)
                if (records[index].DefinitionPointer != ((index & 1) == 0
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
