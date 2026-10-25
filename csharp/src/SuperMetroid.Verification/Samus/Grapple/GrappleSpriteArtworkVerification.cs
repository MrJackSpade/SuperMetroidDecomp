using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Grapple endpoint and rope OBJ attributes as editable art independent of mechanics.</summary>
internal static class GrappleSpriteArtworkVerification
{
    /// <summary>Compares native OAM across angles and animation ticks, then isolates tile and palette edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Grapple endpoint and segment OBJ attributes")]
    internal static void VerifyGrappleSpriteArtwork(SuperMetroidAddressSpace bus)
    {
        byte[] json = GrappleSpriteExtractor.Extract(bus);
        byte[] png = GrappleTileExtractor.Extract(bus);
        var catalog = GrappleSpriteCatalog.Load(new MemoryStream(json));
        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.GrappleEndpointAttributes.Value), catalog.Endpoint, "Extracted endpoint attributes match native immediate");
        for (int i = 0; i < 4; i++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleSpriteDefinitions.SegmentAttributeAddresses[i]), catalog.Segment(i), "Extracted segment attributes match timed native record");
        // The installed grapple sheet composes each frame from the installed appearances, which match the cartridge above:
        // the endpoint as one small OBJ, and each rope segment with the rope direction's flips ORed into its own.
        SpriteSheetSources composed = GrappleSheetComposer.Sources(SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory));
        Dictionary<string, SpriteFrame> frames = composed.Frames.ToDictionary(frame => frame.Name, frame => frame.Frame, StringComparer.Ordinal);
        int cases = 0;
        for (int point = 0; point < GrappleSheetDefinitions.EndpointFrameCount; point++, cases++)
            AssertEqual(SpritePartFrames.SmallObject(new SnesObjAttributeWord(checked((ushort)catalog.Endpoint)), 0).Parts.Single(),
                frames[GrappleSheetDefinitions.EndpointFrame(point)].Parts.Single(), $"Grapple endpoint frame {point} is the native endpoint OBJ");
        foreach (GrappleRopeOrientation orientation in Enum.GetValues<GrappleRopeOrientation>())
        for (int segment = 0; segment < GrappleSpriteDefinitions.SegmentFrameCount; segment++)
        foreach (SpriteMirror mirror in GrappleSheetDefinitions.RopeMirrors)
        {
            ushort flips = (ushort)(((mirror & SpriteMirror.Horizontal) != 0 ? 0x4000 : 0) | ((mirror & SpriteMirror.Vertical) != 0 ? 0x8000 : 0));
            AssertEqual(SpritePartFrames.SmallObject(new SnesObjAttributeWord((ushort)(catalog.Segment(segment) | flips)), 0).Parts.Single(),
                frames[GrappleSheetDefinitions.RopeFrame(orientation, segment, mirror)].Parts.Single(),
                $"Grapple rope {orientation} frame {segment} mirrored {mirror} is the native segment OBJ with the rope's flips");
            cases++;
        }
        foreach (string bad in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1}", System.Text.Encoding.UTF8.GetString(json).Replace("\"palette\": 5", "\"palette\": 8") })
            AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(bad))), "Invalid Grapple presentation rejected");
        var missing = RequiredJson.Parse(json); missing.Required("segments").AsArray().RemoveAt(0);
        AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(missing.ToJsonString()))), "Missing segment appearance rejected");
        var mechanic = RequiredJson.Parse(json); mechanic.Required("segments").Required(0)["duration"] = 1;
        AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mechanic.ToJsonString()))), "Editable timing field rejected");
        AssertThrows<InvalidDataException>(() => catalog.Segment(4), "Out-of-range visual selector rejected");
        foreach (var (field, value) in new VerifyGrappleSpriteArtworkCase[] { new VerifyGrappleSpriteArtworkCase("tileColumn", -1), new VerifyGrappleSpriteArtworkCase("tileColumn", 16), new VerifyGrappleSpriteArtworkCase("tileRow", -1), new VerifyGrappleSpriteArtworkCase("tileRow", 32), new VerifyGrappleSpriteArtworkCase("palette", -1), new VerifyGrappleSpriteArtworkCase("priority", 4) })
        {
            var invalid = RequiredJson.Parse(json); invalid.Required("endpoint")[field] = value;
            AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))), "Invalid Grapple style field: " + field);
        }
        var incomplete = RequiredJson.Parse(json); incomplete.Required("endpoint").AsObject().Remove("priority");
        AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(incomplete.ToJsonString()))), "Missing visual field rejected");
        var boundary = RequiredJson.Parse(json);
        boundary.Required("endpoint")["tileColumn"] = 15; boundary.Required("endpoint")["tileRow"] = 31;
        boundary.Required("endpoint")["palette"] = 7; boundary.Required("endpoint")["priority"] = 3;
        boundary.Required("endpoint")["flipX"] = true; boundary.Required("endpoint")["flipY"] = true;
        AssertEqual(ushort.MaxValue, GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(boundary.ToJsonString()))).Endpoint,
            "Every OBJ attribute bit can be authored without leaking into timing/geometry");
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
