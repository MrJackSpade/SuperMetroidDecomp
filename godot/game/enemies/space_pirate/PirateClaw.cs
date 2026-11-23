using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A claw thrown by a ninja Space Pirate: it flies out, slowing to a stop, and comes back the way it went, gathering
/// speed, sinking a pixel each update all the while. Its settings let it through terrain and end it out of view.
/// </summary>
public partial class PirateClaw : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The speed it loses each update going out, and gains coming back, as a number of pixels.</summary>
    [Export, DataValue("speed_change")]
    public double SpeedChange { get; set; } = 0.125;

    /// <summary>How far it sinks each update, in pixels.</summary>
    [Export, DataValue("sink")]
    public int Sink { get; set; } = 1;

    private Facing _thrown;
    private Pixels _speed;
    private bool _comingBack;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        _thrown = Velocity.X.IsNegative ? Facing.Left : Facing.Right;
        _speed = Velocity.X.Abs();
        // It moves itself, by whole pixels; its settings' own travel has nothing to add.
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        int pixels = _comingBack ? -_speed.Whole : _speed.Whole;
        Pixels change = Pixels.FromNumber(SpeedChange);
        if (_comingBack)
        {
            _speed += change;
        }
        else
        {
            _speed -= change;
            if (_speed.IsZero || _speed.IsNegative)
            {
                _speed = Pixels.Zero;
                _comingBack = true;
            }
        }

        Position += PixelOffset.FromWhole(_thrown == Facing.Left ? -pixels : pixels, Sink);
    }
}
