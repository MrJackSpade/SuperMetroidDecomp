using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A super missile Golden Torizo has caught. It stays in the Torizo's raised hand until its timeline has it thrown:
/// the cue "aim:right" or "aim:left" points it at Samus, or straight away from her if she is not on that side, and
/// "throw" lets it go. It arcs down and blows up on the first wall or floor it meets.
/// </summary>
/// <remarks>Its timelines are "held_right", "held_left" and "impact".</remarks>
public partial class TorizoSuperMissile : CuedProjectile
{
    /// <summary>The speed it is thrown at, in pixels an update.</summary>
    [Export, DataValue("throw_speed")]
    public int ThrowSpeed { get; set; } = 4;

    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0625;

    private Torizo? _holder;
    private bool _thrown;

    /// <summary>Puts it in a Torizo's hand.</summary>
    public void HeldBy(Torizo torizo)
    {
        _holder = torizo;
        ShowTimeline(torizo.FacesRight ? "held_right" : "held_left");
    }

    /// <inheritdoc/>
    protected override void OnOwnCue(Room room, string[] cue)
    {
        switch (cue[0])
        {
            case "aim":
                // The aim is kept to the half of the circle on the side named, by turning it right about if need be.
                PixelOffset toSamus = room.Player.Position - Position;
                int aim = Turns.AngleOf(toSamus.X.Whole, toSamus.Y.Whole);
                int kept = Turns.Wrap(aim + Turns.PerTurn / 4) % (Turns.PerTurn / 2) - Turns.PerTurn / 4;
                if (cue[1] == "left")
                    kept += Turns.PerTurn / 2;
                Pixels speed = Pixels.FromWhole(ThrowSpeed);
                Speed = new PixelVelocity(Turns.Cosine(kept, speed), Turns.Sine(kept, speed));
                break;
            case "throw":
                _thrown = true;
                break;
            default:
                base.OnOwnCue(room, cue);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        if (!_thrown)
        {
            if (_holder is { } holder && IsInstanceValid(holder))
                Position = holder.Position + holder.Ahead(PixelOffset.FromWhole(32, -52));
            return;
        }

        bool met = MoveAcrossMeetingTerrain(room, Speed.X);
        if (!met)
            met = MoveDownMeetingTerrain(room, Speed.Y) && !Speed.Y.IsNegative;
        if (met)
        {
            Moving = false;
            ShowTimeline("impact");
            return;
        }

        Speed = Speed.WithY(Speed.Y + Pixels.FromNumber(Gravity));
        if (IsRunaway(Speed.Y))
            Leave(room);
    }
}
