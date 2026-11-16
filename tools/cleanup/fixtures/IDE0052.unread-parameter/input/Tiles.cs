namespace Fixture;

/// <summary>The room tiles fall in.</summary>
public sealed class Room
{
    /// <summary>The room's number.</summary>
    public int Number { get; init; }
}

/// <summary>Tiles that keep the room they fall in without ever reading it.</summary>
public sealed class FallingTiles
{
    /// <summary>The room the tiles fall in.</summary>
    private readonly Room _room;
    /// <summary>The tiles' speed.</summary>
    private readonly int _speed;

    /// <summary>Binds the tiles to <paramref name="speed"/> in a room.</summary>
    /// <param name="room">The room the tiles fall in.</param>
    /// <param name="speed">The tiles' speed.</param>
    public FallingTiles(Room room, int speed)
    {
        _room = room;
        _speed = speed;
    }

    /// <summary>The tiles' speed.</summary>
    public int Speed => _speed;
}

/// <summary>Builds a room's parts.</summary>
public sealed class RoomParts
{
    /// <summary>The room the parts are in.</summary>
    public Room Room { get; } = new();

    /// <summary>The speed the parts' tiles fall at.</summary>
    public int Speed { get; } = 3;

    /// <summary>The falling tiles of the room this builder holds.</summary>
    public FallingTiles Tiles() => new(Room, Speed);

    /// <summary>Falling tiles built for a room, which only the tiles take.</summary>
    /// <param name="room">The room.</param>
    /// <param name="speed">The tiles' speed.</param>
    public static FallingTiles For(Room room, int speed) => new FallingTiles(room, speed);

    /// <summary>Fast falling tiles, built through <see cref="For"/>.</summary>
    public FallingTiles Fast() => For(Room, speed: Speed * 3);
}
