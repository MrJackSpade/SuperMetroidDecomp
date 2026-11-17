using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where each Samus projectile segment's places stand in the cartridge's bank-$93 lists (#1301, #1411, #1474, #1600):
/// extraction's private knowledge of the instruction pointer each animation place stands for, which the glitch paths'
/// list arithmetic and fresh trails read. The import pipeline decodes those reads into the Samus overread services; the
/// game never sees a list position.
/// </summary>
internal sealed class SamusProjectileListLayout
{
    /// <summary>Bytes in one timed record: its timer, spritemap, radii and animation frame.</summary>
    internal const int TimedRecordByteCount = 8;

    /// <summary>
    /// Each place's instruction pointer, by segment: one per beat, then the segment's end, where its goto or delete
    /// stands or the next list starts; checks read it through their access extension.
    /// </summary>
    internal readonly Dictionary<SamusProjectileSegment, ushort[]> beatPositions;
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

    /// <summary>Where one segment's list stands: its entry and the word just before it.</summary>
    /// <param name="Entry">The list's first instruction pointer.</param>
    /// <param name="PrecedingWord">The word just before the list, which a trail spawned at the list's start reads as its animation frame.</param>
    internal readonly record struct ListPosition(int Entry, int PrecedingWord);

    /// <summary>Lays every segment's places out from its list position.</summary>
    /// <param name="positions">Every segment's list entry and preceding word.</param>
    /// <exception cref="InvalidDataException">A segment has no position, or a timed record stands at no pointer or at two.</exception>
    internal SamusProjectileListLayout(IReadOnlyDictionary<SamusProjectileSegment, ListPosition> positions)
    {
        Ensure.NotNull(positions);
        foreach (SamusProjectileSegment program in Enum.GetValues<SamusProjectileSegment>())
        {
            if (!positions.ContainsKey(program))
                throw new InvalidDataException($"The projectile list layout has no position for {program}.");
        }
        this.positions = positions;
        beatPositions = Enum.GetValues<SamusProjectileSegment>().ToDictionary(program => program, Layout);
        _places = BuildPlaces();
        recordFrames = BuildRecordFrames();
        _recordPositions = BuildRecordPositions(recordFrames);
    }

    /// <summary>The instruction pointer <paramref name="place"/> stands for.</summary>
    internal ushort Position(SamusProjectileSegmentBeat place) => beatPositions[place.Segment][place.Beat];

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
    /// Each place's pointer: eight bytes per timed record from the entry, then the segment's end, which a run on into the
    /// next authored list shares with that list's entry.
    /// </summary>
    private ushort[] Layout(SamusProjectileSegment program)
    {
        int beats = SamusProjectileAnimations.Segments.Beats(program).Count;
        int entry = positions[program].Entry;
        return [.. Enumerable.Range(0, beats + 1).Select(beat => checked((ushort)(entry + beat * TimedRecordByteCount)))];
    }

    /// <summary>Indexes every list position; an entry stands for its own segment's start.</summary>
    private Dictionary<ushort, SamusProjectileSegmentBeat> BuildPlaces()
    {
        var places = ListOrder.ToDictionary(program => checked((ushort)positions[program].Entry), program => new SamusProjectileSegmentBeat(program, 0));
        foreach (SamusProjectileSegment program in ListOrder)
        {
            ushort[] pointers = beatPositions[program];
            for (int beat = 0; beat < pointers.Length; beat++)
                places.TryAdd(pointers[beat], new(program, beat));
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
            for (int beat = 0; beat < beats.Count; beat++)
            {
                if (beats[beat] is not AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.Frame shown)
                    continue;
                int place = beatPositions[program][beat];
                if (!frames.TryAdd(place, shown.Shown) && frames[place] != shown.Shown)
                    throw new InvalidDataException($"The timed projectile record at $93:{place:X4} shows both {frames[place]} and {shown.Shown}.");
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
