namespace Engine.Actors;

/// <summary>
/// What the player carries: equipment owned and switched on, and amounts with their capacities, such as energy and
/// each kind of ammunition. The engine keeps the books; the game says which equipment and amounts exist.
/// </summary>
public sealed class Inventory
{
    /// <summary>The name of the amount that is the player's health.</summary>
    public const string Energy = "energy";

    private readonly HashSet<string> _owned = new(StringComparer.Ordinal);
    private readonly HashSet<string> _switchedOn = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _amounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _capacities = new(StringComparer.Ordinal);
    private readonly HashSet<string> _needs = new(StringComparer.Ordinal);

    /// <summary>Whether the player is in the named need: short of something, and not yet recovered.</summary>
    public bool IsInNeed(string need) => _needs.Contains(need);

    /// <summary>Records that the player has fallen into the named need, or has recovered from it.</summary>
    public void SetInNeed(string need, bool inNeed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(need);
        if (inNeed)
            _needs.Add(need);
        else
            _needs.Remove(need);
    }

    /// <summary>The equipment owned, for saving.</summary>
    public IReadOnlyCollection<string> Owned => _owned;

    /// <summary>The equipment switched on, for saving.</summary>
    public IReadOnlyCollection<string> SwitchedOn => _switchedOn;

    /// <summary>The names of the amounts that have a capacity, for saving.</summary>
    public IReadOnlyCollection<string> Amounts => _capacities.Keys;

    /// <summary>Gives the player a piece of equipment, switched on. One they already own stays as they have it switched.</summary>
    public void Grant(string equipment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(equipment);
        if (_owned.Add(equipment))
            _switchedOn.Add(equipment);
    }

    /// <summary>Whether the player owns a piece of equipment.</summary>
    public bool Owns(string equipment) => _owned.Contains(equipment);

    /// <summary>Whether a piece of equipment is owned and switched on.</summary>
    public bool IsOn(string equipment) => _switchedOn.Contains(equipment);

    /// <summary>Whether any of the named pieces of equipment is owned and switched on.</summary>
    public bool IsAnyOn(IEnumerable<string> equipment) => equipment.Any(_switchedOn.Contains);

    /// <summary>Switches a piece of owned equipment on or off.</summary>
    /// <exception cref="InvalidOperationException">The player does not own it.</exception>
    public void Switch(string equipment, bool on)
    {
        if (!_owned.Contains(equipment))
            throw new InvalidOperationException($"The player does not own \"{equipment}\".");
        if (on)
            _switchedOn.Add(equipment);
        else
            _switchedOn.Remove(equipment);
    }

    /// <summary>How much of an amount the player has.</summary>
    public int Amount(string amount) => _amounts.GetValueOrDefault(amount);

    /// <summary>The most of an amount the player can hold; zero for an amount they cannot hold at all.</summary>
    public int Capacity(string amount) => _capacities.GetValueOrDefault(amount);

    /// <summary>Whether the player can hold no more of an amount, which includes not being able to hold any.</summary>
    public bool IsFull(string amount) => Amount(amount) >= Capacity(amount);

    /// <summary>Sets the most of an amount the player can hold. What they have is cut down to it if it is more.</summary>
    public void SetCapacity(string amount, int capacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _capacities[amount] = capacity;
        _amounts[amount] = Math.Min(Amount(amount), capacity);
    }

    /// <summary>Adds to an amount, up to its capacity, and returns how much was added.</summary>
    public int Add(string amount, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        int added = Math.Min(quantity, Capacity(amount) - Amount(amount));
        if (added > 0)
            _amounts[amount] = Amount(amount) + added;
        return Math.Max(added, 0);
    }

    /// <summary>Fills an amount to its capacity.</summary>
    public void Fill(string amount) => _amounts[amount] = Capacity(amount);

    /// <summary>Takes from an amount if the player has that much, and returns whether they did.</summary>
    public bool Spend(string amount, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        if (Amount(amount) < quantity)
            return false;
        _amounts[amount] = Amount(amount) - quantity;
        return true;
    }

    /// <summary>Takes from an amount, stopping at nothing left.</summary>
    public void Lose(string amount, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        _amounts[amount] = Math.Max(0, Amount(amount) - quantity);
    }

    /// <summary>Puts an amount back to a saved value, within its capacity.</summary>
    public void Restore(string amount, int quantity) => _amounts[amount] = Math.Clamp(quantity, 0, Capacity(amount));
}
