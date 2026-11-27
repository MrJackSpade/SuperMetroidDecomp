using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// What a flying shot leaves behind it as it goes, read from the file its data names ("trail"): how many of its
/// updates pass before the first is left and between each after, and, for each step of the shot's flying timeline,
/// the sparkles left while that step shows. The data file looks like this:
/// <code>
/// { "first_after": 4, "every": 4,
///   "steps": [ [], [ { "x": -3, "y": 2, "timeline": "samus/weapons/trails/sparkles/ice/ice.json" } ] ] }
/// </code>
/// A shot has a file for each way it flies, named as its timelines are, since where a sparkle is left differs by the
/// way flown. A step with no sparkles leaves none.
/// </summary>
/// <param name="FirstAfter">On which of the shot's updates the first is left, counted from one.</param>
/// <param name="Every">How many updates pass from one leaving to the next.</param>
/// <param name="Steps">The sparkles left while each step of the shot's flying timeline shows, by the step's place.</param>
public sealed record ProjectileTrail(int FirstAfter, int Every, IReadOnlyList<IReadOnlyList<ProjectileTrail.Sparkle>> Steps)
{
    /// <summary>One thing left: where, from the shot's own place, and the data file of the timeline it plays.</summary>
    public sealed record Sparkle(PixelOffset At, string Timeline);

    /// <summary>Reads a trail from its data file.</summary>
    /// <exception cref="ContentException">It gives a count that is not at least one.</exception>
    public static ProjectileTrail Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var trail = new ProjectileTrail(
            data.Int("first_after"),
            data.Int("every"),
            [.. data.List("steps").Select(step => (IReadOnlyList<Sparkle>)[.. step.Items().Select(sparkle =>
                new Sparkle(PixelOffset.FromWhole(sparkle.Int("x"), sparkle.Int("y")), sparkle.Text("timeline")))])]);
        if (trail.FirstAfter < 1 || trail.Every < 1)
            throw data.Problem("gives a trail left after no updates");
        return trail;
    }
}
