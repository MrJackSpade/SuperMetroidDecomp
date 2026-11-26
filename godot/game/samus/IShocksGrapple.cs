namespace Game;

/// <summary>
/// Something whose touch on Samus's grapple line hurts her: the line is given up, and she takes the hurt and is
/// knocked back whichever side of her it is on (<see cref="SamusGrapple"/>).
/// </summary>
public interface IShocksGrapple
{
    /// <summary>The energy the line's touch takes from her, before her suit divides it.</summary>
    int GrappleShock { get; }
}
