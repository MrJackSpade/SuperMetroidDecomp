using Engine.Input;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// The player's weapons: which is selected, its charge and cooldown, its cost, the limit on its live shots, and
/// launching its projectiles with a hit made from the weapon and whatever equipment is switched on. The game supplies
/// each weapon.
/// </summary>
public sealed class Weapons
{
    private readonly List<WeaponStats> _weapons = [];
    private readonly Dictionary<string, List<Projectile>> _live = new(StringComparer.Ordinal);
    private Countdown _cooldown = Countdown.Over;
    private int _selected;
    private int _chargedFor;
    private int _heldFor;

    /// <summary>The weapons the player has, in the order they were added.</summary>
    public IReadOnlyList<WeaponStats> All => _weapons;

    /// <summary>The selected weapon, or null when the player has none.</summary>
    public WeaponStats? Selected => _weapons.Count > 0 ? _weapons[_selected] : null;

    /// <summary>How many updates the attack has been held toward a charged shot.</summary>
    public int ChargedFor => _chargedFor;

    /// <summary>Whether a charged shot is ready to be let go.</summary>
    public bool IsCharged => Selected is { ChargeUpdates: > 0 } weapon && _chargedFor >= weapon.ChargeUpdates;

    /// <summary>The updates left before the next shot may be fired.</summary>
    public int CooldownLeft => _cooldown.Remaining;

    /// <summary>Gives the player a weapon.</summary>
    public void Add(WeaponStats weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        _weapons.Add(weapon);
    }

    /// <summary>The weapon of the given name.</summary>
    /// <exception cref="ArgumentException">The player has no weapon of that name.</exception>
    public WeaponStats Named(string name) =>
        _weapons.FirstOrDefault(weapon => weapon.Name == name)
        ?? throw new ArgumentException($"The player has no weapon named \"{name}\".", nameof(name));

    /// <summary>Selects the weapon of the given name. Selecting the weapon already selected changes nothing; another loses any charge.</summary>
    /// <exception cref="ArgumentException">The player has no weapon of that name.</exception>
    public void Select(string name)
    {
        int index = _weapons.FindIndex(weapon => weapon.Name == name);
        if (index < 0)
            throw new ArgumentException($"The player has no weapon named \"{name}\".", nameof(name));
        if (index == _selected)
            return;
        _selected = index;
        _chargedFor = 0;
        _heldFor = 0;
    }

    /// <summary>Selects the next weapon the player can use, after the selected one.</summary>
    public void SelectNext(Inventory inventory)
    {
        for (int offset = 1; offset <= _weapons.Count; offset++)
        {
            int index = (_selected + offset) % _weapons.Count;
            if (CanUse(_weapons[index], inventory))
            {
                _selected = index;
                _chargedFor = 0;
                _heldFor = 0;
                return;
            }
        }
    }

    /// <summary>The live shots of the weapon named <paramref name="weapon"/>.</summary>
    public IReadOnlyList<Projectile> LiveOf(string weapon) =>
        _live.TryGetValue(weapon, out List<Projectile>? live) ? live : [];

    /// <summary>Throws away any charge being built, as when the player is interrupted.</summary>
    public void DropCharge()
    {
        _chargedFor = 0;
        _heldFor = 0;
    }

    /// <summary>
    /// Plays one update: the cooldown moves on, and the selected weapon fires on a press of the attack; when it charges,
    /// as a held attack that has charged is let go; and when holding repeats it, every so often while held.
    /// </summary>
    public void Step(Player player, GameInput input)
    {
        StepCooldown();
        WeaponStats? weapon = Selected;
        if (weapon is null || !CanUse(weapon, player.Inventory))
        {
            DropCharge();
            return;
        }

        if (input.WasPressed(GameAction.Attack))
        {
            _chargedFor = 0;
            _heldFor = 0;
            Fire(player, weapon, charged: false);
            return;
        }

        bool held = input.IsHeld(GameAction.Attack);
        if (weapon.Charges(player.Inventory))
        {
            if (held)
            {
                _chargedFor++;
                return;
            }

            bool charged = _chargedFor >= weapon.ChargeUpdates;
            _chargedFor = 0;
            if (charged)
                Fire(player, weapon, charged: true);
            return;
        }

        _chargedFor = 0;
        if (!held || weapon.RepeatsEvery == 0)
        {
            _heldFor = 0;
            return;
        }

        _heldFor++;
        if (_heldFor >= weapon.RepeatsEvery && Fire(player, weapon, charged: false))
            _heldFor = 0;
    }

