namespace SuperMetroid.Core.Progression;

/// <summary>
/// A place where room data stands an item pickup (#1553). It is an identity, named in
/// <see cref="ItemLocationDefinitions"/>; the game keeps whether each location's pickup has been collected
/// (<see cref="PersistentProgress.CollectedItems"/>), independently of Samus's equipment.
/// </summary>
public sealed class ItemLocation
{
    /// <summary>Creates the item location named <paramref name="name"/>.</summary>
    internal ItemLocation(string name) => Name = Ensure.NotNull(name);

    /// <summary>The location's name, which saves keep it by.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
