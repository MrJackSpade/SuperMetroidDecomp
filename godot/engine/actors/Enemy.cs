using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// What an enemy is. The base owns what every enemy has: health, reacting to a hit, being hurt and stunned, statuses
/// such as frozen, dying and what it leaves, hurting the player on contact, its timelines and its shots, and being
/// stepped by its room. A particular enemy adds only its own behaviour. A boss is an enemy too: one scene, its parts
/// child nodes with their own hurtboxes.
/// </summary>
/// <remarks>
/// Beside its simple values, an enemy's data file may give:
/// <code>
/// "reactions": [ ... ], "drops": { ... }, "colliders": { ... }, "timelines": { ... },
/// "shots": { "spit": { "damage": 10, "speed": 2, "projectile": { ... } } },
/// "hurt_flash_updates": 8, "stunned_while_hurt": true, "hurt_sound": "cry", "status_sounds": { "frozen": "enemy_frozen" },
/// "dud": { "effect": "effects/dud_shot.json", "sound": "dud_shot:low" },
/// "death": { "effect": "effects/small_explosion.json", "sound": "enemy_killed",
///            "by_kind": { "power_bomb": "effects/other_explosion.json" } },
/// "picture_offset": { "x": -8, "y": -8 }, "standing": "in_front"
/// </code>
/// "standing" names where it stands against the player (<see cref="World.Standing"/>); without it, behind them.
/// "hurt_colours" is a change of its pictures' colours (<see cref="Ui.ColourChange"/>) shown while its hurt flash
/// shows, on each of its visuals; without it the flash is its picture shown brighter. "frozen_colours" is the same
/// for the look of a frozen one; without it a frozen one is shown in a blue tint.
/// "lies" says where its pictures lie against the room's tiles (<see cref="LiesAgainstTiles"/>), in the word a
/// timeline says it in: "in_front_of_tiles" has it drawn in front of every tile of the room, the ones in front of the
/// room's things too, and so over the player whatever its standing; without it, it is drawn where it stands. No
/// enemy as a whole lies behind the room.
/// "dud" names what a shot that does it no harm leaves where the shot is, in place of the shot's own ending.
/// "death" names the timeline its death plays, which its <see cref="Remains"/> play out before its drop appears;
/// "by_kind" names another for a death by a hit carrying a kind, the first listed that the hit carries.
/// </remarks>
public abstract partial class Enemy : Node2D, IRoomEntity, IHittable, IPlaced, IHasValues, Movement.IMoves
{
    /// <inheritdoc/>
    Collider Movement.IMoves.MovingBody => RequireBody();

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>The name of the status in which an enemy is stopped and can be stood on.</summary>
    public const string Frozen = "frozen";

    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>How much damage destroys it.</summary>
    [Export, DataValue("health")]
    public int MaxHealth { get; set; } = 1;

    /// <summary>The damage it does to the player on contact; zero for none.</summary>
    [Export, DataValue("contact_damage")]
    public int ContactDamage { get; set; }

    /// <summary>The way it faces when its room is set up.</summary>
    [Export]
    public Facing StartsFacing { get; set; } = Facing.Left;

    /// <summary>Whether it goes on behaving however far from the view it is.</summary>
    [Export]
    public bool AlwaysActive { get; set; }

    /// <summary>
    /// Whether another just like it takes its place after it is killed: once what it left behind, its death effect and
    /// its drop, has gone, and a pause after that. Such an enemy is never recorded as gone.
    /// </summary>
    [Export]
    public bool RespawnsWhenKilled { get; set; }

    /// <summary>How many updates it flashes for after being hurt.</summary>
    public int HurtFlashUpdates { get; private set; } = 8;

    /// <summary>Whether it stops behaving while it flashes from being hurt.</summary>
    public bool StunnedWhileHurt { get; private set; } = true;

    private Room? _room;
    private Health? _health;
    private Reactions _reactions = Reactions.None;
    private DropTable _drops = DropTable.Nothing;
    private Countdown _unhittable = Countdown.Over;
    private Countdown _statusLeft = Countdown.Over;
    private Countdown _flash = Countdown.Over;

    /// <summary>Whether a hit's hurt still holds it: from the hit until its flash has run down far enough.</summary>
    private bool _hurt;
    private TimelinePlayer? _animation;
    private readonly Dictionary<string, ShotType> _shots = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _statusSounds = new(StringComparer.Ordinal);
    private string _hurtSound = "";
    private string _dudEffect = "";
    private string _dudSound = "";
    private Enemy? _replacement;
    private string _deathEffect = "";
    private readonly List<(HitKind Kind, string Effect)> _deathEffectsByKind = [];
    private Hit? _destroyedBy;
    private string _deathSound = "";

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>Its speed.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>The way it faces.</summary>
    public Facing Facing { get; set; }

    /// <summary>The room it is in.</summary>
    public Room Room => _room ?? throw new InvalidOperationException($"The enemy \"{Name}\" is not in a room.");

    /// <summary>Its health.</summary>
    public Health Health => _health ?? throw new InvalidOperationException($"The enemy \"{Name}\" is not in a room.");

    /// <summary>The status it is in, by name; empty for none.</summary>
    public string Status { get; private set; } = "";

    /// <summary>Whether it is frozen.</summary>
    public bool IsFrozen => Status == Frozen;

    /// <summary>How many more updates the enemy's status lasts; none when it is in no status.</summary>
    public int StatusUpdatesLeft => Status.Length > 0 ? _statusLeft.Remaining : 0;

    /// <summary>Whether it has been destroyed.</summary>
    public bool IsDead { get; private set; }

    /// <summary>Whether it is asleep: it neither behaves, nor can be hit, nor hurts on contact, until it is woken.</summary>
    public bool IsAsleep { get; private set; }

    /// <summary>Puts the enemy to sleep, as a boss is before its fight begins.</summary>
    public void Sleep() => IsAsleep = true;

    /// <summary>Wakes the enemy.</summary>
    public void Wake() => IsAsleep = false;

    /// <summary>
    /// How many turns it has taken while awake before the one it is taking: its first turn reads none. The count moves
    /// on once the turn's behaviour has been played, a hurt's or a frozen turn too, and before its timeline moves on,
    /// so a cue reads the turn counted.
    /// </summary>
    public int Updates { get; private set; }

    /// <summary>Whether it was hurt in this update.</summary>
    public bool WasJustHurt { get; private set; }

    /// <summary>Whether it is flashing from being hurt.</summary>
    public bool IsFlashing => _flash.IsRunning;

    /// <summary>
    /// How many updates of its hurt's flash are left, none when it is not flashing: for an enemy that shows its hurt
    /// its own way and times that by the flash. Asked once its turn is over, it is what the turn left.
    /// </summary>
    protected int HurtFlashLeft => _flash.Remaining;

