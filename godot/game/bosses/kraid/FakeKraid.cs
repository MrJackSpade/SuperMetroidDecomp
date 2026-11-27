using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The small Kraid that guards the way to the real one. It always faces Samus and shuffles to and fro, turning about
/// at walls and every few steps; now and then it stops to spit two gobs in an arc, and all the while spikes fly from
/// its belly, one row after another. Killed, it scatters what it drops.
/// </summary>
/// <remarks>
/// It plays "step_forwards_left", "step_forwards_right", "step_backwards_left", "step_backwards_right",
/// "fire_spit_left" and "fire_spit_right". Their cues are "move", which takes a step; "choose", which picks what it
/// does next; "spit", which lets the two gobs go; and "cry". Its shots are "spit" and "spike"; its sounds are named
/// in its data.
/// </remarks>
public partial class FakeKraid : Enemy
{
    /// <summary>How far each step carries it, in pixels.</summary>
    [Export, DataValue("step_pixels")]
    public int StepPixels { get; set; } = 4;

    /// <summary>How many things it scatters when killed.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 4;

    /// <summary>Its cry, by name.</summary>
    [Export, DataValue("cry_sound")]
    public string CrySound { get; set; } = "";

    /// <summary>The sound of a spike flying, by name.</summary>
    [Export, DataValue("spike_sound")]
    public string SpikeSound { get; set; } = "";

    /// <summary>How far below its place each row of spikes flies from: top, middle, bottom.</summary>
    private static readonly int[] SpikeHeights = [-2, 12, 24];

    /// <summary>How long after it starts each row first fires.</summary>
    private static readonly int[] SpikeStartDelays = [65, 97, 49];

    private readonly int[] _spikeWaits = new int[3];
    private Facing _walking;
    private int _turnWait;
    private int _spitWait;
    private int _nextRow;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        int delay = Random.Last.Part(0, 2) + 2;
        _turnWait = delay;
        _spitWait = delay;
        for (int row = 0; row < 3; row++)
            _spikeWaits[row] = delay + SpikeStartDelays[row];
        _nextRow = 0;
        _walking = SideOfSamus();
        Facing = _walking;
        Play(Facing == Facing.Right ? "step_forwards_right" : "step_forwards_left", restart: true);
    }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        for (int drop = 0; drop < DeathDrops; drop++)
        {
            RandomDraw draw = Random.Draw();
            Drop(Drops, Position + PixelOffset.FromWhole(draw.Part(0, 5) - 16, draw.Part(8, 5) - 16));
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // One row of spikes is looked at each update, in turn.
        int row = _nextRow;
        _nextRow = (row + 1) % 3;
        if (_spikeWaits[row] > 0 && --_spikeWaits[row] > 0)
            return;
        _spikeWaits[row] = Random.Last.Part(0, 6) + 17;
        Projectile spike = Fire("spike", Position + PixelOffset.FromWhole(0, SpikeHeights[row]), new PixelVelocity(Facing.Toward(Shot("spike").Speed), Pixels.Zero));

        // A spike with no place free in the room is not fired, and not heard.
        if (spike.GetParent() == Room && EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
            Sound(SpikeSound);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "move":
                if (_spitWait > 0)
                    _spitWait--;
                // It turns about every few steps, and at a wall.
                bool reverse;
                if (_turnWait > 0 && --_turnWait == 0)
                {
                    reverse = true;
                    _turnWait = Random.Last.Part(0, 2) + 7;
                }
                else
                {
                    reverse = Room.Mover.MoveAcross(MovingBody(), _walking.Toward(Pixels.FromWhole(StepPixels)), StridesAcross).Blocked;
                }

                if (reverse)
                    _walking = _walking.Opposite();
                Facing = SideOfSamus();
                break;

            case "choose":
                string side = Facing == Facing.Right ? "right" : "left";
                if (_spitWait > 0)
                {
                    Play(_walking == Facing ? $"step_forwards_{side}" : $"step_backwards_{side}", restart: true);
                    break;
                }

                _spitWait = Random.Last.Part(0, 2) + 3;
                Play($"fire_spit_{side}", restart: true);
                break;

            case "spit":
                PixelPosition from = Position + PixelOffset.FromWhole(Facing == Facing.Right ? 4 : -4, -16);
                foreach (int speed in new[] { 2, 4 })
                    Fire("spit", from, new PixelVelocity(Facing.Toward(Pixels.FromWhole(speed)), Pixels.FromWhole(-5)));
                break;

            case "cry":
                if (EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
                    Sound(CrySound);
                break;

            default:
                throw new InvalidOperationException($"The small Kraid's timeline \"{Playing}\" gives it the cue \"{cue}\", which it does not know.");
        }
    }

    /// <summary>The side of it Samus is on; level with it counts as left.</summary>
    private Facing SideOfSamus() => Position.X.Whole < Room.Player.Position.X.Whole ? Facing.Right : Facing.Left;

    /// <summary>
    /// How a step across is tried, as the cartridge moves an enemy across: only where it would end, and sloped ground
    /// that holds things up by their middle no part of it.
    /// </summary>
    private static readonly MoveOptions StridesAcross =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The small Kraid \"{Name}\" has no body collider.");
}
