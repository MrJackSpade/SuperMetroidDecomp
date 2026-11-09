namespace SuperMetroid.Core.Progression;

/// <summary>
/// A door a cap keeps shut: a coloured, grey or eye door that room data places (#1553). It is an identity, named in
/// <see cref="LockedDoorDefinitions"/>; the game keeps whether each door whose opening persists has been opened for good
/// (<see cref="PersistentProgress.OpenedDoors"/>).
/// </summary>
public sealed class LockedDoor
{
    /// <summary>Creates the door named <paramref name="name"/>.</summary>
    internal LockedDoor(string name) => Name = Ensure.NotNull(name);

    /// <summary>The door's name, which saves keep it by.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
