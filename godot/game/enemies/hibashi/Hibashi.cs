using System.Globalization;
using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Hibashi: a jet of flame that bursts up out of the lava, again and again, with a rest between. Only the flame
/// itself hurts, so the part of it that hurts grows as the jet climbs.
/// </summary>
/// <remarks>
/// Its timeline is "flame". Its cue "reach:&lt;rise&gt;:&lt;half height&gt;" puts the part that hurts that far above
/// where the jet starts and makes it that tall each way, and "done" ends the burst.
/// </remarks>
public partial class Hibashi : Enemy
{
    /// <summary>How many updates it rests between bursts.</summary>
    [Export]
    public int RestUpdates { get; set; } = 64;

    /// <summary>How many updates it waits before its first burst.</summary>
    [Export, DataValue("first_wait")]
    public int FirstWait { get; set; } = 1;

    /// <summary>Half the width of the part of the flame that hurts, in pixels.</summary>
    [Export, DataValue("half_width")]
    public int HalfWidth { get; set; } = 8;

    private enum Doing
    {
        Resting,
        Bursting,
        Spent,
    }

    private Doing _doing;
    private Countdown _rest;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _rest = Countdown.Of(FirstWait);
        Flame.Enabled = false;
        Visual?.SetShown(false);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "done")
        {
            _doing = Doing.Spent;
            Flame.Enabled = false;
            Visual?.SetShown(false);
            return;
        }

        string[] parts = cue.Split(':');
        if (parts.Length != 3 || parts[0] != "reach")
            return;
        int rise = int.Parse(parts[1], CultureInfo.InvariantCulture), halfHeight = int.Parse(parts[2], CultureInfo.InvariantCulture);
        Flame.Rect = new PixelRect(-HalfWidth, -rise - halfHeight, HalfWidth * 2, halfHeight * 2);
        Flame.Enabled = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Resting:
                _rest = _rest.Stepped();
                if (_rest.IsRunning)
                    return;
                _doing = Doing.Bursting;
                Play("flame", restart: true);
                Visual?.SetShown(true);
                return;

            case Doing.Bursting:
                return;

            case Doing.Spent:
                _rest = Countdown.Of(RestUpdates + 1);
                _doing = Doing.Resting;
                return;

            default:
                throw new InvalidOperationException($"Undefined Hibashi stage {_doing}.");
        }
    }

    /// <summary>
    /// The part of the flame that touches: Samus, whom it hurts, and her shots and the grapple's line, which it takes
    /// by its reactions. It is one box for all three, as the cartridge's unseen part of the flame is, there only while
    /// the flame erupts and shaped by each step of the eruption.
    /// </summary>
    private Collider Flame =>
        EntityNode.CollidersOf(this).FirstOrDefault(collider => collider.Role == ColliderRole.Hurtbox)
        ?? throw new InvalidOperationException($"The Hibashi \"{Name}\" has no hurtbox collider.");
}
