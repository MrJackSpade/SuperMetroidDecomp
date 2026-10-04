using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroMotherBrainExplosionParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("MotherBrainExplosion", value.AppendIdentity);
        foreach (var definition in IntroMotherBrainExplosionSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCeresBlastLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original MotherBrainExplosion selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original MotherBrainExplosion compiled fields");
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
                    CheckCeresBlastLoader(definition.Pointer, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited MotherBrainExplosion");
                    AssertTrue(ReferenceEquals(supplied, IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer, supplied)),
                        "independent MotherBrainExplosion field edit stays supplied");
                }
            }
            CheckCeresBlastLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom MotherBrainExplosion");
            AssertTrue(ReferenceEquals(extra, IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated MotherBrainExplosion part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native MotherBrainExplosion OAM at visible and wrapped origins");
            }
        }
    }
    private static void CheckCeresBlastLoader(ushort pointer, SpriteVisualPart[] visual)
    {
        var definition = CeresDestructionSpriteDefinitions.Frames.FirstOrDefault(frame => frame.Pointer == pointer);
        if (definition.Name is null) return;
        var frames = CeresDestructionSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        frames[definition.Name] = visual;
        using var json = new MemoryStream();
        CeresDestructionSpritePresentation.Write(json, new() { Version = CeresDestructionSpriteFormat.Version, Frames = frames });
        json.Position = 0;
        var presentation = CeresDestructionSpritePresentation.Load(json);
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
