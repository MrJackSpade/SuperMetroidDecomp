using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Checks pixel parity and output-buffer reuse across gameplay and cinematic snapshot rendering.</summary>
internal static class GameplayRenderAllocationVerification
{
    /// <summary>Measures allocations for fused gameplay, X-ray, and cinematic packets after warmup while checking exact pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    public static int Run()
    {
        var cgram = new SnesCgram();
        var sprites = new SpriteFrameBuffer();
        cgram.SetColor(0, Bgr555.FromWord(0x1234));
        var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
            ScreenLayers.Bg1 | ScreenLayers.Bg2 | ScreenLayers.Sprites);
        var rooms = RoomBackgroundsFixtures.Seeded(new Random(1346), registers.Bg1Scroll.AsBgPlaneScroll(),
            registers.Bg2Scroll.AsBgPlaneScroll(), door: true, overlay: true);
        var snapshot = new LayeredScene(FramePaintConstants.Capture(cgram, sprites),
            [new OrdinaryGameplayRenderLayer(registers, rooms)], 15);
        var expected = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(cgram, sprites,
            new OrdinaryGameplayRenderLayer(registers, rooms));
        for (int i = 0; i < 10; i++) DisplayRendering.Render(snapshot);
        long before = GC.GetAllocatedBytesForCurrentThread();
        var actual = DisplayRendering.Render(snapshot);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!actual.SequenceEqual(expected)) throw new InvalidDataException("Fused gameplay pixels differ from direct renderer.");
        Console.WriteLine($"Gameplay packet allocation: {allocated} bytes; pixel parity passed.");
        // Output plus detached PPU memory fit comfortably below this budget. A second
        // native RGBA backdrop (229376 bytes), discarded by the fused pass, does not.
        if (allocated >= 450000) throw new InvalidDataException("Gameplay packet allocates a redundant full-size backdrop.");
        var reusable = new SuperMetroid.Core.Hardware.Rgba32[expected.Length];
        before = GC.GetAllocatedBytesForCurrentThread();
        var reused = DisplayRendering.Render(snapshot, reusable);
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!ReferenceEquals(reusable, reused) || !reused.SequenceEqual(expected))
            throw new InvalidDataException("Reusable gameplay output changed ownership or pixels.");
        if (allocated >= 100000) throw new InvalidDataException("Reusable gameplay rendering still allocates a full pixel buffer.");
        Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
        if (!DisplayRendering.Render(snapshot, reusable).SequenceEqual(expected))
            throw new InvalidDataException("Reusable output retained previous frame pixels.");
        Console.WriteLine($"Reusable gameplay packet allocation: {allocated} bytes; identity and dirty-buffer parity passed.");
        var memory = FramePaintConstants.Capture(cgram, sprites);
        var xray = new LayeredScene(memory,
            [new GameplayColorMathRenderLayer(new OrdinaryGameplayRenderLayer(registers, rooms), Enumerable.Repeat(new XrayWindowLine(40, 160), 224).ToArray(), true, new SceneBlend { Layers = ColorBlend.Bg1 | ColorBlend.Backdrop | ColorBlend.Half, BlendsWithSubscreen = false, FixedColor = new Bgr555(3, 7, 11) })], 15);
        var xrayReference = DisplayRendering.Render(xray);
        DisplayRendering.Render(xray, reusable); // Warm pooled scratch.
        Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
        before = GC.GetAllocatedBytesForCurrentThread();
        var xrayBorrowed = DisplayRendering.Render(xray, reusable);
        long xrayBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Console.WriteLine($"X-ray reused packet: {xrayBytes} bytes.");
        if (!ReferenceEquals(xrayBorrowed, reusable) || !xrayBorrowed.SequenceEqual(xrayReference))
            throw new InvalidDataException("X-ray ignores output reuse or retains dirty pixels.");
        if (xrayBytes >= 100000) throw new InvalidDataException("X-ray allocates redundant full-screen scratch or duplicate PPU memory.");
        var identity = new RenderFrameIdentity(1, 1, 0);
        DisplayPacket[] packets = [
            new(identity, new FloorScene(memory, new AffineTransform(256, 0, 0, 256, new Offset16<Mode7Pixels>(0, 0), new Offset16<Mode7Pixels>(0, 0)), 15)),
            new(identity, new FloorScene(memory, null, 9,
                Enumerable.Repeat(new SuperMetroid.Core.Rendering.TitleGradientLine(1, 2, 3, ColorBlend.Subtract | ColorBlend.Backdrop | ColorBlend.Bg1), 224).ToArray()), [new FadeLevel(11)]),
            new(identity, new LayeredScene(memory, [], 15)),
            new(identity, new SuperMetroid.Core.Hardware.Rgba32(90, 130, 210), [new FadeLevel(7)])
        ];
        foreach (var packet in packets)
        {
            var reference = DisplayRendering.Render(packet);
            Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
            before = GC.GetAllocatedBytesForCurrentThread();
            DisplayRendering.Render(packet);
            long freshBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            before = GC.GetAllocatedBytesForCurrentThread();
            var borrowed = DisplayRendering.Render(packet, reusable);
            long reuseBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            if (!ReferenceEquals(borrowed, reusable) || !borrowed.SequenceEqual(reference))
                throw new InvalidDataException("Cinematic packet failed dirty-output reuse or pixel parity.");
            if (freshBytes - reuseBytes < reusable.Length * 4)
                throw new InvalidDataException("Cinematic packet did not eliminate its output allocation.");
            Console.WriteLine($"Cinematic output reuse: {freshBytes} -> {reuseBytes} allocated bytes; exact pixels.");
            if (packet.Mode7?.Background is not null && reuseBytes >= 400000)
                throw new InvalidDataException("Mode 7 packet allocates an unnecessary intermediate background plane.");
            if (packet.Mode7 is not null && reuseBytes >= 100000)
                throw new InvalidDataException("Mode 7 packet allocates unnecessary OBJ scratch planes.");
        }
        return 0;
    }
}
