using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static GrappleEndpointGeometryVerification;
using static SuiteTimingVerification;

internal static class GrappleRopeGeometryVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple rope angle tables")]
    internal static void VerifyGrappleRopeGeometry(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleEndpointGeometry), () => VerifyGrappleEndpointGeometry(bus));
        // Exercise the actual drawing entry point, with valid initial instruction
        // timers. Expected positions emulate $94:AFD5-B091's overlapping-word
        // accumulator, using cartridge samples independently of compiled tables.
        foreach (int direction in new[] { 1, -1 })
        foreach (int origin in new[] { 128, 4, 252 })
        foreach (ushort length in new ushort[] { 120, 1, 7, 8, 16, 128, 32768 })
        {
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing, RopeLength = length,
                BeamStart = new((ushort)origin, 128),
                Anchor = new((ushort)(origin + direction * 100), 28),
                PointAnimationTimer = NativeCountdown.Start(5),
            };
            for (int slot = 0; slot < 16; slot++)
            {
                grapple.SegmentAnimationTimers[slot] = NativeCountdown.Start(1);
                grapple.SegmentAnimationFrames[slot] = (byte)(slot & 3);
            }
            // Equal-magnitude up/right and up/left vectors select $20/$E0.
            int angle = direction > 0 ? 32 : 224;
            int dx = (short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa0b443 + angle * 2) * 2048;
            int dy = (short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa0b3c3 + angle * 2) * 2048;
            int x = unchecked(SnesSixteenSixteen.FromWhole(origin - 4)), y = 124 << 16;
            var expected = new List<(byte X, byte Y)>();
            int visited = 0;
            if ((short)length >= 0)
            {
                int count = Math.Max(1, (length / 8) & 15);
                for (int segment = 0; segment < count; segment++)
                {
                    visited++;
                    ushort sx = SnesFixedPosition.WholeOf(x), sy = SnesFixedPosition.WholeOf(y);
                    if (SnesLittleEndian.HighByte(unchecked((ushort)(sx | sy))) != 0) break;
                    expected.Add(((byte)sx, (byte)sy));
                    x = unchecked(x + dx); y = unchecked(y + dy);
                }
            }
            var oam = new OamBuffer();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, oam, new VramWriteQueue(), new(0, 0));
            string context = $"direction {direction}, origin {origin}, length {length}";
            int endpointCount = (short)length < 0 ? 0 : 1;
            AssertEqual((expected.Count + endpointCount) * 4, oam.NextByteOffset, "Native rope culling/count: " + context);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i].X, oam.LowTable[i * 4], "Native fractional rope X: " + context);
                AssertEqual(expected[i].Y, oam.LowTable[i * 4 + 1], "Native fractional rope Y: " + context);
            }
            for (int slot = 0; slot < 16; slot++)
                AssertEqual((ushort)(slot >= 16 - visited ? 5 : 1), grapple.SegmentAnimationTimers[slot].RawValue, "Native rope ticks only visited slots: " + context);
        }
        Console.WriteLine("Grapple rope geometry: 42 diagonal, viewport-edge, short/wrapped-count and signed-length cases match native positions and visited timers.");
    }

    [VerificationCommand("--grapple-rope-geometry")]
    internal static int GrappleRopeGeometryCommand()
    {
        Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
