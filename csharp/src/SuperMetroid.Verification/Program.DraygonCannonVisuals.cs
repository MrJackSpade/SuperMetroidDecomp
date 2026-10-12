using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using static LookupDraygonCannonStockVerification;

/// <summary>Represents draygon cannon visuals verification.</summary>
internal static class DraygonCannonVisualsVerification
{
    /// <summary>Verifies draygon cannon visual installation.</summary>
    internal static void VerifyDraygonCannonVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("draygon-cannon-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction compares all twelve reachable native layouts, including
            // every row word and signed offset, with the compiled definitions.
            RoomPlmDraygonCannonVisualFiles.Extract(rom,
                installation.RoomPlmDraygonCannonVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmDraygonCannonVisualFiles.ValidateStock(
                installation.RoomPlmDraygonCannonVisualDirectory);
            VerifyDraygonCannonStockMapping(rom, installation.LoadRoomPlmDraygonCannonVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmDraygonCannonVisualDirectory,
                RoomPlmDraygonCannonVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted cannon JSON is empty.");
            JsonNode rightShield = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "right-shield-a")!;
            rightShield["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmDraygonCannonVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmDraygonCannonVisualOverrideDirectory,
                RoomPlmDraygonCannonVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmDraygonCannonVisuals().GetWord(
                    (ushort)DraygonCannonDraw.RightShieldA, 0, 0),
                "installed cannon override selects its edited block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmDraygonCannonVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmDraygonCannonVisualFiles.Load(refreshed,
                    installation.RoomPlmDraygonCannonVisualOverrideDirectory)
                    .GetWord((ushort)DraygonCannonDraw.RightShieldA, 0, 0),
                "cannon override survives stock replacement");

            rightShield["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmDraygonCannonVisuals(),
                "cannon override rejects physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDraygonCannonVisualFiles.ValidateStock(
                    installation.RoomPlmDraygonCannonVisualDirectory),
                "tampered cannon stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
