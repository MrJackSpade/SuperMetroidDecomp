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
/// (played once when a hit wears it down without opening it), "opening", "open" and "closing". One with an
/// "opening" timeline goes on blocking until that has played. Its data may also name its "open_sound",
/// "close_sound", "hurt_sound" and "sealed_sound".
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

    /// <summary>The sound of it opening, by name; empty for none.</summary>
    [Export, DataValue("open_sound")]
    public string OpenSound { get; set; } = "";

    /// <summary>The sound of it closing, by name; empty for none.</summary>
    [Export, DataValue("close_sound")]
    public string CloseSound { get; set; } = "";

    /// <summary>The sound of a hit wearing it down, by name; empty for none.</summary>
    [Export, DataValue("hurt_sound")]
    public string HurtSound { get; set; } = "";

    /// <summary>The sound of a hit on it while it is held shut, by name; empty for none.</summary>
    [Export, DataValue("sealed_sound")]
    public string SealedSound { get; set; } = "";

    private int _health;
    private bool _plain;
    private bool _playerWasInside;
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;
    private string _showingName = "";
    private bool _opening;

    /// <summary>Whether the barrier is open.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Whether the barrier is held shut by its condition right now.</summary>
    public bool IsSealed => !IsOpen && HeldShutBy is not null && !HeldShutBy.IsTrue(Room);

    /// <summary>How much more damage opens it.</summary>
    public int HealthLeft => _health;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
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

        Show(IsOpen ? "open" : IsSealed ? "sealed" : "closed");
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

        int damage = _plain ? 1 : DamageFrom(hit);
        if (damage <= 0)
            return HitResult.Absorbed;
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

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        switch (what)
        {
            case "open":
                Open();
                break;
            case "close":
                Close();
                break;
            default:
                throw new ArgumentException($"A barrier can be told to \"open\" or \"close\", not \"{what}\".", nameof(what));
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

    /// <summary>Moves what shows it on by one update, and goes on to what follows a look that has played out.</summary>
    private void StepLook()
    {
        if (_showing is null)
            return;
        _showing.Step();
        if (_showingName is "closed" or "sealed")
        {
            // Shut and held shut may look different, and it changes between them as its condition does.
            string wanted = IsSealed && _timelines.Has("sealed") ? "sealed" : "closed";
            if (wanted != _showingName)
                Show(wanted);
        }
        else if (_showing.IsOver)
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
                    Show(IsSealed ? "sealed" : "closed");
                    break;
            }
        }

        if (_showing is not null)
        {
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.Show(_showing.Current.Picture);
        }
    }

    /// <summary>Shows the timeline of that name, if its data has one; a look it has none for falls back to "closed" or shows nothing.</summary>
    private void Show(string name)
    {
        string shown = _timelines.Has(name) ? name : name is "sealed" or "hurt" && _timelines.Has("closed") ? "closed" : "";
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

        (_showing, _showingName) = (new TimelinePlayer(_timelines.Get(shown)), shown);
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Current.Picture);
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
