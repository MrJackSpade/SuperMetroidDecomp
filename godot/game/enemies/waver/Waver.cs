using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Waver: it flies across the room rising and falling in a wave, turns round at walls, and rolls over once in every
/// half wave. A floor or ceiling in its way turns the wave back on itself.
/// </summary>
/// <remarks>Its timelines are "steady_left", "steady_right", "spin_left" and "spin_right".</remarks>
public partial class Waver : Enemy
{
    /// <summary>How far it flies across each update, as a number of pixels.</summary>
    [Export, DataValue("speed")]
    public double Speed { get; set; } = 1.5;

    /// <summary>The most it rises or falls in one update, in pixels.</summary>
    [Export, DataValue("wave_height")]
    public int WaveHeight { get; set; } = 4;

    /// <summary>How many angle steps, of 256 to a full wave, the wave advances each update.</summary>
    [Export, DataValue("wave_step")]
    public int WaveStep { get; set; } = 2;

    /// <summary>The place within each half wave, in angle steps, at which it rolls over.</summary>
    [Export, DataValue("spins_at")]
    public int SpinsAt { get; set; } = 56;

    private int _angle;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play(Steady);

    /// <inheritdoc/>
    protected override void Behave()
    {
        // A roll that has played through gives way to level flight at the end of this update, whatever the flight does.
        bool rolled = Playing.StartsWith("spin", StringComparison.Ordinal) && AnimationIsOver;
        if (MoveAcrossBy(Facing.Toward(Pixels.FromNumber(Speed))))
        {
            Facing = Facing.Opposite();
            Play(Steady);
        }
        else
        {
            // The wave is in whole pixels a step, as the original's is.
            int down = -Turns.Sine(_angle, Pixels.FromWhole(WaveHeight), Turns.Fineness.Coarse).Whole;
            _angle = Turns.Wrap(MoveDownBy(Pixels.FromWhole(down)) ? _angle + Turns.PerTurn / 2 : _angle + WaveStep);
        }

        if (_angle % (Turns.PerTurn / 2) == SpinsAt)
            Play(Facing == Facing.Left ? "spin_left" : "spin_right");
        if (rolled)
            Play(Steady);
    }

    /// <summary>The timeline of level flight the way it faces.</summary>
    private string Steady => Facing == Facing.Left ? "steady_left" : "steady_right";
}