    /// <summary>Its timelines, by name.</summary>
    public TimelineSet Timelines { get; private set; } = TimelineSet.None;

    /// <summary>The name of the timeline it is showing; empty for none.</summary>
    public string Playing { get; private set; } = "";

    /// <summary>Whether its timeline is held where it is rather than moving on each update. An enemy sets this for as long as something keeps it from acting.</summary>
    protected bool HoldsTimeline { get; set; }

    /// <summary>Whether the timeline it is showing, one that does not loop, has played through.</summary>
    public bool AnimationIsOver => _animation is { IsOver: true };

    /// <summary>The collider it moves through terrain with, if it has one.</summary>
    protected Collider? Body { get; private set; }

    /// <summary>The collider that blocks and carries the player, if its scene gives it one.</summary>
    protected Collider? Solid { get; private set; }

    /// <summary>
    /// The solid the engine gives an enemy that has no solid of its own, for the one state in which every enemy is
    /// solid: frozen, it blocks and carries by the shape its own size then has (<see cref="OwnSize"/>). It is made as
    /// the enemy freezes and taken away as it thaws, so that it is among the enemy's colliders only while the enemy
    /// does nothing: a class that goes through all its colliders never meets it. It is not the enemy's
    /// <see cref="Solid"/>.
    /// </summary>
    private Collider? _frozenSolid;

    /// <summary>What shows it, if its scene gives it a visual.</summary>
    protected ActorVisual? Visual { get; private set; }

    /// <summary>Where the player is: what most enemies aim at and move toward.</summary>
    protected PixelPosition Target => Room.Player.Position;

    /// <summary>
    /// Whether its place is inside the room's turn window now: the view widened as the game's definition says, and to
    /// either side by its own half-width, whatever else has it take turns (being always active, being frozen).
    /// </summary>
    protected bool IsWithinTurnReach => Room.IsWithinTurnReach(this);

    /// <summary>
    /// The side of it the player is on, by whole pixels: left when the player's whole pixel across is less than its
    /// own, right otherwise, so right when the two are in the same pixel.
    /// </summary>
    protected Facing TowardPlayer => Room.Player.Position.X.Whole < Position.X.Whole ? Facing.Left : Facing.Right;

