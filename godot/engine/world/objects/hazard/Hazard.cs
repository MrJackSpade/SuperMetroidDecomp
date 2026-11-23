using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A fixture that hurts or grabs the player on contact and cannot be destroyed. It can deliver a hit, do ongoing
/// damage, hold the player for a while, or any of these together.
/// </summary>
public partial class Hazard : RoomObject
{
    /// <summary>The hazard's width and height, in pixels; its top-left is where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 16);

    /// <summary>The damage of the hit it delivers on contact; zero for none.</summary>
    [Export]
    public int HitDamage { get; set; }

    /// <summary>The ongoing damage it does each update of contact, in hundredths of a point; zero for none.</summary>
    [Export]
    public int OngoingHundredths { get; set; }

    /// <summary>The name of its ongoing damage, which equipment may cancel.</summary>
    [Export]
    public string OngoingSource { get; set; } = "hazard";

    /// <summary>How many updates it holds a player it catches; zero when it does not hold.</summary>
    [Export]
    public int HoldUpdates { get; set; }

    private Countdown _holding = Countdown.Over;
    private bool _hasPlayer;
    private bool _mustLeave;

    /// <summary>The region of the room the hazard covers.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        if (_hasPlayer)
        {
            if (OngoingHundredths > 0)
                player.AddOngoingDamage(OngoingSource, OngoingHundredths);
            _holding = _holding.Stepped();
            if (_holding.IsRunning)
                return;
            _hasPlayer = false;
            _mustLeave = true;
            player.Release();
            return;
        }

        if (!Area.Overlaps(player.BodyRect))
        {
            _mustLeave = false;
            return;
        }

        if (OngoingHundredths > 0)
            player.AddOngoingDamage(OngoingSource, OngoingHundredths);
        if (HitDamage > 0)
            player.TakeHit(new Hit(HitDamage, HitSide.Neutral, HitKind.Hazard) { Source = this }, player.Hurtbox);
        if (HoldUpdates > 0 && !_mustLeave && player.State.InControl)
        {
            _hasPlayer = true;
            _holding = Countdown.Of(HoldUpdates);
            player.Hold();
        }
    }
}
