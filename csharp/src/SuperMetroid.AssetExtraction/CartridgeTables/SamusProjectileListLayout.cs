using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where each Samus projectile segment's places stand in the cartridge's bank-$93 lists (#1301, #1411, #1474, #1600):
/// extraction's private knowledge of the instruction pointer each animation place stands for, which the glitch paths'
/// list arithmetic and fresh trails read. The import pipeline decodes those reads into the Samus overread services; the
/// game never sees a list position. A list stores one timed record per beat of its segment, except a list that repeats one
/// still picture, whose records are all the segment's one held beat (#2347): the place before its first record is the
/// segment's start, and every later place is the held picture's.
/// </summary>
internal sealed class SamusProjectileListLayout
{
    /// <summary>Bytes in one timed record: its timer, spritemap, radii and animation frame.</summary>
    internal const int TimedRecordByteCount = 8;

    /// <summary>
    /// Each list place's instruction pointer, by segment: one per timed record the list stores, then the list's end, where
    /// its goto or delete stands or the next list starts; checks read it through their access extension.
    /// </summary>
    internal readonly Dictionary<SamusProjectileSegment, ushort[]> recordPositions;
    /// <summary>The place each instruction pointer stands for: a segment's start, else the one place at that pointer.</summary>
    private readonly Dictionary<ushort, SamusProjectileSegmentBeat> _places;
    /// <summary>
    /// The instruction pointers of the timed records that show each frame, in list order: a frame an animation shows again
    /// with the same picture and gameplay is shown from every record that stores it (#2274).
    /// </summary>
    private readonly Dictionary<SamusProjectileFrameId, List<int>> _recordPositions;
    /// <summary>The frame each timed record's instruction pointer shows.</summary>
    internal readonly Dictionary<int, SamusProjectileFrameId> recordFrames;
    /// <summary>Each segment's list entry and the word just before it.</summary>
    internal readonly IReadOnlyDictionary<SamusProjectileSegment, ListPosition> positions;

    /// <summary>Where one segment's list stands: its entry, the word just before it and the timed records it stores.</summary>
    /// <param name="Entry">The list's first instruction pointer.</param>
    /// <param name="PrecedingWord">The word just before the list, which a trail spawned at the list's start reads as its animation frame.</param>
    /// <param name="RecordCount">The timed records the list stores before its goto or delete, or before the next list starts.</param>
    internal readonly record struct ListPosition(int Entry, int PrecedingWord, int RecordCount);

    /// <summary>Lays every segment's places out from its list position.</summary>
    /// <param name="positions">Every segment's list entry and preceding word.</param>
    /// <exception cref="InvalidDataException">
    /// A segment has no position, its list stores neither one record per beat nor one still picture, or a timed record
    /// stands at no pointer or at two.
    /// </exception>
    internal SamusProjectileListLayout(IReadOnlyDictionary<SamusProjectileSegment, ListPosition> positions)
    {
        Ensure.NotNull(positions);
        foreach (SamusProjectileSegment program in Enum.GetValues<SamusProjectileSegment>())
        {
            if (!positions.ContainsKey(program))
                throw new InvalidDataException($"The projectile list layout has no position for {program}.");
            int beats = SamusProjectileAnimations.Segments.Beats(program).Count;
            if (positions[program].RecordCount != beats && !IsStillPicture(program))
                throw new InvalidDataException(
                    $"Projectile list {program} stores {positions[program].RecordCount} timed records for its segment's {beats} beats.");
        }
        this.positions = positions;
        recordPositions = Enum.GetValues<SamusProjectileSegment>().ToDictionary(program => program, Layout);
        _places = BuildPlaces();
        recordFrames = BuildRecordFrames();
        _recordPositions = BuildRecordPositions(recordFrames);
    }

    /// <summary>
    /// The instruction pointer <paramref name="place"/> stands for: its beat's record, or the list's end once the segment's
    /// beats are played.
    /// </summary>
    internal ushort Position(SamusProjectileSegmentBeat place) =>
        recordPositions[place.Segment][place.Beat == SamusProjectileAnimations.Segments.Beats(place.Segment).Count
            ? positions[place.Segment].RecordCount
            : place.Beat];

    /// <summary>
    /// The animation place the list place before record <paramref name="record"/> of <paramref name="program"/>'s list
    /// stands for, the list's end at its record count: the same beat, or, in a list repeating one still picture, the
    /// segment's start before its first record and the held picture after it.
    /// </summary>
    internal SamusProjectileSegmentBeat PlaceOfRecord(SamusProjectileSegment program, int record) =>
        positions[program].RecordCount == SamusProjectileAnimations.Segments.Beats(program).Count
            ? new(program, record)
            : new(program, Math.Min(record, 1));

    /// <summary>
    /// Whether <paramref name="program"/>'s segment is one held still picture, which its list may store as several records
    /// showing that picture.
    /// </summary>
    private static bool IsStillPicture(SamusProjectileSegment program) =>
        SamusProjectileAnimations.Segments.Beats(program) is [AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.HeldFrame];

    /// <summary>The instruction pointer of the first timed record that shows <paramref name="record"/>.</summary>
    internal ushort RecordPosition(SamusProjectileFrameId record) => checked((ushort)_recordPositions[record][0]);

    /// <summary>The instruction pointers of every timed record that shows <paramref name="record"/>, in list order.</summary>
    internal IReadOnlyList<int> RecordPositions(SamusProjectileFrameId record) => _recordPositions[record];

    /// <summary>The animation place a nonzero instruction pointer <paramref name="pointer"/> stands for, when it is a list position.</summary>
    internal bool TryPlaceAt(int pointer, out SamusProjectileSegmentBeat place) =>
        _places.TryGetValue(checked((ushort)pointer), out place);

    /// <summary>The segments in list order.</summary>
    private IEnumerable<SamusProjectileSegment> ListOrder => positions.OrderBy(position => position.Value.Entry).Select(position => position.Key);

    /// <summary>
    /// Each list place's pointer: eight bytes per timed record from the entry, then the list's end, which a run on into the
    /// next authored list shares with that list's entry.
    /// </summary>
    private ushort[] Layout(SamusProjectileSegment program)
    {
        int records = positions[program].RecordCount;
        int entry = positions[program].Entry;
        return [.. Enumerable.Range(0, records + 1).Select(record => checked((ushort)(entry + record * TimedRecordByteCount)))];
    }

    /// <summary>Indexes every list position; an entry stands for its own segment's start.</summary>
    private Dictionary<ushort, SamusProjectileSegmentBeat> BuildPlaces()
    {
        var places = ListOrder.ToDictionary(program => checked((ushort)positions[program].Entry), program => new SamusProjectileSegmentBeat(program, 0));
        foreach (SamusProjectileSegment program in ListOrder)
        {
            ushort[] pointers = recordPositions[program];
            for (int record = 0; record < pointers.Length; record++)
                places.TryAdd(pointers[record], PlaceOfRecord(program, record));
        }
        return places;
    }

    /// <summary>Finds the one frame each timed record's pointer shows.</summary>
    private Dictionary<int, SamusProjectileFrameId> BuildRecordFrames()
    {
        var frames = new Dictionary<int, SamusProjectileFrameId>();
        foreach (SamusProjectileSegment program in ListOrder)
        {
            IReadOnlyList<AnimationBeat<SamusProjectileFrameId, NoAnimationCue>> beats = SamusProjectileAnimations.Segments.Beats(program);
            for (int record = 0; record < positions[program].RecordCount; record++)
            {
                // Every record of a list repeating one still picture shows that picture, the segment's one beat.
                int beat = positions[program].RecordCount == beats.Count ? record : 0;
                if (beats[beat].ShownFrame is not { } shown)
                    continue;
                int place = recordPositions[program][record];
                if (!frames.TryAdd(place, shown) && frames[place] != shown)
                    throw new InvalidDataException($"The timed projectile record at $93:{place:X4} shows both {frames[place]} and {shown}.");
            }
        }
        return frames;
    }

    /// <summary>Finds every pointer each frame is shown from, in list order, from the frame each record shows.</summary>
    private static Dictionary<SamusProjectileFrameId, List<int>> BuildRecordPositions(Dictionary<int, SamusProjectileFrameId> frames)
    {
        var positions = new Dictionary<SamusProjectileFrameId, List<int>>();
        foreach ((int place, SamusProjectileFrameId record) in frames)
        {
            if (!positions.TryGetValue(record, out List<int>? places))
                positions.Add(record, places = []);
            places.Add(place);
        }
        foreach (SamusProjectileFrameId record in SamusProjectileFrameOrder.Frames)
        {
            if (!positions.ContainsKey(record))
                throw new InvalidDataException($"Timed projectile record {record} stands in no list.");
        }
        return positions;
    }
}
