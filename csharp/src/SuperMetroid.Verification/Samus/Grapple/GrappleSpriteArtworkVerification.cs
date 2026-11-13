using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Grapple endpoint and rope OBJ attributes as installed art independent of mechanics.</summary>
internal static class GrappleSpriteArtworkVerification
{
    /// <summary>Compares native appearances with the composed grapple sheet frames and rejects malformed documents.</summary>
    internal static void VerifyGrappleSpriteArtwork(CartridgeImportAddressSpace bus)
    {
        byte[] json = GrappleSpriteExtractor.Extract(bus);
        var catalog = InstalledGrappleSprites.Load(new MemoryStream(json));
        // The cartridge's attribute words are decoded by the import codec into the pieces they describe; the extracted
        // appearances hold the same character, palette row, priority and flips.
        AssertEqual(new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.GrappleEndpointAttributes.Value)).ToSinglePart(),
            catalog.Endpoint, "Extracted endpoint attributes match native immediate");
        for (int i = 0; i < 4; i++)
            AssertEqual(new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleSpriteExtractor.SegmentAttributeAddress(i))).ToSinglePart(),
                catalog.Segment(i), "Extracted segment attributes match timed native record");
        // The installed grapple sheet composes each frame from the installed appearances, which match the cartridge above:
        // the endpoint as one small OBJ centred on its origin, and each rope segment likewise, with the rope direction's flips ORed into its own.
        SpriteSheetSources composed = GrappleSheetComposer.Sources(SamusSheetInputs.Load(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.Directory));
        Dictionary<string, SpriteFrame> frames = composed.Frames.ToDictionary(frame => frame.Name, frame => frame.Frame, StringComparer.Ordinal);
        int cases = 0;
        for (int point = 0; point < GrappleSheetComposer.EndpointFrameCount; point++, cases++)
            AssertEqual(SpritePartFrames.CentredSmallObject(catalog.Endpoint, 0).Parts.Single(),
                frames[GrappleSheets.EndpointFrame(point)].Parts.Single(), $"Grapple endpoint frame {point} is the native endpoint OBJ");
        foreach (GrappleRopeOrientation orientation in Enum.GetValues<GrappleRopeOrientation>())
        for (int segment = 0; segment < GrappleSpriteDefinitions.SegmentFrameCount; segment++)
        foreach (SpriteMirror mirror in GrappleSheetComposer.RopeMirrors)
        {
            TileFlip flips = ((mirror & SpriteMirror.Horizontal) != 0 ? TileFlip.Horizontal : TileFlip.None) |
                ((mirror & SpriteMirror.Vertical) != 0 ? TileFlip.Vertical : TileFlip.None);
            SpritePart appearance = catalog.Segment(segment);
            AssertEqual(SpritePartFrames.CentredSmallObject(appearance with { Flip = appearance.Flip | flips }, 0).Parts.Single(),
                frames[GrappleSheets.RopeFrame(orientation, segment, mirror)].Parts.Single(),
                $"Grapple rope {orientation} frame {segment} mirrored {mirror} is the native segment OBJ with the rope's flips");
            cases++;
        }
        foreach (string bad in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1}", System.Text.Encoding.UTF8.GetString(json).Replace("\"palette\": 5", "\"palette\": 8") })
            AssertThrows<InvalidDataException>(() => InstalledGrappleSprites.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(bad))), "Invalid Grapple presentation rejected");
        var missing = RequiredJson.Parse(json); missing.Required("segments").AsArray().RemoveAt(0);
        AssertThrows<InvalidDataException>(() => InstalledGrappleSprites.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(missing.ToJsonString()))), "Missing segment appearance rejected");
        var mechanic = RequiredJson.Parse(json); mechanic.Required("segments").Required(0)["duration"] = 1;
        AssertThrows<InvalidDataException>(() => InstalledGrappleSprites.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mechanic.ToJsonString()))), "Timing field rejected");
        AssertThrows<InvalidDataException>(() => catalog.Segment(4), "Out-of-range visual selector rejected");
        foreach (var (field, value) in new VerifyGrappleSpriteArtworkCase[] { new VerifyGrappleSpriteArtworkCase("tileColumn", -1), new VerifyGrappleSpriteArtworkCase("tileColumn", 16), new VerifyGrappleSpriteArtworkCase("tileRow", -1), new VerifyGrappleSpriteArtworkCase("tileRow", 32), new VerifyGrappleSpriteArtworkCase("palette", -1), new VerifyGrappleSpriteArtworkCase("priority", 4) })
        {
            var invalid = RequiredJson.Parse(json); invalid.Required("endpoint")[field] = value;
            AssertThrows<InvalidDataException>(() => InstalledGrappleSprites.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))), "Invalid Grapple style field: " + field);
        }
        var incomplete = RequiredJson.Parse(json); incomplete.Required("endpoint").AsObject().Remove("priority");
        AssertThrows<InvalidDataException>(() => InstalledGrappleSprites.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(incomplete.ToJsonString()))), "Missing visual field rejected");
        Console.WriteLine($"Grapple sprite JSON: native appearances extracted and {cases} grapple sheet frames composed from them; invalid documents rejected.");
    }

    /// <summary>Runs the Grapple sprite-artwork verification command with the retail cartridge.</summary>
    [VerificationCommand("--grapple-sprite-artwork")]
    internal static int GrappleSpriteArtworkCommand()
    {
        Suite(nameof(VerifyGrappleSpriteArtwork), () => VerifyGrappleSpriteArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
