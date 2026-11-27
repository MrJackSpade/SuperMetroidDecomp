using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Skultera: a fish that swims across the room, bobbing up and down in a wave, and turns round, taking a moment
/// over it, whenever anything is in its way.
/// </summary>
/// <remarks>
/// Its timelines are "swimming_left", "swimming_right", "turning_left" and "turning_right"; a turning timeline's cue
/// "turned" sets it swimming again.
/// </remarks>
public partial class Skultera : Enemy
{
    /// <summary>How far it swims across each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>How far above and below its line it bobs, in pixels.</summary>
    [Export]
    public int Radius { get; set; } = 16;

    /// <summary>How many angle steps, of 256 to a full bob, it advances each update.</summary>
    [Export]
    public int AngleStep { get; set; } = 2;

    private int _angle;
    private int _lastBob;
    private bool _turning;
    private bool _turned;
    private Facing _startFacing;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _startFacing = Facing;
        _lastBob = Bob();
        Swim(restart: false);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "turned")
            _turned = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_turning)
        {
            if (!_turned)
                return;
            _turning = false;
            Swim();
            return;
        }

        if (GoAcross(Facing.Toward(Pixels.FromNumber(Speed))))
        {
            BeginTurn();
            return;
        }

        int bob = Bob();
        if (GoDown(Pixels.FromWhole(bob - _lastBob)))
            BeginTurn();
        else
            // Its wave steps on while it swims the way it set off, and back while it swims the other way.
            _angle = Turns.Wrap(_angle + (Facing == _startFacing ? AngleStep : -AngleStep));
        _lastBob = bob;
    }

    /// <summary>How far below its line the wave puts it now, in whole pixels: the cartridge's short product a half turn on.</summary>
    private int Bob() => CartridgeAngles.CircleSine(_angle + Turns.PerTurn / 2, Radius).Whole;

    /// <summary>
    /// Sets it swimming the way it faces, standing in front of Samus swimming left and far behind her swimming right.
    /// It keeps that standing through a turn, until it swims again.
    /// </summary>
    private void Swim(bool restart = true)
    {
        Standing = Facing == Facing.Left ? Standing.InFront : Standing.FarBehind;
        Play(Facing == Facing.Left ? "swimming_left" : "swimming_right", restart);
    }

    /// <summary>Starts it turning to swim the other way.</summary>
    private void BeginTurn()
    {
        _turning = true;
        _turned = false;
        Facing = Facing.Opposite();
        Play(Facing == Facing.Left ? "turning_left" : "turning_right", restart: true);
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
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Skultera \"{Name}\" has no body collider.");
}
