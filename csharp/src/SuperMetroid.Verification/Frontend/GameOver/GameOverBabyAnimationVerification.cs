using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Frontend;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks the game-over Baby's animation, its looks, durations, cries and loop against native data.</summary>
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
        AssertEqual(original[0].Pointer, GameOverBabyAnimationDefinitions.FirstPointer, "extraction walk starts at the first native record");
        IReadOnlyList<AnimationBeat<GameOverBabyLook, GameOverBabySound>> beats = GameOverBabyAnimation.Segments.Beats(GameOverBabySegment.Crying);
        int beat = 0;
        for (int index = 0; index < original.Count; index++)
        {
            var expected = original[index];
            // The native stream's record order is the animation's order: each record is the next look, then its cry.
            AssertEqual(index == original.Count - 1 ? original[0].Pointer : original[index + 1].Pointer, expected.Next, "Baby records play in stream order");
            var look = AssertIs<AnimationBeat<GameOverBabyLook, GameOverBabySound>.Frame>(beats[beat++], $"Baby record {index} is a look");
            AssertEqual(expected.Duration, look.Duration, "Baby duration field");
            AssertEqual(expected.Sprite, GameOverBabyAnimationDefinitions.NativeSpritemap(look.Shown.Frame), "Baby frame field");
            AssertEqual(expected.Palette, GameOverBabyAnimationDefinitions.NativePalettePointer(look.Shown.Colours), "Baby palette field");
            if (expected.Sound == 0)
            {
                AssertTrue(beat == beats.Count || beats[beat] is AnimationBeat<GameOverBabyLook, GameOverBabySound>.Frame, "Baby silent handoff");
                continue;
            }
            var cry = AssertIs<AnimationBeat<GameOverBabyLook, GameOverBabySound>.Event>(beats[beat++], $"Baby record {index} is followed by its cry");
            AssertEqual(expected.Sound, GameOverBabyAnimationDefinitions.NativeSoundOpcode(cry.Raised), "Baby sound handoff");
            SoundEffectId sound = GameOverBabyCries.ResolveCry(expected.Sound);
            AssertEqual((byte)0xa9, GameOverBabyAnimationRomByte(rom, 0x820000 | expected.Sound), "native Baby cry LDA immediate");
            AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Room, ReadVerificationWord(rom, 0x820001 + expected.Sound)), sound,
                "the import decodes the native Baby cry effect to the cry");
            AssertEqual(SoundEffectLibrary.Room, sound.Library, "Baby cry library");
            AssertEqual((byte)0x22, GameOverBabyAnimationRomByte(rom, 0x820003 + expected.Sound), "native Baby cry JSL");
            AssertEqual((ushort)0x914d, ReadVerificationWord(rom, 0x820004 + expected.Sound), "native library3 queue address");
            AssertEqual((byte)0x80, GameOverBabyAnimationRomByte(rom, 0x820006 + expected.Sound), "native library3 queue bank");
        }
        AssertEqual(beats.Count, beat, "every beat of the Baby's animation is a native record or its cry");
        AssertTrue(original[^1].Restart, "the native stream ends with its restart marker");
        AssertEqual(AnimationEnd.LoopsFrom(0), GameOverBabyAnimation.Playing.End, "the Baby's animation starts again after its last look, as the native restart does");
        AssertEqual(1, GameOverBabyAnimation.Playing.Phases.Count, "the Baby's animation plays its looks once through before starting again");
    }

    /// <summary>Reads one native animation byte solely as the game-over Baby oracle.</summary>
    private static byte GameOverBabyAnimationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
