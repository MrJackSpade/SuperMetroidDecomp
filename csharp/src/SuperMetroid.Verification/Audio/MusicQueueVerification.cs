using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Checks the music queue (#1692) against the original's eight-position ring for every request sequence retail play
/// reaches: the queue holds the requests in order and plays each after its delay, as the original's handler does.
/// </summary>
/// <remarks>
/// The original keeps the music queue in eight positions with a fetch and a write position: a request queued with the
/// shortest delay ($80:8FC1) is dropped when only one position is free, a scheduled one ($80:8FF7) is written regardless
/// and then makes the queue read as empty. Neither happens while the queue holds seven requests or fewer, and no retail
/// path holds more than five at once (<c>docs/compatibility-layer.md</c>), so the port keeps no positions; this check
/// covers every count up to the seven the ring takes before it is full.
/// </remarks>
internal static class MusicQueueVerification
{
    /// <summary>The most requests the original's queue holds before it is full.</summary>
    private const int MostHeldBeforeFull = 7;

    /// <summary>
    /// Every sequence of one to seven requests, each queued with the shortest delay or scheduled with its own, queued all
    /// on one update or one per handler call, from every place the original's ring can start at: the port and a
    /// transcription of the original's music handler ($80:8F0C), queue routines and queued-music test ($80:8EF4) dispatch
    /// the same requests on the same handler calls and report the same queued music after every call.
    /// </summary>
    internal static void VerifyMusicQueueMatchesOriginalRing()
    {
        int sequences = 0;
        for (int count = 1; count <= MostHeldBeforeFull; count++)
        {
            for (int scheduledMask = 0; scheduledMask < 1 << count; scheduledMask++)
            {
                foreach (bool onePerCall in new[] { false, true })
                {
                    for (int ringStart = 0; ringStart < OriginalMusicQueue.Positions; ringStart++)
                    {
                        Compare(count, scheduledMask, onePerCall, ringStart);
                        sequences++;
                    }
                }
            }
        }
        Console.WriteLine($"Music queue: {sequences} request sequences of up to seven play as the original's ring plays them.");
    }

    /// <summary>Runs one request sequence through the port and the original and compares every handler call.</summary>
    private static void Compare(int count, int scheduledMask, bool onePerCall, int ringStart)
    {
        string sequence = $"{count} requests, scheduled mask {scheduledMask:B}, {(onePerCall ? "one per call" : "all at once")}, ring start {ringStart}";
        MusicRequest[] requests = [.. Enumerable.Range(0, count).Select(Request)];
        int[] delays = [.. Enumerable.Range(0, count).Select(index => IsScheduled(scheduledMask, index) ? 8 + 3 * index : 8)];

        AudioQueues port = AudioQueues.Standalone();
        port.AdvanceFrame(default);
        var original = new OriginalMusicQueue(ringStart);

        int queued = 0;
        for (int call = 0; call < 1000; call++)
        {
            while (queued < count && (!onePerCall || queued == call))
            {
                if (IsScheduled(scheduledMask, queued))
                {
                    port.ScheduleMusic(requests[queued], MusicQueueDelay.AtLeastShortest(delays[queued]));
                    original.QueueScheduled(queued, delays[queued]);
                }
                else
                {
                    port.QueueMusic(requests[queued]);
                    original.QueueShortest(queued);
                }
                queued++;
                if (onePerCall)
                    break;
            }

            AudioRequest[] portMusic = [.. port.AdvanceFrame(default).Where(IsMusic)];
            AudioRequest[] originalMusic = [.. original.Handle().Select(index => Dispatched(requests[index]))];
            AssertSequenceEqual(originalMusic, portMusic, $"{sequence}: handler call {call} dispatches what the original's does");
            AssertEqual(original.HasQueuedMusic, port.HasQueuedMusic, $"{sequence}: queued music after handler call {call}");
            if (queued == count && !original.HasQueuedMusic && original.IsIdle)
                return;
        }
        throw new InvalidOperationException($"{sequence}: the original's queue did not drain.");
    }

    /// <summary>
    /// The request at <paramref name="index"/>: the third loads a data set (the handler then fetches on its next call), the
    /// fifth stops the music, every other plays its own track.
    /// </summary>
    private static MusicRequest Request(int index) => index switch
    {
        2 => MusicRequest.LoadDataSet(MusicDataSet.EmptyCrateria),
        4 => MusicRequest.Stop,
        _ => MusicRequest.SelectTrack(Enum.GetValues<MusicTrack>()[index]),
    };

    /// <summary>Whether the request at <paramref name="index"/> is scheduled rather than queued with the shortest delay.</summary>
    private static bool IsScheduled(int scheduledMask, int index) => (scheduledMask >> index & 1) != 0;

    /// <summary>Whether a handler request is the music's own.</summary>
    private static bool IsMusic(AudioRequest request) =>
        request is AudioRequest.PlayMusicTrack or AudioRequest.StopMusic or AudioRequest.ChangeMusicDataSet;

    /// <summary>What the handler requests when it dispatches <paramref name="request"/>.</summary>
    private static AudioRequest Dispatched(MusicRequest request) => request switch
    {
        MusicRequest.ChangeDataSet change => new AudioRequest.ChangeMusicDataSet(change.DataSet),
        MusicRequest.PlayTrack play => AudioRequest.ForTrack(play.Track),
        _ => AudioRequest.ForTrack(null),
    };

    /// <summary>
    /// A transcription of the original's music queue: eight track and delay words, fetch and write positions stepping by
    /// two within the ring, and the handler's timer and fetched entry. An entry holds the request's index, with the data set
    /// flag on the index of a data set load.
    /// </summary>
    private sealed class OriginalMusicQueue
    {
        /// <summary>The ring's positions.</summary>
        internal const int Positions = 8;

        /// <summary>The flag an entry's word carries for a data set load.</summary>
        private const ushort DataSetFlag = 0x8000;

        /// <summary>Each position's entry word.</summary>
        private readonly ushort[] _tracks = new ushort[Positions];
        /// <summary>Each position's delay; zero for an empty position.</summary>
        private readonly ushort[] _delays = new ushort[Positions];
        /// <summary>The fetch position, stepping by two.</summary>
        private int _read;
        /// <summary>The write position, stepping by two.</summary>
        private int _write;
        /// <summary>The handler's countdown of the fetched entry's delay.</summary>
        private ushort _timer;
        /// <summary>The entry the handler fetched.</summary>
        private ushort _entry;

        /// <summary>An empty ring whose fetch and write positions both stand at <paramref name="start"/>.</summary>
        internal OriginalMusicQueue(int start) => _read = _write = start * 2;

        /// <summary>Whether the handler is waiting out no fetched request.</summary>
        internal bool IsIdle => _timer == 0;

        /// <summary>The queued-music test: whether any position's delay is nonzero.</summary>
        internal bool HasQueuedMusic => _delays.Any(delay => delay != 0);

        /// <summary>The shortest-delay queue routine: written unless only one position is free.</summary>
        internal void QueueShortest(int index)
        {
            if (((_write + 2) & 0xE) == _read)
                return;
            Write(Entry(index), 8);
        }

        /// <summary>The scheduled queue routine: written regardless, its delay lengthened to eight.</summary>
        internal void QueueScheduled(int index, int delay) => Write(Entry(index), Math.Max(delay, 8));

        /// <summary>One handler call; returns the indices of the requests it dispatched.</summary>
        internal List<int> Handle()
        {
            var dispatched = new List<int>();
            bool expires = _timer == 1;
            _timer = unchecked((ushort)(_timer - 1));
            if ((_timer & 0x8000) == 0)
            {
                if (!expires)
                    return dispatched;
                dispatched.Add(_entry & ~DataSetFlag & 0xFF);
                Retire();
                if ((_entry & DataSetFlag) != 0)
                    return dispatched;
            }
            if (_read == _write)
            {
                _timer = 0;
            }
            else
            {
                _entry = _tracks[_read >> 1];
                _timer = _delays[_read >> 1];
            }
            return dispatched;
        }

        /// <summary>The entry word of the request at <paramref name="index"/>.</summary>
        private static ushort Entry(int index) =>
            (ushort)(index == 2 ? DataSetFlag | index : index);

        /// <summary>Writes an entry and its delay at the write position and steps it.</summary>
        private void Write(ushort entry, int delay)
        {
            _tracks[_write >> 1] = entry;
            _delays[_write >> 1] = (ushort)delay;
            _write = (_write + 2) & 0xE;
        }

        /// <summary>Empties the fetch position and steps it.</summary>
        private void Retire()
        {
            _tracks[_read >> 1] = 0;
            _delays[_read >> 1] = 0;
            _read = (_read + 2) & 0xE;
        }
    }
}
