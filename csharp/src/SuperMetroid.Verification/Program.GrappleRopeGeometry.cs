using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyGrappleRopeGeometry(ISnesAddressSpace bus)
    {
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
                BeamStartX = (ushort)origin, BeamStartY = 128,
                AnchorX = (ushort)(origin + direction * 100), AnchorY = 28,
                PointAnimationTimer = 5,
            };
            for (int slot = 0; slot < 16; slot++)
            {
                grapple.SegmentAnimationTimers[slot] = 1;
                grapple.SegmentAnimationFrames[slot] = (byte)(slot & 3);
            }
            // Equal-magnitude up/right and up/left vectors select $20/$E0.
            int angle = direction > 0 ? 32 : 224;
            int dx = (short)RomDataReader.ReadWordFixedBank(bus, 0xa0b443 + angle * 2) * 2048;
            int dy = (short)RomDataReader.ReadWordFixedBank(bus, 0xa0b3c3 + angle * 2) * 2048;
            int x = unchecked((origin - 4) << 16), y = 124 << 16;
            var expected = new List<(byte X, byte Y)>();
            int visited = 0;
            if ((short)length >= 0)
            {
                int count = Math.Max(1, (length / 8) & 15);
                for (int segment = 0; segment < count; segment++)
                {
                    visited++;
                    ushort sx = (ushort)(x >> 16), sy = (ushort)(y >> 16);
                    if (((sx | sy) & 0xff00) != 0) break;
                    expected.Add(((byte)sx, (byte)sy));
                    x = unchecked(x + dx); y = unchecked(y + dy);
                }
            }
            var oam = new OamBuffer();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, oam, new VramWriteQueue(), 0, 0);
            string context = $"direction {direction}, origin {origin}, length {length}";
            int endpointCount = (short)length < 0 ? 0 : 1;
            AssertEqual((expected.Count + endpointCount) * 4, oam.NextByteOffset, "Native rope culling/count: " + context);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i].X, oam.LowTable[i * 4], "Native fractional rope X: " + context);
                AssertEqual(expected[i].Y, oam.LowTable[i * 4 + 1], "Native fractional rope Y: " + context);
            }
            for (int slot = 0; slot < 16; slot++)
                AssertEqual((ushort)(slot >= 16 - visited ? 5 : 1), grapple.SegmentAnimationTimers[slot], "Native rope ticks only visited slots: " + context);
        }
        Console.WriteLine("Grapple rope geometry: 42 diagonal, viewport-edge, short/wrapped-count and signed-length cases match native positions and visited timers.");
    }
}
