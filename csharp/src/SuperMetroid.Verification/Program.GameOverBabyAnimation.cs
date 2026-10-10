using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares all Baby animation records and cry handoffs with the serialized cartridge stream and rejects unsupported frame pointers.</summary>
    /// <param name="rom">Cartridge address space containing the native animation records and sound routines.</param>
    private static void VerifyGameOverBabyAnimation(ISnesAddressSpace rom)
    {
        // Walk the original serialized stream; neither record count nor group lengths
        // nor generated successor pointers determine this oracle's traversal.
        var original = new List<(ushort Pointer, ushort Duration, ushort Sprite, ushort Palette,
            ushort Sound, ushort Next, bool Restart)>();
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
            original.Add(((ushort)pointer, duration, sprite, palette, sound,
                (ushort)(restart ? 0xbc27 : next), restart));
            if (restart) break;
            pointer = next;
            AssertTrue(pointer < 0xbd95, "original Baby stream terminates at bounded marker");
        }
        AssertEqual(60, original.Count, "original Baby frame record count");
        AssertEqual(original.Count, GameOverBabyAnimationDefinitions.InstructionCount, "Baby count contract");
        GameOverBabyInstruction[] enumerated = GameOverBabyAnimationDefinitions.All.ToArray();
        AssertEqual(original.Count, enumerated.Length, "Baby enumeration count");
        for (int index = 0; index < original.Count; index++)
        {
            var expected = original[index];
            GameOverBabyInstruction actual = GameOverBabyAnimationDefinitions.Get(expected.Pointer);
            AssertEqual(expected.Duration, actual.Duration, "Baby duration field");
            AssertEqual(expected.Sprite, GameOverBabyAnimationDefinitions.NativeSpritemap(actual.Frame), "Baby frame field");
            AssertEqual(expected.Palette, GameOverBabyAnimationDefinitions.NativePalettePointer(actual.Palette), "Baby palette field");
            AssertEqual(expected.Next, actual.NextPointer, "Baby successor field");
            AssertEqual(expected.Restart, actual.RestartAfter, "Baby restart field");
            AssertEqual(expected.Pointer, actual.Pointer, "Baby pointer identity");
            if (expected.Sound == 0)
                AssertEqual(GameOverBabySound.None, actual.SoundAfter, "Baby silent handoff");
            else
            {
                AssertEqual(expected.Sound, GameOverBabyAnimationDefinitions.NativeSoundOpcode(actual.SoundAfter), "Baby sound handoff");
                SoundEffectId sound = GameOverRomData.BabyAnimation.ResolveCry(expected.Sound);
                AssertEqual((byte)0xa9, rom.ReadByte(0x820000 | expected.Sound), "native Baby cry LDA immediate");
                AssertEqual((ushort)sound.Value, ReadVerificationWord(rom, 0x820001 + expected.Sound), "native Baby cry effect");
                AssertEqual(SoundEffectLibrary.Library3, sound.Library, "Baby cry library");
                AssertEqual((byte)0x22, rom.ReadByte(0x820003 + expected.Sound), "native Baby cry JSL");
                AssertEqual((ushort)0x914d, ReadVerificationWord(rom, 0x820004 + expected.Sound), "native library3 queue address");
                AssertEqual((byte)0x80, rom.ReadByte(0x820006 + expected.Sound), "native library3 queue bank");
            }
            AssertEqual(actual, enumerated[index], "Baby lazy enumeration order");
        }
        var valid = original.Select(row => row.Pointer).ToHashSet();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort candidate = (ushort)raw;
            if (!valid.Contains(candidate))
                AssertThrows<InvalidDataException>(() => GameOverBabyAnimationDefinitions.Get(candidate), "unsupported Baby pointer");
        }
    }
}
