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

    /// <summary>
    /// Where the view's top-left corner is put as the room is set up, for an arrival that says: a demonstration
    /// that begins with the view somewhere of its own. Null for a view put where the way in has it, or about the
    /// player.
    /// </summary>
    public (int X, int Y)? ViewAt { get; init; }

    /// <summary>
    /// Whether the view is left as it stands, with what it was tracking, for whoever brings the player in to place:
    /// a passage that scrolls the room in. Otherwise the room places the view on the player as it is set up.
    /// </summary>
    public bool KeepsView { get; init; }
}
