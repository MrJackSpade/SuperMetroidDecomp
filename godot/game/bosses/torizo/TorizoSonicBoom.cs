using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>A sonic boom a Torizo throws: it flies level, faster and faster, and breaks on the first wall.</summary>
/// <remarks>Its timeline is "wall_impact".</remarks>
public partial class TorizoSonicBoom : TorizoShot
{
    /// <summary>The speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("quickening")]
    public double Quickening { get; set; } = 0.0625;

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        if (MoveAcrossMeetingTerrain(room, Speed.X))
        {
            Origin = Position;
            Burst("wall_impact");
            return;
        }

        Pixels gain = Pixels.FromNumber(Quickening);
        Speed = Speed.WithX(Speed.X.IsNegative ? Speed.X - gain : Speed.X + gain);
        if (IsRunaway(Speed.X))
            Leave(room);
    }
}
