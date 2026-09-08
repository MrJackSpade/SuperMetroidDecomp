using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static class GameplayRenderAllocationVerification
{
    public static int Run()
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var oam = new OamBuffer();
        cgram.SetColor(0, 0x1234);
        var registers = new OrdinaryGameplayRegisters(0, 0, 0, 0, 64, 32, 0, 0, 0, 0,
            SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
        var snapshot = new LayeredRenderSnapshot(PpuMemorySnapshot.Capture(vram, cgram, oam),
            [new OrdinaryGameplayRenderLayer(registers)], 3, 15);
        var expected = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(vram, cgram, oam, 0, 0, 0, 0,
            bg2TilemapWidthInTiles: 64, bg2TilemapHeightInTiles: 32, bg2TilemapBaseWord: 0,
            bg1CharacterBaseWord: 0, bg2CharacterBaseWord: 0, bg3CharacterBaseWord: 0);
        for (int i = 0; i < 10; i++) SoftwareLayeredSnapshotRenderer.Render(snapshot);
        long before = GC.GetAllocatedBytesForCurrentThread();
        var actual = SoftwareLayeredSnapshotRenderer.Render(snapshot);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!actual.AsSpan().SequenceEqual(expected)) throw new InvalidDataException("Fused gameplay pixels differ from direct renderer.");
        Console.WriteLine($"Gameplay packet allocation: {allocated} bytes; pixel parity passed.");
        // Output plus detached PPU memory fit comfortably below this budget. A second
        // native RGBA backdrop (229376 bytes), discarded by the fused pass, does not.
        if (allocated >= 450000) throw new InvalidDataException("Gameplay packet allocates a redundant full-size backdrop.");
        var reusable = new SuperMetroid.Core.Assets.Rgba32[expected.Length];
        before = GC.GetAllocatedBytesForCurrentThread();
        var reused = SoftwareLayeredSnapshotRenderer.Render(snapshot, reusable);
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!ReferenceEquals(reusable, reused) || !reused.AsSpan().SequenceEqual(expected))
            throw new InvalidDataException("Reusable gameplay output changed ownership or pixels.");
        if (allocated >= 100000) throw new InvalidDataException("Reusable gameplay rendering still allocates a full pixel buffer.");
        Array.Fill(reusable, new SuperMetroid.Core.Assets.Rgba32(255, 0, 255));
        if (!SoftwareLayeredSnapshotRenderer.Render(snapshot, reusable).AsSpan().SequenceEqual(expected))
            throw new InvalidDataException("Reusable output retained previous frame pixels.");
        Console.WriteLine($"Reusable gameplay packet allocation: {allocated} bytes; identity and dirty-buffer parity passed.");
        return 0;
    }
}
