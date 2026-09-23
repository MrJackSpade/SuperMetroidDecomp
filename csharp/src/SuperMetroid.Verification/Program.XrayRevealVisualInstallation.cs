using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Checks every installed visual rule against the compiled cartridge dispatcher,
    /// then changes a tall reveal without changing its two-block copy behavior.
    /// </summary>
    private static void VerifyXrayRevealVisualInstallation(GameInstallation installed)
    {
        XrayRevealVisualCatalog stock = installed.LoadXrayRevealVisuals();
        int drawable = 0;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, unchecked((byte)bts));
            if (native is not { } definition ||
                !XrayRevealVisualCatalog.IsDrawable(definition.Command)) continue;
            AssertEqual(definition, stock.Apply(type, unchecked((byte)bts), definition),
                $"stock installed X-ray visual {type}/BTS ${bts:X2}");
            drawable++;
        }
        AssertEqual(305, drawable, "complete pinned drawable X-ray rule count");

        string stockPath = Path.Combine(installed.XrayRevealVisualDirectory,
            XrayRevealVisualFiles.VisualFileName);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed X-ray visuals are empty.");
        JsonArray entries = document["entries"]!.AsArray();
        JsonObject tall = entries.Select(node => node!.AsObject()).Single(entry =>
            entry["collisionType"]!.GetValue<string>() == nameof(RoomCollisionType.ShootableBlock) &&
            entry["btsValues"]!.AsArray().Any(value => value!.GetValue<int>() == 2));
        AssertEqual("tall", tall["shape"]!.GetValue<string>(),
            "shot-block BTS 2 uses the native tall copy command");
        ushort originalTop = unchecked((ushort)tall["topLeft"]!.GetValue<int>());
        tall["topLeft"] = originalTop + 1;
        Directory.CreateDirectory(installed.XrayRevealVisualOverrideDirectory);
        string overridePath = Path.Combine(installed.XrayRevealVisualOverrideDirectory,
            XrayRevealVisualFiles.VisualFileName);
        File.WriteAllText(overridePath, document.ToJsonString());
        XrayRevealVisualCatalog edited = installed.LoadXrayRevealVisuals();

        var vram = new SnesVram();
        var definitions = new byte[1024 * 8];
        for (int i = 0; i < definitions.Length / 2; i++)
        {
            definitions[i * 2] = (byte)i;
            definitions[i * 2 + 1] = (byte)(i >> 8);
        }
        var level = new RoomLevelData(32, 32,
            Enumerable.Repeat((ushort)0x8000, 1024).ToArray(), new byte[1024],
            new ushort[1024], definitions);
        level.SetPlmForegroundEntry(33, (ushort)((int)RoomCollisionType.ShootableBlock << 12));
        level.SetPlmBehavior(33, 2);
        RoomCollisionBlock before = level.GetPlmCollisionBlockByIndex(33);
        ushort[] originalMap = XrayRevealTilemap.Build(level, vram, 0, 0, 16, 16,
            (byte)AreaId.Brinstar, stock);
        ushort[] editedMap = XrayRevealTilemap.Build(level, vram, 0, 0, 16, 16,
            (byte)AreaId.Brinstar, edited);
        AssertEqual(originalTop * 4, originalMap[0], "stock tall reveal top-left metatile");
        AssertEqual((originalTop + 1) * 4, editedMap[0],
            "edited X-ray metatile changes live reveal tilemap");
        AssertEqual(0xb8 * 4, editedMap[64],
            "edited tall reveal still copies the native lower block");
        AssertEqual(before, level.GetPlmCollisionBlockByIndex(33),
            "X-ray visual override leaves collision type and BTS untouched");

        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("X-ray repair lost the installation.");
        AssertEqual((ushort)(originalTop + 1), installed.LoadXrayRevealVisuals()
                .Apply(RoomCollisionType.ShootableBlock, 2,
                    XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 2)!.Value).TopLeft,
            "X-ray override survives stock installation validation");

        tall["shape"] = "wide";
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadXrayRevealVisuals();
            throw new InvalidOperationException("An X-ray command edit was accepted as artwork.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains(XrayRevealVisualFiles.VisualFileName,
                StringComparison.Ordinal), "invalid X-ray override identifies its file");
        }
        Console.WriteLine($"  X-ray visuals: {drawable} cartridge rules match stock JSON; " +
            "tile edits affect reveal art, not collision/copy shape, survive repair, and reject command edits.");
    }
}
