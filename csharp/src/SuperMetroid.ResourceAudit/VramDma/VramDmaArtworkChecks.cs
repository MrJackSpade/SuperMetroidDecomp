using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Confirm only #1163's ten identified native aliases through the real queue/runtime resolver. No frames or ROM.</summary>
internal static class VramDmaArtworkChecks
{
    /// <summary>Checks ten native DMA aliases through the runtime queue, including length rejection and pending Grapple art rebinding.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void Run(string mapDirectory)
    {
        var maps = AreaMapPresentationCatalog.Load(mapDirectory, null);
        using var paletteStream = File.OpenRead(Path.Combine((Path.GetDirectoryName(Path.GetFullPath(mapDirectory)) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(Path.GetFullPath(mapDirectory)).")),
            "gameplay-palettes", GameplayBasePaletteFormat.ArtworkFileName));
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: GameplayBasePaletteCatalog.Load(paletteStream));
        runtime.Artwork.MapPresentation = maps;
        runtime.Artwork.GrappleArtwork = Grapple(1);
        var pairs = new List<ArtworkTransfer>
        {
            new ArtworkTransfer(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount, VramAssetId.StandardHudTiles),
            new ArtworkTransfer(EscapeTimerTileRomData.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstByteCount, VramAssetId.EscapeTimerFirstTiles),
            new ArtworkTransfer(EscapeTimerTileRomData.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondByteCount, VramAssetId.EscapeTimerSecondTiles),
        };
        pairs.AddRange(GrappleTileDefinitions.Transfers.Select(transfer =>
            new ArtworkTransfer(transfer.SourceAddress, transfer.ByteCount, transfer.Asset)));
        foreach (var pair in pairs)
        {
            var queue = new VramWriteQueue();
            queue.Enqueue((ushort)pair.Count, pair.Source, VramDmaDestination.FromNative(0));
            queue.DrainTo(runtime.Vram, runtime.Artwork);
            ReadOnlySpan<byte> expected = ((IVramAssetProvider)runtime.Artwork).Resolve(pair.Asset).Span;
            VramDmaContractChecks.Require(expected.Length == pair.Count, "native alias uses the typed length");
            for (int offset = 0; offset < pair.Count; offset++)
                VramDmaContractChecks.Require(VramBytes.At(runtime.Vram, offset) == expected[offset], "native alias preserves each compiled byte");
            var invalid = new VramWriteQueue();
            invalid.Enqueue((ushort)(pair.Count + 1), pair.Source, VramDmaDestination.FromNative(0));
            bool rejected = false;
            try { invalid.DrainTo(runtime.Vram, runtime.Artwork); }
            catch (InvalidOperationException) { rejected = true; }
            VramDmaContractChecks.Require(rejected, "wrong alias length still fails loudly");
        }
        // Queue on a separate retained owner to prove NMI resolution, not the
        // eager legacy-queue rewriting performed by the Grapple property setter.
        var pending = new VramWriteQueue();
        var first = GrappleTileDefinitions.Transfers[0];
        pending.Enqueue(checked((ushort)first.ByteCount), first.SourceAddress, VramDmaDestination.FromNative(0));
        runtime.Artwork.GrappleArtwork = Grapple(2);
        pending.DrainTo(runtime.Vram, runtime.Artwork);
        VramDmaContractChecks.Require(VramBytes.At(runtime.Vram, 0) == 0 && VramBytes.At(runtime.Vram, 1) == 255,
            "already-pending native record resolves newly rebound PNG pixels");
        Console.WriteLine("Native DMA artwork: ten HUD/timer/Grapple aliases match typed bytes through the actual runtime/RAM-only queue; invalid lengths rejected; pending PNG rebinding passed.");
    }

    /// <summary>Builds a uniform indexed Grapple atlas to distinguish queued artwork before and after rebinding.</summary>
    private static GrappleTileAtlas Grapple(byte index)
    {
        using var png = new MemoryStream();
        var palette = Enumerable.Range(0, 16).Select(value => new Rgba32((byte)value, 0, 0, 255)).ToArray();
        IndexedPng.Write(png, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height),
            Enumerable.Repeat(index, GrappleTileDefinitions.Width * GrappleTileDefinitions.Height).ToArray(), palette);
        png.Position = 0;
        return GrappleTileAtlas.Load(png);
    }
}
