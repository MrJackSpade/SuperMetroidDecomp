using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroScientistParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("Scientist", value.AppendIdentity);
        foreach (var definition in IntroScientistSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCaretLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroScientistParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original Scientist selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original Scientist compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 16 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = part.Size == 8 ? 16 : 8, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    CheckCaretLoader(definition.Pointer, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited Scientist");
                    AssertTrue(ReferenceEquals(supplied, IntroScientistParts.CalculateIfMatching(definition.Pointer, supplied)),
                        "independent Scientist field edit stays supplied");
                }
            }
            CheckCaretLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom Scientist");
            AssertTrue(ReferenceEquals(extra, IntroScientistParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated Scientist part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, 120, y, 0x0c00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, 120, y, 0x0c00);
                else result.DrawOffScreen(actual, 120, y, 0x0c00);
                actual.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native Scientist OAM at visible and wrapped origins");
            }
        }

        static void CheckCaretLoader(ushort pointer, SpriteVisualPart[] visual)
        {
            if (pointer != IntroCaretSpriteDefinitions.Still) return;
            var expected = new OamBuffer(); expected.BeginFrame();
            IntroCinematicSpriteCompiler.Compile(visual, "caret reference").DrawOnScreen(expected, 120, 72, 0x0c00);
            expected.FinalizeFrame();
            foreach (IntroCaretSpriteVersion version in Enum.GetValues<IntroCaretSpriteVersion>())
            {
                var frames = new Dictionary<string, SpriteVisualPart[]>();
                if (version == IntroCaretSpriteVersion.Previous)
                {
                    foreach (string name in IntroCaretSpriteDefinitions.PreviousFrameNames) frames.Add(name, []);
                    frames[IntroCaretSpriteDefinitions.PreviousFrameNames[0]] = visual;
                }
                else frames.Add("caret-visible", visual);
                using var json = new MemoryStream();
                IntroCaretSpritePresentation.Write(json, new() { Version = (int)version, Frames = frames });
                json.Position = 0;
                var presentation = IntroCaretSpritePresentation.Load(json);
                var actual = new OamBuffer(); actual.BeginFrame();
                presentation.Draw(pointer, actual, 120, 72, 0x0c00);
                actual.FinalizeFrame();
                AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable)
                    && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
                    "both caret schema loaders preserve original and independently edited compositions");
            }
        }
    }
}
