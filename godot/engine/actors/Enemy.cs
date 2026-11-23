using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
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
/// "death": { "effect": "effects/small_explosion.json", "sound": "enemy_killed" },
/// "picture_offset": { "x": -8, "y": -8 }
/// </code>
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
    private TimelinePlayer? _animation;
    private readonly Dictionary<string, ShotType> _shots = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _statusSounds = new(StringComparer.Ordinal);
    private string _hurtSound = "";
    private Enemy? _replacement;
    private int _respawnUpdates = 64;
    private string _deathEffect = "";
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

    /// <summary>How many updates it has been stepped for while awake.</summary>
    public int Updates { get; private set; }

    /// <summary>Whether it was hurt in this update.</summary>
    public bool WasJustHurt { get; private set; }

    /// <summary>Whether it is flashing from being hurt.</summary>
    public bool IsFlashing => _flash.IsRunning;

    /// <summary>Its timelines, by name.</summary>
    public TimelineSet Timelines { get; private set; } = TimelineSet.None;

    /// <summary>The name of the timeline it is showing; empty for none.</summary>
    public string Playing { get; private set; } = "";

    /// <summary>Whether the timeline it is showing, one that does not loop, has played through.</summary>
    public bool AnimationIsOver => _animation is { IsOver: true };

    /// <summary>The collider it moves through terrain with, if it has one.</summary>
    protected Collider? Body { get; private set; }

    /// <summary>The collider that blocks and carries the player, if it has one.</summary>
    protected Collider? Solid { get; private set; }

    /// <summary>What shows it, if its scene gives it a visual.</summary>
    protected ActorVisual? Visual { get; private set; }

    /// <summary>Where the player is: what most enemies aim at and move toward.</summary>
    protected PixelPosition Target => Room.Player.Position;

    /// <summary>The side of it the player is on.</summary>
    protected Facing TowardPlayer => Room.Player.Position.X < Position.X ? Facing.Left : Facing.Right;

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
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.Bind(room.Content);
            Visual ??= visual;
        }

        OnEnterRoom();
    }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _reactions = data.Has("reactions") ? Reactions.Read(data.List("reactions")) : Reactions.None;
        _drops = data.Has("drops") ? DropTable.Read(data.Get("drops")) : DropTable.Nothing;
        Timelines = TimelineSet.ReadFrom(data);
        EntityNode.ReadColliders(this, data);
        HurtFlashUpdates = data.Has("hurt_flash_updates") ? data.Int("hurt_flash_updates") : 8;
        StunnedWhileHurt = !data.Has("stunned_while_hurt") || data.Flag("stunned_while_hurt");
        _hurtSound = data.Has("hurt_sound") ? data.Text("hurt_sound") : "";
        _deathEffect = data.Has("death") && data.Get("death").Has("effect") ? data.Get("death").Text("effect") : "";
        _deathSound = data.Has("death") && data.Get("death").Has("sound") ? data.Get("death").Text("sound") : "";
        _respawnUpdates = data.Has("respawn_updates") ? data.Int("respawn_updates") : 64;
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
    /// Plays one update: its timers move on, it behaves unless a status or a hurt stops it, its timeline moves on, and
    /// it hurts the player if they are touching it.
    /// </summary>
    internal void Step()
    {
        WasJustHurt = false;
        if (IsAsleep)
            return;
        Updates++;
        _unhittable = _unhittable.Stepped();
        bool wasFlashing = _flash.IsRunning;
        _flash = _flash.Stepped();
        if (Status.Length > 0)
        {
            _statusLeft = _statusLeft.Stepped();
            if (_statusLeft.IsOver)
                EndStatus();
        }

        if (!IsFrozen)
        {
            if (!(StunnedWhileHurt && wasFlashing))
            {
                Behave();
                if (!IsDead)
                    StepAnimation();
            }

            if (!IsDead)
                TouchPlayer();
        }

        if (!IsDead)
            ShowLook();
    }

    /// <summary>This enemy's own behaviour for one update.</summary>
    protected abstract void Behave();

    /// <summary>Shows the timeline named <paramref name="name"/> from its start. Naming the one showing changes nothing unless <paramref name="restart"/> is asked for.</summary>
    protected void Play(string name, bool restart = false)
    {
        if (name == Playing && !restart && _animation is not null)
            return;
        Playing = name;
        _animation = new TimelinePlayer(Timelines.Get(name));
        Visual?.Show(_animation.Current.Picture);
    }

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
        Room.Spawn(projectile);
        if (type.Sound.Length > 0)
            Room.Session.Sounds.Request(type.Sound, channel: "enemies");
        return projectile;
    }

    /// <summary>Launches one of its shots from <paramref name="at"/> straight at the player, at the shot's own speed.</summary>
    protected Projectile FireAtPlayer(string shot, PixelPosition at)
    {
        ShotType type = Shot(shot);
        PixelOffset toPlayer = Target - at;
        int angle = Turns.AngleOf(toPlayer.X.Whole, toPlayer.Y.Whole);
        return Fire(shot, at, new PixelVelocity(Turns.Cosine(angle, type.Speed), Turns.Sine(angle, type.Speed)));
    }

    /// <summary>Asks for a sound.</summary>
    protected void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "enemies");
    }

    /// <inheritdoc/>
    public virtual bool IsHitBy(HitSide side) => side != HitSide.Enemies && !IsDead && !IsAsleep;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsDead)
            return HitResult.Ignored;
        if (_unhittable.IsRunning)
            return HitResult.Absorbed;

        Reaction reaction = ReactionTo(hit, part);
        if (reaction.TurnsBack)
            return HitResult.TurnedBack;

        int damage = reaction.DamageFrom(hit.Damage);
        bool lethal = damage > 0 && Health.WouldBeEmptiedBy(damage);
        bool takesStatus = reaction.StatusWhen == StatusWhen.Always ||
            (reaction.StatusWhen == StatusWhen.InsteadOfDying && lethal && Status != reaction.Status);
        if (takesStatus)
            BeginStatus(reaction.Status, Room.StatusUpdatesFor(reaction.Status, reaction.StatusUpdates));
        if (reaction.StatusWhen == StatusWhen.InsteadOfDying && takesStatus)
            return HitResult.Hurt;
        if (damage == 0)
        {
            if (!takesStatus)
                OnAbsorbed(hit, part);
            return takesStatus ? HitResult.Hurt : HitResult.Absorbed;
        }

        Health.Take(damage);
        _unhittable = Countdown.Of(reaction.UnhittableUpdates);
        _flash = Countdown.Of(HurtFlashUpdates);
        WasJustHurt = true;
        if (!IsFrozen)
            Sound(_hurtSound);
        if (Health.IsEmpty)
        {
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

    /// <summary>The enemy's reactions as its data file gives them.</summary>
    protected Reactions Reactions => _reactions;

    /// <summary>Called when a hit landed on the enemy and did it no damage and gave it no status.</summary>
    protected virtual void OnAbsorbed(Hit hit, Collider part)
    {
    }

    /// <summary>Called when a hit hurt the enemy without destroying it.</summary>
    protected virtual void OnHurt(Hit hit, Collider part)
    {
    }

    /// <summary>Destroys the enemy: it is recorded as gone, leaves its death effect and its drop, and is taken out of its room.</summary>
    public void Die()
    {
        if (IsDead)
            return;
        IsDead = true;
        if (!RespawnsWhenKilled)
            Room.MarkGone(Name);
        var leftBehind = new List<Node>();
        if (_deathEffect.Length > 0)
            leftBehind.Add(LeaveEffect(_deathEffect, Position));
        Sound(_deathSound);
        if (Drop(_drops, Position) is { } pickup)
            leftBehind.Add(pickup);
        OnDied();
        Room.Remove(this);
        if (_replacement is { } replacement)
        {
            _replacement = null;
            Room.SpawnLater(replacement, leftBehind, _respawnUpdates);
        }
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        // A copy never used, because the enemy left its room alive, goes with it.
        if (_replacement is { } unused && IsInstanceValid(unused))
            unused.Free();
        _replacement = null;
    }

    /// <summary>Leaves an effect in the room at <paramref name="at"/>, by the data file of its timeline.</summary>
    protected Effect LeaveEffect(string file, PixelPosition at)
    {
        Effect effect = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(file)), at);
        Room.Spawn(effect);
        return effect;
    }

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
        Inventory inventory = Room.Player.Inventory;
        string? drop = table.Roll(Room.Session.Random, inventory, file => PickupType.Read(Room.Content.ReadData(file)).IsUsefulTo(inventory));
        if (drop is null)
            return null;
        Pickup pickup = Pickup.Of(PickupType.Read(Room.Content.ReadData(drop)), at);
        Room.Spawn(pickup);
        return pickup;
    }

    /// <summary>Called as the enemy is destroyed, before it leaves its room.</summary>
    protected virtual void OnDied()
    {
    }

    /// <summary>Puts the enemy in a status for a number of updates. Frozen, it stops and its solid collider blocks and carries.</summary>
    public void BeginStatus(string status, int updates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        bool wasFrozen = IsFrozen;
        Status = status;
        _statusLeft = Countdown.Of(updates);
        if (IsFrozen && Solid is not null)
            Solid.Enabled = true;
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

    /// <summary>Moves the enemy across by its speed and returns what happened.</summary>
    protected MoveResult MoveAcross(MoveOptions options) =>
        Room.Mover.MoveAcross(RequireBody(), Velocity.X, options);

    /// <summary>Moves the enemy down by its speed and returns what happened.</summary>
    protected MoveResult MoveDown(MoveOptions options) =>
        Room.Mover.MoveDown(RequireBody(), Velocity.Y, options);

    /// <summary>Moves the enemy across by <paramref name="distance"/>, treating slopes as walls, and returns whether something stopped it.</summary>
    protected bool MoveAcrossBy(Pixels distance) =>
        Room.Mover.MoveAcross(RequireBody(), distance, MoveOptions.Plain with { Slopes = SlopeTreatment.Wall }).Blocked;

    /// <summary>Moves the enemy down by <paramref name="distance"/>, upward when negative, and returns whether something stopped it.</summary>
    protected bool MoveDownBy(Pixels distance) =>
        Room.Mover.MoveDown(RequireBody(), distance, MoveOptions.Plain with { Slopes = SlopeTreatment.Wall }).Blocked;

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

    /// <summary>Whether the player is standing on the enemy's solid collider.</summary>
    protected bool IsRiddenByPlayer
    {
        get
        {
            if (Solid is not { Enabled: true } solid)
                return false;
            PixelRect top = solid.RoomRect, rider = Room.Player.BodyRect;
            return rider.Bottom == top.Top && rider.Left < top.Right && rider.Right > top.Left;
        }
    }

    /// <summary>Moves the player by <paramref name="offset"/>, as far as terrain lets them go: for an enemy that takes its rider with it.</summary>
    protected void ShiftPlayer(PixelOffset offset)
    {
        MoveOptions carried = MoveOptions.Plain with { StoppedBySolids = false, Slopes = SlopeTreatment.Wall };
        if (!offset.X.IsZero)
            Room.Mover.MoveAcross(Room.Player.Body, offset.X, carried);
        if (!offset.Y.IsZero)
            Room.Mover.MoveDown(Room.Player.Body, offset.Y, carried);
    }

    /// <summary>Whether the enemy stands on terrain or a solid.</summary>
    protected bool IsOnGround => Room.Mover.IsOnGround(RequireBody(), MoveOptions.Plain);

    /// <summary>Whether the enemy's place is in the camera's view, or within <paramref name="margin"/> pixels of it.</summary>
    protected bool IsInView(int margin)
    {
        PixelRect view = Room.Session.Camera.View;
        int x = Position.X.Whole, y = Position.Y.Whole;
        return x >= view.Left - margin && x < view.Left + view.Width + margin && y >= view.Top - margin && y < view.Top + view.Height + margin;
    }

    /// <summary>Whether the player's body is within the given distances of the enemy, across and down.</summary>
    protected bool PlayerIsWithin(int across, int down)
    {
        PixelOffset toPlayer = Target - Position;
        return toPlayer.X.Abs() < Pixels.FromWhole(across) && toPlayer.Y.Abs() < Pixels.FromWhole(down);
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>Moves the enemy's timeline on by one update and hands it the cues reached.</summary>
    private void StepAnimation()
    {
        if (_animation is null)
            return;
        foreach (string cue in _animation.Step())
        {
            // Three cues mean the same for every enemy; the rest are the enemy's own.
            if (cue.StartsWith("sound:", StringComparison.Ordinal))
                Sound(cue["sound:".Length..]);
            else if (cue == "always_active")
                AlwaysActive = true;
            else if (cue == "not_always_active")
                AlwaysActive = false;
            else
                OnCue(cue);
            if (IsDead)
                return;
        }

        Visual?.Show(_animation.Current.Picture);
    }

    /// <summary>Shows the enemy as it is: flashing when hurt, tinted when frozen.</summary>
    private void ShowLook()
    {
        if (Visual is null)
            return;
        Visual.Tint(IsFrozen ? FrozenTint : _flash.IsRunning && _flash.Remaining % 2 == 0 ? HurtTint : RestingTint);
    }

    /// <summary>The tint the enemy is shown with when nothing has happened to it. An enemy that glows says so here.</summary>
    protected virtual Color RestingTint => Colors.White;

    /// <summary>The tint of a frozen enemy.</summary>
    private static readonly Color FrozenTint = new(0.6f, 0.8f, 1.5f);

    /// <summary>The tint of an enemy flashing from a hit.</summary>
    private static readonly Color HurtTint = new(2f, 2f, 2f);

    /// <summary>Ends the enemy's status.</summary>
    private void EndStatus()
    {
        if (IsFrozen && Solid is not null)
            Solid.Enabled = _solidWhenNotFrozen;
        string ended = Status;
        Status = "";
        OnStatusEnded(ended);
    }

    /// <summary>Whether the enemy's solid collider is on when it is not frozen.</summary>
    private bool _solidWhenNotFrozen;

    /// <summary>Lets the enemy touch the player if a part of it overlaps the player's body.</summary>
    private void TouchPlayer()
    {
        if (PartTouchingPlayer() is { } part)
            Touch(part);
    }

    /// <summary>The part of the enemy that overlaps the player's body, if one does: a hitbox, or failing any hitboxes a hurtbox.</summary>
    protected Collider? PartTouchingPlayer()
    {
        var touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hitbox).ToList();
        if (touching.Count == 0)
            touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hurtbox).ToList();
        PixelRect player = Room.Player.BodyRect;
        return touching.FirstOrDefault(collider => collider.RoomRect.Overlaps(player));
    }

    /// <summary>
    /// Called each update that a part of the enemy touches the player. The engine's enemy hurts the player with its
    /// contact damage; an enemy that does something else to whoever it touches says so here.
    /// </summary>
    protected virtual void Touch(Collider part)
    {
        if (ContactDamage <= 0)
            return;
        var direction = new PixelOffset(Room.Player.Position.X >= Position.X ? Pixels.FromWhole(1) : Pixels.FromWhole(-1), Pixels.Zero);
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
