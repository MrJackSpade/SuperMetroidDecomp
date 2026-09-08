using SuperMetroid.Core.Hardware;

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
    private readonly MusicCommand[] _musicEntries =
        new MusicCommand[AudioRomData.Queues.MusicCapacity];
    private readonly MusicCommandDelay[] _musicDelays =
        new MusicCommandDelay[AudioRomData.Queues.MusicCapacity];
    private readonly byte[,] _soundQueues = new byte[
        AudioRomData.Queues.SoundLibraryCount,
        AudioRomData.Queues.SoundCapacity];
    private readonly byte[] _soundReadPositions =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    private readonly byte[] _soundWritePositions =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    private readonly byte[] _soundStates = new byte[AudioRomData.Queues.SoundLibraryCount];
    private readonly byte[] _currentSounds = new byte[AudioRomData.Queues.SoundLibraryCount];
    private readonly byte[] _soundClearDelays =
        new byte[AudioRomData.Queues.SoundLibraryCount];
    private readonly List<CartridgeAudioCommand> _pendingImmediateCommands = [];

    private byte _musicReadPosition;
    private byte _musicWritePosition;
    private ushort _musicTimer;
    private MusicCommand _musicEntry;
    private byte _soundHandlerDowntime;

    public CartridgeAudioState() => Reset();

    /// <summary>Music-data set most recently uploaded by a processed queue command.</summary>
    public byte MusicDataIndex { get; private set; }

    /// <summary>Track most recently written to APU port zero.</summary>
    public byte MusicTrackIndex { get; private set; }

    /// <summary>Exact <c>HasQueuedMusic</c> result: any occupied delayed-command slot.</summary>
    public bool HasQueuedMusic => _musicDelays.Any(delay => delay.Frames != 0);

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
                if (((_soundWritePositions[library] - _soundReadPositions[library]) &
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
        Array.Clear(_musicEntries);
        Array.Clear(_musicDelays);
        Array.Clear(_soundQueues);
        Array.Clear(_soundReadPositions);
        Array.Clear(_soundWritePositions);
        Array.Clear(_soundStates);
        Array.Clear(_currentSounds);
        Array.Clear(_soundClearDelays);
        _musicReadPosition = 0;
        _musicWritePosition = 0;
        _musicTimer = 0;
        _musicEntry = default;
        _soundHandlerDowntime = 0;
        MusicDataIndex = 0;
        MusicTrackIndex = 0;
        _pendingImmediateCommands.Clear();

        // $80:841C uploads the common sound driver/sample bank, then clears music port
        // zero and SFX-library-two port two before the first ordinary dispatcher frame.
        _pendingImmediateCommands.Add(CartridgeAudioCommand.Upload(
            AudioRomData.Assets.InitialAudioBank));
        _pendingImmediateCommands.Add(CartridgeAudioCommand.WritePort(
            AudioRomData.Apu.MusicPort, 0));
        _pendingImmediateCommands.Add(CartridgeAudioCommand.WritePort(
            AudioRomData.Apu.LibraryTwoPort, 0));
    }

    /// <summary>Implements <c>QueueMusic_Delayed8</c> at $80:8FC1.</summary>
    public void QueueMusicDelayed8(MusicCommand command) =>
        QueueMusic(command, MusicCommandDelay.EightFrames, requireFreeSlot: true);

    /// <summary>Implements <c>QueueMusic_DelayedY</c>, including its minimum delay of eight.</summary>
    public void QueueMusicDelayed(MusicCommand command, MusicCommandDelay delay) =>
        QueueMusic(command, delay, requireFreeSlot: false);

    /// <summary>Queues the data-bank and track operations performed by ordinary room loading.</summary>
    public void QueueRoomMusic(byte dataIndex, byte trackIndex)
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
    /// Queues only $82:E071's changed nonzero music data bank. Saved-game loading
    /// shares this operation but lets the appearance coroutine schedule its own tracks.
    /// </summary>
    public bool QueueRoomMusicData(byte dataIndex)
    {
        if (dataIndex == 0 || dataIndex == MusicDataIndex)
            return false;
        QueueMusicDelayed8(MusicCommand.Stop);
        QueueMusicDelayed8(MusicCommand.LoadData(dataIndex));
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
        byte roomTrack = MusicTrackIndex;

        Array.Clear(_musicEntries);
        Array.Clear(_musicDelays);
        _musicReadPosition = _musicWritePosition;
        _musicTimer = 0;
        _musicEntry = default;

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
    public void QueueSound(SoundEffectId soundEffect, byte maximumQueued)
    {
        if (maximumQueued is < 1 or > AudioRomData.Queues.MaximumSoundOccupancy)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumQueued), maximumQueued, "Retail queue limit must be 1..15.");
        }

        int queue = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        int occupancy = (_soundWritePositions[queue] - _soundReadPositions[queue]) &
            AudioRomData.Queues.SoundIndexMask;
        if (occupancy >= maximumQueued)
            return;

        byte write = _soundWritePositions[queue];
        byte next = unchecked((byte)(
            (write + 1) & AudioRomData.Queues.SoundIndexMask));
        if (next == _soundReadPositions[queue])
        {
            // A full native ring retains the lower-numbered (higher-priority) request.
            if (soundEffect.Value < _soundQueues[queue, write])
                _soundQueues[queue, write] = soundEffect.Value;
            return;
        }

        _soundQueues[queue, write] = soundEffect.Value;
        _soundWritePositions[queue] = next;
        _soundQueues[queue, next] = 0;
    }

    /// <summary>Runs one NMI's music/SFX handlers and returns only this frame's APU writes.</summary>
    public IReadOnlyList<CartridgeAudioCommand> AdvanceFrame(
        ISnesAddressSpace bus,
        CartridgeAudioAcknowledgements acknowledgements)
    {
        ArgumentNullException.ThrowIfNull(bus);
        List<CartridgeAudioCommand> commands = [.. _pendingImmediateCommands];
        _pendingImmediateCommands.Clear();
        HandleMusicQueue(bus, commands);
        HandleSoundEffects(acknowledgements, commands);
        return commands.Count == 0 ? Array.Empty<CartridgeAudioCommand>() : commands.ToArray();
    }

    private void QueueMusic(
        MusicCommand command,
        MusicCommandDelay delay,
        bool requireFreeSlot)
    {
        if (delay.Frames < AudioRomData.Queues.MinimumMusicDelayFrames)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay), delay, "Effective music queue delay must be at least eight frames.");
        }

        byte next = unchecked((byte)(
            (_musicWritePosition + 1) & AudioRomData.Queues.MusicIndexMask));
        if (requireFreeSlot && next == _musicReadPosition)
            return;

        _musicEntries[_musicWritePosition] = command;
        _musicDelays[_musicWritePosition] = delay;
        _musicWritePosition = next;
    }

    private void HandleMusicQueue(ISnesAddressSpace bus, List<CartridgeAudioCommand> commands)
    {
        bool timerExpired = _musicTimer-- == 1;
        if ((_musicTimer & AudioRomData.MusicWireFormat.ActiveTimerBit) == 0)
        {
            if (!timerExpired)
                return;

            if (_musicEntry.UsesDataUploadPath)
            {
                MusicDataIndex = _musicEntry.DataIndex;
                MusicTrackIndex = byte.MaxValue;
                int tableEntry = AudioRomData.Assets.MusicPointerTable + _musicEntry.DataIndex;
                int uploadAddress = ReadLong(bus, tableEntry);
                commands.Add(CartridgeAudioCommand.Upload(uploadAddress));
                MusicTrackIndex = 0;
            }
            else
            {
                MusicTrackIndex = _musicEntry.TrackIndex;
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.MusicPort, MusicTrackIndex));
            }

            ClearAndAdvanceMusicEntry();
            _soundHandlerDowntime = AudioRomData.Queues.MusicAndSfxDowntimeFrames;
        }

        if (_musicReadPosition == _musicWritePosition)
        {
            _musicTimer = 0;
            return;
        }

        _musicEntry = _musicEntries[_musicReadPosition];
        _musicTimer = _musicDelays[_musicReadPosition].Frames;
    }

    private void ClearAndAdvanceMusicEntry()
    {
        _musicEntries[_musicReadPosition] = default;
        _musicDelays[_musicReadPosition] = default;
        _musicReadPosition = unchecked((byte)(
            (_musicReadPosition + 1) & AudioRomData.Queues.MusicIndexMask));
    }

    private void HandleSoundEffects(
        CartridgeAudioAcknowledgements acknowledgements,
        List<CartridgeAudioCommand> commands)
    {
        if (_soundHandlerDowntime != 0)
        {
            _soundHandlerDowntime--;
            for (byte queue = 0;
                queue < AudioRomData.Queues.SoundLibraryCount;
                queue++)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    unchecked((byte)(queue + AudioRomData.Apu.FirstSoundPort)), 0));
                _currentSounds[queue] = 0;
            }
            return;
        }

        for (int queue = 0; queue < AudioRomData.Queues.SoundLibraryCount; queue++)
            HandleSoundEffectQueue(
                queue,
                acknowledgements[queue + AudioRomData.Apu.FirstSoundPort],
                commands);
    }

    private void HandleSoundEffectQueue(
        int queue,
        byte acknowledgement,
        List<CartridgeAudioCommand> commands)
    {
        switch (_soundStates[queue])
        {
            case 0: // $82:8A2C - send the next queued request to its APU port.
                SendNextSound(queue, commands);
                break;

            case 1: // $82:8A55 - wait until the SPC echoes the request byte.
                if (acknowledgement != _currentSounds[queue])
                {
                    commands.Add(CartridgeAudioCommand.WritePort(
                        unchecked((byte)(queue + AudioRomData.Apu.FirstSoundPort)),
                        _currentSounds[queue]));
                }
                else
                {
                    _soundStates[queue] = 2;
                    _soundClearDelays[queue] = 2;
                }
                break;

            case 2: // $82:8A6C - wait two frames, then clear both request and mirror.
                if (--_soundClearDelays[queue] == 0)
                {
                    commands.Add(CartridgeAudioCommand.WritePort(
                        unchecked((byte)(queue + AudioRomData.Apu.FirstSoundPort)), 0));
                    _currentSounds[queue] = 0;
                    _soundStates[queue] = 3;
                }
                break;

            case 3: // $82:8A7C - wait for the SPC's zero acknowledgement, then continue.
                if (acknowledgement != 0)
                {
                    commands.Add(CartridgeAudioCommand.WritePort(
                        unchecked((byte)(queue + AudioRomData.Apu.FirstSoundPort)), 0));
                }
                else
                {
                    _soundStates[queue] = 0;
                    SendNextSound(queue, commands);
                }
                break;

            default:
                throw new InvalidDataException(
                    $"SFX library {queue + 1} entered invalid retail dispatcher state {_soundStates[queue]}.");
        }
    }

    private void SendNextSound(int queue, List<CartridgeAudioCommand> commands)
    {
        if (_soundReadPositions[queue] == _soundWritePositions[queue])
            return;

        byte read = _soundReadPositions[queue];
        byte sound = _soundQueues[queue, read];
        commands.Add(CartridgeAudioCommand.WritePort(
            unchecked((byte)(queue + AudioRomData.Apu.FirstSoundPort)), sound));
        _currentSounds[queue] = sound;
        _soundReadPositions[queue] = unchecked((byte)(
            (read + 1) & AudioRomData.Queues.SoundIndexMask));
        _soundStates[queue] = 1;
    }

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(address + 1) << 8) |
        (bus.ReadByte(address + 2) << 16);
}
