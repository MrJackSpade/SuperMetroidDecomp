namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game's music queue and its three sound queues (#1410), a compatibility behaviour whose timing gameplay observes:
/// each music request waits its delay and plays on the update the original plays it; a data set change withdraws every
/// library's sound request for <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimeCalls"/> sound-handler calls; a
/// full music queue drops or overwrites a request as the caller chose; and a sound queue admits a request only below its
/// caller's limit, keeps each library's request until the library accepts it, then withdraws it.
/// </summary>
/// <remarks>
/// The queues end in <see cref="AudioRequest"/>s: play or stop a track, change the music data
/// set, and set or withdraw each sound library's request. Keeping the queues here makes their
/// delays and acknowledgement states visible to a C# debugger without inventing
/// gameplay-specific host sounds.
/// </remarks>
public sealed class AudioQueues
{
    /// <summary>Requests awaiting music dispatch in the circular queue; an empty position holds the stop request.</summary>
    private readonly MusicRequest[] _musicEntries =
        [.. Enumerable.Repeat(MusicRequest.Stop, AudioQueueDefinitions.MusicCapacity)];
    /// <summary>Per-entry frame delays paired with queued music requests.</summary>
    internal readonly MusicQueueDelay[] MusicDelays =
        new MusicQueueDelay[AudioQueueDefinitions.MusicCapacity];
    /// <summary>Three independent unread sound rings, one per sound library; a position holds no sound until one is queued there.</summary>
    internal readonly SoundEffectId?[,] SoundQueues = new SoundEffectId?[
        AudioQueueDefinitions.SoundLibraryCount,
        AudioQueueDefinitions.SoundCapacity];
    /// <summary>Next unread position in each library's SFX ring.</summary>
    internal readonly byte[] SoundReadPositions =
        new byte[AudioQueueDefinitions.SoundLibraryCount];
    /// <summary>Next insertion position in each library's SFX ring.</summary>
    internal readonly byte[] SoundWritePositions =
        new byte[AudioQueueDefinitions.SoundLibraryCount];
    /// <summary>Per-library request phase, from request through acknowledged withdrawal.</summary>
    internal readonly byte[] SoundStates = new byte[AudioQueueDefinitions.SoundLibraryCount];
    /// <summary>The sound currently awaiting each library's acknowledgement, or none.</summary>
    private readonly SoundEffectId?[] _currentSounds = new SoundEffectId?[AudioQueueDefinitions.SoundLibraryCount];

    /// <summary>The sound each of the three sound dispatchers last asked its library for, as an acknowledgement of it, or none.</summary>
    internal SoundAcknowledgements<SoundEffectId> SentSoundRequests =>
        SoundAcknowledgements<SoundEffectId>.Of(_currentSounds[0], _currentSounds[1], _currentSounds[2]);
    /// <summary>Two-frame hold before a recognized sound request is withdrawn from its library.</summary>
    private readonly byte[] _soundClearDelays =
        new byte[AudioQueueDefinitions.SoundLibraryCount];
    /// <summary>The power-on requests, deferred until the next audio frame.</summary>
    private readonly List<AudioRequest> _pendingPowerOnRequests = [];

    /// <summary>Slot of the music entry being retired or fetched by bank-80 dispatch, 0 through <see cref="AudioQueueDefinitions.MusicIndexMask"/>.</summary>
    internal int MusicReadPosition { get; private set; }
    /// <summary>Insertion slot of the circular delayed-music command queue, 0 through <see cref="AudioQueueDefinitions.MusicIndexMask"/>.</summary>
    internal int MusicWritePosition { get; private set; }
    /// <summary>
    /// The music delay countdown. It expires on reaching zero, and a cleared timer wraps negative on the
    /// next call, which is what fetches the next queued entry.
    /// </summary>
    internal WrappingCountdown MusicTimer { get; private set; }
    /// <summary>Music request currently awaiting delay expiry and dispatch.</summary>
    private MusicRequest _musicEntry = MusicRequest.Stop;
    /// <summary>Sound-handler calls, 0 through <see cref="AudioQueueDefinitions.MusicChangeSoundDowntimeCalls"/>, for which a music change withdraws every library's sound request.</summary>
    private int _soundHandlerDowntime;

    /// <summary>Creates empty music and sound queues and zeroed acknowledgement state, scheduling the power-on data set and requests for the next <see cref="AdvanceFrame"/> rather than executing them immediately.</summary>
    public AudioQueues() => Reset();

