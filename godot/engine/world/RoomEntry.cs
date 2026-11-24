using Engine.Shared;

namespace Engine.World;

/// <summary>
/// How the player arrives in a room: by which way in, and what they carry in with them. The room transition builds
/// it; the room and the player read it.
/// </summary>
/// <param name="EntryPoint">The name of the node in the room the player arrives at: an exit, a station, or a plain marker.</param>
/// <param name="Velocity">The speed the player arrives with.</param>
/// <param name="Facing">The way the player faces on arriving.</param>
public sealed record RoomEntry(string EntryPoint, PixelVelocity Velocity, Facing Facing)
{
    /// <summary>
    /// The place in the room the player arrives at, for an arrival by no way in: a demonstration that begins in the
    /// middle of a room. Null for an arrival at <see cref="EntryPoint"/>.
    /// </summary>
    public PixelPosition? At { get; init; }
}
