namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game's music queue and its three sound queues (#1410): each music request waits its delay and plays on the update
/// the game plays it; a data set change withdraws every library's sound request for
/// <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimeCalls"/> sound-handler calls; and a sound queue admits a request by its
/// priority (<see cref="SoundQueueAdmission.For"/>) and hands it to its library through the session's
/// <see cref="SoundRequestHandshakes{TLibrary, TSound}"/>, which repeat each request until the library accepts it, then
/// withdraw it.
/// </summary>
/// <remarks>
/// The queues end in <see cref="AudioRequest"/>s: play or stop a track, change the music data
/// set, and set or withdraw each sound library's request. The queues' delays and acknowledgement
/// states are game state, so the host plays only what the queues release and invents no
/// gameplay-specific sounds.
/// </remarks>
public sealed class AudioQueues
{
    /// <summary>
    /// The music requests waiting for the music handler, oldest first, each with its delay. The request the handler fetched
    /// stays first until it has played. No retail path holds more requests than the game's queue keeps without filling it,
    /// so the queue never refuses or overwrites one (#1692, <c>docs/compatibility-layer.md</c>).
    /// </summary>
    internal readonly Queue<QueuedMusic> Music = new();
    /// <summary>Each sound library's own queue of waiting requests.</summary>
    internal readonly Dictionary<SoundEffectLibrary, SoundQueue> SoundQueues =
        SoundEffectLibraries.InQueueOrder.ToDictionary(library => library, _ => new SoundQueue());
    /// <summary>The handshake each library's handler runs with its library, injected by the session's composition root.</summary>
    internal readonly SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId> SoundHandshakes;

    /// <summary>The sound each of the three sound dispatchers last asked its library for, as an acknowledgement of it, or none.</summary>
    internal SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> SentSoundRequests => SoundHandshakes.Requests;
    /// <summary>The power-on requests, deferred until the next audio frame.</summary>
    private readonly List<AudioRequest> _pendingPowerOnRequests = [];

    /// <summary>
    /// The request the music handler fetched and is waiting out the delay of, or none when its next call fetches the
    /// next request from the queue.
    /// </summary>
    internal PendingMusicDispatch? PendingMusic { get; private set; }
    /// <summary>Sound-handler calls, 0 through <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimeCalls"/>, for which a music change withdraws every library's sound request.</summary>
    private int _soundHandlerDowntime;

    /// <summary>The gameplay states every sound request is tested against as it is made (<see cref="SoundSuppression"/>).</summary>
    private readonly IGameplaySoundState _gameplay;

    /// <summary>Creates empty music and sound queues with every library's handler ready, scheduling the power-on data set and requests for the next <see cref="AdvanceFrame"/> rather than executing them immediately.</summary>
    /// <param name="soundHandshakes">
    /// The handshakes the libraries' handlers run, covering every library of <see cref="SoundEffectLibraries.InQueueOrder"/>;
    /// the queues reset them with their own state.
    /// </param>
    /// <param name="gameplay">
    /// The gameplay states the queues read as each sound request is made: whether an attract demo plays and whether a
    /// power bomb explodes, which turn an admitted request away.
    /// </param>
    internal AudioQueues(SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId> soundHandshakes, IGameplaySoundState gameplay)
    {
        SoundHandshakes = Ensure.NotNull(soundHandshakes);
        _gameplay = Ensure.NotNull(gameplay);
        Reset();
    }

    /// <summary>
    /// Door-owned part of the sound-disable switch: set after the entry stop
    /// requests and cleared after the destination fade. The frontend sets it from the
    /// door transition's phase before every frame.
    /// It rejects new SFX only; queued sounds and music continue to advance.
    /// </summary>
    internal bool DoorTransitionSoundsDisabled { get; set; }

    /// <summary>The music data set most recently loaded by a dispatched request, or null while only the shared sounds are loaded.</summary>
    public MusicDataSet? DataSet { get; private set; }

    /// <summary>The track most recently played, or null after a stop or a data set change.</summary>
    public MusicTrack? PlayingTrack { get; private set; }

    /// <summary>
    /// What phases waiting for the music to settle read: true while the music queue holds any request, including the one
    /// the music handler fetched and is waiting out the delay of.
    /// </summary>
    public bool HasQueuedMusic => Music.Count != 0;

    /// <summary>
    /// What the door transition waits on: true while any sound library has a request waiting in its queue. A request
    /// its handler has already taken and is only waiting for its library to accept does not keep the door waiting.
    /// </summary>
    public bool HasQueuedSounds => SoundQueues.Values.Any(queue => queue.Count != 0);

