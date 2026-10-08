using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyQuakeSoundSelection(ISnesAddressSpace rom)
    {
        int count = 0;
        while (unchecked((short)ReadVerificationWord(rom, 0x88b256 + 4 * count)) >= 0)
            AssertTrue(++count <= 8, "Native rumble list terminates within eight emissions");
        AssertEqual(8, count, "Eight original sound identities before loop marker");
        AssertEqual((ushort)0x8000, ReadVerificationWord(rom, 0x88b256 + 4 * count), "Original reset marker");
        FieldInfo indexField = typeof(RoomLayer3FxState).GetField("earthquakeSoundSequenceIndex",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int index = 0; index <= count; index++)
        {
            var state = new RoomLayer3FxState();
            indexField.SetValue(state, index);
            state.PublishStatueEarthquakeSound(0, mainGameLoopCarry: true);
            AssertEqual(1, state.SoundRequests.Count, "Each original selection emits exactly one rumble");
            var sound = state.SoundRequests[0];
            int originalIndex = index == count ? 0 : index;
            AssertEqual(ReadVerificationWord(rom, 0x88b256 + 4 * originalIndex),
                (ushort)sound.SoundEffect.Value, "Original sound identity including terminator restart");
            AssertEqual(SoundEffectLibrary.Library2, sound.SoundEffect.Library, "Native QueueSound_Lib2_Max6 library");
            AssertEqual((byte)6, sound.MaximumQueued, "Native queue capacity");
            AssertEqual(originalIndex + 1, (int)indexField.GetValue(state)!, "Next chronological index after selection");
        }
    }
}
