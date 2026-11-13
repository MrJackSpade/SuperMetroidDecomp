using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>Checks extracted Hyper Beam palette artwork against native CGRAM frames and malformed JSON.</summary>
internal static class HyperBeamFxColorArtworkVerification
{
    /// <summary>Compares all ten native timed frames with the installed shades the ROM-free palette loop names, then verifies malformed document rejection.</summary>
    internal static void VerifyHyperBeamFxColorArtwork(ISnesAddressSpace bus)
    {
        byte[] json = HyperBeamFxColorExtractor.Extract(bus);
        var catalog = HyperBeamFxColorFile.Read(new MemoryStream(json));
        HyperBeamFxColours colours = catalog.ToColours();
        AssertEqual(HyperBeamPaletteFxColorDefinitionsConstants.FrameCount,
            HyperBeamFxColorFormat.FrameCount, "Extracted Hyper Beam frame count matches compiled control");
        AssertEqual(HyperBeamPaletteFxColorDefinitions.ColorsPerFrame,
            HyperBeamFxColorFormat.ColorsPerFrame, "Extracted Hyper Beam color count matches compiled control");

        // The Hyper Beam lays a cycle frame's shade over her beams; the import pre-colours each frame's shade from the
        // installed cycle, whose colours each step compares with the cartridge's frame.
        var extracted = new HyperBeamPaletteFxState();
        var extractedColours = new SamusColours();
        SamusShadeSources stockSources = InstalledFixtures.SamusShadeSources with { HyperBeamCycle = colours };

        extracted.Spawn();
        for (int call = 0; call < HyperBeamFxColorFormat.FrameCount *
            HyperBeamPaletteFxColorDefinitionsConstants.FrameDuration + 1; call++)
        {
            int expectedFrame = call / 2 % 10;
            ushort originalDuration = ReadVerificationWord(bus, 0x8dd904 + 20 * expectedFrame);
            AssertEqual((ushort)2, originalDuration, "Native Hyper Beam FX duration");
            int? frameBefore = extracted.CurrentFrameIndex;
            extracted.Step(extractedColours);
            AssertEqual(expectedFrame, extracted.CurrentFrameIndex, "Native FX frame order including loop");
            AssertEqual((call & 1) == 0, extracted.CurrentFrameIndex != frameBefore, "Native FX paint/hold cadence");
            AssertEqual((call & 1) == 0 ? originalDuration : originalDuration - 1, extracted.FrameHold, "Native FX timer");
            AssertEqual(SamusShades.HyperBeamCycle(expectedFrame), extractedColours.BeamOverlays[^1],
                "The Hyper Beam lays its current cycle frame over her beams");
            RgbColor[] shownBeams = extractedColours.ShownBeams(stockSources);
            for (int color = 0; color < 8; color++)
                AssertEqual(RgbColor.FromWord(checked(ReadVerificationWord(bus, 0x8dd906 + 20 * expectedFrame + 2 * color))),
                    shownBeams[SamusSheetInks.HyperBeamFirstColour + color],
                    "Extracted Hyper Beam frame's installed shade shows the native frame's colours");
        }

        foreach (int invalidFrame in new[] { -1, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colours.Frame(invalidFrame), "FX frame bounds");
        string source = Encoding.UTF8.GetString(json);
        AssertThrows<InvalidDataException>(() => HyperBeamFxColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(source.Insert(1, "\"version\":1,")))),
            "Duplicate Hyper Beam FX metadata rejected");
        var invalid = RequiredJson.Parse(json);
        invalid.Required("frames").Required(0).Required(0)["red"] = 32;
        Reject(invalid, "Out-of-range Hyper Beam FX channel rejected");
        invalid = JsonNode.Parse(json);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("frames").AsArray().RemoveAt(0);
        Reject(invalid, "Missing Hyper Beam FX frame rejected");
        invalid = JsonNode.Parse(json);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("frames").Required(0).AsArray().RemoveAt(0);
        Reject(invalid, "Missing Hyper Beam FX color rejected");
        invalid = JsonNode.Parse(json);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("frames").Required(0).Required(0)["damage"] = 1;
        Reject(invalid, "Gameplay property in Hyper Beam FX artwork rejected");
        Console.WriteLine("Hyper Beam FX colors: 80 ROM words, all ten timed frames and loop, full-CGRAM parity, ROM-free live cycle and strict JSON pass.");

        static void Reject(JsonNode document, string message) => AssertThrows<InvalidDataException>(
            () => HyperBeamFxColorFile.Read(
                new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }
}
