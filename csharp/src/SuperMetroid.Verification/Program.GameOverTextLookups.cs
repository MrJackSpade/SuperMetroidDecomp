using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that the five game-over text source pointers match their native ROM load instructions.</summary>
    /// <param name="rom">The SNES address space containing the native instruction stream.</param>
    private static void VerifyGameOverTextSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 0, 0xa0, stream => stream.SourcePointer, "source"));

    /// <summary>Checks that the five game-over text destination offsets match their native ROM load instructions.</summary>
    /// <param name="rom">The SNES address space containing the native instruction stream.</param>
    private static void VerifyGameOverTextDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 3, 0xa2, stream => stream.DestinationByteOffset, "destination"));

    /// <summary>Compares one selector field from each bounded text stream with the corresponding native immediate operand.</summary>
    /// <param name="rom">The SNES address space containing the native text-load instructions.</param>
    /// <param name="fieldOffset">Byte offset of the selected operand within each nine-byte instruction.</param>
    /// <param name="opcode">Expected immediate-load opcode preceding the operand.</param>
    /// <param name="field">Selects the source pointer or destination offset from a text-stream definition.</param>
    /// <param name="name">Field label used to distinguish assertion messages.</param>
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
