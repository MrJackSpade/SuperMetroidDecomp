namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The seven hops a Puyo makes. This is a discriminant, not a set of flags: every member selects one height, speed and
/// airborne-behaviour definition in <see cref="PuyoHopDefinitions.Hops"/>.
/// </summary>
public enum PuyoHopType
{
    /// <summary>16-pixel height, 1/256-pixel X speed 1.0, and the short-hop behaviour; ordinary distant launches may randomize to the big hop.</summary>
    NormalSmall,
    /// <summary>32-pixel height, 1/256-pixel X speed 1.0, and the big-hop behaviour; proximity to Samus permits random selection among the three ordinary hops.</summary>
    NormalBig,
    /// <summary>The third ordinary hop, reached only by chance once Samus is within the proximity radius.</summary>
    NormalHigh,
    /// <summary>128-pixel height and 1/256-pixel X speed 1.25, whose fraction is discarded by its movement; selected after a post-drop bounce.</summary>
    Giant,
    /// <summary>No horizontal motion and 1/256-pixel downward speed 1.0; wall or ceiling collision selects this fall before a recovery bounce.</summary>
    Dropping,
    /// <summary>16-pixel post-drop bounce with 1/256-pixel X speed 1.0 and speed-curve step 448; one of two randomized recovery heights.</summary>
    DroppedSmall,
    /// <summary>21-pixel post-drop bounce with 1/256-pixel X speed 1.0 and speed-curve step 448; the taller randomized recovery height.</summary>
    DroppedBig,
}
