using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class InstalledSamusArtworkTests
{
    private static int CheckBodyTransfers(InstalledSamusArtworkFixture fixture,
        SuperMetroidAddressSpace memory, ArtworkPixelCheck check)
    {
        int count = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int sets = upper ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount;
            var destinations = upper ? SamusRenderingRomData.TileTransfers.TopDestinations :
                SamusRenderingRomData.TileTransfers.BottomDestinations;
            for (int set = 0; set < sets; set++)
            {
                var stock = upper ? fixture.Stock.TopSet(set) : fixture.Stock.BottomSet(set);
                var edited = upper ? fixture.Edited.TopSet(set) : fixture.Edited.BottomSet(set);
                Require(stock.Count == edited.Count, "PNG changed definition count");
                for (int position = 0; position < stock.Count; position++)
                {
                    SamusBodyTileDefinition a = stock[position], b = edited[position];
                    Require(a.SourceAddress == b.SourceAddress && a.FirstSize == b.FirstSize && a.SecondSize == b.SecondSize,
                        "PNG changed transfer identity or split size");
                    var va = new SnesVram(); var vb = new SnesVram();
                    // A pending NMI retains the selected definition pointer. Rebind only
                    // the installed catalog, exactly as a restored debugger state does.
                    var transfer = new SamusTileTransferState();
                    int pointer = fixture.Stock.DefinitionAddress(upper, (byte)set, (byte)position);
                    SetPendingDefinition(transfer, upper, pointer);
                    transfer.BindArtwork(fixture.Stock); transfer.TransferToVram(memory, va);
                    transfer.BindArtwork(fixture.Edited); transfer.TransferToVram(memory, vb);
                    Require(transfer.TopDefinitionAddress == (upper ? pointer : 0) &&
                        transfer.BottomDefinitionAddress == (upper ? 0 : pointer),
                        "Artwork rebind changed a pending DMA pointer");
                    var expected = new SnesVram();
                    expected.LoadBytes(destinations.First * 2, b.Planar.Span[..b.FirstSize]);
                    if (b.SecondSize != 0)
                        expected.LoadBytes(destinations.Second * 2, b.Planar.Span[b.FirstSize..]);
                    Require(expected.Bytes.SequenceEqual(vb.Bytes), "Split DMA changed neighboring VRAM");
                    var oam = TileGallery(destinations.First, a.FirstSize, destinations.Second, a.SecondSize);
                    check.Pair(va, vb, oam, $"{(upper ? "top" : "bottom")}/{set:X2}/{position:X2}");
                    count++;
                }
            }
        }
        return count;
    }

    private static void SetPendingDefinition(SamusTileTransferState transfer, bool upper, int pointer)
    {
        // Construct a faithful saved pending-transfer state without modifying the
        // production visibility of private setters or inventing a gameplay pose.
        typeof(SamusTileTransferState).GetProperty(upper ? nameof(transfer.TopDefinitionAddress) :
            nameof(transfer.BottomDefinitionAddress))!.SetValue(transfer, pointer);
        typeof(SamusTileTransferState).GetProperty(upper ? nameof(transfer.TopTransferEnabled) :
            nameof(transfer.BottomTransferEnabled))!.SetValue(transfer, true);
    }

    private static int CheckQueuedTransfers(InstalledSamusArtworkFixture fixture,
        SuperMetroidAddressSpace memory, ArtworkPixelCheck check)
    {
        int count = 0;
        foreach (ushort source in SamusArmCannonArtworkFormat.TileSourcePointers)
        {
            Check(SamusRenderingRomData.Banks.CharacterData | source,
                SamusRenderingRomData.ArmCannon.TileUploadByteCount,
                SamusRenderingRomData.ArmCannon.TileVramDestination, $"cannon {source:X4}");
            count++;
        }
        foreach (SamusDeathTileSegment segment in SamusSpecialSequenceRomData.Death.TileSegments)
        {
            Check(segment.SourceAddress, SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
                segment.EncodedVramDestination, $"death {segment.SourceAddress:X6}");
            count++;
        }
        return count;

        void Check(int source, ushort length, ushort destination, string name)
        {
            var a = new SnesVram(); var b = new SnesVram();
            var queue = new VramWriteQueue();
            queue.Enqueue(length, source, destination);
            VramWriteEntry pending = queue.Entries.Single();
            queue.DrainTo(a, memory, new SamusQueuedArtwork(fixture.Stock));
            queue.Enqueue(pending.SizeInBytes, pending.SourceAddress, pending.EncodedVramDestination);
            Require(queue.Entries.Single() == pending, $"{name}: replacement changed queued transfer");
            queue.DrainTo(b, memory, new SamusQueuedArtwork(fixture.Edited));
            Require(queue.Entries.Count == 0 && queue.TailInBytes == 0, $"{name}: NMI did not clear queue");
            check.Pair(a, b, TileGallery(destination, length, 0, 0), name);
        }
    }

    private sealed class SamusQueuedArtwork(SamusBodyArtworkCatalog artwork) :
        IVramAssetProvider, IInstalledArtworkTransferSource
    {
        public bool TryResolve(int source, int length, out ReadOnlyMemory<byte> data) =>
            artwork.ArmCannon.TryResolveTile(source, length, out data) ||
            artwork.DeathTiles.TryResolve(source, length, out data);
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidDataException($"Unexpected typed transfer {asset} in Samus upload fixture.");
    }

    private static OamBuffer TileGallery(ushort first, int firstSize, ushort second, int secondSize)
    {
        var oam = new OamBuffer(); oam.BeginFrame();
        int slot = 0;
        Add(first, firstSize); Add(second, secondSize);
        oam.FinalizeFrame(); return oam;
        void Add(ushort start, int bytes)
        {
            for (int tile = 0; tile < bytes / RoomCharacterAtlasFormat.BytesPerTile; tile++, slot++)
            {
                int character = (start - SamusRenderingRomData.TileTransfers.TopDestinations.First) / 16 + tile;
                oam.AddOnScreenSpritePart(new SnesSpritemapXWord(0), 0,
                    SnesObjAttributeWord.Create(character, 4, 3),
                    (ushort)(32 + slot % 8 * 16), (ushort)(32 + slot / 8 * 16));
            }
        }
    }
}
