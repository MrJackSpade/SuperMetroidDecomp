using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One library background a room state shows behind its blocks (#1466): a named, pre-coloured picture the import pipeline
/// composed by running the background's command list once. A few backgrounds, the scrolling skies', show another picture
/// when the room is entered through particular doors; the room selects that picture by the door, and every other door
/// shows the background's ordinary picture.
/// </summary>
public sealed class LibraryBackground
{
    /// <summary>Names the background and the entry doors that show their own pictures of it.</summary>
    /// <param name="name">The background's name, the disassembly's label for its command list without its prefix.</param>
    /// <param name="pictureDoors">The entry doors that each show their own picture, in the order the list names them.</param>
    /// <exception cref="ArgumentException">The name is empty, or a door is named twice.</exception>
    public LibraryBackground(string name, IEnumerable<DoorDefinition> pictureDoors)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        PictureDoors = [.. Ensure.NotNull(pictureDoors)];
        if (PictureDoors.Distinct().Count() != PictureDoors.Length)
            throw new ArgumentException($"Library background {name} names an entry door twice.", nameof(pictureDoors));
    }

    /// <summary>The background's name, which its installed pictures are named for.</summary>
    public string Name { get; }

    /// <summary>The entry doors that each show their own picture of the background.</summary>
    public ImmutableArray<DoorDefinition> PictureDoors { get; }

    /// <summary>
    /// The door whose picture a room entered through <paramref name="entry"/> shows: that door when it shows its own
    /// picture, otherwise null for the background's ordinary picture.
    /// </summary>
    public DoorDefinition? PictureDoor(DoorDefinition entry) => PictureDoors.Contains(Ensure.NotNull(entry)) ? entry : null;

    /// <inheritdoc cref="Name"/>
    public override string ToString() => Name;
}
