using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Buffers.Binary;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks rendered pause reserve labels against native tilemap copy rules.</summary>
internal static class PauseReserveLabelsVerification
{
    /// <summary>Compares six capacity and mode combinations using native source and destination words.</summary>
    internal static void VerifyPauseReserveLabels()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort capacity in new ushort[] { 0, 100 })
        foreach (ushort mode in new ushort[] { 0, 1, 2 })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { MaxReserveEnergy = capacity, ReserveTankMode = SuperMetroid.Diagnostics.Saves.SaveSlotNumbering.SavedReserveMode(mode),
                CollectedItems = SamusEquipmentFlags.MorphBall };
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = RetailPresentationFixture(), SpriteSheets = MenuSheetFixtures.For(RetailPresentationFixture()) });
            EnterPauseEquipment(pause);
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "reserve label test reached equipment page");
            var snapshot = pause.CaptureRenderSnapshot();
            byte[] snapshotVram = pause.ShownVram(RetailPresentationFixture());
            byte[] expectedVram = snapshotVram.ToArray();
            // Reset the label regions from the cartridge template first. Otherwise
            // the no-capacity case would compare the renderer against its own output.
            for (int row = 0; row < 2; row++)
            {
                int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c068 + row * 2) - 0x3800;
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.EquipmentPage + destination, 14)
                    .CopyTo(expectedVram, 0x6000 + destination);
            }
            if (capacity != 0)
            {
                // Native A12B copies two seven-word labels, using C068/C088 tables.
                for (int row = 0; row < 2; row++)
                {
                    int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c068 + row * 2) - 0x3800;
                    int source = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c088 + row * 2);
                    RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source, 14).CopyTo(expectedVram, 0x6000 + destination);
                }
                // AB47 preserves attributes while inserting four mode tiles at word 327.
                if (mode != 0)
                    for (int tile = 0; tile < 4; tile++)
                    {
                        int offset = 0x6000 + (327 + tile) * 2;
                        ushort before = BinaryPrimitives.ReadUInt16LittleEndian(expectedVram.AsSpan(offset));
                        ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), (mode == 1 ? 0x82bf2a : 0x82bf22) + tile * 2);
                        BinaryPrimitives.WriteUInt16LittleEndian(expectedVram.AsSpan(offset), (ushort)((before & 0xfc00) | source));
                    }
            }
            // The page's characters match the native label writes, and every picture the page shows in its standard
            // shading resolves to the cartridge's pause colours (#1512): the pause screen holds no colours of its own.
            AssertTrue(expectedVram.AsSpan(0x6000, 0x800).SequenceEqual(snapshotVram.AsSpan(0x6000, 0x800)),
                $"reserve label characters match native ownership/mode ({capacity}/{mode})");
            var nativeColours = new SnesCgram();
            CartridgePaletteImporter.LoadToCgram(nativeColours, bus, MapSourceDefinitions.Artwork.PauseColors);
            foreach (PlacedPlanePicture placed in snapshot.Layers.ToArray().OfType<PicturedPlaneLayer>().SelectMany(layer => layer.Plane.Pictures)
                .Where(placed => placed.Shading == PictureShading.Standard))
                AssertTrue(SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ShowsNativeColours(placed.Picture.Sheet,
                        PictureShading.Standard.KeyColours(placed.Picture.Sheet), nativeColours.Colors),
                    $"pause picture {placed.Picture.Sheet.Name} shows the cartridge's pause colours ({capacity}/{mode})");
            var expectedPixels = DisplayRendering.Render(snapshot.WithNativeColours(PauseShownColours.Of(pause, RetailPresentationFixture())));
            var actualPixels = pause.Render();

            AssertTrue(actualPixels.SequenceEqual(expectedPixels),
                $"visible reserve labels match native ownership/mode ({capacity}/{mode})");
        }
        Console.WriteLine("Pause reserve labels: six ownership/mode cases match native rendered labels.");
    }

    /// <summary>Runs the focused reserve-label pixel comparison.</summary>
    [VerificationCommand("--pause-reserve-labels")]
    internal static int PauseReserveLabelsCommand()
    {
        Suite(nameof(VerifyPauseReserveLabels), () => VerifyPauseReserveLabels());
        return 0;
    }
}
