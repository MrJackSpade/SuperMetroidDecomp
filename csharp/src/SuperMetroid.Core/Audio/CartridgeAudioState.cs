namespace SuperMetroid.Core.Audio;

/// <summary>
/// C# translation of bank $80's music queue and bank $82's three SFX dispatchers.
/// </summary>
/// <remarks>
/// This class intentionally stops at the four APU ports. The SPC sequencer decides notes,
/// instruments, voice stealing, BRR samples, envelopes, echo, and mixing exactly as it did
/// on the cartridge. Keeping the 65816 queue here makes its delays and acknowledgement
/// states visible to a C# debugger without inventing gameplay-specific host sounds.
/// </remarks>
public sealed class CartridgeAudioState
{
    /// <summary>
    /// Commands awaiting bank-80 music dispatch in the native circular queue; an empty slot holds the zero word, the stop
    /// command.
    /// </summary>
    private readonly MusicCommand[] _musicEntries =
        [.. Enumerable.Repeat(MusicCommand.Stop, AudioRomData.Queues.MusicCapacity)];
    /// <summary>Per-entry frame delays paired with queued music commands.</summary>
    internal readonly MusicCommandDelay[] MusicDelays =
        new MusicCommandDelay[AudioRomData.Queues.MusicCapacity];
    /// <summary>Three independent unread SFX rings, one per SPC sound library.</summary>
    internal readonly byte[,] SoundQueues = new byte[
        AudioRomData.Queues.SoundLibraryCount,
        AudioRomData.Queues.SoundCapacity];
    /// <summary>Next unread position in each library's SFX ring.</summary>
    internal readonly byte[] SoundReadPositions =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Next insertion position in each library's SFX ring.</summary>
    internal readonly byte[] SoundWritePositions =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Per-library port handshake phase, from request through zero acknowledgement.</summary>
    internal readonly byte[] SoundStates = new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Request byte currently awaiting each SPC library's acknowledgement.</summary>
    private readonly byte[] _currentSounds = new byte[AudioRomData.Queues.SoundLibraryCount];

    /// <summary>The request bytes the three sound dispatchers last wrote to their APU ports; port 0 is zero.</summary>
    internal CartridgeAudioAcknowledgements SoundPortWrites => new(0, _currentSounds[0], _currentSounds[1], _currentSounds[2]);
    /// <summary>Two-frame hold before a recognized SFX request is cleared from the APU port.</summary>
    private readonly byte[] _soundClearDelays =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    /// <summary>Reset and upload port commands deferred until the next audio frame.</summary>
    private readonly List<CartridgeAudioCommand> _pendingImmediateCommands = [];

    /// <summary>Slot of the music entry being retired or fetched by bank-80 dispatch, 0 through <see cref="AudioRomData.Queues.MusicIndexMask"/>.</summary>
    internal int MusicReadPosition;
    /// <summary>Insertion slot of the circular delayed-music command queue, 0 through <see cref="AudioRomData.Queues.MusicIndexMask"/>.</summary>
    internal int MusicWritePosition;
    /// <summary>
    /// The music delay countdown. It expires on reaching zero, and a cleared timer wraps negative on the
    /// next call, which is what fetches the next queued entry.
    /// </summary>
    internal NativeCountdown MusicTimer;
    /// <summary>Music command currently awaiting delay expiry and APU dispatch.</summary>
    private MusicCommand _musicEntry = MusicCommand.Stop;
    /// <summary>Sound-handler calls, 0 through <see cref="AudioRomData.Queues.MusicAndSfxDowntimeFrames"/>, for which a music change forces all SFX ports to zero.</summary>
    private int _soundHandlerDowntime;

    /// <summary>Creates empty native music/SFX queues and zeroed acknowledgement state, scheduling the common audio-bank upload and reset port writes for the next <see cref="AdvanceFrame"/> rather than executing them immediately.</summary>
    public CartridgeAudioState() => Reset();

    /// <summary>
    /// Door-owned part of native DisableSounds ($05F5): set after the entry stop
    /// commands and cleared after the destination fade. The frontend reconstructs
    /// it from its serialized coroutine state before every frame, including restores.
    /// It rejects new SFX only; queued sounds and music continue to advance.
    /// </summary>
    [field: NonSerialized]
    internal bool DoorTransitionSoundsDisabled { get; set; }

