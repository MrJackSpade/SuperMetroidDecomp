using SuperMetroid.Core;
using SuperMetroid.Core.Programs;
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
    /// <summary>Each timed record's instruction pointer.</summary>
    private readonly Dictionary<SamusProjectileFrame, int> _recordPositions;
    /// <summary>Each segment's list entry and the word just before it.</summary>
    private readonly IReadOnlyDictionary<SamusProjectileSegment, ListPosition> _positions;

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
        _positions = positions;
        beatPositions = Enum.GetValues<SamusProjectileSegment>().ToDictionary(program => program, Layout);
        _places = BuildPlaces();
        _recordPositions = BuildRecordPositions();
    }

    /// <summary>The word just before <paramref name="program"/>'s list.</summary>
    internal ushort Prefix(SamusProjectileSegment program) => checked((ushort)_positions[program].PrecedingWord);

    /// <summary>The instruction pointer <paramref name="place"/> stands for.</summary>
    internal ushort Position(SamusProjectileSegmentBeat place) => beatPositions[place.Segment][place.Beat];

    /// <summary>The instruction pointer timed record <paramref name="record"/> stands at.</summary>
    internal ushort RecordPosition(SamusProjectileFrame record) => checked((ushort)_recordPositions[record]);

    /// <summary>The animation place a nonzero instruction pointer <paramref name="pointer"/> stands for, when it is a list position.</summary>
    internal bool TryPlaceAt(int pointer, out SamusProjectileSegmentBeat place) =>
        _places.TryGetValue(checked((ushort)pointer), out place);

    /// <summary>The segments in list order.</summary>
    private IEnumerable<SamusProjectileSegment> ListOrder => _positions.OrderBy(position => position.Value.Entry).Select(position => position.Key);

    /// <summary>
    /// Each place's pointer: eight bytes per timed record from the entry, then the segment's end, which a run on into the
    /// next authored list shares with that list's entry.
    /// </summary>
    private ushort[] Layout(SamusProjectileSegment program)
    {
        int beats = SamusProjectileAnimations.Segments.Beats(program).Count;
        int entry = _positions[program].Entry;
        return [.. Enumerable.Range(0, beats + 1).Select(beat => checked((ushort)(entry + beat * TimedRecordByteCount)))];
    }

    /// <summary>Indexes every list position; an entry stands for its own segment's start.</summary>
    private Dictionary<ushort, SamusProjectileSegmentBeat> BuildPlaces()
    {
        var places = ListOrder.ToDictionary(program => checked((ushort)_positions[program].Entry), program => new SamusProjectileSegmentBeat(program, 0));
        foreach (SamusProjectileSegment program in ListOrder)
        {
            ushort[] pointers = beatPositions[program];
            for (int beat = 0; beat < pointers.Length; beat++)
                places.TryAdd(pointers[beat], new(program, beat));
        }
        return places;
    }

    /// <summary>Finds the one pointer each timed record is shown from.</summary>
    private Dictionary<SamusProjectileFrame, int> BuildRecordPositions()
    {
        var places = new Dictionary<SamusProjectileFrame, int>();
        foreach (SamusProjectileSegment program in ListOrder)
        {
            IReadOnlyList<AnimationBeat<SamusProjectileFrame, NoAnimationCue>> beats = SamusProjectileAnimations.Segments.Beats(program);
            for (int beat = 0; beat < beats.Count; beat++)
            {
                if (beats[beat] is not AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame shown)
                    continue;
                int place = beatPositions[program][beat];
                SamusProjectileFrame record = shown.Shown;
                if (record.Animation != program)
                    throw new InvalidDataException($"{program} beat {beat} shows {record}, a record of another segment.");
                if (places.TryGetValue(record, out int known) && known != place)
                    throw new InvalidDataException($"Timed projectile record {record} stands at $93:{known:X4} and $93:{place:X4}.");
                places[record] = place;
            }
        }
        foreach (SamusProjectileFrame record in SamusProjectileRecordOrder.Frames)
        {
            if (!places.ContainsKey(record))
                throw new InvalidDataException($"Timed projectile record {record} stands in no list.");
        }
        return places;
    }
}
