using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain encounter state shared across Tourian: the state extension, the end of
/// the tube effects the elevator observes, and the encounter's sound and music requests.
/// </summary>
[RoomLifetime]
internal sealed class MotherBrainEncounter
{
    /// <summary>Receives the encounter's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the encounter's requests to the enemy audio queue.</summary>
    internal MotherBrainEncounter(EnemyAudioQueue audio) => _audio = audio;

    /// <summary>The typed multipart encounter while Mother Brain's retail population is loaded.</summary>
    public MotherBrainEnemyState? MotherBrain { get; internal set; }

    /// <summary>
    /// The warning Zebes's escape shows over Mother Brain's room background (#1555): the glyphs her death's typewriter has
    /// typed, in the order typed, each at its background cell; nothing before her death types any. It lasts the room, as
    /// the encounter does.
    /// </summary>
    public RoomWrittenOverlay ZebesWarning { get; private set; } = RoomWrittenOverlay.Empty;

    /// <summary>Shows <paramref name="picture"/> as the next glyph of <see cref="ZebesWarning"/>.</summary>
    internal void ShowZebesWarning(WrittenPicture picture) => ZebesWarning = ZebesWarning.With(picture);

    /// <summary>
    /// The rows Mother Brain's corpse shows as its own rot has left them, which her corpse's frame is drawn whole through
    /// (#1539); none while she is not loaded or her rainbow beam sequence has not begun.
    /// </summary>
    internal CorpseRotRows? ShownCorpseRows => MotherBrain?.RainbowBeamSequence?.ThirdPhaseDeath.CorpseRotting.Shown;

    /// <summary>The colours the loaded encounter has laid (#1512).</summary>
    /// <exception cref="InvalidOperationException">Mother Brain is not loaded.</exception>
    internal MotherBrainColours Colours => (MotherBrain ?? throw new InvalidOperationException(
        "Mother Brain's colours are laid while her encounter is loaded.")).Colours;

    /// <summary>
    /// Queues a sound of the body's fight, fake death and tube collapse, or of the Baby Metroid's cries in the cutscene,
    /// as it is made, after the update's earlier requests.
    /// </summary>
    internal void RequestBodySound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues the rainbow beam's drain sound starting or stopping.</summary>
    internal void RequestRainbowBeamSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>
    /// Queues a sound a Mother Brain program cue or explosion makes. Mother Brain's explosion competes at low priority;
    /// every other cue sound at normal priority.
    /// </summary>
    internal void RequestCueSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(
            sound,
            sound == MotherBrainSoundDefinitions.Explosion ? SoundRequestPriority.Low : SoundRequestPriority.Normal);

    /// <summary>
    /// Queues one music change. Mother Brain's changes come in sequences (stop, load a data set, select a track), each
    /// queued after the one before it.
    /// </summary>
    internal void RequestMusic(MusicRequest request, MusicQueueDelay delay) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, delay));
}
