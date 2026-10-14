using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Artwork;
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
        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleSpriteDefinitions.EndpointAttributeAddress), catalog.Endpoint, "Extracted endpoint attributes match native immediate");
        for (int i = 0; i < 4; i++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleSpriteDefinitions.SegmentAttributeAddresses[i]), catalog.Segment(i), "Extracted segment attributes match timed native record");
        var document = RequiredJson.Parse(json);
        Change(document.Required("endpoint"));
        foreach (var segment in document.Required("segments").AsArray().RequiredElements()) Change(segment);
        var changedCatalog = GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        var stock = GrappleTileAtlas.Load(new MemoryStream(png), catalog);
        var changed = GrappleTileAtlas.Load(new MemoryStream(png), changedCatalog);
        var guard = new CartridgeReadGuard(bus).Forbid(
            address => address is >= 0x94b13d and <= 0x94b13e or >= 0x94b17d and <= 0x94b17e or >= 0x94b18b and <= 0x94b19e,
            _ => "Grapple still reads native visual records.");
        int cases = 0;
        for (int angle = 0; angle < 256; angle += 4)
        {
            var originalState = Seed(); var stockState = Seed(); var editedState = Seed();
            for (int tick = 0; tick < 25; tick++)
            {
                var originalOam = new OamBuffer(); var stockOam = new OamBuffer(); var editedOam = new OamBuffer();
                var originalQueue = new VramWriteQueue(); var stockQueue = new VramWriteQueue(); var editedQueue = new VramWriteQueue();
                SamusGrappleMovement.DrawConnectedBeam(bus, originalState, originalOam, originalQueue, new(0, 0));
                SamusGrappleMovement.DrawConnectedBeam(guard, stockState, stockOam, stockQueue, new(0, 0), stock);
                SamusGrappleMovement.DrawConnectedBeam(guard, editedState, editedOam, editedQueue, new(0, 0), changed);
                AssertTrue(originalOam.ImageEquals(stockOam), "Stock JSON preserves entire native OAM");
                AssertEqual(stockOam.NextByteOffset, editedOam.NextByteOffset, "Visual edit preserves segment count");
                AssertTrue(stockOam.HighTable.SequenceEqual(editedOam.HighTable), "Visual edit preserves object size and high-X");
                for (int offset = 0; offset < stockOam.NextByteOffset; offset += 4)
                {
                    AssertEqual(stockOam.LowTable[offset], editedOam.LowTable[offset], "Visual edit preserves segment X");
                    AssertEqual(stockOam.LowTable[offset + 1], editedOam.LowTable[offset + 1], "Visual edit preserves segment Y");
                    AssertEqual((byte)(stockOam.LowTable[offset + 2] ^ 1), editedOam.LowTable[offset + 2], "Selected tile column reaches real renderer");
                    AssertEqual((byte)(stockOam.LowTable[offset + 3] ^ 2), editedOam.LowTable[offset + 3], "Selected palette reaches real renderer");
                }
                AssertTrue(GraphDigest(originalState).SequenceEqual(GraphDigest(stockState)) &&
                    GraphDigest(stockState).SequenceEqual(GraphDigest(editedState)), "Visual edits preserve all Grapple state and animation timing");
                AssertTrue(stockQueue.Entries.SequenceEqual(editedQueue.Entries), "Visual attributes leave uploads unchanged");
                cases++;
            }
            SamusGrappleState Seed()
            {
                var state = new SamusGrappleState
                {
                    Phase = GrapplePhase.ConnectedSwinging, RopeLength = 120,
                    BeamStart = new(100, 100), Anchor = new(200, 60),
                    Angle = SnesAngle.FromTableIndex(unchecked((byte)angle)), PointAnimationTimer = NativeCountdown.Start(5),
                };
                for (int i = 0; i < 16; i++) { state.SegmentAnimationTimers[i] = NativeCountdown.Start(1); state.SegmentAnimationFrames[i] = (byte)(i & 3); }
                return state;
            }
        }
        foreach (string bad in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1}", System.Text.Encoding.UTF8.GetString(json).Replace("\"palette\": 5", "\"palette\": 8") })
            AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(bad))), "Invalid Grapple presentation rejected");
        var missing = RequiredJson.Parse(json); missing.Required("segments").AsArray().RemoveAt(0);
        AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(missing.ToJsonString()))), "Missing segment appearance rejected");
        var mechanic = RequiredJson.Parse(json); mechanic.Required("segments").Required(0)["duration"] = 1;
        AssertThrows<InvalidDataException>(() => GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mechanic.ToJsonString()))), "Editable timing field rejected");
        AssertThrows<InvalidDataException>(() => catalog.Segment(4), "Out-of-range visual selector rejected");
        foreach (var (field, value) in new (string, int)[] { ("tileColumn", -1), ("tileColumn", 16), ("tileRow", -1), ("tileRow", 32), ("palette", -1), ("priority", 4) })
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
        Console.WriteLine($"Grapple sprite JSON: {cases} production frames match native OAM; edited tile/palette preserves positions, full simulation state and uploads with visual ROM forbidden.");
        static void Change(JsonNode style)
        {
            style["tileColumn"] = style.Required("tileColumn").GetValue<int>() ^ 1;
            style["palette"] = style.Required("palette").GetValue<int>() ^ 1;
        }
    }

    /// <summary>Runs the Grapple sprite-artwork verification command with the retail cartridge.</summary>
    [VerificationCommand("--grapple-sprite-artwork")]
    internal static int GrappleSpriteArtworkCommand()
    {
        Suite(nameof(VerifyGrappleSpriteArtwork), () => VerifyGrappleSpriteArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