    /// <summary>Moves the cooldown on by one update and forgets shots that have ended.</summary>
    public void StepCooldown()
    {
        _cooldown = _cooldown.Stepped();
        foreach (List<Projectile> live in _live.Values)
            live.RemoveAll(shot => shot.HasEnded || !GodotObject.IsInstanceValid(shot));
    }

    /// <summary>Whether the player can use a weapon: they have its equipment switched on.</summary>
    public static bool CanUse(WeaponStats weapon, Inventory inventory) =>
        weapon.Requires.Length == 0 || inventory.IsOn(weapon.Requires);

    /// <summary>
    /// Launches a shot of <paramref name="weapon"/> if the cooldown, the live limit and the cost allow it. Returns
    /// whether it was launched.
    /// </summary>
    public bool Fire(Player player, WeaponStats weapon, bool charged)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(weapon);
        if (_cooldown.IsRunning)
            return false;
        if (!_live.TryGetValue(weapon.Name, out List<Projectile>? live))
            _live[weapon.Name] = live = [];
        IReadOnlyList<WeaponShot> shots = weapon.ShotsFor(player.Inventory, charged);
        if (weapon.MaxLive > 0 && live.Count + shots.Count > weapon.MaxLive)
            return false;
        if (weapon.Cost > 0 && !player.Inventory.Spend(weapon.Costs, weapon.Cost))
            return false;

        (int across, int down) = player.AimDirection;
        Pixels alongAxis = across != 0 && down != 0 ? weapon.DiagonalSpeed : weapon.Speed;
        var velocity = new PixelVelocity(alongAxis * across, alongAxis * down);
        if (weapon.InheritsSpeed && across != 0 && !player.SpeedAcross.IsZero && player.SpeedAcross.IsNegative == across < 0)
            velocity = velocity.WithX(velocity.X + player.SpeedAcross);
        Hit hit = weapon.HitFor(player.Inventory, charged) with { Direction = velocity.OverOneUpdate, Source = player };

        foreach (WeaponShot shot in shots)
        {
            // A shot's projectiles start side by side across its line of travel.
            PixelOffset side = across != 0 && down == 0
                ? PixelOffset.FromWhole(0, shot.Side)
                : down != 0 && across == 0 ? PixelOffset.FromWhole(shot.Side, 0) : PixelOffset.FromWhole(shot.Side * -across * down, shot.Side);
            Projectile projectile = Make(weapon);
            projectile.Launch(hit, player.MuzzlePoint + side, velocity, shot.Projectile, player);
            player.Room.Spawn(projectile);
            live.Add(projectile);
        }

        string sound = charged && weapon.ChargedSound.Length > 0 ? weapon.ChargedSound : weapon.Sound;
        if (sound.Length > 0)
            player.Room.Session.Sounds.Request(sound, channel: "weapons");
        _cooldown = Countdown.Of(weapon.CooldownFor(player.Inventory, charged));
        return true;
    }

    /// <summary>Makes a weapon's projectile: one of its scene, or a plain one.</summary>
    private static Projectile Make(WeaponStats weapon)
    {
        if (weapon.Scene.Length == 0)
            return new Projectile { Name = "Projectile" };
        PackedScene scene = ResourceLoader.Load<PackedScene>(weapon.Scene)
            ?? throw new InvalidOperationException($"The weapon \"{weapon.Name}\" names a projectile scene that does not exist: \"{weapon.Scene}\".");
        Node root = scene.Instantiate();
        return root as Projectile
            ?? throw new InvalidOperationException($"The weapon \"{weapon.Name}\"'s projectile scene has a {root.GetType().Name} at its root, not a Projectile.");
    }
}
