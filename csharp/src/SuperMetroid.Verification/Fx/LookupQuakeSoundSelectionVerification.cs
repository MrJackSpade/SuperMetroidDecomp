using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks statue earthquake sound ordering and loop restart against the native bank-$88 list.</summary>
internal static class LookupQuakeSoundSelectionVerification
{
    /// <summary>Compares all eight rumble IDs, the reset marker, library-two queue limit, and sequence index after each emission.</summary>
    internal static void VerifyQuakeSoundSelection(ISnesAddressSpace rom)
    {
        int count = 0;
        while (unchecked((short)ReadVerificationWord(rom, 0x88b256 + 4 * count)) >= 0)
            AssertTrue(++count <= 8, "Native rumble list terminates within eight emissions");
        AssertEqual(8, count, "Eight original sound identities before loop marker");
        AssertEqual((ushort)0x8000, ReadVerificationWord(rom, 0x88b256 + 4 * count), "Original reset marker");
        for (int index = 0; index <= count; index++)
        {
            var state = new RoomLayer3FxState();
            state.earthquakeSoundSequenceIndex = index;
            state.PublishStatueEarthquakeSound(0, mainGameLoopCarry: true);
            AssertEqual(1, state.SoundRequests.Count, "Each original selection emits exactly one rumble");
            var sound = state.SoundRequests[0];
            int originalIndex = index == count ? 0 : index;
            AssertEqual(ReadVerificationWord(rom, 0x88b256 + 4 * originalIndex),
                (ushort)sound.SoundEffect.Value, "Original sound identity including terminator restart");
            AssertEqual(SoundEffectLibrary.Library2, sound.SoundEffect.Library, "Native QueueSound_Lib2_Max6 library");
            AssertEqual((byte)6, sound.MaximumQueued, "Native queue capacity");
            AssertEqual(originalIndex + 1, state.earthquakeSoundSequenceIndex, "Next chronological index after selection");
        }
    }
}
