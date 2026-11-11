using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending reward actor bytecode and the extracted Samus sprite compositions against cartridge data.</summary>
internal static class EndingRewardInstructionsVerification
{
    /// <summary>Compares all 37 reward artwork identities and their compositions with the cartridge.</summary>
    internal static void VerifyEndingRewardInstructions(ISnesAddressSpace bus)
    {
        // Original display operands select catalog frames independently of native storage order.
        ushort[] operands = [
            0xed1f, 0xed27, 0xed33, 0xed37, 0xed3b, 0xed3f, 0xed43, 0xed47,
            0xed4b, 0xed4f, 0xed53, 0xed85, 0xed8f, 0xed97, 0xed9f, 0xeda3,
            0xeda7, 0xedb3, 0xedbb, 0xedc3, 0xedcf, 0xedd9, 0xeddd, 0xede1,
            0xede5, 0xede9, 0xeded, 0xedf1, 0xedf5, 0xee1b, 0xee1f, 0xee23,
            0xee2b, 0xee37, 0xee3f, 0xee47, 0xee4f,
        ];
        string[] names = [
            "suitless-idle-upper", "suitless-lower", "suitless-hair-1", "suitless-hair-2",
            "suitless-hair-3", "suitless-hair-4", "suitless-hair-5", "suitless-hair-6",
            "suitless-hair-7", "suitless-hair-8", "suitless-standing", "suitless-prepare-jump",
            "suitless-jumping", "samus-falling", "samus-landing", "samus-landed",
            "samus-shooting", "suited-idle-body", "suited-helmet-head", "helmetless-head-1",
            "suited-headless-body", "suited-arm-1", "suited-arm-2", "suited-arm-3",
            "suited-arm-4", "suited-arm-5", "suited-arm-6", "suited-arm-7",
            "suited-arm-8", "helmetless-head-2", "helmetless-head-3", "helmetless-head-4",
            "suited-prepare-jump", "suited-jumping", "suited-helmet-jump-head-1", "suited-helmet-jump-head-2",
            "helmetless-jump-head",
        ];
        var catalog = EndingRewardSpriteDefinitions.Frames;
        AssertEqual(37, catalog.Count, "reward frame catalog count");
        for (int i = 0; i < catalog.Count; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int header = 0x8c0000 | pointer;
            int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(pointer, CinematicSpritemapSources.Of(catalog[i].Pose), "reward original frame pointer");
            AssertEqual(count, catalog[i].StockPartCount, "reward original OAM part count");
            AssertEqual(names[i], catalog[i].Name, "reward published artwork key");
            VerifyEndingRewardComposition(bus, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "reward catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 37, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[invalid], "reward catalog bounds");
    }

    /// <summary>Checks a reward frame's extracted composition draws the native OAM and clipping.</summary>
    private static void VerifyEndingRewardComposition(ISnesAddressSpace bus, CinematicSpriteFrameDefinition definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus,
            CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var compiledOam = new OamBuffer();
            originalOam.BeginFrame(); compiledOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0800, originIsOnScreen: y == 72);
            if (y == 72) supplied.DrawOnScreen(compiledOam, new(120, y), 0x0800);
            else supplied.DrawOffScreen(compiledOam, new(120, y), 0x0800);
            originalOam.FinalizeFrame(); compiledOam.FinalizeFrame();
            AssertTrue(originalOam.ImageEquals(compiledOam),
                "reward composition preserves native OAM and clipping");
        }
    }
}
