using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks statue earthquake sound ordering and loop restart against the native bank-$88 list.</summary>
internal static class LookupQuakeSoundSelectionVerification
{
    /// <summary>
    /// Compares all eight rumble IDs, the reset marker, library-two queue limit, and the rhythm position after each
    /// emission, the last returning to the first.
    /// </summary>
    internal static void VerifyQuakeSoundSelection(ISnesAddressSpace rom)
    {
        int count = 0;
        while (unchecked((short)ReadVerificationWord(rom, 0x88b256 + 4 * count)) >= 0)
            AssertTrue(++count <= 8, "Native rumble list terminates within eight emissions");
        AssertEqual(8, count, "Eight original sound identities before loop marker");
        AssertEqual((ushort)0x8000, ReadVerificationWord(rom, 0x88b256 + 4 * count), "Original reset marker");
        for (int index = 0; index < count; index++)
        {
            var sounds = SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone();
            var state = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.RoomFxOverlayState>([typeof(global::SuperMetroid.Core.Audio.GameplaySoundRequests)], new object?[] { sounds });
            state.SetEarthquakeSoundRhythmPositionForCheck(index);
            state.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame);
            AssertEqual(1, sounds.SoundRequests.Count, "Each original selection emits exactly one rumble");
            var sound = sounds.SoundRequests[0];
            AssertEqual(SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, ReadVerificationWord(rom, 0x88b256 + 4 * index)),
                sound.SoundEffect, "Original sound, decoded by the import, including terminator restart");
            AssertEqual(SoundEffectLibrary.Enemies, sound.SoundEffect.Library, "Earthquake sounds are library-two sounds");
            AssertEqual(SoundRequestPriority.Normal, sound.Priority, "Earthquake sounds are requested at normal priority");
            AssertEqual((index + 1) % count, state.EarthquakeSoundRhythm.Position,
                "Next chronological position after selection, the last returning to the first as the reset marker does");
        }
    }
}
