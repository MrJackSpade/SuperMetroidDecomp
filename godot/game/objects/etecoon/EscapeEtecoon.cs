using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World.Objects;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// One of the Etecoons trapped in the escape. Two run back and forth across their cell, more wildly once the acid
/// has risen; the third waits by the wall. When Samus has blown the wall open they show their thanks and run out.
/// </summary>
/// <remarks>
/// Its timelines are "stationary", "running_left_low_tide", "running_right_low_tide", "running_left_high_tide",
/// "running_right_high_tide", "running_for_escape" and "gratitude_then_escape". Their cues are "low_tide_run:" and
/// "left" or "right", at the start of a run that gives way to the high-tide one when the acid has risen; "move:"
/// with pixels across; "gratitude"; and "escape".
/// </remarks>
public partial class EscapeEtecoon : Creature
{
    /// <summary>Whether it waits by the wall instead of running.</summary>
    [Export]
    public bool Waits { get; set; }

    /// <summary>The way it runs to begin with.</summary>
    [Export]
    public Facing StartsFacing { get; set; } = Facing.Left;

    /// <summary>How fast it runs, in pixels an update.</summary>
    [Export]
    public double RunSpeed { get; set; } = 2;

    /// <summary>The acid counts as low while its surface is at least this far down the room, in pixels.</summary>
    [Export]
    public int LowTideSurface { get; set; } = 206;

    private enum Doing
    {
        Running,
        Waiting,
        Grateful,
        Escaping,
    }

    private Doing _doing;
    private bool _thanked;

    private bool HaveEscaped => Room.Session.Progress.Has(StoryEvents.AnimalsEscaped);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Facing = StartsFacing;
        _thanked = false;
        _doing = Waits ? Doing.Waiting : Doing.Running;
        Play(Waits ? "stationary" : Facing == Facing.Left ? "running_left_low_tide" : "running_right_low_tide");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "low_tide_run":
                if (Room.Liquid is { } acid && acid.Surface.Whole < LowTideSurface)
                    Play(parts[1] == "left" ? "running_left_high_tide" : "running_right_high_tide", restart: true);
                break;
            case "move":
                Position += PixelOffset.FromWhole(int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), 0);
                break;
            case "gratitude":
                _doing = Doing.Grateful;
                break;
            case "escape":
                _doing = Doing.Escaping;
                break;
            default:
                throw new InvalidOperationException($"The trapped Etecoon's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Running:
                if (MoveAcrossBy(Pixels.FromNumber(Facing == Facing.Right ? RunSpeed : -RunSpeed)))
                {
                    // At each wall it turns; once the way is open, its next run is its way out.
                    Facing = Facing.Opposite();
                    Play(HaveEscaped ? "running_for_escape" : Facing == Facing.Left ? "running_left_low_tide" : "running_right_low_tide", restart: true);
                }

                MoveDownBy(Pixels.FromWhole(1), SlopeTreatment.Climb);
                break;

            case Doing.Waiting:
                if (HaveEscaped && !_thanked)
                {
                    _thanked = true;
                    Play("gratitude_then_escape", restart: true);
                }

                break;

            case Doing.Grateful:
                break;

            case Doing.Escaping:
                Position += new PixelOffset(Pixels.FromNumber(3.5), Pixels.Zero);
                if (Position.X.Whole > Room.Terrain.WidthInPixels + 64)
                    Room.Remove(this);
                break;

            default:
                throw new InvalidOperationException($"The trapped Etecoon has no step {_doing}.");
        }
    }
}
