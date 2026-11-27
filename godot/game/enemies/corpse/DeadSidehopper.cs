using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The Sidehopper the giant Metroid feeds on. It is alive when Samus arrives, hopping across the room, and goes on
/// hopping with the Metroid on its back; then the life drains out of it, its colour going a shade at a time, and it
/// is a husk: solid, and crumbling to dust when Samus comes to rest against it, shoots it or touches it. One placed
/// already dead is a husk from the start.
/// </summary>
/// <remarks>
/// Its timelines are "idle", "hopping", whose cue "end_hop" ends a hop, "corpse" and "initially_dead". Its "Solid"
/// collider is the husk's, and is only there once it is one.
/// </remarks>
public partial class DeadSidehopper : Enemy
{
    /// <summary>Whether it is a husk from the start.</summary>
    [Export]
    public bool AlreadyDead { get; set; }

    /// <summary>It starts to hop once the view's left edge is left of this, in the room's pixels.</summary>
    [Export]
    public int WakesWhenViewLeftOf { get; set; } = 513;

    /// <summary>Left of this, in the room's pixels, it hops through open air above a level floor; right of it, over the room's terrain.</summary>
    [Export]
    public int TerrainFrom { get; set; } = 544;

    /// <summary>Its colour once the life has drained from it.</summary>
    [Export]
    public Color DrainedTint { get; set; } = new(0.55f, 0.55f, 0.5f);

    /// <summary>How many updates it pauses between hops.</summary>
    [Export, DataValue("pause_updates")]
    public int PauseUpdates { get; set; } = 65;

    /// <summary>How many updates each shade of its draining lasts.</summary>
    [Export, DataValue("drain_step_updates")]
    public int DrainStepUpdates { get; set; } = 8;

    /// <summary>How many rows of the husk there are to crumble: the height of its picture.</summary>
    [Export, DataValue("rows")]
    public int Rows { get; set; } = 40;

    /// <summary>How many updates after Samus comes to rest against the husk that it starts to crumble.</summary>
    [Export, DataValue("disturbed_updates")]
    public int DisturbedUpdates { get; set; } = 16;

    /// <summary>The effect of the dust it crumbles into, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The sound of it crumbling, by name.</summary>
    [Export, DataValue("dust_sound")]
    public string DustSound { get; set; } = "";

    /// <summary>Where its dust rises from, from its middle, in pixels.</summary>
    [Export]
    public Vector2I DustFrom { get; set; } = new(-14, 16);

    private enum Doing
    {
        WaitingForView,
        Hopping,
        Landing,
        BeginPause,
        Pause,
        Draining,
        Husk,
    }

    /// <summary>How many shades its draining goes through.</summary>
    private const int DrainStages = 8;

    private static readonly Pixels RisingGravity = Pixels.FromNumber(0.125);
    private static readonly Pixels FallingGravity = Pixels.FromNumber(0.5);

    /// <summary>The speeds it leaves the ground at, hop after hop.</summary>
    private static readonly PixelVelocity[] Launches =
    [
        new(Pixels.FromNumber(1.75), Pixels.FromWhole(-2)), new(Pixels.FromNumber(1.125), Pixels.FromWhole(-2)),
        new(Pixels.FromNumber(1.125), Pixels.FromWhole(-2)), new(Pixels.FromWhole(3), Pixels.FromWhole(-4)),
    ];

    /// <summary>
    /// How its moves over the room's terrain are tried, as the cartridge moves an enemy: only where each would end, and
    /// sloped ground that holds things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private Doing _doing;
    private PixelVelocity _speed;
    private int _nextLaunch;
    private int _floor;
    private int _timer;
    private int _stage;
    private Color _tint = Colors.White;
    private RowCrumble? _crumble;

    /// <inheritdoc/>
    protected override Color RestingTint => _tint;

