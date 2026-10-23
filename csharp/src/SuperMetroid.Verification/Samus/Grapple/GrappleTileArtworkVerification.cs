using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFlarePlacementVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleSpriteArtworkVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleTileBindingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

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
        IndexedPng.Write(changedPng, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var edited = GrappleTileAtlas.Load(new MemoryStream(changedPng.ToArray()));
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            AssertTrue(stock.Resolve(transfer.Asset).Span.SequenceEqual(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), transfer.SourceAddress, transfer.ByteCount)), "Every extracted Grapple transfer matches pinned native planar bytes");
        var guard = new CartridgeReadGuard(bus)
            .ForbidRange(0x9bc342, 0x9bc3c6 - 0x9bc342, _ => "Grapple still reads tile pointer ROM.")
            .Forbid(address => GrappleTileDefinitions.Transfers.Any(transfer =>
                    address >= transfer.SourceAddress && address < transfer.SourceAddress + transfer.ByteCount),
                _ => "Grapple still reads tile pixel ROM.");
        AssertThrows<InvalidOperationException>(() => SamusGrappleBeamDrawing.DrawConnectedBeam(new SamusGrappleState { Phase = GrapplePhase.Firing, PointAnimationTimer = WrappingCountdown.Start(5) },
                new SpriteFrameBuffer(), new(0, 0)),
            "The grapple beam fails loudly without the installed grapple sheet");
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            ushort expected = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusGrappleRomData.Rendering.SegmentTilePointers + ((angle >> 9) & 254));
            int actual = GrappleTileDefinitions.TransferFor(GrappleTileDefinitions.SegmentAssetFor(SnesAngle.FromRaw((ushort)angle))).SourceAddress;
            AssertEqual(0x9a0000 | expected, actual, "Compiled Grapple sector selection matches all 65536 native angles");
        }
        int cases = 0;
        for (int sector = 0; sector < 64; sector++)
        for (byte frame = 0; frame < 4; frame++)
        foreach (ushort timer in new ushort[] { 0, 1, 5, 65535 })
        {
            SamusGrappleState native = Seed(), selected = Seed();
            OamBuffer oa = SpriteFrameBuffer.DrawnOam(sprites => SamusGrappleBeamDrawing.DrawConnectedBeam(native, sprites, new(0, 0), InstalledFixtures.GrappleSheet));
            OamBuffer ob = SpriteFrameBuffer.DrawnOam(sprites => SamusGrappleBeamDrawing.DrawConnectedBeam(selected, sprites, new(0, 0), InstalledFixtures.GrappleSheet));
            AssertTrue(oa.ImageEquals(ob), "Grapple sheet draws are deterministic in rope OBJ placement and composition");
            AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(selected)), "Grapple draws preserve entire rope/animation simulation state");
            // The binding holds the endpoint frame's character, then the angle sector's four rope characters, exactly
            // as the cartridge uploads them to $6200/$6210.
            SpriteArtwork stockTiles = stock.Tiles(selected.PointAnimationFrame, GrappleTileDefinitions.OrientationFor(selected.Angle));
            byte[] nativeTiles = [.. RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    GrapplePointAnimationDefinitions.SourceAddress(selected.PointAnimationFrame), SpriteArtwork.CharacterByteCount),
                .. RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    GrappleTileDefinitions.TransferFor(GrappleTileDefinitions.SegmentAssetFor(selected.Angle)).SourceAddress,
                    4 * SpriteArtwork.CharacterByteCount)];
            AssertTrue(stockTiles.Characters.SequenceEqual(nativeTiles), "Stock grapple binding holds the native endpoint and rope characters");
            byte[] expectedEdited = stockTiles.Characters.ToArray();
            for (int character = 0; character < 5; character++) expectedEdited[character * SpriteArtwork.CharacterByteCount] ^= 128;
            AssertTrue(edited.Tiles(selected.PointAnimationFrame, GrappleTileDefinitions.OrientationFor(selected.Angle)).Characters.SequenceEqual(expectedEdited),
                "Selected Grapple pixels change exactly five mapped bits of the binding");
            cases++;
            SamusGrappleState Seed() => new()
            {
                Phase = GrapplePhase.ConnectedSwinging, PointAnimationFrame = frame, PointAnimationTimer = WrappingCountdown.FromRaw(timer),
                Angle = SnesAngle.FromRaw((ushort)(sector * 1024 + 1023)), RopeLength = (ushort)(sector % 16 * 8),
                BeamStart = new(100, 100), Anchor = new(200, 60), FlareCounter = 1,
            };
        }
        Suite(nameof(VerifyGrappleTileBinding), VerifyGrappleTileBinding);
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, new(8, 8), new byte[64], SnesGraphics.DiagnosticPalette(16)); wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(wrong), "Wrong Grapple PNG dimensions rejected");
        var invalid = new byte[GrappleTileDefinitions.Width * GrappleTileDefinitions.Height]; invalid[0] = 16;
        using var invalidPng = new MemoryStream();
        IndexedPng.Write(invalidPng, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height), invalid, SnesGraphics.DiagnosticPalette(32)); invalidPng.Position = 0;
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(invalidPng), "Grapple PNG out-of-range palette index rejected");
        AssertThrows<InvalidDataException>(() => GrappleTileAtlas.Load(new MemoryStream(new byte[8])), "Malformed Grapple PNG rejected");
        AssertThrows<InvalidDataException>(() => stock.Resolve(VramAssetId.StandardHudTiles), "Grapple provider rejects unrelated identity");
        Console.WriteLine($"Grapple PNG: all 65536 angle selections and {cases} production draws preserve native OAM and full state; bindings hold native tiles and all edited tiles map exactly.");
    }

    /// <summary>Runs the focused Grapple tile extraction, binding, and upload check.</summary>
    [VerificationCommand("--grapple-tile-artwork")]
    internal static int GrappleTileArtworkCommand()
    {
        Suite(nameof(VerifyGrappleTileArtwork), () => VerifyGrappleTileArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
