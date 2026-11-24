using Engine.Shared;
using Engine.World.Objects;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The Dachora trapped in the escape. It runs laps of its cell, faster once the acid has risen, and when Samus has
/// blown the wall open it finishes its lap and runs out. Everything it does is in its timelines.
/// </summary>
/// <remarks>
/// Its timelines are "running_around_low_tide", one "high_tide_after_" a lap for each lap of the low-tide ones,
/// "escape_after_left_lap" and "escape_after_right_lap". Their cues are "low_tide_lap_ends:" and the lap that has
/// ended, where its laps turn to the high-tide ones if the acid has risen; "lap_ends:" and "left" or "right", where
/// its laps turn to its way out if the wall is open; "move:" with pixels across; and "gone", which takes it away.
/// The lap a "low_tide_lap_ends:" names is what stands for it in the name of the high-tide timeline it turns to:
/// "left_3" turns to "high_tide_after_left_3_lap", the high-tide laps going on from its third lap to the left.
/// </remarks>
public partial class EscapeDachora : Creature
{
    /// <summary>The acid counts as low while its surface is at least this far down the room, in pixels.</summary>
    [Export]
    public int LowTideSurface { get; set; } = 206;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("running_around_low_tide");

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "low_tide_lap_ends":
                if (Room.Liquid is { } acid && acid.Surface.Whole < LowTideSurface)
                    Play($"high_tide_after_{parts[1]}_lap", restart: true);
                break;
            case "lap_ends":
                if (Room.Session.Progress.Has(StoryEvents.AnimalsEscaped))
                    Play($"escape_after_{parts[1]}_lap", restart: true);
                break;
            case "move":
                Position += PixelOffset.FromWhole(int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), 0);
                break;
            case "gone":
                Room.Remove(this);
                break;
            default:
                throw new InvalidOperationException($"The trapped Dachora's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
    }
}
