using Engine.Content;

namespace Engine.Shared;

/// <summary>
/// The reactions of something that can be hit. The one that applies to a hit is the one whose kinds the hit all
/// carries and that names the most of them; of several that name equally many, the first listed. Something with no
/// matching reaction takes the hit's full damage. Nothing is worked out from a hit's separate kinds.
/// </summary>
public sealed class Reactions
{
    /// <summary>The reactions, in the order they were listed.</summary>
    private readonly IReadOnlyList<Reaction> _reactions;

    /// <summary>The given reactions.</summary>
    public Reactions(IEnumerable<Reaction> reactions) => _reactions = [.. reactions];

    /// <summary>No reactions: every hit does its full damage.</summary>
    public static Reactions None { get; } = new([]);

    /// <summary>The reaction that applies to <paramref name="hit"/>.</summary>
    public Reaction To(Hit hit)
    {
        Reaction? best = null;
        foreach (Reaction reaction in _reactions)
        {
            if (!reaction.Kinds.All(hit.Carries))
                continue;
            if (best is null || reaction.Kinds.Count > best.Kinds.Count)
                best = reaction;
        }

        return best ?? Reaction.FullDamage;
    }

    /// <summary>
    /// Reads reactions from a data file's list. Each entry names its kinds and, optionally, its damage (as "none",
    /// "half", "full", "double" or a whole-number multiple), a status with when and how long, how long the target is
    /// unhittable after taking the damage ("unhittable_updates") and after being put in the status
    /// ("status_unhittable_updates"), whether the projectile is turned back, and whether the hit "passes": takes no
    /// notice of the target and goes on.
    /// </summary>
    /// <remarks>
    /// <code>
    /// "reactions": [
    ///   { "kinds": ["beam"], "damage": "none" },
    ///   { "kinds": ["beam", "ice"], "damage": "none", "status": "frozen", "status_when": "always", "status_updates": 400,
    ///     "status_unhittable_updates": 10 }
    /// ]
    /// </code>
    /// </remarks>
    public static Reactions Read(IReadOnlyList<DataNode> list)
    {
        var reactions = new List<Reaction>();
        foreach (DataNode entry in list)
        {
            var kinds = entry.List("kinds").Select(kind => new HitKind(kind.AsText())).ToArray();
            (int times, int over) = entry.Has("damage") ? ReadDamage(entry.Get("damage")) : (1, 1);
            string status = entry.Has("status") ? entry.Text("status") : "";
            StatusWhen when = entry.Has("status_when")
                ? entry.Get("status_when").AsEnum<StatusWhen>()
                : status.Length > 0 ? StatusWhen.Always : StatusWhen.Never;
            if (when != StatusWhen.Never && status.Length == 0)
                throw entry.Problem("says when a status applies but names no status");
            reactions.Add(new Reaction(
                [.. kinds], times, over, status, when,
                entry.Has("status_updates") ? entry.Int("status_updates") : 0,
                entry.Has("unhittable_updates") ? entry.Int("unhittable_updates") : 0,
                entry.Has("status_unhittable_updates") ? entry.Int("status_unhittable_updates") : 0,
                entry.Has("turns_back") && entry.Flag("turns_back"),
                entry.Has("passes") && entry.Flag("passes")));
        }

        return new Reactions(reactions);
    }

    /// <summary>Reads how much of a hit's damage is taken.</summary>
    private static (int Times, int Over) ReadDamage(DataNode damage)
    {
        string text = damage.ToString();
        return text switch
        {
            "none" => (0, 1),
            "half" => (1, 2),
            "full" => (1, 1),
            "double" => (2, 1),
            _ => int.TryParse(text, out int times) && times >= 0
                ? (times, 1)
                : throw damage.Problem("is not \"none\", \"half\", \"full\", \"double\" or a whole number"),
        };
    }
}
