using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Known shared-presentation domains only; no ROM or gameplay path discovery.</summary>
    private static void VerifyGameplayContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateGameplayIdentityFixture();
        IReadOnlyDictionary<string, string> reordered = CreateGameplayIdentityFixture(reverse: true);
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, reordered[component], component + " canonical record order");
        foreach ((string edit, string component) in new[]
        {
            ("initial", GameInstallationLayout.GameplayBasePaletteDirectoryName),
            ("common", GameInstallationLayout.GameplayBasePaletteDirectoryName),
            ("sprite-pixel", GameInstallationLayout.StandardObjectDirectoryName),
            ("xray-top-left", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("xray-top-right", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("xray-bottom-left", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("xray-bottom-right", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("xray-item", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("overlay-x", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("overlay-y", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("overlay-word", GameInstallationLayout.XrayRevealVisualDirectoryName),
            ("overlay-order", GameInstallationLayout.XrayRevealVisualDirectoryName),
        })
        {
            IReadOnlyDictionary<string, string> changed = CreateGameplayIdentityFixture(edit);
            AssertTrue(baseline[component] != changed[component], edit + " invalidates selected content");
            foreach (string other in baseline.Keys.Where(name => name != component))
                AssertEqual(baseline[other], changed[other], edit + " preserves unrelated " + other);
            var before = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, baseline);
            var after = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, changed);
            AssertTrue(before.CompositeSha256 != after.CompositeSha256, edit + " changes the host identity");
            AssertTrue(before.GetCompatibilityWarnings(after.ToSnapshot(), "test").Single()
                .Contains(component, StringComparison.Ordinal), edit + " gives a specific compatibility warning");
        }

        // Fingerprint decoded selections, not cosmetic file formatting/palette colors. A tile sheet
        // contributes palette indices; the independently installed CGRAM catalog supplies the colors.
        AssertEqual(baseline[GameInstallationLayout.StandardObjectDirectoryName],
            CreateGameplayIdentityFixture("png-colors")[GameInstallationLayout.StandardObjectDirectoryName],
            "PNG display colors do not change decoded tile indices");
        AssertEqual(baseline[GameInstallationLayout.GameplayBasePaletteDirectoryName],
            CreateGameplayIdentityFixture("json-indent")[GameInstallationLayout.GameplayBasePaletteDirectoryName],
            "JSON whitespace does not change decoded colors");
        Console.WriteLine("  Shared gameplay content: colors, sprite pixels, all X-ray operands, items, " +
            "room overlay positions/order, canonical records and host warnings pass without a ROM.");
    }

    private static IReadOnlyDictionary<string, string> CreateGameplayIdentityFixture(
        string? edit = null, bool reverse = false)
    {
        PaletteRgb5[] Colors(int count, bool changed) => Enumerable.Range(0, count)
            .Select(index => new PaletteRgb5 { Red = changed && index == count - 1 ? 1 : 0, Green = 0, Blue = 0 }).ToArray();
        var paletteDocument = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            Colors(SnesCgram.ColorCount, edit == "initial"),
            Colors(GameplayBasePaletteFormat.SpriteColorCount, edit == "common"));
        GameplayBasePaletteCatalog palettes = GameplayBasePaletteCatalog.Load(new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(paletteDocument,
                new JsonSerializerOptions { WriteIndented = edit == "json-indent" })));
        using var png = new MemoryStream();
        var pixels = new byte[64];
        if (edit == "sprite-pixel") pixels[^1] = 1;
        IndexedPng.Write(png, 8, 8, pixels, [new Rgba32(0, 0, 0),
            edit == "png-colors" ? new Rgba32(255, 0, 0) : new Rgba32(255, 255, 255)]);
        png.Position = 0;
        RoomCharacterAtlas sprites = RoomCharacterAtlas.Load(png, RoomCharacterAtlasFormat.BytesPerTile);

        var entries = new List<(RoomCollisionType Type, byte Bts, XrayRevealVisualWords Visual)>();
        bool editedOperands = false;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            if (XrayRevealTable.Find(type, (byte)bts) is not { } native ||
                !XrayRevealVisualCatalog.IsDrawable(native.Command)) continue;
            var visual = new XrayRevealVisualWords(native.TopLeft, native.TopRight, native.BottomLeft, native.BottomRight);
            if (!editedOperands && native.Command == XrayRevealCodePointers.CopySquare && edit?.StartsWith("xray-", StringComparison.Ordinal) == true)
            {
                visual = visual with
                {
                    TopLeft = (ushort)(visual.TopLeft ^ (edit == "xray-top-left" ? 1 : 0)),
                    TopRight = (ushort)(visual.TopRight ^ (edit == "xray-top-right" ? 1 : 0)),
                    BottomLeft = (ushort)(visual.BottomLeft ^ (edit == "xray-bottom-left" ? 1 : 0)),
                    BottomRight = (ushort)(visual.BottomRight ^ (edit == "xray-bottom-right" ? 1 : 0)),
                };
                editedOperands = true;
            }
            entries.Add((type, (byte)bts, visual));
        }
        AssertEqual(305, entries.Count, "complete compiled drawable rule fixture");
        var items = new ushort[XrayOverlayRomData.DynamicGraphicsSlots * 2];
        if (edit == "xray-item") items[^1] = 1;
        XrayRoomOverlayVisual[] tiles =
        [
            new(1, 2, 3),
            new((byte)(edit == "overlay-x" ? 5 : 4), (byte)(edit == "overlay-y" ? 6 : 5),
                (ushort)(edit == "overlay-word" ? 7 : 6)),
        ];
        if (edit == "overlay-order") Array.Reverse(tiles);
        (ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)[] rooms =
            [(0x8000, tiles), (0x8002, new XrayRoomOverlayVisual[] { new(8, 9, 10) })];
        var overlays = XrayOverlayVisualCatalog.FromOverlaysForVerification(items, reverse ? rooms.Reverse() : rooms);
        var xray = new XrayRevealVisualCatalog(reverse ? entries.AsEnumerable().Reverse() : entries, overlays);
        return GameplayPresentationIdentity.Create(palettes, sprites, xray);
    }
}
