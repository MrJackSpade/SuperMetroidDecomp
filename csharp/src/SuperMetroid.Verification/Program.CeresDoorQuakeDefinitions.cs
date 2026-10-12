using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres door quake definitions")]
    private static void VerifyCeresDoorQuakeDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa6a321;

        for (ushort phase = 0; phase < 4; phase++)
        {
            sbyte expected = unchecked((sbyte)rom.ReadCartridgeByte(sourceAddress + phase));
            AssertEqual(expected, CeresDoorQuakeDefinitions.XOffset(phase),
                $"Ceres private door quake phase {phase}");
        }

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            AssertEqual(
                unchecked((sbyte)rom.ReadCartridgeByte(sourceAddress + (timer & 3))),
                CeresDoorQuakeDefinitions.XOffset((ushort)timer),
                $"Ceres private door quake timer ${timer:X4}");
        }

        var guarded = NewCeresDoorQuakeReadGuard(rom);
        var enemies = new RoomEnemySystem
        {
            CeresStatus = 1,
            TileArtwork = RepositoryInstallation.EnemyTiles,
        };
        enemies.Slots[0].EnemyDefinitionPointer = EnemyDefinitionId.RidleyCeres;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("<Ridley>k__BackingField", flags)!.SetValue(
            enemies,
            new RidleyEnemyState { MovementAnimationEnabled = 0 });
        RoomEnemySlot door = enemies.Slots[1];
        door.EnemyDefinitionPointer = EnemyDefinitionId.CeresDoor;
        door.VariableB = 1;
        door.Position = door.Position.WithWhole(new(100, 80));
        byte[] stockJson = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapCatalog installed = EnemySpritemapCatalog.Load(
            new MemoryStream(stockJson, writable: false));
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = installed });
        short[] expectedOffsets = [0, 0, -4, -1];
        int? baseX = null;
        for (ushort phase = 0; phase < expectedOffsets.Length; phase++)
        {
            enemies.EarthquakeTimer = phase;
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawCeresRidleyImmediateBabyAndDoor(oam, new(0, 0));
            oam.FinalizeFrame();
            baseX ??= oam.GetEntry(0).X;
            AssertEqual(baseX.Value + expectedOffsets[phase], oam.GetEntry(0).X,
                $"Ceres private door production OAM phase {phase}");
        }

        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            NewCeresDoorQuakeReadGuard(rom, blockOverlay: true));
        enemies.EarthquakeTimer = 0;
        var nativeOverlay = new OamBuffer();
        nativeOverlay.BeginFrame();
        DrawImportedEnemySpritemap(rom, nativeOverlay, 0xa6, 0xa329, new(100, 80),
            EnemyPaletteBits.Palette2, 0);
        nativeOverlay.FinalizeFrame();
        var installedOverlay = new OamBuffer();
        installedOverlay.BeginFrame();
        enemies.DrawCeresRidleyImmediateBabyAndDoor(installedOverlay, new(0, 0));
        installedOverlay.FinalizeFrame();
        AssertTrue(nativeOverlay.ImageEquals(installedOverlay),
            "Ceres private overlay matches native OAM with ROM frame reads forbidden");

        EnemySpritemapDocument visual = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart first = visual.Frames["ceres_door_ridley_private_overlay"][0];
        visual.Frames["ceres_door_ridley_private_overlay"][0] = first with
        {
            OffsetY = first.OffsetY + 1,
        };
        byte[] editedJson = JsonSerializer.SerializeToUtf8Bytes(visual,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = EnemySpritemapCatalog.Load(
                new MemoryStream(editedJson, writable: false)) });
        var editedOverlay = new OamBuffer();
        editedOverlay.BeginFrame();
        enemies.DrawCeresRidleyImmediateBabyAndDoor(editedOverlay, new(0, 0));
        editedOverlay.FinalizeFrame();
        AssertEqual(nativeOverlay.GetEntry(0).X, editedOverlay.GetEntry(0).X,
            "private-overlay visual edit preserves the native quake/X coordinate");
        AssertEqual(nativeOverlay.GetEntry(0).Y + 1, editedOverlay.GetEntry(0).Y,
            "private-overlay visual edit changes only its authored Y offset");
        AssertEqual((ushort)1, door.VariableB,
            "private-overlay visual edit preserves the door's control flag");

        Console.WriteLine(
            "Ceres door quake definitions: all four native bytes, 65,536 timer aliases, four real OAM phases, and editable private-overlay art pass with source reads forbidden.");
    }

    private static CartridgeReadGuard NewCeresDoorQuakeReadGuard(
        ISnesAddressSpace source, bool blockOverlay = false) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6a321 and < 0xa6a325 ||
                    (blockOverlay && address is >= 0xa6a329 and < 0xa6a353),
                address => $"Ceres private door draw attempted migrated source read ${address:X6}.");
}
