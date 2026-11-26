using Engine.Shared;

namespace Engine.World;

/// <summary>
/// Something that turns a shot that hits it another way. What answers a hit with <see cref="HitResult.TurnedBack"/>
/// says here which way the shot goes on; the player whose shot it is then launches it anew that way.
/// </summary>
public interface ITurnsShots
{
    /// <summary>
    /// The way the shot that delivered <paramref name="hit"/> goes on: -1, 0 or 1 across (left, neither, right) and
    /// down (up, neither, down).
    /// </summary>
    (int Across, int Down) TurnedWay(Hit hit);
}
