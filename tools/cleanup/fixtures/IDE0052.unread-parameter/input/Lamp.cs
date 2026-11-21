namespace Fixture;

/// <summary>A lamp that keeps the room it lights without ever reading it.</summary>
public sealed class Lamp
{
    /// <summary>The room the lamp lights.</summary>
    private readonly Room _room;

    /// <summary>Hangs the lamp in a room.</summary>
    /// <param name="room">The room the lamp lights.</param>
    public Lamp(Room room)
    {
        _room = room;
    }
}

/// <summary>Hangs lamps in the room it holds.</summary>
public sealed class LampHanger
{
    /// <summary>The room the lamps light.</summary>
    public Room Room { get; } = new();

    /// <summary>A lamp for the room, which reads the room and nothing else of the hanger: without its argument it reads
    /// no instance data, and the build asks for it to be static (CA1822).</summary>
    public Lamp Hang() => new(Room);
}
