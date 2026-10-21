using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks Wrecked Ship treadmill DMA frames through rendered floor-character pixels.</summary>
internal static class TreadmillVisualVerification
{
    /// <summary>Compares four cycling graphics frames with native transfer sources in powered treadmill rooms.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyTreadmillVisual()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var rooms = JsonDocument.Parse(File.ReadAllText("standalone-assets/maps/room-placements.json"));
        foreach (var placement in rooms.RootElement.EnumerateArray())
        {
            if (placement.GetProperty("Area").GetString() != "WreckedShip") continue;
            ushort pointer = Convert.ToUInt16((placement.GetProperty("RoomHeader").GetString() ?? throw new InvalidOperationException("Verification requires placement.GetProperty(\"RoomHeader\").GetString()."))[2..], 16);
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(pointer));
            var room = runtime.Room.LoadedRoom;
            ushort record = RoomFxDefinitions.SelectedRecordPointer(room.State.Fx, null);
            byte bits = record == 0 ? (byte)0 : RoomFxDefinitions.RecordAt(record).AnimatedTileBitset;
            Console.WriteLine($"Room {room.Identity} header={pointer:X4} FX={room.State.FxPointer:X4} animation bits={bits:X2} door treadmill={runtime.WreckedShipTreadmill.IsActive}");
            if ((bits & 12) == 0) continue;
            var images = new HashSet<string>();
            var renderedFrames = new HashSet<string>();
            var palette = new SnesCgram();
            for (int color = 0; color < 16; color++) palette.SetColor(color, Bgr555.FromWord((ushort)(color * 2)));
            int[] sources = (bits & 4) != 0
                ? [WreckedShipTreadmillRomData.Frame0Source, WreckedShipTreadmillRomData.FrameSource(1),
                    WreckedShipTreadmillRomData.FrameSource(2), WreckedShipTreadmillRomData.FrameSource(3)]
                : [WreckedShipTreadmillRomData.FrameSource(3), WreckedShipTreadmillRomData.FrameSource(2),
                    WreckedShipTreadmillRomData.FrameSource(1), WreckedShipTreadmillRomData.Frame0Source];
            for (int frame = 0; frame < 12; frame++)
            {
                runtime.StepFrame(0);
                // Animated background characters bind as artwork over VRAM (#1350); a capture shows the composition.
                byte[] shown = BgArtworkFixtures.Composed(runtime.Vram, runtime.BgArtwork);
                images.Add(Convert.ToHexString(shown.AsSpan(
                    WreckedShipTreadmillRomData.EncodedVramDestination * 2,
                    WreckedShipTreadmillRomData.TransferByteCount)));
                // Isolate the live floor character in a one-tile viewport. A diagnostic
                // ramp exposes every color index; this tests actual decoded pixels,
                // not merely a changing DMA pointer or unrelated enemy animation.
                var tileView = new SnesVram();
                tileView.LoadBytes(0, shown);
                tileView.ExecuteWordTransfer([(WreckedShipTreadmillRomData.EncodedVramDestination / 16)], 0x7000, 1);
                var pixels = SnesBgTilemapRenderer.Render4BppViewport(tileView, palette, new BgVramAddresses { TilemapBaseWord = 0x7000, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = new Extent16<ScreenPixels>(8, 8) });
                renderedFrames.Add(string.Join(',', pixels.Select(pixel => pixel.R)));
                if (frame > 0)
                {
                    int matched = Array.FindIndex(sources, source => pixels.SequenceEqual(NativeFloorPixels(bus, palette, source)));
                    AssertTrue(pixels.SequenceEqual(NativeFloorPixels(bus, palette, sources[(frame - 1) & 3])),
                        $"{room.Identity} native direction/cadence pixels frame {frame}: expected native frame {(frame - 1) & 3}, " +
                        (matched < 0 ? "shown tile matches no native frame" : $"shown tile is native frame {matched}"));
                }
            }
            AssertEqual(4, images.Count, $"{room.Identity} powered floor uploads all four graphics frames");
            AssertEqual(4, renderedFrames.Count, $"{room.Identity} powered floor renders four distinct character frames");
        }
    }

    /// <summary>Renders the floor character as the native transfer of <paramref name="source"/> leaves it.</summary>
    [PpuLayoutBoundary("exact hardware test: renders a native VRAM transfer")]
    private static Rgba32[] NativeFloorPixels(ISnesAddressSpace bus, SnesCgram palette, int source)
    {
        var expected = new SnesVram();
        ImportedVramOracle.ExecuteHardware(expected, bus, source,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        expected.ExecuteWordTransfer([(WreckedShipTreadmillRomData.EncodedVramDestination / 16)], 0x7000, 1);
        return SnesBgTilemapRenderer.Render4BppViewport(expected, palette, new BgVramAddresses { TilemapBaseWord = 0x7000, CharacterBaseWord = 0 },
            new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = new Extent16<ScreenPixels>(8, 8) });
    }

    /// <summary>Runs the focused Wrecked Ship treadmill visual comparison.</summary>
    [VerificationCommand("--treadmill-visual")]
    internal static int TreadmillVisualCommand()
    {
        Suite(nameof(VerifyTreadmillVisual), () => VerifyTreadmillVisual());
        return 0;
    }
}