    /// <summary>Music-data set most recently uploaded by a processed queue command.</summary>
    public int MusicDataIndex { get; private set; }

    /// <summary>Track most recently written to APU port zero, 0 through 127.</summary>
    public int MusicTrackIndex { get; private set; }

    /// <summary>Exact <c>HasQueuedMusic</c> result: any occupied delayed-command slot.</summary>
    public bool HasQueuedMusic => MusicDelays.Any(delay => delay.Frames != 0);

    /// <summary>
    /// Exact door-transition predicate at <c>$82:E2B5-$82:E2D7</c>: true while any of the
    /// three sixteen-entry SFX rings has an unread request. An already-dequeued request that
    /// is merely waiting for SPC acknowledgement does not keep the native door wait alive.
    /// </summary>
    public bool HasQueuedSounds
    {
        get
        {
            for (int library = 0; library < AudioRomData.Queues.SoundLibraryCount; library++)
            {
                if (((SoundWritePositions[library] - SoundReadPositions[library]) &
                     AudioRomData.Queues.SoundIndexMask) != 0)
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
        Array.Fill(_musicEntries, MusicCommand.Stop);
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
        _musicEntry = MusicCommand.Stop;
        _soundHandlerDowntime = 0;
        MusicDataIndex = 0;
        MusicTrackIndex = 0;
        _pendingImmediateCommands.Clear();

        // $80:841C uploads the common sound driver/sample bank, then clears music port
        // zero and SFX-library-two port two before the first ordinary dispatcher frame.
        _pendingImmediateCommands.Add(new CartridgeAudioCommand.Upload(
            AudioRomData.Assets.InitialAudioBank));
        _pendingImmediateCommands.Add(new CartridgeAudioCommand.WritePort(
            ApuPort.Music, 0));
        _pendingImmediateCommands.Add(new CartridgeAudioCommand.WritePort(
            ApuPort.SoundLibrary2, 0));
    }

    /// <summary>Implements <c>QueueMusic_Delayed8</c> at $80:8FC1.</summary>
    public void QueueMusicDelayed8(MusicCommand command) =>
        QueueMusic(command, MusicCommandDelay.EightFrames, overflow: MusicQueueOverflow.SkipWhenFull);

    /// <summary>Implements <c>QueueMusic_DelayedY</c>, including its minimum delay of eight.</summary>
    public void QueueMusicDelayed(MusicCommand command, MusicCommandDelay delay) =>
        QueueMusic(command, delay, overflow: MusicQueueOverflow.Overwrite);

    /// <summary>Queues the data-bank and track operations performed by ordinary room loading.</summary>
    public void QueueRoomMusic(int dataIndex, int trackIndex)
    {
        // LoadRoomMusic first silences the current track and uploads a changed nonzero
        // room data set. LoadNewMusicTrackIfChanged then queues the room's selected track.
        bool changesMusicData = QueueRoomMusicData(dataIndex);
        // `$82:E0E1` returns immediately for a zero room track. Most post-Ridley Ceres
        // states deliberately contain music `(0, 0)` so track seven and its alarm bed
        // continue through every door. Treating zero as a request to stop replaced that
        // live escape track at the first transition.
        // `$82:E0E6-$E104` compares the requested data/track pair, not the track byte
        // alone. A changed data bank resets the SPC track even when both banks happen to
        // use the same numeric track. The Ceres interstitial ends on bank $33/track 5;
        // Landing Site requests bank $06/track 5 and therefore must queue track 5 again
        // after the upload instead of leaving only the periodic gunship-engine SFX audible.
        // Keep the native byte intact here: room data can request a masked track-zero
        // command, which is not the same as the zero/inherit sentinel checked above.
        // The bank-$80 dispatcher performs the seven-bit mask when the delay expires.
        if (trackIndex != 0 && (changesMusicData || trackIndex != MusicTrackIndex))
            QueueMusicDelayed(
                MusicCommand.FromCartridge(trackIndex),
                MusicCommandDelay.FromDelayedYArgument(6));
    }

    /// <summary>
    /// Executes $82:E0D5 after the door scroll and earlier music requests finish.
    /// A zero room track inherits the current track; otherwise compare the native
    /// data/track pair and preserve the track byte until bank-$80 dispatch masks it.
    /// </summary>
    internal void QueueRoomMusicTrack(int dataIndex, int trackIndex)
    {
        if (trackIndex != 0 && (dataIndex != MusicDataIndex || trackIndex != MusicTrackIndex))
            QueueMusicDelayed(
                MusicCommand.FromCartridge(trackIndex),
                MusicCommandDelay.FromDelayedYArgument(6));
    }

    /// <summary>
    /// Queues only $82:E071's changed nonzero music data bank. Saved-game loading
    /// shares this operation but lets the appearance coroutine schedule its own tracks.
    /// </summary>
    public bool QueueRoomMusicData(int dataIndex)
    {
        if (dataIndex == 0 || dataIndex == MusicDataIndex)
            return false;
        QueueMusicDelayed8(MusicCommand.Stop);
        QueueMusicDelayed8(MusicCommand.LoadData(checked((byte)dataIndex)));
        return true;
    }

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
        int roomTrack = MusicTrackIndex;

        Array.Fill(_musicEntries, MusicCommand.Stop);
        Array.Clear(MusicDelays);
        MusicReadPosition = MusicWritePosition;
        MusicTimer = default;
        _musicEntry = MusicCommand.Stop;

        QueueMusicDelayed8(MusicCommand.SelectTrack(
            AudioRomData.Queues.PermanentItemTrack));
        QueueMusicDelayed(
            MusicCommand.Stop,
            MusicCommandDelay.FromDelayedYArgument(
                AudioRomData.Queues.PermanentItemFanfareFrames));
        QueueMusicDelayed8(MusicCommand.SelectTrackOrStop(roomTrack));
    }

    /// <summary>
    /// Queues <c>Cancel_Sound_Effects</c> at <c>$82:BE17</c> through all three native
    /// SFX rings, in the cartridge's exact library order.
    /// </summary>
    public void QueueCancelSoundEffects()
    {
        QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 6);
        QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 6);
        QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 6);
    }

    /// <summary>Queues one request through retail SFX library one, two, or three.</summary>
    public void QueueSound(SoundEffectId soundEffect, int maximumQueued)
        => QueueSoundAndGetAccumulator(soundEffect, maximumQueued);

    /// <summary>
    /// Executes bank $80's sound-queue call and returns its native accumulator.
    /// Queue-threshold rejection returns sound in the high byte and occupancy
    /// in the low byte; the admitted path swaps them, even when sound execution
    /// is suppressed. Speed Booster animation consumes this otherwise incidental
    /// register result after the library-three call at $90:85A6.
    /// </summary>
    /// <param name="soundEffect">Native sound ID and library selecting one of the three independent SFX rings.</param>
    /// <param name="maximumQueued">Admission threshold from 1 through 15 unread requests; an occupancy at or above this value rejects the request before suppression is tested.</param>
    /// <param name="soundSuppressed">
    /// The native disable-sounds, demo-state, or active Power Bomb guard, evaluated
    /// after the occupancy threshold. Suppression leaves the queue unchanged.
    /// </param>
    public ushort QueueSoundAndGetAccumulator(
        SoundEffectId soundEffect, int maximumQueued, bool soundSuppressed = false)
    {
        if (maximumQueued is < 1 or > AudioRomData.Queues.MaximumSoundOccupancy)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumQueued), maximumQueued, "Retail queue limit must be 1..15.");
        }

        int queue = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        int occupancy = (SoundWritePositions[queue] - SoundReadPositions[queue]) &
            AudioRomData.Queues.SoundIndexMask;
        if (occupancy >= maximumQueued)
            return SnesLittleEndian.Word(unchecked((byte)occupancy), checked((byte)soundEffect.Value));

        ushort accumulator = SnesLittleEndian.Word(checked((byte)soundEffect.Value), unchecked((byte)occupancy));
        if (soundSuppressed || DoorTransitionSoundsDisabled)
            return accumulator;

        byte write = SoundWritePositions[queue];
        byte next = unchecked((byte)(
            (write + 1) & AudioRomData.Queues.SoundIndexMask));
        if (next == SoundReadPositions[queue])
        {
            // A full native ring retains the lower-numbered (higher-priority) request.
            if (soundEffect.Value < SoundQueues[queue, write])
                SoundQueues[queue, write] = checked((byte)soundEffect.Value);
            return accumulator;
        }

        SoundQueues[queue, write] = checked((byte)soundEffect.Value);
        SoundWritePositions[queue] = next;
        SoundQueues[queue, next] = 0;
        return accumulator;
    }

    /// <summary>Runs selected audio dispatch owners and returns their pending APU writes, which only the session's port model consumes.</summary>
    internal IReadOnlyList<CartridgeAudioCommand> AdvanceFrame(
        CartridgeAudioAcknowledgements acknowledgements,
        bool advanceMusicQueue = true,
        int soundEffectHandlerCalls = 1)
    {
        Ensure.AtLeastZero(soundEffectHandlerCalls);
        List<CartridgeAudioCommand> commands = [.. _pendingImmediateCommands];
        _pendingImmediateCommands.Clear();
        if (advanceMusicQueue) HandleMusicQueue(commands);
        for (int call = 0; call < soundEffectHandlerCalls; call++)
            HandleSoundEffects(acknowledgements, commands);
        return commands.Count == 0 ? Array.Empty<CartridgeAudioCommand>() : commands.ToArray();
    }

    /// <summary>
    /// Runs <c>$88:84BD</c>'s music call before a new outer door dispatch.
    /// IRQ-only continuations do not invoke this owner.
    /// </summary>
    internal IReadOnlyList<CartridgeAudioCommand> AdvanceMusicDispatch()
    {
        var commands = new List<CartridgeAudioCommand>();
        HandleMusicQueue(commands);
        return commands.Count == 0 ? Array.Empty<CartridgeAudioCommand>() : commands.ToArray();
    }

    /// <summary>Enqueues a delayed music command, optionally dropping it when the native ring has no free slot.</summary>
    private void QueueMusic(
        MusicCommand command,
        MusicCommandDelay delay,
        MusicQueueOverflow overflow)
    {
        if (delay.Frames < AudioRomData.Queues.MinimumMusicDelayFrames)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay), delay, "Effective music queue delay must be at least eight frames.");
        }

        int next = (MusicWritePosition + 1) & AudioRomData.Queues.MusicIndexMask;
        if (overflow == MusicQueueOverflow.SkipWhenFull && next == MusicReadPosition)
            return;

        _musicEntries[MusicWritePosition] = command;
        MusicDelays[MusicWritePosition] = delay;
        MusicWritePosition = next;
    }

    /// <summary>Advances the bank-80 timer and emits either a data upload or music port write at expiry.</summary>
    private void HandleMusicQueue(List<CartridgeAudioCommand> commands)
    {
        NativeWordCounterStep step = MusicTimer.Decrement();
        MusicTimer = step.Remaining;
        bool timerExpired = step.IsZero;
        if (step.IsNonNegative)
        {
            if (!timerExpired)
                return;

            if (_musicEntry.UsesDataUploadPath)
            {
                // The low byte of a queue word always fits the music-data identity.
                byte dataIndex = checked((byte)_musicEntry.DataIndex);
                MusicDataIndex = dataIndex;
                MusicTrackIndex = byte.MaxValue;
                AudioUploadAssetDefinition upload =
                    AudioAssetCatalogDefinitions.ResolveDataIndex(dataIndex);
                commands.Add(new CartridgeAudioCommand.Upload(upload.Bank));
                MusicTrackIndex = 0;
                // `$80:8F89-$8FA2` retires the data entry and returns. The timer stays zero,
                // so the next entry is fetched only on the following call when it underflows.
                ClearAndAdvanceMusicEntry();
                _soundHandlerDowntime = AudioRomData.Queues.MusicAndSfxDowntimeFrames;
                return;
            }
            else
            {
                MusicTrackIndex = _musicEntry.TrackIndex;
                commands.Add(new CartridgeAudioCommand.WritePort(
                    ApuPort.Music, MusicTrackIndex));
            }

            ClearAndAdvanceMusicEntry();
            _soundHandlerDowntime = AudioRomData.Queues.MusicAndSfxDowntimeFrames;
        }

        if (MusicReadPosition == MusicWritePosition)
        {
            MusicTimer = default;
            return;
        }

        _musicEntry = _musicEntries[MusicReadPosition];
        MusicTimer = NativeCountdown.Start(MusicDelays[MusicReadPosition].Frames);
    }

    /// <summary>Retires the processed music command and its delay before fetching the next entry.</summary>
    private void ClearAndAdvanceMusicEntry()
    {
        _musicEntries[MusicReadPosition] = MusicCommand.Stop;
        MusicDelays[MusicReadPosition] = default;
        MusicReadPosition = (MusicReadPosition + 1) & AudioRomData.Queues.MusicIndexMask;
    }

    /// <summary>Either clears all three SFX ports during music downtime or advances their independent handshakes.</summary>
    private void HandleSoundEffects(
        CartridgeAudioAcknowledgements acknowledgements,
        List<CartridgeAudioCommand> commands)
    {
        if (_soundHandlerDowntime != 0)
        {
            // Guarded by the nonzero test above: the downtime counts down to zero and never wraps.
            _soundHandlerDowntime = Ensure.AtLeastZero(_soundHandlerDowntime - 1);
            for (byte queue = 0;
                queue < AudioRomData.Queues.SoundLibraryCount;
                queue++)
            {
                commands.Add(new CartridgeAudioCommand.WritePort(
                    ApuPorts.ForSoundLibrary(queue), 0));
                _currentSounds[queue] = 0;
            }
            return;
        }

        for (int queue = 0; queue < AudioRomData.Queues.SoundLibraryCount; queue++)
            HandleSoundEffectQueue(
                queue,
                acknowledgements[ApuPorts.ForSoundLibrary(queue)],
                commands);
    }

    /// <summary>Performs request, echo, clear-delay, and zero-echo phases for one SPC sound library.</summary>
    private void HandleSoundEffectQueue(
        int queue,
        int acknowledgement,
        List<CartridgeAudioCommand> commands)
    {
        switch (SoundStates[queue])
        {
            case 0: // $82:8A2C - send the next queued request to its APU port.
                SendNextSound(queue, commands);
                break;

            case 1: // $82:8A55 - wait until the SPC echoes the request byte.
                if (acknowledgement != _currentSounds[queue])
                {
                    commands.Add(new CartridgeAudioCommand.WritePort(
                        ApuPorts.ForSoundLibrary(queue),
                        _currentSounds[queue]));
                }
                else
                {
                    SoundStates[queue] = 2;
                    _soundClearDelays[queue] = 2;
                }
                break;

            case 2: // $82:8A6C - wait two frames, then clear both request and mirror.
                if ((_soundClearDelays[queue] = NativeWordCounter.DecrementByte(_soundClearDelays[queue])) == 0)
                {
                    commands.Add(new CartridgeAudioCommand.WritePort(
                        ApuPorts.ForSoundLibrary(queue), 0));
                    _currentSounds[queue] = 0;
                    SoundStates[queue] = 3;
                }
                break;

            case 3: // $82:8A7C - wait for the SPC's zero acknowledgement, then continue.
                if (acknowledgement != 0)
                {
                    commands.Add(new CartridgeAudioCommand.WritePort(
                        ApuPorts.ForSoundLibrary(queue), 0));
                }
                else
                {
                    SoundStates[queue] = 0;
                    SendNextSound(queue, commands);
                }
                break;

            default:
                throw new InvalidDataException(
                    $"SFX library {queue + 1} entered invalid retail dispatcher state {SoundStates[queue]}.");
        }
    }

    /// <summary>Dequeues one unread SFX request, writes its port, and enters the acknowledgement phase.</summary>
    private void SendNextSound(int queue, List<CartridgeAudioCommand> commands)
    {
        if (SoundReadPositions[queue] == SoundWritePositions[queue])
            return;

        byte read = SoundReadPositions[queue];
        byte sound = SoundQueues[queue, read];
        commands.Add(new CartridgeAudioCommand.WritePort(
            ApuPorts.ForSoundLibrary(queue), sound));
        _currentSounds[queue] = sound;
        SoundReadPositions[queue] = unchecked((byte)(
            (read + 1) & AudioRomData.Queues.SoundIndexMask));
        SoundStates[queue] = 1;
    }

}