    /// <summary>
    /// Door-owned part of native DisableSounds ($05F5): set after the entry stop
    /// commands and cleared after the destination fade. The frontend reconstructs
    /// it from its serialized coroutine state before every frame, including restores.
    /// It rejects new SFX only; queued sounds and music continue to advance.
    /// </summary>
    [field: NonSerialized]
    internal bool DoorTransitionSoundsDisabled { get; set; }

    /// <summary>The music data set most recently loaded by a dispatched request, or null while only the shared sounds are loaded.</summary>
    public MusicDataSet? DataSet { get; private set; }

    /// <summary>The track most recently played, or null after a stop or a data set change.</summary>
    public MusicTrack? PlayingTrack { get; private set; }

    /// <summary>Exact <c>HasQueuedMusic</c> result: any occupied delayed-command slot.</summary>
    public bool HasQueuedMusic => MusicDelays.Any(delay => delay.Updates != 0);

    /// <summary>
    /// Exact door-transition predicate at <c>$82:E2B5-$82:E2D7</c>: true while any of the
    /// three sixteen-entry SFX rings has an unread request. An already-dequeued request that
    /// is merely waiting for SPC acknowledgement does not keep the native door wait alive.
    /// </summary>
    public bool HasQueuedSounds
    {
        get
        {
            for (int library = 0; library < AudioQueueDefinitions.SoundLibraryCount; library++)
            {
                if (((SoundWritePositions[library] - SoundReadPositions[library]) &
                     AudioQueueDefinitions.SoundIndexMask) != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>Restores the same audio queue state initialized by <c>Vector_RESET</c>.</summary>
    public void Reset()
    {
        DoorTransitionSoundsDisabled = false;
        Array.Fill(_musicEntries, MusicRequest.Stop);
        Array.Clear(MusicDelays);
        Array.Clear(SoundQueues);
        Array.Clear(SoundReadPositions);
        Array.Clear(SoundWritePositions);
        Array.Clear(SoundStates);
        Array.Clear(_currentSounds);
        Array.Clear(_soundClearDelays);
        MusicReadPosition = 0;
        MusicWritePosition = 0;
        MusicTimer = default;
        _musicEntry = MusicRequest.Stop;
        _soundHandlerDowntime = 0;
        DataSet = null;
        PlayingTrack = null;
        _pendingPowerOnRequests.Clear();

        // $80:841C installs the shared sounds, then stops the music and withdraws library
        // two's request before the first ordinary dispatcher frame.
        _pendingPowerOnRequests.Add(new AudioRequest.ChangeMusicDataSet(null));
        _pendingPowerOnRequests.Add(new AudioRequest.StopMusic());
        _pendingPowerOnRequests.Add(new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Library2));
    }

    /// <summary>Implements <c>QueueMusic_Delayed8</c> at $80:8FC1.</summary>
    public void QueueMusicDelayed8(MusicRequest request) =>
        QueueMusic(request, MusicQueueDelay.Shortest, overflow: MusicQueueOverflow.SkipWhenFull);

    /// <summary>Implements <c>QueueMusic_DelayedY</c>, including its minimum delay of eight.</summary>
    public void QueueMusicDelayed(MusicRequest request, MusicQueueDelay delay) =>
        QueueMusic(request, delay, overflow: MusicQueueOverflow.Overwrite);

    /// <summary>Queues the data set and track requests performed by ordinary room loading.</summary>
    public void QueueRoomMusic(RoomMusic music)
    {
        Ensure.NotNull(music);
        // LoadRoomMusic first silences the current track and loads a changed room data set.
        // LoadNewMusicTrackIfChanged then queues the room's selected track.
        bool changesMusicData = QueueRoomMusicData(music.DataSet);
        // `$82:E0E1` returns immediately for an inherited track. Most post-Ridley Ceres
        // states deliberately inherit so track seven and its alarm bed continue through
        // every door. Treating that as a request to stop replaced the live escape track at
        // the first transition.
        // `$82:E0E6-$E104` compares the requested data set and track, not the track alone.
        // A changed data set restarts the track even when both sets happen to use the same
        // track number. The Ceres interstitial ends on the Zebes-explosion set's track 5;
        // Landing Site requests the empty-Crateria set's track 5 and therefore must queue
        // track 5 again after the load instead of leaving only the gunship-engine sound.
        if (RoomTrackQueues(music.Track, changesMusicData))
            QueueMusicDelayed(music.Track.Request, MusicQueueDelay.AtLeastShortest(6));
    }

    /// <summary>
    /// Executes $82:E0D5 after the door scroll and earlier music requests finish. An
    /// inherited track keeps the current one; otherwise the room's data set and track are
    /// compared with the loaded data set and the playing track.
    /// </summary>
    internal void QueueRoomMusicTrack(RoomMusic music)
    {
        Ensure.NotNull(music);
        if (RoomTrackQueues(music.Track, music.DataSet != DataSet))
            QueueMusicDelayed(music.Track.Request, MusicQueueDelay.AtLeastShortest(6));
    }

    /// <summary>
    /// Queues only $82:E071's change to a different room data set. Saved-game loading
    /// shares this operation but lets the appearance coroutine schedule its own tracks.
    /// </summary>
    /// <param name="dataSet">The room's data set, or null to keep the loaded one.</param>
    /// <returns>Whether the data set change was queued.</returns>
    public bool QueueRoomMusicData(MusicDataSet? dataSet)
    {
        if (dataSet is not MusicDataSet changed || changed == DataSet)
            return false;
        QueueMusicDelayed8(MusicRequest.Stop);
        QueueMusicDelayed8(MusicRequest.LoadDataSet(changed));
        return true;
    }

    /// <summary>
    /// Whether a room's track choice is queued: never for an inherited track, always for a
    /// silenced room, and for a played track when the data set changes or it is not playing.
    /// </summary>
    private bool RoomTrackQueues(RoomMusicTrack track, bool dataSetDiffers) => track switch
    {
        RoomMusicTrack.Inherit => false,
        RoomMusicTrack.Silence => true,
        RoomMusicTrack.Play play => dataSetDiffers || play.Track != PlayingTrack,
        _ => throw new InvalidDataException($"Unknown room music track {track}."),
    };

    /// <summary>
    /// Executes the shared permanent-item music sequence from bank <c>$84</c>.
    /// </summary>
    /// <remarks>
    /// Every exposed, Chozo, and shot-block item list reaches <c>$84:8BDD</c> with track
    /// two before its pickup instruction calls <c>PlayRoomMusicTrackAfterAFrames($0168)</c>.
    /// The latter snapshots the currently playing room track, queues silence after 360
    /// frames, then restores that snapshot eight frames later. Queue clearing is part of
    /// the first opcode: retaining an older delayed room command can pre-empt the fanfare.
    /// </remarks>
    public void QueuePermanentItemFanfare()
    {
        MusicTrack? roomTrack = PlayingTrack;

        Array.Fill(_musicEntries, MusicRequest.Stop);
        Array.Clear(MusicDelays);
        MusicReadPosition = MusicWritePosition;
        MusicTimer = default;
        _musicEntry = MusicRequest.Stop;

        QueueMusicDelayed8(MusicRequest.SelectTrack(AudioQueueDefinitions.PermanentItemTrack));
        QueueMusicDelayed(
            MusicRequest.Stop,
            MusicQueueDelay.AtLeastShortest(AudioQueueDefinitions.PermanentItemFanfareFrames));
        QueueMusicDelayed8(MusicRequest.SelectTrackOrStop(roomTrack));
    }

    /// <summary>
    /// Queues <c>Cancel_Sound_Effects</c> at <c>$82:BE17</c> through all three native
    /// SFX rings, in the cartridge's exact library order.
    /// </summary>
    public void QueueCancelSoundEffects()
    {
        QueueSound(Library1Sound.Silence, maximumQueued: 6);
        QueueSound(Library2Sound.Silence, maximumQueued: 6);
        QueueSound(Library3Sound.Silence, maximumQueued: 6);
    }

    /// <summary>Queues <paramref name="request"/> and returns what the queue did with it (see the overload taking the request's parts).</summary>
    public SoundQueueAdmission QueueSound(SoundRequest request) =>
        QueueSound(request.SoundEffect, request.MaximumQueued.Requests, request.SoundSuppressed);
    /// <summary>
    /// Executes bank $80's sound-queue call: a request is rejected when its library's ring already holds
    /// <paramref name="maximumQueued"/> unread requests, and otherwise queued unless sounds are suppressed. The
    /// outcome says which happened and how full the ring was; the Speed Booster's stage selection reads it after the
    /// library-three call at $90:85A6.
    /// </summary>
    /// <param name="soundEffect">The sound, whose library selects one of the three independent rings.</param>
    /// <param name="maximumQueued">Admission threshold from 1 through 15 unread requests; an occupancy at or above this value rejects the request before suppression is tested.</param>
    /// <param name="soundSuppressed">
    /// The native disable-sounds, demo-state, or active Power Bomb guard, evaluated
    /// after the occupancy threshold. Suppression leaves the queue unchanged.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumQueued"/> is outside 1 through 15, or <paramref name="soundEffect"/> names no library.</exception>
    public SoundQueueAdmission QueueSound(
        SoundEffectId soundEffect, int maximumQueued, bool soundSuppressed = false)
    {
        if (maximumQueued is < 1 or > AudioQueueDefinitions.MaximumSoundOccupancy)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumQueued), maximumQueued, "Retail queue limit must be 1..15.");
        }

        int queue = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        int occupancy = (SoundWritePositions[queue] - SoundReadPositions[queue]) &
            AudioQueueDefinitions.SoundIndexMask;
        if (occupancy >= maximumQueued)
            return SoundQueueAdmission.Rejected(occupancy);

        if (soundSuppressed || DoorTransitionSoundsDisabled)
            return SoundQueueAdmission.Admitted(occupancy);

        // The ring never fills here: a full ring holds fifteen unread requests, and every threshold is at most fifteen,
        // so the occupancy test above has already rejected the request. The cartridge's full-ring branch, which keeps
        // the higher-priority of two requests in the last free position, is unreachable.
        byte write = SoundWritePositions[queue];
        byte next = unchecked((byte)(
            (write + 1) & AudioQueueDefinitions.SoundIndexMask));
        SoundQueues[queue, write] = soundEffect;
        SoundWritePositions[queue] = next;
        SoundQueues[queue, next] = null;
        return SoundQueueAdmission.Admitted(occupancy);
    }

