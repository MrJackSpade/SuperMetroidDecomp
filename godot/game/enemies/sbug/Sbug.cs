using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Sbug: a tiny harmless bug that sits still until Samus comes near and then scurries off through everything,
/// straight, or zigzagging, or in short dashes with a wait after each.
/// </summary>
/// <remarks>
/// Its timelines are named for the way it points: "facing_right", "facing_up_right", "facing_up", "facing_up_left",
/// "facing_left", "facing_down_left", "facing_down" and "facing_down_right".
/// </remarks>
public partial class Sbug : Enemy
{
    /// <summary>How an Sbug runs once it is set off.</summary>
    public enum Run
    {
        /// <summary>Straight ahead, for good.</summary>
        Straight,

        /// <summary>Ahead in a zigzag, for good.</summary>
        ZigZag,

        /// <summary>Straight ahead for a short dash, and then it waits for Samus again.</summary>
        Dash,
    }

    /// <summary>How fast it runs, in pixels an update.</summary>
    [Export]
    public int Speed { get; set; } = 1;

    /// <summary>The way it runs, in 256ths of a turn from straight right, turning upward.</summary>
    [Export]
    public int Heading { get; set; }

    /// <summary>How near Samus must come, in pixels across and down, to set it off.</summary>
    [Export]
    public int ActivationDistance { get; set; } = 64;

    /// <summary>How it runs once set off.</summary>
    [Export]
    public Run Runs { get; set; }

    /// <summary>How many updates one dash lasts.</summary>
    [Export, DataValue("dash_updates")]
    public int DashUpdates { get; set; } = 33;

    /// <summary>How many updates each leg of its zigzag lasts.</summary>
    [Export, DataValue("zigzag_leg_updates")]
    public int ZigZagLegUpdates { get; set; } = 16;

    private static readonly string[] Facings =
        ["facing_right", "facing_up_right", "facing_up", "facing_up_left", "facing_left", "facing_down_left", "facing_down", "facing_down_right"];

    private enum Doing
    {
        Waiting,
        SetOff,
        Running,
    }

    private Doing _doing;
    private Countdown _dash;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play(FacingOf(Heading));

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Waiting:
                if (Math.Abs(Position.X.Whole - Target.X.Whole) < ActivationDistance && Math.Abs(Position.Y.Whole - Target.Y.Whole) < ActivationDistance)
                    _doing = Doing.SetOff;
                return;

            case Doing.SetOff:
                _dash = Countdown.Of(DashUpdates);
                _doing = Doing.Running;
                return;

            case Doing.Running:
                int heading = Heading;
                if (Runs == Run.ZigZag)
                {
                    heading += (Updates / ZigZagLegUpdates) % 2 == 1 ? Turns.PerTurn / 8 : -Turns.PerTurn / 8;
                    Play(FacingOf(heading));
                }
                else if (Runs == Run.Dash)
                {
                    _dash = _dash.Stepped();
                    if (_dash.IsOver)
                    {
                        _doing = Doing.Waiting;
                        return;
                    }
                }

                // Its heading turns upward as it grows, so down is the sine a half turn on. As on the cartridge, a
                // zigzag leg moves by the short product and a straight run by the fine share of its speed.
                if (Runs == Run.ZigZag)
                {
                    Position += new PixelOffset(
                        CartridgeAngles.CircleSine(heading + Turns.PerTurn / 4, Speed),
                        CartridgeAngles.CircleSine(heading + Turns.PerTurn / 2, Speed));
                }
                else
                {
                    Position += CartridgeAngles.VelocityAlong(heading, Speed).OverOneUpdate;
                }

                return;

            default:
                throw new InvalidOperationException($"Undefined Sbug stage {_doing}.");
        }
    }

    /// <summary>The name of the timeline for pointing along <paramref name="heading"/>: eight ways, each an eighth of a turn wide.</summary>
    private static string FacingOf(int heading) => Facings[Turns.Wrap(heading - 48) * Facings.Length / Turns.PerTurn];
}
