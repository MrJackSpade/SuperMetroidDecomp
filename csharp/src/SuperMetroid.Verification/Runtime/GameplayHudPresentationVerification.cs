using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Runtime;

/// <summary>Checks installed gameplay HUD art, live tile updates, and editable layout overrides.</summary>
internal static class GameplayHudPresentationVerification
{
    /// <summary>Compares stock HUD tiles with native data and checks override propagation without live ROM reads.</summary>
    internal static void VerifyGameplayHudPresentationAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentation original,
        GameplayBasePalette initialPalettes)
    {
        string stockPath = Path.Combine(stock, GameplayHudDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayHudPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed gameplay HUD JSON is the deterministic cartridge extraction");
        GameplayHudArtwork presentation = GameplayHudPresentationFile.Read(new MemoryStream(extracted));
        ushort[] stockTopRow = BackgroundTileWordBytes.Words(presentation.TopRowTiles());
        for (int column = 0; column < GameplayHudDefinitions.TopRowCellCount; column++)
            AssertEqual((ushort)(GameplayHudPresentationAssetsRomByte(bus, HudSourceDefinitions.TopRow + column * 2)
                    | GameplayHudPresentationAssetsRomByte(bus, HudSourceDefinitions.TopRow + column * 2 + 1) << 8),
                stockTopRow[column], $"stock immutable HUD row word {column}");
        var guard = new MutableCartridgeReadGuard(bus);
        guard.Forbid(address =>
                InHudRange(address, HudSourceDefinitions.TopRow, GameplayHudDefinitions.TopRowCellCount * 2) ||
                InHudRange(address, HudSourceDefinitions.Template, GameplayHudDefinitions.CellCount * 2) ||
                InHudRange(address, HudSourceDefinitions.Icons, 44) ||
                InHudRange(address, HudSourceDefinitions.HealthDigits, 20) ||
                InHudRange(address, HudSourceDefinitions.AmmoDigits, 20) ||
                InHudRange(address, HudSourceDefinitions.AutoReserve, 24),
            address => $"Unexpected gameplay HUD presentation ROM read ${address:X6}.");
        var topRowRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), InstalledFixtures.SamusOverreads, initialPaletteArt: initialPalettes);
        topRowRuntime.Artwork.MapPresentation = original;
        // The HUD draws its images over the BG3 plane's top rows (#1466): its current images, as the installed artwork lays
        // their tiles out, show the installed HUD row.
        GameplayHudPresentation boundHud = topRowRuntime.Hud.Presentation ?? throw new InvalidOperationException("The runtime binds the HUD artwork.");
        AssertTrue(BackgroundTileWordBytes.Words(GameplayHudArtwork.Of(boundHud).Rows(topRowRuntime.Hud.Shown()))[..GameplayHudDefinitions.TopRowCellCount]
            .SequenceEqual(stockTopRow),
            "the HUD's images show the installed immutable HUD row without source reads");
        ushort equipment = ((SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.GrappleBeam).ToEquipmentWord());
        var snapshot = new HudSnapshot(
            Health: 345, MaxHealth: 499,
            Missiles: 123, MaxMissiles: 230,
            SuperMissiles: 45, MaxSuperMissiles: 50,
            PowerBombs: 6, MaxPowerBombs: 10,
            EquippedItems: SamusEquipmentWordCodec.FromEquipmentWord(equipment), SelectedItem: HudItem.SuperMissile,
            ReserveHealth: 50, ReserveMode: ReserveTankMode.Auto);
        var native = new HudState();
        var installed = new HudState();
        native.BindPresentation(presentation);
        installed.BindPresentation(presentation);
        native.Initialize(snapshot);
        installed.Initialize(snapshot);
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed HUD template, icons, tanks, counters, AUTO and highlight match native");

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())
        {
            Health = 287,
            MaxHealth = 499,
            Missiles = 98,
            MaxMissiles = 230,
            SuperMissiles = 32,
            MaxSuperMissiles = 50,
            PowerBombs = 4,
            MaxPowerBombs = 10,
            EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
            SelectedHudItem = HudItem.PowerBomb,
            ReserveEnergy = 25,
            ReserveTankMode = ReserveTankMode.Auto,
        };
        native.UpdateGameplayCounters(samus);
        installed.UpdateGameplayCounters(samus);
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed live HUD updates match native without presentation ROM reads");
        AssertEqual(native.SelectionSoundRequestedThisFrame, installed.SelectionSoundRequestedThisFrame,
            "installed HUD retains selection-sound mechanics");

        var nativeSystem = new SharedGameState();
        var installedSystem = new SharedGameState();
        nativeSystem.SetAreaMapAcquired(AreaId.Crateria);
        installedSystem.SetAreaMapAcquired(AreaId.Crateria);
        native.UpdateMinimap(nativeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(23, 0), RoomSize = new(144, 80), Samus = new(0x0440, 0x04bb), FrameCounter = 0, PresentationMap = original.Get(AreaId.Crateria) });
        installed.UpdateMinimap(installedSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(23, 0), RoomSize = new(144, 80), Samus = new(0x0440, 0x04bb), FrameCounter = 0, PresentationMap = original.Get(AreaId.Crateria) });
        AssertTrue(native.Tiles.SequenceEqual(installed.Tiles),
            "installed minimap anchor matches native and needs no HUD visual ROM data");

        var document = RequiredJson.Deserialize<GameplayHudPresentationDocument>(extracted,
            ArtworkJson.Options);
        document = document with
        {
            Digits = document.Digits with { HealthAnchor = new(4, 2) },
            MinimapAnchor = new(25, 0),
        };
        document.TopRow[0] = document.TopRow[0] with { FlipX =
            !document.TopRow[0].FlipX };
        document.Icons["Missile"].Cells[0] = document.Icons["Missile"].Cells[0] with { FlipX = true };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, GameplayHudDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameplayHudPresentationFile.Write(output, document);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "gameplay HUD override changes selected catalog identity");
        var editedTopRowRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), InstalledFixtures.SamusOverreads, initialPaletteArt: initialPalettes);
        editedTopRowRuntime.Artwork.MapPresentation = edited;
        GameplayHudArtwork editedArtwork = GameplayHudArtwork.Of(edited.GameplayHud);
        AssertTrue(editedArtwork.Rows(editedTopRowRuntime.Hud.Shown())[..GameplayHudDefinitions.TopRowCellCount]
            .SequenceEqual(editedArtwork.TopRowTiles()),
            "edited top-row cell reaches the HUD the NMI latches");
        AssertTrue(!editedArtwork.TopRowTiles().SequenceEqual(GameplayHudArtwork.Of(original.GameplayHud).TopRowTiles()),
            "edited top-row cell changes the shown row");

        var editedHud = new HudState();
        editedHud.BindPresentation(edited.GameplayHud);
        editedHud.Initialize(snapshot);
        var stockHud = new HudState();
        stockHud.BindPresentation(original.GameplayHud);
        stockHud.Initialize(snapshot);
        AssertEqual(stockHud.TileWords[70], editedHud.TileWords[68],
            "edited health anchor moves its first digit through production HUD setup");
        AssertEqual(stockHud.TileWords[71], editedHud.TileWords[69],
            "edited health anchor moves its second digit through production HUD setup");
        AssertTrue(editedHud.TileWords[10] != stockHud.TileWords[10],
            "edited missile cell changes the installed icon");

        var rebind = new HudState();
        rebind.BindPresentation(original.GameplayHud);
        rebind.Initialize(snapshot);
        var rebindSystem = new SharedGameState();
        rebindSystem.SetAreaMapAcquired(AreaId.Crateria);
        rebind.UpdateMinimap(rebindSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(23, 0), RoomSize = new(144, 80), Samus = new(0x0440, 0x04bb), FrameCounter = 8, PresentationMap = original.Get(AreaId.Crateria) });
        ushort[] logicalMap = Enumerable.Range(0, 15)
            .Select(i => rebind.TileWords[(i / 5) * 32 + 26 + i % 5]).ToArray();
        rebind.BindPresentation(edited.GameplayHud, samus);
        for (int i = 0; i < logicalMap.Length; i++)
            AssertEqual(logicalMap[i], rebind.TileWords[(i / 5) * 32 + 25 + i % 5],
                "HUD content rebind carries logical minimap cells to edited anchor");

        File.WriteAllText(replacement, "{ broken gameplay HUD JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt gameplay HUD override fails loudly");
        using (var output = File.Create(replacement))
            GameplayHudPresentationFile.Write(output, document);

        AssertThrows<InvalidDataException>(() => GameplayHudPresentationFile.Write(Stream.Null,
            document with { SelectedPalette = 8 }), "HUD rejects invalid highlight palette");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentationFile.Write(Stream.Null,
            document with { Template = document.Template[..^1] }), "HUD rejects incomplete template");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentationFile.Write(Stream.Null,
            document with { TopRow = document.TopRow[..^1] }), "HUD rejects incomplete immutable row");
        var overlap = RequiredJson.Deserialize<GameplayHudPresentationDocument>(extracted,
            ArtworkJson.Options);
        overlap = overlap with { Digits = overlap.Digits with { HealthAnchor = new(8, 0) } };
        AssertThrows<InvalidDataException>(() => GameplayHudPresentationFile.Write(Stream.Null, overlap),
            "HUD rejects overlapping visual owners");
        var missingIcon = RequiredJson.Deserialize<GameplayHudPresentationDocument>(extracted,
            ArtworkJson.Options);
        missingIcon.Icons.Remove("XRay");
        AssertThrows<InvalidDataException>(() => GameplayHudPresentationFile.Write(Stream.Null, missingIcon),
            "HUD rejects missing named icons");

        Console.WriteLine("Gameplay HUD presentation: exact stock/live parity, ROM guard, layout/art edits, rebind and strict failures pass.");
    }

    /// <summary>Reads a native HUD byte as the independent stock-art oracle.</summary>
    private static byte GameplayHudPresentationAssetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks whether a bus address lies within one migrated HUD presentation range.</summary>
    private static bool InHudRange(int address, int start, int length) =>
        (uint)(address - start) < (uint)length;
}