    /// <summary>The Metroid has fed: the life begins to drain out of it.</summary>
    public void BeginDraining()
    {
        if (_stage == 0)
            _stage = 1;
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (_doing != Doing.Husk)
            return base.TakeHit(hit, part);
        Crumble();
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (_doing != Doing.Husk)
            base.Touch(part);
        else
            Crumble();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _crumble = null;
        if (AlreadyDead)
        {
            (_doing, _stage, _tint) = (Doing.Husk, DrainStages, DrainedTint);
            Play("initially_dead");
            return;
        }

        AlwaysActive = true;
        if (Solid is { } solid)
            solid.Enabled = false;
        (_doing, _stage, _nextLaunch, _tint) = (Doing.WaitingForView, 0, 0, Colors.White);
        _floor = Position.Y.Whole;
        _speed = new PixelVelocity(Pixels.FromNumber(0.375), Pixels.FromWhole(1));
        _timer = DrainStepUpdates;
        Play("idle");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue != "end_hop")
            throw new InvalidOperationException($"The dead Sidehopper's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        if (_doing == Doing.Landing)
            _doing = _stage > 0 ? Doing.Draining : Doing.BeginPause;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitingForView:
                if (Room.Session.Camera.View.Left >= WakesWhenViewLeftOf)
                    break;
                _doing = Doing.Hopping;
                Hop();
                break;

            case Doing.Hopping:
                Hop();
                break;

            case Doing.Landing:
                break;

            case Doing.BeginPause:
                (_doing, _timer) = (Doing.Pause, PauseUpdates);
                break;

            case Doing.Pause:
                if (--_timer > 0)
                    break;
                _timer = DrainStepUpdates;
                if (_stage > 0)
                {
                    _doing = Doing.Draining;
                    break;
                }

                _doing = Doing.Hopping;
                Play("idle", restart: true);
                _speed = Launches[_nextLaunch];
                break;

            case Doing.Draining:
                if (--_timer > 0)
                    break;
                _timer = DrainStepUpdates;
                _stage++;
                _tint = Colors.White.Lerp(DrainedTint, (float)(_stage - 1) / (DrainStages - 1));
                if (_stage < DrainStages)
                    break;
                Play("corpse", restart: true);
                _doing = Doing.Husk;
                AlwaysActive = false;
                if (Solid is { } solid)
                    solid.Enabled = true;
                break;

            case Doing.Husk:
                StepHusk();
                break;

            default:
                throw new InvalidOperationException($"The dead Sidehopper has no step {_doing}.");
        }
    }

    /// <summary>Carries it through one update of a hop; when it comes down, the hop is over.</summary>
    private void Hop()
    {
        bool landed;
        if (Position.X.Whole >= TerrainFrom)
        {
            Room.Mover.MoveAcross(MovingBody(), _speed.X, MovesAcross);
            _speed = _speed.WithY(_speed.Y + (_speed.Y.IsNegative ? RisingGravity : FallingGravity));
            landed = Room.Mover.MoveDown(MovingBody(), _speed.Y, Moves).Blocked;
        }
        else
        {
            Position += new PixelOffset(_speed.X, Pixels.Zero);
            _speed = _speed.WithY(_speed.Y + (_speed.Y.IsNegative ? RisingGravity : FallingGravity));
            Position += new PixelOffset(Pixels.Zero, _speed.Y);
            landed = Position.Y.Whole >= _floor;
        }

        if (!landed)
            return;
        _nextLaunch = (_nextLaunch + 1) % Launches.Length;
        Play("hopping", restart: true);
        _doing = Doing.Landing;
    }

    /// <summary>As a husk it lies until it is disturbed, and then crumbles away.</summary>
    private void StepHusk()
    {
        if (_crumble is null)
        {
            if (PlayerRestsAgainstIt())
                _crumble = new RowCrumble(Rows, DisturbedUpdates);
            return;
        }

        if (!_crumble.Step(LeaveDust))
            return;
        AlwaysActive = true;
        foreach (Collider part in GetChildren().OfType<Collider>())
            part.Enabled = false;
        Visual?.ShowRows(_crumble.Rows);
        if (_crumble.IsOver)
            Vanish();
    }

    /// <summary>Starts the husk crumbling at once.</summary>
    private void Crumble()
    {
        _crumble ??= new RowCrumble(Rows);
        _crumble.StartNow();
    }

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The dead Sidehopper \"{Name}\" has no body collider.");

    /// <summary>
    /// Leaves the puff of dust of a row reaching the ground: one of the enemies' shots on the cartridge, which holds one
    /// of the room's places while it plays and is not made with none free.
    /// </summary>
    private void LeaveDust()
    {
        RandomDraw roll = Random.Last;
        int spread = roll.Part(1, 1) * 2 + roll.Part(3, 2) * 8;
        if (DustEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(DustEffect)), Position + PixelOffset.FromWhole(DustFrom.X + spread, DustFrom.Y), holdsPlace: true));
        if (Room.Session.EnemyPasses % 8 == 0)
            Sound(DustSound);
    }
}
