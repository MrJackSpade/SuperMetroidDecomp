using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks rendered pause reserve labels against native tilemap copy rules.</summary>
internal static class PauseReserveLabelsVerification
{
    /// <summary>Compares six capacity and mode combinations using native source and destination words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause reserve label tilemaps")]
    internal static void VerifyPauseReserveLabels()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort capacity in new ushort[] { 0, 100 })
        foreach (ushort mode in new ushort[] { 0, 1, 2 })
        {
            var samus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = mode,
                CollectedItems = (ushort)SamusEquipmentFlags.MorphBall };
            var pause = new PauseMenuState(bus, new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = RetailPresentationFixture() });
            EnterPauseEquipment(pause);
            AssertEqual(1, pause.ScreenMode, "reserve label test reached equipment page");
            var snapshot = pause.CaptureRenderSnapshot();
            byte[] expectedVram = snapshot.Memory.Vram.ToArray();
            // Reset the label regions from the cartridge template first. Otherwise
            // the no-capacity case would compare the renderer against its own output.
            for (int row = 0; row < 2; row++)
            {
                int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c068 + row * 2) - 0x3800;
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuRomData.EquipmentTilemap + destination, 14)
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
            var expectedMemory = new PpuMemorySnapshot(expectedVram, snapshot.Memory.Cgram,
                snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
            var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(expectedMemory, snapshot.Layers,
                snapshot.ObjectSelection, snapshot.Brightness));
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
