using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the Ceres destruction sprite catalog's names, frame
/// pointers, part counts, ordering, and index bounds against native operands.</summary>
internal static class CeresDestructionFrameCatalogVerification
{
    /// <summary>Compares all twenty-three backdrop and blast frames with
    /// cartridge definitions and rejects indexes outside each frame family.</summary>
    internal static void VerifyCeresDestructionFrameCatalog(ISnesAddressSpace rom)
    {
        int[] backdropOperands = [0xcc41, 0xccad, 0xccc3, 0xcd85, 0xcd8d, 0xcd95, 0xcd9d];
        string[] backdropNames = ["station-under-attack-large-asteroid", "planet-zebes", "planet-zebes-title",
            "zebes-stars-upper-left", "zebes-stars-upper-right", "zebes-stars-lower-left", "zebes-stars-lower-right"];
        var frames = CeresDestructionSpriteDefinitions.Frames;
        AssertEqual(23, frames.Count, "destruction catalog count");
        var enumerated = frames.ToArray();
        for (int index = 0; index < frames.Count; index++)
        {
            int frame = index < 7 ? index : index < 13 ? index - 7 : index < 17 ? index - 13 : index - 17;
            int operand = index < 7 ? backdropOperands[index] : (index < 13 ? 0xccdd : index < 17 ? 0xcd21 : 0xce1d) + 4 * frame;
            operand |= 0x8b0000;
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(operand);
            int record = 0x8c0000 | pointer;
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(record);
            string name = index < 7 ? backdropNames[index] : $"{(index < 13 ? "small" : index < 17 ? "large" : "station")}-blast-{frame}";
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[index].Pose), "native destruction frame operand");
            AssertEqual(count, frames[index].StockPartCount, "native destruction part count");
            AssertEqual(name, frames[index].Name, "published destruction key");
            AssertEqual(frames[index], enumerated[index], "destruction enumeration order");
        }
        foreach (int invalid in new[] { -1, frames.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = frames[invalid], "destruction catalog bounds");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.StationFrame(invalid), "station blast bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.StationPartCount(invalid), "station count bounds");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.LargeFrame(invalid), "large blast bounds");
    }
}
