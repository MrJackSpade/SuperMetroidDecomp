using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>Travelling the way it faces at a set speed while rising and falling in a wave. A wall turns it around.</summary>
public sealed class WaveFlight
{
    /// <summary>How far it travels across each update.</summary>
    public Pixels Speed { get; set; } = Pixels.FromWhole(1);

    /// <summary>How far it rises above and falls below its line.</summary>
    public Pixels WaveSize { get; set; } = Pixels.FromWhole(16);

    /// <summary>How many angle steps, of 256 to a full wave, it advances each update.</summary>
    public int WaveStep { get; set; } = 4;

    private int _phase;
    private Pixels _offset;

    /// <summary>Moves <paramref name="mover"/> for one update.</summary>
    public MoveReport Step(IMoves mover)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions air = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };

        bool hitWall = room.MoveAcross(body, mover.Facing.Toward(Speed), air).Blocked;
        if (hitWall)
            mover.Facing = mover.Facing.Opposite();

        _phase += WaveStep;
        Pixels offset = Turns.Sine(_phase, WaveSize);
        Pixels change = offset - _offset;
        _offset = offset;
        bool stopped = room.MoveDown(body, change, air).Blocked;
        mover.Velocity = new PixelVelocity(mover.Facing.Toward(Speed), change);
        return new MoveReport(hitWall, stopped && !change.IsNegative, stopped && change.IsNegative, false, false);
    }
}
