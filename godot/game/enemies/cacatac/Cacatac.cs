using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Cacatac: a cactus that shuffles back and forth along a floor or a ceiling and, now and then by chance, stops to
/// throw its spikes out in five directions.
/// </summary>
/// <remarks>
/// Its timelines are "idle" and "attack", or "idle_ceiling" and "attack_ceiling" for one on a ceiling. The attack's
/// cue "spike:" and a direction throws one spike that way: "left", "up_left", "up", "up_right" or "right" from a
/// floor, and the same with "down" from a ceiling. Its cue "patrol" sets it shuffling again. Its shot is "spike".
/// </remarks>
public partial class Cacatac : Enemy
{
    /// <summary>Whether it hangs from a ceiling.</summary>
    [Export]
    public bool OnCeiling { get; set; }

    /// <summary>Whether it starts stopped, and stays so until an attack sets it shuffling.</summary>
    [Export]
    public bool StartsStopped { get; set; }

    /// <summary>How far to each side of where it starts it shuffles, in pixels.</summary>
    [Export]
    public int TravelDistance { get; set; } = 32;

    /// <summary>How far it shuffles each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <summary>How fast a spike thrown straight across or straight up or down flies.</summary>
    [Export, DataValue("spike_speed")]
    public double SpikeSpeed { get; set; } = 2;

    /// <summary>How fast a spike thrown diagonally flies along each axis.</summary>
    [Export, DataValue("diagonal_spike_speed")]
    public double DiagonalSpikeSpeed { get; set; } = 1.5;

    /// <summary>Out of 256, how many of the numbers that can be drawn set off an attack.</summary>
    [Export, DataValue("attack_chances")]
    public int AttackChances { get; set; } = 3;

    private bool _stopped;
    private int _left;
    private int _right;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _stopped = StartsStopped;
        _left = Position.X.Whole - TravelDistance;
        _right = Position.X.Whole + TravelDistance;
        Play(OnCeiling ? "idle_ceiling" : "idle");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "patrol")
        {
            _stopped = false;
            return;
        }

        if (!cue.StartsWith("spike:", StringComparison.Ordinal))
            return;
        Pixels straight = Pixels.FromNumber(SpikeSpeed), diagonal = Pixels.FromNumber(DiagonalSpikeSpeed);
        Pixels none = Pixels.Zero;
        PixelVelocity velocity = cue["spike:".Length..] switch
        {
            "left" => new(-straight, none),
            "right" => new(straight, none),
            "up" => new(none, -straight),
            "down" => new(none, straight),
            "up_left" => new(-diagonal, -diagonal),
            "up_right" => new(diagonal, -diagonal),
            "down_left" => new(-diagonal, diagonal),
            "down_right" => new(diagonal, diagonal),
            _ => throw new InvalidOperationException($"The Cacatac's timeline throws a spike \"{cue}\", which is no direction it has."),
        };
        Fire("spike", Position, velocity);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_stopped)
            return;
        // It moves by its own steps and turns at the ends of its beat; nothing in the room stops it.
        Position += new PixelOffset(Facing.Toward(Pixels.FromNumber(Speed)), Pixels.Zero);
        if (Facing == Facing.Left ? Position.X.Whole < _left : Position.X.Whole >= _right)
            Facing = Facing.Opposite();

        // One number is drawn every update of its shuffle, and with how long it has lived settles whether it attacks.
        if (((Random.Draw().Part(0, 8) + Updates) & 255) >= AttackChances)
            return;
        _stopped = true;
        Play(OnCeiling ? "attack_ceiling" : "attack", restart: true);
    }
}
