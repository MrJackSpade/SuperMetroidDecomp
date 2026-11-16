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
    /// <summary>The tiles' speed.</summary>
    private readonly int _speed;

    /// <summary>Binds the tiles to <paramref name="speed"/> in a room.</summary>
    /// <param name="speed">The tiles' speed.</param>
    public FallingTiles(int speed)
    {
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
    public FallingTiles Tiles() => new(Speed);

    /// <summary>Falling tiles built for a room, which only the tiles take.</summary>
    /// <param name="speed">The tiles' speed.</param>
    public static FallingTiles For(int speed) => new FallingTiles(speed);

    /// <summary>Fast falling tiles, built through <see cref="For"/>.</summary>
    public FallingTiles Fast() => For(speed: Speed * 3);
}
