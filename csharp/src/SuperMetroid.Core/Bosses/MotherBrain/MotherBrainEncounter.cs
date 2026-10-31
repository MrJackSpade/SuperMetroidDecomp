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

    /// <summary>Binds the encounter's page selections to sprite presentation.</summary>
    internal MotherBrainEncounter(EnemyPresentation presentation) => _presentation = presentation;

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

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (MotherBrain?.LastSoundEffect is SoundEffectId motherBrainLibrary2)
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary2, SoundEffectLibrary.Library2), priority: SoundRequestPriority.Normal);
        if (MotherBrain?.LastSoundEffectLibrary1 is SoundEffectId motherBrainLibrary1)
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary1, SoundEffectLibrary.Library1), priority: SoundRequestPriority.Normal);
        if (MotherBrain?.LastSoundEffectLibrary3 is SoundEffectId motherBrainLibrary3)
        {
            // Explosion instruction $13 is QueueSfx3_Max3; the other translated library-
            // three Mother Brain instructions use Max6.
            SoundRequestPriority priority = motherBrainLibrary3 == MotherBrainSoundDefinitions.Explosion ? SoundRequestPriority.Low : SoundRequestPriority.Normal;
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary3, SoundEffectLibrary.Library3), priority);
        }
    }

    /// <summary>
    /// Each published slot stands for one of the cartridge's per-library sound queues, so a sound
    /// recorded in a slot must belong to that slot's library.
    /// </summary>
    private static SoundEffectId RequireLibrary(SoundEffectId sound, SoundEffectLibrary library) =>
        sound.Library == library ? sound : throw new InvalidOperationException(
            $"Mother Brain published {sound} in its {library} sound slot.");

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        if (MotherBrain is not null)
        {
            foreach (DelayedMusicRequest request in MotherBrain.MusicRequests)
            {
                audio.QueueMusic(new DelayedMusicRequest(request.Request, request.Delay));
            }
        }
    }
}
