using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameOverTextSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 0, 0xa0, stream => stream.SourcePointer, "source"));

    private static void VerifyGameOverTextDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 3, 0xa2, stream => stream.DestinationByteOffset, "destination"));

    private static void VerifyGameOverTextField(ISnesAddressSpace rom, int fieldOffset,
        byte opcode, Func<GameOverTextStream, int> field, string name)
    {
        GameOverTextStream[] enumerated = GameOverRomData.Text.All.ToArray();
        AssertEqual(5, enumerated.Length, "native game-over text load count");
        AssertEqual(5, GameOverRomData.Text.Count, "bounded game-over text count");
        for (int index = 0; index < 5; index++)
        {
            int instruction = 0x819206 + index * 9 + fieldOffset;
            AssertEqual(opcode, rom.ReadByte(instruction), $"native text {name} immediate opcode");
            int expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, field(GameOverRomData.Text.Get((GameOverTextElement)index)),
                $"game-over text {name} selector {index}");
            AssertEqual(expected, field(enumerated[index]), $"game-over text {name} enumeration {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, 65536, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameOverRomData.Text.Get((GameOverTextElement)invalid),
                $"invalid game-over text {name} selector {invalid}");
    }
}
