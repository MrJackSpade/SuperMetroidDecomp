namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The seven hops a Puyo makes. This is a discriminant, not a set of flags: every member selects one height, speed and
/// airborne-behaviour definition in <see cref="PuyoHopDefinitions.Hops"/>.
/// </summary>
public enum PuyoHopType
{
    /// <summary>The short hop, low and slow sideways; a Puyo far from Samus may follow it with the big hop at random.</summary>
    NormalSmall,
    /// <summary>The big hop, higher than the short hop at its sideways speed; once Samus is near, the Puyo chooses among the ordinary hops at random.</summary>
    NormalBig,
    /// <summary>The long hop, as high as the big hop but faster sideways: the last ordinary hop, chosen only by chance once Samus is near.</summary>
    NormalLong,
    /// <summary>The giant hop, the highest, its sideways speed's whole pixels moved alone; taken after a bounce from a drop.</summary>
    Giant,
    /// <summary>A straight fall with no sideways motion; a wall or ceiling that cuts a hop short starts it, and a bounce follows it.</summary>
    Dropping,
    /// <summary>A bounce after a drop; the lower of the two bounces chosen at random.</summary>
    DroppedSmall,
    /// <summary>A bounce after a drop; the taller of the two bounces chosen at random.</summary>
    DroppedBig,
}
