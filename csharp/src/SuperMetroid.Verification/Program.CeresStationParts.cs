using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresStationParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresStation", value.AppendIdentity);
        foreach (var definition in CeresFlightSpriteDefinitions.Frames.Where(frame => frame.Name == "station-under-attack"))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCeresStationLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresStationParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresStation selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresStation compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 15 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = part.Size == 8 ? 16 : 8, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    CheckCeresStationLoader(definition.Pointer, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited CeresStation");
                    var selected = CeresStationParts.CalculateIfMatching(definition.Pointer, supplied);
                    AssertEqual(Identity(supplied), Identity(selected), "edited planet identity");
                    AssertTrue(ReferenceEquals(supplied, selected),
                        "independent CeresStation field edit stays supplied");
                }
            }
            CheckCeresStationLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom CeresStation");
            AssertTrue(ReferenceEquals(extra, CeresStationParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated CeresStation part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, 120, y, 0x0a00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, 120, y, 0x0a00);
                else result.DrawOffScreen(actual, 120, y, 0x0a00);
                actual.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresStation OAM at visible and wrapped origins");
            }
        }
    }
    private static void CheckCeresStationLoader(ushort pointer, SpriteVisualPart[] visual)
    {
        var definition = CeresFlightSpriteDefinitions.Frames.FirstOrDefault(frame => frame.Pointer == pointer);
        if (definition.Name is null) return;
        var frames = CeresFlightSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        frames[definition.Name] = visual;
        using var json = new MemoryStream();
        CeresFlightSpritePresentation.Write(json, new() { Version = CeresFlightSpriteFormat.Version, Frames = frames });
        json.Position = 0;
        var presentation = CeresFlightSpritePresentation.Load(json);
        var reference = IntroCinematicSpriteCompiler.Compile(visual, "Ceres blast reference");
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var expected = new OamBuffer(); expected.BeginFrame();
            if (y == 72) reference.DrawOnScreen(expected, 120, y, 0x0a00);
            else reference.DrawOffScreen(expected, 120, y, 0x0a00);
            expected.FinalizeFrame();
            var actual = new OamBuffer(); actual.BeginFrame();
            presentation.Draw(pointer, actual, 120, y, 0x0a00, y == 72);
            actual.FinalizeFrame();
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable)
                && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
                "Ceres alias loader preserves original and independently edited blast compositions");
        }
    }}
