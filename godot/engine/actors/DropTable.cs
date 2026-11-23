using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// What something leaves behind when it is destroyed: the chance of each kind of pickup, and of nothing. What it
/// offers follows the player's need: a pickup the player has no use for is not offered when the table says so, and a
/// table may favour a pickup while one of the player's amounts is low.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "nothing": 20,
///   "skip_useless": true,
///   "drops": [ { "pickup": "pickups/small_energy.json", "chance": 60 }, { "pickup": "pickups/missile.json", "chance": 20 } ],
///   "favour": [ { "pickup": "pickups/small_energy.json", "while": "energy", "below_percent": 30, "times": 3 } ]
/// }
/// </code>
/// </remarks>
public sealed class DropTable
{
    private readonly int _nothing;
    private readonly bool _skipUseless;
    private readonly IReadOnlyList<(string Pickup, int Chance)> _drops;
    private readonly IReadOnlyList<(string Pickup, string While, int BelowPercent, int Times)> _favours;

    private DropTable(
        int nothing, bool skipUseless, IReadOnlyList<(string, int)> drops, IReadOnlyList<(string, string, int, int)> favours)
    {
        _nothing = nothing;
        _skipUseless = skipUseless;
        _drops = drops;
        _favours = favours;
    }

    /// <summary>A table that never drops anything.</summary>
    public static DropTable Nothing { get; } = new(1, false, [], []);

    /// <summary>Reads a drop table from a data file's group.</summary>
    public static DropTable Read(DataNode data)
    {
        var drops = data.List("drops").Select(drop => (drop.Text("pickup"), drop.Int("chance"))).ToList();
        var favours = data.Has("favour")
            ? data.List("favour").Select(favour =>
                (favour.Text("pickup"), favour.Text("while"), favour.Int("below_percent"), favour.Int("times"))).ToList()
            : [];
        int nothing = data.Has("nothing") ? data.Int("nothing") : 0;
        if (nothing < 0 || drops.Any(drop => drop.Item2 < 0) || favours.Any(favour => favour.Item4 < 1))
            throw data.Problem("gives a chance below zero or a favour of less than one time");
        return new DropTable(nothing, data.Has("skip_useless") && data.Flag("skip_useless"), drops, favours);
    }

    /// <summary>
    /// Rolls the table for a player with <paramref name="inventory"/>, and returns the data file of the pickup dropped,
    /// or null for nothing. One draw is taken from <paramref name="random"/> whenever anything could be dropped.
    /// </summary>
    /// <param name="random">The gameplay random generator.</param>
    /// <param name="inventory">The player's inventory, which decides what is useful and what is low.</param>
    /// <param name="addsTo">Says which inventory amount a pickup, named by its data file, adds to.</param>
    public string? Roll(GameRandom random, Inventory inventory, Func<string, string> addsTo)
    {
        var offered = new List<(string Pickup, int Chance)>();
        foreach ((string pickup, int chance) in _drops)
        {
            string amount = addsTo(pickup);
            if (_skipUseless && inventory.IsFull(amount))
                continue;
            int weighted = chance;
            foreach ((string favoured, string whileAmount, int belowPercent, int times) in _favours)
            {
                if (favoured == pickup && inventory.Capacity(whileAmount) > 0 &&
                    inventory.Amount(whileAmount) * 100 < inventory.Capacity(whileAmount) * belowPercent)
                    weighted *= times;
            }

            if (weighted > 0)
                offered.Add((pickup, weighted));
        }

        int total = _nothing + offered.Sum(drop => drop.Chance);
        if (total == 0 || offered.Count == 0)
            return null;
        int draw = random.NextBelow(total);
        foreach ((string pickup, int chance) in offered)
        {
            if (draw < chance)
                return pickup;
            draw -= chance;
        }

        return null;
    }
}
