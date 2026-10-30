using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Frontend.GameOver;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks the five compiled game-over text transfers against their native source and destination immediates.</summary>
internal static class GameOverTextLookupsVerification
{
    /// <summary>Checks source pointers in the five $A0 load instructions.</summary>
    internal static void VerifyGameOverTextSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 0, 0xa0, stream => stream.SourcePointer, "source"));

    /// <summary>Checks destination offsets in the five $A2 load instructions.</summary>
    internal static void VerifyGameOverTextDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyGameOverTextField), () => VerifyGameOverTextField(rom, 3, 0xa2, stream => (stream.Origin.Y * GameOverPageDefinitions.TilemapWidth + stream.Origin.X) * sizeof(ushort), "destination"));

    /// <summary>Compares selector and enumeration order with retail immediates and rejects invalid selectors.</summary>
    private static void VerifyGameOverTextField(ISnesAddressSpace rom, int fieldOffset,
        byte opcode, Func<GameOverTextStream, int> field, string name)
    {
        GameOverTextStream[] enumerated = GameOverTexts.All.ToArray();
        AssertEqual(5, enumerated.Length, "native game-over text load count");
        for (int index = 0; index < 5; index++)
        {
            int instruction = 0x819206 + index * 9 + fieldOffset;
            AssertEqual(opcode, GameOverTextFieldRomByte(rom, instruction), $"native text {name} immediate opcode");
            int expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, field(GameOverTexts.Get((GameOverTextElement)index)),
                $"game-over text {name} selector {index}");
            AssertEqual(expected, field(enumerated[index]), $"game-over text {name} enumeration {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOverTexts.Get((GameOverTextElement)invalid),
                $"invalid game-over text {name} selector {invalid}");
    }

    /// <summary>Reads a native instruction byte only for the cartridge oracle.</summary>
    private static byte GameOverTextFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
