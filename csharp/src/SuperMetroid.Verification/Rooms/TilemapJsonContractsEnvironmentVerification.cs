using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Rooms;

/// <summary>
/// Exercises the environmental, intro, and room-metatile JSON compilers with constructed
/// documents to verify their shared version, shape, and word-order contracts.
/// </summary>
internal static class TilemapJsonContractsEnvironmentVerification
{
    /// <summary>
    /// The source audit identified five related tilemap loaders that bypassed the
    /// shared JSON contract. These constructed documents exercise their real
    /// compilers, not gameplay or a search for cartridge accesses.
    /// </summary>
    internal static void VerifyEnvironmentalTilemapJsonContracts()
    {
        var accepted = new List<string>();
        int rejected = 0;
        foreach (int blockCount in new[] { 1, RoomMetatileFormat.MaximumBlockCount })
        {
            byte[] native = TilemapContractWords(blockCount * 4);
            RoomMetatileCell Cell(int index)
            {
                var cell = TilemapContractCell(BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * 2)));
                return new() { TileColumn = cell.TileColumn, TileRow = cell.TileRow, Palette = cell.Palette,
                    Priority = cell.Priority, FlipX = cell.FlipX, FlipY = cell.FlipY };
            }
            var document = new RoomMetatileDocument { Version = RoomMetatileFormat.Version,
                Blocks = Enumerable.Range(0, blockCount).Select(block => new RoomMetatileDefinition
                { TopLeft = Cell(block * 4), TopRight = Cell(block * 4 + 1),
                    BottomLeft = Cell(block * 4 + 2), BottomRight = Cell(block * 4 + 3) }).ToArray() };
            using var encoded = new MemoryStream();
            RoomMetatileAtlas.Write(encoded, document, native.Length);
            string json = Encoding.UTF8.GetString(encoded.ToArray());
            void Load(string text) => AssertTrue(BackgroundTileWordBytes.Of(RoomMetatileAtlas.Load(TilemapContractStream(text), native.Length)
                .BlockTiles.Span).AsSpan().SequenceEqual(native), "metatile compilation retains every quadrant/tile/priority/flip bit");
            Load(json); Load(TilemapContractHistoricalCasing(json));
            foreach (string corrupt in TilemapContractCorruptions(json, root => (JsonObject)root.Required("blocks").Required(0).Required("topLeft"))
                .Concat(new[] { TilemapContractMutate(json, root => root.Required("blocks").Required(0)["collision"] = 7),
                    json.Replace("\"topLeft\":", "\"TopLeft\":null,\"topLeft\":", StringComparison.Ordinal) }))
                Reject($"metatiles ({blockCount})", corrupt, text => RoomMetatileAtlas.Load(TilemapContractStream(text), native.Length));
        }

        var fxWords = RoomFxLayer3TilemapFormat.Types.ToDictionary(type => type,
            _ => TilemapContractWords(RoomFxLayer3TilemapFormat.CellsPerPage));
        var fx = new RoomFxLayer3TilemapDocument { Version = RoomFxLayer3TilemapFormat.Version,
            Pages = fxWords.ToDictionary(pair => pair.Key.ToString(), pair => Enumerable.Range(0, pair.Value.Length / 2)
                .Select(index => TilemapContractCell(BinaryPrimitives.ReadUInt16LittleEndian(pair.Value.AsSpan(index * 2)))).ToArray()) };
        using var fxEncoded = new MemoryStream();
        RoomFxLayer3Pages.Write(fxEncoded, fx);
        string fxJson = Encoding.UTF8.GetString(fxEncoded.ToArray());
        void LoadFx(string text)
        {
            RoomFxLayer3Pages catalog = RoomFxLayer3Pages.Load(TilemapContractStream(text));
            foreach (var pair in fxWords) AssertTrue(catalog.ResolveBytes(pair.Key).AsSpan().SequenceEqual(pair.Value),
                "all six BG3 effect pages retain every compiled word");
        }
        LoadFx(fxJson); LoadFx(TilemapContractHistoricalCasing(fxJson));
        foreach (string corrupt in TilemapContractCorruptions(fxJson, root => (JsonObject)root.Required("pages").Required("Lava").Required(0))
            .Concat(new[] { fxJson.Replace("\"Lava\":", "\"Lava\":[],\"Lava\":", StringComparison.Ordinal),
                TilemapContractMutate(fxJson, root => root.Required("pages")["Rain"] = new JsonArray()) }))
            Reject("room FX BG3", corrupt, text => RoomFxLayer3Pages.Load(TilemapContractStream(text)));

        var eyeWords = TilemapContractWords(IntroEyeTilemapFormat.FrameCount * IntroEyeTilemapFormat.CellsPerFrame);
        var eyes = new IntroEyeTilemapDocument { Version = IntroEyeTilemapFormat.Version,
            Frames = Enumerable.Range(0, IntroEyeTilemapFormat.FrameCount).Select(frame => new IntroEyeTilemapFrame
            { Id = IntroEyeTilemapFormat.FrameId(frame), Cells = Enumerable.Range(0, IntroEyeTilemapFormat.CellsPerFrame)
                .Select(index => TilemapContractCell(BinaryPrimitives.ReadUInt16LittleEndian(eyeWords.AsSpan(
                    (frame * IntroEyeTilemapFormat.CellsPerFrame + index) * 2)))).ToArray() }).ToArray() };
        using var eyesEncoded = new MemoryStream();
        IntroEyeTilemapPresentation.Write(eyesEncoded, eyes);
        string eyeJson = Encoding.UTF8.GetString(eyesEncoded.ToArray());
        void LoadEyes(string text)
        {
            IntroEyeTilemapPresentation catalog = IntroEyeTilemapPresentation.Load(TilemapContractStream(text));
            for (int frame = 0; frame < IntroEyeTilemapFormat.FrameCount; frame++)
            for (int index = 0; index < IntroEyeTilemapFormat.CellsPerFrame; index++)
                AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(eyeWords.AsSpan((frame * IntroEyeTilemapFormat.CellsPerFrame + index) * 2)),
                    catalog.FrameTiles(frame)[index].PackedWord, "all eye rectangles retain ordered visual words, encoded through the codec");
        }
        LoadEyes(eyeJson);
        Reject("opening eyes (case-sensitive)", TilemapContractHistoricalCasing(eyeJson),
            text => IntroEyeTilemapPresentation.Load(TilemapContractStream(text)));
        foreach (string corrupt in TilemapContractCorruptions(eyeJson, root => (JsonObject)root.Required("frames").Required(0).Required("cells").Required(0))
            .Concat(new[] { TilemapContractMutate(eyeJson, root => root.Required("frames").Required(0)["duration"] = 7),
                eyeJson.Replace("\"id\":", "\"id\":\"unused\",\"id\":", StringComparison.Ordinal) }))
            Reject("opening eyes", corrupt, text => IntroEyeTilemapPresentation.Load(TilemapContractStream(text)));

        byte[] dividerWords = TilemapContractWords(IntroFinalLineTilemapFormat.CellCount);
        var divider = new IntroFinalLineTilemapDocument { Version = IntroFinalLineTilemapFormat.Version,
            Cells = Enumerable.Range(0, IntroFinalLineTilemapFormat.CellCount).Select(index => TilemapContractCell(
                BinaryPrimitives.ReadUInt16LittleEndian(dividerWords.AsSpan(index * 2)))).ToArray() };
        using var dividerEncoded = new MemoryStream();
        IntroFinalLineTilemap.Write(dividerEncoded, divider);
        string dividerJson = Encoding.UTF8.GetString(dividerEncoded.ToArray());
        void LoadDivider(string text)
        {
            ushort[] actual = BackgroundTileWordBytes.Words(IntroFinalLineTilemap.Load(TilemapContractStream(text)).Tiles.Span);
            for (int index = 0; index < actual.Length; index++)
                AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(dividerWords.AsSpan(index * 2)), actual[index],
                    "opening divider retains every native word, encoded through the codec");
        }
        LoadDivider(dividerJson);
        Reject("opening divider (case-sensitive)", TilemapContractHistoricalCasing(dividerJson),
            text => IntroFinalLineTilemap.Load(TilemapContractStream(text)));
        foreach (string corrupt in TilemapContractCorruptions(dividerJson, root => (JsonObject)root.Required("cells").Required(0)))
            Reject("opening divider", corrupt, text => IntroFinalLineTilemap.Load(TilemapContractStream(text)));

        var overlayWords = CeresEscapeOverlayTilemapDefinitions.All.ToDictionary(page => page.Name,
            page => Enumerable.Range(0, page.WordCount).Select(index => unchecked((ushort)(index * 197 + 23))).ToArray());
        string overlayJson = Encoding.UTF8.GetString(CeresEscapeOverlays.Write(new()
            { Version = CeresWarningGlyphDefinitions.Version, Pages = overlayWords.ToDictionary(page => page.Key, page => BackgroundTileWordBytes.Tiles(page.Value)) }));
        var overlay = CeresEscapeOverlays.Load(TilemapContractStream(overlayJson));
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            AssertTrue(overlay.TryResolve(page, out var actual), "every Ceres warning page resolves");
            for (int index = 0; index < page.WordCount; index++) AssertEqual(overlayWords[page.Name][index],
                actual.Span[index].PackedWord, "warning words retain exact order");
        }
        foreach (string corrupt in new[] { "{\"version\":1," + overlayJson[1..],
            TilemapContractMutate(overlayJson, root => root["duration"] = 7),
            overlayJson.Replace("\"emergency\":", "\"emergency\":[],\"emergency\":", StringComparison.Ordinal),
            TilemapContractMutate(overlayJson, root => root.Required("pages").AsObject().Remove("emergency")),
            TilemapContractMutate(overlayJson, root => root.Required("pages")["japanese_0"] = new JsonArray()) })
            Reject("Ceres warning", corrupt, text => CeresEscapeOverlays.Load(TilemapContractStream(text)));

        AssertEqual(0, accepted.Count, "tilemap loaders must reject ambiguous/unknown content:\n" + string.Join('\n', accepted));
        Console.WriteLine($"PASS environmental/intro/metatile JSON contracts: five compilers, {rejected} invalid documents, " +
            "exact all-word roundtrips and historical casing where supported.");

        void Reject(string name, string corrupt, Action<string> load)
        {
            try { load(corrupt); }
            catch (InvalidDataException) { rejected++; return; }
            accepted.Add(name + ": " + corrupt[..Math.Min(110, corrupt.Length)]);
        }
    }
}