    /// <summary>The gameplay random generator.</summary>
    protected GameRandom Random => Room.Session.Random;

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        // The one that takes its place is a copy of it as it was placed, taken before anything about it changes.
        if (RespawnsWhenKilled)
            _replacement = (Enemy)Duplicate();
        _room = room;
        Position = EntityNode.StartingPlace(this, Position);
        Facing = StartsFacing;
        DataValues.Fill(this, room.Content);
        _health = new Health(MaxHealth);
        Body = EntityNode.ColliderOf(this, ColliderRole.Body);
        Solid = EntityNode.ColliderOf(this, ColliderRole.Solid);
        _solidWhenNotFrozen = Solid is { Enabled: true };
        // Its own visual is the one named "Visual", wherever among its visuals that stands; failing that, the first.
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.Bind(room.Content);
            if (Visual is null || (visual.Name == "Visual" && Visual.Name != "Visual"))
                Visual = visual;
        }

        OnEnterRoom();
    }

    private Standing _standing = Standing.Behind;

    /// <summary>
    /// Where it stands against the player and the rest of what stands in the room: as its data says as it enters, and
    /// as it sets for itself after, where its standing changes in play.
    /// </summary>
    public Standing Standing
    {
        get => _standing;
        set
        {
            _standing = value;
            ShowDepth();
        }
    }

    private LiesAgainstTiles _lies;

    /// <summary>
    /// Where its pictures lie against the room's tiles, as its data says ("lies"). In front of every tile, the ones in
    /// front of the room's things too, it is drawn there, over the player as well, whatever its standing: the room
    /// draws in one order, and a thing in front of the tiles the player passes behind is in front of the player.
    /// Otherwise it is drawn where it stands; no enemy as a whole lies behind the room.
    /// </summary>
    /// <exception cref="InvalidOperationException">It is said to lie behind the room.</exception>
    public LiesAgainstTiles Lies
    {
        get => _lies;
        set
        {
            _lies = value;
            ShowDepth();
        }
    }

    private void ShowDepth()
    {
        RoomDepth.Stand(this, _standing);
        switch (_lies)
        {
            case LiesAgainstTiles.WhereItStands:
                break;
            case LiesAgainstTiles.InFrontOfTiles:
                ZIndex = RoomDepth.InFrontOfRoom;
                break;
            default:
                throw new InvalidOperationException(
                    $"The enemy \"{Name}\" is said to lie {_lies}: an enemy as a whole lies in front of the room's tiles or where it stands.");
        }
    }

    /// <summary>What a picture's name holds where the word of the look it is shown in goes (<see cref="Look"/>).</summary>
    public const string LookPlaceholder = "{look}";

    /// <summary>
    /// The look it is drawn in where it is placed, for an enemy whose pictures come in one look for some rooms and
    /// another for others: the word its timelines' pictures are named with in place of
    /// <see cref="LookPlaceholder"/>, as they are read. Empty for an enemy every room draws alike.
    /// </summary>
    [Export]
    public string Look { get; set; } = "";

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _reactions = data.Has("reactions") ? Reactions.Read(data.List("reactions")) : Reactions.None;
        _drops = data.Has("drops") ? DropTable.Read(data.Get("drops")) : DropTable.Nothing;
        Timelines = TimelineSet.ReadFrom(data);
        if (Look.Length > 0)
            Timelines = Timelines.WithPictures(picture => picture.Replace(LookPlaceholder, Look, StringComparison.Ordinal));
        EntityNode.ReadColliders(this, data);
        HurtFlashUpdates = data.Has("hurt_flash_updates") ? data.Int("hurt_flash_updates") : 8;
        _hurtColours = data.Has("hurt_colours") ? Ui.ColourChange.Read(data.Get("hurt_colours")) : null;
        _frozenColours = data.Has("frozen_colours") ? Ui.ColourChange.Read(data.Get("frozen_colours")) : null;
        _lies = data.Has("lies") ? data.Get("lies").AsEnum<LiesAgainstTiles>() : LiesAgainstTiles.WhereItStands;
        Standing = data.Has("standing") ? data.Get("standing").AsEnum<Standing>() : Standing.Behind;
        StunnedWhileHurt = !data.Has("stunned_while_hurt") || data.Flag("stunned_while_hurt");
        _hurtSound = data.Has("hurt_sound") ? data.Text("hurt_sound") : "";
        _dudEffect = data.Has("dud") && data.Get("dud").Has("effect") ? data.Get("dud").Text("effect") : "";
        _dudSound = data.Has("dud") && data.Get("dud").Has("sound") ? data.Get("dud").Text("sound") : "";
        _deathEffect = data.Has("death") && data.Get("death").Has("effect") ? data.Get("death").Text("effect") : "";
        _deathSound = data.Has("death") && data.Get("death").Has("sound") ? data.Get("death").Text("sound") : "";
        _deathEffectsByKind.Clear();
        if (data.Has("death") && data.Get("death").Has("by_kind"))
        {
            foreach ((string kind, DataNode effect) in data.Get("death").Get("by_kind").Entries())
                _deathEffectsByKind.Add((new HitKind(kind), effect.AsText()));
        }

        _statusSounds.Clear();
        if (data.Has("status_sounds"))
        {
            foreach ((string status, DataNode sound) in data.Get("status_sounds").Entries())
                _statusSounds[status] = sound.AsText();
        }

        _shots.Clear();
        if (data.Has("shots"))
        {
            foreach ((string name, DataNode shot) in data.Get("shots").Entries())
                _shots[name] = ShotType.Read(shot);
        }

        if (data.Has("picture_offset"))
        {
            var offset = new Vector2I(data.Get("picture_offset").Int("x"), data.Get("picture_offset").Int("y"));
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                visual.PictureOffset = offset;
        }
    }

    /// <summary>Called once as the enemy enters its room, after its values are loaded.</summary>
    protected virtual void OnEnterRoom()
    {
    }

    /// <summary>
    /// Counts on, for an update the enemies are held still through, the one thing of an enemy that does not stand
    /// still then: the time it cannot be hit for. The room asks it of each enemy whose turn it would have been. The
    /// time counts on only while the settling of hits reaches the enemy (<see cref="UnhittableTimeRuns"/>).
    /// </summary>
    internal void CountWhileHeldStill()
    {
        if (UnhittableTimeRuns)
            _unhittable = _unhittable.Stepped();
        WhileHeldStill();
    }

    /// <summary>
    /// Called on an update the enemies are held still through, when the enemy's turn would have come: for what of it,
    /// besides the time it cannot be hit for, does not stand still then, as a time of that kind one of its parts
    /// keeps for itself.
    /// </summary>
    protected virtual void WhileHeldStill()
    {
    }

    /// <summary>
    /// Whether the time the enemy cannot be hit for counts on this turn. What it is a time of is the settling of
    /// hits, which passes an enemy by while nothing of the player's can reach it: so it runs while the player's side
    /// can hit the enemy at all (<see cref="IsHitBy"/>). An enemy of several parts whose time is one part's says
    /// when that part is in reach, as one whose head is away while the rest of it still takes shots.
    /// </summary>
    protected virtual bool UnhittableTimeRuns => IsHitBy(HitSide.Player);

    /// <summary>
    /// Plays one update, its turn: its timers move on; its hits are settled, the player's shots tried against it and
    /// then its touch against the player, where it stood before moving; it behaves, unless a status or a hurt stops
    /// it; and its timeline moves on, unless it is frozen.
    /// </summary>
    internal void Step()
    {
        WasJustHurt = false;
        if (IsAsleep)
            return;
        bool unhittableAtStart = _unhittable.IsRunning;

        // The time it cannot be hit for counts on only on a turn the settling of hits reaches it: it passes it by
        // otherwise, and the time stands.
        if (UnhittableTimeRuns)
            _unhittable = _unhittable.Stepped();

        // Its hits are settled in its own turn, before it acts and where it stood before moving: the player's shots
        // are tried against it, then its touch against the player. An enemy that could not be hit as its turn began
        // takes part in neither, on the update that ends that too.
        if (!unhittableAtStart)
        {
            Room.TryPlayerShotsAgainst(this);
            if (IsDead)
                return;
            if (!IsFrozen)
                TouchPlayer();
            if (IsDead)
                return;
        }

        // A hurt stops its behaviour from the turn the hit lands on, and holds off a status's time meanwhile. A
        // status holds the turn that finds its time run out, or finds that it no longer lasts, and ends with it;
        // on any other turn its time runs down. Being frozen ends the hurt's flash.
        bool heldByHurt = StunnedWhileHurt && _hurt;
        bool heldByStatus = false;
        if (!heldByHurt && Status.Length > 0)
        {
            heldByStatus = IsFrozen;
            if (heldByStatus)
                (_flash, _hurt) = (Countdown.Over, false);
            if (_statusLeft.IsOver || !Room.Session.Definition.StatusLasts(Room.Session, Status))
                EndStatus();
            else
                _statusLeft = _statusLeft.Stepped();
        }

        bool behaves = !heldByHurt && !heldByStatus && !IsFrozen;
        if (heldByHurt)
            WhileHurt();
        else if (behaves)
            Behave();
        else
            WhileFrozen();

        // Its own count moves on once the turn has been acted, whatever held it, and before its timeline does. The
        // timeline moves on through a hurt, whose hold is on the behaviour alone; only being frozen stops it.
        Updates++;
        if (!IsFrozen && !IsDead)
            StepAnimation();

        if (IsDead)
            return;

        // The flash runs down once the turn has been played, and the hurt ends while some of it is left. A hurt with
        // no flash at all ends with the turn it began on.
        if (_flash.IsRunning)
            _flash = _flash.Stepped();
        if (_hurt && _flash.Remaining < Math.Max(1, Room.Session.Definition.EnemyHurtEndsAtFlashLeft))
            _hurt = false;

        // It is shown as it stands once its turn is over.
        ShowLook();
    }

    /// <summary>
    /// Whether it takes its whole turn with the room's objects, after the player and the projectiles and once the
    /// room's objects have had theirs, wherever the view is, in place of a turn in the enemies' pass: a kind that
    /// takes hits and acts as an enemy does but belongs to the room's fittings. The player's shots meet it as they
    /// move, as they meet the room's objects, since it may lie within one that stops them; one still standing on it
    /// is tried against it in that turn, and it touches the player there. No enemy does unless its kind says so.
    /// </summary>
    public virtual bool ActsWithObjects => false;

    /// <summary>This enemy's own behaviour for one update.</summary>
    protected abstract void Behave();

    /// <summary>
    /// Called in its behaviour's place on each turn a hurt holds the behaviour off: what of the enemy goes on while
    /// its body flashes. Nothing, unless the enemy says otherwise.
    /// </summary>
    protected virtual void WhileHurt()
    {
    }

    /// <summary>Shows the timeline named <paramref name="name"/> from its start. Naming the one showing changes nothing unless <paramref name="restart"/> is asked for.</summary>
    protected void Play(string name, bool restart = false)
    {
        if (name == Playing && !restart && _animation is not null)
            return;
        Playing = name;
        _animation = new TimelinePlayer(Timelines.Get(name));
        ShowStep(_animation.Shown);
    }

    /// <summary>
    /// Shows the timeline named <paramref name="name"/> from its start once the step showing has had its time: the
    /// picture showing stays out its updates, none of the cues left of the timeline it leaves come, and the new
    /// timeline's first cues come as its first step begins. It is the timeline playing from now.
    /// </summary>
    /// <exception cref="InvalidOperationException">It shows no timeline for another to follow.</exception>
    protected void PlayAfterStep(string name)
    {
        TimelinePlayer playing = _animation ?? throw new InvalidOperationException($"The enemy \"{Name}\" shows no timeline for \"{name}\" to follow.");
        Playing = name;
        playing.FollowWith(Timelines.Get(name));
    }

    /// <summary>
    /// The place its timeline has reached, to pick it up from again later. Asked while a cue is being acted on, that is
    /// just past that cue, with the rest of the step still to come; asked at any other time, it is the start of the
    /// step that follows the one showing.
    /// </summary>
    /// <exception cref="InvalidOperationException">It shows no timeline, or the one it shows has ended.</exception>
    protected TimelineMark MarkTimeline()
    {
        TimelinePlayer playing = _animation ?? throw new InvalidOperationException($"The enemy \"{Name}\" shows no timeline to mark a place in.");
        if (_cueBeingActedOn >= 0)
            return new TimelineMark(Playing, _cueStep, _cueBeingActedOn + 1);
        if (playing.IsOver)
            throw new InvalidOperationException($"The timeline \"{Playing}\" of the enemy \"{Name}\" has ended and has no place to mark.");
        if (playing.IsAtStepStart || playing.IsFollowed)
            return new TimelineMark(Playing, playing.IsFollowed ? 0 : playing.StepIndex, 0);
        int next = playing.StepIndex + 1 < playing.Timeline.Steps.Count
            ? playing.StepIndex + 1
            : playing.Timeline.LoopsTo ?? throw new InvalidOperationException($"The timeline \"{Playing}\" of the enemy \"{Name}\" ends after the step showing.");
        return new TimelineMark(Playing, next, 0);
    }

    /// <summary>Picks its timeline up again from a place marked earlier.</summary>
    protected void ResumeTimeline(TimelineMark mark)
    {
        Playing = mark.Name;
        _animation = TimelinePlayer.Resuming(Timelines.Get(mark.Name), mark.Step, mark.CuesPassed);
        ShowStep(_animation.Shown);
    }

    /// <summary>
    /// The picture shown for a step of its timeline. The engine's enemy shows the step's own; an enemy whose pictures
    /// depend on something more, such as what state its body is in, says which here.
    /// </summary>
    protected virtual string PictureOf(TimelineStep step) => step.Picture;

    /// <summary>
    /// Shows a step of its timeline on its own visual: the picture it shows for the step, where the step puts its
    /// picture. A timeline that places its own pictures is shown where it places them.
    /// </summary>
    private void ShowStep(TimelineStep step) => Visual?.Show(step with { Picture = PictureOf(step) });

    /// <summary>
    /// Called for each cue the enemy's timeline reaches that is the enemy's own. The base acts on three itself:
    /// "sound:" and a sound's name asks for that sound, "always_active" makes the enemy go on behaving however far
    /// from the view it is, and "not_always_active" ends that.
    /// </summary>
    protected virtual void OnCue(string cue)
    {
    }

    /// <summary>The shot of the given name from its data file.</summary>
    /// <exception cref="InvalidOperationException">The enemy's data gives it no shot of that name.</exception>
    protected ShotType Shot(string name) =>
        _shots.TryGetValue(name, out ShotType? shot) ? shot : throw new InvalidOperationException($"The enemy \"{Name}\" has no shot named \"{name}\".");

    /// <summary>Launches one of its shots from <paramref name="at"/> at <paramref name="velocity"/>, and returns the projectile.</summary>
    protected Projectile Fire(string shot, PixelPosition at, PixelVelocity velocity)
    {
        ShotType type = Shot(shot);
        Projectile projectile = type.Launch(at, velocity, HitSide.Enemies, this, this);

        // With no place free in the room the shot is not fired: what is returned is in no room, and is done with.
        if (Room.Spawn(projectile) && type.Sound.Length > 0)
            Room.Session.Sounds.Request(type.Sound, channel: "enemies");
        return projectile;
    }

    /// <summary>Asks for a sound, with a priority for a name that does not say its own.</summary>
    protected void Sound(string sound, SoundPriority priority = SoundPriority.Normal)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "enemies", priority: priority);
    }

    /// <inheritdoc/>
    public virtual bool IsHitBy(HitSide side) => side != HitSide.Enemies && !IsDead && !IsAsleep;

    /// <inheritdoc/>
    public virtual bool Stops(Hit hit) => false;

    /// <summary>Makes the enemy flash for <paramref name="updates"/> updates, as a hit that hurt it does.</summary>
    protected void FlashFor(int updates) => _flash = Countdown.Of(updates);

    /// <summary>
    /// Makes the enemy hurt with no hit, flashing for <paramref name="updates"/> updates: whatever a hurt holds off
    /// is held off from its next turn, as after a hit that hurt it.
    /// </summary>
    protected void BeHurtFor(int updates) => (_flash, _hurt) = (Countdown.Of(updates), true);

    /// <summary>Makes it one no hit can land on for <paramref name="updates"/> updates, as after a hit whose reaction says so.</summary>
    protected void BeUnhittableFor(int updates) => _unhittable = Countdown.Of(updates);

    /// <summary>Ends its hurt and its flash at once: its behaviour is held off no longer, from the turn this is called on.</summary>
    protected void EndHurt() => (_flash, _hurt) = (Countdown.Over, false);

    /// <summary>
    /// Has the player's shots tried against the enemy again, each where it stands and the enemy where it now is: for
    /// an enemy a part of which meets shots at a moment of its own turn, and not with the rest of it before it acts.
    /// Its own answer to a hit (<see cref="TakeHit"/>) says which of its parts answer at which of the two moments.
    /// </summary>
    protected void TryPlayerShotsNow() => Room.TryPlayerShotsAgainst(this);

    /// <inheritdoc/>
    public virtual HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsDead)
            return HitResult.Ignored;

        // A hit its reaction lets pass takes no notice of it, whatever state it is in.
        Reaction reaction = ReactionTo(hit, part);
        if (reaction.Passes)
            return HitResult.Ignored;

        // One it swallows does nothing to it and is spent on it, with nothing shown.
        if (reaction.Swallows)
            return HitResult.Dud;
        if (_unhittable.IsRunning)
            return HitResult.Absorbed;

        int damage = reaction.DamageFrom(hit.Damage);
        Health health = HealthOf(part);
        bool lethal = damage > 0 && health.WouldBeEmptiedBy(damage);
        bool takesStatus = reaction.StatusWhen == StatusWhen.Always ||
            (reaction.StatusWhen == StatusWhen.InsteadOfDying && lethal && Status != reaction.Status);
        if (takesStatus)
        {
            BeginStatus(reaction.Status, Room.StatusUpdatesFor(reaction.Status, reaction.StatusUpdates));
            _unhittable = Countdown.Of(reaction.StatusUnhittableUpdates);
        }

        if (reaction.StatusWhen == StatusWhen.InsteadOfDying && takesStatus)
            return HitResult.Hurt;
        if (damage == 0)
        {
            if (takesStatus)
                return HitResult.Hurt;
            HitResult answer = LeaveDud(hit);
            OnAbsorbed(hit, part);
            return answer;
        }

        health.Take(damage);
        _unhittable = Countdown.Of(reaction.UnhittableUpdates);
        _flash = Countdown.Of(Math.Max(0, HurtFlashUpdates - hit.FlashFewer));
        _hurt = true;
        WasJustHurt = true;
        if (!IsFrozen && !hit.Quiet)
            Sound(_hurtSound);
        if (health.IsEmpty)
        {
            _destroyedBy = hit;
            if (!DiesAtOnce(hit))
                return HitResult.Hurt;
            Die();
            return HitResult.Destroyed;
        }

        OnHurt(hit, part);
        return HitResult.Hurt;
    }

    /// <summary>
    /// Called when a hit has emptied the enemy's health. An enemy whose death plays out over some updates returns
    /// false, takes no more hits, and calls <see cref="Die"/> itself when it is done.
    /// </summary>
    protected virtual bool DiesAtOnce(Hit hit) => true;

    /// <summary>
    /// The reaction to a hit that landed on <paramref name="part"/>. An enemy whose parts react differently says so here.
    /// </summary>
    protected virtual Reaction ReactionTo(Hit hit, Collider part) => _reactions.To(hit);

    /// <summary>
    /// The health a hit that landed on <paramref name="part"/> is taken from; emptying it destroys the enemy. The
    /// engine's enemy has one health. An enemy whose parts each have health of their own says which here.
    /// </summary>
    protected virtual Health HealthOf(Collider part) => Health;

    /// <summary>The enemy's reactions as its data file gives them.</summary>
    protected Reactions Reactions => _reactions;

    /// <summary>What the enemy leaves when it is destroyed, as its data file gives it.</summary>
    protected DropTable Drops => _drops;

    /// <summary>
    /// Leaves what the enemy's data gives for a shot that does it no harm ("dud"), where the shot is, and asks for
    /// the dud's sound. Only a hit that comes of something with a place leaves it, and only an enemy whose data gives
    /// one: the answer is then that the hit was a dud, and otherwise that it was absorbed. An enemy that answers a
    /// hit itself answers a harmless one with this.
    /// </summary>
    protected HitResult LeaveDud(Hit hit)
    {
        if (hit.At is not { } at || (_dudEffect.Length == 0 && _dudSound.Length == 0))
            return HitResult.Absorbed;
        if (_dudEffect.Length > 0)
            LeaveEffect(_dudEffect, at);
        Sound(_dudSound);
        return HitResult.Dud;
    }

    /// <summary>Called when a hit landed on the enemy and did it no damage and gave it no status.</summary>
    protected virtual void OnAbsorbed(Hit hit, Collider part)
    {
    }

    /// <summary>Called when a hit hurt the enemy without destroying it.</summary>
    protected virtual void OnHurt(Hit hit, Collider part)
    {
    }

    /// <summary>
    /// Destroys the enemy: it is recorded as gone and taken out of its room, and it leaves its <see cref="Remains"/>,
    /// which play its death and then leave its drop. One with no death to play leaves its drop at once. The death is
    /// the one its data gives for what destroyed it.
    /// </summary>
    public void Die() => Die(DeathEffectFor(_destroyedBy));

    /// <summary>
    /// Destroys the enemy as <see cref="Die()"/> does, in the death it names itself: <paramref name="death"/>, the
    /// data file of the timeline its remains play, in place of the one its data gives for what destroyed it; empty
    /// for no death to play. For an enemy whose death is settled by something of its own as it dies.
    /// </summary>
    protected void Die(string death)
    {
        ArgumentNullException.ThrowIfNull(death);
        if (IsDead)
            return;
        IsDead = true;
        Room.CountKilled();
        if (!RespawnsWhenKilled)
            Room.MarkGone(Name);
        Sound(_deathSound);
        Enemy? replacement = _replacement;
        _replacement = null;
        if (replacement is not null)
            OnReplacedBy(replacement);
        if (death.Length > 0)
        {
            var remains = Remains.Of(FrameTimeline.Read(Room.Content.ReadData(death)), Position, _drops);
            if (replacement is not null)
                remains.ReplaceWith(replacement, Room.Session.Definition.SpentDropUpdates);
            Room.Spawn(remains);
        }
        else
        {
            Pickup? pickup = Drop(_drops, Position);
            if (replacement is not null)
                Room.SpawnLater(replacement, pickup is null ? [] : [pickup], Room.Session.Definition.SpentDropUpdates);
        }

        OnDied();
        Room.Remove(this);
    }

    /// <summary>The sound of its death, by name, as its data gives it ("death", "sound"); empty when it has none.</summary>
    protected string SoundOfDeath => _deathSound;

    /// <summary>The timeline of its death by <paramref name="hit"/>; of its death by nothing in particular when that is null.</summary>
    private string DeathEffectFor(Hit? hit)
    {
        if (hit is { } by)
        {
            foreach ((HitKind kind, string effect) in _deathEffectsByKind)
            {
                if (by.Carries(kind))
                    return effect;
            }
        }

        return _deathEffect;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        // A copy never used, because the enemy left its room alive, goes with it.
        if (_replacement is { } unused && IsInstanceValid(unused))
            unused.Free();
        _replacement = null;
    }

    /// <summary>
    /// Leaves an effect in the room at <paramref name="at"/>, by the data file of its timeline; standing where
    /// <paramref name="standing"/> says, or where what is launched and left lies.
    /// </summary>
    protected Effect LeaveEffect(string file, PixelPosition at, Standing? standing = null)
    {
        Effect effect = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(file)), at, standing: standing);
        Room.Spawn(effect);
        return effect;
    }

    /// <summary>
    /// What a hit has left of the enemy, as it stands: for a kind that carries a hit of its own onto another enemy
    /// (<see cref="TakeAftermath"/>).
    /// </summary>
    public HitAftermath AftermathOfHit => new(_shake.Remaining, _unhittable.Remaining, _flash.Remaining, Status, StatusUpdatesLeft);

    /// <summary>
    /// Gives the enemy what a hit left of another, or of a part of another: its shake, the time it cannot be hit, its
    /// hurt flash and its status take those counts in place of their own, a status of none ending the one it has.
    /// It counts them down on its own turns from then. Nothing of its health changes.
    /// </summary>
    public void TakeAftermath(HitAftermath left)
    {
        if (IsDead)
            return;
        _shake = Countdown.Of(left.ShakeShows);
        _unhittable = Countdown.Of(left.UnhittableUpdates);
        _flash = Countdown.Of(left.FlashUpdates);
        _hurt = left.FlashUpdates > 0;
        if (left.Status.Length > 0)
            BeginStatus(left.Status, left.StatusUpdates);
        else if (Status.Length > 0)
            EndStatus();
    }

    /// <summary>
    /// Takes the enemy out of its room at another's word, with no death: for a kind whose destroyed part deletes
    /// another enemy outright.
    /// </summary>
    public void BeTakenOut() => Vanish();

    /// <summary>Takes the enemy out of its room with no death: no effect, no drop, and not recorded as gone.</summary>
    protected void Vanish()
    {
        if (IsDead)
            return;
        IsDead = true;
        Room.Remove(this);
    }

    /// <summary>Rolls a drop table and leaves what it gives at <paramref name="at"/>. Returns the pickup left, if any.</summary>
    protected Pickup? Drop(DropTable table, PixelPosition at)
    {
        return Pickup.Dropped(Room, table, at);
    }

    /// <summary>Called as the enemy is destroyed, before it leaves its room.</summary>
    protected virtual void OnDied()
    {
    }

    /// <summary>
    /// Called as an enemy that is replaced when killed dies, with the copy that will take its place, which has not
    /// yet entered the room. A kind something of which is not set afresh by its setup, and so is found by the next
    /// of its place as the last one left it, hands that on here.
    /// </summary>
    protected virtual void OnReplacedBy(Enemy replacement)
    {
    }

    /// <summary>
    /// Puts the enemy in a status for a number of updates. Frozen, it stops and blocks and carries: by its solid
    /// collider, or, having none, by one the shape of its body.
    /// </summary>
    public void BeginStatus(string status, int updates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        bool wasFrozen = IsFrozen;
        Status = status;
        _statusLeft = Countdown.Of(updates);
        if (IsFrozen && Solid is not null)
            Solid.Enabled = true;
        if (IsFrozen && Solid is null && OwnSize is { } own)
        {
            if (_frozenSolid is null)
            {
                _frozenSolid = new Collider { Name = "FrozenSolid", Role = ColliderRole.Solid };
                AddChild(_frozenSolid);
                Room.Collision.Register(_frozenSolid);
            }

            _frozenSolid.Rect = own.Rect;
        }
        else if (!IsFrozen)
        {
            DropFrozenSolid();
        }
        if (IsFrozen != wasFrozen || !wasFrozen)
            Sound(_statusSounds.GetValueOrDefault(status, ""));
        OnStatusBegan(status);
    }

    /// <summary>Called as the enemy is put in a status.</summary>
    protected virtual void OnStatusBegan(string status)
    {
    }

    /// <summary>Called as a status the enemy was in ends.</summary>
    protected virtual void OnStatusEnded(string status)
    {
    }

    /// <summary>
    /// Moves the enemy across by its speed and returns what happened. No solid stops an enemy but one that stands as
    /// part of the room, whatever the options say of solids.
    /// </summary>
    protected MoveResult MoveAcross(MoveOptions options) =>
        Room.Mover.MoveAcross(RequireBody(), Velocity.X, options with { StoppedBySolids = false });

    /// <summary>Moves the enemy down by its speed and returns what happened; as across, no solid but the room's stops it.</summary>
    protected MoveResult MoveDown(MoveOptions options) =>
        Room.Mover.MoveDown(RequireBody(), Velocity.Y, options with { StoppedBySolids = false });

    /// <summary>
    /// Moves the enemy by <paramref name="offset"/> through everything, carrying along whoever stands on its solid
    /// collider and pushing ahead whoever is in its way. An enemy with no solid collider on just moves.
    /// </summary>
    protected void MoveCarrying(PixelOffset offset)
    {
        if (Solid is { Enabled: true } solid)
            Room.Mover.MoveSolid(solid, offset);
        else
            Position += offset;
    }

    /// <summary>
    /// Whether the player stands on the enemy's solid collider: the two overlap across, the player's feet are no
    /// more than <paramref name="feetAbove"/> pixels over its top, and the middle of the player, reckoned that many
    /// pixels lower, lies above the collider's own. So a rider sunk into the top rides still, until their middle
    /// passes the collider's, and one that close over it rides already. How close is the game's to say, and may be
    /// one enemy's own.
    /// </summary>
    protected bool IsRiddenByPlayer(int feetAbove)
    {
        if (Solid is not { Enabled: true } solid)
            return false;
        PixelRect top = solid.RoomRect, rider = Room.Player.BodyRect;
        if (rider.Left >= top.Right || rider.Right <= top.Left)
            return false;
        return top.Top - rider.Bottom <= feetAbove && rider.Top + rider.Bottom + 2 * feetAbove < top.Top + top.Bottom;
    }

    /// <summary>
    /// Gives the player <paramref name="offset"/> to move by: for an enemy that takes its rider with it, or pushes
    /// whoever it meets. In its own turn, before the player has moved, the offset becomes part of the player's own
    /// move of that update (<see cref="Player.MoveFromOutside"/>), so the enemies whose turns come after meet
    /// the player where they were; given at any other time it moves them at once, as far as terrain lets them go.
    /// </summary>
    protected void ShiftPlayer(PixelOffset offset)
    {
        // Moved at once, the player is carried as far as terrain lets them go, and into a way out of the room as by
        // their own move.
        MoveOptions carried = MoveOptions.AmongBlocks with { Slopes = SlopeTreatment.Wall, StoppedByWaysOut = false };
        Room.Player.MoveFromOutside(offset, carried);
    }

    /// <summary>Whether the enemy stands on terrain, or on a solid that stands as part of the room.</summary>
    protected bool IsOnGround => Room.Mover.IsOnGround(RequireBody(), MoveOptions.AmongBlocks);

    /// <summary>Whether the player's body is within the given distances of the enemy, across and down.</summary>
    protected bool PlayerIsWithin(int across, int down)
    {
        PixelOffset toPlayer = Target - Position;
        return toPlayer.X.Abs() < Pixels.FromWhole(across) && toPlayer.Y.Abs() < Pixels.FromWhole(down);
    }

    /// <inheritdoc/>
    public void ShowPosition()
    {
        if (!_shake.IsRunning)
        {
            EntityNode.Show(this, Position);
            return;
        }

        // Shaken, it is shown a pixel to one side, two of its turns each way by its own count; where it is stays.
        EntityNode.Show(this, Position + PixelOffset.FromWhole(Updates % 4 >= 2 ? -1 : 1, 0));
        _shake = _shake.Stepped();
    }

    /// <summary>The updates left for which it is shown shaken.</summary>
    private Countdown _shake = Countdown.Over;

    /// <summary>
    /// Shakes the enemy where it is shown: for the next <paramref name="shows"/> updates it is shown a pixel to the
    /// right or to the left by turns, by its own count of turns. Only what is seen moves: it is met where it is.
    /// </summary>
    public void ShakeFor(int shows) => _shake = Countdown.Of(shows);

    /// <summary>Moves the enemy's timeline on by one update and hands it the cues reached.</summary>
    private void StepAnimation()
    {
        if (_animation is null || HoldsTimeline)
            return;
        // A cue may start another timeline. The rest of the old step's cues are then dropped, and the new timeline's
        // first step begins in this same update, so that nothing is lost between the two.
        for (int switches = 0; ; switches++)
        {
            if (switches > MostTimelineSwitches)
                throw new InvalidOperationException($"The timelines of the enemy \"{Name}\" start one another without end; the last was \"{Playing}\".");
            TimelinePlayer playing = _animation;
            FrameTimeline stepped = playing.Timeline;
            _cueStep = playing.StepIndex;
            IReadOnlyList<string> cues = playing.Step();
            for (int index = 0; index < cues.Count && _animation == playing; index++)
            {
                string cue = cues[index];
                _cueBeingActedOn = stepped.Steps[_cueStep].Cues.Count - cues.Count + index;
                // Three cues mean the same for every enemy; the rest are the enemy's own.
                if (cue.StartsWith("sound:", StringComparison.Ordinal))
                    Sound(cue["sound:".Length..]);
                else if (cue == "always_active")
                    AlwaysActive = true;
                else if (cue == "not_always_active")
                    AlwaysActive = false;
                else
                    OnCue(cue);
                _cueBeingActedOn = -1;
                if (IsDead)
                    return;
            }

            if (_animation == playing)
                break;
        }

        // The picture is that of the step this update fell in: it changes on the update the step's cues come.
        ShowStep(_animation.Shown);
    }

    /// <summary>How many times in one update its timelines may start one another before that is taken for a mistake in them.</summary>
    private const int MostTimelineSwitches = 64;

    /// <summary>The step whose cues are being acted on, and which of them; the latter is below nothing when none is.</summary>
    private int _cueStep;
    private int _cueBeingActedOn = -1;

    /// <summary>
    /// Shows the enemy as it is: flashing from a hurt, frozen, or neither. On an update its hurt flash shows, that is
    /// what it shows, over its own look and not its frozen one: a frozen enemy that is hurt shows the flash and its
    /// frozen look by turns. A look its data gives colours for is its own pictures in those colours; one it gives
    /// none for is a tint of its picture.
    /// </summary>
    private void ShowLook()
    {
        if (Visual is null)
            return;
        bool flashes = ShowsHurtFlash, frozen = !flashes && ShowsFrozenLook;
        Visual.Tint(flashes && _hurtColours is null ? HurtTint : frozen && _frozenColours is null ? FrozenTint : RestingTint);
        ShowColours(_frozenColours, frozen, FrozenColours, _frozenShownOn);
        ShowColours(_hurtColours, flashes, HurtFlashColours, _flashShownOn);
    }

    /// <summary>
    /// Shows a look's colours on each of the enemy's visuals while the look shows, and takes them back when it does
    /// not: one drawn in parts takes the look whole. A change is worked out for a visual by the pictures it shows, so
    /// one that shows none yet is left until it does, and one that has the look is shown it again each update, since
    /// it may have taken up another set of its pictures.
    /// </summary>
    /// <param name="colours">The look's colours, as the enemy's data gives them; null for a look shown as a tint.</param>
    /// <param name="shows">Whether the look shows on this update.</param>
    /// <param name="name">The name the look's colours are shown under.</param>
    /// <param name="shownOn">The visuals the look's colours are showing on now.</param>
    private void ShowColours(Ui.ColourChange? colours, bool shows, string name, HashSet<ActorVisual> shownOn)
    {
        if (colours is null || (!shows && shownOn.Count == 0))
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            if (visual.Picture.Length == 0 || (!shows && !shownOn.Contains(visual)))
                continue;
            (shows ? colours : Ui.ColourChange.None).ShowOn(visual, name);
            if (shows)
                shownOn.Add(visual);
            else
                shownOn.Remove(visual);
        }
    }

    /// <summary>The name its hurt flash's colours are shown under.</summary>
    private const string HurtFlashColours = "HurtFlash";

    /// <summary>The name its frozen look's colours are shown under.</summary>
    private const string FrozenColours = "Frozen";

    /// <summary>
    /// The colours its pictures are shown in while it shows frozen, as its data gives them ("frozen_colours"); null
    /// for one whose data gives none, which is tinted instead.
    /// </summary>
    private Ui.ColourChange? _frozenColours;

    /// <summary>The visuals of its own its frozen look's colours are showing on now.</summary>
    private readonly HashSet<ActorVisual> _frozenShownOn = [];

    /// <summary>
    /// The colours its pictures are shown in while its hurt flash shows, as its data gives them ("hurt_colours");
    /// null for one whose data gives none, which is shown brighter instead.
    /// </summary>
    private Ui.ColourChange? _hurtColours;

    /// <summary>The visuals of its own its hurt flash's colours are showing on now.</summary>
    private readonly HashSet<ActorVisual> _flashShownOn = [];

    /// <summary>
    /// Whether the picture of this update shows the enemy's hurt flash: while the flash still runs once the turn
    /// has counted it down, on so many updates in turn as the game's definition gives the flash, by the updates the
    /// flash has left. An enemy that shows its hurt its own way, and not by the flash every enemy shares, says so
    /// here.
    /// </summary>
    protected virtual bool ShowsHurtFlash
    {
        get
        {
            if (!_flash.IsRunning)
                return false;
            int each = Room.Session.Definition.EnemyFlashUpdatesEachWay;
            return each == 0 || _flash.Remaining % (2 * each) >= each;
        }
    }

    /// <summary>
    /// Whether the picture of this update shows the frozen look: steadily while as many updates of the freeze are
    /// left as the game's definition says, and then on so many updates in turn, by the updates left.
    /// </summary>
    private bool ShowsFrozenLook
    {
        get
        {
            if (!IsFrozen)
                return false;
            int each = Room.Session.Definition.FrozenLookBlinkUpdatesEachWay;
            return each == 0 || _statusLeft.Remaining >= Room.Session.Definition.FrozenLookSteadyUpdates
                || _statusLeft.Remaining % (2 * each) >= each;
        }
    }

    /// <summary>The tint the enemy is shown with when nothing has happened to it. An enemy that glows says so here.</summary>
    protected virtual Color RestingTint => Colors.White;

    /// <summary>The tint of a frozen enemy whose data gives the look no colours.</summary>
    private static readonly Color FrozenTint = new(0.6f, 0.8f, 1.5f);

    /// <summary>The tint of an enemy flashing from a hit whose data gives the flash no colours.</summary>
    private static readonly Color HurtTint = new(2f, 2f, 2f);

    /// <summary>Takes away the solid the engine made for the enemy's freeze, if it has one.</summary>
    private void DropFrozenSolid()
    {
        if (_frozenSolid is null)
            return;
        Room.Collision.Unregister(_frozenSolid);
        RemoveChild(_frozenSolid);
        _frozenSolid.QueueFree();
        _frozenSolid = null;
    }

    /// <summary>
    /// Called in its behaviour's place on each turn that begins with it frozen and no hurt holding it, as
    /// <see cref="WhileHurt"/> is on a turn a hurt holds it: what of the enemy goes on while its body stands frozen.
    /// The turn that finds the freeze's time run out is one of them: the freeze has ended by the time this is called
    /// on it (<see cref="IsFrozen"/> is false), and its behaviour comes back on the turn after. Nothing, unless the
    /// enemy says otherwise.
    /// </summary>
    protected virtual void WhileFrozen()
    {
    }

    /// <summary>Ends its being frozen at once, as the freeze's time running out does. Nothing for an enemy that is not frozen.</summary>
    protected void EndFreeze()
    {
        if (IsFrozen)
            EndStatus();
    }

    /// <summary>
    /// Counts the enemy among the room's killed (<see cref="World.Room.EnemiesKilled"/>) as its death does, for one
    /// that is taken away without dying.
    /// </summary>
    protected void CountAsKilled() => Room.CountKilled();

    /// <summary>Ends the enemy's status.</summary>
    private void EndStatus()
    {
        if (IsFrozen && Solid is not null)
            Solid.Enabled = _solidWhenNotFrozen;
        DropFrozenSolid();
        string ended = Status;
        Status = "";
        OnStatusEnded(ended);
    }

    /// <summary>Whether the enemy's solid collider is on when it is not frozen.</summary>
    private bool _solidWhenNotFrozen;

    /// <summary>
    /// Lets the enemy touch the player if a part of it overlaps the player's body. A player whose body is itself an
    /// attack is kept safe from no enemy by an earlier hurt: as an enemy that shows a picture takes its turn to
    /// touch, whatever safety a hurt left the player ends, whether or not the two then meet.
    /// </summary>
    private void TouchPlayer()
    {
        if (_animation is { Shown.Picture.Length: > 0 } && Room.Player.BodyPowers.Any(power => power.IsActive))
            Room.Player.BeInvincibleFor(0);
        if (PartTouchingPlayer() is { } part)
            Touch(part);
    }

    /// <summary>
    /// The collider that is the enemy's own size: the one it moves through the room by, whatever shape its pose gives
    /// what can be hit of it; failing one, as for an enemy that never meets terrain, its first hurtbox; failing any,
    /// its first hitbox. Null where it has none of the three.
    /// </summary>
    private Collider? OwnSize
    {
        get
        {
            var parts = EntityNode.CollidersOf(this).ToList();
            return parts.FirstOrDefault(collider => collider.Role == ColliderRole.Body)
                ?? parts.FirstOrDefault(collider => collider.Role == ColliderRole.Hurtbox)
                ?? parts.FirstOrDefault(collider => collider.Role == ColliderRole.Hitbox);
        }
    }

    /// <summary>Half the width of the enemy's own size (<see cref="OwnSize"/>); nothing where it has none.</summary>
    public int BodyHalfWidth => OwnSize is { } own ? own.RoomRect.Width / 2 : 0;

    /// <summary>
    /// The enemy's own size where it stands, in room pixels (<see cref="OwnSize"/>); null where it has none. What
    /// measures itself against the enemy and not against what its pose gives of it to be hit, as a game's grappling
    /// line may, asks this.
    /// </summary>
    public PixelRect? OwnRect => OwnSize?.RoomRect;

    /// <summary>Whether no hit can land on it for the moment, as for the updates a hit whose reaction says so leaves it so.</summary>
    public bool IsSafeFromHits => _unhittable.IsRunning;

    /// <summary>The part of the enemy that overlaps the player's body, if one does: a hitbox, or failing any hitboxes a hurtbox.</summary>
    protected Collider? PartTouchingPlayer()
    {
        var touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hitbox).ToList();
        if (touching.Count == 0)
            touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hurtbox).ToList();
        PixelRect player = Room.Player.BodyRect;
        return touching.FirstOrDefault(collider => collider.RoomRect.Overlaps(player));
    }

    /// <summary>Whether the player is standing on the enemy's solid or pressed against it.</summary>
    protected bool PlayerRestsAgainstIt()
    {
        if (Solid is not { Enabled: true } solid)
            return false;
        PixelRect body = Room.Player.BodyRect;
        var reach = new PixelRect(body.Left - 1, body.Top - 1, body.Width + 2, body.Height + 2);
        return reach.Overlaps(solid.RoomRect);
    }

    /// <summary>
    /// Called each update that a part of the enemy touches the player. The engine's enemy hurts the player with its
    /// contact damage; an enemy that does something else to whoever it touches says so here.
    /// </summary>
    protected virtual void Touch(Collider part)
    {
        if (ContactDamage <= 0)
            return;
        // The player is knocked away from the enemy by where the two stand in whole pixels, to the right when level.
        var direction = new PixelOffset(Pixels.FromWhole(Room.Player.Position.X.Whole >= Position.X.Whole ? 1 : -1), Pixels.Zero);
        Collider hurtbox = Room.Player.Hurtbox;
        HitResult result = Room.Player.TakeHit(
            new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Direction = direction, Source = this }, hurtbox);
        if (result is HitResult.Hurt or HitResult.Destroyed)
            OnTouchedPlayer();
    }

    /// <summary>Called when the enemy's touch has hurt the player.</summary>
    protected virtual void OnTouchedPlayer()
    {
    }

    /// <summary>The enemy's body collider, which moving needs.</summary>
    private Collider RequireBody() =>
        Body ?? throw new InvalidOperationException($"The enemy \"{Name}\" has no body collider to move with.");
}
