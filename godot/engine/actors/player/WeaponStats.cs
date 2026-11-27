using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A weapon's definition: the hit it delivers, its speed, cooldown and cost, how many of its shots may be live at once,
/// how its projectiles behave, and how switched-on equipment changes its shots.
/// </summary>
/// <remarks>
/// Equipment changes a weapon's shots in one of two ways. <c>properties</c> add to it: each switched-on piece adds its
/// damage and kinds. <c>variants</c> replace it: the variant that applies is the one whose equipment is all switched
/// on and that names the most of it, and its damage, kinds and projectiles are the shot's. A weapon whose combinations
/// do not add up lists variants.
/// <para>
/// A weapon that charges counts the updates its attack is held, the first included, and fires a plain shot on the
/// first of them; let go, it fires a charged shot once "charge_updates" were counted, and with "part_charge_fires" a
/// plain one when fewer were. A weapon that does not charge fires on a press; with a "held_cooldown" it also fires
/// while the attack stays held, whenever the cooldown allows, and waits that long after such a shot; with
/// "repeats_every" it fires every so many held updates instead.
/// </para>
/// <para>
/// "max_live" is how many places there are for its shots out at once. Weapons that give the same "pool" share those
/// places. With "places" one firing takes that many of them together, however many projectiles it launches;
/// without, each projectile takes one of its own. A place is given up once the projectile has ended and what it left
/// on ending is gone.
/// </para>
/// <code>
/// {
///   "name": "beam", "damage": 20, "kinds": ["beam"], "speed": 4, "diagonal_speed": 2.668, "cooldown": 15, "max_live": 5,
///   "costs": "", "cost": 0, "requires": "", "inherits_speed": true, "repeats_every": 25,
///   "charge_updates": 60, "charge_needs": "charge_beam", "charged_damage_times": 3, "charged_kind": "charged", "charged_cooldown": 30,
///   "sound": "beam", "charged_sound": "charged_beam",
///   "properties": [ { "equipment": "ice_beam", "kinds": ["ice"], "damage": 10 } ],
///   "projectile": { "width": 8, "height": 4, "picture": "shots/beam.png" },
///   "variants": [ { "when": ["ice_beam", "wave_beam"], "damage": 60, "kinds": ["beam", "ice", "wave"],
///                   "shots": [ { "side": 0, "projectile": { "motion": "accelerating" } } ],
///                   "charged_shots": [ { "side": 0, "projectile": { "motion": "accelerating" } } ] } ]
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

    /// <summary>How fast its projectile is launched straight across or straight up or down.</summary>
    public Pixels Speed { get; private init; }

    /// <summary>How fast its projectile is launched along each axis when launched diagonally.</summary>
    public Pixels DiagonalSpeed { get; private init; }

    /// <summary>Whether its projectile is launched with the launcher's own speed across added to its own.</summary>
    public bool InheritsSpeed { get; private init; }

    /// <summary>How many updates must pass between shots.</summary>
    public int Cooldown { get; private init; }

    /// <summary>How many updates must pass after a charged shot.</summary>
    public int ChargedCooldown { get; private init; }

    /// <summary>How many updates pass between shots while the attack is held without charging; zero when holding does not repeat.</summary>
    public int RepeatsEvery { get; private init; }

    /// <summary>
    /// How many updates must pass after a shot fired by holding the attack down, for a weapon that fires for as long as
    /// it is held; zero for one that does not.
    /// </summary>
    public int HeldCooldown { get; private init; }

    /// <summary>Whether letting the attack go before a full charge fires a plain shot.</summary>
    public bool PartChargeFires { get; private init; }

    /// <summary>
    /// How many places its pool has for shots that are out at once; zero for no limit. A shot is out until every
    /// projectile of it has ended and what each left on ending, its effect or its burst, is gone too.
    /// </summary>
    public int MaxLive { get; private init; }

    /// <summary>
    /// The name of the pool its shots take their places in. Weapons that name the same pool share its places, and must
    /// give it the same number of them; a weapon that names none has a pool of its own.
    /// </summary>
    public string Pool { get; private init; } = "";

    /// <summary>
    /// How many of the pool's places one firing takes, however many projectiles it launches; zero when each
    /// projectile takes a place of its own and gives it up by itself.
    /// </summary>
    public int PlacesTaken { get; private init; }

    /// <summary>The inventory amount a shot costs, by name; empty when it costs nothing.</summary>
    public string Costs { get; private init; } = "";

    /// <summary>How much of that amount a shot costs.</summary>
    public int Cost { get; private init; }

    /// <summary>The equipment that must be switched on to use the weapon; empty when it needs none.</summary>
    public string Requires { get; private init; } = "";

    /// <summary>How many updates the attack must be held to charge a shot; zero for a weapon that does not charge.</summary>
    public int ChargeUpdates { get; private init; }

    /// <summary>The equipment that must be switched on for the weapon to charge; empty when it needs none.</summary>
    public string ChargeNeeds { get; private init; } = "";

    /// <summary>A charged shot does its damage this many times over.</summary>
    public int ChargedDamageTimes { get; private init; } = 1;

    /// <summary>The kind a charged shot also carries; empty for none.</summary>
    public string ChargedKind { get; private init; } = "";

    /// <summary>The sound asked for as a shot is fired, by name; empty for none.</summary>
    public string Sound { get; private init; } = "";

    /// <summary>The sound asked for as a charged shot is fired, by name; empty for the plain shot's.</summary>
    public string ChargedSound { get; private init; } = "";

    /// <summary>The scene of its projectile, when the projectile is a class of its own; empty for a plain projectile.</summary>
    public string Scene { get; private init; } = "";

    /// <summary>What switched-on equipment adds to each shot.</summary>
    public IReadOnlyList<WeaponProperty> Properties { get; private init; } = [];

    /// <summary>The shots that replace the weapon's own when their equipment is switched on.</summary>
    public IReadOnlyList<WeaponVariant> Variants { get; private init; } = [];

    /// <summary>How its projectile behaves.</summary>
    public ProjectileSettings Projectile { get; private init; } = new();

    /// <summary>Whether the weapon charges for a player carrying <paramref name="inventory"/>.</summary>
    public bool Charges(Inventory inventory) =>
        ChargeUpdates > 0 && (ChargeNeeds.Length == 0 || inventory.IsOn(ChargeNeeds)) && (VariantFor(inventory)?.Charges ?? true);

    /// <summary>Reads a weapon from its data file.</summary>
    public static WeaponStats Read(DataNode data)
    {
        ProjectileSettings projectile = data.Has("projectile") ? ProjectileSettings.Read(data.Get("projectile")) : new ProjectileSettings();
        Pixels speed = data.Distance("speed");
        var stats = new WeaponStats
        {
            Name = data.Text("name"),
            Damage = data.Int("damage"),
            Kinds = data.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList(),
            Speed = speed,
            DiagonalSpeed = data.Has("diagonal_speed") ? data.Distance("diagonal_speed") : Diagonal(speed),
            InheritsSpeed = data.Has("inherits_speed") && data.Flag("inherits_speed"),
            Cooldown = data.Has("cooldown") ? data.Int("cooldown") : 0,
            ChargedCooldown = data.Has("charged_cooldown") ? data.Int("charged_cooldown") : data.Has("cooldown") ? data.Int("cooldown") : 0,
            RepeatsEvery = data.Has("repeats_every") ? data.Int("repeats_every") : 0,
            HeldCooldown = data.Has("held_cooldown") ? data.Int("held_cooldown") : 0,
            PartChargeFires = data.Has("part_charge_fires") && data.Flag("part_charge_fires"),
            MaxLive = data.Has("max_live") ? data.Int("max_live") : 0,
            Pool = data.Has("pool") ? data.Text("pool") : data.Text("name"),
            PlacesTaken = data.Has("places") ? data.Int("places") : 0,
            Costs = data.Has("costs") ? data.Text("costs") : "",
            Cost = data.Has("cost") ? data.Int("cost") : 0,
            Requires = data.Has("requires") ? data.Text("requires") : "",
            ChargeUpdates = data.Has("charge_updates") ? data.Int("charge_updates") : 0,
            ChargeNeeds = data.Has("charge_needs") ? data.Text("charge_needs") : "",
            ChargedDamageTimes = data.Has("charged_damage_times") ? data.Int("charged_damage_times") : 1,
            ChargedKind = data.Has("charged_kind") ? data.Text("charged_kind") : "",
            Sound = data.Has("sound") ? data.Text("sound") : "",
            ChargedSound = data.Has("charged_sound") ? data.Text("charged_sound") : "",
            Scene = data.Has("scene") ? data.Text("scene") : "",
            Properties = data.Has("properties")
                ? data.List("properties").Select(property => new WeaponProperty(
                    property.Text("equipment"),
                    property.Has("kinds") ? property.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList() : [],
                    property.Has("damage") ? property.Int("damage") : 0)).ToList()
                : [],
            Variants = data.Has("variants") ? data.List("variants").Select(variant => WeaponVariant.Read(variant, projectile)).ToList() : [],
            Projectile = projectile,
        };
        if (stats.Damage < 0 || stats.Cooldown < 0 || stats.MaxLive < 0 || stats.PlacesTaken < 0 || stats.Cost < 0 || stats.Kinds.Count == 0 || stats.RepeatsEvery < 0 ||
            stats.HeldCooldown < 0)
            throw data.Problem("gives a negative number, or a weapon whose hit carries no kind");
        return stats;
    }

    /// <summary>The variant that applies for a player carrying <paramref name="inventory"/>, or null when the weapon's own shot does.</summary>
    public WeaponVariant? VariantFor(Inventory inventory)
    {
        WeaponVariant? best = null;
        foreach (WeaponVariant variant in Variants)
        {
            if (!variant.When.All(inventory.IsOn))
                continue;
            if (best is null || variant.When.Count > best.When.Count)
                best = variant;
        }

        return best;
    }

    /// <summary>The hit one shot delivers for a player carrying <paramref name="inventory"/>.</summary>
    /// <param name="inventory">The player's inventory, which says which equipment is switched on.</param>
    /// <param name="charged">Whether the shot is charged.</param>
    public Hit HitFor(Inventory inventory, bool charged)
    {
        WeaponVariant? variant = VariantFor(inventory);
        int damage = variant?.Damage ?? Damage;
        var kinds = new List<HitKind>(variant?.Kinds ?? Kinds);
        if (variant is null)
        {
            foreach (WeaponProperty property in Properties)
            {
                if (!inventory.IsOn(property.Equipment))
                    continue;
                damage += property.Damage;
                kinds.AddRange(property.Kinds);
            }
        }

        if (charged)
        {
            damage *= ChargedDamageTimes;
            if (ChargedKind.Length > 0)
                kinds.Add(new HitKind(ChargedKind));
        }

        return new Hit(damage, HitSide.Player, [.. kinds.Distinct()]);
    }

    /// <summary>The projectiles one shot launches for a player carrying <paramref name="inventory"/>.</summary>
    public IReadOnlyList<WeaponShot> ShotsFor(Inventory inventory, bool charged)
    {
        WeaponVariant? variant = VariantFor(inventory);
        if (variant is null)
            return [new WeaponShot(0, Projectile)];
        return charged ? variant.ChargedShots : variant.Shots;
    }

    /// <summary>The cooldown after one shot for a player carrying <paramref name="inventory"/>.</summary>
    public int CooldownFor(Inventory inventory, bool charged) =>
        charged ? ChargedCooldown : VariantFor(inventory)?.Cooldown ?? Cooldown;

    /// <summary>
    /// The cooldown after a shot fired by holding the attack down, for a player carrying <paramref name="inventory"/>;
    /// zero when holding does not fire the weapon.
    /// </summary>
    public int HeldCooldownFor(Inventory inventory) => VariantFor(inventory)?.HeldCooldown ?? HeldCooldown;

    /// <summary>The sound of one shot for a player carrying <paramref name="inventory"/>; empty for none.</summary>
    public string SoundFor(Inventory inventory, bool charged)
    {
        WeaponVariant? variant = VariantFor(inventory);
        string plain = variant is { Sound.Length: > 0 } ? variant.Sound : Sound;
        if (!charged)
            return plain;
        return variant is { ChargedSound.Length: > 0 } ? variant.ChargedSound : ChargedSound.Length > 0 ? ChargedSound : plain;
    }

    /// <summary>The speed along each axis of a shot fired diagonally at <paramref name="speed"/>.</summary>
    private static Pixels Diagonal(Pixels speed)
    {
        // 181/256 is the sine of an eighth of a turn, to the nearest 256th.
        long fractions = ((long)speed.Whole * Pixels.FractionsPerPixel + speed.Fraction) * 181 / 256;
        return Pixels.FromParts((int)(fractions / Pixels.FractionsPerPixel), (int)(fractions % Pixels.FractionsPerPixel));
    }
}
