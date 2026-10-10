using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks Ceres destruction frame identities, part counts, order, and index bounds against native ROM data.</summary>
    /// <param name="rom">Address space containing the supported cartridge's backdrop and sprite-list pointers.</param>
    private static void VerifyCeresDestructionFrameCatalog(ISnesAddressSpace rom)
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
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            int record = 0x8c0000 | pointer;
            int count = rom.ReadByte(record) | rom.ReadByte(record + 1) << 8;
            string name = index < 7 ? backdropNames[index] : $"{(index < 13 ? "small" : index < 17 ? "large" : "station")}-blast-{frame}";
            AssertEqual(pointer, frames[index].Pointer, "native destruction frame operand");
            AssertEqual(count, frames[index].StockPartCount, "native destruction part count");
            AssertEqual(name, frames[index].Name, "published destruction key");
            AssertEqual(frames[index], enumerated[index], "destruction enumeration order");
        }
        foreach (int invalid in new[] { -1, frames.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = frames[invalid], "destruction catalog bounds");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.SmallFrame(invalid), "small blast bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.StationFrame(invalid), "station blast bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.StationPartCount(invalid), "station count bounds");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionSpriteDefinitions.LargeFrame(invalid), "large blast bounds");
    }
}
