using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A weapon's definition: the hit it delivers, its speed, cooldown and cost, how many of its shots may be live at once,
/// how its projectile behaves, and the properties that switched-on equipment adds to its shots.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "name": "beam", "damage": 20, "kinds": ["beam"], "speed": 4, "cooldown": 8, "max_live": 3,
///   "costs": "", "cost": 0, "requires": "",
///   "charge_updates": 60, "charged_damage_times": 3, "charged_kind": "charged",
///   "properties": [ { "equipment": "ice beam", "kinds": ["ice"], "damage": 10 } ],
///   "projectile": { "width": 8, "height": 4, "picture": "shots/beam.png" }
/// }
/// </code>
/// </remarks>
public sealed class WeaponStats
{
    /// <summary>The weapon's name.</summary>
    public string Name { get; private init; } = "";

    /// <summary>The damage of its hit before any property adds to it.</summary>
    public int Damage { get; private init; }

    /// <summary>The kinds its hit always carries.</summary>
    public IReadOnlyList<HitKind> Kinds { get; private init; } = [];

    /// <summary>How fast its projectile is launched.</summary>
    public Pixels Speed { get; private init; }

    /// <summary>How many updates must pass between shots.</summary>
    public int Cooldown { get; private init; }

    /// <summary>How many of its shots may be live at once; zero for no limit.</summary>
    public int MaxLive { get; private init; }

    /// <summary>The inventory amount a shot costs, by name; empty when it costs nothing.</summary>
    public string Costs { get; private init; } = "";

    /// <summary>How much of that amount a shot costs.</summary>
    public int Cost { get; private init; }

    /// <summary>The equipment that must be switched on to use the weapon; empty when it needs none.</summary>
    public string Requires { get; private init; } = "";

    /// <summary>How many updates the attack must be held to charge a shot; zero for a weapon that does not charge.</summary>
    public int ChargeUpdates { get; private init; }

    /// <summary>A charged shot does its damage this many times over.</summary>
    public int ChargedDamageTimes { get; private init; } = 1;

    /// <summary>The kind a charged shot also carries; empty for none.</summary>
    public string ChargedKind { get; private init; } = "";

    /// <summary>What switched-on equipment adds to each shot.</summary>
    public IReadOnlyList<WeaponProperty> Properties { get; private init; } = [];

    /// <summary>How its projectile behaves.</summary>
    public ProjectileSettings Projectile { get; private init; } = new();

    /// <summary>Reads a weapon from its data file.</summary>
    public static WeaponStats Read(DataNode data)
    {
        var stats = new WeaponStats
        {
            Name = data.Text("name"),
            Damage = data.Int("damage"),
            Kinds = data.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList(),
            Speed = data.Distance("speed"),
            Cooldown = data.Has("cooldown") ? data.Int("cooldown") : 0,
            MaxLive = data.Has("max_live") ? data.Int("max_live") : 0,
            Costs = data.Has("costs") ? data.Text("costs") : "",
            Cost = data.Has("cost") ? data.Int("cost") : 0,
            Requires = data.Has("requires") ? data.Text("requires") : "",
            ChargeUpdates = data.Has("charge_updates") ? data.Int("charge_updates") : 0,
            ChargedDamageTimes = data.Has("charged_damage_times") ? data.Int("charged_damage_times") : 1,
            ChargedKind = data.Has("charged_kind") ? data.Text("charged_kind") : "",
            Properties = data.Has("properties")
                ? data.List("properties").Select(property => new WeaponProperty(
                    property.Text("equipment"),
                    property.Has("kinds") ? property.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList() : [],
                    property.Has("damage") ? property.Int("damage") : 0)).ToList()
                : [],
            Projectile = data.Has("projectile") ? ProjectileSettings.Read(data.Get("projectile")) : new ProjectileSettings(),
        };
        if (stats.Damage < 0 || stats.Cooldown < 0 || stats.MaxLive < 0 || stats.Cost < 0 || stats.Kinds.Count == 0)
            throw data.Problem("gives a negative number, or a weapon whose hit carries no kind");
        return stats;
    }

    /// <summary>The hit one shot delivers for a player carrying <paramref name="inventory"/>.</summary>
    /// <param name="inventory">The player's inventory, which says which properties are switched on.</param>
    /// <param name="charged">Whether the shot is charged.</param>
    public Hit HitFor(Inventory inventory, bool charged)
    {
        int damage = Damage;
        var kinds = new List<HitKind>(Kinds);
        foreach (WeaponProperty property in Properties)
        {
            if (!inventory.IsOn(property.Equipment))
                continue;
            damage += property.Damage;
            kinds.AddRange(property.Kinds);
        }

        if (charged)
        {
            damage *= ChargedDamageTimes;
            if (ChargedKind.Length > 0)
                kinds.Add(new HitKind(ChargedKind));
        }

        return new Hit(damage, HitSide.Player, [.. kinds.Distinct()]);
    }
}
