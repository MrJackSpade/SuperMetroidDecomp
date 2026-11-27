using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A swooper with a tail of flame: it perches until Samus comes within reach across and chance lets it go, then
/// swoops down at her in an arc, and climbs back up until it meets the ceiling. The Geruta and the Holtz are both
/// this scene with their own data files. Its flames are a thing of the room in their own right, placed after it
/// (<see cref="SwooperFlames"/>): it only says which flames it shows and how far below it they lie.
/// </summary>
/// <remarks>
/// Its timelines are "idle", "takeoff", "dive", "climb_start", "climb" and, where its landing takes a moment,
/// "land". The cue "finished" marks the end of a timeline that leads on to the next thing; "flames_down" and
/// "flames_up" show the flames under a diving or climbing body and "flames_off" hides them, each cue's number after a
/// colon saying how far below the body they are. Its flames' timelines are "flames_down" and "flames_up", which
/// its flames play from its data file.
/// </remarks>
public partial class Swooper : Enemy
{
    /// <summary>How near across Samus must come, in pixels, to set it off.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 192;

    /// <summary>How far it travels across each update of a swoop, as a number of pixels.</summary>
    [Export, DataValue("across_speed")]
    public double AcrossSpeed { get; set; } = 1;

    /// <summary>The downward speed a swoop starts with.</summary>
    [Export, DataValue("dive_speed")]
    public double DiveSpeed { get; set; } = 7;

    /// <summary>The downward speed a slow swoop starts with; the same as the other for one that never swoops slowly.</summary>
    [Export, DataValue("slow_dive_speed")]
    public double SlowDiveSpeed { get; set; } = 7;

    /// <summary>The downward speed a swoop loses each update.</summary>
    [Export, DataValue("lift")]
    public double Lift { get; set; } = 0.125;

    /// <summary>The upward speed its climb back starts with.</summary>
    [Export, DataValue("return_speed")]
    public double ReturnSpeed { get; set; } = 0.00390625;

    /// <summary>The sound of a swoop beginning, by name.</summary>
    [Export, DataValue("dive_sound")]
    public string DiveSound { get; set; } = "";

    /// <summary>The sound of the climb back beginning, by name.</summary>
    [Export, DataValue("return_sound")]
    public string ReturnSound { get; set; } = "";

    /// <summary>
    /// Whether it shows its idle on every update it stays perched. One that does not goes back to its idle only when
    /// a timeline it is showing finishes, keeping its climb and its flames until then.
    /// </summary>
    [Export, DataValue("idles_whenever_perched")]
    public bool IdlesWheneverPerched { get; set; }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsAgainstSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private enum Doing
    {
        Perched,
        TakingOff,
        Diving,
        Climbing,
        Landing,
    }

    private Doing _doing;
    private bool _finished;
    private string? _flames;

    /// <summary>The timeline of the flames it shows now, by name; null while it shows none, as while it idles.</summary>
    public string? FlamesShown => Playing != "idle" ? _flames : null;

    /// <summary>How far below it its flames lie, in pixels.</summary>
    public int FlamesBelow { get; private set; }

    /// <summary>The offset its pictures are drawn at, which its flames' are drawn at too.</summary>
    public Vector2I PictureOffset => Visual?.PictureOffset ?? Vector2I.Zero;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("idle");

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "finished":
                _finished = true;
                break;
            case "flames_down" or "flames_up":
                FlamesBelow = parts.Length > 1 ? int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) : FlamesBelow;
                _flames = parts[0];
                break;
            case "flames_off":
                _flames = null;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // It draws a number every update, whatever it is doing.
        RandomDraw drawn = Random.Draw();
        switch (_doing)
        {
            case Doing.Perched:
                // Chance holds it back when the lowest digit of each half of the number drawn is clear.
                bool heldBack = drawn.Part(0, 1) == 0 && drawn.Part(8, 1) == 0;
                if (heldBack || Math.Abs(Target.X.Whole - Position.X.Whole) >= Reach)
                {
                    if (_finished || IdlesWheneverPerched)
                        Play("idle");
                    _finished = false;
                    break;
                }

                // The same number picks a fast or a slow swoop.
                double dive = drawn.Part(2, 1) == 1 ? SlowDiveSpeed : DiveSpeed;
                Velocity = new PixelVelocity(TowardPlayer.Toward(Pixels.FromNumber(AcrossSpeed)), Pixels.FromNumber(dive));
                Play("takeoff");
                _doing = Doing.TakingOff;
                break;

            case Doing.TakingOff:
                if (!_finished)
                    break;
                _finished = false;
                Play("dive");
                _doing = Doing.Diving;
                Sound(DiveSound);
                break;

            case Doing.Diving:
                if (MoveAcross(MovesAcross).Blocked)
                    Velocity = Velocity.WithX(-Velocity.X);
                if (MoveDown(Moves).Blocked)
                {
                    BeginClimb();
                    break;
                }

                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(Lift));
                if (Velocity.Y.IsNegative)
                    BeginClimb();
                break;

            case Doing.Climbing:
                if (MoveAcross(MovesAcross).Blocked)
                    Velocity = Velocity.WithX(-Velocity.X);
                if (MoveDown(Moves).Blocked)
                {
                    // An animation's finish is kept until it is taken, through the landing: one its climb's start gave
                    // ends a landing at once.
                    if (Timelines.Has("land"))
                        Play("land");
                    _doing = Doing.Landing;
                    break;
                }

                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(Lift));
                if (_finished)
                {
                    _finished = false;
                    Play("climb");
                }

                break;

            case Doing.Landing:
                // With no timeline of its own the landing takes this one update and leaves the finish to the perch.
                if (!Timelines.Has("land"))
                {
                    _doing = Doing.Perched;
                    break;
                }

                if (!_finished)
                    break;
                _finished = false;
                _doing = Doing.Perched;
                break;

            default:
                throw new InvalidOperationException($"Undefined swooper stage {_doing}.");
        }
    }

    /// <summary>Starts the climb back to the perch.</summary>
    private void BeginClimb()
    {
        Velocity = Velocity.WithY(-Pixels.FromNumber(ReturnSpeed));
        Play("climb_start");
        _doing = Doing.Climbing;
        Sound(ReturnSound);
    }
}
