using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #2603: the boss sounds that play an instrument only their room's music data set defines install audible, as
/// the cartridge plays them with that data set loaded. Phantoon's three appearance sounds play in his room and
/// Crocomire's cry and melting cry in his; the shared sounds leave that instrument empty, so a render from them alone was
/// silent where the sound keys it. Each installed clip holds sound and is the driver's render of the request with the
/// room state's own data set uploaded after the shared sounds.
/// </summary>
internal static class RoomMusicSoundClipVerification
{
    /// <summary>Checks each sound's installed clip against its render in the music data set its room state loads.</summary>
    internal static void VerifyRoomMusicSoundClips()
    {
        ExtractedAudioAssetCatalog catalog = InstalledFixtures.Audio;
        ClipAudioLibrary clips = InstalledFixtures.AudioClips;
        (SoundEffectId Sound, RoomStateSettings Room)[] sounds =
        [
            .. PhantoonSoundDefinitions.MaterializationSounds.Select(sound => (sound, WreckedShipRoomStateDefinitions.PhantoonDefault)),
            (CrocomireSoundDefinitions.Cry, NorfairRoomStateDefinitions.CrocomireDefault),
            (CrocomireSoundDefinitions.FirstMeltDissolve, NorfairRoomStateDefinitions.CrocomireDefault),
        ];
        foreach ((SoundEffectId sound, RoomStateSettings room) in sounds)
        {
            MusicDataSet dataSet = room.Music.DataSet
                ?? throw new InvalidOperationException($"{room.Name} loads no music data set of its own.");
            if (clips.GetSoundEffect(sound).Shape is not OneShotSoundEffectClip { Clip: AudioClipFile clip } shot)
                throw new InvalidOperationException($"{sound} installs no one-shot clip, but the cartridge plays it audibly in {room.Name}.");
            short[] installed = clips.ReadClip(clip);
            AssertTrue(Array.Exists(installed, sample => sample != 0), $"{sound} is audible in {room.Name}");
            SoundEffectRendering.OneShot inRoom = SoundEffectRendering.RenderOneShot(
                catalog, AudioAssetCatalogDefinitions.BankOf(dataSet), sound.Library, SoundEffectCommands.Command(sound));
            AssertSequenceEqual(inRoom.Clip, installed, $"{sound} installs its render with {dataSet} loaded");
            AssertEqual(inRoom.ActiveFrames, shot.ActiveFrames, $"{sound} occupies its library as long as its render with {dataSet} loaded");
        }
        Console.WriteLine(
            $"  Room music sounds: {sounds.Length} boss sounds install audible, each rendered with its room's music data set loaded.");
    }
}
