using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// What something leaves behind when it is destroyed: the chance of each kind of pickup, and of nothing. What it
/// offers follows the player's need. A pickup the player has no use for is not offered. The pickups come in two
/// groups: the major ones keep exactly the chances the table gives them, and the minor ones, with "nothing" among
/// them, share out whatever the offered major ones leave, each in proportion to its chance. And a table may name a
/// need: while the player is short of an amount, only the pickups the need lists are offered.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "out_of": 255, "nothing": 20,
///   "minor": [ { "pickup": "pickups/small_energy.json", "chance": 60 }, { "pickup": "pickups/missile.json", "chance": 50 } ],
///   "major": [ { "pickup": "pickups/super_missile.json", "chance": 5 } ],
///   "need": { "name": "energy", "amounts": ["energy", "reserve_energy"], "below": 30, "until": 50,
///             "only": ["pickups/small_energy.json"] }
/// }
/// </code>
/// "out_of" is what every chance is out of; when not given it is the sum of the chances. The need begins when the
/// amounts together fall below "below" and lasts until they reach "until". A plain table lists its pickups under
/// "minor", or "drops", and nothing else.
/// </remarks>
public sealed class DropTable
{
    private readonly int _outOf;
    private readonly int _nothing;
    private readonly IReadOnlyList<(string Pickup, int Chance)> _minor;
    private readonly IReadOnlyList<(string Pickup, int Chance)> _major;
    private readonly Need? _need;

    private DropTable(int outOf, int nothing, IReadOnlyList<(string, int)> minor, IReadOnlyList<(string, int)> major, Need? need)
    {
        _outOf = outOf;
        _nothing = nothing;
        _minor = minor;
        _major = major;
        _need = need;
    }

    /// <summary>A table that never drops anything.</summary>
    public static DropTable Nothing { get; } = new(1, 1, [], [], null);

    /// <summary>Reads a drop table from a data file's group.</summary>
    public static DropTable Read(DataNode data)
    {
        IReadOnlyList<(string, int)> Group(string name) =>
            data.Has(name) ? data.List(name).Select(drop => (drop.Text("pickup"), drop.Int("chance"))).ToList() : [];

        IReadOnlyList<(string Pickup, int Chance)> minor = data.Has("minor") ? Group("minor") : Group("drops");
        IReadOnlyList<(string Pickup, int Chance)> major = Group("major");
        int nothing = data.Has("nothing") ? data.Int("nothing") : 0;
        if (nothing < 0 || minor.Concat(major).Any(drop => drop.Chance < 0))
            throw data.Problem("gives a chance below zero");
        int sum = nothing + minor.Sum(drop => drop.Chance) + major.Sum(drop => drop.Chance);
        int outOf = data.Has("out_of") ? data.Int("out_of") : sum;
        if (outOf < sum)
            throw data.Problem($"gives chances that sum to {sum}, past the {outOf} they are out of");

        Need? need = null;
        if (data.Has("need"))
        {
            DataNode group = data.Get("need");
            need = new Need(
                group.Text("name"),
                group.List("amounts").Select(amount => amount.AsText()).ToList(),
                group.Int("below"),
                group.Int("until"),
                group.List("only").Select(pickup => pickup.AsText()).ToList());
        }

        return new DropTable(Math.Max(1, outOf), nothing, minor, major, need);
    }

    /// <summary>
    /// Rolls the table for a player with <paramref name="inventory"/>, and returns the data file of the pickup dropped,
    /// or null for nothing. One number is drawn from <paramref name="random"/>.
    /// </summary>
    /// <param name="random">The gameplay random generator.</param>
    /// <param name="inventory">The player's inventory, which decides what is useful and what is needed.</param>
    /// <param name="isUseful">Says whether the player has a use for a pickup, named by its data file.</param>
    public string? Roll(GameRandom random, Inventory inventory, Func<string, bool> isUseful)
    {
        if (_minor.Count == 0 && _major.Count == 0)
            return null;

        // The number drawn is 1 through what the chances are out of.
        int drawn;
        do
        {
            drawn = random.NextBelow(_outOf + 1);
        }
        while (drawn == 0);

        bool inNeed = false;
        if (_need is { } need)
        {
            int held = need.Amounts.Sum(inventory.Amount);
            if (held < need.Below)
                inventory.SetInNeed(need.Name, true);
            else if (held >= need.Until)
                inventory.SetInNeed(need.Name, false);
            inNeed = inventory.IsInNeed(need.Name);
        }

        var offeredMinor = new List<(string? Pickup, int Chance)>();
        int leftByMajor = _outOf;
        var offeredMajor = new List<(string Pickup, int Chance)>();
        if (inNeed)
        {
            offeredMinor.AddRange(_minor.Where(drop => _need!.Only.Contains(drop.Pickup)).Select(drop => ((string?)drop.Pickup, drop.Chance)));
        }
        else
        {
            offeredMinor.AddRange(_minor.Where(drop => isUseful(drop.Pickup)).Select(drop => ((string?)drop.Pickup, drop.Chance)));
            offeredMinor.Add((null, _nothing));
            foreach ((string pickup, int chance) in _major)
            {
                if (!isUseful(pickup))
                    continue;
                offeredMajor.Add((pickup, chance));
                leftByMajor -= chance;
            }
        }

        int pooled = offeredMinor.Sum(drop => drop.Chance);
        int reached = 0;
        if (pooled != 0)
        {
            foreach ((string? pickup, int chance) in offeredMinor)
            {
                reached += leftByMajor * chance / pooled;
                if (reached >= drawn)
                    return pickup;
            }
        }

        foreach ((string pickup, int chance) in offeredMajor)
        {
            reached += chance;
            if (reached >= drawn)
                return pickup;
        }

        return null;
    }

    /// <summary>A need a table follows: while the amounts together are short, only the listed pickups are offered.</summary>
    private sealed record Need(string Name, IReadOnlyList<string> Amounts, int Below, int Until, IReadOnlyList<string> Only);
}
