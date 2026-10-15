using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Checks pixel parity and output-buffer reuse across gameplay and cinematic snapshot rendering.</summary>
internal static class GameplayRenderAllocationVerification
{
    /// <summary>Measures allocations for fused gameplay, X-ray, and cinematic packets after warmup while checking exact pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    public static int Run()
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var sprites = new SpriteFrameBuffer();
        cgram.SetColor(0, Bgr555.FromWord(0x1234));
        var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32), 0, 0, 0, 0,
            SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
        var snapshot = new LayeredRenderSnapshot(PpuMemorySnapshot.Capture(vram, cgram, sprites),
            [new OrdinaryGameplayRenderLayer(registers)], 3, 15);
        var expected = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(vram, cgram, sprites,
            new OrdinaryGameplayRenderLayer(registers));
        for (int i = 0; i < 10; i++) SoftwareLayeredSnapshotRenderer.Render(snapshot);
        long before = GC.GetAllocatedBytesForCurrentThread();
        var actual = SoftwareLayeredSnapshotRenderer.Render(snapshot);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!actual.SequenceEqual(expected)) throw new InvalidDataException("Fused gameplay pixels differ from direct renderer.");
        Console.WriteLine($"Gameplay packet allocation: {allocated} bytes; pixel parity passed.");
        // Output plus detached PPU memory fit comfortably below this budget. A second
        // native RGBA backdrop (229376 bytes), discarded by the fused pass, does not.
        if (allocated >= 450000) throw new InvalidDataException("Gameplay packet allocates a redundant full-size backdrop.");
        var reusable = new SuperMetroid.Core.Hardware.Rgba32[expected.Length];
        before = GC.GetAllocatedBytesForCurrentThread();
        var reused = SoftwareLayeredSnapshotRenderer.Render(snapshot, reusable);
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!ReferenceEquals(reusable, reused) || !reused.SequenceEqual(expected))
            throw new InvalidDataException("Reusable gameplay output changed ownership or pixels.");
        if (allocated >= 100000) throw new InvalidDataException("Reusable gameplay rendering still allocates a full pixel buffer.");
        Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
        if (!SoftwareLayeredSnapshotRenderer.Render(snapshot, reusable).SequenceEqual(expected))
            throw new InvalidDataException("Reusable output retained previous frame pixels.");
        Console.WriteLine($"Reusable gameplay packet allocation: {allocated} bytes; identity and dirty-buffer parity passed.");
        var memory = PpuMemorySnapshot.Capture(vram, cgram, sprites);
        var xray = new LayeredRenderSnapshot(memory,
            [new GameplayColorMathRenderLayer(new OrdinaryGameplayRenderLayer(registers), Enumerable.Repeat(new XrayWindowLine(40, 160), 224).ToArray(), true, new ColorMathRegisters { ColorMath = SnesColorMathControl.Bg1 | SnesColorMathControl.Backdrop | SnesColorMathControl.Half, AddSubscreen = false, FixedRed = 3, FixedGreen = 7, FixedBlue = 11 })], 3, 15);
        var xrayReference = SoftwareLayeredSnapshotRenderer.Render(xray);
        SoftwareLayeredSnapshotRenderer.Render(xray, reusable); // Warm pooled scratch.
        Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
        before = GC.GetAllocatedBytesForCurrentThread();
        var xrayBorrowed = SoftwareLayeredSnapshotRenderer.Render(xray, reusable);
        long xrayBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Console.WriteLine($"X-ray reused packet: {xrayBytes} bytes.");
        if (!ReferenceEquals(xrayBorrowed, reusable) || !xrayBorrowed.SequenceEqual(xrayReference))
            throw new InvalidDataException("X-ray ignores output reuse or retains dirty pixels.");
        if (xrayBytes >= 100000) throw new InvalidDataException("X-ray allocates redundant full-screen scratch or duplicate PPU memory.");
        var identity = new RenderFrameIdentity(1, 1, 0);
        RenderFrameSnapshot[] packets = [
            new(identity, new Mode7ObjRenderSnapshot(memory, new Mode7RenderRegisters(256, 0, 0, 256, new Offset16<Mode7Pixels>(0, 0), new Offset16<Mode7Pixels>(0, 0)), 3, 15)),
            new(identity, new Mode7ObjRenderSnapshot(memory, null, 3, 9,
                Enumerable.Repeat(new SuperMetroid.Core.Rendering.TitleGradientLine(1, 2, 3, 0xa1), 224).ToArray()), new byte[] { 11 }),
            new(identity, new LayeredRenderSnapshot(memory, [], 3, 15)),
            new(identity, new SuperMetroid.Core.Hardware.Rgba32(90, 130, 210), new byte[] { 7 })
        ];
        foreach (var packet in packets)
        {
            var reference = SoftwareFrameSnapshotRenderer.Render(packet);
            Array.Fill(reusable, new SuperMetroid.Core.Hardware.Rgba32(255, 0, 255));
            before = GC.GetAllocatedBytesForCurrentThread();
            SoftwareFrameSnapshotRenderer.Render(packet);
            long freshBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            before = GC.GetAllocatedBytesForCurrentThread();
            var borrowed = SoftwareFrameSnapshotRenderer.Render(packet, reusable);
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
