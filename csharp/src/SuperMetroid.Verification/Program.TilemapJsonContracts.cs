using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Exercises the actual shared room/Kraid loaders, not a parallel JSON validator.</summary>
    private static void VerifyTilemapJsonContracts()
    {
        VerifyEnvironmentalTilemapJsonContracts();
        int rejected = 0;
        foreach (int pages in new[] { 1, 2 })
        {
            byte[] native = TilemapContractWords(pages * RoomBackgroundTilemapFormat.CellsPerPage);
            var cells = Enumerable.Range(0, native.Length / 2).Select(index => TilemapContractCell(
                BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * 2)))).ToArray();
            var document = new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = Enumerable.Range(0, pages).Select(page => new RoomBackgroundTilemapPage
                { Cells = cells.Skip(page * RoomBackgroundTilemapFormat.CellsPerPage)
                    .Take(RoomBackgroundTilemapFormat.CellsPerPage).ToArray() }).ToArray(),
            };
            using var output = new MemoryStream();
            RoomBackgroundTilemapAtlas.Write(output, document, native.Length);
            string json = Encoding.UTF8.GetString(output.ToArray());
            void Load(string text) => AssertTrue(RoomBackgroundTilemapAtlas.Load(
                TilemapContractStream(text), native.Length).Transfer.Span.SequenceEqual(native),
                "room map preserves every tile/palette/priority/flip bit and page order");
            Load(json);
            Load(TilemapContractHistoricalCasing(json));
            JsonObject Cell(JsonObject root) => (JsonObject)root["pages"]![0]!["cells"]![0]!;
            foreach (string corrupt in TilemapContractCorruptions(json, Cell).Concat(new[]
            {
                TilemapContractMutate(json, root => ((JsonObject)root["pages"]![0]!)["duration"] = 7),
                json.Replace("\"cells\":", "\"Cells\":[],\"cells\":", StringComparison.Ordinal),
                TilemapContractMutate(json, root => root["pages"] = new JsonArray()),
                TilemapContractMutate(json, root => ((JsonObject)root["pages"]![0]!)["cells"] = new JsonArray()),
            }))
            {
                AssertThrows<InvalidDataException>(() => RoomBackgroundTilemapAtlas.Load(
                    TilemapContractStream(corrupt), native.Length), "room map rejects ambiguous/invalid authored data");
                rejected++;
            }
        }

        byte[] headWords = TilemapContractWords(KraidBackgroundRomData.HeadTilemapWords);
        string headJson = Encoding.UTF8.GetString(KraidHeadTilemapAtlas.Encode(headWords));
        void LoadHead(string text)
        {
            ReadOnlySpan<ushort> actual = KraidHeadTilemapAtlas.Load(TilemapContractStream(text)).Words.Span;
            for (int word = 0; word < actual.Length; word++)
                AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(headWords.AsSpan(word * 2)),
                    actual[word], "head map preserves every tile/palette/priority/flip bit");
        }
        LoadHead(headJson);
        LoadHead(TilemapContractHistoricalCasing(headJson));
        JsonObject HeadCell(JsonObject root) => (JsonObject)root["cells"]![0]!;
        foreach (string corrupt in TilemapContractCorruptions(headJson, HeadCell).Concat(new[]
        {
            TilemapContractMutate(headJson, root => root["width"] = KraidHeadTilemapFormat.Width - 1),
            TilemapContractMutate(headJson, root => root["height"] = KraidHeadTilemapFormat.Height + 1),
            TilemapContractMutate(headJson, root => root["cells"] = new JsonArray()),
        }))
        {
            AssertThrows<InvalidDataException>(() => KraidHeadTilemapAtlas.Load(TilemapContractStream(corrupt)),
                "Kraid head rejects ambiguous/invalid authored data");
            rejected++;
        }
        Console.WriteLine($"PASS tilemap JSON contracts: one/two-page room maps and Kraid heads, " +
            $"exact word roundtrips/historical casing; {rejected} invalid documents rejected.");
    }

    private static IEnumerable<string> TilemapContractCorruptions(string json, Func<JsonObject, JsonObject> cell)
    {
        yield return TilemapContractMutate(json, root => root["attackDuration"] = 7);
        yield return "{\"version\":1," + json[1..];
        yield return "{\"VERSION\":1," + json[1..];
        yield return TilemapContractMutate(json, root => cell(root)["hitbox"] = 7);
        yield return json.Replace("\"palette\":", "\"palette\":0,\"palette\":", StringComparison.Ordinal);
        yield return json.Replace("\"palette\":", "\"Palette\":0,\"palette\":", StringComparison.Ordinal);
        yield return TilemapContractMutate(json, root => root["version"] = RoomBackgroundTilemapFormat.Version + 1);
        yield return TilemapContractMutate(json, root => cell(root).Remove("priority"));
        yield return TilemapContractMutate(json, root => cell(root)["tileColumn"] = RoomBackgroundTilemapFormat.TileColumns);
        yield return TilemapContractMutate(json, root => cell(root)["tileRow"] = -1);
        yield return TilemapContractMutate(json, root => cell(root)["palette"] = RoomBackgroundTilemapFormat.PaletteCount);
    }

    private static byte[] TilemapContractWords(int count)
    {
        var bytes = new byte[count * sizeof(ushort)];
        for (int word = 0; word < count; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(word * 2), unchecked((ushort)(word * 197 + 23)));
        return bytes;
    }

    private static RoomBackgroundTilemapCell TilemapContractCell(ushort word)
    {
        var fields = new SnesBgTilemapWord(word);
        return new()
        {
            TileColumn = fields.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
            TileRow = fields.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
            Palette = fields.PaletteIndex, Priority = fields.HasPriority,
            FlipX = fields.FlipHorizontally, FlipY = fields.FlipVertically,
        };
    }

    private static string TilemapContractMutate(string json, Action<JsonObject> mutate)
    {
        var root = (JsonObject)JsonNode.Parse(json)!;
        mutate(root);
        return root.ToJsonString();
    }

    private static MemoryStream TilemapContractStream(string text) => new(Encoding.UTF8.GetBytes(text), writable: false);
    private static string TilemapContractHistoricalCasing(string json) => json
        .Replace("\"version\"", "\"VERSION\"", StringComparison.Ordinal)
        .Replace("\"tileColumn\"", "\"TILECOLUMN\"", StringComparison.Ordinal);
}
