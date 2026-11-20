using Engine.Input;
using Engine.Shared;
using Engine.World;

namespace Engine.Actors;

/// <summary>
/// The player's weapons: which is selected, its charge and cooldown, its cost, the limit on its live shots, and
/// launching its projectile with a hit made from the weapon and whatever equipment is switched on. The game supplies
/// each weapon.
/// </summary>
public sealed class Weapons
{
    private readonly List<WeaponStats> _weapons = [];
    private readonly List<Projectile> _live = [];
    private Countdown _cooldown = Countdown.Over;
    private int _selected;
    private int _chargedFor;

    /// <summary>The weapons the player has, in the order they were added.</summary>
    public IReadOnlyList<WeaponStats> All => _weapons;

    /// <summary>The selected weapon, or null when the player has none.</summary>
    public WeaponStats? Selected => _weapons.Count > 0 ? _weapons[_selected] : null;

    /// <summary>How many updates the attack has been held toward a charged shot.</summary>
    public int ChargedFor => _chargedFor;

    /// <summary>Whether a charged shot is ready to be let go.</summary>
    public bool IsCharged => Selected is { ChargeUpdates: > 0 } weapon && _chargedFor >= weapon.ChargeUpdates;

    /// <summary>Gives the player a weapon.</summary>
    public void Add(WeaponStats weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        _weapons.Add(weapon);
    }

    /// <summary>Selects the weapon of the given name.</summary>
    /// <exception cref="ArgumentException">The player has no weapon of that name.</exception>
    public void Select(string name)
    {
        int index = _weapons.FindIndex(weapon => weapon.Name == name);
        if (index < 0)
            throw new ArgumentException($"The player has no weapon named \"{name}\".", nameof(name));
        _selected = index;
        _chargedFor = 0;
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
                return;
            }
        }
    }

    /// <summary>
    /// Plays one update: the cooldown moves on, and the selected weapon fires on a press of the attack, or, for a weapon
    /// that charges, when a held attack that has charged is let go.
    /// </summary>
    internal void Step(Player player, GameInput input)
    {
        _cooldown = _cooldown.Stepped();
        _live.RemoveAll(shot => shot.HasEnded);
        WeaponStats? weapon = Selected;
        if (weapon is null || !CanUse(weapon, player.Inventory))
        {
            _chargedFor = 0;
            return;
        }

        if (input.WasPressed(GameAction.Attack))
        {
            _chargedFor = 0;
            Fire(player, weapon, charged: false);
            return;
        }

        if (weapon.ChargeUpdates == 0)
            return;
        if (input.IsHeld(GameAction.Attack))
        {
            _chargedFor++;
            return;
        }

        bool charged = _chargedFor >= weapon.ChargeUpdates;
        _chargedFor = 0;
        if (charged)
            Fire(player, weapon, charged: true);
    }

    /// <summary>Whether the player can use a weapon: they have its equipment switched on.</summary>
    private static bool CanUse(WeaponStats weapon, Inventory inventory) =>
        weapon.Requires.Length == 0 || inventory.IsOn(weapon.Requires);

    /// <summary>Launches a shot if the cooldown, the live limit and the cost allow it.</summary>
    private void Fire(Player player, WeaponStats weapon, bool charged)
    {
        if (_cooldown.IsRunning)
            return;
        if (weapon.MaxLive > 0 && _live.Count >= weapon.MaxLive)
            return;
        if (weapon.Cost > 0 && !player.Inventory.Spend(weapon.Costs, weapon.Cost))
            return;

        (int across, int down) = player.AimDirection;
        Pixels alongAxis = across != 0 && down != 0 ? Diagonal(weapon.Speed) : weapon.Speed;
        var velocity = new PixelVelocity(alongAxis * across, alongAxis * down);
        Hit hit = weapon.HitFor(player.Inventory, charged) with { Direction = velocity.OverOneUpdate, Source = player };
        Projectile shot = Projectile.Of(hit, player.MuzzlePoint, velocity, weapon.Projectile, player);
        player.Room.Spawn(shot);
        _live.Add(shot);
        _cooldown = Countdown.Of(weapon.Cooldown);
    }

    /// <summary>The speed along each axis of a shot fired diagonally at <paramref name="speed"/>.</summary>
    private static Pixels Diagonal(Pixels speed)
    {
        // 181/256 is the sine of an eighth of a turn, to the nearest 256th.
        long fractions = ((long)speed.Whole * Pixels.FractionsPerPixel + speed.Fraction) * 181 / 256;
        return Pixels.FromParts((int)(fractions / Pixels.FractionsPerPixel), (int)(fractions % Pixels.FractionsPerPixel));
    }
}
