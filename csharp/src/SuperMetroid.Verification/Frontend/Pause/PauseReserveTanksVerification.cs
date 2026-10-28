using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares reserve-tank sprites on the pause equipment screen with retail spritemaps.</summary>
internal static class PauseReserveTanksVerification
{
    /// <summary>Checks tank capacity, fill, and flicker combinations at the rendered pixel level, including save restoration.</summary>
    internal static void VerifyPauseReserveTanks()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int cases = 0;
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var samus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = ReserveTankMode.Manual,
                CollectedBeams = SamusBeamFlags.Charge };
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
            EnterPauseEquipment(pause);
            foreach (ushort supply in new ushort[] { 0, 1, 13, 14, 15, 41, 42, 55, 56, 98, 99, 100, 101, 199, 200, 299, 300, 399, 400 }.Where(n => n <= capacity))
            foreach (byte phase in new byte[] { 0, 4 })
            {
                samus.ReserveEnergy = supply;
                pause.Step(0, 0, frameCounterByte: phase);
                var actual = pause.Render();
                var capture = pause.CaptureRenderSnapshot();
                byte[] captureVram = pause.ShownVram();
                var native = new OamBuffer();
                native.BeginFrame();
                int tank = 0;
                for (; tank < supply / 100; tank++) Draw(0x1b, tank);
                if (supply % 100 != 0)
                {
                    int q = supply % 100 / 14;
                    // Native compares DOUBLED quotient with seven, not quotient.
                    if (q * 2 < 7 && supply % 100 % 14 != 0 && (phase & 4) == 0) q++;
                    int table = 0x82b3d9 + (supply >= 100 ? 16 : 0);
                    Draw(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + q * 2), tank++);
                }
                for (; tank < capacity / 100; tank++) Draw(0x20, tank);
                if (capacity != 0) Draw(0x1f, tank);
                native.FinalizeFrame();
                byte[] objects = native.LowTable.ConcatToArray(native.HighTable);
                var memory = FramePaint.FromOam(captureVram, capture.Paint.Colors, objects, native.LastFinalizedSpriteCount, PauseMenuLayout.ObjectSelection);
                var expected = DisplayRendering.Render(new LayeredScene(memory, capture.Layers, capture.Brightness));
                // This rectangle encloses the tank strip and end-cap, but not the
                // independently animated equipment selector. BG remains identical.
                for (int y = 88; y < 120; y++)
                for (int x = 16; x < 72; x++)
                    AssertEqual(expected[y * 256 + x], actual[y * 256 + x], $"reserve tank pixels {capacity}/{supply}/phase{phase} at {x},{y}");
                AssertTrue(actual.SequenceEqual(pause.Render()), "rendering tank strip does not advance fill flicker");
                if (capacity == 100 && supply == 1 && phase == 4)
                {
                    using var saved = new MemoryStream();
                    DebuggerObjectGraphSerializer.Serialize(saved, pause);
                    saved.Position = 0;
                    var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(saved);
                    restored.BindFixtureArtwork(RetailPresentationFixture());
                    AssertTrue(actual.SequenceEqual(restored.Render()), "saved pause preserves nonzero native tank-flicker phase");
                }
                if (capacity == 400 && supply == 199 && phase == 0)
                {

                }
                cases++;

                // Draw one tank segment from the retail spritemap table at its native strip position.
                void Draw(ushort id, int index)
                {
                    int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c569 + id * 2);
                    ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c1d6 + index * 2);
                    ushort y = (ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82c1e2) - 1);
                    DrawImportedSpritemap(bus, native, pointer, new(x, y), 0x0600);
                }
            }
        }
        Console.WriteLine($"Reserve tank strip: {cases} capacity/fill/flicker cases match native rendered sprites.");
    }

    /// <summary>Runs the reserve-tank strip rendering verification.</summary>
    [VerificationCommand("--pause-reserve-tanks")]
    internal static int PauseReserveTanksCommand()
    {
        Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
        return 0;
    }
}