    /// <summary>Runs selected audio dispatch owners and returns their requests, which only the session's audio advance consumes.</summary>
    internal IReadOnlyList<AudioRequest> AdvanceFrame(
        SoundAcknowledgements<SoundEffectId> acknowledgements,
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
    /// Runs <c>$88:84BD</c>'s music call before a new outer door dispatch.
    /// IRQ-only continuations do not invoke this owner.
    /// </summary>
    internal IReadOnlyList<AudioRequest> AdvanceMusicDispatch()
    {
        var requests = new List<AudioRequest>();
        HandleMusicQueue(requests);
        return requests.Count == 0 ? Array.Empty<AudioRequest>() : requests.ToArray();
    }

    /// <summary>Enqueues a delayed music command, optionally dropping it when the native ring has no free slot.</summary>
    private void QueueMusic(
        MusicRequest request,
        MusicQueueDelay delay,
        MusicQueueOverflow overflow)
    {
        if (delay.Updates < AudioQueueDefinitions.MinimumMusicDelayFrames)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay), delay, "Effective music queue delay must be at least eight frames.");
        }

        int next = (MusicWritePosition + 1) & AudioQueueDefinitions.MusicIndexMask;
        if (overflow == MusicQueueOverflow.SkipWhenFull && next == MusicReadPosition)
            return;

        _musicEntries[MusicWritePosition] = request;
        MusicDelays[MusicWritePosition] = delay;
        MusicWritePosition = next;
    }

    /// <summary>Advances the bank-80 timer and requests either a data set change or a track at expiry.</summary>
    private void HandleMusicQueue(List<AudioRequest> requests)
    {
        CountdownStep step = MusicTimer.Decrement();
        MusicTimer = step.Remaining;
        bool timerExpired = step.IsZero;
        if (step.IsNonNegative)
        {
            if (!timerExpired)
                return;

            if (_musicEntry is MusicRequest.ChangeDataSet change)
            {
                DataSet = change.DataSet;
                PlayingTrack = null;
                requests.Add(new AudioRequest.ChangeMusicDataSet(change.DataSet));
                // `$80:8F89-$8FA2` retires the data entry and returns. The timer stays zero,
                // so the next entry is fetched only on the following call when it underflows.
                ClearAndAdvanceMusicEntry();
                _soundHandlerDowntime = AudioQueueDefinitions.MusicChangeSoundDowntimeCalls;
                return;
            }

            PlayingTrack = (_musicEntry as MusicRequest.PlayTrack)?.Track;
            requests.Add(AudioRequest.ForTrack(PlayingTrack));

            ClearAndAdvanceMusicEntry();
            _soundHandlerDowntime = AudioQueueDefinitions.MusicChangeSoundDowntimeCalls;
        }

        if (MusicReadPosition == MusicWritePosition)
        {
            MusicTimer = default;
            return;
        }

        _musicEntry = _musicEntries[MusicReadPosition];
        MusicTimer = WrappingCountdown.Start(MusicDelays[MusicReadPosition].Updates);
    }

    /// <summary>Retires the processed music command and its delay before fetching the next entry.</summary>
    private void ClearAndAdvanceMusicEntry()
    {
        _musicEntries[MusicReadPosition] = MusicRequest.Stop;
        MusicDelays[MusicReadPosition] = default;
        MusicReadPosition = (MusicReadPosition + 1) & AudioQueueDefinitions.MusicIndexMask;
    }

    /// <summary>Either withdraws all three libraries' requests during music downtime or advances their independent handshakes.</summary>
    private void HandleSoundEffects(
        SoundAcknowledgements<SoundEffectId> acknowledgements,
        List<AudioRequest> requests)
    {
        if (_soundHandlerDowntime != 0)
        {
            // Guarded by the nonzero test above: the downtime counts down to zero and never wraps.
            _soundHandlerDowntime = Ensure.AtLeastZero(_soundHandlerDowntime - 1);
            for (byte queue = 0;
                queue < AudioQueueDefinitions.SoundLibraryCount;
                queue++)
            {
                requests.Add(new AudioRequest.ClearSoundRequest(SoundEffectLibraries.FromQueueIndex(queue)));
                _currentSounds[queue] = null;
            }
            return;
        }

        for (int queue = 0; queue < AudioQueueDefinitions.SoundLibraryCount; queue++)
            HandleSoundEffectQueue(
                queue,
                acknowledgements[queue],
                requests);
    }

    /// <summary>Performs request, acknowledgement, withdrawal-delay and zero-acknowledgement phases for one sound library.</summary>
    private void HandleSoundEffectQueue(
        int queue,
        SoundAcknowledgement<SoundEffectId> acknowledgement,
        List<AudioRequest> requests)
    {
        switch (SoundStates[queue])
        {
            case 0: // $82:8A2C - send the next queued request to its library.
                SendNextSound(queue, requests);
                break;

            case 1: // $82:8A55 - wait until the library acknowledges the request.
                if (!acknowledgement.Acknowledges(_currentSounds[queue]))
                    requests.Add(AudioRequest.ForSound(SoundEffectLibraries.FromQueueIndex(queue), _currentSounds[queue]));
                else
                {
                    SoundStates[queue] = 2;
                    _soundClearDelays[queue] = 2;
                }
                break;

            case 2: // $82:8A6C - wait two frames, then withdraw the request.
                if ((_soundClearDelays[queue] = WrappingCounter.DecrementByte(_soundClearDelays[queue])) == 0)
                {
                    requests.Add(new AudioRequest.ClearSoundRequest(SoundEffectLibraries.FromQueueIndex(queue)));
                    _currentSounds[queue] = null;
                    SoundStates[queue] = 3;
                }
                break;

            case 3: // $82:8A7C - wait for the library to acknowledge the withdrawal, then continue.
                if (!acknowledgement.IsNone)
                {
                    requests.Add(new AudioRequest.ClearSoundRequest(SoundEffectLibraries.FromQueueIndex(queue)));
                }
                else
                {
                    SoundStates[queue] = 0;
                    SendNextSound(queue, requests);
                }
                break;

            default:
                throw new InvalidDataException(
                    $"SFX library {queue + 1} entered invalid retail dispatcher state {SoundStates[queue]}.");
        }
    }

    /// <summary>Dequeues one unread sound request, asks its library for it, and enters the acknowledgement phase.</summary>
    private void SendNextSound(int queue, List<AudioRequest> requests)
    {
        if (SoundReadPositions[queue] == SoundWritePositions[queue])
            return;

        byte read = SoundReadPositions[queue];
        SoundEffectId? sound = SoundQueues[queue, read];
        requests.Add(AudioRequest.ForSound(SoundEffectLibraries.FromQueueIndex(queue), sound));
        _currentSounds[queue] = sound;
        SoundReadPositions[queue] = unchecked((byte)(
            (read + 1) & AudioQueueDefinitions.SoundIndexMask));
        SoundStates[queue] = 1;
    }

}
