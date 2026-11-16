namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One piece of the broken n00b tube: the crack, a glass shard or a released-air bubble, and how it starts.</summary>
public sealed class NoobTubePiece
{
    /// <summary>Describes one piece of the tube's break.</summary>
    /// <param name="definition">Which kind of piece it is.</param>
    /// <param name="offset">Where it appears from the tube's block.</param>
    /// <param name="velocity">The speed it starts with.</param>
    /// <param name="start">How it starts moving.</param>
    internal NoobTubePiece(NoobTubeProjectileDefinition definition, Offset16<RoomPixels> offset, EightEightVector<RoomPixels> velocity,
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
    /// The speed the piece starts with. A shard flies off at both of its axes; a bubble rises at its Y and keeps the
    /// sideways speed its projectile's place was left with; the crack keeps both.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; }

    /// <summary>How the piece starts moving, which the tube's behaviour turns into the animation it plays.</summary>
    internal NoobTubePieceStart Start { get; }
}
