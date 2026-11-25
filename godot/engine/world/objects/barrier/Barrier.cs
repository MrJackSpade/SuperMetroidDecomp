using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A solid object with health that only certain damage wears down, or that opens when told to. It stands wherever
/// something must be got through and knows nothing about exits. Door caps, gates, shutters, breakable walls and the
/// shell around a held item are all barriers with different settings. It can be told to "open" and "close", and can
/// tell others when it opens.
/// </summary>
/// <remarks>
/// Its data may name the timelines that show it: "closed", "sealed" (while its condition holds it shut), "hurt"
/// (played once when a hit wears it down without opening it), "opening", "open" and "closing", and "sealing"
/// (played once as the room is entered while it is held shut, before "sealed"). One with an "opening" timeline goes
/// on blocking until that has played. Its data may also name its "open_sound", "close_sound", "hurt_sound" and
/// "sealed_sound", and its "refused_sound" for a hit of no kind that wears it down. One that is plain, having once been opened and remembered so, shows the timeline named with
/// "plain_" before each of those names where its data has one, and is opened by the kinds its data lists under
/// "plain_opened_by", or by any hit where it lists none: a red door's cap comes back as a blue one.
/// </remarks>
public partial class Barrier : RoomObject, IHittable, ITriggerTarget
{
    /// <summary>What an open barrier does.</summary>
    public enum ComesBackWhen
    {
        /// <summary>It stays open.</summary>
        Never,

        /// <summary>It closes once the player has gone through and is clear of it.</summary>
        AfterPassing,

        /// <summary>It is closed again when its room is next set up.</summary>
        NextSetup,

        /// <summary>It never opens: a hit that would open it only tells whom it tells, every time. A switch.</summary>
        AtOnce,
    }

    /// <summary>How a barrier that was opened and remembered is set up afterwards.</summary>
    public enum RememberedAsWhat
    {
        /// <summary>Already open.</summary>
        Open,

        /// <summary>Closed, but as a plain barrier any hit opens.</summary>
        Plain,
    }

    /// <summary>How much damage opens it.</summary>
    [Export, DataValue("health")]
    public int MaxHealth { get; set; } = 1;

    /// <summary>The amount each accepted hit kind takes off, by the kind's name. A kind not listed does nothing.</summary>
    [Export]
    public Godot.Collections.Dictionary<string, int> DamagedBy { get; set; } = [];

    /// <summary>The amount a hit of any kind not listed takes off; zero when only listed kinds damage it.</summary>
    [Export, DataValue("any_hit")]
    public int AnyHit { get; set; }

    /// <summary>A condition that holds the barrier shut while it is false; null when nothing does.</summary>
    [Export]
    public Condition? HeldShutBy { get; set; }

    /// <summary>What an open barrier does.</summary>
    [Export]
    public ComesBackWhen ComesBack { get; set; } = ComesBackWhen.NextSetup;

    /// <summary>How the barrier is set up once it has been opened and remembered.</summary>
    [Export]
    public RememberedAsWhat Remembered { get; set; }

    /// <summary>The things told when it opens.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> TellsWhenOpened { get; set; } = [];

    /// <summary>What they are told.</summary>
    [Export]
    public string TellsWhat { get; set; } = "open";

    /// <summary>
    /// How near the player must come, in pixels both across and down from where it stands, for it to open by itself;
    /// it closes again once they are farther. Zero for one that does not. One held shut stays shut however near they are.
    /// </summary>
    [Export]
    public int OpensWhenNear { get; set; }

    /// <summary>The sound of it opening, by name; empty for none.</summary>
    [Export, DataValue("open_sound")]
    public string OpenSound { get; set; } = "";

    /// <summary>The sound of it closing, by name; empty for none.</summary>
    [Export, DataValue("close_sound")]
    public string CloseSound { get; set; } = "";

    /// <summary>The sound of a hit wearing it down, by name; empty for none.</summary>
    [Export, DataValue("hurt_sound")]
    public string HurtSound { get; set; } = "";

    /// <summary>The sound of a hit that does nothing to it, being of no kind that wears it down, by name; empty for none.</summary>
    [Export, DataValue("refused_sound")]
    public string RefusedSound { get; set; } = "";

    /// <summary>The sound asked for when it is hit while it is held shut, by name; empty for none.</summary>
    [Export, DataValue("sealed_sound")]
    public string SealedSound { get; set; } = "";

