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
/// (played once as the room is entered while it is held shut, before "sealed"). One that a condition holds open
/// (<see cref="HeldOpenBy"/>) shows "held_open" over and over while it is held, and "let_go" once it is not, which
/// ends in its being shut. One with an "opening" timeline goes
/// on blocking until that has played. A look's steps may say how the barrier stands in the way from that step on:
/// "passable" and "blocking", and "solid_height:" with how many pixels down from its top its solid part reaches, for
/// one that comes down or goes up bit by bit; what the first step of "open" or "closed" says holds from the moment
/// that look is taken up. A step of "let_go" says where the barrier is shut ("shut"): until then it stands open
/// as it did while held. One the player has just come in through (<see cref="ClosesBehindEntry"/>) stands open as
/// the room is set up and plays "closing_behind", a step of which says where it is shut with no sound
/// ("shut_unheard") and another where its closing is heard ("close_sound"). Its data may also name its "open_sound", "close_sound", "hurt_sound" and
/// "sealed_sound", and its "refused_sound" for a hit of no kind that wears it down. One that opens when the player is
/// near (<see cref="OpensWhenNear"/>) rests in its "closed", "open" and "sealed" looks, none of which loops: it looks
/// how near the player is, and whether it is held shut, each time one of them has played through. One that is plain, having once been opened and remembered so, shows the timeline named with
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

    /// <summary>
    /// A condition that holds the barrier open while it is true, from the room's setting up on: it stands open,
    /// nothing remembered and no one told, showing its "held_open" look over and over. The condition is looked at
    /// each time that look has played through: no longer held, the barrier plays its "let_go" look, standing open
    /// still until a step of that shuts it. Null when nothing holds it open. A door's cap that does not shut behind
    /// the player until something has happened is held so.
    /// </summary>
    [Export]
    public Condition? HeldOpenBy { get; set; }

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
    /// The entry point of its room whose use has it stand open as the room is set up and close behind the player:
    /// arriving there they have come through it, and it plays its "closing_behind" look, open and letting through
    /// until the step of that look that shuts it. One that is open anyway stays open. Empty for none.
    /// </summary>
    [Export]
    public string ClosesBehindEntry { get; set; } = "";

    /// <summary>
    /// How near the player must come, in whole pixels both across and down from where it stands, for it to open by
    /// itself; it closes again once they are farther. Zero for one that does not. One held shut stays shut however
    /// near they are. It does not look on every update: it looks each time the look it rests in, shut, held shut or
    /// open, has played through, and the look that follows is played from that very update. As its room is set up it
    /// stands open and unseen; looking for the first time it stays so, or is shut at once with no closing shown, or,
    /// held shut, shows itself shutting.
    /// </summary>
    [Export]
    public int OpensWhenNear { get; set; }

    /// <summary>
    /// Whether, resting shut, it opens for good as the middle of the player comes to lie inside its solid part, as
    /// when it shut on them: a gate never keeps the player in its column ("opens_around_player").
    /// </summary>
    [Export, DataValue("opens_around_player")]
    public bool OpensAroundPlayer { get; set; }

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

    /// <summary>Whether one that opens when the player is near has yet to look how near they are since its room was set up.</summary>
    private bool _appearing;

    /// <summary>Whether its solid part, done blocking, stands out the update it let go on (<see cref="LetThrough"/>).</summary>
    private bool _solidStandsOut;

    /// <summary>The kinds of hit that open it once it is plain; none listed when any hit does.</summary>
    private readonly HashSet<HitKind> _plainOpenedBy = [];

    /// <summary>Whether the barrier is open.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Whether the barrier is open and has finished showing itself opening.</summary>
    public bool HasOpened => IsOpen && !_opening;

    /// <summary>Whether the barrier's solid part, where it stands, lies over any of <paramref name="area"/>, blocking just now or not.</summary>
    public bool Covers(PixelRect area) => EntityNode.ColliderOf(this, ColliderRole.Solid) is { } solid && solid.RoomRect.Overlaps(area);

    /// <summary>Whether the barrier is held shut by its condition right now.</summary>
    public bool IsSealed => !IsOpen && ConditionHoldsShut;

    /// <summary>Whether its condition would hold it shut right now, open or not.</summary>
    private bool ConditionHoldsShut => HeldShutBy is not null && !HeldShutBy.IsTrue(Room);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
        _plainOpenedBy.Clear();
        if (data.Has("plain_opened_by"))
            _plainOpenedBy.UnionWith(data.List("plain_opened_by").Select(kind => new HitKind(kind.AsText())));
        _reactions = data.Has("reactions") ? Reactions.Read(data.List("reactions")) : Reactions.None;
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
        (_showing, _showingName, _opening, _appearing) = (null, "", false, false);
        _solidWhole = EntityNode.ColliderOf(this, ColliderRole.Solid)?.Rect;
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

        // One its condition holds open stands open for now: it is not gone, and nothing is remembered or told.
        if (!IsOpen && HeldOpenBy is not null && HeldOpenBy.IsTrue(Room))
        {
            if (!_timelines.Has("held_open") || !_timelines.Has("let_go"))
                throw new InvalidOperationException($"The barrier \"{Name}\" is held open by a condition, and its data has no \"held_open\" and \"let_go\" looks to show it by.");
            IsOpen = true;
            SetSolid(false);
            Show("held_open");
            return;
        }

        // One the player has just come in through stands open, and closes behind them as its look says.
        if (!IsOpen && ClosesBehindEntry.Length > 0 && Room.Entry?.EntryPoint == ClosesBehindEntry)
        {
            if (!_timelines.Has("closing_behind"))
                throw new InvalidOperationException($"The barrier \"{Name}\" closes behind whoever comes in at \"{ClosesBehindEntry}\", and its data has no \"closing_behind\" look to show it by.");
            IsOpen = true;
            SetSolid(false);
            Show("closing_behind");
            return;
        }

        if (OpensWhenNear > 0)
        {
            foreach (string resting in (string[])["closed", "open", "sealed"])
            {
                if (resting == "sealed" && !_timelines.Has(resting))
                    continue;
                if (!_timelines.Has(resting) || _timelines.Get(resting).LoopsTo is not null)
                    throw new InvalidOperationException($"The barrier \"{Name}\" opens when the player is near, and looks about it each time its \"{resting}\" look has played through: its data must give that look, and it must not loop.");
            }

            // It stands open and unseen until it first looks about it.
            if (!IsOpen)
            {
                (IsOpen, _appearing) = (true, true);
                SetSolid(false);
                Show("open");
                return;
            }
        }

        // One that is held shut as the room is entered may show itself shutting first.
        Show(IsOpen ? "open" : IsSealed ? _timelines.Has("sealing") ? "sealing" : "sealed" : "closed");
    }

    /// <summary>
    /// What its data says of the hits that take no notice of it or that it swallows ("reactions", as an enemy's are
    /// written; <see cref="Reaction.Passes"/>, <see cref="Reaction.Swallows"/>). A barrier with none refuses or is
    /// worn by every hit that lands on it.
    /// </summary>
    private Reactions _reactions = Reactions.None;

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Enemies && !IsOpen;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsOpen)
            return HitResult.Ignored;

        // A hit its data's reactions let pass, or swallow, never comes to open it or be refused.
        Reaction reaction = _reactions.To(hit);
        if (reaction.Passes)
            return HitResult.Ignored;
        if (reaction.Swallows)
            return HitResult.Dud;
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
        Shut();
        Show(_timelines.Has("closing") ? "closing" : "closed");
    }

    /// <summary>Shuts it where it stands, at full strength, whatever it goes on showing; its closing is heard unless <paramref name="heard"/> says not.</summary>
    private void Shut(bool heard = true)
    {
        IsOpen = false;
        _opening = false;
        _health = _plain ? 1 : MaxHealth;
        SetSolid(true);
        if (heard)
            Sound(CloseSound);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        // A solid part left standing out the update it let go on is taken away as the next turn begins.
        if (_solidStandsOut)
            SetSolid(false);
        StepLook();

        if (OpensAroundPlayer && !IsOpen && _showingName == "closed" && EntityNode.ColliderOf(this, ColliderRole.Solid) is { } around
            && around.RoomRect.Contains(Room.Player.Position.X.Whole, Room.Player.Position.Y.Whole))
        {
            Open();
        }

        // One that opens when the player is near closes by looking how near they are, as its looks play through.
        if (OpensWhenNear > 0)
            return;

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

    /// <summary>Whether the player is nearer than <see cref="OpensWhenNear"/> both across and down, by the whole pixels each stands on.</summary>
    private bool PlayerIsNear =>
        Math.Abs(Room.Player.Position.X.Whole - Position.X.Whole) < OpensWhenNear &&
        Math.Abs(Room.Player.Position.Y.Whole - Position.Y.Whole) < OpensWhenNear;

    /// <summary>
    /// For one that opens when the player is near: goes on from a look that has played out, the look that follows
    /// played from this very update. Coming out of a look it rests in, it looks about it: shut, it opens if the
    /// player is near and nothing holds it shut; open, it closes if they are not near, or if it is held shut.
    /// </summary>
    private void FollowNearLook()
    {
        switch (_showingName)
        {
            case "closed" when PlayerIsNear && !IsSealed:
                // It opens for now, not for good: nothing is remembered and nothing is told.
                IsOpen = true;
                Sound(OpenSound);
                _opening = _timelines.Has("opening");
                if (_opening)
                    Take("opening");
                else
                {
                    LetThrough();
                    Take("open");
                }

                break;

            case "opening":
                _opening = false;
                LetThrough();
                Take("open");
                break;

            case "open":
                // Looking for the first time since its room was set up, it is shut at once; after that it shows
                // itself closing. Held shut, it shuts however near the player is, showing that where it can.
                bool first = _appearing;
                _appearing = false;
                if (ConditionHoldsShut)
                {
                    Shut();
                    Take(_timelines.Has("sealing") ? "sealing" : "sealed");
                }
                else if (PlayerIsNear)
                    Take("open");
                else
                {
                    Shut();
                    Take(!first && _timelines.Has("closing") ? "closing" : "closed");
                }

                break;

            default:
                Take(IsSealed ? "sealed" : "closed");
                break;
        }
    }

    /// <summary>
    /// Stops standing in the way, having opened. One that takes its turns with the enemies was counted, as the update
    /// opened, among what the player can run into, as the cartridge lists its enemies once a frame before any of them
    /// takes its turn: its solid part stands out the update, stopping the player once more, and is taken away as its
    /// next turn begins. Nothing else of it is met from now on.
    /// </summary>
    private void LetThrough()
    {
        if (!ActsWithEnemies)
        {
            SetSolid(false);
            return;
        }

        foreach (Collider collider in EntityNode.CollidersOf(this))
        {
            if (collider.Role != ColliderRole.Solid)
                collider.Enabled = false;
        }

        _solidStandsOut = true;
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

        if (_showing.IsOver && OpensWhenNear > 0 && _showingName is "closed" or "opening" or "open" or "closing" or "sealing" or "sealed")
        {
            FollowNearLook();
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
                case "closing_behind":
                case "hurt":
                case "sealing":
                case "let_go":
                    Show(IsSealed ? "sealed" : "closed");
                    break;
                case "held_open":
                    // Its hold is looked at each time the look has played through. Held still, the look plays again;
                    // let go, it plays the look that ends in its being shut. Either is played from this very update.
                    Take(HeldOpenBy is { } hold && hold.IsTrue(Room) ? "held_open" : "let_go");
                    break;
            }
        }
        else
        {
            // A look may say, step by step, how the barrier stands in the way meanwhile.
            StandAs(_showing.Step());

            if (OpensWhenNear == 0 && _showingName is "closed" or "sealed")
            {
                // Shut and held shut may look different, and it changes between them as its condition does. One
                // that opens when the player is near looks at its condition as its looks play through.
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

    /// <summary>Takes up the look of that name and plays its first update on this one.</summary>
    private void Take(string name)
    {
        Show(name);
        if (_showing is not null)
            StandAs(_showing.Step());
    }

    /// <summary>Shows the timeline of that name, if its data has one; a look it has none for falls back to "closed" or shows nothing.</summary>
    private void Show(string name)
    {
        // A plain one has its own timeline for a look, where its data gives one.
        string Named(string look) => _plain && _timelines.Has("plain_" + look) ? "plain_" + look : look;
        string shown =_timelines.Has(Named(name)) ? name : name is "sealed" or "hurt" && _timelines.Has(Named("closed")) ? "closed" : "";
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

        // A look it rests in says how it stands from the moment it is taken up; one it moves through, as its steps come.
        if (shown is "open" or "closed")
            StandAs(_showing.Current.Cues);
    }

    /// <summary>
    /// Stands in the way as a look's step says: blocking or not; with its solid part so many pixels tall from its top
    /// ("solid_height:"); with only so many pixels at each end of its length solid and its middle letting through
    /// ("solid_ends:"), or whole again ("solid_whole").
    /// </summary>
    private void StandAs(IEnumerable<string> cues)
    {
        const string height = "solid_height:", ends = "solid_ends:";
        foreach (string cue in cues)
        {
            // One let go is shut where its look says, and stands whole until a later cue says otherwise.
            if (cue is "shut" or "shut_unheard")
            {
                Shut(heard: cue == "shut");
                continue;
            }

            if (cue == "close_sound")
            {
                Sound(CloseSound);
                continue;
            }

            if (EntityNode.ColliderOf(this, ColliderRole.Solid) is not { } solid)
                continue;
            if (cue is "passable" or "blocking")
                solid.Enabled = cue == "blocking";
            else if (cue.StartsWith(height, StringComparison.Ordinal))
                solid.Size = new Vector2I(solid.Size.X, int.Parse(cue[height.Length..], System.Globalization.CultureInfo.InvariantCulture));
            else if (cue.StartsWith(ends, StringComparison.Ordinal))
                ShapeSolidEnds(int.Parse(cue[ends.Length..], System.Globalization.CultureInfo.InvariantCulture));
            else if (cue == "solid_whole")
                ShapeSolidEnds(0);
        }
    }

    /// <summary>Its solid part as its data sizes it, before any step of a look reshapes it.</summary>
    private PixelRect? _solidWhole;

    /// <summary>The second of its solid colliders, which is the far end of a solid part that is only its two ends.</summary>
    private Collider? FarSolid => EntityNode.CollidersOf(this).Where(collider => collider.Role == ColliderRole.Solid).Skip(1).FirstOrDefault();

    /// <summary>
    /// Makes its solid part only <paramref name="each"/> pixels at each end of its length, down it when it is taller
    /// than wide and along it otherwise, the middle letting through; or whole again for nought. The far end is a
    /// solid collider of its own, which a barrier that stands so must have.
    /// </summary>
    private void ShapeSolidEnds(int each)
    {
        if (EntityNode.ColliderOf(this, ColliderRole.Solid) is not { } near || _solidWhole is not { } whole)
            return;
        Collider? far = FarSolid;
        if (each <= 0)
        {
            near.Rect = whole;
            if (far is not null)
                far.Enabled = false;
            return;
        }

        if (far is null)
            throw new InvalidOperationException($"The barrier \"{Name}\" is to be solid at its two ends only and has one solid collider.");
        bool tall = whole.Height >= whole.Width;
        near.Rect = tall ? new PixelRect(whole.Left, whole.Top, whole.Width, each) : new PixelRect(whole.Left, whole.Top, each, whole.Height);
        far.Rect = tall
            ? new PixelRect(whole.Left, whole.Bottom - each, whole.Width, each)
            : new PixelRect(whole.Right - each, whole.Top, each, whole.Height);
        far.Enabled = near.Enabled;
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }

    /// <summary>Turns the barrier's solid and its hurtboxes on or off together.</summary>
    private void SetSolid(bool solid)
    {
        _solidStandsOut = false;
        foreach (Collider collider in EntityNode.CollidersOf(this))
            collider.Enabled = solid;

        // One with a far end stands whole again, until a step of a look says otherwise.
        if (FarSolid is not null)
            ShapeSolidEnds(0);
    }
}
