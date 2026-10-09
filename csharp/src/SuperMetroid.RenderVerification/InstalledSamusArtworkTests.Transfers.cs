using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class InstalledSamusArtworkTests
{
    /// <summary>Compares stock and edited body-artwork uploads for every top and bottom tile set.</summary>
    /// <param name="fixture">Installed stock and edited artwork catalogs used by the comparison.</param>
    /// <param name="memory">Cartridge address space supplying bytes for the queued tile transfers.</param>
    /// <param name="check">Compares the rendered VRAM and OAM results for each transfer pair.</param>
    /// <returns>The number of body tile definitions checked.</returns>
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

    /// <summary>Builds a pending top or bottom transfer state as if restored from a saved debugger state.</summary>
    /// <param name="transfer">Transfer state whose selected definition and enable flag are set.</param>
    /// <param name="upper"><see langword="true"/> to set the top transfer; otherwise sets the bottom transfer.</param>
    /// <param name="pointer">Definition address retained by the pending transfer.</param>
    private static void SetPendingDefinition(SamusTileTransferState transfer, bool upper, int pointer)
    {
        // Construct a faithful saved pending-transfer state without modifying the
        // production visibility of private setters or inventing a gameplay pose.
        typeof(SamusTileTransferState).GetProperty(upper ? nameof(transfer.TopDefinitionAddress) :
            nameof(transfer.BottomDefinitionAddress))!.SetValue(transfer, pointer);
        typeof(SamusTileTransferState).GetProperty(upper ? nameof(transfer.TopTransferEnabled) :
            nameof(transfer.BottomTransferEnabled))!.SetValue(transfer, true);
    }

    /// <summary>Checks that queued cannon and death-sequence uploads use the rebound installed artwork.</summary>
    /// <param name="fixture">Stock and edited artwork catalogs used as the two transfer sources.</param>
    /// <param name="memory">Cartridge address space used to perform each queued transfer.</param>
    /// <param name="check">Compares the resulting VRAM images and tile-gallery sprites.</param>
    /// <returns>The number of queued source transfers checked.</returns>
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

    /// <summary>Adapts installed Samus artwork catalogs to the byte-source interfaces used by queued uploads.</summary>
    /// <param name="artwork">Catalog supplying cannon and death-tile bytes for queued DMA requests.</param>
    private sealed class SamusQueuedArtwork(SamusBodyArtworkCatalog artwork) :
        IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Resolves a raw upload range from the cannon or death-tile catalogs.</summary>
        /// <param name="source">Cartridge source address requested by the queued transfer.</param>
        /// <param name="length">Number of bytes requested from that source.</param>
        /// <param name="data">Receives the matching artwork bytes when either catalog contains the range.</param>
        /// <returns><see langword="true"/> when an installed artwork catalog supplies the requested range.</returns>
        public bool TryResolve(int source, int length, out ReadOnlyMemory<byte> data) =>
            artwork.ArmCannon.TryResolveTile(source, length, out data) ||
            artwork.DeathTiles.TryResolve(source, length, out data);

        /// <summary>Rejects typed asset requests because this fixture models raw cartridge-addressed transfers.</summary>
        /// <param name="asset">Typed asset identifier that is unsupported by this test adapter.</param>
        /// <exception cref="InvalidDataException">A typed transfer was unexpectedly routed through the raw-source fixture.</exception>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidDataException($"Unexpected typed transfer {asset} in Samus upload fixture.");
    }

    /// <summary>Builds an OAM gallery covering the tile ranges represented by one or two VRAM uploads.</summary>
    /// <param name="first">First upload's starting VRAM character destination.</param>
    /// <param name="firstSize">First upload length in bytes.</param>
    /// <param name="second">Second upload's starting VRAM character destination.</param>
    /// <param name="secondSize">Second upload length in bytes; zero omits the second range.</param>
    /// <returns>An OAM buffer that places each uploaded tile in a visible grid.</returns>
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