    private int _health;
    private bool _plain;
    private bool _playerWasInside;
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;
    private string _showingName = "";
    private bool _opening;

    /// <summary>The kinds of hit that open it once it is plain; none listed when any hit does.</summary>
    private readonly HashSet<HitKind> _plainOpenedBy = [];

    /// <summary>Whether the barrier is open.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Whether the barrier is open and has finished showing itself opening.</summary>
    public bool HasOpened => IsOpen && !_opening;

    /// <summary>Whether the barrier's solid part, where it stands, lies over any of <paramref name="area"/>, blocking just now or not.</summary>
    public bool Covers(PixelRect area) => EntityNode.ColliderOf(this, ColliderRole.Solid) is { } solid && solid.RoomRect.Overlaps(area);

    /// <summary>Whether the barrier is held shut by its condition right now.</summary>
    public bool IsSealed => !IsOpen && HeldShutBy is not null && !HeldShutBy.IsTrue(Room);

    /// <summary>How much more damage opens it.</summary>
    public int HealthLeft => _health;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
        _plainOpenedBy.Clear();
        if (data.Has("plain_opened_by"))
            _plainOpenedBy.UnionWith(data.List("plain_opened_by").Select(kind => new HitKind(kind.AsText())));
        if (!data.Has("damaged_by"))
            return;
        DamagedBy = [];
        foreach ((string kind, DataNode amount) in data.Get("damaged_by").Entries())
            DamagedBy[kind] = amount.AsInt();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _health = MaxHealth;
        IsOpen = false;
        _plain = false;
        (_showing, _showingName, _opening) = (null, "", false);
        SetSolid(true);
        if (IsRemembered)
        {
            if (Remembered == RememberedAsWhat.Open)
            {
                IsOpen = true;
                SetSolid(false);
                Room.MarkGone(Name);
            }
            else
            {
                _plain = true;
                _health = 1;
            }
        }

