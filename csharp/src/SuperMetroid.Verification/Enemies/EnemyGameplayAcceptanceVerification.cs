using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.VerificationOracles;
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
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

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
            for (int field = 0; field < EnemyVulnerabilityDefinitions.RecordSize; field++)
                _ = EnemyVulnerabilityDefinitions.Read(definition.Vulnerability, field);
            if (definition.DropChances is { } table)
                EnemyDropChanceDefinitions.Copy(table, drop);
        }
        Console.WriteLine("Enemy reference closure: all 153 headers resolve compiled vulnerabilities and drops.");
    }

    /// <summary>Loads a retail KiHunter body-wing population without cartridge memory and checks a PNG edit affects the OBJ characters but no enemy slot or quota.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyEnemyPopulationPresentationIsolation(CartridgeImportAddressSpace source)
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        string stockPath = Path.Combine(temporary.Root, "stock");
        string overridePath = Path.Combine(temporary.Root, "overrides");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        InstalledEnemyArtwork stock = EnemyTileArtworkFiles.Load(stockPath, null);

        // Body/wing adjacency is gameplay-visible: the wing initializer aliases the
        // immediately preceding physical slot. Choose one complete retail pair list,
        // not a hand-written visual-only approximation or a full room playthrough.
        RoomStateSettings state = RoomStateDefinitions.All
            .Where(candidate => IsKiHunterPairList(RoomEnemyPopulationDefinitions.PointerOf(candidate.EnemyPopulation)))
            .OrderBy(candidate => candidate.EnemyPopulation.Records.Length)
            .ThenBy(candidate => RoomEnemyPopulationDefinitions.PointerOf(candidate.EnemyPopulation)).First();
        RoomEnemyPopulationDefinition population = state.EnemyPopulation;
        (RoomEnemySystem baseline, byte[] beforeCharacters, SamusColourRig beforeColors) = Load(stock);
        AssertEqual(population.Records.Length, baseline.SlotTable.EnemyCount, "compiled population publishes its exact count");
        AssertEqual(population.Records.Length * EnemySlotTable.EnemyIndexStride, baseline.SlotTable.FirstFreeEnemyIndex,
            "compiled population publishes its physical first-free offset");
        AssertEqual(population.DeathQuota, baseline.PopulationLoader.DeathQuota, "compiled population publishes its authored death quota");
        for (int index = 0; index < population.Records.Length; index++)
        {
            RoomEnemySlot slot = baseline.SlotTable.Slots[index];
            AssertEqual(index * EnemySlotTable.EnemyIndexStride, slot.EnemyIndex, "linked population physical slot order");
            AssertEqual(population.Records.Span[index], slot.Spawn.Population, "linked population preserves every spawn word");
            AssertEqual(RoomEnemyDefinitionCatalog.Get(slot.DefinitionId), slot.Definition, "linked slot uses compiled header");
            AssertEqual(slot.Definition.Health, slot.Health, "linked slot starts at compiled health");
            if ((index & 1) != 0)
            {
                RoomEnemySlot body = baseline.SlotTable.Slots[index - 1];
                AssertEqual(body.Position.X.Whole, slot.Position.X.Whole, "wing initializer retains its preceding body's X");
                AssertEqual(body.Position.Y.Whole, slot.Position.Y.Whole, "wing initializer retains its preceding body's Y");
                AssertEqual(body.TileIndexBase, slot.TileIndexBase, "wing initializer retains its preceding body's tile binding");
            }
        }

        // Each definition shows its own installed sheet (#1412): editing one body variant's sheet changes that
        // variant's uploads alone, though the cartridge's three variants share one tile source.
        EnemyDefinitionId editedPointer = EnemyDefinitionId.KihunterGreen;
        RoomEnemyDefinition editedDefinition = RoomEnemyDefinitionCatalog.Get(editedPointer);
        Directory.CreateDirectory(overridePath);
        foreach (EnemyDefinitionId alias in new[] { editedPointer })
        {
            int tiles = editedDefinition.TileByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            using var input = File.OpenRead(Path.Combine(stockPath, EnemyTileArtworkFiles.FileName(alias)));
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)((tiles + columns - 1) / columns * 8))));
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overridePath, EnemyTileArtworkFiles.FileName(alias)));
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        }
        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockPath, EnemyTileArtworkFiles.FileName(editedPointer)));
        string stockHash = Convert.ToHexString(SHA256.HashData(stockBytes));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockPath, overridePath);
        (RoomEnemySystem changed, byte[] afterCharacters, SamusColourRig afterColors) = Load(edited);
        RoomEnemyGraphicsSetEntry[] entries = [.. baseline.GraphicsSet.Entries];
        SpriteCharacterPlacement[] uploads = [.. baseline.Room.Session.SpriteCharacters.Uploads];
        AssertEqual(entries.Length, uploads.Length, "each selected enemy tile set lands as one OBJ character upload");
        byte[] expectedCharacters = beforeCharacters.ToArray();
        for (int index = 0; index < entries.Length; index++)
            if (entries[index].DefinitionId == editedPointer)
                expectedCharacters[uploads[index].FirstByte] ^= 0x80;
        static string ChangedOffsets(ReadOnlySpan<byte> before, ReadOnlySpan<byte> after)
        {
            var changed = new List<string>();
            for (int offset = 0; offset < before.Length; offset++)
                if (before[offset] != after[offset])
                    changed.Add($"{offset:X5}:{before[offset]:X2}->{after[offset]:X2}");
            return string.Join(",", changed);
        }
        AssertEqual(ChangedOffsets(beforeCharacters, expectedCharacters), ChangedOffsets(beforeCharacters, afterCharacters),
            "PNG replacement changes exactly the authored pixel where its upload lands on the OBJ characters");
        AssertTrue(new CgramPalettes(beforeColors).ImageEquals(new CgramPalettes(afterColors)), "PNG replacement does not change enemy colors");
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "PNG replacement changes selected presentation identity");
        AssertEqual(edited.ContentIdentity, EnemyTileArtworkFiles.Load(stockPath, overridePath).ContentIdentity,
            "enemy replacement persists through a fresh catalog load");
        AssertEqual(stockHash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(
            Path.Combine(stockPath, EnemyTileArtworkFiles.FileName(editedPointer))))), "enemy replacement leaves stock untouched");
        foreach (PropertyInfo property in MetadataShape.Properties(typeof(RoomEnemySlot), BindingFlags.Instance | BindingFlags.Public))
        for (int index = 0; index < baseline.SlotTable.Slots.Count; index++)
            AssertEqual(property.GetValue(baseline.SlotTable.Slots[index]), property.GetValue(changed.SlotTable.Slots[index]),
                $"enemy artwork cannot change slot {index} {property.Name}");
        AssertEqual(baseline.PopulationLoader.DeathQuota, changed.PopulationLoader.DeathQuota, "enemy artwork cannot change death quota");
        AssertEqual(baseline.SlotTable.FirstFreeEnemyIndex, changed.SlotTable.FirstFreeEnemyIndex, "enemy artwork cannot change slot allocation");
        AssertEqual(baseline.RoomStatus.BossId, changed.RoomStatus.BossId, "enemy artwork cannot change boss selection");
        Console.WriteLine($"Enemy population binding: {population.Records.Length} linked slots at {population.Name} " +
            "initialize with RAM-only memory; PNG edits change the OBJ characters, not any slot field or quota.");

        LoadResult Load(InstalledEnemyArtwork artwork)
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 1)) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
            var vram = new SnesVram(); var colors = new SamusColourRig();
            enemies.Load(new RoomEnemyLoadRequest(state.EnemyPopulation, state.EnemyGraphicsSet, BgArtworkFixtures.For(vram), colors.Gameplay));
            // The room's enemy tile set reaches the OBJ characters as named transfers.
            var transfers = new EnemyTileTransfers(artwork);
            enemies.GraphicsSet.ShowGraphics();
            byte[] characters = new ObjCharacterTablePage().Page(enemies.Room.Session.SpriteCharacters, transfers).Characters.ToArray();
            return new LoadResult(enemies, characters, colors);
        }

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
