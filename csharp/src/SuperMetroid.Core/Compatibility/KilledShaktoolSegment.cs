namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the rest of a Shaktool chain reads in place of the segment a hit killed, for the remainder of that update
/// (#1543): an emptied segment, with no heading, orbit, angular speed, orientation, motion or motion flags. The kill
/// deletes the whole chain, but the segments after the killed one in the update's order still run that update; they
/// orbit, orient and reverse against the emptied segment, and what they write to it they read back the same update.
/// </summary>
/// <remarks>
/// Recorded by the chain's fatal-shot reaction, once per kill, and consulted whenever a segment of the chain reaches
/// another for the rest of that update. The deleted chain leaves the room before the next update, so nothing reads the
/// remains after it. Evidence: the chain's routines address their neighbours' enemy records without testing them, and
/// the kill clears the killed segment's record. Created with each room's Shaktool behaviour, for its room.
/// </remarks>
/// <typeparam name="TPlace">An enemy place.</typeparam>
/// <typeparam name="TSegment">The state of one chain segment; a new one is the emptied segment.</typeparam>
[CompatibilityService("the rest of a Shaktool chain runs out the update one of its segments is killed in, reading the killed segment emptied")]
public sealed class KilledShaktoolSegment<TPlace, TSegment>
    where TPlace : class
    where TSegment : class, new()
{
    /// <summary>The place of the segment last killed this room, or null before any kill.</summary>
    private TPlace? _place;

    /// <summary>What the rest of that segment's chain reads in its place.</summary>
    private TSegment? _remains;

    /// <summary>Records that a hit killed the segment in <paramref name="place"/>, whose remains start emptied.</summary>
    public void RecordKill(TPlace place)
    {
        _place = Ensure.NotNull(place);
        _remains = new TSegment();
    }

    /// <summary>The emptied segment the rest of the chain reads in place of <paramref name="place"/>, or null when no kill left one there.</summary>
    public TSegment? RemainsAt(TPlace place) => ReferenceEquals(Ensure.NotNull(place), _place) ? _remains : null;
}
