using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Baby Turtle: it crawls back and forth near where it was placed, over its mother's shell while she sleeps. Samus
/// can stand on it; stood on, it hides in its shell, and stood on again while hiding it spins away across the room.
/// Bumped from the side it turns round, and its mother wakes.
/// </summary>
/// <remarks>
/// Its timelines are "crawling_left" and "crawling_right", whose cue "crawl" takes one step and "loop" ends a stretch
/// of crawling; "hiding_left" and "hiding_right", whose cue "come_out" brings it out if Samus is still on it;
/// "leave_shell", whose cue "out" sets it crawling; and "spinning", whose cue "stoppable" lets Samus stop it by
/// standing on it. Its hurtbox reaches a pixel past its solid collider on either side, so that a bump is felt.
/// </remarks>
public partial class BabyTurtle : Enemy
{
    /// <summary>Its mother.</summary>
    [Export]
    public NodePath Mother { get; set; } = new();

    /// <summary>How far it crawls each step, in pixels.</summary>
    [Export, DataValue("crawl_speed")]
    public int CrawlSpeed { get; set; } = 1;

    /// <summary>How far to either side of where it was placed it crawls, in pixels.</summary>
    [Export, DataValue("travel_distance")]
    public int TravelDistance { get; set; } = 48;

    /// <summary>How many updates after Samus steps off it stays hidden before it counts her gone.</summary>
    [Export, DataValue("rider_left_delay")]
    public int RiderLeftDelay { get; set; } = 4;

    /// <summary>How many updates it stays hidden once she is gone.</summary>
    [Export, DataValue("hide_updates")]
    public int HideUpdates { get; set; } = 60;

    /// <summary>How far it spins across each update, in pixels.</summary>
    [Export, DataValue("spin_speed")]
    public int SpinSpeed { get; set; } = 3;

    /// <summary>How far it sinks each update while spinning, in pixels.</summary>
    [Export, DataValue("spin_fall_speed")]
    public int SpinFallSpeed { get; set; } = 1;

    private enum Doing
    {
        Crawling,
        HidingRidden,
        Hiding,
        SpinningFree,
        SpinningStoppable,
        CrawlingRidden,
    }

    private Doing _doing;
    private Countdown _wait;
    private Countdown _riderGone;
    private PixelPosition _placedTop;
    private int _speed;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        int halfHeight = Body is { } body ? body.Size.Y / 2 : 0;
        _placedTop = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole - halfHeight);
        _speed = CrawlSpeed;
        Play(Crawl);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "crawl":
                CrawlStep();
                break;
            case "loop":
                int toPlace = _placedTop.X.Whole - Position.X.Whole;
                if (Math.Abs(toPlace) >= TravelDistance)
                {
                    Facing = toPlace < 0 ? Facing.Left : Facing.Right;
                    _speed = CrawlSpeed;
                }

                Play(Crawl, restart: true);
                break;
            case "come_out":
                if (IsRiddenByPlayer)
                    Play("leave_shell", restart: true);
                break;
            case "out":
                _doing = IsRiddenByPlayer ? Doing.CrawlingRidden : Doing.Crawling;
                Play(Crawl, restart: true);
                break;
            case "stoppable":
                _doing = Doing.SpinningStoppable;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // A bump from the side turns it round, puts it clear of Samus, and wakes its mother.
        if (_doing == Doing.CrawlingRidden || IsRiddenByPlayer)
            return;
        Facing = Facing.Opposite();
        _speed = CrawlSpeed;
        Play(Crawl, restart: true);
        PixelRect samus = Room.Player.BodyRect;
        int halfWidth = Body is { } body ? body.Size.X / 2 : 0;
        int x = Position.X.Whole >= Room.Player.Position.X.Whole ? samus.Right + halfWidth : samus.Left - halfWidth;
        Position = new PixelPosition(Position.X.WithWhole(x), Position.Y);
        _doing = Doing.Crawling;
        Stride(Facing.Toward(Pixels.FromWhole(_speed)));
        if (!Mother.IsEmpty && GetNodeOrNull<MamaTurtle>(Mother) is { } mother)
            mother.Rouse();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        bool ridden = IsRiddenByPlayer;
        switch (_doing)
        {
            case Doing.Crawling:
                if (!ridden)
                    return;
                _doing = Doing.HidingRidden;
                _riderGone = Countdown.Of(RiderLeftDelay);
                Play(Facing == Facing.Left ? "hiding_left" : "hiding_right", restart: true);
                return;

            case Doing.HidingRidden:
                if (ridden)
                {
                    _riderGone = Countdown.Of(RiderLeftDelay);
                    return;
                }

                _riderGone = _riderGone.Stepped();
                if (_riderGone.IsRunning)
                    return;
                _doing = Doing.Hiding;
                _wait = Countdown.Of(HideUpdates);
                return;

            case Doing.Hiding:
                if (ridden)
                {
                    // Stood on again in its shell, it spins off the way Samus faces.
                    _doing = Doing.SpinningFree;
                    Play("spinning", restart: true);
                    Facing = Room.Player.Facing;
                    _speed = SpinSpeed;
                    return;
                }

                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                Play(Crawl, restart: true);
                _doing = Doing.Crawling;
                return;

            case Doing.SpinningFree:
                Spin();
                return;

            case Doing.SpinningStoppable:
                if (ridden)
                {
                    Play(Crawl, restart: true);
                    _doing = Doing.Crawling;
                    return;
                }

                Spin();
                return;

            case Doing.CrawlingRidden:
                if (!ridden)
                    _doing = Doing.Crawling;
                return;

            default:
                throw new InvalidOperationException($"Undefined Baby Turtle stage {_doing}.");
        }
    }

    /// <summary>The name of its crawling timeline for the way it faces.</summary>
    private string Crawl => Facing == Facing.Left ? "crawling_left" : "crawling_right";

    /// <summary>Spins one update across the room, turning at walls and sinking as it goes.</summary>
    private void Spin()
    {
        if (Stride(Facing.Toward(Pixels.FromWhole(_speed))))
        {
            Facing = Facing.Opposite();
            return;
        }

        Drop(Pixels.FromWhole(SpinFallSpeed));
    }

    /// <summary>
    /// Takes one crawling step, over its mother's shell if she lies in it. Samus standing on it is taken along as far
    /// across as the step asks, wall or not, and up or down as it went only while it walks the shell.
    /// </summary>
    private void CrawlStep()
    {
        bool ridden = IsRiddenByPlayer;
        Pixels step = Facing.Toward(Pixels.FromWhole(_speed));
        if (ridden)
            ShiftPlayer(new PixelOffset(step, Pixels.Zero));
        int beforeY = Position.Y.Whole;
        Position = new PixelPosition(Position.X, Position.Y.WithWhole(_placedTop.Y.Whole));
        Stride(step);
        if (Mother.IsEmpty || GetNodeOrNull<MamaTurtle>(Mother) is not { LiesInShell: true } mother)
            return;
        Drop(Pixels.FromWhole(mother.ShellSurface(Position.X.Whole - mother.Position.X.Whole) ?? 1));
        if (ridden)
            ShiftPlayer(PixelOffset.FromWhole(0, Position.Y.Whole - beforeY));
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>Moves it across by <paramref name="distance"/>; returns whether something stopped it.</summary>
    private bool Stride(Pixels distance) => Room.Mover.MoveAcross(MovingBody(), distance, MovesAcross).Blocked;

    /// <summary>Moves it down by <paramref name="distance"/>, upward when less than nothing; returns whether something stopped it.</summary>
    private bool Drop(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Baby Turtle \"{Name}\" has no body collider.");
}
