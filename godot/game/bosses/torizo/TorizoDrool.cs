using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A drop that falls from a hurt Torizo, or a gob thrown from its broken gut. It hangs where it forms until the cue
/// "fall" of its timeline, then falls, slowing across as it goes, and splashes on what it meets.
/// </summary>
/// <remarks>Its timelines are "wall_impact" and "floor_impact".</remarks>
public partial class TorizoDrool : CuedProjectile
{
    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0625;

    /// <summary>The speed across it loses each update.</summary>
    [Export, DataValue("drag")]
    public double Drag { get; set; } = 0.015625;

    /// <summary>The speed across it is left with once the drag has carried it past rest.</summary>
    [Export, DataValue("drift")]
    public double Drift { get; set; } = 0.01171875;

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
        if (MoveAcrossMeetingTerrain(room, Speed.X))
        {
            Moving = false;
            ShowTimeline("wall_impact");
            return;
        }

        Pixels dragged = Dragged(Speed.X, Pixels.FromNumber(Drag));
        Speed = Speed.WithX(dragged.IsNegative == Speed.X.IsNegative ? dragged : Pixels.FromNumber(Drift));
        if (MoveDownMeetingTerrain(room, Speed.Y) && !Speed.Y.IsNegative)
        {
            Moving = false;
            Position -= PixelOffset.FromWhole(0, 3);
            ShowTimeline("floor_impact");
            return;
        }

        Speed = Speed.WithY(Speed.Y + Pixels.FromNumber(Gravity));
        if (IsRunaway(Speed.Y))
            Leave(room);
    }
}