    /// <summary>Restores the audio queue state the game starts with at power-on.</summary>
    public void Reset()
    {
        DoorTransitionSoundsDisabled = false;
        Music.Clear();
        PendingMusic = null;
        foreach (SoundQueue queue in SoundQueues.Values)
            queue.Clear();
        SoundHandshakes.Reset();
        _soundHandlerDowntime = 0;
        DataSet = null;
        PlayingTrack = null;
        _pendingPowerOnRequests.Clear();

        // Starting the game installs the shared sounds, then stops the music and withdraws library
        // two's request before the first ordinary dispatcher frame.
        _pendingPowerOnRequests.Add(new AudioRequest.ChangeMusicDataSet(null));
        _pendingPowerOnRequests.Add(new AudioRequest.StopMusic());
        _pendingPowerOnRequests.Add(new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Enemies));
    }

    /// <summary>
    /// Queues <paramref name="request"/> to play after the shortest delay (<see cref="MusicQueueDelay.Shortest"/>), behind
    /// the requests waiting.
    /// </summary>
    /// <param name="request">The music request.</param>
    public void QueueMusic(MusicRequest request) =>
        Music.Enqueue(new QueuedMusic(request, MusicQueueDelay.Shortest));

    /// <summary>
    /// Schedules <paramref name="request"/> to play after <paramref name="delay"/>, behind the requests waiting.
    /// </summary>
    /// <param name="request">The music request.</param>
    /// <param name="delay">How long it waits on the queue, already lengthened to the shortest delay.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is shorter than the shortest delay.</exception>
    public void ScheduleMusic(MusicRequest request, MusicQueueDelay delay)
    {
        if (delay.Updates < AudioQueueDefinitions.MinimumMusicDelayFrames)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay), delay, "Effective music queue delay must be at least eight frames.");
        }
        Music.Enqueue(new QueuedMusic(request, delay));
    }

    /// <summary>Queues the data set and track requests performed by ordinary room loading.</summary>
    public void QueueRoomMusic(RoomMusic music)
    {
        // Loading a room's music first silences the current track and loads a changed room data set, then queues the
        // room's selected track if it changed.
        bool changesMusicData = QueueRoomMusicData(music.DataSet);
        // A door into a room that inherits its track leaves the music alone. Most post-Ridley Ceres
        // states deliberately inherit so the escape song and its alarm bed continue through
        // every door. Treating that as a request to stop replaced the live escape track at
        // the first transition.
        // The door compares the requested data set and track, not the track alone.
        // A changed data set restarts the room's song even when that song already plays: the
        // load stops the music, so the song must be queued again after it instead of leaving
        // only the gunship-engine sound at the Landing Site.
        if (RoomTrackQueues(music.Track, changesMusicData))
            ScheduleMusic(music.Track.Request, MusicQueueDelay.AtLeastShortest(6));
    }

    /// <summary>
    /// Runs the door's music change after the door scroll and earlier music requests finish. An
    /// inherited track keeps the current one; otherwise the room's data set and track are
    /// compared with the loaded data set and the playing track.
    /// </summary>
    internal void QueueRoomMusicTrack(RoomMusic music)
    {
        if (RoomTrackQueues(music.Track, music.DataSet != DataSet))
            ScheduleMusic(music.Track.Request, MusicQueueDelay.AtLeastShortest(6));
    }

    /// <summary>
    /// Queues only the change to a different room data set. Saved-game loading
    /// shares this operation but lets the appearance coroutine schedule its own tracks.
    /// </summary>
    /// <param name="dataSet">The room's data set, or null to keep the loaded one.</param>
    /// <returns>Whether the data set change was queued.</returns>
    public bool QueueRoomMusicData(MusicDataSet? dataSet)
    {
        if (dataSet is not MusicDataSet changed || changed == DataSet)
            return false;
        QueueMusic(MusicRequest.Stop);
        QueueMusic(MusicRequest.LoadDataSet(changed));
        return true;
    }

    /// <summary>
    /// Whether a room's track choice is queued: never for an inherited track, always for a
    /// silenced room, and for a played song or area theme when the data set changes or it is not playing.
    /// </summary>
    private bool RoomTrackQueues(RoomMusicTrack track, bool dataSetDiffers) => track switch
    {
        RoomMusicTrack.Inherit => false,
        RoomMusicTrack.Silence => true,
        RoomMusicTrack.Play play => dataSetDiffers || play.Track != PlayingTrack,
        // With no data set loaded no area theme can play, so the choice is queued.
        RoomMusicTrack.PlayAreaTheme => dataSetDiffers || DataSet is not MusicDataSet loaded || AreaThemeDefinitions.Themes[loaded] != PlayingTrack,
        _ => throw new InvalidDataException($"Unknown room music track {track}."),
    };

    /// <summary>
    /// Runs the shared permanent-item music sequence.
    /// </summary>
    /// <remarks>
    /// Every exposed, Chozo, and shot-block item reaches the item fanfare with track
    /// two before its pickup step schedules the room track's return.
    /// That step snapshots the currently playing room track, queues silence after 360
    /// frames, then restores that snapshot eight frames later. Queue clearing is part of
    /// the first step: retaining an older delayed room-track request can pre-empt the fanfare.
    /// </remarks>
    public void QueuePermanentItemFanfare()
    {
        MusicTrack? roomTrack = PlayingTrack;

        Music.Clear();
        PendingMusic = null;

        QueueMusic(MusicRequest.SelectTrack(AudioQueueDefinitions.PermanentItemTrack));
        ScheduleMusic(
            MusicRequest.Stop,
            MusicQueueDelay.AtLeastShortest(AudioQueueDefinitions.PermanentItemFanfareFrames));
        QueueMusic(MusicRequest.SelectTrackOrStop(roomTrack));
    }

    /// <summary>
    /// Silences all three sound libraries by queueing each one's silence request at normal
    /// priority, in library order.
    /// </summary>
    public void QueueCancelSoundEffects()
    {
        QueueSound(SamusSound.Silence, priority: SoundRequestPriority.Normal);
        QueueSound(EnemySound.Silence, priority: SoundRequestPriority.Normal);
        QueueSound(RoomSound.Silence, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Queues <paramref name="request"/> and returns what the queue did with it (see the overload taking the request's parts).</summary>
    public SoundQueueAdmission QueueSound(SoundRequest request) =>
        QueueSound(request.SoundEffect, request.Priority);
    /// <summary>
    /// Requests <paramref name="soundEffect"/> from its library's queue: <see cref="SoundQueueAdmission.For"/> decides
    /// from the queue's waiting requests and the request's priority whether it is admitted, and an admitted request joins
    /// the queue unless the game turns sounds away now (<see cref="SoundQueueAdmission.Joins"/>: the door's switch
    /// disables sounds, an attract demo plays or a power bomb explodes). Every sound request, whoever makes it, passes this
    /// one test. The outcome says whether it was admitted and how full the queue was; the Speed Booster's stage selection
    /// reads it after requesting its echo.
    /// </summary>
    /// <param name="soundEffect">The sound, whose library selects one of the three independent queues.</param>
    /// <param name="priority">How full the queue may be for the request to be admitted; tested before suppression.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="soundEffect"/> names no library.</exception>
    public SoundQueueAdmission QueueSound(SoundEffectId soundEffect, SoundRequestPriority priority)
    {
        SoundQueue queue = QueueOf(soundEffect.Library);
        SoundQueueAdmission admission = SoundQueueAdmission.For(queue.Count, priority);
        if (!admission.Joins(new SoundSuppression(DoorTransitionSoundsDisabled, _gameplay.AttractDemoPlaying, _gameplay.PowerBombExploding)))
            return admission;

        // Every priority turns a request away once fifteen wait, so an admitted request always finds room: the game's
        // handling of a request arriving at a queue with no room, which keeps the higher-priority of the two, is never
        // reached.
        queue.Add(soundEffect);
        return admission;
    }

    /// <summary>Runs selected audio dispatch owners and returns their requests, which only the session's audio advance consumes.</summary>
    internal IReadOnlyList<AudioRequest> AdvanceFrame(
        SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> acknowledgements,
        bool advanceMusicQueue = true,
        int soundEffectHandlerCalls = 1)
    {
        Ensure.AtLeastZero(soundEffectHandlerCalls);
        List<AudioRequest> requests = [.. _pendingPowerOnRequests];
        _pendingPowerOnRequests.Clear();
        if (advanceMusicQueue) HandleMusicQueue(requests);
        for (int call = 0; call < soundEffectHandlerCalls; call++)
            HandleSoundEffects(acknowledgements, requests);
        return requests.Count == 0 ? Array.Empty<AudioRequest>() : requests.ToArray();
    }

    /// <summary>
    /// Processes the music queue once before a new update of the running phase.
    /// An update that continues a waiting step does not invoke this owner.
    /// </summary>
    internal IReadOnlyList<AudioRequest> AdvanceMusicDispatch()
    {
        var requests = new List<AudioRequest>();
        HandleMusicQueue(requests);
        return requests.Count == 0 ? Array.Empty<AudioRequest>() : requests.ToArray();
    }

    /// <summary>
    /// Runs one music-handler call. While a fetched request waits out its delay the call counts the wait down, and the
    /// call that ends it dispatches the request and retires it from the queue: a data set change then returns, so the
    /// next request is fetched on the following call, and a track or a stop goes on to fetch the next request in the same
    /// call. A call that is not waiting fetches the next request, if the queue has one to fetch, and starts its delay.
    /// </summary>
    private void HandleMusicQueue(List<AudioRequest> requests)
    {
        if (PendingMusic is PendingMusicDispatch pending)
        {
            // A fetched request waits at least the shortest delay, so the wait reaches zero on its last call and never wraps.
            int callsUntilSent = Ensure.AtLeastZero(pending.CallsUntilSent - 1);
            if (callsUntilSent != 0)
            {
                PendingMusic = pending with { CallsUntilSent = callsUntilSent };
                return;
            }
            PendingMusic = null;

            if (pending.Request is MusicRequest.ChangeDataSet change)
            {
                DataSet = change.DataSet;
                PlayingTrack = null;
                requests.Add(new AudioRequest.ChangeMusicDataSet(change.DataSet));
                Music.Dequeue();
                _soundHandlerDowntime = AudioQueueDefinitions.MusicChangeSoundDowntimeCalls;
                return;
            }

            PlayingTrack = pending.Request switch
            {
                MusicRequest.PlayTrack play => play.Track,
                MusicRequest.PlayAreaTheme => AreaThemeOf(DataSet),
                MusicRequest.StopMusic => null,
                _ => throw new InvalidDataException($"Unknown music request {pending.Request}."),
            };
            requests.Add(AudioRequest.ForTrack(PlayingTrack));
            Music.Dequeue();
            _soundHandlerDowntime = AudioQueueDefinitions.MusicChangeSoundDowntimeCalls;
        }

        if (Music.TryPeek(out QueuedMusic? next))
            PendingMusic = new PendingMusicDispatch(next.Request, next.Delay.Updates);
    }

    /// <summary>The area theme of <paramref name="dataSet"/>, the data set loaded as an area-theme request plays.</summary>
    /// <exception cref="InvalidOperationException">No data set is loaded, so no area theme can play.</exception>
    private static MusicTrack AreaThemeOf(MusicDataSet? dataSet) => dataSet is MusicDataSet loaded
        ? AreaThemeDefinitions.Themes[loaded]
        : throw new InvalidOperationException("An area theme was requested with no music data set loaded.");

    /// <summary>Either withdraws all three libraries' requests during music downtime or advances their independent handshakes.</summary>
    private void HandleSoundEffects(
        SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> acknowledgements,
        List<AudioRequest> requests)
    {
        if (_soundHandlerDowntime != 0)
        {
            // Guarded by the nonzero test above: the downtime counts down to zero and never wraps.
            _soundHandlerDowntime = Ensure.AtLeastZero(_soundHandlerDowntime - 1);
            foreach (SoundEffectLibrary library in SoundEffectLibraries.InQueueOrder)
                requests.Add(new AudioRequest.ClearSoundRequest(library));
            SoundHandshakes.DropAwaitedRequests();
            return;
        }

        foreach (SoundEffectLibrary library in SoundEffectLibraries.InQueueOrder)
            HandleSoundEffectQueue(library, acknowledgements[library], requests);
    }

    /// <summary>Runs one handler call of <paramref name="library"/>'s handshake and does what the call asks of its queue.</summary>
    private void HandleSoundEffectQueue(
        SoundEffectLibrary library,
        SoundAcknowledgement<SoundEffectId> acknowledgement,
        List<AudioRequest> requests)
    {
        SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.HandlerStep step = SoundHandshakes.Advance(library, acknowledgement);
        switch (step)
        {
            case SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.HandlerStep.Wait:
                break;

            case SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.HandlerStep.RequestNext:
                SendNextSound(library, requests);
                break;

            case SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.HandlerStep.RepeatRequest:
                requests.Add(AudioRequest.ForSound(library, SoundHandshakes.Awaited(library)));
                break;

            case SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.HandlerStep.Withdraw:
                requests.Add(new AudioRequest.ClearSoundRequest(library));
                break;

            default:
                throw new InvalidDataException($"Sound library {library}'s handler asked for unknown step {step}.");
        }
    }

    /// <summary>Takes <paramref name="library"/>'s oldest waiting request, if one waits, asks the library for it, and starts awaiting its acceptance.</summary>
    private void SendNextSound(SoundEffectLibrary library, List<AudioRequest> requests)
    {
        if (QueueOf(library).TakeOldest() is not SoundEffectId sound)
            return;

        requests.Add(AudioRequest.ForSound(library, sound));
        SoundHandshakes.Requested(library, sound);
    }

    /// <summary>The queue of <paramref name="library"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound library.</exception>
    private SoundQueue QueueOf(SoundEffectLibrary library) =>
        SoundQueues.TryGetValue(library, out SoundQueue? queue)
            ? queue
            : throw new ArgumentOutOfRangeException(nameof(library), library, "Unknown sound-effect library.");
}
