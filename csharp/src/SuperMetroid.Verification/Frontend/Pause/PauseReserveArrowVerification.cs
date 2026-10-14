using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowRebindVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks reserve-tank arrow colors, visibility, and native tilemap palette writes.</summary>
internal static class PauseReserveArrowVerification
{
    /// <summary>Compares thirty-two AUTO color phases and MANUAL selection transitions with rendered arrow pixels.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause reserve arrow spritemap")]
    internal static void VerifyPauseReserveArrow()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 20, MaxHealth = 99, ReserveEnergy = 2,
            MaxReserveEnergy = 100, ReserveTankMode = 1, CollectedBeams = (ushort)SamusBeamFlags.Charge };
        var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(pause);
        pause.Step(0, (ushort)SnesButton.Up);
        Rgba32[]? first = null;
        for (byte frame = 0; frame < 32; frame++)
        {
            pause.Step(0, 0, nmiFrameCounter8: frame);
            CheckArrow(true, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82ad5d + frame * 2),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82ad9d + frame * 2), $"AUTO phase {frame}");
            if (frame == 0) first = pause.Render();
            if (frame == 15)
            {
                var pixels = pause.Render();
                AssertTrue(Enumerable.Range(32, 64).Any(y => Enumerable.Range(8, 8).Any(x => pixels[y * 256 + x] != (first ?? throw new InvalidOperationException("Verification requires first."))[y * 256 + x])),
                    "arrow itself visibly changes color, not only a palette entry");

            }
        }
        pause.Step(0, (ushort)SnesButton.A);
        CheckArrow(false, 0x0156, 0x039e, "MANUAL mode disables arrow");
        pause.Step(0, (ushort)SnesButton.Down);
        CheckArrow(true, 0x0156, 0x039e, "manual transfer selection enables solid arrow");
        pause.Step(0, (ushort)SnesButton.A);
        CheckArrow(true, 0x0156, 0x039e, "refill keeps solid arrow");
        pause.Step(0, 0);
        CheckArrow(false, 0x0156, 0x039e, "refill completion disables arrow");
        pause.Step(0, (ushort)SnesButton.A, nmiFrameCounter8: 7);
        pause.Step(0, (ushort)SnesButton.Down, nmiFrameCounter8: 8);
        CheckArrow(false, 0x0156, 0x039e, "leaving tanks disables arrow on same call");
        Console.WriteLine("Reserve arrow: all 32 AUTO colors, visible pixels, MANUAL selection/refill/completion and exit agree.");

        void CheckArrow(bool enabled, ushort color6, ushort colorB, string context)
        {
            var capture = pause.CaptureRenderSnapshot();
            var tiles = capture.Memory.Vram.ToArray();
            var colors = capture.Memory.Cgram.ToArray();
            // Independent native AE01/AE46 walk: eight vertical words followed
            // by two horizontal words. Only the three palette bits may change.
            foreach (int offset in Enumerable.Range(0, 8).Select(i => 0x102 + i * 0x40).Concat(new[] { 0x302, 0x304 }))
            {
                int address = 0x6000 + offset;
                ushort before = BinaryPrimitives.ReadUInt16LittleEndian(tiles.AsSpan(address));
                ushort expected = (ushort)((before & 0xe3ff) | (enabled ? 0x1800 : 0x1c00));
                BinaryPrimitives.WriteUInt16LittleEndian(tiles.AsSpan(address), expected);
            }
            colors[102] = Bgr555.FromWord(color6);
            colors[107] = Bgr555.FromWord(colorB);
            var memory = new PpuMemorySnapshot(tiles, colors, capture.Memory.Oam, capture.Memory.ModeledSpriteCount);
            var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(memory, capture.Layers, capture.ObjectSelection, capture.Brightness));
            AssertTrue(pause.Render().SequenceEqual(expectedPixels), context + " rendered arrow matches native tile/palette writes");
        }
    }

    /// <summary>Runs reserve-arrow visual and rebind-latch checks.</summary>
    [VerificationCommand("--pause-reserve-arrow")]
    internal static int PauseReserveArrowCommand()
    {
        Suite(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow());
        Suite(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch());
        return 0;
    }
}
