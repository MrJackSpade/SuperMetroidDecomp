using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Tripper, or one of the plain platforms that behave like it: it glides back and forth between walls, and Samus
/// can ride it. Under her weight it sinks, faster and faster; when she steps off it climbs back to where it rides.
/// </summary>
/// <remarks>
/// Its timelines are "still_left", "still_right", "moving_left" and "moving_right": the way it is heading, and
/// whether it is rising or sinking. Its solid collider is what Samus stands on.
/// </remarks>
public partial class Tripper : Enemy
{
    /// <summary>How far it glides each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <summary>The mark of the speed curve its sinking and rising speed up to and stay at.</summary>
    [Export]
    public int FastestMark { get; set; } = 16;

    /// <summary>How far below where it is placed it rides, in pixels.</summary>
    [Export, DataValue("resting_depth")]
    public int RestingDepth { get; set; } = 1;

    /// <summary>The data file of the speed curve its sinking and rising follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private SpeedCurve? _curve;
    private int _restsAt;
    private int _place;
    private bool _wasSinking;
    private bool _showsMoving;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _restsAt = Position.Y.Whole + RestingDepth;
        Play(Facing == Facing.Left ? "still_left" : "still_right");
    }

    /// <summary>Its touch does nothing: the cartridge's ignores hers, and it is only something to stand on.</summary>
    protected override void Touch(Engine.Collision.Collider part)
    {
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Tripper \"{Name}\" is not in a room.");
        bool sinking = IsRiddenByPlayer;
        int beforeX = Position.X.Whole;
        if (MoveAcrossBy(Facing.Toward(Pixels.FromNumber(Speed))))
        {
            Facing = Facing.Opposite();
            Show(moving: false, restart: true);
        }

        int top = FastestMark * SpeedCurve.PartsPerMark;
        if (sinking)
        {
            _place = Math.Min(_place + SpeedCurve.PartsPerMark, top);
            int beforeY = Position.Y.Whole;
            if (MoveDownBy(curve.SpeedAt(_place)))
            {
                _place = 0;
                Show(moving: false, restart: false);
            }

            // Its rider goes where it went, by whole pixels.
            ShiftPlayer(PixelOffset.FromWhole(Position.X.Whole - beforeX, Position.Y.Whole - beforeY));
        }
        else if (Position.Y.Whole < _restsAt)
        {
            _place = 0;
            Show(moving: false, restart: false);
        }
        else
        {
            Show(moving: true, restart: false);
            _place = Math.Min(_place + SpeedCurve.PartsPerMark, top);
            if (MoveDownBy(-curve.SpeedAt(_place)))
            {
                _place = 0;
                Show(moving: false, restart: false);
            }
        }

        if (sinking != _wasSinking)
            _place = 0;
        _wasSinking = sinking;
    }

    /// <summary>Shows it heading the way it faces, moving up or down or not.</summary>
    private void Show(bool moving, bool restart)
    {
        if (!restart && moving == _showsMoving)
            return;
        _showsMoving = moving;
        Play($"{(moving ? "moving" : "still")}_{(Facing == Facing.Left ? "left" : "right")}", restart: true);
    }
}
