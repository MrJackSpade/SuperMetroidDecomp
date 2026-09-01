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
    private const int InitialAudioBank = 0xcf8000;
    private const int MusicPointerTable = 0x8fe7e1;
    private const int MusicQueueMask = 0x07;
    private const int SoundQueueMask = 0x0f;
    private const int MusicAndSfxDowntimeFrames = 8;

    private readonly ushort[] _musicEntries = new ushort[8];
    private readonly ushort[] _musicDelays = new ushort[8];
    private readonly byte[,] _soundQueues = new byte[3, 16];
    private readonly byte[] _soundReadPositions = new byte[3];
    private readonly byte[] _soundWritePositions = new byte[3];
    private readonly byte[] _soundStates = new byte[3];
    private readonly byte[] _currentSounds = new byte[3];
    private readonly byte[] _soundClearDelays = new byte[3];
    private readonly List<CartridgeAudioCommand> _pendingImmediateCommands = [];

    private byte _musicReadPosition;
    private byte _musicWritePosition;
    private ushort _musicTimer;
    private ushort _musicEntry;
    private byte _soundHandlerDowntime;

    public CartridgeAudioState() => Reset();

    /// <summary>Music-data set most recently uploaded by a processed queue command.</summary>
    public byte MusicDataIndex { get; private set; }

    /// <summary>Track most recently written to APU port zero.</summary>
    public byte MusicTrackIndex { get; private set; }

    /// <summary>Exact <c>HasQueuedMusic</c> result: any occupied delayed-command slot.</summary>
    public bool HasQueuedMusic => _musicDelays.Any(delay => delay != 0);

    /// <summary>
    /// Exact door-transition predicate at <c>$82:E2B5-$82:E2D7</c>: true while any of the
    /// three sixteen-entry SFX rings has an unread request. An already-dequeued request that
    /// is merely waiting for SPC acknowledgement does not keep the native door wait alive.
    /// </summary>
    public bool HasQueuedSounds
    {
        get
        {
            for (int library = 0; library < 3; library++)
            {
                if (((_soundWritePositions[library] - _soundReadPositions[library]) &
                     SoundQueueMask) != 0)
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
        _musicEntry = 0;
        _soundHandlerDowntime = 0;
        MusicDataIndex = 0;
        MusicTrackIndex = 0;
        _pendingImmediateCommands.Clear();

        // $80:841C uploads the common sound driver/sample bank, then clears music port
        // zero and SFX-library-two port two before the first ordinary dispatcher frame.
        _pendingImmediateCommands.Add(CartridgeAudioCommand.Upload(InitialAudioBank));
        _pendingImmediateCommands.Add(CartridgeAudioCommand.WritePort(0, 0));
        _pendingImmediateCommands.Add(CartridgeAudioCommand.WritePort(2, 0));
    }

    /// <summary>Implements <c>QueueMusic_Delayed8</c> at $80:8FC1.</summary>
    public void QueueMusicDelayed8(ushort entry) => QueueMusic(entry, 8, requireFreeSlot: true);

    /// <summary>Implements <c>QueueMusic_DelayedY</c>, including its minimum delay of eight.</summary>
    public void QueueMusicDelayed(ushort entry, ushort delay) =>
        QueueMusic(entry, Math.Max(delay, (ushort)8), requireFreeSlot: false);

    /// <summary>Queues the data-bank and track operations performed by ordinary room loading.</summary>
    public void QueueRoomMusic(byte dataIndex, byte trackIndex)
    {
        // LoadRoomMusic first silences the current track and uploads a changed nonzero
        // room data set. LoadNewMusicTrackIfChanged then queues the room's selected track.
        if (dataIndex != 0 && dataIndex != MusicDataIndex)
        {
            QueueMusicDelayed8(0);
            QueueMusicDelayed8(unchecked((ushort)(0xff00 | dataIndex)));
        }
        if (trackIndex != MusicTrackIndex)
            QueueMusicDelayed(trackIndex, 6);
    }

    /// <summary>Queues one request through retail SFX library one, two, or three.</summary>
    public void QueueSound(byte library, byte soundId, byte maximumQueued)
    {
        if (library is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(library), library, "SFX library must be 1..3.");
        if (maximumQueued is < 1 or > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumQueued), maximumQueued, "Retail queue limit must be 1..15.");
        }

        int queue = library - 1;
        int occupancy = (_soundWritePositions[queue] - _soundReadPositions[queue]) & SoundQueueMask;
        if (occupancy >= maximumQueued)
            return;

        byte write = _soundWritePositions[queue];
        byte next = unchecked((byte)((write + 1) & SoundQueueMask));
        if (next == _soundReadPositions[queue])
        {
            // A full native ring retains the lower-numbered (higher-priority) request.
            if (soundId < _soundQueues[queue, write])
                _soundQueues[queue, write] = soundId;
            return;
        }

        _soundQueues[queue, write] = soundId;
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

    private void QueueMusic(ushort entry, ushort delay, bool requireFreeSlot)
    {
        byte next = unchecked((byte)((_musicWritePosition + 1) & MusicQueueMask));
        if (requireFreeSlot && next == _musicReadPosition)
            return;

        _musicEntries[_musicWritePosition] = entry;
        _musicDelays[_musicWritePosition] = delay;
        _musicWritePosition = next;
    }

    private void HandleMusicQueue(ISnesAddressSpace bus, List<CartridgeAudioCommand> commands)
    {
        bool timerExpired = _musicTimer-- == 1;
        if ((_musicTimer & 0x8000) == 0)
        {
            if (!timerExpired)
                return;

            if ((_musicEntry & 0x8000) != 0)
            {
                MusicDataIndex = unchecked((byte)_musicEntry);
                MusicTrackIndex = byte.MaxValue;
                int tableEntry = MusicPointerTable + unchecked((byte)_musicEntry);
                int uploadAddress = ReadLong(bus, tableEntry);
                commands.Add(CartridgeAudioCommand.Upload(uploadAddress));
                MusicTrackIndex = 0;
            }
            else
            {
                MusicTrackIndex = unchecked((byte)(_musicEntry & 0x7f));
                commands.Add(CartridgeAudioCommand.WritePort(0, MusicTrackIndex));
            }

            ClearAndAdvanceMusicEntry();
            _soundHandlerDowntime = MusicAndSfxDowntimeFrames;
        }

        if (_musicReadPosition == _musicWritePosition)
        {
            _musicTimer = 0;
            return;
        }

        _musicEntry = _musicEntries[_musicReadPosition];
        _musicTimer = _musicDelays[_musicReadPosition];
    }

    private void ClearAndAdvanceMusicEntry()
    {
        _musicEntries[_musicReadPosition] = 0;
        _musicDelays[_musicReadPosition] = 0;
        _musicReadPosition = unchecked((byte)((_musicReadPosition + 1) & MusicQueueMask));
    }

    private void HandleSoundEffects(
        CartridgeAudioAcknowledgements acknowledgements,
        List<CartridgeAudioCommand> commands)
    {
        if (_soundHandlerDowntime != 0)
        {
            _soundHandlerDowntime--;
            for (byte queue = 0; queue < 3; queue++)
            {
                commands.Add(CartridgeAudioCommand.WritePort(unchecked((byte)(queue + 1)), 0));
                _currentSounds[queue] = 0;
            }
            return;
        }

        for (int queue = 0; queue < 3; queue++)
            HandleSoundEffectQueue(queue, acknowledgements[queue + 1], commands);
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
                        unchecked((byte)(queue + 1)), _currentSounds[queue]));
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
                    commands.Add(CartridgeAudioCommand.WritePort(unchecked((byte)(queue + 1)), 0));
                    _currentSounds[queue] = 0;
                    _soundStates[queue] = 3;
                }
                break;

            case 3: // $82:8A7C - wait for the SPC's zero acknowledgement, then continue.
                if (acknowledgement != 0)
                {
                    commands.Add(CartridgeAudioCommand.WritePort(unchecked((byte)(queue + 1)), 0));
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
        commands.Add(CartridgeAudioCommand.WritePort(unchecked((byte)(queue + 1)), sound));
        _currentSounds[queue] = sound;
        _soundReadPositions[queue] = unchecked((byte)((read + 1) & SoundQueueMask));
        _soundStates[queue] = 1;
    }

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(address + 1) << 8) |
        (bus.ReadByte(address + 2) << 16);
}
