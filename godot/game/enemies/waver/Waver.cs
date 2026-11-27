using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
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
        if (Room.Mover.MoveAcross(MovingBody(), Facing.Toward(Pixels.FromNumber(Speed)), MovesAcross).Blocked)
        {
            Facing = Facing.Opposite();
            Play(Steady);
        }
        else
        {
            // The wave is in whole pixels a step, as the original's is: the whole pixels of the cartridge's short
            // product a half turn on.
            int down = CartridgeAngles.CircleSine(_angle + Turns.PerTurn / 2, WaveHeight).Whole;
            bool met = Room.Mover.MoveDown(MovingBody(), Pixels.FromWhole(down), Moves).Blocked;
            _angle = Turns.Wrap(met ? _angle + Turns.PerTurn / 2 : _angle + WaveStep);
        }

        if (_angle % (Turns.PerTurn / 2) == SpinsAt)
            Play(Facing == Facing.Left ? "spin_left" : "spin_right");
        if (rolled)
            Play(Steady);
    }

    /// <summary>The timeline of level flight the way it faces.</summary>
    private string Steady => Facing == Facing.Left ? "steady_left" : "steady_right";

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Waver \"{Name}\" has no body collider.");
}