        // One that is held shut as the room is entered may show itself shutting first.
        Show(IsOpen ? "open" : IsSealed ? _timelines.Has("sealing") ? "sealing" : "sealed" : "closed");
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Enemies && !IsOpen;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsOpen)
            return HitResult.Ignored;
        if (IsSealed)
        {
            Sound(SealedSound);
            return HitResult.Absorbed;
        }

        int damage = !_plain ? DamageFrom(hit) : _plainOpenedBy.Count == 0 || hit.CarriesAny(_plainOpenedBy) ? 1 : 0;
        if (damage <= 0)
        {
            Sound(RefusedSound);
            return HitResult.Absorbed;
        }
        _health -= damage;
        if (_health > 0)
        {
            Sound(HurtSound);
            Show("hurt");
            return HitResult.Hurt;
        }

        Open();
        return HitResult.Destroyed;
    }

    /// <summary>Opens the barrier: it stops blocking, is recorded as gone, and is remembered if it is one that is.</summary>
    public void Open()
    {
        if (IsOpen)
            return;
        if (ComesBack == ComesBackWhen.AtOnce)
        {
            // A switch only tells, and is ready to be hit again.
            _health = MaxHealth;
            Sound(OpenSound);
            Tell();
            return;
        }

        IsOpen = true;
        _playerWasInside = false;
        Room.MarkGone(Name);
        Remember();
        Sound(OpenSound);
        // One that shows itself opening goes on blocking until it has.
        _opening = _timelines.Has("opening");
        if (_opening)
            Show("opening");
        else
        {
            SetSolid(false);
            Show("open");
        }

        Tell();
    }

    /// <summary>Tells everything it tells when it opens.</summary>
    private void Tell()
    {
        foreach (NodePath path in TellsWhenOpened)
        {
            (GetNode(path) as ITriggerTarget
                ?? throw new InvalidOperationException($"The barrier \"{Name}\" is to tell \"{path}\" that it has opened, but that is nothing that can be told."))
                .Triggered(TellsWhat);
        }
    }

    /// <summary>Closes the barrier at full strength.</summary>
    public void Close()
    {
        if (!IsOpen)
            return;
        IsOpen = false;
        _opening = false;
        _health = _plain ? 1 : MaxHealth;
        SetSolid(true);
        Sound(CloseSound);
        Show(_timelines.Has("closing") ? "closing" : "closed");
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        StepLook();
        if (OpensWhenNear > 0)
        {
            StepNearness();
            return;
        }

        if (!IsOpen || ComesBack != ComesBackWhen.AfterPassing)
            return;
        Collider? solid = EntityNode.ColliderOf(this, ColliderRole.Solid);
        if (solid is null)
            return;
        bool inside = solid.RoomRect.OverlapsOrTouches(Room.Player.BodyRect);
        if (_playerWasInside && !inside)
            Close();
        _playerWasInside = inside;
    }

    /// <summary>Opens it while the player is near and closes it when they are not.</summary>
    private void StepNearness()
    {
        PixelOffset apart = Room.Player.Position - Position;
        bool near = apart.X.Abs().Whole < OpensWhenNear && apart.Y.Abs().Whole < OpensWhenNear;
        if (IsOpen)
        {
            if (!near && !_opening)
                Close();
        }
        else if (near && !IsSealed && _showingName is "closed" or "sealed")
        {
            // It opens for now, not for good: nothing is remembered and nothing is told.
            IsOpen = true;
            Sound(OpenSound);
            _opening = _timelines.Has("opening");
            if (_opening)
                Show("opening");
            else
            {
                SetSolid(false);
                Show("open");
            }
        }
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        switch (what)
        {
            case "open":
                Open();
                break;
            case "toggle":
                // Told while it is still opening or closing, it takes no notice.
                if (_showingName is "opening" or "closing" && _showing is { IsOver: false })
                    break;
                if (IsOpen)
                    Close();
                else
                    Open();
                break;
            case "close":
                Close();
                break;
            default:
                throw new ArgumentException($"A barrier can be told to \"open\", \"close\" or \"toggle\", not \"{what}\".", nameof(what));
        }
    }

    /// <summary>How much a hit takes off: the most that any kind it carries takes off, or what any hit does.</summary>
    private int DamageFrom(Hit hit)
    {
        int most = 0;
        bool listed = false;
        foreach (HitKind kind in hit.Kinds)
        {
            if (!DamagedBy.TryGetValue(kind.Name, out int amount))
                continue;
            listed = true;
            most = Math.Max(most, amount);
        }

        return listed ? most : AnyHit;
    }

    /// <summary>
    /// Moves what shows it on by one update, and goes on to what follows a look that has played out. A look gives
    /// way on the update after its last, as a frame of it gives way to the next: one that opens stops blocking then,
    /// and what follows shows from that update.
    /// </summary>
    private void StepLook()
    {
        if (_showing is null)
            return;

        if (_showing.IsOver)
        {
            switch (_showingName)
            {
                case "opening":
                    _opening = false;
                    SetSolid(false);
                    Show("open");
                    break;
                case "closing":
                case "hurt":
                case "sealing":
                    Show(IsSealed ? "sealed" : "closed");
                    break;
            }
        }
        else
        {
            // A look may say, step by step, whether the barrier stands in the way meanwhile.
            foreach (string cue in _showing.Step())
            {
                if (cue is "passable" or "blocking" && EntityNode.ColliderOf(this, ColliderRole.Solid) is { } solid)
                    solid.Enabled = cue == "blocking";
            }

            if (_showingName is "closed" or "sealed")
            {
                // Shut and held shut may look different, and it changes between them as its condition does.
                string wanted = IsSealed && _timelines.Has("sealed") ? "sealed" : "closed";
                if (wanted != _showingName)
                    Show(wanted);
            }
        }

        if (_showing is not null)
        {
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.Show(_showing.Shown);
        }
    }

    /// <summary>Shows the timeline of that name, if its data has one; a look it has none for falls back to "closed" or shows nothing.</summary>
    private void Show(string name)
    {
        // A plain one has its own timeline for a look, where its data gives one.
        string Named(string look) => _plain && _timelines.Has("plain_" + look) ? "plain_" + look : look;
        string shown = _timelines.Has(Named(name)) ? name : name is "sealed" or "hurt" && _timelines.Has(Named("closed")) ? "closed" : "";
        if (name == "open" && shown.Length == 0)
        {
            // With no look for being open it simply is not there.
            (_showing, _showingName) = (null, name);
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.Show("");
            return;
        }

        if (shown.Length == 0)
        {
            (_showing, _showingName) = (null, name);
            return;
        }

        (_showing, _showingName) = (new TimelinePlayer(_timelines.Get(Named(shown))), shown);
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }

    /// <summary>Turns the barrier's solid and its hurtboxes on or off together.</summary>
    private void SetSolid(bool solid)
    {
        foreach (Collider collider in EntityNode.CollidersOf(this))
            collider.Enabled = solid;
    }
}
