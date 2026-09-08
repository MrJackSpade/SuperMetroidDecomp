using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZebesDoesNotWrapDuringDescent()
    {
        VerifyOffScreenCinematicOam();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scene = new CeresDestructionCinematicState(bus);
        int previousBottom = FrontendFrame.Height;
        int visibleFrames = 0, slideFrames = 0;
        for (int frame = 0; frame < 2500 && !scene.Finished; frame++)
        {
            scene.Step();
            if (scene.Phase != CeresDestructionPhase.SlideZebesSceneAway) continue;
            var pixels = scene.Render();
            int bottom = -1;
            for (int y = 0; y < FrontendFrame.Height; y++)
            {
                int planetPixels = 0;
                for (int x = 0; x < FrontendFrame.Width; x++)
                {
                    var p = pixels[y * FrontendFrame.Width + x];
                    if (p.R > p.G && p.G > 30 && p.B < p.G / 2) planetPixels++;
                }
                // A row of planet gold, rather than isolated stars. Keeping the
                // bottom edge monotonic detects wrapping even while another part
                // of the planet remains visible at the top of the same frame.
                if (planetPixels >= 8) bottom = y;
            }
            AssertTrue(bottom <= previousBottom,
                $"Zebes slide frame {slideFrames}: planet bottom moved down {previousBottom}->{bottom}");
            previousBottom = bottom;
            if (bottom >= 0) visibleFrames++;
            slideFrames++;
        }
        AssertTrue(scene.Finished && visibleFrames > 10 && previousBottom == -1,
            "Zebes descends with one visible upward pass and fully exits before handoff");
        Console.WriteLine($"  Zebes descent: {slideFrames} frames, {visibleFrames} with planet, no lower-screen reappearance.");
    }

    private static void VerifyOffScreenCinematicOam()
    {
        var bus = new TestAddressSpace();
        // One small, zero-X-offset component. Exhaust the ADC operands rather than
        // testing only the negative origins present in one planet animation.
        WriteTestWord(bus, 0x8c8000, 1);
        WriteTestWord(bus, 0x8c8002, 0);
        WriteTestWord(bus, 0x8c8005, 0);
        var oam = new OamBuffer();
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        {
            bus.WriteByte(0x8c8004, (byte)offset);
            oam.BeginFrame();
            oam.AddOffScreenSpritemap(bus, 0x8c8000, 5, (ushort)origin, 0);
            int sum = origin + offset;
            // Direct branch transcription of $81:8853, independent of the shared
            // production helper's inversion of the on-screen predicate.
            bool parked = offset < 128 ? sum < 224
                : (sum & 256) != 0 ? (byte)sum < 224 : (byte)sum >= 224;
            AssertEqual(parked ? (byte)128 : (byte)5, oam.LowTable[0], "off-screen X parking");
            AssertEqual(parked ? (byte)224 : (byte)sum, oam.LowTable[1], "off-screen Y parking");
            AssertEqual(parked ? 1 : 0, oam.HighTable[0] & 1, "off-screen X high bit");
        }
        Console.WriteLine("  Cinematic off-screen OAM: all 65,536 origin/offset byte pairs match native branches.");
    }
}
