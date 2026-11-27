using Engine.Content;

namespace Engine.Shared;

/// <summary>
/// The timelines of one thing, each under a name: an actor's walk, its attack, its death. They are read together from
/// the thing's data file, and asking for a name the file does not have is an error naming the file.
/// </summary>
/// <remarks>
/// <code>
/// "timelines": {
///   "walk": { "steps": [ { "picture": "enemies/waver/walk_0.png", "updates": 4 }, { "picture": "enemies/waver/walk_1.png", "updates": 4 } ], "loops_to": 0 },
///   "turn": { "steps": [ { "picture": "enemies/waver/turn_0.png", "updates": 6, "cues": ["turned"] } ] }
/// }
/// </code>
/// </remarks>
public sealed class TimelineSet
{
    private readonly string _file;
    private readonly Dictionary<string, FrameTimeline> _timelines;

    private TimelineSet(string file, Dictionary<string, FrameTimeline> timelines)
    {
        _file = file;
        _timelines = timelines;
    }

    /// <summary>A set with no timelines, for a thing that is shown by one picture or not at all.</summary>
    public static TimelineSet None { get; } = new("", new Dictionary<string, FrameTimeline>(StringComparer.Ordinal));

    /// <summary>The names of the timelines, in the order the file gives them.</summary>
    public IReadOnlyCollection<string> Names => _timelines.Keys;

    /// <summary>Reads the timelines in a data file's group.</summary>
    public static TimelineSet Read(DataNode group)
    {
        var timelines = new Dictionary<string, FrameTimeline>(StringComparer.Ordinal);
        foreach ((string name, DataNode timeline) in group.Entries())
            timelines[name] = FrameTimeline.Read(timeline);
        return new TimelineSet(group.File, timelines);
    }

    /// <summary>Reads the timelines under "timelines" in a thing's data file, or none when the file has no such group.</summary>
    public static TimelineSet ReadFrom(DataNode data) => data.Has("timelines") ? Read(data.Get("timelines")) : None;

    /// <summary>
    /// The same timelines with every picture named as <paramref name="named"/> names it: for a thing whose pictures'
    /// names hold a word that is decided where the thing is placed.
    /// </summary>
    public TimelineSet WithPictures(Func<string, string> named)
    {
        ArgumentNullException.ThrowIfNull(named);
        var timelines = new Dictionary<string, FrameTimeline>(StringComparer.Ordinal);
        foreach ((string name, FrameTimeline timeline) in _timelines)
            timelines[name] = new FrameTimeline(timeline.Steps.Select(step => step with { Picture = named(step.Picture) }).ToList(), timeline.LoopsTo);
        return new TimelineSet(_file, timelines);
    }

    /// <summary>Whether the set has a timeline named <paramref name="name"/>.</summary>
    public bool Has(string name) => _timelines.ContainsKey(name);

    /// <summary>The timeline named <paramref name="name"/>.</summary>
    /// <exception cref="ContentException">The set has no timeline of that name.</exception>
    public FrameTimeline Get(string name) =>
        _timelines.TryGetValue(name, out FrameTimeline? timeline)
            ? timeline
            : throw new ContentException(_file.Length > 0 ? _file : "(no data file)", $"has no timeline named \"{name}\".");
}
