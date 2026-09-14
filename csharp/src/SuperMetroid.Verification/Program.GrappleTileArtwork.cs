using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyGrappleTileArtwork(SuperMetroidAddressSpace bus)
    {
        VerifyGrappleSpriteArtwork(bus);
        byte[] png = GrappleTileExtractor.Extract(bus);
        var stock = GrappleTileAtlas.Load(new MemoryStream(png));
        var image = IndexedPng.Read(new MemoryStream(png), GrappleTileDefinitions.Width, GrappleTileDefinitions.Height);
        for (int tile = 0; tile < 16; tile++) image.Pixels[tile * 8] ^= 1;
        using var changedPng = new MemoryStream();
        IndexedPng.Write(changedPng, image.Width, image.Height, image.Pixels, image.Palette);
        var edited = GrappleTileAtlas.Load(new MemoryStream(changedPng.ToArray()));
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            AssertTrue(stock.Resolve(transfer.Asset).Span.SequenceEqual(RomDataReader.ReadFixedBank(bus, transfer.SourceAddress, transfer.ByteCount)), "Every extracted Grapple transfer matches pinned native planar bytes");
        var guard = new GrappleTileReadGuard(bus);
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            ushort expected = RomDataReader.ReadWordFixedBank(bus, SamusGrappleRomData.Rendering.SegmentTilePointers + ((angle >> 9) & 254));
            int actual = GrappleTileDefinitions.TransferFor(GrappleTileDefinitions.SegmentAssetFor((ushort)angle)).SourceAddress;
            AssertEqual(0x9a0000 | expected, actual, "Compiled Grapple sector selection matches all 65536 native angles");
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(guard,
                new SamusGrappleState { Phase = GrapplePhase.Firing, Angle = SnesAngle.FromRaw((ushort)angle), PointAnimationTimer = 5 },
                new OamBuffer(), queue, 0, 0);
            AssertEqual(0x9a0000 | expected, queue.Entries[1].SourceAddress, "No-artwork producer uses native sector mapping without pointer ROM");
            AssertEqual((ushort)128, queue.Entries[1].SizeInBytes, "No-artwork producer preserves segment transfer size");
            AssertEqual((ushort)0x6210, queue.Entries[1].EncodedVramDestination, "No-artwork producer preserves segment transfer destination");
        }
        byte[] poison = new byte[65536]; Array.Fill(poison, (byte)0xa5);
        int cases = 0;
        for (int sector = 0; sector < 64; sector++)
        for (byte frame = 0; frame < 4; frame++)
        foreach (ushort timer in new ushort[] { 0, 1, 5, 65535 })
        {
            SamusGrappleState native = Seed(), selected = Seed(), changed = Seed();
            var a = new VramWriteQueue(); var b = new VramWriteQueue(); var c = new VramWriteQueue();
            var oa = new OamBuffer(); var ob = new OamBuffer(); var oc = new OamBuffer();
            SamusGrappleMovement.DrawConnectedBeam(bus, native, oa, a, 0, 0);
            SamusGrappleMovement.DrawConnectedBeam(guard, selected, ob, b, 0, 0, stock);
            SamusGrappleMovement.DrawConnectedBeam(guard, changed, oc, c, 0, 0, edited);
            AssertEqual(a.TailInBytes, b.TailInBytes, "Grapple atlas preserves native queue size");
            for (int i = 0; i < 2; i++)
            {
                AssertEqual(a.Entries[i].SizeInBytes, b.Entries[i].SizeInBytes, "Grapple atlas preserves transfer byte count");
                AssertEqual(a.Entries[i].EncodedVramDestination, b.Entries[i].EncodedVramDestination, "Grapple atlas preserves transfer ordering and destination");
            }
            AssertTrue(oa.LowTable.SequenceEqual(ob.LowTable) && oa.HighTable.SequenceEqual(ob.HighTable) && oa.LowTable.SequenceEqual(oc.LowTable) && oa.HighTable.SequenceEqual(oc.HighTable), "Grapple PNG edits do not change rope OBJ placement or composition");
            AssertTrue(SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(selected)) && SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(changed)), "Grapple PNG edits preserve entire rope/animation simulation state");
            var va = new SnesVram(); var vb = new SnesVram(); var vc = new SnesVram();
            va.LoadBytes(0, poison); vb.LoadBytes(0, poison); vc.LoadBytes(0, poison);
            a.DrainTo(va, bus); b.DrainTo(vb, guard, stock); c.DrainTo(vc, guard, edited);
            AssertTrue(va.Bytes.SequenceEqual(vb.Bytes), "Actual Grapple stock producer/drain preserves all VRAM with pointer/tile ROM forbidden");
            byte[] expectedBytes = va.Bytes.ToArray();
            expectedBytes[GrappleTileDefinitions.PointDestination * 2] ^= 128;
            for (int tile = 0; tile < 4; tile++) expectedBytes[GrappleTileDefinitions.SegmentDestination * 2 + tile * 32] ^= 128;
            AssertTrue(expectedBytes.AsSpan().SequenceEqual(vc.Bytes), "Selected Grapple pixels change exactly five mapped bits and no neighboring VRAM");
            cases++;
            SamusGrappleState Seed() => new()
            {
                Phase = GrapplePhase.ConnectedSwinging, PointAnimationFrame = frame, PointAnimationTimer = timer,
                Angle = SnesAngle.FromRaw((ushort)(sector * 1024 + 1023)), RopeLength = (ushort)(sector % 16 * 8),
                BeamStartX = 100, BeamStartY = 100, AnchorX = 200, AnchorY = 60, FlareCounter = 1,
            };
        }
        VerifyGrappleTileBinding(bus, stock, edited);
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, 8, 8, new byte[64], SnesGraphics.DiagnosticPalette(16)); wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(wrong), "Wrong Grapple PNG dimensions rejected");
        var invalid = new byte[GrappleTileDefinitions.Width * GrappleTileDefinitions.Height]; invalid[0] = 16;
        using var invalidPng = new MemoryStream();
        IndexedPng.Write(invalidPng, GrappleTileDefinitions.Width, GrappleTileDefinitions.Height, invalid, SnesGraphics.DiagnosticPalette(32)); invalidPng.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(invalidPng), "Grapple PNG out-of-range palette index rejected");
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(new MemoryStream(new byte[8])), "Malformed Grapple PNG rejected");
        AssertThrows<InvalidDataException>(() => stock.Resolve(VramAssetId.StandardHudTiles), "Grapple provider rejects unrelated identity");
        Console.WriteLine($"Grapple PNG: all 65536 angle selections and {cases} production uploads preserve native OAM, full state and full VRAM; all sixteen edited tiles map exactly.");
    }

    private sealed class GrappleTileReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x9bc342 and < 0x9bc3c6) throw new InvalidOperationException("Grapple still reads tile pointer ROM.");
            foreach (var transfer in GrappleTileDefinitions.Transfers)
                if (address >= transfer.SourceAddress && address < transfer.SourceAddress + transfer.ByteCount)
                    throw new InvalidOperationException("Grapple still reads tile pixel ROM.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
