using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks statue earthquake sound ordering and loop restart against the native bank-$88 list.</summary>
internal static class LookupQuakeSoundSelectionVerification
{
    /// <summary>
    /// Compares all eight rumble IDs and the reset marker with the rumbles one statue earthquake plays, update after update,
    /// and checks that each rumble waits the next delay of the sound's rhythm before the one after it, the last returning
    /// to the first as the reset marker does.
    /// </summary>
    internal static void VerifyQuakeSoundSelection(ISnesAddressSpace rom)
    {
        int count = 0;
        while (unchecked((short)ReadVerificationWord(rom, 0x88b256 + 4 * count)) >= 0)
            AssertTrue(++count <= 8, "Native rumble list terminates within eight emissions");
        AssertEqual(8, count, "Eight original sound identities before loop marker");
        AssertEqual((ushort)0x8000, ReadVerificationWord(rom, 0x88b256 + 4 * count), "Original reset marker");
        IReadOnlyList<int> delays = RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays;
        AssertEqual(count, delays.Count, "the sound's rhythm has a delay for each original rumble");

        // One statue earthquake plays its rumbles on the updates its countdown runs past zero; with the random number at
        // zero and no power-on delay, each waits only its rhythm's delay. An update after the longest delay, its most
        // jitter and the update it runs past zero on with no rumble is a rhythm that has stopped.
        var sounds = SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone();
        RoomFxOverlayState earthquake = RoomEffectFixtures.Rain(sounds);
        int longestWait = delays.Max() + RoomEarthquakeDefinitions.RisingLiquidSoundJitter.Outcomes + 1;
        var rumbleUpdates = new List<int>();
        for (int update = 0, quiet = 0; rumbleUpdates.Count < count + 2; update++)
        {
            int before = sounds.SoundRequests.Count;
            earthquake.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None);
            if (sounds.SoundRequests.Count == before)
            {
                AssertTrue(++quiet <= longestWait, $"a rumble follows within {longestWait} updates of the one before");
                continue;
            }
            quiet = 0;
            AssertEqual(before + 1, sounds.SoundRequests.Count, "each rumble is one sound");
            int rumble = rumbleUpdates.Count;
            rumbleUpdates.Add(update);
            var sound = sounds.SoundRequests[before];
            AssertEqual(SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, ReadVerificationWord(rom, 0x88b256 + 4 * (rumble % count))),
                sound.SoundEffect, $"rumble {rumble}: the original sound, decoded by the import, including terminator restart");
            AssertEqual(SoundEffectLibrary.Enemies, sound.SoundEffect.Library, "Earthquake sounds are library-two sounds");
            AssertEqual(SoundRequestPriority.Normal, sound.Priority, "Earthquake sounds are requested at normal priority");
        }
        // Each rumble waits its own delay of the rhythm, in order, past the same fixed updates of the countdown; after the
        // last delay the rhythm starts again from the first.
        int countdownUpdates = rumbleUpdates[1] - rumbleUpdates[0] - delays[0];
        for (int rumble = 0; rumble <= count; rumble++)
        {
            AssertEqual(delays[rumble % count] + countdownUpdates, rumbleUpdates[rumble + 1] - rumbleUpdates[rumble],
                $"the rumble after rumble {rumble} waits the rhythm's delay {rumble % count}");
        }
    }
}
