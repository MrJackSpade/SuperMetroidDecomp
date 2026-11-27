using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Rio: it hovers until Samus comes within reach across, then swoops down at her in an arc. If the swoop bottoms
/// out above her it drifts on toward her until it is level with her. Then, or on meeting anything solid, it climbs
/// back up until it meets the ceiling, and settles again.
/// </summary>
/// <remarks>
/// Its timelines are "idle", "swoop_start", whose cue "finished" lets "swoop" follow it, "land", whose cue
/// "finished" ends the landing, and "hover".
/// </remarks>
public partial class Rio : Enemy
{
    /// <summary>How near across Samus must come, in pixels, to set it off.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 160;

    /// <summary>How far it travels across each update of a swoop, as a number of pixels.</summary>
    [Export, DataValue("across_speed")]
    public double AcrossSpeed { get; set; } = 1.5;

    /// <summary>The downward speed a swoop starts with.</summary>
    [Export, DataValue("dive_speed")]
    public double DiveSpeed { get; set; } = 5.5;

    /// <summary>The downward speed a swoop loses each update.</summary>
    [Export, DataValue("lift")]
    public double Lift { get; set; } = 0.09375;

    /// <summary>The upward speed its climb back starts with.</summary>
    [Export, DataValue("return_speed")]
    public double ReturnSpeed { get; set; } = 0.00390625;

    /// <summary>The sound of a swoop beginning, by name: heard only from one inside the window in which enemies take turns.</summary>
    [Export, DataValue("dive_sound")]
    public string DiveSound { get; set; } = "";

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
        Waiting,
        Swooping,
        Drifting,
        Climbing,
        Landing,
    }

    private Doing _doing;
    private Pixels _savedAcross;
    private bool _finished;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("idle");

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "finished")
            _finished = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // The original draws a number for every Rio every update and uses none of them; later draws depend on it.
        Random.Next();
        switch (_doing)
        {
            case Doing.Waiting:
                if (Math.Abs(Target.X.Whole - Position.X.Whole) >= Reach)
                    return;
                Velocity = new PixelVelocity(TowardPlayer.Toward(Pixels.FromNumber(AcrossSpeed)), Pixels.FromNumber(DiveSpeed));
                Play("swoop_start");
                _doing = Doing.Swooping;

                // Its cry is heard only when the swoop begins inside the window in which enemies take turns.
                if (IsWithinTurnReach)
                    Sound(DiveSound);
                return;

            case Doing.Swooping:
                if (MoveAcross(MovesAcross).Blocked)
                {
                    Velocity = new PixelVelocity(-Velocity.X, -Velocity.Y);
                    _doing = Doing.Climbing;
                    return;
                }

                if (MoveDown(Moves).Blocked)
                {
                    Velocity = Velocity.WithY(-Velocity.Y);
                    _doing = Doing.Climbing;
                    return;
                }

                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(Lift));
                if (Velocity.Y.IsNegative)
                {
                    _savedAcross = Velocity.X;
                    Velocity = PixelVelocity.Zero;
                    _doing = Doing.Drifting;
                }
                else if (_finished)
                {
                    _finished = false;
                    Play("swoop");
                }

                return;

            case Doing.Drifting:
                // Level with Samus, it takes back the speed across it swooped with and starts its climb.
                if (Position.Y.Whole >= Target.Y.Whole)
                {
                    Velocity = new PixelVelocity(_savedAcross, -Pixels.FromNumber(ReturnSpeed));
                    _doing = Doing.Climbing;
                    return;
                }

                // As the cartridge has it: the aim between whole pixels, counting up from right, and a pixel along its
                // sine across and along its sine a quarter turn on down, in 256ths of a pixel.
                int aim = CartridgeAngles.AimAngle(Target.X.Whole - Position.X.Whole, Target.Y.Whole - Position.Y.Whole);
                Velocity = new PixelVelocity(
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, aim)),
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, aim + 64)));
                MoveAcross(MovesAcross);
                MoveDown(Moves);
                return;

            case Doing.Climbing:
                if (MoveAcross(MovesAcross).Blocked)
                    Velocity = Velocity.WithX(-Velocity.X);
                if (MoveDown(Moves).Blocked)
                {
                    // An animation's finish is kept until it is taken: one its swoop's opening gave after a wall or the
                    // floor turned it back ends its landing at once.
                    Play("land");
                    _doing = Doing.Landing;
                }
                else
                {
                    Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(Lift));
                }

                return;

            case Doing.Landing:
                if (!_finished)
                    return;
                _finished = false;
                Play("hover");
                _doing = Doing.Waiting;
                return;

            default:
                throw new InvalidOperationException($"Undefined Rio stage {_doing}.");
        }
    }

    /// <summary>A distance given in 256ths of a pixel.</summary>
    private static Pixels FromTwoFiftySixths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 0xFF) << 8);
}
