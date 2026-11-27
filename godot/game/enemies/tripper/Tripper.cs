using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
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
/// whether it is rising or sinking; and "frozen_left" and "frozen_right" where it has them. Its solid collider is what
/// Samus stands on.
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
    protected override void Touch(Collider part)
    {
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Tripper \"{Name}\" is not in a room.");
        bool sinking = IsRiddenByPlayer(Riding.FeetAbove);
        int beforeX = Position.X.Whole;
        if (GoAcross(Facing.Toward(Pixels.FromNumber(Speed))))
        {
            // A wall turns it and starts its still picture the other way, but it still counts as showing what it
            // showed: a rise under way goes on showing the still picture until it ends, as on the cartridge.
            Facing = Facing.Opposite();
            Play(Facing == Facing.Left ? "still_left" : "still_right", restart: true);
        }

        int top = FastestMark * SpeedCurve.PartsPerMark;
        if (sinking)
        {
            _place = Math.Min(_place + SpeedCurve.PartsPerMark, top);
            int beforeY = Position.Y.Whole;
            if (GoDown(curve.SpeedAt(_place)))
            {
                _place = 0;
                Show(moving: false);
            }

            // Its rider goes where it went, by whole pixels.
            ShiftPlayer(PixelOffset.FromWhole(Position.X.Whole - beforeX, Position.Y.Whole - beforeY));
        }
        else if (Position.Y.Whole < _restsAt)
        {
            _place = 0;
            Show(moving: false);
        }
        else
        {
            Show(moving: true);
            _place = Math.Min(_place + SpeedCurve.PartsPerMark, top);
            if (GoDown(-curve.SpeedAt(_place)))
            {
                _place = 0;
                Show(moving: false);
            }
        }

        if (sinking != _wasSinking)
            _place = 0;
        _wasSinking = sinking;
    }

    /// <summary>Where its gliding was when a frozen picture took its place; none while it shows its gliding.</summary>
    private TimelineMark? _beforeFrozen;

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        // Frozen, it shows its frozen picture for the way it heads, where it has one.
        string frozen = Facing == Facing.Left ? "frozen_left" : "frozen_right";
        if (status != Frozen || !Timelines.Has(frozen) || Playing == frozen)
            return;
        _beforeFrozen = MarkTimeline();
        Play(frozen);
    }

    /// <inheritdoc/>
    protected override void OnStatusEnded(string status)
    {
        // Thawed, its gliding goes on from where it was; one with no frozen picture never left it.
        if (_beforeFrozen is not { } mark)
            return;
        _beforeFrozen = null;
        ResumeTimeline(mark);
    }

    /// <summary>Shows it heading the way it faces, moving up or down or not, unless it already counts as showing that.</summary>
    private void Show(bool moving)
    {
        if (moving == _showsMoving)
            return;
        _showsMoving = moving;
        Play($"{(moving ? "moving" : "still")}_{(Facing == Facing.Left ? "left" : "right")}", restart: true);
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>Moves it across by <paramref name="distance"/>, sloped ground no part of the move; returns whether a wall stopped it.</summary>
    private bool GoAcross(Pixels distance) => Room.Mover.MoveAcross(MovingBody(), distance, MovesAcross).Blocked;

    /// <summary>Moves it down by <paramref name="distance"/>, upward when less than nothing; returns whether something stopped it.</summary>
    private bool GoDown(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Tripper \"{Name}\" has no body collider.");
}
