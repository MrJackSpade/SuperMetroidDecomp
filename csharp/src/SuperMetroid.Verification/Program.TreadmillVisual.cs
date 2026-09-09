using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyTreadmillVisual()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var rooms = JsonDocument.Parse(File.ReadAllText("standalone-assets/maps/room-placements.json"));
        foreach (var placement in rooms.RootElement.EnumerateArray())
        {
            if (placement.GetProperty("Area").GetString() != "WreckedShip") continue;
            ushort pointer = Convert.ToUInt16(placement.GetProperty("RoomHeader").GetString()![2..], 16);
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(pointer);
            var room = runtime.ActiveRoom!;
            ushort record = RoomFxRomData.SelectRecord(bus, room.State.FxPointer, 0);
            byte bits = record == 0 ? (byte)0 : RoomFxRomData.ReadRecordByte(bus, record,
                RoomFxRomData.Record.AnimatedTileBitsetOffset);
            Console.WriteLine($"Room {room.Identity} header={pointer:X4} FX={room.State.FxPointer:X4} animation bits={bits:X2} door treadmill={runtime.WreckedShipTreadmill.IsActive}");
            if ((bits & 12) == 0) continue;
            var images = new HashSet<string>();
            var renderedFrames = new HashSet<string>();
            var palette = new SnesCgram();
            for (int color = 0; color < 16; color++) palette.SetColor(color, (ushort)(color * 2));
            int[] sources = (bits & 4) != 0
                ? [WreckedShipTreadmillRomData.Frame0Source, WreckedShipTreadmillRomData.Frame1Source,
                    WreckedShipTreadmillRomData.Frame2Source, WreckedShipTreadmillRomData.Frame3Source]
                : [WreckedShipTreadmillRomData.Frame3Source, WreckedShipTreadmillRomData.Frame2Source,
                    WreckedShipTreadmillRomData.Frame1Source, WreckedShipTreadmillRomData.Frame0Source];
            for (int frame = 0; frame < 12; frame++)
            {
                runtime.StepFrame(0);
                images.Add(Convert.ToHexString(runtime.Vram.Bytes.Slice(
                    WreckedShipTreadmillRomData.EncodedVramDestination * 2,
                    WreckedShipTreadmillRomData.TransferByteCount)));
                // Isolate the live floor character in a one-tile viewport. A diagnostic
                // ramp exposes every color index; this tests actual decoded pixels,
                // not merely a changing DMA pointer or unrelated enemy animation.
                var tileView = new SnesVram();
                tileView.LoadBytes(0, runtime.Vram.Bytes);
                tileView.ExecuteWordTransfer([(ushort)(WreckedShipTreadmillRomData.EncodedVramDestination / 16)], 0x7000, 1);
                var pixels = SnesBgTilemapRenderer.Render4BppViewport(tileView, palette, 0x7000, 0, 0, 0, 8, 8);
                renderedFrames.Add(string.Join(',', pixels.Select(pixel => pixel.R)));
                if (frame > 0)
                {
                    var expected = new SnesVram();
                    expected.ExecuteHardwareDmaWrite(bus, sources[(frame - 1) & 3],
                        WreckedShipTreadmillRomData.TransferByteCount,
                        WreckedShipTreadmillRomData.EncodedVramDestination);
                    expected.ExecuteWordTransfer([(ushort)(WreckedShipTreadmillRomData.EncodedVramDestination / 16)], 0x7000, 1);
                    AssertTrue(pixels.SequenceEqual(SnesBgTilemapRenderer.Render4BppViewport(expected,
                        palette, 0x7000, 0, 0, 0, 8, 8)), $"{room.Identity} native direction/cadence pixels frame {frame}");
                }
            }
            AssertEqual(4, images.Count, $"{room.Identity} powered floor uploads all four graphics frames");
            AssertEqual(4, renderedFrames.Count, $"{room.Identity} powered floor renders four distinct character frames");
        }
    }
}
