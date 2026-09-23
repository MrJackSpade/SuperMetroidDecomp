using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Checks every stock visual reference against its native source, then proves an
    /// installed override changes both rendered planes without changing collision/BTS.
    /// </summary>
    private static void VerifyRoomVisualLayouts(GameInstallation installed,
        ISnesAddressSpace bus, CartridgeRoomHeader landing)
    {
        RoomVisualLayoutCatalog stock = installed.LoadRoomVisualLayouts();
        AssertEqual(246, RoomVisualLayoutFiles.RetailSources.Count,
            "distinct pinned room-level visual sources");
        foreach ((int source, int width) in RoomVisualLayoutFiles.RetailSources)
        {
            RoomVisualLayout layout = stock.Get(source);
            byte[] native = RomDataReader.Decompress(bus, source);
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.WidthInBlocks, $"room ${source:X6} visual stride");
            AssertEqual(count / width, layout.HeightInBlocks,
                $"room ${source:X6} visual allocation height");
            for (int index = 0; index < count; index++)
            {
                ushort foreground = unchecked((ushort)(
                    BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2)) & 0x0fff));
                ushort background = index * 2 < backgroundBytes
                    ? unchecked((ushort)(BinaryPrimitives.ReadUInt16LittleEndian(
                        native.AsSpan(backgroundOffset + index * 2)) & 0x0fff))
                    : (ushort)0;
                AssertEqual(foreground, layout.ForegroundVisualWords.Span[index],
                    $"room ${source:X6} BG1 visual word {index}");
                AssertEqual(background, layout.BackgroundVisualWords.Span[index],
                    $"room ${source:X6} BG2 visual word {index}");
            }
        }

        int landingSource = landing.State.CompressedLevelDataAddress;
        string name = RoomVisualLayoutFiles.SourceFileName(landingSource);
        string stockPath = Path.Combine(installed.RoomVisualLayoutDirectory, name);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed Landing Site layout is empty.");
        JsonArray foregroundWords = document["foregroundVisualWords"]!.AsArray();
        JsonArray backgroundWords = document["backgroundVisualWords"]!.AsArray();
        foregroundWords[0] = foregroundWords[0]!.GetValue<int>() ^ 0x0400;
        backgroundWords[0] = backgroundWords[0]!.GetValue<int>() ^ 0x0400;
        Directory.CreateDirectory(installed.RoomVisualLayoutOverrideDirectory);
        string overridePath = Path.Combine(installed.RoomVisualLayoutOverrideDirectory, name);
        File.WriteAllText(overridePath, document.ToJsonString());

        CartridgeRoomAssets nativeRoom = CartridgeRoomAssets.Load(bus, landing);
        CartridgeRoomAssets stockRoom = CartridgeRoomAssets.Load(bus, landing,
            visualLayouts: stock);
        CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(bus, landing,
            visualLayouts: installed.LoadRoomVisualLayouts());
        AssertTrue(nativeRoom.LevelData.ForegroundEntries.Span.SequenceEqual(
                editedRoom.LevelData.ForegroundEntries.Span) &&
            nativeRoom.LevelData.BehaviorBytes.Span.SequenceEqual(
                editedRoom.LevelData.BehaviorBytes.Span) &&
            nativeRoom.LevelData.BackgroundEntries.Span.SequenceEqual(
                editedRoom.LevelData.BackgroundEntries.Span),
            "visual room-layout override leaves full native BG1/BTS/BG2 allocations unchanged");

        BackgroundTilemapStreamer nativeStream = nativeRoom.LevelData.CreateBackgroundStreamer();
        BackgroundTilemapStreamer stockStream = stockRoom.LevelData.CreateBackgroundStreamer();
        BackgroundTilemapStreamer editedStream = editedRoom.LevelData.CreateBackgroundStreamer();
        PlmTilemapUpdate nativeFirst = nativeStream.BuildPlmLevelBlockUpdate(0, 0);
        PlmTilemapUpdate stockFirst = stockStream.BuildPlmLevelBlockUpdate(0, 0);
        PlmTilemapUpdate editedFirst = editedStream.BuildPlmLevelBlockUpdate(0, 0);
        AssertTrue(nativeFirst.TopRow.AsSpan().SequenceEqual(stockFirst.TopRow) &&
            nativeFirst.BottomRow.AsSpan().SequenceEqual(stockFirst.BottomRow),
            "stock installed BG1 layout retains native rendered tile words");
        AssertTrue(!nativeFirst.TopRow.AsSpan().SequenceEqual(editedFirst.TopRow) ||
            !nativeFirst.BottomRow.AsSpan().SequenceEqual(editedFirst.BottomRow),
            "edited BG1 layout changes the live renderer's first block");

        var bg2Request = new BackgroundUpdateRequest(BackgroundLayer.Background,
            BackgroundUpdateAxis.Row, SourceXBlock: 0, SourceYBlock: 0,
            VramXBlock: 0, VramYBlock: 0);
        TilemapStreamUpdate nativeBg2 = nativeStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Native BG2 row was suppressed.");
        TilemapStreamUpdate stockBg2 = stockStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Stock BG2 row was suppressed.");
        TilemapStreamUpdate editedBg2 = editedStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Edited BG2 row was suppressed.");
        AssertTrue(nativeBg2.FirstHalves.AsSpan().SequenceEqual(stockBg2.FirstHalves),
            "stock installed BG2 layout retains native rendered tile words");
        AssertTrue(!nativeBg2.FirstHalves.AsSpan().SequenceEqual(editedBg2.FirstHalves),
            "edited BG2 layout changes the live renderer's first row");

        // Existing user content is outside the replaceable stock installation. Reimport
        // must retain the selected visual word while native collision remains unchanged.
        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("Room-layout repair lost the installation.");
        AssertEqual(foregroundWords[0]!.GetValue<int>(),
            installed.LoadRoomVisualLayouts().Get(landingSource).ForegroundVisualWords.Span[0],
            "room-layout override survives stock installation validation");

        foregroundWords[0] = 0x8000;
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadRoomVisualLayouts();
            throw new InvalidOperationException("A collision bit was accepted as visual artwork.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains(name, StringComparison.Ordinal),
                "invalid visual-layout override identifies its file");
        }
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailSources.Count} native sources match stock JSON; " +
            "BG1/BG2 edits affect rendering but not collision/BTS, survive repair, and reject collision bits.");
    }
}
