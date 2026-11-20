using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// What an enemy is. The base owns what every enemy has: health, reacting to a hit, being hurt, statuses such as
/// frozen, dying, dropping a pickup, hurting the player on contact, and being stepped by its room. A particular enemy
/// adds only its own behaviour. A boss is an enemy too: one scene, its parts child nodes with their own hurtboxes.
/// </summary>
public abstract partial class Enemy : Node2D, IRoomEntity, IHittable, IPlaced, IHasValues
{
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

    private Room? _room;
    private Health? _health;
    private Reactions _reactions = Reactions.None;
    private DropTable _drops = DropTable.Nothing;
    private Countdown _unhittable = Countdown.Over;
    private Countdown _statusLeft = Countdown.Over;

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

    /// <summary>Whether it has been destroyed.</summary>
    public bool IsDead { get; private set; }

    /// <summary>Whether it was hurt in this update, for a flash.</summary>
    public bool WasJustHurt { get; private set; }

    /// <summary>The collider it moves through terrain with, if it has one.</summary>
    protected Collider? Body { get; private set; }

    /// <summary>The collider that blocks and carries the player, if it has one.</summary>
    protected Collider? Solid { get; private set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        Position = EntityNode.StartingPlace(this, Position);
        Facing = StartsFacing;
        DataValues.Fill(this, room.Content);
        _health = new Health(MaxHealth);
        Body = EntityNode.ColliderOf(this, ColliderRole.Body);
        Solid = EntityNode.ColliderOf(this, ColliderRole.Solid);
        _solidWhenNotFrozen = Solid is { Enabled: true };
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Bind(room.Content);
        OnEnterRoom();
    }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _reactions = data.Has("reactions") ? Reactions.Read(data.List("reactions")) : Reactions.None;
        _drops = data.Has("drops") ? DropTable.Read(data.Get("drops")) : DropTable.Nothing;
        EntityNode.ReadColliders(this, data);
    }

    /// <summary>Called once as the enemy enters its room, after its values are loaded.</summary>
    protected virtual void OnEnterRoom()
    {
    }

    /// <summary>
    /// Plays one update: its timers move on, it behaves unless a status stops it, and it hurts the player if they are
    /// touching it.
    /// </summary>
    internal void Step()
    {
        WasJustHurt = false;
        _unhittable = _unhittable.Stepped();
        if (Status.Length > 0)
        {
            _statusLeft = _statusLeft.Stepped();
            if (_statusLeft.IsOver)
                EndStatus();
        }

        if (!IsFrozen)
        {
            Behave();
            if (!IsDead)
                TouchPlayer();
        }
    }

    /// <summary>This enemy's own behaviour for one update.</summary>
    protected abstract void Behave();

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Enemies && !IsDead;

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
        bool takesStatus = reaction.StatusWhen == StatusWhen.Always || (reaction.StatusWhen == StatusWhen.InsteadOfDying && lethal);
        if (takesStatus)
            BeginStatus(reaction.Status, reaction.StatusUpdates);
        if (reaction.StatusWhen == StatusWhen.InsteadOfDying && lethal)
            return HitResult.Hurt;
        if (damage == 0)
            return takesStatus ? HitResult.Hurt : HitResult.Absorbed;

        Health.Take(damage);
        _unhittable = Countdown.Of(reaction.UnhittableUpdates);
        WasJustHurt = true;
        if (Health.IsEmpty)
        {
            Die();
            return HitResult.Destroyed;
        }

        OnHurt(hit, part);
        return HitResult.Hurt;
    }

    /// <summary>
    /// The reaction to a hit that landed on <paramref name="part"/>. An enemy whose parts react differently says so here.
    /// </summary>
    protected virtual Reaction ReactionTo(Hit hit, Collider part) => _reactions.To(hit);

    /// <summary>Called when a hit hurt the enemy without destroying it.</summary>
    protected virtual void OnHurt(Hit hit, Collider part)
    {
    }

    /// <summary>Destroys the enemy: it is recorded as gone, leaves its drop, and is taken out of its room.</summary>
    public void Die()
    {
        if (IsDead)
            return;
        IsDead = true;
        Room.MarkGone(Name);
        string? drop = _drops.Roll(Room.Session.Random, Room.Player.Inventory, file => PickupType.Read(Room.Content.ReadData(file)).AddsTo);
        if (drop is not null)
            Room.Spawn(Pickup.Of(PickupType.Read(Room.Content.ReadData(drop)), Position));
        OnDied();
        Room.Remove(this);
    }

    /// <summary>Called as the enemy is destroyed, before it leaves its room.</summary>
    protected virtual void OnDied()
    {
    }

    /// <summary>Puts the enemy in a status for a number of updates. Frozen, it stops and its solid collider blocks and carries.</summary>
    public void BeginStatus(string status, int updates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        Status = status;
        _statusLeft = Countdown.Of(updates);
        if (IsFrozen && Solid is not null)
            Solid.Enabled = true;
        OnStatusBegan(status);
    }

    /// <summary>Called as the enemy is put in a status.</summary>
    protected virtual void OnStatusBegan(string status)
    {
    }

    /// <summary>Moves the enemy across by its speed and returns what happened.</summary>
    protected MoveResult MoveAcross(MoveOptions options) =>
        Room.Mover.MoveAcross(RequireBody(), Velocity.X, options);

    /// <summary>Moves the enemy down by its speed and returns what happened.</summary>
    protected MoveResult MoveDown(MoveOptions options) =>
        Room.Mover.MoveDown(RequireBody(), Velocity.Y, options);

    /// <summary>Whether the enemy stands on terrain or a solid.</summary>
    protected bool IsOnGround => Room.Mover.IsOnGround(RequireBody(), MoveOptions.Plain);

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>Ends the enemy's status.</summary>
    private void EndStatus()
    {
        if (IsFrozen && Solid is not null)
            Solid.Enabled = _solidWhenNotFrozen;
        Status = "";
    }

    /// <summary>Whether the enemy's solid collider is on when it is not frozen.</summary>
    private bool _solidWhenNotFrozen;

    /// <summary>Hurts the player if the enemy's hitboxes, or failing those its hurtboxes, overlap the player's body.</summary>
    private void TouchPlayer()
    {
        if (ContactDamage <= 0)
            return;
        var touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hitbox).ToList();
        if (touching.Count == 0)
            touching = EntityNode.CollidersOf(this).Where(collider => collider.Enabled && collider.Role == ColliderRole.Hurtbox).ToList();
        PixelRect player = Room.Player.BodyRect;
        Collider? met = touching.FirstOrDefault(collider => collider.RoomRect.Overlaps(player));
        if (met is null)
            return;
        var direction = new PixelOffset(Room.Player.Position.X >= Position.X ? Pixels.FromWhole(1) : Pixels.FromWhole(-1), Pixels.Zero);
        Collider hurtbox = Room.Player.Hurtbox;
        Room.Player.TakeHit(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Direction = direction, Source = this }, hurtbox);
    }

    /// <summary>The enemy's body collider, which moving needs.</summary>
    private Collider RequireBody() =>
        Body ?? throw new InvalidOperationException($"The enemy \"{Name}\" has no body collider to move with.");
}
