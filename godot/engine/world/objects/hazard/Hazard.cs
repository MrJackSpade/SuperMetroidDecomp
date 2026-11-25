using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A fixture that hurts or grabs the player on contact and cannot be destroyed. It can deliver a hit, do ongoing
/// damage, hold the player for a while, or any of these together.
/// </summary>
/// <remarks>
/// Its data may name the timelines that show it: "rest", "hold" while it holds the player, and "release" after it
/// lets go, during which it catches nothing. One with a "hold" timeline holds for as long as that lasts; its cue
/// "bite" takes the bite's energy from the player, and "sound:" with a name asks for that sound. A hazard whose reach
/// changes as it is shown says so in its timelines: the cue "area:left:top:width:height" makes that region, measured
/// from where it stands, the one it covers, and "safe" makes it cover nothing, until the next of either.
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

    /// <summary>The ongoing damage it does each update of contact, in hundredths of a point; zero for none.</summary>
    [Export, DataValue("ongoing_hundredths")]
    public int OngoingHundredths { get; set; }

    /// <summary>The name of its ongoing damage, which equipment may cancel.</summary>
    [Export]
    public string OngoingSource { get; set; } = "hazard";

    /// <summary>How many updates it holds a player it catches; zero when it does not hold, or holds for as long as its "hold" timeline.</summary>
    [Export, DataValue("hold_updates")]
    public int HoldUpdates { get; set; }

    /// <summary>Whether it keeps the player at one place while it holds them.</summary>
    [Export]
    public bool HoldsInPlace { get; set; }

    /// <summary>Where it keeps them, in pixels from where it stands.</summary>
    [Export]
    public Vector2I HoldsAt { get; set; }

    /// <summary>The energy each "bite" of its hold takes.</summary>
    [Export, DataValue("bite_damage")]
    public int BiteDamage { get; set; }

    /// <summary>How many updates the player cannot be hurt after it lets go.</summary>
    [Export, DataValue("release_invincible_updates")]
    public int ReleaseInvincibleUpdates { get; set; }

    /// <summary>
    /// The most updates it stays out of sight, covering nothing, once the room is entered: it draws a number from 1
    /// to this for the wait. Zero for one that is there at once.
    /// </summary>
    [Export]
    public int StartsWithinUpdates { get; set; }

    private Countdown _holding = Countdown.Over;
    private bool _hasPlayer;
    private bool _mustLeave;
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;
    private string _showingName = "";
    private PixelRect? _reach;
    private bool _safe;
    private int _startsIn;

    /// <summary>The region of the room the hazard covers; empty while it covers nothing.</summary>
    public PixelRect Area => _safe
        ? new PixelRect(Position.X.Whole, Position.Y.Whole, 0, 0)
        : _reach is { } reach
            ? reach.MovedBy(Position.X.Whole, Position.Y.Whole)
            : new PixelRect(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <summary>Whether it is holding the player.</summary>
    public bool IsHolding => _hasPlayer;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_hasPlayer, _mustLeave, _holding) = (false, false, Countdown.Over);
        (_reach, _safe, _startsIn) = (null, false, 0);
        if (StartsWithinUpdates > 0)
        {
            // Until its wait is over it shows nothing and covers nothing.
            _startsIn = Room.Session.Random.Draw().Below(StartsWithinUpdates) + 1;
            _safe = true;
            (_showing, _showingName) = (null, "");
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.Show("");
            return;
        }

        Show("rest");
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
            Show("rest");
        }

        bool lookOver = StepLook(player);
        if (_hasPlayer)
        {
            if (OngoingHundredths > 0)
                player.AddOngoingDamage(OngoingSource, OngoingHundredths);
            if (HoldsInPlace)
                player.Position = Position + PixelOffset.FromWhole(HoldsAt.X, HoldsAt.Y);
            bool done;
            if (_showingName == "hold")
            {
                done = lookOver;
            }
            else
            {
                _holding = _holding.Stepped();
                done = !_holding.IsRunning;
            }

            if (!done)
                return;
            _hasPlayer = false;
            _mustLeave = true;
            if (player.State is HeldState)
                player.Release();
            if (ReleaseInvincibleUpdates > 0)
                player.BeInvincibleFor(ReleaseInvincibleUpdates);
            Show(_timelines.Has("release") ? "release" : "rest");
            return;
        }

        if (_showingName == "release")
        {
            // It catches nothing until it has settled.
            if (lookOver)
                Show("rest");
            return;
        }

        if (!Area.Overlaps(player.BodyRect))
        {
            _mustLeave = false;
            return;
        }

        if (OngoingHundredths > 0)
            player.AddOngoingDamage(OngoingSource, OngoingHundredths);
        if (HitDamage > 0 || HitsWithoutDamage)
            player.TakeHit(new Hit(HitDamage, HitSide.Neutral, HitKind.Hazard) { Source = this }, player.Hurtbox);
        bool holds = HoldUpdates > 0 || _timelines.Has("hold");
        if (holds && !_mustLeave && player.State.InControl)
        {
            _hasPlayer = true;
            _holding = Countdown.Of(HoldUpdates);
            player.Hold();
            Show("hold");
        }
    }

    /// <summary>Moves what shows it on by one update and acts on its cues. Returns whether a look that ends has ended.</summary>
    private bool StepLook(Player player)
    {
        if (_showing is null)
            return false;
        foreach (string cue in _showing.Step())
        {
            if (cue == "bite")
                player.Sap(new Hit(BiteDamage, HitSide.Neutral, HitKind.Hazard) { Source = this });
            else if (cue.StartsWith("sound:", StringComparison.Ordinal))
                Room.Session.Sounds.Request(cue["sound:".Length..], channel: "objects");
            else if (cue == "safe")
                _safe = true;
            else if (cue.StartsWith("area:", StringComparison.Ordinal))
                (_reach, _safe) = (ReachOf(cue), false);
            else
                throw new InvalidOperationException($"The hazard \"{Name}\" shows a timeline with the cue \"{cue}\", which it does not know.");
        }

        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
        return _showing.IsOver;
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

    /// <summary>Shows the timeline of that name, if its data has one.</summary>
    private void Show(string name)
    {
        _showingName = _timelines.Has(name) ? name : "";
        _showing = _showingName.Length > 0 ? new TimelinePlayer(_timelines.Get(name)) : null;
        if (_showing is null)
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
    }
}
