using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Fune or a Namihe: a stone face in a wall that spits a fireball straight out across the room. A Fune spits on a
/// steady beat; a Namihe spits whenever Samus is level with it.
/// </summary>
/// <remarks>
/// Its timelines are "idle_left", "idle_right", "active_left" and "active_right". The cue "fire" of the active ones
/// lets the fireball go and "done" ends the spit. Its shot is "fireball".
/// </remarks>
public partial class Fune : Enemy
{
    /// <summary>How many updates pass between its spits. None, and it waits for Samus to come level instead.</summary>
    [Export]
    public int CooldownUpdates { get; set; }

    /// <summary>How near Samus's height must be to its own, in pixels, for one that waits for her to spit.</summary>
    [Export]
    public int LevelDistance { get; set; }

    /// <summary>How fast its fireball flies, as a number of pixels an update.</summary>
    [Export]
    public double LaunchSpeed { get; set; } = 2;

    /// <summary>How far below its middle its mouth is, in pixels.</summary>
    [Export, DataValue("mouth_drop")]
    public int MouthDrop { get; set; }

    private Countdown _cooldown;
    private bool _spitting;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _cooldown = Countdown.Of(Math.Max(1, CooldownUpdates));
        Play(Named("idle"));
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "fire")
        {
            Fire("fireball", Position + PixelOffset.FromWhole(0, MouthDrop), new PixelVelocity(Facing.Toward(Pixels.FromNumber(LaunchSpeed)), Pixels.Zero));
        }
        else if (cue == "done")
        {
            _spitting = false;
            Play(Named("idle"), restart: true);
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_spitting)
            return;
        if (CooldownUpdates > 0)
        {
            _cooldown = _cooldown.Stepped();
            if (_cooldown.IsRunning)
                return;
            _cooldown = Countdown.Of(Math.Max(1, CooldownUpdates));
        }
        else if (Math.Abs(Target.Y.Whole - Position.Y.Whole) >= LevelDistance)
        {
            return;
        }

        _spitting = true;
        Play(Named("active"), restart: true);
    }

    /// <summary>The name of one of its timelines for the way it faces.</summary>
    private string Named(string timeline) => Facing == Facing.Left ? $"{timeline}_left" : $"{timeline}_right";
}
