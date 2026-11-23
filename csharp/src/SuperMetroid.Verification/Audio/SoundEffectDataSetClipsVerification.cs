using SuperMetroid.Core.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #2645, #2648, #2650 and #2652: a sound effect plays with the instruments the loaded music data set holds. Each
/// data set redefines instruments past the shared sounds', and the game plays an enemy's or a room's sound with the set's
/// own: Kraid's roar and dying cry in his fight's set, lava's bubbling and the Funes' spit in Norfair's, the Etecoons'
/// sounds in Brinstar's. The port played each with the shared sounds' instrument in every room, which sounded like the
/// Metroid, or nothing. The audio library hands the backend each such sound's own clip for the loaded set.
/// </summary>
internal static class SoundEffectDataSetClipsVerification
{
    /// <summary>The reported sounds, each with a data set it plays in.</summary>
    private static readonly (SoundEffectId Sound, MusicDataSet DataSet)[] Reported =
    [
        (EnemySound.Roar, MusicDataSet.KraidAndPhantoonBattles),
        (EnemySound.KraidDyingCry, MusicDataSet.KraidAndPhantoonBattles),
        (EnemySound.LavaBubblingRising, MusicDataSet.UpperNorfair),
        (EnemySound.LavaBubblingFalling, MusicDataSet.UpperNorfair),
        (EnemySound.LavaBubblingBurst, MusicDataSet.UpperNorfair),
        (EnemySound.HighSpit, MusicDataSet.UpperNorfair),
        (EnemySound.HighSpit, MusicDataSet.LowerNorfair),
        (EnemySound.EtecoonCry, MusicDataSet.GreenBrinstar),
        (EnemySound.EtecoonWallJump, MusicDataSet.GreenBrinstar),
        (EnemySound.EtecoonTheme, MusicDataSet.GreenBrinstar),
    ];

    /// <summary>Checks that each reported sound plays its own clip in its set, not the one the shared sounds give it.</summary>
    internal static void VerifySoundEffectsPlayTheDataSetsInstruments()
    {
        ClipAudioLibrary clips = InstalledFixtures.AudioClips;
        foreach ((SoundEffectId sound, MusicDataSet dataSet) in Reported)
        {
            SoundEffectClipEntry own = clips.GetSoundEffect(sound);
            SoundEffectClipEntry inSet = clips.GetSoundEffect(dataSet, sound);
            AssertTrue(!ReferenceEquals(own, inSet) && inSet.Shape != own.Shape,
                $"{sound} plays its own clip with {dataSet} loaded, not the shared sounds' instrument's");
        }
    }

    /// <summary>Runs the data-set sound clip check.</summary>
    [VerificationCommand("--sound-effect-data-set-clips")]
    internal static int SoundEffectDataSetClipsCommand()
    {
        Suite(nameof(VerifySoundEffectsPlayTheDataSetsInstruments), VerifySoundEffectsPlayTheDataSetsInstruments);
        return 0;
    }
}
