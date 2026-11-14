using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the Ceres destruction sprite catalog's names, frame
/// pointers, part counts, ordering, and index bounds against native operands.</summary>
internal static class CeresDestructionFrameCatalogVerification
{
    /// <summary>Compares all twenty-four backdrop, blast and subtitle frames with
    /// cartridge definitions and rejects indexes outside each frame family.</summary>
    internal static void VerifyCeresDestructionFrameCatalog(ISnesAddressSpace rom)
    {
        int[] backdropOperands = [0xcc41, 0xccad, 0xccc3, 0xcd85, 0xcd8d, 0xcd95, 0xcd9d];
        string[] backdropNames = ["station-under-attack-large-asteroid", "planet-zebes", "planet-zebes-title",
            "zebes-stars-upper-left", "zebes-stars-upper-right", "zebes-stars-lower-left", "zebes-stars-lower-right"];
        // The Japanese subtitle's one pose, which its list ($8B:CCD5) holds for 192 updates (#1733).
        const int subtitleOperand = 0xccd7, subtitle = 23;
        var frames = CeresDestructionSpriteFrameDefinitions.Frames;
        AssertEqual(24, frames.Count, "destruction catalog count");
        var enumerated = frames.ToArray();
        for (int index = 0; index < frames.Count; index++)
        {
            int frame = index < 7 ? index : index < 13 ? index - 7 : index < 17 ? index - 13 : index - 17;
            int operand = index == subtitle ? subtitleOperand
                : index < 7 ? backdropOperands[index] : (index < 13 ? 0xccdd : index < 17 ? 0xcd21 : 0xce1d) + 4 * frame;
            operand |= 0x8b0000;
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(operand);
            int record = 0x8c0000 | pointer;
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(record);
            string name = index == subtitle ? "planet-zebes-subtitle"
                : index < 7 ? backdropNames[index] : $"{(index < 13 ? "small" : index < 17 ? "large" : "station")}-blast-{frame}";
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[index].Pose), "native destruction frame operand");
            AssertEqual(count, frames[index].StockPartCount, "native destruction part count");
            AssertEqual(name, frames[index].Name, "published destruction key");
            AssertEqual(frames[index], enumerated[index], "destruction enumeration order");
        }
        foreach (int invalid in new[] { -1, frames.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = frames[invalid], "destruction catalog bounds");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpritePoses.StationFrame(invalid), "station blast bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpritePoses.StationPartCount(invalid), "station count bounds");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpritePoses.LargeFrame(invalid), "large blast bounds");
    }
}
