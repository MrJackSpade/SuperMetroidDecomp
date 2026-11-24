using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A piece of the statue hand that held the Bombs: it hangs where it broke off until the cue "fall" of its timeline,
/// then falls, faster and faster, and lies where it lands.
/// </summary>
public partial class TorizoFragment : CuedProjectile
{
    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0625;

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
        {
            Moving = false;
            return;
        }

        Pixels faster = Speed.Y + Pixels.FromNumber(Gravity);
        Speed = Speed.WithY(IsRunaway(faster) ? Pixels.FromWhole(16) : faster);
    }
}
