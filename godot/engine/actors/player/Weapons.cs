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

    /// <summary>What holds places in the weapons' pools: each firing, or each projectile, with what it left on ending.</summary>
    private readonly List<Held> _held = [];

    /// <summary>Places of a pool held by a firing or a projectile: until all its projectiles have ended and what they left is gone.</summary>
    private sealed class Held(string pool, int places, IReadOnlyList<int> numbers)
    {
        public string Pool { get; } = pool;

        public int Places { get; } = places;

        /// <summary>The places it holds, by their numbers in the pool, counted from nought.</summary>
        public IReadOnlyList<int> Numbers { get; } = numbers;

        public List<Projectile> Shots { get; } = [];

        public List<Node> Left { get; } = [];

        public bool StillHolds =>
            Shots.Any(shot => GodotObject.IsInstanceValid(shot) && !shot.HasEnded) || Left.Any(GodotObject.IsInstanceValid);
    }

    /// <summary>How many places of <paramref name="pool"/> are held.</summary>
    public int PlacesHeld(string pool) => _held.Where(held => held.Pool == pool && held.StillHolds).Sum(held => held.Places);

    /// <summary>
    /// The lowest-numbered places of <paramref name="pool"/> that nothing holds, as many as are wanted or as there
    /// are: a pool's places are numbered from nought, and what is fired takes the lowest free.
    /// </summary>
    private int[] FreePlaces(string pool, int wanted)
    {
        HashSet<int> taken = [.. _held.Where(held => held.Pool == pool && held.StillHolds).SelectMany(held => held.Numbers)];
        return [.. Enumerable.Range(0, PlacesIn(pool)).Where(number => !taken.Contains(number)).Take(wanted)];
    }

    /// <summary>
    /// Where what holds a numbered place of <paramref name="pool"/> stands, the places counted from nought: the
    /// first of its shots still flying, or failing that the first thing they left that is still there. Null for a
    /// place nothing holds, and, where only a shot still flying is asked for, for a place held by what one left. A
    /// game that reads one of the player's shots by its place asks this.
    /// </summary>
    public PixelPosition? StandsIn(string pool, int place, bool flyingOnly = false)
    {
        Held? holder = _held.FirstOrDefault(held => held.Pool == pool && held.StillHolds && held.Numbers.Contains(place));
        if (holder is null)
            return null;
        if (holder.Shots.FirstOrDefault(shot => GodotObject.IsInstanceValid(shot) && !shot.HasEnded) is { } flying)
            return flying.Position;
        return flyingOnly ? null : holder.Left.Where(GodotObject.IsInstanceValid).OfType<Engine.Collision.ICollisionOwner>().FirstOrDefault()?.Position;
    }

    /// <summary>How many places <paramref name="pool"/> has, as the weapons that share it give it.</summary>
    /// <exception cref="InvalidOperationException">No weapon shares the pool.</exception>
    public int PlacesIn(string pool) =>
        (_weapons.FirstOrDefault(weapon => weapon.Pool == pool) ?? throw new InvalidOperationException($"No weapon shares the pool \"{pool}\".")).MaxLive;

    /// <summary>
    /// Has a projectile that no weapon fired hold one place of <paramref name="pool"/>, until it has ended and what it
    /// left is gone: what a game launches for the player by itself and counts among their shots.
    /// </summary>
    public void HoldPlace(string pool, Projectile projectile)
    {
        ArgumentNullException.ThrowIfNull(projectile);
        var alone = new Held(pool, 1, FreePlaces(pool, 1));
        alone.Shots.Add(projectile);
        projectile.LeftOnEnding += alone.Left.Add;
        _held.Add(alone);
    }

    /// <summary>
    /// Has a projectile that no weapon fired take a numbered place of <paramref name="pool"/> over, the places
    /// counted from nought: whatever holds the place, shots still flying and what shots left alike, is taken out of
    /// <paramref name="room"/> with no ending of its own, and the projectile holds the place until it has ended and
    /// what it left is gone. What a game launches into particular places of the player's shots, whatever is there.
    /// </summary>
    public void TakePlace(string pool, int place, Projectile projectile, Room room)
    {
        ArgumentNullException.ThrowIfNull(projectile);
        ArgumentNullException.ThrowIfNull(room);
        ClearPlace(pool, place, room);
        var taken = new Held(pool, 1, [place]);
        taken.Shots.Add(projectile);
        projectile.LeftOnEnding += taken.Left.Add;
        _held.Add(taken);
    }

    /// <summary>
    /// Empties a numbered place of <paramref name="pool"/>, the places counted from nought: whatever holds it, shots
    /// still flying and what shots left alike, is taken out of <paramref name="room"/> with no ending of its own. An
    /// empty place stays empty.
    /// </summary>
    public void ClearPlace(string pool, int place, Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (place < 0 || place >= PlacesIn(pool))
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The pool \"{pool}\" has places 0 through {PlacesIn(pool) - 1}.");
        foreach (Held before in _held.Where(held => held.Pool == pool && held.StillHolds && held.Numbers.Contains(place)).ToList())
        {
            foreach (Projectile shot in before.Shots.Where(shot => GodotObject.IsInstanceValid(shot) && !shot.HasEnded))
                room.Remove(shot);
            foreach (Node left in before.Left.Where(GodotObject.IsInstanceValid))
            {
                if (left is IRoomEntity entity)
                    room.Remove(entity);
                else
                    left.QueueFree();
            }

            _held.Remove(before);
        }
    }

    /// <summary>Has <paramref name="projectiles"/> hold places of <paramref name="weapon"/>'s pool: together, or each its own.</summary>
    private void HoldPlaces(WeaponStats weapon, IReadOnlyList<Projectile> projectiles)
    {
        if (weapon.PlacesTaken > 0)
        {
            var together = new Held(weapon.Pool, weapon.PlacesTaken, FreePlaces(weapon.Pool, weapon.PlacesTaken));
            foreach (Projectile projectile in projectiles)
            {
                together.Shots.Add(projectile);
                projectile.LeftOnEnding += together.Left.Add;
            }

            _held.Add(together);
            return;
        }

        foreach (Projectile projectile in projectiles)
        {
            var alone = new Held(weapon.Pool, 1, FreePlaces(weapon.Pool, 1));
            alone.Shots.Add(projectile);
            projectile.LeftOnEnding += alone.Left.Add;
            _held.Add(alone);
        }
    }
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

    /// <summary>Gives the player a weapon.</summary>
    public void Add(WeaponStats weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        if (_weapons.FirstOrDefault(other => other.Pool == weapon.Pool && other.MaxLive != weapon.MaxLive) is { } other)
            throw new InvalidOperationException(
                $"The weapons \"{other.Name}\" and \"{weapon.Name}\" share the pool \"{weapon.Pool}\" and give it {other.MaxLive} and {weapon.MaxLive} places.");
        _weapons.Add(weapon);
    }

    /// <summary>The weapon of the given name.</summary>
    /// <exception cref="ArgumentException">The player has no weapon of that name.</exception>
    public WeaponStats Named(string name) =>
        _weapons.FirstOrDefault(weapon => weapon.Name == name)
        ?? throw new ArgumentException($"The player has no weapon named \"{name}\".", nameof(name));

    /// <summary>
    /// Selects the weapon of the given name. A charge being built is the player's, not the selection's: it stands
    /// through a change of weapon, counted on only while a weapon that charges is selected, until it is let go or
    /// dropped (<see cref="DropCharge"/>).
    /// </summary>
    /// <exception cref="ArgumentException">The player has no weapon of that name.</exception>
    public void Select(string name)
    {
        int index = _weapons.FindIndex(weapon => weapon.Name == name);
        if (index < 0)
            throw new ArgumentException($"The player has no weapon named \"{name}\".", nameof(name));
        if (index == _selected)
            return;
        _selected = index;
        _heldFor = 0;
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
    /// Plays one update: the cooldown moves on, and the selected weapon fires. One that charges fires a plain shot on
    /// the first update its attack is held, and as the attack is let go a charged one, or a plain one where it had not
    /// charged and the weapon says so. One that does not charge fires on a press, and while held as its data says.
    /// </summary>
    public void Step(Player player, GameInput input)
    {
        StepCooldown();
        WeaponStats? weapon = Selected;
        // With no weapon to use nothing fires, and a charge already built stands.
        if (weapon is null || !CanUse(weapon, player.Inventory))
            return;

        bool held = input.IsHeld(GameAction.Attack);
        if (weapon.Charges(player.Inventory))
        {
            _heldFor = 0;
            if (held)
            {
                _chargedFor++;
                if (_chargedFor == 1)
                    Fire(player, weapon, charged: false);
                return;
            }

            if (_chargedFor == 0)
                return;
            bool charged = _chargedFor >= weapon.ChargeUpdates;
            _chargedFor = 0;
            if (charged || weapon.PartChargeFires)
                Fire(player, weapon, charged);
            return;
        }

        // A weapon that does not charge leaves a charge already built as it stands.
        if (input.WasPressed(GameAction.Attack))
        {
            _heldFor = 0;
            Fire(player, weapon, charged: false);
            return;
        }

        int heldCooldown = weapon.HeldCooldownFor(player.Inventory);
        if (held && heldCooldown > 0)
        {
            if (Fire(player, weapon, charged: false))
                _cooldown = Countdown.Of(heldCooldown);
            return;
        }

        if (!held || weapon.RepeatsEvery == 0)
        {
            _heldFor = 0;
            return;
        }

        _heldFor++;
        if (_heldFor >= weapon.RepeatsEvery && Fire(player, weapon, charged: false))
            _heldFor = 0;
    }

    /// <summary>Makes the player wait at least <paramref name="updates"/> before the next shot, as something that has hold of them does.</summary>
    public void WaitAtLeast(int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        if (_cooldown.Remaining < updates)
            _cooldown = Countdown.Of(updates);
    }

    /// <summary>Moves the cooldown on by one update and forgets shots that have ended.</summary>
    public void StepCooldown()
    {
        _cooldown = _cooldown.Stepped();
        foreach (List<Projectile> live in _live.Values)
            live.RemoveAll(shot => !GodotObject.IsInstanceValid(shot) || shot.HasEnded);
        _held.RemoveAll(held => !held.StillHolds);
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
        int places = weapon.PlacesTaken > 0 ? weapon.PlacesTaken : shots.Count;
        if (weapon.MaxLive > 0 && PlacesHeld(weapon.Pool) + places > weapon.MaxLive)
            return false;
        if (weapon.Cost > 0 && !player.Inventory.Spend(weapon.Costs, weapon.Cost))
            return false;

        (int across, int down) = player.AimDirection;
        Pixels alongAxis = across != 0 && down != 0 ? weapon.DiagonalSpeed : weapon.Speed;
        var velocity = new PixelVelocity(alongAxis * across, alongAxis * down);
        if (weapon.InheritsSpeed)
            velocity = player.ShotVelocity(weapon, velocity, across, down);
        Hit hit = weapon.HitFor(player.Inventory, charged) with { Direction = velocity.OverOneUpdate, Source = player };

        var launched = new List<Projectile>();
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
            launched.Add(projectile);
        }

        HoldPlaces(weapon, launched);

        string sound = weapon.SoundFor(player.Inventory, charged);
        if (sound.Length > 0)
            player.Room.Session.Sounds.Request(sound, channel: "weapons");
        _cooldown = Countdown.Of(weapon.CooldownFor(player.Inventory, charged));
        player.OnFired(weapon);
        return true;
    }

    /// <summary>Makes a weapon's projectile: one of its scene, or a plain one.</summary>
    /// <summary>
    /// Launches one projectile of a weapon by hand, with its own place, speed and settings, counted among that
    /// weapon's live ones. For a player whose weapon can be let loose some way firing does not cover.
    /// </summary>
    public Projectile Launch(Player player, WeaponStats weapon, PixelPosition at, PixelVelocity velocity, ProjectileSettings settings)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(weapon);
        ArgumentNullException.ThrowIfNull(settings);
        if (!_live.TryGetValue(weapon.Name, out List<Projectile>? live))
            _live[weapon.Name] = live = [];
        Hit hit = weapon.HitFor(player.Inventory, charged: false) with { Direction = velocity.OverOneUpdate, Source = player };
        Projectile projectile = Make(weapon);
        projectile.Launch(hit, at, velocity, settings, player);
        player.Room.Spawn(projectile);
        live.Add(projectile);
        HoldPlaces(weapon, [projectile]);
        return projectile;
    }

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
