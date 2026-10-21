using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks game-over Baby animation steps, cry commands, and step domain against native data.</summary>
internal static class GameOverBabyAnimationVerification
{
    /// <summary>Traverses the serialized stream independently and compares 60 frame records and library-three sound handoffs.</summary>
    internal static void VerifyGameOverBabyAnimation(ISnesAddressSpace rom)
    {
        // Walk the original serialized stream; neither record count nor group lengths
        // nor generated successor pointers determine this oracle's traversal.
        var original = new List<Original>();
        int pointer = 0xbc27;
        while (true)
        {
            ushort duration = ReadVerificationWord(rom, 0x820000 | pointer);
            ushort sprite = ReadVerificationWord(rom, 0x820000 | (pointer + 2));
            ushort palette = ReadVerificationWord(rom, 0x820000 | (pointer + 4));
            int next = pointer + 6;
            ushort command = ReadVerificationWord(rom, 0x820000 | next);
            bool restart = command == 0xffff;
            ushort sound = 0;
            if (!restart && (command & 0x8000) != 0)
            {
                sound = command;
                next += 2;
            }
            original.Add(new Original((ushort)pointer, duration, sprite, palette, sound,
                (ushort)(restart ? 0xbc27 : next), restart));
            if (restart) break;
            pointer = next;
            AssertTrue(pointer < 0xbd95, "original Baby stream terminates at bounded marker");
        }
        AssertEqual(60, original.Count, "original Baby frame record count");
        AssertEqual(original.Count, GameOverBabyAnimationDefinitions.InstructionCount, "Baby count contract");
        AssertEqual(original.Count, GameOverBabyAnimation.StepCount, "Baby step count");
        AssertEqual(original[0].Pointer, GameOverBabyAnimationDefinitions.FirstPointer, "extraction walk starts at the first native record");
        GameOverBabyInstruction[] enumerated = GameOverBabyAnimation.All.ToArray();
        AssertEqual(original.Count, enumerated.Length, "Baby enumeration count");
        for (int index = 0; index < original.Count; index++)
        {
            var expected = original[index];
            // The native stream's record order is the step identity: record N of the walk is step N.
            GameOverBabyInstruction actual = GameOverBabyAnimation.Get(index);
            AssertEqual(index, actual.Step, "Baby step identity follows native record order");
            AssertEqual(expected.Duration, actual.Duration, "Baby duration field");
            AssertEqual(expected.Sprite, GameOverBabyAnimationDefinitions.NativeSpritemap(actual.Frame), "Baby frame field");
            AssertEqual(expected.Palette, GameOverBabyAnimationDefinitions.NativePalettePointer(actual.Palette), "Baby palette field");
            AssertEqual(original.FindIndex(row => row.Pointer == expected.Next), actual.NextStep, "Baby successor step");
            AssertEqual(expected.Restart, actual.RestartAfter, "Baby restart field");
            if (expected.Sound == 0)
                AssertEqual(GameOverBabySound.None, actual.SoundAfter, "Baby silent handoff");
            else
            {
                AssertEqual(expected.Sound, GameOverBabyAnimationDefinitions.NativeSoundOpcode(actual.SoundAfter), "Baby sound handoff");
                SoundEffectId sound = GameOverRomData.BabyAnimation.ResolveCry(expected.Sound);
                AssertEqual((byte)0xa9, GameOverBabyAnimationRomByte(rom, 0x820000 | expected.Sound), "native Baby cry LDA immediate");
                AssertEqual((ushort)sound.Value, ReadVerificationWord(rom, 0x820001 + expected.Sound), "native Baby cry effect");
                AssertEqual(SoundEffectLibrary.Library3, sound.Library, "Baby cry library");
                AssertEqual((byte)0x22, GameOverBabyAnimationRomByte(rom, 0x820003 + expected.Sound), "native Baby cry JSL");
                AssertEqual((ushort)0x914d, ReadVerificationWord(rom, 0x820004 + expected.Sound), "native library3 queue address");
                AssertEqual((byte)0x80, GameOverBabyAnimationRomByte(rom, 0x820006 + expected.Sound), "native library3 queue bank");
            }
            AssertEqual(actual, enumerated[index], "Baby lazy enumeration order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, original.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOverBabyAnimation.Get(invalid), "unsupported Baby step");
    }

    /// <summary>Reads one native animation byte solely as the game-over Baby oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for game over Baby animation")]
    private static byte GameOverBabyAnimationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
