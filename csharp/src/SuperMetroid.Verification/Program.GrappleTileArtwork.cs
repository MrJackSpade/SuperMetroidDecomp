using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static GraphDigestVerification;
using static GrappleFlarePlacementVerification;
using static GrappleSpriteArtworkVerification;
using static GrappleTileBindingVerification;
using static SuiteTimingVerification;

/// <summary>Checks installed Grapple PNG transfers and edited tile pixels against native VRAM output.</summary>
internal static class GrappleTileArtworkVerification
{
    /// <summary>Compares every angle's source selection and all uploaded bytes while preserving rope state and OAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Grapple tile transfers and segment tile pointers")]
    internal static void VerifyGrappleTileArtwork(SuperMetroidAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleSpriteArtwork), () => VerifyGrappleSpriteArtwork(bus));
        Suite(nameof(VerifyGrappleFlarePlacement), () => VerifyGrappleFlarePlacement(bus));
        byte[] png = GrappleTileExtractor.Extract(bus);
        var stock = GrappleTileAtlas.Load(new MemoryStream(png));
        var image = IndexedPng.Read(new MemoryStream(png), new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        for (int tile = 0; tile < 16; tile++) image.Pixels[tile * 8] ^= 1;
        using var changedPng = new MemoryStream();
        IndexedPng.Write(changedPng, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var edited = GrappleTileAtlas.Load(new MemoryStream(changedPng.ToArray()));
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            AssertTrue(stock.Resolve(transfer.Asset).Span.SequenceEqual(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), transfer.SourceAddress, transfer.ByteCount)), "Every extracted Grapple transfer matches pinned native planar bytes");
        var guard = new CartridgeReadGuard(bus)
            .ForbidRange(0x9bc342, 0x9bc3c6 - 0x9bc342, _ => "Grapple still reads tile pointer ROM.")
            .Forbid(address => GrappleTileDefinitions.Transfers.Any(transfer =>
                    address >= transfer.SourceAddress && address < transfer.SourceAddress + transfer.ByteCount),
                _ => "Grapple still reads tile pixel ROM.");
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            ushort expected = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusGrappleRomData.Rendering.SegmentTilePointers + ((angle >> 9) & 254));
            int actual = GrappleTileDefinitions.TransferFor(GrappleTileDefinitions.SegmentAssetFor((ushort)angle)).SourceAddress;
            AssertEqual(0x9a0000 | expected, actual, "Compiled Grapple sector selection matches all 65536 native angles");
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(guard,
                new SamusGrappleState { Phase = GrapplePhase.Firing, Angle = SnesAngle.FromRaw((ushort)angle), PointAnimationTimer = 5 },
                new OamBuffer(), queue, new(0, 0));
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
            SamusGrappleMovement.DrawConnectedBeam(bus, native, oa, a, new(0, 0));
            SamusGrappleMovement.DrawConnectedBeam(guard, selected, ob, b, new(0, 0), stock);
            SamusGrappleMovement.DrawConnectedBeam(guard, changed, oc, c, new(0, 0), edited);
            AssertEqual(a.TailInBytes, b.TailInBytes, "Grapple atlas preserves native queue size");
            for (int i = 0; i < 2; i++)
            {
                AssertEqual(a.Entries[i].SizeInBytes, b.Entries[i].SizeInBytes, "Grapple atlas preserves transfer byte count");
                AssertEqual(a.Entries[i].EncodedVramDestination, b.Entries[i].EncodedVramDestination, "Grapple atlas preserves transfer ordering and destination");
            }
            AssertTrue(oa.ImageEquals(ob) && oa.ImageEquals(oc), "Grapple PNG edits do not change rope OBJ placement or composition");
            AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(selected)) && GraphDigest(native).SequenceEqual(GraphDigest(changed)), "Grapple PNG edits preserve entire rope/animation simulation state");
            var va = new SnesVram(); var vb = new SnesVram(); var vc = new SnesVram();
            va.LoadBytes(0, poison); vb.LoadBytes(0, poison); vc.LoadBytes(0, poison);
            ImportedVramOracle.Drain(a, va, bus); b.DrainTo(vb, ReferenceMutableMemory.From(guard), stock); c.DrainTo(vc, ReferenceMutableMemory.From(guard), edited);
            AssertTrue(va.ImageEquals(vb), "Actual Grapple stock producer/drain preserves all VRAM with pointer/tile ROM forbidden");
            byte[] expectedBytes = va.Bytes.ToArray();
            expectedBytes[GrappleTileDefinitions.PointDestination * 2] ^= 128;
            for (int tile = 0; tile < 4; tile++) expectedBytes[GrappleTileDefinitions.SegmentDestination * 2 + tile * 32] ^= 128;
            AssertTrue(expectedBytes.AsSpan().SequenceEqual(vc.Bytes), "Selected Grapple pixels change exactly five mapped bits and no neighboring VRAM");
            cases++;
            SamusGrappleState Seed() => new()
            {
                Phase = GrapplePhase.ConnectedSwinging, PointAnimationFrame = frame, PointAnimationTimer = timer,
                Angle = SnesAngle.FromRaw((ushort)(sector * 1024 + 1023)), RopeLength = (ushort)(sector % 16 * 8),
                BeamStart = new(100, 100), Anchor = new(200, 60), FlareCounter = 1,
            };
        }
        Suite(nameof(VerifyGrappleTileBinding), () => VerifyGrappleTileBinding(bus, stock, edited));
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, new(8, 8), new byte[64], SnesGraphics.DiagnosticPalette(16)); wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(wrong), "Wrong Grapple PNG dimensions rejected");
        var invalid = new byte[GrappleTileDefinitions.Width * GrappleTileDefinitions.Height]; invalid[0] = 16;
        using var invalidPng = new MemoryStream();
        IndexedPng.Write(invalidPng, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height), invalid, SnesGraphics.DiagnosticPalette(32)); invalidPng.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(invalidPng), "Grapple PNG out-of-range palette index rejected");
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(new MemoryStream(new byte[8])), "Malformed Grapple PNG rejected");
        AssertThrows<InvalidDataException>(() => stock.Resolve(VramAssetId.StandardHudTiles), "Grapple provider rejects unrelated identity");
        Console.WriteLine($"Grapple PNG: all 65536 angle selections and {cases} production uploads preserve native OAM, full state and full VRAM; all sixteen edited tiles map exactly.");
    }

    /// <summary>Runs the focused Grapple tile extraction, binding, and upload check.</summary>
    [VerificationCommand("--grapple-tile-artwork")]
    internal static int GrappleTileArtworkCommand()
    {
        Suite(nameof(VerifyGrappleTileArtwork), () => VerifyGrappleTileArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
