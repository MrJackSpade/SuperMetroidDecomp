namespace Fixture;

/// <summary>A lamp that keeps the room it lights without ever reading it.</summary>
public sealed class Lamp
{
    /// <summary>Hangs the lamp in a room.</summary>
    public Lamp()
    {
    }
}

/// <summary>Hangs lamps in the room it holds.</summary>
public sealed class LampHanger
{
    /// <summary>The room the lamps light.</summary>
    public Room Room { get; } = new();

    /// <summary>A lamp for the room, which reads the room and nothing else of the hanger: without its argument it reads
    /// no instance data, and the build asks for it to be static (CA1822).</summary>
    public Lamp Hang() => new();
}
