using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>A fixture that hits the player on contact and cannot be destroyed.</summary>
/// <remarks>
/// Its data may name the timeline that shows it, "rest". A hazard whose reach changes as it is shown says so in that
/// timeline: the cue "area:left:top:width:height" makes that region, measured from where it stands, the one it
/// covers, and "safe" makes it cover nothing, until the next of either.
/// </remarks>
public partial class Hazard : RoomObject
{
    /// <summary>The hazard's width and height, in pixels; its top-left is where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 16);

    /// <summary>The damage of the hit it delivers on contact; zero for none.</summary>
    [Export, DataValue("hit_damage")]
    public int HitDamage { get; set; }

    /// <summary>
    /// Whether its contact is a hit even when the hit does no damage: the player is knocked back, and safe for a
    /// moment, all the same.
    /// </summary>
    [Export, DataValue("hits_without_damage")]
    public bool HitsWithoutDamage { get; set; }

    /// <summary>
    /// The most updates it stays out of sight, covering nothing, once the room is entered: it draws a number from 1
    /// to this for the wait. Zero for one that is there at once.
    /// </summary>
    [Export]
    public int StartsWithinUpdates { get; set; }

    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;
    private PixelRect? _reach;
    private bool _safe;
    private int _startsIn;

    /// <summary>The region of the room the hazard covers; empty while it covers nothing.</summary>
    public PixelRect Area => _safe
        ? new PixelRect(Position.X.Whole, Position.Y.Whole, 0, 0)
        : _reach is { } reach
            ? reach.MovedBy(Position.X.Whole, Position.Y.Whole)
            : new PixelRect(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_reach, _safe, _startsIn) = (null, false, 0);
        if (StartsWithinUpdates > 0)
        {
            // Until its wait is over it shows nothing and covers nothing.
            _startsIn = Room.Session.Random.Draw().Below(StartsWithinUpdates) + 1;
            _safe = true;
            _showing = null;
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.Show("");
            return;
        }

        ShowAtRest();
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        if (_startsIn > 0)
        {
            if (--_startsIn > 0)
                return;
            _safe = false;
            ShowAtRest();
        }

        StepLook();
        if (!Area.Overlaps(player.BodyRect))
            return;
        // Its hit is a touch, as an enemy's is: the hurt it gives lasts as a touch's does, not as the terrain's.
        if (HitDamage > 0 || HitsWithoutDamage)
            player.TakeHit(new Hit(HitDamage, HitSide.Neutral, HitKind.Contact) { Source = this }, player.Hurtbox);
    }

    /// <summary>Moves what shows it on by one update and acts on its cues.</summary>
    private void StepLook()
    {
        if (_showing is null)
            return;
        foreach (string cue in _showing.Step())
        {
            if (cue == "safe")
                _safe = true;
            else if (cue.StartsWith("area:", StringComparison.Ordinal))
                (_reach, _safe) = (ReachOf(cue), false);
            else
                throw new InvalidOperationException($"The hazard \"{Name}\" shows a timeline with the cue \"{cue}\", which it does not know.");
        }

        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
    }

    /// <summary>Reads the region an "area:" cue names.</summary>
    private PixelRect ReachOf(string cue)
    {
        string[] parts = cue.Split(':');
        if (parts.Length != 5 || !int.TryParse(parts[1], out int left) || !int.TryParse(parts[2], out int top)
            || !int.TryParse(parts[3], out int width) || !int.TryParse(parts[4], out int height) || width < 0 || height < 0)
            throw new InvalidOperationException($"The hazard \"{Name}\" shows a timeline with the cue \"{cue}\", which is not \"area:left:top:width:height\".");
        return new PixelRect(left, top, width, height);
    }

    /// <summary>Shows its timeline at rest, if its data has one.</summary>
    private void ShowAtRest()
    {
        _showing = _timelines.Has("rest") ? new TimelinePlayer(_timelines.Get("rest")) : null;
        if (_showing is null)
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
    }
}
