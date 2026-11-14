using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks the pause characters against the cartridge, the menu's page changes and its restored pixels.</summary>
internal static class PauseTileAtlasVerification
{
    /// <summary>
    /// Compares the stock pause characters with the cartridge's, and in three areas drives the menu from the map to the
    /// equipment page and back and confirms the restored equipment page keeps its pixels.
    /// </summary>
    internal static void VerifyPauseTileAtlas(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var vram = new SnesVram(); InstalledFixtures.ImportOf(original).Characters.Pause.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(PauseTileAtlasFormatConstants.DestinationByte / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        AssertTrue(vram.Bytes.Slice(PauseTileAtlasFormatConstants.DestinationByte, PauseTileAtlasFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.PauseInterfaceCharacters, PauseTileAtlasFormat.ByteCount)), "pause UI PNG round-trips all native character bytes");
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Maridia, AreaId.Tourian })
        {
            var system = new SharedGameState(new SharedRandom()); system.SetAreaMapAcquired(area);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge };
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, Area = area, RoomMapPosition = new(10, 10), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            int toMap = 2 + PausePageTransitionNativeLength + 1;
            for (int tick = 0; tick < toMap + PausePageTransitionNativeLength + 2; tick++)
            {
                ControllerButton pressed = tick == 2 ? ControllerButton.R : tick == toMap ? ControllerButton.L : 0;
                pause.Step(pressed, pressed);
            }
            AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "pause PNG fixture returns to the map page");
            EnterPauseEquipment(pause);
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "pause PNG fixture reaches equipment page");
            LayeredScene before = pause.CaptureRenderSnapshot();
            // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
            pause = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(pause);
            pause.BindFixtureArtwork(original);
            AssertTrue(ScenePixels.Same(before, pause.CaptureRenderSnapshot()), "restored pause keeps its pixels and animation phase");
        }
        Console.WriteLine("Pause UI atlas: exact bytes, three-area map/equipment page changes and capture/restore pass.");
    }

}
