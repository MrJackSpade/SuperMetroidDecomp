using Engine.Actors;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// A piece of the statue hand that held the Bombs: it hangs where it broke off until the cue "fall" of its timeline,
/// then falls at the speed it was let go with and lies where it lands.
/// </summary>
public partial class TorizoFragment : CuedProjectile
{
    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        base.OnLaunched(room);
        Moving = false;
    }

    /// <inheritdoc/>
    protected override void OnOwnCue(Room room, string[] cue)
    {
        if (cue[0] == "fall")
            Moving = true;
        else
            base.OnOwnCue(room, cue);
    }

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        if (MoveDownMeetingTerrain(room, Speed.Y) && !Speed.Y.IsNegative)
            Moving = false;
    }
}
