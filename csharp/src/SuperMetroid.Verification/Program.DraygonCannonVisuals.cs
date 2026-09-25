using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyDraygonCannonVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "draygon-cannon-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Draygon cannon test root escaped test-temp.");
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
            AssertEqual((ushort)0x0514,
                installation.LoadRoomPlmDraygonCannonVisuals().GetWord(
                    DraygonCannonPlmDrawDefinitions.RightShieldA, 0, 0),
                "stock right cannon shield retains cartridge visual reference");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmDraygonCannonVisualCatalog(
                    [new RoomPlmDraygonCannonVisualEntry("right-shield-a",
                        new ushort[4])]),
                "cannon catalog rejects missing reachable frames");

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
                    DraygonCannonPlmDrawDefinitions.RightShieldA, 0, 0),
                "installed cannon override selects its edited block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmDraygonCannonVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmDraygonCannonVisualFiles.Load(refreshed,
                    installation.RoomPlmDraygonCannonVisualOverrideDirectory)
                    .GetWord(DraygonCannonPlmDrawDefinitions.RightShieldA, 0, 0),
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
