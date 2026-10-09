using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirm only #1163's ten identified native aliases through the real queue/runtime resolver. No frames or ROM.</summary>
internal static class VramDmaArtworkChecks
{
    /// <summary>Confirms native DMA aliases resolve the exact installed HUD, timer, and Grapple artwork.</summary>
    internal static void Run(string mapDirectory)
    {
        var maps = AreaMapPresentationCatalog.Load(mapDirectory, null);
        using var paletteStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(mapDirectory))!,
            "gameplay-palettes", GameplayBasePaletteFormat.ArtworkFileName));
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var runtime = new SuperMetroidRuntime(memory, initialPaletteArt: GameplayBasePaletteCatalog.Load(paletteStream))
            { MapPresentation = maps, GrappleArtwork = Grapple(1) };
        var pairs = new List<(int Source, int Count, VramAssetId Asset)>
        {
            (HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount, VramAssetId.StandardHudTiles),
            (EscapeTimerTileRomData.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstByteCount, VramAssetId.EscapeTimerFirstTiles),
            (EscapeTimerTileRomData.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondByteCount, VramAssetId.EscapeTimerSecondTiles),
        };
        pairs.AddRange(GrappleTileDefinitions.Transfers.ToArray().Select(transfer =>
            (transfer.SourceAddress, (int)transfer.ByteCount, transfer.Asset)));
        foreach (var pair in pairs)
        {
            var queue = new VramWriteQueue();
            queue.Enqueue((ushort)pair.Count, pair.Source, 0);
            queue.DrainTo(runtime.Vram, memory, runtime);
            ReadOnlySpan<byte> expected = ((IVramAssetProvider)runtime).Resolve(pair.Asset).Span;
            VramDmaContractChecks.Require(expected.Length == pair.Count, "native alias uses the typed length");
            for (int offset = 0; offset < pair.Count; offset++)
                VramDmaContractChecks.Require(runtime.Vram.ReadByte(offset) == expected[offset], "native alias preserves each compiled byte");
            var invalid = new VramWriteQueue();
            invalid.Enqueue((ushort)(pair.Count + 1), pair.Source, 0);
            bool rejected = false;
            try { invalid.DrainTo(runtime.Vram, memory, runtime); }
            catch (InvalidOperationException) { rejected = true; }
            VramDmaContractChecks.Require(rejected, "wrong alias length still fails loudly");
        }
        // Queue on a separate retained owner to prove NMI resolution, not the
        // eager legacy-queue rewriting performed by the Grapple property setter.
        var pending = new VramWriteQueue();
        var first = GrappleTileDefinitions.Transfers[0];
        pending.Enqueue(first.ByteCount, first.SourceAddress, 0);
        runtime.GrappleArtwork = Grapple(2);
        pending.DrainTo(runtime.Vram, memory, runtime);
        VramDmaContractChecks.Require(runtime.Vram.ReadByte(0) == 0 && runtime.Vram.ReadByte(1) == 255,
            "already-pending native record resolves newly rebound PNG pixels");
        Console.WriteLine("Native DMA artwork: ten HUD/timer/Grapple aliases match typed bytes through the actual runtime/RAM-only queue; invalid lengths rejected; pending PNG rebinding passed.");
    }

    /// <summary>Creates a deterministic Grapple tile atlas filled with the specified palette index.</summary>
    private static GrappleTileAtlas Grapple(byte index)
    {
        using var png = new MemoryStream();
        var palette = Enumerable.Range(0, 16).Select(value => new Rgba32((byte)value, 0, 0, 255)).ToArray();
        IndexedPng.Write(png, GrappleTileDefinitions.Width, GrappleTileDefinitions.Height,
            Enumerable.Repeat(index, GrappleTileDefinitions.Width * GrappleTileDefinitions.Height).ToArray(), palette);
        png.Position = 0;
        return GrappleTileAtlas.Load(png);
    }
}
