using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents game over text lookups verification.</summary>
internal static class GameOverTextLookupsVerification
{
    /// <summary>Verifies game over text sources.</summary>
    internal static void VerifyGameOverTextSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 0, 0xa0, stream => stream.SourcePointer, "source"));

    /// <summary>Verifies game over text destinations.</summary>
    internal static void VerifyGameOverTextDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 3, 0xa2, stream => stream.DestinationByteOffset, "destination"));

    /// <summary>Verifies game over text field.</summary>
    private static void VerifyGameOverTextField(ISnesAddressSpace rom, int fieldOffset,
        byte opcode, Func<GameOverTextStream, int> field, string name)
    {
        GameOverTextStream[] enumerated = GameOverRomData.Text.All.ToArray();
        AssertEqual(5, enumerated.Length, "native game-over text load count");
        AssertEqual(5, GameOverRomData.Text.Count, "bounded game-over text count");
        for (int index = 0; index < 5; index++)
        {
            int instruction = 0x819206 + index * 9 + fieldOffset;
            AssertEqual(opcode, GameOverTextFieldRomByte(rom, instruction), $"native text {name} immediate opcode");
            int expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, field(GameOverRomData.Text.Get((GameOverTextElement)index)),
                $"game-over text {name} selector {index}");
            AssertEqual(expected, field(enumerated[index]), $"game-over text {name} enumeration {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, 65536, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameOverRomData.Text.Get((GameOverTextElement)invalid),
                $"invalid game-over text {name} selector {invalid}");
    }

    /// <summary>Game over text field rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for game over text field")]
    private static byte GameOverTextFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
