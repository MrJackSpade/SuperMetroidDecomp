namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One piece of the broken n00b tube: the crack, a glass shard or a released-air bubble, and how it starts.</summary>
public sealed class NoobTubePiece
{
    /// <summary>Describes one piece of the tube's break.</summary>
    /// <param name="definition">Which kind of piece it is.</param>
    /// <param name="offset">Where it appears from the tube's block.</param>
    /// <param name="velocity">The speed it starts with.</param>
    /// <param name="start">How it starts moving.</param>
    internal NoobTubePiece(NoobTubeProjectileDefinition definition, Offset16<RoomPixels> offset, PixelVelocity<RoomPixels> velocity,
        NoobTubePieceStart start)
    {
        Definition = definition;
        Offset = offset;
        Velocity = velocity;
        Start = start;
    }

    /// <summary>Which kind of piece it is: the crack, a glass shard or a released-air bubble.</summary>
    public NoobTubeProjectileDefinition Definition { get; }

    /// <summary>Where the piece appears, in pixels from the top-left corner of the tube's block.</summary>
    public Offset16<RoomPixels> Offset { get; }

    /// <summary>
    /// The speed the piece starts with. A shard flies off at both of its axes and a bubble rises at its Y; the crack falls
    /// at its own speed and sets neither.
    /// </summary>
    /// <remarks>
    /// A bubble sways sideways along its own arc and never moves by a sideways speed, and the crack moves by neither, so
    /// each keeps the speed its enemy-projectile place's previous projectile left on the axes it does not set
    /// (<see cref="EnemyProjectileHandOver"/>) and hands it on to the next projectile spawned in that place.
    /// </remarks>
    public PixelVelocity<RoomPixels> Velocity { get; }

    /// <summary>How the piece starts moving, which the tube's behaviour turns into the animation it plays.</summary>
    internal NoobTubePieceStart Start { get; }
}
