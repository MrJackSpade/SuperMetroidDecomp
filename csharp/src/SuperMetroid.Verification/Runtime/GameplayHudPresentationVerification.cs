using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Runtime;

/// <summary>Checks installed gameplay HUD art and live tile updates.</summary>
internal static class GameplayHudPresentationVerification
{
    /// <summary>Compares stock HUD tiles with native data without live ROM reads.</summary>
    internal static void VerifyGameplayHudPresentationAssets(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayHudPresentationExtractor.Extract(bus);
        GameplayHudArtwork presentation = GameplayHudPresentationFile.Read(new MemoryStream(extracted));
        ushort[] stockTopRow = BackgroundTileWordBytes.Words(presentation.TopRowTiles());
        for (int column = 0; column < GameplayHudDocumentDefinitions.TopRowCellCount; column++)
            AssertEqual((ushort)(GameplayHudPresentationAssetsRomByte(bus, HudSourceDefinitions.TopRow + column * 2)
                    | GameplayHudPresentationAssetsRomByte(bus, HudSourceDefinitions.TopRow + column * 2 + 1) << 8),
                stockTopRow[column], $"stock immutable HUD row word {column}");
        var guard = new MutableCartridgeReadGuard(bus);
        guard.Forbid(address =>
                InHudRange(address, HudSourceDefinitions.TopRow, GameplayHudDocumentDefinitions.TopRowCellCount * 2) ||
                InHudRange(address, HudSourceDefinitions.Template, GameplayHudDocumentDefinitions.CellCount * 2) ||
                InHudRange(address, HudSourceDefinitions.Icons, 44) ||
                InHudRange(address, HudSourceDefinitions.HealthDigits, 20) ||
                InHudRange(address, HudSourceDefinitions.AmmoDigits, 20) ||
                InHudRange(address, HudSourceDefinitions.AutoReserve, 24),
            address => $"Unexpected gameplay HUD presentation ROM read ${address:X6}.");
        var topRowRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(SuperMetroid.Core.Gameplay.GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        topRowRuntime.Artwork.MapPresentation = original;
        // The HUD draws its images over the BG3 plane's top rows (#1466): its current images, as the installed artwork lays
        // their tiles out, show the installed HUD row.
        _ = topRowRuntime.Hud.Presentation ?? throw new InvalidOperationException("The runtime binds the HUD layout.");
        AssertTrue(BackgroundTileWordBytes.Words(topRowRuntime.InstalledHudArtwork.Rows(topRowRuntime.Hud.Shown()))[..GameplayHudDocumentDefinitions.TopRowCellCount]
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
        native.BindPresentation(presentation.Presentation);
        installed.BindPresentation(presentation.Presentation);
        native.Initialize(snapshot);
        installed.Initialize(snapshot);
        AssertTrue(native.Tiles(presentation).SequenceEqual(installed.Tiles(presentation)),
            "installed HUD template, icons, tanks, counters, AUTO and highlight match native");

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
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
        AssertTrue(native.Tiles(presentation).SequenceEqual(installed.Tiles(presentation)),
            "installed live HUD updates match native without presentation ROM reads");
        AssertEqual(native.SelectionSoundRequestedThisFrame, installed.SelectionSoundRequestedThisFrame,
            "installed HUD retains selection-sound mechanics");

        var nativeSystem = new SharedGameState(new SharedRandom());
        var installedSystem = new SharedGameState(new SharedRandom());
        nativeSystem.SetAreaMapAcquired(AreaId.Crateria);
        installedSystem.SetAreaMapAcquired(AreaId.Crateria);
        native.UpdateMinimap(nativeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(23, 0), RoomSize = new(144, 80), Samus = new(0x0440, 0x04bb), FrameCounter = 0, PresentationMap = original.Get(AreaId.Crateria) });
        installed.UpdateMinimap(installedSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(23, 0), RoomSize = new(144, 80), Samus = new(0x0440, 0x04bb), FrameCounter = 0, PresentationMap = original.Get(AreaId.Crateria) });
        AssertTrue(native.Tiles(presentation).SequenceEqual(installed.Tiles(presentation)),
            "installed minimap anchor matches native and needs no HUD visual ROM data");

        Console.WriteLine("Gameplay HUD presentation: exact stock/live parity and ROM guard pass.");
    }

    /// <summary>Reads a native HUD byte as the independent stock-art oracle.</summary>
    private static byte GameplayHudPresentationAssetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks whether a bus address lies within one migrated HUD presentation range.</summary>
    private static bool InHudRange(int address, int start, int length) =>
        (uint)(address - start) < (uint)length;
}
