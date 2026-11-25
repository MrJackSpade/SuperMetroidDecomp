using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
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
                // The aim is kept to the half of the circle on the side named, by turning it right about if need be. As
                // the cartridge counts an angle: none right, a quarter turn up.
                int aim = CartridgeAngles.AimAngle(room.Player.Position.X.Whole - Position.X.Whole, room.Player.Position.Y.Whole - Position.Y.Whole);
                int fromUp = Turns.Wrap(Turns.PerTurn / 4 - aim) % (Turns.PerTurn / 2);
                int kept = Turns.PerTurn / 4 - (cue[1] == "left" ? fromUp + Turns.PerTurn / 2 : fromUp);
                int speedIn256ths = ThrowSpeed * 256;
                Speed = new PixelVelocity(
                    From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, kept + Turns.PerTurn / 4)),
                    From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, kept + Turns.PerTurn / 2)));
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

    /// <summary>A distance counted in 256ths of a pixel.</summary>
    private static Pixels From256ths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 255) * 256);
}
