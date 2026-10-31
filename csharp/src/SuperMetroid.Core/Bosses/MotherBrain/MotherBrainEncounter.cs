using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain encounter state shared across Tourian: the bank-$A9 state extension, the end of
/// the tube effects the elevator observes, and the pages of artwork both Mother Brain and the
/// cutscene Baby show their sprites.
/// </summary>
[RoomLifetime]
internal sealed class MotherBrainEncounter
{
    /// <summary>Sprite presentation owner whose frames picture the pages the encounter shows.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Receives the encounter's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the encounter's page selections to sprite presentation and its requests to the enemy audio queue.</summary>
    internal MotherBrainEncounter(EnemyPresentation presentation, EnemyAudioQueue audio)
    {
        _presentation = presentation;
        _audio = audio;
    }

    /// <summary>The typed multipart encounter while Mother Brain's retail population is loaded.</summary>
    public MotherBrainEnemyState? MotherBrain { get; internal set; }

    /// <summary>
    /// Shows <paramref name="page"/> to the sprites on this update (#1406): from now on the frames drawn over its characters
    /// picture what it holds.
    /// </summary>
    internal void ShowSpritePage(MotherBrainSpritePage page) =>
        _presentation.ShowLook(MotherBrainCharacterSources.Of(page));

    /// <summary>
    /// Shows every row of the rotting corpse as rot step <paramref name="step"/> left it (#1392, #1406): the corpse's sheet
    /// pictures each step, so the room records only the step its rows show.
    /// </summary>
    internal void ShowCorpseRotStep(int step)
    {
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RotRows.Count; row++)
            _presentation.ShowRotRow(row, step);
    }

    /// <summary>
    /// Requests a sound of the body's fight, fake death and tube collapse, or of the Baby Metroid's cries in the
    /// cutscene, at the end of the enemy update. A later sound from this source in the same update replaces it: the
    /// update-end rule of <see cref="EnemyAudioQueue.RequestSoundAtUpdateEnd"/>.
    /// </summary>
    internal void RequestBodySound(SoundEffectId sound) =>
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.MotherBrainBody, sound, SoundRequestPriority.Normal);

    /// <summary>Requests the rainbow beam's drain sound starting or stopping, at the end of the enemy update.</summary>
    internal void RequestRainbowBeamSound(SoundEffectId sound) =>
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.MotherBrainRainbowBeam, sound, SoundRequestPriority.Normal);

    /// <summary>
    /// Requests a sound a Mother Brain program cue or explosion makes, at the end of the enemy update. Mother Brain's
    /// explosion competes at low priority; every other cue sound at normal priority.
    /// </summary>
    internal void RequestCueSound(SoundEffectId sound) =>
        _audio.RequestSoundAtUpdateEnd(
            EnemySoundSource.MotherBrainCue,
            sound,
            sound == MotherBrainSoundDefinitions.Explosion ? SoundRequestPriority.Low : SoundRequestPriority.Normal);

    /// <summary>
    /// Requests one music change at the end of the enemy update. Mother Brain's changes come in sequences (stop, load a
    /// data set, select a track), so every request in an update is kept in order.
    /// </summary>
    internal void RequestMusic(MusicRequest request, MusicQueueDelay delay) =>
        _audio.AppendMusicAtUpdateEnd(EnemyMusicSource.MotherBrain, new DelayedMusicRequest(request, delay));
}
