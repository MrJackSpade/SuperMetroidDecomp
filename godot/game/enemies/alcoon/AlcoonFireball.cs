using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of an Alcoon's fireballs: it leaves fast and slows, across only, to a steady speed, keeping whatever rise or
/// fall it was breathed with. Its settings do the rest.
/// </summary>
public partial class AlcoonFireball : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The speed across it loses each update, as a number of pixels.</summary>
    [Export, DataValue("slowing")]
    public double Slowing { get; set; } = 0.25;

    /// <summary>The speed across it slows to and keeps.</summary>
    [Export, DataValue("steady_speed")]
    public double SteadySpeed { get; set; } = 2;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room) => DataValues.Fill(this, room.Content);

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (Age == 1)
            return;
        Pixels steady = Pixels.FromNumber(SteadySpeed), slowing = Pixels.FromNumber(Slowing);
        Pixels across = Velocity.X.Abs() - slowing;
        if (across < steady)
            across = steady;
        Velocity = Velocity.WithX(Velocity.X.IsNegative ? -across : across);
    }
}
