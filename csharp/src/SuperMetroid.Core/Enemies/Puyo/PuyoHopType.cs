namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The seven hops a Puyo makes. This is a discriminant, not a set of flags: every member selects one height, speed and
/// airborne-behaviour definition in <see cref="PuyoHopDefinitions.Hops"/>.
/// </summary>
public enum PuyoHopType
{
    /// <summary>A 16-pixel hop at a pixel an update sideways, the short hop; a Puyo far from Samus may follow it with the big hop at random.</summary>
    NormalSmall,
    /// <summary>A 32-pixel hop at a pixel an update sideways, the big hop; once Samus is near, the Puyo chooses among the three ordinary hops at random.</summary>
    NormalBig,
    /// <summary>A 32-pixel hop at two pixels an update sideways, the long hop: the third ordinary hop, chosen only by chance once Samus is near.</summary>
    NormalLong,
    /// <summary>A 128-pixel hop at a pixel and a quarter an update sideways, of which it moves only the whole pixel; taken after a bounce from a drop.</summary>
    Giant,
    /// <summary>A straight fall with no sideways motion, a pixel an update; a wall or ceiling that cuts a hop short starts it, and a bounce follows it.</summary>
    Dropping,
    /// <summary>A 16-pixel bounce after a drop at a pixel an update sideways, a mark and three quarters along the speed curve an update; the lower of the two bounces chosen at random.</summary>
    DroppedSmall,
    /// <summary>A 21-pixel bounce after a drop at a pixel an update sideways, a mark and three quarters along the speed curve an update; the taller of the two bounces chosen at random.</summary>
    DroppedBig,
}
